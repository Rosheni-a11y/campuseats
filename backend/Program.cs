using CampusEats.Api.Services;
using CampusEats.Api.Data;
using CampusEats.Api.Models;

using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

using System.Text;

var builder = WebApplication.CreateBuilder(args);


// -------------------------
// Controllers
// -------------------------

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();


// -------------------------
// CORS (frontend origin from env var)
// -------------------------

var frontendOrigin =
    builder.Configuration["FrontendOrigin"] ?? "http://localhost:5173";

builder.Services.AddCors(o => o.AddPolicy("Frontend", p =>
    p.WithOrigins(frontendOrigin)
     .AllowAnyHeader()
     .AllowAnyMethod()));


// -------------------------
// Swagger + JWT
// -------------------------

builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition(
        "Bearer",
        new OpenApiSecurityScheme
        {
            Name = "Authorization",
            In = ParameterLocation.Header,

            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",

            Description =
                "Paste ONLY the token - no 'Bearer ' prefix."
        }
    );

    c.AddSecurityRequirement(
        new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },

                Array.Empty<string>()
            }
        }
    );
});


// -------------------------
// Menu service
// -------------------------

builder.Services.AddScoped<IMenuService, MenuService>();


// -------------------------
// PostgreSQL / EF Core
// -------------------------

builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseNpgsql(
        builder.Configuration
            .GetConnectionString("Default")
    )
);


// -------------------------
// JWT Authentication
// -------------------------

var jwt =
    builder.Configuration.GetSection("Jwt");

var key =
    Encoding.UTF8.GetBytes(jwt["Key"]!);

builder.Services
    .AddAuthentication(
        JwtBearerDefaults.AuthenticationScheme
    )
    .AddJwtBearer(opt =>
    {
        opt.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwt["Issuer"],

                ValidateAudience = true,
                ValidAudience = jwt["Audience"],

                ValidateIssuerSigningKey = true,

                IssuerSigningKey =
                    new SymmetricSecurityKey(key),

                ValidateLifetime = true
            };
    });


builder.Services.AddAuthorization();


var app = builder.Build();


// -------------------------
// Seed menu data
// -------------------------

using (var scope =
       app.Services.CreateScope())
{
    var db =
        scope.ServiceProvider
            .GetRequiredService<AppDbContext>();

    if (!db.MenuItems.Any())
    {
        db.MenuItems.AddRange(

            new MenuItem
            {
                Name = "Kottu Roti",
                Price = 750m,
                Category = "Mains"
            },

            new MenuItem
            {
                Name = "Fried Rice",
                Price = 850m,
                Category = "Mains"
            },

            new MenuItem
            {
                Name = "Watalappan",
                Price = 350m,
                Category = "Dessert"
            }
        );

        db.SaveChanges();
    }
}


// -------------------------
// HTTP pipeline
// -------------------------

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("Frontend");


// Authentication MUST come before Authorization

app.UseAuthentication();

app.UseAuthorization();


app.MapControllers();

app.Run();
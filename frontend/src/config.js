// Read the API base URL from the environment (set on the hosting platform in production)
export const API_URL = import.meta.env.VITE_API_URL;

// Falls back to the local static menu.json when no backend is configured yet
export const MENU_ENDPOINT = API_URL ? `${API_URL}/api/menu` : "/menu.json";

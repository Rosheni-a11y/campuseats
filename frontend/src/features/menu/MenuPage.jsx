import { useState } from "react";
import { useDebounce } from " .. /hooks/useDebounce";
import MenuList from " .. /components/MenuList";
import { useFetch } from "../hooks/useFetch";

function MenuPage () {
const [query, setQuery] = useState ("");
const debounced = useDebounce (query, 400) ;

const filtered = SAMPLE. filter ( (d) =>
d. name. toLowerCase () .includes (debounced. toLowerCase () )

);

return (
<div>
<input
value= { query }
onChange={ (e) => setQuery(e.target.value) }
placeholder="Search dishes ... "
/>
<MenuList dishes={filtered} />
</div>
);
}
export default MenuPage;
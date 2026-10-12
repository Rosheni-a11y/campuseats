import { useState, useEffect } from "react";

export function useDebounce (value, delay = 400) {
const [debounced, setDebounced] = useState (value) ;
useEffect(() =>{
const id = setTimeout ( () => setDebounced (value), delay) ;
// cleanup

return () => clearTimeout (id) ;
}, [value, delay] ) ;
return debounced;

}
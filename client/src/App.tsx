import { APITester } from "./APITester";
import "./index.css";

import logo from "./logo.svg";
import reactLogo from "./react.svg";
import {useEffect, useState} from "react";
import {Api, type GroceryItem} from "../Api.ts";

const api = new Api();

export function App() {

    const [groceries, setgroceris] = useState<GroceryItem[]>([])

    useEffect(() => {
        api.getAllGroceries.groceriesGetAllGroceries().then(result => {
            setgroceris(result.data)
        })
        api.create.groceriesCreate({
            category: "",
            isDiscontinued:
        })
    }, []);

  return (
    <div className="app">
        {
            JSON.stringify(groceries)
        }

    </div>
  );
}

export default App;

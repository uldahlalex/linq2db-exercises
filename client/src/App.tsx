import { APITester } from "./APITester";
import "./index.css";

import logo from "./logo.svg";
import reactLogo from "./react.svg";
import {useEffect, useState} from "react";
import {Api, type GroceryItem} from "../Api.ts";
import toast from "react-hot-toast";

const api = new Api();

export function App() {

    const [groceries, setgroceris] = useState<GroceryItem[]>([])

    useEffect(() => {
        api.getAllGroceries.groceriesGetAllGroceries().then(result => {
            setgroceris(result.data)
        })


    }, []);

  return (
    <div className="app">
        {
            JSON.stringify(groceries)
        }

        <button onClick={async () => {

            try {
                const result = await api.create.groceriesCreate({
                })
            } catch (e: any) {
                toast(e.error.title)
            }
        }}>Click me to trigger an error</button>
    </div>
  );
}

export default App;

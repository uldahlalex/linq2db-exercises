import { APITester } from "./APITester";
import "./index.css";

import logo from "./logo.svg";
import reactLogo from "./react.svg";
import {useEffect, useState} from "react";
import {Api, type GroceryItem} from "../Api.ts";
import toast from "react-hot-toast";

const api = new Api();

export function App() {

    const [grocies, setGroceries] = useState<GroceryItem[]>([])

    useEffect(() => {

        api.getAllMyGroceries.groceriesGetAllMyGroceries()
            .then(r => {
            setGroceries(r.data)
        })
    }, []);

  return (
    <div className="app">

        {
            JSON.stringify(grocies)
        }

        <button onClick={async () => {
            try {
                 await api.upsert.groceriesUpsert({})
                 //Did not throw: means status code 200-someting
            } catch (e: any) {
                //we got a status code 400 or 500-something
                toast(e.error.title)
            }
        }}>Click this to trigger a server error and get appropriate response handling</button>
    </div>
  );
}

export default App;

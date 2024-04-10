import { useState } from 'react'
import ReactDOM from "react-dom/client";
import { BrowserRouter, Routes, Route } from "react-router-dom";
import './App.css'

import Layout from "./pages/Layout";
// User UI
import Home from "./pages/Home";
import Register from "./pages/Register";
import Logout from "./pages/Logout";

// Tasks UI
import Tasks from "./pages/Tasks";

import NoPage from "./pages/NoPage";

function App() {
  const [count, setCount] = useState(0)

  return (
    <>
      <BrowserRouter>
        <Routes>
          <Route path="/" element={<Layout />}>
            <Route index element={<Home />} />
            <Route path="register" element={<Register />} />
            <Route path="logout" element={<Logout />} />
            <Route path="tasks" element={<Tasks />} />
            <Route path="*" element={<NoPage />} />
          </Route>
        </Routes>
      </BrowserRouter>
    </>
  )
}

export default App

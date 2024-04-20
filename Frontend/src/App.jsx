import { useState } from 'react'
import ReactDOM from "react-dom/client";
import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import './App.css'

import Layout from "./pages/Layout";
// User UI
import Home from "./pages/Home";
import Register from "./pages/Register";
import Logout from "./pages/Logout";
import Account from './pages/Account';

// Tasks UI
import Tasks from "./pages/Tasks";

// Activity
import Activity from "./pages/Activity";

// Statistics
import Statistics from "./pages/Statistics";


import NoPage from "./pages/NoPage";

function App() {
  return (
    <Router>
      <Routes>
        <Route path="/" element={<Layout />}>
          <Route index element={<Home />} />
          <Route path="register" element={<Register />} />
          <Route path="activity" element={<Activity />} />
          <Route path="account" element={<Account />} />
          <Route path="logout" element={<Logout />} />
          <Route path="tasks" element={<Tasks />} />
          <Route path="statistics" element={<Statistics/>} />
          
          <Route path="*" element={<NoPage />} />
        </Route>
      </Routes>
    </Router>
  );
}

export default App

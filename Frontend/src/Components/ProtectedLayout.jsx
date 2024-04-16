import React, { useEffect, useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { userAuthorized } from '../Services/userServices';

const ProtectedLayout = ({ children }) => {
  const navigate = useNavigate();
  const [isLoggedIn, setIsLoggedIn] = useState(null); // Variable to determine if user is logged in

    var hasRun = false;
    useEffect(() => {
        if (hasRun) return; // Prevent running code twice
        hasRun = true;
        
        async function checkAuthentication() {
            const authorized = await userAuthorized();
            setIsLoggedIn(authorized);
            console.info("Authorized: ", authorized);
        }

        checkAuthentication();
    }, [setIsLoggedIn, navigate]);



    // Render children only if user is logged in
    return isLoggedIn != null && isLoggedIn ? (
        <div>{children}</div>
    ) : isLoggedIn == null ? (
        <div style={{ margin: "10px" }}>
            <p>Loading...</p>
        </div>
    ) : (
        <div style={{ margin: "10px" }}>
            <p>Not Authorized!</p>
            <Link to="/">Go to Login Page</Link>
        </div>
    );
};

export default ProtectedLayout;
import React, { useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import API_ROOT from '../config';

const token = sessionStorage.getItem("sessionToken");

const Logout = () => {
    const navigate = useNavigate();

    useEffect(() => {
        async function logout() {
            if (!token) return;
            
            const response = await fetch(`${API_ROOT}/logout`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${token}`
                }
            });
            
            if (response.ok) {
                sessionStorage.clear("sessionToken");
                sessionStorage.clear("tokenExpirationUTC");
            }
        }
        logout();
        navigate('/');
    }, []); // Empty dependency array ensures useEffect runs only once

    return (
        <div>
            <h2>Logging out...</h2>
        </div>
    );
};

export default Logout;

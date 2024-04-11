import React, { useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import API_ROOT from '../config';



const Logout = () => {
    const navigate = useNavigate();

    var firstRun = true;
    useEffect(() => {
        if (!firstRun) return; // Make sure we dont fire this useEffect twice
        firstRun = false;

        async function logout() {
            
            const token = sessionStorage.getItem("sessionToken");

            if (!token) {
                navigate('/');
                return;
            };
            
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
            navigate('/');
        }
        logout();
    }, []);

    return (
        <div>
            <h2>Logging out...</h2>
        </div>
    );
};

export default Logout;

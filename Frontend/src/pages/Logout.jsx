import React, { useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { logoutUser } from '../Services/userServices';
import ProtectedLayout from '../Components/ProtectedLayout';

const Logout = () => {
    const navigate = useNavigate();

    var firstRun = true;
    useEffect(() => {
        if (!firstRun) return; // Make sure we dont fire this useEffect twice
        firstRun = false;

        async function logout() {
            await logoutUser();
            navigate('/');
        }
        logout();
    }, []);

    return (
        <ProtectedLayout>
            <h2>Logging out...</h2>
        </ProtectedLayout>
    );
};

export default Logout;

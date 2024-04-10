
import React, { useState, useEffect } from 'react';
import { Container, Form, Button } from 'react-bootstrap';
import { useNavigate, Link } from 'react-router-dom';
import API_ROOT from '../config';




const Home = () => {
    const [username, setUsername] = useState('');
    const [password, setPassword] = useState('');
    const [errorMessage, setShowError] = useState(null);
    const navigate = useNavigate();
    
    const token = sessionStorage.getItem("sessionToken");

    useEffect(() => {
        async function tasks() {
            const sessionTokenExpiresUTC = new Date(sessionStorage.getItem("tokenExpirationUTC"));

            // If session is still valid, and user is logged in -> forward to tasks
            if (token && sessionTokenExpiresUTC > new Date()) navigate('/tasks');
        }
        tasks();
    }, []);


    const handleLogin = async (e) => {
        e.preventDefault();
        const formData = new FormData();
        formData.append('username', username);
        formData.append('password', password);

        try {
            const response = await fetch(`${API_ROOT}/login`, {
                method: 'POST',
                body: formData,
            });
            
            if (response.ok) {
                const data = await response.json();
                console.log('Login successful:', data);
                
                const sessionToken = data.sessionToken;
                console.log('Session token:', sessionToken);
                
                sessionStorage.setItem("sessionToken", sessionToken);
                sessionStorage.setItem("tokenExpirationUTC",data.tokenExpirationUTC);
                navigate('/tasks');
            } else {
                const errorMessage = await response.text();
                console.error('Registration failed:', errorMessage);
                setShowError(errorMessage);
            }
        } catch (error) {
            console.error('Error:', error.message);
        }
    };
    return (
        <Container style={{ marginTop: '50px' }}>
            <h1>Login</h1>
            <Form onSubmit={handleLogin}>
                <Form.Group controlId="formUsername">
                    <Form.Label>Username</Form.Label>
                    <Form.Control
                    type="text"
                    placeholder="Enter username"
                    value={username}
                    onChange={(e) => setUsername(e.target.value)}
                    />
                </Form.Group>
                <Form.Group controlId="formPassword">
                    <Form.Label>Password</Form.Label>
                    <Form.Control
                    type="password"
                    placeholder="Enter password"
                    value={password}
                    onChange={(e) => setPassword(e.target.value)}
                    />
                </Form.Group>
                <br></br>
                <Button variant="primary" type="submit">
                    Login
                </Button>
            </Form>
            <br></br>
            <p>If you don't already have account. Register from here:</p>
            <Link to="/register">
                <Button variant='primary'>Register</Button>
            </Link>

            {errorMessage != null && (
                <div className="alert alert-danger">
                    <strong>Error!</strong>
                    <br></br>
                    {errorMessage}
                </div>
            )}
        </Container>
        
    );
};
  
export default Home;
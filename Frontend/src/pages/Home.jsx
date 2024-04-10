
import React, { useState } from 'react';
import { Container, Form, Button } from 'react-bootstrap';
import { useNavigate, Link } from 'react-router-dom';
import API_ROOT from '../config';


const token = sessionStorage.getItem("sessionToken");

const Home = () => {
    const [username, setUsername] = useState('');
    const [password, setPassword] = useState('');
    const [errorMessage, setShowError] = useState(null);
    const navigate = useNavigate();

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
                <Button variant="primary" type="submit">
                    Login
                </Button>
            </Form>
            <br></br>
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
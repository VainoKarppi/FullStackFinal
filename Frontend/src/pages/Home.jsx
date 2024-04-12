
import React, { useState, useEffect } from 'react';
import { Container, Form, Button } from 'react-bootstrap';
import { useNavigate, Link } from 'react-router-dom';
import { loginUser, userAuthorized } from '../Services/userServices';


const Home = () => {
    const [username, setUsername] = useState('');
    const [password, setPassword] = useState('');
    const [useEffectCompleted, setUseEffectCompleted] = useState(false);
    const [errorMessage, setShowError] = useState(null);
    const navigate = useNavigate();

    useEffect(() => {
        async function forward() {
            setUseEffectCompleted(true);
            if (await userAuthorized()) {
                navigate('/tasks');
                return;
            }
        }
        forward();
    }, []);


    const handleLogin = async (e) => {
        e.preventDefault();

        try {
            // Apply username and password to form-data
            const formData = new FormData();
            formData.append('username', username);
            formData.append('password', password);

            if (await loginUser(formData, setShowError)) navigate("/tasks");
        } catch (error) {
            if (error.response.data) {
                setShowError(error.response.data);
            } else {
                setShowError("Unable to contact API server");
            }
        }
    };

    return (
        useEffectCompleted && (
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
                {errorMessage != null && (
                    <div className="alert alert-danger">
                        <strong>Error!</strong>
                        <br></br>
                        {errorMessage}
                    </div>
                )}
                <br></br>
                <p>If you don't already have account. Register from here:</p>
                <Link to="/register">
                    <Button variant='primary'>Register</Button>
                </Link>
            </Container>
        )
    );
};
  
export default Home;
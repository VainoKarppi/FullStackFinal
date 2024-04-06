
import React, { useState } from 'react';
import { Container, Form, Button } from 'react-bootstrap';
import { useNavigate } from 'react-router-dom';
import API_ROOT from '../config';

const Home = () => {
    const [username, setUsername] = useState('');
    const [password, setPassword] = useState('');
    const [showSuccess, setShowSuccess] = useState(false);
    const navigate  = useNavigate();

    const handleRegister = (e) => {
        e.preventDefault();
        const formData = new FormData();
        formData.append('username', username);
        formData.append('password', password);

        fetch(`${API_ROOT}/register`, {
            method: 'POST',
            body: formData,
        })
        .then(response => response.json())
        .then(data => {
            console.log(data);
            const sessionToken = data.sessionToken;
            console.log('Session token:', sessionToken);
            setShowSuccess(true);
            setTimeout(() => {
                navigate('/tasks'); // Redirect to /tasks after 2 seconds
            }, 2000); // 2 seconds
        })
        .catch((error) => console.error('Error:', error));

        // Handle subscription logic (e.g., send email to server)
        // For now, let's just show a success message.
        // TODO call rest api register and wait for status 201
        // Send password as SHA256
        // TODO Forward to login page after 2 seconds
    };
    return (
        <Container style={{ marginTop: '50px' }}>
            <div className="card">
                <div className="card-header">
                    Welcome
                </div>
                <div className="card-body">
                    <blockquote className="blockquote mb-0">
                    <h3>Home Page</h3>
                    <p>This is the home page</p>
                    </blockquote>
                </div>
            </div>

            <br/>

            <Form onSubmit={handleRegister}>
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
                    <Form.Text className="text-muted">
                    Passwords are hashed using SHA256
                    </Form.Text>
                </Form.Group>
                <Button variant="primary" type="submit">
                    Register
                </Button>

                <br/><br/>

                {showSuccess && (
                    <div className="alert alert-success">
                        <strong>Success!</strong> Registerd with username: {username}
                        <br></br>
                        <ul>Forwarding to tasks page...</ul>
                    </div>
                )}
            </Form>
        </Container>
        
    );
};
  
export default Home;

import React, { useState } from 'react';
import { Container, Form, Button } from 'react-bootstrap';

const Home = () => {
    const [username, setUsername, password, setPassword] = useState('');
    const [showSuccess, setShowSuccess] = useState(false);

    const handleRegister = (e) => {
        e.preventDe
        fault();
        // Handle subscription logic (e.g., send email to server)
        // For now, let's just show a success message.
        // TODO call rest api register and wait for status 201
        // Send password as SHA256
        // TODO Forward to login page after 2 seconds
        setShowSuccess(true);
    };
    return (
        <Container style={{ marginTop: '50px' }}>
            <div class="card">
                <div class="card-header">
                    Welcome
                </div>
                <div class="card-body">
                    <blockquote class="blockquote mb-0">
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
                    </div>
                )}
            </Form>
        </Container>
        
    );
};
  
export default Home;
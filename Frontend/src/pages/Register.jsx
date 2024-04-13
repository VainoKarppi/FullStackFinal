
import React, { useState } from 'react';
import { Container, Form, Button } from 'react-bootstrap';
import { useNavigate } from 'react-router-dom';
import { registerUser } from '../Services/userServices';

const Register = () => {
    const [username, setUsername] = useState('');
    const [password, setPassword] = useState('');
    const [showSuccess, setShowSuccess] = useState(false);
    const [errorMessage, setShowError] = useState(null);
    const navigate = useNavigate();


    const handleRegister = async (e) => {
        e.preventDefault();
        
        try {
            const formData = new FormData();
            formData.append('username', username);
            formData.append('password', password);
        
            await registerUser(formData);
            setShowSuccess(true);

            setTimeout(() => {
                navigate('/tasks'); // Redirect to /tasks after 2 seconds
            }, 2000); // 2 seconds


        } catch (error) {
            setShowError(error.response.data);
            console.error('Error:', error.message);
        }
    };


    return (
        <Container style={{ marginTop: '50px' }}>
            <div style={{display: 'flex', justifyContent: 'center', alignItems: 'center', margin:"20px"}}>
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
                    <br></br>
                    <Button variant="primary" type="submit">
                        Register
                    </Button>

                    <br/><br/>

                    
                </Form>
            </div>
            {errorMessage != null && !showSuccess && (
                <div className="alert alert-danger">
                    <strong>Error!</strong>
                    <br></br>
                    {errorMessage}
                </div>
            )}
            {showSuccess && (
                <div className="alert alert-success">
                    <strong>Registration Success!</strong>
                    <br></br>
                    <ul>Forwarding to tasks page...</ul>
                </div>
            )}
        </Container>
        
    );
};
  
export default Register;
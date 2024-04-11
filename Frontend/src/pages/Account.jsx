
import React, { useState } from 'react';
import { Container, Form, Button } from 'react-bootstrap';
import API_ROOT from '../config';



const Account = () => {
    const [username, setUsername] = useState('');
    const [password, setPassword] = useState('');
    const [showSuccess, setShowSuccess] = useState(false);
    const [errorMessage, setShowError] = useState(null);


    const handleUpdate = async (e) => {
        e.preventDefault();
        
        try {
            const token = sessionStorage.getItem("sessionToken");

            if (!token) {
                navigate('/');
                return;
            };

            var updatedAccount = {};
            if (username.trim() !== '') updatedAccount.username = username;
            if (password.trim() !== '') updatedAccount.password = password;
            

            const response = await fetch(`${API_ROOT}/user/update`, {
                method: 'PATCH',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${token}`
                },
                body: JSON.stringify(updatedAccount)
            });
            
            if (response.ok) {
                setShowSuccess(true);     
            } else {
                setShowSuccess(false);
                const errorMessage = await response.text();
                console.error('Update failed:', errorMessage);
                setShowError(errorMessage);
            }
        } catch (error) {
            console.error('Error:', error.message);
        }
    };
    return (
        <Container style={{ marginTop: '50px' }}>
            <div style={{display: 'flex', justifyContent: 'center', alignItems: 'center', margin:"20px"}}>
                <Form onSubmit={handleUpdate}>
                    <Form.Group controlId="formUsername">
                        <Form.Label>Username</Form.Label>
                        <Form.Control
                        type="text"
                        placeholder="Enter new username"
                        value={username}
                        onChange={(e) => setUsername(e.target.value)}
                        />
                    </Form.Group>
                    <Form.Group controlId="formPassword">
                        <Form.Label>Password</Form.Label>
                        <Form.Control
                        type="password"
                        placeholder="Enter new password"
                        value={password}
                        onChange={(e) => setPassword(e.target.value)}
                        />
                    </Form.Group>
                    <br></br>
                    <Button variant="primary" type="submit">
                        Update
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
                    <strong>Update Success!</strong>
                </div>
            )}
        </Container>
        
    );
};
  
export default Account;
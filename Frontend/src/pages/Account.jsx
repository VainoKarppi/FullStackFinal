
import React, { useState, useEffect } from 'react';
import { Container, Form, Button } from 'react-bootstrap';
import { useNavigate } from 'react-router-dom';
import { updateUser, userAuthorized } from '../Services/userServices';
import ProtectedLayout from '../Components/ProtectedLayout';

const Account = () => {
    const [username, setUsername] = useState('');
    const [password, setPassword] = useState('');
    const [showSuccess, setShowSuccess] = useState(false);
    const [errorMessage, setShowError] = useState(null);
    const navigate = useNavigate();


    const handleUpdate = async (e) => {
        e.preventDefault();

        if (!await userAuthorized()) {
            navigate('/');
            return;
        };

        try {
            // Update only the values that were changed
            var updatedAccount = {};
            if (username.trim() !== '') updatedAccount.username = username;
            if (password.trim() !== '') updatedAccount.password = password;
            
            await updateUser(updatedAccount);
            setShowSuccess(true);
            setShowError(null);
        } catch (error) {
            setShowSuccess(false);
            setShowError(error.response.data);
        }
    };
    return (
        <ProtectedLayout>
            <Container style={{ marginTop: '50px' }}>
                <div style={{display: 'flex', justifyContent: 'center', alignItems: 'center', margin:"20px"}}>
                    <Form onSubmit={handleUpdate}>
                        <Form.Group controlId="formUsername">
                        <Form.Label>Username</Form.Label>
                            <div style={{marginTop:"-5%"}}>
                                <span style={{ fontSize: '12px'}}>(leave blank to keep current)</span>
                            </div>
                            <Form.Control
                            type="text"
                            placeholder="Enter new username"
                            value={username}
                            onChange={(e) => setUsername(e.target.value)}
                            />
                        </Form.Group>
                        <br></br>
                        <Form.Group controlId="formPassword">
                            <Form.Label>Password</Form.Label>
                            <div style={{marginTop:"-5%"}}>
                                <span style={{ fontSize: '12px'}}>(leave blank to keep current)</span>
                            </div>
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
        </ProtectedLayout>
    );
};
  
export default Account;
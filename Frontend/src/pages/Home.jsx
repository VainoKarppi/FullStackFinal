
import React, { useState } from 'react';
import { Container, Form, Button } from 'react-bootstrap';

const Home = () => {
    const [email, setEmail] = useState('');
    const [showSuccess, setShowSuccess] = useState(false);

    const handleSubscribe = (e) => {
        e.preventDefault();
        // Handle subscription logic (e.g., send email to server)
        // For now, let's just show a success message.
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

            <Form onSubmit={handleSubscribe}>
                <Form.Group controlId="formBasicEmail">
                    <Form.Label>Email address</Form.Label>
                    <Form.Control
                    type="email"
                    placeholder="Enter email"
                    value={email}
                    onChange={(e) => setEmail(e.target.value)}
                    />
                    <Form.Text className="text-muted">
                    We'll never share your email with anyone else.
                    </Form.Text>
                </Form.Group>
                <Button variant="primary" type="submit">
                    Subscribe
                </Button>

                <br/><br/>

                {showSuccess && (
                    
                    <div className="alert alert-success">
                        <strong>Success!</strong> Subscibed with email: {email}
                    </div>
                )}
            </Form>
        </Container>
        
    );
};
  
export default Home;
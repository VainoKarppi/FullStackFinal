
import { BrowserRouter, Routes, Route } from "react-router-dom";
import { Container, Form, Button } from 'react-bootstrap';

const Contact = () => {
    return (
        <Container style={{ marginTop: '50px' }}>
            <div className="card">
                <div className="card-header">
                    Contact Me
                </div>
                <div className="card-body">
                    <blockquote className="blockquote mb-0">
                    <p>This is the contact page</p>
                    </blockquote>
                </div>
            </div>
            <br/>

            <div className="card">
                <div className="card-header">
                    General
                </div>
                <div className="card-body">
                    <blockquote className="blockquote mb-0">
                    <p>General</p>
                    <Button variant="warning" type="submit">
                        general@example.com
                    </Button>
                    </blockquote>
                </div>
            </div>
            <br/>

            <div className="card">
                <div className="card-header">
                    Feedback
                </div>
                <div className="card-body">
                    <blockquote className="blockquote mb-0">
                    <p>Feedback</p>
                    <Button variant="primary" type="submit">
                        feedback@example.com
                    </Button>
                    </blockquote>
                </div>
            </div>
            <br/>

            <div className="card">
                <div className="card-header">
                    Support
                </div>
                <div className="card-body">
                    <blockquote className="blockquote mb-0">
                    <p>Support</p>
                    <Button variant="secondary" type="submit">
                        support@example.com
                    </Button>
                    </blockquote>
                </div>
            </div>
        </Container>
    );
};
  
export default Contact;
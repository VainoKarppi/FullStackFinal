import { Outlet, useLocation } from "react-router-dom";
import { Container, Navbar, Nav, Button } from 'react-bootstrap';
import React, { useState, useEffect } from 'react';

const Layout = () => {
  const [isLoggedIn, setIsLoggedIn] = useState(false);
  const location = useLocation();

  useEffect(() => {
    // Check if the user is logged in whenever the location changes
    setIsLoggedIn(sessionStorage.getItem("sessionToken") !== null);
  }, [location]);

  
  return (
    <div>
      <Navbar bg="dark" variant="dark" expand="lg">
        <Container>
          <Navbar.Brand href="/">React-Bootstrap</Navbar.Brand>

          {isLoggedIn ? (
            <>
              <Navbar.Toggle aria-controls="basic-navbar-nav" />
              <Navbar.Collapse id="basic-navbar-nav">
                <Nav className="me-auto">
                  <Nav.Link href="/tasks">Tasks</Nav.Link>
                  <Nav.Link href="/statistics">Statistics</Nav.Link> 
                  <Nav.Link href="/account">Account</Nav.Link> 
                </Nav>
              </Navbar.Collapse>
            </>
          ) : null}
          
    
          {isLoggedIn && ( // Add Logout button only, if the user has logged in
              <Button href="/logout">Logout</Button>
          )}
        </Container>
      </Navbar>
      <Outlet />
    </div>
  )
};

export default Layout;
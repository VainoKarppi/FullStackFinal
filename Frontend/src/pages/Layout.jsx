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
          <Navbar.Brand href="/">FullStackFinal</Navbar.Brand>

          {isLoggedIn ? (
            <>
              <Navbar.Toggle aria-controls="basic-navbar-nav" />
              <Navbar.Collapse id="basic-navbar-nav">
                <Nav className="me-auto">
                  <Nav.Link href="/tasks">Tasks</Nav.Link>
                  <Nav.Link href="/activity">Activities</Nav.Link> 
                  {/* <Nav.Link href="/statistics">Statistics</Nav.Link> */}
                  <Nav.Link href="/account">Account</Nav.Link> 
                </Nav>
                <Button href="/logout">Logout</Button>
              </Navbar.Collapse>
            </>
          ) : null}
        </Container>
      </Navbar>
      <Outlet />
    </div>
  )
};

export default Layout;
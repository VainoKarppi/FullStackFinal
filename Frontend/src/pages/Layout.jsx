import { Outlet, Link } from "react-router-dom";
import { Container, Navbar, Nav, Button } from 'react-bootstrap';
import React from 'react';

const Layout = () => {
  return (
    <div>
      <Navbar bg="dark" variant="dark" expand="lg">
        <Container>
          <Navbar.Brand href="/">React-Bootstrap</Navbar.Brand>
          <Navbar.Toggle aria-controls="basic-navbar-nav" />
          <Navbar.Collapse id="basic-navbar-nav">
            <Nav className="me-auto">
              <Nav.Link href="/tasks">Tasks</Nav.Link>
              <Nav.Link href="/statistics">Statistics</Nav.Link> 
              <Nav.Link href="/account">Account</Nav.Link> 
            </Nav>
          </Navbar.Collapse>
    
          {sessionStorage.getItem("sessionToken") !== null ? ( // Add Logout button only, if the user has logged in
              <Button href="/logout">Logout</Button>
          ) : null}
        </Container>
      </Navbar>
      <Outlet />
    </div>
  )
};

export default Layout;
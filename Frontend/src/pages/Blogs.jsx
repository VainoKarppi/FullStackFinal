
import React, { useState } from 'react';

import { Container, Accordion, Button, Card } from 'react-bootstrap';


const Blogs = () => {
    return (
        <Container style={{ marginTop: '50px' }}>
            <div class="card">
                <div class="card-header">
                    Blogs
                </div>
                <div class="card-body">
                    <blockquote class="blockquote mb-0">
                    <h3>Blog articles</h3>
                    <p>This is the blog page</p>
                    </blockquote>
                </div>
            </div>
            <br/>
        <Accordion defaultActiveKey="0">
          <Accordion.Item eventKey="0">
            <Accordion.Header>Accordion Item #1</Accordion.Header>
            <Accordion.Body>
            <div class="card">
                <div class="card-header">
                    Blog header 1
                </div>
                <div class="card-body">
                    <blockquote class="blockquote mb-0">
                    <h3>Blog 1</h3>
                    <p>blog text 1</p>
                    </blockquote>
                </div>
            </div>
            </Accordion.Body>
          </Accordion.Item>
          <Accordion.Item eventKey="1">
            <Accordion.Header>Accordion Item #2</Accordion.Header>
            <Accordion.Body>
            <div class="card">
                <div class="card-header">
                    Blog header 2
                </div>
                <div class="card-body">
                    <blockquote class="blockquote mb-0">
                    <h3>Blog 2</h3>
                    <p>blog text 2</p>
                    </blockquote>
                </div>
            </div>
            </Accordion.Body>
          </Accordion.Item>
          <Accordion.Item eventKey="2">
            <Accordion.Header>Accordion Item #3</Accordion.Header>
            <Accordion.Body>
            <div class="card">
                <div class="card-header">
                    Blog header 3
                </div>
                <div class="card-body">
                    <blockquote class="blockquote mb-0">
                    <h3>Blog 3</h3>
                    <p>blog text 3</p>
                    </blockquote>
                </div>
            </div>
            </Accordion.Body>
          </Accordion.Item>
        </Accordion>
        </Container>
      );
};
  
export default Blogs;
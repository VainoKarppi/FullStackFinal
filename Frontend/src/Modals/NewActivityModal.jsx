import React, { useState, useEffect } from 'react';
import { Button, Modal, Form } from 'react-bootstrap';

function NewActivityModal({ show, handleClose, onSave }) {
    const [searchQuery, setSearchQuery] = useState('');
    const [dueDate, setDueDate] = useState('');
    const [repeat, setRepeat] = useState('none'); // Options: none, daily, weekly

    // Reset the state when the modal is shown
    useEffect(() => {
        if (show) {
          setName('');
          setDescription('');
        }
    }, [show]);

    const handleSearch = (event) => {
        setSearchQuery(event.target.value);
        // Perform search logic here
    };

    const handleAddTask = (task) => {
        
    };

    const handleDueDateChange = (event) => {
        setDueDate(event.target.value);
    };

    const handleRepeatChange = (event) => {
        setRepeat(event.target.value);
    };


    return (
        <Modal show={show} onHide={handleClose} centered>
          <Modal.Header closeButton>
            <Modal.Title>Create New Activity</Modal.Title>
          </Modal.Header>
          <Modal.Body>
            <Form>
              <Form.Group controlId="taskName">
                <Form.Label>Name</Form.Label>
                <Form.Control
                  type="text"
                  placeholder="Enter task name"
                  value="cc"
                  onChange={(e) => setName(e.target.value)}
                />
              </Form.Group>
              <Form.Group controlId="taskDescription">
                <Form.Label>Description</Form.Label>
                <Form.Control
                  as="textarea"
                  rows={3}
                  placeholder="Enter task description"
                  value="a"
                  onChange={(e) => setDescription(e.target.value)}
                />
              </Form.Group>
            </Form>
          </Modal.Body>
          <Modal.Footer>
            <Button variant="secondary" onClick={handleClose}>
              Cancel
            </Button>
            <Button variant="primary" onClick={handleSave}>
              Save Task
            </Button>
          </Modal.Footer>
        </Modal>
      );
};

export default NewActivityModal;

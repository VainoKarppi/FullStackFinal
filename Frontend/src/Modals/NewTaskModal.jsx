import React, { useState, useEffect } from 'react';
import { Button, Modal, Form } from 'react-bootstrap';
import { createTask } from '../Services/taskServices';


function NewTaskModal({ show, handleClose, onSave }) {
  const [name, setName] = useState('');
  const [description, setDescription] = useState('');

    // Reset the state when the modal is shown
    useEffect(() => {
      if (show) {
        setName('');
        setDescription('');
      }
    }, [show]);

    const handleSave = async (e) => {
        e.preventDefault();

        try {
          const formData = new FormData();
          formData.append('name', name);
          formData.append('description', description);

          const task = await createTask(formData);
          onSave(task);
        } catch (error) {
            console.error('Error creating task:', error.message);
        }

        handleClose();
    };

  return (
    <Modal show={show} onHide={handleClose} centered>
      <Modal.Header closeButton>
        <Modal.Title>Create New Task</Modal.Title>
      </Modal.Header>
      <Modal.Body>
        <Form>
          <Form.Group controlId="taskName">
            <Form.Label>Name</Form.Label>
            <Form.Control
              type="text"
              placeholder="Enter task name"
              value={name}
              onChange={(e) => setName(e.target.value)}
            />
          </Form.Group>
          <Form.Group controlId="taskDescription">
            <Form.Label>Description</Form.Label>
            <Form.Control
              as="textarea"
              rows={3}
              placeholder="Enter task description"
              value={description}
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
}

export default NewTaskModal;
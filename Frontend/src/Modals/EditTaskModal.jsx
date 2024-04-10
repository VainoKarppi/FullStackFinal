import React, { useState, useEffect } from 'react';
import { Modal, Button, Form } from 'react-bootstrap';
import API_ROOT from '../config';

function EditTaskModal({ show, handleClose, taskDetails, onSave }) {
    const [editedName, setEditedName] = useState(taskDetails.name);
    const [editedDescription, setEditedDescription] = useState(taskDetails.description);
    const [editedStatus, setEditedStatus] = useState(taskDetails.status);
  

    // Update data to old data
    useEffect(() => {
        if (taskDetails) {
            setEditedName(taskDetails.name);
            setEditedDescription(taskDetails.description);
            setEditedStatus(taskDetails.status);
        }
    }, [taskDetails]);
  
    const handleSaveChanges = async (e) => {
        e.preventDefault();

        const token = sessionStorage.getItem("sessionToken");

        try {
            var updatedTask = {
                name: editedName,
                description: editedDescription,
                status: parseInt(editedStatus)
            };
            const response = await fetch(`${API_ROOT}/tasks/update/${taskDetails.id}`, {
                method: 'PATCH',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${token}`
                },
                body: JSON.stringify(updatedTask)
            });
      
            if (response.ok) {
                var returnedUpdatedTask = {
                    ...taskDetails, // Spread the existing task object
                    name: editedName,
                    description: editedDescription,
                    status: parseInt(editedStatus)
                };
                onSave(returnedUpdatedTask);
            } else {
                console.error('Failed to update task:', await response.text());
            }
        } catch (error) {
            console.error('Error updating task:', error.message);
        }

        // Handle saving edited task details here
        handleClose(); // Close the modal after saving changes
  };

  return (
    <Modal show={show} onHide={handleClose}>
      <Modal.Header closeButton>
        <Modal.Title>Edit Task</Modal.Title>
      </Modal.Header>
      <Modal.Body>
        <Form>
          <Form.Group controlId="formTaskName">
            <Form.Label>Name</Form.Label>
            <Form.Control type="text" value={editedName} onChange={(e) => setEditedName(e.target.value)} />
          </Form.Group>
          <Form.Group controlId="formTaskDescription">
            <Form.Label>Description</Form.Label>
            <Form.Control as="textarea" rows={3} value={editedDescription} onChange={(e) => setEditedDescription(e.target.value)} />
          </Form.Group>
          <Form.Group controlId="formTaskStatus">
            <Form.Label>Status</Form.Label>
            <Form.Control as="select" value={editedStatus} onChange={(e) => setEditedStatus(e.target.value)}>
              <option value={0}>In Progress</option>
              <option value={1}>Done</option>
              <option value={2}>Cancelled</option>
            </Form.Control>
          </Form.Group>
        </Form>
      </Modal.Body>
      <Modal.Footer>
        <Button variant="secondary" onClick={handleClose}>Cancel</Button>
        <Button variant="primary" onClick={handleSaveChanges}>Save Changes</Button>
      </Modal.Footer>
    </Modal>
  );
}

export default EditTaskModal;

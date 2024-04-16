import React, { useState, useEffect } from 'react';
import { Modal, Button, Form } from 'react-bootstrap';
import { updateTask } from '../Services/taskServices';

function EditTaskModal({ show, handleClose, taskDetails, onSave }) {
    const [editedName, setEditedName] = useState('');
    const [editedDescription, setEditedDescription] = useState('');
    const [editedStatus, setEditedStatus] = useState(0);
  

    // Update data to old data
    useEffect(() => {
        if (taskDetails) {
            setEditedName(taskDetails.name || '');
            setEditedDescription(taskDetails.description || '');
            setEditedStatus(taskDetails.status || 0);
        }
    }, [taskDetails]);
  
    const handleSaveChanges = async (e) => {
        e.preventDefault();


        try {
            // Update only the fields that were changed
            var updatedTask = {};
            if (editedName.trim() !== taskDetails.name) updatedTask.name = editedName;
            if (editedDescription.trim() !== taskDetails.description) updatedTask.description = editedDescription;
            const status = parseInt(editedStatus);
            if (status !== taskDetails.status) updatedTask.status = status;

            await updateTask(updatedTask, taskDetails.id);
            console.log(updatedTask);

            var returnedUpdatedTask = {
                ...taskDetails, // Spread the existing task object
                name: editedName,
                description: editedDescription,
                status: parseInt(editedStatus)
            };
            onSave(returnedUpdatedTask);

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

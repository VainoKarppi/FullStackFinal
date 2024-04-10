
import React, { useState, useEffect } from 'react';
import { Container, Form, Button, Table } from 'react-bootstrap';
import { useNavigate } from 'react-router-dom';
import { FaPen, FaTrash } from 'react-icons/fa';
import API_ROOT from '../config.js';
import NewTaskModal from '../Modals/NewTaskModal.jsx';
import EditTaskModal from '../Modals/EditTaskModal.jsx';

const Tasks = () => {
    const navigate = useNavigate();

    const [tasks, setTasks] = useState([]);
    const [editTaskDetails, setEditTaskDetails] = useState({});
    const [showCreateModal, setShowCreateModal] = useState(false);
    const [showEditModal, setShowEditModal] = useState(false);

    const handleShowCreateTaskModal = () => setShowCreateModal(true);
    const handleCloseCreateTaskModal = () => setShowCreateModal(false);
    const handleShowEditTaskModal = () => setShowEditModal(true);
    const handleCloseEditTaskModal = () => setShowEditModal(false);

    const handleCreateTaskSave = (task) => {
        console.log('Received created task data:', task);

        // Add task to list
        setTasks(prevTasks => [...prevTasks, task]); // Add the new task to the tasks array

        handleCloseCreateTaskModal(); // Close the modal after handling the data
    };

    const handleEditTaskSave = (taskData) => {
        console.log('Received updated task data:', taskData);
        
        // Find the index of the task with the same ID
        const taskIndex = tasks.findIndex(task => task.id === taskData.id);
        if (taskIndex !== -1) {
            // Update the existing task with the new data
            setTasks(prevTasks => {
                const updatedTasks = [...prevTasks];
                updatedTasks[taskIndex] = { ...updatedTasks[taskIndex], ...taskData };
                return updatedTasks;
            });
        }
        handleCloseEditTaskModal(); // Close the modal after handling the data
    };

    const handleEditTask = (task) => {
        setShowEditModal(true); // Open the modal
        setEditTaskDetails(task); // Set task details to state
    };


    const handleRemoveTask = async (taskId) => {

        const token = sessionStorage.getItem("sessionToken");

        const response = await fetch(`${API_ROOT}/tasks/delete/${taskId}`, {
            method: 'DELETE',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${token}`
            }
        });

        if (response.ok) {
            // If the deletion was successful, update the tasks list
            setTasks(prevTasks => prevTasks.filter(task => task.id !== taskId));
            console.log('Task deleted successfully', taskId);
        } else {
            console.error('Failed to delete task:', await response.text());
        }
    };



    useEffect(() => {
        async function getTasks() {

            const token = sessionStorage.getItem("sessionToken");

            // Fetch data from /tasks endpoint when the component mounts
            const response = await fetch(`${API_ROOT}/tasks`, {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${token}`
                }
            });
            
            if (response.ok) {
                setTasks(await response.json());
            } else {
                navigate("/");
            }
        }
        getTasks();
    }, []);


    function getStatusString(statusCode) {
        switch (statusCode) {
          case 0:
            return "In Progress";
          case 1:
            return "Done";
          case 2:
            return "Cancelled";
          default:
            return "Unknown";
        }
    }

    function getStatusColor(statusCode) {
        switch (statusCode) {
          case 0:
            return "blue";
          case 1:
            return "green";
          case 2:
            return "red";
          default:
            return "black";
        }
    }

    

    return (
        <Container style={{ marginTop: '50px' }}>
            <h1>Tasks</h1>

            <div>
            <Button onClick={handleShowCreateTaskModal}>Create New Task</Button>

            <NewTaskModal
                show={showCreateModal}
                handleClose={handleCloseCreateTaskModal}
                onSave={handleCreateTaskSave}
            />
            
            <EditTaskModal
                show={showEditModal}
                handleClose={handleCloseEditTaskModal}
                taskDetails={editTaskDetails}
                onSave={handleEditTaskSave}
            />
            </div>
            <br></br>
            <Table>
                <thead>
                <tr>
                    <th>ID</th>
                    <th>Title</th>
                    <th>Description</th>
                    <th>Start Date</th>
                    <th>Status</th>
                    <th>Actions</th>
                </tr>
                </thead>
                <tbody>
                {tasks.map(task => (
                    <tr key={task.id}>
                    <td>{task.id}</td>
                    <td>{task.name}</td>
                    <td>{task.description}</td>
                    <td>{task.startDateUTC}</td>
                    <td style={{color:getStatusColor(task.status)}}>{getStatusString(task.status)}</td>
                    <td>
                        <Button style={{margin:"2px"}}
                            onClick={() => handleEditTask(task)}>
                            <FaPen /> Edit
                        </Button>
                        <Button style={{margin:"2px"}} variant="danger"
                            onClick={() => handleRemoveTask(task.id)}>
                            <FaTrash /> Delete
                        </Button>
                    </td>
                    </tr>
                ))}
                </tbody>
            </Table>
        </Container>
        
    );
};
  
export default Tasks;
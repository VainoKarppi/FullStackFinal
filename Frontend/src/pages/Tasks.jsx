
import React, { useState, useEffect } from 'react';
import { Container, Form, Button, Table, InputGroup } from 'react-bootstrap';
import { useNavigate } from 'react-router-dom';
import { FaPen, FaTrash, FaSearch } from 'react-icons/fa';
import { getTasks, removeTask, getTasksByFilter, getNextTasks, getPreviousTasks } from '../Services/taskServices';
import NewTaskModal from '../Modals/NewTaskModal.jsx';
import EditTaskModal from '../Modals/EditTaskModal.jsx';
import ProtectedLayout from '../Components/ProtectedLayout.jsx';

const Tasks = () => {
    const navigate = useNavigate();

    const [tasks, setTasks] = useState([]);

    const [editTaskDetails, setEditTaskDetails] = useState({});
    const [showCreateModal, setShowCreateModal] = useState(false);
    const [showEditModal, setShowEditModal] = useState(false);

    const [currentPage, setCurrentPage] = useState(0);
    const [searchQuery, setSearchQuery] = useState("");

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

    const handleNextPage = async (taskList) => {
        let largestId = 0;
        for (const task of taskList) {
            if (task.id > largestId) {
                largestId = task.id;
            }
        }
        const tasks = await getNextTasks(largestId);
        setTasks(tasks);
        setCurrentPage(currentPage => currentPage + 1);
    };
    const handlePreviousPage = async (taskList) => {
        let lowestId = 99999;
        for (const task of taskList) {
            if (task.id < lowestId) {
                lowestId = task.id;
            }
        }
        const tasks = await getPreviousTasks(lowestId);
        setTasks(tasks);
        setCurrentPage(currentPage => currentPage - 1);
    };


    const handleRemoveTask = async (taskId) => {
        try {
            await removeTask(taskId);
            setTasks(prevTasks => prevTasks.filter(task => task.id !== taskId));
            console.log('Task deleted successfully', taskId);
        } catch (error) {
            console.error('Failed to delete task:', error);
        }
    };

    const searchTask = async () => {
        try {
            const tasks = await getTasksByFilter(searchQuery);
            setTasks(tasks);
        } catch (error) {
            console.error('Failed to search task:', error);
        }
    };

    

    var hasRun = false;
    useEffect(() => {
        if (hasRun) return; // Prevent running twice
        hasRun = true;

        async function loadTasks() {
            try {
                const tasks = await getTasks();
                console.log(tasks);
                setTasks(tasks);
            } catch (error) {
                console.error("Unable to load tasks: ", error);
                if (error.request) { // Server not responding
                    sessionStorage.clear("sessionToken");
                    sessionStorage.clear("tokenExpirationUTC");
                    navigate("/");
                }
            }
        }
        loadTasks();
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
        <ProtectedLayout>
            <Container style={{ marginTop: "50px", marginBottom: "50px" }}>
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
                <Form.Group controlId="formSearch" className="mb-3">
                    <Form.Label>Search</Form.Label>
                    <InputGroup>
                        <Form.Control
                        type="search"
                        placeholder="Search for title"
                        value={searchQuery}
                        onChange={(e) => setSearchQuery(e.target.value)}
                        style={{ maxWidth: '200px', marginRight: '10px' }} // Limiting the width and adding margin
                        />
                        <Button variant="primary" onClick={searchTask}>
                        <FaSearch /> Search
                        </Button>
                    </InputGroup>
                </Form.Group>

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
                {currentPage !== 0 && (
                    <Button onClick={() => handlePreviousPage(tasks)} style={{marginBottom:"3px"}}>Previous Page</Button>
                )}
                {tasks.length === 10 && (
                    <Button onClick={() => handleNextPage(tasks)}>Next Page</Button>
                )}
            </Container>
        </ProtectedLayout>
    );
};
  
export default Tasks;

import React, { useState, useEffect } from 'react';
import { Container, Form, Button, Table, InputGroup, Accordion, Card } from 'react-bootstrap';
import { useNavigate } from 'react-router-dom';
import { FaPen, FaTrash, FaSearch, FaCheckCircle, FaUndo } from 'react-icons/fa';

import NewActivityModal from '../Modals/NewActivityModal.jsx';
//import EditActivityModal from '../Modals/EditActivityModal.jsx';
import ProtectedLayout from '../Components/ProtectedLayout.jsx';

import { getActivities, removeActivity, updateActivity, resetActivity } from '../Services/activityServices';
import { getTask, updateTask } from '../Services/taskServices';

const Activities = () => {
    const navigate = useNavigate();
    const [showCreateModal, setShowCreateModal] = useState(false);
    const [loadingTasks, setTaskLoad] = useState(true);
    const [activities, setActivities] = useState([]);
    const [activityTasks, setActivityTasks] = useState([]);
    const [searchQuery, setSearchQuery] = useState("");
    const handleCloseCreateActivityModal = () => setShowCreateModal(false);
    const handleShowCreateTaskModal = () => setShowCreateModal(true);

    const handleCreateActivitySave = (activity) => {
        setActivities([...activities, activity]);
        handleCloseCreateActivityModal(); // Close the modal after handling the data
    };

    var hasRun = false;
    useEffect(() => {
        if (hasRun) return; // Prevent running twice
        hasRun = true;

        async function loadActivities() {
            try {
                const activities = await getActivities();
                console.log(activities);
                setActivities(activities);
            } catch (error) {
                console.error("Unable to load activities: ", error);
                if (error.request) { // Server not responding
                    sessionStorage.clear("sessionToken");
                    sessionStorage.clear("tokenExpirationUTC");
                    navigate("/");
                }
            }
        }
        loadActivities();
    }, []);
    
    const searchActivity = async (query) => {
      // If query removed, search all
      if (query === "") {
        const activities = await getActivities();
        setActivities(activities);
        return;
      }

      try {
        setActivities([]);
          // Filter activities based on the query
          const foundActivities = activities.filter(activity =>
            activity.name.toLowerCase().includes(query.toLowerCase())
          );
          setActivities(foundActivities);
      } catch (error) {
          console.error('Failed to search activity:', error);
      }
    };

    const fetchTasks = async (index) => {
      if (index === null) return; // Item closed

      setTaskLoad(true);
      setActivityTasks([]);

      const taskIds = activities[index].taskIds;
      try {
        // Get each task manually, one by one
        const fetchedTasks = [];
        for (const taskId of taskIds) {
          const data = await getTask(taskId);
          fetchedTasks.push(data);
        }

        setActivityTasks(fetchedTasks);
      } catch (error) {
        console.error(error);
      }
      setTaskLoad(false);
    };

    function getStatusString(statusCode) {
        switch (statusCode) {
          case 0:
            return "In Progress";
          case 1:
            return "Done";
          case 2:
            return "Cancelled";
          case 3:
            return "Failed";
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

    function TimeLeftMessage(targetDate) {
      // Parse the target date string to create a Date object
      const targetDateTime = new Date(targetDate).getTime();

      // Calculate the difference between the current date and the target date in milliseconds
      const now = new Date().getTime();
      const differenceMs = targetDateTime - now;

      // Convert the milliseconds difference to days, hours, and minutes
      const daysLeft = Math.floor(differenceMs / (1000 * 60 * 60 * 24));
      const hoursLeft = Math.floor((differenceMs % (1000 * 60 * 60 * 24)) / (1000 * 60 * 60));
      const minutesLeft = Math.floor((differenceMs % (1000 * 60 * 60)) / (1000 * 60));

      // Construct the time left message
      let message = '';
      if (daysLeft > 0) {
        message = `${daysLeft}d ${hoursLeft}h`;
      } else if (hoursLeft > 0) {
        message = `${hoursLeft}h ${minutesLeft}m`;
      } else {
        message = `${minutesLeft}m`;
      }

      return message;
    }

    const handleRemoveActivity = async (activityId) => {
      try {
          await removeActivity(activityId);
          
          // Update activities list
          setActivities(prevActivities => prevActivities.filter(activity => activity.id !== activityId));

          console.log('Task deleted successfully', activityId);
      } catch (error) {
          console.error('Failed to delete task:', error);
      }
    }

    const handleCompleteTask = async (taskId, activityIndex) => {

      // Send task complete update to server
      const updatedTask = {
        status: 1
      };
      await updateTask(updatedTask, taskId);

      const indexToUpdate = activityTasks.findIndex(task => task.id === taskId);
      if (indexToUpdate !== -1) {
        // Update task status in local array
        const updatedTasks = [...activityTasks];
        updatedTasks[indexToUpdate] = {
          ...updatedTasks[indexToUpdate],
          status: 1
        };

        // Update the state with the new array
        setActivityTasks(updatedTasks);

        // If all tasks are set to done -> set activity as done
        const allTasksCompleted = updatedTasks.every(task => task.status === 1);
        if (allTasksCompleted) {
          updateActivityState(activityIndex, 1);
        }
      }
    }

    const updateActivityState = async (activityIndex, state) => {
      const updatedActivity = {
        status: state,
      };
      await updateActivity(updatedActivity, activities[activityIndex].id);
      
      // Update local activities list array
      const updatedActivities = [...activities];
      updatedActivities[activityIndex] = {
        ...updatedActivities[activityIndex],
        timesCompleted: updatedActivities[activityIndex].timesCompleted + state,
        status: state
      };

      if (state === 1) {
        updatedActivities[activityIndex].endDateUTC = new Date().toISOString();
      } else {
        updatedActivities[activityIndex].endDateUTC = null;
      }

      setActivities(updatedActivities);
    }



    const handleUnCompleteTask = async (taskId, activityIndex) => {
      const updatedTask = {
        status: 0,
        endDateUTC: null
      };
      await updateTask(updatedTask, taskId);

      const indexToUpdate = activityTasks.findIndex(task => task.id === taskId);
      if (indexToUpdate !== -1) {
        // Update task status
        const updatedTasks = [...activityTasks];
        updatedTasks[indexToUpdate] = {
          ...updatedTasks[indexToUpdate],
          status: 0
        };

        // Update the state with the new array
        setActivityTasks(updatedTasks);

        // If all tasks are NOT set to done -> set activity as NOT done
        // If all tasks are set to done -> set activity as done
        const allTasksCompleted = updatedTasks.every(task => task.status === 1);
        if (!allTasksCompleted) {
          updateActivityState(activityIndex, 0);
        }
      }
    }

    
    const handleResetActivity = async (activityId) => {
      // Get tasks and reset those status to 0
      // Reset due date and endDate
      await resetActivity(activityId);
      
      // Set all tasks to status 0
      const updatedTasks = activityTasks.map(task => ({ ...task, status: 0 }));
      setActivityTasks(updatedTasks);

      // Update activity status to 0
      const updatedActivities = activities.map(activity => {
        if (activity.id === activityId) {
          return { ...activity, status: 0, endDateUTC: null };
        }
        return activity;
      });
      setActivities(updatedActivities);
    }






    
    return (
        <ProtectedLayout>
          <Container style={{ marginTop: "50px", marginBottom: "50px" }}>
            <h1>Activities</h1>

            <div>
                <Button onClick={handleShowCreateTaskModal}>Create New Activity</Button>
                <NewActivityModal
                    show={showCreateModal}
                    handleClose={handleCloseCreateActivityModal}
                    onSave={handleCreateActivitySave}
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
                  <Button variant="primary" onClick={() => searchActivity(searchQuery)}>
                  <FaSearch /> Search
                  </Button>
              </InputGroup>
            </Form.Group>
            <br></br>
            <Container style={{ marginTop: "5px", minHeight:"10rem", marginBottom: "8px", padding: '15px' }}>
              <Accordion defaultActiveKey="-1" onSelect={(index) => fetchTasks(index)}>
                {activities.map((activity, activityIndex) => (
                  <Accordion.Item key={activityIndex} eventKey={activityIndex.toString()}>
                    <Accordion.Header style={{ display: "flex" }}>
                      <span style={{ flex: 1 }}>
                        <h4>{activity.name}</h4>
                        <span style={{ color: getStatusColor(activity.status) }}> ({getStatusString(activity.status)})</span>
                      </span>
                      <span style={{ fontStyle: "italic", textAlign: "right", marginRight: "20px" }}>
                        Due: {new Date(activity.dueDateUTC).toLocaleDateString()} ({TimeLeftMessage(activity.dueDateUTC)} left)
                      </span>
                    </Accordion.Header>
                    <Accordion.Body style={{ backgroundColor: "#e0e0e0" }}>
                      {activityTasks.length === 0 || loadingTasks ? (
                        loadingTasks ? (
                          <div>
                            <p>Loading data...</p>
                          </div>
                        ) : (
                          <div>
                            <p>No Tasks Found!</p>
                            <Button style={{margin:"2px"}} variant="danger"
                              onClick={() => handleRemoveActivity(activity.id)}>
                              <FaTrash /> Delete Activity
                            </Button>
                          </div>
                        )
                      ) : (
                        
                        <div>
                          <div className="card" style={{marginBottom: "10px"}}>
                            <div className="card-header">
                                <div className="card-body">
                                  <h5>Description:</h5>
                                  <blockquote className="blockquote mb-0">
                                    <p>{activity.description}</p>
                                  </blockquote>
                                  <hr/>
                                  <h5>Statistics:</h5>
                                  <div>
                                    <p style={{ margin: "0" }}>
                                      <strong>Times completed: </strong>{activity.timesCompleted}
                                    </p>
                                    <p style={{ margin: "0" }}>
                                      <strong>Start Date: </strong>{new Date(activity.startDateUTC).toLocaleDateString()}
                                    </p>
                                    {activity.endDateUTC !== null ? (
                                      <p style={{ margin: "0" }}>
                                        <strong>End Date: </strong>{new Date(activity.endDateUTC).toLocaleDateString()}
                                      </p>
                                    ) : null}
                                  </div>
                                  <hr/>
                                  <div className="button-row" style={{ display: 'flex', gap: '5px' }}>
                                    <Button style={{margin:"2px"}}
                                        onClick={() => handleResetActivity(activity.id)}>
                                        <FaUndo /> Reset Activity
                                    </Button>
                                    <Button style={{margin:"2px"}} variant="danger"
                                      onClick={() => handleRemoveActivity(activity.id)}>
                                      <FaTrash /> Delete Activity
                                    </Button>
                                  </div>
                                </div>
                              </div>
                          </div>
                          <hr style={{ borderWidth: '2px' }}></hr>
                          <h3 style={{ textDecoration: 'underline' }}>Tasks:</h3>
                          {activityTasks.map((task, taskIndex) => (
                            <div key={taskIndex} className="card" style={{marginBottom: "10px"}}>
                              <div className="card-header">
                                {task.name}
                                <span style={{ color: getStatusColor(task.status) }}> ({getStatusString(task.status)})</span>
                              </div>
                              <div className="card-body">
                                <blockquote className="blockquote mb-0">
                                  <p>{task.description}</p>
                                </blockquote>
                              </div>
                              {task.status === 0 ? (
                                <div className="card-footer">
                                  <Button style={{margin:"2px"}}
                                      onClick={() => handleCompleteTask(task.id, activityIndex)}>
                                      <FaCheckCircle /> Mark as Completed
                                  </Button>
                                </div>
                              ) : (
                                <div className="card-footer">
                                  <Button style={{margin:"2px"}} variant="warning"
                                      onClick={() => handleUnCompleteTask(task.id, activityIndex)}>
                                      Mark as Incomplete
                                  </Button>
                                </div>
                              )}
                            </div>
                          ))}
                        </div>
                      )}
                    </Accordion.Body>
                    <hr/>
                  </Accordion.Item>
                ))}
              </Accordion>
            </Container>
          </Container>
        </ProtectedLayout>
    );
};
  
export default Activities;
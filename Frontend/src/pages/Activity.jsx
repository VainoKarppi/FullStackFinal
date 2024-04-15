
import React, { useState, useEffect } from 'react';
import { Container, Form, Button, Table, InputGroup, Accordion, Card } from 'react-bootstrap';
import { useNavigate } from 'react-router-dom';
import { FaPen, FaTrash, FaSearch } from 'react-icons/fa';

import NewActivityModal from '../Modals/NewActivityModal.jsx';
//import EditActivityModal from '../Modals/EditActivityModal.jsx';
import ProtectedLayout from '../Components/ProtectedLayout.jsx';

import { getActivities } from '../Services/activityServices';
import { getTask } from '../Services/taskServices';

const Activities = () => {
    const navigate = useNavigate();
    const [showCreateModal, setShowCreateModal] = useState(false);
    const [activities, setActivities] = useState([]);
    const [activityTasks, setActivityTasks] = useState([]);
    const handleCloseCreateActivityModal = () => setShowCreateModal(false);
    const handleShowCreateTaskModal = () => setShowCreateModal(true);

    const handleCreateActivitySave = (activity) => {
        console.log('Received created activity data:', activity);
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
    
    const fetchTasks = async (taskIds) => {
      console.log("asd");
      setActivityTasks([]);
      try {
        const fetchedTasks = await Promise.all(taskIds.map(async (taskId) => {
          return await getTask(taskId);
        }));
        console.log(fetchedTasks);
        setActivityTasks(fetchedTasks);
      } catch (error) {
        console.error(error);
      }
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

            <Container style={{ marginTop: "5px", minHeight:"10rem", marginBottom: "8px", border: '1px solid #dee2e6', padding: '15px' }}>
              <Accordion defaultActiveKey="-1">
                {activities.map((activity, index) => (
                  <Accordion.Item key={index} eventKey={index.toString()} onClick={() => fetchTasks(activity.taskIds)}>
                    <Accordion.Header style={{ display: "flex" }}>
                      <span style={{ flex: 1 }}>
                        {activity.name}
                        <span style={{ color: getStatusColor(activity.status) }}> ({getStatusString(activity.status)})</span>
                      </span>
                      <span style={{ fontStyle: "italic", textAlign: "right", marginRight: "20px" }}>
                        Due: {new Date(activity.dueDateUTC).toLocaleDateString()} ({TimeLeftMessage(activity.dueDateUTC)} left)
                      </span>
                    </Accordion.Header>
                    <Accordion.Body style={{ backgroundColor: "#e0e0e0" }}>
                      {/*
                      {activityTasks.map((task, index) => (
                        <div key={index} className="card">
                          <div className="card-header">
                            {task.name}
                          </div>
                          <div className="card-body">
                            <blockquote className="blockquote mb-0">
                              <p>{task.description}</p>
                            </blockquote>
                          </div>
                          <div className="card-footer">
                            <Button>Mark Task As Completed</Button>
                          </div>
                        </div>
                      ))}
                    */}
                  </Accordion.Body>
                  </Accordion.Item>
                ))}
              </Accordion>
            </Container>
          </Container>
        </ProtectedLayout>
    );
};
  
export default Activities;
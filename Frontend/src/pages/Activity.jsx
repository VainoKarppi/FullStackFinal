
import React, { useState, useEffect } from 'react';
import { Container, Form, Button, Table, InputGroup } from 'react-bootstrap';
import { useNavigate } from 'react-router-dom';
import { FaPen, FaTrash, FaSearch } from 'react-icons/fa';

import NewActivityModal from '../Modals/NewActivityModal.jsx';
//import EditActivityModal from '../Modals/EditActivityModal.jsx';
import ProtectedLayout from '../Components/ProtectedLayout.jsx';

const Activities = () => {
    const navigate = useNavigate();
    const [showCreateModal, setShowCreateModal] = useState(false);
    const handleCloseCreateTaskModal = () => setShowCreateModal(false);

    const handleShowCreateTaskModal = () => setShowCreateModal(true);

    const handleCreateActivitySave = (task) => {
        console.log('Received created activity data:', task);

        handleCloseCreateTaskModal(); // Close the modal after handling the data
    };

    var hasRun = false;
    useEffect(() => {
        if (hasRun) return; // Prevent running twice
        hasRun = true;

        async function loadActivities() {
            try {
                //const activities = await getActivities();
                //console.log(activities);
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
                <h1>Activities</h1>

                <div>
                    <Button onClick={handleShowCreateTaskModal}>Create New Activity</Button>

                    <NewActivityModal
                        show={showCreateModal}
                        handleClose={handleCloseCreateTaskModal}
                        onSave={handleCreateActivitySave}
                    />

                </div>
                <br></br>
            </Container>
        </ProtectedLayout>
    );
};
  
export default Activities;
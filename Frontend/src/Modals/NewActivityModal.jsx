import React, { useState, useEffect } from 'react';
import { Button, Modal, Form, FormControl, ListGroup, Container } from 'react-bootstrap';
import { FaTrash, FaSearch } from 'react-icons/fa';
import { getTasksByFilter } from '../Services/taskServices';
import { createActivity } from '../Services/activityServices';

function NewActivityModal({ show, handleClose, onSave }) {
    const [activityName, setActivityName] = useState('');
    const [searchQuery, setSearchQuery] = useState('');

    const [description, setDescription] = useState('');
    const [dueDate, setDueDate] = useState('');
    const [repeatOption, setRepeatOption] = useState('');

    const [searchResults, setSearchResults] = useState([]);
    const [selectedTasks, setSelectedTasks] = useState([]);

    const [errorMessage, setErrorMessage] = useState('');

    // Reset the state when the modal is shown
    useEffect(() => {
        if (show) {
          setSearchQuery('');
          setActivityName('');
          setSelectedTasks([]);
          setSearchResults([]);
          setRepeatOption('');

          // Calculate 1 week from now
          const oneWeekLater = new Date(new Date().getTime() + 7 * 24 * 60 * 60 * 1000);
          
          // Format the date as "YYYY-MM-DD" for input type="date"
          const formattedDate = oneWeekLater.toISOString().slice(0, 10);
          setDueDate(formattedDate);
        }
    }, [show]);


    const handleSearch = async (filter) => {
      setSearchQuery(filter);

      // Return results from server
      let results = await getTasksByFilter(filter);

      // Filter out tasks from results that are already present in selectedTasks based on task.id
      results = results.filter(result => !selectedTasks.some(task => task.id === result.id));

      // Calculate relevance score for each task. Used to sort
      results.forEach(task => {
        const score = calculateRelevanceScore(task.name, filter);
        task.relevanceScore = score;
      });

      // Sort the results array based on relevance score
      results.sort((a, b) => b.relevanceScore - a.relevanceScore);

      // Function to calculate relevance score for a task
      function calculateRelevanceScore(taskName, filter) {
        const regex = new RegExp(filter, 'gi');
        const matches = taskName.match(regex);
        return matches ? matches.length : 0;
      }

      setSearchResults(results.slice(0, 5));
    };

    const handleSelectTask = (task) => {
      setSelectedTasks([...selectedTasks, task]);
      setSearchQuery(''); // Clear search term after selecting a task
      setSearchResults([]); // Clear search results
    };

    const handleRemoveTask = (taskToRemove) => {
      const updatedTasks = selectedTasks.filter(task => task.id !== taskToRemove.id);
      setSelectedTasks(updatedTasks);
    }

    const handleSave = async (e) => {
      e.preventDefault();

      const selectedTaskIds = selectedTasks.map(task => task.id);

      console.log(activityName);
      console.log(selectedTaskIds);
      console.log(dueDate);
      console.log(repeatOption);

      const formData = new FormData();
      formData.append('name', activityName);
      formData.append('description', description);
      formData.append('tasks', selectedTaskIds);
      formData.append('due', dueDate);
      formData.append('repeat', repeatOption);

      try {
        const data = await createActivity(formData);
        console.log(data);
        handleClose();
        onSave(data);
      } catch (error) {
        console.error('Error creating task:', error);
        if (error.response.data) {
          setErrorMessage(error.response.data);
        } else {
          setErrorMessage(error.message);
        }
      }
    };



    return (
        <Modal show={show} onHide={handleClose} centered>
          <Modal.Header closeButton>
            <Modal.Title>Create New Activity</Modal.Title>
          </Modal.Header>
          <Modal.Body>
            <Form>
              <Form.Group>
                <Form.Label><h6>Activity Name</h6></Form.Label>
                <Form.Control
                  type="text"
                  placeholder="Enter task name"
                  value={activityName}
                  onChange={(e) => setActivityName(e.target.value)}
                />
              </Form.Group>
              <hr/>
              <Form.Group controlId="taskDescription">
                <Form.Label><h6>Description</h6></Form.Label>
                <Form.Control
                  as="textarea"
                  rows={2}
                  placeholder="Enter task description"
                  value={description}
                  onChange={(e) => setDescription(e.target.value)}
                />
              </Form.Group>
              <hr/>
              <Form.Group>
                <Form.Label><h6>Add Task</h6></Form.Label>
                <FormControl
                  type="text"
                  placeholder="Search for task to add..."
                  value={searchQuery}
                  onChange={(e) => setSearchQuery(e.target.value)}
                />
                <Button
                  style={{marginTop:"5px", marginBottom:"5px"}}
                  variant="primary"
                  onClick={() => handleSearch(searchQuery)}
                >
                  <FaSearch /> Search
                </Button>
                <ListGroup>
                  {searchResults.map((task, index) => (
                    <ListGroup.Item key={index} action onClick={() => handleSelectTask(task)}>
                      {task.name}
                    </ListGroup.Item>
                  ))}
                </ListGroup>
              </Form.Group>
              
              <Container style={{ marginTop: "5px", marginBottom: "8px", border: '1px solid #dee2e6', padding: '15px' }}>
                <Form.Group>
                  <Form.Label><h6>Added Tasks:</h6></Form.Label>
                  <hr style={{ marginTop: "0" }} /> {/* Move the hr higher */}
                  {selectedTasks.length === 0 ? (
                    <p style={{ fontStyle: 'italic' }}>No tasks added to list yet...</p>
                  ) : (
                    <ListGroup>
                      {selectedTasks.map((task, index) => (
                        <ListGroup.Item key={index} style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                          <div>{task.name}</div>
                          <Button
                            variant="danger"
                            onClick={() => handleRemoveTask(task)}
                          >
                            <FaTrash />
                          </Button>
                        </ListGroup.Item>
                      ))}
                    </ListGroup>
                  )}
                </Form.Group>
              </Container>
              <hr/>

              <Form.Group style={{marginBottom:"8px"}}>
                <Form.Label><h6>Due Date:</h6></Form.Label>
                <Form.Control
                  type="date"
                  value={dueDate}
                  onChange={(e) => setDueDate(e.target.value)}
                />
              </Form.Group>
              <hr/>
              <Form.Group style={{marginBottom:"8px"}}>
                <Form.Label><h6>Repeat:</h6></Form.Label>
                <Form.Control
                  as="select"
                  value={repeatOption}
                  onChange={(e) => setRepeatOption(e.target.value)}
                >
                  <option value={0}>No Repeat</option>
                  <option value={1}>Daily</option>
                  <option value={2}>Weekly</option>
                  <option value={3}>Monthly</option>
                  <option value={4}>Yearly</option>
                </Form.Control>
              </Form.Group>
            </Form>
          </Modal.Body>
          <Modal.Footer>
            {selectedTasks.length === 0 || dueDate === "" || activityName === "" || errorMessage !== "" ? (
              <div>
                {errorMessage !== "" && (
                  <p style={{fontSize: "12px", color: "red", fontStyle: 'italic', marginBottom: "0" }}>ERROR: {errorMessage}</p>
                )}
                {activityName === "" && (
                  <p style={{fontSize: "12px", color: "red", fontStyle: 'italic', marginBottom: "0" }}>(No activity name added)</p>
                )}
                {selectedTasks.length === 0 && (
                  <p style={{fontSize: "12px", color: "red", fontStyle: 'italic', marginBottom: "0" }}>(No tasks added)</p>
                )}
                {dueDate === "" && (
                  <p style={{fontSize: "12px", color: "red", fontStyle: 'italic', marginBottom: "0" }}>(No due date added)</p>
                )}
              </div>
            ) : null}
            <Button variant="secondary" onClick={handleClose}>
              Cancel
            </Button>
            <Button variant="primary" onClick={handleSave} disabled={selectedTasks.length === 0 || dueDate === "" || activityName === ""}>
              Create
            </Button>
          </Modal.Footer>
        </Modal>
      );
};

export default NewActivityModal;

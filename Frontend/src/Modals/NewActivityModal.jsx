import React, { useState, useEffect } from 'react';
import { Button, Modal, Form, FormControl, ListGroup, Container } from 'react-bootstrap';
import { FaPen, FaTrash, FaSearch } from 'react-icons/fa';

function NewActivityModal({ show, handleClose, onSave }) {
    const [activityName, setActivityName] = useState('');
    const [searchQuery, setSearchQuery] = useState('');

    // TODO
    const [dueDate, setDueDate] = useState('');
    const [repeatOption, setRepeatOption] = useState('');

    const [searchResults, setSearchResults] = useState([]);
    const [selectedTasks, setSelectedTasks] = useState([]);

    const allTasks = ["Task 1", "Task 2", "Task 3", "Task 4"];

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

    const handleSearch = (term) => {
      setSearchQuery(term);
      const results = allTasks.filter(task => task.toLowerCase().includes(term.toLowerCase()));
      setSearchResults(results);
    };

    const handleSelectTask = (task) => {
      setSelectedTasks([...selectedTasks, task]);
      setSearchQuery(''); // Clear search term after selecting a task
      setSearchResults([]); // Clear search results
    };

    const handleRemoveTask = (taskToRemove) => {
      console.log(taskToRemove);
      console.log(selectedTasks);
      const updatedTasks = selectedTasks.filter(task => task.toLowerCase() !== taskToRemove.toLowerCase());
      setSelectedTasks(updatedTasks);
    }

    const handleAddTask = () => {
      if (searchQuery.trim() !== '') {
        setSelectedTasks([...selectedTasks, searchQuery]);
        setSearchQuery('');
      }
    };

    const handleSave = async (e) => {
      e.preventDefault();
      console.log(activityName);
      console.log(selectedTasks);
      console.log(dueDate);
      console.log(repeatOption);
      handleClose();
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
              <Form.Group>
                <Form.Label>Activity Name</Form.Label>
                <Form.Control
                  type="text"
                  placeholder="Enter task name"
                  value={activityName}
                  onChange={(e) => setActivityName(e.target.value)}
                />
              </Form.Group>
              <hr/>

              <Form.Group>
                <Form.Label>Add Task</Form.Label>
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
                      {task}
                    </ListGroup.Item>
                  ))}
                </ListGroup>
              </Form.Group>
              
              <Container style={{ marginTop: "5px", marginBottom: "8px", border: '1px solid #dee2e6', padding: '15px' }}>
                <Form.Group>
                  <Form.Label><h6>Added Tasks:</h6></Form.Label>
                  <hr style={{ marginTop: "0" }} /> {/* Move the hr higher */}
                  {selectedTasks.length === 0 ? (
                    <p style={{ fontStyle: 'italic' }}>No tasks added...</p>
                  ) : (
                    <ListGroup>
                      {selectedTasks.map((task, index) => (
                        <ListGroup.Item key={index} style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                          <div>{task}</div>
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
                <Form.Label>Due Date:</Form.Label>
                <Form.Control
                  type="date"
                  value={dueDate}
                  onChange={(e) => setDueDate(e.target.value)}
                />
              </Form.Group>
              <hr/>
              <Form.Group style={{marginBottom:"8px"}}>
                <Form.Label>Repeat:</Form.Label>
                <Form.Control
                  as="select"
                  value={repeatOption}
                  onChange={(e) => setRepeatOption(e.target.value)}
                >
                  <option value="">No Repeat</option>
                  <option value="daily">Daily</option>
                  <option value="weekly">Weekly</option>
                  <option value="monthly">Monthly</option>
                </Form.Control>
              </Form.Group>
            </Form>
          </Modal.Body>
          <Modal.Footer>
            {selectedTasks.length === 0 && (
              <p style={{ fontStyle: 'italic' }}>(No tasks added)</p>
            )}
            <Button variant="secondary" onClick={handleClose}>
              Cancel
            </Button>
            <Button variant="primary" onClick={handleSave} disabled={selectedTasks.length === 0}>
              Create
            </Button>
          </Modal.Footer>
        </Modal>
      );
};

export default NewActivityModal;

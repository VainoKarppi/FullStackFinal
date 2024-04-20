import React, { useEffect, useState } from 'react';
import { Container, Form, Button, Table, InputGroup, Accordion, Card } from 'react-bootstrap';
import { useNavigate } from 'react-router-dom';
import ProtectedLayout from '../Components/ProtectedLayout';
import { getStatistics } from '../Services/statisticsServices';

const Statistics = () => {
    const navigate = useNavigate();
    const [statistics, setStatistics] = useState([]);

    var firstRun = true;
    useEffect(() => {
        if (!firstRun) return; // Make sure we dont fire this useEffect twice
        firstRun = false;

        async function loadStatistics() {
            const statistics = await getStatistics();
            console.log(statistics);
            setStatistics(statistics);
        }
        loadStatistics();
    }, []);

    return (
        <ProtectedLayout>
            <Container style={{ marginTop: "50px", marginBottom: "50px" }}>
                <h1>Statistics</h1>
                <p>Tasks completed: {statistics.tasksCompleted}</p>
                <p>Tasks in Progress: {statistics.tasksCompleted}</p>

                <p>Activities completed: {statistics.activitiesCompleted}</p>
                <p>Activities in Progress: {statistics.activitiesInProgress}</p>
            </Container>

        </ProtectedLayout>
    );
};

export default Statistics;

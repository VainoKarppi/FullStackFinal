import React, { useEffect, useState } from 'react';
import { Container, Form, Button, Table, InputGroup, Accordion, Card } from 'react-bootstrap';
import { useNavigate } from 'react-router-dom';
import ProtectedLayout from '../Components/ProtectedLayout';
import { getStatistics } from '../Services/statisticsServices';

const Statistics = () => {
    const navigate = useNavigate();

    var firstRun = true;
    useEffect(() => {
        if (!firstRun) return; // Make sure we dont fire this useEffect twice
        firstRun = false;

        async function loadStatistics() {
            await getStatistics();
        }
        loadStatistics();
    }, []);

    return (
        <ProtectedLayout>
            <Container style={{ marginTop: "50px", marginBottom: "50px" }}>
                <h1>Statistics</h1>
                <p>Total tasks completed: </p>
                <p>Total tasks created: </p>
                <p>Tasks in Progress: </p>
            </Container>

        </ProtectedLayout>
    );
};

export default Statistics;

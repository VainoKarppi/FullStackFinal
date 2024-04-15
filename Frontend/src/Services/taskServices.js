import axiosInstance from '../axiosInstance';
import { userAuthorized } from './userServices';

export const getTasks = async () => {
    if (!await userAuthorized()) throw new Error("Not authorized!");

    const token = sessionStorage.getItem("sessionToken");

    const response = await axiosInstance.get("/tasks", {
        headers: {
            'Authorization': `Bearer ${token}`
        }
    });
    return (response.data);
};

export const getTasksByFilter = async (filter) => {
    if (!await userAuthorized()) throw new Error("Not authorized!");

    const token = sessionStorage.getItem("sessionToken");

    const response = await axiosInstance.get("/tasks", {
        headers: {
            'Authorization': `Bearer ${token}`
        },
        params: {
            filter: filter
        }
    });
    return (response.data);
};

export const getNextTasks = async (lastId) => {
    if (!await userAuthorized()) throw new Error("Not authorized!");

    const token = sessionStorage.getItem("sessionToken");

    const response = await axiosInstance.get("/tasks", {
        headers: {
            'Authorization': `Bearer ${token}`
        },
        params: {
            lastTaskId: lastId
        }
    });
    return (response.data);
};

export const getPreviousTasks = async (lowestId) => {
    if (!await userAuthorized()) throw new Error("Not authorized!");

    const token = sessionStorage.getItem("sessionToken");

    const response = await axiosInstance.get("/tasks", {
        headers: {
            'Authorization': `Bearer ${token}`
        },
        params: {
            lastTaskId: lowestId,
            decend: true
        }
    });
    return (response.data);
};

export const removeTask = async (taskId) => {
    if(!await userAuthorized()) throw new Error("Not authorized!");

    const token = sessionStorage.getItem("sessionToken");

    await axiosInstance.delete(`/tasks/delete/${taskId}`, {
        headers: {
            'Authorization': `Bearer ${token}`
        }
    });
};

export const createTask = async (formData) => {
    if(!await userAuthorized()) throw new Error("Not authorized!");

    const token = sessionStorage.getItem("sessionToken");

    const response = await axiosInstance.post("/tasks/create", formData, {
        headers: {
            'Authorization': `Bearer ${token}`
        }
    });

    return (response.data);
};


export const updateTask = async (updatedTask, taskId) => {
    if(!await userAuthorized()) throw new Error("Not authorized!");

    const token = sessionStorage.getItem("sessionToken");

    const response = await axiosInstance.patch(`/tasks/update/${taskId}`, JSON.stringify(updatedTask), {
        headers: {
            'Authorization': `Bearer ${token}`
        }
    });

    return (response.data);
};


export const getTask = async (taskId) => {
    if(!await userAuthorized()) throw new Error("Not authorized!");

    const token = sessionStorage.getItem("sessionToken");

    const response = await axiosInstance.get(`/tasks/${taskId}`, {
        headers: {
            'Authorization': `Bearer ${token}`
        }
    });

    return (response.data);
};
import axiosInstance from '../axiosInstance';
import { userAuthorized } from './userServices';


export const createActivity = async (formData) => {
    if(!await userAuthorized()) throw new Error("Not authorized!");

    const token = sessionStorage.getItem("sessionToken");

    const response = await axiosInstance.post("/activities/create", formData, {
        headers: {
            'Authorization': `Bearer ${token}`
        }
    });

    return (response.data);
};

export const getActivities = async () => {
    if (!await userAuthorized()) throw new Error("Not authorized!");

    const token = sessionStorage.getItem("sessionToken");

    const response = await axiosInstance.get("/activities", {
        headers: {
            'Authorization': `Bearer ${token}`
        }
    });
    return (response.data);
};

export const removeActivity = async (activityId) => {
    if(!await userAuthorized()) throw new Error("Not authorized!");

    const token = sessionStorage.getItem("sessionToken");

    await axiosInstance.delete(`/activities/delete/${activityId}`, {
        headers: {
            'Authorization': `Bearer ${token}`
        }
    });
};

export const updateActivity = async (updatedActivity, activityId) => {
    if(!await userAuthorized()) throw new Error("Not authorized!");

    const token = sessionStorage.getItem("sessionToken");

    const response = await axiosInstance.patch(`/activities/update/${activityId}`, JSON.stringify(updatedActivity), {
        headers: {
            'Authorization': `Bearer ${token}`
        }
    });

    return (response.data);
};

export const resetActivity = async (activityId) => {
    if(!await userAuthorized()) throw new Error("Not authorized!");

    const token = sessionStorage.getItem("sessionToken");

    const response = await axiosInstance.patch(`/activities/reset/${activityId}`, null, {
        headers: {
            'Authorization': `Bearer ${token}`
        }
    });

    return (response.data);
};
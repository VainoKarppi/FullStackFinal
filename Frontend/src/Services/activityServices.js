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
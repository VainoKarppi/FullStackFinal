import axiosInstance from '../axiosInstance';
import { userAuthorized } from './userServices';


export const getStatistics = async () => {
    if(!await userAuthorized()) throw new Error("Not authorized!");

    const token = sessionStorage.getItem("sessionToken");

    const response = await axiosInstance.get(`/statistics`, null, {
        headers: {
            'Authorization': `Bearer ${token}`
        }
    });

    return (response.data);
};
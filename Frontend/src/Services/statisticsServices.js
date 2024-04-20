import axiosInstance from '../axiosInstance';
import { userAuthorized } from './userServices';


export const getStatistics = async () => {
    if(!await userAuthorized()) throw new Error("Not authorized!");

    const token = sessionStorage.getItem("sessionToken");

    const response = await axiosInstance.get(`/statistics`, {
        headers: {
            'Authorization': `Bearer ${token}`
        }
    });

    console.log(response.data);
    return (response.data);
};
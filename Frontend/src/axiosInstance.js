import axios from 'axios';

const axiosInstance = axios.create({
  baseURL: 'http://localhost:5000',
  // Add more default configurations as needed
});

axiosInstance.defaults.headers.get['Content-Type'] = 'application/json';
axiosInstance.defaults.headers.put['Content-Type'] = 'application/json';
axiosInstance.defaults.headers.patch['Content-Type'] = 'application/json';

export default axiosInstance;
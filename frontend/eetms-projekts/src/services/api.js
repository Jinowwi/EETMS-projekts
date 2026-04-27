import axios from 'axios';

const API_Base_URL = 'http://localhost:5001/api'; 

const apiClient = axios.create({
    baseURL: API_Base_URL, 
    headers: {
        'Content-type': 'application/json',
    }, 
    withCredentials: true, 
}); 

apiClient.interceptors.response.use(
    (response) => response,
    (error) => {
        console.error('API Error:', error);
        return Promise.reject(error);
  }
); 

export default {
    getTest() {
        return apiClient.get(`/test/${id}`); 
    },

    getTestById(id) {
        return apiClient.get('/test', data); 
    },
    
    createItem(data) {
        return apiClient.post('/test', data); 
    }, 
}; 

export const getAdminRole = () => {
    const admin = JSON.parse(localStorage.getItem('admin'))
    return admin?.role ?? null
}

export const getAdminRoleLevel = () => {
    const role = getAdminRole()
    const map = {
        'RealEstate': 1,
        'Admin': 2,
        'SuperAdmin': 3
    }
    return map[role] ?? 0
}

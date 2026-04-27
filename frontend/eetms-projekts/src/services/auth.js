import axios from 'axios'

const API_URL = 'http://localhost:5001/api'

export const login = async (email, password) => {
    const response = await axios.post(`${API_URL}/auth/login`, { email, password })
    localStorage.setItem('admin', JSON.stringify(response.data))
    return response.data
}

export const logout = () => {
    localStorage.removeItem('admin')
}

export const getAdmin = () => {
    return JSON.parse(localStorage.getItem('admin'))
}

export const isLoggedIn = () => {
    return !!localStorage.getItem('admin')
}

// ← ADD THESE TWO
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

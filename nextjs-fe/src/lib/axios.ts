import axios from 'axios';

const instance = axios.create({
  baseURL: process.env.NEXT_PUBLIC_API_BASE_URL,
});

instance.interceptors.request.use((config) => {
  const token = typeof window !== 'undefined'
    ? document.cookie.split('; ').find(row => row.startsWith('token='))?.split('=')[1]
    : null;

  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }

  return config;
});

export default instance;

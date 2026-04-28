import axiosClient from './axiosClient';

export const categoriesApi = {
  getAll: async () => {
    const response = await axiosClient.get('/categories');
    return response.data;
  },
  create: async (payload) => {
    const response = await axiosClient.post('/categories', payload);
    return response.data;
  },
  update: async (id, payload) => {
    const response = await axiosClient.put(`/categories/${id}`, payload);
    return response.data;
  },
  remove: async (id) => {
    await axiosClient.delete(`/categories/${id}`);
  },
};

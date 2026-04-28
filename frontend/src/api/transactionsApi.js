import axiosClient from './axiosClient';

export const transactionsApi = {
  getAll: async (params = {}) => {
    const response = await axiosClient.get('/transactions', { params });
    return response.data;
  },
  getById: async (id) => {
    const response = await axiosClient.get(`/transactions/${id}`);
    return response.data;
  },
  create: async (payload) => {
    const response = await axiosClient.post('/transactions', payload);
    return response.data;
  },
  update: async (id, payload) => {
    const response = await axiosClient.put(`/transactions/${id}`, payload);
    return response.data;
  },
  remove: async (id) => {
    await axiosClient.delete(`/transactions/${id}`);
  },
};

import axiosClient from './axiosClient';

export const budgetsApi = {
  getAll: async () => {
    const response = await axiosClient.get('/budgets');
    return response.data;
  },
  getProgress: async (period = 'current-month') => {
    const response = await axiosClient.get('/budgets/progress', { params: { period } });
    return response.data;
  },
  create: async (payload) => {
    const response = await axiosClient.post('/budgets', payload);
    return response.data;
  },
  update: async (id, payload) => {
    const response = await axiosClient.put(`/budgets/${id}`, payload);
    return response.data;
  },
  remove: async (id) => {
    await axiosClient.delete(`/budgets/${id}`);
  },
};

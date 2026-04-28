import axiosClient from './axiosClient';

export const statsApi = {
  getSummary: async (period = 'all') => {
    const response = await axiosClient.get('/stats/summary', { params: { period } });
    return response.data;
  },
  getByCategory: async ({ period = 'all', type } = {}) => {
    const response = await axiosClient.get('/stats/by-category', { params: { period, type } });
    return response.data;
  },
  getLatestTransactions: async (limit = 5) => {
    const response = await axiosClient.get('/stats/latest-transactions', { params: { limit } });
    return response.data;
  },
};

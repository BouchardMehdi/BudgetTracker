import axiosClient from './axiosClient';

export const statsApi = {
  getSummary: async () => {
    const response = await axiosClient.get('/stats/summary');
    return response.data;
  },
  getByCategory: async () => {
    const response = await axiosClient.get('/stats/by-category');
    return response.data;
  },
};

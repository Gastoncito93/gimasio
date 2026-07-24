import api from './api';

export default {
  async getAll(page = 1, pageSize = 10, search = '') {
    const response = await api.get('/plan', {
      params: {
        page,
        pageSize,
        search,
      },
    });
    return response.data;
  },

  async getById(id) {
    const response = await api.get(`/plan/${id}`);
    return response.data;
  },

  async create(data) {
    const response = await api.post('/plan', data);
    return response.data;
  },

  async update(id, data) {
    const response = await api.put(`/plan/${id}`, data);
    return response.data;
  },

  async updateEstado(id, estado) {
    const response = await api.patch(`/plan/${id}/estado`, { estado });
    return response.data;
  },

  async buscar(q, limit = 10) {
    const response = await api.get('/plan/buscar', {
      params: {
        q,
        limit,
      },
    });
    return response.data;
  },
};

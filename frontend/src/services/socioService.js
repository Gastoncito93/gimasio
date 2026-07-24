import api from './api';

export default {
  async getAll(page = 1, pageSize = 10, search = '', estado = '', idPlan = '') {
    const response = await api.get('/socio', {
      params: {
        page,
        pageSize,
        search,
        estado,
        idPlan,
      },
    });
    return response.data;
  },

  async getById(id) {
    const response = await api.get(`/socio/${id}`);
    return response.data;
  },

  async buscar(q, limit = 10) {
    const response = await api.get('/socio/buscar', {
      params: {
        q,
        limit,
      },
    });
    return response.data;
  },

  async create(data) {
    const response = await api.post('/socio', data);
    return response.data;
  },

  async update(id, data) {
    const response = await api.put(`/socio/${id}`, data);
    return response.data;
  },

  async updateEstado(id, estado) {
    const response = await api.patch(`/socio/${id}/estado`, { estado });
    return response.data;
  },
};

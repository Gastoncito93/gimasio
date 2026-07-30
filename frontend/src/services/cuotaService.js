import api from './api';

export default {
  async getAll(page = 1, pageSize = 10, search = '', estado = '', periodo = '') {
    const params = {
      page,
      pageSize,
      search,
      estado,
    };
    if (periodo) {
      params.periodo = Number(periodo);
    }
    const response = await api.get('/cuota', { params });
    return response.data;
  },

  async getById(id) {
    const response = await api.get(`/cuota/${id}`);
    return response.data;
  },

  async create(data) {
    const response = await api.post('/cuota', data);
    return response.data;
  },

  async updateObservacion(id, observacion) {
    const response = await api.put(`/cuota/${id}/observacion`, { observacion });
    return response.data;
  },

  async pagar(id, fechaPago) {
    const payload = typeof fechaPago === 'string' ? { fechaPago } : (fechaPago && fechaPago.fechaPago ? fechaPago : { fechaPago });
    const response = await api.patch(`/cuota/${id}/pagar`, payload);
    return response.data;
  },

  async anular(id) {
    const response = await api.patch(`/cuota/${id}/anular`);
    return response.data;
  },
};

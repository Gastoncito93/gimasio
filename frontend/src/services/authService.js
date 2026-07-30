import api from './api';

export default {
  async login(username, password) {
    const response = await api.post('/auth/login', { username, password });
    if (response.data && response.data.token) {
      localStorage.setItem('token', response.data.token);
      localStorage.setItem('usuario', JSON.stringify(response.data.usuario));
    }
    return response.data;
  },

  async register(data) {
    const response = await api.post('/auth/register', data);
    if (response.data && response.data.token) {
      localStorage.setItem('token', response.data.token);
      localStorage.setItem('usuario', JSON.stringify(response.data.usuario));
    }
    return response.data;
  },

  logout() {
    localStorage.removeItem('token');
    localStorage.removeItem('usuario');
  },

  getToken() {
    return localStorage.getItem('token');
  },

  getUsuario() {
    try {
      const userStr = localStorage.getItem('usuario');
      return userStr ? JSON.parse(userStr) : null;
    } catch {
      return null;
    }
  },

  setUsuario(usuario) {
    if (usuario) {
      localStorage.setItem('usuario', JSON.stringify(usuario));
    }
  },

  isAuthenticated() {
    return !!this.getToken();
  },

  hasRole(role) {
    const user = this.getUsuario();
    return user && user.rol === role;
  },

  async fetchMe() {
    const response = await api.get('/auth/me');
    if (response.data) {
      this.setUsuario(response.data);
    }
    return response.data;
  },

  async updateProfile(nombre) {
    const response = await api.put('/auth/profile', { nombre });
    if (response.data) {
      this.setUsuario(response.data);
    }
    return response.data;
  },

  async uploadAvatar(file) {
    const formData = new FormData();
    formData.append('avatar', file);

    const response = await api.post('/auth/avatar', formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    });

    if (response.data && response.data.rutaAvatar) {
      const currentUser = this.getUsuario();
      if (currentUser) {
        currentUser.rutaAvatar = response.data.rutaAvatar;
        this.setUsuario(currentUser);
      }
    }
    return response.data;
  },

  async cambiarPassword(passwordActual, nuevaPassword) {
    const response = await api.post('/auth/cambiar-password-primer-ingreso', {
      passwordActual,
      nuevaPassword
    });
    if (response.data && response.data.token) {
      localStorage.setItem('token', response.data.token);
      localStorage.setItem('usuario', JSON.stringify(response.data.usuario));
    }
    return response.data;
  },

  async cambiarPasswordPrimerIngreso(passwordActual, nuevaPassword) {
    return this.cambiarPassword(passwordActual, nuevaPassword);
  },
};

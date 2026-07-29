<script setup>
import { useRouter } from 'vue-router';
import authService from '../services/authService';

const router = useRouter();
const user = authService.getUsuario() || { username: 'Usuario', rol: 'Ninguno' };

const onLogout = () => {
  authService.logout();
  router.push('/login');
};
</script>

<template>
  <div class="layout-container">
    <aside class="sidebar">
      <div class="logo-area">
        <span class="logo-text">Gimnasio</span>
      </div>

      <nav class="nav-links">
        <router-link to="/dashboard" class="nav-item" active-class="active">
          Dashboard
        </router-link>
        <router-link to="/planes" class="nav-item" active-class="active">
          Planes
        </router-link>
        <router-link to="/socios" class="nav-item" active-class="active">
          Socios
        </router-link>
        <router-link to="/cuotas" class="nav-item" active-class="active">
          Cuotas
        </router-link>
      </nav>

      <div class="user-profile">
        <div class="user-info">
          <span class="user-name">{{ user.nombre || user.username }}</span>
          <span class="user-role">{{ user.rol }}</span>
        </div>
        <button @click="onLogout" class="btn-logout">
          Cerrar Sesión
        </button>
      </div>
    </aside>

    <main class="main-content">
      <router-view />
    </main>
  </div>
</template>

<style scoped>
.layout-container {
  display: grid;
  grid-template-columns: 260px minmax(0, 1fr);
  min-height: 100vh;
}

@media (max-width: 768px) {
  .layout-container {
    grid-template-columns: minmax(0, 1fr);
  }
  .sidebar {
    height: auto !important;
    position: static !important;
  }
}

.sidebar {
  background-color: var(--code-bg);
  border-right: 1px solid var(--border);
  padding: 30px 20px;
  display: flex;
  flex-direction: column;
  height: 100vh;
  position: sticky;
  top: 0;
  box-sizing: border-box;
}

.logo-area {
  margin-bottom: 40px;
}

.logo-text {
  font-size: 24px;
  font-weight: 700;
  color: var(--accent);
  letter-spacing: -0.5px;
}

.nav-links {
  display: flex;
  flex-direction: column;
  gap: 8px;
  flex-grow: 1;
}

.nav-item {
  display: block;
  padding: 12px 16px;
  color: var(--text);
  text-decoration: none;
  font-weight: 500;
  border-radius: 8px;
  transition: all 0.2s;
}

.nav-item:hover {
  background-color: rgba(0, 0, 0, 0.05);
  color: var(--text-h);
}

.nav-item.active {
  background-color: var(--neon-bg);
  color: var(--neon);
  border: 1px solid var(--neon-border);
}

.user-profile {
  border-top: 1px solid var(--border);
  padding-top: 20px;
  margin-top: 20px;
}

.user-info {
  display: flex;
  flex-direction: column;
  margin-bottom: 15px;
}

.user-name {
  font-weight: 600;
  color: var(--text-h);
}

.user-role {
  font-size: 13px;
  color: var(--text);
}

.btn-logout {
  width: 100%;
  padding: 10px;
  font-size: 14px;
  font-weight: 500;
  border: 1px solid var(--border);
  background-color: transparent;
  color: #c0392b;
  border-radius: 8px;
  cursor: pointer;
  transition: all 0.2s;
}

.btn-logout:hover {
  background-color: rgba(231, 76, 60, 0.08);
  border-color: rgba(231, 76, 60, 0.3);
}

.main-content {
  background-color: var(--bg);
  min-height: 100vh;
  min-width: 0;
  width: 100%;
}
</style>

<script setup>
import { ref, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import authService from '../services/authService';

const router = useRouter();
const user = ref(authService.getUsuario() || { nombre: 'Usuario', username: 'usuario', rol: 'Ninguno' });
const currentTheme = ref(localStorage.getItem('theme') || 'dark');

const setTheme = (theme) => {
  currentTheme.value = theme;
  localStorage.setItem('theme', theme);
  document.documentElement.setAttribute('data-theme', theme);
};

const goToProfile = () => {
  router.push('/profile');
};

const onLogout = () => {
  if (confirm('¿Estás seguro de que deseas cerrar sesión?')) {
    authService.logout();
    router.push('/login');
  }
};

onMounted(() => {
  const savedTheme = localStorage.getItem('theme') || 'dark';
  currentTheme.value = savedTheme;
  document.documentElement.setAttribute('data-theme', savedTheme);
});
</script>

<template>
  <div class="page-container">
    <header class="page-header">
      <div>
        <h1 class="page-title">Configuración del Sistema</h1>
        <p class="page-subtitle">Personaliza tu experiencia visual y gestiona tu sesión</p>
      </div>
    </header>

    <div class="config-grid">
      <!-- Sección Tema Visual -->
      <div class="config-card">
        <h3 class="card-title">Apariencia y Tema Visual</h3>
        <p class="card-desc">Selecciona la modalidad de color preferida para la interfaz del sistema.</p>

        <div class="theme-options">
          <button
            type="button"
            class="theme-btn"
            :class="{ active: currentTheme === 'dark' }"
            @click="setTheme('dark')"
          >
            <span class="theme-icon">🌙</span>
            <div class="theme-text">
              <span class="theme-name">Modo Nocturno</span>
              <span class="theme-desc">Fondo oscuro con bajo impacto visual</span>
            </div>
          </button>

          <button
            type="button"
            class="theme-btn"
            :class="{ active: currentTheme === 'light' }"
            @click="setTheme('light')"
          >
            <span class="theme-icon">☀️</span>
            <div class="theme-text">
              <span class="theme-name">Modo Día</span>
              <span class="theme-desc">Fondo claro y contraste alto</span>
            </div>
          </button>

          <button
            type="button"
            class="theme-btn"
            :class="{ active: currentTheme === 'pink' }"
            @click="setTheme('pink')"
          >
            <span class="theme-icon">🌸</span>
            <div class="theme-text">
              <span class="theme-name">Modo Pink / Glamour</span>
              <span class="theme-desc">Fondo rosa suave con acentos en tonos magenta y rose gold</span>
            </div>
          </button>
        </div>
      </div>

      <!-- Sección Sesión & Cuenta -->
      <div class="config-card">
        <h3 class="card-title">Sesión de Usuario</h3>
        <p class="card-desc">Información de la cuenta conectada actualmente.</p>

        <div class="user-summary">
          <div class="user-detail">
            <span class="label">Usuario Conectado:</span>
            <span class="value font-bold">{{ user.nombre || user.username }} (@{{ user.username }})</span>
          </div>
          <div class="user-detail">
            <span class="label">Rol Asignado:</span>
            <span class="role-badge">{{ user.rol }}</span>
          </div>
        </div>

        <div class="action-buttons">
          <button type="button" class="btn-secondary" @click="goToProfile">
            Editar Mi Perfil
          </button>
          <button type="button" class="btn-danger" @click="onLogout">
            Cerrar Sesión
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.page-container {
  max-width: 1120px;
  margin: 0 auto;
  padding: 28px 24px;
  box-sizing: border-box;
  text-align: left;
}

.page-header {
  margin-bottom: 24px;
  padding-bottom: 16px;
  border-bottom: 1px solid var(--border);
}

.page-title {
  margin: 0;
  font-size: 20px;
  font-weight: 600;
  letter-spacing: -0.3px;
  color: var(--text-h);
}

.page-subtitle {
  margin: 4px 0 0 0;
  font-size: 13px;
  color: var(--text);
  opacity: 0.75;
}

.config-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(340px, 1fr));
  gap: 20px;
}

.config-card {
  background-color: var(--bg);
  border: 1px solid var(--border);
  border-radius: 10px;
  padding: 24px;
  box-shadow: var(--shadow);
}

.card-title {
  margin-top: 0;
  margin-bottom: 6px;
  font-size: 16px;
  font-weight: 600;
  color: var(--text-h);
}

.card-desc {
  margin: 0 0 20px 0;
  font-size: 13px;
  color: var(--text);
  opacity: 0.8;
}

.theme-options {
  display: flex;
  flex-direction: column;
  gap: 10px;
}

.theme-btn {
  display: flex;
  align-items: center;
  gap: 14px;
  padding: 14px 16px;
  border: 1px solid var(--border);
  border-radius: 8px;
  background-color: var(--code-bg);
  color: var(--text);
  cursor: pointer;
  text-align: left;
  transition: all 0.15s ease;
}

.theme-btn:hover {
  border-color: var(--accent);
}

.theme-btn.active {
  border-color: var(--accent);
  background-color: var(--accent-bg);
  color: var(--text-h);
}

.theme-icon {
  font-size: 22px;
}

.theme-text {
  display: flex;
  flex-direction: column;
}

.theme-name {
  font-size: 14px;
  font-weight: 600;
  color: var(--text-h);
}

.theme-desc {
  font-size: 12px;
  opacity: 0.75;
  margin-top: 2px;
}

.user-summary {
  display: flex;
  flex-direction: column;
  gap: 10px;
  margin-bottom: 24px;
  background-color: var(--code-bg);
  padding: 14px 16px;
  border-radius: 8px;
  border: 1px solid var(--border);
}

.user-detail {
  display: flex;
  justify-content: space-between;
  align-items: center;
  font-size: 13px;
}

.user-detail .label {
  color: var(--text);
  opacity: 0.8;
}

.user-detail .value {
  color: var(--text-h);
}

.font-bold {
  font-weight: 600;
}

.role-badge {
  padding: 3px 10px;
  border-radius: 12px;
  font-size: 11px;
  font-weight: 600;
  background-color: var(--accent-bg);
  color: var(--accent);
  border: 1px solid var(--accent-border);
  text-transform: uppercase;
}

.action-buttons {
  display: flex;
  gap: 10px;
}

.btn-secondary {
  flex: 1;
  padding: 10px 14px;
  font-size: 13px;
  font-weight: 600;
  background-color: var(--code-bg);
  border: 1px solid var(--border);
  color: var(--text-h);
  border-radius: 8px;
  cursor: pointer;
  transition: all 0.15s ease;
}

.btn-secondary:hover {
  border-color: var(--accent);
  color: var(--accent);
}

.btn-danger {
  flex: 1;
  padding: 10px 14px;
  font-size: 13px;
  font-weight: 600;
  background-color: rgba(239, 68, 68, 0.1);
  border: 1px solid rgba(239, 68, 68, 0.3);
  color: #ef4444;
  border-radius: 8px;
  cursor: pointer;
  transition: all 0.15s ease;
}

.btn-danger:hover {
  background-color: rgba(239, 68, 68, 0.2);
}
</style>

<script setup>
import { ref, onMounted, onUnmounted } from 'vue';
import { useRouter } from 'vue-router';
import authService from '../services/authService';

const router = useRouter();
const user = ref(authService.getUsuario() || { username: 'usuario', nombre: 'Usuario', rol: 'Ninguno', rutaAvatar: null });

const refreshUser = () => {
  const current = authService.getUsuario();
  if (current) {
    user.value = current;
  }
};

const getAvatarUrl = (ruta) => {
  if (!ruta) return null;
  if (ruta.startsWith('http')) return ruta;
  return `http://localhost:5055${ruta}`;
};

const onLogout = () => {
  authService.logout();
  router.push('/login');
};

onMounted(() => {
  refreshUser();
  const savedTheme = localStorage.getItem('theme') || 'dark';
  document.documentElement.setAttribute('data-theme', savedTheme);
  window.addEventListener('storage', refreshUser);
  window.addEventListener('user-profile-updated', refreshUser);
});

onUnmounted(() => {
  window.removeEventListener('storage', refreshUser);
  window.removeEventListener('user-profile-updated', refreshUser);
});
</script>

<template>
  <div class="layout-container">
    <aside class="sidebar">
      <div class="logo-area">
        <div class="logo-badge">🏋️</div>
        <span class="logo-text">Gimnasio</span>
      </div>

      <nav class="nav-links">
        <!-- Navegación Administrador -->
        <template v-if="user.rol === 'Administrador'">
          <router-link to="/dashboard" class="nav-item" active-class="active">
            <span class="nav-icon">📊</span> Dashboard General
          </router-link>
          <router-link to="/coaches" class="nav-item" active-class="active">
            <span class="nav-icon">🏋️</span> Coaches / Equipo
          </router-link>
          <router-link to="/coach/alumnos" class="nav-item" active-class="active">
            <span class="nav-icon">📋</span> Mis Alumnos
          </router-link>
          <router-link to="/actividades" class="nav-item" active-class="active">
            <span class="nav-icon">🤸‍♂️</span> Actividades
          </router-link>
          <router-link to="/planes" class="nav-item" active-class="active">
            <span class="nav-icon">📋</span> Planes
          </router-link>
          <router-link to="/socios" class="nav-item" active-class="active">
            <span class="nav-icon">👥</span> Alumnos / Socios
          </router-link>
          <router-link to="/cuotas" class="nav-item" active-class="active">
            <span class="nav-icon">💳</span> Cuotas
          </router-link>
          <router-link to="/profile" class="nav-item" active-class="active">
            <span class="nav-icon">👤</span> Mi Perfil
          </router-link>
          <router-link to="/configuracion" class="nav-item" active-class="active">
            <span class="nav-icon">⚙️</span> Configuración
          </router-link>
        </template>

        <!-- Navegación Coach -->
        <template v-else-if="user.rol === 'Coach'">
          <router-link to="/dashboard" class="nav-item" active-class="active">
            <span class="nav-icon">📊</span> Dashboard
          </router-link>
          <router-link to="/coach/alumnos" class="nav-item" active-class="active">
            <span class="nav-icon">👥</span> Mis Alumnos
          </router-link>
          <router-link to="/profile" class="nav-item" active-class="active">
            <span class="nav-icon">👤</span> Mi Perfil
          </router-link>
          <router-link to="/configuracion" class="nav-item" active-class="active">
            <span class="nav-icon">⚙️</span> Configuración
          </router-link>
        </template>

        <!-- Navegación Alumno -->
        <template v-else-if="user.rol === 'Alumno'">
          <router-link to="/dashboard" class="nav-item" active-class="active">
            <span class="nav-icon">📊</span> Mi Panel
          </router-link>
          <router-link to="/profile" class="nav-item" active-class="active">
            <span class="nav-icon">👤</span> Mi Perfil
          </router-link>
          <router-link to="/configuracion" class="nav-item" active-class="active">
            <span class="nav-icon">⚙️</span> Configuración
          </router-link>
        </template>

        <!-- Fallback si no tiene rol -->
        <template v-else>
          <router-link to="/dashboard" class="nav-item" active-class="active">
            <span class="nav-icon">📊</span> Inicio
          </router-link>
          <router-link to="/profile" class="nav-item" active-class="active">
            <span class="nav-icon">👤</span> Mi Perfil
          </router-link>
          <router-link to="/configuracion" class="nav-item" active-class="active">
            <span class="nav-icon">⚙️</span> Configuración
          </router-link>
        </template>
      </nav>

      <div class="user-profile-card">
        <div class="avatar-container">
          <img
            v-if="user.rutaAvatar"
            :src="getAvatarUrl(user.rutaAvatar)"
            alt="Avatar"
            class="user-avatar-img"
          />
          <div v-else class="user-avatar-placeholder">
            {{ (user.nombre || user.username || 'U').charAt(0).toUpperCase() }}
          </div>
        </div>

        <div class="user-info">
          <span class="user-name">{{ user.nombre || user.username }}</span>
          <span class="user-username">@{{ user.username }}</span>
          <span
            class="user-role-badge"
            :class="{
              'badge-admin': user.rol === 'Administrador',
              'badge-coach': user.rol === 'Coach',
              'badge-alumno': user.rol === 'Alumno'
            }"
          >
            {{ user.rol }}
          </span>
        </div>

        <button @click="onLogout" class="btn-logout" title="Cerrar Sesión">
          🚪 Cerrar Sesión
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
  grid-template-columns: 270px minmax(0, 1fr);
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
  padding: 24px 18px;
  display: flex;
  flex-direction: column;
  height: 100vh;
  position: sticky;
  top: 0;
  box-sizing: border-box;
}

.logo-area {
  display: flex;
  align-items: center;
  gap: 12px;
  margin-bottom: 32px;
  padding: 0 8px;
}

.logo-badge {
  font-size: 24px;
  background: var(--neon-bg);
  border: 1px solid var(--neon-border);
  border-radius: 10px;
  width: 40px;
  height: 40px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.logo-text {
  font-size: 22px;
  font-weight: 700;
  color: var(--accent);
  letter-spacing: -0.5px;
}

.nav-links {
  display: flex;
  flex-direction: column;
  gap: 6px;
  flex-grow: 1;
}

.nav-item {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 11px 14px;
  color: var(--text);
  text-decoration: none;
  font-weight: 500;
  border-radius: 8px;
  transition: all 0.2s ease;
}

.nav-icon {
  font-size: 16px;
}

.nav-item:hover {
  background-color: rgba(0, 0, 0, 0.05);
  color: var(--text-h);
}

.nav-item.active {
  background-color: var(--neon-bg);
  color: var(--neon);
  border: 1px solid var(--neon-border);
  font-weight: 600;
}

.user-profile-card {
  border-top: 1px solid var(--border);
  padding-top: 18px;
  margin-top: 16px;
  display: flex;
  flex-direction: column;
  align-items: center;
  text-align: center;
  gap: 10px;
}

.avatar-container {
  width: 64px;
  height: 64px;
  border-radius: 50%;
  overflow: hidden;
  border: 2px solid var(--accent);
  box-shadow: 0 4px 10px rgba(0,0,0,0.1);
  display: flex;
  align-items: center;
  justify-content: center;
  background-color: var(--bg);
}

.user-avatar-img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.user-avatar-placeholder {
  width: 100%;
  height: 100%;
  display: flex;
  align-items: center;
  justify-content: center;
  background: linear-gradient(135deg, var(--accent), #4f46e5);
  color: #fff;
  font-size: 26px;
  font-weight: 700;
}

.user-info {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 2px;
}

.user-name {
  font-weight: 700;
  font-size: 15px;
  color: var(--text-h);
}

.user-username {
  font-size: 13px;
  color: var(--text);
  opacity: 0.8;
}

.user-role-badge {
  display: inline-block;
  margin-top: 4px;
  padding: 3px 10px;
  font-size: 11px;
  font-weight: 700;
  border-radius: 12px;
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

.badge-admin {
  background-color: rgba(99, 102, 241, 0.15);
  color: #6366f1;
  border: 1px solid rgba(99, 102, 241, 0.3);
}

.badge-coach {
  background-color: rgba(245, 158, 11, 0.15);
  color: #d97706;
  border: 1px solid rgba(245, 158, 11, 0.3);
}

.badge-alumno {
  background-color: rgba(16, 185, 129, 0.15);
  color: #10b981;
  border: 1px solid rgba(16, 185, 129, 0.3);
}

.btn-logout {
  width: 100%;
  padding: 9px 12px;
  font-size: 13px;
  font-weight: 600;
  border: 1px solid var(--border);
  background-color: transparent;
  color: #ef4444;
  border-radius: 8px;
  cursor: pointer;
  transition: all 0.2s ease;
  margin-top: 6px;
}

.btn-logout:hover {
  background-color: rgba(239, 68, 68, 0.08);
  border-color: rgba(239, 68, 68, 0.3);
}

.main-content {
  background-color: var(--bg);
  min-height: 100vh;
  min-width: 0;
  width: 100%;
}
</style>

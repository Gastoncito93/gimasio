<script setup>
import { ref, onMounted, onUnmounted } from 'vue';
import { useRouter } from 'vue-router';
import authService from '../services/authService';

const router = useRouter();
const user = ref(authService.getUsuario() || { username: 'usuario', nombre: 'Usuario', rol: 'Ninguno', rutaAvatar: null });

const showMandatoryPasswordModal = ref(false);
const formCurrentPass = ref('');
const formNewPass = ref('');
const formConfirmPass = ref('');
const passwordChangeErrors = ref([]);
const isSubmittingPassword = ref(false);
const passwordChangeSuccess = ref(false);

const checkDebeCambiarPassword = () => {
  const current = authService.getUsuario();
  if (current && current.debeCambiarPassword) {
    showMandatoryPasswordModal.value = true;
  } else {
    showMandatoryPasswordModal.value = false;
  }
};

const refreshUser = () => {
  const current = authService.getUsuario();
  if (current) {
    user.value = current;
    checkDebeCambiarPassword();
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

const onSubmitNewPassword = async () => {
  passwordChangeErrors.value = [];
  if (!formCurrentPass.value) {
    passwordChangeErrors.value.push('Ingrese su contraseña actual.');
    return;
  }
  if (!formNewPass.value || formNewPass.value.length < 6) {
    passwordChangeErrors.value.push('La nueva contraseña debe tener al menos 6 caracteres.');
    return;
  }
  if (formNewPass.value !== formConfirmPass.value) {
    passwordChangeErrors.value.push('La confirmación de la nueva contraseña no coincide con la nueva contraseña.');
    return;
  }
  if (formCurrentPass.value === formNewPass.value) {
    passwordChangeErrors.value.push('La nueva contraseña debe ser distinta a la contraseña actual.');
    return;
  }

  isSubmittingPassword.value = true;
  try {
    await authService.cambiarPasswordPrimerIngreso(formCurrentPass.value, formNewPass.value);
    passwordChangeSuccess.value = true;
    user.value = authService.getUsuario();
    setTimeout(() => {
      showMandatoryPasswordModal.value = false;
      passwordChangeSuccess.value = false;
      formCurrentPass.value = '';
      formNewPass.value = '';
      formConfirmPass.value = '';
    }, 1500);
  } catch (err) {
    if (err.response?.data?.errors) {
      passwordChangeErrors.value = err.response.data.errors;
    } else {
      passwordChangeErrors.value = ['Ocurrió un error al actualizar la contraseña.'];
    }
  } finally {
    isSubmittingPassword.value = false;
  }
};

onMounted(() => {
  refreshUser();
  checkDebeCambiarPassword();
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
          <div class="nav-section">
            <span class="nav-section-title">PRINCIPAL</span>
            <router-link to="/dashboard" class="nav-item" active-class="active">
              <span class="nav-icon">📊</span> Dashboard
            </router-link>
          </div>

          <div class="nav-section">
            <span class="nav-section-title">GESTIÓN</span>
            <router-link to="/socios" class="nav-item" active-class="active">
              <span class="nav-icon">👥</span> Alumnos & Coaches
            </router-link>
            <router-link to="/coach/alumnos" class="nav-item" active-class="active">
              <span class="nav-icon">📋</span> Mis Alumnos
            </router-link>
            <router-link to="/actividades" class="nav-item" active-class="active">
              <span class="nav-icon">🤸‍♂️</span> Actividades
            </router-link>
            <router-link to="/planes" class="nav-item" active-class="active">
              <span class="nav-icon">🏷️</span> Planes
            </router-link>
          </div>

          <div class="nav-section">
            <span class="nav-section-title">FINANZAS</span>
            <router-link to="/cuotas" class="nav-item" active-class="active">
              <span class="nav-icon">💳</span> Cuotas y Pagos
            </router-link>
          </div>

          <div class="nav-section">
            <span class="nav-section-title">MI CUENTA</span>
            <router-link to="/profile" class="nav-item" active-class="active">
              <span class="nav-icon">👤</span> Mi Perfil
            </router-link>
            <router-link to="/configuracion" class="nav-item" active-class="active">
              <span class="nav-icon">⚙️</span> Configuración
            </router-link>
          </div>
        </template>

        <!-- Navegación Coach -->
        <template v-else-if="user.rol === 'Coach'">
          <div class="nav-section">
            <span class="nav-section-title">PRINCIPAL</span>
            <router-link to="/dashboard" class="nav-item" active-class="active">
              <span class="nav-icon">📊</span> Dashboard
            </router-link>
          </div>

          <div class="nav-section">
            <span class="nav-section-title">ENTRENAMIENTO</span>
            <router-link to="/coach/alumnos" class="nav-item" active-class="active">
              <span class="nav-icon">👥</span> Mis Alumnos
            </router-link>
          </div>

          <div class="nav-section">
            <span class="nav-section-title">MI CUENTA</span>
            <router-link to="/profile" class="nav-item" active-class="active">
              <span class="nav-icon">👤</span> Mi Perfil
            </router-link>
            <router-link to="/configuracion" class="nav-item" active-class="active">
              <span class="nav-icon">⚙️</span> Configuración
            </router-link>
          </div>
        </template>

        <!-- Navegación Alumno -->
        <template v-else-if="user.rol === 'Alumno'">
          <div class="nav-section">
            <span class="nav-section-title">PRINCIPAL</span>
            <router-link to="/dashboard" class="nav-item" active-class="active">
              <span class="nav-icon">📊</span> Mi Panel
            </router-link>
          </div>

          <div class="nav-section">
            <span class="nav-section-title">MI CUENTA</span>
            <router-link to="/profile" class="nav-item" active-class="active">
              <span class="nav-icon">👤</span> Mi Perfil
            </router-link>
            <router-link to="/configuracion" class="nav-item" active-class="active">
              <span class="nav-icon">⚙️</span> Configuración
            </router-link>
          </div>
        </template>

        <!-- Fallback si no tiene rol -->
        <template v-else>
          <div class="nav-section">
            <span class="nav-section-title">PRINCIPAL</span>
            <router-link to="/dashboard" class="nav-item" active-class="active">
              <span class="nav-icon">📊</span> Inicio
            </router-link>
          </div>

          <div class="nav-section">
            <span class="nav-section-title">MI CUENTA</span>
            <router-link to="/profile" class="nav-item" active-class="active">
              <span class="nav-icon">👤</span> Mi Perfil
            </router-link>
            <router-link to="/configuracion" class="nav-item" active-class="active">
              <span class="nav-icon">⚙️</span> Configuración
            </router-link>
          </div>
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

    <!-- MODAL OBLIGATORIO DE CAMBIO DE CONTRASEÑA DE PRIMER INGRESO -->
    <div v-if="showMandatoryPasswordModal" class="mandatory-modal-backdrop">
      <div class="mandatory-modal-card">
        <div class="mandatory-header">
          <div class="lock-badge">🔒</div>
          <h2>Cambio de Contraseña Obligatorio</h2>
          <p class="mandatory-sub">
            Es tu primer ingreso al sistema. Por motivos de seguridad, debes actualizar tu contraseña antes de continuar.
          </p>
        </div>

        <div v-if="passwordChangeSuccess" class="alert alert-success">
          Contraseña actualizada con éxito. Accediendo al sistema...
        </div>

        <div v-if="passwordChangeErrors.length > 0" class="alert alert-error">
          <ul>
            <li v-for="(err, idx) in passwordChangeErrors" :key="idx">{{ err }}</li>
          </ul>
        </div>

        <form v-if="!passwordChangeSuccess" @submit.prevent="onSubmitNewPassword">
          <div class="form-group mb-3">
            <label class="form-label">Contraseña Actual *</label>
            <input type="password" v-model="formCurrentPass" class="form-control" required placeholder="••••••••" />
          </div>

          <div class="form-group mb-3">
            <label class="form-label">Nueva Contraseña (mínimo 6 caracteres) *</label>
            <input type="password" v-model="formNewPass" class="form-control" required minlength="6" placeholder="••••••••" />
          </div>

          <div class="form-group mb-4">
            <label class="form-label">Confirmar Nueva Contraseña *</label>
            <input type="password" v-model="formConfirmPass" class="form-control" required minlength="6" placeholder="••••••••" />
          </div>

          <button type="submit" class="btn-submit-pass" :disabled="isSubmittingPassword">
            {{ isSubmittingPassword ? 'Guardando...' : 'Actualizar Contraseña y Continuar →' }}
          </button>
        </form>
      </div>
    </div>
  </div>
</template>

<style scoped>
.layout-container {
  display: grid;
  grid-template-columns: 220px minmax(0, 1fr);
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
  padding: 16px 12px;
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
  gap: 10px;
  margin-bottom: 18px;
  padding: 0 4px;
}

.logo-badge {
  font-size: 18px;
  background: var(--neon-bg);
  border: 1px solid var(--neon-border);
  border-radius: 8px;
  width: 32px;
  height: 32px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.logo-text {
  font-size: 18px;
  font-weight: 700;
  color: var(--accent);
  letter-spacing: -0.5px;
}

.nav-links {
  display: flex;
  flex-direction: column;
  gap: 12px;
  flex-grow: 1;
  overflow-y: auto;
}

.nav-section {
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.nav-section-title {
  font-size: 10px;
  font-weight: 700;
  color: var(--text-muted, #9ca3af);
  letter-spacing: 0.8px;
  padding: 4px 10px 2px 10px;
  opacity: 0.65;
}

.nav-item {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 7px 10px;
  color: var(--text);
  text-decoration: none;
  font-size: 13.5px;
  font-weight: 500;
  border-radius: 6px;
  transition: all 0.2s ease;
}

.nav-icon {
  font-size: 14px;
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
  padding-top: 12px;
  margin-top: 10px;
  display: flex;
  flex-direction: column;
  align-items: center;
  text-align: center;
  gap: 4px;
}

.avatar-container {
  width: 46px;
  height: 46px;
  border-radius: 50%;
  overflow: hidden;
  border: 2px solid var(--accent);
  box-shadow: 0 3px 8px rgba(0,0,0,0.1);
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
  font-size: 20px;
  font-weight: 700;
}

.user-info {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 1px;
}

.user-name {
  font-weight: 700;
  font-size: 13.5px;
  color: var(--text-h);
}

.user-username {
  font-size: 11.5px;
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

.mandatory-modal-backdrop {
  position: fixed;
  top: 0;
  left: 0;
  width: 100vw;
  height: 100vh;
  background-color: rgba(0, 0, 0, 0.85);
  backdrop-filter: blur(8px);
  z-index: 99999;
  display: flex;
  justify-content: center;
  align-items: center;
  padding: 20px;
}

.mandatory-modal-card {
  background-color: var(--bg);
  border: 1px solid var(--border);
  border-radius: 16px;
  padding: 36px;
  width: 100%;
  max-width: 440px;
  box-shadow: 0 20px 40px rgba(0, 0, 0, 0.4);
}

.mandatory-header {
  text-align: center;
  margin-bottom: 24px;
}

.lock-badge {
  font-size: 36px;
  margin-bottom: 8px;
}

.mandatory-header h2 {
  font-size: 20px;
  font-weight: 700;
  color: var(--text-h);
  margin: 0 0 8px 0;
}

.mandatory-sub {
  font-size: 13px;
  color: var(--text);
  opacity: 0.8;
  line-height: 1.5;
  margin: 0;
}

.form-label {
  display: block;
  font-size: 13px;
  font-weight: 600;
  color: var(--text-h);
  margin-bottom: 6px;
}

.form-control {
  width: 100%;
  padding: 10px 14px;
  font-size: 14px;
  border: 1px solid var(--border);
  border-radius: 8px;
  background-color: var(--code-bg);
  color: var(--text-h);
  box-sizing: border-box;
}

.form-control:focus {
  outline: none;
  border-color: var(--accent);
}

.btn-submit-pass {
  width: 100%;
  padding: 12px;
  font-size: 15px;
  font-weight: 600;
  background-color: var(--accent);
  color: #fff;
  border: none;
  border-radius: 8px;
  cursor: pointer;
  transition: opacity 0.2s ease;
}

.btn-submit-pass:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.mb-3 { margin-bottom: 16px; }
.mb-4 { margin-bottom: 22px; }

.alert-success {
  background-color: rgba(16, 185, 129, 0.12);
  border: 1px solid rgba(16, 185, 129, 0.3);
  color: #10b981;
  padding: 12px;
  border-radius: 8px;
  font-size: 13px;
  text-align: center;
  margin-bottom: 16px;
}

.alert-error {
  background-color: rgba(239, 68, 68, 0.12);
  border: 1px solid rgba(239, 68, 68, 0.3);
  color: #ef4444;
  padding: 12px;
  border-radius: 8px;
  font-size: 13px;
  margin-bottom: 16px;
}
.alert-error ul { margin: 0; padding-left: 18px; }
</style>

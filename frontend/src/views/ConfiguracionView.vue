<script setup>
import { ref, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import authService from '../services/authService';

const router = useRouter();
const user = ref(authService.getUsuario() || { nombre: 'Usuario', username: 'usuario', rol: 'Ninguno' });
const currentTheme = ref(localStorage.getItem('theme') || 'dark');

// Cambiar Contraseña
const passActual = ref('');
const passNueva = ref('');
const passConfirmar = ref('');

const passErrors = ref({
  actual: '',
  nueva: '',
  confirmar: ''
});

const passSuccessMsg = ref('');
const passGlobalError = ref('');
const isChangingPass = ref(false);

const validatePassActual = () => {
  if (!passActual.value) {
    passErrors.value.actual = 'Ingresa tu contraseña actual.';
    return false;
  }
  passErrors.value.actual = '';
  return true;
};

const validatePassNueva = () => {
  const val = passNueva.value;
  if (!val) {
    passErrors.value.nueva = 'Ingresa tu nueva contraseña.';
    return false;
  }
  if (val.length < 6 || val.length > 16) {
    passErrors.value.nueva = 'La contraseña debe tener entre 6 y 16 caracteres.';
    return false;
  }
  if (!/(?=.*[a-z])(?=.*[A-Z])(?=.*\d)/.test(val)) {
    passErrors.value.nueva = 'Debe incluir al menos una letra mayúscula, una minúscula y un número.';
    return false;
  }
  if (passActual.value && val === passActual.value) {
    passErrors.value.nueva = 'La nueva contraseña no puede ser igual a la contraseña actual.';
    return false;
  }
  passErrors.value.nueva = '';
  return true;
};

const validatePassConfirmar = () => {
  if (!passConfirmar.value) {
    passErrors.value.confirmar = 'Confirma tu nueva contraseña.';
    return false;
  }
  if (passConfirmar.value !== passNueva.value) {
    passErrors.value.confirmar = 'Las contraseñas no coinciden.';
    return false;
  }
  passErrors.value.confirmar = '';
  return true;
};

const onCambiarPassword = async () => {
  passSuccessMsg.value = '';
  passGlobalError.value = '';

  const v1 = validatePassActual();
  const v2 = validatePassNueva();
  const v3 = validatePassConfirmar();

  if (!v1 || !v2 || !v3) return;

  isChangingPass.value = true;
  try {
    await authService.cambiarPassword(passActual.value, passNueva.value);
    passSuccessMsg.value = '¡Contraseña actualizada correctamente!';
    passActual.value = '';
    passNueva.value = '';
    passConfirmar.value = '';
    passErrors.value.actual = '';
    passErrors.value.nueva = '';
    passErrors.value.confirmar = '';
  } catch (err) {
    if (err.response?.data?.errors) {
      if (Array.isArray(err.response.data.errors)) {
        passGlobalError.value = err.response.data.errors.join(' ');
      } else {
        passGlobalError.value = String(err.response.data.errors);
      }
    } else {
      passGlobalError.value = 'Error al actualizar la contraseña. Verifica que la contraseña actual sea correcta.';
    }
  } finally {
    isChangingPass.value = false;
  }
};

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
        <p class="page-subtitle">Personaliza tu experiencia visual, cambia tu clave y gestiona tu sesión</p>
      </div>
    </header>

    <div class="config-grid">
      <!-- Sección Tema Visual -->
      <div class="config-card">
        <h3 class="card-title">🎨 Apariencia y Tema Visual</h3>
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

      <!-- Sección Cambiar Contraseña -->
      <div class="config-card">
        <h3 class="card-title">🔒 Seguridad y Cambiar Contraseña</h3>
        <p class="card-desc">Actualiza tu clave de acceso. Debe tener entre 6 y 16 caracteres.</p>

        <div v-if="passSuccessMsg" class="alert alert-success">
          {{ passSuccessMsg }}
        </div>
        <div v-if="passGlobalError" class="alert alert-error">
          {{ passGlobalError }}
        </div>

        <form @submit.prevent="onCambiarPassword" class="form-pass" novalidate>
          <div class="form-group">
            <label for="pass-actual">Contraseña Actual *</label>
            <input
              id="pass-actual"
              type="password"
              maxlength="16"
              v-model="passActual"
              @input="validatePassActual"
              @blur="validatePassActual"
              :class="{ 'input-error': passErrors.actual }"
              placeholder="Ingresa tu clave actual"
            />
            <span v-if="passErrors.actual" class="field-error-text">{{ passErrors.actual }}</span>
          </div>

          <div class="form-group">
            <label for="pass-nueva">Nueva Contraseña *</label>
            <input
              id="pass-nueva"
              type="password"
              maxlength="16"
              v-model="passNueva"
              @input="validatePassNueva"
              @blur="validatePassNueva"
              :class="{ 'input-error': passErrors.nueva }"
              placeholder="Entre 6 y 16 caracteres (Mayúscula, minúscula y número)"
            />
            <span v-if="passErrors.nueva" class="field-error-text">{{ passErrors.nueva }}</span>
          </div>

          <div class="form-group">
            <label for="pass-confirmar">Confirmar Nueva Contraseña *</label>
            <input
              id="pass-confirmar"
              type="password"
              maxlength="16"
              v-model="passConfirmar"
              @input="validatePassConfirmar"
              @blur="validatePassConfirmar"
              :class="{ 'input-error': passErrors.confirmar }"
              placeholder="Repite la nueva contraseña"
            />
            <span v-if="passErrors.confirmar" class="field-error-text">{{ passErrors.confirmar }}</span>
          </div>

          <button type="submit" class="btn-submit-pass" :disabled="isChangingPass">
            {{ isChangingPass ? 'Actualizando clave...' : 'Actualizar Contraseña' }}
          </button>
        </form>
      </div>

      <!-- Sección Sesión & Cuenta -->
      <div class="config-card">
        <h3 class="card-title">👤 Sesión de Usuario</h3>
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
  width: 100%;
  max-width: 1400px;
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
  border-radius: 12px;
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

/* Estilos formulario contraseña */
.form-pass {
  display: flex;
  flex-direction: column;
  gap: 14px;
}

.form-group {
  display: flex;
  flex-direction: column;
  gap: 6px;
}

.form-group label {
  font-size: 13px;
  font-weight: 600;
  color: var(--text-h);
}

.form-group input {
  width: 100%;
  padding: 10px 14px;
  font-size: 14px;
  border: 1px solid var(--border);
  border-radius: 8px;
  background-color: var(--code-bg);
  color: inherit;
  box-sizing: border-box;
  outline: none;
  transition: border-color 0.2s ease, background-color 0.2s ease;
}

.form-group input:focus {
  border-color: var(--accent);
}

.form-group input.input-error {
  border-color: #ef4444 !important;
  background-color: rgba(239, 68, 68, 0.06) !important;
}

.field-error-text {
  font-size: 12px;
  color: #ef4444;
  font-weight: 600;
  line-height: 1.3;
}

.btn-submit-pass {
  width: 100%;
  padding: 11px;
  font-size: 14px;
  font-weight: 700;
  background-color: var(--accent);
  color: #fff;
  border: none;
  border-radius: 8px;
  cursor: pointer;
  margin-top: 6px;
  transition: opacity 0.2s ease;
}

.btn-submit-pass:hover {
  opacity: 0.9;
}

.btn-submit-pass:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.alert {
  padding: 10px 14px;
  border-radius: 8px;
  font-size: 13px;
  margin-bottom: 14px;
}

.alert-success {
  background-color: rgba(16, 185, 129, 0.12);
  border: 1px solid rgba(16, 185, 129, 0.3);
  color: #10b981;
}

.alert-error {
  background-color: rgba(239, 68, 68, 0.12);
  border: 1px solid rgba(239, 68, 68, 0.3);
  color: #ef4444;
}
</style>

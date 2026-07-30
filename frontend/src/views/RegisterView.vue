<script setup>
import { ref } from 'vue';
import { useRouter } from 'vue-router';
import authService from '../services/authService';

const router = useRouter();

const nombre = ref('');
const username = ref('');
const password = ref('');
const dni = ref('');
const email = ref('');
const telefono = ref('');

const fieldErrors = ref({
  nombre: '',
  dni: '',
  username: '',
  password: '',
  email: '',
  telefono: ''
});

const errors = ref([]);
const isLoading = ref(false);

const validateNombre = () => {
  const val = nombre.value.trim();
  if (!val) {
    fieldErrors.value.nombre = 'El nombre completo es obligatorio.';
    return false;
  }
  if (!/^[a-zA-ZáéíóúñÁÉÍÓÚÑ\s]+$/.test(val)) {
    fieldErrors.value.nombre = 'El nombre solo puede contener letras y espacios (sin números ni símbolos).';
    return false;
  }
  if (val.length < 2) {
    fieldErrors.value.nombre = 'El nombre debe tener al menos 2 caracteres.';
    return false;
  }
  fieldErrors.value.nombre = '';
  return true;
};

const validateDni = () => {
  const val = dni.value.trim();
  if (!val) {
    fieldErrors.value.dni = 'El DNI es obligatorio.';
    return false;
  }
  if (!/^\d+$/.test(val)) {
    fieldErrors.value.dni = 'El DNI solo puede contener números (sin letras, puntos ni símbolos).';
    return false;
  }
  if (val.length < 7 || val.length > 10) {
    fieldErrors.value.dni = 'El DNI debe tener entre 7 y 10 dígitos.';
    return false;
  }
  fieldErrors.value.dni = '';
  return true;
};

const validateUsername = () => {
  const val = username.value.trim();
  if (!val) {
    fieldErrors.value.username = 'El nombre de usuario es obligatorio.';
    return false;
  }
  if (!/^[a-zA-Z0-9_]+$/.test(val)) {
    fieldErrors.value.username = 'El usuario solo puede contener letras, números y guión bajo _ (sin espacios ni símbolos).';
    return false;
  }
  if (val.length < 3 || val.length > 30) {
    fieldErrors.value.username = 'El usuario debe tener entre 3 y 30 caracteres.';
    return false;
  }
  fieldErrors.value.username = '';
  return true;
};

const validatePassword = () => {
  const val = password.value;
  if (!val) {
    fieldErrors.value.password = 'La contraseña es obligatoria.';
    return false;
  }
  if (val.length < 6 || val.length > 16) {
    fieldErrors.value.password = 'La contraseña debe tener entre 6 y 16 caracteres.';
    return false;
  }
  if (!/(?=.*[a-z])(?=.*[A-Z])(?=.*\d)/.test(val)) {
    fieldErrors.value.password = 'La contraseña debe incluir al menos una letra mayúscula, una minúscula y un número.';
    return false;
  }
  fieldErrors.value.password = '';
  return true;
};

const validateEmail = () => {
  const val = email.value.trim();
  if (!val) {
    fieldErrors.value.email = '';
    return true;
  }
  if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(val)) {
    fieldErrors.value.email = 'Ingresa un correo electrónico válido (ej. usuario@email.com).';
    return false;
  }
  fieldErrors.value.email = '';
  return true;
};

const validateTelefono = () => {
  const val = telefono.value.trim();
  if (!val) {
    fieldErrors.value.telefono = '';
    return true;
  }
  if (!/^[0-9\s\+\-\(\)]+$/.test(val)) {
    fieldErrors.value.telefono = 'El teléfono solo puede contener números, espacios y los símbolos + - ( ).';
    return false;
  }
  fieldErrors.value.telefono = '';
  return true;
};

const validateAll = () => {
  const v1 = validateNombre();
  const v2 = validateDni();
  const v3 = validateUsername();
  const v4 = validatePassword();
  const v5 = validateEmail();
  const v6 = validateTelefono();
  return v1 && v2 && v3 && v4 && v5 && v6;
};

const onRegister = async () => {
  errors.value = [];
  
  if (!validateAll()) {
    return;
  }

  isLoading.value = true;

  const payload = {
    nombre: nombre.value.trim(),
    username: username.value.trim(),
    password: password.value,
    rol: 'Alumno',
    dni: dni.value.trim(),
    email: email.value.trim() || null,
    telefono: telefono.value.trim() || null,
    idCoach: null
  };

  try {
    await authService.register(payload);
    window.dispatchEvent(new Event('storage'));
    window.dispatchEvent(new CustomEvent('user-profile-updated'));
    router.push('/dashboard');
  } catch (err) {
    if (err.response?.data?.errors) {
      if (Array.isArray(err.response.data.errors)) {
        errors.value = err.response.data.errors;
      } else if (typeof err.response.data.errors === 'object') {
        errors.value = Object.values(err.response.data.errors).flat();
      } else {
        errors.value = [String(err.response.data.errors)];
      }
    } else if (err.response?.data?.message) {
      errors.value = [err.response.data.message];
    } else {
      errors.value = ['Ha ocurrido un error al procesar tu inscripción. Verifica que el DNI o usuario no estén registrados.'];
    }
  } finally {
    isLoading.value = false;
  }
};

const goToLogin = () => {
  router.push('/login');
};
</script>

<template>
  <div class="register-page">
    <div class="register-card">
      <div class="header">
        <div class="logo">🏋️</div>
        <h1>Inscripción de Alumno</h1>
        <p class="subtitle">Registrate para comenzar a entrenar en el gimnasio</p>
      </div>

      <div v-if="errors.length > 0" class="alert alert-error">
        <ul>
          <li v-for="(err, idx) in errors" :key="idx">{{ err }}</li>
        </ul>
      </div>

      <form @submit.prevent="onRegister" class="form" novalidate>
        <div class="form-group">
          <label for="nombre">Nombre Completo *</label>
          <input
            id="nombre"
            type="text"
            v-model="nombre"
            @input="validateNombre"
            @blur="validateNombre"
            :class="{ 'input-error': fieldErrors.nombre }"
            placeholder="Ej. Juan Pérez"
          />
          <span v-if="fieldErrors.nombre" class="field-error-text">{{ fieldErrors.nombre }}</span>
        </div>

        <div class="form-group">
          <label for="dni">DNI *</label>
          <input
            id="dni"
            type="text"
            v-model="dni"
            @input="validateDni"
            @blur="validateDni"
            :class="{ 'input-error': fieldErrors.dni }"
            placeholder="Ej. 40123456"
          />
          <span v-if="fieldErrors.dni" class="field-error-text">{{ fieldErrors.dni }}</span>
        </div>

        <div class="form-group">
          <label for="username">Nombre de Usuario *</label>
          <input
            id="username"
            type="text"
            v-model="username"
            @input="validateUsername"
            @blur="validateUsername"
            :class="{ 'input-error': fieldErrors.username }"
            placeholder="Ej. juanperez"
          />
          <span v-if="fieldErrors.username" class="field-error-text">{{ fieldErrors.username }}</span>
        </div>

        <div class="form-group">
          <label for="password">Contraseña *</label>
          <input
            id="password"
            type="password"
            maxlength="16"
            v-model="password"
            @input="validatePassword"
            @blur="validatePassword"
            :class="{ 'input-error': fieldErrors.password }"
            placeholder="Entre 6 y 16 caracteres (mayúscula, minúscula y número)"
          />
          <span v-if="fieldErrors.password" class="field-error-text">{{ fieldErrors.password }}</span>
        </div>

        <div class="form-group">
          <label for="email">Correo Electrónico</label>
          <input
            id="email"
            type="email"
            v-model="email"
            @input="validateEmail"
            @blur="validateEmail"
            :class="{ 'input-error': fieldErrors.email }"
            placeholder="Ej. alumno@gmail.com"
          />
          <span v-if="fieldErrors.email" class="field-error-text">{{ fieldErrors.email }}</span>
        </div>

        <div class="form-group">
          <label for="telefono">Teléfono / WhatsApp</label>
          <input
            id="telefono"
            type="text"
            v-model="telefono"
            @input="validateTelefono"
            @blur="validateTelefono"
            :class="{ 'input-error': fieldErrors.telefono }"
            placeholder="Ej. 11-4455-6677"
          />
          <span v-if="fieldErrors.telefono" class="field-error-text">{{ fieldErrors.telefono }}</span>
        </div>

        <button type="submit" class="btn-submit" :disabled="isLoading">
          {{ isLoading ? 'Procesando inscripción...' : 'Completar Registro' }}
        </button>
      </form>

      <div class="footer-links">
        <p>¿Ya tienes una cuenta?</p>
        <button @click="goToLogin" class="btn-link">Inicia sesión aquí</button>
      </div>
    </div>
  </div>
</template>

<style scoped>
.register-page {
  display: flex;
  justify-content: center;
  align-items: center;
  min-height: 100vh;
  background: radial-gradient(circle at top right, var(--neon-bg), var(--bg));
  padding: 30px 20px;
  box-sizing: border-box;
}

.register-card {
  background-color: var(--bg);
  border: 1px solid var(--border);
  border-radius: 16px;
  padding: 36px;
  width: 100%;
  max-width: 440px;
  box-shadow: 0 12px 36px rgba(0, 0, 0, 0.15);
  text-align: left;
}

.header {
  text-align: center;
  margin-bottom: 24px;
}

.logo {
  font-size: 36px;
  margin-bottom: 8px;
}

.header h1 {
  font-size: 24px;
  margin: 0;
  color: var(--text-h);
}

.subtitle {
  font-size: 14px;
  color: var(--text);
  opacity: 0.8;
  margin-top: 4px;
}

.form-group {
  margin-bottom: 18px;
}

.form-group label {
  display: block;
  font-size: 13px;
  font-weight: 600;
  margin-bottom: 6px;
  color: var(--text-h);
}

.form-group input {
  width: 100%;
  padding: 11px 14px;
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
  display: block;
  font-size: 12px;
  color: #ef4444;
  margin-top: 5px;
  font-weight: 600;
  line-height: 1.3;
}

.btn-submit {
  width: 100%;
  padding: 12px;
  font-size: 15px;
  font-weight: 700;
  background-color: var(--accent);
  color: #fff;
  border: none;
  border-radius: 8px;
  cursor: pointer;
  margin-top: 10px;
  transition: opacity 0.2s ease;
}

.btn-submit:hover {
  opacity: 0.9;
}

.footer-links {
  text-align: center;
  margin-top: 24px;
  padding-top: 16px;
  border-top: 1px solid var(--border);
  font-size: 14px;
}

.footer-links p {
  margin: 0 0 6px 0;
  color: var(--text);
  opacity: 0.8;
}

.btn-link {
  background: transparent;
  border: none;
  color: var(--accent);
  font-weight: 700;
  cursor: pointer;
  text-decoration: underline;
  font-size: 14px;
}

.alert-error {
  background-color: rgba(239, 68, 68, 0.12);
  border: 1px solid rgba(239, 68, 68, 0.3);
  color: #ef4444;
  padding: 12px 16px;
  border-radius: 8px;
  font-size: 13px;
  margin-bottom: 18px;
}

.alert-error ul {
  margin: 0;
  padding-left: 18px;
}
</style>

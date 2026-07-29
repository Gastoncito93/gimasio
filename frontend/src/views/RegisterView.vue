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

const errors = ref([]);
const isLoading = ref(false);

const onRegister = async () => {
  errors.value = [];
  isLoading.value = true;

  if (!nombre.value.trim() || !username.value.trim() || !password.value || !dni.value.trim()) {
    errors.value.push('Por favor, completa todos los campos obligatorios (*).');
    isLoading.value = false;
    return;
  }

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
    if (err.response && err.response.data && err.response.data.errors) {
      errors.value = err.response.data.errors;
    } else {
      errors.value = ['Ha ocurrido un error al procesar tu inscripción. Intenta nuevamente.'];
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

      <form @submit.prevent="onRegister" class="form">
        <div class="form-group">
          <label for="nombre">Nombre Completo *</label>
          <input
            id="nombre"
            type="text"
            v-model="nombre"
            required
            placeholder="Ej. Juan Pérez"
          />
        </div>

        <div class="form-group">
          <label for="dni">DNI *</label>
          <input
            id="dni"
            type="text"
            v-model="dni"
            required
            placeholder="Ej. 40123456"
          />
        </div>

        <div class="form-group">
          <label for="username">Nombre de Usuario *</label>
          <input
            id="username"
            type="text"
            v-model="username"
            required
            placeholder="Ej. juanperez"
          />
        </div>

        <div class="form-group">
          <label for="password">Contraseña *</label>
          <input
            id="password"
            type="password"
            v-model="password"
            required
            placeholder="Mínimo 6 caracteres"
          />
        </div>

        <div class="form-group">
          <label for="email">Correo Electrónico</label>
          <input
            id="email"
            type="email"
            v-model="email"
            placeholder="Ej. alumno@gmail.com"
          />
        </div>

        <div class="form-group">
          <label for="telefono">Teléfono / WhatsApp</label>
          <input
            id="telefono"
            type="text"
            v-model="telefono"
            placeholder="Ej. 11-4455-6677"
          />
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
  margin-bottom: 16px;
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
}

.form-group input:focus {
  border-color: var(--accent);
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

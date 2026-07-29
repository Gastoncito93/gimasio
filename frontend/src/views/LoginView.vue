<script setup>
import { ref } from 'vue';
import { useRouter } from 'vue-router';
import authService from '../services/authService';

const router = useRouter();
const username = ref('');
const password = ref('');
const errors = ref([]);
const isLoading = ref(false);

const onLogin = async () => {
  errors.value = [];
  isLoading.value = true;
  try {
    await authService.login(username.value, password.value);
    router.push('/dashboard');
  } catch (err) {
    if (err.response && err.response.data && err.response.data.errors) {
      errors.value = err.response.data.errors;
    } else {
      errors.value = ['Las credenciales de ingreso son incorrectas o no hay conexión con el servidor.'];
    }
  } finally {
    isLoading.value = false;
  }
};
</script>

<template>
  <div class="login-page">
    <div class="login-card">
      <div class="logo-area">
        <h1>Gimnasio</h1>
        <p class="subtitle">Inicie sesión para acceder al sistema</p>
      </div>

      <div v-if="errors.length > 0" class="error-alert">
        <ul>
          <li v-for="(err, idx) in errors" :key="idx">{{ err }}</li>
        </ul>
      </div>

      <form @submit.prevent="onLogin">
        <div class="form-group">
          <label for="username">Usuario *</label>
          <input
            type="text"
            id="username"
            v-model="username"
            required
            placeholder="Ej. admin"
            :disabled="isLoading"
          />
        </div>

        <div class="form-group">
          <label for="password">Contraseña *</label>
          <input
            type="password"
            id="password"
            v-model="password"
            required
            placeholder="••••••••"
            :disabled="isLoading"
          />
        </div>

        <button type="submit" class="btn-login" :disabled="isLoading">
          {{ isLoading ? 'Ingresando...' : 'Ingresar' }}
        </button>
      </form>

      <div class="register-footer">
        <p>¿No tienes una cuenta aún?</p>
        <router-link to="/register" class="btn-register-link">
          Crear nueva cuenta →
        </router-link>
      </div>
    </div>
  </div>
</template>

<style scoped>
.login-page {
  display: flex;
  justify-content: center;
  align-items: center;
  min-height: 100vh;
  background-color: var(--code-bg);
  box-sizing: border-box;
}

.login-card {
  background: var(--bg);
  border: 1px solid var(--border);
  border-radius: 12px;
  padding: 40px;
  width: 100%;
  max-width: 400px;
  box-shadow: var(--shadow);
  text-align: left;
}

.logo-area {
  text-align: center;
  margin-bottom: 30px;
}

.logo-area h1 {
  margin: 0;
  font-size: 32px;
  font-weight: 700;
  color: var(--accent);
}

.subtitle {
  color: var(--text);
  font-size: 14px;
  margin-top: 6px;
}

.form-group {
  margin-bottom: 20px;
}

.form-group label {
  display: block;
  font-weight: 500;
  margin-bottom: 6px;
  font-size: 14px;
  color: var(--text-h);
}

.form-group input {
  width: 100%;
  padding: 12px;
  font-size: 15px;
  border: 1px solid var(--border);
  border-radius: 8px;
  outline: none;
  background-color: transparent;
  box-sizing: border-box;
  color: inherit;
}

.form-group input:focus {
  border-color: var(--accent);
}

.btn-login {
  width: 100%;
  padding: 12px;
  font-size: 16px;
  font-weight: 600;
  background-color: var(--accent);
  color: #fff;
  border: none;
  border-radius: 8px;
  cursor: pointer;
  transition: opacity 0.2s;
  margin-top: 10px;
}

.btn-login:disabled {
  opacity: 0.7;
  cursor: not-allowed;
}

.register-footer {
  margin-top: 24px;
  padding-top: 16px;
  border-top: 1px solid var(--border);
  text-align: center;
  font-size: 14px;
}

.register-footer p {
  margin: 0 0 6px 0;
  color: var(--text);
  opacity: 0.8;
}

.btn-register-link {
  display: inline-block;
  color: var(--accent);
  font-weight: 700;
  text-decoration: underline;
  font-size: 14px;
}
</style>

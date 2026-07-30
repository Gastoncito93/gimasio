<script setup>
import { ref } from 'vue';
import { useRouter } from 'vue-router';
import api from '../services/api';

const router = useRouter();
const step = ref(1); // 1: Solicitar código, 2: Ingresar código y nueva clave

const emailOrUsername = ref('');
const codigo6Digitos = ref('');
const nuevaPassword = ref('');
const confirmarPassword = ref('');

const isLoading = ref(false);
const errors = ref([]);
const successMsg = ref('');
const codigoDevInfo = ref('');

const onSolicitarCodigo = async () => {
  errors.value = [];
  successMsg.value = '';
  codigoDevInfo.value = '';

  if (!emailOrUsername.value.trim()) {
    errors.value.push('Debe ingresar su usuario o correo electrónico.');
    return;
  }

  isLoading.value = true;
  try {
    const res = await api.post('/auth/solicitar-recuperacion', {
      emailOrUsername: emailOrUsername.value.trim()
    });

    successMsg.value = 'Si el usuario o correo existe, se ha generado el código de 6 dígitos.';
    if (res.data.codigoDev) {
      codigoDevInfo.value = res.data.codigoDev;
      codigo6Digitos.value = res.data.codigoDev;
    }
    step.value = 2;
  } catch (err) {
    if (err.response?.data?.errors) {
      errors.value = err.response.data.errors;
    } else {
      errors.value = ['Error al procesar la solicitud de recuperación.'];
    }
  } finally {
    isLoading.value = false;
  }
};

const onRestablecerPassword = async () => {
  errors.value = [];
  successMsg.value = '';

  if (!codigo6Digitos.value.trim() || codigo6Digitos.value.trim().length !== 6) {
    errors.value.push('El código de recuperación debe tener exactamente 6 dígitos.');
    return;
  }

  if (!nuevaPassword.value || nuevaPassword.value.length < 6) {
    errors.value.push('La nueva contraseña debe tener al menos 6 caracteres.');
    return;
  }

  if (nuevaPassword.value !== confirmarPassword.value) {
    errors.value.push('La confirmación de la contraseña no coincide.');
    return;
  }

  isLoading.value = true;
  try {
    const res = await api.post('/auth/restablecer-password-codigo', {
      emailOrUsername: emailOrUsername.value.trim(),
      codigo6Digitos: codigo6Digitos.value.trim(),
      nuevaPassword: nuevaPassword.value
    });

    successMsg.value = res.data.message || 'Contraseña restablecida correctamente. Redirigiendo al login...';
    setTimeout(() => {
      router.push('/login');
    }, 2000);
  } catch (err) {
    if (err.response?.data?.errors) {
      errors.value = err.response.data.errors;
    } else {
      errors.value = ['Error al restablecer la contraseña. Verifique el código ingresado.'];
    }
  } finally {
    isLoading.value = false;
  }
};
</script>

<template>
  <div class="recovery-page">
    <div class="recovery-card">
      <div class="header-area">
        <div class="icon-badge">🔑</div>
        <h1>Recuperar Contraseña</h1>
        <p class="subtitle">
          {{ step === 1 ? 'Ingresa tu usuario o email para recibir el código de 6 dígitos' : 'Ingresa el código de 6 dígitos recibido y tu nueva contraseña' }}
        </p>
      </div>

      <div v-if="successMsg" class="alert alert-success">
        {{ successMsg }}
      </div>

      <div v-if="codigoDevInfo" class="alert alert-info">
        ⚡ <strong>Entorno de Desarrollo:</strong> Tu código de 6 dígitos es <code>{{ codigoDevInfo }}</code>
      </div>

      <div v-if="errors.length > 0" class="alert alert-error">
        <ul>
          <li v-for="(err, idx) in errors" :key="idx">{{ err }}</li>
        </ul>
      </div>

      <!-- PASO 1: Solicitar Código -->
      <form v-if="step === 1" @submit.prevent="onSolicitarCodigo">
        <div class="form-group">
          <label>Usuario o Correo Electrónico *</label>
          <input
            type="text"
            v-model="emailOrUsername"
            required
            placeholder="Ej. alumno@gimnasio.com o alumno"
            :disabled="isLoading"
          />
        </div>

        <button type="submit" class="btn-submit" :disabled="isLoading">
          {{ isLoading ? 'Enviando...' : 'Enviar Código de 6 Dígitos →' }}
        </button>
      </form>

      <!-- PASO 2: Ingresar Código y Nueva Clave -->
      <form v-else @submit.prevent="onRestablecerPassword">
        <div class="form-group">
          <label>Código de 6 Dígitos *</label>
          <input
            type="text"
            maxlength="6"
            v-model="codigo6Digitos"
            required
            placeholder="123456"
            class="code-input"
            :disabled="isLoading"
          />
        </div>

        <div class="form-group">
          <label>Nueva Contraseña (mínimo 6 caracteres) *</label>
          <input
            type="password"
            v-model="nuevaPassword"
            required
            minlength="6"
            placeholder="••••••••"
            :disabled="isLoading"
          />
        </div>

        <div class="form-group">
          <label>Confirmar Nueva Contraseña *</label>
          <input
            type="password"
            v-model="confirmarPassword"
            required
            minlength="6"
            placeholder="••••••••"
            :disabled="isLoading"
          />
        </div>

        <button type="submit" class="btn-submit" :disabled="isLoading">
          {{ isLoading ? 'Restableciendo...' : 'Restablecer Contraseña' }}
        </button>

        <button type="button" class="btn-back-step" @click="step = 1" :disabled="isLoading">
          ← Solicitar otro código
        </button>
      </form>

      <div class="footer-links">
        <router-link to="/login" class="back-link">
          ← Volver a Iniciar Sesión
        </router-link>
      </div>
    </div>
  </div>
</template>

<style scoped>
.recovery-page {
  display: flex;
  justify-content: center;
  align-items: center;
  min-height: 100vh;
  background-color: var(--code-bg);
  box-sizing: border-box;
  padding: 20px;
}

.recovery-card {
  background: var(--bg);
  border: 1px solid var(--border);
  border-radius: 16px;
  padding: 40px;
  width: 100%;
  max-width: 440px;
  box-shadow: 0 20px 40px rgba(0, 0, 0, 0.4);
  text-align: left;
}

.header-area {
  text-align: center;
  margin-bottom: 28px;
}

.icon-badge {
  font-size: 38px;
  margin-bottom: 8px;
}

.header-area h1 {
  margin: 0;
  font-size: 24px;
  font-weight: 700;
  color: var(--text-h);
}

.subtitle {
  color: var(--text);
  opacity: 0.8;
  font-size: 13px;
  margin-top: 6px;
  line-height: 1.5;
}

.form-group {
  margin-bottom: 18px;
}

.form-group label {
  display: block;
  font-weight: 600;
  margin-bottom: 6px;
  font-size: 13px;
  color: var(--text-h);
}

.form-group input {
  width: 100%;
  padding: 12px;
  font-size: 14px;
  border: 1px solid var(--border);
  border-radius: 8px;
  outline: none;
  background-color: var(--code-bg);
  box-sizing: border-box;
  color: var(--text-h);
}

.code-input {
  letter-spacing: 6px;
  font-size: 20px !important;
  font-weight: 700;
  text-align: center;
}

.form-group input:focus {
  border-color: var(--accent);
}

.btn-submit {
  width: 100%;
  padding: 13px;
  font-size: 15px;
  font-weight: 600;
  background-color: var(--accent);
  color: #fff;
  border: none;
  border-radius: 8px;
  cursor: pointer;
  transition: opacity 0.2s;
  margin-top: 8px;
}

.btn-submit:disabled {
  opacity: 0.7;
  cursor: not-allowed;
}

.btn-back-step {
  width: 100%;
  background: transparent;
  border: 1px solid var(--border);
  color: var(--text);
  padding: 10px;
  border-radius: 8px;
  font-size: 13px;
  cursor: pointer;
  margin-top: 10px;
}

.footer-links {
  margin-top: 24px;
  padding-top: 16px;
  border-top: 1px solid var(--border);
  text-align: center;
}

.back-link {
  color: var(--accent);
  font-size: 14px;
  font-weight: 600;
  text-decoration: none;
}

.back-link:hover {
  text-decoration: underline;
}

.alert-success {
  background-color: rgba(16, 185, 129, 0.12);
  border: 1px solid rgba(16, 185, 129, 0.3);
  color: #10b981;
  padding: 12px;
  border-radius: 8px;
  font-size: 13px;
  margin-bottom: 18px;
  text-align: center;
}

.alert-info {
  background-color: rgba(14, 165, 233, 0.12);
  border: 1px solid rgba(14, 165, 233, 0.3);
  color: #0ea5e9;
  padding: 12px;
  border-radius: 8px;
  font-size: 13px;
  margin-bottom: 18px;
  text-align: center;
}

.alert-error {
  background-color: rgba(239, 68, 68, 0.12);
  border: 1px solid rgba(239, 68, 68, 0.3);
  color: #ef4444;
  padding: 12px;
  border-radius: 8px;
  font-size: 13px;
  margin-bottom: 18px;
}
.alert-error ul { margin: 0; padding-left: 18px; }
</style>

<script setup>
import { useRouter } from 'vue-router';
import authService from '../services/authService';

const router = useRouter();
const user = authService.getUsuario() || { nombre: 'Usuario', rol: 'Ninguno' };

const goToPlanes = () => {
  router.push('/planes');
};

const goToSocios = () => {
  router.push('/socios');
};
</script>

<template>
  <div class="dashboard-container">
    <header class="header">
      <h1>Panel de Control</h1>
      <p class="subtitle">Bienvenido al sistema de administración del gimnasio</p>
    </header>

    <div class="welcome-card">
      <div class="welcome-text">
        <h2>¡Hola, {{ user.nombre || user.username }}!</h2>
        <p>Tu rol de acceso actual es: <strong>{{ user.rol }}</strong></p>
        <p class="desc">Usa el menú lateral o los botones para administrar planes y socios del gimnasio.</p>
      </div>
      <div class="dashboard-actions">
        <button @click="goToPlanes" class="btn-primary-dashboard">
          Administrar Planes
        </button>
        <button @click="goToSocios" class="btn-primary-dashboard btn-secondary-dash">
          Administrar Socios
        </button>
      </div>
    </div>
  </div>
</template>

<style scoped>
.dashboard-container {
  max-width: 1000px;
  margin: 0 auto;
  padding: 40px 20px;
  text-align: left;
  box-sizing: border-box;
}

.header {
  margin-bottom: 30px;
  border-bottom: 2px solid var(--border);
  padding-bottom: 20px;
}

.subtitle {
  color: var(--text);
  font-size: 16px;
  margin-top: 4px;
}

.welcome-card {
  background: var(--bg);
  border: 1px solid var(--border);
  border-radius: 12px;
  padding: 30px;
  box-shadow: var(--shadow);
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 20px;
}

@media (max-width: 600px) {
  .welcome-card {
    flex-direction: column;
    align-items: flex-start;
  }
}

.welcome-text h2 {
  margin-top: 0;
  color: var(--text-h);
}

.welcome-text p {
  margin: 6px 0;
  font-size: 16px;
}

.welcome-text .desc {
  margin-top: 12px;
  color: var(--text);
  font-size: 15px;
}

.btn-primary-dashboard {
  background-color: var(--accent);
  color: #fff;
  border: none;
  border-radius: 8px;
  padding: 12px 24px;
  font-size: 16px;
  font-weight: 600;
  cursor: pointer;
  transition: opacity 0.2s;
  white-space: nowrap;
}

.btn-primary-dashboard:hover {
  opacity: 0.9;
}

.dashboard-actions {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.btn-secondary-dash {
  background-color: transparent;
  border: 1px solid var(--border);
  color: var(--text-h);
}

.btn-secondary-dash:hover {
  background-color: var(--code-bg);
}
</style>

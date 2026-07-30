<script setup>
import { ref, onMounted } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import api from '../services/api';
import FotosProgresoModule from '../components/FotosProgresoModule.vue';

const route = useRoute();
const router = useRouter();

const alumno = ref(null);
const isLoading = ref(true);
const errorMessage = ref('');
const isForbidden = ref(false);

const getAvatarUrl = (ruta) => {
  if (!ruta) return null;
  if (ruta.startsWith('http')) return ruta;
  return `http://localhost:5055${ruta}`;
};

const fetchDetalle = async () => {
  isLoading.value = true;
  errorMessage.value = '';
  isForbidden.value = false;

  const id = route.params.id;
  try {
    const response = await api.get(`/coach/alumnos/${id}`);
    alumno.value = response.data;
  } catch (err) {
    if (err.response && err.response.status === 403) {
      isForbidden.value = true;
      errorMessage.value = '🚫 Acceso Denegado: No tienes permisos para consultar la información de este alumno.';
    } else if (err.response && err.response.status === 404) {
      errorMessage.value = `El alumno solicitado no fue encontrado.`;
    } else if (err.response && err.response.data && err.response.data.errors) {
      errorMessage.value = err.response.data.errors.join(' ');
    } else {
      errorMessage.value = 'Ocurrió un error al cargar el detalle del alumno.';
    }
  } finally {
    isLoading.value = false;
  }
};

const volverListado = () => {
  router.push('/coach/alumnos');
};

onMounted(() => {
  fetchDetalle();
});
</script>

<template>
  <div class="detalle-page">
    <button @click="volverListado" class="btn-back">
      ← Volver a Mis Alumnos
    </button>

    <div v-if="isLoading" class="loading-state">
      <div class="spinner"></div>
      <p>Cargando detalle del alumno...</p>
    </div>

    <div v-else-if="errorMessage" class="error-card">
      <h2>{{ isForbidden ? 'Acceso Denegado' : 'Error' }}</h2>
      <p>{{ errorMessage }}</p>
      <button @click="volverListado" class="btn-back-main">
        Volver a Mis Alumnos
      </button>
    </div>

    <div v-else-if="alumno" class="detalle-container">
      <div class="header-card">
        <div class="avatar-large">
          <img
            v-if="alumno.avatar"
            :src="getAvatarUrl(alumno.avatar)"
            alt="Avatar"
            class="avatar-img"
          />
          <div v-else class="avatar-placeholder">
            {{ (alumno.nombre || 'A').charAt(0).toUpperCase() }}
          </div>
        </div>

        <div class="header-info">
          <h2>{{ alumno.nombre }}</h2>
          <p v-if="alumno.username" class="username">@{{ alumno.username }}</p>
          <div class="badges-row">
            <span class="badge-status" :class="alumno.estado === 'Activo' ? 'status-active' : 'status-pause'">
              {{ alumno.estado }}
            </span>
            <span class="badge-plan">{{ alumno.planNombre }}</span>
          </div>
        </div>
      </div>

      <div class="grid-details">
        <!-- Datos Personales -->
        <div class="detail-card">
          <h3>Información Personal</h3>
          <div class="info-item">
            <span class="label">DNI:</span>
            <span class="value">{{ alumno.dni || 'No disponible todavía' }}</span>
          </div>
          <div class="info-item">
            <span class="label">Teléfono:</span>
            <span class="value">{{ alumno.telefono || 'No disponible todavía' }}</span>
          </div>
          <div class="info-item">
            <span class="label">Email:</span>
            <span class="value">{{ alumno.email || 'No disponible todavía' }}</span>
          </div>
          <div class="info-item">
            <span class="label">Fecha de Alta:</span>
            <span class="value">{{ alumno.fechaAlta ? new Date(alumno.fechaAlta).toLocaleDateString() : 'No disponible todavía' }}</span>
          </div>
        </div>

        <!-- Plan & Coach -->
        <div class="detail-card">
          <h3>Plan & Entrenador</h3>
          <div class="info-item">
            <span class="label">Plan Actual:</span>
            <span class="value font-bold text-accent">{{ alumno.planNombre || 'No disponible todavía' }}</span>
          </div>
          <div class="info-item">
            <span class="label">Precio Mensual:</span>
            <span class="value">${{ alumno.planPrecio ? alumno.planPrecio.toLocaleString() : 'No disponible todavía' }}</span>
          </div>
          <div class="info-item">
            <span class="label">Coach Asignado:</span>
            <span class="value">{{ alumno.coachNombre || 'No disponible todavía' }}</span>
          </div>
        </div>

        <!-- Estado de Deuda -->
        <div class="detail-card">
          <h3>Estado de Cuenta</h3>
          <div class="info-item">
            <span class="label">Estado de Deuda:</span>
            <span
              class="badge-deuda"
              :class="alumno.deudaEstado === 'Al día' ? 'deuda-ok' : 'deuda-pending'"
            >
              {{ alumno.deudaEstado }}
            </span>
          </div>
          <div class="info-item">
            <span class="label">Cuotas Pendientes:</span>
            <span class="value">{{ alumno.cuotasPendientesCount }}</span>
          </div>
          <div class="info-item">
            <span class="label">Próximo Vencimiento:</span>
            <span class="value">{{ alumno.proximoVencimiento || 'No disponible todavía' }}</span>
          </div>
        </div>

        <!-- Progreso & Sesiones -->
        <div class="detail-card">
          <h3>Progreso & Sesiones</h3>
          <div class="info-item">
            <span class="label">Progreso:</span>
            <span class="value text-subtle">{{ alumno.progreso || 'No disponible todavía' }}</span>
          </div>
          <div class="info-item">
            <span class="label">Evoluciones cargadas:</span>
            <span class="value font-bold text-accent">{{ alumno.cantidadEvoluciones || 0 }}</span>
          </div>
          <div class="info-item">
            <span class="label">Última Sesión:</span>
            <span class="value text-subtle">{{ alumno.ultimaSesion || 'No disponible todavía' }}</span>
          </div>
          <div class="info-item">
            <span class="label">Próxima Sesión:</span>
            <span class="value text-subtle">{{ alumno.proximaSesion || 'No disponible todavía' }}</span>
          </div>
        </div>
      </div>

      <!-- Observaciones -->
      <div class="detail-card margin-top-20">
        <h3>Observaciones</h3>
        <p class="observaciones-text">{{ alumno.observaciones || 'No disponible todavía' }}</p>
      </div>

      <!-- Módulo Fotos de Progreso & Evolución Corporal -->
      <FotosProgresoModule :id-socio="alumno.id" :can-edit="false" />
    </div>
  </div>
</template>

<style scoped>
.detalle-page {
  width: 100%;
  max-width: 1400px;
  margin: 0 auto;
  padding: 36px 24px;
  text-align: left;
  box-sizing: border-box;
}

.btn-back {
  background: transparent;
  border: 1px solid var(--border);
  color: var(--text-h);
  padding: 8px 16px;
  border-radius: 8px;
  font-size: 14px;
  font-weight: 600;
  cursor: pointer;
  margin-bottom: 24px;
  transition: all 0.2s ease;
}

.btn-back:hover {
  background-color: var(--code-bg);
  border-color: var(--accent);
}

.loading-state, .error-card {
  background-color: var(--bg);
  border: 1px solid var(--border);
  border-radius: 12px;
  padding: 48px 24px;
  text-align: center;
  box-shadow: var(--shadow);
}

.error-icon {
  font-size: 44px;
  display: block;
  margin-bottom: 12px;
}

.error-card h2 {
  margin: 0 0 8px 0;
  color: var(--text-h);
}

.error-card p {
  color: var(--text);
  margin-bottom: 20px;
  opacity: 0.9;
}

.btn-back-main {
  padding: 10px 20px;
  font-size: 14px;
  font-weight: 600;
  background-color: var(--accent);
  color: #fff;
  border: none;
  border-radius: 8px;
  cursor: pointer;
}

.spinner {
  width: 32px;
  height: 32px;
  border: 3px solid var(--border);
  border-top-color: var(--accent);
  border-radius: 50%;
  animation: spin 0.8s linear infinite;
  margin: 0 auto 16px auto;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}

.detalle-container {
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.header-card {
  background-color: var(--bg);
  border: 1px solid var(--border);
  border-radius: 12px;
  padding: 24px;
  box-shadow: var(--shadow);
  display: flex;
  align-items: center;
  gap: 20px;
}

.avatar-large {
  width: 80px;
  height: 80px;
  border-radius: 50%;
  overflow: hidden;
  border: 3px solid var(--accent);
  flex-shrink: 0;
}

.avatar-img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.avatar-placeholder {
  width: 100%;
  height: 100%;
  background: linear-gradient(135deg, var(--accent), #4f46e5);
  color: #fff;
  font-size: 36px;
  font-weight: 700;
  display: flex;
  align-items: center;
  justify-content: center;
}

.header-info h2 {
  margin: 0 0 4px 0;
  font-size: 24px;
  color: var(--text-h);
}

.username {
  margin: 0 0 8px 0;
  font-size: 14px;
  color: var(--text);
  opacity: 0.7;
}

.badges-row {
  display: flex;
  gap: 8px;
}

.badge-status {
  padding: 4px 10px;
  border-radius: 12px;
  font-size: 12px;
  font-weight: 600;
}

.status-active {
  background-color: rgba(16, 185, 129, 0.12);
  color: #10b981;
  border: 1px solid rgba(16, 185, 129, 0.3);
}

.status-pause {
  background-color: rgba(245, 158, 11, 0.12);
  color: #d97706;
  border: 1px solid rgba(245, 158, 11, 0.3);
}

.badge-plan {
  font-size: 12px;
  font-weight: 600;
  color: var(--accent);
  background-color: var(--neon-bg);
  border: 1px solid var(--neon-border);
  padding: 4px 10px;
  border-radius: 12px;
}

.grid-details {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(380px, 1fr));
  gap: 20px;
}

@media (max-width: 600px) {
  .grid-details {
    grid-template-columns: 1fr;
  }
}

.detail-card {
  background-color: var(--bg);
  border: 1px solid var(--border);
  border-radius: 12px;
  padding: 22px;
  box-shadow: var(--shadow);
}

.detail-card h3 {
  margin: 0 0 16px 0;
  font-size: 17px;
  color: var(--text-h);
  border-bottom: 1px solid var(--border);
  padding-bottom: 10px;
}

.info-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 8px 0;
  border-bottom: 1px dashed var(--border);
  font-size: 14px;
}

.info-item:last-child {
  border-bottom: none;
}

.label {
  color: var(--text);
  opacity: 0.8;
}

.value {
  color: var(--text-h);
  font-weight: 500;
}

.font-bold { font-weight: 600; }
.text-accent { color: var(--accent); }
.text-subtle { font-style: italic; opacity: 0.8; }

.badge-deuda {
  padding: 3px 8px;
  border-radius: 10px;
  font-size: 12px;
  font-weight: 600;
}

.deuda-ok {
  background-color: rgba(16, 185, 129, 0.12);
  color: #10b981;
  border: 1px solid rgba(16, 185, 129, 0.3);
}

.deuda-pending {
  background-color: rgba(239, 68, 68, 0.12);
  color: #ef4444;
  border: 1px solid rgba(239, 68, 68, 0.3);
}

.observaciones-text {
  margin: 0;
  font-size: 14px;
  color: var(--text);
  line-height: 1.5;
  background-color: var(--code-bg);
  padding: 12px 16px;
  border-radius: 8px;
  border: 1px solid var(--border);
}

.margin-top-20 {
  margin-top: 4px;
}
</style>

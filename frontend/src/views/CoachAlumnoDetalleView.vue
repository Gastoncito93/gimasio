<script setup>
import { ref, onMounted, computed } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import api from '../services/api';
import authService from '../services/authService';
import socioService from '../services/socioService';
import FotosProgresoModule from '../components/FotosProgresoModule.vue';
import { getCoachBadgeStyle, getPlanBadgeStyle } from '../utils/badgeStyles';

const route = useRoute();
const router = useRouter();

const alumno = ref(null);
const isLoading = ref(true);
const errorMessage = ref('');
const isForbidden = ref(false);

const isAdmin = computed(() => authService.hasRole('Administrador'));
const coaches = ref([]);
const showAssignModal = ref(false);
const selectedActividadNombre = ref('');
const selectedCoachId = ref('');
const isSubmittingAssign = ref(false);
const actividades = ref([]);

const loadCoaches = async () => {
  try {
    const [cRes, aRes] = await Promise.all([
      api.get('/socio/coaches'),
      api.get('/actividades')
    ]);
    coaches.value = cRes.data || [];
    actividades.value = aRes.data || [];
  } catch (err) {
    console.error('Error cargando coaches y actividades', err);
  }
};

const filteredCoaches = computed(() => {
  if (!selectedActividadNombre.value) return coaches.value;
  return coaches.value.filter(c => {
    if (!c.actividadNombre) return true;
    return c.actividadNombre.toLowerCase().trim() === selectedActividadNombre.value.toLowerCase().trim();
  });
});

const onActividadChange = () => {
  if (selectedCoachId.value) {
    const currentCoach = coaches.value.find(c => c.id === Number(selectedCoachId.value));
    if (currentCoach && currentCoach.actividadNombre && selectedActividadNombre.value) {
      if (currentCoach.actividadNombre.toLowerCase().trim() !== selectedActividadNombre.value.toLowerCase().trim()) {
        selectedCoachId.value = '';
      }
    }
  }
};

const onCoachChange = () => {
  if (selectedCoachId.value) {
    const currentCoach = coaches.value.find(c => c.id === Number(selectedCoachId.value));
    if (currentCoach && currentCoach.actividadNombre) {
      selectedActividadNombre.value = currentCoach.actividadNombre;
    }
  }
};

const openAssignModal = () => {
  if (alumno.value) {
    selectedActividadNombre.value = alumno.value.actividadNombre && alumno.value.actividadNombre !== 'Sin asignación' ? alumno.value.actividadNombre : '';
    selectedCoachId.value = alumno.value.idCoach || '';
    showAssignModal.value = true;
  }
};

const closeAssignModal = () => {
  showAssignModal.value = false;
  selectedActividadNombre.value = '';
  selectedCoachId.value = '';
};

const submitAssignCoach = async () => {
  if (!alumno.value || !selectedCoachId.value) return;
  isSubmittingAssign.value = true;
  try {
    const payload = {
      dni: alumno.value.dni,
      nombreCompleto: alumno.value.nombreCompleto || alumno.value.nombre,
      telefono: alumno.value.telefono,
      email: alumno.value.email,
      fechaAlta: alumno.value.fechaAlta,
      estado: alumno.value.estado,
      idPlan: alumno.value.idPlan,
      idCoach: Number(selectedCoachId.value),
      observacion: alumno.value.observaciones
    };
    await socioService.update(alumno.value.id, payload);
    closeAssignModal();
    fetchDetalle();
  } catch (err) {
    console.error('Error al asignar coach', err);
  } finally {
    isSubmittingAssign.value = false;
  }
};

const getAvatarUrl = (ruta) => {
  if (!ruta) return null;
  if (ruta.startsWith('http')) return ruta;
  return `http://localhost:5055${ruta}`;
};

const getActivityStyle = (actNombre) => {
  if (!actNombre) return { background: 'rgba(156, 163, 175, 0.12)', color: '#6b7280', border: '1px solid rgba(156, 163, 175, 0.25)' };
  const lower = actNombre.toLowerCase();
  if (lower.includes('musculaci') || lower.includes('fuerza')) {
    return { background: 'rgba(16, 185, 129, 0.12)', color: '#10b981', border: '1px solid rgba(16, 185, 129, 0.25)' };
  }
  if (lower.includes('crossfit') || lower.includes('funcional')) {
    return { background: 'rgba(245, 158, 11, 0.12)', color: '#f59e0b', border: '1px solid rgba(245, 158, 11, 0.25)' };
  }
  if (lower.includes('spin') || lower.includes('ciclismo')) {
    return { background: 'rgba(14, 165, 233, 0.12)', color: '#0ea5e9', border: '1px solid rgba(14, 165, 233, 0.25)' };
  }
  if (lower.includes('yoga') || lower.includes('pilates')) {
    return { background: 'rgba(139, 92, 246, 0.12)', color: '#8b5cf6', border: '1px solid rgba(139, 92, 246, 0.25)' };
  }
  return { background: 'rgba(99, 102, 241, 0.12)', color: '#6366f1', border: '1px solid rgba(99, 102, 241, 0.25)' };
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
  loadCoaches();
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
            <span :style="getPlanBadgeStyle(alumno.planNombre)">🏷️ {{ alumno.planNombre || 'No disponible todavía' }}</span>
          </div>
          <div class="info-item">
            <span class="label">Precio Mensual:</span>
            <span class="value">${{ alumno.planPrecio ? alumno.planPrecio.toLocaleString() : '0' }}</span>
          </div>
          <div class="info-item">
            <span class="label">Actividad / Disciplina:</span>
            <span class="badge-actividad" :style="getActivityStyle(alumno.actividadNombre)">
              {{ alumno.actividadNombre || 'Sin asignación' }}
            </span>
          </div>
          <div class="info-item">
            <span class="label">Coach Asignado:</span>
            <div class="coach-val-wrapper">
              <span :style="getCoachBadgeStyle(alumno.coachNombre)">🧢 {{ alumno.coachNombre || 'Sin asignación' }}</span>
              <button
                v-if="isAdmin"
                @click="openAssignModal"
                class="btn-quick-assign-sm"
              >
                ✏️ Cambiar / Asignar Coach & Actividad
              </button>
            </div>
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
      <div class="detail-card margin-top-24">
        <h3>Observaciones</h3>
        <p class="observaciones-text">{{ alumno.observaciones || 'No disponible todavía' }}</p>
      </div>

      <!-- Módulo Fotos de Progreso & Evolución Corporal -->
      <FotosProgresoModule :id-socio="alumno.id" :can-edit="false" />
    </div>

    <!-- Modal de Asignación Rápida -->
    <div v-if="showAssignModal" class="modal-backdrop">
      <div class="modal-panel">
        <div class="modal-header">
          <h2>Asignar Entrenador y Actividad</h2>
          <button @click="closeAssignModal" class="btn-close-modal" title="Cerrar">✕</button>
        </div>

        <form @submit.prevent="submitAssignCoach" class="modal-form" v-if="alumno">
          <div class="form-group">
            <label>Alumno</label>
            <input type="text" :value="`${alumno.nombreCompleto || alumno.nombre} (DNI: ${alumno.dni})`" disabled class="input-disabled" />
          </div>

          <div class="form-group">
            <label for="detailQuickActividad">Actividad / Disciplina</label>
            <select id="detailQuickActividad" v-model="selectedActividadNombre" @change="onActividadChange">
              <option value="">-- Todas las actividades --</option>
              <option v-for="act in actividades" :key="act.id" :value="act.nombre">
                {{ act.nombre }}
              </option>
            </select>
          </div>

          <div class="form-group">
            <label for="detailQuickCoach">Entrenador Asignado *</label>
            <select id="detailQuickCoach" v-model="selectedCoachId" @change="onCoachChange" required>
              <option value="" disabled>-- Seleccione un entrenador --</option>
              <option
                v-for="c in filteredCoaches"
                :key="c.id"
                :value="c.id"
                :disabled="c.cupoCompleto && Number(selectedCoachId) !== c.id"
              >
                {{ c.nombre }} ({{ c.actividadNombre || 'Sin actividad' }}) - {{ c.alumnosActuales }}/20 {{ c.cupoCompleto ? 'COMPLETO' : 'disponibles' }}
              </option>
            </select>
          </div>

          <div class="form-buttons">
            <button type="submit" class="btn btn-primary" :disabled="!selectedCoachId || isSubmittingAssign">
              {{ isSubmittingAssign ? 'Guardando...' : 'Confirmar Asignación' }}
            </button>
            <button type="button" @click="closeAssignModal" class="btn btn-secondary">
              Cancelar
            </button>
          </div>
        </form>
      </div>
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

.badge-actividad {
  padding: 4px 10px;
  border-radius: 12px;
  font-size: 12px;
  font-weight: 600;
  display: inline-block;
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

.coach-val-wrapper {
  display: flex;
  align-items: center;
  gap: 8px;
}

.btn-quick-assign-sm {
  background: linear-gradient(135deg, rgba(16, 185, 129, 0.15) 0%, rgba(59, 130, 246, 0.15) 100%);
  color: #10b981;
  border: 1px dashed rgba(16, 185, 129, 0.5);
  padding: 3px 8px;
  border-radius: 6px;
  font-size: 11px;
  font-weight: 700;
  cursor: pointer;
  transition: all 0.2s ease;
}

.btn-quick-assign-sm:hover {
  background: #10b981;
  color: #ffffff;
  border-style: solid;
}

.modal-quick-panel {
  max-width: 440px !important;
  text-align: left;
}

.quick-info-text {
  font-size: 14px;
  color: var(--text-h);
  line-height: 1.5;
}

.quick-select {
  width: 100%;
  padding: 10px 12px;
  border-radius: 8px;
  border: 1px solid var(--border);
  background-color: var(--code-bg);
  color: var(--text-h);
  font-size: 14px;
  outline: none;
}

.quick-actions-row {
  display: flex;
  justify-content: flex-end;
  gap: 12px;
  margin-top: 24px;
}
</style>

<script setup>
import { ref, onMounted, watch, computed } from 'vue';
import { useRouter } from 'vue-router';
import api from '../services/api';
import { getCoachBadgeStyle, getPlanBadgeStyle } from '../utils/badgeStyles';

import authService from '../services/authService';
import socioService from '../services/socioService';

const router = useRouter();
const alumnos = ref([]);
const searchQuery = ref('');
const isLoading = ref(false);
const errorMessage = ref('');
const activeTab = ref('nuevos');

const isAdmin = computed(() => authService.hasRole('Administrador'));

const coaches = ref([]);
const actividades = ref([]);
const showQuickModal = ref(false);
const quickSocio = ref(null);
const quickSelectedActividad = ref('');
const quickSelectedCoachId = ref('');
const isSubmittingQuick = ref(false);

const loadCoachesAndActividades = async () => {
  try {
    const [cRes, aRes] = await Promise.all([
      api.get('/socio/coaches'),
      api.get('/actividades')
    ]);
    coaches.value = cRes.data || [];
    actividades.value = aRes.data || [];
  } catch (err) {
    console.error('Error cargando coaches y actividades:', err);
  }
};

const filteredQuickCoaches = computed(() => {
  if (!quickSelectedActividad.value) return coaches.value;
  return coaches.value.filter(c => {
    if (!c.actividadNombre) return true;
    return c.actividadNombre.toLowerCase().trim() === quickSelectedActividad.value.toLowerCase().trim();
  });
});

const onActividadChange = () => {
  if (quickSelectedCoachId.value) {
    const currentCoach = coaches.value.find(c => c.id === Number(quickSelectedCoachId.value));
    if (currentCoach && currentCoach.actividadNombre && quickSelectedActividad.value) {
      if (currentCoach.actividadNombre.toLowerCase().trim() !== quickSelectedActividad.value.toLowerCase().trim()) {
        quickSelectedCoachId.value = '';
      }
    }
  }
};

const onCoachChange = () => {
  if (quickSelectedCoachId.value) {
    const currentCoach = coaches.value.find(c => c.id === Number(quickSelectedCoachId.value));
    if (currentCoach && currentCoach.actividadNombre) {
      quickSelectedActividad.value = currentCoach.actividadNombre;
    }
  }
};

const openQuickModal = (alumno) => {
  quickSocio.value = alumno;
  quickSelectedActividad.value = alumno.actividadNombre && alumno.actividadNombre !== 'Sin asignación' ? alumno.actividadNombre : '';
  quickSelectedCoachId.value = alumno.idCoach || '';
  showQuickModal.value = true;
};

const closeQuickModal = () => {
  showQuickModal.value = false;
  quickSocio.value = null;
  quickSelectedActividad.value = '';
  quickSelectedCoachId.value = '';
};

const submitQuickAssign = async () => {
  if (!quickSocio.value || !quickSelectedCoachId.value) return;
  isSubmittingQuick.value = true;
  try {
    const fullSocio = await socioService.getById(quickSocio.value.id);
    const payload = {
      dni: fullSocio.dni,
      nombreCompleto: fullSocio.nombreCompleto,
      telefono: fullSocio.telefono,
      email: fullSocio.email,
      fechaAlta: fullSocio.fechaAlta,
      estado: fullSocio.estado,
      idPlan: fullSocio.idPlan,
      idCoach: Number(quickSelectedCoachId.value),
      observacion: fullSocio.observacion
    };
    await socioService.update(quickSocio.value.id, payload);
    closeQuickModal();
    await fetchAlumnos();
  } catch (err) {
    console.error('Error al asignar coach y actividad:', err);
  } finally {
    isSubmittingQuick.value = false;
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

const fetchAlumnos = async () => {
  isLoading.value = true;
  errorMessage.value = '';
  try {
    const params = {};
    if (searchQuery.value.trim()) {
      params.search = searchQuery.value.trim();
    }
    const response = await api.get('/coach/alumnos', { params });
    alumnos.value = response.data;
  } catch (err) {
    if (err.response && err.response.data && err.response.data.errors) {
      errorMessage.value = err.response.data.errors.join(' ');
    } else {
      errorMessage.value = 'Ocurrió un error al cargar la lista de alumnos.';
    }
  } finally {
    isLoading.value = false;
  }
};

let searchTimeout = null;
watch(searchQuery, () => {
  clearTimeout(searchTimeout);
  searchTimeout = setTimeout(() => {
    fetchAlumnos();
  }, 350);
});

const goToDetalle = (id) => {
  router.push(`/coach/alumnos/${id}`);
};

const isNuevoSocio = (alumno) => {
  if (!alumno || !alumno.fechaAlta) return false;
  if (alumno.idCoach || (alumno.coachNombre && alumno.coachNombre.trim() !== '' && alumno.coachNombre !== 'Sin asignación')) {
    return false;
  }
  const fechaAlta = new Date(alumno.fechaAlta);
  const now = new Date();
  const diffTime = Math.abs(now - fechaAlta);
  const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));
  return diffDays <= 7;
};

const nuevosAlumnos = computed(() => {
  return alumnos.value.filter(a => isNuevoSocio(a));
});

const activosAlumnos = computed(() => {
  return alumnos.value.filter(a => a.estado === 'Activo' && !isNuevoSocio(a));
});

const inactivosAlumnos = computed(() => {
  return alumnos.value.filter(a => a.estado === 'Inactivo');
});

const displayedAlumnos = computed(() => {
  if (activeTab.value === 'nuevos') return nuevosAlumnos.value;
  if (activeTab.value === 'inactivos') return inactivosAlumnos.value;
  return activosAlumnos.value;
});

onMounted(() => {
  fetchAlumnos();
  if (isAdmin.value) {
    loadCoachesAndActividades();
  }
});
</script>

<template>
  <div class="coach-alumnos-page">
    <header class="page-header">
      <div>
        <h1>Mis Alumnos</h1>
        <p class="subtitle">Gestión y seguimiento individualizado de alumnos asignados</p>
      </div>

      <div class="search-box">
        <span class="search-icon">🔍</span>
        <input
          type="text"
          v-model="searchQuery"
          placeholder="Buscar alumno por nombre o DNI..."
          class="search-input"
        />
      </div>
    </header>

    <!-- Tabs Navigation -->
    <div class="tabs-container">
      <button
        class="tab-btn"
        :class="{ active: activeTab === 'nuevos' }"
        @click="activeTab = 'nuevos'"
      >
        <span>✨ Nuevos Alumnos</span>
        <span class="tab-badge badge-nuevo">{{ nuevosAlumnos.length }}</span>
      </button>

      <button
        class="tab-btn"
        :class="{ active: activeTab === 'activos' }"
        @click="activeTab = 'activos'"
      >
        <span>✅ Alumnos Activos</span>
        <span class="tab-badge badge-activo">{{ activosAlumnos.length }}</span>
      </button>

      <button
        class="tab-btn"
        :class="{ active: activeTab === 'inactivos' }"
        @click="activeTab = 'inactivos'"
      >
        <span>🔴 Inactivos</span>
        <span class="tab-badge badge-inactivo">{{ inactivosAlumnos.length }}</span>
      </button>
    </div>

    <div v-if="errorMessage" class="alert alert-error">
      {{ errorMessage }}
    </div>

    <div v-if="isLoading" class="loading-state">
      <div class="spinner"></div>
      <p>Cargando lista de alumnos...</p>
    </div>

    <div v-else-if="displayedAlumnos.length === 0" class="empty-state">
      <span class="empty-icon">👥</span>
      <h3>No hay alumnos en esta sección</h3>
      <p v-if="searchQuery">No hay resultados que coincidan con "{{ searchQuery }}".</p>
      <p v-else-if="activeTab === 'nuevos'">Actualmente no hay alumnos nuevos pendientes de asignación.</p>
      <p v-else-if="activeTab === 'activos'">Actualmente no hay alumnos activos asignados.</p>
      <p v-else>No hay alumnos inactivos.</p>
    </div>

    <div v-else class="alumnos-grid">
      <div v-for="alumno in displayedAlumnos" :key="alumno.id" class="alumno-card">
        <div class="card-top">
          <div class="avatar-wrapper">
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

          <div class="card-identity">
            <div class="name-badge-header">
              <h3 class="alumno-name">{{ alumno.nombreCompleto || alumno.nombre }}</h3>
              <span v-if="isNuevoSocio(alumno)" class="tag-nuevo-alumno">
                ✨ Nuevo alumno
              </span>
            </div>
            <span v-if="alumno.username" class="alumno-username">@{{ alumno.username }}</span>
            <span :style="getPlanBadgeStyle(alumno.planNombre)">🏷️ {{ alumno.planNombre }}</span>
          </div>
        </div>

        <div class="card-body">
          <div class="info-row">
            <span class="info-label">DNI:</span>
            <span class="info-val font-bold">{{ alumno.dni || 'Sin DNI' }}</span>
          </div>
          <div class="info-row">
            <span class="info-label">Estado de Deuda:</span>
            <span
              class="badge-deuda"
              :class="alumno.deudaEstado === 'Al día' ? 'deuda-ok' : 'deuda-pending'"
            >
              {{ alumno.deudaEstado }}
            </span>
          </div>

          <div class="info-row">
            <span class="info-label">Actividad:</span>
            <span class="badge-actividad" :style="getActivityStyle(alumno.actividadNombre)">{{ alumno.actividadNombre || 'Musculación' }}</span>
          </div>

          <div class="info-row">
            <span class="info-label">Coach Asignado:</span>
            <div class="coach-val-wrapper">
              <span :style="getCoachBadgeStyle(alumno.coachNombre)">🧢 {{ alumno.coachNombre || 'Sin asignación' }}</span>
              <button
                v-if="isAdmin"
                @click.stop="openQuickModal(alumno)"
                class="btn-quick-assign-sm"
              >
                ✏️ Asignar / Cambiar
              </button>
            </div>
          </div>

          <div class="info-row">
            <span class="info-label">Evoluciones cargadas:</span>
            <span class="info-val font-bold text-accent">{{ alumno.cantidadEvoluciones || 0 }}</span>
          </div>

          <div class="info-row">
            <span class="info-label">Progreso:</span>
            <span class="info-val subtle">{{ alumno.progreso }}</span>
          </div>

          <div class="info-row">
            <span class="info-label">Última Sesión:</span>
            <span class="info-val subtle">{{ alumno.ultimaSesion }}</span>
          </div>
        </div>

        <div class="card-footer">
          <button @click="goToDetalle(alumno.id)" class="btn-detail">
            Ver detalle →
          </button>
        </div>
      </div>
    </div>

    <!-- Modal de Asignación Rápida de Coach y Actividad (Admin) -->
    <div v-if="showQuickModal" class="modal-backdrop">
      <div class="modal-panel">
        <div class="modal-header">
          <h2>Asignar Entrenador y Actividad</h2>
          <button @click="closeQuickModal" class="btn-close-modal" title="Cerrar">✕</button>
        </div>

        <form @submit.prevent="submitQuickAssign" class="modal-form" v-if="quickSocio">
          <div class="form-group">
            <label>Alumno</label>
            <input type="text" :value="`${quickSocio.nombreCompleto || quickSocio.nombre} (DNI: ${quickSocio.dni})`" disabled class="input-disabled" />
          </div>

          <div class="form-group">
            <label for="quickActividadSelect">Actividad / Disciplina</label>
            <select id="quickActividadSelect" v-model="quickSelectedActividad" @change="onActividadChange">
              <option value="">-- Todas las actividades --</option>
              <option v-for="act in actividades" :key="act.id" :value="act.nombre">
                {{ act.nombre }}
              </option>
            </select>
          </div>

          <div class="form-group">
            <label for="quickCoachSelect">Entrenador Asignado *</label>
            <select id="quickCoachSelect" v-model="quickSelectedCoachId" @change="onCoachChange" required>
              <option value="" disabled>-- Seleccione un entrenador --</option>
              <option
                v-for="c in filteredQuickCoaches"
                :key="c.id"
                :value="c.id"
                :disabled="c.cupoCompleto && Number(quickSelectedCoachId) !== c.id"
              >
                {{ c.nombre }} ({{ c.actividadNombre || 'Sin actividad' }}) - {{ c.alumnosActuales }}/20 {{ c.cupoCompleto ? 'COMPLETO' : 'disponibles' }}
              </option>
            </select>
          </div>

          <div class="form-buttons">
            <button type="submit" class="btn btn-primary" :disabled="!quickSelectedCoachId || isSubmittingQuick">
              {{ isSubmittingQuick ? 'Guardando...' : 'Confirmar Asignación' }}
            </button>
            <button type="button" @click="closeQuickModal" class="btn btn-secondary">
              Cancelar
            </button>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>

<style scoped>
.coach-alumnos-page {
  width: 100%;
  max-width: 1400px;
  margin: 0 auto;
  padding: 28px 24px;
  text-align: left;
  box-sizing: border-box;
}

.page-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 24px;
  border-bottom: 1px solid var(--border);
  padding-bottom: 16px;
  gap: 20px;
  flex-wrap: wrap;
}

.page-header h1 {
  margin: 0;
  font-size: 20px;
  font-weight: 600;
  letter-spacing: -0.3px;
  color: var(--text-h);
}

.subtitle {
  color: var(--text);
  font-size: 13px;
  margin-top: 4px;
  margin-bottom: 0;
  opacity: 0.75;
}

.search-box {
  position: relative;
  width: 100%;
  max-width: 320px;
}

.search-icon {
  position: absolute;
  left: 12px;
  top: 50%;
  transform: translateY(-50%);
  font-size: 15px;
  opacity: 0.6;
}

.search-input {
  width: 100%;
  padding: 10px 12px 10px 36px;
  font-size: 14px;
  border: 1px solid var(--border);
  border-radius: 8px;
  background-color: var(--bg);
  color: inherit;
  outline: none;
  box-sizing: border-box;
}

.search-input:focus {
  border-color: var(--accent);
}

.loading-state, .empty-state {
  background-color: var(--bg);
  border: 1px solid var(--border);
  border-radius: 12px;
  padding: 48px 24px;
  text-align: center;
  box-shadow: var(--shadow);
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

.empty-icon {
  font-size: 40px;
  display: block;
  margin-bottom: 12px;
}

.empty-state h3 {
  margin: 0 0 6px 0;
  color: var(--text-h);
  font-size: 18px;
}

.empty-state p {
  margin: 0;
  color: var(--text);
  opacity: 0.8;
  font-size: 14px;
}

.alumnos-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(320px, 1fr));
  gap: 22px;
}

.alumno-card {
  background-color: var(--bg);
  border: 1px solid var(--border);
  border-radius: 12px;
  padding: 22px;
  box-shadow: var(--shadow);
  display: flex;
  flex-direction: column;
  justify-content: space-between;
  transition: transform 0.2s ease, border-color 0.2s ease;
}

.alumno-card:hover {
  transform: translateY(-2px);
  border-color: var(--accent);
}

.card-top {
  display: flex;
  align-items: center;
  gap: 14px;
  margin-bottom: 16px;
  border-bottom: 1px solid var(--border);
  padding-bottom: 14px;
}

.avatar-wrapper {
  width: 54px;
  height: 54px;
  border-radius: 50%;
  overflow: hidden;
  border: 2px solid var(--accent);
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
  font-size: 22px;
  font-weight: 700;
  display: flex;
  align-items: center;
  justify-content: center;
}

.card-identity {
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.alumno-name {
  margin: 0;
  font-size: 16px;
  font-weight: 700;
  color: var(--text-h);
}

.alumno-username {
  font-size: 12px;
  color: var(--text);
  opacity: 0.7;
}

.badge-plan {
  display: inline-block;
  margin-top: 4px;
  font-size: 11px;
  font-weight: 600;
  color: var(--accent);
  background-color: var(--neon-bg);
  border: 1px solid var(--neon-border);
  padding: 2px 8px;
  border-radius: 10px;
  width: fit-content;
}

.badge-actividad {
  display: inline-block;
  font-size: 11px;
  font-weight: 600;
  color: #6366f1;
  background-color: rgba(99, 102, 241, 0.12);
  border: 1px solid rgba(99, 102, 241, 0.3);
  padding: 2px 8px;
  border-radius: 10px;
}

.card-body {
  display: flex;
  flex-direction: column;
  gap: 10px;
  margin-bottom: 18px;
}

.info-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  font-size: 13px;
}

.info-label {
  color: var(--text);
  opacity: 0.8;
}

.info-val {
  color: var(--text-h);
  font-weight: 500;
}

.info-val.subtle {
  font-style: italic;
  opacity: 0.7;
}

.badge-deuda {
  padding: 3px 8px;
  border-radius: 10px;
  font-size: 11px;
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

.card-footer {
  border-top: 1px solid var(--border);
  padding-top: 14px;
}

.btn-detail {
  width: 100%;
  padding: 10px;
  font-size: 14px;
  font-weight: 600;
  background-color: var(--accent);
  color: #fff;
  border: none;
  border-radius: 8px;
  cursor: pointer;
  transition: opacity 0.2s ease;
}

.btn-detail:hover {
  opacity: 0.9;
}

.alert-error {
  background-color: rgba(239, 68, 68, 0.12);
  border: 1px solid rgba(239, 68, 68, 0.3);
  color: #ef4444;
  padding: 12px 16px;
  border-radius: 8px;
  margin-bottom: 20px;
}

.name-badge-header {
  display: flex;
  align-items: center;
  gap: 8px;
  flex-wrap: wrap;
}

.tag-nuevo-alumno {
  display: inline-flex;
  align-items: center;
  gap: 4px;
  background: linear-gradient(135deg, #ef4444 0%, #f59e0b 100%);
  color: #ffffff;
  font-size: 10px;
  font-weight: 800;
  padding: 2px 8px;
  border-radius: 12px;
  text-transform: uppercase;
  letter-spacing: 0.5px;
  box-shadow: 0 2px 8px rgba(239, 68, 68, 0.4);
  animation: pulseNuevoTag 1.8s infinite alternate;
  white-space: nowrap;
}

@keyframes pulseNuevoTag {
  0% {
    transform: scale(1);
    box-shadow: 0 2px 8px rgba(239, 68, 68, 0.4);
  }
  100% {
    transform: scale(1.04);
    box-shadow: 0 4px 14px rgba(245, 158, 11, 0.6);
  }
}

.tabs-container {
  display: flex;
  gap: 12px;
  margin-bottom: 24px;
  border-bottom: 1px solid var(--border);
  padding-bottom: 14px;
  flex-wrap: wrap;
}

.tab-btn {
  background: var(--code-bg);
  color: var(--text);
  border: 1px solid var(--border);
  padding: 10px 18px;
  border-radius: 20px;
  font-size: 14px;
  font-weight: 600;
  cursor: pointer;
  display: inline-flex;
  align-items: center;
  gap: 10px;
  transition: all 0.2s ease;
}

.tab-btn:hover {
  border-color: var(--accent);
  color: var(--text-h);
}

.tab-btn.active {
  background: var(--accent-bg);
  color: var(--accent);
  border-color: var(--accent);
  box-shadow: 0 2px 10px rgba(212, 175, 55, 0.15);
}

.tab-badge {
  padding: 3px 9px;
  border-radius: 12px;
  font-size: 11px;
  font-weight: 800;
}

.badge-nuevo {
  background: rgba(16, 185, 129, 0.2);
  color: #10b981;
  border: 1px solid rgba(16, 185, 129, 0.4);
}

.badge-activo {
  background: rgba(59, 130, 246, 0.2);
  color: #3b82f6;
  border: 1px solid rgba(59, 130, 246, 0.4);
}

.badge-inactivo {
  background: rgba(239, 68, 68, 0.2);
  color: #ef4444;
  border: 1px solid rgba(239, 68, 68, 0.4);
}
</style>

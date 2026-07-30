<script setup>
import { ref, onMounted, watch } from 'vue';
import { useRouter } from 'vue-router';
import api from '../services/api';

const router = useRouter();
const alumnos = ref([]);
const searchQuery = ref('');
const isLoading = ref(false);
const errorMessage = ref('');

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

onMounted(() => {
  fetchAlumnos();
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

    <div v-if="errorMessage" class="alert alert-error">
      {{ errorMessage }}
    </div>

    <div v-if="isLoading" class="loading-state">
      <div class="spinner"></div>
      <p>Cargando lista de alumnos...</p>
    </div>

    <div v-else-if="alumnos.length === 0" class="empty-state">
      <span class="empty-icon">👥</span>
      <h3>No se encontraron alumnos</h3>
      <p v-if="searchQuery">No hay resultados que coincidan con "{{ searchQuery }}".</p>
      <p v-else>Actualmente no hay alumnos asignados para mostrar.</p>
    </div>

    <div v-else class="alumnos-grid">
      <div v-for="alumno in alumnos" :key="alumno.id" class="alumno-card">
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
            <h3 class="alumno-name">{{ alumno.nombreCompleto || alumno.nombre }}</h3>
            <span v-if="alumno.username" class="alumno-username">@{{ alumno.username }}</span>
            <span class="badge-plan">{{ alumno.planNombre }}</span>
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
            <span class="info-val font-bold">{{ alumno.coachNombre || 'Sin asignación' }}</span>
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
  color: #991b1b;
  padding: 12px 16px;
  border-radius: 8px;
  font-size: 14px;
  margin-bottom: 20px;
}
</style>

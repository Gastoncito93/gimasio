<script setup>
import { ref, onMounted, computed } from 'vue';
import authService from '../services/authService';
import api from '../services/api';

const isAdmin = computed(() => authService.hasRole('Administrador'));

const actividades = ref([]);
const searchQuery = ref('');
const statusFilter = ref('');
const isLoading = ref(false);
const errors = ref([]);

// Modal State
const showModal = ref(false);
const isEditing = ref(false);
const formId = ref(null);
const formNombre = ref('');
const formDescripcion = ref('');
const formEstado = ref('Activo');

const cleanCoachName = (name) => {
  if (!name) return '';
  return name.replace(/^Coach\s+/i, '');
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

const fetchActividades = async () => {
  isLoading.value = true;
  errors.value = [];
  try {
    const params = {};
    if (searchQuery.value.trim()) params.search = searchQuery.value.trim();
    if (statusFilter.value) params.estado = statusFilter.value;

    const res = await api.get('/actividad', { params });
    actividades.value = res.data;
  } catch (err) {
    if (err.response && err.response.data && err.response.data.errors) {
      errors.value = err.response.data.errors;
    } else {
      errors.value = ['Error al cargar las actividades.'];
    }
  } finally {
    isLoading.value = false;
  }
};

const openCreateModal = () => {
  resetForm();
  showModal.value = true;
};

const editActividad = (act) => {
  errors.value = [];
  isEditing.value = true;
  formId.value = act.id;
  formNombre.value = act.nombre;
  formDescripcion.value = act.descripcion || '';
  formEstado.value = act.estado;
  showModal.value = true;
};

const onSubmit = async () => {
  errors.value = [];

  if (!formNombre.value.trim()) {
    errors.value.push('El nombre de la actividad es obligatorio.');
    return;
  }

  const payload = {
    nombre: formNombre.value.trim(),
    descripcion: formDescripcion.value.trim() || null,
    estado: formEstado.value
  };

  try {
    if (isEditing.value) {
      await api.put(`/actividad/${formId.value}`, payload);
    } else {
      await api.post('/actividad', payload);
    }
    closeModal();
    fetchActividades();
  } catch (err) {
    if (err.response && err.response.data && err.response.data.errors) {
      errors.value = err.response.data.errors;
    } else {
      errors.value = ['Ocurrió un error al guardar la actividad.'];
    }
  }
};

const deleteActividad = async (act) => {
  if (!confirm(`¿Está seguro de eliminar o inactivar la actividad "${act.nombre}"?`)) return;
  try {
    await api.delete(`/actividad/${act.id}`);
    fetchActividades();
  } catch (err) {
    if (err.response && err.response.data && err.response.data.errors) {
      errors.value = err.response.data.errors;
    }
  }
};

const closeModal = () => {
  showModal.value = false;
  resetForm();
};

const resetForm = () => {
  isEditing.value = false;
  formId.value = null;
  formNombre.value = '';
  formDescripcion.value = '';
  formEstado.value = 'Activo';
  errors.value = [];
};

let searchTimeout = null;
const onSearchInput = () => {
  clearTimeout(searchTimeout);
  searchTimeout = setTimeout(fetchActividades, 350);
};

onMounted(() => {
  fetchActividades();
});
</script>

<template>
  <div class="page-container">
    <header class="page-header">
      <div>
        <h1 class="page-title">Gestión de Actividades</h1>
        <p class="page-subtitle">Disciplinas del gimnasio y entrenadores a cargo</p>
      </div>

      <button v-if="isAdmin" @click="openCreateModal" class="btn-primary">
        + Nueva Actividad
      </button>
    </header>

    <div v-if="errors.length > 0" class="error-alert">
      <ul>
        <li v-for="(err, idx) in errors" :key="idx">{{ err }}</li>
      </ul>
    </div>

    <!-- Filtros -->
    <div class="filter-bar">
      <div class="search-box">
        <input
          type="text"
          v-model="searchQuery"
          placeholder="Buscar por nombre o descripción..."
          @input="onSearchInput"
        />
      </div>

      <div class="filter-select">
        <select v-model="statusFilter" @change="fetchActividades">
          <option value="">Todos los estados</option>
          <option value="Activo">Activo</option>
          <option value="Inactivo">Inactivo</option>
        </select>
      </div>
    </div>

    <!-- Lista / Tabla de Actividades -->
    <div v-if="isLoading" class="state-msg">Cargando actividades...</div>

    <div v-else-if="actividades.length === 0" class="empty-card">
      <p>No se encontraron disciplinas registradas.</p>
    </div>

    <div v-else class="table-card">
      <table class="data-table">
        <thead>
          <tr>
            <th>Disciplina</th>
            <th>Descripción</th>
            <th>Entrenadores a cargo</th>
            <th>Estado</th>
            <th v-if="isAdmin" class="text-right">Acciones</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="act in actividades" :key="act.id" :class="{ 'inactive-row': act.estado === 'Inactivo' }">
            <td class="font-bold">
              <span class="badge-actividad" :style="getActivityStyle(act.nombre)">
                {{ act.nombre }}
              </span>
            </td>
            <td class="text-muted">{{ act.descripcion || 'Sin descripción' }}</td>
            <td>
              <div v-if="act.nombresCoaches.length > 0" class="coaches-list">
                <span v-for="(coach, i) in act.nombresCoaches" :key="i" class="coach-tag">
                  {{ cleanCoachName(coach) }}
                </span>
              </div>
              <span v-else class="subtle-text">Sin asignar</span>
            </td>
            <td>
              <span :class="['badge-status', act.estado === 'Activo' ? 'status-active' : 'status-inactive']">
                {{ act.estado }}
              </span>
            </td>
            <td v-if="isAdmin" class="actions-cell text-right">
              <button @click="editActividad(act)" class="btn-action">Editar</button>
              <button @click="deleteActividad(act)" class="btn-action btn-danger">Eliminar</button>
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- Modal Dialog para Nueva / Editar Actividad -->
    <div v-if="showModal" class="modal-backdrop" @click.self="closeModal">
      <div class="modal-card">
        <h3>{{ isEditing ? `Editar Actividad #${formId}` : 'Nueva Actividad' }}</h3>

        <form @submit.prevent="onSubmit" class="modal-form">
          <div class="form-group">
            <label for="nombre">Nombre de la Actividad *</label>
            <input type="text" id="nombre" v-model="formNombre" required placeholder="Ej. Musculación, Crossfit, Yoga..." />
          </div>

          <div class="form-group">
            <label for="descripcion">Descripción</label>
            <textarea id="descripcion" v-model="formDescripcion" placeholder="Breve descripción del entrenamiento..." rows="3"></textarea>
          </div>

          <div class="form-group">
            <label for="estado">Estado *</label>
            <select id="estado" v-model="formEstado">
              <option value="Activo">Activo</option>
              <option value="Inactivo">Inactivo</option>
            </select>
          </div>

          <div class="modal-actions">
            <button type="button" @click="closeModal" class="btn-secondary">Cancelar</button>
            <button type="submit" class="btn-primary">
              {{ isEditing ? 'Guardar Cambios' : 'Crear Actividad' }}
            </button>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>

<style scoped>
.page-container {
  max-width: 1120px;
  margin: 0 auto;
  padding: 28px 24px;
  box-sizing: border-box;
  text-align: left;
}

.page-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
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

.filter-bar {
  display: flex;
  gap: 12px;
  margin-bottom: 18px;
}

.search-box {
  width: 280px;
}

.search-box input, .filter-select select {
  width: 100%;
  padding: 9px 12px;
  font-size: 13px;
  border: 1px solid var(--border);
  border-radius: 8px;
  background-color: var(--code-bg);
  color: inherit;
  outline: none;
  box-sizing: border-box;
}

.search-box input:focus, .filter-select select:focus {
  border-color: var(--accent);
}

.table-card {
  background-color: var(--bg);
  border: 1px solid var(--border);
  border-radius: 10px;
  overflow: hidden;
  box-shadow: var(--shadow);
}

.data-table {
  width: 100%;
  border-collapse: collapse;
  border-spacing: 0;
  font-size: 13px;
  margin: 0;
}

.data-table tr {
  border-bottom: 1px solid var(--border);
}

.data-table tr:last-child {
  border-bottom: none;
}

.data-table th {
  background-color: var(--code-bg);
  color: var(--text-h);
  font-weight: 600;
  padding: 11px 16px;
  border: none;
  font-size: 12px;
  text-transform: uppercase;
  letter-spacing: 0.4px;
}

.data-table td {
  padding: 12px 16px;
  border-bottom: 1px solid var(--border);
  vertical-align: middle;
}

.data-table tr:last-child td {
  border-bottom: none;
}

.font-bold { font-weight: 600; }
.text-muted { color: var(--text); opacity: 0.8; }
.subtle-text { color: var(--text); opacity: 0.55; font-style: italic; font-size: 12px; }

.badge-actividad {
  padding: 4px 10px;
  border-radius: 12px;
  font-size: 12px;
  font-weight: 600;
  display: inline-block;
}

.coaches-list {
  display: flex;
  gap: 6px;
  flex-wrap: wrap;
}

.coach-tag {
  background-color: var(--code-bg);
  border: 1px solid var(--border);
  color: var(--text-h);
  padding: 2px 8px;
  border-radius: 6px;
  font-size: 12px;
  font-weight: 500;
}

.badge-status {
  padding: 3px 8px;
  border-radius: 10px;
  font-size: 11px;
  font-weight: 600;
}

.status-active {
  background-color: rgba(16, 185, 129, 0.12);
  color: #10b981;
  border: 1px solid rgba(16, 185, 129, 0.25);
}

.status-inactive {
  background-color: rgba(239, 68, 68, 0.12);
  color: #ef4444;
  border: 1px solid rgba(239, 68, 68, 0.25);
}

.inactive-row { opacity: 0.6; }

.actions-cell {
  display: flex;
  gap: 6px;
  justify-content: flex-end;
}

.text-right { text-align: right; }

.btn-action {
  background-color: var(--code-bg);
  border: 1px solid var(--border);
  border-radius: 6px;
  padding: 5px 10px;
  font-size: 12px;
  font-weight: 600;
  color: var(--text-h);
  cursor: pointer;
  transition: all 0.15s ease;
}

.btn-action:hover {
  border-color: var(--accent);
  color: var(--accent);
}

.btn-danger { color: #ef4444; }
.btn-danger:hover {
  background-color: rgba(239, 68, 68, 0.1);
  border-color: rgba(239, 68, 68, 0.3);
}

.btn-primary {
  background-color: var(--accent);
  color: #fff;
  border: none;
  padding: 9px 16px;
  border-radius: 8px;
  font-size: 13px;
  font-weight: 600;
  cursor: pointer;
}

.btn-secondary {
  background-color: transparent;
  border: 1px solid var(--border);
  color: var(--text);
  padding: 9px 16px;
  border-radius: 8px;
  font-size: 13px;
  font-weight: 600;
  cursor: pointer;
}

.modal-backdrop {
  position: fixed;
  top: 0;
  left: 0;
  width: 100vw;
  height: 100vh;
  background-color: rgba(0, 0, 0, 0.45);
  backdrop-filter: blur(3px);
  display: flex;
  justify-content: center;
  align-items: center;
  z-index: 999;
}

.modal-card {
  background-color: var(--bg);
  border-radius: 12px;
  border: 1px solid var(--border);
  padding: 24px;
  width: 100%;
  max-width: 420px;
  box-shadow: 0 12px 32px rgba(0, 0, 0, 0.15);
}

.modal-card h3 {
  margin-top: 0;
  margin-bottom: 18px;
  font-size: 16px;
  font-weight: 600;
  color: var(--text-h);
}

.form-group { margin-bottom: 14px; }
.form-group label {
  display: block;
  font-size: 12px;
  font-weight: 600;
  margin-bottom: 5px;
}

.form-group input, .form-group textarea, .form-group select {
  width: 100%;
  padding: 9px 11px;
  font-size: 13px;
  border: 1px solid var(--border);
  border-radius: 8px;
  background-color: var(--code-bg);
  color: inherit;
  box-sizing: border-box;
}

.modal-actions {
  display: flex;
  justify-content: flex-end;
  gap: 8px;
  margin-top: 20px;
}

.state-msg, .empty-card {
  background-color: var(--bg);
  border: 1px solid var(--border);
  border-radius: 10px;
  padding: 32px;
  text-align: center;
  color: var(--text);
  opacity: 0.75;
}

.error-alert {
  background-color: rgba(239, 68, 68, 0.12);
  border: 1px solid rgba(239, 68, 68, 0.3);
  color: #ef4444;
  padding: 10px 14px;
  border-radius: 8px;
  font-size: 13px;
  margin-bottom: 16px;
}

.error-alert ul { margin: 0; padding-left: 18px; }
</style>

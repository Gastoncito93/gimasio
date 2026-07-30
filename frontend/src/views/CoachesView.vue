<script setup>
import { ref, onMounted } from 'vue';
import api from '../services/api';
import { getCoachBadgeStyle } from '../utils/badgeStyles';

const coaches = ref([]);
const actividades = ref([]);
const searchQuery = ref('');
const isLoading = ref(false);
const showModal = ref(false);
const modalErrors = ref([]);
const isEditing = ref(false);
const editingId = ref(null);

const formNombre = ref('');
const formUsername = ref('');
const formPassword = ref('');
const formIdActividad = ref('');

const loadCoaches = async () => {
  isLoading.value = true;
  try {
    const res = await api.get('/usuario/coaches', {
      params: { search: searchQuery.value }
    });
    coaches.value = res.data;
  } catch (err) {
    console.error('Error al cargar coaches:', err);
  } finally {
    isLoading.value = false;
  }
};

const loadActividades = async () => {
  try {
    const res = await api.get('/actividad');
    actividades.value = res.data.filter(a => a.estado === 'Activo');
  } catch (err) {
    console.error('Error al cargar actividades:', err);
  }
};

const cleanName = (name) => {
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

const openCreateModal = () => {
  isEditing.value = false;
  editingId.value = null;
  formNombre.value = '';
  formUsername.value = '';
  formPassword.value = '';
  formIdActividad.value = '';
  modalErrors.value = [];
  showModal.value = true;
};

const openEditModal = (c) => {
  isEditing.value = true;
  editingId.value = c.id;
  formNombre.value = c.nombre;
  formUsername.value = c.username;
  formPassword.value = '';
  formIdActividad.value = c.idActividad || '';
  modalErrors.value = [];
  showModal.value = true;
};

const saveCoach = async () => {
  modalErrors.value = [];
  if (!formNombre.value.trim()) {
    modalErrors.value.push('El nombre completo es obligatorio.');
    return;
  }

  if (!isEditing.value) {
    if (!formUsername.value.trim() || !formPassword.value) {
      modalErrors.value.push('El usuario y la contraseña son obligatorios.');
      return;
    }
  }

  try {
    if (isEditing.value) {
      const payload = {
        nombre: formNombre.value.trim(),
        password: formPassword.value ? formPassword.value : null,
        idActividad: formIdActividad.value ? Number(formIdActividad.value) : null
      };
      await api.put(`/usuario/coach/${editingId.value}`, payload);
    } else {
      const payload = {
        nombre: formNombre.value.trim(),
        username: formUsername.value.trim(),
        password: formPassword.value,
        idActividad: formIdActividad.value ? Number(formIdActividad.value) : null
      };
      await api.post('/usuario/coach', payload);
    }
    showModal.value = false;
    loadCoaches();
  } catch (err) {
    if (err.response && err.response.data && err.response.data.errors) {
      modalErrors.value = err.response.data.errors;
    } else {
      modalErrors.value = ['Error al guardar el entrenador. Intente nuevamente.'];
    }
  }
};

const deleteCoach = async (c) => {
  if (!confirm(`¿Estás seguro de que deseas inhabilitar a ${cleanName(c.nombre)}?`)) return;
  try {
    await api.delete(`/usuario/coach/${c.id}`);
    loadCoaches();
  } catch (err) {
    alert('No se pudo inhabilitar al entrenador.');
  }
};

const defaultAvatar = (nombre) => {
  return `https://ui-avatars.com/api/?name=${encodeURIComponent(nombre)}&background=6366f1&color=fff`;
};

onMounted(() => {
  loadCoaches();
  loadActividades();
});
</script>

<template>
  <div class="page-container">
    <header class="page-header">
      <div>
        <h1 class="page-title">Gestión de Entrenadores</h1>
        <p class="page-subtitle">Administra el equipo y asigna sus disciplinas</p>
      </div>
      <button class="btn-primary" @click="openCreateModal">+ Nuevo Entrenador</button>
    </header>

    <!-- Filtros de búsqueda -->
    <div class="filter-bar">
      <div class="search-box">
        <input
          type="text"
          v-model="searchQuery"
          @input="loadCoaches"
          placeholder="Buscar por nombre o usuario..."
        />
      </div>
    </div>

    <!-- Carga -->
    <div v-if="isLoading" class="loading-state">
      <p>Cargando entrenadores...</p>
    </div>

    <!-- Tabla -->
    <div v-else class="table-card">
      <table class="data-table">
        <thead>
          <tr>
            <th>Nombre</th>
            <th>Usuario</th>
            <th>Disciplina / Actividad</th>
            <th>Alumnos Asignados</th>
            <th class="text-right">Acciones</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="c in coaches" :key="c.id">
            <td>
              <div class="user-cell">
                <img :src="c.rutaAvatar || defaultAvatar(c.nombre)" class="avatar" alt="Avatar" />
                <div class="user-info">
                  <span :style="getCoachBadgeStyle(c.nombre)">🧢 {{ cleanName(c.nombre) }}</span>
                </div>
              </div>
            </td>
            <td><code>@{{ c.username }}</code></td>
            <td>
              <span v-if="c.actividadNombre" class="badge-actividad" :style="getActivityStyle(c.actividadNombre)">
                {{ c.actividadNombre }}
              </span>
              <span v-else class="badge-none">Sin asignar</span>
            </td>
            <td>
              <span v-if="c.cupoCompleto" class="badge-completo">
                {{ c.cantidadAlumnos }}/{{ c.cupoMaximo || 20 }} COMPLETO
              </span>
              <span v-else class="count-badge">
                {{ c.cantidadAlumnos }}/{{ c.cupoMaximo || 20 }} alumnos
              </span>
            </td>
            <td class="text-right">
              <div class="actions-cell text-right">
                <button class="btn-action" @click="openEditModal(c)">Editar</button>
                <button class="btn-action btn-danger" @click="deleteCoach(c)">Inhabilitar</button>
              </div>
            </td>
          </tr>
          <tr v-if="coaches.length === 0">
            <td colspan="5" class="empty-state">No se encontraron entrenadores registrados.</td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- Modal Crear / Editar -->
    <div v-if="showModal" class="modal-backdrop" @click.self="showModal = false">
      <div class="modal-card">
        <h3>{{ isEditing ? 'Editar Entrenador' : 'Crear Nuevo Entrenador' }}</h3>

        <div v-if="modalErrors.length > 0" class="alert alert-error">
          <ul>
            <li v-for="(err, idx) in modalErrors" :key="idx">{{ err }}</li>
          </ul>
        </div>

        <form @submit.prevent="saveCoach">
          <div class="form-group">
            <label>Nombre Completo *</label>
            <input type="text" v-model="formNombre" required placeholder="Ej. Roberto Gómez" />
          </div>

          <div class="form-group" v-if="!isEditing">
            <label>Nombre de Usuario *</label>
            <input type="text" v-model="formUsername" required placeholder="Ej. roberto.gomez" />
          </div>

          <div class="form-group">
            <label>{{ isEditing ? 'Nueva Contraseña (dejar en blanco para mantener)' : 'Contraseña Inicial *' }}</label>
            <input type="password" v-model="formPassword" :required="!isEditing" placeholder="Mínimo 6 caracteres" />
          </div>

          <div class="form-group">
            <label>Disciplina / Actividad Asignada</label>
            <select v-model="formIdActividad">
              <option value="">-- Sin Actividad / Seleccionar después --</option>
              <option v-for="act in actividades" :key="act.id" :value="act.id">
                {{ act.nombre }}
              </option>
            </select>
          </div>

          <div class="modal-actions">
            <button type="button" class="btn-secondary" @click="showModal = false">Cancelar</button>
            <button type="submit" class="btn-primary">
              {{ isEditing ? 'Guardar Cambios' : 'Crear Entrenador' }}
            </button>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>

<style scoped>
.page-container {
  width: 100%;
  max-width: 1400px;
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
  margin-bottom: 18px;
}

.search-box {
  width: 280px;
}

.search-box input {
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

.search-box input:focus {
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
  text-align: left;
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
  padding: 11px 16px;
  font-weight: 600;
  font-size: 12px;
  text-transform: uppercase;
  letter-spacing: 0.4px;
  color: var(--text-h);
  border: none;
}

.data-table td {
  padding: 12px 16px;
  vertical-align: middle;
  border: none;
}

.user-cell {
  display: flex;
  align-items: center;
  gap: 10px;
}

.avatar {
  width: 34px;
  height: 34px;
  border-radius: 50%;
  object-fit: cover;
  border: 1px solid var(--border);
}

.user-info .name {
  font-weight: 600;
  color: var(--text-h);
}

.badge-actividad {
  padding: 4px 10px;
  border-radius: 12px;
  font-size: 12px;
  font-weight: 600;
  display: inline-block;
}

.badge-completo {
  background-color: rgba(239, 68, 68, 0.12);
  color: #ef4444;
  border: 1px solid rgba(239, 68, 68, 0.25);
  padding: 4px 10px;
  border-radius: 12px;
  font-size: 12px;
  font-weight: 600;
  display: inline-block;
}

.badge-none {
  background-color: rgba(156, 163, 175, 0.12);
  color: #6b7280;
  padding: 4px 10px;
  border-radius: 12px;
  font-size: 12px;
}

.count-badge {
  font-size: 13px;
  font-weight: 500;
  color: var(--text);
}

.actions-cell {
  display: inline-flex;
  gap: 6px;
  justify-content: flex-end;
}

.text-right {
  text-align: right;
}

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

.btn-danger {
  color: #ef4444;
}

.btn-danger:hover {
  background-color: rgba(239, 68, 68, 0.1);
  border-color: rgba(239, 68, 68, 0.3);
  color: #ef4444;
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
  transition: opacity 0.15s ease;
}

.btn-primary:hover {
  opacity: 0.9;
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

.empty-state {
  text-align: center;
  padding: 32px;
  color: var(--text);
  opacity: 0.7;
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

.form-group {
  margin-bottom: 14px;
}

.form-group label {
  display: block;
  font-size: 12px;
  font-weight: 600;
  margin-bottom: 5px;
  color: var(--text-h);
}

.form-group input, .form-group select {
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

.alert-error {
  background-color: rgba(239, 68, 68, 0.12);
  border: 1px solid rgba(239, 68, 68, 0.3);
  color: #ef4444;
  padding: 10px 14px;
  border-radius: 8px;
  font-size: 13px;
  margin-bottom: 16px;
}

.alert-error ul {
  margin: 0;
  padding-left: 18px;
}
</style>

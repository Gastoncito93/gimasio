<script setup>
import { ref, onMounted, computed } from 'vue';
import socioService from '../services/socioService';
import planService from '../services/planService';
import authService from '../services/authService';
import api from '../services/api';

const getAvatarUrl = (ruta) => {
  if (!ruta) return null;
  if (ruta.startsWith('http')) return ruta;
  return `http://localhost:5055${ruta}`;
};

// Auth State
const isAdmin = computed(() => authService.hasRole('Administrador'));

// Table / Filter State
const socios = ref([]);
const currentPage = ref(1);
const pageSize = ref(10);
const totalPages = ref(0);
const totalItems = ref(0);
const search = ref('');
const statusFilter = ref('');
const planFilter = ref('');
const errors = ref([]);

// Form / Drawer State
const showDrawer = ref(false);
const isEditing = ref(false);
const formSocioId = ref(null);
const formDni = ref('');
const formNombreCompleto = ref('');
const formTelefono = ref('');
const formEmail = ref('');
const formFechaAlta = ref(new Date().toISOString().substring(0, 10));
const formEstado = ref('Activo');
const formIdCoach = ref('');
const formObservacion = ref('');

// Plan dropdown state
const activePlans = ref([]);
const formIdPlan = ref('');

// Coaches dropdown state
const coaches = ref([]);

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

const loadActivePlans = async () => {
  try {
    const response = await planService.getAll(1, 100);
    activePlans.value = response.data.filter(p => p.estado === 'Activo');
  } catch (err) {
    console.error('Error cargando planes activos', err);
  }
};

const loadCoaches = async () => {
  try {
    const response = await api.get('/socio/coaches');
    coaches.value = response.data;
  } catch (err) {
    console.error('Error cargando coaches', err);
  }
};

const availablePlans = computed(() => {
  if (isEditing.value && formSocioId.value) {
    const currentSocio = socios.value.find(s => s.id === formSocioId.value);
    if (currentSocio) {
      const exists = activePlans.value.some(p => p.id === currentSocio.idPlan);
      if (!exists) {
        return [
          ...activePlans.value,
          {
            id: currentSocio.idPlan,
            nombre: `${currentSocio.planNombre} (Inactivo)`,
            estado: 'Inactivo'
          }
        ];
      }
    }
  }
  return activePlans.value;
});

// Load data
const fetchSocios = async () => {
  try {
    errors.value = [];
    const result = await socioService.getAll(
      currentPage.value,
      pageSize.value,
      search.value,
      statusFilter.value,
      planFilter.value
    );
    socios.value = result.data;
    currentPage.value = result.pagination.currentPage;
    totalPages.value = result.pagination.totalPages;
    totalItems.value = result.pagination.totalItems;
  } catch (err) {
    handleError(err);
  }
};

// Handle errors
const handleError = (err) => {
  if (err.response && err.response.data && err.response.data.errors) {
    errors.value = err.response.data.errors;
  } else {
    errors.value = ['Ha ocurrido un error inesperado al conectar con el servidor.'];
  }
};

// Filter search & reset
let searchTimeout;
const onSearchInput = () => {
  clearTimeout(searchTimeout);
  searchTimeout = setTimeout(() => {
    currentPage.value = 1;
    fetchSocios();
  }, 350);
};

const triggerSearch = () => {
  currentPage.value = 1;
  fetchSocios();
};

const clearSearch = () => {
  search.value = '';
  statusFilter.value = '';
  planFilter.value = '';
  currentPage.value = 1;
  fetchSocios();
};

// Pagination
const prevPage = () => {
  if (currentPage.value > 1) {
    currentPage.value--;
    fetchSocios();
  }
};

const nextPage = () => {
  if (currentPage.value < totalPages.value) {
    currentPage.value++;
    fetchSocios();
  }
};

// Open drawer for creating
const openCreateDrawer = () => {
  resetForm();
  showDrawer.value = true;
};

// Edit handler
const editSocio = (socio) => {
  errors.value = [];
  isEditing.value = true;
  formSocioId.value = socio.id;
  formDni.value = socio.dni;
  formNombreCompleto.value = socio.nombreCompleto;
  formTelefono.value = socio.telefono || '';
  formEmail.value = socio.email || '';
  formFechaAlta.value = socio.fechaAlta.substring(0, 10);
  formEstado.value = socio.estado;
  formIdPlan.value = socio.idPlan;
  formIdCoach.value = socio.idCoach || '';
  formObservacion.value = socio.observacion || '';
  showDrawer.value = true;
};

// Form submission
const onSubmit = async () => {
  errors.value = [];

  // Frontend Validations
  if (!formDni.value.trim()) {
    errors.value.push('El DNI es obligatorio.');
  }
  if (!formNombreCompleto.value.trim()) {
    errors.value.push('El nombre completo es obligatorio.');
  }
  if (!formIdPlan.value) {
    errors.value.push('Debe seleccionar un plan válido.');
  }
  if (!formFechaAlta.value) {
    errors.value.push('La fecha de alta es obligatoria.');
  }
  if (formEmail.value.trim()) {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!emailRegex.test(formEmail.value.trim())) {
      errors.value.push('El formato de correo electrónico no es válido.');
    }
  }

  if (errors.value.length > 0) return;

  const payload = {
    dni: formDni.value.trim(),
    nombreCompleto: formNombreCompleto.value.trim(),
    telefono: formTelefono.value.trim() || null,
    email: formEmail.value.trim() || null,
    fechaAlta: new Date(formFechaAlta.value).toISOString(),
    estado: formEstado.value,
    idPlan: Number(formIdPlan.value),
    idCoach: formIdCoach.value ? Number(formIdCoach.value) : null,
    observacion: formObservacion.value.trim() || null
  };

  try {
    if (isEditing.value) {
      await socioService.update(formSocioId.value, payload);
    } else {
      await socioService.create(payload);
    }
    closeDrawer();
    fetchSocios();
  } catch (err) {
    handleError(err);
  }
};

// Change state (Activar/Inactivar)
const toggleStatus = async (socio) => {
  if (!isAdmin.value) return;
  errors.value = [];
  const newStatus = socio.estado === 'Activo' ? 'Inactivo' : 'Activo';
  try {
    await socioService.updateEstado(socio.id, newStatus);
    fetchSocios();
  } catch (err) {
    handleError(err);
  }
};

// Close drawer
const closeDrawer = () => {
  showDrawer.value = false;
  resetForm();
};

// Reset form to default values
const resetForm = () => {
  isEditing.value = false;
  formSocioId.value = null;
  formDni.value = '';
  formNombreCompleto.value = '';
  formTelefono.value = '';
  formEmail.value = '';
  formFechaAlta.value = new Date().toISOString().substring(0, 10);
  formEstado.value = 'Activo';
  formIdPlan.value = '';
  formIdCoach.value = '';
  formObservacion.value = '';
  errors.value = [];
};

// Format Date
const formatDate = (dateStr) => {
  if (!dateStr) return '-';
  const d = new Date(dateStr);
  return d.toLocaleDateString('es-AR');
};

onMounted(() => {
  fetchSocios();
  loadActivePlans();
  loadCoaches();
});
</script>

<template>
  <div class="socios-container">
    <header class="header">
      <h1>Gestión de Alumnos / Socios</h1>
      <p class="subtitle">Administración de socios e inscripción con asignación de Coach</p>
    </header>

    <!-- Error Alerts -->
    <div v-if="errors.length > 0" class="error-alert">
      <strong>Por favor, corrija los siguientes errores:</strong>
      <ul>
        <li v-for="(err, idx) in errors" :key="idx">{{ err }}</li>
      </ul>
    </div>

    <!-- Main Workspace -->
    <div class="workspace">
      <div class="main-content">
        <!-- Filters Card -->
        <div class="filters-card">
          <div class="filter-group flex-grow">
            <input
              type="text"
              v-model="search"
              placeholder="Buscar por DNI, nombre, teléfono o email..."
              @input="onSearchInput"
            />
          </div>
          
          <div class="filter-group">
            <select v-model="statusFilter" @change="triggerSearch">
              <option value="">Todos los estados</option>
              <option value="Activo">Activo</option>
              <option value="Inactivo">Inactivo</option>
            </select>
          </div>

          <div class="filter-buttons">
            <button @click="clearSearch" class="btn btn-secondary">Limpiar búsqueda</button>
            <button @click="openCreateDrawer" class="btn btn-primary">Nuevo Socio</button>
          </div>
        </div>

        <!-- Table Area -->
        <div v-if="socios.length > 0" class="table-responsive">
          <table class="data-table">
            <thead>
              <tr>
                <th>DNI</th>
                <th>Nombre Completo</th>
                <th>Actividad</th>
                <th>Coach Asignado</th>
                <th>Plan</th>
                <th>Teléfono</th>
                <th>Fecha Alta</th>
                <th class="status-col">Estado</th>
                <th class="actions-col">Acciones</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="socio in socios" :key="socio.id" :class="{ 'inactive-row': socio.estado === 'Inactivo' }">
                <td class="font-bold">{{ socio.dni }}</td>
                <td>
                  <div class="user-cell">
                    <div class="table-avatar">
                      <img v-if="socio.avatar" :src="getAvatarUrl(socio.avatar)" alt="Avatar" class="avatar-img-sm" />
                      <div v-else class="avatar-initial-sm">
                        {{ (socio.nombreCompleto || 'A').charAt(0).toUpperCase() }}
                      </div>
                    </div>
                    <span>{{ socio.nombreCompleto }}</span>
                  </div>
                </td>
                <td>
                  <span class="actividad-badge" :style="getActivityStyle(socio.actividadNombre)">{{ socio.actividadNombre || 'Musculación' }}</span>
                </td>
                <td>
                  <span v-if="socio.coachNombre" class="coach-tag">{{ cleanCoachName(socio.coachNombre) }}</span>
                  <span v-else class="subtle-text">Sin asignación</span>
                </td>
                <td>
                  <span class="plan-tag">{{ socio.planNombre }}</span>
                </td>
                <td>{{ socio.telefono || '-' }}</td>
                <td>{{ formatDate(socio.fechaAlta) }}</td>
                <td class="status-cell">
                  <span :class="['badge', socio.estado === 'Activo' ? 'badge-active' : 'badge-inactive']">
                    {{ socio.estado }}
                  </span>
                </td>
                <td class="actions-cell">
                  <div class="btn-actions-wrapper">
                    <button @click="editSocio(socio)" class="btn-action btn-action-edit" title="Editar">Editar</button>
                    <button
                      v-if="isAdmin"
                      @click="toggleStatus(socio)"
                      :class="['btn-action', socio.estado === 'Activo' ? 'btn-action-off' : 'btn-action-on']"
                    >
                      {{ socio.estado === 'Activo' ? 'Inactivar' : 'Activar' }}
                    </button>
                  </div>
                </td>
              </tr>
            </tbody>
          </table>
        </div>

        <div v-else class="empty-state-card">
          <span class="empty-icon">🔍</span>
          <p class="empty-text">No se encontraron socios registrados.</p>
        </div>

        <!-- Pagination -->
        <div class="pagination" v-if="totalPages > 0">
          <button @click="prevPage" :disabled="currentPage === 1" class="btn btn-page">Anterior</button>
          <span class="page-info">Página {{ currentPage }} de {{ totalPages }} ({{ totalItems }} items)</span>
          <button @click="nextPage" :disabled="currentPage === totalPages" class="btn btn-page">Siguiente</button>
        </div>
      </div>
    </div>

    <!-- Floating Centered Modal Dialog for Create / Edit Form -->
    <div v-if="showDrawer" class="modal-backdrop" @click.self="closeDrawer">
      <div class="modal-panel">
        <div class="modal-header">
          <h2>{{ isEditing ? `Editar Socio #${formSocioId}` : 'Nuevo Socio' }}</h2>
          <button @click="closeDrawer" class="btn-close-modal">✕</button>
        </div>

        <form @submit.prevent="onSubmit" class="modal-form">
          <div class="form-group">
            <label for="dni">DNI *</label>
            <input type="text" id="dni" v-model="formDni" required placeholder="Ej. 35123456" />
          </div>

          <div class="form-group">
            <label for="nombre">Nombre Completo *</label>
            <input type="text" id="nombre" v-model="formNombreCompleto" required placeholder="Ej. Juan Pérez" />
          </div>

          <div class="form-group">
            <label for="telefono">Teléfono</label>
            <input type="text" id="telefono" v-model="formTelefono" placeholder="Ej. 1150000000" />
          </div>

          <div class="form-group">
            <label for="email">Email</label>
            <input type="email" id="email" v-model="formEmail" placeholder="Ej. juan@gmail.com" />
          </div>

          <div class="form-group">
            <label for="fechaAlta">Fecha de Alta *</label>
            <input type="date" id="fechaAlta" v-model="formFechaAlta" required />
          </div>

          <div class="form-group">
            <label for="estado">Estado *</label>
            <select id="estado" v-model="formEstado">
              <option value="Activo">Activo</option>
              <option value="Inactivo">Inactivo</option>
            </select>
          </div>

          <!-- Plan Dropdown Selection -->
          <div class="form-group">
            <label for="idPlan">Plan *</label>
            <select id="idPlan" v-model="formIdPlan" required>
              <option value="" disabled>-- Seleccione un plan --</option>
              <option v-for="plan in availablePlans" :key="plan.id" :value="plan.id">
                {{ plan.nombre }}
              </option>
            </select>
          </div>

          <!-- Coach Dropdown Selection -->
          <div class="form-group">
            <label for="idCoach">Coach Asignado (Cupo máx. 20 alumnos)</label>
            <select id="idCoach" v-model="formIdCoach">
              <option value="">-- Sin Coach Asignado --</option>
              <option
                v-for="c in coaches"
                :key="c.id"
                :value="c.id"
                :disabled="c.cupoCompleto && Number(formIdCoach) !== c.id"
              >
                {{ cleanCoachName(c.nombre) }} ({{ c.alumnosActuales }}/20 {{ c.cupoCompleto ? 'COMPLETO' : 'disponibles' }})
              </option>
            </select>
          </div>

          <!-- Observaciones -->
          <div class="form-group">
            <label for="observacion">Observación</label>
            <textarea id="observacion" v-model="formObservacion" placeholder="Observaciones o notas adicionales del socio..." maxlength="500"></textarea>
          </div>

          <div class="form-buttons">
            <button type="submit" class="btn btn-primary">
              {{ isEditing ? 'Guardar Cambios' : 'Registrar Socio' }}
            </button>
            <button type="button" @click="closeDrawer" class="btn btn-secondary">
              Cancelar
            </button>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>

<style scoped>
.socios-container {
  width: 100%;
  max-width: 1400px;
  margin: 0 auto;
  padding: 28px 24px;
  text-align: left;
  box-sizing: border-box;
}

.header {
  margin-bottom: 24px;
  border-bottom: 1px solid var(--border);
  padding-bottom: 16px;
}

.header h1 {
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
  opacity: 0.75;
}

.workspace {
  width: 100%;
  box-sizing: border-box;
}

.main-content {
  background: var(--bg);
  border: 1px solid var(--border);
  border-radius: 12px;
  padding: 24px;
  box-shadow: var(--shadow);
  width: 100%;
  box-sizing: border-box;
}

.filters-card {
  display: flex;
  flex-wrap: wrap;
  gap: 15px;
  margin-bottom: 24px;
  align-items: center;
}

.filter-group {
  min-width: 240px;
}

.flex-grow {
  flex-grow: 1;
}

.filter-group input, .filter-group select {
  width: 100%;
  padding: 12px 14px;
  font-size: 15px;
  border: 1px solid var(--border);
  border-radius: 8px;
  outline: none;
  background-color: transparent;
  box-sizing: border-box;
  color: inherit;
}

.filter-group input:focus, .filter-group select:focus {
  border-color: var(--accent);
}

.filter-buttons {
  display: flex;
  gap: 10px;
}

.table-responsive {
  width: 100%;
  overflow-x: hidden;
}

.data-table {
  width: 100%;
  border-collapse: collapse;
  margin-bottom: 20px;
  font-size: 13px;
}

.data-table tr {
  border-bottom: 1px solid var(--border);
}

.data-table th, .data-table td {
  padding: 10px 14px;
  text-align: left;
  border: none;
  white-space: nowrap;
}

.data-table td {
  color: var(--text-h);
}

.data-table th {
  font-weight: 600;
  color: var(--text-h);
  background-color: var(--code-bg);
}

.font-bold {
  font-weight: 600;
  color: var(--text-h);
}

.plan-tag {
  background-color: var(--accent-bg);
  color: var(--accent);
  padding: 3px 8px;
  border-radius: 6px;
  font-size: 12px;
  font-weight: 600;
}

.coach-tag {
  background-color: rgba(245, 158, 11, 0.12);
  color: #d97706;
  border: 1px solid rgba(245, 158, 11, 0.3);
  padding: 3px 8px;
  border-radius: 6px;
  font-size: 12px;
  font-weight: 600;
}

.subtle-text {
  color: var(--text);
  opacity: 0.6;
  font-style: italic;
}

.inactive-row {
  opacity: 0.65;
  background-color: rgba(0, 0, 0, 0.01);
}

.empty-state-card {
  text-align: center;
  padding: 45px;
  color: var(--text);
  background-color: var(--code-bg);
  border-radius: 8px;
}

.status-col, .status-cell {
  width: 95px;
  min-width: 95px;
  text-align: center;
}

.badge {
  display: inline-block;
  padding: 3px 9px;
  font-size: 11.5px;
  font-weight: 600;
  border-radius: 20px;
  min-width: 70px;
  text-align: center;
  box-sizing: border-box;
}

.badge-active {
  background-color: rgba(46, 204, 113, 0.15);
  color: #27ae60;
  border: 1px solid rgba(46, 204, 113, 0.3);
}

.badge-inactive {
  background-color: rgba(231, 76, 60, 0.15);
  color: #c0392b;
  border: 1px solid rgba(231, 76, 60, 0.3);
}

.actions-col, .actions-cell {
  width: 145px;
  min-width: 145px;
  vertical-align: middle;
}

.btn-actions-wrapper {
  display: flex;
  gap: 6px;
  align-items: center;
}

.btn-action {
  padding: 4px 9px;
  font-size: 11px;
  font-weight: 600;
  border-radius: 6px;
  cursor: pointer;
  border: 1px solid transparent;
  transition: all 0.15s ease;
  white-space: nowrap;
  min-width: 68px;
  text-align: center;
  box-sizing: border-box;
}

.actividad-badge {
  background-color: rgba(99, 102, 241, 0.12);
  color: #6366f1;
  border: 1px solid rgba(99, 102, 241, 0.3);
  padding: 4px 10px;
  border-radius: 12px;
  font-size: 12px;
  font-weight: 600;
  display: inline-block;
}

.btn-action-edit {
  background-color: var(--code-bg);
  border-color: var(--border);
  color: var(--text-h);
}

.btn-action-edit:hover {
  border-color: var(--accent);
  color: var(--accent);
}

.btn-action-off {
  background-color: rgba(239, 68, 68, 0.12);
  border-color: rgba(239, 68, 68, 0.3);
  color: #ef4444;
}

.btn-action-off:hover {
  background-color: rgba(239, 68, 68, 0.2);
}

.btn-action-on {
  background-color: rgba(16, 185, 129, 0.12);
  border-color: rgba(16, 185, 129, 0.3);
  color: #10b981;
}

.btn-action-on:hover {
  background-color: rgba(16, 185, 129, 0.2);
}

.modal-backdrop {
  position: fixed;
  top: 0;
  left: 0;
  width: 100vw;
  height: 100vh;
  background-color: rgba(0, 0, 0, 0.65);
  backdrop-filter: blur(5px);
  z-index: 1000;
  display: flex;
  justify-content: center;
  align-items: center;
  box-sizing: border-box;
}

.modal-panel {
  background-color: var(--code-bg);
  width: 480px;
  max-width: 90vw;
  max-height: 85vh;
  box-shadow: var(--shadow);
  padding: 30px;
  display: flex;
  flex-direction: column;
  box-sizing: border-box;
  overflow-y: auto;
  border: 1px solid var(--border);
  border-radius: 12px;
  position: relative;
  z-index: 1001;
  text-align: left;
}

.modal-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 25px;
  border-bottom: 1px solid var(--border);
  padding-bottom: 15px;
}

.modal-header h2 {
  margin: 0;
  font-size: 22px;
  color: var(--text-h);
}

.btn-close-modal {
  background: transparent;
  border: none;
  font-size: 20px;
  color: var(--text);
  cursor: pointer;
  padding: 4px;
}

.btn-close-modal:hover {
  color: var(--neon);
}

.modal-form {
  display: flex;
  flex-direction: column;
}

.form-group {
  margin-bottom: 18px;
}

.form-group label {
  display: block;
  font-weight: 500;
  margin-bottom: 6px;
  font-size: 14px;
  color: var(--text-h);
}

.form-group input, .form-group textarea, .form-group select {
  width: 100%;
  padding: 11px;
  font-size: 15px;
  border: 1px solid var(--border);
  border-radius: 8px;
  outline: none;
  background-color: transparent;
  box-sizing: border-box;
  color: inherit;
}

.form-group input:focus, .form-group select:focus {
  border-color: var(--accent);
}

.form-buttons {
  display: flex;
  gap: 12px;
  margin-top: 25px;
}

.btn {
  padding: 11px 20px;
  font-size: 15px;
  font-weight: 500;
  border: 1px solid transparent;
  border-radius: 8px;
  cursor: pointer;
  transition: all 0.2s;
  display: inline-flex;
  align-items: center;
  justify-content: center;
}

.btn-sm {
  padding: 6px 12px;
  font-size: 13px;
  border-radius: 6px;
}

.btn-primary {
  background-color: var(--accent);
  color: #fff;
}

.btn-primary:hover {
  filter: brightness(0.9);
}

.btn-secondary {
  background-color: transparent;
  border-color: var(--border);
  color: var(--text-h);
}

.btn-secondary:hover {
  background-color: var(--code-bg);
}

.btn-edit {
  background-color: rgba(52, 152, 219, 0.15);
  color: #2980b9;
}

.btn-status-inactive {
  background-color: rgba(231, 76, 60, 0.15);
  color: #c0392b;
}

.btn-status-active {
  background-color: rgba(46, 204, 113, 0.15);
  color: #27ae60;
}

.pagination {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-top: 20px;
}

.error-alert {
  background-color: rgba(231, 76, 60, 0.15);
  border: 1px solid rgba(231, 76, 60, 0.3);
  color: #c0392b;
  padding: 15px;
  border-radius: 8px;
  margin-bottom: 20px;
  font-size: 14px;
}

.error-alert ul {
  margin: 8px 0 0 0;
  padding-left: 20px;
}
</style>

<script setup>
import { ref, onMounted, computed } from 'vue';
import planService from '../services/planService';
import authService from '../services/authService';

// Auth State
const isAdmin = computed(() => authService.hasRole('Administrador'));

// State
const planes = ref([]);
const currentPage = ref(1);
const pageSize = ref(10); // Roomier default page size
const totalPages = ref(0);
const totalItems = ref(0);
const search = ref('');
const errors = ref([]);

// Form / Drawer State
const showDrawer = ref(false);
const isEditing = ref(false);
const formPlanId = ref(null);
const formName = ref('');
const formDescription = ref('');
const formPrice = ref(0);
const formStatus = ref('Activo');

// Load data
const fetchPlanes = async () => {
  try {
    errors.value = [];
    const result = await planService.getAll(currentPage.value, pageSize.value, search.value);
    planes.value = result.data;
    currentPage.value = result.pagination.currentPage;
    totalPages.value = result.pagination.totalPages;
    totalItems.value = result.pagination.totalItems;
  } catch (err) {
    handleError(err);
  }
};

// Handle server/validation errors
const handleError = (err) => {
  if (err.response && err.response.data && err.response.data.errors) {
    errors.value = err.response.data.errors;
  } else {
    errors.value = ['Ha ocurrido un error inesperado al conectar con el servidor.'];
  }
};

// Search watchers/triggers
let searchTimeout;
const onSearchInput = () => {
  clearTimeout(searchTimeout);
  searchTimeout = setTimeout(() => {
    currentPage.value = 1;
    fetchPlanes();
  }, 350);
};

// Clear search only
const clearSearch = () => {
  search.value = '';
  currentPage.value = 1;
  fetchPlanes();
};

// Pagination handlers
const prevPage = () => {
  if (currentPage.value > 1) {
    currentPage.value--;
    fetchPlanes();
  }
};

const nextPage = () => {
  if (currentPage.value < totalPages.value) {
    currentPage.value++;
    fetchPlanes();
  }
};

// Open drawer for creating
const openCreateDrawer = () => {
  resetForm();
  showDrawer.value = true;
};

// Open form for editing
const editPlan = (plan) => {
  if (!isAdmin.value) return;
  errors.value = [];
  isEditing.value = true;
  formPlanId.value = plan.id;
  formName.value = plan.nombre;
  formDescription.value = plan.descripcion || '';
  formPrice.value = plan.precioMensual;
  formStatus.value = plan.estado;
  showDrawer.value = true;
};

// Close drawer
const closeDrawer = () => {
  showDrawer.value = false;
  resetForm();
};

// Submit form (create or update)
const onSubmit = async () => {
  if (!isAdmin.value) return;
  errors.value = [];
  const payload = {
    nombre: formName.value,
    descripcion: formDescription.value,
    precioMensual: Number(formPrice.value),
    estado: formStatus.value
  };

  try {
    if (isEditing.value) {
      await planService.update(formPlanId.value, payload);
    } else {
      await planService.create(payload);
    }
    closeDrawer();
    fetchPlanes();
  } catch (err) {
    handleError(err);
  }
};

// Change state (Activar/Inactivar)
const toggleStatus = async (plan) => {
  if (!isAdmin.value) return;
  errors.value = [];
  const newStatus = plan.estado === 'Activo' ? 'Inactivo' : 'Activo';
  try {
    await planService.updateEstado(plan.id, newStatus);
    fetchPlanes();
  } catch (err) {
    handleError(err);
  }
};

// Reset form to default values
const resetForm = () => {
  isEditing.value = false;
  formPlanId.value = null;
  formName.value = '';
  formDescription.value = '';
  formPrice.value = 0;
  formStatus.value = 'Activo';
  errors.value = [];
};

// Initial load
onMounted(() => {
  fetchPlanes();
});
</script>

<template>
  <div class="planes-container">
    <header class="header">
      <h1>Gestión de Planes</h1>
      <p class="subtitle">Creación, edición y administración de los planes del gimnasio</p>
    </header>

    <!-- Error Alert Container -->
    <div v-if="errors.length > 0" class="error-alert">
      <strong>Por favor, corrija los siguientes errores:</strong>
      <ul>
        <li v-for="(err, idx) in errors" :key="idx">{{ err }}</li>
      </ul>
    </div>

    <!-- Main Workspace (Takes 100% width) -->
    <div class="workspace">
      <div class="main-content">
        <div class="table-actions">
          <div class="search-box">
            <input
              type="text"
              v-model="search"
              @input="onSearchInput"
              placeholder="Buscar por nombre o descripción..."
            />
          </div>
          <div class="filter-buttons">
            <button @click="clearSearch" class="btn btn-secondary">Limpiar búsqueda</button>
            <button v-if="isAdmin" @click="openCreateDrawer" class="btn btn-primary">Nuevo Plan</button>
          </div>
        </div>

        <div v-if="planes.length > 0" class="table-responsive">
          <table class="data-table">
            <thead>
              <tr>
                <th>Nombre</th>
                <th>Descripción</th>
                <th>Precio Mensual</th>
                <th>Estado</th>
                <th v-if="isAdmin" class="actions-col">Acciones</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="plan in planes" :key="plan.id" :class="{ 'inactive-row': plan.estado === 'Inactivo' }">
                <td class="font-bold">{{ plan.nombre }}</td>
                <td>{{ plan.descripcion || '-' }}</td>
                <td>${{ plan.precioMensual.toLocaleString('es-AR', { minimumFractionDigits: 2 }) }}</td>
                <td>
                  <span :class="['badge', plan.estado === 'Activo' ? 'badge-active' : 'badge-inactive']">
                    {{ plan.estado }}
                  </span>
                </td>
                <td v-if="isAdmin" class="actions-cell">
                  <button @click="editPlan(plan)" class="btn btn-sm btn-edit">Editar</button>
                  <button
                    @click="toggleStatus(plan)"
                    :class="['btn', 'btn-sm', plan.estado === 'Activo' ? 'btn-status-inactive' : 'btn-status-active']"
                  >
                    {{ plan.estado === 'Activo' ? 'Inactivar' : 'Activar' }}
                  </button>
                </td>
              </tr>
            </tbody>
          </table>
        </div>

        <div v-else class="empty-state-card">
          <span class="empty-icon">🔍</span>
          <p class="empty-text">No se encontraron planes registrados.</p>
        </div>

        <!-- Pagination -->
        <div class="pagination" v-if="totalPages > 0">
          <button @click="prevPage" :disabled="currentPage === 1" class="btn btn-page">Anterior</button>
          <span class="page-info">Página {{ currentPage }} de {{ totalPages }} ({{ totalItems }} items)</span>
          <button @click="nextPage" :disabled="currentPage === totalPages" class="btn btn-page">Siguiente</button>
        </div>
      </div>
    </div>

    <!-- Floating Centered Modal Dialog for Create / Edit Form (Admin Only) -->
    <div v-if="showDrawer && isAdmin" class="modal-backdrop" @click.self="closeDrawer">
      <div class="modal-panel">
        <div class="modal-header">
          <h2>{{ isEditing ? `Editar Plan #${formPlanId}` : 'Nuevo Plan' }}</h2>
          <button @click="closeDrawer" class="btn-close-modal">✕</button>
        </div>

        <form @submit.prevent="onSubmit" class="modal-form">
          <div class="form-group">
            <label for="name">Nombre del Plan *</label>
            <input type="text" id="name" v-model="formName" required maxlength="100" placeholder="Ej. Plan Pase Libre" />
          </div>

          <div class="form-group">
            <label for="description">Descripción</label>
            <textarea id="description" v-model="formDescription" placeholder="Detalles de los beneficios del plan"></textarea>
          </div>

          <div class="form-group">
            <label for="price">Precio Mensual ($) *</label>
            <input type="number" id="price" v-model="formPrice" required min="1" step="any" />
          </div>

          <div class="form-group">
            <label for="status">Estado *</label>
            <select id="status" v-model="formStatus">
              <option value="Activo">Activo</option>
              <option value="Inactivo">Inactivo</option>
            </select>
          </div>

          <div class="form-buttons">
            <button type="submit" class="btn btn-primary">
              {{ isEditing ? 'Guardar Cambios' : 'Crear Plan' }}
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
.planes-container {
  width: 100%;
  max-width: 1120px;
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

.table-actions {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 24px;
  gap: 15px;
}

.search-box {
  flex-grow: 1;
}

.search-box input {
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

.search-box input:focus {
  border-color: var(--accent);
}

.filter-buttons {
  display: flex;
  gap: 10px;
}

.table-responsive {
  width: 100%;
  overflow-x: auto;
}

.data-table {
  width: 100%;
  border-collapse: collapse;
  margin-bottom: 20px;
  font-size: 15px;
}

.data-table th, .data-table td {
  padding: 14px 18px;
  border-bottom: 1px solid var(--border);
  text-align: left;
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

.inactive-row {
  opacity: 0.65;
  background-color: rgba(0, 0, 0, 0.01);
}

.empty-state {
  text-align: center;
  padding: 45px;
  color: var(--text);
  font-style: italic;
}

.badge {
  display: inline-block;
  padding: 5px 12px;
  font-size: 13px;
  font-weight: 500;
  border-radius: 30px;
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

.actions-col {
  width: 160px;
}

.actions-cell {
  display: flex;
  gap: 8px;
}

/* Centered Floating Modal Dialog styles */
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

.form-group input:focus, .form-group textarea:focus, .form-group select:focus {
  border-color: var(--accent);
}

.form-group textarea {
  height: 100px;
  resize: vertical;
}

.form-buttons {
  display: flex;
  gap: 12px;
  margin-top: 25px;
}

/* Buttons */
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
  border: 1px solid rgba(52, 152, 219, 0.3);
}

.btn-edit:hover {
  background-color: #2980b9;
  color: #fff;
}

.btn-status-inactive {
  background-color: rgba(231, 76, 60, 0.15);
  color: #c0392b;
  border: 1px solid rgba(231, 76, 60, 0.3);
}

.btn-status-inactive:hover {
  background-color: #c0392b;
  color: #fff;
}

.btn-status-active {
  background-color: rgba(46, 204, 113, 0.15);
  color: #27ae60;
  border: 1px solid rgba(46, 204, 113, 0.3);
}

.btn-status-active:hover {
  background-color: #27ae60;
  color: #fff;
}

/* Pagination */
.pagination {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-top: 20px;
}

.page-info {
  font-size: 14px;
  color: var(--text);
}

.btn-page {
  padding: 6px 14px;
  font-size: 14px;
  border-color: var(--border);
  background: transparent;
  color: var(--text-h);
}

.btn-page:disabled {
  opacity: 0.4;
  cursor: not-allowed;
}

/* Errors */
.error-alert {
  background-color: rgba(231, 76, 60, 0.1);
  border: 1px solid rgba(231, 76, 60, 0.3);
  color: #c0392b;
  padding: 16px;
  border-radius: 8px;
  margin-bottom: 24px;
  font-size: 15px;
}

.error-alert strong {
  display: block;
  margin-bottom: 8px;
}

.error-alert ul {
  margin: 0;
  padding-left: 20px;
}

/* Empty State Card styling */
.empty-state-card {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 60px 20px;
  border: 1px dashed var(--border);
  border-radius: 8px;
  background-color: var(--code-bg);
  color: var(--text);
  text-align: center;
  margin-top: 10px;
  width: 100%;
  box-sizing: border-box;
}

.empty-icon {
  font-size: 36px;
  margin-bottom: 12px;
}

.empty-text {
  font-size: 16px;
  margin: 0;
  font-style: italic;
}
</style>

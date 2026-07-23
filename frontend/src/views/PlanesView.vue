<script setup>
import { ref, onMounted, computed } from 'vue';
import planService from '../services/planService';
import authService from '../services/authService';

// Auth State
const isAdmin = computed(() => authService.hasRole('Administrador'));

// State
const planes = ref([]);
const currentPage = ref(1);
const pageSize = ref(5);
const totalPages = ref(0);
const totalItems = ref(0);
const search = ref('');
const errors = ref([]);

// Form State
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
    resetForm();
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

    <div :class="['workspace', { 'admin-grid': isAdmin }]">
      <!-- Left side: Table and Filters -->
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
          <button @click="clearSearch" class="btn btn-secondary">Limpiar búsqueda</button>
        </div>

        <div class="table-responsive">
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
              <tr v-if="planes.length === 0">
                <td :colspan="isAdmin ? 5 : 4" class="empty-state">No se encontraron planes registrados.</td>
              </tr>
            </tbody>
          </table>
        </div>

        <!-- Pagination -->
        <div class="pagination" v-if="totalPages > 0">
          <button @click="prevPage" :disabled="currentPage === 1" class="btn btn-page">Anterior</button>
          <span class="page-info">Página {{ currentPage }} de {{ totalPages }} ({{ totalItems }} items)</span>
          <button @click="nextPage" :disabled="currentPage === totalPages" class="btn btn-page">Siguiente</button>
        </div>
      </div>

      <!-- Right side: Form (Create / Edit) - Visible only for Admin -->
      <div v-if="isAdmin" class="form-container">
        <h2>{{ isEditing ? `Editar Plan #${formPlanId}` : 'Nuevo Plan' }}</h2>
        <form @submit.prevent="onSubmit">
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
            <button type="button" @click="resetForm" class="btn btn-secondary">
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
  max-width: 1200px;
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

.workspace {
  display: grid;
  grid-template-columns: 1fr;
  gap: 30px;
  align-items: start;
}

.workspace.admin-grid {
  grid-template-columns: 1fr 380px;
}

@media (max-width: 900px) {
  .workspace.admin-grid {
    grid-template-columns: 1fr;
  }
}

.main-content {
  background: var(--bg);
  border: 1px solid var(--border);
  border-radius: 12px;
  padding: 24px;
  box-shadow: var(--shadow);
}

.table-actions {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 20px;
  gap: 15px;
}

.search-box {
  flex-grow: 1;
}

.search-box input {
  width: 100%;
  padding: 10px 14px;
  font-size: 15px;
  border: 1px solid var(--border);
  border-radius: 8px;
  outline: none;
  transition: border-color 0.2s;
  box-sizing: border-box;
}

.search-box input:focus {
  border-color: var(--accent);
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
  padding: 12px 16px;
  border-bottom: 1px solid var(--border);
  text-align: left;
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
  background-color: rgba(0,0,0,0.01);
}

.empty-state {
  text-align: center;
  padding: 40px;
  color: var(--text);
  font-style: italic;
}

.badge {
  display: inline-block;
  padding: 4px 10px;
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
  width: 180px;
}

.actions-cell {
  display: flex;
  gap: 8px;
}

/* Forms */
.form-container {
  background: var(--bg);
  border: 1px solid var(--border);
  border-radius: 12px;
  padding: 24px;
  box-shadow: var(--shadow);
}

.form-container h2 {
  margin-top: 0;
  margin-bottom: 20px;
  font-size: 20px;
  border-bottom: 1px solid var(--border);
  padding-bottom: 10px;
}

.form-group {
  margin-bottom: 16px;
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
  padding: 10px;
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
  height: 80px;
  resize: vertical;
}

.form-buttons {
  display: flex;
  gap: 10px;
  margin-top: 24px;
}

/* Buttons */
.btn {
  padding: 10px 18px;
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
</style>

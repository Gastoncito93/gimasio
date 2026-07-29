<script setup>
import { ref, onMounted, computed } from 'vue';
import cuotaService from '../services/cuotaService';
import socioService from '../services/socioService';
import authService from '../services/authService';
import planService from '../services/planService';

// Auth State
const isAdmin = computed(() => authService.hasRole('Administrador'));

// Table / Filter State
const cuotas = ref([]);
const currentPage = ref(1);
const pageSize = ref(10);
const totalPages = ref(0);
const totalItems = ref(0);
const search = ref('');
const statusFilter = ref('');
const periodFilter = ref('');
const errors = ref([]);

// Modals State
const showCreateModal = ref(false);
const showEditObsModal = ref(false);
const showPagarModal = ref(false);

// Create Cuota Form State
const selectedSocio = ref(null); // { id, dni, nombreCompleto, planNombre }
const socioSearchQuery = ref('');
const socioSuggestions = ref([]);
const showSocioSuggestions = ref(false);

const formMes = ref('');
const formAnio = ref('');
const formMonto = ref(0);
const formFechaVencimiento = ref(new Date().toISOString().substring(0, 10));
const formObservacion = ref('');

const mesesOptions = [
  { value: '01', label: 'Enero' },
  { value: '02', label: 'Febrero' },
  { value: '03', label: 'Marzo' },
  { value: '04', label: 'Abril' },
  { value: '05', label: 'Mayo' },
  { value: '06', label: 'Junio' },
  { value: '07', label: 'Julio' },
  { value: '08', label: 'Agosto' },
  { value: '09', label: 'Septiembre' },
  { value: '10', label: 'Octubre' },
  { value: '11', label: 'Noviembre' },
  { value: '12', label: 'Diciembre' }
];

const aniosOptions = computed(() => {
  const currentYear = new Date().getFullYear();
  const list = [];
  for (let y = currentYear - 2; y <= currentYear + 3; y++) {
    list.push(y.toString());
  }
  return list;
});

// Dynamic list of periods for the search filter dropdown
const periodOptions = computed(() => {
  const options = [];
  const date = new Date();
  // Go back 12 months, go forward 6 months
  date.setMonth(date.getMonth() - 12);
  for (let i = 0; i < 19; i++) {
    const monthVal = (date.getMonth() + 1).toString().padStart(2, '0');
    const yearVal = date.getFullYear().toString();
    const monthNames = [
      'Enero', 'Febrero', 'Marzo', 'Abril', 'Mayo', 'Junio',
      'Julio', 'Agosto', 'Septiembre', 'Octubre', 'Noviembre', 'Diciembre'
    ];
    options.push({
      value: `${yearVal}${monthVal}`,
      label: `${monthNames[date.getMonth()]} ${yearVal}`
    });
    date.setMonth(date.getMonth() + 1);
  }
  return options.reverse();
});

// Edit Observation Form State
const editCuotaId = ref(null);
const editObservacion = ref('');

// Pagar Form State
const pagarCuotaId = ref(null);
const pagarFechaPago = ref(new Date().toISOString().substring(0, 10));

// Load data
const fetchCuotas = async () => {
  try {
    errors.value = [];
    const result = await cuotaService.getAll(
      currentPage.value,
      pageSize.value,
      search.value,
      statusFilter.value,
      periodFilter.value
    );
    cuotas.value = result.data;
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

// AJAX Socio Autocomplete
const onSocioSearchInput = async () => {
  if (socioSearchQuery.value.trim().length < 3) {
    socioSuggestions.value = [];
    showSocioSuggestions.value = false;
    return;
  }

  try {
    // We import socioService dynamically or directly. Let's make sure import is correct.
    // Wait, earlier we created: frontend/src/services/socioService.js.
    // Let's import it from '../services/socioService' instead of '../socioService'!
    // Let's check import at top: import socioService from '../services/socioService';
    const result = await socioService.buscar(socioSearchQuery.value, 10);
    socioSuggestions.value = result;
    showSocioSuggestions.value = result.length > 0;
  } catch (err) {
    console.error('Error buscando socios', err);
  }
};

const selectSocio = (socio) => {
  selectedSocio.value = socio;
  socioSearchQuery.value = '';
  socioSuggestions.value = [];
  showSocioSuggestions.value = false;
};

const clearSelectedSocio = () => {
  selectedSocio.value = null;
};

// Filter search & reset
let searchTimeout;
const onSearchInput = () => {
  clearTimeout(searchTimeout);
  searchTimeout = setTimeout(() => {
    currentPage.value = 1;
    fetchCuotas();
  }, 350);
};

const triggerSearch = () => {
  currentPage.value = 1;
  fetchCuotas();
};

const clearSearch = () => {
  search.value = '';
  statusFilter.value = '';
  periodFilter.value = '';
  currentPage.value = 1;
  fetchCuotas();
};

// Pagination
const prevPage = () => {
  if (currentPage.value > 1) {
    currentPage.value--;
    fetchCuotas();
  }
};

const nextPage = () => {
  if (currentPage.value < totalPages.value) {
    currentPage.value++;
    fetchCuotas();
  }
};

// Create Modal actions
const openCreateModal = () => {
  errors.value = [];
  selectedSocio.value = null;
  socioSearchQuery.value = '';
  socioSuggestions.value = [];
  showSocioSuggestions.value = false;
  
  const today = new Date();
  formMes.value = (today.getMonth() + 1).toString().padStart(2, '0');
  formAnio.value = today.getFullYear().toString();

  formMonto.value = 0;
  formFechaVencimiento.value = new Date().toISOString().substring(0, 10);
  formObservacion.value = '';
  showCreateModal.value = true;
};

const closeCreateModal = () => {
  showCreateModal.value = false;
};

const onCreateSubmit = async () => {
  errors.value = [];

  // Validations
  if (!selectedSocio.value) {
    errors.value.push('Debe seleccionar un socio.');
  }

  if (!formMes.value || !formAnio.value) {
    errors.value.push('El mes y el año del período son obligatorios.');
  }

  if (Number(formMonto.value) <= 0) {
    errors.value.push('El monto debe ser mayor que 0.');
  }

  if (!formFechaVencimiento.value) {
    errors.value.push('La fecha de vencimiento es obligatoria.');
  }

  if (errors.value.length > 0) return;

  const combinedPeriod = Number(formAnio.value + formMes.value);

  const payload = {
    idSocio: selectedSocio.value.id,
    periodo: combinedPeriod,
    monto: Number(formMonto.value),
    fechaVencimiento: new Date(formFechaVencimiento.value).toISOString(),
    observacion: formObservacion.value.trim() || null
  };

  try {
    await cuotaService.create(payload);
    closeCreateModal();
    fetchCuotas();
  } catch (err) {
    handleError(err);
  }
};

// Edit Observation Modal actions
const openEditObsModal = (cuota) => {
  errors.value = [];
  editCuotaId.value = cuota.id;
  editObservacion.value = cuota.observacion || '';
  showEditObsModal.value = true;
};

const closeEditObsModal = () => {
  showEditObsModal.value = false;
  editCuotaId.value = null;
  editObservacion.value = '';
};

const onEditObsSubmit = async () => {
  errors.value = [];
  try {
    await cuotaService.updateObservacion(editCuotaId.value, editObservacion.value.trim() || null);
    closeEditObsModal();
    fetchCuotas();
  } catch (err) {
    handleError(err);
  }
};

// Pagar Modal actions
const openPagarModal = (cuota) => {
  errors.value = [];
  pagarCuotaId.value = cuota.id;
  pagarFechaPago.value = new Date().toISOString().substring(0, 10);
  showPagarModal.value = true;
};

const closePagarModal = () => {
  showPagarModal.value = false;
  pagarCuotaId.value = null;
  pagarFechaPago.value = new Date().toISOString().substring(0, 10);
};

const onPagarSubmit = async () => {
  errors.value = [];
  if (!pagarFechaPago.value) {
    errors.value.push('La fecha de pago es obligatoria para registrar el pago.');
    return;
  }

  if (!confirm('¿Confirmar el registro de pago para esta cuota?')) {
    return;
  }

  try {
    await cuotaService.pagar(pagarCuotaId.value, new Date(pagarFechaPago.value).toISOString());
    closePagarModal();
    fetchCuotas();
  } catch (err) {
    handleError(err);
  }
};

// Anular action
const triggerAnular = async (cuota) => {
  if (!isAdmin.value) return;
  errors.value = [];
  if (!confirm(`¿Está seguro que desea ANULAR la cuota del período ${cuota.periodo} para el socio ${cuota.socioNombreCompleto}? Esta acción no se puede deshacer.`)) {
    return;
  }

  try {
    await cuotaService.anular(cuota.id);
    fetchCuotas();
  } catch (err) {
    handleError(err);
  }
};

// Date Formatters
const formatDate = (dateStr) => {
  if (!dateStr) return '-';
  const d = new Date(dateStr);
  return d.toLocaleDateString('es-AR');
};

const formatPeriod = (p) => {
  if (!p) return '-';
  const pStr = p.toString();
  if (pStr.length === 6) {
    return `${pStr.substring(4, 6)}/${pStr.substring(0, 4)}`;
  }
  return p;
};

onMounted(() => {
  fetchCuotas();
});
</script>

<template>
  <div class="cuotas-container">
    <header class="header">
      <h1>Gestión de Cuotas</h1>
      <p class="subtitle">Administración de cuotas, cobros y anulaciones de membresías</p>
    </header>

    <!-- Error Alerts -->
    <div v-if="errors.length > 0" class="error-alert">
      <strong>Por favor, corrija los siguientes errores:</strong>
      <ul>
        <li v-for="(err, idx) in errors" :key="idx">{{ err }}</li>
      </ul>
    </div>

    <!-- Main Workspace (Wide list view) -->
    <div class="workspace">
      <div class="main-content">
        <!-- Filters Area -->
        <div class="filters-card">
          <div class="filter-group flex-grow">
            <input
              type="text"
              v-model="search"
              placeholder="Buscar por DNI o nombre del socio..."
              @input="onSearchInput"
            />
          </div>

          <div class="filter-group select-filter">
            <select v-model="statusFilter" @change="triggerSearch">
              <option value="">Todos los estados</option>
              <option value="Pendiente">Pendiente</option>
              <option value="Pagado">Pagado</option>
              <option value="Anulado">Anulado</option>
            </select>
          </div>

          <div class="filter-group select-filter">
            <select v-model="periodFilter" @change="triggerSearch">
              <option value="">Todos los períodos</option>
              <option v-for="opt in periodOptions" :key="opt.value" :value="opt.value">
                {{ opt.label }}
              </option>
            </select>
          </div>

          <div class="filter-buttons">
            <button @click="clearSearch" class="btn btn-secondary">Limpiar búsqueda</button>
            <button @click="openCreateModal" class="btn btn-primary">Nueva Cuota</button>
          </div>
        </div>

        <!-- Table Area -->
        <div v-if="cuotas.length > 0" class="table-responsive">
          <table class="data-table">
            <thead>
              <tr>
                <th>Socio</th>
                <th>DNI</th>
                <th>Período</th>
                <th>Monto</th>
                <th>Fecha Vencimiento</th>
                <th>Fecha Pago</th>
                <th>Estado</th>
                <th>Observación</th>
                <th class="actions-col">Acciones</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="cuota in cuotas" :key="cuota.id" :class="{ 'inactive-row': cuota.Estado === 'Anulado' }">
                <td class="font-bold">{{ cuota.socioNombreCompleto }}</td>
                <td>{{ cuota.socioDni }}</td>
                <td>
                  <span class="period-tag">{{ formatPeriod(cuota.periodo) }}</span>
                </td>
                <td>${{ cuota.monto.toLocaleString('es-AR', { minimumFractionDigits: 2 }) }}</td>
                <td>{{ formatDate(cuota.fechaVencimiento) }}</td>
                <td>{{ formatDate(cuota.fechaPago) }}</td>
                <td>
                  <span :class="['badge', 
                    cuota.estado === 'Pagado' ? 'badge-active' : 
                    cuota.estado === 'Pendiente' ? 'badge-pending' : 'badge-inactive']">
                    {{ cuota.estado }}
                  </span>
                </td>
                <td class="obs-cell" :title="cuota.observacion">{{ cuota.observacion || '-' }}</td>
                <td class="actions-cell">
                  <!-- Edit Observation -->
                  <button @click="openEditObsModal(cuota)" class="btn btn-sm btn-edit">Obs</button>
                  
                  <!-- Registrar Pago -->
                  <button 
                    v-if="cuota.estado === 'Pendiente'" 
                    @click="openPagarModal(cuota)" 
                    class="btn btn-sm btn-status-active"
                  >
                    Pagar
                  </button>

                  <!-- Anular (Admin Only) -->
                  <button 
                    v-if="isAdmin && cuota.estado !== 'Anulado'" 
                    @click="triggerAnular(cuota)" 
                    class="btn btn-sm btn-status-inactive"
                  >
                    Anular
                  </button>
                </td>
              </tr>
            </tbody>
          </table>
        </div>

        <!-- Empty state card -->
        <div v-else class="empty-state-card">
          <span class="empty-icon">🔍</span>
          <p class="empty-text">No se encontraron cuotas registradas.</p>
        </div>

        <!-- Pagination -->
        <div class="pagination" v-if="totalPages > 0">
          <button @click="prevPage" :disabled="currentPage === 1" class="btn btn-page">Anterior</button>
          <span class="page-info">Página {{ currentPage }} de {{ totalPages }} ({{ totalItems }} items)</span>
          <button @click="nextPage" :disabled="currentPage === totalPages" class="btn btn-page">Siguiente</button>
        </div>
      </div>
    </div>

    <!-- MODAL 1: NUEVA CUOTA -->
    <div v-if="showCreateModal" class="modal-backdrop" @click.self="closeCreateModal">
      <div class="modal-panel">
        <div class="modal-header">
          <h2>Nueva Cuota</h2>
          <button @click="closeCreateModal" class="btn-close-modal">✕</button>
        </div>

        <form @submit.prevent="onCreateSubmit" class="modal-form">
          <!-- Selection of Socio by Ajax search -->
          <div class="form-group relative">
            <label>Socio *</label>

            <!-- Socio Selected Tag -->
            <div v-if="selectedSocio" class="selected-socio-box">
              <div class="selected-socio-info">
                <span class="socio-name">{{ selectedSocio.nombreCompleto }}</span>
                <span class="socio-details">DNI: {{ selectedSocio.dni }} - {{ selectedSocio.planNombre }}</span>
              </div>
              <button type="button" @click="clearSelectedSocio" class="btn-clear-socio" title="Cambiar socio">✕</button>
            </div>

            <!-- Autocomplete input -->
            <div v-else class="socio-search-wrapper">
              <input
                type="text"
                v-model="socioSearchQuery"
                @input="onSocioSearchInput"
                placeholder="Escriba DNI o nombre del socio (mín. 3 letras)..."
                required
              />
              <div v-if="showSocioSuggestions" class="suggestions-list">
                <div
                  v-for="socio in socioSuggestions"
                  :key="socio.id"
                  @click="selectSocio(socio)"
                  class="suggestion-item"
                >
                  <span class="sug-name">{{ socio.nombreCompleto }}</span>
                  <span class="sug-details">DNI: {{ socio.dni }} - {{ socio.planNombre }}</span>
                </div>
              </div>
            </div>
          </div>

          <div class="form-group">
            <label>Período *</label>
            <div class="period-select-grid">
              <select v-model="formMes" required>
                <option value="" disabled>Mes</option>
                <option v-for="mes in mesesOptions" :key="mes.value" :value="mes.value">
                  {{ mes.label }}
                </option>
              </select>
              <select v-model="formAnio" required>
                <option value="" disabled>Año</option>
                <option v-for="anio in aniosOptions" :key="anio" :value="anio">
                  {{ anio }}
                </option>
              </select>
            </div>
          </div>

          <div class="form-group">
            <label for="monto">Monto ($) *</label>
            <input type="number" id="monto" v-model="formMonto" required min="0.01" step="any" placeholder="Ej. 15000" />
          </div>

          <div class="form-group">
            <label for="fechaVencimiento">Fecha de Vencimiento *</label>
            <input type="date" id="fechaVencimiento" v-model="formFechaVencimiento" required />
          </div>

          <div class="form-group">
            <label for="observacion">Observación</label>
            <textarea id="observacion" v-model="formObservacion" placeholder="Notas sobre esta cuota..." maxlength="255"></textarea>
          </div>

          <div class="form-buttons">
            <button type="submit" class="btn btn-primary">Crear Cuota</button>
            <button type="button" @click="closeCreateModal" class="btn btn-secondary">Cancelar</button>
          </div>
        </form>
      </div>
    </div>

    <!-- MODAL 2: EDITAR OBSERVACION -->
    <div v-if="showEditObsModal" class="modal-backdrop" @click.self="closeEditObsModal">
      <div class="modal-panel">
        <div class="modal-header">
          <h2>Editar Observación</h2>
          <button @click="closeEditObsModal" class="btn-close-modal">✕</button>
        </div>

        <form @submit.prevent="onEditObsSubmit" class="modal-form">
          <div class="form-group">
            <label for="editObs">Observación</label>
            <textarea id="editObs" v-model="editObservacion" placeholder="Detalles de la cuota..." maxlength="255"></textarea>
          </div>

          <div class="form-buttons">
            <button type="submit" class="btn btn-primary">Guardar Observación</button>
            <button type="button" @click="closeEditObsModal" class="btn btn-secondary">Cancelar</button>
          </div>
        </form>
      </div>
    </div>

    <!-- MODAL 3: REGISTRAR PAGO -->
    <div v-if="showPagarModal" class="modal-backdrop" @click.self="closePagarModal">
      <div class="modal-panel">
        <div class="modal-header">
          <h2>Registrar Pago</h2>
          <button @click="closePagarModal" class="btn-close-modal">✕</button>
        </div>

        <form @submit.prevent="onPagarSubmit" class="modal-form">
          <div class="form-group">
            <label for="fechaPago">Fecha de Pago *</label>
            <input type="date" id="fechaPago" v-model="pagarFechaPago" required />
          </div>

          <div class="form-buttons">
            <button type="submit" class="btn btn-primary">Registrar Pago</button>
            <button type="button" @click="closePagarModal" class="btn btn-secondary">Cancelar</button>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>

<style scoped>
.cuotas-container {
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

.select-filter {
  min-width: 180px;
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

.period-tag {
  background-color: var(--code-bg);
  color: var(--text-h);
  border: 1px solid var(--border);
  padding: 3px 8px;
  border-radius: 6px;
  font-size: 13px;
  font-weight: 600;
}

.obs-cell {
  max-width: 180px;
  overflow: hidden;
  text-overflow: ellipsis;
}

.inactive-row {
  opacity: 0.6;
  background-color: rgba(0, 0, 0, 0.02);
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

.badge-pending {
  background-color: rgba(241, 196, 15, 0.15);
  color: #f1c40f;
  border: 1px solid rgba(241, 196, 15, 0.3);
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

/* Modals Overlay & Panel */
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

.relative {
  position: relative;
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
  height: 80px;
  resize: vertical;
}

.form-buttons {
  display: flex;
  gap: 12px;
  margin-top: 25px;
}

/* Selected Socio Box */
.selected-socio-box {
  display: flex;
  justify-content: space-between;
  align-items: center;
  background-color: var(--bg);
  border: 1px solid var(--border);
  border-radius: 8px;
  padding: 10px 14px;
}

.selected-socio-info {
  display: flex;
  flex-direction: column;
}

.socio-name {
  font-weight: 600;
  color: var(--accent);
}

.socio-details {
  font-size: 13px;
  color: var(--text);
}

.btn-clear-socio {
  background: transparent;
  border: none;
  color: #c0392b;
  font-size: 16px;
  cursor: pointer;
}

/* Autocomplete suggestions */
.socio-search-wrapper {
  position: relative;
}

.suggestions-list {
  position: absolute;
  top: 100%;
  left: 0;
  right: 0;
  background-color: var(--bg);
  border: 1px solid var(--border);
  border-top: none;
  border-radius: 0 0 8px 8px;
  z-index: 10;
  max-height: 200px;
  overflow-y: auto;
  box-shadow: var(--shadow);
}

.suggestion-item {
  display: flex;
  flex-direction: column;
  padding: 10px 14px;
  cursor: pointer;
  border-bottom: 1px solid var(--border);
}

.suggestion-item:last-child {
  border-bottom: none;
}

.suggestion-item:hover {
  background-color: var(--code-bg);
}

.sug-name {
  font-weight: 500;
  color: var(--text-h);
}

.sug-details {
  font-size: 12px;
  color: var(--text);
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

.period-select-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 10px;
}
</style>

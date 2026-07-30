<script setup>
import { ref, onMounted, computed, watch } from 'vue';
import cuotaService from '../services/cuotaService';
import socioService from '../services/socioService';
import authService from '../services/authService';
import planService from '../services/planService';

const getAvatarUrl = (ruta) => {
  if (!ruta) return null;
  if (ruta.startsWith('http')) return ruta;
  return `http://localhost:5055${ruta}`;
};

// Auth State
const isAdmin = computed(() => {
  const user = authService.getUsuario();
  return user && (user.rol === 'Administrador' || user.rol === 'Empleado');
});

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

// Plans list and Prorate calculation state
const planes = ref([]);
const registrarPagoInmediato = ref(true);

// Create Cuota Form State
const selectedSocio = ref(null); // { id, dni, nombreCompleto, planNombre }
const socioSearchQuery = ref('');
const socioSuggestions = ref([]);
const showSocioSuggestions = ref(false);

const formFechaInicio = ref(new Date().toISOString().substring(0, 10));
const formFechaVencimiento = ref(new Date().toISOString().substring(0, 10));
const formMonto = ref(0);
const formObservacion = ref('');

const modoCobro = ref('completo'); // 'completo', 'medio_mes', 'personalizado'

const getPrecioBasePlan = () => {
  if (!selectedSocio.value) return 0;
  if (selectedSocio.value.planPrecio && selectedSocio.value.planPrecio > 0) {
    return selectedSocio.value.planPrecio;
  }
  const sPlan = (selectedSocio.value.planNombre || '').trim().toLowerCase();
  const p = planes.value.find(x => 
    (x.nombre && x.nombre.trim().toLowerCase() === sPlan) ||
    (selectedSocio.value.idPlan && x.id === selectedSocio.value.idPlan)
  );
  return p ? p.precioMensual : 0;
};

const getDiasEntreFechas = () => {
  if (!formFechaInicio.value || !formFechaVencimiento.value) return 1;
  const start = new Date(formFechaInicio.value);
  const end = new Date(formFechaVencimiento.value);
  const diffTime = end - start;
  const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24)) + 1;
  return Math.max(1, diffDays);
};

const recalcularMontoPorFechas = () => {
  if (!selectedSocio.value) {
    formMonto.value = 0;
    formObservacion.value = 'Seleccione un socio para calcular el monto según su plan';
    return;
  }
  const precioBase = getPrecioBasePlan();
  const valorDia = precioBase / 30;

  const startDt = new Date(formFechaInicio.value);

  if (modoCobro.value === 'completo') {
    let targetYear = startDt.getFullYear();
    let targetMonth = startDt.getMonth() + 1;
    if (startDt.getDate() > 1) {
      targetMonth += 1;
      if (targetMonth > 12) {
        targetMonth = 1;
        targetYear += 1;
      }
    }
    const lastDayNextMonth = new Date(targetYear, targetMonth, 0).getDate();
    formFechaVencimiento.value = `${targetYear}-${targetMonth.toString().padStart(2, '0')}-${lastDayNextMonth.toString().padStart(2, '0')}`;
  } else if (modoCobro.value === 'medio_mes') {
    let targetYear = startDt.getFullYear();
    let targetMonth = startDt.getMonth() + 1;
    if (startDt.getDate() > 1) {
      targetMonth += 1;
      if (targetMonth > 12) {
        targetMonth = 1;
        targetYear += 1;
      }
    }
    formFechaVencimiento.value = `${targetYear}-${targetMonth.toString().padStart(2, '0')}-15`;
  }

  const dias = getDiasEntreFechas();
  formMonto.value = Math.round(dias * valorDia);

  if (modoCobro.value === 'completo') {
    formObservacion.value = `Mes Completo 100% (${dias} días: ${formatDate(formFechaInicio.value)} al ${formatDate(formFechaVencimiento.value)})`;
  } else if (modoCobro.value === 'medio_mes') {
    formObservacion.value = `Medio Mes 50% (${dias} días: ${formatDate(formFechaInicio.value)} al ${formatDate(formFechaVencimiento.value)})`;
  } else {
    formObservacion.value = `Período personalizado (${dias} días: ${formatDate(formFechaInicio.value)} al ${formatDate(formFechaVencimiento.value)})`;
  }
};

watch([modoCobro, formFechaInicio, formFechaVencimiento, selectedSocio], () => {
  recalcularMontoPorFechas();
});

const selectSocio = (socio) => {
  selectedSocio.value = socio;
  socioSearchQuery.value = '';
  socioSuggestions.value = [];
  showSocioSuggestions.value = false;
  recalcularMontoPorFechas();
};

const clearSelectedSocio = () => {
  selectedSocio.value = null;
  socioSearchQuery.value = '';
  socioSuggestions.value = [];
  showSocioSuggestions.value = false;
  recalcularMontoPorFechas();
};

// AJAX Socio Autocomplete
const onSocioSearchInput = async () => {
  const query = socioSearchQuery.value.trim();
  if (query.length < 1) {
    socioSuggestions.value = [];
    showSocioSuggestions.value = false;
    return;
  }

  try {
    if (query.length >= 3) {
      const result = await socioService.buscar(query, 10);
      const arr = Array.isArray(result) ? result : (result ? [result] : []);
      socioSuggestions.value = arr;
    } else {
      const paged = await socioService.getAll(1, 10, query, 'Activo');
      socioSuggestions.value = (paged.data || []).map(s => ({
        id: s.id,
        dni: s.dni,
        nombreCompleto: `${s.nombre} ${s.apellido}`.trim(),
        estado: s.estado,
        idPlan: s.idPlan,
        planNombre: s.planNombre || 'Sin Plan',
        planPrecio: s.planPrecio || 0
      }));
    }
    showSocioSuggestions.value = socioSuggestions.value.length > 0;
  } catch (err) {
    console.error('Error buscando socios', err);
  }
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
  
  const today = new Date().toISOString().substring(0, 10);
  formFechaInicio.value = today;
  modoCobro.value = 'completo';
  registrarPagoInmediato.value = true;
  formMonto.value = 0;
  formObservacion.value = '';
  showCreateModal.value = true;
  recalcularMontoPorFechas();
};

const closeCreateModal = () => {
  showCreateModal.value = false;
  selectedSocio.value = null;
  socioSearchQuery.value = '';
  socioSuggestions.value = [];
  showSocioSuggestions.value = false;
  modoCobro.value = 'completo';
  registrarPagoInmediato.value = true;
  formMonto.value = 0;
  formObservacion.value = '';
  errors.value = [];
};

const onCreateSubmit = async () => {
  errors.value = [];

  if (!selectedSocio.value) {
    errors.value.push('Debe seleccionar un socio.');
  }

  if (!formFechaInicio.value || !formFechaVencimiento.value) {
    errors.value.push('La fecha de inicio y de vencimiento son obligatorias.');
  }

  if (new Date(formFechaVencimiento.value) < new Date(formFechaInicio.value)) {
    errors.value.push('La fecha de vencimiento debe ser posterior o igual a la fecha de inicio.');
  }

  if (Number(formMonto.value) <= 0) {
    errors.value.push('El monto debe ser mayor que 0.');
  }

  if (errors.value.length > 0) return;

  const startDt = new Date(formFechaInicio.value);
  const combinedPeriod = Number(`${startDt.getFullYear()}${(startDt.getMonth() + 1).toString().padStart(2, '0')}`);

  const payload = {
    idSocio: selectedSocio.value.id,
    periodo: combinedPeriod,
    monto: Number(formMonto.value),
    fechaVencimiento: new Date(formFechaVencimiento.value).toISOString(),
    observacion: formObservacion.value.trim() || `Cobertura (${getDiasEntreFechas()} días: ${formatDate(formFechaInicio.value)} al ${formatDate(formFechaVencimiento.value)})`
  };

  try {
    const created = await cuotaService.create(payload);
    if (registrarPagoInmediato.value && created && created.id) {
      try {
        await cuotaService.pagar(created.id, new Date(formFechaInicio.value).toISOString());
      } catch (e) {
        console.error('Error al registrar pago automático:', e);
      }
    }

    closeCreateModal();
    await fetchCuotas();
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

// Confirm Modal State
const showConfirmModal = ref(false);
const confirmTitle = ref('');
const confirmMessage = ref('');
const confirmCuotaId = ref(null);

const closeConfirmModal = () => {
  showConfirmModal.value = false;
  confirmCuotaId.value = null;
};

const executeConfirmAction = async () => {
  if (!confirmCuotaId.value) return;
  try {
    errors.value = [];
    await cuotaService.anular(confirmCuotaId.value);
    showConfirmModal.value = false;
    confirmCuotaId.value = null;
    await fetchCuotas();
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

  try {
    await cuotaService.pagar(pagarCuotaId.value, new Date(pagarFechaPago.value).toISOString());
    closePagarModal();
    fetchCuotas();
  } catch (err) {
    handleError(err);
  }
};

// Anular action
const triggerAnular = (cuota) => {
  if (!isAdmin.value) return;
  errors.value = [];
  confirmTitle.value = '¿Anular Cuota?';
  confirmMessage.value = `¿Está seguro que desea ANULAR la cuota del período ${formatPeriod(cuota.periodo)} de ${cuota.socioNombreCompleto}? La cuota pasará a estado Anulado y podrá registrar una nueva cuota si lo requiere.`;
  confirmCuotaId.value = cuota.id;
  showConfirmModal.value = true;
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

// Load data
const fetchCuotas = async () => {
  try {
    errors.value = [];
    const result = await cuotaService.getAll(
      currentPage.value,
      pageSize.value,
      search.value,
      statusFilter.value,
      ''
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

onMounted(async () => {
  fetchCuotas();
  try {
    planes.value = await planService.getAll();
  } catch (e) {
    console.error('Error cargando planes para calculadora', e);
  }
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
                <th>Monto</th>
                <th>Fecha Vencimiento</th>
                <th>Fecha Pago</th>
                <th>Estado</th>
                <th>Observación</th>
                <th class="actions-col">Acciones</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="cuota in cuotas" :key="cuota.id" :class="{ 'inactive-row': cuota.estado === 'Anulado' }">
                <td class="user-cell">
                  <div class="table-avatar">
                    <img v-if="cuota.socioAvatar" :src="getAvatarUrl(cuota.socioAvatar)" alt="Avatar" class="avatar-img-sm" />
                    <div v-else class="avatar-initial-sm">
                      {{ (cuota.socioNombreCompleto || 'S').charAt(0).toUpperCase() }}
                    </div>
                  </div>
                  <span class="font-bold">{{ cuota.socioNombreCompleto }}</span>
                </td>
                <td>{{ cuota.socioDni }}</td>
                <td class="font-bold">${{ Number(cuota.monto).toLocaleString('es-AR', { minimumFractionDigits: 2 }) }}</td>
                <td>{{ formatDate(cuota.fechaVencimiento) }}</td>
                <td>{{ formatDate(cuota.fechaPago) }}</td>
                <td class="status-cell">
                  <span :class="['badge', 
                    cuota.estado === 'Pagado' ? 'badge-active' : 
                    cuota.estado === 'Pendiente' ? 'badge-pending' : 'badge-inactive']">
                    {{ cuota.estado }}
                  </span>
                </td>
                <td class="obs-cell" :title="cuota.observacion">
                  <span>{{ cuota.observacion || '-' }}</span>
                </td>
                <td class="actions-cell">
                  <div class="btn-actions-wrapper">
                    <button @click="openEditObsModal(cuota)" class="btn-action btn-action-edit" title="Editar nota">Nota</button>
                    <button
                      v-if="cuota.estado === 'Pendiente'"
                      @click="openPagarModal(cuota)"
                      class="btn-action btn-action-on"
                      title="Registrar pago"
                    >
                      Pagar
                    </button>
                    <button
                      v-if="isAdmin && cuota.estado !== 'Anulado'"
                      @click="triggerAnular(cuota)"
                      class="btn-action btn-action-off"
                      title="Anular cuota"
                    >
                      Anular
                    </button>
                  </div>
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
    <div v-if="showCreateModal" class="modal-backdrop">
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
                <span class="socio-details">DNI: {{ selectedSocio.dni }} - {{ selectedSocio.planNombre }} (${{ Number(getPrecioBasePlan()).toLocaleString('es-AR', { minimumFractionDigits: 2 }) }})</span>
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
                  <span class="sug-details">DNI: {{ socio.dni }} - {{ socio.planNombre }} (${{ Number(socio.planPrecio || 0).toLocaleString('es-AR', { minimumFractionDigits: 2 }) }})</span>
                </div>
              </div>
            </div>
          </div>

          <!-- Modalidad de Cobertura / Días -->
          <div class="form-group">
            <label>Modalidad de Cobertura 🧮</label>
            <div class="modality-options">
              <label class="modality-radio" :class="{ 'active-radio': modoCobro === 'completo' }">
                <input type="radio" name="modoCobro" v-model="modoCobro" value="completo" />
                <span>Mes Completo (100%)</span>
              </label>
              <label class="modality-radio" :class="{ 'active-radio': modoCobro === 'medio_mes' }">
                <input type="radio" name="modoCobro" v-model="modoCobro" value="medio_mes" />
                <span>Medio Mes (50%)</span>
              </label>
              <label class="modality-radio" :class="{ 'active-radio': modoCobro === 'personalizado' }">
                <input type="radio" name="modoCobro" v-model="modoCobro" value="personalizado" />
                <span>Fechas Personalizadas</span>
              </label>
            </div>
          </div>

          <div class="form-group grid-2-cols">
            <div>
              <label for="fechaInicio">Fecha de Inicio *</label>
              <input type="date" id="fechaInicio" v-model="formFechaInicio" required />
            </div>
            <div>
              <label for="fechaVencimiento">Fecha de Vencimiento *</label>
              <input type="date" id="fechaVencimiento" v-model="formFechaVencimiento" required />
            </div>
          </div>

          <!-- Resumen Visual de Total a Pagar -->
          <div class="total-calculator-card">
            <div class="calc-label">💰 TOTAL CALCULADO A COBRAR EN CAJA</div>
            <div class="calc-amount">
              ${{ Number(formMonto || 0).toLocaleString('es-AR', { minimumFractionDigits: 2 }) }}
            </div>
            <div class="calc-desc">
              <div><strong>Cobertura:</strong> {{ getDiasEntreFechas() }} días de entrenamiento (${{ Math.round(getPrecioBasePlan() / 30) }}/día)</div>
              <div><strong>Vigencia:</strong> Desde {{ formatDate(formFechaInicio) }} hasta {{ formatDate(formFechaVencimiento) }}</div>
            </div>
          </div>

          <div class="form-group">
            <label for="monto">Monto Total ($) *</label>
            <input type="number" id="monto" v-model="formMonto" required min="0.01" step="any" placeholder="Ej. 15000" />
          </div>

          <div class="form-group">
            <label for="observacion">Observación</label>
            <textarea id="observacion" v-model="formObservacion" placeholder="Notas sobre esta cuota..." maxlength="255"></textarea>
          </div>

          <div class="form-group checkbox-group">
            <label class="checkbox-label">
              <input type="checkbox" v-model="registrarPagoInmediato" />
              <span>⚡ Marcar como PAGADO INMEDIATAMENTE</span>
            </label>
          </div>

          <div v-if="errors.length > 0" class="modal-error-alert">
            <strong>⚠️ No se pudo completar la operación:</strong>
            <ul>
              <li v-for="(err, idx) in errors" :key="idx">{{ err }}</li>
            </ul>
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

          <div v-if="errors.length > 0" class="modal-error-alert">
            <strong>⚠️ Error:</strong>
            <ul>
              <li v-for="(err, idx) in errors" :key="idx">{{ err }}</li>
            </ul>
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

          <div v-if="errors.length > 0" class="modal-error-alert">
            <strong>⚠️ Error al registrar pago:</strong>
            <ul>
              <li v-for="(err, idx) in errors" :key="idx">{{ err }}</li>
            </ul>
          </div>

          <div class="form-buttons">
            <button type="submit" class="btn btn-primary">Registrar Pago</button>
            <button type="button" @click="closePagarModal" class="btn btn-secondary">Cancelar</button>
          </div>
        </form>
      </div>
    </div>

    <!-- MODAL 4: CONFIRMACIÓN PERSONALIZADA (Sin alertas del navegador) -->
    <div v-if="showConfirmModal" class="modal-backdrop" @click.self="closeConfirmModal">
      <div class="modal-panel confirm-modal-panel">
        <div class="modal-header">
          <h2>{{ confirmTitle }}</h2>
          <button @click="closeConfirmModal" class="btn-close-modal">✕</button>
        </div>
        <div class="confirm-modal-body">
          <p>{{ confirmMessage }}</p>

          <div v-if="errors.length > 0" class="modal-error-alert">
            <strong>⚠️ No se pudo anular:</strong>
            <ul>
              <li v-for="(err, idx) in errors" :key="idx">{{ err }}</li>
            </ul>
          </div>
        </div>
        <div class="form-buttons">
          <button type="button" @click="executeConfirmAction" class="btn btn-status-inactive">Confirmar</button>
          <button type="button" @click="closeConfirmModal" class="btn btn-secondary">Cancelar</button>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.cuotas-container {
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

.user-cell {
  display: flex;
  align-items: center;
  gap: 10px;
}

.table-avatar {
  width: 34px;
  height: 34px;
  border-radius: 50%;
  overflow: hidden;
  display: flex;
  align-items: center;
  justify-content: center;
  background-color: var(--accent-bg);
  border: 1px solid var(--border);
  flex-shrink: 0;
}

.avatar-img-sm {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.avatar-initial-sm {
  font-size: 13px;
  font-weight: 700;
  color: var(--accent);
  text-transform: uppercase;
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
  font-size: 12px;
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

.actions-col, .actions-cell {
  width: 165px;
  min-width: 165px;
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

/* Modality & Prorate Styles */
.modality-options {
  display: flex;
  flex-direction: column;
  gap: 8px;
  background-color: var(--bg);
  padding: 10px 14px;
  border-radius: 8px;
  border: 1px solid var(--border);
}

.modality-radio {
  display: flex;
  align-items: center;
  gap: 8px;
  cursor: pointer;
  font-size: 13.5px;
  font-weight: 500;
  color: var(--text-h);
  transition: all 0.15s ease;
}

.modality-radio.active-radio {
  color: #10b981;
}

.auto-days-card {
  background-color: var(--bg);
  border: 1px solid var(--border);
  padding: 10px 14px;
  border-radius: 8px;
  margin-top: 4px;
  text-align: left;
}

.auto-days-val {
  font-size: 14.5px;
  font-weight: 700;
  color: #10b981;
}

.auto-days-desc {
  font-size: 12px;
  color: var(--text);
  margin-top: 2px;
}

.addon-period-box {
  background-color: rgba(59, 130, 246, 0.08);
  border: 1px dashed rgba(59, 130, 246, 0.35);
  padding: 12px 14px;
  border-radius: 8px;
  margin-top: 10px;
  margin-bottom: 15px;
}

.highlight-checkbox span {
  color: #3b82f6;
  font-weight: 600;
}

.addon-details {
  margin-top: 10px;
  padding-top: 10px;
  border-top: 1px dashed rgba(59, 130, 246, 0.25);
}

.addon-title {
  display: block;
  font-size: 13px;
  font-weight: 600;
  color: var(--text-h);
  margin-bottom: 6px;
}

.addon-desc-line {
  margin-top: 4px;
  color: #10b981;
  font-weight: 600;
}

.modality-radio input[type="radio"] {
  width: auto;
  accent-color: var(--accent);
}

.prorate-box {
  background-color: rgba(99, 102, 241, 0.08);
  border: 1px dashed var(--accent);
  padding: 12px;
  border-radius: 8px;
}

.prorate-hint {
  display: block;
  font-size: 12px;
  color: var(--accent);
  margin-top: 6px;
  font-weight: 500;
}

.total-calculator-card {
  background: linear-gradient(135deg, rgba(99, 102, 241, 0.15), rgba(79, 70, 229, 0.25));
  border: 1.5px solid var(--accent);
  border-radius: 10px;
  padding: 16px;
  margin: 16px 0;
  text-align: center;
  box-shadow: inset 0 0 10px rgba(99, 102, 241, 0.1);
}

.calc-label {
  font-size: 11px;
  font-weight: 700;
  letter-spacing: 0.5px;
  color: var(--accent);
  margin-bottom: 4px;
  text-transform: uppercase;
}

.calc-amount {
  font-size: 28px;
  font-weight: 800;
  color: var(--text-h);
  line-height: 1.2;
}

.calc-desc {
  font-size: 12px;
  color: var(--text);
  margin-top: 4px;
  font-style: italic;
}

.checkbox-group {
  margin-top: 10px;
}

.checkbox-label {
  display: flex;
  align-items: center;
  gap: 10px;
  cursor: pointer;
  font-size: 14px;
  font-weight: 600;
  color: var(--text-h);
}

.checkbox-label input[type="checkbox"] {
  width: 18px;
  height: 18px;
  accent-color: #10b981;
  cursor: pointer;
}

.modal-error-alert {
  background-color: rgba(239, 68, 68, 0.12);
  border: 1px solid rgba(239, 68, 68, 0.3);
  color: #ef4444;
  padding: 12px;
  border-radius: 8px;
  margin-top: 15px;
  margin-bottom: 15px;
  font-size: 13.5px;
  text-align: left;
}

.modal-error-alert ul {
  margin: 6px 0 0 18px;
  padding: 0;
}

.confirm-modal-panel {
  max-width: 440px;
}

.confirm-modal-body p {
  margin: 0 0 20px 0;
  font-size: 15px;
  line-height: 1.5;
  color: var(--text-h);
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

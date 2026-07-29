<script setup>
import { ref, onMounted, computed, watch } from 'vue';
import api from '../services/api';
import authService from '../services/authService';

const props = defineProps({
  idSocio: {
    type: [Number, String],
    required: true
  },
  canEdit: {
    type: Boolean,
    default: true
  }
});

const currentUser = computed(() => authService.getUsuario() || {});
const userRole = computed(() => currentUser.value.rol || currentUser.value.role || '');
const isAlumno = computed(() => userRole.value === 'Alumno');
const isCoachOrAdmin = computed(() => ['Administrador', 'Coach'].includes(userRole.value));
const canUpload = computed(() => props.canEdit || isAlumno.value);
const canDelete = computed(() => isCoachOrAdmin.value); // Alumnos cannot delete evolutions

const progresos = ref([]);
const isLoading = ref(false);
const errorMsg = ref('');
const hoveredPoint = ref(null);

// Selection for comparison
const selectedIds = ref([]);
const showCompareModal = ref(false);
const activeAngleTab = ref('frente'); // 'frente', 'perfil', 'espalda'

// Modal upload form
const showUploadModal = ref(false);
const isSubmitting = ref(false);
const uploadErrors = ref([]);

const formFecha = ref(new Date().toISOString().substring(0, 10));
const formPesoKg = ref('');
const formObservaciones = ref('');

const fileFrente = ref(null);
const filePerfil = ref(null);
const fileEspalda = ref(null);

const previewFrente = ref(null);
const previewPerfil = ref(null);
const previewEspalda = ref(null);

const getImageUrl = (path) => {
  if (!path) return null;
  if (path.startsWith('http')) return path;
  return `http://localhost:5055${path}`;
};

const formatDate = (dateStr) => {
  if (!dateStr) return '-';
  const d = new Date(dateStr);
  return d.toLocaleDateString('es-AR', { day: '2-digit', month: '2-digit', year: 'numeric' });
};

const loadProgresos = async () => {
  if (!props.idSocio) return;
  isLoading.value = true;
  errorMsg.value = '';
  try {
    const res = await api.get(`/progreso/socio/${props.idSocio}`);
    progresos.value = res.data;
  } catch (err) {
    errorMsg.value = 'No se pudo cargar el historial de evolución corporal.';
  } finally {
    isLoading.value = false;
  }
};

watch(() => props.idSocio, () => {
  selectedIds.value = [];
  loadProgresos();
});

// Graph Computations
const weightHistory = computed(() => {
  return progresos.value
    .filter(p => p.pesoKg != null && p.pesoKg > 0)
    .sort((a, b) => new Date(a.fecha) - new Date(b.fecha));
});

const chartMetrics = computed(() => {
  const list = weightHistory.value;
  if (list.length === 0) return null;

  const inicial = list[0].pesoKg;
  const actual = list[list.length - 1].pesoKg;
  const diffNum = Number((actual - inicial).toFixed(1));
  const diffStr = diffNum > 0 ? `+${diffNum}` : `${diffNum}`;

  const weights = list.map(p => p.pesoKg);
  const min = Math.min(...weights);
  const max = Math.max(...weights);

  return { inicial, actual, diffNum, diffStr, min, max, count: list.length };
});

const chartPoints = computed(() => {
  const list = weightHistory.value;
  if (list.length === 0) return [];

  const svgWidth = 540;
  const svgHeight = 160;
  const padX = 45;
  const padY = 25;

  const weights = list.map(p => p.pesoKg);
  let min = Math.min(...weights);
  let max = Math.max(...weights);
  if (min === max) {
    min -= 2;
    max += 2;
  } else {
    const margin = (max - min) * 0.15;
    min -= margin;
    max += margin;
  }

  const usableW = svgWidth - padX * 2;
  const usableH = svgHeight - padY * 2;

  return list.map((item, index) => {
    const x = list.length === 1 ? padX + usableW / 2 : padX + (index / (list.length - 1)) * usableW;
    const y = svgHeight - padY - ((item.pesoKg - min) / (max - min)) * usableH;
    return {
      x,
      y,
      peso: item.pesoKg,
      fecha: formatDate(item.fecha),
      id: item.id
    };
  });
});

const chartPath = computed(() => {
  const pts = chartPoints.value;
  if (pts.length < 2) return '';
  return pts.reduce((acc, pt, i) => `${acc} ${i === 0 ? 'M' : 'L'} ${pt.x} ${pt.y}`, '');
});

const chartAreaPath = computed(() => {
  const pts = chartPoints.value;
  if (pts.length < 2) return '';
  const first = pts[0];
  const last = pts[pts.length - 1];
  const line = pts.reduce((acc, pt, i) => `${acc} ${i === 0 ? 'M' : 'L'} ${pt.x} ${pt.y}`, '');
  return `${line} L ${last.x} 145 L ${first.x} 145 Z`;
});

const toggleSelect = (id) => {
  const index = selectedIds.value.indexOf(id);
  if (index > -1) {
    selectedIds.value.splice(index, 1);
  } else {
    if (selectedIds.value.length >= 2) {
      selectedIds.value.shift();
    }
    selectedIds.value.push(id);
  }
};

const selectedSet1 = computed(() => {
  if (selectedIds.value.length < 2) return null;
  const p1 = progresos.value.find(p => p.id === selectedIds.value[0]);
  const p2 = progresos.value.find(p => p.id === selectedIds.value[1]);
  if (!p1 || !p2) return null;
  return new Date(p1.fecha) <= new Date(p2.fecha) ? p1 : p2;
});

const selectedSet2 = computed(() => {
  if (selectedIds.value.length < 2) return null;
  const p1 = progresos.value.find(p => p.id === selectedIds.value[0]);
  const p2 = progresos.value.find(p => p.id === selectedIds.value[1]);
  if (!p1 || !p2) return null;
  return new Date(p1.fecha) <= new Date(p2.fecha) ? p2 : p1;
});

const weightDifference = computed(() => {
  if (!selectedSet1.value || !selectedSet2.value) return null;
  if (selectedSet1.value.pesoKg == null || selectedSet2.value.pesoKg == null) return null;
  const diff = selectedSet2.value.pesoKg - selectedSet1.value.pesoKg;
  return diff.toFixed(1);
});

const openUploadModal = () => {
  formFecha.value = new Date().toISOString().substring(0, 10);
  formPesoKg.value = '';
  formObservaciones.value = '';
  fileFrente.value = null;
  filePerfil.value = null;
  fileEspalda.value = null;
  previewFrente.value = null;
  previewPerfil.value = null;
  previewEspalda.value = null;
  uploadErrors.value = [];
  showUploadModal.value = true;
};

const onFileChange = (e, type) => {
  const file = e.target.files[0];
  if (!file) return;

  const reader = new FileReader();
  reader.onload = (evt) => {
    if (type === 'frente') {
      fileFrente.value = file;
      previewFrente.value = evt.target.result;
    } else if (type === 'perfil') {
      filePerfil.value = file;
      previewPerfil.value = evt.target.result;
    } else if (type === 'espalda') {
      fileEspalda.value = file;
      previewEspalda.value = evt.target.result;
    }
  };
  reader.readAsDataURL(file);
};

const submitProgreso = async () => {
  uploadErrors.value = [];
  if (!formFecha.value) {
    uploadErrors.value.push('La fecha del registro es obligatoria.');
    return;
  }
  if (!fileFrente.value && !filePerfil.value && !fileEspalda.value && !formPesoKg.value) {
    uploadErrors.value.push('Debes subir al menos 1 foto de evolución o registrar el peso en kg.');
    return;
  }

  isSubmitting.value = true;
  try {
    const formData = new FormData();
    formData.append('IdSocio', props.idSocio);
    formData.append('Fecha', formFecha.value);
    if (formPesoKg.value) formData.append('PesoKg', formPesoKg.value);
    if (formObservaciones.value) formData.append('Observaciones', formObservaciones.value);

    if (fileFrente.value) formData.append('FotoFrente', fileFrente.value);
    if (filePerfil.value) formData.append('FotoPerfil', filePerfil.value);
    if (fileEspalda.value) formData.append('FotoEspalda', fileEspalda.value);

    await api.post('/progreso', formData, {
      headers: { 'Content-Type': 'multipart/form-data' }
    });

    showUploadModal.value = false;
    loadProgresos();
  } catch (err) {
    if (err.response?.data?.errors) {
      uploadErrors.value = err.response.data.errors;
    } else {
      uploadErrors.value = ['Ocurrió un error al subir el registro de progreso.'];
    }
  } finally {
    isSubmitting.value = false;
  }
};

const deleteProgreso = async (progreso) => {
  if (!confirm(`¿Estás seguro de eliminar el registro de evolución del ${formatDate(progreso.fecha)}?`)) return;
  try {
    await api.delete(`/progreso/${progreso.id}`);
    selectedIds.value = selectedIds.value.filter(id => id !== progreso.id);
    loadProgresos();
  } catch (err) {
    alert('No se pudo eliminar el registro.');
  }
};

onMounted(() => {
  loadProgresos();
});
</script>

<template>
  <div class="progreso-module">
    <!-- Header Módulo -->
    <div class="module-header">
      <div>
        <h3 class="module-title">Evolución Corporal & Control de Peso</h3>
        <p class="module-subtitle">Gráfico de variación de peso y seguimiento de fotos de progreso</p>
      </div>

      <div class="header-actions">
        <button
          type="button"
          class="btn-compare"
          :disabled="selectedIds.length !== 2"
          @click="showCompareModal = true"
        >
          Comparar Fotos ({{ selectedIds.length }}/2 seleccionados)
        </button>

        <button
          v-if="canUpload"
          type="button"
          class="btn-primary"
          @click="openUploadModal"
        >
          + Registrar Peso / Evolución
        </button>
      </div>
    </div>

    <!-- Carga / Errores -->
    <div v-if="isLoading" class="state-msg">Cargando datos de evolución corporal...</div>
    <div v-else-if="errorMsg" class="alert alert-danger">{{ errorMsg }}</div>

    <!-- Sección de Gráfico de Peso -->
    <div v-if="chartMetrics" class="weight-chart-card">
      <div class="chart-header">
        <div class="chart-title-area">
          <h4>Gráfico de Evolución del Peso Corporal</h4>
          <span class="history-count">{{ chartMetrics.count }} registros guardados</span>
        </div>

        <div class="chart-stats-summary">
          <div class="kpi-box">
            <span class="kpi-label">Peso Inicial</span>
            <span class="kpi-val">{{ chartMetrics.inicial }} kg</span>
          </div>
          <div class="kpi-box">
            <span class="kpi-label">Peso Actual</span>
            <span class="kpi-val font-bold">{{ chartMetrics.actual }} kg</span>
          </div>
          <div class="kpi-box">
            <span class="kpi-label">Variación Total</span>
            <span :class="['kpi-val', chartMetrics.diffNum <= 0 ? 'text-green' : 'text-amber']">
              {{ chartMetrics.diffStr }} kg
            </span>
          </div>
        </div>
      </div>

      <!-- SVG Line Chart -->
      <div class="svg-chart-wrapper">
        <svg viewBox="0 0 540 160" class="weight-svg">
          <defs>
            <linearGradient id="weightGrad" x1="0" y1="0" x2="0" y2="1">
              <stop offset="0%" stop-color="var(--accent)" stop-opacity="0.3" />
              <stop offset="100%" stop-color="var(--accent)" stop-opacity="0.0" />
            </linearGradient>
          </defs>

          <!-- Grid Lines -->
          <line x1="45" y1="25" x2="495" y2="25" stroke="var(--border)" stroke-dasharray="3,3" opacity="0.5" />
          <line x1="45" y1="85" x2="495" y2="85" stroke="var(--border)" stroke-dasharray="3,3" opacity="0.5" />
          <line x1="45" y1="145" x2="495" y2="145" stroke="var(--border)" stroke-width="1" />

          <!-- Fill Area under Line -->
          <path v-if="chartPoints.length >= 2" :d="chartAreaPath" fill="url(#weightGrad)" />

          <!-- Line Path -->
          <path v-if="chartPoints.length >= 2" :d="chartPath" fill="none" stroke="var(--accent)" stroke-width="3" stroke-linecap="round" stroke-linejoin="round" />

          <!-- Points & Tooltip Targets -->
          <g v-for="pt in chartPoints" :key="pt.id" class="chart-point-group" @mouseenter="hoveredPoint = pt" @mouseleave="hoveredPoint = null">
            <circle :cx="pt.x" :cy="pt.y" r="6" class="point-circle" />
            <circle :cx="pt.x" :cy="pt.y" r="14" fill="transparent" class="point-hitbox" />

            <!-- Point Label Text -->
            <text :x="pt.x" :y="pt.y - 12" text-anchor="middle" class="point-text">{{ pt.peso }}kg</text>
          </g>

          <!-- Interactive Hover Tooltip -->
          <g v-if="hoveredPoint" class="tooltip-g">
            <rect :x="hoveredPoint.x - 55" :y="hoveredPoint.y - 45" width="110" height="28" rx="6" fill="var(--code-bg)" stroke="var(--accent)" stroke-width="1" />
            <text :x="hoveredPoint.x" :y="hoveredPoint.y - 27" text-anchor="middle" class="tooltip-text">
              {{ hoveredPoint.fecha }}: {{ hoveredPoint.peso }}kg
            </text>
          </g>
        </svg>
      </div>
    </div>

    <div v-else-if="progresos.length === 0" class="empty-card">
      <p class="empty-title">Sin registros de evolución aún</p>
      <p class="empty-desc">Ingresa el peso corporal y sube fotos de frente, perfil o espalda para visualizar el progreso.</p>
    </div>

    <!-- Lista / Galería de Registros -->
    <div v-if="progresos.length > 0" class="cards-grid">
      <div
        v-for="item in progresos"
        :key="item.id"
        class="progreso-card"
        :class="{ 'selected-card': selectedIds.includes(item.id) }"
      >
        <div class="card-header">
          <label class="checkbox-container">
            <input
              type="checkbox"
              :checked="selectedIds.includes(item.id)"
              @change="toggleSelect(item.id)"
            />
            <span class="checkmark"></span>
            <span class="fecha-label">{{ formatDate(item.fecha) }}</span>
          </label>

          <span v-if="item.pesoKg" class="peso-badge">{{ item.pesoKg }} kg</span>
        </div>

        <!-- Miniaturas de fotos -->
        <div class="photos-strip">
          <div class="photo-thumb" v-if="item.rutaFotoFrente">
            <img :src="getImageUrl(item.rutaFotoFrente)" alt="Frente" />
            <span class="photo-tag">Frente</span>
          </div>
          <div class="photo-thumb" v-if="item.rutaFotoPerfil">
            <img :src="getImageUrl(item.rutaFotoPerfil)" alt="Perfil" />
            <span class="photo-tag">Perfil</span>
          </div>
          <div class="photo-thumb" v-if="item.rutaFotoEspalda">
            <img :src="getImageUrl(item.rutaFotoEspalda)" alt="Espalda" />
            <span class="photo-tag">Espalda</span>
          </div>
          <div class="no-photo-box" v-if="!item.rutaFotoFrente && !item.rutaFotoPerfil && !item.rutaFotoEspalda">
            <span>Sin fotos cargadas</span>
          </div>
        </div>

        <p v-if="item.observaciones" class="obs-text">"{{ item.observaciones }}"</p>

        <div class="card-footer" v-if="canDelete">
          <button type="button" class="btn-delete" @click="deleteProgreso(item)">Eliminar</button>
        </div>
      </div>
    </div>

    <!-- Modal Nuevo Registro -->
    <div v-if="showUploadModal" class="modal-backdrop" @click.self="showUploadModal = false">
      <div class="modal-card">
        <h3>Nuevo Registro de Evolución</h3>

        <div v-if="uploadErrors.length > 0" class="alert alert-error">
          <ul>
            <li v-for="(err, idx) in uploadErrors" :key="idx">{{ err }}</li>
          </ul>
        </div>

        <form @submit.prevent="submitProgreso">
          <div class="form-row">
            <div class="form-group flex-1">
              <label>Fecha de Registro *</label>
              <input type="date" v-model="formFecha" required />
            </div>
            <div class="form-group flex-1">
              <label>Peso Corporal (kg) *</label>
              <input type="number" step="0.1" v-model="formPesoKg" placeholder="Ej. 78.5" />
            </div>
          </div>

          <div class="form-group">
            <label>Observaciones / Notas de Progreso</label>
            <input type="text" v-model="formObservaciones" placeholder="Ej. Definición abdominal evidente, mejor postura." />
          </div>

          <div class="photos-upload-grid">
            <!-- Frente -->
            <div class="upload-box">
              <span class="upload-title">Foto Frente</span>
              <div class="preview-area" @click="$refs.inputFrente.click()">
                <img v-if="previewFrente" :src="previewFrente" alt="Preview Frente" />
                <span v-else class="upload-placeholder">+ Subir Foto</span>
              </div>
              <input type="file" ref="inputFrente" accept="image/*" class="hidden-input" @change="e => onFileChange(e, 'frente')" />
            </div>

            <!-- Perfil -->
            <div class="upload-box">
              <span class="upload-title">Foto Perfil</span>
              <div class="preview-area" @click="$refs.inputPerfil.click()">
                <img v-if="previewPerfil" :src="previewPerfil" alt="Preview Perfil" />
                <span v-else class="upload-placeholder">+ Subir Foto</span>
              </div>
              <input type="file" ref="inputPerfil" accept="image/*" class="hidden-input" @change="e => onFileChange(e, 'perfil')" />
            </div>

            <!-- Espalda -->
            <div class="upload-box">
              <span class="upload-title">Foto Espalda</span>
              <div class="preview-area" @click="$refs.inputEspalda.click()">
                <img v-if="previewEspalda" :src="previewEspalda" alt="Preview Espalda" />
                <span v-else class="upload-placeholder">+ Subir Foto</span>
              </div>
              <input type="file" ref="inputEspalda" accept="image/*" class="hidden-input" @change="e => onFileChange(e, 'espalda')" />
            </div>
          </div>

          <div class="modal-actions">
            <button type="button" class="btn-secondary" @click="showUploadModal = false">Cancelar</button>
            <button type="submit" class="btn-primary" :disabled="isSubmitting">
              {{ isSubmitting ? 'Guardando...' : 'Guardar Evolución' }}
            </button>
          </div>
        </form>
      </div>
    </div>

    <!-- Modal Comparativa Side-by-Side -->
    <div v-if="showCompareModal && selectedSet1 && selectedSet2" class="modal-backdrop" @click.self="showCompareModal = false">
      <div class="modal-card modal-compare">
        <div class="compare-header">
          <h3>Comparativa de Evolución Corporal</h3>
          <button type="button" class="btn-close" @click="showCompareModal = false">✕</button>
        </div>

        <div class="diff-summary" v-if="weightDifference !== null">
          <span class="diff-label">Diferencia de Peso:</span>
          <span :class="['diff-val', Number(weightDifference) <= 0 ? 'text-green' : 'text-amber']">
            {{ weightDifference > 0 ? `+${weightDifference}` : weightDifference }} kg
          </span>
        </div>

        <!-- Pestañas de Ángulo -->
        <div class="angle-tabs">
          <button
            type="button"
            class="tab-btn"
            :class="{ active: activeAngleTab === 'frente' }"
            @click="activeAngleTab = 'frente'"
          >
            Frente
          </button>
          <button
            type="button"
            class="tab-btn"
            :class="{ active: activeAngleTab === 'perfil' }"
            @click="activeAngleTab = 'perfil'"
          >
            Perfil
          </button>
          <button
            type="button"
            class="tab-btn"
            :class="{ active: activeAngleTab === 'espalda' }"
            @click="activeAngleTab = 'espalda'"
          >
            Espalda
          </button>
        </div>

        <!-- Side-by-Side Comparison -->
        <div class="side-by-side-grid">
          <!-- Set 1 (Antes / Anterior) -->
          <div class="compare-column">
            <div class="column-header">
              <span class="set-tag">ANTERIOR</span>
              <span class="set-date">{{ formatDate(selectedSet1.fecha) }}</span>
              <span class="set-peso" v-if="selectedSet1.pesoKg">{{ selectedSet1.pesoKg }} kg</span>
            </div>

            <div class="compare-photo-container">
              <template v-if="activeAngleTab === 'frente'">
                <img v-if="selectedSet1.rutaFotoFrente" :src="getImageUrl(selectedSet1.rutaFotoFrente)" alt="Antes Frente" />
                <div v-else class="no-photo-cmp">Sin foto de frente</div>
              </template>
              <template v-else-if="activeAngleTab === 'perfil'">
                <img v-if="selectedSet1.rutaFotoPerfil" :src="getImageUrl(selectedSet1.rutaFotoPerfil)" alt="Antes Perfil" />
                <div v-else class="no-photo-cmp">Sin foto de perfil</div>
              </template>
              <template v-else>
                <img v-if="selectedSet1.rutaFotoEspalda" :src="getImageUrl(selectedSet1.rutaFotoEspalda)" alt="Antes Espalda" />
                <div v-else class="no-photo-cmp">Sin foto de espalda</div>
              </template>
            </div>
          </div>

          <!-- Set 2 (Después / Reciente) -->
          <div class="compare-column">
            <div class="column-header">
              <span class="set-tag tag-recent">ACTUAL</span>
              <span class="set-date">{{ formatDate(selectedSet2.fecha) }}</span>
              <span class="set-peso" v-if="selectedSet2.pesoKg">{{ selectedSet2.pesoKg }} kg</span>
            </div>

            <div class="compare-photo-container">
              <template v-if="activeAngleTab === 'frente'">
                <img v-if="selectedSet2.rutaFotoFrente" :src="getImageUrl(selectedSet2.rutaFotoFrente)" alt="Después Frente" />
                <div v-else class="no-photo-cmp">Sin foto de frente</div>
              </template>
              <template v-else-if="activeAngleTab === 'perfil'">
                <img v-if="selectedSet2.rutaFotoPerfil" :src="getImageUrl(selectedSet2.rutaFotoPerfil)" alt="Después Perfil" />
                <div v-else class="no-photo-cmp">Sin foto de perfil</div>
              </template>
              <template v-else>
                <img v-if="selectedSet2.rutaFotoEspalda" :src="getImageUrl(selectedSet2.rutaFotoEspalda)" alt="Después Espalda" />
                <div v-else class="no-photo-cmp">Sin foto de espalda</div>
              </template>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.progreso-module {
  margin-top: 24px;
}

.module-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 18px;
  gap: 16px;
  flex-wrap: wrap;
}

.module-title {
  margin: 0;
  font-size: 16px;
  font-weight: 600;
  color: var(--text-h);
}

.module-subtitle {
  margin: 2px 0 0 0;
  font-size: 12px;
  color: var(--text);
  opacity: 0.75;
}

.header-actions {
  display: flex;
  gap: 10px;
}

.btn-primary {
  background-color: var(--accent);
  color: #fff;
  border: none;
  padding: 8px 14px;
  border-radius: 8px;
  font-size: 12px;
  font-weight: 600;
  cursor: pointer;
}

.btn-compare {
  background-color: var(--code-bg);
  border: 1px solid var(--border);
  color: var(--text-h);
  padding: 8px 14px;
  border-radius: 8px;
  font-size: 12px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.15s ease;
}

.btn-compare:hover:not(:disabled) {
  border-color: var(--accent);
  color: var(--accent);
}

.btn-compare:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

/* Weight Chart Card */
.weight-chart-card {
  background-color: var(--bg);
  border: 1px solid var(--border);
  border-radius: 10px;
  padding: 20px;
  margin-bottom: 20px;
  box-shadow: var(--shadow);
}

.chart-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 16px;
  flex-wrap: wrap;
  gap: 12px;
}

.chart-title-area h4 {
  margin: 0;
  font-size: 14px;
  font-weight: 600;
  color: var(--text-h);
}

.history-count {
  font-size: 11px;
  color: var(--text);
  opacity: 0.75;
}

.chart-stats-summary {
  display: flex;
  gap: 16px;
}

.kpi-box {
  display: flex;
  flex-direction: column;
  align-items: flex-end;
}

.kpi-label {
  font-size: 10px;
  color: var(--text);
  text-transform: uppercase;
  letter-spacing: 0.4px;
}

.kpi-val {
  font-size: 14px;
  font-weight: 600;
  color: var(--text-h);
}

.svg-chart-wrapper {
  width: 100%;
  overflow-x: auto;
}

.weight-svg {
  width: 100%;
  height: 160px;
  display: block;
}

.point-circle {
  fill: var(--accent);
  stroke: var(--bg);
  stroke-width: 2px;
  transition: transform 0.15s ease;
}

.chart-point-group:hover .point-circle {
  r: 8px;
  fill: #fff;
  stroke: var(--accent);
}

.point-hitbox {
  cursor: pointer;
}

.point-text {
  font-size: 10px;
  fill: var(--text-h);
  font-weight: 600;
}

.tooltip-text {
  font-size: 11px;
  fill: var(--text-h);
  font-weight: 600;
}

.text-green { color: #10b981; }
.text-amber { color: #f59e0b; }

.cards-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
  gap: 16px;
}

.progreso-card {
  background-color: var(--bg);
  border: 1px solid var(--border);
  border-radius: 10px;
  padding: 14px;
  display: flex;
  flex-direction: column;
  gap: 12px;
  transition: border-color 0.2s ease;
}

.selected-card {
  border-color: var(--accent);
  background-color: var(--accent-bg);
}

.card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.checkbox-container {
  display: flex;
  align-items: center;
  gap: 8px;
  cursor: pointer;
  font-size: 13px;
  font-weight: 600;
  color: var(--text-h);
}

.peso-badge {
  background-color: var(--code-bg);
  border: 1px solid var(--border);
  padding: 3px 8px;
  border-radius: 10px;
  font-size: 12px;
  font-weight: 600;
}

.photos-strip {
  display: flex;
  gap: 8px;
}

.photo-thumb {
  flex: 1;
  position: relative;
  height: 120px;
  border-radius: 6px;
  overflow: hidden;
  border: 1px solid var(--border);
}

.photo-thumb img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.photo-tag {
  position: absolute;
  bottom: 4px;
  left: 4px;
  background-color: rgba(0, 0, 0, 0.65);
  color: #fff;
  font-size: 10px;
  padding: 2px 5px;
  border-radius: 4px;
}

.no-photo-box {
  width: 100%;
  height: 80px;
  background-color: var(--code-bg);
  border: 1px dashed var(--border);
  border-radius: 6px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 12px;
  color: var(--text);
  opacity: 0.6;
}

.obs-text {
  font-size: 12px;
  font-style: italic;
  color: var(--text);
  margin: 0;
}

.card-footer {
  display: flex;
  justify-content: flex-end;
}

.btn-delete {
  background: transparent;
  border: none;
  color: #ef4444;
  font-size: 11px;
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
  max-width: 480px;
  box-shadow: 0 12px 32px rgba(0, 0, 0, 0.15);
}

.modal-card h3 {
  margin-top: 0;
  margin-bottom: 16px;
  font-size: 16px;
  font-weight: 600;
  color: var(--text-h);
}

.form-row {
  display: flex;
  gap: 12px;
}

.flex-1 { flex: 1; }

.form-group {
  margin-bottom: 14px;
}

.form-group label {
  display: block;
  font-size: 12px;
  font-weight: 600;
  margin-bottom: 5px;
}

.form-group input {
  width: 100%;
  padding: 8px 10px;
  font-size: 13px;
  border: 1px solid var(--border);
  border-radius: 8px;
  background-color: var(--code-bg);
  color: inherit;
  box-sizing: border-box;
}

.photos-upload-grid {
  display: flex;
  gap: 10px;
  margin: 16px 0;
}

.upload-box {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 6px;
  text-align: center;
}

.upload-title {
  font-size: 11px;
  font-weight: 600;
  color: var(--text-h);
}

.preview-area {
  height: 100px;
  border: 1px dashed var(--border);
  border-radius: 8px;
  background-color: var(--code-bg);
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  overflow: hidden;
}

.preview-area img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.upload-placeholder {
  font-size: 11px;
  color: var(--accent);
  font-weight: 600;
}

.hidden-input {
  display: none;
}

.modal-actions {
  display: flex;
  justify-content: flex-end;
  gap: 8px;
  margin-top: 20px;
}

.btn-secondary {
  background-color: transparent;
  border: 1px solid var(--border);
  color: var(--text);
  padding: 8px 14px;
  border-radius: 8px;
  font-size: 12px;
  font-weight: 600;
  cursor: pointer;
}

/* Modal Compare */
.modal-compare {
  max-width: 680px;
}

.compare-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 12px;
}

.btn-close {
  background: transparent;
  border: none;
  font-size: 18px;
  cursor: pointer;
  color: var(--text);
}

.diff-summary {
  background-color: var(--code-bg);
  padding: 8px 14px;
  border-radius: 8px;
  border: 1px solid var(--border);
  display: flex;
  gap: 8px;
  align-items: center;
  margin-bottom: 16px;
  font-size: 13px;
}

.diff-label { color: var(--text); }
.diff-val { font-weight: 700; }

.angle-tabs {
  display: flex;
  gap: 8px;
  margin-bottom: 16px;
}

.tab-btn {
  flex: 1;
  padding: 8px;
  font-size: 12px;
  font-weight: 600;
  border: 1px solid var(--border);
  background-color: var(--code-bg);
  color: var(--text);
  border-radius: 6px;
  cursor: pointer;
}

.tab-btn.active {
  border-color: var(--accent);
  background-color: var(--accent-bg);
  color: var(--accent);
}

.side-by-side-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 16px;
}

.compare-column {
  border: 1px solid var(--border);
  border-radius: 8px;
  padding: 12px;
  background-color: var(--bg);
}

.column-header {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 4px;
  margin-bottom: 10px;
}

.set-tag {
  font-size: 10px;
  font-weight: 700;
  padding: 2px 6px;
  border-radius: 4px;
  background-color: rgba(156, 163, 175, 0.15);
  color: #6b7280;
}

.tag-recent {
  background-color: var(--accent-bg);
  color: var(--accent);
}

.set-date { font-weight: 600; font-size: 13px; color: var(--text-h); }
.set-peso { font-size: 12px; color: var(--text); opacity: 0.8; }

.compare-photo-container {
  height: 280px;
  border-radius: 6px;
  overflow: hidden;
  background-color: var(--code-bg);
  display: flex;
  align-items: center;
  justify-content: center;
}

.compare-photo-container img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.no-photo-cmp {
  font-size: 12px;
  color: var(--text);
  opacity: 0.6;
}

.empty-card, .state-msg {
  background-color: var(--bg);
  border: 1px solid var(--border);
  border-radius: 10px;
  padding: 28px;
  text-align: center;
}
.empty-title { font-weight: 600; color: var(--text-h); margin-bottom: 4px; }
.empty-desc { font-size: 12px; color: var(--text); opacity: 0.75; }

.alert-error {
  background-color: rgba(239, 68, 68, 0.12);
  border: 1px solid rgba(239, 68, 68, 0.3);
  color: #ef4444;
  padding: 10px 14px;
  border-radius: 8px;
  font-size: 13px;
  margin-bottom: 16px;
}
.alert-error ul { margin: 0; padding-left: 18px; }
</style>

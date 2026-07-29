<script setup>
import { ref, computed, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import authService from '../services/authService';
import api from '../services/api';
import FotosProgresoModule from '../components/FotosProgresoModule.vue';

const router = useRouter();
const user = computed(() => authService.getUsuario() || { nombre: 'Usuario', username: 'usuario', rol: 'Ninguno' });
const alumnoSocioId = ref(null);

const loadAlumnoSocioId = async () => {
  if (user.value.rol === 'Alumno') {
    try {
      const res = await api.get('/socio/mi-socio');
      if (res.data && res.data.id) {
        alumnoSocioId.value = res.data.id;
      }
    } catch (err) {
      try {
        const resList = await api.get('/socio', { params: { pageSize: 100 } });
        const mySocio = resList.data.items?.find(s => s.idUsuario === user.value.id);
        if (mySocio) {
          alumnoSocioId.value = mySocio.id;
        }
      } catch (err2) {
        console.error('Error al obtener socio del alumno:', err2);
      }
    }
  }
};

// Métricas reales del administrador
const adminStats = ref({
  totalAlumnos: 0,
  totalCoaches: 0,
  totalPlanesVigentes: 0,
  recaudadoEsteMes: 0
});
const isLoadingAdminStats = ref(false);

// Lista de alumnos del coach desde API
const misAlumnos = ref([]);
const isLoadingAlumnos = ref(false);
const errorAlumnos = ref('');

// Detalle del alumno seleccionado
const selectedAlumno = ref(null);
const isLoadingDetalle = ref(false);
const errorDetalle = ref('');
const isModalOpen = ref(false);

// Datos simulados para vista de Alumno
const miInformacion = ref({
  plan: 'Plan Musculación & Pase Libre',
  precioPlan: '$15.000 / mes',
  estadoCuenta: 'Al día',
  proximoVencimiento: '10/08/2026',
  progresoMes: '12 asistencias de 16 objetivo',
  porcentajeProgreso: 75,
  miCoach: 'Prof. Lucas Fernández (Coach Senior)',
  proximaSesion: 'Hoy a las 18:00 hs - Zona Musculación'
});

const loadAdminStats = async () => {
  if (user.value.rol !== 'Administrador') return;
  isLoadingAdminStats.value = true;
  try {
    const res = await api.get('/dashboard/stats');
    adminStats.value = res.data;
  } catch (err) {
    console.error('Error al cargar métricas de administrador:', err);
  } finally {
    isLoadingAdminStats.value = false;
  }
};

const loadMisAlumnos = async () => {
  if (user.value.rol !== 'Coach' && user.value.rol !== 'Administrador') return;
  isLoadingAlumnos.value = true;
  errorAlumnos.value = '';
  try {
    const res = await api.get('/coach/alumnos');
    misAlumnos.value = res.data;
  } catch (err) {
    if (err.response && err.response.data && err.response.data.errors) {
      errorAlumnos.value = err.response.data.errors.join(' ');
    } else {
      errorAlumnos.value = 'No se pudieron cargar tus alumnos asignados.';
    }
  } finally {
    isLoadingAlumnos.value = false;
  }
};

const openDetalleAlumno = async (alumnoId) => {
  isModalOpen.value = true;
  isLoadingDetalle.value = true;
  errorDetalle.value = '';
  selectedAlumno.value = null;

  try {
    const res = await api.get(`/coach/alumnos/${alumnoId}`);
    selectedAlumno.value = res.data;
  } catch (err) {
    if (err.response && err.response.status === 403) {
      errorDetalle.value = '🚫 Acceso Denegado: No tienes permisos para consultar la información de este alumno.';
    } else if (err.response && err.response.data && err.response.data.errors) {
      errorDetalle.value = err.response.data.errors.join(' ');
    } else {
      errorDetalle.value = 'Ocurrió un error al consultar la información del alumno.';
    }
  } finally {
    isLoadingDetalle.value = false;
  }
};

const closeModal = () => {
  isModalOpen.value = false;
  selectedAlumno.value = null;
  errorDetalle.value = '';
};

const navigateTo = (path) => {
  router.push(path);
};

onMounted(() => {
  if (user.value.rol === 'Administrador') {
    loadAdminStats();
  }
  if (user.value.rol === 'Coach' || user.value.rol === 'Administrador') {
    loadMisAlumnos();
  }
  if (user.value.rol === 'Alumno') {
    loadAlumnoSocioId();
  }
});
</script>

<template>
  <div class="dashboard-container">
    <!-- ========================================== -->
    <!-- VISTA ADMINISTRADOR -->
    <!-- ========================================== -->
    <template v-if="user.rol === 'Administrador'">
      <header class="header">
        <div>
          <h1>Panel de Control General</h1>
          <p class="subtitle">Bienvenido al sistema de administración global del gimnasio</p>
        </div>
        <button @click="navigateTo('/profile')" class="btn-profile-top">
          ⚙️ Mi Perfil
        </button>
      </header>

      <!-- Métricas globales reales -->
      <div class="metrics-grid">
        <div class="metric-card">
          <div class="metric-icon bg-blue">👥</div>
          <div class="metric-info">
            <span class="metric-value">{{ isLoadingAdminStats ? '...' : adminStats.totalAlumnos }}</span>
            <span class="metric-label">Alumnos / Socios</span>
          </div>
        </div>
        <div class="metric-card">
          <div class="metric-icon bg-orange">🏋️</div>
          <div class="metric-info">
            <span class="metric-value">{{ isLoadingAdminStats ? '...' : adminStats.totalCoaches }}</span>
            <span class="metric-label">Coaches Activos</span>
          </div>
        </div>
        <div class="metric-card">
          <div class="metric-icon bg-green">💳</div>
          <div class="metric-info">
            <span class="metric-value">${{ isLoadingAdminStats ? '...' : adminStats.recaudadoEsteMes.toLocaleString() }}</span>
            <span class="metric-label">Recaudado este mes</span>
          </div>
        </div>
        <div class="metric-card">
          <div class="metric-icon bg-purple">📋</div>
          <div class="metric-info">
            <span class="metric-value">{{ isLoadingAdminStats ? '...' : adminStats.totalPlanesVigentes }}</span>
            <span class="metric-label">Planes Vigentes</span>
          </div>
        </div>
      </div>

      <!-- Módulos de Administración Global -->
      <h2 class="section-title">Módulos de Gestión</h2>
      <div class="admin-modules-grid">
        <div class="module-card" @click="navigateTo('/socios')">
          <div class="module-header">
            <span class="module-icon">👥</span>
            <h3>Alumnos / Socios</h3>
          </div>
          <p>Alta, baja, edición de datos de socios y seguimiento de estado.</p>
          <span class="module-action">Gestionar Alumnos →</span>
        </div>

        <div class="module-card" @click="navigateTo('/planes')">
          <div class="module-header">
            <span class="module-icon">📋</span>
            <h3>Planes y Tarifas</h3>
          </div>
          <p>Configuración de membresías, precios mensuales y condiciones.</p>
          <span class="module-action">Gestionar Planes →</span>
        </div>

        <div class="module-card" @click="navigateTo('/cuotas')">
          <div class="module-header">
            <span class="module-icon">💳</span>
            <h3>Cobros y Cuotas</h3>
          </div>
          <p>Registro de pagos, emisión de recibos y consulta de cuotas vencidas.</p>
          <span class="module-action">Gestionar Cuotas →</span>
        </div>
      </div>
    </template>

    <!-- ========================================== -->
    <!-- VISTA COACH -->
    <!-- ========================================== -->
    <template v-else-if="user.rol === 'Coach'">
      <header class="header">
        <div>
          <h1>Mis Alumnos</h1>
          <p class="subtitle">Gestión y seguimiento individualizado de tus alumnos asignados</p>
        </div>
        <span class="badge-role-header">🏋️ Coach</span>
      </header>

      <div class="coach-summary-banner">
        <h2>¡Hola, {{ user.nombre }}!</h2>
        <p>Tienes <strong>{{ misAlumnos.length }} alumno(s) asignado(s)</strong> a tu cargo.</p>
      </div>

      <!-- Tabla Mis Alumnos -->
      <div class="content-card">
        <h2 class="section-title margin-top-0">Mis Alumnos Asignados</h2>

        <div v-if="isLoadingAlumnos" class="state-msg">Cargando tus alumnos asignados...</div>
        <div v-else-if="errorAlumnos" class="alert alert-danger">{{ errorAlumnos }}</div>
        <div v-else-if="misAlumnos.length === 0" class="state-msg">No tienes alumnos asignados actualmente.</div>

        <div v-else class="table-responsive">
          <table class="data-table">
            <thead>
              <tr>
                <th>Alumno</th>
                <th>DNI</th>
                <th>Plan</th>
                <th>Estado</th>
                <th>Deuda / Cuota</th>
                <th>Evoluciones</th>
                <th>Acción</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="alumno in misAlumnos" :key="alumno.id">
                <td class="font-bold">{{ alumno.nombreCompleto || alumno.nombre }}</td>
                <td>{{ alumno.dni || '-' }}</td>
                <td>{{ alumno.planNombre }}</td>
                <td>
                  <span class="status-badge" :class="alumno.estado === 'Activo' ? 'badge-success' : 'badge-warning'">
                    {{ alumno.estado }}
                  </span>
                </td>
                <td>
                  <span class="status-badge" :class="alumno.deudaEstado === 'Al día' ? 'badge-success' : 'badge-danger'">
                    {{ alumno.deudaEstado }}
                  </span>
                </td>
                <td>
                  <span class="count-badge">{{ alumno.cantidadEvoluciones || 0 }} evoluciones</span>
                </td>
                <td>
                  <button @click="openDetalleAlumno(alumno.id)" class="btn-detail">
                    🔍 Ver Detalle
                  </button>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </template>

    <!-- ========================================== -->
    <!-- VISTA ALUMNO -->
    <!-- ========================================== -->
    <template v-else-if="user.rol === 'Alumno'">
      <header class="header">
        <div>
          <h1>Mi Panel de Alumno</h1>
          <p class="subtitle">Bienvenido a tu espacio personal de entrenamiento</p>
        </div>
        <span class="badge-role-header badge-green">🎓 Alumno</span>
      </header>

      <div class="alumno-dashboard-grid">
        <!-- Mi Plan y Estado de Cuenta -->
        <div class="content-card">
          <div class="card-header-icon">
            <span class="card-icon">📋</span>
            <h3>Mi Plan y Membresía</h3>
          </div>
          <div class="info-group">
            <span class="info-label">Plan Actual:</span>
            <span class="info-value font-bold text-accent">{{ miInformacion.plan }}</span>
          </div>
          <div class="info-group">
            <span class="info-label">Costo Mensual:</span>
            <span class="info-value">{{ miInformacion.precioPlan }}</span>
          </div>
          <div class="info-group">
            <span class="info-label">Estado de Cuenta:</span>
            <span class="status-badge badge-success">{{ miInformacion.estadoCuenta }}</span>
          </div>
          <div class="info-group">
            <span class="info-label">Próximo Vencimiento:</span>
            <span class="info-value">📅 {{ miInformacion.proximoVencimiento }}</span>
          </div>
        </div>

        <!-- Mi Progreso -->
        <div class="content-card">
          <div class="card-header-icon">
            <span class="card-icon">📈</span>
            <h3>Mi Progreso este Mes</h3>
          </div>
          <p class="text-subtle">{{ miInformacion.progresoMes }}</p>
          <div class="large-progress-container">
            <div class="large-progress-fill" :style="{ width: miInformacion.porcentajeProgreso + '%' }"></div>
          </div>
          <span class="large-progress-percentage">{{ miInformacion.porcentajeProgreso }}% Cumplido</span>
        </div>
      </div>

      <!-- Módulo Fotos de Progreso & Evolución Corporal (Alumno) -->
      <FotosProgresoModule v-if="alumnoSocioId" :id-socio="alumnoSocioId" :can-edit="true" />
    </template>

    <!-- ========================================== -->
    <!-- MODAL DETALLE DE ALUMNO (COACH / ADMIN) -->
    <!-- ========================================== -->
    <div v-if="isModalOpen" class="modal-overlay" @click.self="closeModal">
      <div class="modal-card">
        <button class="modal-close-btn" @click="closeModal">✕</button>

        <div v-if="isLoadingDetalle" class="state-msg">Cargando detalle del alumno...</div>
        <div v-else-if="errorDetalle" class="alert alert-danger">{{ errorDetalle }}</div>
        
        <template v-else-if="selectedAlumno">
          <div class="modal-header">
            <h2>{{ selectedAlumno.nombreCompleto }}</h2>
            <span class="status-badge" :class="selectedAlumno.estado === 'Activo' ? 'badge-success' : 'badge-warning'">
              {{ selectedAlumno.estado }}
            </span>
          </div>

          <div class="detail-grid">
            <!-- Sección Información Personal -->
            <div class="detail-section">
              <h4>📌 Datos Personales</h4>
              <p><strong>DNI:</strong> {{ selectedAlumno.dni }}</p>
              <p><strong>Teléfono:</strong> {{ selectedAlumno.telefono || 'No especificado' }}</p>
              <p><strong>Email:</strong> {{ selectedAlumno.email || 'No especificado' }}</p>
              <p><strong>Fecha de Alta:</strong> {{ new Date(selectedAlumno.fechaAlta).toLocaleDateString() }}</p>
              <p v-if="selectedAlumno.observacion"><strong>Observación:</strong> {{ selectedAlumno.observacion }}</p>
            </div>

            <!-- Sección Plan & Coach -->
            <div class="detail-section">
              <h4>📋 Plan & Entrenador</h4>
              <p><strong>Plan Actual:</strong> {{ selectedAlumno.planNombre }}</p>
              <p><strong>Precio Mensual:</strong> ${{ selectedAlumno.planPrecio.toLocaleString() }}</p>
              <p><strong>Coach Asignado:</strong> {{ selectedAlumno.coachNombre }}</p>
            </div>

            <!-- Sección Estado de Cuenta -->
            <div class="detail-section">
              <h4>💳 Estado de Cuenta</h4>
              <p><strong>Estado:</strong> 
                <span class="status-badge" :class="selectedAlumno.deudaEstado === 'Al día' ? 'badge-success' : 'badge-danger'">
                  {{ selectedAlumno.deudaEstado }}
                </span>
              </p>
              <p><strong>Cuotas Pendientes:</strong> {{ selectedAlumno.cuotasPendientesCount }}</p>
              <p><strong>Próximo Vencimiento:</strong> {{ selectedAlumno.proximoVencimiento }}</p>
            </div>

            <!-- Sección Progreso & Sesiones -->
            <div class="detail-section">
              <h4>📈 Progreso & Sesiones</h4>
              <p><strong>Rendimiento:</strong> {{ selectedAlumno.progreso }}</p>
              <p><strong>Asistencias del Mes:</strong> {{ selectedAlumno.asistenciasMes }} días</p>
              <p><strong>Próxima Sesión:</strong> {{ selectedAlumno.proximaSesion }}</p>
            </div>
          </div>

          <div class="detail-section margin-top-16">
            <h4>📅 Últimas Sesiones de Entrenamiento</h4>
            <ul class="sessions-list">
              <li v-for="(sesion, idx) in selectedAlumno.ultimasSesiones" :key="idx">
                {{ sesion }}
              </li>
            </ul>
          </div>

          <!-- Módulo Fotos de Progreso para Alumno Seleccionado -->
          <FotosProgresoModule v-if="selectedAlumno && selectedAlumno.id" :id-socio="selectedAlumno.id" :can-edit="true" />

          <div class="modal-actions">
            <button @click="closeModal" class="btn-close-modal">Cerrar Detalle</button>
          </div>
        </template>
      </div>
    </div>
  </div>
</template>

<style scoped>
.dashboard-container {
  max-width: 1120px;
  margin: 0 auto;
  padding: 28px 24px;
  text-align: left;
  box-sizing: border-box;
}

.header {
  display: flex;
  justify-content: space-between;
  align-items: center;
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
  margin-bottom: 0;
  opacity: 0.75;
}

.btn-profile-top {
  padding: 10px 18px;
  font-size: 14px;
  font-weight: 600;
  background-color: var(--code-bg);
  color: var(--text-h);
  border: 1px solid var(--border);
  border-radius: 8px;
  cursor: pointer;
  transition: all 0.2s ease;
}

.btn-profile-top:hover {
  border-color: var(--accent);
  background-color: rgba(0, 0, 0, 0.05);
}

.badge-role-header {
  padding: 6px 14px;
  font-size: 13px;
  font-weight: 700;
  background-color: rgba(245, 158, 11, 0.15);
  color: #d97706;
  border: 1px solid rgba(245, 158, 11, 0.3);
  border-radius: 20px;
}

.badge-green {
  background-color: rgba(16, 185, 129, 0.15) !important;
  color: #10b981 !important;
  border-color: rgba(16, 185, 129, 0.3) !important;
}

/* Métricas Admin */
.metrics-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));
  gap: 20px;
  margin-bottom: 36px;
}

.metric-card {
  background-color: var(--bg);
  border: 1px solid var(--border);
  border-radius: 12px;
  padding: 20px;
  display: flex;
  align-items: center;
  gap: 16px;
  box-shadow: var(--shadow);
}

.metric-icon {
  width: 48px;
  height: 48px;
  border-radius: 10px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 22px;
}

.bg-blue { background-color: rgba(59, 130, 246, 0.12); color: #3b82f6; }
.bg-orange { background-color: rgba(245, 158, 11, 0.12); color: #f59e0b; }
.bg-green { background-color: rgba(16, 185, 129, 0.12); color: #10b981; }
.bg-purple { background-color: rgba(139, 92, 246, 0.12); color: #8b5cf6; }

.metric-info {
  display: flex;
  flex-direction: column;
}

.metric-value {
  font-size: 22px;
  font-weight: 700;
  color: var(--text-h);
}

.metric-label {
  font-size: 13px;
  color: var(--text);
  opacity: 0.8;
}

.section-title {
  font-size: 20px;
  font-weight: 700;
  color: var(--text-h);
  margin-bottom: 20px;
}

.margin-top-0 {
  margin-top: 0;
}

.margin-top-16 {
  margin-top: 16px;
}

/* Grilla de módulos Admin */
.admin-modules-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
  gap: 20px;
}

.module-card {
  background-color: var(--bg);
  border: 1px solid var(--border);
  border-radius: 12px;
  padding: 24px;
  box-shadow: var(--shadow);
  cursor: pointer;
  transition: all 0.2s ease;
  display: flex;
  flex-direction: column;
  justify-content: space-between;
}

.module-card:hover {
  transform: translateY(-2px);
  border-color: var(--accent);
  box-shadow: 0 6px 20px rgba(0,0,0,0.08);
}

.module-header {
  display: flex;
  align-items: center;
  gap: 12px;
  margin-bottom: 10px;
}

.module-icon {
  font-size: 24px;
}

.module-header h3 {
  margin: 0;
  font-size: 18px;
  color: var(--text-h);
}

.module-card p {
  font-size: 14px;
  color: var(--text);
  opacity: 0.8;
  margin-bottom: 18px;
  line-height: 1.4;
}

.module-action {
  font-size: 14px;
  font-weight: 600;
  color: var(--accent);
}

/* Banner y Tablas Coach */
.coach-summary-banner {
  background: linear-gradient(135deg, rgba(245, 158, 11, 0.1), rgba(99, 102, 241, 0.1));
  border: 1px solid rgba(245, 158, 11, 0.25);
  border-radius: 12px;
  padding: 24px;
  margin-bottom: 28px;
}

.coach-summary-banner h2 {
  margin-top: 0;
  margin-bottom: 6px;
  color: var(--text-h);
}

.coach-summary-banner p {
  margin: 0;
  font-size: 15px;
}

.content-card {
  background-color: var(--bg);
  border: 1px solid var(--border);
  border-radius: 12px;
  padding: 24px;
  box-shadow: var(--shadow);
  margin-bottom: 24px;
}

.table-responsive {
  overflow-x: auto;
}

.data-table {
  width: 100%;
  border-collapse: collapse;
  font-size: 14px;
}

.data-table th, .data-table td {
  padding: 12px 14px;
  text-align: left;
  border-bottom: 1px solid var(--border);
}

.data-table th {
  background-color: var(--code-bg);
  color: var(--text-h);
  font-weight: 600;
}

.font-bold { font-weight: 600; }
.text-accent { color: var(--accent); }
.text-subtle { color: var(--text); opacity: 0.8; font-size: 13px; }

.status-badge {
  padding: 4px 10px;
  border-radius: 12px;
  font-size: 12px;
  font-weight: 600;
}

.badge-success { background-color: rgba(16, 185, 129, 0.15); color: #10b981; }
.badge-warning { background-color: rgba(245, 158, 11, 0.15); color: #d97706; }
.badge-danger { background-color: rgba(239, 68, 68, 0.15); color: #ef4444; }

.btn-detail {
  padding: 6px 12px;
  font-size: 13px;
  font-weight: 600;
  background-color: var(--code-bg);
  border: 1px solid var(--border);
  border-radius: 6px;
  cursor: pointer;
  transition: all 0.2s ease;
  color: var(--text-h);
}

.btn-detail:hover {
  background-color: var(--accent);
  color: #fff;
  border-color: var(--accent);
}

.progress-bar-container {
  width: 90px;
  height: 8px;
  background-color: var(--border);
  border-radius: 4px;
  overflow: hidden;
  display: inline-block;
  vertical-align: middle;
  margin-right: 8px;
}

.progress-bar-fill {
  height: 100%;
  background-color: var(--accent);
}

.progress-text {
  font-size: 12px;
  font-weight: 600;
}

.state-msg {
  padding: 20px;
  text-align: center;
  color: var(--text);
  font-size: 15px;
}

.alert-danger {
  background-color: rgba(239, 68, 68, 0.12);
  border: 1px solid rgba(239, 68, 68, 0.3);
  color: #991b1b;
  padding: 12px;
  border-radius: 8px;
  font-size: 14px;
}

/* Modal Detalle */
.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  width: 100vw;
  height: 100vh;
  background-color: rgba(0, 0, 0, 0.5);
  backdrop-filter: blur(4px);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 999;
  padding: 20px;
  box-sizing: border-box;
}

.modal-card {
  background-color: var(--bg);
  border: 1px solid var(--border);
  border-radius: 14px;
  padding: 28px;
  width: 100%;
  max-width: 650px;
  max-height: 90vh;
  overflow-y: auto;
  box-shadow: 0 10px 30px rgba(0, 0, 0, 0.2);
  position: relative;
}

.modal-close-btn {
  position: absolute;
  top: 18px;
  right: 18px;
  background: transparent;
  border: none;
  font-size: 20px;
  font-weight: 700;
  color: var(--text);
  cursor: pointer;
}

.modal-header {
  display: flex;
  align-items: center;
  gap: 14px;
  margin-bottom: 20px;
}

.modal-header h2 {
  margin: 0;
  font-size: 24px;
  color: var(--text-h);
}

.detail-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 16px;
}

@media (max-width: 600px) {
  .detail-grid {
    grid-template-columns: 1fr;
  }
}

.detail-section {
  background-color: var(--code-bg);
  border: 1px solid var(--border);
  border-radius: 10px;
  padding: 16px;
}

.detail-section h4 {
  margin-top: 0;
  margin-bottom: 10px;
  font-size: 15px;
  color: var(--text-h);
}

.detail-section p {
  margin: 6px 0;
  font-size: 13px;
  color: var(--text);
}

.sessions-list {
  margin: 0;
  padding-left: 20px;
  font-size: 13px;
  color: var(--text);
}

.sessions-list li {
  margin-bottom: 6px;
}

.modal-actions {
  margin-top: 24px;
  display: flex;
  justify-content: flex-end;
}

.btn-close-modal {
  padding: 10px 20px;
  font-size: 14px;
  font-weight: 600;
  background-color: var(--accent);
  color: #fff;
  border: none;
  border-radius: 8px;
  cursor: pointer;
}
</style>

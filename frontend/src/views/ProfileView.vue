<script setup>
import { ref, onMounted } from 'vue';
import authService from '../services/authService';

const user = ref({
  id: 0,
  username: '',
  nombre: '',
  rol: '',
  rutaAvatar: null,
});

const nombreInput = ref('');
const selectedFile = ref(null);
const previewAvatarUrl = ref(null);

const isSavingProfile = ref(false);
const isUploadingAvatar = ref(false);

const profileSuccessMsg = ref('');
const profileErrorMsg = ref('');
const avatarSuccessMsg = ref('');
const avatarErrorMsg = ref('');

const getAvatarUrl = (ruta) => {
  if (!ruta) return null;
  if (ruta.startsWith('http')) return ruta;
  return `http://localhost:5055${ruta}`;
};

const loadProfile = async () => {
  try {
    const profileData = await authService.fetchMe();
    user.value = profileData;
    nombreInput.value = profileData.nombre;
  } catch (err) {
    const localUser = authService.getUsuario();
    if (localUser) {
      user.value = localUser;
      nombreInput.value = localUser.nombre;
    }
  }
};

const onFileSelected = (event) => {
  avatarErrorMsg.value = '';
  avatarSuccessMsg.value = '';
  const file = event.target.files[0];

  if (!file) {
    selectedFile.value = null;
    previewAvatarUrl.value = null;
    return;
  }

  // Validate extension
  const allowedTypes = ['image/jpeg', 'image/png', 'image/webp'];
  if (!allowedTypes.includes(file.type)) {
    avatarErrorMsg.value = 'Formato no válido. Selecciona una imagen JPG, PNG o WEBP.';
    selectedFile.value = null;
    previewAvatarUrl.value = null;
    return;
  }

  // Validate max size 5 MB
  if (file.size > 5 * 1024 * 1024) {
    avatarErrorMsg.value = 'La imagen excede el tamaño máximo permitido (5 MB).';
    selectedFile.value = null;
    previewAvatarUrl.value = null;
    return;
  }

  selectedFile.value = file;
  previewAvatarUrl.value = URL.createObjectURL(file);
};

const uploadAvatar = async () => {
  if (!selectedFile.value) return;

  isUploadingAvatar.value = true;
  avatarErrorMsg.value = '';
  avatarSuccessMsg.value = '';

  try {
    const res = await authService.uploadAvatar(selectedFile.value);
    avatarSuccessMsg.value = '¡Avatar actualizado correctamente!';
    selectedFile.value = null;
    previewAvatarUrl.value = null;
    
    // Refresh user profile
    await loadProfile();
    window.dispatchEvent(new Event('user-profile-updated'));
  } catch (err) {
    if (err.response && err.response.data && err.response.data.errors) {
      avatarErrorMsg.value = err.response.data.errors.join(' ');
    } else {
      avatarErrorMsg.value = 'Ocurrió un error al subir la imagen del avatar.';
    }
  } finally {
    isUploadingAvatar.value = false;
  }
};

const updateProfile = async () => {
  if (!nombreInput.value.trim()) {
    profileErrorMsg.value = 'El nombre no puede estar vacío.';
    return;
  }

  isSavingProfile.value = true;
  profileErrorMsg.value = '';
  profileSuccessMsg.value = '';

  try {
    const updatedUser = await authService.updateProfile(nombreInput.value.trim());
    user.value = updatedUser;
    profileSuccessMsg.value = '¡Nombre de perfil actualizado correctamente!';
    window.dispatchEvent(new Event('user-profile-updated'));
  } catch (err) {
    if (err.response && err.response.data && err.response.data.errors) {
      profileErrorMsg.value = err.response.data.errors.join(' ');
    } else {
      profileErrorMsg.value = 'Ocurrió un error al actualizar los datos del perfil.';
    }
  } finally {
    isSavingProfile.value = false;
  }
};

onMounted(() => {
  loadProfile();
});
</script>

<template>
  <div class="profile-container">
    <header class="page-header">
      <h1>Perfil de Usuario</h1>
      <p class="subtitle">Gestiona tu información personal y foto de perfil</p>
    </header>

    <div class="profile-grid">
      <!-- Card Avatar -->
      <div class="card avatar-card">
        <h3>Imagen de Perfil</h3>
        <p class="section-desc">Esta imagen se mostrará en el menú y en la barra lateral.</p>

        <div class="avatar-display">
          <img
            v-if="previewAvatarUrl"
            :src="previewAvatarUrl"
            alt="Vista previa"
            class="avatar-image preview"
          />
          <img
            v-else-if="user.rutaAvatar"
            :src="getAvatarUrl(user.rutaAvatar)"
            alt="Avatar actual"
            class="avatar-image"
          />
          <div v-else class="avatar-placeholder">
            {{ (user.nombre || user.username || 'U').charAt(0).toUpperCase() }}
          </div>
        </div>

        <div v-if="avatarSuccessMsg" class="alert alert-success">
          {{ avatarSuccessMsg }}
        </div>
        <div v-if="avatarErrorMsg" class="alert alert-error">
          {{ avatarErrorMsg }}
        </div>

        <div class="file-upload-section">
          <label for="avatar-input" class="btn-file-select">
            {{ selectedFile ? 'Cambiar imagen seleccionada' : 'Seleccionar imagen' }}
          </label>
          <input
            id="avatar-input"
            type="file"
            accept="image/jpeg,image/png,image/webp"
            @change="onFileSelected"
            class="hidden-file-input"
          />
          <span v-if="selectedFile" class="file-name">{{ selectedFile.name }}</span>

          <button
            v-if="selectedFile"
            @click="uploadAvatar"
            class="btn-upload"
            :disabled="isUploadingAvatar"
          >
            {{ isUploadingAvatar ? 'Subiendo...' : 'Guardar Nuevo Avatar' }}
          </button>
        </div>
      </div>

      <!-- Card Datos -->
      <div class="card data-card">
        <h3>Información Personal</h3>
        <p class="section-desc">Modifica tu nombre público en el sistema.</p>

        <div v-if="profileSuccessMsg" class="alert alert-success">
          {{ profileSuccessMsg }}
        </div>
        <div v-if="profileErrorMsg" class="alert alert-error">
          {{ profileErrorMsg }}
        </div>

        <form @submit.prevent="updateProfile">
          <div class="form-group">
            <label for="username">Usuario (No modificable)</label>
            <input
              type="text"
              id="username"
              :value="user.username"
              disabled
              class="input-disabled"
            />
          </div>

          <div class="form-group">
            <label for="rol">Rol (No modificable)</label>
            <input
              type="text"
              id="rol"
              :value="user.rol"
              disabled
              class="input-disabled"
            />
          </div>

          <div class="form-group">
            <label for="nombre">Nombre Visible *</label>
            <input
              type="text"
              id="nombre"
              v-model="nombreInput"
              required
              placeholder="Ingresa tu nombre completo"
            />
          </div>

          <button
            type="submit"
            class="btn-save"
            :disabled="isSavingProfile"
          >
            {{ isSavingProfile ? 'Guardando...' : 'Guardar Cambios' }}
          </button>
        </form>
      </div>
    </div>
  </div>
</template>

<style scoped>
.profile-container {
  width: 100%;
  max-width: 1400px;
  margin: 0 auto;
  padding: 28px 24px;
  text-align: left;
  box-sizing: border-box;
}

.page-header {
  margin-bottom: 24px;
  border-bottom: 1px solid var(--border);
  padding-bottom: 16px;
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
  opacity: 0.75;
}

.profile-grid {
  display: grid;
  grid-template-columns: 320px 1fr;
  gap: 24px;
}

@media (max-width: 768px) {
  .profile-grid {
    grid-template-columns: 1fr;
  }
}

.card {
  background-color: var(--bg);
  border: 1px solid var(--border);
  border-radius: 12px;
  padding: 24px;
  box-shadow: var(--shadow);
}

.card h3 {
  margin-top: 0;
  margin-bottom: 6px;
  font-size: 18px;
  color: var(--text-h);
}

.section-desc {
  font-size: 13px;
  color: var(--text);
  margin-bottom: 20px;
  opacity: 0.8;
}

.avatar-display {
  display: flex;
  justify-content: center;
  margin-bottom: 20px;
}

.avatar-image {
  width: 130px;
  height: 130px;
  border-radius: 50%;
  object-fit: cover;
  border: 3px solid var(--accent);
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
}

.avatar-image.preview {
  border-color: #10b981;
}

.avatar-placeholder {
  width: 130px;
  height: 130px;
  border-radius: 50%;
  background: linear-gradient(135deg, var(--accent), #4f46e5);
  color: #fff;
  font-size: 54px;
  font-weight: 700;
  display: flex;
  align-items: center;
  justify-content: center;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
}

.file-upload-section {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 12px;
}

.hidden-file-input {
  display: none;
}

.btn-file-select {
  display: inline-block;
  padding: 10px 16px;
  font-size: 14px;
  font-weight: 600;
  border: 1px solid var(--border);
  border-radius: 8px;
  background-color: var(--code-bg);
  color: var(--text-h);
  cursor: pointer;
  transition: all 0.2s ease;
  width: 100%;
  text-align: center;
  box-sizing: border-box;
}

.btn-file-select:hover {
  background-color: rgba(0, 0, 0, 0.05);
  border-color: var(--accent);
}

.file-name {
  font-size: 12px;
  color: var(--text);
  word-break: break-all;
}

.btn-upload, .btn-save {
  width: 100%;
  padding: 12px;
  font-size: 15px;
  font-weight: 600;
  background-color: var(--accent);
  color: #fff;
  border: none;
  border-radius: 8px;
  cursor: pointer;
  transition: opacity 0.2s ease;
}

.btn-upload:hover, .btn-save:hover {
  opacity: 0.9;
}

.btn-upload:disabled, .btn-save:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.form-group {
  margin-bottom: 18px;
}

.form-group label {
  display: block;
  font-size: 14px;
  font-weight: 500;
  color: var(--text-h);
  margin-bottom: 6px;
}

.form-group input {
  width: 100%;
  padding: 11px 14px;
  font-size: 15px;
  border: 1px solid var(--border);
  border-radius: 8px;
  background-color: transparent;
  color: inherit;
  outline: none;
  box-sizing: border-box;
}

.form-group input:focus {
  border-color: var(--accent);
}

.input-disabled {
  background-color: var(--code-bg) !important;
  color: var(--text) !important;
  opacity: 0.7;
  cursor: not-allowed;
}

.alert {
  padding: 10px 14px;
  border-radius: 8px;
  font-size: 13px;
  margin-bottom: 16px;
}

.alert-success {
  background-color: rgba(16, 185, 129, 0.12);
  border: 1px solid rgba(16, 185, 129, 0.3);
  color: #065f46;
}

.alert-error {
  background-color: rgba(239, 68, 68, 0.12);
  border: 1px solid rgba(239, 68, 68, 0.3);
  color: #991b1b;
}
</style>

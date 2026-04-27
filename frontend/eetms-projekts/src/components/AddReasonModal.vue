<template>
  <div v-if="isOpen" class="modal-overlay" @click.self="$emit('close')">
    <div class="modal-content">
      <div v-if="showWarning" class="warning-banner">
        <div class="warning-content">
          <span class="warning-icon">!</span>
          <span class="warning-text">{{ warningMessage }}</span>
          <button class="warning-close" @click="showWarning = false">×</button>
        </div>
      </div>

      <div class="section-header">
        <h2>Add New Reason</h2>
      </div>

      <form novalidate @submit.prevent="handleSave" class="company-details">
        <div class="form-group">
          <label>Reason Name:</label>
          <input
            v-model.trim="form.Name"
            class="edit-input"
            required
            maxlength="25"
            @keyup.enter="handleSave"
          />
        </div>
      </form>

      <div class="modal-actions">
        <button @click="handleSave" class="btn-save" title="Save Reason">
          <FontAwesomeIcon :icon="['fas', 'check']" />
        </button>
        <button @click="$emit('close')" class="btn-delete" title="Cancel">
          <FontAwesomeIcon :icon="['fas', 'xmark']" />
        </button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, watch, onMounted } from 'vue'; 
import { FontAwesomeIcon } from '@fortawesome/vue-fontawesome';
import axios from 'axios';

const emit = defineEmits(['close', 'save']); 
const props = defineProps({ isOpen: Boolean }); 
const companies = ref([]); 
const API_BASE = 'http://localhost:5001/api'; 

const showWarning = ref(false)
const warningMessage = ref('')
let warningTimer = null 

const showWarningMessage = (message) => {
  warningMessage.value = message 
  showWarning.value = true 

  if (warningTimer) clearTimeout(warningTimer)
  warningTimer = setTimeout(() => {
    showWarning.value = false 
    warningTimer = null
  }, 3000) 
}; 

const form = ref({
    Name: '',
    CompanyID: null 
});

onMounted(async () => {
    try {
        const res = await axios.get(`${API_BASE}/companies`);
        companies.value = res.data; 
    } catch (error) {
        console.error('Companies load failed:', error); 
    }
}); 

const handleSave = () => {
  showWarning.value = false 
  warningMessage.value = ''

  const name = (form.value.Name ?? '').trim()
  
  if (!name) {
    showWarningMessage("Please fill in all fields.");
    return; 
  } 

  if (name.length < 5) {
    showWarningMessage('Reason name must contain at least 5 characters.')
    return
  }

  const newReason = {
    ReasonID: 0, 
    Name: form.value.Name, 
    CompanyID: parseInt(form.value.CompanyID), 
    Shifts: [],
    CompanyReasons: [] 
  }; 

  emit('save', newReason); 
}; 

watch(() => props.isOpen, (val) => {
    if (!val) {
        form.value = {
           Name: '',
           CompanyID: null 
        }; 
    }
}); 
</script>

<style scoped>
.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: var(--color-overlay);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 2000;
  padding: 20px;
}

.modal-content {
  font-family: 'Inter';
  background: var(--color-white);
  border-radius: 12px;
  max-width: 600px;
  width: 100%;
  position: relative;
  box-shadow: 0 4px 20px var(--shadow-berry);
  overflow: hidden; 
}

.section-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  background: var(--header-gradient);
}

.section-header h2 {
  margin: 0;
  color: var(--color-white);
  font-family: 'Inter';
  font-size: 22px;
  font-weight: 600;
  padding: 15px 20px;
}

.warning-banner {
  position: fixed;
  top: 20px;
  right: 20px;
  z-index: 3000; 
  animation: slideIn 0.3s ease-out;
}

@keyframes slideIn {
  from {
    transform: translateX(400px);
    opacity: 0;
  }
  to {
    transform: translateX(0);
    opacity: 1;
  }
}

.warning-content {
  background: var(--color-warning-bg);
  border: 1px solid var(--color-warning-border);
  border-radius: 8px;
  padding: 16px 20px;
  display: flex;
  align-items: center;
  gap: 12px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
  min-width: 300px;
  max-width: 500px;
}

.warning-icon {
  background: var(--color-warning-border);
  color: var(--color-white);
  width: 24px;
  height: 24px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: bold;
  flex-shrink: 0;
}

.warning-close {
  background: none;
  border: none;
  font-size: 24px;
  color: var(--color-warning-text);
  cursor: pointer;
  padding: 0;
  margin-left: auto;
  flex-shrink: 0;
}

.warning-close:hover {
  color: var(--color-black);
}

.company-details {
  padding: 30px;
  background: var(--color-white);
}

.form-row {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 20px;
}

.form-group {
  margin-bottom: 20px;
}

.form-group label {
  font-family: 'Inter';
  font-size: 14px;
  font-weight: 600;
  color: var(--brand-berry);
  margin-bottom: 8px;
  display: block;
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

.edit-input, 
.edit-select {
  width: 100%; 
  height: 42px; 
  border: 2px solid var(--color-border); 
  border-radius: 40px;
  font-family: 'Inter';
  box-shadow: 0 2px 8px var(--shadow-berry);
  padding: 0 15px;
  box-sizing: border-box;
  transition: border-color 0.3s ease;
}

.edit-input:focus, .edit-select:focus {
  outline: none;
  border-color: var(--brand-berry);
}

.modal-actions {
  display: flex;
  justify-content: flex-end;
  gap: 15px;
  padding: 0 30px 30px 30px;
}

.btn-save, .btn-delete {
  width: 45px;
  height: 45px;
  border: none;
  background: var(--color-white);
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  color: var(--brand-berry);
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  transition: all 0.3s ease;
}

.btn-save:hover, .btn-delete:hover {
  background: var(--header-gradient);
  color: var(--color-white);
  transform: scale(1.1);
}
</style>


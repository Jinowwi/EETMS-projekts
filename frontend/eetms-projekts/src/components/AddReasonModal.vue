<template>
  <!-- Modālais logs tiek attēlots tikai tad, ja props isOpen ir true -->
  <div v-if="isOpen" class="modal-overlay" @click.self="$emit('close')">
    <div class="modal-content">

      <!-- Brīdinājuma paziņojums, kas parādās validācijas kļūdas gadījumā -->
      <div v-if="showWarning" class="warning-banner">
        <div class="warning-content">
          <span class="warning-icon">!</span>
          <span class="warning-text">{{ warningMessage }}</span>
          
          <!-- Poga brīdinājuma aizvēršanai -->
          <button class="warning-close" @click="showWarning = false">×</button>
        </div>
      </div>

      <!-- Modālā loga virsraksta daļa -->
      <div class="section-header">
        <h2>Add New Reason</h2>
      </div>

      <!-- Forma jauna iemesla pievienošanai -->
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

      <!-- Darbību pogas saglabāšanai vai aizvēršanai -->
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
// Importēt Vue funkcijas un ārējās bibliotēkas
import { ref, watch, onMounted } from 'vue'; 
import { FontAwesomeIcon } from '@fortawesome/vue-fontawesome';
import axios from 'axios';

// Definēt notikumus, ko komponents var izsūtīt pamata komponentam 
const emit = defineEmits(['close', 'save']); 

// Definēt ienākošos props
const props = defineProps({ isOpen: Boolean }); 

// Masīvs uzņēmumu datu glabāšanai
const companies = ref([]); 

// API pamatadrese datu pieprasījumiem
const API_BASE = 'http://localhost:5002/api'; 

// Mainīgie brīdinājuma ziņas attēlošanai
const showWarning = ref(false)
const warningMessage = ref('')
let warningTimer = null 

// Funkcija brīdinājuma ziņas parādīšanai uz noteiktu laiku
const showWarningMessage = (message) => {
  warningMessage.value = message 
  showWarning.value = true 

  // Ja iepriekšējais taimeris vēl darbojas, tas tiek notīrīts
  if (warningTimer) clearTimeout(warningTimer)
  
  // Brīdinājums pazūd pēc 3 sekundēm
  warningTimer = setTimeout(() => {
    showWarning.value = false 
    warningTimer = null
  }, 3000) 
}; 

// Formas dati jaunā iemesla izveidei
const form = ref({
    Name: '',
    CompanyID: null 
});

// Kad komponents tiek ielādēts, tiek pieprasīts uzņēmumu saraksts
onMounted(async () => {
    try {
        const res = await axios.get(`${API_BASE}/companies`);
        companies.value = res.data; 
    } catch (error) {
        console.error('Companies load failed:', error); 
    }
}); 

// Funkcija datu validācijai un datu saglabāšanai
const handleSave = () => {
  showWarning.value = false 
  warningMessage.value = ''

  // No formas iegūst iemesla nosaukumu bez atstarpēm
  const name = (form.value.Name ?? '').trim()
  
  // Pārbaudīt, vai lauks nav tukšs
  if (!name) {
    showWarningMessage("Please fill in all fields.");
    return; 
  } 

  // Pārbaudīt, vai iemesla nosaukumā ir vismaz 5 rakstzīmes
  if (name.length < 5) {
    showWarningMessage('Reason name must contain at least 5 characters.')
    return
  }

  // Izveidot jauna objekta struktūru saglabāšanai
  const newReason = {
    ReasonID: 0, 
    Name: form.value.Name, 
    CompanyID: parseInt(form.value.CompanyID), 
    Shifts: [],
    CompanyReasons: [] 
  }; 

  // Saglabāšanas notikums ar formas datiem
  emit('save', newReason); 
}; 

// Uzraudzīt modālā loga statusu 
// Atiestatīt formu, kad logs tiek aizvērts
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
/* Fona pārklājums aiz modālā loga */
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

/* Modālā loga galvenais konteiners */
.modal-content {
  font-family: 'Inter', sans-serif;
  background: var(--color-white);
  border-radius: 12px;
  max-width: 600px;
  width: 100%;
  position: relative;
  box-shadow: 0 4px 20px var(--shadow-berry);
  overflow: hidden; 
}

/* Galvenes noformējums */
.section-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  background: var(--header-gradient);
}

.section-header h2 {
  margin: 0;
  color: var(--color-white);
  font-family: 'Inter', sans-serif;
  font-size: 22px;
  font-weight: 600;
  padding: 15px 20px;
}

/* Brīdinājuma josla */
.warning-banner {
  position: fixed;
  top: 20px;
  right: 20px;
  z-index: 3000; 
  animation: slideIn 0.3s ease-out;
}

/* Brīdinājuma parādīšanās animācija */
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

/* Brīdinājuma satura bloka stils */
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

/* Brīdinājuma ikonas izskats */
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

/* Poga brīdinājuma aizvēršanai */
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

/* Formas satura konteiners */
.company-details {
  padding: 30px;
  background: var(--color-white);
}

/* Divu kolonnu izkārtojums */
.form-row {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 20px;
}

.form-group {
  margin-bottom: 20px;
}

/* Ievades lauku virsrakstu stils */
.form-group label {
  font-family: 'Inter', sans-serif;
  font-size: 14px;
  font-weight: 600;
  color: var(--brand-berry);
  margin-bottom: 8px;
  display: block;
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

/* Ievades un izvēlnes lauku pamata stils */
.edit-input, 
.edit-select {
  width: 100%; 
  height: 42px; 
  border: 2px solid var(--color-border); 
  border-radius: 40px;
  font-family: 'Inter', sans-serif;
  box-shadow: 0 2px 8px var(--shadow-berry);
  padding: 0 15px;
  box-sizing: border-box;
  transition: border-color 0.3s ease;
}

.edit-input:focus, .edit-select:focus {
  outline: none;
  border-color: var(--brand-berry);
}

/* Pogu zonas izkārtojums */
.modal-actions {
  display: flex;
  justify-content: flex-end;
  gap: 15px;
  padding: 0 30px 30px 30px;
}

/* Darbību pogu pamata noformējums */
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


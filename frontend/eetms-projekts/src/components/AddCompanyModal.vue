<template>
  <!-- Modālais logs tiek rādīts tikai tad, ja isOpen ir true -->
  <div v-if="isOpen" class="modal-overlay" @click.self="$emit('close')">
    <div class="modal-content">
      <!-- Brīdinājuma josla, kas parādās validācijas kļūdu gadījumā -->
      <div class="warning-banner" v-if="showWarning">
        <div class="warning-content">
          <span class="warning-icon">!</span>
          <span>{{ warningMessage }}</span>
          <!-- Poga brīdinājuma aizvēršanai -->
          <button class="warning-close" @click="showWarning = false">×</button>
        </div>
      </div>
      <!-- Modālā loga virsraksts -->
      <div class="section-header">
        <h2>Add New Company</h2>
      </div>

      <!-- Forma jauna uzņēmuma pievienošanai -->
      <form novalidate @submit.prevent="handleSave" class="company-details">
        <div class="form-group">
          <label>Company Name:</label>
          <input
            v-model.trim="form.companyName"
            class="edit-input"
            required
            maxlength="50"
            @keyup.enter="handleSave"
          />
        </div>

        <!-- Valsts izvēlēs saraksts -->
        <div class="form-row">
          <div class="form-group">
            <label>Country:</label>
            <select v-model="form.country" class="edit-select">
              <option :value="1">Lithuania</option>
              <option :value="2">Latvia</option>
              <option :value="3">Estonia</option>
            </select>
          </div>

          <!-- Talruņa numura ievades lauks -->
          <div class="form-group">
            <label>Phone Number:</label>
            <input
              v-model.trim="form.phoneNumber"
              minlength="8"
              class="edit-input"
              required
              maxlength="12"
              pattern="\\+?[0-9]+"
              @keyup.enter="handleSave"
            />
          </div>
        </div>

        <!-- Adreses ievades lauks -->
        <div class="form-group">
          <label>Address:</label>
          <input
            v-model.trim="form.address"
            minlength="10"
            class="edit-input"
            required
            maxlength="50"
            placeholder="Street, City, Postal Code"
            @keyup.enter="handleSave"
          />
        </div>  

        <!-- Registrācijas numurs -->
        <div class="form-group">
          <label>Registration Number:</label>
          <input
            v-model.trim="form.registrationNumber"
            minlength="9"
            class="edit-input"
            required
            maxlength="11"
            @keyup.enter="handleSave"
          />
        </div>

        <!-- E-pasta adrese -->
        <div class="form-group">
          <label>Email:</label>
          <input
            v-model.trim="form.email"
            type="email"
            class="edit-input"
            required
            maxlength="50"
            placeholder="company@example.com"
            @keyup.enter="handleSave"
          />
        </div>
      </form>

      <!-- Darbību pogas: saglabāt vai atcelt -->
      <div class="modal-actions">
        <button @click="handleSave" class="btn-save" title="Save Company">
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
// Importēt Vue funkcijas un FontAwesome ikonu komponenti
import { ref, watch } from 'vue';
import { FontAwesomeIcon } from '@fortawesome/vue-fontawesome';

// Definēt notikumus, ko komponents var izsūtīt pamata komponentam 
const emit = defineEmits(['close', 'save']);

// Definēt ienākošos parametrus
const props = defineProps({ isOpen: Boolean });

// Mainīgie brīdinājuma ziņas attēlošanai
const showWarning = ref(false);
const warningMessage = ref('');

// Taimeris, lai brīdinājums pazustu pēc noteikta laika
let warningTimer = null;

// Funkcija brīdinājuma ziņas parādīšanai
const showWarningMessage = (message) => {
  warningMessage.value = message;
  showWarning.value = true;
  
  // Ja iepriekšējais taimeris vēl darbojas, to notīra
  if (warningTimer) clearTimeout(warningTimer);
  
  // Paslēpj brīdinājumu pēc 3 sekundēm
  warningTimer = setTimeout(() => {
    showWarning.value = false;
  }, 3000);
};

// Formas dati
const form = ref({
  companyName: '',
  country: 2,
  phoneNumber: '',
  address: '',
  registrationNumber: '',
  email: '',
  remID: null 
});

// Funkcija datu validācijai un saglabāšanai
const handleSave = () => {
  showWarning.value = false;
  warningMessage.value = '';

  // Iegūt un apgrieZT ievadītās vērtības
  const companyName = (form.value.companyName ?? '').trim();
  const phone = (form.value.phoneNumber ?? '').trim();
  const address = (form.value.address ?? '').trim();
  const regNo = (form.value.registrationNumber ?? '').trim();
  const email = (form.value.email ?? '').trim();

  // PārbaudĪT, vai visi obligātie lauki ir aizpildīti
  if (!companyName || !phone || !address || !regNo || !email) {
    showWarningMessage('Please fill in all required fields.');
    return;
  }

  // Pārbaudīt, vai telefona numurā ir tikai cipari un izvēles + zīme sākumā
  if (!/^\+?\d+$/.test(phone)) {
    showWarningMessage('Phone number may contain only numbers.');
    return;
  }

  // Pārbaudīt minimālo garumu dažiem laukiem
  if (phone.length < 8 || address.length < 10 || regNo.length < 9) {
    showWarningMessage('Some fields are too short.');
    return;
  }

  // Pārbaudīt e-pasta formātu
  if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) {
    showWarningMessage('Please enter a valid email address.');
    return;
  }

  // Saglabāšanas notikums ar formas datiem
  emit('save', {
    companyName: companyName,
    address: address,
    registrationNumber: regNo,
    phoneNumber: phone,
    email: email,
    country: Number(form.value.country),
    remID: Number(form.value.remID)
  });
};

// Novērot modālā loga statusu
watch(() => props.isOpen, (val) => {
  // Kad modālais logs tiek aizvērts, 
  // notīrīt brīdinājumus un atiestatīt formu
  if (!val) {
    if (warningTimer) clearTimeout(warningTimer);
    showWarning.value = false;
    warningMessage.value = '';
    form.value = {
      companyName: '',
      phoneNumber: '',
      country: 2,
      address: '',
      registrationNumber: '',
      email: '',
      remID: null
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

/* Modālā loga konteiners */
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

/* Virsraksta josla */
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

/* Brīdinājuma joslas novietojums */
.warning-banner {
  position: fixed;
  top: 20px;
  right: 20px;
  z-index: 3000;
  animation: slideIn 0.3s ease-out;
}

/* Animācija brīdinājuma joslas parādīšanai */
@keyframes slideIn {
  from { transform: translateX(400px); opacity: 0; }
  to { transform: translateX(0); opacity: 1; }
}

/* Brīdinājuma saturs */
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

/* Brīdinājuma ikona */
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

/* Formas galvenais konteiners */
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

/* Lauku virsraksti */
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

/* Ievades lauki un select lauki */
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

.edit-input:focus,
.edit-select:focus {
  outline: none;
  border-color: var(--brand-berry);
}

/* Pogu zona modālā loga apakšā */
.modal-actions {
  display: flex;
  justify-content: flex-end;
  gap: 15px;
  padding: 0 30px 30px 30px;
}

/* Saglabāšanas un atcelšanas pogu pamata stils */
.btn-save,
.btn-delete {
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

.btn-save:hover,
.btn-delete:hover {
  background: var(--header-gradient);
  color: var(--color-white);
  transform: scale(1.1);
}
</style>
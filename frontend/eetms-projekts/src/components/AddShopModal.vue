<template>
  <!-- Modālais logs tiek rādīts tikai tad, ja isOpen ir true -->
  <div v-if="isOpen" class="modal-overlay" @click.self="$emit('close')">
    <div class="modal-content">
      
      <!-- Brīdinājuma josla tiek parādīta tikai tad, ja showWarning ir true -->
      <div class="warning-banner" v-if="showWarning">
        <div class="warning-content">
          <span class="warning-icon">!</span>
          <span>{{ warningMessage }}</span>
          <button class="warning-close" @click="showWarning = false">×</button>
        </div>
      </div>
      
      <!-- Loga virsraksts -->
      <div class="section-header">
        <h2>Add New Shop</h2>
      </div>

      <!-- Forma veikala datu ievadei -->
      <form novalidate @submit.prevent="handleSave" class="company-details">
        
        <!-- Veikala tipa izvēle -->
        <div class="form-group">
          <label>Shop Type:</label>
          <select v-model="form.type" class="edit-select" required>
            <option value="" disabled>Select shop type</option>
            <option value="Express">Express</option>
            <option value="Mini">Mini</option>
            <option value="Super">Super</option>
            <option value="Hyper">Hyper</option>
          </select>
        </div>

        <!-- Valsts izvēle -->
        <div class="form-row">
          <div class="form-group">
            <label>Country:</label>
            <select v-model="form.country" class="edit-select" required>
              <option value="" disabled>Select country</option>
              <option value="1">Lithuania</option>
              <option value="2">Latvia</option>
              <option value="3">Estonia</option>
            </select>
          </div>

          <!-- Veikala kods tiek ģenerēts automātiski -->
          <div class="form-group">
            <label>Shop Code:</label>
            <input 
              v-model.trim="form.code"
              class="edit-input"
              readonly
              placeholder="Auto-generated"
            />
          </div>
        </div>

        <!-- Adreses ievades lauks -->
        <div class="form-group">
          <label>Address:</label>
          <input
            v-model.trim="form.address"
            class="edit-input"
            required
            maxlength="100"
            minlength="10"
            placeholder="Street, City, Postal Code"
            @keyup.enter="handleSave"
          />
        </div>

        <!-- E-pasta ievades lauks -->
        <div class="form-group">
          <label>Email:</label>
          <input
            v-model.trim="form.email"
            class="edit-input"
            type="email"
            required
            placeholder="shop@example.com"
            @keyup.enter="handleSave"
          />
        </div>
      </form>
      
      <!-- Pogas saglabāšanai vai aizvēršanai -->
      <div class="modal-actions">
        <button @click="handleSave" class="btn-save" title="Save Shop">
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
// Importēt nepieciešamās Vue funkcijas un FontAwesome ikonu komponenti
import { ref, watch, computed } from 'vue'; 
import { FontAwesomeIcon } from '@fortawesome/vue-fontawesome';

// Definēt komponentes notikumus
const emit = defineEmits(['close', 'save']);

// Definēt ienākošos parametrus 
const props = defineProps({ isOpen: Boolean }); 

// Mainīgie brīdinājuma paziņojuma kontrolei
const showWarning = ref(false); 
const warningMessage = ref(''); 
let warningTimer = null; 

// Funkcija brīdinājuma ziņojuma parādīšanai uz 3 sekundēm
const showWarningMessage = (message) => {
  warningMessage.value = message;
  showWarning.value = true;
  
  // Ja jau ir iepriekšējs taimeris, notīrīt to
  if (warningTimer) clearTimeout(warningTimer); 
  
  // Pēc 3 sekundēm brīdinājumu paslēpt
  warningTimer = setTimeout(() => {
    showWarning.value = false;
  }, 3000);  
};

// Funkcija, kas atgriež tukšu formas objektu
const emptyForm = () => ({ type: '', country: 2, code: '', address: '', email: '' });

// Reaktīvais formas objekts
const form = ref(emptyForm());

// Valsts ID pārveidošana uz prefiksu valsts kodam
const countryPrefixMap = { '1': 'LT', '2': 'LV', '3': 'EE' };

// Automātiski ģenerēt veikala kodu, kad mainās valsts
watch(() => form.value.country, (newCountry) => {
  if (!newCountry) return;
  
  const prefix = countryPrefixMap[String(newCountry)];
  if (prefix) {
    form.value.code = prefix; 
  }
});

// Saglabāšanas funkcija
const handleSave = () => {
  // Notīrīt iepriekšējos brīdinājumus
  showWarning.value = false;
  warningMessage.value = '';

  // Iegūst formas vērtības un noņemt atstarpes
  const type    = (form.value.type    ?? '').trim();
  const country =  form.value.country;
  const code    = (form.value.code    ?? '').trim();
  const address = (form.value.address ?? '').trim();
  const email   = (form.value.email   ?? '').trim();

  // Pārbaudīt, vai visi obligātie lauki ir aizpildīti
  if (!type || !country || !address || !email) {
    showWarningMessage('Please fill in all required fields.');
    return;
  }

  // Pārbaudīt, vai adrese ir pietiekami gara
  if (address.length < 10) {
    showWarningMessage('Address should be at least 10 characters long.');
    return;
  }

  // Pārbaudīt e-pasta formātu 
  if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) {
    showWarningMessage('Please enter a valid email address.');
    return;
  }

  // Nosūtīt saglabāšanas notikumu pamata komponentam
  emit('save', {
    ShopID: 0,
    type:    type,
    country: Number(country),
    code:    code.toUpperCase(),
    address: address,
    email:   email  
  });
  
  // Pēc saglabāšanas notīrīt formu
  form.value = emptyForm();
};

// Uzraudzīt, vai modālais logs tiek aizvērts
watch(() => props.isOpen, (val) => {
  if (!val) {

    // Notīrīt taimeri un atiestatīt stāvokli
    if (warningTimer) clearTimeout(warningTimer); 
    showWarning.value = false;
    warningMessage.value = '';
    form.value = emptyForm();
  }
});
</script>

<style scoped>
/* Fons aiz modālā loga */
.modal-overlay {
  position: fixed;
  top: 0; left: 0; right: 0; bottom: 0;
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

/* Virsraksta teksts */
.section-header h2 {
  margin: 0;
  color: var(--color-white);
  font-family: 'Inter', sans-serif;
  font-size: 22px;
  font-weight: 600;
  padding: 15px 20px;
}

/* Brīdinājuma paziņojums */ 
.warning-banner {
  position: fixed;
  top: 20px; right: 20px;
  z-index: 3000; 
  animation: slideIn 0.3s ease-out;
}

/* Animācija brīdinājuma parādīšanai */
@keyframes slideIn {
  from { transform: translateX(400px); opacity: 0; }
  to   { transform: translateX(0);     opacity: 1; }
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
  box-shadow: 0 4px 12px rgba(0,0,0,0.15);
  min-width: 300px;
  max-width: 500px;
}

/* Izsaukuma zīmes ikona */
.warning-icon {
  background: var(--color-warning-border);
  color: var(--color-white);
  width: 24px; height: 24px;
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

.warning-close:hover { color: var(--color-black); }

/* Formas kopējais konteiners */
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

/* Atstarpe starp formas laukiem */
.form-group { margin-bottom: 20px; }

/* Etiķešu stils */
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

/* Ievades lauku un select kopīgais stils */
.edit-input, .edit-select {
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

/* Pogas loga apakšā */
.modal-actions {
  display: flex;
  justify-content: flex-end;
  gap: 15px;
  padding: 0 30px 30px 30px;
}

/* Saglabāšanas un atcelšanas pogu kopīgais stils */
.btn-save, .btn-delete {
  width: 45px; height: 45px;
  border: none;
  background: var(--color-white);
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  color: var(--brand-berry);
  box-shadow: 0 2px 8px rgba(0,0,0,0.1);
  transition: all 0.3s ease;
}

.btn-save:hover, .btn-delete:hover {
  background: var(--header-gradient);
  color: var(--color-white);
  transform: scale(1.1);
}
</style>
<template>
  <!-- Lapas galvenais konteineris -->
  <div class="set-password-page">
    <!-- Kaste ar formu un ziņojumiem -->
    <div class="set-password-box">
      <h2>Set Your Password</h2>
      <p class="subtitle">Create a password for your {{ accountLabel }} account.</p>

      <!-- Veiksmīgas paroles iestatīšanas ziņojums -->
      <div v-if="success" class="msg success">
        Password set! You can now <a :href="accountType === 'company' ? 
        '/login-comp' : '/login-shop'"> log in</a>.
      </div>

      <!-- Kļūdas paziņojums -->
      <div v-else-if="error" class="msg error">
        {{ error }}
      </div>

      <!-- Paroles ievades lauks -->
      <form v-else @submit.prevent="handleSubmit">
        <div class="field">
          <label>New Password</label>
          <input
            v-model="password"
            type="password"
            placeholder="Enter password"
            minlength="6"
            required
          />
        </div>
        <!-- Paroles apstiprinājuma ievades lauks -->
        <div class="field">
          <label>Confirm Password</label>
          <input
            v-model="confirmPassword"
            type="password"
            placeholder="Confirm password"
            required
          />
        </div>
        <!-- Paziņojums, ja ievādītas paroles nesakrīt -->
        <p v-if="mismatch" class="validation-error">Passwords do not match.</p>
        
        <!-- Apstriprinājuma poga -->
        <button type="submit" :disabled="loading">
          {{ loading ? 'Saving...' : 'Set Password' }}
        </button>
      </form>
    </div>
  </div>
</template>

<script setup>
// Importēt Vue funkcijas un maršrutēšanu 
import { ref, onMounted, computed } from 'vue';
import { useRoute } from 'vue-router';
import axios from 'axios';

// Iegūst pašreizējo maršrutu
const route = useRoute();

// Reaktīvie mainīgie formas datiem un stāvokļiem
const email = ref('');
const password = ref('');
const confirmPassword = ref('');
const loading = ref(false);
const success = ref(false);
const error = ref('');
const mismatch = ref(false);
const accountType = ref(''); 

// Kad komponents tiek ielādēts, nolasīt query parametrus no saites
onMounted(() => {
  email.value = route.query.email || '';
  accountType.value = route.query.type || ''; 

  // Ja nav e-pasta vai konta tipa, rāda kļūdu
  if (!email.value || !accountType.value) {
    error.value = 'Invalid or expired link.';
  }
});

// Izvēlēta konta nosaukuma noteikšana 
const accountLabel = computed(() => 
  accountType.value === 'company' ? 'company' : 'shop' 
); 

// Funkcija paroles saglabāšanai
const handleSubmit = async () => {
  mismatch.value = false;

  // Pārbaude, vai abas paroles sakrīt 
  if (password.value !== confirmPassword.value) {
    mismatch.value = true;
    return;
  }

  loading.value = true;

  try {
    // Izvēlēties pareizo API endpointu, atkārībā no konta tipa 
    const endpoint = 
      accountType.value === 'company'
      ? 'http://localhost:5001/api/companies/set-password'
      : 'http://localhost:5001/api/shops/set-password'; 

    // Atsūtīt e-pasta adresi un jauno paroli uz serveri 
    await axios.post(endpoint, {
      email: email.value,
      password: password.value
    });
    // Ja pieprasījums ir veiksmīgs, parādīt veiksmīgu ziņojumu 
    success.value = true;
  } catch (err) {
    error.value = err.response?.data || 'Something went wrong. Please try again.';
  } finally {
    // Pabeigt ielādes stāvokli 
    loading.value = false;
  }
};
</script>

<style scoped>
/* Galvenais lapas izkārtojums */
.set-password-page {
  min-height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  background: #f7f6f2;
  font-family: 'Inter', sans-serif;
}

/* Forma centrālajā kastē */
.set-password-box {
  background: white;
  padding: 2rem;
  border-radius: 12px;
  box-shadow: 0 4px 24px rgba(0,0,0,0.08);
  width: 100%;
  max-width: 400px;
}

/* Galvenais virsraksts */
h2 { 
  margin-bottom: 0.25rem; 
  color: #1a1a1a; 
}

/* Apakšvirsraksts */
.subtitle { 
  color: #888; 
  margin-bottom: 1.5rem; 
  font-size: 0.9rem; 
}

/* Formas lauku noformējums */
.field { 
  margin-bottom: 1rem; 
}

.field label { 
  display: block; 
  margin-bottom: 0.4rem; 
  font-size: 0.875rem; 
  font-weight: 500; 
}

/* Ievades lauka stils */
.field input {
  width: 100%;
  padding: 0.6rem 0.75rem;
  border: 1px solid #ddd;
  border-radius: 8px;
  font-size: 1rem;
  outline: none;
  transition: border-color 0.2s;
}

.field input:focus { 
  border-color: #a12971; 
}

/* Galvenās pogas stils */
button {
  width: 100%;
  padding: 0.75rem;
  background: #a12971;
  color: white;
  border: none;
  border-radius: 24px;
  font-size: 1rem;
  font-weight: 600;
  cursor: pointer;
  margin-top: 0.5rem;
  transition: background 0.2s;
}

/* Pogas hover efekts */
button:hover { 
  background: #8a1f5e; 
}

/* Poga izslēgtā stāvoklī */
button:disabled { 
  opacity: 0.6; 
  cursor: not-allowed; 
}

/* Validācijas kļūdas teksts */
.validation-error { 
  color: #a12971; 
  font-size: 0.85rem; 
  margin-top: -0.5rem; 
  margin-bottom: 0.75rem; 
}

/* Kopīgais stils ziņojumiem */
.msg { 
  padding: 1rem; 
  border-radius: 8px; 
  margin-bottom: 1rem; 
}

/* Veiksmīgs ziņojums */
.msg.success { 
  background: #e8f5e9; 
  color: #2e7d32; 
}

/* Kļūdas ziņojums */
.msg.error { 
  background: #fce4ec; 
  color: #c62828; 
}

/* Saite ziņojumā */
.msg a { 
  color: #a12971; 
  font-weight: 600; 
}
</style>
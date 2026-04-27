<template>
  <div class="set-password-page">
    <div class="set-password-box">
      <h2>Set Your Password</h2>
      <p class="subtitle">Create a password for your shop account.</p>

      <div v-if="success" class="msg success">
        Password set! You can now <a href="/login-shop">log in</a>.
      </div>

      <div v-else-if="error" class="msg error">
        {{ error }}
      </div>

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
        <div class="field">
          <label>Confirm Password</label>
          <input
            v-model="confirmPassword"
            type="password"
            placeholder="Confirm password"
            required
          />
        </div>
        <p v-if="mismatch" class="validation-error">Passwords do not match.</p>
        <button type="submit" :disabled="loading">
          {{ loading ? 'Saving...' : 'Set Password' }}
        </button>
      </form>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue';
import { useRoute } from 'vue-router';
import axios from 'axios';

const route = useRoute();

const email = ref('');
const password = ref('');
const confirmPassword = ref('');
const loading = ref(false);
const success = ref(false);
const error = ref('');
const mismatch = ref(false);

onMounted(() => {
  email.value = route.query.email || '';
  if (!email.value) {
    error.value = 'Invalid or expired link.';
  }
});

const handleSubmit = async () => {
  mismatch.value = false;

  if (password.value !== confirmPassword.value) {
    mismatch.value = true;
    return;
  }

  loading.value = true;
  try {
    await axios.post('http://localhost:5001/api/shops/set-password', {
      email: email.value,
      password: password.value
    });
    success.value = true;
  } catch (err) {
    error.value = err.response?.data || 'Something went wrong. Please try again.';
  } finally {
    loading.value = false;
  }
};
</script>

<style scoped>
.set-password-page {
  min-height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  background: #f7f6f2;
  font-family: 'Inter', sans-serif;
}
.set-password-box {
  background: white;
  padding: 2rem;
  border-radius: 12px;
  box-shadow: 0 4px 24px rgba(0,0,0,0.08);
  width: 100%;
  max-width: 400px;
}
h2 { margin-bottom: 0.25rem; color: #1a1a1a; }
.subtitle { color: #888; margin-bottom: 1.5rem; font-size: 0.9rem; }
.field { margin-bottom: 1rem; }
.field label { display: block; margin-bottom: 0.4rem; font-size: 0.875rem; font-weight: 500; }
.field input {
  width: 100%;
  padding: 0.6rem 0.75rem;
  border: 1px solid #ddd;
  border-radius: 8px;
  font-size: 1rem;
  outline: none;
  transition: border-color 0.2s;
}
.field input:focus { border-color: #a12971; }
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
button:hover { background: #8a1f5e; }
button:disabled { opacity: 0.6; cursor: not-allowed; }
.validation-error { color: #a12971; font-size: 0.85rem; margin-top: -0.5rem; margin-bottom: 0.75rem; }
.msg { padding: 1rem; border-radius: 8px; margin-bottom: 1rem; }
.msg.success { background: #e8f5e9; color: #2e7d32; }
.msg.error { background: #fce4ec; color: #c62828; }
.msg a { color: #a12971; font-weight: 600; }
</style>
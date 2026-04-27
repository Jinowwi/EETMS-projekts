<template>
  <div class="login-wrapper">
    <div class="blob blob-teal"></div>
    <div class="blob blob-pink"></div>

    <div class="login-card">
      <button class="back-btn" @click="router.push('/')">
        <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none"
          stroke="currentColor" stroke-width="2.5" width="15" height="15">
          <path d="M19 12H5M12 5l-7 7 7 7"/>
        </svg>
        Back
      </button>

      <h2 class="login-title">Welcome back</h2>
      <p class="login-sub">Sign in to your EETMS shop director account</p>

      <form @submit.prevent="handleLogin">
        <div class="input-group">
          <label>Email</label>
          <input v-model="email" type="email" placeholder="example@rimibaltic.com" required />
        </div>

        <div class="input-group">
          <label>Password</label>
          <div class="password-wrapper">
            <input
              v-model="password"
              :type="showPassword ? 'text' : 'password'"
              placeholder="Enter your password"
              required autocomplete="current-password"
            />
            <button type="button" class="toggle-pw" @click="showPassword = !showPassword">
              <svg v-if="!showPassword" xmlns="http://www.w3.org/2000/svg" width="18" height="18"
                viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z"/>
                <circle cx="12" cy="12" r="3"/>
              </svg>
              <svg v-else xmlns="http://www.w3.org/2000/svg" width="18" height="18"
                viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <path d="M17.94 17.94A10.07 10.07 0 0 1 12 20c-7 0-11-8-11-8a18.45 18.45 0 0 1 5.06-5.94"/>
                <path d="M9.9 4.24A9.12 9.12 0 0 1 12 4c7 0 11 8 11 8a18.5 18.5 0 0 1-2.16 3.19"/>
                <line x1="1" y1="1" x2="23" y2="23"/>
              </svg>
            </button>
          </div>
        </div>

        <p v-if="error" class="error">{{ error }}</p>

        <button type="submit" :disabled="loading" class="login-btn">
          <span v-if="loading" class="btn-spinner"></span>
          <span v-else>Login</span>
        </button>
      </form>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'

const router = useRouter()
const API_BASE = 'http://localhost:5001/api'

const email = ref('')
const password = ref('')

const showPassword = ref(false)
const error = ref('')
const successMessage = ref('')
const loading = ref(false)

const handleLogin = async () => {
  error.value = '';
  loading.value = true;

  try {
    const res = await fetch(`${API_BASE}/shops/login`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ email: email.value, password: password.value })
    });

    const data = await res.json();

    if (!res.ok) {
      error.value = data.error || 'Invalid email or password.';
      return;
    }

    localStorage.setItem('shopId', data.shopID);
    router.push('/shop-home');
  } catch (e) {
    console.error(e);
    error.value = 'A network error occurred. Is the API running?';
  } finally {
    loading.value = false;
  }
};
</script>

<style scoped>
.login-wrapper {
  min-height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  background: var(--background-color);
  font-family: 'Inter', sans-serif;
  position: relative;
  overflow: hidden;
}

.blob {
  position: absolute;
  border-radius: 50%;
  filter: blur(200px);
  opacity: 0.65;
  pointer-events: none;
}

.blob-teal {
  width: 1050px;
  height: 1050px;
  background: var(--brand-teal);
  top: -350px;
  left: -350px;
}

.blob-pink {
  width: 950px;
  height: 950px;
  background: var(--brand-berry);
  bottom: -320px;
  right: -320px;
}

.login-card {
  position: relative;
  z-index: 1;
  background: rgba(255, 255, 255, 0.35);
  backdrop-filter: blur(24px);
  -webkit-backdrop-filter: blur(24px);
  border: 1.5px solid rgba(255, 255, 255, 0.6);
  border-radius: 20px;
  padding: 48px 40px 40px;
  width: 100%;
  max-width: 440px;
  box-shadow: 0 8px 32px rgba(0, 0, 0, 0.1);
  display: flex;
  flex-direction: column;
  align-items: center;
}

input[type="password"]::-ms-reveal,
input[type="password"]::-ms-clear,
input::-webkit-credentials-auto-fill-button {
  display: none;
}

.back-btn {
  position: absolute;
  top: 20px;
  left: 24px;
  display: flex;
  align-items: center;
  gap: 5px;
  font-size: 13px;
  font-family: 'Inter', sans-serif;
  color: #6b7280;
  background: none;
  border: none;
  cursor: pointer;
  padding: 0;
  transition: color 0.15s;
}

.back-btn:hover {
  color: var(--brand-teal);
}

.login-title {
  font-family: 'Inter', sans-serif;
  font-size: 26px;
  font-weight: 800;
  background: var(--brand-gradient);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  background-clip: text;
  margin: 0 0 6px;
  text-align: center;
}

.login-sub {
  font-family: 'Inter', sans-serif;
  font-size: 14px;
  color: var(--color-text-dim);
  margin: 0 0 32px;
  text-align: center;
}

form {
  width: 100%;
}

.input-group {
  margin-bottom: 20px;
}

.input-group label {
  display: block;
  font-family: 'Inter', sans-serif;
  font-size: 13px;
  font-weight: 600;
  color: var(--color-text-main);
  margin-bottom: 8px;
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

.input-group input {
  width: 100%;
  padding: 12px 16px;
  border: 2px solid rgba(255, 255, 255, 0.7);
  border-radius: 12px;
  font-family: 'Inter', sans-serif;
  font-size: 14px;
  color: var(--color-text-main);
  background: rgba(255, 255, 255, 0.6);
  backdrop-filter: blur(12px);
  transition: border-color 0.2s ease, box-shadow 0.2s ease;
  box-sizing: border-box;
  outline: none;
}

.input-group input:focus {
  border-color: var(--brand-teal, #2ba492);
  box-shadow: 0 0 0 3px rgba(43, 164, 146, 0.15);
  background: rgba(255, 255, 255, 0.82);
}

.input-group input::placeholder {
  color: #aaa;
}

.password-wrapper {
  position: relative;
}

.password-wrapper input {
  padding-right: 44px;
}

.toggle-pw {
  position: absolute;
  right: 12px;
  top: 50%;
  transform: translateY(-50%);
  background: none;
  border: none;
  cursor: pointer;
  color: #9ca3af;
  display: flex;
  align-items: center;
  padding: 0;
  transition: color 0.15s;
}

.toggle-pw:hover {
  color: var(--brand-teal);
}

.error {
  font-family: 'Inter', sans-serif;
  font-size: 13px;
  color: #e53e3e;
  background: rgba(255, 245, 245, 0.8);
  border: 1px solid #fed7d7;
  border-radius: 8px;
  padding: 10px 14px;
  margin-bottom: 16px;
  text-align: center;
}

.login-btn {
  width: 100%;
  padding: 14px;
  background: var(--header-gradient, linear-gradient(135deg, #a12971, #2ba492));
  color: white;
  border: none;
  border-radius: 50px;
  font-family: 'Inter', sans-serif;
  font-size: 15px;
  font-weight: 700;
  cursor: pointer;
  transition: all 0.3s ease;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
  margin-top: 8px;
  box-shadow: 0 4px 16px rgba(161, 41, 113, 0.3);
}

.login-btn:hover:not(:disabled) {
  transform: translateY(-2px);
  box-shadow: 0 6px 20px rgba(161, 41, 113, 0.4);
}

.login-btn:active:not(:disabled) {
  transform: translateY(0);
}

.login-btn:disabled {
  opacity: 0.7;
  cursor: not-allowed;
  transform: none;
}

.btn-spinner {
  width: 18px;
  height: 18px;
  border: 2px solid rgba(255, 255, 255, 0.4);
  border-top-color: white;
  border-radius: 50%;
  animation: spin 0.7s linear infinite;
  display: inline-block;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}

@media (max-width: 480px) {
  .login-card {
    margin: 20px;
    padding: 36px 24px;
  }
}

@media (min-width: 1024px) and (max-width: 1280px) {
  .blob-teal {
    width: 850px;
    height: 850px;
  }

  .blob-pink {
    width: 750px;
    height: 750px;
  }
}
</style>
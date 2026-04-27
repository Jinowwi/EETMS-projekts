<template>
  <div class="role-page">
    <div class="blob blob-teal"></div>
    <div class="blob blob-pink"></div>

    <div class="role-container">
      <div class="role-cards">
        <button class="role-card" @click="navigateTo('/login')">
          <div class="role-icon teal-icon">
            <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="currentColor" width="22" height="22">
              <path d="M12 2a5 5 0 1 1 0 10A5 5 0 0 1 12 2zm0 12c5.33 0 8 2.67 8 4v2H4v-2c0-1.33 2.67-4 8-4z"/>
            </svg>
          </div>
          <span class="role-name">RIMI Administration</span>
        </button>

        <button class="role-card" @click="navigateTo('/login-comp')">
          <div class="role-icon grey-icon">
            <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none"
              stroke="currentColor" stroke-width="1.8" width="22" height="22">
              <rect x="2" y="7" width="20" height="14" rx="2"/>
              <path d="M16 7V5a2 2 0 0 0-2-2h-4a2 2 0 0 0-2 2v2"/>
              <line x1="12" y1="12" x2="12" y2="16"/>
              <line x1="10" y1="14" x2="14" y2="14"/>
            </svg>
          </div>
          <span class="role-name">External Company representative</span>
        </button>

        <button class="role-card" @click="navigateTo('/login-shop')">
          <div class="role-icon grey-icon">
            <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none"
              stroke="currentColor" stroke-width="1.8" width="22" height="22">
              <path d="M3 9l1-5h16l1 5"/>
              <path d="M3 9a2 2 0 0 0 2 2 2 2 0 0 0 2-2 2 2 0 0 0 2 2 2 2 0 0 0 2-2 2 2 0 0 0 2 2 2 2 0 0 0 2-2"/>
              <path d="M5 11v9h14v-9"/>
              <path d="M10 15h4v5h-4z"/>
            </svg>
          </div>
          <span class="role-name">Shop director</span>
        </button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { useRouter } from 'vue-router';

const router = useRouter();

const navigateTo = (path) => {
    router.push(path);
};
</script>

<style scoped>
.role-page {
  padding-top: 0;
  min-height: 100vh;
  background: var(--background-color);
  display: flex;
  align-items: center;
  justify-content: center;
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

.role-container {
  position: relative;
  z-index: 1;
  display: flex;
  flex-direction: column;
  align-items: center;
}

.role-cards {
  display: flex;
  flex-direction: column;
  gap: 12px;
  background: rgba(255, 255, 255, 0.35);
  backdrop-filter: blur(24px);
  -webkit-backdrop-filter: blur(24px);
  border: 1.5px solid rgba(255, 255, 255, 0.6);
  border-radius: 20px;
  padding: 20px;
  box-shadow: 0 8px 32px rgba(0, 0, 0, 0.1);
  width: 440px;
}

.role-card {
  display: flex;
  align-items: center;
  gap: 14px;
  padding: 30px 18px;
  background: rgba(255, 255, 255, 0.6);
  backdrop-filter: blur(12px);
  -webkit-backdrop-filter: blur(12px);
  border: 1.5px solid rgba(255, 255, 255, 0.85);
  border-radius: 14px;
  cursor: pointer;
  text-align: left;
  transition: all 0.2s ease;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.05);
  width: 100%;
}

.role-card:hover {
  background: rgba(255, 255, 255, 0.82);
  transform: translateY(-1px);
  box-shadow: 0 6px 20px rgba(0, 0, 0, 0.1);
}

.role-icon {
  width: 40px;
  height: 40px;
  border-radius: 10px;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.teal-icon {
  background: rgba(43, 164, 146, 0.15);
  color: var(--brand-teal);
}

.grey-icon {
  background: rgba(43, 164, 146, 0.15);
  color: var(--brand-teal);
}

.role-name {
  font-size: 20px;
  font-weight: bold;
  color: var(--brand-teal);
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

  .role-cards {
    width: 340px;
  }
}
</style>

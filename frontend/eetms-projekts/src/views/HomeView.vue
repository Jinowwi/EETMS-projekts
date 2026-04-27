<template>
  <div class="home">
    <div class="blob blob-teal"></div>
    <div class="blob blob-pink"></div>

    <button class="logout-btn" @click="handleLogout">
      <FontAwesomeIcon icon="fa-solid fa-right-from-bracket" />
      Logout
    </button>

    <div class="page-center">
      <div class="hero">
        <h1 class="hero-title">Welcome to EETMS!</h1>
        <p class="hero-sub">Easy tool to manage your external employees.</p>
      </div>

      <div class="cards-row">
        <button @click="navigateTo('/list')" class="info-card">
          <div class="card-icon">
            <FontAwesomeIcon icon="fa-solid fa-list" />
          </div>
          <span class="card-label">Tables</span>
          <span class="card-desc">View and manage companies, employees, reasons & shifts.</span>
        </button>

        <button @click="navigateTo('/calendar')" class="info-card">
          <div class="card-icon">
            <FontAwesomeIcon icon="fa-solid fa-calendar" />
          </div>
          <span class="card-label">Calendar</span>
          <span class="card-desc">Access and manage missed shiftpunches.</span>
        </button>

        <button @click="navigateTo('/reminders')" class="info-card">
          <div class="card-icon">
            <FontAwesomeIcon icon="fa-solid fa-message" />
          </div>
          <span class="card-label">Notifications</span>
          <span class="card-desc">Access notifications about missed punches and unasigned companies.</span>
        </button>

        <button @click="navigateTo('/charts')" class="info-card">
          <div class="card-icon">
            <FontAwesomeIcon icon="fa-solid fa-chart-area" />
          </div>
          <span class="card-label">Statistics</span>
          <span class="card-desc">View differend kinds of shift statistics.</span>
        </button>

        <button @click="navigateTo('/hometab')" class="info-card">
          <div class="card-icon">
            <FontAwesomeIcon icon="fa-solid fa-tablet-screen-button" />
          </div>
          <span class="card-label">Tablet</span>
          <span class="card-desc">Switch to the Tablet version.</span>
        </button>
      </div>
    </div>

  </div>
</template>

<script setup>
import { useRouter } from 'vue-router';
import { logout } from '@/services/auth.js';
import { FontAwesomeIcon } from '@fortawesome/vue-fontawesome';

const router = useRouter();
const navigateTo = (path) => router.push(path);
const handleLogout = () => {
  logout();
  router.push('/roleselect');
};
</script>

<style scoped>
.home {
  height: 100vh;
  position: fixed;
  inset: 0;
  overflow: hidden;
  display: flex;
  align-items: center;
  justify-content: center;
}

/* ── Center wrapper ── */
.page-center {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 48px;
  z-index: 10;
  width: 100%;
  padding: 0 40px;
}

/* ── Hero ── */
.hero {
  text-align: center;
}

.hero-title {
  font-family: 'Inter', sans-serif;
  font-size: clamp(28px, 4vw, 52px);
  font-weight: 800;
  background: var(--brand-gradient);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  background-clip: text;
  letter-spacing: -1px;
  margin: 0;
}

.hero-sub {
  font-family: 'Inter', sans-serif;
  font-size: clamp(14px, 1.5vw, 18px);
  color: var(--color-text-dim);
  margin-top: 8px;
  font-weight: 400;
}

/* ── Cards row ── */
.cards-row {
  display: flex;
  flex-direction: row;
  gap: 20px;
  flex-wrap: wrap;
  justify-content: center;
  align-items: stretch;
}

/* ── Glossy card ── */
.info-card {
  position: relative;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: flex-start;
  gap: 12px;
  width: 160px;
  padding: 28px 20px 24px;
  border-radius: 28px;
  border: 1.5px solid rgba(255, 255, 255, 0.6);
  cursor: pointer;
  font-family: 'Inter', sans-serif;
  text-align: center;
  overflow: hidden;
  transition: transform 0.3s ease, box-shadow 0.3s ease, background 0.3s ease;

  /* Glassmorphism base */
  background: linear-gradient(
    160deg,
    rgba(255, 255, 255, 0.75) 0%,
    rgba(255, 255, 255, 0.35) 100%
  );
  backdrop-filter: blur(16px);
  -webkit-backdrop-filter: blur(16px);
  box-shadow:
    0 8px 32px rgba(160, 50, 120, 0.10),
    inset 0 1px 0 rgba(255, 255, 255, 0.9);
  color: var(--brand-berry);
}

/* Glossy top shine */
.info-card::before {
  content: '';
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  height: 50%;
  background: linear-gradient(
    180deg,
    rgba(255, 255, 255, 0.55) 0%,
    rgba(255, 255, 255, 0.0) 100%
  );
  border-radius: 28px 28px 0 0;
  pointer-events: none;
}

/* Shimmer sweep on hover */
.info-card::after {
  content: '';
  position: absolute;
  top: 0;
  left: -100%;
  width: 60%;
  height: 100%;
  background: linear-gradient(
    120deg,
    transparent 0%,
    rgba(255, 255, 255, 0.45) 50%,
    transparent 100%
  );
  transition: left 0.6s ease;
  pointer-events: none;
}

.info-card:hover::after {
  left: 160%;
}

.info-card:hover {
  transform: translateY(-6px) scale(1.03);
  box-shadow:
    0 16px 40px rgba(160, 50, 120, 0.18),
    inset 0 1px 0 rgba(255, 255, 255, 0.9);
  background: linear-gradient(
    160deg,
    rgba(255, 255, 255, 0.88) 0%,
    rgba(255, 255, 255, 0.5) 100%
  );
}

.info-card:active {
  transform: translateY(-2px) scale(1.01);
  transition: all 0.1s ease;
}

/* ── Icon circle ── */
.card-icon {
  width: 52px;
  height: 52px;
  border-radius: 16px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 22px;
  background: linear-gradient(
    135deg,
    rgba(255, 255, 255, 0.9) 0%,
    rgba(255, 255, 255, 0.5) 100%
  );
  box-shadow:
    0 4px 12px rgba(160, 50, 120, 0.12),
    inset 0 1px 0 rgba(255, 255, 255, 1);
  color: var(--brand-berry);
  transition: background 0.3s ease, transform 0.3s ease;
  flex-shrink: 0;
}

.info-card:hover .card-icon {
  background: linear-gradient(
    135deg,
    rgba(255, 255, 255, 1) 0%,
    rgba(255, 255, 255, 0.7) 100%
  );
  transform: scale(1.1);
}

.card-label {
  font-size: clamp(13px, 1.1vw, 15px);
  font-weight: 700;
  color: var(--brand-teal);
}

.card-desc {
  font-size: clamp(10px, 0.85vw, 12px);
  font-weight: 300;
  color: var(--color-text-dim);
  line-height: 1.4;
}

/* ── Logout ── */
.logout-btn {
  position: fixed;
  top: 24px;
  right: 80px;
  z-index: 200;
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 10px 20px;
  background: white;
  color: var(--brand-berry);
  border: 2px solid var(--brand-berry);
  border-radius: 50px;
  font-family: Inter;
  font-size: 14px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s ease;
  box-shadow: 0 4px 12px var(--shadow-box);
}

.logout-btn:hover {
  background: var(--brand-berry);
  color: white;
  transform: translateY(-2px);
}

/* ── Brand logo ── */
.brand-logo {
  position: fixed;
  bottom: 40px;
  right: 75px;
  z-index: 100;
  opacity: 0.9;
  transition: all 0.3s ease;
}

.brand-logo:hover {
  opacity: 1;
  transform: scale(1.05);
}

.brand-logo img {
  width: 200px;
  height: 80px;
  object-fit: contain;
}

/* ── Responsive: tablets ── */
@media (max-width: 1023px) {
  .cards-row { gap: 14px; }
  .info-card { width: 140px; padding: 22px 16px; }
}

/* ── Responsive: mobile ── */
@media (max-width: 767px) {
  .home {
    position: relative;
    overflow-y: auto;
    max-height: none;
    min-height: 100vh;
  }
  .page-center { padding: 100px 20px 80px; gap: 32px; }
  .cards-row { gap: 12px; }
  .info-card { width: 130px; padding: 20px 14px; border-radius: 22px; }
  .card-icon { width: 44px; height: 44px; font-size: 18px; border-radius: 13px; }
  .logout-btn { right: 20px; }
  .brand-logo { right: 20px; bottom: 20px; }
  .brand-logo img { width: 120px; height: 50px; }
}
</style>
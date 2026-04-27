<template>
  <button class="logout-btn" @click="handleLogout">
        <FontAwesomeIcon icon="fa-solid fa-right-from-bracket" />
        Logout
      </button>
  <div class="page-content">
    <div class="blob blob-teal"></div>
    <div class="blob blob-pink"></div>

    <div class="home-header">
      <div class="welcome-wrapper">
        <h1 class="welcome-title">Welcome to EETMS!</h1>
      </div>
    </div>

    <div class="home-grid">
      <div class="widget widget-notifications">
        <h2 class="widget-title widget-title-link" @click="router.push('/reminders')">
          <span class="title-icon">
            <FontAwesomeIcon icon="fa-solid fa-message" />
          </span>
          <span class="title-text">Notifications<span class="title-arrow">»</span></span>
        </h2>
        <div class="notifications-list">
          <div
            v-for="r in reminders"
            :key="r.shiftID ?? r.ShiftID"
            class="reminder-card"
          >
            <div class="reminder-icon">
              <FontAwesomeIcon :icon="['fas', 'triangle-exclamation']" />
            </div>
            <div class="reminder-content">
              <p class="reminder-header">{{ formatDate(r.startDate || r.endDate) }}</p>
              <p class="reminder-body">
                Start: {{ r.startTime ? String(r.startTime).slice(0,5) : 'Missed' }},
                End: {{ r.endTime ? String(r.endTime).slice(0,5) : 'Missed' }}
              </p>
              <p class="reminder-nav" @click="goToCalendar(r)">To Calendar</p>
            </div>
          </div>
          <div v-if="reminders.length === 0" class="no-more">No more notifications</div>
        </div>
      </div>

      <div class="widget widget-calendar">
        <h2 class="widget-title widget-title-link" @click="router.push('/calendar')">
          <span class="title-icon">
            <FontAwesomeIcon icon="fa-solid fa-calendar" />
          </span>
          <span class="title-text">Calendar<span class="title-arrow">»</span></span>
        </h2>
        <Calendar :missedPunches="shifts" @punchClick="handlePunchClick" />
      </div>

      <div class="widget widget-companies">
        <h2 class="widget-title">
            <span class="title-icon">
                <FontAwesomeIcon icon="fa-solid fa-list" />
            </span>
            Your companies
        </h2>
        <div class="companies-list">
          <div
            v-for="company in companies"
            :key="company.id"
            class="company-row"
          >
            <p class="company-name">{{ company.companyName }}</p>
            <p class="company-sub">{{ company.address }}</p>
          </div>
        </div>
        <button class="view-stats-btn" @click="router.push('/charts')">View stats</button>
      </div>

      

    </div>

    <MissedPunchModal
      :isOpen="modalOpen"
      :punchData="selectedPunch"
      @close="modalOpen = false"
      @save="handleSave"
    />
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { FontAwesomeIcon } from '@fortawesome/vue-fontawesome'
import Calendar from '@/components/calendar.vue'
import MissedPunchModal from '@/components/MissedPunchModal.vue'
import { logout } from '@/services/auth.js'
import { getAdminRoleLevel, getAdmin } from '@/services/auth.js'

const roleLevel = Number(getAdminRoleLevel())
const currentAdmin = getAdmin()

const router = useRouter()
const API_BASE_REM = 'http://localhost:5001/api'
const API_BASE_ADMIN = 'http://localhost:5001/api'

const reminders = ref([])
const shifts = ref([])
const companies = ref([])
const modalOpen = ref(false)
const selectedPunch = ref({})

const getMyRemId = () => {
  if (currentAdmin && currentAdmin.remId) {
    return Number(currentAdmin.remId);
  }
  const storedId = localStorage.getItem('remId');
  return storedId ? Number(storedId) : 1; 
};

const fetchCompanies = async () => {
  const res = await fetch(`${API_BASE_ADMIN}/companies`)
  if (!res.ok) return []
  
  let data = await res.json()
  
  if (roleLevel === 1) {
    const myRemId = getMyRemId();
    data = data.filter(c => c.remID === myRemId);
  }
  
  companies.value = data.map(c => ({
    id: c.companyID,
    companyName: c.companyName,
    address: c.address,
  }))
  
  return companies.value.map(c => c.companyName); 
}

const fetchShifts = async (myCompanyNames = []) => {
  const res = await fetch(`${API_BASE_REM}/shifts`)
  if (res.ok) {
    let data = await res.json()
    
    if (roleLevel === 1 && myCompanyNames.length > 0) {
      data = data.filter(shift => {
        return myCompanyNames.includes(shift.companyName);
      }); 
    }
    
    shifts.value = data.map(shift => ({
      ...shift,
      date: shift.startDate,
      type: !shift.startTime ? 'start' : !shift.endTime ? 'end' : null,
    }))
  }
}

const fetchReminders = async (myCompanyNames = []) => {
  const res = await fetch(`${API_BASE_REM}/shifts/reminders`)
  if (res.ok) {
    let data = await res.json()
    
    if (roleLevel === 1 && myCompanyNames.length > 0) {
      data = data.filter(r => {
        return myCompanyNames.includes(r.companyName);
      });
    }
    reminders.value = data;
  }
}

onMounted(async () => {
  const myCompanyNames = await fetchCompanies()
  
  await fetchShifts(myCompanyNames) 
  await fetchReminders(myCompanyNames)
})

const handleLogout = () => {
  logout();
  router.push('/roleselect');
};

const goToCalendar = (r) => {
  router.push({ path: '/calendar', query: { date: r.startDate || r.endDate } })
}

const handlePunchClick = (punchData) => {
  selectedPunch.value = punchData
  modalOpen.value = true
}

const toHHMMSS = (hhmm) => (hhmm && hhmm.length === 5 ? `${hhmm}:00` : hhmm)

const handleSave = async (updatedShift) => {
  const id = updatedShift.ShiftID || updatedShift.shiftID
  const patch = []
  if (selectedPunch.value.type === 'start') {
    patch.push({ op: 'replace', path: '/start_time', value: toHHMMSS(updatedShift.startTime) })
  } else if (selectedPunch.value.type === 'end') {
    if (updatedShift.EndDate) patch.push({ op: 'replace', path: '/end_date', value: updatedShift.EndDate })
    patch.push({ op: 'replace', path: '/end_time', value: toHHMMSS(updatedShift.endTime) })
  }
  const res = await fetch(`${API_BASE_REM}/shifts/${id}`, {
    method: 'PATCH',
    headers: { 'Content-Type': 'application/json-patch+json' },
    body: JSON.stringify(patch),
  })
  if (res.ok) {
    modalOpen.value = false
    await fetchShifts(companies.value.map(c => c.companyName))
  }
}

const formatDate = (dateLike) => {
  if (!dateLike) return ''
  const d = new Date(String(dateLike).trim().replaceAll('/home', '-'))
  if (isNaN(d.valueOf())) return String(dateLike)
  return d.toLocaleDateString('de-DE', { day: '2-digit', month: '2-digit', year: 'numeric' })
}
</script>

<style scoped>
.page-content {
  padding: 32px 32px;
  min-height: 100vh;
  box-sizing: border-box;
  font-family: 'Inter', sans-serif;
  background: var(--background-color);
  position: relative;
  overflow-x: hidden;
  align-items: flex-start;
}

.home-header {
  position: relative;
  z-index: 1;
  display: flex;
  align-items: flex-start;
  gap: 12px;
  margin-bottom: 28px;
}

.welcome-wrapper {
  background: rgba(255, 255, 255, 0.5);
  backdrop-filter: blur(24px);
  -webkit-backdrop-filter: blur(24px);
  border: 1.5px solid rgba(255, 255, 255, 0.6);
  border-radius: 20px;
  padding: 12px 24px;
}

.welcome-title {
  font-size: 40px;
  font-weight: 900;
  background: var(--brand-berry);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  background-clip: text;
  margin: 0;
  text-align: left;
}

.home-grid {
  position: relative;
  z-index: 1;
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 20px;
  width: 100%;
  align-items: start;
}

.widget-notifications {
  grid-column: 1;
  grid-row: 1 / 3;
}

.widget-calendar {
  grid-column: 2;
  grid-row: 1;
}

.widget-companies {
  grid-column: 2;
  grid-row: 2;
}

.widget {
  background: rgba(255, 255, 255, 0.5);
  backdrop-filter: blur(24px);
  -webkit-backdrop-filter: blur(24px);
  border: 1.5px solid rgba(255, 255, 255, 0.6);
  border-radius: 20px;
  padding: 20px;
  box-shadow: 0 8px 32px rgba(0, 0, 0, 0.08);
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.widget-title-link {
  cursor: pointer;
  width: fit-content;
}

.title-text {
  display: inline-block;
  position: relative;
  transition: padding-right 0.4s ease;
  /* inherit the gradient clip from .widget-title */
  background: var(--brand-berry);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  background-clip: text;
}

.title-arrow {
  position: absolute;
  right: -22px;
  top: 0;
  opacity: 0;
  transition: opacity 0.4s ease, right 0.4s ease;
  background: var(--brand-berry);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  background-clip: text;
}

.widget-title-link:hover .title-text {
  padding-right: 28px;
}

.widget-title-link:hover .title-arrow {
  opacity: 1;
  right: 0;
}

.widget-title {
  font-size: 28px;
  font-weight: 700;
  margin: 0;
  display: flex;
  align-items: center;
  gap: 10px;
  color: var(--brand-berry)
}

.title-icon {
  width: 36px;
  height: 36px;
  border-radius: 50%;
  background: var(--brand-berry);
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
  color: white;
  font-size: 16px;
}

.notifications-list {
  display: flex;
  flex-direction: column;
  gap: 10px;
  max-height: 500px;
  overflow-y: auto;
  padding-right: 4px;
}

.notifications-list::-webkit-scrollbar {
  width: 6px;
}

.notifications-list::-webkit-scrollbar-track {
  background: transparent;
}

.notifications-list::-webkit-scrollbar-thumb {
  background: var(--brand-berry);
  border-radius: 10px;
  opacity: 0.5;
}

.reminder-card {
  display: flex;
  align-items: flex-start;
  gap: 12px;
  background: rgba(255, 255, 255, 0.6);
  backdrop-filter: blur(12px);
  -webkit-backdrop-filter: blur(12px);
  border: 1.5px solid rgba(255, 255, 255, 0.85);
  border-radius: 12px;
  padding: 14px;
  box-shadow: 0 2px 8px rgba(0,0,0,0.04);
}

.reminder-icon {
  color: var(--reminder-header, #e53e3e);
  font-size: 18px;
  flex-shrink: 0;
  margin-top: 2px;
}

.reminder-content {
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.reminder-header {
  font-size: 14px;
  font-weight: 700;
  color: var(--brand-berry);
  margin: 0;
}

.reminder-body {
  font-size: 13px;
  color: var(--brand-berry);
  margin: 0;
}

.reminder-nav {
  font-size: 13px;
  color: var(--brand-berry);
  margin: 0;
  cursor: pointer;
}

.reminder-nav:hover {
  font-weight: 700;
  color: #841c40;
}

.no-more {
  text-align: center;
  justify-content: center;
  color: #aaa;
  font-size: 13px;
  padding: 16px 0;
}

.widget-calendar :deep(.calendar-container) {
  width: 100%;
  max-width: 100%;
  margin: 0;
  padding: 0;
  background: transparent;
  box-shadow: none;
  border-radius: 0;
}

.companies-list {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.company-row {
  background: rgba(255, 255, 255, 0.6);
  backdrop-filter: blur(12px);
  -webkit-backdrop-filter: blur(12px);
  border: 1.5px solid rgba(255, 255, 255, 0.85);
  border-radius: 12px;
  padding: 10px 14px;
  box-shadow: 0 2px 6px rgba(0,0,0,0.04);
}

.company-name {
  font-size: 14px;
  font-weight: 600;
  color: var(--brand-berry);
  margin: 0;
}

.company-sub {
  font-size: 12px;
  color: #9ca3af;
  margin: 0;
}

.view-stats-btn {
  align-self: flex-end;
  padding: 10px 20px;
  background: var(--brand-berry, #a12971);
  color: white;
  border: none;
  border-radius: 50px;
  font-family: 'Inter', sans-serif;
  font-size: 14px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.2s ease;
  box-shadow: 0 4px 12px rgba(161, 41, 113, 0.3);
}

.view-stats-btn:hover {
  transform: translateY(-1px);
  box-shadow: 0 6px 16px rgba(161, 41, 113, 0.4);
}


@media (max-width: 1024px) {
  .page-content {
    padding: 24px;
  }

  .home-grid {
    grid-template-columns: 1fr;
    grid-template-rows: auto;
  }

  .widget-notifications,
  .widget-calendar,
  .widget-companies {
    grid-column: 1;
    grid-row: auto;
  }
}

@media (max-width: 600px) {
  .page-content {
    padding: 16px 12px;
  }
}
</style>
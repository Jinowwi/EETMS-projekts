<template>
  <!-- Izrakstīšanās poga -->
  <button class="logout-btn" @click="handleLogout">
    <FontAwesomeIcon icon="fa-solid fa-right-from-bracket" />
    Logout
  </button>

  <div class="page-content">
    <!-- Dekoratīvie fona elementi -->
    <div class="blob blob-teal"></div>
    <div class="blob blob-pink"></div>

    <div class="home-header">
      <div class="welcome-wrapper">
        <!-- Sākuma lapas virsraksts -->
        <h1 class="welcome-title">Welcome to EETMS!</h1>
      </div>
    </div>

    <div class="home-grid">
      <!-- Paziņojumu bloks -->
      <div class="widget widget-notifications">
        <h2 class="widget-title widget-title-link" @click="router.push('/reminders')">
          <span class="title-icon">
            <FontAwesomeIcon icon="fa-solid fa-message" />
          </span>
          <span class="title-text">Notifications<span class="title-arrow">»</span></span>
        </h2>

        <div class="notifications-list">
          <!-- Atgādinājumu saraksts -->
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
                Start: {{ r.startTime ? String(r.startTime).slice(0, 5) : 'Missed' }},
                End: {{ r.endTime ? String(r.endTime).slice(0, 5) : 'Missed' }}
              </p>
              <p class="reminder-nav" @click="goToCalendar(r)">To Calendar</p>
            </div>
          </div>
          <!-- Teksts, ja paziņojumu nav -->
          <div v-if="reminders.length === 0" class="no-more">No more notifications</div>
        </div>
      </div>

      <!-- Maiņu pieprasījumu bloks -->
      <div class="widget widget-shiftrequests">
        <h2 class="widget-title">
          <span class="title-icon">
            <FontAwesomeIcon icon="fa-solid fa-clipboard-list" />
          </span>
          Shift requests
        </h2>

        <div class="shiftrequests-list">
          <!-- AttēloT lietotājam aktuālos maiņu pieprasījumus -->
          <div
            v-for="req in relevantShiftRequests"
            :key="req.shiftRequestID"
            class="shiftrequest-row"
          >
            <div class="shiftrequest-main">
              <div class="shiftrequest-top">
                <p class="shiftrequest-reason">{{ req.reason?.name ?? 'Unknown reason' }}</p>
                
                <!-- Atzīmet, ja pieprasījums attiecas uz lietotāja uzņēmumu -->
                <span
                  v-if="isMyCompanyRequest(req)"
                  class="your-company-tag"
                >
                  Your Company
                </span>
              </div>

              <p class="shiftrequest-status">
                {{ shiftRequestStatusMap[req.status] ?? req.status }}
              </p>
            </div>

            <!-- Poga detalizēta pieprasījuma skatīšanai -->
            <button class="view-btn" type="button" @click="selectedShiftRequest = req">
              View
            </button>
          </div>

          <!-- Teksts, ja nav atbilstošu pieprasījumu -->
          <div v-if="relevantShiftRequests.length === 0" class="no-more">
            No relevant shift requests
          </div>
        </div>
      </div>

      <!-- Kalendāra bloks -->
      <div class="widget widget-calendar">
        <h2 class="widget-title widget-title-link" @click="router.push('/calendar')">
          <span class="title-icon">
            <FontAwesomeIcon icon="fa-solid fa-calendar" />
          </span>
          <span class="title-text">Calendar<span class="title-arrow">»</span></span>
        </h2>
        <Calendar :missedPunches="shifts" @punchClick="handlePunchClick" />
      </div>

      <!-- Lietotāja uzņēmumu bloks -->
      <div class="widget widget-companies">
        <h2 class="widget-title">
          <span class="title-icon">
            <FontAwesomeIcon icon="fa-solid fa-list" />
          </span>
          Your companies
        </h2>

        <!-- Uzņēmumu saraksts -->
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

        <!-- Poga statistikas atvēršanai -->
        <button class="view-stats-btn" @click="router.push('/charts')">View stats</button>
      </div>
    </div>

    <!-- Modālais logs maiņu pieprasījuma apskatei -->
    <ShiftRequestRemModal
      v-if="selectedShiftRequest"
      :request="selectedShiftRequest"
      :remId="myRemId"
      :companies="companies"
      :companyReasons="allCompanyReasons"
      @close="selectedShiftRequest = null"
      @accepted="refreshShiftRequests"
      @updated="refreshAllData"
    />

    <!-- Modālais logs nokavēta punch ieraksta labošanai -->
    <MissedPunchModal
      :isOpen="modalOpen"
      :punchData="selectedPunch"
      @close="modalOpen = false"
      @save="handleSave"
    />
  </div>
</template>

<script setup>
// Importēt Vue funkcijas reaktivitātei 
import { ref, onMounted, computed } from 'vue'

// Importēt Vue Router navigācijai
import { useRouter } from 'vue-router'

// Importēt FontAwesome ikonu komponenti
import { FontAwesomeIcon } from '@fortawesome/vue-fontawesome'

// Importē kalendāra komponenti
import Calendar from '@/components/calendar.vue'

// Importēt modālos logus
import MissedPunchModal from '@/components/MissedPunchModal.vue'
import ShiftRequestRemModal from '@/components/ShiftRequestRemModal.vue'

// Importēt izrakstīšanās funkciju un administratoru piekļūves līmeni
import { logout } from '@/services/auth.js'
import { getAdminRoleLevel, getAdmin } from '@/services/auth.js'

// Noteikt lietotāja lomas līmeni un administratoru
const roleLevel = Number(getAdminRoleLevel())
const currentAdmin = getAdmin()

// Inicializēt routeri
const router = useRouter()

// API bāzes adreses
const API_BASE_REM = 'http://localhost:5001/api'
const API_BASE_ADMIN = 'http://localhost:5001/api'

// Reaktīvie dati paziņojumiem, maiņām un uzņēmumiem
const reminders = ref([])
const shifts = ref([])
const companies = ref([])
const modalOpen = ref(false)
const selectedPunch = ref({})

// Reaktīvie dati darba iemesliem un pieprasījumiem
const allReasons = ref([])
const allCompanyReasons = ref([])
const allShiftRequests = ref([])

// Izvēlētais maiņas pieprasījums
const selectedShiftRequest = ref(null)

// Iegūst lietotāja remId no auth vai localStorage
function getMyRemId() {
  if (currentAdmin && currentAdmin.remId) {
    return Number(currentAdmin.remId)
  }

  const storedId = localStorage.getItem('remId')
  return storedId ? Number(storedId) : 1
}

// Lietotāja remId vērtība
const myRemId = getMyRemId()

// Maiņu pieprasījumu statusu inicializācija
const shiftRequestStatusMap = {
  1: 'Sent',
  2: 'Approved',
  3: 'In Progress',
  4: 'Done'
}

// Atjaunot maiņu pieprasījumu datus
const refreshShiftRequests = async () => {
  await fetchShiftRequestData()

  if (selectedShiftRequest.value) {
    const updated = allShiftRequests.value.find(
      r => r.shiftRequestID === selectedShiftRequest.value.shiftRequestID
    )

    if (updated) {
      selectedShiftRequest.value = updated
    }
  }
}

// Atjaunot visus sākuma lapas datus
const refreshAllData = async () => {
  const myCompanyNames = companies.value.map(c => c.companyName)

  await Promise.all([
    fetchShiftRequestData(),
    fetchShifts(myCompanyNames),
    fetchReminders(myCompanyNames)
  ])

  if (selectedShiftRequest.value) {
    const updated = allShiftRequests.value.find(
      r => r.shiftRequestID === selectedShiftRequest.value.shiftRequestID
    )

    if (updated) {
      selectedShiftRequest.value = updated
    }
  }
}

// Ielādēt uzņēmumus no API
const fetchCompanies = async () => {
  const res = await fetch(`${API_BASE_ADMIN}/companies`)
  if (!res.ok) return []

  let data = await res.json()

  // Ja lietotājam ir 1. līmeņa piekļuve, rādīt tikai viņa uzņēmumus
  if (roleLevel === 1) {
    const myRemIdLocal = getMyRemId()
    data = data.filter(c => c.remID === myRemIdLocal)
  }

  companies.value = data.map(c => ({
    id: c.companyID,
    companyName: c.companyName,
    address: c.address
  }))

  return companies.value.map(c => c.companyName)
}

// Ielādēt maiņu datus
const fetchShifts = async (myCompanyNames = []) => {
  const res = await fetch(`${API_BASE_REM}/shifts`)
  if (res.ok) {
    let data = await res.json()

    // Filtrēt maiņas tikai lietotāja uzņēmumiem
    if (roleLevel === 1 && myCompanyNames.length > 0) {
      data = data.filter(shift => myCompanyNames.includes(shift.companyName))
    }

    shifts.value = data.map(shift => ({
      ...shift,
      date: shift.startDate,
      type: !shift.startTime ? 'start' : !shift.endTime ? 'end' : null
    }))
  }
}

// Ielādēt atgādinājumus no API
const fetchReminders = async (myCompanyNames = []) => {
  const res = await fetch(`${API_BASE_REM}/shifts/reminders`)
  if (res.ok) {
    let data = await res.json()

    // Filtrēt atgādinājumus tikai lietotāja uzņēmumiem
    if (roleLevel === 1 && myCompanyNames.length > 0) {
      data = data.filter(r => myCompanyNames.includes(r.companyName))
    }

    reminders.value = data
  }
}

// Ielādēt maiņu pieprasījumus, iemeslus un uzņēmumu iemeslus
const fetchShiftRequestData = async () => {
  const [requestsRes, reasonsRes, companyReasonsRes] = await Promise.all([
    fetch(`${API_BASE_REM}/shiftrequests`),
    fetch(`${API_BASE_REM}/reasons`),
    fetch(`${API_BASE_REM}/companyreasons`)
  ])

  if (requestsRes.ok) allShiftRequests.value = await requestsRes.json()
  if (reasonsRes.ok) allReasons.value = await reasonsRes.json()
  if (companyReasonsRes.ok) allCompanyReasons.value = await companyReasonsRes.json()
}

// Lietotāja uzņēmumu ID saraksts
const myCompanyIds = computed(() => companies.value.map(c => c.id))

// Lietotāja uzņēmumiem piesaistīto iemeslu ID saraksts
const myReasonIds = computed(() => {
  const ids = allCompanyReasons.value
    .filter(cr => myCompanyIds.value.includes(cr.companiesCompanyID))
    .map(cr => cr.reasonsReasonID)

  return [...new Set(ids)]
})

// Lietotājam redzamie maiņu pieprasījumi
const relevantShiftRequests = computed(() => {
  return allShiftRequests.value.filter(req => {
    const reasonMatches = myReasonIds.value.includes(req.reasonID)
    const visibleToMe =
      req.status === 1 || req.remId === myRemId

    return reasonMatches && visibleToMe
  })
})

// Pārbaudīt, vai pieprasījums pieder lietotāja uzņēmumam
const isMyCompanyRequest = (req) => {
  return req.companyId != null && myCompanyIds.value.includes(req.companyId)
}

// Parādīt sākotnējos datus, kad komponents tiek ielādēts
onMounted(async () => {
  const myCompanyNames = await fetchCompanies()

  await Promise.all([
    fetchShifts(myCompanyNames),
    fetchReminders(myCompanyNames),
    fetchShiftRequestData()
  ])
})

// Izrakstīt lietotāju no sistēmas
const handleLogout = () => {
  logout()
  router.push('/roleselect')
}

// Atvert kalendāru ar konkrētu datumu
const goToCalendar = (r) => {
  router.push({ path: '/calendar', query: { date: r.startDate || r.endDate } })
}

// Atvert modālo logu izvēlētajam  ierakstam
const handlePunchClick = (punchData) => {
  selectedPunch.value = punchData
  modalOpen.value = true
}

// Pārveidot laiku uz HH:MM:SS formātu
const toHHMMSS = (hhmm) => (hhmm && hhmm.length === 5 ? `${hhmm}:00` : hhmm)

// Saglabāt izlaboto maiņu ierakstu
const handleSave = async (updatedShift) => {
  const id = updatedShift.ShiftID || updatedShift.shiftID
  const patch = []

  if (selectedPunch.value.type === 'start') {
    patch.push({ op: 'replace', path: '/start_time', value: toHHMMSS(updatedShift.startTime) })
  } else if (selectedPunch.value.type === 'end') {
    if (updatedShift.EndDate) {
      patch.push({ op: 'replace', path: '/end_date', value: updatedShift.EndDate })
    }
    patch.push({ op: 'replace', path: '/end_time', value: toHHMMSS(updatedShift.endTime) })
  }

  const res = await fetch(`${API_BASE_REM}/shifts/${id}`, {
    method: 'PATCH',
    headers: { 'Content-Type': 'application/json-patch+json' },
    body: JSON.stringify(patch)
  })

  if (res.ok) {
    modalOpen.value = false
    await fetchShifts(companies.value.map(c => c.companyName))
  }
}

// Formatēt datumu attēlošanai
const formatDate = (dateLike) => {
  if (!dateLike) return ''
  const d = new Date(String(dateLike).trim().replaceAll('/home', '-'))
  if (isNaN(d.valueOf())) return String(dateLike)
  return d.toLocaleDateString('de-DE', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric'
  })
}
</script>

<style scoped>
/* Galvenais lapas satura konteiners */
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

/* Augšējā daļa ar virsrakstu */
.home-header {
  position: relative;
  z-index: 1;
  display: flex;
  align-items: flex-start;
  gap: 12px;
  margin-bottom: 28px;
}

/* Bloks ap virsraksta tekstu */
.welcome-wrapper {
  background: rgba(255, 255, 255, 0.5);
  backdrop-filter: blur(24px);
  -webkit-backdrop-filter: blur(24px);
  border: 1.5px solid rgba(255, 255, 255, 0.6);
  border-radius: 20px;
  padding: 12px 24px;
}

/* Virsraksta stils */
.welcome-title {
  font-family: 'Inter', sans-serif;
  font-size: 40px;
  font-weight: 900;
  background: var(--brand-berry);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  background-clip: text;
  margin: 0;
  text-align: left;
}

/* Galvenais logrīku režģis */
.home-grid {
  position: relative;
  z-index: 1;
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 20px;
  width: 100%;
  align-items: start;
}

/* Paziņojumu logrīka pozīcija */
.widget-notifications {
  grid-column: 1;
  grid-row: 1;
}

/* Maiņu pieprasījumu logrīka pozīcija */
.widget-shiftrequests {
  grid-column: 1;
  grid-row: 2;
}

/* Kalendāra logrīka pozīcija */
.widget-calendar {
  grid-column: 2;
  grid-row: 1;
}

/* Uzņēmumu logrīka pozīcija */
.widget-companies {
  grid-column: 2;
  grid-row: 2;
}

/* Logrīku stils */
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

/* Klikšķināms logrīka virsraksts */
.widget-title-link {
  cursor: pointer;
  width: fit-content;
}

/* Virsraksta teksta stils */
.title-text {
  display: inline-block;
  position: relative;
  transition: padding-right 0.4s ease;
  background: var(--brand-berry);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  background-clip: text;
}

/* Bultiņas stils pie virsraksta */
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

/* Virsraksta animācija un bultiņas */
.widget-title-link:hover .title-text {
  padding-right: 28px;
}

.widget-title-link:hover .title-arrow {
  opacity: 1;
  right: 0;
}

/* Logrīku virsrakstu stils */
.widget-title {
  font-size: 28px;
  font-weight: 700;
  margin: 0;
  display: flex;
  align-items: center;
  gap: 10px;
  color: var(--brand-berry);
}

/* Ikonas aplis virsrakstos */
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

/* Sarakstu stils */
.notifications-list,
.shiftrequests-list {
  display: flex;
  flex-direction: column;
  gap: 10px;
  max-height: 500px;
  overflow-y: auto;
  padding-right: 4px;
}

/* Scrollbar platums */
.notifications-list::-webkit-scrollbar,
.shiftrequests-list::-webkit-scrollbar {
  width: 6px;
}

/* Scrollbar fons */
.notifications-list::-webkit-scrollbar-track,
.shiftrequests-list::-webkit-scrollbar-track {
  background: transparent;
}

/* Scrollbar stils */
.notifications-list::-webkit-scrollbar-thumb,
.shiftrequests-list::-webkit-scrollbar-thumb {
  background: var(--brand-berry);
  border-radius: 10px;
  opacity: 0.5;
}

/* Atgādinājuma kartītes stils */
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

/* Maiņu pieprasījuma rindas stils */
.shiftrequest-row {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 12px;
  background: rgba(255, 255, 255, 0.6);
  backdrop-filter: blur(12px);
  -webkit-backdrop-filter: blur(12px);
  border: 1.5px solid rgba(255, 255, 255, 0.85);
  border-radius: 12px;
  padding: 14px;
  box-shadow: 0 2px 8px rgba(0,0,0,0.04);
}

/* Atgādinājuma ikonas stils */
.reminder-icon {
  color: var(--reminder-header, #e53e3e);
  font-size: 18px;
  flex-shrink: 0;
  margin-top: 2px;
}

/* Atgādinājumu un pieprasījumu tekstu stili */
.reminder-content,
.shiftrequest-main {
  display: flex;
  flex-direction: column;
  gap: 2px;
  min-width: 0;
}

.reminder-header,
.shiftrequest-reason {
  font-size: 14px;
  font-weight: 700;
  color: var(--brand-berry);
  margin: 0;
}

.reminder-body,
.shiftrequest-status {
  font-size: 13px;
  color: var(--brand-berry);
  margin: 0;
}

/* Saite uz kalendāru (atgādinājuma kartīte) */
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

/* Maiņu pieprasījuma bloka iekšējais izkārtojums */
.shiftrequest-top {
  display: flex;
  align-items: center;
  gap: 8px;
  flex-wrap: wrap;
}

.your-company-tag {
  display: inline-flex;
  align-items: center;
  padding: 3px 8px;
  border-radius: 999px;
  background: rgba(43, 164, 146, 0.14);
  color: var(--brand-teal);
  font-size: 11px;
  font-weight: 700;
  white-space: nowrap;
}

/* Pogas pieprasījuma apskatei */
.view-btn {
  align-self: center;
  padding: 8px 16px;
  background: transparent;
  color: var(--brand-berry);
  border: 1.5px solid var(--brand-berry);
  border-radius: 999px;
  font-family: 'Inter', sans-serif;
  font-size: 13px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.2s ease;
  flex-shrink: 0;
}

.view-btn:hover {
  background: var(--brand-berry);
  color: white;
}

/* Tukšs saraksts – teksts, ja nav datu */
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

/* Uzņēmumu saraksta bloki */
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

/* Poga statistikas atvēršanai */
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

/* Responsivitāte: planšetdatori */
@media (max-width: 1024px) {
  .page-content {
    padding: 24px;
  }

  .home-grid {
    grid-template-columns: 1fr;
    grid-template-rows: auto;
  }

  .widget-notifications,
  .widget-shiftrequests,
  .widget-calendar,
  .widget-companies {
    grid-column: 1;
    grid-row: auto;
  }
}

/* Responsivitāte: mobilas ierīces */
@media (max-width: 600px) {
  .page-content {
    padding: 16px 12px;
  }

  .widget-title {
    font-size: 24px;
  }

  .welcome-title {
    font-size: 32px;
  }

  .shiftrequest-row {
    flex-direction: column;
    align-items: stretch;
  }

  .view-btn {
    align-self: flex-end;
  }
}
</style>
<template>
  <div class="page">
    <div class="container">
      <!-- Dekoratīvie fona elementi -->
      <div class="blob blob-teal"></div>
      <div class="blob blob-pink"></div>

      <!-- Izrakstīšanās poga -->
      <button class="logout-btn" @click="handleLogout">
        <FontAwesomeIcon icon="fa-solid fa-right-from-bracket" />
        Logout
      </button>

      <!-- Lapas virsraksts-->
      <h1>Welcome to EETMS!</h1>

      <!-- Ielādes ziņojums, kamēr tiek saņemti veikala dati -->
      <div v-if="isLoading" class="loading-msg">Loading shop info...</div>

      <!-- Galvenais lapas saturs, ja veikala dati ir veiksmīgi ielādēti -->
      <div v-else-if="shop" class="cards-grid">
        <!-- Veikala informācijas kartīte -->
        <div class="card shop-info card-tall">
          <h2>{{ shop.code }}</h2>
          <div class="info-row">
            <span class="label">Type: </span>
            <span class="value">{{ typeMap[shop.type] ?? shop.type }}</span>
          </div>
          <div class="info-row">
            <span class="label">Country: </span>
            <span class="value">{{ countryMap[shop.country] ?? shop.country }}</span>
          </div>
          <div class="info-row">
            <span class="label">Address: </span>
            <span class="value">{{ shop.address }}</span>
          </div>
          <div class="info-row">
            <span class="label">Total external shifts: </span>
            <span class="value">{{ totalExternalShifts }}</span>
          </div>
        </div>

        <!-- Aktuālo maiņu pieprasījumu kartīte -->
        <div class="card shift-requests card-wide">
          <div class="card-header">
            <h2>Relevant shift requests</h2>
            <span class="card-count">{{ activeShiftRequests.length }}</span>
          </div>

          <!-- Ziņojums, ja nav aktuālu pieprasījumu -->
          <div v-if="activeShiftRequests.length === 0" class="empty-msg">
            No relevant shift requests
          </div>

          <!-- Pieprasījumu saraksts -->
          <div v-else class="request-list">
            <div
              v-for="req in activeShiftRequests"
              :key="req.shiftRequestID"
              class="request-row"
            >
              <div class="request-info">
                <span class="req-reason">{{ req.reason?.name }}</span>
                <span :class="['req-status', statusClass(req.status)]">
                  {{ statusMap[req.status] ?? req.status }}
                </span>
              </div>

              <!-- Poga pieprasījuma detaļu apskatei -->
              <button class="btn-view" @click="selectedRequest = req">View</button>
            </div>
          </div>
        </div>

        <!-- Kartīte jauna pieprasījuma izveidei -->
        <div class="card make-request card-action">
          <h2>Make request</h2>
          <p class="card-subtext">Create a new external shift request.</p>
          <button class="plus-btn" @click="showModal = true">+</button>
        </div>

        <!-- Kartīte pieprasījumiem, kas gaida novērtējumu -->
        <div class="card rating-card">
          <div class="card-header">
            <h2>Waiting for rating</h2>
            <span class="card-count">{{ waitingForRating.length }}</span>
          </div>

          <!-- Ziņojums, ja nav pieprasījumu novērtēšanai -->
          <div v-if="waitingForRating.length === 0" class="empty-msg">
            No shift requests waiting for rating
          </div>

          <!-- Saraksts ar pieprasījumiem, kas jānovērtē, ja tie ir -->
          <div v-else class="request-list">
            <div
              v-for="req in waitingForRating"
              :key="req.shiftRequestID"
              class="request-row"
            >
              <div class="request-info">
                <span class="req-reason">{{ req.reason?.name ?? 'Shift request' }}</span>
                <span class="req-status status-done">Done</span>
              </div>
              <button class="btn-view" @click="selectedRatingRequest = req">Rate</button>
            </div>
          </div>
        </div>

        <!-- Kartīte ar jau iesniegtajiem novērtējumiem -->
        <div class="card rating-card">
          <div class="card-header">
            <h2>Your ratings</h2>
            <span class="card-count">{{ ratedRequests.length }}</span>
          </div>

          <!-- Ziņojums, ja vēl nav iesniegtu novērtējumu -->
          <div v-if="ratedRequests.length === 0" class="empty-msg">
            No ratings submitted yet
          </div>

          <!-- Saraksts ar novērtētajiem pieprasījumiem -->
          <div v-else class="request-list">
            <div
              v-for="req in ratedRequests"
              :key="req.shiftRequestID"
              class="request-row"
            >
              <div class="request-info">
                <span class="req-reason">{{ req.reason?.name ?? 'Shift request' }}</span>
                <span class="rating-stars">{{ renderStars(req.rating?.stars ?? 0) }}</span>
              </div>
              <button class="btn-view" @click="selectedRatedRequest = req">View/Edit</button>
            </div>
          </div>
        </div>

        <!-- Modālais logs jauna pieprasījuma izveidei -->
        <ShiftRequestModal
          v-if="showModal"
          :shopId="Number(shopId)"
          @close="showModal = false"
          @submitted="refreshRequests"
        />

        <!-- Modālais logs pieprasījuma detaļu apskatei -->
        <ShiftRequestDetailModal
          v-if="selectedRequest"
          :request="selectedRequest"
          @close="selectedRequest = null"
          @deleted="refreshRequests"
          @updated="refreshRequests"
        />

        <!-- Modālais logs novērtējuma pievienošanai -->
        <RatingModal
          v-if="selectedRatingRequest"
          :request="selectedRatingRequest"
          @close="selectedRatingRequest = null"
          @submitted="handleRatingSubmitted"
        />

        <!-- Modālais logs esoša novērtējuma apskatei vai rediģēšanai -->
        <RatingModal
          v-if="selectedRatedRequest"
          :request="selectedRatedRequest"
          :allowEdit="true"
          @close="selectedRatedRequest = null"
          @submitted="handleRatedEdited"
        />
      </div>

      <!-- Kļūdas ziņojums, ja neizdevās ielādēt veikala informāciju -->
      <div v-else class="error-msg">Failed to load shop info.</div>
    </div>
  </div>
</template>

<script setup>
// Importēt Vue funkcijas un komponentes
import { ref, onMounted, computed } from 'vue'
import { useRouter } from 'vue-router'
import { logout } from '@/services/auth.js'
import { FontAwesomeIcon } from '@fortawesome/vue-fontawesome'
import ShiftRequestModal from '../components/ShiftRequestModal.vue'
import ShiftRequestDetailModal from '../components/ShiftRequestDetailModal.vue'
import RatingModal from '../components/RatingModal.vue'

// Mainīgais modālā loga parādīšanai
const showModal = ref(false)

// Masīvs ar maiņu pieprasījumiem
const shiftRequests = ref([])

// API bāzes adrese
const API_BASE = 'http://localhost:5002/api'

// Router inicializācija navigācijai starp lapām 
const router = useRouter()

// Reaktīvie mainīgie lapas datiem un stāvokļiem
const shop = ref(null)
const totalExternalShifts = ref(0)
const isLoading = ref(true)
const shopId = ref(null)
const selectedRequest = ref(null)
const selectedRatingRequest = ref(null)
const selectedRatedRequest = ref(null)

// Valsts nosaukumi 
const countryMap = {
  1: 'Lithuania',
  2: 'Latvia',
  3: 'Estonia'
}

// Veikalu tipi 
const typeMap = {
  1: 'Hyper',
  2: 'Super',
  3: 'Mini',
  4: 'Express'
}

// Pieprasījumu statusi 
const statusMap = {
  1: 'Sent',
  2: 'Approved',
  3: 'In Progress',
  4: 'Done'
}

// Funkcija CSS klases noteikšanai pēc statusa
function statusClass(status) {
  const map = {
    1: 'status-sent',
    2: 'status-approved',
    3: 'status-inprogress',
    4: 'status-done'
  }
  return map[status] ?? 'status-default'
}

// Aprēķināts saraksts ar aktīvajiem pieprasījumiem
const activeShiftRequests = computed(() => {
  return shiftRequests.value.filter(req => Number(req.status) !== 4)
})

// Aprēķināts saraksts ar pieprasījumiem, kas gaida novērtējumu
const waitingForRating = computed(() => {
  return shiftRequests.value.filter(req =>
    Number(req.status) === 4 && !req.rating
  )
})

// Funkcija pēc novērtējuma labošanas
async function handleRatedEdited() {
  selectedRatedRequest.value = null
  await refreshRequests()
}

// Aprēķināts saraksts ar jau novērtētajiem pieprasījumiem
const ratedRequests = computed(() => {
  return shiftRequests.value.filter(req =>
    Number(req.status) === 4 && req.rating
  )
})

// Funkcija zvaigžņu attēlošanai pēc vērtējuma
function renderStars(count) {
  const n = Number(count) || 0
  return '★'.repeat(n) + '☆'.repeat(5 - n)
}

// Izrakstīšanās funkcija
const handleLogout = () => {
  logout()
  router.push('/roleselect')
}

// Funkcija pieprasījumu saraksta atjaunošanai no servera
async function refreshRequests() {
  const res = await fetch(`${API_BASE}/shiftrequests/byshop/${shopId.value}`)
  if (res.ok) {
    shiftRequests.value = await res.json()
  }
}

// Funkcija pēc novērtējuma iesniegšanas
async function handleRatingSubmitted() {
  selectedRatingRequest.value = null
  await refreshRequests()
}

// Kad komponents ielādējas, iegūt veikala datus un pieprasījumus
onMounted(async () => {
  shopId.value = localStorage.getItem('shopId')

  // Ja nav veikala ID, pārvirza uz pieteikšanās lapu
  if (!shopId.value) {
    router.push('/login')
    return
  }

  try {
    // Iegūt veikala datus
    const shopRes = await fetch(`${API_BASE}/shops/${shopId.value}`)
    if (!shopRes.ok) throw new Error('Failed to fetch shop')
    shop.value = await shopRes.json()

    // Iegūt maiņu sarakstu
    const shiftsRes = await fetch(`${API_BASE}/shifts/byshop/${shopId.value}`)
    if (shiftsRes.ok) {
      const shifts = await shiftsRes.json()
      totalExternalShifts.value = shifts.length
    }

    // Atjaunot maiņu pieprasījumu sarakstu
    await refreshRequests()
  } catch (err) {
    console.error('Error loading shop home:', err)
    shop.value = null
  } finally {
    // Beigt ielādes stāvokli
    isLoading.value = false
  }
})
</script>

<style scoped>

/* Pamatstili visiem elementiem */
* {
  margin: 0;
  padding: 0;
  box-sizing: border-box;
  font-family: 'Segoe UI', sans-serif;
}

/* Galvenais lapas konteiners */
.page {
  min-height: 100vh;
  display: flex;
  flex-direction: column;
}

/* Satura konteiners */
.container {
  position: relative;
  flex: 1;
  display: flex;
  flex-direction: column;
  align-items: center;
  padding: 30px 20px 40px;
}

/* Virsraksti */
h1 {
  color: var(--brand-berry);
  font-size: 30px;
  font-weight: 800;
  margin-bottom: 28px;
  text-align: center;
  z-index: 2;
}

/* Kartīšu režģis */
.cards-grid {
  width: 100%;
  max-width: 1200px;
  display: grid;
  grid-template-columns: repeat(12, minmax(0, 1fr));
  gap: 20px;
  align-items: stretch;
  z-index: 2;
}

/* Kartītes stils */
.card {
  background: rgba(255, 255, 255, 0.5);
  backdrop-filter: blur(24px);
  -webkit-backdrop-filter: blur(24px);
  border: 1.5px solid rgba(255, 255, 255, 0.6);
  box-shadow: 0 8px 32px rgba(0, 0, 0, 0.08);
  border-radius: 20px;
  padding: 22px 24px;
  min-height: 210px;
  display: flex;
  flex-direction: column;
}

/* Kartītes virsraksts */
.card h2 {
  color: var(--brand-berry);
  font-size: 24px;
  font-weight: 700;
  margin-bottom: 16px;
}

/* Kartītes galvene */
.card-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
  margin-bottom: 12px;
}

/* Kartītes galvenes virsraksts */
.card-header h2 {
  margin-bottom: 0;
}

/* Skaitītāja un etiķetes kopīgais stils */
.card-count,
.card-pill {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  min-width: 34px;
  height: 28px;
  padding: 0 10px;
  border-radius: 999px;
  font-size: 12px;
  font-weight: 700;
}

.card-count {
  background: rgba(161, 41, 113, 0.12);
  color: var(--brand-berry);
}

.card-pill {
  background: rgba(43, 164, 146, 0.14);
  color: #218c74;
}

/* Apakšteksts kartītē */
.card-subtext {
  color: #7b6d75;
  font-size: 14px;
  line-height: 1.45;
  margin-bottom: 16px;
}

/* Karšu platumi */
.shop-info {
  grid-column: span 4;
}

.shift-requests {
  grid-column: span 8;
}

.make-request {
  grid-column: span 4;
}

.rating-card {
  grid-column: span 4;
}

/* Informācijas rindas stils */
.info-row {
  margin-bottom: 10px;
  font-size: 14px;
  line-height: 1.45;
}

/* Etiķetes teksts */
.label {
  font-weight: 900;
  color: var(--brand-berry);
}

.value {
  color: var(--brand-berry);
}

.shift-requests {
  display: flex;
  flex-direction: column;
}

/* Tukša saraksta ziņojums */
.empty-msg {
  flex: 1;
  display: flex;
  align-items: center;
  justify-content: center;
  color: #aaa;
  font-size: 13px;
  font-style: italic;
  margin-top: 10px;
  text-align: center;
}

.make-request {
  align-items: flex-start;
  justify-content: space-between;
}

/* Poga jauna pieprasījuma izveidei */
.plus-btn {
  width: 50px;
  height: 50px;
  border-radius: 50%;
  border: 2px solid #4caf87;
  background: transparent;
  color: #4caf87;
  font-size: 30px;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: background 0.2s, transform 0.2s;
  padding-bottom: 4px;
  margin-top: auto;
}

.plus-btn:hover {
  background: rgba(76, 175, 135, 0.15);
  transform: translateY(-1px);
}

/* Pieprasījumu saraksts */
.request-list {
  display: flex;
  flex-direction: column;
  gap: 10px;
  margin-top: 6px;
}

/* Viena pieprasījuma rinda */
.request-row {
  display: flex;
  align-items: center;
  justify-content: space-between;
  background: rgba(255, 255, 255, 0.7);
  border: 1.5px solid rgba(255, 255, 255, 0.6);
  border-radius: 12px;
  padding: 12px 16px;
  gap: 12px;
}

.request-info {
  display: flex;
  flex-direction: column;
  gap: 4px;
  min-width: 0;
}

/* Pieprasījuma iemesla teksts */
.req-reason {
  font-size: 14px;
  font-weight: 700;
  color: var(--brand-berry);
}

.req-status {
  font-size: 11px;
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 0.5px;
  padding: 2px 10px;
  border-radius: 999px;
  width: fit-content;
}

/* Statuss: gaida apstiprinājumu */
.status-pending {
  background: #fff3cd;
  color: #856404;
}

/* Statuss: apstiprināts */
.status-approved {
  background: #d4edda;
  color: #155724;
}

/* Statuss: noraidīts */
.status-rejected {
  background: #f8d7da;
  color: #721c24;
}

/* Noklusējuma statusa stils */
.status-default {
  background: var(--color-bg-muted);
  color: var(--color-text-dim);
}

/* Poga detaļu apskatei */
.btn-view {
  flex-shrink: 0;
  background: transparent;
  border: 1.5px solid var(--brand-berry);
  color: var(--brand-berry);
  border-radius: 10px;
  padding: 7px 14px;
  font-size: 13px;
  font-weight: 600;
  cursor: pointer;
  transition: background 0.2s, color 0.2s, transform 0.2s;
}

.btn-view:hover {
  background: var(--brand-berry);
  color: white;
  transform: translateY(-1px);
}

/* Placeholder saraksts */
.placeholder-list {
  display: flex;
  flex-direction: column;
  gap: 10px;
  margin-top: auto;
}

.placeholder-row {
  height: 16px;
  border-radius: 999px;
  background: linear-gradient(
    90deg,
    rgba(161, 41, 113, 0.08),
    rgba(43, 164, 146, 0.1)
  );
}

.placeholder-row.short {
  width: 65%;
}

/* Statusu krāsas */
.status-sent { background: #ddeeff; color: #1a4a8b; }
.status-approved { background: #d4efcc; color: #2a6e1a; }
.status-inprogress { background: #fff0cc; color: #7a5500; }
.status-done { background: #e0e0e0; color: #444; }

/* Ielādes un kļūdas ziņojumu stils */
.loading-msg,
.error-msg {
  color: var(--brand-berry);
  font-size: 15px;
  z-index: 2;
}

/* Zvaigžņu vērtējuma stils */
.rating-stars {
  font-size: 15px;
  font-weight: 700;
  color: #f2b01e;
  letter-spacing: 1px;
}

/* Responsivitāte: planšetdatoriem un mazākiem ekrāniem */
@media (max-width: 1100px) {
  .shop-info,
  .shift-requests,
  .make-request,
  .rating-card {
    grid-column: span 6;
  }
}

/* Responsivitāte: mobilajām ierīcēm */
@media (max-width: 700px) {
  .cards-grid {
    grid-template-columns: 1fr;
  }

  .shop-info,
  .shift-requests,
  .make-request,
  .rating-card {
    grid-column: auto;
  }

  .card {
    min-height: unset;
  }

  .request-row {
    flex-direction: column;
    align-items: stretch;
  }

  .btn-view {
    align-self: flex-end;
  }

  h1 {
    font-size: 26px;
  }

  .card h2 {
    font-size: 22px;
  }
}
</style>
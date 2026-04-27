<template>
  <div class="page">
    <div class="container">
      <div class="blob blob-teal"></div>
      <div class="blob blob-pink"></div>

      <button class="logout-btn" @click="handleLogout">
        <FontAwesomeIcon icon="fa-solid fa-right-from-bracket" />
        Logout
      </button>

      <h1>Welcome to EETMS!</h1>

      <div v-if="isLoading" class="loading-msg">Loading shop info...</div>

      <div v-else-if="shop" class="cards-row">

        <div class="card shop-info">
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
          <div class="info-row">
            <span class="label">Total requested shifts: </span>
            <span class="value">—</span>
          </div>
        </div>

        <div class="card shift-requests">
          <h2>Relevant shift requests</h2>
          <div v-if="shiftRequests.length === 0" class="empty-msg">
            No relevant shift requests
          </div>
          <div v-else>
            <div
              v-for="req in shiftRequests"
              :key="req.shiftRequestID"
              class="request-item"
              @click="selectedRequest = req"
              style="cursor: pointer;"
            >
              <span class="req-reason">{{ req.reason?.name }}</span>
              <span class="req-status">{{ req.status }}</span>
            </div>
          </div>
        </div>

        <div class="card make-request">
          <h2>Make request</h2>
          <button class="plus-btn" @click="showModal = true">+</button>
        </div>

        <ShiftRequestModal
          v-if="showModal"
          :shopId="Number(shopId)"
          @close="showModal = false"
          @submitted="refreshRequests"
        />

        <ShiftRequestDetailModal
          v-if="selectedRequest"
          :request="selectedRequest"
          @close="selectedRequest = null"
        />
      </div>

      <div v-else class="error-msg">Failed to load shop info.</div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { logout } from '@/services/auth.js'
import { FontAwesomeIcon } from '@fortawesome/vue-fontawesome'
import ShiftRequestModal from '../components/ShiftRequestModal.vue'
import ShiftRequestDetailModal from '../components/ShiftRequestDetailModal.vue'

const showModal = ref(false)
const shiftRequests = ref([])

const API_BASE = 'http://localhost:5001/api'
const router = useRouter()

const shop = ref(null)
const totalExternalShifts = ref(0)
const isLoading = ref(true)
const shopId = ref(null)

const selectedRequest = ref(null)

const countryMap = {
  1: 'Lithuania',
  2: 'Latvia',
  3: 'Estonia'
}

const typeMap = {
  1: 'Hyper',
  2: 'Super',
  3: 'Mini',
  4: 'Express'
}

const handleLogout = () => {
  logout()
  router.push('/roleselect')
}

async function refreshRequests() {
  const res = await fetch(`${API_BASE}/shiftrequests/byshop/${shopId.value}`)
  if (res.ok) shiftRequests.value = await res.json()
}

onMounted(async () => {
  shopId.value = localStorage.getItem('shopId')

  if (!shopId.value) {
    router.push('/login')
    return
  }

  try {
    const shopRes = await fetch(`${API_BASE}/shops/${shopId.value}`)
    if (!shopRes.ok) throw new Error('Failed to fetch shop')
    shop.value = await shopRes.json()

    const shiftsRes = await fetch(`${API_BASE}/shifts/byshop/${shopId.value}`)
    if (shiftsRes.ok) {
      const shifts = await shiftsRes.json()
      totalExternalShifts.value = shifts.length
    }

    const reqRes = await fetch(`${API_BASE}/shiftrequests/byshop/${shopId.value}`)
    if (reqRes.ok) shiftRequests.value = await reqRes.json()

  } catch (err) {
    console.error('Error loading shop home:', err)
    shop.value = null
  } finally {
    isLoading.value = false
  }
})
</script>

<style scoped>
* {
  margin: 0;
  padding: 0;
  box-sizing: border-box;
  font-family: 'Segoe UI', sans-serif;
}

.page {
  min-height: 100vh;
  display: flex;
  flex-direction: column;
}

.container {
  flex: 1;
  display: flex;
  flex-direction: column;
  align-items: center;
  padding: 30px 20px;
}

h1 {
  color: var(--brand-berry);
  font-size: 26px;
  font-weight: 800;
  margin-bottom: 28px;
}

.cards-row {
  display: flex;
  gap: 20px;
  width: 100%;
  max-width: 900px;
  align-items: flex-start;
  z-index: 2;
}

.card {
  background: rgba(255, 255, 255, 0.5);
  backdrop-filter: blur(24px);
  -webkit-backdrop-filter: blur(24px);
  border: 1.5px solid rgba(255, 255, 255, 0.6);
  box-shadow: 0 8px 32px rgba(0, 0, 0, 0.08);
  border-radius: 20px;
  padding: 22px 24px;
  flex: 1;
}

.card .shop-info {
  padding: 30px;
}

.card h2 {
  color: var(--brand-berry);
  font-size: 24px;
  font-weight: 700;
  margin-bottom: 18px;
}

.info-row {
  margin-bottom: 10px;
  font-size: 14px;
}

.label {
  font-weight: 900;
  color: var(--brand-berry);
}

.value {
  color: var(--brand-berry);
}

.right-col {
  display: flex;
  flex-direction: column;
  gap: 16px;
  flex: 1;
}

.shift-requests {
  min-height: 120px;
  display: flex;
  flex-direction: column;
}

.empty-msg {
  flex: 1;
  display: flex;
  align-items: center;
  justify-content: center;
  color: #aaa;
  font-size: 13px;
  font-style: italic;
  margin-top: 10px;
}

.make-request {
  display: flex;
  flex-direction: column;
  align-items: center;
  padding: 16px;
}

.make-request h2 {
  margin-bottom: 10px;
}

.plus-btn {
  width: 42px;
  height: 42px;
  border-radius: 50%;
  border: 2px solid #4caf87;
  background: transparent;
  color: #4caf87;
  font-size: 28px;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: background 0.2s;
  padding-bottom: 5px;
}

.plus-btn:hover {
  background: rgba(76, 175, 135, 0.15);
}
</style>
<template>
  <div class="modal-overlay" @click.self="$emit('close')">
    <div class="modal-wrapper">
      <div class="modal">
        <button class="modal-close" @click="$emit('close')">✕</button>

        <div class="shop-info-card" v-if="request.shop">
          <h2 class="shop-code">{{ request.shop.code }}</h2>

          <div class="info-row">
            <span class="label">Type:</span>
            <span class="value">{{ typeMap[request.shop.type] ?? request.shop.type ?? '—' }}</span>
          </div>

          <div class="info-row">
            <span class="label">Country:</span>
            <span class="value">{{ countryMap[request.shop.country] ?? request.shop.country ?? '—' }}</span>
          </div>

          <div class="info-row">
            <span class="label">Address:</span>
            <span class="value">{{ request.shop.address ?? '—' }}</span>
          </div>
        </div>

        <div class="request-details">
          <h3>Shift Request</h3>

          <div class="detail-row">
            <span class="label">Reason:</span>
            <span class="value">{{ request.reason?.name ?? '—' }}</span>
          </div>

          <div class="detail-row">
            <span class="label">Status:</span>
            <span class="value">{{ statusMap[localStatus] ?? localStatus }}</span>
          </div>

          <div class="detail-row">
            <span class="label">Description:</span>
            <span class="value">{{ request.description ?? '—' }}</span>
          </div>

          <div class="detail-row">
            <span class="label">Company:</span>

            <template v-if="canEdit">
              <select v-model.number="selectedCompanyId" class="edit-select">
                <option :value="null">— None —</option>
                <option
                  v-for="company in eligibleCompanies"
                  :key="company.id"
                  :value="company.id"
                >
                  {{ company.companyName }}
                </option>
              </select>
            </template>

            <template v-else>
              <span class="value">{{ request.company?.companyName ?? '—' }}</span>
            </template>
          </div>

          <div
            v-if="request.plannedShift && localStatus >= 3"
            class="detail-row"
          >
            <span class="label">Planned Shift:</span>
            <span class="value">
              {{ formattedExistingPlannedDate || '—' }}
              <template v-if="formattedExistingPlannedTime">
                , {{ formattedExistingPlannedTime }}
              </template>
              <template v-if="request.plannedShift?.approx_duration">
                , {{ request.plannedShift.approx_duration }}h
              </template>
            </span>
          </div>
        </div>

        <div class="modal-actions">
          <button class="btn-secondary" @click="$emit('close')">Close</button>

          <button
            v-if="canAccept"
            class="btn-accept"
            @click="acceptRequest"
            :disabled="isSaving"
          >
            {{ isSaving ? 'Accepting...' : 'Accept' }}
          </button>

          <template v-if="canEdit">
            <button
              v-if="localStatus === 2"
              class="btn-progress"
              @click="updateStatus(3)"
              :disabled="isSaving"
            >
              {{ isSaving ? 'Saving...' : 'Change to In Progress' }}
            </button>

            <button
              v-if="localStatus === 3"
              class="btn-plan"
              @click="showPlanPanel = !showPlanPanel"
              :disabled="isSaving"
            >
              {{ showPlanPanel ? 'Hide Plan Shift' : 'Plan Shift' }}
            </button>

            <button
              v-if="localStatus === 3"
              class="btn-done"
              @click="updateStatus(4)"
              :disabled="isSaving"
            >
              {{ isSaving ? 'Saving...' : 'Change to Done' }}
            </button>

            <button
              v-if="companyChanged"
              class="btn-save"
              @click="saveCompany"
              :disabled="isSaving"
            >
              {{ isSaving ? 'Saving...' : 'Save Company' }}
            </button>
          </template>
        </div>
      </div>

      <transition name="slide">
        <div class="side-panel" v-if="showPlanPanel && localStatus === 3">
          <div class="modal-header">
            <h2>Plan Shift</h2>
            <p class="date-label" v-if="plannedDate">
              {{ formattedPlannedDate }}
              <span v-if="plannedStartTime"> {{ formatHHMM(plannedStartTime) }}</span>
            </p>
          </div>

          <div class="punch-form">
            <div class="form-group">
              <label>Approx. Date</label>
              <FlatPickr
                v-model="plannedDate"
                :config="dateConfig"
                class="form-input"
              />
            </div>

            <div class="form-group">
              <label>Approx. Start Time</label>
              <FlatPickr
                v-model="plannedStartTime"
                :config="timeConfig"
                class="form-input"
              />
            </div>

            <div class="form-group">
              <label>Approx. Duration (hours)</label>
              <input
                v-model.number="plannedDuration"
                type="number"
                min="1"
                class="form-input"
              />
            </div>

            <div class="form-actions">
              <button class="btn-cancel" @click="showPlanPanel = false">Cancel</button>
              <button class="btn-save-plan" @click="savePlannedShift" :disabled="isSaving">
                {{ isSaving ? 'Saving...' : 'Save' }}
              </button>
            </div>
          </div>
        </div>
      </transition>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, watch } from 'vue'
import FlatPickr from 'vue-flatpickr-component'
import 'flatpickr/dist/flatpickr.css'

const props = defineProps({
  request: { type: Object, required: true },
  remId: { type: Number, required: true },
  companies: { type: Array, default: () => [] },
  companyReasons: { type: Array, default: () => [] }
})

const emit = defineEmits(['close', 'accepted', 'updated'])

const API_BASE = 'http://localhost:5002/api'
const isSaving = ref(false)

const localStatus = ref(Number(props.request.status ?? 1))
const selectedCompanyId = ref(props.request.companyId ?? null)
const showPlanPanel = ref(false)

const plannedDate = ref('')
const plannedStartTime = ref('')
const plannedDuration = ref(8)

const statusMap = {
  1: 'Sent',
  2: 'Approved',
  3: 'In Progress',
  4: 'Done'
}

const typeMap = {
  1: 'Hyper',
  2: 'Super',
  3: 'Mini',
  4: 'Express'
}

const countryMap = {
  1: 'Lithuania',
  2: 'Latvia',
  3: 'Estonia',
  4: 'Baltics'
}

const dateConfig = {
  altInput: true,
  altFormat: 'd.m.Y',
  dateFormat: 'Y-m-d',
  allowInput: true,
  altInputClass: 'form-input',
  minDate: 'today'
}

const timeConfig = computed(() => {
  const now = new Date()

  const yyyy = now.getFullYear()
  const mm = String(now.getMonth() + 1).padStart(2, '0')
  const dd = String(now.getDate()).padStart(2, '0')
  const todayStr = `${yyyy}-${mm}-${dd}`

  const currentHHMM = `${String(now.getHours()).padStart(2, '0')}:${String(now.getMinutes()).padStart(2, '0')}`

  return {
    enableTime: true,
    noCalendar: true,
    dateFormat: 'H:i',
    time_24hr: true,
    allowInput: true,
    minuteIncrement: 1,
    minTime: plannedDate.value === todayStr ? currentHHMM : '00:00'
  }
})

const formatHHMM = (t) => (t ? String(t).slice(0, 5) : '')

const toLocalDateTime = (dateStr, timeStr) => {
  if (!dateStr || !timeStr) return null
  const normalizedTime = String(timeStr).slice(0, 5)
  const dt = new Date(`${dateStr}T${normalizedTime}:00`)
  return Number.isNaN(dt.getTime()) ? null : dt
}

const isPlannedShiftInPast = () => {
  const planned = toLocalDateTime(plannedDate.value, plannedStartTime.value)
  if (!planned) return false
  return planned < new Date()
}

watch(
  () => props.request,
  (req) => {
    if (!req) return
    localStatus.value = Number(req.status ?? 1)
    selectedCompanyId.value = req.companyId ?? null
    plannedDate.value = req.plannedShift?.approx_date ?? ''
    plannedStartTime.value = req.plannedShift?.approx_start_time
      ? String(req.plannedShift.approx_start_time).slice(0, 5)
      : ''
    plannedDuration.value = req.plannedShift?.approx_duration ?? 8
  },
  { immediate: true }
)

const formattedPlannedDate = computed(() => {
  if (!plannedDate.value) return ''
  return new Date(plannedDate.value).toLocaleDateString('de-DE', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric'
  })
})

const formattedExistingPlannedDate = computed(() => {
  const d = props.request?.plannedShift?.approx_date
  if (!d) return ''
  return new Date(d).toLocaleDateString('de-DE', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric'
  })
})

const formattedExistingPlannedTime = computed(() => {
  return formatHHMM(props.request?.plannedShift?.approx_start_time)
})

const canAccept = computed(() => {
  return localStatus.value === 1 && !props.request.remId
})

const canEdit = computed(() => {
  return props.request.remId === props.remId
})

const eligibleCompanies = computed(() => {
  const companiesList = Array.isArray(props.companies) ? props.companies : []
  const companyReasonsList = Array.isArray(props.companyReasons) ? props.companyReasons : []

  return companiesList.filter(company =>
    companyReasonsList.some(cr =>
      cr.companiesCompanyID === company.id &&
      cr.reasonsReasonID === props.request.reasonID
    )
  )
})

const companyChanged = computed(() => {
  return selectedCompanyId.value !== (props.request.companyId ?? null)
})

async function acceptRequest() {
  isSaving.value = true
  try {
    const res = await fetch(`${API_BASE}/shiftrequests/${props.request.shiftRequestID}/accept`, {
      method: 'PATCH',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ remId: props.remId })
    })

    if (!res.ok) throw new Error('Failed to accept request')

    localStatus.value = 2
    props.request.remId = props.remId
    props.request.status = 2

    emit('accepted')
    emit('updated')
  } catch (err) {
    console.error(err)
  } finally {
    isSaving.value = false
  }
}

async function updateStatus(newStatus) {
  isSaving.value = true
  try {
    const res = await fetch(`${API_BASE}/shiftrequests/${props.request.shiftRequestID}`, {
      method: 'PATCH',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        status: newStatus,
        companyId: selectedCompanyId.value ? Number(selectedCompanyId.value) : null
      })
    })

    if (!res.ok) throw new Error('Failed to update status')

    localStatus.value = newStatus
    props.request.status = newStatus
    emit('updated')
  } catch (err) {
    console.error(err)
  } finally {
    isSaving.value = false
  }
}

async function saveCompany() {
  isSaving.value = true
  try {
    const res = await fetch(`${API_BASE}/shiftrequests/${props.request.shiftRequestID}`, {
      method: 'PATCH',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        status: localStatus.value,
        companyId: selectedCompanyId.value ? Number(selectedCompanyId.value) : null
      })
    })

    if (!res.ok) throw new Error('Failed to update company')

    props.request.companyId = selectedCompanyId.value

    const matchedCompany = eligibleCompanies.value.find(c => c.id === selectedCompanyId.value)
    props.request.company = matchedCompany
      ? { ...props.request.company, companyName: matchedCompany.companyName }
      : null

    emit('updated')
  } catch (err) {
    console.error(err)
  } finally {
    isSaving.value = false
  }
}

async function savePlannedShift() {
  if (!plannedDate.value || !plannedStartTime.value || !plannedDuration.value) return
  if (isPlannedShiftInPast()) {
        alert('Planned shift cannot be set in the past.')
        return
    }

  isSaving.value = true
  try {
    const res = await fetch(`${API_BASE}/shiftrequests/${props.request.shiftRequestID}/planned-shift`, {
      method: 'PATCH',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        approxDate: plannedDate.value,
        approxStartTime: `${plannedStartTime.value}:00`,
        approxDuration: Number(plannedDuration.value)
      })
    })

    if (!res.ok) throw new Error('Failed to save planned shift')

    props.request.plannedShift = {
      ...(props.request.plannedShift ?? {}),
      approx_date: plannedDate.value,
      approx_start_time: `${plannedStartTime.value}:00`,
      approx_duration: Number(plannedDuration.value)
    }

    showPlanPanel.value = false
    emit('updated')
  } catch (err) {
    console.error(err)
  } finally {
    isSaving.value = false
  }
}
</script>

<style scoped>
.modal-overlay {
  position: fixed;
  inset: 0;
  background: rgba(30, 5, 20, 0.45);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 500;
  backdrop-filter: blur(4px);
}

.modal-wrapper {
  display: flex;
  gap: 16px;
  align-items: flex-start;
  max-width: 1020px;
  width: 100%;
  padding: 0 16px;
}

.modal {
  width: 100%;
  max-width: 520px;
  background: rgba(255,255,255,0.92);
  backdrop-filter: blur(24px);
  -webkit-backdrop-filter: blur(24px);
  border: 1.5px solid rgba(255,255,255,0.7);
  border-radius: 22px;
  padding: 26px;
  box-shadow: 0 12px 40px rgba(0,0,0,0.14);
  position: relative;
}

.modal-close {
  position: absolute;
  top: 14px;
  right: 14px;
  border: none;
  background: transparent;
  color: var(--brand-berry);
  font-size: 20px;
  cursor: pointer;
}

.shop-info-card {
  background: rgba(255, 255, 255, 0.62);
  backdrop-filter: blur(12px);
  -webkit-backdrop-filter: blur(12px);
  border: 1.5px solid rgba(255, 255, 255, 0.82);
  border-radius: 18px;
  padding: 18px 20px;
  box-shadow: 0 2px 10px rgba(0,0,0,0.05);
  margin-bottom: 18px;
}

.shop-code {
  margin: 0 0 14px;
  color: var(--brand-berry);
  font-size: 24px;
  font-weight: 800;
}

.request-details h3 {
  margin: 0 0 12px;
  color: var(--brand-berry);
  font-size: 22px;
  font-weight: 800;
}

.info-row,
.detail-row {
  display: flex;
  flex-direction: column;
  gap: 4px;
  padding: 8px 0;
  border-bottom: 1px solid rgba(161, 41, 113, 0.08);
}

.info-row:last-child,
.detail-row:last-child {
  border-bottom: none;
}

.label {
  font-size: 12px;
  font-weight: 700;
  letter-spacing: 0.05em;
  text-transform: uppercase;
  color: #8a7a84;
}

.value {
  font-size: 15px;
  color: var(--brand-berry);
  font-weight: 600;
}

.edit-select {
  width: 100%;
  border: 1.5px solid rgba(161, 41, 113, 0.14);
  border-radius: 10px;
  padding: 10px 12px;
  font-size: 14px;
  color: var(--brand-berry);
  font-family: 'Inter', sans-serif;
  background: rgba(255,255,255,0.85);
  outline: none;
}

.modal-actions {
  display: flex;
  justify-content: flex-end;
  gap: 10px;
  margin-top: 22px;
  flex-wrap: wrap;
}

.btn-secondary,
.btn-save,
.btn-accept,
.btn-progress,
.btn-done,
.btn-plan {
  padding: 10px 18px;
  border-radius: 999px;
  border: none;
  font-weight: 700;
  cursor: pointer;
  font-family: 'Inter', sans-serif;
  transition: all 0.2s ease;
}

.btn-secondary {
  border: 1.5px solid rgba(161, 41, 113, 0.25);
  background: white;
  color: var(--brand-berry);
}

.btn-save {
  background: var(--brand-berry);
  color: white;
  min-width: 110px;
}

.btn-accept {
  background: var(--brand-teal);
  color: white;
}

.btn-progress {
  background: #f59e0b;
  color: white;
}

.btn-done {
  background: #10b981;
  color: white;
}

.btn-plan {
  background: linear-gradient(135deg, #a12971, #2ba492);
  color: white;
}

.btn-save:disabled,
.btn-accept:disabled,
.btn-progress:disabled,
.btn-done:disabled,
.btn-plan:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.side-panel {
  background: white;
  border-radius: 16px;
  padding: 40px;
  width: 100%;
  max-width: 380px;
  flex-shrink: 0;
  box-shadow: 0 10px 40px rgba(0, 0, 0, 0.3);
}

.modal-header {
  margin-bottom: 30px;
}

.modal-header h2 {
  font-family: 'Inter', sans-serif;
  font-size: 28px;
  font-weight: bold;
  background: linear-gradient(135deg, #a12971, #2ba492);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  background-clip: text;
  margin: 0 0 10px 0;
}

.date-label {
  font-family: 'Inter', sans-serif;
  font-size: 16px;
  color: #666;
  margin: 0;
}

.punch-form {
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.form-group {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.form-group label {
  font-family: 'Inter', sans-serif;
  font-size: 14px;
  font-weight: bold;
  color: #333;
}

.form-input {
  font-family: 'Inter', sans-serif;
  font-size: 16px;
  padding: 12px 16px;
  border: 2px solid #e0e0e0;
  border-radius: 8px;
  outline: none;
  transition: all 0.2s ease;
}

.form-input:focus {
  border-color: #a12971;
  box-shadow: 0 0 0 3px rgba(161, 41, 113, 0.1);
}

.form-actions {
  display: flex;
  gap: 12px;
  margin-top: 10px;
}

.btn-cancel,
.btn-save-plan {
  flex: 1;
  font-family: 'Inter', sans-serif;
  font-size: 16px;
  font-weight: 600;
  padding: 14px 24px;
  border-radius: 8px;
  border: none;
  cursor: pointer;
  transition: all 0.3s ease;
}

.btn-cancel {
  background: #f5f5f5;
  color: #666;
}

.btn-cancel:hover {
  background: #e0e0e0;
}

.btn-save-plan {
  background: linear-gradient(135deg, #a12971, #96537b);
  color: white;
}

.btn-save-plan:hover {
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(161, 41, 113, 0.3);
}

.btn-save-plan:disabled {
  opacity: 0.6;
  cursor: not-allowed;
  transform: none;
}

:deep(.form-input) {
  font-family: 'Inter', sans-serif;
  font-size: 16px;
  padding: 12px 16px;
  border: 2px solid #e0e0e0;
  border-radius: 8px;
  outline: none;
  transition: all 0.2s ease;
}

.slide-enter-active,
.slide-leave-active {
  transition: all 0.25s ease;
}

.slide-enter-from,
.slide-leave-to {
  opacity: 0;
  transform: translateX(30px);
}

@media (max-width: 768px) {
  .modal-wrapper {
    flex-direction: column;
    align-items: stretch;
  }

  .modal,
  .side-panel {
    max-width: 100%;
  }

  .side-panel {
    padding: 30px 20px;
  }

  .modal-header h2 {
    font-size: 24px;
  }

  .date-label {
    font-size: 14px;
  }

  .form-input {
    font-size: 14px;
  }
}

@media (max-width: 480px) {
  .modal {
    padding: 25px 20px;
  }

  .side-panel {
    padding: 25px 20px;
  }

  .modal-header h2 {
    font-size: 22px;
  }

  .btn-cancel,
  .btn-save-plan {
    font-size: 14px;
    padding: 12px 20px;
  }
}
</style>
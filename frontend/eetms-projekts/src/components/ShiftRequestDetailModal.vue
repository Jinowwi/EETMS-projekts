<template>
  <div class="modal-overlay" @click.self="$emit('close')">
    <div class="modal">
      <button class="modal-close" @click="$emit('close')">✕</button>

      <h2>Shift Request Details</h2>

      <div class="detail-row">
        <span class="label">Reason:</span>
        <span class="value">{{ request.reason?.name ?? '—' }}</span>
      </div>

      <div class="detail-row">
        <span class="label">Status:</span>
        <span class="value status-badge" :class="statusInfo.cls">{{ statusInfo.label }}</span>
      </div>

      <div class="detail-row">
        <span class="label">Description:</span>
        <span class="value">{{ request.description ?? '—' }}</span>
      </div>

      <div class="detail-row">
        <span class="label">Company:</span>
        <span class="value">{{ request.company?.companyName ?? '—' }}</span>
      </div>

      <div class="detail-row">
        <span class="label">Assigned to:</span>
        <span class="value">{{ request.rem?.email ?? '—' }}</span>
      </div>

      <div class="detail-row">
        <span class="label">Shop:</span>
        <span class="value">{{ request.shop?.shopName ?? request.shop?.name ?? '—' }}</span>
      </div>

      <div class="planned-section">
        <h3>Planned shift</h3>

        <template v-if="hasPlannedShift">
          <div class="detail-row">
            <span class="label">Date:</span>
            <span class="value">{{ formatDate(request.plannedShift?.approx_date) }}</span>
          </div>

          <div class="detail-row">
            <span class="label">Start:</span>
            <span class="value">{{ formatTime(request.plannedShift?.approx_start_time) }}</span>
          </div>

          <div class="detail-row">
            <span class="label">Duration:</span>
            <span class="value">{{ formatDuration(request.plannedShift?.approx_duration) }}</span>
          </div>

          <div class="detail-row">
            <span class="label">Ends at:</span>
            <span class="value">{{ plannedShiftEndLabel }}</span>
          </div>

          <div class="detail-row">
            <span class="label">Availability:</span>
            <span class="value">
              <span
                class="status-badge"
                :class="isPlannedShiftFinished ? 'status-done' : 'status-inprogress'"
              >
                {{ isPlannedShiftFinished ? 'Shift time passed' : 'Still in progress' }}
              </span>
            </span>
          </div>
        </template>

        <div v-else class="no-planned-shift">
          No planned shift added yet.
        </div>
      </div>

      <div class="modal-actions">
        <button
          v-if="request.status === 1"
          class="btn-cancel-request"
          @click="showCancelConfirm = true"
        >
          Cancel request
        </button>

        <button
          v-if="canMarkDone"
          class="btn-done"
          @click="markAsDone"
          :disabled="isSavingDone"
        >
          {{ isSavingDone ? 'Saving...' : 'Mark as Done' }}
        </button>
      </div>

      <p
        v-if="showDoneHint"
        class="done-hint"
      >
        This can be marked as done after the planned shift end time has passed.
      </p>

      <div v-if="showCancelConfirm" class="confirm-overlay">
        <div class="confirm-box">
          <h3>Cancel shift request?</h3>
          <p>This action will permanently delete the request.</p>
          <div class="confirm-actions">
            <button class="btn-secondary" @click="showCancelConfirm = false">
              Keep
            </button>
            <button class="btn-danger" @click="deleteRequest" :disabled="isDeleting">
              {{ isDeleting ? 'Deleting...' : 'Delete' }}
            </button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { computed, ref } from 'vue'

const props = defineProps({
  request: {
    type: Object,
    required: true
  }
})

const emit = defineEmits(['close', 'deleted', 'updated'])

const API_BASE = 'http://localhost:5001/api'
const showCancelConfirm = ref(false)
const isDeleting = ref(false)
const isSavingDone = ref(false)

const statusMap = {
  1: { label: 'Sent', cls: 'status-sent' },
  2: { label: 'Approved', cls: 'status-approved' },
  3: { label: 'In Progress', cls: 'status-inprogress' },
  4: { label: 'Done', cls: 'status-done' }
}

const statusInfo = computed(() =>
  statusMap[props.request.status] ?? { label: props.request.status, cls: '' }
)

const plannedShift = computed(() =>
  props.request.plannedShift ?? props.request.planned_Shift ?? null
)

const hasPlannedShift = computed(() => {
  return !!(
    plannedShift.value &&
    plannedShift.value.approx_date &&
    plannedShift.value.approx_start_time &&
    plannedShift.value.approx_duration != null
  )
})

function normalizeTimeString(timeValue) {
  if (!timeValue) return null
  const raw = String(timeValue).trim()
  return raw.length >= 5 ? raw.slice(0, 5) : raw
}

function parseDurationToMinutes(durationValue) {
  if (durationValue == null) return null

  if (typeof durationValue === 'number' && !Number.isNaN(durationValue)) {
    return durationValue
  }

  const raw = String(durationValue).trim()

  if (/^\d+$/.test(raw)) {
    return Number(raw)
  }

  const hhmmss = raw.match(/^(\d{1,2}):(\d{2})(?::(\d{2}))?$/)
  if (hhmmss) {
    const hours = Number(hhmmss[1] || 0)
    const minutes = Number(hhmmss[2] || 0)
    return hours * 60 + minutes
  }

  return null
}

function buildLocalDateTime(dateValue, timeValue) {
  if (!dateValue || !timeValue) return null

  const datePart = String(dateValue).slice(0, 10)
  const timePart = normalizeTimeString(timeValue)

  if (!datePart || !timePart) return null

  const dt = new Date(`${datePart}T${timePart}:00`)
  return Number.isNaN(dt.getTime()) ? null : dt
}

const plannedShiftStart = computed(() => {
  if (!hasPlannedShift.value) return null
  return buildLocalDateTime(
    plannedShift.value.approx_date,
    plannedShift.value.approx_start_time
  )
})

const plannedShiftEnd = computed(() => {
  if (!plannedShiftStart.value) return null

  const minutes = parseDurationToMinutes(plannedShift.value.approx_duration)
  if (minutes == null) return null

  return new Date(plannedShiftStart.value.getTime() + minutes * 60000)
})

const isPlannedShiftFinished = computed(() => {
  if (!plannedShiftEnd.value) return false
  return new Date() >= plannedShiftEnd.value
})

const canMarkDone = computed(() => {
  return (
    props.request.status === 3 &&
    hasPlannedShift.value &&
    isPlannedShiftFinished.value
  )
})

const showDoneHint = computed(() => {
  return (
    props.request.status === 3 &&
    hasPlannedShift.value &&
    !isPlannedShiftFinished.value
  )
})

const plannedShiftEndLabel = computed(() => {
  if (!plannedShiftEnd.value) return '—'
  return plannedShiftEnd.value.toLocaleString('de-DE', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  })
})

function formatDate(dateLike) {
  if (!dateLike) return '—'
  const d = new Date(String(dateLike))
  if (Number.isNaN(d.getTime())) return String(dateLike)
  return d.toLocaleDateString('de-DE', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric'
  })
}

function formatTime(timeLike) {
  if (!timeLike) return '—'
  return String(timeLike).slice(0, 5)
}

function formatDuration(durationValue) {
  const totalMinutes = parseDurationToMinutes(durationValue)
  if (totalMinutes == null) return String(durationValue ?? '—')

  const hours = Math.floor(totalMinutes / 60)
  const minutes = totalMinutes % 60

  if (hours > 0 && minutes > 0) return `${hours}h ${minutes}m`
  if (hours > 0) return `${hours}h`
  return `${minutes}m`
}

async function deleteRequest() {
  isDeleting.value = true
  try {
    const res = await fetch(`${API_BASE}/shiftrequests/${props.request.shiftRequestID}`, {
      method: 'DELETE'
    })

    if (!res.ok) {
      throw new Error('Failed to delete request')
    }

    emit('deleted')
    emit('close')
  } catch (err) {
    console.error(err)
  } finally {
    isDeleting.value = false
    showCancelConfirm.value = false
  }
}

async function markAsDone() {
  if (!canMarkDone.value) return

  isSavingDone.value = true
  try {
    const res = await fetch(`${API_BASE}/shiftrequests/${props.request.shiftRequestID}`, {
      method: 'PATCH',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({
        status: 4,
        companyId: props.request.companyId ?? props.request.company?.companyID ?? null
      })
    })

    if (!res.ok) {
      throw new Error('Failed to update request status')
    }

    emit('updated')
    emit('close')
  } catch (err) {
    console.error(err)
  } finally {
    isSavingDone.value = false
  }
}
</script>

<style scoped>
.modal-overlay {
  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, 0.45);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
}

.modal {
  background: var(--color-surface, #fff);
  border-radius: 12px;
  padding: 2rem;
  min-width: 340px;
  max-width: 560px;
  width: 100%;
  position: relative;
  box-shadow: 0 8px 32px rgba(0, 0, 0, 0.18);
}

.modal-close {
  position: absolute;
  top: 1rem;
  right: 1rem;
  background: none;
  border: none;
  font-size: 1.1rem;
  cursor: pointer;
  color: var(--brand-berry);
  line-height: 1;
}

.modal-close:hover {
  color: var(--color-text, #222);
}

h2 {
  margin-bottom: 1.25rem;
  font-size: 1.2rem;
}

h3 {
  margin: 0 0 0.75rem;
  font-size: 1rem;
  color: var(--brand-berry);
}

.detail-row {
  display: flex;
  gap: 0.75rem;
  padding: 0.5rem 0;
  border-bottom: 1px solid var(--color-divider, #eee);
}

.label {
  font-weight: 600;
  min-width: 100px;
  color: var(--brand-berry);
}

.value {
  color: var(--color-text, #222);
}

.status-badge {
  padding: 0.15rem 0.6rem;
  border-radius: 999px;
  font-size: 0.85rem;
  font-weight: 600;
}

.status-sent { background: #ddeeff; color: #1a4a8b; }
.status-approved { background: #d4efcc; color: #2a6e1a; }
.status-inprogress { background: #fff0cc; color: #7a5500; }
.status-done { background: #e0e0e0; color: #444; }

.planned-section {
  margin-top: 1.25rem;
  padding-top: 0.5rem;
}

.no-planned-shift {
  margin-top: 0.5rem;
  color: var(--color-text-dim, #777);
  font-size: 0.95rem;
}

.modal-actions {
  display: flex;
  justify-content: flex-end;
  gap: 10px;
  margin-top: 18px;
  flex-wrap: wrap;
}

.btn-cancel-request {
  background: #fff1f3;
  color: #b42318;
  border: 1px solid #f3c7cd;
  border-radius: 10px;
  padding: 9px 14px;
  font-size: 14px;
  font-weight: 600;
  cursor: pointer;
}

.btn-cancel-request:hover {
  background: #fdecee;
}

.btn-done {
  background: #1f7a1f;
  color: white;
  border: none;
  border-radius: 10px;
  padding: 9px 14px;
  font-size: 14px;
  font-weight: 600;
  cursor: pointer;
}

.btn-done:hover {
  background: #176417;
}

.btn-done:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.done-hint {
  margin-top: 10px;
  font-size: 13px;
  color: var(--color-text-dim, #777);
}

.confirm-overlay {
  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, 0.28);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 300;
}

.confirm-box {
  width: 340px;
  background: white;
  border-radius: 18px;
  padding: 22px;
  box-shadow: 0 12px 32px rgba(0, 0, 0, 0.16);
}

.confirm-box h3 {
  color: var(--brand-berry);
  font-size: 20px;
  margin-bottom: 8px;
}

.confirm-box p {
  color: var(--color-text-dim, #666);
  font-size: 14px;
  margin-bottom: 18px;
}

.confirm-actions {
  display: flex;
  justify-content: flex-end;
  gap: 10px;
}

.btn-secondary {
  background: white;
  color: var(--color-text-dim, #666);
  border: 1px solid var(--color-border, #ddd);
  border-radius: 10px;
  padding: 8px 14px;
  cursor: pointer;
}

.btn-danger {
  background: #c62828;
  color: white;
  border: none;
  border-radius: 10px;
  padding: 8px 14px;
  cursor: pointer;
}

.btn-danger:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

@media (max-width: 640px) {
  .modal {
    min-width: unset;
    width: calc(100% - 24px);
    padding: 1.25rem;
  }

  .detail-row {
    flex-direction: column;
    gap: 0.25rem;
  }

  .label {
    min-width: unset;
  }
}
</style>
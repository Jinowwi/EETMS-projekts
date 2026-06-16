<template>
  <div class="modal-overlay" @click.self="$emit('close')">
    <div class="modal">
      <button class="modal-close" type="button" @click="$emit('close')">✕</button>

      <h2>{{ modalTitle }}</h2>

      <div class="detail-row">
        <span class="label">Reason:</span>
        <span class="value">{{ request.reason?.name ?? '—' }}</span>
      </div>

      <div class="detail-row">
        <span class="label">Company:</span>
        <span class="value">{{ request.company?.companyName ?? '—' }}</span>
      </div>

      <div class="detail-row">
        <span class="label">Assigned to:</span>
        <span class="value">{{ request.rem?.email ?? '—' }}</span>
      </div>

      <div class="rating-section">
        <p class="section-label">Stars</p>
        <div class="stars">
          <button
            v-for="n in 5"
            :key="n"
            type="button"
            class="star-btn"
            :class="{ active: n <= stars }"
            :disabled="isReadOnly"
            @click="setStars(n)"
          >
            ★
          </button>
        </div>
      </div>

      <div class="rating-section">
        <label class="section-label">Comment</label>
        <textarea
          v-model="comment"
          class="comment-input"
          :readonly="isReadOnly"
          rows="4"
          maxlength="1000"
          placeholder="Write your feedback here..."
        />
      </div>

      <div class="modal-actions">
        <button type="button" class="btn-secondary" @click="$emit('close')">
          Close
        </button>

        <button
          v-if="showEditButton"
          type="button"
          class="btn-secondary"
          @click="startEditing"
        >
          Edit
        </button>

        <!-- <button
          v-if="showCancelEditButton"
          type="button"
          class="btn-secondary"
          @click="cancelEditing"
        >
          Cancel edit
        </button> -->

        <button
          v-if="showSubmitButton"
          type="button"
          class="btn-primary"
          @click="submitRating"
          :disabled="isSaving || stars < 1"
        >
          {{ isSaving ? 'Saving...' : submitLabel }}
        </button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { computed, ref, watch } from 'vue'

const props = defineProps({
  request: {
    type: Object,
    required: true
  },
  allowEdit: {
    type: Boolean,
    default: false
  }
})

const emit = defineEmits(['close', 'submitted'])

const API_BASE = 'http://localhost:5002/api'

const stars = ref(0)
const comment = ref('')
const isSaving = ref(false)
const isEditing = ref(false)

const existingRating = computed(() => props.request?.rating ?? null)
const hasExistingRating = computed(() => !!existingRating.value)

const isReadOnly = computed(() => {
  if (!hasExistingRating.value) return false
  return !isEditing.value
})

const modalTitle = computed(() => {
  if (!hasExistingRating.value) return 'Rate Service'
  return isEditing.value ? 'Edit Rating' : 'Rating Details'
})

const submitLabel = computed(() => {
  return hasExistingRating.value ? 'Save changes' : 'Submit rating'
})

const showEditButton = computed(() => {
  return hasExistingRating.value && props.allowEdit && !isEditing.value
})

const showCancelEditButton = computed(() => {
  return hasExistingRating.value && isEditing.value
})

const showSubmitButton = computed(() => {
  if (!hasExistingRating.value) return true
  return isEditing.value
})

function syncFromRequest(req) {
  stars.value = Number(req?.rating?.stars ?? 0)
  comment.value = req?.rating?.comment ?? ''
  isEditing.value = false
}

watch(
  () => props.request,
  (req) => {
    syncFromRequest(req)
  },
  { immediate: true }
)

function setStars(n) {
  if (isReadOnly.value) return
  stars.value = n
}

function startEditing() {
  isEditing.value = true
}

function cancelEditing() {
  syncFromRequest(props.request)
}

async function submitRating() {
  if (stars.value < 1) return

  isSaving.value = true
  try {
    let res

    if (hasExistingRating.value) {
      res = await fetch(`${API_BASE}/ratings/${existingRating.value.ratingID}`, {
        method: 'PATCH',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          stars: stars.value,
          comment: comment.value,
          shiftRequestID: props.request.shiftRequestID
        })
      })
    } else {
      res = await fetch(`${API_BASE}/ratings`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          stars: stars.value,
          comment: comment.value,
          shiftRequestID: props.request.shiftRequestID
        })
      })
    }

    if (!res.ok) {
      throw new Error('Failed to save rating')
    }

    emit('submitted')
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
  background: rgba(0, 0, 0, 0.45);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1200;
}

.modal {
  background: white;
  border-radius: 18px;
  padding: 24px;
  width: min(100%, 520px);
  position: relative;
  box-shadow: 0 12px 36px rgba(0, 0, 0, 0.18);
}

.modal-close {
  position: absolute;
  top: 14px;
  right: 14px;
  background: none;
  border: none;
  color: var(--brand-berry);
  font-size: 18px;
  cursor: pointer;
}

h2 {
  color: var(--brand-berry);
  margin-bottom: 18px;
  font-size: 24px;
}

.detail-row {
  display: flex;
  gap: 8px;
  padding: 8px 0;
  border-bottom: 1px solid #eee;
}

.label {
  min-width: 92px;
  font-weight: 700;
  color: var(--brand-berry);
}

.value {
  color: #4b3951;
}

.rating-section {
  margin-top: 18px;
}

.section-label {
  display: block;
  margin-bottom: 10px;
  font-size: 14px;
  font-weight: 700;
  color: var(--brand-berry);
}

.stars {
  display: flex;
  gap: 6px;
}

.star-btn {
  border: none;
  background: transparent;
  font-size: 30px;
  line-height: 1;
  color: #d7cfd8;
  cursor: pointer;
  transition: transform 0.15s ease, color 0.15s ease;
}

.star-btn.active {
  color: #f2b01e;
}

.star-btn:hover:not(:disabled) {
  transform: scale(1.08);
}

.star-btn:disabled {
  cursor: default;
}

.comment-input {
  width: 90%;
  resize: vertical;
  min-height: 110px;
  border-radius: 12px;
  border: 1.5px solid #e2d8e4;
  padding: 12px 14px;
  font-size: 14px;
  color: #4b3951;
  outline: none;
}

.comment-input:focus {
  border-color: var(--brand-berry);
}

.comment-input:read-only {
  background: #faf8fb;
}

.modal-actions {
  margin-top: 22px;
  display: flex;
  justify-content: flex-end;
  gap: 10px;
  flex-wrap: wrap;
}

.btn-secondary {
  border: 1px solid #d8d0da;
  background: white;
  color: #6a5b70;
  border-radius: 10px;
  padding: 9px 14px;
  cursor: pointer;
}

.btn-primary {
  border: none;
  background: var(--brand-berry);
  color: white;
  border-radius: 10px;
  padding: 9px 14px;
  cursor: pointer;
}

.btn-primary:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}
</style>
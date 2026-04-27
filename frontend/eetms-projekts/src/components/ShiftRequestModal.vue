<template>
  <div class="modal-overlay" @click.self="$emit('close')">
    <div class="modal">
      <h2>New Shift Request</h2>

      <label>Reason</label>
      <select v-model="form.reasonID">
        <option v-for="r in reasons" :key="r.reasonID" :value="r.reasonID">
          {{ r.name }}
        </option>
      </select>
      <label>Company <span style="color:#aaa; font-weight:400">(optional)</span></label>
        <select v-model="form.companyId">
        <option :value="null">— None —</option>
        <option v-for="c in companies" :key="c.companyID" :value="c.companyID">
            {{ c.companyName }}
        </option>
        </select>

      <label>Description</label>
      <textarea v-model="form.description" placeholder="Describe your request..." ></textarea>

      <div class="modal-actions">
        <button class="btn-cancel" @click="$emit('close')">Cancel</button>
        <button class="btn-submit" @click="submit" :disabled="isSubmitting">
          {{ isSubmitting ? 'Sending...' : 'Send Request' }}
        </button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'

const props = defineProps({ shopId: Number })
const emit = defineEmits(['close', 'submitted'])

const API_BASE = 'http://localhost:5002/api'

const reasons = ref([])
const companies = ref([])
const isSubmitting = ref(false)
const form = ref({ description: '', reasonID: null, companyId: null })

onMounted(async () => {
  const res = await fetch(`${API_BASE}/reasons`)
  if (res.ok) reasons.value = await res.json()

  const compRes = await fetch(`${API_BASE}/companies`)
  if (compRes.ok) companies.value = await compRes.json()
})

async function submit() {
  if (!form.value.description || !form.value.reasonID) return

  isSubmitting.value = true
  try {
    const res = await fetch(`${API_BASE}/shiftrequests`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        description: form.value.description,
        reasonID: form.value.reasonID,
        shopId: props.shopId,
        companyId: form.value.companyId
      })
    })
    if (!res.ok) throw new Error('Failed to submit')
    emit('submitted')
    emit('close')
  } catch (err) {
    console.error(err)
  } finally {
    isSubmitting.value = false
  }
}
</script>

<style scoped>
.modal-overlay {
  position: fixed;
  inset: 0;
  background: rgba(0,0,0,0.3);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 100;
}
.modal {
  background: white;
  border-radius: 16px;
  padding: 28px;
  width: 380px;
  display: flex;
  flex-direction: column;
  gap: 12px;
  box-shadow: 0 8px 32px rgba(0,0,0,0.15);
}
.modal h2 {
  color: var(--brand-berry);
  font-size: 20px;
  font-weight: 700;
}
label {
  font-weight: 600;
  font-size: 13px;
  color: var(--brand-berry);
}
select, textarea {
  width: 100%;
  border: 1.5px solid #ddd;
  border-radius: 8px;
  padding: 8px 10px;
  font-size: 14px;
  outline: none;
}
textarea { resize: vertical; min-height: 80px; }
.modal-actions {
  display: flex;
  justify-content: flex-end;
  gap: 10px;
  margin-top: 6px;
}
.btn-cancel {
  background: transparent;
  border: 1.5px solid #ccc;
  border-radius: 8px;
  padding: 8px 16px;
  cursor: pointer;
}
.btn-submit {
  background: #4caf87;
  color: white;
  border: none;
  border-radius: 8px;
  padding: 8px 16px;
  cursor: pointer;
  font-weight: 600;
}
.btn-submit:disabled { opacity: 0.6; cursor: not-allowed; }
</style>
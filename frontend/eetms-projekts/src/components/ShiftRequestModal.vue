<template>
  <div class="modal-overlay" @click.self="$emit('close')">
    <div class="modal">
      <h2>New Shift Request</h2>

      <label>Reason</label>
      <select v-model="form.reasonID">
        <option :value="null" disabled>— Select reason —</option>
        <option v-for="r in filteredReasons" :key="r.reasonID" :value="r.reasonID">
          {{ r.name }}
        </option>
      </select>
      <label>Company <span style="color:#aaa; font-weight:400">(optional)</span></label>
        <select v-model="form.companyId">
          <option :value="null">— None —</option>
          <option v-for="c in filteredCompanies" :key="c.companyID" :value="c.companyID">
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
import { ref, computed, onMounted, watch } from 'vue'

const props = defineProps({ shopId: Number })
const emit = defineEmits(['close', 'submitted'])

const API_BASE = 'http://localhost:5001/api'

const reasons = ref([])
const companies = ref([])
const companyReasons = ref([])
const shopCountryNumber = ref(null) // stores 1, 2, or 3
const isSubmitting = ref(false)
const form = ref({ description: '', reasonID: null, companyId: null })

const prefixToCountryNumber = {
  'LT': 1, // Lithuania
  'LV': 2, // Latvia
  'EE': 3  // Estonia
}
const BALTICS = 4

onMounted(async () => {
  const shopRes = await fetch(`${API_BASE}/shops/${props.shopId}`)
  if (shopRes.ok) {
    const shop = await shopRes.json()
    const prefix = shop.code?.slice(0, 2).toUpperCase()
    shopCountryNumber.value = prefixToCountryNumber[prefix] ?? null
  }

  const [reasonsRes, companiesRes, companyReasonsRes] = await Promise.all([
    fetch(`${API_BASE}/reasons`),
    fetch(`${API_BASE}/companies`),
    fetch(`${API_BASE}/companyreasons`)
  ])

  if (reasonsRes.ok) reasons.value = await reasonsRes.json()
  if (companiesRes.ok) companies.value = await companiesRes.json()
  if (companyReasonsRes.ok) companyReasons.value = await companyReasonsRes.json()
})

// Base pool: companies in the shop's country OR Baltics (4)
const countryFilteredCompanies = computed(() => {
  if (!shopCountryNumber.value) return companies.value
  return companies.value.filter(c =>
    c.country === shopCountryNumber.value || c.country === BALTICS
  )
})

// --- PATH A: Reason chosen first → filter companies by reason + country
// --- PATH B: Company chosen first → filter reasons by company
// Both paths always start from countryFilteredCompanies as the base

const filteredCompanies = computed(() => {
  // If no reason selected, show all country-valid companies
  if (!form.value.reasonID) return countryFilteredCompanies.value

  const linkedIds = companyReasons.value
    .filter(cr => cr.reasonsReasonID === form.value.reasonID)
    .map(cr => cr.companiesCompanyID)

  return countryFilteredCompanies.value.filter(c => linkedIds.includes(c.companyID))
})

const filteredReasons = computed(() => {
  const validCompanyIds = countryFilteredCompanies.value.map(c => c.companyID)

  return reasons.value.filter(r => {
    const links = companyReasons.value.filter(cr => cr.reasonsReasonID === r.reasonID)

    // Reason must have at least one company valid for this shop's country
    const hasValidCompany = links.some(cr => validCompanyIds.includes(cr.companiesCompanyID))
    if (!hasValidCompany) return false

    // If company already selected, only show reasons that company provides
    if (form.value.companyId) {
      return links.some(cr => cr.companiesCompanyID === form.value.companyId)
    }

    return true
  })
})

// When reason changes → reset company (company must match new reason)
watch(() => form.value.reasonID, () => {
  form.value.companyId = null
})

// When company changes → if current reason no longer valid, reset it
watch(() => form.value.companyId, () => {
  if (form.value.reasonID) {
    const stillValid = filteredReasons.value.some(r => r.reasonID === form.value.reasonID)
    if (!stillValid) form.value.reasonID = null
  }
})

async function submit() {
  if (!form.value.description || !form.value.reasonID) return

  isSubmitting.value = true
  try {
    const payload = {
      description: form.value.description,
      reasonID: Number(form.value.reasonID),
      shopId: Number(props.shopId),
      companyId: form.value.companyId ? Number(form.value.companyId) : null
    }

    console.log('Submitting payload:', payload)

    const res = await fetch(`${API_BASE}/shiftrequests`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload)
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
  background: var(--color-overlay);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 100;
  backdrop-filter: blur(4px);
}

.modal {
  background: var(--color-white);
  border-radius: 20px;
  padding: 32px;
  width: 420px;
  display: flex;
  flex-direction: column;
  gap: 16px;
  box-shadow: 0 12px 48px var(--shadow-box), 0 2px 8px rgba(0,0,0,0.06);
  border-top: 4px solid var(--brand-berry);
}

.modal h2 {
  color: var(--brand-berry);
  font-size: 20px;
  font-weight: 700;
  letter-spacing: -0.3px;
}

label {
  font-weight: 600;
  font-size: 12px;
  text-transform: uppercase;
  letter-spacing: 0.5px;
  color: var(--color-text-dim);
}

/* ✅ Box-sizing fix — makes select stretch to full width like textarea */
select, textarea {
  box-sizing: border-box;
  width: 100%;
  border: 1.5px solid var(--color-border);
  border-radius: 10px;
  padding: 10px 12px;
  font-size: 14px;
  outline: none;
  background: var(--color-bg-muted);
  color: var(--color-text-main);
  transition: border-color 0.2s, box-shadow 0.2s;
}

select:focus, textarea:focus {
  border-color: var(--brand-berry);
  box-shadow: 0 0 0 3px rgba(161, 41, 113, 0.1);
  background: var(--color-white);
}

textarea {
  resize: vertical;
  min-height: 90px;
}

.modal-actions {
  display: flex;
  justify-content: flex-end;
  gap: 10px;
  margin-top: 4px;
}

.btn-cancel {
  background: transparent;
  border: 1.5px solid var(--color-border);
  border-radius: 10px;
  padding: 9px 20px;
  cursor: pointer;
  font-size: 14px;
  font-weight: 500;
  color: var(--color-text-dim);
  transition: border-color 0.2s, color 0.2s;
}

.btn-cancel:hover {
  border-color: var(--brand-berry);
  color: var(--brand-berry);
}

.btn-submit {
  background: var(--brand-berry);
  color: var(--color-white);
  border: none;
  border-radius: 10px;
  padding: 9px 22px;
  cursor: pointer;
  font-size: 14px;
  font-weight: 600;
  box-shadow: 0 4px 14px var(--shadow-box);
  transition: opacity 0.2s, transform 0.15s, box-shadow 0.2s;
}

.btn-submit:hover:not(:disabled) {
  opacity: 0.88;
  transform: translateY(-1px);
  box-shadow: 0 6px 20px var(--shadow-box);
}

.btn-submit:active:not(:disabled) {
  transform: translateY(0);
}

.btn-submit:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}
</style>
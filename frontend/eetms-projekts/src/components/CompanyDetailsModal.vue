<template>
  <!-- Modālais logs aizverās, nospiežot uz fona -->
  <div class="modal-overlay" @click.self="$emit('close')">
    
    <!-- Galvenais modālā loga konteiners -->
    <div class="modal">
      
      <!-- Aizvēršanas poga -->
      <button class="modal-close" type="button" @click="$emit('close')">✕</button>

      <!-- Uzņēmuma nosaukums --> 
      <h2>{{ company.companyName ?? 'Company details' }}</h2>

      <!-- Uzņēmuma adreses rinda -->
      <div class="detail-row">
        <span class="label">Address:</span>
        <span class="value">{{ company.address || '—' }}</span>
      </div>

      <!-- Valsts rinda, izmantojot countryMap pārvēršanai no koda uz nosaukumu -->
      <div class="detail-row">
        <span class="label">Country:</span>
        <span class="value">{{ countryMap[company.country] ?? company.country ?? '—' }}</span>
      </div>

      <!-- Telefona numura rinda -->
      <div class="detail-row">
        <span class="label">Phone:</span>
        <span class="value">{{ company.phoneNumber || '—' }}</span>
      </div>

      <!-- Reģistrācijas numura rinda -->
      <div class="detail-row">
        <span class="label">Registration No.:</span>
        <span class="value">{{ company.registrationNumber || '—' }}</span>
      </div>

      <!-- E-pasta rinda -->
      <div class="detail-row">
        <span class="label">Email:</span>
        <span class="value">{{ company.email || '—' }}</span>
      </div>

      <!-- Vidējā vērtējuma rinda -->
      <div class="detail-row">
        <span class="label">Average rating:</span>
        <span class="value rating-value">
          {{ averageRatingText }}
        </span>
      </div>

      <!-- Komentāru sadaļa -->
      <div class="comments-section">
        <h3>Comments</h3>
        
        <!-- Ziņojums, ja komentāru nav -->
        <div v-if="comments.length === 0" class="empty-msg">
          No rating comments yet
        </div>

        <!-- Komentāru saraksts -->
        <div v-else class="comments-list">
          <div
            v-for="item in comments"
            :key="item.shiftRequestID"
            class="comment-card"
            >
            <!-- Veikala kods -->
            <p class="comment-shop-code">
                {{ item.shop?.code ?? '—' }}
            </p>

            <!-- Komentāra augšējā daļa: iemesls un zvaigznes -->
            <div class="comment-top">
                <p class="comment-reason">{{ item.reason?.name ?? 'Shift request' }}</p>
                <span class="comment-stars">{{ renderStars(item.rating?.stars ?? 0) }}</span>
            </div>

            <!-- Komentāra teksts -->
            <p class="comment-text">
                {{ item.rating?.comment?.trim() || 'No written comment provided.' }}
            </p>
            </div>
        </div>
      </div>

      <!-- Modālā loga aizvēršanas poga -->
      <div class="modal-actions">
        <button class="btn-secondary" type="button" @click="$emit('close')">
          Close
        </button>
      </div>
    </div>
  </div>
</template>

<script setup>
// Importē computed no Vue
import { computed } from 'vue'

const props = defineProps({
  // Uzņēmuma objekts, kuru rāda modālajā logā
  company: {
    type: Object,
    required: true
  },

  // Vērtējumu un komentāru masīvs
  ratings: {
    type: Array,
    default: () => []
  }
})

// Valstu kodu pārvēršana uz valsts nosaukumiem
const countryMap = {
  1: 'Lithuania',
  2: 'Latvia',
  3: 'Estonia'
}

// Izveidot masīvu tikai ar skaitliskām zvaigžņu vērtībām
const numericRatings = computed(() =>
  props.ratings
    .map(item => Number(item.rating?.stars))
    .filter(value => !Number.isNaN(value) && value > 0)
)

// Aprēķināt uzņēmuma vidējo vērtējumu
const averageRating = computed(() => {
  // Ja nav neviena vērtējuma, atgriezt null 
  if (numericRatings.value.length === 0) return null

  // Saskaitīt visas zvaigznes
  const total = numericRatings.value.reduce((sum, value) => sum + value, 0)
  
  // Atgriezt vidējo vērtību
  return total / numericRatings.value.length
})

// Izveidot tekstu pie vidējā vērtējuma
const averageRatingText = computed(() => {
  // Ja vērtējumu nav, rādīt informatīvu tekstu
  if (averageRating.value == null) return 'No ratings yet'
  
  // Formatēt vidējo vērtējumu ar vienu ciparu aiz komata
  return `${averageRating.value.toFixed(1)} / 5`
})

// Filtrēt tikai tos ierakstus, kuriem ir rating objekts
const comments = computed(() => {
  return props.ratings.filter(item => item.rating)
})

// Pārvērst zvaigžņu skaitu par zvaigžņu virkni
function renderStars(count) {
  const n = Number(count) || 0
  return '★'.repeat(n) + '☆'.repeat(5 - n)
}
</script>

<style scoped>
/* Fona pārklājums aiz modālā loga */
.modal-overlay {
  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, 0.45);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1200;
}

/* Galvenais modālā loga konteiners */
.modal {
  width: min(720px, calc(100% - 24px));
  max-height: 90vh;
  overflow-y: auto;
  background: white;
  border-radius: 20px;
  padding: 24px;
  position: relative;
  box-shadow: 0 12px 36px rgba(0, 0, 0, 0.18);
}

/* Aizvēršanas poga */
.modal-close {
  position: absolute;
  top: 14px;
  right: 14px;
  border: none;
  background: transparent;
  color: var(--brand-berry);
  font-size: 18px;
  cursor: pointer;
}

/* Galvenais virsraksts */
h2 {
  color: var(--brand-berry);
  margin: 0 0 18px;
  font-size: 24px;
}

/* Sekcijas apakšvirsraksts */
h3 {
  color: var(--brand-berry);
  margin: 0 0 12px;
  font-size: 18px;
}

/* Informācijas rinda */
.detail-row {
  display: flex;
  gap: 8px;
  padding: 8px 0;
  border-bottom: 1px solid #eee;
}

.label {
  min-width: 140px;
  font-weight: 700;
  color: var(--brand-berry);
}

.value {
  color: #4b3951;
}

/* Vidējā vērtējuma stils */
.rating-value {
  color: #f2b01e;
  font-weight: 700;
}

/* Komentāru sadaļa */
.comments-section {
  margin-top: 22px;
}

.comments-list {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

/* Veikala koda teksts */
.comment-shop-code {
  margin: 0 0 6px;
  font-size: 12px;
  font-weight: 800;
  letter-spacing: 0.4px;
  text-transform: uppercase;
  color: #8e7a88;
}

/* Komentāra kartīte */
.comment-card {
  background: #faf8fb;
  border: 1px solid #eee4f0;
  border-radius: 14px;
  padding: 14px;
}

.comment-top {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
  margin-bottom: 8px;
}

/* Komentāra iemesla teksts */
.comment-reason {
  margin: 0;
  font-size: 14px;
  font-weight: 700;
  color: var(--brand-berry);
}

/* Zvaigžņu attēlojums */
.comment-stars {
  font-size: 15px;
  font-weight: 700;
  color: #f2b01e;
  letter-spacing: 1px;
}

/* Komentāra galvenais teksts */
.comment-text {
  margin: 0;
  color: #5f5165;
  font-size: 14px;
  line-height: 1.45;
}

/* Teksts, ja komentāru nav */
.empty-msg {
  color: #aaa;
  font-size: 13px;
  padding: 12px 0;
}

/* Darbību pogu bloks */
.modal-actions {
  margin-top: 22px;
  display: flex;
  justify-content: flex-end;
}

.btn-secondary {
  border: 1px solid #d8d0da;
  background: white;
  color: #6a5b70;
  border-radius: 10px;
  padding: 9px 14px;
  cursor: pointer;
}

/* Responsivitāte: mazajiem ekrāniem */
@media (max-width: 600px) {
  .detail-row {
    flex-direction: column;
    gap: 4px;
  }

  .label {
    min-width: unset;
  }

  .comment-top {
    flex-direction: column;
    align-items: flex-start;
  }
}
</style>
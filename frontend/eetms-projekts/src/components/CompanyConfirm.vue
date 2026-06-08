<template>
  <!-- Atvērt, ja isOpen vērtība ir true -->
  <div v-if="isOpen" class="modal-overlay" @click.self="$emit('cancel')">
    
    <!-- Modāla loga galvenais saturs -->
    <div class="modal-content">
      
      <!-- Virsraksts ar tulkojumu -->
      <h2>{{ t('companyConfirm.confirmCompany') }}</h2>
      
      <!-- Uzņēmuma nosaukums, parādās tikai tad, ja izvēlētais uzņēmums eksistē -->
      <p v-if="companySelected">{{ companySelected.companyName }}</p>
      
      <!-- Apstiprinājuma un atcelšanas poga -->
      <div class="modal-actions">
        <button class="cancel-btn" @click="$emit('cancel')">{{t('common.cancel')}}</button>
        <button class="confirm-btn" @click="$emit('confirm')">{{t('common.confirm')}}</button>
      </div>
    </div>
  </div>
</template>

<script setup>
// Importēt useI18n, lai izmantotu tulkojumus komponentā
import { useI18n } from 'vue-i18n'

// Tulkošanas funckijas inicializācija 
const { t } = useI18n()

defineProps({
  // Noteikt, vai modālais logs ir atvērts
  isOpen: Boolean,

  // Satur izvēlēta uzņēmuma objektu 
  companySelected: Object 
});

// Definēt notikumus, kurus komponents var sūtīt pamata sadaļai 
defineEmits(['confirm', 'cancel']);
</script>

<style scoped>
/* Fona stils */ 
.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background: rgba(0, 0, 0, 0.5);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
}

/* Modālā loga stils */
.modal-content {
  background: white;
  padding: 40px;
  border-radius: 20px;
  min-width: 400px;
  text-align: center;
}

/* Modālā loga virsraksts */
.modal-content h2 {
  font-family: 'Inter', sans-serif;
  font-size: 28px;
  font-weight: 700;
  color: #333;
  margin-bottom: 20px;
}

/* Uzņēmuma nosaukuma teksts */
.modal-content p {
  font-family: 'Inter', sans-serif;
  font-size: 20px;
  color: #666;
  margin-bottom: 30px;
}

/* Pogu konteiners */
.modal-actions {
  display: flex;
  gap: 20px;
  justify-content: center;
}

/* Stils apstiprinājuma/atclelšanas pogām */
.cancel-btn, .confirm-btn {
  padding: 12px 40px;
  border: none;
  border-radius: 10px;
  font-family: 'Inter', sans-serif;
  font-size: 18px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s ease;
}

/* Papildus stils atcelšanas pogai */
.cancel-btn {
  background: #e0e0e0;
  color: #333;
}

.cancel-btn:hover {
  background: #d0d0d0;
}

/* Papildus stils apstiprinājuma pogai */ 
.confirm-btn {
  background: var(--brand-teal);
  color: white;
}

.confirm-btn:hover {
  transform: translateY(-2px);
}
</style>

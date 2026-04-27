<template>
  <div v-if="isOpen" class="modal-overlay" @click.self="$emit('cancel')">
    <div class="modal-content">
      <h2>{{ t('companyConfirm.confirmCompany') }}</h2>
      <p v-if="companySelected">{{ companySelected.companyName }}</p>
      
      <div class="modal-actions">
        <button class="cancel-btn" @click="$emit('cancel')">{{t('common.cancel')}}</button>
        <button class="confirm-btn" @click="$emit('confirm')">{{t('common.confirm')}}</button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { useI18n } from 'vue-i18n'
const { t } = useI18n()

defineProps({
  isOpen: Boolean,
  companySelected: Object 
});

defineEmits(['confirm', 'cancel']);
</script>

<style scoped>
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

.modal-content {
  background: white;
  padding: 40px;
  border-radius: 20px;
  min-width: 400px;
  text-align: center;
}

.modal-content h2 {
  font-family: 'Inter';
  font-size: 28px;
  font-weight: 700;
  color: #333;
  margin-bottom: 20px;
}

.modal-content p {
  font-family: 'Inter';
  font-size: 20px;
  color: #666;
  margin-bottom: 30px;
}

.modal-actions {
  display: flex;
  gap: 20px;
  justify-content: center;
}

.cancel-btn, .confirm-btn {
  padding: 12px 40px;
  border: none;
  border-radius: 10px;
  font-family: 'Inter';
  font-size: 18px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s ease;
}

.cancel-btn {
  background: #e0e0e0;
  color: #333;
}

.cancel-btn:hover {
  background: #d0d0d0;
}

.confirm-btn {
  background: var(--brand-teal);
  color: white;
}

.confirm-btn:hover {
  transform: translateY(-2px);
}
</style>

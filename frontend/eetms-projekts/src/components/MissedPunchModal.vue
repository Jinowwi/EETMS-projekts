<template>
  <div v-if="isOpen" class="modal-overlay" @click.self="closeModal">
    <div class="modal-content">
      <button class="close-btn" @click="closeModal">
        <FontAwesomeIcon :icon="['fas', 'times']" />
      </button>
      <div class="modal-header">
        <h2>Missed Punch</h2>
        <p class="date-label">{{ formattedDate }} {{ formatHHMM(punchData.startTime) }}</p>
      </div>
      <div class="punch-form">
        <div class="form-group">
          <label>Missed Punch Type</label>
          <div class="punch-type">
            <span class="badge-missed">{{ punchData.type === 'start' ? 'Start Time' : 'End Time' }}</span>
          </div>
        </div>

        <div class="form-group">
          <label>Company Name: {{ punchData.companyName }}</label>
        </div>
        <div class="form-group">
          <label>{{ punchData.type === 'start' ? 'Start Time' : 'End Time' }}</label>
          <FlatPickr v-model="editedTime" :config="timeConfig" class="form-input" />
        </div>
        <div class="form-actions">
          <button class="btn-cancel" @click="closeModal">Cancel</button>
          <button class="btn-save" @click="savePunch">Save</button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, watch } from 'vue';
import { FontAwesomeIcon } from '@fortawesome/vue-fontawesome';
import FlatPickr from 'vue-flatpickr-component';
import 'flatpickr/dist/flatpickr.css';

const props = defineProps({
  isOpen: Boolean,
  punchData: {
    type: Object,
    default: () => ({ date: '', type: 'start' })
  }
});

const emit = defineEmits(['close', 'save']);
const editedTime = ref('');
const editedEndDate = ref(''); 
const reason = ref('');
const formatHHMM = (t) => (t ? String(t).slice(0, 5) : ''); 

const endDateConfig = {
  altInput: true,
  altFormat: 'd.m.Y',
  dateFormat: 'Y-m-d',
  allowInput: true, 
  altInputClass: 'form-input'
}; 

const timeConfig = computed(() => {
  const config = {
    enableTime: true,
    noCalendar: true, 
    dateFormat: 'H:i', 
    time_24hr: true, 
    allowInput: true
  }; 

  return config; 
}); 

const formattedDate = computed(() => {
  const d = props.punchData?.date || props.punchData?.startDate;
  if (!d) return '';
  return new Date(d).toLocaleDateString('de-DE', {
    day: '2-digit', month: '2-digit', year: 'numeric'
  });
});

const toHHMM = (t) => {
  if (!t) return '';
  return String(t).slice(0, 5); 
};

const toYYYYMMDD = (d) => {
  if (!d) return ''

  const s = String(d) 
  if (/^\d{4}-\d{2}-\d{2}/.test(s)) return s.slice(0, 10)

  const dt = new Date(d)
  if (Number.isNaN(dt.valueOf())) return ''

  return dt.toISOString().slice(0, 10)
}; 

const combineDateTimeLocal = (dateLike, hhmm) => {
  const ymd = toYYYYMMDD(dateLike)
  if (!ymd || !hhmm) return ''
  return `${ymd} ${hhmm}:00`
}; 

watch(() => props.punchData, (newData) => {
  if (!newData) return;

  if (newData.type === 'start') {
    editedTime.value = toHHMM(newData.startTime);
  } else {
    editedTime.value = toHHMM(newData.endTime);
  }

  editedEndDate.value = ''; 

  reason.value = '';
}, { immediate: true });

const closeModal = () => emit('close');

const timeToMinutes = (hhmm) => {
  if (!hhmm) return null;
  const [h, m] = String(hhmm).slice(0,5).split(':').map(Number); 
  if (Number.isNaN(h) || Number.isNaN(m)) return null;
  return h * 60 + m;
}; 

const savePunch = () => {
  if (!editedTime.value) return

  const shiftId = props.punchData.shiftID || props.punchData.ShiftID

  const baseDate =
    props.punchData.startDate ||
    props.punchData.endDate ||
    props.punchData.date

  const existingStart = toHHMM(props.punchData.startTime || props.punchData.startTime)
  const existingEnd = toHHMM(props.punchData.endTime || props.punchData.endTime)

  const startTime = props.punchData.type === 'start' ? editedTime.value : existingStart
  const endTime = props.punchData.type === 'end' ? editedTime.value : existingEnd

  let calculatedEndDate = baseDate; 

  if (props.punchData.type === 'end' && startTime && endTime) {
    const s = timeToMinutes(startTime); 
    const e = timeToMinutes(endTime); 

    if (endTime < startTime) {
      const d = new Date(baseDate); 
      d.setDate(d.getDate() + 1);
      calculatedEndDate = toYYYYMMDD(d); 
    } else {
      calculatedEndDate = toYYYYMMDD(baseDate); 
    }
  } else {
    calculatedEndDate = props.punchData.endDate || props.punchData.EndDate || baseDate;
  }

  const endDatetoUse = calculatedEndDate; 
      
  const startDateTime = combineDateTimeLocal(baseDate, startTime)
  const EndDateTime = combineDateTimeLocal(endDatetoUse, endTime)

  const updatedShift = {
    ShiftID: shiftId,

    startDate: baseDate,
    EndDate: endDatetoUse,
    startTime,
    endTime,

    startDateTime,
    EndDateTime,

    Reason: reason.value || null,

    CompanyReasonID: props.punchData.companyReasonID || props.punchData.CompanyReasonID,
    ShopID: props.punchData.shopID || props.punchData.ShopID,
    employee_phone_number: props.punchData.employee_phone_number,
    verification_code: props.punchData.verification_code
  }

  emit('save', updatedShift)
}

</script>

<style scoped>
.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0, 0, 0, 0.6);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 20000;
  padding: 20px;
}

.modal-content {
  background: white;
  border-radius: 16px;
  padding: 40px;
  max-width: 500px;
  width: 100%;
  max-height: 90vh;
  overflow-y: auto;
  position: relative;
  box-shadow: 0 10px 40px rgba(0, 0, 0, 0.3);
}

.close-btn {
  position: absolute;
  top: 20px;
  right: 20px;
  background: transparent;
  border: none;
  font-size: 24px;
  color: #999;
  cursor: pointer;
  width: 40px;
  height: 40px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: all 0.3s ease;
  z-index: 10;
}

.close-btn:hover {
  background: #f5f5f5;
  color: #a12971;
}

.modal-header {
  margin-bottom: 30px;
}

.modal-header h2 {
  font-family: 'Inter';
  font-size: 28px;
  font-weight: bold;
  background: linear-gradient(135deg, #a12971, #2ba492);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  background-clip: text;
  margin: 0 0 10px 0;
}

.date-label {
  font-family: 'Inter';
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
  font-family: 'Inter';
  font-size: 14px;
  font-weight: bold;
  color: #333;
}

.punch-type {
  display: flex;
}

.badge-missed {
  background: linear-gradient(135deg, #dc3545, #c82333);
  color: white;
  padding: 8px 16px;
  border-radius: 20px;
  font-weight: 600;
  font-size: 14px;
  display: inline-block;
  font-family: 'Inter';
}

.form-input {
  font-family: 'Inter';
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

.form-textarea {
  font-family: 'Inter';
  font-size: 16px;
  padding: 12px 16px;
  border: 2px solid #e0e0e0;
  border-radius: 8px;
  outline: none;
  transition: all 0.2s ease;
  resize: vertical;
}

.form-textarea:focus {
  border-color: #a12971;
  box-shadow: 0 0 0 3px rgba(161, 41, 113, 0.1);
}

.form-actions {
  display: flex;
  gap: 12px;
  margin-top: 10px;
}

.btn-cancel,
.btn-save {
  flex: 1;
  font-family: 'Inter';
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

.btn-save {
  background: linear-gradient(135deg, #a12971, #96537b);
  color: white;
}

.btn-save:hover {
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(161, 41, 113, 0.3);
}

.btn-save:active {
  transform: translateY(0);
}

:deep(.form-input) {
  font-family: 'Inter';
  font-size: 16px;
  padding: 12px 16px;
  border: 2px solid #e0e0e0;
  border-radius: 8px;
  outline: none;
  transition: all 0.2s ease;
}

/* Responsive */
@media (max-width: 768px) {
  .modal-content {
    padding: 30px 20px;
  }

  .modal-header h2 {
    font-size: 24px;
  }

  .date-label {
    font-size: 14px;
  }

  .form-input,
  .form-textarea {
    font-size: 14px;
  }
}

@media (max-width: 480px) {
  .modal-content {
    padding: 25px 20px;
  }

  .modal-header h2 {
    font-size: 22px;
  }

  .btn-cancel,
  .btn-save {
    font-size: 14px;
    padding: 12px 20px;
  }
}
</style>

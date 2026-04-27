<template>
  <div v-if="isOpen" class="modal-overlay" @click.self="closeModal">
    <div class="modal-content">
      <button class="close-btn" @click="closeModal">
        <FontAwesomeIcon :icon="['fas', 'times']" />
      </button>
      
      <div class="user-details">
        <div class="detail-row"><span class="label">Company:</span> {{ user.companyName }}</div>
      </div>
      
      <div class="tab-switcher">
        <button
          :class="['tab-btn', { active: activeTab === 'shifts'}]"
          @click="activeTab = 'shifts'">
          Shifts
        </button>
        <button
          :class="['tab-btn', { active: activeTab === 'reasons'}]"
          @click="activeTab = 'reasons'">
          Reasons
        </button>
      </div>

      <div v-if="activeTab === 'shifts'" class="table-section">
        <div class="section-header">
          <h2>Shift Information</h2>
          <div class="buttons">
            <button :class="['btn', { 'active-btn': isSortModeShifts }]" @click="toggleSortModeShifts">
              <FontAwesomeIcon :icon="['fas', 'sliders-h']" />
            </button>                    
          </div>
        </div>

        <transition name="slide-down">
          <div v-if="isSortModeShifts" class="sort-toolbar">
            <span class="sort-label">Filter By</span>
            <div class="sort-options">
              <div class="search-container">
                <input
                  type="text"
                  v-model="shiftQuery"
                  placeholder="Search shifts..."
                  class="search-input"
                />  
              </div>
              <select v-model="filterReason" class="search-input">
                <option value="">All Reasons</option>
                <option v-for="reason in uniqueReasons" :key="reason" :value="reason">
                  {{ reason }}
                </option>
              </select>
              <select v-model="filterShop" class="search-input">
                <option value="">All Shops</option>
                <option v-for="shop in shops" :key="shop" :value="shop">
                  {{ shop }}
                </option>
              </select>
            </div>
          </div>
        </transition>

        <div class="table-scroll-wrapper">
          <table>
            <thead>
              <tr>
                <th @click="handleHeaderSort('shopCode')" class="sortable-header">Shop Code
                  <FontAwesomeIcon v-if="sortColumn === 'shopCode'" :icon="sortDirection === 'asc' ? ['fas', 'sort-up'] : ['fas', 'sort-down']" class="sort-icon" />
                  <FontAwesomeIcon v-else :icon="['fas', 'sort']" class="sort-icon placeholder" />
                </th>
                <th @click="handleHeaderSort('startDate')" class="sortable-header">Start Date
                  <FontAwesomeIcon v-if="sortColumn === 'startDate'" :icon="sortDirection === 'asc' ? ['fas', 'sort-up'] : ['fas', 'sort-down']" class="sort-icon" />
                  <FontAwesomeIcon v-else :icon="['fas', 'sort']" class="sort-icon placeholder" />
                </th>
                <th @click="handleHeaderSort('endDate')" class="sortable-header">End Date
                  <FontAwesomeIcon v-if="sortColumn === 'endDate'" :icon="sortDirection === 'asc' ? ['fas', 'sort-up'] : ['fas', 'sort-down']" class="sort-icon" />
                  <FontAwesomeIcon v-else :icon="['fas', 'sort']" class="sort-icon placeholder" />
                </th>
                <th>Start Time</th>
                <th>End Time</th>
                <th>Total Hours</th>
                <th @click="handleHeaderSort('reason')" class="sortable-header">Reason
                  <FontAwesomeIcon v-if="sortColumn === 'reason'" :icon="sortDirection === 'asc' ? ['fas', 'sort-up'] : ['fas', 'sort-down']" class="sort-icon"/>
                  <FontAwesomeIcon v-else :icon="['fas', 'sort']" class="sort-icon placeholder" />
                </th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="(shift, index) in sortedShiftsData" :key="index">
                <td>{{ shift.shopCode }}</td>
                <td>{{ shift.startDate }}</td>
                <td>{{ shift.endDate }}</td>
                <td>{{ shift.startTime }}</td>
                <td>{{ shift.endTime }}</td>
                <td>{{ calculateDuration(shift.startTime, shift.endTime) }}</td>
                <td>{{ shift.companyReason?.reason?.name }}</td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>

      <div v-if="activeTab === 'reasons'" class="table-section">
        <div class="add-company-bar">
          <span class="add-label">Assign Reason:</span>
          <div class="add-controls">
            <select v-model="selectedReasonToAdd" class="modern-select">
              <option value="" disabled>Select a reason to add...</option>
              <option v-for="reason in availableReasonsToAdd" :key="reason.id" :value="reason.id">
                {{ reason.name || reason.reasonName }}
              </option>
            </select>
            <button 
              class="btn-add-company" 
              @click="handleAddReason" 
              :disabled="!selectedReasonToAdd"
            >
              <FontAwesomeIcon :icon="['fas', 'plus']" /> Add
            </button>
          </div>
        </div>

        <div class="section-header">
          <h2>Company Reasons</h2>
          <div class="buttons">
            <button :class="['btn', { 'active-btn': isSortModeReasons }]" @click="toggleSortModeReasons">
              <FontAwesomeIcon :icon="['fas', 'sliders-h']" />
            </button>
          </div>
        </div>

        <transition name="slide-down">
          <div v-if="isSortModeReasons" class="sort-toolbar">
            <span class="sort-label">Filter By</span>
            <div class="sort-options">
              <div class="search-container">
                <input
                  type="text"
                  v-model="reasonQuery"
                  placeholder="Search reasons..."
                  class="search-input"
                />  
              </div>
            </div>
          </div>
        </transition>

        <p v-if="!user?.reasons || user.reasons.length === 0" class="no-data">
          No reasons are currently assigned to this company.
        </p>

        <table v-else>
          <tbody>
            <tr v-for="reason in sortedReasonsData" :key="reason.id">
              <td style="font-weight: normal; width: 90%;">{{ reason.reasonName || reason.name }}</td>
              <td style="text-align: right; width: 10%;">
                <button 
                  @click="$emit('remove-reason', reason.id)" 
                  class="btn-delete" 
                  title="Remove Reason"
                >
                  <FontAwesomeIcon :icon="['fas', 'times']" />
                </button>
              </td>
            </tr>
          </tbody> 
        </table>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, watch } from 'vue'; 
import { FontAwesomeIcon } from '@fortawesome/vue-fontawesome';

const activeTab = ref('shifts'); 

const props = defineProps({
  isOpen: Boolean,
  user: Object,
  shiftData: Array,
  allReasons: { type: Array, default: () => [] } 
});

watch(() => props.isOpen, (newVal) => {
  if (newVal) activeTab.value = 'shifts'; 
}); 

const emit = defineEmits(['close', 'refresh', 'remove-reason', 'add-reason']);

const isSortModeShifts = ref(false);
const shiftQuery = ref('');
const filterReason = ref(''); 
const filterShop = ref('');
const sortColumn = ref('startDate');
const sortDirection = ref('asc'); 

const uniqueReasons = computed(() => {
  const reasons = props.shiftData 
    .map(shift => shift.companyReason?.reason?.name)
    .filter(Boolean);
  return [...new Set(reasons)]; 
}); 

const shops = computed(() => {
  const shopList = props.shiftData
    .map(shift => shift.shopCode)
    .filter(Boolean);
  return [...new Set(shopList)];
});

const toggleSortModeShifts = () => {
  isSortModeShifts.value = !isSortModeShifts.value; 
};

const handleHeaderSort = (column) => {
  if (sortColumn.value === column) {
    sortDirection.value = sortDirection.value === 'asc' ? 'desc' : 'asc';
  } else {
    sortColumn.value = column;
    sortDirection.value = 'asc'; 
  }
}; 

const sortedShiftsData = computed(() => {
  let filtered = [...props.shiftData];

  if (filterReason.value) {
    filtered = filtered.filter(shift => 
      shift.companyReason?.reason?.name === filterReason.value
    ); 
  }

  if (filterShop.value) {
    filtered = filtered.filter(shift => shift.shopCode === filterShop.value);
  }

  if (shiftQuery.value) {
    const s = shiftQuery.value.toLowerCase();
    filtered = filtered.filter(shift => 
      (shift.shopCode || '').toLowerCase().includes(s) || 
      (shift.startDate || '').includes(s) ||
      (shift.companyReason?.reason?.name || '').toLowerCase().includes(s)
    );
  }

  return filtered.sort((a, b) => {
    let valA = sortColumn.value === 'reason' ? a.companyReason?.reason?.name || '' : a[sortColumn.value] || '';
    let valB = sortColumn.value === 'reason' ? b.companyReason?.reason?.name || '' : b[sortColumn.value] || '';
    valA = String(valA).toLowerCase();
    valB = String(valB).toLowerCase();
    if (valA < valB) return sortDirection.value === 'asc' ? -1 : 1;
    if (valA > valB) return sortDirection.value === 'asc' ? 1 : -1;
    return 0;
  }); 
});

const calculateDuration = (start, end) => {
  if (!start || !end) return '00:00';
  const getMin = (t) => {
    const [h, m] = t.split(':').map(Number);
    return h * 60 + m;
  };
  let diff = getMin(end) - getMin(start);
  if (diff < 0) diff += 1440; 
  return `${String(Math.floor(diff / 60)).padStart(2, '0')}:${String(diff % 60).padStart(2, '0')}`;
};

const selectedReasonToAdd = ref('');
const isSortModeReasons = ref(false);
const reasonQuery = ref('');

const availableReasonsToAdd = computed(() => {
  if (!props.allReasons) return [];
  if (!props.user?.reasons || props.user.reasons.length === 0) return props.allReasons;
  const assignedIds = props.user.reasons.map(r => Number(r.id || r.reasonID));
  return props.allReasons.filter(r => !assignedIds.includes(Number(r.id || r.reasonID)));
});

const handleAddReason = () => {
  if (!selectedReasonToAdd.value) return;
  emit('add-reason', selectedReasonToAdd.value);
  selectedReasonToAdd.value = '';
};

const toggleSortModeReasons = () => {
  isSortModeReasons.value = !isSortModeReasons.value;
};

const sortedReasonsData = computed(() => {
  if (!props.user?.reasons) return [];
  let filtered = [...props.user.reasons];
  if (reasonQuery.value) {
    const s = reasonQuery.value.toLowerCase();
    filtered = filtered.filter(r => (r.reasonName || r.name || '').toLowerCase().includes(s));
  }
  return filtered.sort((a, b) => {
    let valA = String(a.reasonName || a.name || '').toLowerCase();
    let valB = String(b.reasonName || b.name || '').toLowerCase();
    if (valA < valB) return -1;
    if (valA > valB) return 1;
    return 0;
  });
});

const closeModal = () => emit('close'); 
</script>

<style scoped>
.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(30, 5, 20, 0.5);
  backdrop-filter: blur(6px);
  -webkit-backdrop-filter: blur(6px);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
  padding: 20px;
  box-sizing: border-box;
}

.modal-content {
  background: rgba(255, 255, 255, 0.92);
  backdrop-filter: blur(20px);
  -webkit-backdrop-filter: blur(20px);
  border: 1.5px solid rgba(255, 255, 255, 0.95);
  border-radius: 20px;
  padding: 40px;
  width: 100%;
  max-width: 1000px;
  max-height: 90vh;
  overflow-y: auto;
  position: relative;
  box-shadow: 0 16px 48px rgba(161, 41, 113, 0.12), 0 4px 16px rgba(0,0,0,0.08);
  box-sizing: border-box;
}

/* Vertical scrollbar */
.modal-content::-webkit-scrollbar { width: 6px; }
.modal-content::-webkit-scrollbar-track { background: transparent; }
.modal-content::-webkit-scrollbar-thumb { background: var(--brand-berry); border-radius: 10px; }

.close-btn {
  position: absolute;
  top: 16px;
  right: 16px;
  background: rgba(255,255,255,0.7);
  border: 1.5px solid rgba(255,255,255,0.9);
  font-size: 18px;
  color: var(--brand-berry);
  cursor: pointer;
  width: 36px;
  height: 36px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: all 0.2s ease;
  z-index: 10;
}

.close-btn:hover {
  background: var(--brand-berry);
  color: white;
}

.user-details {
  background: rgba(161, 41, 113, 0.06);
  border: 1px solid rgba(161, 41, 113, 0.1);
  padding: 16px 20px;
  border-radius: 12px;
  margin-bottom: 20px;
}

.detail-row {
  font-family: 'Inter';
  font-size: 16px;
  margin-bottom: 0;
}

.label {
  font-weight: bold;
  color: var(--brand-berry);
  margin-right: 8px;
}

.tab-switcher {
  display: flex;
  gap: 4px;
  border-bottom: 2px solid rgba(161, 41, 113, 0.15);
  margin-bottom: 16px;
}

.tab-btn {
  font-family: 'Inter', sans-serif;
  font-size: 15px;
  font-weight: 600;
  padding: 10px 18px;
  background-color: transparent;
  border: none;
  border-bottom: 3px solid transparent;
  color: #888;
  cursor: pointer;
  transition: all 0.2s ease;
  margin-bottom: -2px;
}

.tab-btn:hover { color: #333; }

.tab-btn.active {
  color: var(--brand-berry);
  border-bottom: 3px solid var(--brand-berry);
}

.table-section { margin-top: 16px; }

.table-scroll-wrapper {
  width: 100%;
  overflow-x: auto;
  -webkit-overflow-scrolling: touch;
  border-radius: 10px;
}

/* Hide horizontal scrollbar visually but keep functionality */
.table-scroll-wrapper::-webkit-scrollbar { height: 0; display: none; }
.table-scroll-wrapper { scrollbar-width: none; -ms-overflow-style: none; }

.section-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 12px;
}

.section-header h2 {
  font-family: 'Inter', sans-serif;
  font-weight: 700;
  font-size: 18px;
  color: var(--brand-berry);
  margin: 0;
}

.sort-toolbar {
  background: rgba(255, 255, 255, 0.7);
  border: 1px solid rgba(255, 255, 255, 0.9);
  backdrop-filter: blur(8px);
  padding: 12px 16px;
  border-radius: 10px;
  margin-bottom: 16px;
  display: flex;
  align-items: center;
  gap: 12px;
  flex-wrap: wrap;
}

.sort-label {
  font-family: 'Inter', sans-serif;
  font-weight: 600;
  font-size: 13px;
  color: var(--brand-berry);
  white-space: nowrap;
}

.sort-options {
  display: flex;
  gap: 10px;
  flex: 1;
  flex-wrap: wrap;
}

.search-container { flex: 1; min-width: 120px; }

.search-input {
  font-family: 'Inter', sans-serif;
  width: 100%;
  padding: 7px 12px;
  border: 1px solid rgba(161, 41, 113, 0.2);
  border-radius: 8px;
  outline: none;
  background: rgba(255, 255, 255, 0.85);
  font-size: 13px;
  box-sizing: border-box;
}

.search-input:focus { border-color: var(--brand-berry); }

table {
  width: 100%;
  border-collapse: collapse;
  font-family: 'Inter', sans-serif;
  min-width: 520px;
}

th {
  background: linear-gradient(135deg, var(--brand-berry), var(--brand-berry-light));
  color: white;
  padding: 12px 14px;
  text-align: center;
  font-size: 12px;
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 0.4px;
  white-space: nowrap;
}

td {
  font-family: 'Inter', sans-serif;
  padding: 12px 14px;
  border-bottom: 1px solid rgba(161, 41, 113, 0.08);
  font-size: 14px;
  text-align: center;
}

tbody tr:hover { background: rgba(161, 41, 113, 0.04); }
tbody tr:last-child td { border-bottom: none; }

.buttons {
  display: flex;
  gap: 8px;
  align-items: center;
}

.btn {
  width: 36px;
  height: 36px;
  border-radius: 50%;
  border: 1.5px solid rgba(255, 255, 255, 0.9);
  background: rgba(255, 255, 255, 0.75);
  color: var(--brand-berry);
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  font-size: 14px;
  transition: all 0.2s ease;
  box-shadow: 0 2px 8px rgba(0,0,0,0.06);
}

.btn:hover {
  background: rgba(255, 255, 255, 0.95);
  transform: translateY(-1px);
}

.active-btn {
  background: var(--brand-berry) !important;
  color: white !important;
  border-color: var(--brand-berry) !important;
}

.add-company-bar {
  display: flex;
  align-items: center;
  gap: 12px;
  background: rgba(255, 255, 255, 0.65);
  border: 1px solid rgba(255, 255, 255, 0.85);
  border-radius: 12px;
  padding: 14px 16px;
  margin-bottom: 16px;
  flex-wrap: wrap;
}

.add-label {
  font-family: 'Inter', sans-serif;
  font-weight: 700;
  font-size: 14px;
  color: var(--brand-berry);
  white-space: nowrap;
}

.add-controls {
  display: flex;
  gap: 10px;
  flex: 1;
  min-width: 0;
}

.modern-select {
  font-family: 'Inter', sans-serif;
  flex: 1;
  min-width: 0;
  padding: 9px 12px;
  border: 1px solid rgba(161, 41, 113, 0.2);
  border-radius: 8px;
  font-size: 14px;
  outline: none;
  background: rgba(255, 255, 255, 0.85);
  box-sizing: border-box;
}

.modern-select:focus { border-color: var(--brand-berry); }

.btn-add-company {
  font-family: 'Inter', sans-serif;
  background: var(--brand-berry);
  color: white;
  border: none;
  padding: 9px 18px;
  border-radius: 8px;
  font-weight: 600;
  font-size: 14px;
  cursor: pointer;
  transition: all 0.2s ease;
  display: flex;
  align-items: center;
  gap: 6px;
  white-space: nowrap;
  flex-shrink: 0;
}

.btn-add-company:hover:not(:disabled) {
  background: var(--brand-berry-light);
  transform: translateY(-1px);
}

.btn-add-company:disabled {
  background: #ccc;
  cursor: not-allowed;
}

.btn-delete {
  width: 32px;
  height: 32px;
  border-radius: 50%;
  border: 1.5px solid rgba(255, 255, 255, 0.9);
  background: rgba(255, 255, 255, 0.75);
  color: var(--brand-berry);
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  transition: all 0.2s ease;
}

.btn-delete:hover {
  background: var(--brand-berry);
  color: white;
}

.no-data {
  font-family: 'Inter', sans-serif;
  text-align: center;
  color: #999;
  font-style: italic;
  margin-top: 20px;
  padding: 20px;
  background: rgba(255, 255, 255, 0.6);
  border-radius: 10px;
}

.slide-down-enter-active,
.slide-down-leave-active { transition: all 0.3s ease; }
.slide-down-enter-from,
.slide-down-leave-to { opacity: 0; transform: translateY(-8px); }

@media (max-width: 1024px) {
  .modal-content { padding: 30px 24px; }
}

@media (max-width: 768px) {
  .modal-content { padding: 24px 16px; }
  .detail-row { font-size: 14px; }
  th, td { padding: 10px 10px; font-size: 12px; }
  th { font-size: 11px; }
}

@media (max-width: 600px) {
  .add-company-bar { flex-direction: column; align-items: stretch; }
  .add-controls { width: 100%; }
  .sort-toolbar { flex-direction: column; align-items: flex-start; }
  .sort-options { flex-direction: column; width: 100%; }
  .tab-btn { font-size: 13px; padding: 8px 12px; }
}

@media (max-width: 480px) {
  .modal-content { padding: 16px 12px; border-radius: 14px; }
  .section-header h2 { font-size: 15px; }
  th, td { padding: 8px 7px; font-size: 11px; }
}
</style>

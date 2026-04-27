<template>
  <div v-if="isOpen" class="modal-overlay" @click.self="$emit('close')">
    <div class="modal-content">
      <button class="close-btn" @click="$emit('close')">
        <FontAwesomeIcon :icon="['fas', 'times']" />
      </button>

      <div class="modal-header">
        <h2>Reason Details</h2>
      </div>

      <div class="user-details">
        <div class="detail-row">
            <span class="label">Companies with Reason:</span> 
            <span class="value">{{ reason?.reasonName || 'Unknown' }}</span>
        </div>
      </div>

      <div class="add-company-bar">
        <span class="add-label">Assign to Company:</span>
        <div class="add-controls">
          <select v-model="selectedCompanyToAdd" class="modern-select">
            <option value="" disabled>Select a company to add...</option>
            <option v-for="company in availableCompaniesToAdd" 
            :key="company.id" :value="company.id">
              {{ company.companyName }}
            </option>
          </select>
          <button 
              class="btn-add-company" 
              @click="handleAddCompany" 
              :disabled="!selectedCompanyToAdd"
          >
            <FontAwesomeIcon :icon="['fas', 'plus']" /> Add
          </button>
        </div>
      </div>

      <div class="table-section">
        <div class="section-header">
          <h2>Assigned Companies</h2>
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
                  v-model="companyQuery"
                  placeholder="Search companies..."
                  class="search-input"
                />  
              </div>

              <div class="search-container">
                <select v-model="filterCountry" class="search-input">
                  <option value="">All Countries</option>
                  <option v-for="country in uniqueCountries" :key="country" :value="country">
                    {{ country }}
                  </option>
                </select>
              </div>
            </div>
          </div>
        </transition>

        <p v-if="companies.length === 0" class="no-data">
            No companies are currently using this reason.
        </p>

        <table v-else>
          <tbody>
            <tr v-for="company in sortedCompaniesData" :key="company.id">
              <td style="width: 60%;"><strong>{{ company.companyName }}</strong></td>
              <td style="width: 30%;"><span class="country-badge">{{ company.country }}</span></td>
              <td style="text-align: right; width: 10%;">
                <button @click="$emit('remove-company', company.id)" title="Remove from reason" class="btn-delete">
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
import { ref, computed } from 'vue';
import { FontAwesomeIcon } from '@fortawesome/vue-fontawesome';

const props = defineProps({
  isOpen: Boolean,
  reason: Object,
  companies: { type: Array, default: () => [] },
  allCompanies: { type: Array, default: () => [] }
});

const emit = defineEmits(['close', 'remove-company', 'add-company']);

const selectedCompanyToAdd = ref('');

const availableCompaniesToAdd = computed(() => {
  if (!props.companies || !props.allCompanies) return [];
  const assignedIds = props.companies.map(c => Number(c.id));
  return props.allCompanies.filter(c => !assignedIds.includes(Number(c.id)));
});

const handleAddCompany = () => {
  if (!selectedCompanyToAdd.value) return;
  emit('add-company', selectedCompanyToAdd.value);
  selectedCompanyToAdd.value = '';
};

const isSortModeReasons = ref(false);
const companyQuery = ref('');
const filterCountry = ref('');
const sortColumn = ref('companyName');
const sortDirection = ref('asc');

const uniqueCountries = computed(() => {
  const countries = props.companies
    .map(c => c.country)
    .filter(Boolean);
  return [...new Set(countries)];
});

const toggleSortModeReasons = () => {
  isSortModeReasons.value = !isSortModeReasons.value;
};

const sortedCompaniesData = computed(() => {
  let filtered = [...props.companies];

  if (filterCountry.value) {
    filtered = filtered.filter(c => c.country === filterCountry.value);
  }

  if (companyQuery.value) {
    const s = companyQuery.value.toLowerCase();
    filtered = filtered.filter(c => 
      (c.companyName || '').toLowerCase().includes(s) || 
      (c.country || '').toLowerCase().includes(s)
    );
  }

  return filtered.sort((a, b) => {
    let valA = a[sortColumn.value] || '';
    let valB = b[sortColumn.value] || '';

    valA = String(valA).toLowerCase();
    valB = String(valB).toLowerCase();

    if (valA < valB) return sortDirection.value === 'asc' ? -1 : 1;
    if (valA > valB) return sortDirection.value === 'asc' ? 1 : -1;
    return 0;
  });
});
</script>

<style scoped>
* {
  font-family: 'Inter', sans-serif;
}

.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: var(--color-overlay, rgba(0, 0, 0, 0.5));
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
  padding: 20px;
}

.modal-content {
  background: var(--color-white, #ffffff);
  border-radius: 16px;
  padding: 40px;
  max-width: 800px;
  width: 100%;
  max-height: 90vh;
  overflow-y: auto;
  position: relative;
  box-shadow: 0 10px 40px rgba(0,0,0,0.1);
  font-family: 'Inter', sans-serif;
}

.close-btn {
  position: absolute;
  top: 20px;
  right: 20px;
  background: transparent;
  border: none;
  font-size: 24px;
  color: #888;
  cursor: pointer;
  width: 40px;
  height: 40px;
  border-radius: 50%;
  transition: all 0.3s ease;
}

.close-btn:hover {
  background: #f1f1f1;
  color: #ff4d4d;
}

.modal-header h2 {
  font-family: 'Inter', sans-serif;
  font-size: 28px;
  margin-bottom: 20px;
  font-weight: 700;
}

.user-details {
  background: #f8f9fa;
  padding: 20px;
  border-radius: 12px;
  margin-bottom: 25px;
}

.detail-row {
  font-family: 'Inter', sans-serif;
  font-size: 16px;
}

.value {
  font-family: 'Inter', sans-serif;
  font-weight: 500;
}

.label {
  font-family: 'Inter', sans-serif;
  font-weight: 700;
  color: #E63946;
  margin-right: 10px;
}

.add-company-bar {
  display: flex;
  align-items: center;
  justify-content: space-between;
  background: #ffffff;
  border: 1px solid #e0e0e0;
  padding: 15px 20px;
  border-radius: 12px;
  margin-bottom: 30px;
  box-shadow: 0 4px 12px rgba(0,0,0,0.03);
}

.add-label {
  font-family: 'Inter', sans-serif;
  font-weight: 700;
  font-size: 16px;
  color: #333;
}

.add-controls {
  display: flex;
  gap: 15px;
  flex: 1;
  max-width: 450px;
  margin-left: 20px;
}

.modern-select {
  font-family: 'Inter', sans-serif;
  flex: 1;
  padding: 10px 15px;
  border: 1px solid #ccc;
  border-radius: 8px;
  font-size: 15px;
  outline: none;
  transition: border-color 0.2s;
  box-sizing: border-box;
}

.modern-select:focus {
  border-color: #E63946;
}

.btn-add-company {
  font-family: 'Inter', sans-serif;
  background: #E63946;
  color: white;
  border: none;
  padding: 10px 20px;
  border-radius: 8px;
  font-weight: 700;
  cursor: pointer;
  transition: all 0.2s ease;
  display: flex;
  align-items: center;
  gap: 8px;
}

.btn-add-company:hover:not(:disabled) {
  background: #d62828;
  transform: translateY(-1px);
}

.btn-add-company:disabled {
  background: #ccc;
  cursor: not-allowed;
}

.section-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 0px;
}

.section-header h2 {
  font-family: 'Inter', sans-serif;
  font-weight: 700;
  font-size: 20px;
}

.btn {
  background: #f8f9fa;
  border: 1px solid #e0e0e0;
  border-radius: 8px;
  padding: 8px 12px;
  cursor: pointer;
  color: #555;
  transition: all 0.2s;
}

.active-btn {
  background: #E63946;
  color: white;
  border-color: #E63946;
}

.sort-toolbar {
  background: #f8f9fa;
  padding: 15px;
  border-radius: 8px;
  margin-bottom: 20px;
  display: flex;
  align-items: center;
  gap: 15px;
}

.sort-label {
  font-family: 'Inter', sans-serif;
  font-weight: 600;
  color: #555;
}

.sort-options {
  display: flex;
  gap: 15px;
  flex: 1;
}

.search-container {
  flex: 1;
  min-width: 0;
}

.search-input {
  font-family: 'Inter', sans-serif;
  width: 100%;
  padding: 8px 12px;
  border: 1px solid #ddd;
  border-radius: 6px;
  outline: none;
  box-sizing: border-box;
}

.search-input:focus {
  border-color: #E63946;
}

table {
  width: 100%;
  border-collapse: collapse;
  margin-top: 0;
  font-family: 'Inter', sans-serif;
}

td {
  font-family: 'Inter', sans-serif;
  padding: 15px;
  border-bottom: 1px solid #eee;
}

strong {
  font-family: 'Inter', sans-serif;
  font-weight: 600;
}

.country-badge {
  font-family: 'Inter', sans-serif;
  font-weight: 500;
  background: #e9ecef;
  padding: 4px 10px;
  border-radius: 20px;
  font-size: 14px;
}

.no-data {
  font-family: 'Inter', sans-serif;
  text-align: center;
  color: #888;
  font-style: italic;
  margin-top: 30px;
  padding: 20px;
  background: #f8f9fa;
  border-radius: 8px;
}

.slide-down-enter-active,
.slide-down-leave-active {
  transition: all 0.3s ease;
}

.slide-down-enter-from,
.slide-down-leave-to {
  opacity: 0;
  transform: translateY(-10px);
}

@media (max-width: 600px) {
  .add-company-bar {
    flex-direction: column;
    align-items: flex-start;
    gap: 10px;
  }
  
  .add-controls {
    margin-left: 0;
    max-width: 100%;
    width: 100%;
  }
  
  .sort-toolbar {
    flex-direction: column;
    align-items: flex-start;
  }
  
  .sort-options {
    flex-direction: column;
    width: 100%;
  }
}
</style>

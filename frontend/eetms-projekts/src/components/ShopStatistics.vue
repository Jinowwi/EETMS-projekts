<template>
  <div class="shop-statistics-container">
    <div class="warning-banner" v-if="showWarning">
      <div class="warning-content">
        <span class="warning-icon">!</span>
        <span>{{ warningMessage }}</span>
        <button class="warning-close" @click="showWarning = false">×</button>
      </div>
    </div>

    <div class="controls-row">
      <div class="shop-selector">
        <div class="filter-panel">
          <div class="filter-content">
            <div class="filter-section-header">Filter by:</div>

            <div class="filter-item" @click="toggleTypeDropdown">
              <div class="filter-item-content">
                <span>Type</span>
                <span class="filter-count" v-if="selectedTypes.length > 0">
                  {{ selectedTypes.length }} selected
                </span>
              </div>
              <span class="arrow" :class="{ rotated: typeDropdownOpen }">›</span>
            </div>

            <div v-if="typeDropdownOpen" class="submenu">
              <div 
                class="submenu-item"
                :class="{ active: selectedTypes.length === shopTypes.length }"
                @click.stop="toggleAllTypes"
              >
                <input type="checkbox" :checked="selectedTypes.length === shopTypes.length" readonly />
                <span>All types</span>
              </div>
              <div class="submenu-divider"></div>
              <div 
                v-for="type in shopTypes" 
                :key="type" 
                class="submenu-item"
                :class="{ active: selectedTypes.includes(type) }"
                @click.stop="toggleType(type)"
              >
                <input type="checkbox" :checked="selectedTypes.includes(type)" readonly />
                <span>{{ type }}</span>
              </div>
            </div>

            <div class="filter-item" @click="toggleCountryDropdown">
              <div class="filter-item-content">
                <span>Country</span>
                <span class="filter-count" v-if="selectedCountries.length > 0">
                  {{ selectedCountries.length }} selected
                </span>
              </div>
              <span class="arrow" :class="{ rotated: countryDropdownOpen }">›</span>
            </div>

            <div v-if="countryDropdownOpen" class="submenu">
              <div 
                class="submenu-item"
                :class="{ active: selectedCountries.length === countries.length }"
                @click.stop="toggleAllCountries"
              >
                <input type="checkbox" :checked="selectedCountries.length === countries.length" readonly />
                <span>All countries</span>
              </div>
              <div class="submenu-divider"></div>
              <div 
                v-for="country in countries" 
                :key="country" 
                class="submenu-item"
                :class="{ active: selectedCountries.includes(country) }"
                @click.stop="toggleCountry(country)"
              >
                <input type="checkbox" :checked="selectedCountries.includes(country)" readonly />
                <span>{{ country }}</span>
              </div>
            </div>

            <div class="filter-divider"></div>

            <div class="filter-section-header">
              Select shops 
              <span class="shops-count">({{ filteredShops.length }} available)</span>
            </div>

            <div class="filter-item" @click="toggleShopsDropdown">
              <input type="checkbox" :checked="isAllFilteredSelected" @click.stop="toggleSelectAll" />
              <span>All shops</span>
              <span class="arrow" :class="{ rotated: shopsDropdownOpen }">›</span>
            </div>

            <div v-if="shopsDropdownOpen" class="submenu shops-list">
              <div 
                v-for="shop in filteredShops" 
                :key="shop.shopID" 
                class="submenu-item shop-item"
                :class="{ selected: selectedShops.includes(shop.shopID) }"
                @click.stop="toggleShop(shop.shopID)"
              >
                <div class="shop-info-item">
                  <input type="checkbox" :checked="selectedShops.includes(shop.shopID)" readonly />
                  <div class="shop-details">
                    <div class="shop-code">{{ shop.code }}</div>
                    <div class="shop-address">{{ shop.address }}</div>
                  </div>
                </div>
              </div>
              <div v-if="filteredShops.length === 0" class="no-shops">
                <p>No shops match your filters</p>
                <button class="btn-reset-small" @click="clearFiltersKeepPanel">Reset filters</button>
              </div>
            </div>

            <div class="filter-actions">
              <button 
                class="btn-clear" 
                @click="clearFilters"
                :disabled="isApplying || (selectedTypes.length === 0 && selectedCountries.length === 0 && selectedShops.length === 0)"
              >
                Clear All
              </button>
              <button 
                class="btn-apply" 
                @click="applyFilters"
                :disabled="isApplying || selectedShops.length === 0"
              >
                <span v-if="isApplying" class="btn-spinner"></span>
                <span v-else>Apply</span>
              </button>
            </div>
          </div>
        </div>
      </div>

      <div class="time-period-card" v-if="appliedShops.length > 0">
        <div class="time-period-section">
          <div class="time-period">
            <label>Time period:</label>
            <select v-model="selectedPreset" @change="onPresetChange" class="preset-select" :disabled="isApplying">
              <option value="week">Past Week</option>
              <option value="month">Past Month</option>
              <option value="3months">Past 3 Months</option>
              <option value="6months">Past 6 Months</option>
              <option value="year">Past Year</option>
              <option value="custom">Custom Range</option>
            </select>

            <div class="date-inputs">
              <flat-pickr  
                v-model="selectedDateRange[0]"
                :config="startDateConfig"
                class="date-input"
                :disabled="selectedPreset !== 'custom' || isApplying"
                @on-change="onDateInputChange"
              />
              <span class="date-separator">-</span>
              <flat-pickr 
                v-model="selectedDateRange[1]"
                :config="endDateConfig"
                class="date-input"
                :disabled="selectedPreset !== 'custom' || isApplying"
                @on-change="onDateInputChange"
              />
            </div>
          </div>

          <button 
            class="btn-apply-dates" 
            @click="applyDateRange"
            :disabled="isApplying || !hasDateChanges || !isDateRangeValid()"
            v-if="hasDateChanges"
          >
            <span v-if="isApplying" class="btn-spinner"></span>
            <span v-else>Apply</span>
          </button>
        </div>
      </div>
    </div>

    <div class="content-row" v-if="appliedShops.length > 0">
      <div class="chart-section" v-if="companies.length > 0">
        <div class="chart-header">
          <h2>Working hours of each company</h2>
          <div class="chart-controls">
            <div class="chart-type-switcher">
              <button 
                class="chart-type-btn"
                :class="{ active: chartType === 'pie' }"
                @click="changeChartType('pie')"
                :disabled="isApplying"
                title="Pie Chart"
              >
                <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                  <path d="M21.21 15.89A10 10 0 1 1 8 2.83"></path>
                  <path d="M22 12A10 10 0 0 0 12 2v10z"></path>
                </svg>
              </button>
              <button 
                class="chart-type-btn"
                :class="{ active: chartType === 'bar' }"
                @click="changeChartType('bar')"
                :disabled="isApplying"
                title="Bar Chart"
              >
                <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                  <line x1="12" y1="20" x2="12" y2="10"></line>
                  <line x1="18" y1="20" x2="18" y2="4"></line>
                  <line x1="6" y1="20" x2="6" y2="16"></line>
                </svg>
              </button>
              <button 
                class="chart-type-btn"
                :class="{ active: chartType === 'line' }"
                @click="changeChartType('line')"
                :disabled="isApplying"
                title="Line Chart"
              >
                <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                  <polyline points="22 12 18 12 15 21 9 3 6 12 2 12"></polyline>
                </svg>
              </button>
            </div>
            <button 
              class="export-btn"
              @click="exportChartAsPNG"
              :disabled="isApplying || companies.length === 0"
              title="Export Chart as PNG"
            >
              <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4"></path>
                <polyline points="7 10 12 15 17 10"></polyline>
                <line x1="12" y1="15" x2="12" y2="3"></line>
              </svg>
              Export PNG
            </button>
          </div>
        </div>

        <div class="chart-wrapper" ref="chartWrapperElement">
          <div class="chart-container">
            <canvas ref="workingHoursChart" :key="chartKey"></canvas>
          </div>
          <div class="chart-legend">
            <div v-for="(company, index) in companies" :key="index" class="legend-item">
              <span class="legend-color" :style="{ backgroundColor: company.color }"></span>
              <span class="legend-label">{{ company.name }}</span>
            </div>
          </div>
        </div>
      </div>

      <div class="company-table" v-if="tableData.length > 0">
        <div class="table-header-section">
          <h3><strong :style="{ color: 'var(--brand-berry-light)' }">Company Details</strong></h3>
          <button 
            class="export-btn"
            @click="exportTableAsXLSX"
            :disabled="isApplying || tableData.length === 0"
            title="Export Table as Excel"
          >
            <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4"></path>
              <polyline points="7 10 12 15 17 10"></polyline>
              <line x1="12" y1="15" x2="12" y2="3"></line>
            </svg>
            Export Excel
          </button>
        </div>


        <table>
          <thead>
            <tr>
              <th @click="handleHeaderSortTable('name')" class="sortable-header">
                COMPANY NAME
                <FontAwesomeIcon 
                  v-if="sortColumnTable === 'name'" 
                  :icon="sortDirectionTable === 'asc' ? ['fas', 'sort-up'] : ['fas', 'sort-down']" 
                  class="sort-icon active" 
                />
                <FontAwesomeIcon 
                  v-else 
                  :icon="['fas', 'sort']" 
                  class="sort-icon placeholder" 
                />
              </th>
              <th>ADDRESS</th>
              <th>PHONE NUMBER</th>
              <th @click="handleHeaderSortTable('hours')" class="sortable-header">
                WORKING HOURS
                <FontAwesomeIcon 
                  v-if="sortColumnTable === 'hours'" 
                  :icon="sortDirectionTable === 'asc' ? ['fas', 'sort-up'] : ['fas', 'sort-down']" 
                  class="sort-icon active" 
                />
                <FontAwesomeIcon 
                  v-else 
                  :icon="['fas', 'sort']" 
                  class="sort-icon placeholder" 
                />
              </th>
            </tr>
          </thead>
          <tbody>
            <tr
              v-for="(row, index) in sortedTableData"
              :key="index"
              @click="openReasonModal(row)"
              style="cursor: pointer"
            >
              <td>{{ row.name }}</td>
              <td>{{ row.address }}</td>
              <td>{{ row.phone }}</td>
              <td>{{ row.hours }}</td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <div v-else-if="appliedShops.length === 0 && !isLoading" class="empty-state">
      <div class="empty-icon-circle">
        <svg width="48" height="48" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
          <rect x="3" y="3" width="18" height="18" rx="2"/>
          <path d="M9 3v18"/>
          <path d="M15 3v18"/>
          <path d="M3 9h18"/>
          <path d="M3 15h18"/>
        </svg>
      </div>
      <h3>Select shops to view reports</h3>
      <p>Choose one or more shops from the filter above</p>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, computed, nextTick } from 'vue'
import { 
  Chart, 
  ArcElement, 
  BarElement, 
  LineElement, 
  PointElement,
  CategoryScale,
  LinearScale,
  Tooltip, 
  Legend, 
  PieController,
  BarController,
  LineController
} from 'chart.js'
import ExcelJS from 'exceljs'
import html2canvas from 'html2canvas'
import CompanyReasonModal from './CompanyReasonModal.vue'
import flatPickr from 'vue-flatpickr-component' 
import 'flatpickr/dist/flatpickr.css' 
import { getAdmin, getAdminRoleLevel } from '@/services/auth.js'

Chart.register(
  ArcElement, 
  BarElement, 
  LineElement, 
  PointElement,
  CategoryScale,
  LinearScale,
  Tooltip, 
  Legend, 
  PieController,
  BarController,
  LineController
)

const API_BASE = 'http://localhost:5001/api'

const currentAdmin = getAdmin()
const roleLevel = getAdminRoleLevel()

const allShops = ref([])
const selectedShops = ref([])
const appliedShops = ref([])

const filterPanelOpen = ref(false)
const typeDropdownOpen = ref(false)
const countryDropdownOpen = ref(false)
const shopsDropdownOpen = ref(false)

const shopTypes = ref([])
const countries = ref([])
const selectedTypes = ref([])
const selectedCountries = ref([])
const appliedTypes = ref([])
const appliedCountries = ref([])

const workingHoursChart = ref(null)
const chartInstance = ref(null)
const chartType = ref('pie')
const companies = ref([])
const tableData = ref([])

const isLoading = ref(false)
const isApplying = ref(false)

const showWarning = ref(false)
const warningMessage = ref('')

const today = new Date().toISOString().split('T')[0]
const selectedDateRange = ref(['', ''])
const appliedDateRange = ref(['', ''])
const selectedPreset = ref('week')
const appliedPreset = ref('week')

const isUpdatingChart = ref(false)
const chartKey = ref(0)

const sortColumnTable = ref('name')
const sortDirectionTable = ref('asc')

const chartWrapperElement = ref(null)

const showReasonModal = ref(false)
const selectedCompanyForReasons = ref(null)
const selectedCompanyStats = ref(null) // will hold shifts etc.

const companiesValueRaw = ref([])
const nonZeroCompaniesRaw = ref([])
const isSettingPresetDates = ref(false)

const toISODate = (v) => {
  if (!v) return '' 
  const s = String(v).trim()

  if (/^\d{4}-\d{2}$/.test(s)) return s

  const us = s.match(/^(\d{2})\/(\d{2})\/(\d{4})$/) 
  if (us) return `${us[3]}-${us[1]}-${us[2]}`

  const d = new Date(v)
  return Number.isNaN(d.valueOf()) ? '' : d.toISOString().slice(0, 10)
  
}; 

const startDateConfig = computed(() => ({
  disableMobile: true, 
  altInput: true, 
  altFormat: 'd.m.Y', 
  dateFormat: 'Y-m-d', 
  allowInput: true, 
  altInputClass: 'date-input',
  maxDate: selectedDateRange.value?.[1] || null, 
})) 

const endDateConfig = computed(() => ({
  disableMobile: true, 
  altInput: true, 
  altFormat: 'd.m.Y', 
  dateFormat: 'Y-m-d',
  allowInput: true, 
  altInputClass: 'date-input', 
  minDate: selectedDateRange.value?.[0] || null,
  maxDate: today,  
}))

const openReasonModal = (row) => {
  const companyRaw = companiesValueRaw.value.find(c => c.name === row.name)
  if (!companyRaw) {
    console.warn('Company raw data not found:', row.name)
    return
  }
  
  selectedCompanyForReasons.value = row
  selectedCompanyStats.value = companyRaw  // Pass full stats with shifts
  showReasonModal.value = true
}

const closeReasonModal = () => {
  showReasonModal.value = false
  selectedCompanyForReasons.value = null
  selectedCompanyStats.value = null
}

const sortedTableData = computed(() => {
  if (!tableData.value || tableData.value.length === 0) {
    return []
  }

  return [...tableData.value].sort((a, b) => {
    let valA, valB

    if (sortColumnTable.value === 'hours') {
      valA = parseFloat(a.hours.replace('h', '')) || 0
      valB = parseFloat(b.hours.replace('h', '')) || 0
    } else if (sortColumnTable.value === 'name') {
      valA = (a.name || '').toLowerCase()
      valB = (b.name || '').toLowerCase()
    } else {
      return 0
    }

    if (valA < valB) return sortDirectionTable.value === 'asc' ? -1 : 1
    if (valA > valB) return sortDirectionTable.value === 'asc' ? 1 : -1
    return 0
  })
})

const handleHeaderSortTable = (column) => {
  if (sortColumnTable.value === column) {
    sortDirectionTable.value = sortDirectionTable.value === 'asc' ? 'desc' : 'asc'
  } else {
    sortColumnTable.value = column
    sortDirectionTable.value = 'asc'
  }
}

const filteredShops = computed(() => {
  let result = allShops.value
  if (selectedTypes.value.length > 0) {
    result = result.filter(shop => selectedTypes.value.includes(shop.type))
  }
  if (selectedCountries.value.length > 0) {
    result = result.filter(shop => selectedCountries.value.includes(shop.country))
  }
  return result
})

const isAllFilteredSelected = computed(() => {
  return filteredShops.value.length > 0 && 
         selectedShops.value.length === filteredShops.value.length &&
         filteredShops.value.every(shop => selectedShops.value.includes(shop.shopID))
})

const hasDateChanges = computed(() => {
  return selectedDateRange.value[0] !== appliedDateRange.value[0] ||
         selectedDateRange.value[1] !== appliedDateRange.value[1]
})

const getAppliedFilterDescription = () => {
  const parts = []
  if (appliedTypes.value.length > 0) {
    parts.push(appliedTypes.value.join(', '))
  }
  if (appliedCountries.value.length > 0) {
    parts.push(appliedCountries.value.join(', '))
  }
  return parts.join(' • ')
}

const showWarningMessage = (message) => {
  warningMessage.value = message
  showWarning.value = true
  setTimeout(() => {
    showWarning.value = false
  }, 80)
}

const toggleTypeDropdown = () => {
  typeDropdownOpen.value = !typeDropdownOpen.value
  countryDropdownOpen.value = false
  shopsDropdownOpen.value = false
}

const toggleCountryDropdown = () => {
  countryDropdownOpen.value = !countryDropdownOpen.value
  typeDropdownOpen.value = false
  shopsDropdownOpen.value = false
}

const toggleShopsDropdown = () => {
  shopsDropdownOpen.value = !shopsDropdownOpen.value
  typeDropdownOpen.value = false
  countryDropdownOpen.value = false
}

const toggleType = (type) => {
  const index = selectedTypes.value.indexOf(type)
  if (index > -1) {
    selectedTypes.value.splice(index, 1)
  } else {
    selectedTypes.value.push(type)
  }
  updateShopsAfterFilter()
}

const toggleCountry = (country) => {
  const index = selectedCountries.value.indexOf(country)
  if (index > -1) {
    selectedCountries.value.splice(index, 1)
  } else {
    selectedCountries.value.push(country)
  }
  updateShopsAfterFilter()
}

const toggleAllTypes = () => {
  if (selectedTypes.value.length === shopTypes.value.length) {
    selectedTypes.value = []
  } else {
    selectedTypes.value = [...shopTypes.value]
  }
  updateShopsAfterFilter()
}

const toggleAllCountries = () => {
  if (selectedCountries.value.length === countries.value.length) {
    selectedCountries.value = []
  } else {
    selectedCountries.value = [...countries.value]
  }
  updateShopsAfterFilter()
}

const updateShopsAfterFilter = () => {
  const validShopIds = filteredShops.value.map(shop => shop.shopID)
  selectedShops.value = selectedShops.value.filter(id => validShopIds.includes(id))
}

const toggleSelectAll = () => {
  if (isAllFilteredSelected.value) {
    selectedShops.value = []
  } else {
    selectedShops.value = filteredShops.value.map(shop => shop.shopID)
  }
}

const toggleShop = (shopId) => {
  const index = selectedShops.value.indexOf(shopId)
  if (index > -1) {
    selectedShops.value.splice(index, 1)
  } else {
    selectedShops.value.push(shopId)
  }
}

const applyFilters = async () => {
  if (selectedShops.value.length === 0) {
    showWarningMessage('Please select at least one shop to view reports')
    return
  }

  isApplying.value = true
  typeDropdownOpen.value = false
  countryDropdownOpen.value = false
  shopsDropdownOpen.value = false
  filterPanelOpen.value = false

  appliedShops.value = [...selectedShops.value]
  appliedTypes.value = [...selectedTypes.value]
  appliedCountries.value = [...selectedCountries.value]

  saveStatePending()
  saveState()

  clearData()
  
  await fetchReportData(appliedShops.value, appliedTypes.value, appliedCountries.value)

  isApplying.value = false
}

const clearFilters = () => {
  selectedTypes.value = []
  selectedCountries.value = []
  selectedShops.value = []
  typeDropdownOpen.value = false
  countryDropdownOpen.value = false
  shopsDropdownOpen.value = false
  filterPanelOpen.value = false
  clearData()
  appliedShops.value = []
  appliedTypes.value = []
  appliedCountries.value = []
  saveState()
}

const clearFiltersKeepPanel = () => {
  selectedTypes.value = []
  selectedCountries.value = []
  selectedShops.value = filteredShops.value.map(shop => shop.shopID)
}

const clearData = () => {
  tableData.value = []
  companies.value = []
  if (chartInstance.value) {
    chartInstance.value.destroy()
    chartInstance.value = null
  }
}

const onPresetChange = () => {
  isSettingPresetDates.value = true

  const endDate = new Date()
  let startDate = new Date()
  switch (selectedPreset.value) {
    case 'week':
      startDate.setDate(endDate.getDate() - 7)
      break
    case 'month':
      startDate.setMonth(endDate.getMonth() - 1)
      break
    case '3months':
      startDate.setMonth(endDate.getMonth() - 3)
      break
    case '6months':
      startDate.setMonth(endDate.getMonth() - 6)
      break
    case 'year':
      startDate.setFullYear(endDate.getFullYear() - 1)
      break
    case 'custom':
      return
  }
  selectedDateRange.value = [
    startDate.toISOString().split('T')[0],
    endDate.toISOString().split('T')[0]
  ]

  queueMicrotask(() => { isSettingPresetDates.value = false })
}

const onDateInputChange = () => {
  if (isSettingPresetDates.value) return
  selectedPreset.value = 'custom'
}

const applyDateRange = async () => {
  if (!isDateRangeValid()) {
    showWarningMessage('Please select a valid date range')
    return
  }

  isApplying.value = true

  appliedDateRange.value = [...selectedDateRange.value]
  appliedPreset.value = selectedPreset.value

  saveState()

  if (appliedShops.value.length > 0) {
    clearData()
    await fetchReportData(appliedShops.value, appliedTypes.value, appliedCountries.value)
  }

  isApplying.value = false
}

const isDateRangeValid = () => {
  if (!selectedDateRange.value[0] || !selectedDateRange.value[1]) {
    return false
  }
  const start = new Date(selectedDateRange.value[0])
  const end = new Date(selectedDateRange.value[1])
  if (start > end) {
    return false
  }
  return true
}

const saveStatePending = () => {
  localStorage.setItem('reportStatePending', JSON.stringify({
    selectedShops: selectedShops.value,
    selectedTypes: selectedTypes.value,
    selectedCountries: selectedCountries.value
  }))
}

const saveState = () => {
  localStorage.setItem('reportState', JSON.stringify({
    appliedShops: appliedShops.value,
    appliedTypes: appliedTypes.value,
    appliedCountries: appliedCountries.value,
    selectedPreset: appliedPreset.value,
    selectedDateRange: appliedDateRange.value,
    chartType: chartType.value
  }))
}

const loadState = () => {
  const saved = localStorage.getItem('reportState')
  if (!saved) return false

  const state = JSON.parse(saved)
  appliedShops.value = state.appliedShops || []
  appliedTypes.value = state.appliedTypes || []
  appliedCountries.value = state.appliedCountries || []
  appliedPreset.value = state.selectedPreset || 'week'
  chartType.value = state.chartType || 'pie'
  selectedShops.value = [...appliedShops.value]
  selectedTypes.value = [...appliedTypes.value]
  selectedCountries.value = [...appliedCountries.value]
  selectedPreset.value = appliedPreset.value

  if (appliedPreset.value !== 'custom') {
    const endDate = new Date()
    const startDate = new Date()
    switch (appliedPreset.value) {
      case 'week':    startDate.setDate(endDate.getDate() - 7);         break
      case 'month':   startDate.setMonth(endDate.getMonth() - 1);       break
      case '3months': startDate.setMonth(endDate.getMonth() - 3);       break
      case '6months': startDate.setMonth(endDate.getMonth() - 6);       break
      case 'year':    startDate.setFullYear(endDate.getFullYear() - 1); break
    }
    const range = [
      startDate.toISOString().split('T')[0],
      endDate.toISOString().split('T')[0]
    ]
    selectedDateRange.value = range
    appliedDateRange.value = range
  } else {
    appliedDateRange.value = state.selectedDateRange || ['', '']
    selectedDateRange.value = [...appliedDateRange.value]
  }

  return true
}

const fetchShops = async () => {
  try {
    const res = await fetch(`${API_BASE}/shops`)
    if (!res.ok) {
      throw new Error(`HTTP error! status: ${res.status}`)
    }
    allShops.value = await res.json()
    const uniqueTypes = [...new Set(allShops.value.map(shop => shop.type).filter(Boolean))]
    shopTypes.value = uniqueTypes.sort()
    const uniqueCountries = [...new Set(allShops.value.map(shop => shop.country).filter(Boolean))]
    countries.value = uniqueCountries.sort()
  } catch (error) {
    console.error('Failed to load shops:', error)
    showWarningMessage('Failed to load shops. Please refresh the page.')
  }
}

const fetchReportData = async (shopIds, types, countriesFilter) => {
  if (shopIds.length === 0) {
    clearData()
    return
  }
  if (!isDateRangeValid()) {
    return
  }

  isLoading.value = true

  try {
    const allShiftsPromises = shopIds.map(shopId => 
      fetch(`${API_BASE}/shifts/byshop/${shopId}`).then(res => res.json())
    )
    const allShiftsArrays = await Promise.all(allShiftsPromises)
    const allShifts = allShiftsArrays.flat()

    const startDate = new Date(appliedDateRange.value[0])
    const endDate = new Date(appliedDateRange.value[1])

    const shiftsInRange = allShifts.filter(shift => {
      const shiftstartDate = new Date(shift.startDate)
      return shiftstartDate >= startDate && shiftstartDate <= endDate
    })

    if (shiftsInRange.length === 0) {
      showWarningMessage(`No shifts found in this range`)
      clearData()
      isLoading.value = false
      return
    }

    const companyStats = {}

    for (const shift of shiftsInRange) {
      const companyReason = shift.companyReason
      if (!companyReason || !companyReason.company) {
        continue
      }
      const company = companyReason.company
      const companyId = company.companyID
      if (!companyStats[companyId]) {
        companyStats[companyId] = {
          companyId: companyId,
          name: company.companyName,
          address: company.address,
          phone: company.phoneNumber,
          remId: company.remID,
          totalHours: 0,
          shifts: []
        }
      }
      const hours = calculateShiftHours(shift)
      companyStats[companyId].totalHours += hours
      companyStats[companyId].shifts.push(shift)
    }

    const companiesArray = Object.values(companyStats)

    const filteredCompanies = roleLevel === 1
    ? companiesArray.filter(c => c.remId === currentAdmin.remId)
    : companiesArray

    const nonZeroCompanies = filteredCompanies.filter(c => c.totalHours >= 0.1)

    if (nonZeroCompanies.length === 0) {
      showWarningMessage('No companies with working hours in this range')
      clearData()
      isLoading.value = false
      return
    }

    const totalHours = nonZeroCompanies.reduce((sum, company) => sum + company.totalHours, 0)

    companiesValueRaw.value = nonZeroCompanies  // Store raw data for modal
    nonZeroCompaniesRaw.value = nonZeroCompanies

    tableData.value = nonZeroCompanies.map(company => ({
      name: company.name,
      address: company.address,
      phone: company.phone,
      hours: `${company.totalHours.toFixed(1)}h`
    }))

    companies.value = nonZeroCompanies.map((company, index) => ({
      name: company.name,
      percentage: totalHours > 0 ? ((company.totalHours / totalHours) * 100).toFixed(1) : 0,
      hours: company.totalHours,
      color: getCompanyColor(index)
    }))

    await nextTick()
    await nextTick()
    
    if (workingHoursChart.value) {
      updateChart()
    } else {
      setTimeout(() => {
        if (workingHoursChart.value) {
          updateChart()
        }
      }, 100)
    }
  } catch (error) {
    console.error('Failed to fetch shifts:', error)
    showWarningMessage('Failed to load shift data. Please try again.')
    tableData.value = []
    companies.value = []
  } finally {
    isLoading.value = false
  }
}

const calculateShiftHours = (shift) => {
  try {
    const startTime = shift.startTime || shift.startTime
    const endTime = shift.endTime || shift.endTime
    const startDate = shift.startDate || shift.startDate
    const endDate = shift.endDate || shift.EndDate
    if (!startTime || !endTime) return 0
    const start = new Date(`${startDate}T${startTime}`)
    const end = new Date(`${endDate}T${endTime}`)
    const diffMs = end - start
    const diffHours = diffMs / (1000 * 60 * 60)
    return diffHours > 0 ? diffHours : 0
  } catch (error) {
    console.error('Error calculating shift hours:', error, shift)
    return 0
  }
}

const getCompanyColor = (index) => {
  const palette = [
    '#FF6B6B', '#4ECDC4', '#45B7D1', '#FFA07A', '#98D8C8',
    '#F7DC6F', '#BB8FCE', '#85C1E2', '#F8B739', '#52B788',
    '#E63946', '#A8DADC', '#457B9D', '#F4A261', '#2A9D8F',
    '#E76F51', '#264653', '#E9C46A', '#F77F00', '#06AED5',
    '#FF99C8', '#A0CED9', '#F4ACB7', '#FFD6A5', '#CAFFBF',
    '#9BF6FF', '#BDB2FF', '#FFC6FF', '#FFADAD', '#FDFFB6'
  ]
  if (index < palette.length) {
    return palette[index]
  }
  const goldenRatio = 0.618033988749895
  const hue = ((index - palette.length) * goldenRatio * 360) % 360
  const saturation = 70 + (index % 3) * 10
  const lightness = 50 + (index % 3) * 8
  return `hsl(${Math.round(hue)}, ${saturation}%, ${lightness}%)`
}

const changeChartType = async (type) => {
  if (isUpdatingChart.value || !workingHoursChart.value || companies.value.length === 0) {
    return
  }
  isUpdatingChart.value = true
  try {
    if (chartInstance.value) {
      chartInstance.value.stop()
      chartInstance.value.destroy()
      chartInstance.value = null
    }
    chartType.value = type
    chartKey.value += 1
    await nextTick()
    await nextTick()
    await new Promise(resolve => setTimeout(resolve, 150))
    await updateChart()
    saveState()
  } finally {
    isUpdatingChart.value = false
  }
}

const updateChart = async () => {
  if (!workingHoursChart.value) {
    console.error('Canvas ref not available')
    return
  }
  const canvas = workingHoursChart.value
  if (!canvas.getContext) {
    console.error('Canvas context not available')
    return
  }
  if (chartType.value === 'pie') {
    chartInstance.value = new Chart(canvas, {
      type: 'pie',
      data: {
        labels: companies.value.map(c => c.name),
        datasets: [{
          data: companies.value.map(c => c.percentage),
          backgroundColor: companies.value.map(c => c.color),
          borderColor: '#ffffff',
          borderWidth: 2
        }]
      },
      options: {
        responsive: true,
        maintainAspectRatio: true,
        animation: false,
        plugins: {
          legend: { display: false },
          tooltip: {
            callbacks: {
              label: function(context) {
                return context.label + ': ' + context.parsed.toFixed(1) + '%'
              }
            }
          }
        }
      }
    })
  } else if (chartType.value === 'bar') {
    chartInstance.value = new Chart(canvas, {
      type: 'bar',
      data: {
        labels: companies.value.map(c => c.name),
        datasets: [{
          label: 'Working Hours',
          data: companies.value.map(c => c.hours),
          backgroundColor: companies.value.map(c => c.color),
          borderColor: companies.value.map(c => c.color),
          borderWidth: 1
        }]
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        animation: false,
        scales: {
          y: {
            beginAtZero: true,
            ticks: {
              callback: function(value) {
                return value.toFixed(1) + 'h'
              }
            }
          },
          x: {
            ticks: {
              maxRotation: 45,
              minRotation: 45
            }
          }
        },
        plugins: {
          legend: { display: false },
          tooltip: {
            callbacks: {
              label: function(context) {
                return 'Hours: ' + context.parsed.y.toFixed(1) + 'h'
              }
            }
          }
        }
      }
    })
  } else if (chartType.value === 'line') {
    chartInstance.value = new Chart(canvas, {
      type: 'line',
      data: {
        labels: companies.value.map(c => c.name),
        datasets: [{
          label: 'Working Hours',
          data: companies.value.map(c => c.hours),
          backgroundColor: 'rgba(161, 41, 113, 0.1)',
          borderColor: '#a12971',
          borderWidth: 2,
          pointBackgroundColor: companies.value.map(c => c.color),
          pointBorderColor: '#fff',
          pointBorderWidth: 2,
          pointRadius: 5,
          pointHoverRadius: 7,
          tension: 0.4
        }]
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        animation: false,
        scales: {
          y: {
            beginAtZero: true,
            ticks: {
              callback: function(value) {
                return value.toFixed(1) + 'h'
              }
            }
          },
          x: {
            ticks: {
              maxRotation: 45,
              minRotation: 45
            }
          }
        },
        plugins: {
          legend: { display: false },
          tooltip: {
            callbacks: {
              label: function(context) {
                return context.label + ': ' + context.parsed.y.toFixed(1) + 'h'
              }
            }
          }
        }
      }
    })
  }
}

const exportChartAsPNG = async () => {
  try {
    if (!chartInstance.value) {
      showWarningMessage('No chart available to export')
      return
    }
    
    if (!chartWrapperElement.value) {
      showWarningMessage('Chart wrapper not found')
      return
    }
    
    const canvas = await html2canvas(chartWrapperElement.value, {
      backgroundColor: '#ffffff',
      scale: 2
    })
    
    const url = canvas.toDataURL('image/png')
    const link = document.createElement('a')
    
    const startDate = appliedDateRange.value[0]
    const endDate = appliedDateRange.value[1]
    link.download = `company-working-hours-chart-${startDate}_to_${endDate}.png`
    
    link.href = url
    document.body.appendChild(link)
    link.click()
    document.body.removeChild(link)
  } catch (error) {
    console.error('Export chart error:', error)
    showWarningMessage('Failed to export chart: ' + error.message)
  }
}

const exportTableAsXLSX = async () => {
  if (tableData.value.length === 0) {
    showWarningMessage('No table data available to export')
    return
  }

  const workbook = new ExcelJS.Workbook()

  const worksheet = workbook.addWorksheet('Company Details')

  worksheet.columns = [
    { header: 'Company Name', key: 'name', width: 30 },
    { header: 'Address', key: 'address', width: 40 },
    { header: 'Phone Number', key: 'phone', width: 20 },
    { header: 'Working Hours', key: 'hours', width: 15 }
  ]

  sortedTableData.value.forEach((row) => {
    worksheet.addRow({
      name: row.name,
      address: row.address,
      phone: row.phone,
      hours: row.hours
    })
  })

  const lastRow = sortedTableData.value.length + 1
  worksheet.addTable({
    name: 'CompanyTable',
    ref: `A1:D${lastRow}`,
    headerRow: true,
    totalsRow: false,
    style: { theme: 'TableStyleMedium2', showRowStripes: true },
    columns: [
      { name: 'Company Name', filterButton: true },
      { name: 'Address', filterButton: true },
      { name: 'Phone Number', filterButton: true },
      { name: 'Working Hours', filterButton: true }
    ],
    rows: sortedTableData.value.map(row => [row.name, row.address, row.phone, row.hours])
  })

  for (let i = 1; i <= lastRow; i++) {
    ['A', 'B', 'C', 'D'].forEach(col => {
      worksheet.getCell(`${col}${i}`).alignment = { horizontal: 'center', vertical: 'middle' }
    })
  }

  for (const company of companiesValueRaw.value) {
    const sheetName = company.name
      .replace(/[\\/*?:\[\]]/g, '')
      .substring(0, 31)

    const companySheet = workbook.addWorksheet(sheetName)

    companySheet.columns = [
      { header: 'Start Date',    key: 'startDate',   width: 15 },
      { header: 'Start Time',    key: 'startTime',   width: 12 },
      { header: 'End Date',      key: 'endDate',     width: 15 },
      { header: 'End Time',      key: 'endTime',     width: 12 },
      { header: 'Reason',        key: 'reason',      width: 40 },
      { header: 'Work Hours',    key: 'workHours',   width: 12 }
    ]

    companySheet.getRow(1).eachCell(cell => {
      cell.font = { bold: true }
      cell.alignment = { horizontal: 'center', vertical: 'middle' }
      cell.fill = {
        type: 'pattern',
        pattern: 'solid',
        fgColor: { argb: 'FFD9E1F2' }
      }
    })

    const shiftRows = []

    company.shifts.forEach(shift => {
      const hours = calculateShiftHours(shift)
      const reason = shift.companyReason?.reason?.name ?? '—'

      companySheet.addRow({
        startDate: shift.startDate ?? '',
        startTime: shift.startTime ?? '',
        endDate:   shift.endDate   ?? shift.EndDate ?? '',
        endTime:   shift.endTime   ?? '',
        reason,
        workHours: parseFloat(hours.toFixed(2))
      })

      shiftRows.push([
        shift.startDate ?? '',
        shift.startTime ?? '',
        shift.endDate ?? shift.EndDate ?? '',
        shift.endTime ?? '',
        reason,
        parseFloat(hours.toFixed(2))
      ])
    })

    const shiftLastRow = company.shifts.length + 1

    // Add as a formatted table
    companySheet.addTable({
      name: `Shifts_${company.companyId}`, // must be unique across workbook
      ref: `A1:F${shiftLastRow}`,
      headerRow: true,
      totalsRow: true, // adds a totals row at the bottom
      style: { theme: 'TableStyleMedium4', showRowStripes: true },
      columns: [
        { name: 'Start Date',  filterButton: true,  totalsRowLabel: '' },
        { name: 'Start Time',  filterButton: true,  totalsRowLabel: '' },
        { name: 'End Date',    filterButton: true,  totalsRowLabel: '' },
        { name: 'End Time',    filterButton: true,  totalsRowLabel: '' },
        { name: 'Reason',      filterButton: true,  totalsRowLabel: 'Total' },
        { name: 'Work Hours',  filterButton: true,  totalsRowFunction: 'sum' }
      ],
      rows: shiftRows
    })

    for (let i = 1; i <= shiftLastRow + 1; i++) {
      ['A', 'B', 'C', 'D', 'E', 'F'].forEach(col => {
        companySheet.getCell(`${col}${i}`).alignment = {
          horizontal: 'center',
          vertical: 'middle'
        }
      })
    }
  }

  const buffer = await workbook.xlsx.writeBuffer()
  const blob = new Blob([buffer], {
    type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet'
  })
  const url = window.URL.createObjectURL(blob)
  const link = document.createElement('a')
  link.href = url

  const startDate = appliedDateRange.value[0]
  const endDate = appliedDateRange.value[1]
  link.download = `company-details-${startDate}_to_${endDate}.xlsx`

  document.body.appendChild(link)
  link.click()
  document.body.removeChild(link)
  window.URL.revokeObjectURL(url)
}

onMounted(async () => {
  await fetchShops()
  const hasState = loadState()
  if (!hasState) {
    selectedPreset.value = 'year'
    const endDate = new Date()
    const startDate = new Date()
    startDate.setFullYear(endDate.getFullYear() - 1)
    selectedDateRange.value = [
      startDate.toISOString().split('T')[0],
      endDate.toISOString().split('T')[0]
    ]
    appliedDateRange.value = [...selectedDateRange.value]
    appliedPreset.value = 'year'
    if (allShops.value.length > 0) {
      selectedShops.value = allShops.value.map(shop => shop.shopID)
      appliedShops.value = [...selectedShops.value]
    }
    saveState()
    if (appliedShops.value.length > 0) {
      await fetchReportData(appliedShops.value, appliedTypes.value, appliedCountries.value)
    }
  } else {
    const validShopIds = allShops.value.map(s => s.shopID)
    appliedShops.value = appliedShops.value.filter(id => validShopIds.includes(id))
    selectedShops.value = [...appliedShops.value]
    await nextTick()
    if (appliedShops.value.length > 0 && isDateRangeValid()) {
      await fetchReportData(appliedShops.value, appliedTypes.value, appliedCountries.value)
    }
  }

  document.addEventListener('click', (e) => {
    if (!e.target.closest('.filter-panel')) {
      filterPanelOpen.value = false
      typeDropdownOpen.value = false
      countryDropdownOpen.value = false
      shopsDropdownOpen.value = false
    }
  })
})
</script>

<style scoped>
.shop-statistics-container {
  width: 100%;
  box-sizing: border-box;
}

.controls-row {
  display: grid;
  grid-template-columns: auto 1fr;
  gap: 20px;
  margin-bottom: 30px;
  align-items: start;
}

.content-row {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 20px;
}

.warning-banner {
  position: fixed;
  top: 20px;
  right: 20px;
  z-index: 1000;
  animation: slideIn 0.3s ease-out;
}

@keyframes slideIn {
  from { transform: translateX(400px); opacity: 0; }
  to   { transform: translateX(0);     opacity: 1; }
}

.warning-content {
  background: var(--color-warning-bg);
  border: 1px solid var(--color-warning-border);
  border-radius: 12px;
  padding: 14px 18px;
  display: flex;
  align-items: center;
  gap: 12px;
  box-shadow: 0 4px 16px rgba(0,0,0,0.12);
  min-width: 280px;
  max-width: 460px;
}

.warning-icon {
  background: var(--color-warning-border);
  color: var(--color-white);
  width: 24px;
  height: 24px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: bold;
  flex-shrink: 0;
}

.warning-close {
  background: none;
  border: none;
  font-size: 22px;
  color: var(--color-warning-text);
  cursor: pointer;
  padding: 0;
  margin-left: auto;
  flex-shrink: 0;
}

.warning-close:hover { color: var(--color-black); }

.shop-selector {
  display: flex;
  flex-direction: column;
}

.filter-panel {
  position: relative;
  width: 633px;
  border-radius: 20px;
  background: rgba(255, 255, 255, 0.82);
  backdrop-filter: blur(16px);
  -webkit-backdrop-filter: blur(16px);
  border: 1.5px solid rgba(255, 255, 255, 0.9);
  box-shadow: 0 8px 32px rgba(161, 41, 113, 0.08), 0 2px 8px rgba(0,0,0,0.06);
  padding: 0;
  overflow: hidden;
}

.filter-content {
  border-top: none;
}

.filter-section-header {
  padding: 10px 16px;
  font-size: 11px;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.5px;
  color: var(--color-text-dim);
  background: rgba(161, 41, 113, 0.05);
}

.shops-count {
  color: var(--brand-teal);
  text-transform: none;
  font-weight: 500;
}

.filter-item {
  padding: 11px 16px;
  display: flex;
  justify-content: space-between;
  align-items: center;
  cursor: pointer;
  border-bottom: 1px solid rgba(161, 41, 113, 0.07);
  transition: background 0.15s;
}

.filter-item:hover { background: rgba(161, 41, 113, 0.04); }

.filter-item-content {
  display: flex;
  align-items: center;
  gap: 8px;
  flex: 1;
  font-size: 14px;
}

.filter-count {
  background: var(--brand-berry);
  color: var(--color-white);
  padding: 2px 8px;
  border-radius: 12px;
  font-size: 11px;
  font-weight: 500;
}

.arrow {
  color: var(--color-text-light);
  font-size: 16px;
  transition: transform 0.2s;
}

.arrow.rotated { transform: rotate(90deg); }

/* Checkboxes */
.filter-item input[type="checkbox"],
.submenu-item input[type="checkbox"] {
  -webkit-appearance: none;
  appearance: none;
  width: 16px;
  height: 16px;
  border: 1.5px solid var(--color-checkbox-border);
  border-radius: 4px;
  background-color: var(--color-white);
  cursor: pointer;
  position: relative;
  transition: all 0.2s ease;
  flex-shrink: 0;
}

.filter-item input[type="checkbox"]:checked,
.submenu-item input[type="checkbox"]:checked {
  background-color: var(--brand-teal);
  border-color: var(--brand-teal);
}

.filter-item input[type="checkbox"]::after,
.submenu-item input[type="checkbox"]::after {
  content: "";
  position: absolute;
  display: none;
  left: 4px;
  top: 1px;
  width: 4px;
  height: 8px;
  border: solid var(--color-white);
  border-width: 0 2px 2px 0;
  transform: rotate(45deg);
}

.filter-item input[type="checkbox"]:checked::after,
.submenu-item input[type="checkbox"]:checked::after { display: block; }

/* Submenus */
.submenu {
  background: rgba(248, 249, 250, 0.9);
  border-left: 3px solid var(--brand-berry);
}

.submenu-item {
  padding: 9px 20px;
  display: flex;
  align-items: center;
  gap: 10px;
  cursor: pointer;
  border-bottom: 1px solid rgba(161, 41, 113, 0.06);
  font-size: 13px;
  transition: background 0.15s;
}

.submenu-item:hover { background: rgba(161, 41, 113, 0.05); }
.submenu-item.active { background: rgba(161, 41, 113, 0.07); font-weight: 500; }
.submenu-item.selected { background: var(--row-hover-gradient); }

.submenu-divider { height: 1px; background: var(--color-border); margin: 3px 0; }

.shops-list { max-height: 260px; overflow-y: auto; }
.shops-list::-webkit-scrollbar { width: 4px; }
.shops-list::-webkit-scrollbar-thumb { background: var(--brand-berry); border-radius: 4px; }

.shop-item { padding: 8px 20px; }

/* .shop-info-item {
  display: flex;
  align-items: flex-start;
  gap: 10px;
  width: 100%;
} */

.shop-details { flex: 1; }
.shop-code { font-weight: 600; color: var(--brand-berry); font-size: 13px; }
.shop-address { font-size: 11px; color: var(--color-text-dim); margin-top: 2px; }

.no-shops { padding: 24px 16px; text-align: center; color: var(--color-text-light); }
.no-shops p { margin: 0 0 12px 0; font-size: 13px; }

.btn-reset-small {
  padding: 6px 14px;
  background: var(--brand-berry);
  color: var(--color-white);
  border: none;
  border-radius: 20px;
  cursor: pointer;
  font-size: 12px;
  font-family: 'Inter', sans-serif;
}

.filter-divider { height: 1px; background: var(--color-border); margin: 6px 0; }

.filter-actions {
  display: flex;
  gap: 8px;
  padding: 12px 16px;
  background: rgba(255,255,255,0.5);
}

.btn-clear,
.btn-apply {
  flex: 1;
  padding: 9px;
  border-radius: 20px;
  font-weight: 600;
  font-size: 13px;
  font-family: 'Inter', sans-serif;
  cursor: pointer;
  transition: all 0.2s;
}

.btn-clear {
  background: var(--color-white);
  border: 1.5px solid var(--brand-berry);
  color: var(--brand-berry);
}

.btn-clear:hover:not(:disabled) { background: rgba(161, 41, 113, 0.06); }
.btn-clear:disabled { opacity: 0.4; cursor: not-allowed; }

.btn-apply {
  background: var(--brand-berry);
  border: none;
  color: var(--color-white);
  position: relative;
}

.btn-apply:hover:not(:disabled) { background: var(--brand-berry-light); }
.btn-apply:disabled { opacity: 0.5; cursor: not-allowed; }

/* ── Time period card ── */
.time-period-card {
  display: flex;
  flex-direction: column;
  gap: 16px;
  justify-content: flex-start;
  max-width: 790px;
}

.time-period-section {
  display: flex;
  flex-direction: column;
  gap: 14px;
  background: rgba(255, 255, 255, 0.82);
  backdrop-filter: blur(16px);
  -webkit-backdrop-filter: blur(16px);
  border: 1.5px solid rgba(255, 255, 255, 0.9);
  border-radius: 20px;
  padding: 20px;
  width: 100%;
  box-shadow: 0 8px 32px rgba(161, 41, 113, 0.08), 0 2px 8px rgba(0,0,0,0.06);
}

.time-period {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.time-period label {
  font-size: 14px;
  font-weight: 700;
  color: var(--brand-berry);
}

.preset-select {
  padding: 10px 14px;
  border: 1.5px solid rgba(161, 41, 113, 0.2);
  border-radius: 20px;
  font-size: 14px;
  font-family: 'Inter', sans-serif;
  background: rgba(255,255,255,0.85);
  cursor: pointer;
  width: 100%;
  transition: border-color 0.2s;
  outline: none;
}

.preset-select:focus { border-color: var(--brand-berry); }
.preset-select:disabled { opacity: 0.6; cursor: not-allowed; }

.date-inputs {
  display: flex;
  align-items: center;
  gap: 8px;
}

.date-separator { color: var(--color-text-dim); font-weight: bold; }

:deep(input.date-input) {
  padding: 10px 12px;
  border: 1.5px solid rgba(161, 41, 113, 0.2);
  border-radius: 20px;
  font-size: 14px;
  font-family: 'Inter', sans-serif;
  width: 100%;
  background: rgba(255,255,255,0.85);
  transition: border-color 0.2s;
  outline: none;
  box-sizing: border-box;
}

:deep(input.date-input:focus) { border-color: var(--brand-teal); }
:deep(input.date-input:disabled) { opacity: 0.6; cursor: not-allowed; }

.btn-apply-dates {
  background: var(--brand-berry);
  color: white;
  border: none;
  border-radius: 20px;
  padding: 10px 22px;
  font-size: 14px;
  font-weight: 600;
  font-family: 'Inter', sans-serif;
  cursor: pointer;
  transition: all 0.2s ease;
  height: 42px;
  display: flex;
  align-items: center;
  justify-content: center;
  align-self: flex-end;
  box-shadow: 0 2px 8px rgba(161, 41, 113, 0.25);
}

.btn-apply-dates:hover:not(:disabled) { background: var(--brand-berry-light); transform: translateY(-1px); }
.btn-apply-dates:disabled { opacity: 0.5; cursor: not-allowed; }

.shop-info {
  background: rgba(255, 255, 255, 0.82);
  backdrop-filter: blur(16px);
  -webkit-backdrop-filter: blur(16px);
  border: 1.5px solid rgba(255, 255, 255, 0.9);
  border-radius: 20px;
  padding: 20px 24px;
  box-shadow: 0 8px 32px rgba(161, 41, 113, 0.08);
  text-align: right;
}

.shop-info h3 { font-size: 16px; font-weight: 600; margin: 0 0 4px 0; color: var(--brand-teal); }
.shop-info .shop-address { font-size: 13px; color: var(--color-text-dim); margin: 0; }

/* ── Chart section ── */
.chart-section {
  background: rgba(255, 255, 255, 0.82);
  backdrop-filter: blur(16px);
  -webkit-backdrop-filter: blur(16px);
  border: 1.5px solid rgba(255, 255, 255, 0.9);
  padding: 24px;
  border-radius: 20px;
  box-shadow: 0 8px 32px rgba(161, 41, 113, 0.08), 0 2px 8px rgba(0,0,0,0.06);
}

.chart-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 20px;
  flex-wrap: wrap;
  gap: 12px;
}

.chart-header h2 {
  font-size: 16px;
  font-weight: 700;
  margin: 0;
  color: var(--color-text-main);
}

.chart-controls { display: flex; align-items: center; gap: 10px; }

.chart-type-switcher {
  display: flex;
  gap: 4px;
  background: rgba(161, 41, 113, 0.07);
  padding: 4px;
  border-radius: 10px;
}

.chart-type-btn {
  padding: 7px 10px;
  background: transparent;
  border: none;
  border-radius: 7px;
  cursor: pointer;
  color: var(--color-text-dim);
  transition: all 0.2s;
  display: flex;
  align-items: center;
  justify-content: center;
}

.chart-type-btn:hover { background: rgba(255,255,255,0.7); color: var(--brand-berry); }
.chart-type-btn.active { background: var(--brand-berry); color: var(--color-white); }

.chart-wrapper { display: flex; flex-direction: column; gap: 16px; align-items: center; }

.chart-container {
  width: 100%;
  height: 360px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.chart-wrapper canvas { max-width: 100%; max-height: 100%; }

.chart-legend {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 8px 16px;
  width: 100%;
}

.legend-item { display: flex; align-items: center; gap: 7px; }
.legend-color { width: 11px; height: 11px; border-radius: 50%; flex-shrink: 0; }
.legend-label { font-size: 12px; color: var(--color-text-main); line-height: 1.2; }

/* ── Company table ── */
.company-table {
  background: rgba(255, 255, 255, 0.82);
  backdrop-filter: blur(16px);
  width: 100%;
  -webkit-backdrop-filter: blur(16px);
  border: 1.5px solid rgba(255, 255, 255, 0.9);
  padding: 24px;
  border-radius: 20px;
  box-shadow: 0 8px 32px rgba(161, 41, 113, 0.08), 0 2px 8px rgba(0,0,0,0.06);
  overflow-x: visible;
}

.company-table::-webkit-scrollbar { height: 0; display: none; }
.company-table { scrollbar-width: none; }

.table-header-section {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 12px;
  margin-bottom: 16px;
  flex-wrap: wrap;
}

.table-header-section h3 {
  font-size: 18px;
  font-weight: 700;
  margin: 0;
  color: var(--brand-berry-light);
}

.company-table > table {
  width: 100%;
  min-width: 700px;
  border-collapse: separate;
  border-spacing: 0;
  border: 1.5px solid rgba(161, 41, 113, 0.12);
  border-radius: 14px;
  overflow: hidden;
  font-size: 13px;
  min-width: 480px;
}

.company-table > table > thead > tr > th {
  text-align: center;
  padding: 13px 16px;
  font-size: 11px;
  font-weight: 700;
  color: var(--color-white);
  text-transform: uppercase;
  letter-spacing: 0.5px;
  background: var(--header-gradient);
  white-space: nowrap;
  border: none;
}

.sortable-header {
  cursor: pointer;
  user-select: none;
  transition: opacity 0.2s;
  white-space: nowrap;
}

.sortable-header:hover { opacity: 0.85; }

.company-table > table > tbody > tr {
  border-bottom: 1px solid rgba(161, 41, 113, 0.07);
  transition: background 0.15s;
  cursor: pointer;
}

.company-table > table > tbody > tr:hover { background: rgba(161, 41, 113, 0.04); }
.company-table > table > tbody > tr:last-child { border-bottom: none; }

.company-table > table > tbody > tr > td {
  padding: 13px 16px;
  font-size: 13px;
  color: var(--color-text-main);
  text-align: center;
  border: none;
}

.company-table > table > tbody > tr > td:first-child,
.company-table > table > tbody > tr > td:last-child {
  font-weight: 600;
}

/* ── Export button ── */
.export-btn {
  display: flex;
  align-items: center;
  gap: 7px;
  padding: 8px 14px;
  background: rgba(255,255,255,0.8);
  color: var(--brand-teal);
  border: 1.5px solid rgba(43, 164, 146, 0.25);
  border-radius: 10px;
  font-size: 13px;
  font-weight: 600;
  font-family: 'Inter', sans-serif;
  cursor: pointer;
  transition: all 0.2s ease;
  white-space: nowrap;
  flex-shrink: 0;
}

.export-btn:hover:not(:disabled) { transform: translateY(-1px); box-shadow: 0 4px 10px rgba(43, 164, 146, 0.2); }
.export-btn:disabled { opacity: 0.45; cursor: not-allowed; }

.empty-state {
  background: rgba(255, 255, 255, 0.75);
  backdrop-filter: blur(12px);
  -webkit-backdrop-filter: blur(12px);
  border: 1.5px solid rgba(255, 255, 255, 0.9);
  padding: 60px 40px;
  border-radius: 20px;
  text-align: center;
  margin-top: 20px;
  box-shadow: 0 8px 32px rgba(0,0,0,0.06);
}

.empty-icon-circle {
  width: 72px;
  height: 72px;
  background: rgba(255,255,255,0.9);
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  margin: 0 auto 16px;
  color: var(--brand-berry);
  box-shadow: 0 4px 12px rgba(161, 41, 113, 0.1);
}

.empty-state h3 { font-size: 18px; font-weight: 600; margin: 0 0 8px; color: var(--color-text-main); }
.empty-state p  { font-size: 14px; color: var(--color-text-dim); margin: 0; }

.btn-spinner {
  width: 15px;
  height: 15px;
  border-radius: 50%;
  border: 2px solid rgba(255,255,255,0.35);
  border-top-color: var(--color-white);
  display: inline-block;
  animation: btn-spin 0.7s linear infinite;
}

@keyframes btn-spin { to { transform: rotate(360deg); } }

@media (max-width: 1400px) {
  /* .shop-statistics-container { margin-right: 50px; } */
  .controls-row {
    grid-template-columns: minmax(0, 0.65fr) minmax(0, 1fr); /* ratio matches desired panel size */
    gap: 20px;
  }
  .filter-panel { width: 100%; }
  .time-period-card { max-width: 94%; }
  .chart-legend { grid-template-columns: repeat(3, 1fr); }
  .company-table { width: 670px; }
}

@media (max-width: 1100px) {
  .content-row { grid-template-columns: 1fr; }
}

@media (max-width: 900px) {
  .chart-container { height: 300px; }
  .chart-legend { grid-template-columns: repeat(2, 1fr); }
}

@media (max-width: 768px) {
  .chart-container { height: 260px; }
  .chart-legend { grid-template-columns: 1fr 1fr; }

  .chart-header h2 { font-size: 14px; }
  .chart-type-btn { padding: 6px 8px; }
  .chart-type-btn svg { width: 16px; height: 16px; }

  .company-table { padding: 16px; }
  .table-header-section h3 { font-size: 16px; }

  .filter-actions { flex-direction: row; }
  .warning-content { min-width: 220px; }
}

@media (max-width: 576px) {
  .chart-container { height: 220px; }
  .chart-legend { grid-template-columns: 1fr; }

  .chart-section { padding: 16px; }
  .company-table { padding: 12px; }

  .company-table > table > tbody > tr > td:nth-child(2) { display: none; }

  .date-inputs { flex-direction: column; gap: 8px; }
  .date-separator { display: none; }

  .empty-state { padding: 40px 20px; }
  .empty-icon-circle { width: 56px; height: 56px; }
  .empty-state h3 { font-size: 16px; }
}

@media (max-width: 480px) {
  .controls-row { gap: 12px; }
  .chart-container { height: 200px; }
  .filter-panel { border-radius: 14px; }
  .shop-info { padding: 16px; }
  .time-period-section { padding: 16px; }
}
</style>
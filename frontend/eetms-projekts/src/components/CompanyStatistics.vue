<template>
  <div class="company-statistics-container">
    <div class="warning-banner" v-if="showWarning">
      <div class="warning-content">
        <span class="warning-icon">!</span>
        <span>{{ warningMessage }}</span>
        <button class="warning-close" @click="showWarning = false">×</button>
      </div>
    </div>

    <div class="controls-row">
      <div class="company-selector">
        <div class="filter-panel">
          <div class="filter-content">
            <div class="filter-section-header">Filter by:</div>

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
                <span>{{ getCountryName(country) }}</span>
              </div>
            </div>

            <div class="filter-divider"></div>

            <div class="filter-section-header">
              Select companies 
              <span class="companies-count">({{ filteredCompanies.length }} available)</span>
            </div>

            <div class="filter-item" @click="toggleCompaniesDropdown">
              <input type="checkbox" :checked="isAllFilteredSelected" @click.stop="toggleSelectAll" />
              <span>All companies</span>
              <span class="arrow" :class="{ rotated: companiesDropdownOpen }">›</span>
            </div>

            <div v-if="companiesDropdownOpen" class="submenu companies-list">
              <div 
                v-for="company in filteredCompanies" 
                :key="company.companyID" 
                class="submenu-item company-item"
                :class="{ selected: selectedCompanies.includes(company.companyID) }"
                @click.stop="toggleCompany(company.companyID)"
              >
                <div class="company-info-item">
                  <input type="checkbox" :checked="selectedCompanies.includes(company.companyID)" readonly />
                  <div class="company-details">
                    <div class="company-name">{{ company.companyName || company.name }}</div>  
                    <div class="company-address">{{ company.address }}</div>
                  </div>
                </div>
              </div>
              <div v-if="filteredCompanies.length === 0" class="no-companies">
                <p>No companies match your filters</p>
                <button class="btn-reset-small" @click="clearFiltersKeepPanel">Reset filters</button>
              </div>
            </div>

            <div class="filter-actions">
              <button 
                class="btn-clear" 
                @click="clearFilters"
                :disabled="isApplying || (selectedCountries.length === 0 && selectedCompanies.length === 0)"
              >
                Clear All
              </button>
              <button 
                class="btn-apply" 
                @click="applyFilters"
                :disabled="isApplying || selectedCompanies.length === 0"
              >
                <span v-if="isApplying" class="btn-spinner"></span>
                <span v-else>Apply</span>
              </button>
            </div>
          </div>
        </div>
      </div>

      <div class="time-period-card" v-if="appliedCompanies.length > 0">
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
              <div class="date-field-wrapper">
                <input 
                  type="text"
                  class="date-input"
                  :value="formatDate(selectedDateRange[0])"
                  readonly
                  :disabled="selectedPreset !== 'custom' || isApplying"
                  @click="!isApplying && $refs.startPicker.showPicker()"
                />
                <input
                  type="date"
                  ref="startPicker"
                  v-model="selectedDateRange[0]"
                  :max="selectedDateRange[1] || today"
                  @change="onDateInputChange"
                  class="date-input-hidden"
                />
              </div>

              <span class="date-separator">-</span>

              <div class="date-field-wrapper">
                <input 
                  type="text"
                  class="date-input"
                  :value="formatDate(selectedDateRange[1])"
                  readonly
                  :disabled="selectedPreset !== 'custom' || isApplying"
                  @click="!isApplying && $refs.endPicker.showPicker()"
                />
                <input
                  type="date"
                  ref="endPicker"
                  v-model="selectedDateRange[1]"
                  :min="selectedDateRange[0] || undefined"
                  :max="today"
                  @change="onDateInputChange"
                  class="date-input-hidden"
                />
              </div>
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

    <div class="content-row" v-if="appliedCompanies.length > 0">
      <div class="chart-section" v-if="shops.length > 0">
        <div class="chart-header">
          <h2>Working hours of each shop</h2>
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
        :disabled="isApplying || shops.length === 0"
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
            <div v-for="(shop, index) in shops" :key="index" class="legend-item">
              <span class="legend-color" :style="{ backgroundColor: shop.color }"></span>
              <span class="legend-label">{{ shop.code }}</span>
            </div>
          </div>
        </div>
      </div>

      <div class="shop-table" v-if="tableData.length > 0">
        <div class="table-header-section">
          <h3><strong :style="{ color: 'var(--brand-berry-light)' }">Shop Details</strong></h3>
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

        <div class="shop-table-wrapper">
          <table>
            <thead>
              <tr>
                <th @click="handleHeaderSortTable('code')" class="sortable-header">
                  SHOP CODE
                  <FontAwesomeIcon 
                    v-if="sortColumnTable === 'code'" 
                    :icon="sortDirectionTable === 'asc' ? ['fas', 'sort-up'] : ['fas', 'sort-down']" 
                    class="sort-icon active" 
                  />
                  <FontAwesomeIcon 
                    v-else 
                    :icon="['fas', 'sort']" 
                    class="sort-icon placeholder" 
                  />
                </th>
                <th>TYPE</th>
                <th>COUNTRY</th>
                <th>ADDRESS</th>
                <th>EMAIL</th>
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
              <tr v-for="(row, index) in sortedTableData" :key="index">
                <td>{{ row.code }}</td>
                <td>{{ row.type }}</td>
                <td>{{ row.country }}</td>
                <td>{{ row.address }}</td>
                <td>{{ row.email }}</td>
                <td>{{ row.hours }}</td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>

    <div v-else-if="appliedCompanies.length === 0 && !isLoading" class="empty-state">
      <div class="empty-icon-circle">
        <svg width="48" height="48" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
          <rect x="3" y="3" width="18" height="18" rx="2"/>
          <path d="M9 3v18"/>
          <path d="M15 3v18"/>
          <path d="M3 9h18"/>
          <path d="M3 15h18"/>
        </svg>
      </div>
      <h3>Select companies to view reports</h3>
      <p>Choose one or more companies from the filter above</p>
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

const allCompanies = ref([])
const selectedCompanies = ref([])
const appliedCompanies = ref([])

const filterPanelOpen = ref(false)
const countryDropdownOpen = ref(false)
const companiesDropdownOpen = ref(false)

const countries = ref([])
const selectedCountries = ref([])
const appliedCountries = ref([])

const workingHoursChart = ref(null)
const chartInstance = ref(null)
const chartType = ref('pie')
const shops = ref([])
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

const sortColumnTable = ref('code')
const sortDirectionTable = ref('asc')
const isSettingPresetDates = ref(false)
const shopsRaw = ref([])

const formatDate = (iso) => {
  if (!iso) return ''
  const [y, m, d] = iso.split('-')
  return `${d}.${m}.${y}` 
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

const shopTypeMap = {
  1: 'Hyper',
  2: 'Super',
  3: 'Mini',
  4: 'Express'
}

const countryMap = {
  1: 'Lithuania',
  2: 'Latvia',
  3: 'Estonia',
}

const chartWrapperElement = ref(null)

const getShopTypeName = (typeId) => {
  return shopTypeMap[typeId] || 'Unknown'
}

const getCountryName = (countryId) => countryMap[Number(countryId)] || 'Unknown'

const sortedTableData = computed(() => {
  if (!tableData.value || tableData.value.length === 0) {
    return []
  }

  return [...tableData.value].sort((a, b) => {
    let valA, valB

    if (sortColumnTable.value === 'hours') {
      valA = parseFloat(a.hours.replace('h', '')) || 0
      valB = parseFloat(b.hours.replace('h', '')) || 0
    } else if (sortColumnTable.value === 'code') {
      valA = (a.code || '').toLowerCase()
      valB = (b.code || '').toLowerCase()
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

const filteredCompanies = computed(() => {
  let result = allCompanies.value
  if (selectedCountries.value.length > 0) {
    result = result.filter(company => selectedCountries.value.includes(company.country))
  }
  return result
})

const isAllFilteredSelected = computed(() => {
  return filteredCompanies.value.length > 0 && 
         selectedCompanies.value.length === filteredCompanies.value.length &&
         filteredCompanies.value.every(company => selectedCompanies.value.includes(company.companyID))
})

const hasDateChanges = computed(() => {
  return selectedDateRange.value[0] !== appliedDateRange.value[0] ||
         selectedDateRange.value[1] !== appliedDateRange.value[1]
})

const getAppliedFilterDescription = () => {
  const parts = []
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

const toggleCountryDropdown = () => {
  countryDropdownOpen.value = !countryDropdownOpen.value
  companiesDropdownOpen.value = false
}

const toggleCompaniesDropdown = () => {
  companiesDropdownOpen.value = !companiesDropdownOpen.value
  countryDropdownOpen.value = false
}

const toggleCountry = (country) => {
  const index = selectedCountries.value.indexOf(country)
  if (index > -1) {
    selectedCountries.value.splice(index, 1)
  } else {
    selectedCountries.value.push(country)
  }
  updateCompaniesAfterFilter()
}

const toggleAllCountries = () => {
  if (selectedCountries.value.length === countries.value.length) {
    selectedCountries.value = []
  } else {
    selectedCountries.value = [...countries.value]
  }
  updateCompaniesAfterFilter()
}

const updateCompaniesAfterFilter = () => {
  const validCompanyIds = filteredCompanies.value.map(company => company.companyID)
  selectedCompanies.value = selectedCompanies.value.filter(id => validCompanyIds.includes(id))
}

const toggleSelectAll = () => {
  if (isAllFilteredSelected.value) {
    selectedCompanies.value = []
  } else {
    selectedCompanies.value = filteredCompanies.value.map(company => company.companyID)
  }
}

const toggleCompany = (companyId) => {
  const index = selectedCompanies.value.indexOf(companyId)
  if (index > -1) {
    selectedCompanies.value.splice(index, 1)
  } else {
    selectedCompanies.value.push(companyId)
  }
}

const applyFilters = async () => {
  if (selectedCompanies.value.length === 0) {
    showWarningMessage('Please select at least one company to view reports')
    return
  }

  isApplying.value = true
  countryDropdownOpen.value = false
  companiesDropdownOpen.value = false
  filterPanelOpen.value = false

  appliedCompanies.value = [...selectedCompanies.value]
  appliedCountries.value = [...selectedCountries.value]

  saveStatePending()
  saveState()

  clearData()
  
  await fetchReportData(appliedCompanies.value, appliedCountries.value)

  isApplying.value = false
}

const clearFilters = () => {
  selectedCountries.value = []
  selectedCompanies.value = []
  countryDropdownOpen.value = false
  companiesDropdownOpen.value = false
  filterPanelOpen.value = false
  clearData()
  appliedCompanies.value = []
  appliedCountries.value = []
  saveState()
}

const clearFiltersKeepPanel = () => {
  selectedCountries.value = []
  selectedCompanies.value = filteredCompanies.value.map(company => company.companyID)
}

const clearData = () => {
  tableData.value = []
  shops.value = []
  if (chartInstance.value) {
    chartInstance.value.destroy()
    chartInstance.value = null
  }
}

const onPresetChange = () => {
  isSettingPresetDates.value = true; 

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

  if (appliedCompanies.value.length > 0) {
    clearData()
    await fetchReportData(appliedCompanies.value, appliedCountries.value)
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
  localStorage.setItem('companyReportStatePending', JSON.stringify({
    selectedCompanies: selectedCompanies.value,
    selectedCountries: selectedCountries.value
  }))
}

const saveState = () => {
  localStorage.setItem('companyReportState', JSON.stringify({
    appliedCompanies: appliedCompanies.value,
    appliedCountries: appliedCountries.value,
    selectedPreset: appliedPreset.value,
    selectedDateRange: appliedDateRange.value,
    chartType: chartType.value
  }))
}

const loadState = () => {
  const saved = localStorage.getItem('companyReportState')
  if (saved) {
    const state = JSON.parse(saved)
    appliedCompanies.value = state.appliedCompanies || []
    appliedCountries.value = state.appliedCountries || []
    appliedPreset.value = state.selectedPreset || 'week'
    chartType.value = state.chartType || 'pie'
    selectedCompanies.value = [...appliedCompanies.value]
    selectedCountries.value = [...appliedCountries.value]
    selectedPreset.value = appliedPreset.value

    if (appliedPreset.value !== 'custom') {
      const endDate = new Date()
      const startDate = new Date()
      switch (appliedPreset.value) {
        case 'week':    startDate.setDate(endDate.getDate() - 7);          break
        case 'month':   startDate.setMonth(endDate.getMonth() - 1);        break
        case '3months': startDate.setMonth(endDate.getMonth() - 3);        break
        case '6months': startDate.setMonth(endDate.getMonth() - 6);        break
        case 'year':    startDate.setFullYear(endDate.getFullYear() - 1);  break
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
  return false
}

const fetchCompanies = async () => {
  try {
    const res = await fetch(`${API_BASE}/companies`)
    if (!res.ok) throw new Error(`HTTP error! status: ${res.status}`)

    let companies = await res.json()

    if (roleLevel === 1) {
      companies = companies.filter(c => c.remID === currentAdmin.remId)
    }

    allCompanies.value = companies

    const uniqueCountries = [...new Set(allCompanies.value.map(company => Number(company.country)))].filter(Boolean)
    countries.value = uniqueCountries.sort((a, b) => a - b)
  } catch (error) {
    console.error('Failed to load companies:', error)
    showWarningMessage('Failed to load companies. Please refresh the page.')
  }
}

const fetchReportData = async (companyIds, countriesFilter) => {
  if (companyIds.length === 0) {
    clearData()
    return
  }
  if (!isDateRangeValid()) {
    return
  }

  isLoading.value = true

  try {
    const allShiftsPromises = companyIds.map(companyId => 
      fetch(`${API_BASE}/shifts/bycompany/${companyId}`).then(res => res.json())
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

    const shopStats = {}

    for (const shift of shiftsInRange) {
      const shop = shift.shop
      
      if (!shop) {
        continue
      }
      
      const shopId = shop.shopID
      
      if (!shopStats[shopId]) {
        shopStats[shopId] = {
          shopId: shopId,
          code: shop.code || shop.shopCode || 'Unknown',
          type: shop.type || 'N/A',
          country: shop.country || 'N/A',
          address: shop.address || 'N/A',
          email: shop.email || 'N/A',
          totalHours: 0,
          shifts: []
        }
      }
      
      const hours = calculateShiftHours(shift)
      shopStats[shopId].totalHours += hours
      shopStats[shopId].shifts.push(shift)
    }

    const shopsArray = Object.values(shopStats).filter(shop => shop.totalHours >= 0.1)
    shopsRaw.value = shopsArray
    
    if (shopsArray.length === 0) {
      showWarningMessage('No shop data available for selected companies')
      clearData()
      isLoading.value = false
      return
    }

    const totalHours = shopsArray.reduce((sum, shop) => sum + shop.totalHours, 0)

    tableData.value = shopsArray.map(shop => ({
      code: shop.code,
      type: getShopTypeName(shop.type),
      country: getCountryName(shop.country),
      address: shop.address,
      email: shop.email,
      hours: `${shop.totalHours.toFixed(1)}h`
    }))

    shops.value = shopsArray.map((shop, index) => ({
      code: shop.code,
      percentage: totalHours > 0 ? ((shop.totalHours / totalHours) * 100).toFixed(1) : 0,
      hours: shop.totalHours,
      color: getShopColor(index)
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
    shops.value = []
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
    console.error('Error calculating shift hours:', error)
    return 0
  }
}

const getShopColor = (index) => {
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
  if (isUpdatingChart.value || !workingHoursChart.value || shops.value.length === 0) {
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
    return
  }
  const canvas = workingHoursChart.value
  if (!canvas.getContext) {
    return
  }
  
  if (chartType.value === 'pie') {
    chartInstance.value = new Chart(canvas, {
      type: 'pie',
      data: {
        labels: shops.value.map(s => s.code),
        datasets: [{
          data: shops.value.map(s => s.percentage),
          backgroundColor: shops.value.map(s => s.color),
          borderColor: '#ffffff',
          borderWidth: 2
        }]
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
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
        labels: shops.value.map(s => s.code),
        datasets: [{
          label: 'Working Hours',
          data: shops.value.map(s => s.hours),
          backgroundColor: shops.value.map(s => s.color),
          borderColor: shops.value.map(s => s.color),
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
        labels: shops.value.map(s => s.code),
        datasets: [{
          label: 'Working Hours',
          data: shops.value.map(s => s.hours),
          backgroundColor: 'rgba(161, 41, 113, 0.1)',
          borderColor: '#a12971',
          borderWidth: 2,
          pointBackgroundColor: shops.value.map(s => s.color),
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
    link.download = `shop-working-hours-chart-${startDate}_to_${endDate}.png`
    
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

  const worksheet = workbook.addWorksheet('Shop Details')

  worksheet.columns = [
    { header: 'Shop Code', key: 'code', width: 15 },
    { header: 'Type', key: 'type', width: 15 },
    { header: 'Country', key: 'country', width: 15 },
    { header: 'Address', key: 'address', width: 40 },
    { header: 'Email', key: 'email', width: 30 },
    { header: 'Working Hours', key: 'hours', width: 15 }
  ]

  sortedTableData.value.forEach((row) => {
    worksheet.addRow({
      code: row.code,
      type: row.type,
      country: row.country,
      address: row.address,
      email: row.email,
      hours: row.hours
    })
  })

  const lastRow = sortedTableData.value.length + 1
  worksheet.addTable({
    name: 'ShopTable',
    ref: `A1:F${lastRow}`,
    headerRow: true,
    totalsRow: false,
    style: { theme: 'TableStyleMedium2', showRowStripes: true },
    columns: [
      { name: 'Shop Code',     filterButton: true },
      { name: 'Type',          filterButton: true },
      { name: 'Country',       filterButton: true },
      { name: 'Address',       filterButton: true },
      { name: 'Email',         filterButton: true },
      { name: 'Working Hours', filterButton: true }
    ],
    rows: sortedTableData.value.map(row => [
      row.code, row.type, row.country, row.address, row.email, row.hours
    ])
  })

  for (let i = 1; i <= lastRow; i++) {
    ['A', 'B', 'C', 'D', 'E', 'F'].forEach(col => {
      worksheet.getCell(`${col}${i}`).alignment = { horizontal: 'center', vertical: 'middle' }
    })
  }

  for (const shop of shopsRaw.value) {
    const sheetName = (shop.code || `Shop_${shop.shopId}`)
      .replace(/[\\/*?:\[\]]/g, '')
      .substring(0, 31)

    const shopSheet = workbook.addWorksheet(sheetName)

    shopSheet.columns = [
      { header: 'Start Date',  key: 'startDate',  width: 15 },
      { header: 'Start Time',  key: 'startTime',  width: 12 },
      { header: 'End Date',    key: 'endDate',    width: 15 },
      { header: 'End Time',    key: 'endTime',    width: 12 },
      { header: 'Reason',      key: 'reason',     width: 40 },
      { header: 'Work Hours',  key: 'workHours',  width: 12 }
    ]

    shopSheet.getRow(1).eachCell(cell => {
      cell.font = { bold: true }
      cell.alignment = { horizontal: 'center', vertical: 'middle' }
      cell.fill = {
        type: 'pattern',
        pattern: 'solid',
        fgColor: { argb: 'FFD9E1F2' }
      }
    })

    const shiftRows = []

    shop.shifts.forEach(shift => {
      const hours = calculateShiftHours(shift)
      const reason = shift.companyReason?.reason?.name ?? '—'

      shopSheet.addRow({
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

    const shiftLastRow = shop.shifts.length + 1

    shopSheet.addTable({
      name: `Shifts_${shop.shopId}`,
      ref: `A1:F${shiftLastRow}`,
      headerRow: true,
      totalsRow: true,
      style: { theme: 'TableStyleMedium4', showRowStripes: true },
      columns: [
        { name: 'Start Date',  filterButton: true, totalsRowLabel: '' },
        { name: 'Start Time',  filterButton: true, totalsRowLabel: '' },
        { name: 'End Date',    filterButton: true, totalsRowLabel: '' },
        { name: 'End Time',    filterButton: true, totalsRowLabel: '' },
        { name: 'Reason',      filterButton: true, totalsRowLabel: 'Total' },
        { name: 'Work Hours',  filterButton: true, totalsRowFunction: 'sum' }
      ],
      rows: shiftRows
    })

    for (let i = 1; i <= shiftLastRow + 1; i++) {
      ['A', 'B', 'C', 'D', 'E', 'F'].forEach(col => {
        shopSheet.getCell(`${col}${i}`).alignment = { horizontal: 'center', vertical: 'middle' }
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
  link.download = `shop-details-${startDate}_to_${endDate}.xlsx`

  document.body.appendChild(link)
  link.click()
  document.body.removeChild(link)
  window.URL.revokeObjectURL(url)
}


onMounted(async () => {
  await fetchCompanies()
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
    if (allCompanies.value.length > 0) {
      selectedCompanies.value = allCompanies.value.map(company => company.companyID)
      appliedCompanies.value = [...selectedCompanies.value]
    }
    saveState()
    if (appliedCompanies.value.length > 0) {
      await fetchReportData(appliedCompanies.value, appliedCountries.value)
    }
  } else {
    const validCompanyIds = allCompanies.value.map(c => c.companyID)
    appliedCompanies.value = appliedCompanies.value.filter(id => validCompanyIds.includes(id))
    selectedCompanies.value = [...appliedCompanies.value]
    await nextTick()
    if (appliedCompanies.value.length > 0 && isDateRangeValid()) {
      await fetchReportData(appliedCompanies.value, appliedCountries.value)
    }
  }

  document.addEventListener('click', (e) => {
    if (!e.target.closest('.filter-panel')) {
      filterPanelOpen.value = false
      countryDropdownOpen.value = false
      companiesDropdownOpen.value = false
    }
  })
})
</script>

<style scoped>
.company-statistics-container {
  font-family: 'Inter', sans-serif;
  /* margin-left: 80px; */
}

.filter-count {
  background: var(--brand-berry);
  color: var(--color-white);
  padding: 2px 8px;
  border-radius: 12px;
  font-size: 12px;
  font-weight: 500;
}

.submenu-divider {
  height: 1px;
  background: var(--color-border);
  margin: 4px 0;
}

.warning-banner {
  position: fixed;
  top: 20px;
  right: 20px;
  z-index: 1000;
  animation: slideIn 0.3s ease-out;
}

@keyframes slideIn {
  from {
    transform: translateX(400px);
    opacity: 0;
  }
  to {
    transform: translateX(0);
    opacity: 1;
  }
}

.warning-content {
  background: var(--color-warning-bg);
  border: 1px solid var(--color-warning-border);
  border-radius: 8px;
  padding: 16px 20px;
  display: flex;
  align-items: center;
  gap: 12px;
  box-shadow: 0 4px 16px rgba(0, 0, 0, 0.12);
  min-width: 300px;
  max-width: 500px;
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
  font-size: 24px;
  color: var(--color-warning-text);
  cursor: pointer;
  padding: 0;
  margin-left: auto;
  flex-shrink: 0;
}

.warning-close:hover {
  color: var(--color-black);
}

.controls-row {
  display: grid;
  grid-template-columns: auto 1fr;
  gap: 10px;
  margin-bottom: 30px;
}

.content-row {
  display: grid;
  grid-template-columns: 1fr auto;
  gap: 10px;
}

.company-selector {
  display: flex;
  flex-direction: column;
}

.filter-panel {
  position: relative;
  width: 660px;
  border-radius: 20px;
  background: rgba(255, 255, 255, 0.82);
  backdrop-filter: blur(16px);
  -webkit-backdrop-filter: blur(16px);
  border: 1.5px solid rgba(255, 255, 255, 0.9);
  box-shadow: 0 8px 32px rgba(161, 41, 113, 0.08), 0 2px 8px rgba(0, 0, 0, 0.06);
  /* padding: 15px; */
  overflow: hidden;
}

.filter-content {
  border-top: none;
}

.filter-section-header {
  padding: 12px 16px;
  font-size: 12px;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.5px;
  color: var(--color-text-dim);
  background: rgba(161, 41, 113, 0.05);
}

.companies-count {
  color: var(--brand-teal);
  text-transform: none;
  font-weight: 500;
}

.filter-item {
  padding: 12px 16px;
  display: flex;
  justify-content: space-between;
  align-items: center;
  cursor: pointer;
  border-bottom: 1px solid rgba(161, 41, 113, 0.07);
  transition: background 0.15s;
}

.filter-item:hover {
  background: rgba(161, 41, 113, 0.04);
}

.filter-item input[type="checkbox"],
.submenu-item input[type="checkbox"] {
  -webkit-appearance: none;
  appearance: none;
  width: 18px;
  height: 18px;
  border: 1.5px solid var(--color-checkbox-border);
  border-radius: 4px;
  background-color: var(--color-white);
  cursor: pointer;
  position: relative;
  transition: all 0.2s ease;
  flex-shrink: 0;
}

.filter-item input[type="checkbox"]:hover,
.submenu-item input[type="checkbox"]:hover {
  border-color: var(--brand-teal);
  background-color: rgba(255, 255, 255, 0.85);
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
  left: 5px;
  top: 2px;
  width: 4px;
  height: 8px;
  border: solid var(--color-white);
  border-width: 0 2px 2px 0;
  transform: rotate(45deg);
}

.filter-item input[type="checkbox"]:checked::after,
.submenu-item input[type="checkbox"]:checked::after {
  display: block;
}

.filter-item-content {
  display: flex;
  align-items: center;
  gap: 8px;
  flex: 1;
}

.arrow {
  color: var(--color-text-light);
  font-size: 18px;
  transition: transform 0.2s;
}

.arrow.rotated {
  transform: rotate(90deg);
}

.submenu {
  background: rgba(248, 249, 250, 0.9);
  border-left: 3px solid var(--brand-berry);
}

.submenu-item {
  padding: 10px 24px;
  display: flex;
  align-items: center;
  gap: 10px;
  cursor: pointer;
  border-bottom: 1px solid rgba(161, 41, 113, 0.06);
  font-size: 13px;
  transition: background 0.15s;
}

.submenu-item:hover {
  background: rgba(161, 41, 113, 0.05);
}

.submenu-item.active {
  background: rgba(161, 41, 113, 0.07);
  font-weight: 500;
}

.submenu-item.selected {
  background: var(--row-hover-gradient);
}

.submenu-item input {
  cursor: pointer;
}

.companies-list {
  max-height: 300px;
  overflow-y: auto;
}

.company-item {
  padding: 8px 24px;
}

.company-info-item {
  display: flex;
  align-items: flex-start;
  gap: 10px;
  width: 100%;
}

.company-details {
  flex: 1;
}

.company-name {
  font-weight: 600;
  color: var(--brand-berry);
  font-size: 14px;
}

.company-address {
  font-size: 12px;
  color: var(--color-text-dim);
  margin-top: 2px;
}

.no-companies {
  padding: 30px 20px;
  text-align: center;
  color: var(--color-text-light);
}

.no-companies p {
  margin: 0 0 15px 0;
}

.btn-reset-small {
  padding: 6px 16px;
  background: var(--brand-berry);
  color: var(--color-white);
  border: none;
  border-radius: 20px;
  cursor: pointer;
  font-size: 13px;
}

.btn-reset-small:hover {
  background: var(--brand-berry-light);
}

.filter-divider {
  height: 1px;
  background: var(--color-border);
  margin: 8px 0;
}

.filter-actions {
  display: flex;
  gap: 10px;
  padding: 12px 16px;
  background: rgba(255, 255, 255, 0.5);
}

.btn-clear,
.btn-apply {
  flex: 1;
  padding: 10px;
  border-radius: 20px;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.2s;
}

.btn-clear {
  background: var(--color-white);
  border: 1.5px solid var(--brand-berry);
  color: var(--brand-berry);
}

.btn-clear:hover:not(:disabled) {
  background: rgba(161, 41, 113, 0.06);
}

.btn-clear:disabled {
  opacity: 0.4;
  cursor: not-allowed;
}

.btn-apply {
  background: var(--brand-berry);
  border: none;
  color: var(--color-white);
}

.btn-apply:hover:not(:disabled) {
  background: var(--brand-berry-light);
}

.btn-apply:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.time-period-card {
  background: none;
  border-radius: 12px;
  width: 100%;
  display: flex;
  flex-direction: column;
  justify-content: space-between;
  /* margin: 0px 50px; */
}

.company-info {
  margin-top: auto;
  text-align: right;
  border-radius: 20px;
  background: rgba(255, 255, 255, 0.82);
  backdrop-filter: blur(16px);
  -webkit-backdrop-filter: blur(16px);
  border: 1.5px solid rgba(255, 255, 255, 0.9);
  box-shadow: 0 8px 32px rgba(161, 41, 113, 0.08);
  padding: 30px;
  margin-top: 10px;
}

.company-info h3 {
  font-size: 18px;
  font-weight: 600;
  margin: 0 0 5px 0;
  color: var(--brand-teal);
}

.company-info .company-address {
  font-size: 14px;
  color: var(--color-text-dim);
  margin: 0;
}

.time-period-section {
  display: flex;
  flex-direction: column;
  gap: 16px;
  background: rgba(255, 255, 255, 0.82);
  backdrop-filter: blur(16px);
  -webkit-backdrop-filter: blur(16px);
  border: 1.5px solid rgba(255, 255, 255, 0.9);
  border-radius: 20px;
  padding: 20px;
  box-shadow: 0 8px 32px rgba(161, 41, 113, 0.08), 0 2px 8px rgba(0, 0, 0, 0.06);
}

.time-period {
  background: transparent;
  display: flex;
  flex-direction: column;
  gap: 15px;
  border-radius: 0;
  padding: 0;
  box-shadow: none;
}

.time-period label {
  font-size: 16px;
  font-weight: bold;
  color: var(--brand-berry);
}

.preset-select {
  padding: 10px 15px;
  border: 1.5px solid rgba(161, 41, 113, 0.2);
  border-radius: 20px;
  font-size: 14px;
  font-family: 'Inter', sans-serif;
  background: rgba(255, 255, 255, 0.85);
  cursor: pointer;
  width: 100%;
  transition: border-color 0.2s;
  outline: none;
}

.preset-select:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.preset-select:focus,
.preset-select:focus-visible {
  outline: none;
  border-color: var(--brand-berry);
  box-shadow: none;
}

.date-inputs {
  display: flex;
  align-items: center;
  gap: 10px;
}

.date-input {
  padding: 10px 12px;
  border: 1.5px solid rgba(161, 41, 113, 0.2);
  border-radius: 20px;
  font-size: 14px;
  font-family: 'Inter', sans-serif;
  flex: 1;
  transition: border-color 0.2s;
  background: rgba(255, 255, 255, 0.85);
  outline: none;
}

.date-input:hover:not(:disabled) {
  border-color: var(--brand-berry);
}

.date-input:focus {
  outline: none;
  border-color: var(--brand-teal);
  box-shadow: none;
}

.date-input:disabled {
  background-color: rgba(255, 255, 255, 0.5);
  cursor: not-allowed;
  opacity: 0.6;
  border-color: rgba(161, 41, 113, 0.15);
}

.date-separator {
  color: var(--color-text-light);
  font-weight: bold;
}

:deep(input.date-input) {
  padding: 10px 12px;
  border: 1.5px solid rgba(161, 41, 113, 0.2);
  border-radius: 20px;
  font-size: 14px;
  font-family: Inter, sans-serif;
  width: 100%;
  background: rgba(255, 255, 255, 0.85);
  transition: border-color 0.2s;
  outline: none;
  box-sizing: border-box;
}

:deep(input.date-input:focus) {
  outline: none;
  border-color: var(--brand-teal);
  box-shadow: none;
}

.date-field-wrapper {
  position: relative; 
  flex: 1; 
  min-width: 0; 
}

.date-input-hidden { 
  position: absolute;
  opacity: 0;
  pointer-events: none;
  width: 0;
  height: 0;
  top: 0;
  left: 0;
} 

.btn-apply-dates {
  background: var(--brand-berry);
  color: white;
  border: none;
  border-radius: 20px;
  padding: 12px 24px;
  font-size: 14px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.2s ease;
  min-width: 100px;
  height: 44px;
  display: flex;
  align-items: center;
  justify-content: center;
  box-shadow: 0 2px 8px rgba(161, 41, 113, 0.25);
  align-self: flex-end;
}

.btn-apply-dates:hover:not(:disabled) {
  background: var(--brand-berry-light);
  transform: translateY(-1px);
}

.btn-apply-dates:active:not(:disabled) {
  transform: translateY(1px);
  box-shadow: 0 1px 4px rgba(161, 41, 113, 0.2);
}

.btn-apply-dates:disabled {
  opacity: 0.5;
  cursor: not-allowed;
  transform: none;
}

.btn-spinner {
  width: 16px;
  height: 16px;
  border: 2px solid rgba(255, 255, 255, 0.4);
  border-top-color: var(--color-white);
  border-radius: 50%;
  display: inline-block;
  animation: btn-spin 0.7s linear infinite;
}

@keyframes btn-spin {
  to {
    transform: rotate(360deg);
  }
}

.chart-section {
  background: rgba(255, 255, 255, 0.82);
  backdrop-filter: blur(16px);
  -webkit-backdrop-filter: blur(16px);
  border: 1.5px solid rgba(255, 255, 255, 0.9);
  padding: 30px;
  border-radius: 12px;
  box-shadow: 0 8px 32px rgba(161, 41, 113, 0.08), 0 2px 8px rgba(0, 0, 0, 0.06);
  width: 600px;
}

.chart-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 20px;
  flex-wrap: wrap;
  gap: 16px;
}

.chart-header h2 {
  font-size: 18px;
  font-weight: 600;
  margin: 0;
  color: var(--color-text-main);
}

.chart-type-switcher {
  display: flex;
  gap: 4px;
  background: rgba(161, 41, 113, 0.07);
  padding: 4px;
  border-radius: 8px;
}

.chart-type-btn {
  padding: 8px 12px;
  background: transparent;
  border: none;
  border-radius: 6px;
  cursor: pointer;
  color: var(--color-text-dim);
  transition: all 0.2s;
  display: flex;
  align-items: center;
  justify-content: center;
}

.chart-type-btn:hover:not(:disabled) {
  background: rgba(255, 255, 255, 0.7);
  color: var(--brand-berry);
}

.chart-type-btn.active {
  background: var(--brand-berry);
  color: var(--color-white);
}

.chart-type-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.chart-type-btn svg {
  display: block;
}

.chart-wrapper {
  display: flex;
  flex-direction: column;
  gap: 20px;
  align-items: center;
}

.chart-container {
  width: 100%;
  height: 400px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.chart-wrapper canvas {
  max-width: 100%;
  max-height: 100%;
}

.chart-legend {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 10px 20px;
  width: 100%;
}

.legend-item {
  display: flex;
  align-items: center;
  gap: 8px;
}

.legend-color {
  width: 12px;
  height: 12px;
  border-radius: 50%;
  flex-shrink: 0;
}

.legend-label {
  font-size: 13px;
  color: var(--color-text-main);
  line-height: 1.2;
}

.shop-table {
  background: rgba(255, 255, 255, 0.82);
  backdrop-filter: blur(16px);
  -webkit-backdrop-filter: blur(16px);
  border: 1.5px solid rgba(255, 255, 255, 0.9);
  padding: 20px;
  border-radius: 12px;
  box-shadow: 0 8px 32px rgba(161, 41, 113, 0.08), 0 2px 8px rgba(0, 0, 0, 0.06);
}

.table-header-section {
  flex-shrink: 0;
  margin-bottom: 20px;
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 12px;
}

.table-header-section h3 {
  font-size: 24px;
  width: 100%;
  font-weight: bolder;
  margin: 0;
  color: var(--brand-berry-light);
  line-height: 1;
}

.shop-table-wrapper {
  overflow-y: auto;
  overflow-x: hidden;
  max-height: 600px;
  border-radius: 12px;
  position: relative;
  border: 1.5px solid rgba(161, 41, 113, 0.12);
  box-shadow: none;
  -ms-overflow-style: none;
}

.shop-table-wrapper::-webkit-scrollbar {
  width: 8px;
}

.shop-table-wrapper::-webkit-scrollbar-track {
  background: var(--color-bg-light);
  border-radius: 10px;
}

.shop-table-wrapper::-webkit-scrollbar-thumb {
  background: var(--brand-berry);
  border-radius: 10px;
  transition: background 0.2s ease;
}

.shop-table-wrapper::-webkit-scrollbar-thumb:hover {
  background: var(--brand-berry-light);
}

.shop-table table {
  width: 100%;
  border-collapse: collapse;
  background: var(--color-white);
  font-size: 13px;
  table-layout: auto;
}

.shop-table thead {
  position: sticky;
  top: 0;
  z-index: 10;
  background: var(--header-gradient);
}

.shop-table thead tr th {
  text-align: center;
  padding: 12px 16px;
  font-size: 11px;
  font-weight: 600;
  color: var(--color-white);
  text-transform: uppercase;
  letter-spacing: 0.6px;
  border: none;
  background: transparent;
  vertical-align: middle;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.shop-table tbody tr {
  border-bottom: 1px solid rgba(161, 41, 113, 0.07);
  transition: background 0.15s;
}

.shop-table tbody tr:hover {
  background: rgba(161, 41, 113, 0.04);
}

.shop-table tbody tr:last-child {
  border-bottom: none;
}

.shop-table tbody tr td {
  padding: 12px 16px;
  font-size: 14px;
  color: var(--color-text-main);
  font-weight: 500;
  text-align: center;
  border-bottom: none;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.shop-table tbody tr td:first-child,
.shop-table tbody tr td:last-child {
  font-weight: 600;
  color: var(--color-black);
}

.sortable-header {
  cursor: pointer;
  user-select: none;
  position: relative;
  transition: opacity 0.2s;
  white-space: nowrap;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 6px;
}

.sortable-header:hover {
  opacity: 0.85;
}

.empty-state {
  background: rgba(255, 255, 255, 0.75);
  backdrop-filter: blur(12px);
  -webkit-backdrop-filter: blur(12px);
  border: 1.5px solid rgba(255, 255, 255, 0.9);
  padding: 60px 40px;
  border-radius: 12px;
  text-align: center;
  margin-top: 30px;
  box-shadow: 0 8px 32px rgba(0, 0, 0, 0.06);
}

.empty-icon-circle {
  width: 80px;
  height: 80px;
  background: rgba(255, 255, 255, 0.9);
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  margin: 0 auto 20px;
  color: var(--brand-berry);
  box-shadow: 0 4px 12px rgba(161, 41, 113, 0.1);
}

.empty-state h3 {
  font-size: 20px;
  font-weight: 600;
  margin: 0 0 10px 0;
  color: var(--color-text-main);
}

.empty-state p {
  font-size: 14px;
  color: var(--color-text-dim);
  margin: 0;
}

.export-btn {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 10px 12px;
  background: rgba(255, 255, 255, 0.8);
  color: var(--brand-teal);
  border: 1.5px solid rgba(43, 164, 146, 0.25);
  border-radius: 8px;
  font-size: 14px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.2s ease;
  box-shadow: none;
  flex-shrink: 0;
  white-space: nowrap;
}

.export-btn:hover:not(:disabled) {
  transform: translateY(-1px);
  box-shadow: 0 4px 10px rgba(43, 164, 146, 0.2);
}

.export-btn:disabled {
  opacity: 0.45;
  cursor: not-allowed;
  transform: none;
}

.export-btn svg {
  flex-shrink: 0;
}

.chart-controls {
  display: flex;
  align-items: center;
  gap: 12px;
}

@media (max-width: 1400px) {
  .controls-row {
    grid-template-columns: minmax(0, 0.55fr) minmax(0, 1fr); /* adjust ratio to taste */
    gap: 10px;
  }

  /* Remove fixed widths — let elements fill their column */
  .filter-panel { width: 100%; margin: 0; }
  .time-period-card { width: 100%; margin: 0; } /* ← drop the 795px */

  .chart-legend { grid-template-columns: repeat(3, 1fr); }
  .shop-table { padding: 10px 24px; }
  .shop-table tbody tr td { font-size: 13px; padding: 10px 10px; }
  .shop-table thead tr th { font-size: 10px; padding: 10px 10px; }
  .chart-section { width: 400px; padding: 20px; }
  .chart-container { height: 340px; }
  .time-period-section { padding: 16px; }
  .company-info { padding: 20px; }
}

@media (max-width: 1200px) {
  .controls-row { grid-template-columns: 1fr 1fr; gap: 16px; }

  .shop-table { padding: 10px 16px; }
  .shop-table tbody tr td { font-size: 12px; padding: 9px 8px; }
  .shop-table thead tr th { font-size: 10px; padding: 9px 8px; }
  .table-header-section h3 { font-size: 20px; }

  .chart-container { height: 300px; }
  .chart-header h2 { font-size: 15px; }
  .chart-legend { grid-template-columns: repeat(2, 1fr); gap: 8px 14px; }
  .legend-label { font-size: 12px; }

  .filter-item { padding: 10px 12px; }
  .filter-section-header { padding: 8px 12px; font-size: 11px; }
  .submenu-item { padding: 8px 16px; font-size: 13px; }
  .filter-actions { padding: 10px 12px; }
  .btn-clear, .btn-apply { padding: 8px; font-size: 13px; }

  .time-period label { font-size: 14px; }
  .preset-select { font-size: 13px; padding: 9px 12px; }
  .date-input { font-size: 13px; padding: 9px 10px; }
  :deep(input.date-input) { font-size: 13px; padding: 9px 10px; }
  .btn-apply-dates { height: 40px; padding: 10px 16px; font-size: 13px; }

  .company-info h3 { font-size: 15px; }
  .company-info .company-address { font-size: 13px; }

  .export-btn { font-size: 13px; padding: 8px 10px; }
}

@media (max-width: 1100px) {
  .content-row { grid-template-columns: 1fr; }
  .chart-section { width: 100%; }
  .shop-table { width: 100%; }
}

@media (max-width: 900px) {
  .chart-container { height: 300px; }
  .chart-legend { grid-template-columns: repeat(2, 1fr); }
  .filter-panel { border-radius: 16px; }
  .time-period-section { padding: 16px; }
  .company-info { padding: 20px; }
}

@media (max-width: 768px) {
  .controls-row { grid-template-columns: 1fr; gap: 16px; }

  .chart-container { height: 260px; }
  .chart-legend { grid-template-columns: 1fr 1fr; gap: 6px 12px; }

  .chart-header h2 { font-size: 14px; }
  .chart-type-btn { padding: 6px 8px; }
  .chart-type-btn svg { width: 16px; height: 16px; }

  .shop-table { padding: 16px; }
  .table-header-section h3 { font-size: 18px; }
  .shop-table tbody tr td { font-size: 13px; padding: 10px 12px; }
  .shop-table thead tr th { padding: 10px 12px; }

  .filter-item { padding: 10px 12px; }
  .submenu-item { padding: 8px 16px; font-size: 12px; }
  .filter-section-header { padding: 8px 12px; font-size: 11px; }
  .filter-actions { flex-direction: row; padding: 10px 12px; }

  .time-period label { font-size: 14px; }
  .preset-select { font-size: 13px; padding: 9px 12px; }
  .date-input { font-size: 13px; padding: 9px 10px; }
  :deep(input.date-input) { font-size: 13px; padding: 9px 10px; }

  .btn-apply-dates { height: 40px; padding: 10px 18px; font-size: 13px; }
  .btn-clear, .btn-apply { padding: 8px; font-size: 13px; }

  .warning-content { min-width: 220px; }
  .company-info h3 { font-size: 15px; }
}

@media (max-width: 576px) {
  .chart-container { height: 220px; }
  .chart-legend { grid-template-columns: 1fr; gap: 6px; }
  .legend-label { font-size: 11px; }
  .legend-color { width: 10px; height: 10px; }

  .chart-section { padding: 16px; }
  .chart-header h2 { font-size: 13px; }

  .shop-table { padding: 12px; }
  .table-header-section h3 { font-size: 16px; }
  .shop-table tbody tr td { font-size: 12px; padding: 8px 10px; }
  .shop-table thead tr th { font-size: 10px; padding: 8px 10px; }
  .shop-table tbody tr td:nth-child(2) { display: none; }

  .filter-item { padding: 9px 10px; font-size: 13px; }
  .filter-item-content span { font-size: 13px; }
  .filter-count { font-size: 11px; padding: 2px 6px; }
  .companies-count { font-size: 11px; }
  .submenu-item { padding: 7px 12px; font-size: 12px; }
  .company-name { font-size: 12px; }
  .company-address { font-size: 11px; }

  .time-period label { font-size: 13px; }
  .preset-select, .date-input { font-size: 12px; padding: 8px 10px; }
  :deep(input.date-input) { font-size: 12px; padding: 8px 10px; }
  .btn-apply-dates { height: 38px; padding: 8px 14px; font-size: 12px; }
  .btn-clear, .btn-apply { padding: 7px; font-size: 12px; }

  .date-inputs { flex-direction: column; gap: 8px; }
  .date-separator { display: none; }

  .time-period-section { padding: 14px; gap: 10px; }
  .company-info { padding: 16px; }
  .company-info h3 { font-size: 14px; }

  .empty-state { padding: 40px 20px; }
  .empty-icon-circle { width: 56px; height: 56px; }
  .empty-state h3 { font-size: 16px; }
  .empty-state p { font-size: 13px; }

  .export-btn { font-size: 12px; padding: 7px 10px; }
}

@media (max-width: 480px) {
  .controls-row { gap: 12px; }
  .chart-container { height: 200px; }
  .filter-panel { border-radius: 14px; padding: 10px; }
  .company-info { padding: 14px; }
  .time-period-section { padding: 12px; gap: 8px; }

  .shop-table { padding: 10px; }
  .table-header-section h3 { font-size: 15px; }
  .shop-table tbody tr td { font-size: 11px; padding: 7px 8px; }
  .shop-table thead tr th { font-size: 10px; padding: 7px 8px; }

  .filter-item { padding: 8px 10px; }
  .submenu-item { padding: 6px 10px; font-size: 11px; }
  .filter-section-header { font-size: 10px; padding: 6px 10px; }

  .preset-select, .date-input { font-size: 12px; padding: 7px 9px; }
  :deep(input.date-input) { font-size: 12px; padding: 7px 9px; }
  .btn-apply-dates { height: 36px; font-size: 12px; padding: 7px 12px; }

  .empty-icon-circle { width: 48px; height: 48px; }
  .empty-state h3 { font-size: 15px; }
  .export-btn { font-size: 11px; padding: 6px 9px; }
}
</style>
<template>
  <div class="single-company-statistics-container">
    <div class="blob-pink"></div>

    <div v-if="showWarning" class="warning-banner">
      <div class="warning-content">
        <span class="warning-icon">!</span>
        <span>{{ warningMessage }}</span>
        <button class="warning-close" @click="showWarning = false">×</button>
      </div>
    </div>

    <div class="header-section">
      <h2>{{ companyName }} – Work Statistics</h2>
      <p v-if="appliedDateRange[0] && appliedDateRange[1]">
        Period: {{ formatDate(appliedDateRange[0]) }} – {{ formatDate(appliedDateRange[1]) }}
      </p>
    </div>

    <div class="time-period-card">
      <div class="time-period-section">
        <div class="time-period">
          <label>Time period</label>
          <select
            v-model="selectedPreset"
            @change="onPresetChange"
            class="preset-select"
            :disabled="isApplying"
          >
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
                :value="selectedDateRange[0]"
                :max="selectedDateRange[1] || today"
                @change="e => { selectedDateRange[0] = e.target.value; onDateInputChange() }"
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
                :value="selectedDateRange[1]"
                :min="selectedDateRange[0] || undefined"
                :max="today"
                @change="e => { selectedDateRange[1] = e.target.value; onDateInputChange() }"
                class="date-input-hidden"
              />
            </div>
          </div>
          <button
            class="btn-apply-dates"
            v-if="hasDateChanges"
            @click="applyDateRange"
            :disabled="isApplying || !isDateRangeValid"
          >
            <span v-if="isApplying" class="btn-spinner"></span>
            <span v-else>Apply</span>
          </button>
        </div>
      </div>
    </div>

    <template v-if="!isLoading">
      <div v-if="noData" class="empty-state">
        <div class="empty-icon-circle">
          <svg width="48" height="48" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <rect x="3" y="3" width="18" height="18" rx="2" ry="2" />
            <path d="M9 3v18" />
            <path d="M15 3v18" />
            <path d="M3 9h18" />
            <path d="M3 15h18" />
          </svg>
        </div>
        <h3>No shift data available</h3>
        <p>No shifts found for this company in the selected period.</p>
      </div>

      <div v-else class="content-wrapper">
        <section class="full-card">
          <div class="dual-charts-row">
            <div class="chart-section">
              <div class="chart-header">
                <h3>Reasons</h3>
                <div class="chart-header-right">
                  <div class="btn-group">
                    <button
                      class="chart-type-btn"
                      :class="{ active: reasonsChartType === 'pie' }"
                      @click="changeReasonsChartType('pie')"
                      :disabled="!reasonStats.length"
                      title="Pie Chart"
                    >Pie</button>
                    <button
                      class="chart-type-btn"
                      :class="{ active: reasonsChartType === 'bar' }"
                      @click="changeReasonsChartType('bar')"
                      :disabled="!reasonStats.length"
                      title="Bar Chart"
                    >Bar</button>
                    <span class="btn-group-divider"></span>
                    <button class="chart-type-btn" @click="exportChartAsPNG('reasons')" :disabled="!reasonStats.length" title="Export PNG">PNG</button>
                    <button class="chart-type-btn" @click="exportTableAsXLSX('reasons')" :disabled="!reasonStats.length" title="Export XLSX">XLSX</button>
                  </div>
                </div>
              </div>
              <div class="chart-wrapper" ref="reasonsChartWrapper">
                <div class="chart-container">
                  <canvas ref="reasonsChartCanvas" :key="reasonsChartCanvasKey"></canvas>
                </div>
              </div>
            </div>

            <div class="chart-section">
              <div class="chart-header">
                <h3>Shops</h3>
                <div class="chart-header-right">
                  <div class="btn-group">
                    <button
                      class="chart-type-btn"
                      :class="{ active: shopsChartType === 'pie' }"
                      @click="changeShopsChartType('pie')"
                      :disabled="!shopStats.length"
                      title="Pie Chart"
                    >Pie</button>
                    <button
                      class="chart-type-btn"
                      :class="{ active: shopsChartType === 'bar' }"
                      @click="changeShopsChartType('bar')"
                      :disabled="!shopStats.length"
                      title="Bar Chart"
                    >Bar</button>
                    <span class="btn-group-divider"></span>
                    <button class="chart-type-btn" @click="exportChartAsPNG('shops')" :disabled="!shopStats.length" title="Export PNG">PNG</button>
                    <button class="chart-type-btn" @click="exportTableAsXLSX('shops')" :disabled="!shopStats.length" title="Export XLSX">XLSX</button>
                  </div>
                </div>
              </div>
              <div class="chart-wrapper" ref="shopsChartWrapper">
                <div class="chart-container">
                  <canvas ref="shopsChartCanvas" :key="shopsChartCanvasKey"></canvas>
                </div>
              </div>
            </div>

          </div>

          <!-- Table -->
          <div class="table-section">
            <div class="table-tabs">
              <button
                @click="currentTableSection = 'reasons'"
                :class="{ active: currentTableSection === 'reasons' }"
                class="tab-btn"
              >Reasons</button>
              <button
                @click="currentTableSection = 'shops'"
                :class="{ active: currentTableSection === 'shops' }"
                class="tab-btn"
              >Shops</button>
            </div>
            <table class="glass-table">
              <thead>
                <tr>
                  <th v-if="currentTableSection === 'shops'">Shop Code</th>
                  <th v-else>Reason</th>
                  <th v-if="currentTableSection === 'shops'">Type</th>
                  <th v-if="currentTableSection === 'shops'">Country</th>
                  <th v-if="currentTableSection === 'shops'">Address</th>
                  <th class="hours-cell">{{ currentTableSection === 'reasons' ? 'Total Hours' : 'Working Hours' }}</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="item in currentTableData" :key="item.reason || item.shopId">
                  <td v-if="currentTableSection === 'shops'">{{ item.code }}</td>
                  <td v-else>{{ item.reason }}</td>
                  <td v-if="currentTableSection === 'shops'">{{ getShopTypeName(item.type) }}</td>
                  <td v-if="currentTableSection === 'shops'">{{ getCountryName(item.country) }}</td>
                  <td v-if="currentTableSection === 'shops'">{{ item.address }}</td>
                  <td class="hours-cell">{{ item.totalHours.toFixed(1) }} h</td>
                </tr>
              </tbody>
            </table>
          </div>
        </section>
      </div>
    </template>

    <div v-if="isLoading" class="loading-state">
      <span class="btn-spinner" style="width:28px;height:28px;border-width:3px;border-color:rgba(161,41,113,0.2);border-top-color:#a12971;"></span>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, computed, nextTick, watch } from 'vue'
import { Chart, ArcElement, BarElement, CategoryScale, LinearScale, Tooltip, Legend, PieController, BarController } from 'chart.js'
import ExcelJS from 'exceljs'
import html2canvas from 'html2canvas'

Chart.register(ArcElement, BarElement, CategoryScale, LinearScale, Tooltip, Legend, PieController, BarController)

const companyId = ref(localStorage.getItem('companyId'))

const API_BASE = 'http://localhost:5001/api'

const companyName = ref('')
const isLoading = ref(false)
const isApplying = ref(false)
const showWarning = ref(false)
const warningMessage = ref('')

const selectedDateRange = ref(['', ''])
const appliedDateRange = ref(['', ''])
const selectedPreset = ref('year')
const appliedPreset = ref('year')
const isSettingPresetDates = ref(false)

const reasonStats = ref([])
const shopStats = ref([])
const reasonsChartCanvas = ref(null)
const shopsChartCanvas = ref(null)
const reasonsChartInstance = ref(null)
const shopsChartInstance = ref(null)
const reasonsChartWrapper = ref(null)
const shopsChartWrapper = ref(null)
const shopsRaw = ref([])

const reasonsChartType = ref('pie')
const shopsChartType = ref('pie')
const reasonsChartCanvasKey = ref(0)
const shopsChartCanvasKey = ref(0)

const currentTableSection = ref('reasons')

const shopTypeMap = { 1: 'Hyper', 2: 'Super', 3: 'Mini', 4: 'Express' }
const countryMap = { 1: 'Lithuania', 2: 'Latvia', 3: 'Estonia' }

const noData = computed(() => 
  reasonStats.value.length === 0 && shopStats.value.length === 0 &&
!isLoading.value)

const hasDateChanges = computed(() =>
  selectedDateRange.value[0] !== appliedDateRange.value[0] ||
  selectedDateRange.value[1] !== appliedDateRange.value[1]
)

const isDateRangeValid = computed(() => {
  if (!selectedDateRange.value[0] || !selectedDateRange.value[1]) return false
  return new Date(selectedDateRange.value[0]) <= new Date(selectedDateRange.value[1])
})

const currentTableData = computed(() =>
  currentTableSection.value === 'reasons' ? reasonStats.value : shopStats.value
)

const today = new Date().toISOString().split('T')[0]

const formatDate = (iso) => {
  if (!iso) return ''
  const [y, m, d] = iso.split('-')
  return `${d}.${m}.${y}`
}

function getShopTypeName(typeId) { return shopTypeMap[typeId] || 'Unknown' }
function getCountryName(countryId) { return countryMap[countryId] || 'Unknown' }

function showWarningMessage(message) {
  warningMessage.value = message
  showWarning.value = true
  setTimeout(() => { showWarning.value = false }, 4000)
}

function onPresetChange() {
  isSettingPresetDates.value = true
  const endDate = new Date()
  let startDate = new Date()
  switch (selectedPreset.value) {
    case 'week':    startDate.setDate(endDate.getDate() - 7); break
    case 'month':   startDate.setMonth(endDate.getMonth() - 1); break
    case '3months': startDate.setMonth(endDate.getMonth() - 3); break
    case '6months': startDate.setMonth(endDate.getMonth() - 6); break
    case 'year':    startDate.setFullYear(endDate.getFullYear() - 1); break
    case 'custom':  isSettingPresetDates.value = false; return
  }
  selectedDateRange.value = [
    startDate.toISOString().split('T')[0],
    endDate.toISOString().split('T')[0]
  ]
  queueMicrotask(() => { isSettingPresetDates.value = false })
}

function onDateInputChange() {
  if (isSettingPresetDates.value) return
  selectedPreset.value = 'custom'
}

async function applyDateRange() {
  if (!isDateRangeValid.value) return showWarningMessage('Please select a valid date range')
  isApplying.value = true
  appliedDateRange.value = [...selectedDateRange.value]
  appliedPreset.value = selectedPreset.value
  await fetchCompanyStatistics()
  isApplying.value = false
}

function calculateShiftHours(shift) {
  try {
    const { startTime, endTime, startDate } = shift
    const endDate = shift.endDate || shift.EndDate
    if (!startTime || !endTime) return 0
    const diff = new Date(`${endDate}T${endTime}`) - new Date(`${startDate}T${startTime}`)
    const hours = diff / (1000 * 60 * 60)
    return hours > 0 ? hours : 0
  } catch { return 0 }
}

async function fetchCompanyStatistics() {
  reasonStats.value = []
  shopStats.value = []
  shopsRaw.value = []

  if (!companyId.value) return
  if (!appliedDateRange.value[0] || !appliedDateRange.value[1]) return

  isLoading.value = true
  try {
    const companyRes = await fetch(`${API_BASE}/companies/${companyId.value}`)
    if (companyRes.ok) {
      const c = await companyRes.json()
      companyName.value = c.companyName || c.name || ''
    }

    const shiftsRes = await fetch(`${API_BASE}/shifts/bycompany/${companyId.value}`)
    if (!shiftsRes.ok) return showWarningMessage('Failed to load shifts')
    const allShifts = await shiftsRes.json()

    const start = new Date(appliedDateRange.value[0])
    const end = new Date(appliedDateRange.value[1])
    end.setHours(23, 59, 59, 999)

    const shiftsInRange = allShifts.filter(s => {
      const d = new Date(s.startDate)
      return d >= start && d <= end
    })

    if (shiftsInRange.length === 0) {
      showWarningMessage('No shifts found in this range')
      return
    }

    const reasonMap = {}
    const shopStatsMap = {}

    for (const shift of shiftsInRange) {
      const hours = calculateShiftHours(shift)
      if (!hours) continue

      const reasonName = shift.companyReason?.reason?.name || 'Unknown'
      if (!reasonMap[reasonName]) reasonMap[reasonName] = { reason: reasonName, totalHours: 0 }
      reasonMap[reasonName].totalHours += hours

      const shop = shift.shop
      if (shop) {
        const shopId = shop.shopID
        if (!shopStatsMap[shopId]) {
          shopStatsMap[shopId] = {
            shopId,
            code: shop.code || shop.shopCode || 'Unknown',
            type: shop.type ?? null,
            country: shop.country ?? null,
            address: shop.address ?? '',
            email: shop.email ?? '', 
            totalHours: 0, 
            shifts: [] 
          }
        }
        shopStatsMap[shopId].totalHours += hours
        shopStatsMap[shopId].shifts.push(shift) 
      }
    }

    const reasonArray = Object.values(reasonMap).filter(r => r.totalHours > 0.1)
    const shopArray = Object.values(shopStatsMap).filter(s => s.totalHours > 0.1)

    shopsRaw.value = shopArray

    const totalReasonHours = reasonArray.reduce((s, r) => s + r.totalHours, 0)
    const totalShopHours = shopArray.reduce((s, r) => s + r.totalHours, 0)

    reasonStats.value = reasonArray.map(r => ({ 
      ...r, 
      percentage: totalReasonHours ? (r.totalHours / totalReasonHours) * 100 : 0 
    }))

    shopStats.value = shopArray.map(s => ({ 
      ...s, 
      percentage: totalShopHours ? (s.totalHours / totalShopHours) * 100 : 0 
    }))

    await nextTick()
  } catch (e) {
    console.error('Failed to fetch statistics', e)
    showWarningMessage('Failed to load statistics. Please try again.')
  } finally {
    isLoading.value = false
  }
}

function destroyReasonsChart() {
  if (reasonsChartInstance.value) { reasonsChartInstance.value.destroy(); reasonsChartInstance.value = null }
}
function destroyShopsChart() {
  if (shopsChartInstance.value) { shopsChartInstance.value.destroy(); shopsChartInstance.value = null }
}

async function initReasonsChart() {
  await nextTick()
  if (!reasonsChartWrapper.value || !reasonsChartCanvas.value || !reasonStats.value.length) return

  const canvas = reasonsChartCanvas.value
  const width = Math.min(800, reasonsChartWrapper.value.offsetWidth - 40)
  if (width <= 0) return

  const height = reasonsChartType.value === 'pie' ? 460 : 400

  canvas.width = width
  canvas.height = height
  canvas.style.width = `${width}px`
  canvas.style.height = `${height}px`

  destroyReasonsChart()
  reasonsChartInstance.value = new Chart(canvas.getContext('2d'), getReasonsChartConfig())
}

async function initShopsChart() {
  await nextTick()
  if (!shopsChartWrapper.value || !shopsChartCanvas.value || !shopStats.value.length) return
  
  const canvas = shopsChartCanvas.value
  const width = Math.min(800, shopsChartWrapper.value.offsetWidth - 40)
  if (width <= 0) return

  const height = shopsChartType.value === 'pie' ? 460 : 400 

  canvas.width = width 
  canvas.height = height
  canvas.style.width = `${width}px`; 
  canvas.style.height = `${height}px`

  destroyShopsChart()
  shopsChartInstance.value = new Chart(canvas.getContext('2d'), getShopsChartConfig())
}

function getPalette(count) {
  const base = ['#FF6B6B','#4ECDC4','#45B7D1','#FFA07A','#98D8C8','#F7DC6F','#BB8FCE','#85C1E2','#F8B739','#52B788']
  return Array.from({ length: count }, (_, i) => base[i % base.length])
}

function getReasonsChartConfig() {
  const labels = reasonStats.value.map(i => i.reason)
  const data = reasonsChartType.value === 'pie'
    ? reasonStats.value.map(i => i.percentage)
    : reasonStats.value.map(i => i.totalHours)
  return {
    type: reasonsChartType.value,
    data: { labels, datasets: [{ label: reasonsChartType.value === 'pie' ? 'Share %' : 'Hours', data, backgroundColor: getPalette(labels.length), borderColor: '#ffffff', borderWidth: 2 }] },
    options: {
      responsive: true, maintainAspectRatio: false,
      layout: { padding: { left: 20, right: 20, top: 20, bottom: 20 } },
      plugins: {
        legend: { 
          display: reasonsChartType.value === 'pie', 
          position: 'bottom', 
          maxHeight: 80, 
          labels: {
            usePointStyle: true, 
            pointStyle: 'circle',
            padding: 14, 
            boxWidth: 10 
          }
         },
        tooltip: { callbacks: { label(ctx) { return reasonsChartType.value === 'pie' ? `${ctx.label}: ${ctx.parsed.toFixed(1)}%` : `${ctx.label}: ${ctx.parsed.toFixed(1)} h` } } }
      },
      scales: reasonsChartType.value === 'bar' ? { y: { beginAtZero: true, ticks: { callback: v => `${v.toFixed(1)} h` } } } : {}
    }
  }
}

function getShopsChartConfig() {
  const labels = shopStats.value.map(i => i.code)
  const data = shopsChartType.value === 'pie'
    ? shopStats.value.map(i => i.percentage)
    : shopStats.value.map(i => i.totalHours)
  return {
    type: shopsChartType.value,
    data: { labels, datasets: [{ label: shopsChartType.value === 'pie' ? 'Share %' : 'Hours', data, backgroundColor: getPalette(labels.length), borderColor: '#ffffff', borderWidth: 2 }] },
    options: {
      responsive: true, maintainAspectRatio: false,
      layout: { padding: { left: 20, right: 20, top: 20, bottom: 20 } },
      plugins: {
        legend: { 
          display: reasonsChartType.value === 'pie', 
          position: 'bottom', 
          maxHeight: 80, 
          labels: {
            usePointStyle: true, 
            pointStyle: 'circle',
            padding: 14, 
            boxWidth: 10 
          }
         },
        tooltip: { callbacks: { label(ctx) { return shopsChartType.value === 'pie' ? `${ctx.label}: ${ctx.parsed.toFixed(1)}%` : `${ctx.label}: ${ctx.parsed.toFixed(1)} h` } } }
      },
      scales: shopsChartType.value === 'bar' ? { y: { beginAtZero: true, ticks: { callback: v => `${v.toFixed(1)} h` } } } : {}
    }
  }
}

function changeReasonsChartType(type) {
  if (reasonsChartType.value === type || !reasonStats.value.length) return
  reasonsChartType.value = type
  reasonsChartCanvasKey.value++
  nextTick(() => initReasonsChart())
}

function changeShopsChartType(type) {
  if (shopsChartType.value === type || !shopStats.value.length) return
  shopsChartType.value = type
  shopsChartCanvasKey.value++
  nextTick(() => initShopsChart())
}

async function exportChartAsPNG(section) {
  const wrapper = section === 'reasons' ? reasonsChartWrapper.value : shopsChartWrapper.value
  if (!wrapper) return showWarningMessage('No chart available to export')
  try {
    const canvas = await html2canvas(wrapper, { backgroundColor: '#ffffff', scale: 2 })
    const link = document.createElement('a')
    link.download = `${companyName.value}-${section}-${appliedDateRange.value[0]}to${appliedDateRange.value[1]}.png`
    link.href = canvas.toDataURL('image/png')
    document.body.appendChild(link)
    link.click()
    document.body.removeChild(link)
  } catch (e) {
    showWarningMessage('Failed to export chart: ' + e.message)
  }
}

async function exportTableAsXLSX(section) {
  const data = section === 'reasons' ? reasonStats.value : shopStats.value
  if (!data.length) {
    return showWarningMessage('No data to export')
  }

  try {
    const workbook = new ExcelJS.Workbook()
    workbook.creator = 'EETMS'
    workbook.created = new Date()

    if (section === 'reasons') {
      const worksheet = workbook.addWorksheet('Reasons')

      worksheet.columns = [
        { header: 'Reason', key: 'reason', width: 35 }, 
        { header: 'Total Hours', key: 'totalHours', width: 18 }, 
      ]

      const rows = reasonStats.value.map(r => [
        r.reason,
        Number(r.totalHours.toFixed(1)), 
      ])

      worksheet.addTable({
        name: 'ReasonsTable',
        ref: 'A1:C1',
        headerRow: true,
        totalsRow: true, 
        style: {
          theme: 'TableStyleMedium2',
          showRowStripes: true
        }, 
        columns: [ 
          { name: 'Reason', filterButton: true, totalsRowLabel: 'Total' },
          { name: 'Total Hours', filterButton: true, totalsRowFunction: 'sum' }, 
        ], 
        rows
      })

      const totalRows = rows.length + 2 
      for (let i = 1; i <= totalRows; i++) {
        ;['A', 'B', 'C'].forEach(col => {
          worksheet.getCell(`${col}${i}`).alignment = {
            horizontal: 'center', 
            vertical: 'middle' 
          }
        })
      }
    } else {
      const worksheet = workbook.addWorksheet('Shop Details')

      worksheet.columns = [ 
        { header: 'Shop Code', key: 'code', width: 15 }, 
        { header: 'Type', key: 'type', width: 15 },
        { header: 'Country', key: 'country', width: 15 },
        { header: 'Address', key: 'address', width: 40 },
        { header: 'Email', key: 'email', width: 30 }, 
        { header: 'Working Hours', key: 'hours', width: 15 }
      ]

      const summaryRows = shopStats.value.map(s => [ 
        s.code, 
        getShopTypeName(s.type), 
        getCountryName(s.country), 
        s.address, 
        s.email || '', 
        Number(s.totalHours.toFixed(1))
      ])

      worksheet.addTable({
        name: 'ShopTable',
        ref: 'A1:F1', 
        headerRow: true, 
        totalsRow: false, 
        style: {
          theme: 'TableStyleMedium2',
          showRowStripes: true
        }, 
        columns: [
          { name: 'Shop Code', filterButton: true },
          { name: 'Type', filterButton: true },
          { name: 'Country', filterButton: true },
          { name: 'Address', filterButton: true },
          { name: 'Email', filterButton: true },
          { name: 'Working Hours', filterButton: true }
        ],
        rows: summaryRows 
      })

      const summaryRowCount = summaryRows.length + 1
      for (let i = 1; i <= summaryRowCount; i++) {
        ;['A', 'B', 'C', 'D', 'E', 'F'].forEach(col => {
          worksheet.getCell(`${col}${i}`).alignment = {
            horizontal: 'center',
            vertical: 'middle'
          }
        })
      }

      for (const shop of shopsRaw.value) {
        const sheetName = (shop.code || `Shop_${shop.shopId}`)
          .replace(/[\\/*?:[\]]/g, '')
          .substring(0, 31)
        
        const shopSheet = workbook.addWorksheet(sheetName)

        shopSheet.columns = [
          { header: 'Start Date', key: 'startDate', width: 15 },
          { header: 'Start Time', key: 'startTime', width: 12 },
          { header: 'End Date', key: 'endDate', width: 15 },
          { header: 'End Time', key: 'endTime', width: 12 },
          { header: 'Reason', key: 'reason', width: 40 },
          { header: 'Work Hours', key: 'workHours', width: 12 }
        ]

        const shiftRows = (shop.shifts || []).map(shift => {
          const hours = calculateShiftHours(shift)
          const reason = shift.companyReason?.reason?.name || 'Unknown'
          return [
            shift.startDate ?? '',
            shift.startTime ?? '',
            shift.endDate ?? shift.EndDate ?? '',
            shift.endTime ?? '',
            reason,
            Number(hours.toFixed(2))
          ]
        })

        if (shiftRows.length > 0) {
          shopSheet.addTable({
            name: `Shifts_${shop.shopId}`,
            ref: 'A1:F1',
            headerRow: true,
            totalsRow: true,
            style: {
              theme: 'TableStyleMedium4',
              showRowStripes: true
            },
            columns: [
              { name: 'Start Date', filterButton: true, totalsRowLabel: '' },
              { name: 'Start Time', filterButton: true, totalsRowLabel: '' },
              { name: 'End Date', filterButton: true, totalsRowLabel: '' },
              { name: 'End Time', filterButton: true, totalsRowLabel: '' },
              { name: 'Reason', filterButton: true, totalsRowLabel: 'Total' },
              { name: 'Work Hours', filterButton: true, totalsRowFunction: 'sum' }
            ],
            rows: shiftRows
          })

          const shiftRowCount = shiftRows.length + 2
          for (let i = 1; i <= shiftRowCount; i++) {
            ;['A', 'B', 'C', 'D', 'E', 'F'].forEach(col => {
              shopSheet.getCell(`${col}${i}`).alignment = {
                horizontal: 'center',
                vertical: 'middle'
              }
            })
          }
        }
      }
    }

    const buffer = await workbook.xlsx.writeBuffer()

    const blob = new Blob([buffer], {
      type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet'
    })

    const url = window.URL.createObjectURL(blob)
    const link = document.createElement('a')
    link.href = url
    link.download = `${companyName.value}-${section}-${appliedDateRange.value[0]} to ${appliedDateRange.value[1]}.xlsx`

    document.body.appendChild(link)
    link.click()
    document.body.removeChild(link)
    window.URL.revokeObjectURL(url)

  } catch (error) {
    console.error('Failed to export XLSX', error)
    showWarningMessage('Failed to export XLSX file')
  }
}

watch(reasonStats, async (val) => { if (val.length) { await nextTick(); await initReasonsChart() } }, { deep: true })
watch(shopStats,   async (val) => { if (val.length) { await nextTick(); await initShopsChart()   } }, { deep: true })

onMounted(async () => {
  const end = new Date()
  const start = new Date()
  start.setFullYear(end.getFullYear() - 1)
  selectedDateRange.value = [start.toISOString().split('T')[0], end.toISOString().split('T')[0]]
  appliedDateRange.value = [...selectedDateRange.value]
  await fetchCompanyStatistics()
})
</script>

<style scoped>
.single-company-statistics-container {
  font-family: 'Inter', sans-serif;
  padding: 32px 48px;
  max-width: 1600px;
  margin: 0 auto;
}

.blob-pink {
  width: 950px;
  height: 950px;
  background: var(--brand-berry);
  top: -450px; right: -320px;
  position: absolute;
  border-radius: 50%;
  opacity: 0.1;
  z-index: -1;
  filter: blur(60px);
}

.header-section h2 {
  margin: 0 0 4px;
  font-size: 24px;
  color: var(--brand-berry, #a12971);
}
.header-section p {
  margin: 0 0 16px;
  color: var(--color-text-dim, #666);
  font-size: 13px;
}

.time-period-card {
  background: var(--color-white, #fff);
  border-radius: 16px;
  box-shadow: 0 2px 8px rgba(0,0,0,0.06);
  padding: 24px 32px;
  width: 100%;
  max-width: 1400px;
  margin: 0 auto 20px;
}

.time-period-section { 
  display: flex; 
  flex-direction: column; 
  gap: 12px; 
}

.time-period { 
  display: flex; 
  flex-direction: column; 
  gap: 8px; 
}

.time-period label { 
  font-size: 13px; 
  font-family: 'Inter';
  font-weight: 600; 
  color: var(--brand-berry, #a12971); 
}

.preset-select {
  padding: 8px 12px;
  border-radius: 14px;
  border: 1px solid var(--color-border, #e0e0e0);
  font-size: 13px;
  font-family: 'Inter';
  width: 100%;
  box-sizing: border-box; 
}

.date-inputs { 
  display: flex; 
  align-items: center; 
  gap: 8px; 
  margin-top: 8px; 
  width: 100%;
}

.date-input  { 
  flex: 1; 
  width: 100%; 
  padding: 8px 10px; 
  border-radius: 14px; 
  border: 1px solid var(--color-border, #e0e0e0); 
  font-size: 13px;
  font-family: 'Inter';
  box-sizing: border-box; 
  cursor: pointer; 
}

.date-separator { 
  font-weight: 600; 
  color: var(--color-text-light, #999); 
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
  margin-top: 10px;
  align-self: flex-end;
  padding: 8px 16px;
  border-radius: 16px;
  border: none;
  background: var(--brand-berry, #a12971);
  color: #fff;
  font-size: 13px;
  font-weight: 600;
  cursor: pointer;
  transition: background 0.18s;
}
.btn-apply-dates:hover:not(:disabled) { background: #87205d; }
.btn-apply-dates:disabled { opacity: 0.5; cursor: not-allowed; }

.btn-spinner {
  width: 14px; height: 14px;
  border-radius: 50%;
  border: 2px solid rgba(255,255,255,0.4);
  border-top-color: #fff;
  animation: spin 0.7s linear infinite;
  display: inline-block;
}
@keyframes spin { to { transform: rotate(360deg); } }

.content-wrapper { width: 100%; max-width: 1400px; margin: 0 auto; }

.full-card {
  background: #fff;
  border-radius: 20px;
  box-shadow: 0 8px 32px rgba(0,0,0,0.08);
  padding: 32px;
  display: flex;
  flex-direction: column;
  gap: 24px;
  width: 100%;
}

.dual-charts-row { display: flex; gap: 24px; align-items: flex-start; flex-wrap: wrap; }
.chart-section   { flex: 1; min-width: 400px; max-width: 50%; }

.chart-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 8px;
}
.chart-header h3 { margin: 0; font-size: 16px; font-weight: 600; }
.chart-header-right { display: flex; align-items: center; gap: 8px; flex-shrink: 0; }

.btn-group {
  display: inline-flex;
  align-items: center;
  gap: 2px;
  background: #f4f6fb;
  border: 1px solid rgba(0,0,0,0.07);
  padding: 3px;
  border-radius: 10px;
  box-shadow: 0 1px 3px rgba(0,0,0,0.06);
}

.btn-group-divider {
  width: 1px;
  height: 16px;
  background: #d8d5d0;
  margin: 0 3px;
  flex-shrink: 0;
  border-radius: 1px;
}

.chart-type-btn {
  display: inline-flex;
  align-items: center;
  gap: 4px;
  padding: 4px 11px;
  border-radius: 7px;
  border: none;
  font-size: 12px;
  font-weight: 600;
  cursor: pointer;
  background: transparent;
  color: #777;
  transition: background 0.18s, color 0.18s, box-shadow 0.18s;
  white-space: nowrap;
}
.chart-type-btn:hover:not(:disabled):not(.active) {
  background: rgba(161,41,113,0.08);
  color: var(--brand-berry, #a12971);
}

.chart-type-btn.active {
  background: var(--brand-berry, #a12971);
  color: #fff;
  box-shadow: 0 1px 4px rgba(161,41,113,0.3);
}

.chart-type-btn:disabled { opacity: 0.38; cursor: not-allowed; }

.chart-type-btn.export-btn { color: #888; }
.chart-type-btn.export-btn:hover:not(:disabled) {
  background: rgba(161,41,113,0.08);
  color: var(--brand-berry, #a12971);
}

.chart-wrapper   { height: 400px; width: 100%; position: relative; display: block; }
.chart-container { position: absolute; top: 0; left: 0; width: 100%; height: 100%; }

.table-section { width: 100%; }

.table-tabs {
  display: inline-flex;
  gap: 2px;
  background: #f4f6fb;
  border: 1px solid rgba(0,0,0,0.07);
  padding: 3px;
  border-radius: 10px;
  box-shadow: 0 1px 3px rgba(0,0,0,0.06);
  margin-bottom: 16px;
  width: fit-content;
}

.tab-btn {
  display: inline-flex;
  align-items: center;
  padding: 4px 14px;
  border-radius: 7px;
  border: none;
  font-size: 12px;
  font-weight: 600;
  cursor: pointer;
  background: transparent;
  color: #777;
  transition: background 0.18s, color 0.18s, box-shadow 0.18s;
}
.tab-btn:hover:not(.active) {
  background: rgba(161,41,113,0.08);
  color: var(--brand-berry, #a12971);
}
.tab-btn.active {
  background: var(--brand-berry, #a12971);
  color: #fff;
  box-shadow: 0 1px 4px rgba(161,41,113,0.3);
}

.glass-table { 
  width: 100%; 
  border-collapse: collapse; 
  background: #fff; 
  font-size: 13px; 
}

.glass-table th,
.glass-table td { 
  padding: 8px 10px; 
  border-bottom: 1px solid rgba(0,0,0,0.05); 
}

.glass-table thead th {
  background: var(--header-gradient, linear-gradient(135deg, #667eea 0%, #764ba2 100%));
  color: #fff;
  text-transform: uppercase;
  font-size: 11px;
  letter-spacing: 0.5px;
}

.hours-cell { 
  text-align: right; 
  font-weight: 600; 
  color: var(--brand-berry, #a12971); 
}

.empty-state {
  background: var(--color-bg-light, #f8f9fa);
  padding: 40px 30px;
  border-radius: 12px;
  text-align: center;
  border: 2px dashed var(--color-border, #e0e0e0);
  margin-top: 16px;
}
.empty-icon-circle {
  width: 70px; height: 70px;
  border-radius: 50%;
  background: #fff;
  display: flex;
  align-items: center;
  justify-content: center;
  margin: 0 auto 16px;
  color: var(--brand-berry, #a12971);
}

.warning-banner { position: fixed; top: 20px; right: 20px; z-index: 1000; }
.warning-content {
  background: var(--color-warning-bg, #fff7e6);
  border: 1px solid var(--color-warning-border, #ffa940);
  border-radius: 8px;
  padding: 10px 14px;
  display: flex;
  align-items: center;
  gap: 8px;
}
.warning-icon {
  width: 22px; height: 22px;
  border-radius: 50%;
  background: var(--color-warning-border, #ffa940);
  color: #fff;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: 700;
}
.warning-close { margin-left: auto; border: none; background: transparent; font-size: 18px; cursor: pointer; }

.loading-state {
  margin-top: 48px;
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 12px;
  font-size: 14px;
  color: var(--color-text-dim, #666);
}
</style>
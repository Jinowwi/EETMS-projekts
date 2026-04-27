<template>
  <div class="modal-backdrop" @click.self="$emit('close')">
    <div class="modal-card">

      <div class="modal-header">
        <div>
          <h2>{{ company?.name }} reason statistics</h2>
          <p class="modal-subtitle">
            Total hours: {{ totalHours.toFixed(1) }}h &nbsp;|&nbsp;
            Time period: {{ dateRange[0] }} – {{ dateRange[1] }}
          </p>
        </div>
        <button class="modal-close" @click="$emit('close')">×</button>
      </div>

      <div class="modal-body">
        <div class="chart-section" v-if="reasons.length > 0">
          <div class="chart-header">
            <h2>Reason work hours</h2>
            <div class="chart-controls">
              <div class="chart-type-switcher">
                <button
                  class="chart-type-btn"
                  :class="{ active: chartType === 'pie' }"
                  @click="changeChartType('pie')"
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
                  title="Line Chart"
                >
                  <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                    <polyline points="22 12 18 12 15 21 9 3 6 12 2 12"></polyline>
                  </svg>
                </button>
              </div>
              <button class="export-btn" @click="exportChartAsPNG" :disabled="reasons.length === 0" title="Export Chart as PNG">
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
              <canvas ref="reasonChart"></canvas>
            </div>
            <div class="chart-legend">
              <div v-for="(r, index) in reasons" :key="index" class="legend-item">
                <span class="legend-color" :style="{ backgroundColor: r.color }"></span>
                <span class="legend-label">{{ r.name }}</span>
              </div>
            </div>
          </div>
        </div>

      <div class="company-table" v-if="reasons.length > 0">
          <div class="table-header-section">
            <h3><strong>Reason Details</strong></h3>
            <button class="export-btn" @click="exportTableAsXLSX" :disabled="reasons.length === 0" title="Export Table as Excel">
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
                <th>
                  <span class="sortable-header" @click="toggleSort('name')">
                    NAME
                    <FontAwesomeIcon
                      v-if="sortColumn === 'name'"
                      :icon="sortDirection === 'asc' ? ['fas', 'sort-up'] : ['fas', 'sort-down']"
                    />
                    <FontAwesomeIcon v-else :icon="['fas', 'sort']" />
                  </span>
                </th>
                <th>
                  <span class="sortable-header" @click="toggleSort('hours')">
                    WORK HOURS
                    <FontAwesomeIcon
                      v-if="sortColumn === 'hours'"
                      :icon="sortDirection === 'asc' ? ['fas', 'sort-up'] : ['fas', 'sort-down']"
                    />
                    <FontAwesomeIcon v-else :icon="['fas', 'sort']" />
                  </span>
                </th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="(r, index) in sortedReasons" :key="index">
                <td>{{ r.name }}</td>
                <td>{{ r.hours.toFixed(1) }}h</td>
              </tr>
            </tbody>
          </table>
        </div>

        <div v-if="reasons.length === 0" class="empty-state">
          <div class="empty-icon-circle">
            <svg width="48" height="48" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <circle cx="12" cy="12" r="10"/>
              <line x1="12" y1="8" x2="12" y2="12"/>
              <line x1="12" y1="16" x2="12.01" y2="16"/>
            </svg>
          </div>
          <h3>No reason data available</h3>
          <p>No shifts with valid reasons found for this company in the selected period</p>
        </div>
      </div>

    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, onBeforeUnmount, watch, nextTick } from 'vue'
import {
  Chart, ArcElement, BarElement, LineElement, PointElement,
  CategoryScale, LinearScale, Tooltip, Legend,
  PieController, BarController, LineController
} from 'chart.js'

import ExcelJS from 'exceljs'
import html2canvas from 'html2canvas'

Chart.register(
  ArcElement, BarElement, LineElement, PointElement,
  CategoryScale, LinearScale, Tooltip, Legend,
  PieController, BarController, LineController
)

const props = defineProps({
  company: { type: Object, required: true },
  companyStat: { type: Object, required: true },
  dateRange: { type: Array, required: true }
})

defineEmits(['close'])

const reasonChart = ref(null)
const chartInstance = ref(null)
const chartType = ref('pie')
const reasons = ref([])
const chartWrapperElement = ref(null)
const totalHours = ref(0)

const sortColumn = ref('hours')
const sortDirection = ref('desc')

const toggleSort = (column) => {
  if (sortColumn.value === column) {
    sortDirection.value = sortDirection.value === 'asc' ? 'desc' : 'asc'
  } else {
    sortColumn.value = column
    sortDirection.value = 'asc'
  }
}

const sortedReasons = computed(() => {
  return [...reasons.value].sort((a, b) => {
    let valA, valB
    if (sortColumn.value === 'hours') {
      valA = a.hours
      valB = b.hours
    } else {
      valA = (a.name || '').toLowerCase()
      valB = (b.name || '').toLowerCase()
    }
    if (valA < valB) return sortDirection.value === 'asc' ? -1 : 1
    if (valA > valB) return sortDirection.value === 'asc' ? 1 : -1
    return 0
  })
})

const buildReasonStats = () => {
  const stats = {}

  for (const shift of props.companyStat.shifts || []) {
    // CORRECT PATH confirmed from data structure
    const reasonName = shift.companyReason?.reason?.name
    if (!reasonName) continue

    if (!stats[reasonName]) {
      stats[reasonName] = { name: reasonName, hours: 0 }
    }

    const hours = calculateShiftHours(shift)
    if (hours > 0) {
      stats[reasonName].hours += hours
    }
  }

  const reasonArray = Object.values(stats)
    .filter(r => r.hours >= 0.1)
    .sort((a, b) => b.hours - a.hours)

  totalHours.value = reasonArray.reduce((sum, r) => sum + r.hours, 0)

  const palette = [
    '#FF6B6B', '#4ECDC4', '#45B7D1', '#FFA07A', '#98D8C8',
    '#F7DC6F', '#BB8FCE', '#85C1E2', '#F8B739', '#52B788',
    '#E63946', '#A8DADC', '#457B9D', '#F4A261', '#2A9D8F',
    '#E76F51', '#264653', '#E9C46A', '#F77F00', '#06AED5'
  ]

  reasons.value = reasonArray.map((reason, index) => ({
    ...reason,
    percentage: totalHours.value > 0
      ? ((reason.hours / totalHours.value) * 100).toFixed(1)
      : 0,
    color: palette[index % palette.length]
  }))
}

const calculateShiftHours = (shift) => {
  try {
    const startTime = shift.startTime || shift.StartTime
    const endTime = shift.endTime || shift.EndTime
    const startDate = shift.startDate || shift.StartDate
    const endDate = shift.endDate || shift.EndDate
    if (!startTime || !endTime || !startDate) return 0
    const start = new Date(`${startDate}T${startTime}`)
    const end = new Date(`${endDate}T${endTime}`)
    const diffHours = (end.getTime() - start.getTime()) / (1000 * 60 * 60)
    return Math.max(0, diffHours)
  } catch {
    return 0
  }
}

const updateChart = () => {
  if (!reasonChart.value || reasons.value.length === 0) return

  if (chartInstance.value) {
    chartInstance.value.destroy()
    chartInstance.value = null
  }

  const canvas = reasonChart.value
  const labels = reasons.value.map(r => r.name)
  const hoursData = reasons.value.map(r => r.hours)
  const colors = reasons.value.map(r => r.color)

  const commonTooltip = {
    callbacks: {
      label: (context) => {
        const hours = hoursData[context.dataIndex]
        const pct = reasons.value[context.dataIndex].percentage
        return `${context.label}: ${hours.toFixed(1)}h (${pct}%)`
      }
    }
  }

  if (chartType.value === 'pie') {
    chartInstance.value = new Chart(canvas, {
      type: 'pie',
      data: {
        labels,
        datasets: [{
          data: reasons.value.map(r => parseFloat(r.percentage)),
          backgroundColor: colors,
          borderColor: '#ffffff',
          borderWidth: 2
        }]
      },
      options: {
        responsive: true,
        maintainAspectRatio: true,
        animation: false,
        plugins: { legend: { display: false }, tooltip: commonTooltip }
      }
    })
  } else if (chartType.value === 'bar') {
    chartInstance.value = new Chart(canvas, {
      type: 'bar',
      data: {
        labels,
        datasets: [{
          label: 'Working Hours',
          data: hoursData,
          backgroundColor: colors,
          borderColor: colors,
          borderWidth: 1
        }]
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        animation: false,
        scales: {
          y: { beginAtZero: true, ticks: { callback: (v) => `${v.toFixed(1)}h` } },
          x: { ticks: { maxRotation: 45, minRotation: 45 } }
        },
        plugins: { legend: { display: false }, tooltip: commonTooltip }
      }
    })
  } else if (chartType.value === 'line') {
    chartInstance.value = new Chart(canvas, {
      type: 'line',
      data: {
        labels,
        datasets: [{
          label: 'Working Hours',
          data: hoursData,
          backgroundColor: 'rgba(161, 41, 113, 0.1)',
          borderColor: '#a12971',
          borderWidth: 2,
          pointBackgroundColor: colors,
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
          y: { beginAtZero: true, ticks: { callback: (v) => `${v.toFixed(1)}h` } },
          x: { ticks: { maxRotation: 45, minRotation: 45 } }
        },
        plugins: { legend: { display: false }, tooltip: commonTooltip }
      }
    })
  }
}

const exportChartAsPNG = async () => {
  try {
    if (!chartInstance.value) return
    if (!chartWrapperElement.value) return
    const canvas = await html2canvas(chartWrapperElement.value, {
      backgroundColor: '#ffffff',
      scale: 2
    })
    const url = canvas.toDataURL('image/png')
    const link = document.createElement('a')
    link.download = `${props.company.name}-reason-chart-${props.dateRange[0]}to${props.dateRange[1]}.png`
    link.href = url
    document.body.appendChild(link)
    link.click()
    document.body.removeChild(link)
  } catch (error) {
    console.error('Export chart error:', error)
  }
}

const exportTableAsXLSX = async () => {
  if (reasons.value.length === 0) return
  const workbook = new ExcelJS.Workbook()
  const worksheet = workbook.addWorksheet('Reason Details')
  worksheet.columns = [
    { header: 'Reason Name', key: 'name', width: 30 },
    { header: 'Working Hours', key: 'hours', width: 20 }
  ]
  sortedReasons.value.forEach(r => {
    worksheet.addRow({ name: r.name, hours: `${r.hours.toFixed(1)}h` })
  })
  const lastRow = sortedReasons.value.length + 1
  worksheet.addTable({
    name: 'ReasonTable',
    ref: 'A1',
    headerRow: true,
    totalsRow: false,
    style: { theme: 'TableStyleMedium2', showRowStripes: true },
    columns: [
      { name: 'Reason Name', filterButton: true },
      { name: 'Working Hours', filterButton: true }
    ],
    rows: sortedReasons.value.map(r => [r.name, `${r.hours.toFixed(1)}h`])
  })
  for (let i = 1; i <= lastRow; i++) {
    ['A', 'B'].forEach(col => {
      worksheet.getCell(`${col}${i}`).alignment = { horizontal: 'center', vertical: 'middle' }
    })
  }
  const buffer = await workbook.xlsx.writeBuffer()
  const blob = new Blob([buffer], { type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' })
  const url = window.URL.createObjectURL(blob)
  const link = document.createElement('a')
  link.href = url
  link.download = `${props.company.name}-reasons-${props.dateRange[0]}to${props.dateRange[1]}.xlsx`
  document.body.appendChild(link)
  link.click()
  document.body.removeChild(link)
  window.URL.revokeObjectURL(url)
}

const changeChartType = async (type) => {
  chartType.value = type
  await nextTick()
  updateChart()
}

onMounted(() => {
  buildReasonStats()
  nextTick().then(updateChart)
})

onBeforeUnmount(() => {
  if (chartInstance.value) chartInstance.value.destroy()
})

watch(() => props.companyStat, () => {
  buildReasonStats()
  nextTick().then(updateChart)
}, { deep: true })
</script>

<style scoped>
/* Modal backdrop + card */
.modal-backdrop {
  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, 0.5);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 3000;
  backdrop-filter: blur(4px);
}

.modal-card {
  background: var(--color-white);
  border-radius: 20px;
  width: min(1100px, 95vw);
  max-height: 90vh;
  overflow: hidden;
  display: flex;
  flex-direction: column;
  box-shadow: 0 20px 60px rgba(0, 0, 0, 0.3);
  animation: modalSlideIn 0.3s ease-out;
}

@keyframes modalSlideIn {
  from { opacity: 0; transform: translateY(-20px) scale(0.97); }
  to   { opacity: 1; transform: translateY(0)   scale(1); }
}

/* Header */
.modal-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  padding: 24px 28px 16px;
  border-bottom: 1px solid var(--color-border);
  flex-shrink: 0;
}

.modal-header h2 {
  font-size: 22px;
  font-weight: 700;
  margin: 0 0 6px 0;
  color: var(--brand-berry);
}

.modal-subtitle {
  margin: 0;
  font-size: 13px;
  color: var(--color-text-dim);
}

.modal-close {
  background: var(--color-bg-light);
  border: none;
  border-radius: 50%;
  width: 36px;
  height: 36px;
  font-size: 20px;
  cursor: pointer;
  color: var(--color-text-dim);
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
  transition: all 0.2s;
}

.modal-close:hover {
  background: var(--brand-berry);
  color: white;
}

/* Body - same grid as ShopStatistics .content-row */
.modal-body {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 20px;
  padding: 28px;
  overflow-y: auto;
  flex: 1;
  min-height: 0;
}

/* CHART SECTION - exact copy from ShopStatistics */
.chart-section {
  background: var(--color-white);
  padding: 30px;
  border-radius: 12px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
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

.chart-controls {
  display: flex;
  align-items: center;
  gap: 12px;
}

.chart-type-switcher {
  display: flex;
  gap: 4px;
  background: var(--color-bg-light);
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

.chart-type-btn:hover {
  background: var(--color-bg-muted);
  color: var(--color-text-main);
}

.chart-type-btn.active {
  background: var(--brand-berry);
  color: var(--color-white);
}

.chart-type-btn svg { display: block; }

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

/* ── Table wrapper ── */
.company-table {
  background: var(--color-white);
  padding: 30px;
  border-radius: 12px;
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

.company-table table {
  width: 100%;
  border-collapse: separate;
  border-spacing: 0;
  background: var(--color-white);
  font-size: 11px;
  table-layout: fixed;
  border: 2px solid var(--color-border);
  border-radius: 12px;
  overflow: hidden;
  box-shadow: var(--shadow-box) 0px 4px 12px;
}

.company-table table thead {
  background: var(--header-gradient);
}

.company-table table thead tr th {
  text-align: center;
  padding: 16px 20px;
  font-size: 11px;
  font-weight: 600;
  color: var(--color-white);
  text-transform: uppercase;
  letter-spacing: 0.6px;
  background: transparent;
  vertical-align: middle;
  border: none;
  white-space: nowrap;
  overflow: hidden;
}

.sortable-header {
  cursor: pointer;
  user-select: none;
  white-space: nowrap;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 6px;
  width: 100%;
}

.company-table table tbody tr {
  border-bottom: 1px solid var(--color-border);
  transition: background-color 0.2s;
}

.company-table table tbody tr:hover {
  background-color: var(--color-bg-light);
}

.company-table table tbody tr:last-child {
  border-bottom: none;
}

.company-table table tbody tr td {
  padding: 16px 20px;
  font-size: 14px;
  color: var(--color-text-main);
  font-weight: 500;
  text-align: center;
  border-bottom: none;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.company-table table tbody tr td:first-child,
.company-table table tbody tr td:last-child {
  font-weight: 600;
  color: var(--color-black);
}

.empty-icon-circle {
  width: 80px;
  height: 80px;
  background: var(--color-white);
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  margin: 0 auto 20px;
  color: var(--brand-berry);
}

.empty-state h3 {
  font-size: 20px;
  font-weight: 600;
  margin: 0 0 10px;
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
  background: var(--color-white);
  color: var(--brand-teal);
  border: none;
  border-radius: 8px;
  font-size: 14px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.2s ease;
  box-shadow: 0 2px 6px rgba(72, 187, 185, 0.3);
  flex-shrink: 0;
  white-space: nowrap;
}

.export-btn:hover:not(:disabled) {
  transform: translateY(-1px);
  box-shadow: 0 4px 8px rgba(72, 187, 185, 0.4);
}

.export-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
  transform: none;
}

.export-btn svg {
  flex-shrink: 0;
}

/* Responsive */
@media (max-width: 900px) {
  .modal-body { grid-template-columns: 1fr; }
  .chart-legend { grid-template-columns: repeat(3, 1fr); }
}

@media (max-width: 600px) {
  .modal-body { padding: 16px; gap: 16px; }
  .chart-container { height: 250px; }
  .chart-legend { grid-template-columns: 1fr; }
}
</style>

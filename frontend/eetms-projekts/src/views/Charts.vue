<template>
  <div class="page-content">
    <!-- Fona noformējums -->
    <div class="blob blob-teal"></div>
    <div class="blob blob-pink"></div>

    <!-- Rādīt uzņēmuma pārstāvja statistiku, ja lomas limenis ir 0, vai lomas nav -->
    <template v-if="!roleLevel || roleLevel === 0">
      <CompanyRepresentativeStats />
    </template>

    <!-- Citām lomām rādīt veikalu/uzņēmumu statistiku -->
    <template v-else>
      <div class="reports-header">
      <div class="statistics-switcher">
        <!-- Pogas parslēgšanai starp veikaliem/uzņēmumiem -->
        <button 
          :class="['switch-btn', { active: activeTab === 'shops' }]"
          @click="activeTab = 'shops'"
        >
          <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <path d="M3 9l9-7 9 7v11a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2z"></path>
            <polyline points="9 22 9 12 15 12 15 22"></polyline>
          </svg>
          Shop Statistics
        </button>
        <button 
          :class="['switch-btn', { active: activeTab === 'companies' }]"
          @click="activeTab = 'companies'"
        >
          <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"></path>
            <circle cx="9" cy="7" r="4"></circle>
            <path d="M23 21v-2a4 4 0 0 0-3-3.87"></path>
            <path d="M16 3.13a4 4 0 0 1 0 7.75"></path>
          </svg>
          Company Statistics
        </button>
      </div>
    </div>

    <!-- Rādīt atbilstošo komponentu atkarībā no izvēlētas cilnes -->
    <ShopStatistics v-if="activeTab === 'shops'" />
    <CompanyStatistics v-else />
    </template>
  </div>
</template>

<script setup>
// Vue funkcijas
import { ref, onMounted, watch } from 'vue'

// Veikalu, Uzņēmumu un Uzņēmuma pārstavja statistikas komponenti
import ShopStatistics from '@/components/ShopStatistics.vue'
import CompanyStatistics from '@/components/CompanyStatistics.vue'
import CompanyRepresentativeStats from '@/components/CompanyRepresentativeStats.vue'

// Funkcija administratora lomas iegūšanai
import { getAdminRoleLevel } from '@/services/auth'

// Aktīvā cilne pēc noklusējuma
const activeTab = ref('shops')

// Droša loģika lomas līmeņa noteikšanai
// Neapstrādātas vērtības iegūšana un tās pārveidošana
// Ja vērtība nav pieejama, tad pārveido to par 0
const rawLevel = getAdminRoleLevel()
const roleLevel = ref(rawLevel ? Number(rawLevel) : 0);

// Izvēlētas cilnes atjaunošana, kad komponents ir ielādēts
onMounted(() => {
  const savedTab = localStorage.getItem('activeStatisticsTab')
  if (savedTab && (savedTab === 'shops' || savedTab === 'companies')) {
    activeTab.value = savedTab
  }
})

// Aktīvās cilnes novērošana un tās saglabāšana
watch(activeTab, (newTab) => {
  localStorage.setItem('activeStatisticsTab', newTab)
})
</script>


<style scoped>
/* Galvenais lapas satura konteineris */ 
.page-content {
  padding: 16px 24px;
  font-family: 'Inter', sans-serif;
  box-sizing: border-box;
  width: 100%;
}

/* Veikala statistikas bloks */ 
.shop-statistics-container {
  width: 100%;
  max-width: 1300px;
  box-sizing: border-box;
}

/* Augšējā daļa ar virsrakstu un statistikas pārslēgu */
.reports-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 20px;
}

.reports-header h1 {
  font-size: 48px;
  font-weight: 700;
  margin: 0;
  color: var(--color-black);
}

/* Pogu grupa pārslēgšanai starp statistikas sadaļām */
.statistics-switcher {
  display: flex;
  gap: 8px;
  background: var(--color-bg-light);
  padding: 6px;
  border-radius: 12px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.06);
}

/* Pārslēgšanas pogas stils */
.switch-btn {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 12px 24px;
  border: none;
  border-radius: 8px;
  background: transparent;
  color: var(--color-text-dim);
  font-family: 'Inter', sans-serif;
  font-size: 14px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.2s ease;
}

.switch-btn svg {
  transition: all 0.2s ease;
}

.switch-btn:hover {
  color: var(--brand-berry);
  background: rgba(161, 41, 113, 0.05);
}

/* Otrā līmeņa virsraksti */
.switch-btn.active {
  background: var(--brand-berry);
  color: var(--color-white);
  box-shadow: 0 2px 8px rgba(161, 41, 113, 0.3);
}

.page-content h2 {
  font-size: 24px;
  font-weight: 600;
  margin-bottom: 20px;
  color: var(--color-text-main);
}

/* Pielāgojumi planšetēm un mobilajām ierīcēm */
@media (max-width: 768px) {
  .reports-header {
    flex-direction: column;
    align-items: flex-start;
    gap: 16px;
  }

  .reports-header h1 {
    font-size: 32px;
  }

  .statistics-switcher {
    width: 100%;
  }

  .switch-btn {
    flex: 1;
    justify-content: center;
    font-size: 13px;
    padding: 10px 16px;
  }
}
</style>

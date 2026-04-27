<template>
    <div class="page-layout">
        <div class="shop-info-bar">
            <span v-if="shopCode"><b>Rimi {{shopType}}</b></span>
            <span v-if="shopAddress">{{ shopAddress }}</span>
        </div>
        <div class="page-container">
            <div class="hero">
                <p class="shift-type-label">{{ shiftType === 'start' ? t('common.startShiftType') : t('common.endShiftType') }}</p>
                <h1 class="hero-title">{{ t('companyTablet.chooseCompany') }}</h1>
            </div>
            <div class="search-container">
                <div class="search-bar">
                    <input
                        type="text"
                        v-model="query"
                        :placeholder="`${t('companyTablet.search')}...`"
                        class="search-input"
                        readonly
                        @focus="$event.target.blur()"
                    />
                    
                </div>
            </div>

            <div class="company-buttons">
                <button 
                    v-for="company in filteredCompanies"
                    :key="company.companyID"
                    class="company-btn"
                    @click="selectCompany(company)"
                >
                {{ company.companyName }}
            </button>
            </div>
           
            <button class="back-btn" @click="navigateTo('/shifts')">{{ t('common.back') }}</button>
        </div>

        <CompanyConfirm 
            :isOpen="confirmOpen"
            :companySelected="selectedCompany"
            @confirm="handleConfirm"
            @cancel="handleCancel"
        />

        <Keyboard v-model="queryLimited" />
    </div>
</template>

<script setup>
import { ref, onMounted, computed } from 'vue';
import { useRouter } from 'vue-router';
import { useShiftRegistration } from '@/composables/useShiftRegistration';
import CompanyConfirm from '../components/CompanyConfirm.vue';
import Keyboard from '../components/Keyboard.vue';
import { useI18n } from 'vue-i18n';
const { t } = useI18n();

const router = useRouter();
const { registrationData, setCompany, reset } = useShiftRegistration();

const shopCode = computed(() => registrationData.value.shopCode);
const shopType = computed(() => registrationData.value.shopType);
const shopAddress = computed(() => registrationData.value.shopAddress);

const selectedCompany = ref(null);
const companies = ref([]);
const API_BASE = 'http://localhost:5001/api';
const query = ref(''); 
const maxlength = 20; 
const confirmOpen = ref(false);

const queryLimited = computed({
    get: () => query.value, 
    set: (v) => {
        query.value = (v ?? '').slice(0, maxlength); 
    }
}); 

const shiftType = computed(() => registrationData.value.shiftType);

const filteredCompanies = computed(() => {
  if (!query.value.trim()) return companies.value;
  return companies.value.filter(company => 
    company.companyName.toLowerCase().includes(query.value.toLowerCase())
  );
});

const handleConfirm = () => {
  console.log('Company confirmed:', selectedCompany.value);
  setCompany(selectedCompany.value.companyID, selectedCompany.value.companyName);
  
  router.push('/reason');
  confirmOpen.value = false;
};

const handleCancel = () => {
  confirmOpen.value = false;
  selectedCompany.value = null;
};

const navigateTo = (path) => {
    router.push(path);
};

const fetchCompanies = async () => {
    try {
        const res = await fetch('http://localhost:5001/api/companies');
        companies.value = await res.json();
    } catch (error) {
        console.error('Failed to load companies:', error);
    }
};

const selectCompany = (company) => {
    selectedCompany.value = company;
    confirmOpen.value = true;
};

onMounted(() => {
    console.log('Verification - Registration Data:', registrationData.value);
    console.log('Verification - Shift Type:', shiftType.value);
    fetchCompanies();
});
</script>

<style scoped>
.page-layout {
    display: flex;
    align-items: center;
    justify-content: center;
    width: 100vw;
    height: 100vh;
    background: #e8e8e8;
    padding: 20px;
    box-sizing: border-box;
    position: fixed;
    top: 0;
    left: 0;
    right: 0;
    bottom: 0;
    margin: 0;
}
 
.page-container {
    background: white;
    border-radius: 20px;
    box-shadow: 0 4px 20px rgba(0, 0, 0, 0.1);
    position: relative;
    width: 500px;  /* FIXED WIDTH */
    height: 450px; /* FIXED HEIGHT */
    padding: 10px 40px;
    display: flex;
    flex-direction: column;
    align-items: center;
}

.hero {
    width: 100%;
    flex-shrink: 0;
}

.hero-title {
    text-align: left;
    font-family: 'Inter';
    font-size: 32px;
    font-weight: 700;
    color: #333;
    margin: 10px 0 15px 0;
}

.search-container {
    display: flex;
    justify-content: center;
    align-items: center;
    width: 100%;
    background-color: none;
    flex-shrink: 0;
}

.search-bar {
    display: flex;
    gap: 8px;
    padding: 8px;
    border-bottom: 1px solid rgba(150, 154, 167, 0.3);
    color: #e5dee0;
    background: white;
    width: 100%;
}

.search-input {
    font-family: 'Inter';
    flex: 1;
    border: none;
    padding: 6px;
    outline: none;
    color: #969AA7;
    font-size: 15px;
    border-radius: 20px;
    cursor: pointer;
}

.search-input::placeholder {
    color: #969AA7;
    opacity: 1;
}

.company-buttons {
    display: flex;
    flex-wrap: wrap;
    gap: 18px;
    justify-content: flex-start;
    margin-top: 15px;
    height: 280px; /* FIXED HEIGHT for scrollable area */
    overflow-y: auto;
    overflow-x: hidden;
    padding-top: 10px;
    padding-left: 5px;
    width: 100%;
}

.company-buttons::-webkit-scrollbar {
    width: 3px;
}

.company-buttons::-webkit-scrollbar-track {
    background: transparent;
}

.company-buttons::-webkit-scrollbar-thumb {
    background: rgba(43, 164, 146, 0.4);
    border-radius: 4px;
}

.company-buttons::-webkit-scrollbar-thumb:hover {
  background-color: rgba(43, 164, 146, 0.9);
}

.company-btn {
    width: 110px;
    height: 110px;
    background: linear-gradient(to bottom right, var(--brand-berry), var(--brand-berry-light));
    color: white;
    border: none;
    border-radius: 16px;
    padding: 10px 18px;
    font-family: 'Inter';
    font-size: 12px;
    font-weight: 600;
    cursor: pointer;
    transition: all 0.5s ease;
    box-shadow: 0 1px 20px #47103059;
    flex-shrink: 0;
}

.company-btn:hover {
    background: linear-gradient(to bottom right, var(--brand-berry-light), var(--brand-berry));
    transform: translateY(-2px);
}

.company-btn:active {
    background: var(--brand-berry-light);
    transform: translateY(-2px);
}

.back-btn {
  position: absolute;
  top: 15px;
  right: 20px;
  background: transparent;
  border: none;
  color: #a12971;
  font-family: 'Inter';
  font-size: 22px;
  font-weight: bold;
  cursor: pointer;
  transition: all 0.2s ease;
}

.back-btn:hover {
  color: #7d1f56;
  font-weight: 600;
}

/* Responsive */
@media (max-width: 768px) {
    .page-container {
        width: 450px;
        height: 420px;
        padding: 10px 35px;
    }
 
    .hero-title {
        font-size: 28px;
    }
    
    .company-btn {
        width: 100px;
        height: 100px;
        font-size: 11px;
    }
}
</style>

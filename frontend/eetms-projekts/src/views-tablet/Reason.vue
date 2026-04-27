<template>
    <div class="page-layout">
        <div class="shop-info-bar">
            <span v-if="shopCode"><b>Rimi {{shopType}}</b></span>
            <span v-if="shopAddress">{{ shopAddress }}</span>
        </div>
        <div class="page-container">
            <div class="hero">
                <p class="shift-type-label">{{ shiftType === 'start' ? t('common.startShiftType') : t('common.endShiftType') }}</p>
                <p class="company-label">{{t('common.company')}}: {{ companyName }}</p>
                <h1 class="hero-title">{{ t('reason.reasonChoose') }}</h1>
            </div>

            <div class="reason-buttons">
                <button 
                    v-for="reason in reasons"
                    :key="reason.reasonID"
                    class="reason-btn"
                    @click="selectReason(reason)"
                >
                    {{ reason.name }}
                </button>
            </div>
           
            <button class="back-btn" @click="navigateTo('/verification')">{{ t('common.back') }}</button>
        </div>
    </div>
</template>

<script setup>
import { ref, onMounted, computed } from 'vue';
import { useRouter } from 'vue-router';
import { useShiftRegistration } from '@/composables/useShiftRegistration';
import { useI18n } from 'vue-i18n';
const { t } = useI18n();

const router = useRouter();
const { registrationData, setReason, setCompanyReasonId, reset } = useShiftRegistration();

const selectedReason = ref(null);
const reasons = ref([]);

const shiftType = computed(() => registrationData.value.shiftType);
const companyId = computed(() => registrationData.value.companyId);
const companyName = computed(() => registrationData.value.companyName);

const shopCode = computed(() => registrationData.value.shopCode);
const shopType = computed(() => registrationData.value.shopType);
const shopAddress = computed(() => registrationData.value.shopAddress);

const fetchReasons = async () => {
    console.log('Fetching reasons for company ID:', companyId.value);
    
    if (!companyId.value) {
        console.error('No company ID provided!');
        return;
    }
    
    try {
        const url = `http://localhost:5001/api/reasons/bycompany/${companyId.value}`;
        const res = await fetch(url);
        
        if (!res.ok) {
            console.error('API response not OK:', res.status, res.statusText);
            return;
        }
        
        const data = await res.json();
        console.log('Fetched reasons:', data);
        reasons.value = data;
    } catch (error) {
        console.error('Failed to load reasons:', error);
    }
};

const selectReason = async (reason) => {
    setReason(reason.reasonID, reason.name);
    
    try {
        // Get CompanyReasonID from junction table
        const response = await fetch(
            `http://localhost:5001/api/shifts/companyreason?companyId=${companyId.value}&reasonId=${reason.reasonID}`
        );
        
        if (!response.ok) throw new Error('Failed to get CompanyReasonID');
        
        const data = await response.json();
        setCompanyReasonId(data.companyReasonID);
        
        
        router.push('/phone');
        
    } catch (error) {
        console.error('Error:', error);
        alert('Failed to save reason. Please try again.');
    }
};

const navigateTo = (path) => {
    router.push(path);
};

onMounted(() => {
    console.log('Reason - Registration Data:', registrationData.value);
    fetchReasons();
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
    width: 800px;
    height: 450px;
    padding: 10px 60px;
    max-height: 500px;
    display: flex;
    flex-direction: column;
    align-items: center;
}

.hero {
    width: 100%;
}

.company-label {
    font-family: 'Inter';
    font-size: 16px;
    font-weight: 500;
    color: #666;
    margin: 0;
    margin-bottom: 10px;
}

.hero-title {
    text-align: center;
    font-family: 'Inter';
    font-size: 40px;
    font-weight: 700;
    color: #333;
    padding: none;
}

.reason-buttons {
    display: flex;
    flex-wrap: wrap;
    gap: 30px;
    justify-content: left;
    margin-top: 20px;
    max-height: 800px;
    overflow-y: auto;
    overflow-x: hidden;
    padding-top: 20px;
    padding-left: 10px;
}

.reason-buttons::-webkit-scrollbar {
    width: 3px;
}

.reason-buttons::-webkit-scrollbar-track {
    background: transparent;
}

.reason-buttons::-webkit-scrollbar-thumb {
    background: rgba(43, 164, 146, 0.4);
    border-radius: 4px;
}

.reason-buttons::-webkit-scrollbar-thumb:hover {
    background-color: rgba(43, 164, 146, 0.9);
}

.reason-btn {
    width: 170px;
    height: 170px;
    background: linear-gradient(to bottom right, #a12971, #762554);
    color: white;
    border: none;
    border-radius: 25px;
    padding: 15px 30px;
    font-family: 'Inter';
    font-size: 14px;
    font-weight: 600;
    cursor: pointer;
    transition: all 0.5s ease;
    box-shadow: 0 1px 20px #47103059;
}

.reason-btn:hover {
    background: linear-gradient(to bottom right, #762554, #a12971);
    transform: translateY(-2px);
}

.reason-btn:active {
    background: linear-gradient(to bottom right, #762554, #a12971);
    transform: translateY(-2px);
}

.back-btn {
    position: absolute;
    top: 20px;
    right: 25px;
    background: transparent;
    border: none;
    color: #a12971;
    font-family: 'Inter';
    font-size: 25px;
    font-weight: bold;
    cursor: pointer;
    transition: all 0.2s ease;
}

.back-btn:hover {
    color: #7d1f56;
    font-weight: 600;
}

.search-container {
    display: flex;
    justify-content: center;
    align-items: center;
    width: 100%;
    background-color: none;
}

.search-bar {
    display: flex;
    gap: 8px;
    padding: 10px;
    border-bottom: 1px solid rgba(150, 154, 167, 0.3);
    color: #e5dee0;
    background: white;
    width: 800px;
}

.search-input {
    font-family: 'Inter';
    flex: 1;
    border: none;
    padding: 8px;
    outline: none;
    color: #969AA7;
    font-size: 16px;
    border-radius: 20px;
}

.search-input::placeholder {
    color: #969AA7;
    opacity: 1;
}

/* Responsive */
@media (max-width: 768px) {
    .page-container {
        padding: 40px 50px 60px;
    }

    .hero-title {
        font-size: 32px;
        margin-bottom: 40px;
    }
}

@media (max-width: 600px) {
    .page-container {
        padding: 35px 40px 55px;
    }

    .hero-title {
        font-size: 26px;
    }
}

@media (max-width: 480px) {
    .page-container {
        padding: 30px 35px 50px;
    }

    .hero-title {
        font-size: 22px;
    }
}
</style>

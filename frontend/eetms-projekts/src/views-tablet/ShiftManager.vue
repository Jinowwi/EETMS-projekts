<template>
  <div class="shift-manager">

    <div class="shop-info-bar">
      <span v-if="shopCode"><b>Rimi {{shopType}}</b></span>
      <span v-if="shopAddress">{{ shopAddress }}</span>
    </div>

    <div class="shift-container">
      <LanguageSwitcher />
      <h2 class="choose-title">{{ t('shiftManagerTablet.choose') }}</h2>
      
      <div class="shift-buttons">
        <button class="shift-btn end-shift" @click="selectShift('end')">
          {{ t('shiftManagerTablet.endShift') }}
        </button>

        <button class="shift-btn start-shift" @click="selectShift('start')">
          {{ t('shiftManagerTablet.startShift') }}
        </button>
      </div>

      <button class="back-btn" @click="navigateTo('/hometab')">{{ t('common.back') }}</button>
    </div>
  </div>
</template>

<script setup>
import { computed } from 'vue';
import { useRouter } from 'vue-router';
import { useShiftRegistration } from '@/composables/useShiftRegistration';
import { useI18n } from 'vue-i18n';
import LanguageSwitcher from '../components/LanguageSwitcher.vue'
const { t } = useI18n();

const router = useRouter();
const { registrationData, setShiftType, reset } = useShiftRegistration();

const shopCode = computed(() => registrationData.value.shopCode);
const shopType = computed(() => registrationData.value.shopType);
const shopAddress = computed(() => registrationData.value.shopAddress);

const selectShift = (type) => {
  setShiftType(type);
  
  if (type === 'end') {
    router.push('/phone'); 
  } else {
    router.push('/verification');
  }
};

const navigateTo = (path) => {
  reset();
  router.push(path);
};
</script>


<style scoped>
.shift-manager {
  display: flex;
  align-items: center;
  justify-content: center;
  min-height: 100vh;
  height: 100vh;
  background: #e8e8e8;
  padding: 0;
  margin: 0;
  position: fixed;  
  top: 0;           
  left: 0;          
  right: 0;         
  bottom: 0;
  overflow: hidden;
}

.shift-container {
  background: white;
  border-radius: 20px;
  padding: 50px 60px;
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.1);
  position: relative;
  min-width: 750px;
  max-width: 800px;
  max-height: 500px;
}

.choose-title {
  text-align: center;
  font-family: 'Inter';
  font-size: 32px;
  font-weight: 700;
  color: #333;
  margin: 0 0 50px 0;
}

.shift-buttons {
  display: flex;
  gap: 20px;
  justify-content: center;
  margin-bottom: 40px;
}

.shift-btn {
  width: 290px;
  height: 290px;
  border: none;
  border-radius: 20px;
  font-family: 'Inter';
  font-size: 36px;
  font-weight: 700;
  color: white;
  cursor: pointer;
  transition: all 0.3s ease;
  box-shadow: 0 8px 20px rgba(0, 0, 0, 0.15);
}

.shift-btn:hover {
  transform: translateY(-5px);
  box-shadow: 0 15px 35px rgba(0, 0, 0, 0.25);
}

.shift-btn:active {
  transform: translateY(-2px);
}

.end-shift {
  background: linear-gradient(135deg, #ff6b6b, #ee5a6f);
}

.end-shift:active {
  background: linear-gradient(135deg, #e55a5a, #dd4a5a);
  transform: translateY(-5px);
}

.start-shift {
  background: linear-gradient(135deg, #51cf66, #37b24d);
}

.start-shift:active {
  background: linear-gradient(135deg, #45b85a, #2fa43f);
  transform: translateY(-5px);
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

/* Responsive */
@media (max-width: 768px) {
  .shift-container {
    padding: 40px 50px;
    min-width: 550px;
  }

  .choose-title {
    font-size: 28px;
    margin-bottom: 40px;
  }

  .shift-btn {
    width: 220px;
    height: 150px;
    font-size: 30px;
  }

  .shift-buttons {
    gap: 30px;
  }
}

@media (max-width: 480px) {
  .shift-container {
    padding: 35px 40px;
    min-width: 350px;
  }

  .choose-title {
    font-size: 24px;
    margin-bottom: 35px;
  }

  .shift-btn {
    width: 140px;
    height: 110px;
    font-size: 22px;
  }

  .shift-buttons {
    gap: 20px;
  }
}
</style>

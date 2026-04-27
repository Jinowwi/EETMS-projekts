<template>
  <div class="tablet-layout">
    <div class="tablet-container" @click="handleTap">
      <div class="hero">
        <h2 class="hero-title">{{ t('homeTablet.welcome') }}</h2>
        <p class="hero-sub">{{ t('homeTablet.tapToStart') }}</p>
      </div>

      <div class="brand-logo">
        <img src="/assets/rimi.svg" alt="Rimi"/>
      </div>

      <div class="time">
        <p>{{ currentTime }}</p>
      </div>

    <div class="bottom-row" @click.stop>
        <div class="shop-select-wrapper">
            <label class="shop-label">Select shop</label>
            <select v-model="selectedShopId" class="shop-select">
            <option disabled value="">-- Choose shop --</option>
            <option
                v-for="shop in shops"
                :key="shop.shopID"
                :value="shop.shopID"
            >
                {{ shop.code }}
            </option>
            </select>

            <p v-if="shopError" class="shop-error">{{ shopError }}</p>
        </div>

        <button class="toggle-btn" @click.stop="navigateTo('/home')">
            Toggle to Desktop Version
        </button>
    </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, onBeforeUnmount, computed } from 'vue';
import { useRouter } from 'vue-router';
import { useShiftRegistration } from '@/composables/useShiftRegistration';
import { useI18n } from 'vue-i18n'
const { t } = useI18n()


const router = useRouter();
const { reset, setShop, registrationData } = useShiftRegistration();

const currentTime = ref('');
let timer = null;

const shops = ref([]);
const selectedShopId = ref('');
const shopError = ref('');

const API_BASE = 'http://localhost:5001/api';

const navigateTo = (path) => {
  router.push(path);
};

const updateTime = () => {
  const now = new Date();
  currentTime.value = now.toLocaleTimeString('en-US', {
    hour: '2-digit',
    minute: '2-digit',
    second: '2-digit',
    hour12: false
  });
};

const fetchShops = async () => {
  try {
    const res = await fetch(`${API_BASE}/shops`);
    if (!res.ok) throw new Error('Failed to load shops');
    shops.value = await res.json(); // expect { shopID, code, name }
  } catch (e) {
    console.error('Failed to load shops:', e);
    shopError.value = 'Failed to load shops.';
  }
};

const handleTap = () => {
  if (!selectedShopId.value) {
    shopError.value = 'Please select a shop before starting.';
    return;
  }

  shopError.value = '';

  const shop = shops.value.find(s => s.shopID === Number(selectedShopId.value));
  if (!shop) {
    console.error('Shop not found for id', selectedShopId.value, shops.value);
    return;
  }

  console.log('shop object:', JSON.stringify(shop));

  setShop(shop.shopID, shop.code, shop.type, shop.address);
  console.log('After setShop:', registrationData.value);  // debug

  const tappedAt = new Date().toISOString();
  router.push({ path: '/shifts', query: { tappedAt } });
};

const shopCode = computed(() => registrationData.value.shopCode);
const shopType = computed(() => registrationData.value.shopType);
const shopAddress = computed(() => registrationData.value.shopAddress);

onMounted(() => {
  reset();
  fetchShops();
  updateTime();
  timer = setInterval(updateTime, 1000);
});

onBeforeUnmount(() => {
  clearInterval(timer);
});
</script>

<style scoped>
.tablet-layout {
    display: flex;
    align-items: center;
    justify-content: center;
    min-height: 100vh;
    height: 100vh;
    background: #e8e8e8;
    padding: 20px;
    margin: 0;
    position: fixed;  
    top: 0;           
    left: 0;          
    right: 0;         
    bottom: 0;
    overflow: hidden;
    box-sizing: border-box;
}

.tablet-container {
    background: white;
    border-radius: 20px;
    padding: 50px 60px;
    box-shadow: 0 4px 20px rgba(0, 0, 0, 0.1);
    display: flex; 
    flex-direction: column;
    align-items: center;
    text-align: center;
    justify-content: center;
    gap: 40px;
    font-family: 'Inter';
    position: relative;
    cursor: pointer;
    transition: transform 0.2s ease;
    min-width: 1000px;
    max-height: 500px;
}

.hero-title {
    font-size: 50px;
    margin: 0;
    font-weight: 700;
    color: black;
}

.hero-sub {
    font-size: 30px;
    margin: 10px 0 0 0;
    color: #666;
}

.brand-logo img {
    width: 500px;
    max-width: 100%;
    height: auto;
}

.time {
    font-size: 70px;
    font-weight: bold;
    color: #333;
    margin: 0;
}

.time p {
    margin: 0;
}

.toggle-btn {
    border-radius: 40px;
    font-family: 'Inter';
    border: none;
    font-size: 24px;
    padding: 12px 50px;
    cursor: pointer;
    transition: all 0.3s ease;
    font-weight: 500;
}

.toggle-btn:hover {
    opacity: 0.9;
}

.shop-select-wrapper {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 6px;
}

.shop-label {
  font-family: 'Inter';
  font-size: 18px;
  font-weight: 600;
  color: #333;
}

.shop-select {
  min-width: 260px;
  padding: 8px 12px;
  border-radius: 999px;
  border: 1.5px solid #333;
  font-family: 'Inter';
  font-size: 16px;
  outline: none;
}

.shop-select:focus {
  border-color: #a12971;
}

.shop-error {
  margin: 0;
  font-family: 'Inter';
  font-size: 14px;
  color: #e5484d;
}

.bottom-row {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 24px;
  width: 100%;
  max-width: 700px;
}

.shop-select-wrapper {
  display: flex;
  flex-direction: column;
  align-items: flex-start;
  gap: 6px;
  flex: 1;
}

.toggle-btn {
  border-radius: 40px;
  font-family: 'Inter';
  border: none;
  font-size: 18px;
  padding: 10px 26px;
  cursor: pointer;
  transition: all 0.3s ease;
  font-weight: 500;
  white-space: nowrap;
}


@media screen and (max-width: 768px) {
    .tablet-container {
        gap: 30px;
        padding: 40px 50px;
    }
    
    .hero-title {
        font-size: 36px;
    }
    
    .hero-sub {
        font-size: 22px;
    }
    
    .brand-logo img {
        width: 350px;
    }
    
    .time {
        font-size: 50px;
    }
    
    .toggle-btn {
        font-size: 14px;
        padding: 8px 16px;
    }
}

@media screen and (max-width: 600px) {
    .tablet-container {
        gap: 25px;
        padding: 35px 40px;
    }
    
    .hero-title {
        font-size: 28px;
    }
    
    .hero-sub {
        font-size: 18px;
    }
    
    .brand-logo img {
        width: 280px;
    }
    
    .time {
        font-size: 40px;
    }
    
    .toggle-btn {
        font-size: 13px;
        padding: 8px 14px;
    }
}

@media screen and (max-width: 480px) {
    .tablet-container {
        gap: 20px;
        padding: 30px 35px;
    }
    
    .hero-title {
        font-size: 24px;
    }
    
    .hero-sub {
        font-size: 16px;
    }
    
    .brand-logo img {
        width: 220px;
    }
    
    .time {
        font-size: 32px;
    }
}
</style>
<template>
    <div class="page-layout">
        <div class="shop-info-bar">
            <span v-if="shopCode"><b>Rimi {{shopType}}</b></span>
            <span v-if="shopAddress">{{ shopAddress }}</span>
        </div>
        <div class="layout-content">
            <div class="form-container">
                <div class="top-section">
                    <div class="info-section">
                        <p class="shift-type-label">{{ shiftType === 'start' ? t('common.startShiftType') : t('common.endShiftType') }}</p>
                        <p class="company-label">{{ t('common.company') }}: {{ companyName }}</p>
                        <p class="reason-label">{{ t('common.reason') }}: {{ reasonName }}</p>
                    </div>
                    <button class="back-btn" @click="navigateTo('/phone')">{{ t('common.back') }}</button>
                </div>

                <h1>Enter verification code</h1>

                <div class="form-group">
                    <input 
                        type="text"
                        v-model="code"
                        placeholder=""
                        class="form-input"
                        readonly
                    />
                </div>

                <div class="button-container">
                    <button class="btn-submit" @click="handleSubmit">{{ t('common.submit') }}</button>
                </div>
            </div>

            <NumericKeypad 
                @number-click="handleNumberClick"
                @backspace="handleBackspace"
            />
        </div>
    </div>
</template>

<script setup>
import { ref, computed } from 'vue';
import { useRouter } from 'vue-router';
import { useShiftRegistration } from '@/composables/useShiftRegistration';
import NumericKeypad from '../components/NumericKeypad.vue';
import { useI18n } from 'vue-i18n';
const { t } = useI18n();

const router = useRouter();
const { registrationData, setVerificationCode, setShiftId } = useShiftRegistration();

const API_BASE = 'http://localhost:5001/api';

const shopCode = computed(() => registrationData.value.shopCode);
const shopType = computed(() => registrationData.value.shopType);
const shopAddress = computed(() => registrationData.value.shopAddress);

const code = ref('');
const isSubmitting = ref(false);

const shiftType = computed(() => registrationData.value.shiftType);
const shiftId = computed(() => registrationData.value.shiftId);
const companyName = computed(() => registrationData.value.companyName);
const reasonName = computed(() => registrationData.value.reasonName);
const phoneNumber = computed(() => registrationData.value.phoneNumber);

const handleNumberClick = (num) => {
  code.value += num;
};

const handleBackspace = () => {
  code.value = code.value.slice(0, -1);
};

const handleSubmit = async () => {
  if (isSubmitting.value) return;
  if (!code.value) {
    alert('Please enter verification code.');
    return;
  }

  isSubmitting.value = true;
  setVerificationCode(code.value);

  try {
    const verifyRes = await fetch(`${API_BASE}/sms/verify`, {
        method: 'POST', 
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
            phoneNumber: phoneNumber.value,
            otp: code.value
        })
    }); 

    if (!verifyRes.ok) {
        const err = await verifyRes.json(); 
        if (err.error === 'expired') {
            alert('Code has expired. Request a new one.');
        } else {
            alert('Incorrect code.')
            code.value = ''; 
        }
        return; 
    }

    if (shiftType.value === 'start') {
      const startDateTime = new Date(registrationData.value.startDate);

      const shiftData = {
        StartDate: startDateTime.toISOString().split('T')[0],
        StartTime: startDateTime.toTimeString().split(' ')[0],
        CompanyReasonID: registrationData.value.companyReasonId,
        ShopID: registrationData.value.shopId,
        employee_phone_number: registrationData.value.phoneNumber,
        verification_code: code.value
      };

      const response = await fetch(`${API_BASE}/shifts`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(shiftData)
      });

      if (!response.ok) {
        const err = await response.text();
        console.error('Backend error:', err);
        alert(err || 'Failed to create shift.');
        return;
      }

      const created = await response.json();
      setShiftId(created.shiftID);

      router.push('/successful');
      return;
    }

    // shiftType === 'end'
    const response = await fetch(`${API_BASE}/shifts/${shiftId.value}/end`, {
      method: 'PATCH',
      headers: { 'Content-Type': 'application/json' }
    });

    if (!response.ok) {
      const err = await response.text();
      console.error('Backend error:', err);
      alert(err || 'Failed to end shift.');
      return;
    }

    router.push('/successful');
  } catch (e) {
    console.error(e);
    alert('Request failed. Please try again.');
  } finally {
    isSubmitting.value = false;
  }
};

const navigateTo = (path) => {
  router.push(path);
};
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

.layout-content {
    display: flex;
    gap: 30px;
    align-items: stretch;
}

.form-container {
    background: white;
    border-radius: 20px;
    padding: 40px 50px;
    box-shadow: 0 4px 20px rgba(0, 0, 0, 0.1);
    display: flex;
    flex-direction: column;
    justify-content: center;
    min-width: 320px;
}

.top-section {
    display: flex;
    flex-direction: row;
    align-items: center;
    justify-content: space-between;
    margin-bottom: 20px;
    padding-bottom: 15px;
    border-bottom: 1px solid #e0e0e0;
}

.info-section {
    margin-bottom: 0;
    padding-bottom: 0;
    border-bottom: none;
}

.shift-type-label {
    font-family: 'Inter';
    font-size: 16px;
    font-weight: 600;
    color: #a12971;
    margin: 0 0 5px 0;
}

.company-label,
.reason-label,
.phone-label {
    font-family: 'Inter';
    font-size: 14px;
    font-weight: 500;
    color: #666;
    margin: 0 0 3px 0;
}

h1 {
    text-align: center;
    font-family: 'Inter';
    font-size: 28px;
    font-weight: 700;
    color: #333;
    margin: 0 0 40px 0;
    line-height: 1.3;
}

.form-group {
    width: 100%;
    margin-bottom: 30px;
}

.form-input {
    width: 100%;
    height: 50px;
    border-radius: 25px;
    border: 1.5px solid #333;
    padding: 0 20px;
    font-family: 'Inter';
    font-size: 16px;
    color: #333;
    outline: none;
    transition: border-color 0.3s ease;
    box-sizing: border-box;
    background: white;
}

.form-input:focus {
    border-color: #a12971;
    border-width: 3px;
}

.button-container {
    width: 100%;
    display: flex;
    justify-content: center;
}

button {
    border-radius: 40px;
    font-family: 'Inter';
    border: none;
    font-size: 20px;
    padding: 12px 45px;
    cursor: pointer;
    transition: all 0.3s ease;
    font-weight: 500;
}

.btn-submit {
    background-color: #2ab492;
    color: white;
    box-shadow: 0 4px 10px rgba(42, 180, 146, 0.3);
}

.btn-submit:hover {
    background-color: #239f7f;
    transform: translateY(-2px);
    box-shadow: 0 6px 15px rgba(42, 180, 146, 0.4);
}

button:active {
    transform: translateY(0);
}

.back-btn {
    background: transparent;
    border: none;
    color: #a12971;
    font-family: 'Inter';
    font-size: 25px;
    font-weight: bold;
    cursor: pointer;
    padding: 0;
    flex-shrink: 0;
    align-self: flex-start;
    box-shadow: none;
}

.back-btn:hover {
    background: transparent;
    color: #1a7d63;
    transform: none;
    box-shadow: none;
    font-weight: 600;
}

@media (max-width: 768px) {
    .layout-content {
        flex-direction: column;
        gap: 20px;
    }
    
    .form-container {
        min-width: auto;
        width: 100%;
    }
    
    h1 {
        font-size: 24px;
    }
}
</style>

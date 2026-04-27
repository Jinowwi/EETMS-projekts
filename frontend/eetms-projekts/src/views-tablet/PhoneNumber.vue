<template>
    <div class="page-layout">
        <div class="shop-info-bar">
            <span v-if="shopCode"><b>Rimi {{shopType}}</b></span>
            <span v-if="shopAddress">{{ shopAddress }}</span>
        </div>
        <div class="layout-content">
            <div class="form-container">
                <div class="info-section">
                    <p class="shift-type-label">{{ shiftType === 'start' ? t('common.startShiftType') : t('common.endShiftType') }}</p>
                    <p v-if="shiftType === 'start' && companyName" class="company-label">{{t('common.company')}}: {{ companyName }}</p>
                    <p v-if="shiftType === 'start' && reasonName" class="reason-label">{{t('common.reason')}}: {{ reasonName }}</p>
                </div>
                
                <h1>{{ t('phoneNumber.enterPhone') }}</h1>
                
                <div class="form-group">
                    <div class="phone-input-wrapper">
                        <div class="select-wrapper" :class="{ 'is-open': isDropdownOpen }">
                            <select 
                                v-model="countryCode" 
                                class="country-code-select"
                                @focus="isDropdownOpen = true"
                                @blur="isDropdownOpen = false"
                            >
                                <option value="+370">+370</option>
                                <option value="+371">+371</option>
                                <option value="+372">+372</option>
                            </select>
                        </div>
                        <input 
                            type="text"
                            v-model="code"
                            placeholder=""
                            class="form-input"
                            readonly
                        />
                    </div>
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
        
        <button class="back-btn" @click="navigateTo('/reason')">{{ t('common.back') }}</button>
    </div>
</template>

<script setup>
import { ref, computed, watchEffect } from 'vue';
import { useRouter } from 'vue-router';
import { useShiftRegistration } from '@/composables/useShiftRegistration';
import NumericKeypad from '../components/NumericKeypad.vue';
import { useI18n } from 'vue-i18n';
const { t } = useI18n();

const router = useRouter();
const { registrationData, setPhoneNumber, setShiftId, setOngoingShift, isReadyForDatabase } = useShiftRegistration();

const code = ref('');
const isDropdownOpen = ref(false);
const isSubmitting = ref(false);

const countryCodeMap = {
  1: '+370', // Lithuania
  2: '+371', // Latvia
  3: '+372', // Estonia
};

const countryCode = ref('+370');

watchEffect(() => {
  const code = registrationData.value.shopCode || '';
  if (code.startsWith('LV')) countryCode.value = '+371';
  else if (code.startsWith('EE')) countryCode.value = '+372';
  else countryCode.value = '+370'; // default LT
});

const shiftType = computed(() => registrationData.value.shiftType);
const companyName = computed(() => registrationData.value.companyName);
const reasonName = computed(() => registrationData.value.reasonName);
const shopCode = computed(() => registrationData.value.shopCode);
const shopType = computed(() => registrationData.value.shopType);
const shopAddress = computed(() => registrationData.value.shopAddress);

const API_BASE = 'http://localhost:5001/api';

const handleNumberClick = (num) => { code.value += num; };
const handleBackspace = () => { code.value = code.value.slice(0, -1); };

const handleSubmit = async () => {
    if (isSubmitting.value) return;
    if (!code.value) { 
        alert('Please enter your phone number.'); 
        return; 
    }

    // Phone number validation
    const digitsOnly = code.value.replace(/\D/g, '');
    
    const lengthRules = {
        '+370': { min: 8, max: 8 },
        '+371': { min: 8, max: 8 },
        '+372': { min: 7, max: 8 },
    };
    
    const rule = lengthRules[countryCode.value];
    
    if (!rule || !/^\d+$/.test(code.value)) {
        alert('Phone number can only contain digits.');
        return;
    }
    if (digitsOnly.length < rule.min) {
        alert(`Phone number is too short for ${countryCode.value}. Expected ${rule.min} digits.`);
        return;
    }
    if (digitsOnly.length > rule.max) {
        alert(`Phone number is too long for ${countryCode.value}. Expected ${rule.max} digits.`);
        return;
    }

    if (/^(\d)\1+$/.test(digitsOnly)) {
        alert('Please enter a valid phone number.');
        return;
    }

    // Check for sequential digits (e.g. 12345678, 87654321)
    const ascending  = '0123456789';
    const descending = '9876543210';
    if (ascending.includes(digitsOnly) || descending.includes(digitsOnly)) {
        alert('Please enter a valid phone number.');
        return;
    }

    // Check number doesn't start with 0 (no valid Baltic mobile starts with 0)
    if (digitsOnly.startsWith('0')) {
        alert('Phone number cannot start with 0.');
        return;
    }

    isSubmitting.value = true;
    const fullPhoneNumber = countryCode.value + code.value;
    setPhoneNumber(fullPhoneNumber);
    
    if (shiftType.value === 'end') {
        await handleEndShift(fullPhoneNumber);
    } else {
        await handleStartShift(fullPhoneNumber);
    }
    isSubmitting.value = false;
};

const handleEndShift = async (fullPhoneNumber) => {
    try {
        const response = await fetch(`${API_BASE}/shifts/find-ongoing`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                phoneNumber: fullPhoneNumber,
                shopId: registrationData.value.shopId
            })
        });
        if (!response.ok) {
            const errorData = await response.json();
            console.error('Backend error:', errorData);
            alert('No ongoing shift found. You either pressed the wrong button or forgot to register the start of the shift.');
            return;
        }
        const ongoingShift = await response.json();
        console.log('Found ongoing shift:', ongoingShift);
        setOngoingShift(ongoingShift);

        const smsRes = await fetch(`${API_BASE}/sms/send`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ phoneNumber: fullPhoneNumber })
        });
        if (!smsRes.ok) {
            const err = await smsRes.json();
            alert(err.error || 'Failed to send verification code.');
            return;
        }
        router.push('/code');
    } catch (error) {
        console.error('Error finding ongoing shift:', error);
        alert('Failed to find ongoing shift. Please try again.');
    }
};

const handleStartShift = async (phoneNumber) => {
    if (!isReadyForDatabase()) {
        alert('Missing required information. Please start over.');
        return;
    }
    try {
        const response = await fetch(`${API_BASE}/sms/send`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ phoneNumber })
        });
        if (!response.ok) {
            const err = await response.json();
            alert(err.error || 'Failed to send verification code.');
            return;
        }
        router.push('/code');
    } catch (error) {
        alert('Failed to send verification code. Please try again.');
    }
};

const navigateTo = () => {
  if (shiftType.value === 'end') {
    router.push('/shifts');
  } else {
    router.push('/reason');
  }
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

.info-section {
    margin-bottom: 20px;
    padding-bottom: 15px;
    border-bottom: 1px solid #e0e0e0;
}

.shift-type-label {
    font-family: 'Inter';
    font-size: 16px;
    font-weight: 600;
    color: #a12971;
    margin: 0 0 5px 0;
}

.company-label {
    font-family: 'Inter';
    font-size: 14px;
    font-weight: 500;
    color: #666;
    margin: 0 0 3px 0;
}

.reason-label {
    font-family: 'Inter';
    font-size: 14px;
    font-weight: 500;
    color: #666;
    margin: 0;
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

.phone-input-wrapper {
    display: flex;
    gap: 10px;
    align-items: center;
}

.select-wrapper {
    position: relative;
    flex-shrink: 0;
}

.select-wrapper::after {
    content: "";
    position: absolute;
    right: 15px;
    top: 50%;
    transform: translateY(-50%);
    width: 0;
    height: 0;
    border-left: 4px solid transparent;
    border-right: 4px solid transparent;
    border-top: 5px solid #333;
    pointer-events: none;
    transition: transform 0.3s ease;
}

.select-wrapper.is-open::after {
    transform: translateY(-50%) rotate(180deg);
}

.country-code-select {
    height: 50px;
    border-radius: 25px;
    border: 1.5px solid #333;
    padding: 0 0 0 15px;
    font-family: 'Inter';
    font-size: 16px;
    color: #333;
    outline: none;
    transition: border-color 0.3s ease;
    background: white;
    cursor: pointer;
    width: 90px;
    -webkit-appearance: none;
    -moz-appearance: none;
    appearance: none;
}

.country-code-select:focus {
    border-color: #a12971;
    border-width: 2px;
}

.form-input {
    flex: 1;
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
    border-width: 2px;
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
    
    .phone-input-wrapper {
        gap: 8px;
    }
    
    .country-code-select {
        width: 85px;
    }
}
</style>

import { ref, watch } from 'vue';

const STORAGE_KEY = 'shift_registration_data';
const TIMEOUT_DURATION = 1800000; // 1 minute in milliseconds

const registrationData = ref(
  JSON.parse(localStorage.getItem(STORAGE_KEY) || '{}')
);

let timeoutId = null;

watch(registrationData, (newData) => {
  localStorage.setItem(STORAGE_KEY, JSON.stringify(newData));
}, { deep: true });

export function useShiftRegistration() {
  const resetTimeout = () => {
    if (timeoutId) {
      clearTimeout(timeoutId);
    }
    
    timeoutId = setTimeout(() => {
      console.log('Session timeout - clearing data');
      reset();
      // Optionally redirect to home
      window.location.href = '/hometab';
    }, TIMEOUT_DURATION);
  };

  const setShiftType = (type) => {
    registrationData.value.shiftType = type;
    registrationData.value.startDate = new Date().toISOString();
    registrationData.value.startTime = new Date().toISOString();
    registrationData.value.lastActivity = Date.now();
    resetTimeout();
  };

  const setCompany = (companyId, companyName) => {
    registrationData.value.companyId = companyId;
    registrationData.value.companyName = companyName;
    registrationData.value.lastActivity = Date.now();
    resetTimeout();
  };

  const setReason = (reasonId, reasonName) => {
    registrationData.value.reasonId = reasonId;
    registrationData.value.reasonName = reasonName;
    registrationData.value.lastActivity = Date.now();
    resetTimeout();
  };

  const setCompanyReasonId = (companyReasonId) => {
    registrationData.value.companyReasonId = companyReasonId;
    registrationData.value.lastActivity = Date.now();
    resetTimeout();
  };

  const setPhoneNumber = (phone) => {
    registrationData.value.phoneNumber = phone;
    registrationData.value.lastActivity = Date.now();
    resetTimeout();
  };

  const setVerificationCode = (code) => {
    registrationData.value.verificationCode = code;
    registrationData.value.lastActivity = Date.now();
    resetTimeout();
  };

  const setShop = (shopId, shopCode, shopType, shopAddress) => {
    registrationData.value.shopId = shopId;
    registrationData.value.shopCode = shopCode;
    registrationData.value.shopType = shopType;
    registrationData.value.shopAddress = shopAddress;
    registrationData.value.lastActivity = Date.now();
    resetTimeout();
  };

  const reset = () => {
    if (timeoutId) {
      clearTimeout(timeoutId);
      timeoutId = null;
    }
    registrationData.value = {};
    localStorage.removeItem(STORAGE_KEY);
  };

  const getData = () => registrationData.value;

  const setShiftId = (id) => {
    registrationData.value.shiftId = id;
    registrationData.value.lastActivity = Date.now();
    resetTimeout();
  };

  // Check if data is complete enough to save to database
  const isReadyForDatabase = () => {
    const data = registrationData.value;
    return !!(
      data.shiftType === 'start' &&
      data.startDate &&
      data.startTime &&
      data.companyReasonId &&
      data.phoneNumber
    );
  };

  const setOngoingShift = (shiftData) => {
    console.log('Setting ongoing shift data:', shiftData); // Debug
    registrationData.value.shiftId = shiftData.shiftID;
    registrationData.value.companyName = shiftData.companyName;
    registrationData.value.reasonName = shiftData.reasonName;
    registrationData.value.lastActivity = Date.now();
    resetTimeout();
};


  return {
    registrationData,
    setShiftType,
    setCompany,
    setReason,
    setPhoneNumber,
    setVerificationCode,
    reset,
    getData,
    setShiftId,
    setOngoingShift,
    setCompanyReasonId,
    isReadyForDatabase,
    resetTimeout,
    setShop
  };
}
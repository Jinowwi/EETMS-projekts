<template> 
    <div class="page-content">
      <div class="blob blob-teal"></div>
      <div class="blob blob-pink"></div>
      <h1>Calendar</h1>
      <Calendar :missedPunches="shifts" 
      :initialDate = "initialDate"
      @punchClick="handlePunchClick" />
      <MissedPunchModal 
        :isOpen="modalOpen"
        :punchData="selectedPunch"
        @close="closeModal"
        @save="handleSave"
      />
    </div>
</template>

<script setup>
import { useRoute } from 'vue-router'; 
import { ref, computed, onMounted  } from 'vue';
// import Navbar from '@/components/navbar.vue';
import Calendar from '@/components/calendar.vue';
import MissedPunchModal from '@/components/MissedPunchModal.vue';
import { getAdminRoleLevel, getAdmin } from '@/services/auth.js'; 

const roleLevel = Number(getAdminRoleLevel());
const currentAdmin = getAdmin(); 
const shifts = ref([]); 
const modalOpen = ref(false);
const selectedPunch = ref({});
const API_BASE = 'http://localhost:5001/api';
const route = useRoute(); 

const initialDate = computed(() => {
  const q = route.query.date;
  return q ? new Date(String(q)) : null; 
}); 

const fetchShifts = async () => {
  try {
    const compRes = await fetch(`${API_BASE}/companies`);
    let myCompanyNames = [];

    if (compRes.ok) {
      const companies = await compRes.json();

      if (roleLevel === 1) {
        const currentRemId = currentAdmin?.remId
          ? Number(currentAdmin.remId)
          : Number(localStorage.getItem('remId'));

        myCompanyNames = companies
          .filter(c => Number(c.remID) === currentRemId)
          .map(c => c.companyName);
      }
    }

    const response = await fetch(`${API_BASE}/shifts`);
    if (response.ok) {
      let data = await response.json();

      if (roleLevel === 1) {
        data = myCompanyNames.length > 0
          ? data.filter(shift => myCompanyNames.includes(shift.companyName))
          : [];
      }

      shifts.value = data.map(shift => ({
        ...shift,
        date: shift.startDate,
        type: !shift.startTime ? 'start' : !shift.endTime ? 'end' : null,
      }));
    }
  } catch (error) {
    console.error("Failed to load shifts:", error);
  }
}; 

onMounted(fetchShifts); 

const handlePunchClick = (shiftData) => {
  selectedPunch.value = shiftData;
  modalOpen.value = true;
};

const closeModal = () => {
  modalOpen.value = false;
};

const toHHMMSS = (hhmm) => (hhmm && hhmm.length === 5 ? `${hhmm}:00` : hhmm);

const handleSave = async (updatedShift) => {
  const id = updatedShift.ShiftID || updatedShift.shiftID;

  const patch = [];
  if (selectedPunch.value.type === 'start') {
    patch.push({ op: 'replace', path: '/start_time', value: toHHMMSS(updatedShift.startTime) });
  } else if (selectedPunch.value.type === 'end') {
  if (updatedShift.EndDate) {
    patch.push({ op: 'replace', path: '/end_date', value: updatedShift.EndDate })
  }
  patch.push({ op: 'replace', path: '/end_time', value: toHHMMSS(updatedShift.endTime) })
  }

  const res = await fetch(`${API_BASE}/shifts/${id}`, {
    method: 'PATCH',
    headers: { 'Content-Type': 'application/json-patch+json' },
    body: JSON.stringify(patch),
  });

  if (res.ok) {
    modalOpen.value = false;
    await fetchShifts();
  } else {
    console.error(await res.text());
  }
};

</script>

<style scoped>
.page-content {
  margin-left: 0;
  height: 100vh;
  overflow: hidden;
  display: flex;
  flex-direction: column;
  box-sizing: border-box;
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
}

/* Responsive font size */
@media (max-width: 768px) {
  h1 {
    font-size: 40px;
    padding: 15px;
  }
}

@media (max-width: 480px) {
  h1 {
    font-size: 32px;
    padding: 12px;
  }
}
</style>

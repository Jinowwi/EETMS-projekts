<template> 
    <div class="page-content">
      <!-- Fona noformējuma elementi -->
      <div class="blob blob-teal"></div>
      <div class="blob blob-pink"></div>

      <!-- Lapas virsraksts -->
      <h1>Calendar</h1>

      <!-- Kalendāra komponents -->
      <Calendar :missedPunches="shifts" 
        :initialDate = "initialDate"
        @punchClick="handlePunchClick" 
      />
      
      <!-- Neatzīmēto maiņu modālais logs -->
      <MissedPunchModal 
        :isOpen="modalOpen"
        :punchData="selectedPunch"
        @close="closeModal"
        @save="handleSave"
      />
    </div>
</template>

<script setup>
// Maršrutēšana
import { useRoute } from 'vue-router'; 

// Vue funkcijas
import { ref, computed, onMounted  } from 'vue';

// Komponenti
import Calendar from '@/components/calendar.vue';
import MissedPunchModal from '@/components/MissedPunchModal.vue';

// Autentifikācijas dati
import { getAdminRoleLevel, getAdmin } from '@/services/auth.js'; 

// Lietotāja dati
const roleLevel = Number(getAdminRoleLevel());
const currentAdmin = getAdmin(); 

// Komponenta stāvokļi
const shifts = ref([]); 
const modalOpen = ref(false);
const selectedPunch = ref({});

// API un maršrūts
const API_BASE = 'http://localhost:5001/api';
const route = useRoute(); 

// Sākuma datums no query 
const initialDate = computed(() => {
  const q = route.query.date;
  return q ? new Date(String(q)) : null; 
}); 

// Ielādēt maiņas datus
const fetchShifts = async () => {
  try {
    const compRes = await fetch(`${API_BASE}/companies`);
    let myCompanyNames = [];

    // Ielādēt uzņēmumus
    if (compRes.ok) {
      const companies = await compRes.json();

      // Filtrs REM lietotājiem 
      if (roleLevel === 1) {
        const currentRemId = currentAdmin?.remId
          ? Number(currentAdmin.remId)
          : Number(localStorage.getItem('remId'));

        myCompanyNames = companies
          .filter(c => Number(c.remID) === currentRemId)
          .map(c => c.companyName);
      }
    }

    // Ielādēt maiņu sarakstu 
    const response = await fetch(`${API_BASE}/shifts`);
    if (response.ok) {
      let data = await response.json();

      // Filtrēt tikai savus uzņēmumus
      if (roleLevel === 1) {
        data = myCompanyNames.length > 0
          ? data.filter(shift => myCompanyNames.includes(shift.companyName))
          : [];
      }

      // Pārveidot datus kalendāram 
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

// Ielādēt datus pēc mount
onMounted(fetchShifts); 

// Atvērt modālo logu pēc klikšķa 
const handlePunchClick = (shiftData) => {
  selectedPunch.value = shiftData;
  modalOpen.value = true;
};

// Aizvērt modālo logu 
const closeModal = () => {
  modalOpen.value = false;
};

// Pārveidot laiku uz HH:MM:SS
const toHHMMSS = (hhmm) => (hhmm && hhmm.length === 5 ? `${hhmm}:00` : hhmm);

// Saglabāt izmaiņas 
const handleSave = async (updatedShift) => {
  const id = updatedShift.ShiftID || updatedShift.shiftID;

  const patch = [];
  // Atjaunot sākuma laiku 
  if (selectedPunch.value.type === 'start') {
    patch.push({ op: 'replace', path: '/start_time', value: toHHMMSS(updatedShift.startTime) });
  
  // Atjaunot beigu laiku 
  } else if (selectedPunch.value.type === 'end') {
  if (updatedShift.EndDate) {
    patch.push({ op: 'replace', path: '/end_date', value: updatedShift.EndDate })
  }
  patch.push({ op: 'replace', path: '/end_time', value: toHHMMSS(updatedShift.endTime) })
  }

  // Atsūtīt PATCH pieprasījumu 
  const res = await fetch(`${API_BASE}/shifts/${id}`, {
    method: 'PATCH',
    headers: { 'Content-Type': 'application/json-patch+json' },
    body: JSON.stringify(patch),
  });

  // Pārlādēt datus pēc saglabāšanas 
  if (res.ok) {
    modalOpen.value = false;
    await fetchShifts();
  } else {
    console.error(await res.text());
  }
};

</script>

<style scoped>
/* Galvenais lapas konteiners */
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

/* Mazāks virsraksts planšetē */
@media (max-width: 768px) {
  h1 {
    font-size: 40px;
    padding: 15px;
  }
}

/* Mazāks virsraksts telefonā */
@media (max-width: 480px) {
  h1 {
    font-size: 32px;
    padding: 12px;
  }
}
</style>

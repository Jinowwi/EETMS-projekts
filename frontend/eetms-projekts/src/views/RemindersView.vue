<template>
    <div class="page-content">
        <!-- Fona noformējuma elementi-->
        <div class="blob blob-teal"></div>
        <div class="blob blob-pink"></div>
        
        <!-- Virsraksts, pārslēgšanās starp cilnēm -->
        <h1>{{ activeTab === 'notifications' ? 'Notifications' : 'Assign REM' }}</h1>
        
        <!-- Atļaut pārslēgšanās ja lietotāja loma ir 3.līmeņa -->
        <!-- Pogu pārslēgšanās starp paziņojumiem un REM pievienošanu uzņēmumiem -->
        <div class="toggle-container" v-if="roleLevel === 3">
            <div class="toggle-switch">
                <button 
                    :class="['toggle-btn', { active: activeTab === 'notifications' }]"
                    @click="activeTab = 'notifications'"
                >
                    Notifications
                </button>
                <button 
                    :class="['toggle-btn', { active: activeTab === 'assign' }]"
                    @click="activeTab = 'assign'"
                >
                    Assign REM
                </button>
            </div>
        </div>

        <!-- Rādīt paziņojumus, ja lietotāja loma ir 1. vai 3. līmeņa -->
        <div v-if="activeTab === 'notifications' && (roleLevel === 1 || roleLevel === 3)" class="reminders-container">
            <div 
                v-for="r in reminders"
                :key="r.shiftID ?? r.ShiftID" 
                class="reminder-card"
            >
                <div class="reminder-icon">
                    <FontAwesomeIcon :icon="['fas', 'triangle-exclamation']" />
                </div>
                <div class="reminder-content">
                    <p class="reminder-header">
                        {{ formatDate(r.startDate || r.endDate) }}
                    </p>
                    <p class="reminder-body">
                        Start: {{ r.startTime ? String(r.startTime).slice(0,5) : 'Missed' }}, 
                        End: {{ r.endTime ? String(r.endTime).slice(0,5) : 'Missed' }}
                    </p>
                    <p class="reminder-navigation" @click="goToCalendar(r)">To Calendar</p>
                </div>
            </div>

            <!-- Teksts, ja vairs nav paziņojumu -->
            <div v-if="reminders.length === 0" class="no-more">
                No More Notifications
            </div>
        </div>

        <!-- REM piešķiršanas sadaļa redzama 2. vai 3. līmeņa lietotājam -->
        <div v-if="activeTab === 'assign' && (roleLevel === 2 || roleLevel === 3)" class="reminders-container">
            <div 
                v-for="(c, index) in unassignedCompanies"
                :key="c.companyID" 
                class="reminder-card assign-card"
            >
                <!-- Uzņēmuma informācija -->
                <div class="reminder-content">
                    <p class="reminder-header">{{ c.companyName || c.CompanyName }}</p>
                    <p class="reminder-body">{{ c.address || c.Address }}</p>
                </div>

                <!-- REM izvēle un piešķiršanas poga -->
                <div class="assign-action">
                  <select v-model="c.selectedRemId" class="rem-select">
                    <option disabled value="">Choose REM</option>
                    <option
                      v-for="admin in remAdmins"
                      :key="admin.remID ?? admin.RemID"
                      :value="admin.remID ?? admin.RemID" 
                    >
                      {{ admin.email ?? admin.Email }} 
                    </option>
                  </select>
                    <button @click="assignRem(c)" class="assign-btn">Assign</button>
                </div>
            </div>
            
            <!-- Ziņa, ja nav neviena uzņēmuma bez piešķirta REM -->
            <div v-if="unassignedCompanies.length === 0" class="no-more">
                No unassigned companies waiting.
            </div>
        </div>
    </div>
</template>

<script setup>
import { FontAwesomeIcon } from '@fortawesome/vue-fontawesome';
import { ref, onMounted, computed, watch } from 'vue'; 
import { useRouter } from 'vue-router';
import { getAdminRoleLevel, getAdmin } from '@/services/auth.js'; 

// Lietotāja lomas līmenis
const roleLevel = Number(getAdminRoleLevel());

// Pašreizējā administratora dati
const currentAdmin = getAdmin(); 

// Routeris navigācijai starp lapām
const router = useRouter();

// API bāzes adrese
const API_BASE = 'http://localhost:5002/api'; 

// Paziņojumu saraksts
const reminders = ref([]); 

// Aktīvā cilne
// 2. līmenim pēc noklusējuma "assign", pārējiem "notifications"
const activeTab = ref(roleLevel === 2 ? 'assign' : 'notifications'); 
const unassignedCompanies = ref([]);

// const visibleReminderCount = ref(6); 

// Administratori ar REM lomu
const remAdmins = ref([]); 

/* 
const reminderSortBy = ref('date'); 
const reminderSortDirection = ref('asc'); 
const assignSortBy = ref('companyName');
const assignSortDirection = ref('asc'); 

const handleReminderSort = (column) => {
  if (reminderSortBy.value === column) {
    reminderSortDirection.value = reminderSortDirection.value === 'asc' ? 'desc' :
    'asc'; 
  } else {
    reminderSortBy.value = column; 
    reminderSortDirection.value = 'asc'; 
  }
}; 
*/ 

// Ielādē paziņojumus no API
const fetchReminders = async () => {
    // Iegūst uzņēmumu sarakstu
    const compRes = await fetch(`${API_BASE}/companies`); 
    let myCompanyNames = []; 
    
    if (compRes.ok) {
        const companies = await compRes.json(); 

        // Ja lietotājs ir 1. līmeņa REM administrators,
        // sagatavot sarakstu ar viņam piesaistītajiem uzņēmumiem
        if (roleLevel === 1) {
          const currentRemId = currentAdmin?.remId
            ? Number(currentAdmin.remId)
            : Number(localStorage.getItem('remId'));

          const normalized = companies.map(c => ({
            remID: c.remID,
            companyName: c.companyName,
          }));

          myCompanyNames = normalized
            .filter(c => Number(c.remID) === currentRemId)
            .map(c => c.companyName);
        }
    }

    // Iegūst maiņu atgādinājumus
    const res = await fetch(`${API_BASE}/shifts/reminders`); 
    if (!res.ok) throw new Error(await res.text());
    let data = await res.json(); 

    // Ja lietotājs ir REM administrators, 
    // filtrē tikai viņa uzņēmumu paziņojumus
    if (roleLevel === 1) {
      if (myCompanyNames.length > 0) {
        data = data.filter(r => myCompanyNames.includes(r.companyName));
      }
    }
    reminders.value = data;  
}; 

// Ielādēt uzņēmumus, kuriem vēl nav piešķirts REM
const fetchUnassignedCompanies = async () => {
    try {
        const res = await fetch(`${API_BASE}/companies`);
        if (res.ok) {
            const data = await res.json();

            unassignedCompanies.value = data
                .filter(c => {
                    const remId = c.remID ?? c.RemID;
                    return remId == null || Number(remId) === 0;
                })
                .map(c => ({
                    ...c,
                    companyID: c.companyID ?? c.CompanyID,
                    companyName: c.companyName ?? c.CompanyName,
                    address: c.address ?? c.Address, 
                    selectedRemId: ''
                }));
        }
    } catch (error) {
        console.error("Failed to fetch unassigned companies", error);
    }
};

// Ielādēt administratorus, kuriem ir REM tips
const fetchRemAdmins = async () => {
  try {
    const res = await fetch(`${API_BASE}/administrators`); 
    if (res.ok) {
      const data = await res.json(); 

      remAdmins.value = data.filter(a => {
        const type = a.typeOfAdmin ?? a.TypeOfAdmin; 
        return Number(type) === 1;  
      }); 
    }
  } catch (error) {
    console.error("Failed to REM admins", error); 
  }
}; 

// Piešķirt izvēlēto REM uzņēmumam
const assignRem = async (company) => {
    if (!company.selectedRemId) return;

    try {
      // Sagatavot datus nosūtīšanai uz backend
        const payload = {
            ...company,
            RemID: Number(company.selectedRemId),
            remID: Number(company.selectedRemId)
        };

        delete payload.selectedRemId;

        // Noteikt uzņēmuma ID
        const compId = company.companyID ?? company.CompanyID;

        // Atjaunot uzņēmuma datus
        const res = await fetch(`${API_BASE}/companies/${compId}`, {
            method: 'PUT',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(payload)
        });

        // Ja piešķiršana veiksmīga, pārlādēt nepiešķirto uzņēmumu sarakstu
        if (res.ok) {
            await fetchUnassignedCompanies();
        } else {
            console.error(await res.text());
        }
    } catch (error) {
        console.error("Failed to assign REM", error);
    }
}; 

// Komponentes ielādes brīdī ielādēt nepieciešamos datus
onMounted(async () => {
    // Ielādēt paziņojumus 1. un 3. līmeņa lietotājiem
    if (roleLevel === 1 || roleLevel === 3) {
        try {
            await fetchReminders(); 
        } catch (e) {
            console.error("fetchReminders failed:", e);
        }
    }

    // Ielādēt REM administratorus un uzņēmumus 2. un 3. līmeņa lietotājiem
    if (roleLevel === 2 || roleLevel === 3) {
        try {
            await Promise.all([
                fetchRemAdmins(),
                fetchUnassignedCompanies()
            ]);
        } catch (e) {
            console.error("fetchUnassignedCompanies failed:", e);
        }
    }
}); 

// Pāriet uz kalendāra lapu ar izvēlēto datumu
const goToCalendar = (r) => {
    const date = r.startDate || r.endDate; 
    router.push({ path: '/calendar', query: { date } });
};

// Formatēt datumu uz DD.MM.GGGG formātu
const formatDate = (dateLike) => {
    if (!dateLike) return ''
    const s = String(dateLike).trim().replaceAll('/home', '-')
    const d = new Date(s)
    if (Number.isNaN(d.valueOf())) return String(dateLike)
    return d.toLocaleDateString('de-DE', {
        day: '2-digit',
        month: '2-digit',
        year: 'numeric', 
    })
}
</script>

<style scoped>
/* Galvenais lapas konteiners */
.page-content {
  display: flex;
  flex-direction: column;
  align-items: center;
  width: 100%;
  min-height: 100vh;
  padding: 40px 20px;
  box-sizing: border-box;
  background: var(--background-color);
  position: relative;
  overflow-x: hidden;
}

/* Cilņu pārslēgšanas ārējais konteiners */
.toggle-container {
  display: flex;
  justify-content: center;
  margin-bottom: 30px;
  width: 100%;
  position: relative;
  z-index: 1;
}

/* Pārslēgšanas slēdža stils */
.toggle-switch {
  display: flex;
  background: rgba(255, 255, 255, 0.5);
  backdrop-filter: blur(12px);
  border-radius: 30px;
  padding: 4px;
  width: 100%;
  max-width: 500px;
  border: 1px solid rgba(255, 255, 255, 0.7);
}

/* Vienas cilnes pogas stils */
.toggle-btn {
  flex: 1;
  padding: 10px 0;
  border: none;
  background: transparent;
  border-radius: 26px;
  font-family: 'Inter', sans-serif;
  font-size: 14px;
  font-weight: 600;
  color: #6b7280;
  cursor: pointer;
  transition: all 0.3s ease;
}

/* Aktīvās cilnes poga */
.toggle-btn.active {
  background-color: white;
  color: var(--brand-berry, #a12971);
  box-shadow: 0 2px 8px rgba(0,0,0,0.1);
}

/* Kartīte REM piešķiršanas režīmā */
.assign-card {
  align-items: center;
}

.assign-action {
  display: flex;
  gap: 10px;
  align-items: center;
}

/* Teksta ievades lauks */
.rem-input {
  width: 80px;
  padding: 8px 12px;
  border: 2px solid rgba(255, 255, 255, 0.7);
  border-radius: 8px;
  font-family: 'Inter', sans-serif;
  font-size: 14px;
  background: rgba(255, 255, 255, 0.6);
  outline: none;
}

.rem-input:focus {
  border-color: var(--brand-teal);
}

/* REM izvēles saraksts */
.rem-select {
  width: 220px;
  height: 42px;
  padding: 0 16px;
  border: none;
  border-radius: 40px;
  background: var(--color-white);
  color: var(--brand-berry);
  font-family: 'Inter', sans-serif;
  font-size: 14px;
  font-weight: 500;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  outline: none;
  cursor: pointer;
  appearance: none;
  -webkit-appearance: none;
  -moz-appearance: none;
  transition: all 0.25s ease;
}

/* Poga REM piešķiršanai */
.assign-btn {
  padding: 0px 16px;
  width: 80px; 
  height: 42px;
  background: var(--brand-berry);
  color: white;
  border: none;
  border-radius: 40px;
  font-family: 'Inter', sans-serif;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.2s ease;
}

.assign-btn:hover {
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(161, 41, 113, 0.3);
}

/* Paziņojumu / kartīšu konteiners */
.reminders-container {
  position: relative;
  z-index: 10; 
  width: 100%;
  max-width: 800px;
  display: flex;
  flex-direction: column;
  gap: 16px;
  margin-top: 20px; 
}

/* Paziņojumu kartīte */
.reminder-card {
  display: flex;
  align-items: flex-start;
  gap: 20px;
  padding: 30px;
  background: rgba(255, 255, 255, 0.55);
  backdrop-filter: blur(16px);
  -webkit-backdrop-filter: blur(16px);
  border: 1.5px solid rgba(255, 255, 255, 0.85);
  border-radius: 16px;
  box-shadow: 0 8px 32px rgba(0, 0, 0, 0.08);
  transition: all 0.3s ease;
}

.reminder-card:hover {
  transform: translateY(-2px);
  box-shadow: 0 12px 36px rgba(0, 0, 0, 0.12);
  background: rgba(255, 255, 255, 0.65);
}

/* Paziņojuma ikonas zona */
.reminder-icon {
  flex-shrink: 0;
  width: 28px;
  height: 28px;
  color: var(--reminder-header);
  font-size: 28px;
  display: flex;
  align-items: center;
  justify-content: center;
  margin-top: 0;
}

/* Paziņojuma teksta saturs */
.reminder-content {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.reminder-header {
  margin: 0; 
  font-family: 'Inter', sans-serif;
  font-size: 25px;
  font-weight: bold;
  color: var(--brand-berry);
}

.reminder-body {
  margin: 0;
  font-family: 'Inter', sans-serif;
  font-size: 20px;
  color: var(--brand-berry);
}

/* Saite uz kalendāru */
.reminder-navigation {
  margin: 0;
  font-family: 'Inter', sans-serif;
  font-size: 20px;
  color: var(--brand-berry);
  cursor: pointer;
  width: fit-content;
  transition: color 0.15s;
}

.reminder-navigation:hover {
  color: #841c40;
  font-weight: bold;
}

/* Teksts, ja saraksts ir tukšs */
.no-more {
  text-align: center;
  padding: 50px 20px;
  font-family: 'Inter', sans-serif;
  font-size: 15px;
  font-weight: 500;
  color: #aaa;
  letter-spacing: 0.3px;
}

/* Responsivitāte: lielajiem ekrāniem */
@media (min-width: 1441px) {
  .reminders-container { max-width: 900px; }
  .reminder-card { padding: 35px 40px; }
  .reminder-header { font-size: 28px; }
  .reminder-body { font-size: 22px; }
  .reminder-navigation { font-size: 22px; }
}

@media (max-width: 1300px) {
  .reminders-container {
    max-width: 650px;
  }

  .reminder-card {
    padding: 22px 24px;
    gap: 16px;
  }

  .reminder-icon {
    font-size: 22px;
    width: 22px;
    height: 22px;
  }

  .reminder-header {
    font-size: 20px;
  }

  .reminder-body {
    font-size: 16px;
  }

  .reminder-navigation {
    font-size: 16px;
  }

  .toggle-switch {
    max-width: 400px;
  }

  .toggle-btn {
    font-size: 13px;
    padding: 9px 0;
  }
}

/* Responsivitāte: planšetdatoriem un klēpjdatoriem */
@media (max-width: 1024px) {
  .page-content { padding: 30px 20px; }
  h1 { margin-bottom: 32px; }
  .reminders-container { max-width: 680px; }
  .reminder-card { padding: 25px; }
  .reminder-icon { font-size: 24px; width: 24px; height: 24px; }
  .reminder-header { font-size: 22px; }
  .reminder-body { font-size: 18px; }
  .reminder-navigation { font-size: 18px; }
}

@media (max-width: 768px) {
  .page-content { padding: 20px 15px; }
  h1 { margin-bottom: 24px; }
  .reminders-container { max-width: 100%; border-radius: 16px; }
  .reminder-card { padding: 20px; gap: 15px; }
  .reminder-header { font-size: 19px; }
  .reminder-body { font-size: 15px; }
  .reminder-navigation { font-size: 15px; }
}

/* Responsivitāte: mobilam ierīcem */
@media (max-width: 480px) {
  .page-content { padding: 16px 12px; }
  h1 { margin-bottom: 20px; }
  .reminders-container { border-radius: 14px; }
  .reminder-card { padding: 18px 15px; gap: 12px; }
  .reminder-icon { font-size: 20px; width: 20px; height: 20px; }
  .reminder-header { font-size: 17px; }
  .reminder-body { font-size: 14px; }
  .reminder-navigation { font-size: 14px; }
  .no-more { padding: 30px 15px; font-size: 13px; }
  
  .assign-action {
    flex-direction: column;
    align-items: flex-end;
  }
}

@media (max-width: 375px) {
  .page-content { padding: 12px 8px; }
  .reminder-card { padding: 15px 12px; gap: 10px; }
  .reminder-icon { font-size: 18px; width: 18px; height: 18px; }
  .reminder-header { font-size: 15px; }
  .reminder-body { font-size: 12px; }
  .reminder-navigation { font-size: 12px; }
  .no-more { padding: 24px 12px; font-size: 12px; }
}
</style>



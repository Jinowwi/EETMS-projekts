<template>
    <div>
    <button v-if="currentUserRole === 2" class="logout-btn" @click="handleLogout">
        <FontAwesomeIcon icon="fa-solid fa-right-from-bracket" />
        Logout
    </button>
    <div class="page-content">
        <div class="blob blob-teal"></div>
        <div class="blob blob-pink"></div>

        <h1>Data</h1>
        <div class="table-container">
            <div class="table-wrapper">
                <div class="table-section">
                    <div class="section-header">
                        <h2>Company Information</h2>
                        <div class="buttons">
                            <!-- Poga filtrēšanas un kārtošanas joslas atvēršanai/aizvēršanai -->
                            <button :class="['btn', { 'active-btn': isSortModeCompanies }]" @click="toggleSortModeCompanies">
                                <FontAwesomeIcon :icon="['fas', 'sliders-h']" />
                            </button>

                            <!-- Poga rediģēšanas režīma ieslēgšanai/izslēgšanai -->
                            <button :class="['btn', { 'active-btn': isEditModeCompanies }]" @click="toggleEditModeCompanies()">
                                <FontAwesomeIcon :icon="['fas', 'pen']" />
                            </button>
                            
                            <!-- Poga jauna uzņēmuma pievienošanas modāla loga atvēršanai -->
                            <button :class="['btn', { 'active-btn': addCompanyModalOpen }]" @click="openAddCompanyModal">
                                <FontAwesomeIcon :icon="['fas', 'plus']" />
                            </button>
                        </div>
                    </div>
                    
                    <!-- Tabulas filtrēšanas josla -->
                    <transition name="slide-down">
                        <div v-if="isSortModeCompanies" class="sort-toolbar">
                            <span class="sort-label">Filter By</span>
                            <div class="sort-options">

                                <!-- Filtrēšana pēc valsts -->
                                <select v-model="filterCountry" class="search-input">
                                    <option value="">All Countries</option>
                                    <option value="Lithuania">Lithuania</option>
                                    <option value="Latvia">Latvia</option>
                                    <option value="Estonia">Estonia</option>
                                </select>

                                <!-- Filtrēšana pēc piesaistītā pārvaldnieka -->
                                <select v-model="filterRem" class="search-input">
                                    <option value="">All REMs</option>
                                    <option v-for="admin in remAdministrators" :key="admin.remID"
                                    :value="admin.email">
                                        {{ admin.email }}
                                </option>
                                </select>
                                
                                <!-- Meklēšanas ievadlauks -->
                                <div class="search-container">
                                    <input
                                        type="text"
                                        v-model="query"
                                        :placeholder="`Search`"
                                        class="search-input"
                                    />    
                                </div>
                            </div>
                        </div>
                    </transition>

                    <!-- Tabulas konteineris -->
                    <div class="table-scrollable">
                        <table>
                        <thead>
                            <tr>
                                <!-- Kolonna uzņēmuma nosaukuma kārtošanai -->
                                <th @click="handleHeaderSort('companyName')" class="sortable-header">
                                    Company Name
                                    <FontAwesomeIcon v-if="sortColumn === 'companyName'" :icon="sortDirection === 'asc' ? ['fas', 'sort-up'] : ['fas', 'sort-down']" class="sort-icon" />
                                    <FontAwesomeIcon v-else :icon="['fas', 'sort']" class="sort-icon placeholder" /> 
                                </th>

                                <!-- Kolonna valsts kārtošanai -->
                                <th @click="handleHeaderSort('country')" class="sortable-header">
                                    Country
                                    <FontAwesomeIcon v-if="sortColumn === 'country'" :icon="sortDirection === 'asc' ? ['fas', 'sort-up'] : ['fas', 'sort-down']" class="sort-icon" />
                                    <FontAwesomeIcon v-else :icon="['fas', 'sort']" class="sort-icon placeholder" />
                                </th>

                                <!-- REM administratora e-pasts -->
                                <th>Rimi Employee</th>

                                <!-- Kolonna dzēšanas pogai rediģēšanas režīmā -->
                                <th v-if="isEditModeCompanies"></th> 
                            </tr>
                        </thead>
                        <tbody>

                            <!-- Uzņēmumu saraksta attēlošana -->
                            <tr v-for="row in sortedCompanyData" :key="row.id"
                            @click="!isEditModeCompanies && openCompanyModal(row)"
                            class="clickable-row">

                                <!-- Standarta skats: uzņēmuma nosaukums -->
                                <td v-if="!isEditModeCompanies">{{ row.companyName }}</td>

                                <!-- Rediģēšanas skats: uzņēmuma nosaukuma rediģēšana -->
                                <td v-else @click.stop>
                                    <input 
                                    v-model="row.companyName" 
                                    maxlength="40"
                                    @blur="updateCompany(row)"
                                    @keydown.enter="updateCompany(row)"
                                    class="edit-input" 
                                    />
                                </td>

                                <!-- Standarta skats: valsts -->
                                <td v-if="!isEditModeCompanies"><span class="country-badge">{{ row.country }}</span></td>

                                <!-- Rediģēšanas skats: valsts izvēlne -->
                                <td v-else @click.stop>
                                    <select v-model="row.country" @change="updateCompany(row)" class="edit-select">
                                        <option value="Lithuania">Lithuania</option>
                                        <option value="Latvia">Latvia</option>
                                        <option value="Estonia">Estonia</option>
                                    </select>
                                </td>

                                <!-- Standarta skats: Rimi administrācijas darbnieka e-pasts -->
                                <td v-if="!isEditModeCompanies">{{ row.rimiEmployeeEmail }}</td>
                                
                                <!-- Rediģēšanas skats: e-pasta adreses izvēlne -->
                                <td v-else @click.stop>
                                    <select
                                        v-model="row.remID"
                                        @change="updateCompany(row)"
                                        class="edit-select" 
                                    >
                                        <option :value="null">No REM</option>
                                        <option
                                            v-for="admin in remAdministrators"
                                            :key="admin.remID"
                                            :value="admin.remID"
                                        >
                                            {{ admin.email }}
                                        </option>
                                    </select>
                                </td>
                                
                                <!-- Rediģēšanas skats: Dzēšanas poga -->
                                <td v-if="isEditModeCompanies">
                                    <button @click.stop="deleteCompany(row.id, row.companyName)" class="btn-delete">
                                        <FontAwesomeIcon :icon="['fas', 'xmark']" />
                                    </button>
                                </td> 
                            </tr>
                        </tbody>
                    </table>
                    </div>
                </div>
 
                <div class="table-section">
                    <div class="section-header">
                        <h2>Reasons</h2>
                        <div class="buttons">
                            <button :class="['btn', { 'active-btn': isSortModeReasons }]" @click="toggleSortModeReasons()">
                                <FontAwesomeIcon :icon="['fas', 'sliders-h']" />
                            </button>
                            <button class="btn btn-edit" @click="toggleEditModeReasons()">
                                <FontAwesomeIcon :icon="['fas', 'pen']" />
                            </button>
                            <button class="btn btn-add" @click="openAddReasonModal()">
                                <FontAwesomeIcon :icon="['fas', 'plus']" />
                            </button>
                        </div>
                    </div>
                    <transition name="slide-down">
                        <div v-if="isSortModeReasons" class="sort-toolbar">
                            <div class="sort-options">
                                <div class="search-container">
                                <input
                                    type="text"
                                    v-model="queryReasons"
                                    placeholder="Search"
                                    class="search-input"
                                />
                                </div>
                            </div>
                        </div>
                    </transition>
                    <div class="table-scrollable">
                       <table>
                        <thead>
                            <tr>
                                <th>Reasons</th>
                                <th v-if="isEditModeReasons"></th>
                            </tr>
                        </thead>
                        <tbody>
                            <tr v-for="row in filteredReasonData" :key="row.id"
                            @click="!isEditModeReasons && openReasonModal(row)"
                            class="clickable-row">

                                <td v-if="!isEditModeReasons">
                                    {{ row.reasonName }}
                                </td>
                                <td v-else>
                                    <input v-model="row.reasonName"
                                    @blur="updateReason(row)"
                                    @keydown.enter="updateReason(row)"
                                    class="edit-input" />
                                </td>

                                <td v-if="isEditModeReasons">
                                    <button @click.stop="deleteReason(row.id, row.reasonName)" class="btn-delete">
                                        <FontAwesomeIcon :icon="['fas', 'xmark']" />
                                    </button>
                                </td>
                            </tr>
                        </tbody>
                    </table> 
                    </div>
                </div> 

                <div class="table-section">
                    <div class="section-header">
                        <h2>Shops</h2>
                        <div class="buttons">
                            <button :class="['btn', { 'active-btn': isSortModeShops }]" @click="toggleSortModeShops()">
                                <FontAwesomeIcon :icon="['fas', 'sliders-h']" />
                            </button>
                            <button :class="['btn', { 'active-btn': isEditModeShops }]" @click="toggleEditModeShops()">
                                <FontAwesomeIcon :icon="['fas', 'pen']" />
                            </button>
                            <button :class="['btn', { 'active-btn': addShopModalOpen }]" @click="openAddShopModal">
                                <FontAwesomeIcon :icon="['fas', 'plus']" />
                            </button>
                        </div>
                    </div>
                    <transition name="slide-down">
                        <div v-if="isSortModeShops" class="sort-toolbar">
                            <span class="sort-label">Filter By</span>
                            <div class="sort-options">
                                    <select v-model="filterShopType" class="search-input">
                                        <option value="">All Types</option>
                                        <option value="Express">Express</option>
                                        <option value="Mini">Mini</option>
                                        <option value="Super">Super</option>
                                        <option value="Hyper">Hyper</option>
                                    </select>

                                    <select v-model="filterShopCountry" class="search-input">
                                        <option value="">All Countries</option>
                                        <option value="Lithuania">Lithuania</option>
                                        <option value="Latvia">Latvia</option>
                                        <option value="Estonia">Estonia</option>
                                    </select>

                                    <input
                                        type="text"
                                        v-model="queryShops"
                                        placeholder="Search"
                                        class="search-input"
                                    />
                            </div>
                        </div>
                    </transition>
                    <div class="table-scrollable">
                        <table>
                        <thead>
                        <tr>
                            <th @click="handleHeaderSortShops('type')" class="sortable-header">
                                Shop Type
                                <FontAwesomeIcon v-if="sortColumnShops === 'type'" 
                            :icon="sortDirectionShops === 'asc' ? ['fas', 'sort-up'] : ['fas', 'sort-down']" 
                            class="sort-icon" />
                                <FontAwesomeIcon v-else :icon="['fas', 'sort']" class="sort-icon placeholder" />
                            </th>
                            <th @click="handleHeaderSortShops('country')" class="sortable-header">
                                Country 
                                <FontAwesomeIcon v-if="sortColumnShops === 'country'"
                            :icon="sortDirectionShops === 'asc' ? ['fas', 'sort-up'] : ['fas', 'sort-down']" 
                            class="sort-icon" />
                                <FontAwesomeIcon v-else :icon="['fas', 'sort']" class="sort-icon placeholder" /> 
                            </th>
                            <th @click="handleHeaderSortShops('address')" class="sortable-header">
                                Address
                                <FontAwesomeIcon v-if="sortColumnShops === 'address'"
                            :icon="sortDirectionShops === 'asc' ? ['fas', 'sort-up'] : ['fas', 'sort-down']"
                            class="sort-icon" />
                                <FontAwesomeIcon v-else :icon="['fas', 'sort']" class="sort-icon placeholder" />
                            </th>
                            <th @click="handleHeaderSortShops('code')" class="sortable-header">
                                Shop Code 
                                <FontAwesomeIcon v-if="sortColumnShops === 'code'"
                            :icon="sortDirectionShops === 'asc' ? ['fas', 'sort-up'] : ['fas', 'sort-down']"
                            class="sort-icon" />
                                <FontAwesomeIcon v-else :icon="['fas', 'sort']" class="sort-icon placeholder" />
                            </th>
                            <th>Email</th>
                            <th v-if="isEditModeShops"></th>
                        </tr>
                        </thead>
                        <tbody>
                            <tr v-for="row in sortedShopData" :key="row.id">
                                <td v-if="!isEditModeShops">
                                    {{ row.type }}
                                </td>
                                <td v-else @click.stop>
                                    <input v-model="row.type"
                                        @blur="updateShop(row)"
                                        @keydown.enter="updateShop(row)"
                                        class="edit-input" />
                                </td>

                                <td v-if="!isEditModeShops">
                                    {{ row.country }}
                                </td>
                                <td v-else @click.stop>
                                    <select v-model="row.country"
                                            @change="updateShop(row)"
                                            class="edit-select">
                                        <option value="Lithuania">Lithuania</option>
                                        <option value="Latvia">Latvia</option>
                                        <option value="Estonia">Estonia</option>
                                    </select>
                                </td>
                                <td v-if="!isEditModeShops">
                                    {{ row.address }}
                                </td>
                                <td v-else>
                                    <input v-model="row.address"
                                    @blur="updateShop(row)"
                                    @keydown.enter="updateShop(row)"
                                    class="edit-input" 
                                    />
                                </td>
                                <td v-if="!isEditModeShops">
                                    {{ row.code }}
                                </td>
                                <td v-else>
                                    <input v-model="row.code" 
                                    @blur="updateShop(row)"
                                    @keydown.enter="updateShop(row)"
                                    class="edit-input" 
                                    />
                                </td>
                                <td v-if="!isEditModeShops">{{ row.email }}</td>
                                <td v-else @click.stop>
                                    <input v-model="row.email" type="email" @blur="updateShop(row)" @keydown.enter="updateShop(row)" class="edit-input"/>
                                </td>
                                <td v-if="isEditModeShops">
                                    <button @click.stop="deleteShop(row.id, row.code)" class="btn-delete">
                                        <FontAwesomeIcon :icon="['fas', 'xmark']" />
                                    </button>
                                </td>
                            </tr>
                        </tbody>
                    </table>
                    </div>   
                </div>

                <div v-if="currentUserRole === 3" class="table-section">
                    <div class="section-header">
                        <h2>Rimi Administration</h2>
                        <div class="buttons">
                            <button :class="['btn', {
                                'active-btn' : isSortModeAdmins }]"
                                @click="toggleSortModeAdmins()">
                                <FontAwesomeIcon :icon="['fas', 'sliders-h']" />
                            </button>
                        </div>
                    </div>

                    <transition name="slide-down">
                        <div v-if="isSortModeAdmins" class="sort-toolbar">
                            <div class="sort-options">
                                <select v-model="filterAdminType" class="search-input">
                                    <option value="">All Types</option>
                                    <option value="1">Real Estate (REM)</option>
                                    <option value="2">Admin</option>
                                </select>
                                <div class="search-container">
                                    <input
                                        type="text"
                                        v-model="queryAdmins"
                                        placeholder="Search administrations"
                                        class="search-input"
                                    />
                                </div>
                            </div>
                        </div>
                    </transition>

                    <div class="table-scrollable">
                        <table>
                            <thead>
                                <tr>
                                    <th @click="handleHeaderSortAdmins('email')" class="sortable-header">
                                        Email 
                                        <FontAwesomeIcon v-if="sortColumnAdmins === 'email'"
                                        :icon="sortDirectionAdmins === 'asc' ? ['fas', 'sort-up'] : 
                                        ['fas', 'sort-down']"
                                        class="sort-icon" 
                                        />
                                        <FontAwesomeIcon v-else :icon="['fas', 'sort']" class="sort-icon placeholder" />
                                    </th>
                                    <th @click="handleHeaderSortAdmins('typeOfAdmin')" class="sortable-header">
                                        Admin Type
                                        <FontAwesomeIcon v-if="sortColumnAdmins === 'typeOfAdmin'" 
                                            :icon="sortDirectionAdmins === 'asc' ? ['fas', 'sort-up'] : ['fas', 'sort-down']" 
                                            class="sort-icon" />
                                        <FontAwesomeIcon v-else :icon="['fas', 'sort']" class="sort-icon placeholder" />
                                    </th>
                                </tr>
                            </thead>
                            <tbody>
                                <tr v-for="row in sortedAdminData" :key="row.remID || row.id">
                                    <td>{{  row.email }}</td>
                                    <td>
                                        <span class="country-badge">
                                            {{ formatAdminType(row.typeOfAdmin) }}
                                        </span>
                                    </td>
                                </tr>
                            </tbody>
                        </table>
                    </div>
                </div>
            </div>
        </div>
        
        <div v-if="confirmModalOpen" class="confirm-overlay" @click.self="closeConfirmModal">
            <div class="confirm-box">
            <div class="confirm-header">
                <h3 class="confirm-title">{{ confirmTitle }}</h3>
            </div>

            <div class="confirm-body">
                <p>{{ confirmMessage }}</p>
            </div>

            <div class="confirm-actions">
                <button class="confirm-cancel" @click="closeConfirmModal">Cancel</button>
                <button class="confirm-delete" @click="executeConfirmAction">Delete</button>
            </div>
            </div>
        </div>

        <ShiftModal
          :isOpen="modalOpen"
          :user="selectedUser"
          :shiftData="selectedShift"
          :allReasons="reasonData"
          @close="closeModal"
          @refresh="fetchCompanies"
          @remove-reason="handleRemoveReasonFromCompany"
          @add-reason="handleAddReasonToCompany"
        />
        
        <!-- Modālais logs jauna uzņēmuma pievienošanai -->
        <AddCompanyModal
            :isOpen="addCompanyModalOpen"
            @close="closeAddCompanyModal"
            @save="addCompany"
        />
        
        <AddReasonModal 
            :isOpen="addReasonModalOpen"
            @close="closeAddReasonModal"
            @save="addReason"
        /> 

        <AddShopModal 
            :isOpen="addShopModalOpen"
            @close="closeAddShopModal"
            @save="addShop"
        />
    </div>
    </div>
</template>
 
<script setup>
import { ref, computed, onMounted, nextTick } from 'vue';
import axios from 'axios';
import { FontAwesomeIcon } from '@fortawesome/vue-fontawesome';
import ShiftModal from '@/components/ShiftModal.vue';
import AddCompanyModal from '@/components/AddCompanyModal.vue';
import AddReasonModal from '@/components/AddReasonModal.vue';
import AddShopModal from '@/components/AddShopModal.vue';
import { useRouter } from 'vue-router';
import { logout } from '@/services/auth.js';

const API_BASE = 'http://localhost:5001/api';
 
const query = ref('');
const queryReasons = ref(''); 
const queryShops = ref(''); 
const queryAdmins = ref(''); 
const filterAdminType = ref(''); 

const modalOpen = ref(false);
const selectedUser = ref({});
const selectedShift = ref([]);

const filterCountry = ref(''); 
const filterRem = ref(''); 
const filterShopType = ref('');
const filterShopCountry = ref(''); 

const isEditModeCompanies = ref(false);
const isEditModeReasons = ref(false); 
const isEditModeShops = ref(false); 

const companyData = ref([]);
const reasonData = ref([]);
const shopData = ref([]); 

const addCompanyModalOpen = ref(false); 
const addShopModalOpen = ref(false); 

const isSortModeCompanies = ref(false); 
const isSortModeReasons = ref(false); 
const isSortModeShops = ref(false); 
const isSortModeAdmins = ref(false); 

const sortColumn = ref('companyName'); 
const sortDirection = ref('asc'); 
const sortColumnShops = ref('type'); 
const sortDirectionShops = ref('asc'); 
const sortColumnAdmins = ref('email');
const sortDirectionAdmins = ref('asc');

const frozenData = ref([]); 
const frozenShopData = ref([]); 

const administrators = ref([]); 
const companyReasonsData = ref([]); 
const currentUserRole = ref(Number(localStorage.getItem('userRole')));
const router = useRouter();

const confirmModalOpen = ref(false) 
const confirmTitle = ref('Confirm action')
const confirmMessage = ref('')
const confirmAction = ref(null)

const openConfirmModal = (message, action, title = 'Confirm deletion') => {
  confirmTitle.value = title
  confirmMessage.value = message
  confirmAction.value = action
  confirmModalOpen.value = true
}

const closeConfirmModal = () => {
  confirmModalOpen.value = false
  confirmTitle.value = 'Confirm action'
  confirmMessage.value = ''
  confirmAction.value = null
}

const executeConfirmAction = async () => {
  if (confirmAction.value) {
    await confirmAction.value()
  }
  closeConfirmModal()
}

const handleLogout = () => {
    logout();
    router.push('/roleselect');
};

function showCustomAlert(message) {
  const oldBox = document.querySelector('.alert-box')
  if (oldBox) oldBox.remove()

  const box = document.createElement('div')
  box.className = 'alert-box'

  const text = document.createElement('span')
  text.className = 'alert-text'
  text.textContent = message

  const btn = document.createElement('button')
  btn.className = 'alert-btn'
  btn.textContent = 'OK'
  btn.onclick = () => box.remove()

  box.appendChild(text)
  box.appendChild(btn)
  document.body.appendChild(box)
}

const fetchAdministrators = async () => {
    try {
        const res = await axios.get(`${API_BASE}/administrators`);
        administrators.value = Array.isArray(res.data) ? res.data : []; 
    } catch (error) {
        console.error('Failed to load administrators:', error); 
        administrators.value = [];
    }
}; 

const fetchCompanyReasons = async () => {
    try {
        const res = await axios.get(`${API_BASE}/companyreasons`);
        companyReasonsData.value = res.data;
    } catch (error) {
        console.warn('Could not load company-reasons bridging table:', error);
    }
}; 

const toggleSortModeReasons = () => {
    if(!isSortModeReasons.value) {
        isEditModeReasons.value = false;
    }
    isSortModeReasons.value = !isSortModeReasons.value; 
}; 

const toggleSortModeCompanies = () => {
    if (!isSortModeCompanies.value) {
        isEditModeCompanies.value = false; 
    } 
    isSortModeCompanies.value = !isSortModeCompanies.value; 
}; 

const toggleSortModeShops = () => {
    if (!isSortModeShops.value) {
        isEditModeShops.value = false; 
    }
    isSortModeShops.value = !isSortModeShops.value; 
}; 

const toggleEditModeCompanies = () => {
    if (!isEditModeCompanies.value) {
        isSortModeCompanies.value = false; 
        frozenData.value = [...sortedCompanyData.value];
    } else {
        frozenData.value = [];
    }
    isEditModeCompanies.value = !isEditModeCompanies.value;
};

const toggleEditModeShops = () => {
    if (!isEditModeShops.value) {
        isSortModeShops.value = false; 
        frozenShopData.value = [...sortedShopData.value]; 
    } else {
        frozenShopData.value = []; 
    } 
    isEditModeShops.value = !isEditModeShops.value; 
}; 

const toggleSortModeAdmins = () => {
    isSortModeAdmins.value = !isSortModeAdmins.value;
}; 

const handleHeaderSortAdmins = (column) => {
    if (sortColumnAdmins.value === column) {
        sortDirectionAdmins.value = sortDirectionAdmins.value === 'asc' ? 'desc' : 'asc';
    } else {
        sortColumnAdmins.value = column;
        sortDirectionAdmins.value = 'asc';
    }
};

const openAddShopModal = () => {
    isSortModeShops.value = false;
    isEditModeShops.value = false;
    addShopModalOpen.value = true;
};

const closeAddShopModal = () => {
    addShopModalOpen.value = false;
};

const sortedCompanyData = computed(() => {
    if (isEditModeCompanies.value && frozenData.value.length > 0) {
        return frozenData.value; 
    }

    let filtered = companyData.value; 

    if (query.value) {
        const search = query.value.toLowerCase();
        filtered = filtered.filter(row => {
            return (
                row.companyName.toLowerCase().includes(search) || 
                row.country.toLowerCase().includes(search) ||
                (row.rimiEmployeeEmail && row.rimiEmployeeEmail.toLowerCase().includes(search))
            ); 
        }); 
    }

    if (filterCountry.value) {
        filtered = filtered.filter(row => row.country === filterCountry.value); 
    }

    if (filterRem.value) {
        filtered = filtered.filter(row => row.rimiEmployeeEmail === filterRem.value); 
    }

    const result = [...filtered].sort((a, b) => {
        let valA = a[sortColumn.value] || '';
        let valB = b[sortColumn.value] || ''; 

        if (typeof valA === 'string') valA = valA.toLowerCase(); 
        if (typeof valB === 'string') valB = valB.toLowerCase(); 

        if (valA < valB) return sortDirection.value === 'asc' ? -1 : 1; 
        if (valA > valB) return sortDirection.value === 'asc' ? 1 : -1; 
        return 0; 
    });

    frozenData.value = result; 
    return result; 
});

const sortedShopData = computed(() => {
    if (isEditModeShops.value && frozenShopData.value.length > 0) {
        return frozenShopData.value; 
    }

    let filtered = shopData.value; 

    if (queryShops.value) {
        const search = queryShops.value.toLowerCase();
        filtered = filtered.filter(row => 
            row.type?.toLowerCase().includes(search) ||
            row.country?.toLowerCase().includes(search) ||
            row.address?.toLowerCase().includes(search) ||
            row.code?.toLowerCase().includes(search) 
        );
    }

    if (filterShopType.value) {
        filtered = filtered.filter(row => row.type === filterShopType.value);
    }

    if (filterShopCountry.value) {
        filtered = filtered.filter(row => row.country === filterShopCountry.value); 
    }

    return [...filtered].sort((a, b) => {
        let valA = a[sortColumnShops.value] || '';
        let valB = b[sortColumnShops.value] || ''; 
        if (typeof valA === 'string') valA = valA.toLowerCase();
        if (typeof valB === 'string') valB = valB.toLowerCase();
        if (valA < valB) return sortDirectionShops.value === 'asc' ? -1 : 1;
        if (valA > valB) return sortDirectionShops.value === 'asc' ? 1 : -1;
        return 0;
    }); 
}); 

const sortedAdminData = computed(() => {
    let filtered = (administrators.value || []).filter(row => 
        Number(row.typeOfAdmin) !== 3); 

    if (filterAdminType.value !== '') {
        filtered = filtered.filter(row => Number(row.typeOfAdmin) === Number(filterAdminType.value));
    }

    if (queryAdmins.value) {
        const search = queryAdmins.value.toLowerCase();
        filtered = filtered.filter(row => 
            (row.email || '').toLowerCase().includes(search) || 
            (row.firstName || '').toLowerCase().includes(search) ||
            (row.lastName || '').toLowerCase().includes(search)
        );
    }

        return [...filtered].sort((a, b) => {
            let valA = a[sortColumnAdmins.value] || ''; 
            let valB = b[sortColumnAdmins.value] || ''; 

            if (typeof valA === 'string') valA = 
        valA.toLowerCase(); 
            if (typeof valB === 'string') valB = 
        valB.toLowerCase(); 

            if (valA < valB) return sortDirectionAdmins.value === 
        'asc' ? -1 : 1;
            if (valA > valB) return sortDirectionAdmins.value === 
        'asc' ? 1 : -1;

            return 0;
        }); 
    }
); 

const formatAdminType = (typeValue) => {
    if (typeof typeValue === 'string') return typeValue;
    
    switch(Number(typeValue)) {
        case 1: return 'Real Estate';
        case 2: return 'Admin';
        case 3: return 'Super Admin';
        default: return 'Unknown';
    }
};

const filteredReasonData = computed(() => {
    if (!queryReasons.value) {
        return reasonData.value; 
    }

    const search = queryReasons.value.toLowerCase(); 
    return reasonData.value.filter(row => 
        row.reasonName?.toLowerCase().includes(search)
    ); 
}); 

const addReasonModalOpen = ref(false); 

const openAddReasonModal = () => {
    addReasonModalOpen.value = true; 
}; 

const openAddCompanyModal = () => {
    isSortModeCompanies.value = false;
    isEditModeCompanies.value = false; 
    addCompanyModalOpen.value = true; 
}; 

const closeAddReasonModal = () => {
    addReasonModalOpen.value = false; 
}; 

const closeAddCompanyModal = () => {
    addCompanyModalOpen.value = false; 
}; 

const toggleEditModeReasons = () => {
    isEditModeReasons.value = !isEditModeReasons.value; 
}; 

const addReason = async (formData) => {
    try {
        await axios.post(`${API_BASE}/reasons`, formData); 
        closeAddReasonModal();
        await fetchReasons(); 
        await fetchCompanyReasons(); 
        await fetchCompanies(); 
    } catch (error) {
        console.error('Reason add failed:', error.message); 
    }
}; 

const handleHeaderSort = (column) => {
    if (sortColumn.value === column) {
        sortDirection.value = sortDirection.value === 'asc' ? 'desc' : 'asc'; 
    } else {
        sortColumn.value = column;
        sortDirection.value = 'asc'; 
    }
}; 

const handleHeaderSortShops = (column) => {
    if (sortColumnShops.value === column) {
        sortDirectionShops.value = sortDirectionShops.value === 'asc' ? 'desc' : 'asc';
    } else {
        sortColumnShops.value = column;
        sortDirectionShops.value = 'asc';
    }
};

const handleAddReasonToCompany = async (reasonId) => {
    try {
        const payload = {
            CompaniesCompanyID: selectedUser.value.companyId, 
            ReasonsReasonID: Number(reasonId)
        }; 

        await axios.post(`${API_BASE}/companyreasons`, payload); 
        await fetchCompanies(); 
        await fetchCompanyReasons(); 

        const addedReason = reasonData.value.find(r => Number(r.id) === Number(reasonId));
        if (addedReason) {
            selectedUser.value.reasons.push(addedReason);
        } 
    } catch (error) {
        console.error('Failed to add reason to company:', error);
    }
}; 

const addCompany = async (formData) => {
    try {
        await axios.post(`${API_BASE}/companies`, formData);
        closeAddCompanyModal();
        await fetchCompanies(); 
        await fetchCompanyReasons();
        await fetchReasons(); 

    } catch (error) {
        console.error('Company add failed:', error.message); 
    }
}; 

const addShop = async (formData) => {
  try {
    await axios.post(`${API_BASE}/shops`, formData);
    closeAddShopModal();
    await fetchShops();
  } catch (error) {
    const msg = error.response?.data?.error;
    if (msg) {
      showCustomAlert(msg); // shows "A shop with this email already exists." from backend
    } else {
      showCustomAlert('Failed to add shop. Please try again.');
    }
  }
};

const remAdministrators = computed(() => {
    return administrators.value.filter(admin => admin.typeOfAdmin === 1);
});

const updateCompany = async (row) => { 
    try {
        const payload = {
            CompanyID: row.id, 
            CompanyName: row.companyName,
            Country: getCountryCode(row.country), 
            Address: row.address, 
            PhoneNumber: row.phoneNumber, 
            RemID: row.remID,
            RimiEmployeeEmail: row.rimiEmployeeEmail,
            Password: undefined
        }; 

        await axios.put(`${API_BASE}/companies/${row.id}`, payload);
        await fetchCompanies(); 
    } catch (error) {
        console.error('Update failed:', error); 
        await fetchCompanies();
    }
}; 

const updateReason = async (row) => {
    const numericId = parseInt(row.id);
    const numericCompanyId = parseInt(row.companyID); 

    try {
        const payload = {
            ReasonID: numericId,
            Name: row.reasonName,
            CompanyID: numericCompanyId, 
            Shifts: [], 
            CompanyReasons: []
        };

        await axios.put(`${API_BASE}/reasons/${numericId}`, payload);
        
        await fetchReasons(); 
        isEditModeReasons.value = false;
    } catch (error) {
        console.error('Update failed details:', error);
        await fetchReasons(); 
    }
};

const updateShop = async (row) => {
  try {
    const original = shopData.value.find(s => s.id === row.id);
    const emailChanged = original && original.email !== row.email;

    await axios.put(`${API_BASE}/shops/${row.id}`, {
      shopID: row.id,
      type: row.type,
      country: row.country,
      address: row.address,
      code: row.code,
      email: row.email
    });

    if (emailChanged && row.email) {
      await axios.post(`${API_BASE}/shops/${row.id}/resend-setup-email`);
    }

    await fetchShops();
  } catch (error) {
    const msg = error.response?.data?.error;
    if (msg) {
      showCustomAlert(msg); // shows "A shop with this email already exists." from backend
    } else {
      showCustomAlert('Failed to update shop. Please try again.');
    }
    // Revert the row back to original values so the table doesn't show the bad email
    await fetchShops();
  }
};

const deleteCompany = async (id, companyName) => {
  openConfirmModal(`Delete company "${companyName}"?`, async () => {
    try {
      await axios.delete(`${API_BASE}/companies/${id}`)
      await fetchCompanies()
    } catch (error) {
      console.error('Delete failed:', error)
      await fetchCompanies()
    }
  })
}

const deleteShop = async (id, code) => {
  openConfirmModal(`Delete shop "${code}"?`, async () => {
    try {
      await axios.delete(`${API_BASE}/shops/${id}`)
      await fetchShops()
    } catch (error) {
      console.error('Shop delete failed:', error)
      await fetchShops()
    }
  })
}

const deleteReason = async (id, reasonName) => {
  openConfirmModal(`Delete reason "${reasonName}"?`, async () => {
    try {
      await axios.delete(`${API_BASE}/reasons/${id}`)
      reasonData.value = reasonData.value.filter(row => row.id !== id)
      await fetchReasons()
      await fetchCompanyReasons()
      await fetchCompanies()
    } catch (error) {
      console.error('Failed to delete reason:', error)
    }
  })
} 

const handleRemoveReasonFromCompany = async (reasonId) => {
  openConfirmModal('Remove this reason from this company?', async () => {
    try {
      await axios.delete(`${API_BASE}/companyreasons/${selectedUser.value.companyId}/${reasonId}`)
      await fetchCompanies()
      await fetchCompanyReasons()
      selectedUser.value.reasons = selectedUser.value.reasons.filter(r => r.id !== reasonId)
    } catch (error) {
      console.error('Failed to remove reason:', error)
    }
  })
}

const fetchCompanies = async () => {
    try {
        const res = await axios.get(`${API_BASE}/companies`);
        companyData.value = res.data.map(c => ({
            id: c.companyID,
            companyName: c.companyName,
            country: getCountryName(c.country),
            address: c.address,
            rimiEmployeeEmail: c.rimiEmployeeEmail,
            phoneNumber: c.phoneNumber,
            remID: c.remID,
            reasonIds: (c.companyReasons || c.CompanyReasons || []).map(r => r.reasonID)  
        }));
    } catch (error) {
        console.error('Failed to load companies:', error);
    }
}
 
// const fetchShifts = async () => {
//     try {
//         const res = await axios.get(`${API_BASE}/shifts`);
//         shiftDataArray.value = res.data;
//     } catch (error) {
//         console.error('Failed to load shifts:', error);
//     }
// };
 
const fetchReasons = async () => {
    try {
        const res = await axios.get(`${API_BASE}/reasons`);
        reasonData.value = res.data.map(r => ({
            id: r.reasonID,
            reasonName: r.name,
            companyID: r.companyID, 
            companyName: r.companyName
        }));
        await nextTick();
    } catch (error) {
        console.error('Failed to load reasons', error);
    }
};

const fetchShops = async () => {
    try {
        const res = await axios.get(`${API_BASE}/shops`);
        shopData.value = res.data.map(s => ({
            id: s.shopID, 
            type: s.type, 
            country: s.country, 
            address: s.address, 
            code: s.code,
            email: s.email
        })); 
    } catch (error) {
        console.error('Failed to fetch shops data:', error);; 
    }
}; 

const getCountryCode = (countryName) => {
    const countryMap = {
        'Lithuania': 1, 
        'Latvia': 2, 
        'Estonia' : 3,
        'Baltics' : 4
    }; 
    return countryMap[countryName]; 
}; 

const getCountryName = (countryCode) => {
    const countryMap = {
        1: 'Lithuania',
        2: 'Latvia', 
        3: 'Estonia',
        4: 'Baltics'
    }; 

    return countryMap[Number(countryCode)] || countryCode;
}; 

onMounted(async () => { 
    fetchCompanies();
    fetchReasons();  
    fetchAdministrators();
    fetchShops(); 
    fetchCompanyReasons();
    //fetchEmployees();
    //fetchShifts();
});
 
const openCompanyModal = async (company) => {
  const mappingsForCompany = companyReasonsData.value.filter(cr => 
    Number(cr.companiesCompanyID || cr.CompaniesCompanyID || cr.companyID || cr.CompanyID) === Number(company.id)
  ); 

  const associatedReasonIds = mappingsForCompany.map(cr => 
    Number(cr.reasonsReasonID || cr.ReasonsReasonID || cr.reasonID || cr.ReasonID)
  );
  
  const companyReasons = reasonData.value.filter(r => 
    associatedReasonIds.includes(Number(r.id))
  ); 

  selectedUser.value = { 
    companyId: company.id, 
    companyName: company.companyName, 
    reasons: companyReasons
  };
  
  try {
    const res = 
  await axios.get(`${API_BASE}/shifts/bycompany/${company.id}`);
    selectedShift.value = res.data;  
  } catch (error) {
    console.error('Company shifts error:', error);
    selectedShift.value = [];
  }
  modalOpen.value = true;
};

const closeModal = () => {
    modalOpen.value = false;
};
</script>

<style scoped>
</style>
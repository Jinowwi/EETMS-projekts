<template>
  <div class="page-content">
    <div class="blob blob-teal"></div>
    <div class="blob blob-pink"></div>
    <button class="logout-btn" @click="handleLogout">
      <FontAwesomeIcon icon="fa-solid fa-right-from-bracket" />
      Logout
    </button>
    <h1>Company Profile</h1>

    <div class="profile-container">
      <div class="profile-card">
        <div class="section-header">
          <h2>Account Information</h2>
          <div class="buttons">
            <button 
              :class="['btn', { 'active-btn': isEditMode }]" 
              @click="toggleEditMode"
              title="Edit Profile"
            >
              <FontAwesomeIcon :icon="['fas', 'pen']" />
            </button>
          </div>
        </div>

        <div v-if="error" class="error-state">
          {{ error }}
        </div>

        <form v-else @submit.prevent="saveProfile" class="profile-form">
          <div class="form-grid">
            <div class="form-group">
              <label>Company Email</label>
              <template v-if="!isEditMode">
                <div class="static-value">{{ companyData.email || 'Not provided' }}</div>
              </template>
              <input v-else v-model="editForm.email" type="email" class="edit-input" requred />
            </div>

            <div class="form-group">
              <label>Company Name</label>
              <template v-if="!isEditMode">
                <div class="static-value">{{ companyData.companyName || 'Not provided' }}</div>
              </template>
              <input v-else v-model="editForm.companyName" type="text" class="edit-input" required />
            </div>

            <div class="form-group">
              <label>Registration Number</label>
              <template v-if="!isEditMode">
                <div class="static-value">{{ companyData.registrationNumber || 'Not provided' }}</div>
              </template>
              <input v-else v-model="editForm.registrationNumber" type="text" class="edit-input" required />
            </div>

            <div class="form-group">
              <label>Phone Number</label>
              <template v-if="!isEditMode">
                <div class="static-value">{{ companyData.phoneNumber || 'Not provided' }}</div>
              </template>
              <input v-else v-model="editForm.phoneNumber" type="tel" class="edit-input" />
            </div>

            <div class="form-group">
              <label>Country</label>
              <template v-if="!isEditMode">
                <div class="static-value">
                  <span class="country-badge">{{ getCountryName(Number(companyData.country)) || 'Not selected' }}</span>
                </div>
              </template>
              <select v-else v-model.number="editForm.country" class="edit-select" required>
                <option value="1">Lithuania</option>
                <option value="2">Latvia</option>
                <option value="3">Estonia</option>
                <option value="4">Baltics</option>
              </select>
            </div>

            <div class="form-group">
              <label>Address</label>
              <template v-if="!isEditMode">
                <div class="static-value">{{ companyData.address || 'Not provided' }}</div>
              </template>
              <input v-else v-model="editForm.address" type="text" class="edit-input" />
            </div>
          </div>

          <div v-if="isEditMode" class="form-actions slide-down">
            <button type="button" class="btn-cancel" @click="cancelEdit">Cancel</button>
            <button type="submit" class="btn-save-override" :disabled="saving">
              {{ saving ? 'Saving...' : 'Submit' }}
            </button>
          </div>

          <div v-if="successMessage" class="confirm-overlay" @click="successMessage = ''">
            <div class="confirm-box" @click.stop>
              <div class="confirm-body">
                <p class="confirm-message">{{ successMessage }}</p>
              </div>

              <div class="confirm-actions">
                <button class="confirm-delete" @click="successMessage = ''">
                  OK
                </button>
              </div>
            </div>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue';
import axios from 'axios'; 
import { FontAwesomeIcon } from '@fortawesome/vue-fontawesome';
import { useRouter } from 'vue-router';
import { logout } from '@/services/auth.js'

const API_BASE = 'http://localhost:5001/api'; 
const currentCompanyId = ref(parseInt(localStorage.getItem('companyId') || '0'));

const isEditMode = ref(false);
const saving = ref(false);
const error = ref('');
const successMessage = ref('');

const companyData = ref({});
const editForm = ref({});

const router = useRouter(); 

const handleLogout = () => {
  logout();
  router.push('/roleselect');
};

const getCountryName = (val) => {
  const countries = {
    1: 'Lithuania',
    2: 'Latvia', 
    3: 'Estonia',
    4: 'Baltics'
  };
  return countries[val] || 'Unknown';
};

const fetchCompanyProfile = async () => {
  if (!currentCompanyId.value) {
    error.value = 'No company ID found. Please log in.';
    return;
  }
  
  try {
    const res = await axios.get(`${API_BASE}/companies/${currentCompanyId.value}`);
    companyData.value = res.data;
    populateEditForm();
    error.value = '';
  } catch (err) {
    console.error('Failed to load profile:', err);
    error.value = err.response?.data || 'Failed to load company information.';
  }
};

const populateEditForm = () => {
  editForm.value = {
    email: companyData.value.email || '', 
    companyName: companyData.value.companyName || '',
    address: companyData.value.address || '',
    phoneNumber: companyData.value.phoneNumber || '',
    country: Number(companyData.value.country) || 2,
    registrationNumber: companyData.value.registrationNumber || '',
    password: ''
  };
};

const toggleEditMode = () => {
  isEditMode.value = !isEditMode.value;
  if (isEditMode.value) {
    successMessage.value = '';
    populateEditForm();
  }
};

const cancelEdit = () => {
  isEditMode.value = false;
};

const saveProfile = async () => {
  saving.value = true;
  error.value = '';
  successMessage.value = '';

  try {
    const payload = {
    CompanyID: currentCompanyId.value,
    CompanyName: editForm.value.companyName,
    Country: parseInt(editForm.value.country),
    Address: editForm.value.address,
    PhoneNumber: editForm.value.phoneNumber,
    RegistrationNumber: editForm.value.registrationNumber,
    Email: editForm.value.email,
    RemID: companyData.value.remID,
    RimiEmployeeEmail: companyData.value.rimiEmployeeEmail || null
    };

    if (editForm.value.password) {
      payload.Password = editForm.value.password; 
    }

    await axios.put(`${API_BASE}/companies/${currentCompanyId.value}`, payload);
    
    await fetchCompanyProfile(); 
    
    successMessage.value = 'Profile updated successfully!';
    isEditMode.value = false;
    
  } catch (err) {
    console.error('Failed to update profile:', err);
    error.value = err.response?.data?.message || 'Failed to update profile.';
  } finally {
    saving.value = false;
  }
};

onMounted(() => {
  fetchCompanyProfile();
});
</script>

<style scoped>
.page-content {
    display: flex;
    flex-direction: column;
    align-items: center;
    width: 100%;
    min-height: 100vh;
}

.profile-container {
    width: 100%;
    padding: 20px;
    box-sizing: border-box;
    display: flex;
    justify-content: center;
}

.profile-card {
    display: flex;
    flex-direction: column;
    gap: 0;
    background: var(--color-white);
    border-radius: 12px;
    overflow: hidden;
    box-shadow: 0 4px 20px var(--shadow-box);
    width: 100%;
    max-width: 800px;
    margin-left: var(--navbar-width, 85px);
    transition: transform 0.3s ease, box-shadow 0.3s ease;
}

.section-header {
    display: flex;
    align-items: center;
    justify-content: space-between;
    padding: 0;
}

.section-header h2 {
    margin: 0;
    flex: 1;
    text-align: left;
    font-size: 25px;
    font-family: 'Inter';
    font-weight: 600;
    padding: 12px 20px;
    background: var(--header-gradient);
    border-radius: 8px 8px 0 0;
    color: var(--color-white);
}

.buttons {
    display: flex;
    gap: 10px;
    padding: 12px 20px;
}

.btn {
    width: 42px;
    height: 42px;
    border: 2px solid transparent;
    background: var(--color-white);
    border-radius: 50%;
    display: flex;
    align-items: center;
    justify-content: center;
    transition: all 0.3s ease;
    flex-shrink: 0;
    cursor: pointer;
    color: var(--brand-berry);
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
}

.btn:hover,
.btn.active-btn {
    background: var(--header-gradient);
    transform: scale(1.1) rotate(5deg);
    color: var(--color-white);
    box-shadow: 0 4px 15px rgba(161, 41, 113, 0.4);
}

.btn:active {
    transform: scale(0.95);
}

.profile-form {
    padding: 30px;
}

.form-grid {
    display: grid;
    grid-template-columns: 1fr 1fr;
    gap: 30px 20px;
}

.form-group {
    display: flex;
    flex-direction: column;
}

.form-group.full-width {
    grid-column: 1 / -1;
}

label {
    font-family: 'Inter';
    font-size: 14px;
    font-weight: 600;
    color: var(--brand-berry);
    margin-bottom: 8px;
    text-transform: uppercase;
    letter-spacing: 0.5px;
}

.static-value {
    font-family: 'Inter';
    font-size: 16px;
    color: var(--color-text-main);
    padding: 5px 0;
    min-height: 25px;
    border-bottom: 1px solid var(--color-border);
}

.country-badge {
    background: var(--color-bg-light);
    padding: 6px 14px;
    border-radius: 12px;
    font-size: 14px;
    font-weight: 600;
    color: var(--brand-teal);
    border: 1px solid var(--color-border);
}

.edit-input, 
.edit-select {
    width: 100%; 
    height: 40px; 
    border: 2px solid var(--color-border); 
    border-radius: 40px;
    font-family: 'Inter';
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.05);
    padding: 0 15px;
    transition: all 0.2s ease;
    box-sizing: border-box;
}

.edit-input:focus, 
.edit-select:focus {
    border-color: var(--brand-teal);
    outline: none;
    box-shadow: 0 2px 8px rgba(43, 164, 146, 0.2);
}

.form-actions {
    display: flex;
    justify-content: flex-end;
    gap: 15px;
    margin-top: 30px;
    padding-top: 20px;
    border-top: 1px solid var(--color-border);
}

.btn-cancel {
    padding: 10px 24px;
    background: var(--color-white);
    border: 2px solid var(--color-border);
    border-radius: 40px;
    font-family: 'Inter';
    font-weight: 600;
    color: var(--color-text-dim);
    cursor: pointer;
    transition: all 0.3s ease;
}

.btn-cancel:hover {
    background: var(--header-gradient);
    transform: translateY(-2px);
    box-shadow: 0 6px 20px  var(--color-border);
    color: var(--color-white);
}

.btn-save-override {
    padding: 10px 24px;
    background: var(--color-white);
    border: 2px solid var(--color-border);
    border-radius: 40px;
    font-family: 'Inter';
    font-weight: 600;
    color: var(--color-text-dim);
    cursor: pointer;
    transition: all 0.3s ease;
}

.btn-save-override:hover {
    background: var(--header-gradient);
    transform: translateY(-2px);
    box-shadow: 0 6px 20px var(--color-border);
    color: var(--color-white);
}

.error-state {
    font-family: 'Inter';
    color: var(--color-warning-text);
    background: var(--color-warning-bg);
    border: 1px solid var(--color-warning-border);
    padding: 15px;
    border-radius: 8px;
    margin: 20px;
    text-align: center;
}

.slide-down {
    animation: slideDown 0.3s ease-out;
}

@keyframes slideDown {
    from { opacity: 0; transform: translateY(-10px); }
    to { opacity: 1; transform: translateY(0); }
}

@media (max-width: 1400px) {
    .page-content h1 {
        font-size: 48px;
        margin-top: 0;
    }
}

@media (max-width: 1024px) {
    .page-content h1 {
        font-size: 42px;
        margin-left: 0;
    }
    
    .profile-card {
        margin-left: 0;
        max-width: 90%;
    }
}

@media (max-width: 768px) {
    .page-content h1 {
        font-size: 36px;
    }

    .form-grid {
        grid-template-columns: 1fr;
        gap: 20px;
    }
    
    .section-header h2 {
        font-size: 20px;
    }
}

@media (max-width: 480px) {
    .page-content h1 {
        font-size: 28px;
    }

    .profile-form {
        padding: 20px;
    }
    
    .section-header h2 {
        font-size: 16px;
    }

    .btn {
        width: 36px;
        height: 36px;
    }
}
</style>

<template>
    <div class="profile-container"> 
        <!-- <Navbar /> -->
        <h1>External Employee Information</h1>
        <div class="content-wrapper"> 
            <div class="profile-picture">
                <svg class="placeholder-icon" viewBox="0 0 200 200" xmlns="http://www.w3.org/2000/svg" aria-hidden="true">
                    <circle cx="100" cy="60" r="40" fill="#e0e0e0"/>
                    <path d="M 40 160 Q 40 120, 100 120 Q 160 120, 160 160" fill="#e0e0e0"/>
                </svg>
            </div>

            <div class="user-info">
                <div class="info-row">
                    <span class="label">Name:</span> Aaaa Aaaa
                </div>
                <div class="info-row">
                    <span class="label">Email:</span> aaaa.aaaa@example.com
                </div>
                <div class="info-row">
                    <span class="label">Phone:</span> +371 123 4567
                </div>
                <div class="info-row">
                    <span class="label">Position:</span> Software Developer
                </div>
                <div class="info-row">
                    <span class="label">Department:</span> IT Services
                </div>
                <div class="info-row">
                    <span class="label">Location:</span> Riga, Latvia
                </div>
            </div>
        </div>
        <div class="table-container">
            <div class="table-section">
                <div class="section-header">
                    <h2>Shift Data</h2>
                    <div class="buttons">
                        <button class="btn">
                            <FontAwesomeIcon :icon="['fas', 'sliders-h']" />
                        </button>
                        <button class="btn">
                            <FontAwesomeIcon :icon="['fas', 'pen']" />
                        </button>
                        <button class="btn">
                            <FontAwesomeIcon :icon="['fas', 'plus']" />
                        </button>
                        <button class="btn">
                            <FontAwesomeIcon :icon="['fas', 'ellipsis-v']" />
                        </button>
                    </div>
                </div>
                <table>
                    <thead>
                        <tr>
                            <th>User ID</th>
                            <th>Full Name</th>
                            <th>Date</th>
                            <th>Start Shift Time</th>
                            <th>End Shift Time</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr v-for="row in tableData" :key="row.id">
                            <td>{{ row.id }}</td>
                            <td>{{ row.name }}</td>
                            <td>{{ row.date }}</td>
                            <td>{{ row.startShiftTime }}</td>
                            <td>{{ row.endShiftTime }}</td>
                        </tr>
                    </tbody>
                </table>
            </div>
        </div>
    </div>
</template>

<script setup>
import { ref } from 'vue';
// import Navbar from '../components/navbar.vue';
import { FontAwesomeIcon } from '@fortawesome/vue-fontawesome';

const tableData = ref([
    { id: 1, name: 'Aaaa Aaaa', date: '01.06.2025', startShiftTime: '09:00', endShiftTime: '17:00' },
    { id: 1, name: 'Aaaa Aaaa', date: '02.06.2025', startShiftTime: '09:00', endShiftTime: '17:00' },
    { id: 1, name: 'Aaaa Aaaa', date: '03.06.2025', startShiftTime: '09:00', endShiftTime: '17:00' },
    { id: 1, name: 'Aaaa Aaaa', date: '04.06.2025', startShiftTime: '09:00', endShiftTime: '17:00' },
    { id: 1, name: 'Aaaa Aaaa', date: '05.06.2025', startShiftTime: '09:00', endShiftTime: '17:00' },
]);
</script>

<style scoped>
/* Profila sadaļas stili */
.profile-container {
    display: flex;
    flex-direction: column;
    align-items: center;
    width: 100%;
}

.content-wrapper {
    margin-left: 85px;
    display: flex;
    gap: 40px;
    align-items: flex-start;
    padding: 20px;
}

.profile-picture {
    width: 300px;
    height: 300px;
    margin-top: 30px;
    background-color: #f5f5f5;
    display: flex;
    align-items: center;
    justify-content: center;
    border: 2px solid #ddd;
    border-radius: 10px;
    flex-shrink: 0;
}

.placeholder-icon {
    width: 200px;
    height: 200px;
    opacity: 0.6;
}

.user-info {
    display: flex;
    flex-direction: column;
    gap: 25px;
    padding-top: 20px;
    font-size: 25px;
    font-family: 'Inter';
    margin-bottom: 120px;
}

.label {
    font-weight: bold;
    font-size: 25px;
}

.info-row {
    font-size: 22px;
    word-wrap: break-word;
    overflow-wrap: break-word;
}

/* RESPONSIVE FIXES */

/* Tablet */
@media (max-width: 1024px) {
    .content-wrapper {
        margin-left: 100px;
        margin-right: 20px;
    }
    
    .user-info {
        font-size: 22px;
    }
    
    .label {
        font-size: 22px;
    }
    
    .info-row {
        font-size: 20px;
    }
}

/* Mobile landscape and tablets */
@media (max-width: 768px) {
    .content-wrapper {
        margin-left: 20px;
        margin-right: 20px;
        flex-direction: column;
        align-items: center;
    }

    .profile-picture {
        width: 250px;
        height: 250px;
    }

    .placeholder-icon {
        width: 150px;
        height: 150px;
    }

    .user-info {
        width: 100%;
        max-width: 500px;
        font-size: 20px;
    }

    .info-row {
        font-size: 18px;
    }

    .label {
        font-size: 20px;
    }
}

/* Mobile */
@media (max-width: 480px) {
    .content-wrapper {
        margin-left: 10px;
        margin-right: 10px;
        padding: 10px;
        gap: 20px;
    }

    .profile-picture {
        width: 200px;
        height: 200px;
        margin-top: 10px;
    }

    .placeholder-icon {
        width: 120px;
        height: 120px;
    }

    .user-info {
        gap: 20px;
        padding-top: 10px;
        margin-bottom: 40px;
        font-size: 16px;
    }

    .info-row {
        font-size: 14px;
    }

    .label {
        font-size: 16px;
    }
}

/* Extra small mobile */
@media (max-width: 375px) {
    .user-info {
        font-size: 14px;
    }
    
    .info-row {
        font-size: 12px;
    }
    
    .label {
        font-size: 14px;
    }
}
</style>

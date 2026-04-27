<template>
    <div class="navbar">
        <div class="hamburger" @click="collapsed = !collapsed">
            <FontAwesomeIcon icon="fa-solid fa-bars" />
        </div>

        <div class="nav-panel" :class="{ collapsed }">
            <div class="nav-icons">
                <div
                    class="nav-icon"
                    v-if="visibleIcons.includes('home')"
                    :class="{ active: activeIcon === 'home' }"
                    @click="navigate('home')">

                    <FontAwesomeIcon icon="fa-solid fa-house" />

                    <span class="nav-label">Home</span>
                </div>

                <div
                    class="nav-icon"
                    v-if="visibleIcons.includes('list')"
                    :class="{ active: activeIcon === 'list' }"
                    @click="navigate('list')">

                    <FontAwesomeIcon icon="fa-solid fa-list" />

                    <span class="nav-label">Data</span>
                </div>

                <div
                    class="nav-icon"
                    v-if="visibleIcons.includes('calendar')"
                    :class="{ active: activeIcon === 'calendar' }"
                    @click="navigate('calendar')">

                    <FontAwesomeIcon icon="fa-solid fa-calendar" />

                    <span class="nav-label">Calendar</span>
                </div>

                <div
                    class="nav-icon"
                    v-if="visibleIcons.includes('message')"
                    :class="{ active: activeIcon === 'message' }"
                    @click="navigate('message')">
                    <FontAwesomeIcon icon="fa-solid fa-message" />
                    <span class="nav-label">Notifications</span>
                </div>

                <div
                    class="nav-icon"
                    v-if="visibleIcons.includes('profile')"
                    :class="{ active: activeIcon === 'profile'}"
                    @click ="navigate('profile')">
                    <FontAwesomeIcon icon="fa-solid fa-address-book" />
                    <span class="nav-label">Profile</span>
                </div>
                
                <div
                    class="nav-icon"
                    v-if="visibleIcons.includes('chart')"
                    :class="{ active: activeIcon === 'chart' }"
                    @click="navigate('chart')">
                    <FontAwesomeIcon icon="fa-solid fa-chart-area" />
                    <span class="nav-label">Statistics</span>
                </div>

                <div class="nav-icon" v-if="visibleIcons.includes('company-chart')"
                    :class="{ active: activeIcon === 'company-chart' }" @click="navigate('company-chart')">
                    <FontAwesomeIcon icon="fa-solid fa-chart-area" />
                    <span class="nav-label">Statistics</span>
                </div>
            </div>
        </div>
    </div>
</template>

<script setup>
import { ref, computed, watch, onMounted, nextTick } from 'vue';
import { useRouter, useRoute } from 'vue-router';
import { FontAwesomeIcon } from '@fortawesome/vue-fontawesome';
import { getAdminRoleLevel } from '@/services/auth.js'

const roleLevel = ref(Number(getAdminRoleLevel()));

const router = useRouter();
const route = useRoute();
const activeIcon = ref('home');
const collapsed = ref(false);

const visibleIcons = computed(() => {    
    if (!roleLevel.value || roleLevel.value === 0) {
        return ['profile', 'chart'];
    }
    
    if (roleLevel.value === 3) return ['home', 'list', 'calendar', 'message', 'chart'];
    if (roleLevel.value === 2) return ['list', 'message'];
    if (roleLevel.value === 1) return ['home', 'calendar', 'message', 'chart'];
    return ['home'];
});

const routeToIcon = (r) => {
    const name = r?.name || r?.matched?.[0]?.name;
    const path = r?.path;
    if (name === 'home' || name === 'homerem' || path === '/home' || path === '/homerem') return 'home';
    if (name === 'list' || path?.startsWith('/list')) return 'list';
    if (name === 'calendar' || path?.startsWith('/calendar')) return 'calendar';
    if (name === 'reminders' || path?.startsWith('/reminders')) return 'message';
    if (name === 'charts' || path?.startsWith('/charts')) return 'chart';
    if (name === 'company-charts' || path === '/company-charts') return 'company-chart'; 
    if (name === 'profile-comp' || path?.startsWith('/profile')) return 'profile'; 
    return 'home';
};

const navigate = async (icon) => {
    activeIcon.value = icon;
    
    const routes = {
        home: roleLevel.value === 1
            ? { name: 'homerem', fallback: '/homerem' }
            : { name: 'home', fallback: '/home' },
        list: { name: 'list', fallback: '/list' },
        calendar: { name: 'calendar', fallback: '/calendar' },
        message: { name: 'reminders', fallback: '/reminders' },
        chart: { name: 'charts', fallback: '/charts' },
        'company-chart': { name: 'company-charts', fallback: '/company-charts' },
        profile: { name: 'profile-comp', fallback: '/profile-comp' }
    };
    
    const r = routes[icon];
    try {
        await router.push({ name: r.name });
    } catch (error) {
        if (error.name !== 'NavigationFailure') {
            try { await router.push(r.fallback); } catch (e) {}
        }
    }
};

onMounted(async () => {
    activeIcon.value = routeToIcon(route);
});

watch(
    () => route.path,
    () => { activeIcon.value = routeToIcon(route); }
);
</script>

<style scoped>
.navbar {
    position: fixed;
    left: 16px;
    top: 16px;
    height: calc(100vh - 32px);
    display: flex;
    flex-direction: column;
    align-items: flex-start;
    gap: 12px;
    z-index: 100;
    pointer-events: none;
}

.hamburger {
    pointer-events: all;
    width: 36px;
    height: 36px;
    display: flex;
    align-items: center;
    justify-content: center;
    cursor: pointer;
    color: var(--brand-berry, #a12971);
    font-size: 18px;
    border-radius: 8px;
    transition: background 0.2s;
    margin-left: 4px;
}

.hamburger:hover {
    background: rgba(161, 41, 113, 0.08);
}

.nav-panel {
    pointer-events: all;
    background: rgba(255, 255, 255, 0.75);
    backdrop-filter: blur(16px);
    -webkit-backdrop-filter: blur(16px);
    border: 1.5px solid rgba(255, 255, 255, 0.85);
    border-radius: 20px;
    box-shadow: 0 8px 32px rgba(0, 0, 0, 0.08);
    padding: 12px;
    overflow: hidden;
    transition: opacity 0.25s ease, transform 0.25s ease;
}

.nav-panel.collapsed {
    opacity: 0;
    pointer-events: none;
    transform: translateX(-8px);
}

.nav-icons {
    display: flex;
    flex-direction: column;
    gap: 4px;
}

.nav-icon {
    display: flex;
    align-items: center;
    gap: 12px;
    padding: 12px 16px;
    cursor: pointer;
    border-radius: 14px;
    transition: background 0.2s ease;
    color: var(--brand-berry, #a12971);
    white-space: nowrap;
    min-width: 200px;
}

.nav-icon svg {
    width: 20px;
    height: 20px;
    flex-shrink: 0;
}

.nav-icon:hover {
    background: rgba(161, 41, 113, 0.07);
}

.nav-icon.active {
    background: var(--brand-berry, #a12971);
    color: white;
}

.nav-icon.active svg {
    color: white;
}

.nav-label {
    font-family: 'Inter', sans-serif;
    font-size: 15px;
    font-weight: 500;
}

@media (max-width: 600px) {
    .navbar {
        left: 8px;
        top: 8px;
    }

    .nav-icon {
        min-width: 160px;
        padding: 10px 12px;
    }

    .nav-label {
        font-size: 13px;
    }
}
</style>

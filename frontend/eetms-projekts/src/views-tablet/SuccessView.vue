<template>
    <div class="page-layout">
        <div class="page-container" @click="navigateTo('/hometab')">
            <div class="brand-logo">
                <img src="/assets/rimi.svg" alt="Rimi"/>
            </div> 
            <h1>{{ t('success.successText') }}</h1>
            <p class="countdown-text"> {{ t('success.redirecting') }} {{ countdown }}</p>
        </div>
    </div>
</template>

<script setup>
import { ref, onMounted, onBeforeUnmount, onUnmounted } from 'vue'; 
import { useRouter } from 'vue-router'; 
import { useShiftRegistration } from '@/composables/useShiftRegistration';
import { useI18n } from 'vue-i18n';
const { t } = useI18n();

const router = useRouter(); 
const { reset } = useShiftRegistration();
const countdown = ref(10); 
let intervalID = null; 
const { locale } = useI18n();

const navigateTo = (path) => {
    reset();
    router.push(path);
};

onMounted(() => {
    intervalID = setInterval(() => {
        countdown.value--; 

        if(countdown.value == 0) {
            clearInterval(intervalID);
            reset();
            router.push('/hometab');
        }
    }, 1000); 
}); 

onBeforeUnmount (() => {
    if (intervalID) {
        clearInterval(intervalID);
    }
});

onUnmounted(() => {
  locale.value = 'en'
  localStorage.setItem('lang', 'en')
})
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
    font-family: 'Inter';
}

.page-container {
    background: white;
    border-radius: 20px;
    padding: 50px 60px;
    box-shadow: 0 4px 20px rgba(0, 0, 0, 0.1);
    width: 100%;
    max-width: 800px;
    display: flex;
    flex-direction: column;
    align-items: center;
    text-align: center;
}

.brand-logo {
    margin-bottom: 30px;
}

.brand-logo img {
    width: 400px;
    height: auto;
}

h1 {
    font-family: 'Inter';
    font-size: 40px;
    font-weight: 700;
    color: #333;
    margin: 0;
}

/* Responsive */
@media (max-width: 768px) {
    .page-container {
        padding: 40px 50px;
    }

    h1 {
        font-size: 32px;
    }

    .brand-logo img {
        max-width: 150px;
    }
}

@media (max-width: 600px) {
    .page-container {
        padding: 35px 40px;
    }

    h1 {
        font-size: 26px;
    }
}

@media (max-width: 480px) {
    .page-container {
        padding: 30px 35px;
    }

    h1 {
        font-size: 22px;
    }

    .brand-logo img {
        max-width: 120px;
    }
}
</style>

<template>
    <div class="numeric-keypad">
        <div class="keypad-grid">
            <button 
                v-for="num in [1, 2, 3, 4, 5, 6, 7, 8, 9]" 
                :key="num"
                class="keypad-button"
                @click="handleNumberClick(num)"
            >
                {{ num }}
            </button>
            
            <button 
                class="keypad-button"
                @click="handleNumberClick(0)"
            >
                0
            </button>
            
            <button 
                class="keypad-button backspace"
                @click="handleBackspace"
            >
                <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                    <path d="M21 4H8l-7 8 7 8h13a2 2 0 0 0 2-2V6a2 2 0 0 0-2-2z"></path>
                    <line x1="18" y1="9" x2="12" y2="15"></line>
                    <line x1="12" y1="9" x2="18" y2="15"></line>
                </svg>
            </button>
        </div>
    </div>
</template>

<script setup>
import { defineEmits } from 'vue';

const emit = defineEmits(['number-click', 'backspace']);

const handleNumberClick = (num) => {
    emit('number-click', num);
};

const handleBackspace = () => {
    emit('backspace');
};
</script>

<style scoped>
.numeric-keypad {
    background: linear-gradient(135deg, #f6d4ed, #f0b1df);
    border-radius: 16px;
    padding: 20px;
    box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
}

.keypad-grid {
    display: grid;
    grid-template-columns: repeat(3, 1fr);
    gap: 12px;
}

.keypad-button {
    background-color: #fae9f6;
    border: none;
    border-radius: 12px;
    padding: 24px;
    font-size: 28px;
    font-weight: 600;
    color: #a12971;
    cursor: pointer;
    transition: all 0.2s ease;
    min-width: 80px;
    min-height: 80px;
    display: flex;
    align-items: center;
    justify-content: center;
    font-family: 'Inter', sans-serif;
}

.keypad-button:hover {
    background-color: #fae9f6;
    transform: scale(1.05);
}

.keypad-button:active {
    transform: scale(0.95);
    background-color: #f0b1df;
}

.keypad-button.backspace {
    display: flex;
    align-items: center;
    justify-content: center;
}

.keypad-button.backspace svg {
    width: 28px;
    height: 28px;
}
</style>

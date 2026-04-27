<template>
  <div class="keyboard-container">
    <div class="keyboard-grid">

      <button 
        v-for="key in row1" 
        :key="'r1-' + key"
        class="key-button"
        @click="handleKeyClick(key)"
      >
        {{ key }}
      </button>
      
      <button 
        v-for="key in row2" 
        :key="'r2-' + key"
        class="key-button row2-btn"
        @click="handleKeyClick(key)"
      >
        {{ key }}
      </button>
      
      <button 
        v-for="key in row3" 
        :key="'r3-' + key"
        class="key-button row3-btn"
        @click="handleKeyClick(key)"
      >
        {{ key }}
      </button>
      
      <button 
        class="key-button backspace"
        @click="handleBackspace"
      >
        <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
          <path d="M21 4H8l-7 8 7 8h13a2 2 0 0 0 2-2V6a2 2 0 0 0-2-2z"></path>
          <line x1="18" y1="9" x2="12" y2="15"></line>
          <line x1="12" y1="9" x2="18" y2="15"></line>
        </svg>
      </button>
      
      <button 
        v-for="num in numbers" 
        :key="'num-' + num"
        class="key-button"
        @click="handleKeyClick(num)"
      >
        {{ num }}
      </button>
      
      <button 
        class="key-button space-bar"
        @click="handleKeyClick(' ')"
      >
        Space
      </button>
    </div>
  </div>
</template>

<script setup>
import { defineProps, defineEmits } from 'vue';

const props = defineProps({
  modelValue: {
    type: String,
    default: ''
  }
});

const emit = defineEmits(['update:modelValue']);

const row1 = ['Q', 'W', 'E', 'R', 'T', 'Y', 'U', 'I', 'O', 'P'];
const row2 = ['A', 'S', 'D', 'F', 'G', 'H', 'J', 'K', 'L'];
const row3 = ['Z', 'X', 'C', 'V', 'B', 'N', 'M'];
const numbers = ['1', '2', '3', '4', '5', '6', '7', '8', '9', '0'];

const handleKeyClick = (key) => {
  const newValue = props.modelValue + key;
  emit('update:modelValue', newValue);
};

const handleBackspace = () => {
  const newValue = props.modelValue.slice(0, -1);
  emit('update:modelValue', newValue);
};
</script>

<style scoped>
.keyboard-container {
  background: linear-gradient(135deg, #A4EBD4, #2AB492);
  border-radius: 12px;
  padding: 16px;
  box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
  width: fit-content;
  margin: 15px;
}

.keyboard-grid {
  display: grid;
  grid-template-columns: repeat(0, 32px);
  gap: 20px 5px;
}

.key-button {
  background-color: #D1F6E9;
  border: none;
  border-radius: 8px;
  padding: 32px 17px;
  font-size: 20px;
  font-weight: 600;
  color: #1B6A59;
  cursor: pointer;
  transition: all 0.2s ease;
  height: 70px;
  width: 16px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-family: 'Inter', sans-serif;
}

.key-button:hover {
  background-color: #6EDABA;
  transform: scale(1.05);
}

.key-button:active {
  transform: scale(0.95);
  background-color: #40C19F;
}

.row2-btn:nth-of-type(1) {
  grid-column: 1;
}

.row3-btn:nth-of-type(1) {
  grid-column: 2;
}

.key-button.backspace {
  grid-column: span 4;
  width: 150px;
}

.key-button.backspace svg {
  width: 20px;
  height: 20px;
  stroke: #1B6A59;
}

.key-button.space-bar {
  grid-column: span 10;
  width: 385px;
  font-size: 14px;
  height: 50px;
}

@media (max-width: 768px) {
  .keyboard-container {
    padding: 14px;
  }
  
  .keyboard-grid {
    grid-template-columns: repeat(10, 30px);
    gap: 5px;
  }
  
  .key-button {
    width: 30px;
    height: 55px;
    padding: 10px 3px;
    font-size: 15px;
  }
}
</style>
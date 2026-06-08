<template>
  <!-- Tiek rādīts tikai tad, kad isOpen vērtība ir true -->
  <div v-if="isOpen" class="confirm-overlay">
    
    <!-- Galvenais apstiprinājuma logs -->
    <div class="confirm-box">
      
      <!-- Virsraksts -->
      <h3>Submit?</h3>

      <!-- Navigācijas un atcelšanas pogas -->
      <div class="confirm-buttons">
        <button class="btn-no" @click="cancel">No</button>
        <button class="btn-yes" @click="navigateTo('/verification')">Yes</button>
      </div>
    </div>
  </div>
</template>

<script setup>
// Importēt Vue router navigācijai  
import { useRouter } from 'vue-router';

// Iegūt router instanci
const router = useRouter();

// Funkcija navigācijai starp lapām 
const navigateTo = (path) => {
    router.push(path);
};

// Definēt props, ko komponents saņem no pamata komponenta 
const props = defineProps({
  // Noteikt, vai modālais logs ir atvērts
  isOpen: Boolean,

  // Noteikt maiņas tipu 
  shiftType: String
});

// Definēt notikumus, apstiprinājumam vai atcelšanai 
const emit = defineEmits(['confirm', 'cancel']);

// Funkcija apstiprināšanai
const confirm = () => {
  emit('confirm');
};

// Funkcija atcelšanai/modālā loga aizvēršanai  
const cancel = () => {
  emit('cancel');
};
</script>

<style scoped>
/* Fona stils komponentam */ 
.confirm-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0, 0, 0, 0.4);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1001;
}

/* Modālā loga kaste */
.confirm-box {
  background: #e8e8e8;
  border-radius: 16px;
  padding: 35px 50px;
  box-shadow: 0 8px 30px rgba(0, 0, 0, 0.2);
  text-align: center;
  min-width: 280px;
}

/* Modālā loga virsraksts */
.confirm-box h3 {
  font-family: 'Inter', sans-serif;
  font-size: 24px;
  font-weight: 700;
  color: #333;
  margin: 0 0 30px 0;
}

/* Pogu konteiners */
.confirm-buttons {
  display: flex;
  gap: 20px;
  justify-content: center;
}

/* Stils abām pogām */
.btn-no,
.btn-yes {
  font-family: 'Inter', sans-serif;
  font-size: 18px;
  font-weight: 700;
  padding: 14px 35px;
  border-radius: 30px;
  border: none;
  cursor: pointer;
  transition: all 0.3s ease;
  min-width: 100px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
}

/* Atcelšanas pogas stils */
.btn-no {
  background: linear-gradient(135deg, #ff6b6b, #ee5a6f);
  color: white;
}

.btn-no:hover {
  transform: translateY(-2px);
  box-shadow: 0 6px 20px rgba(255, 107, 107, 0.4);
}

/* Apstiprinājuma pogas stils */
.btn-yes {
  background: linear-gradient(135deg, #51cf66, #37b24d);
  color: white;
}

.btn-yes:hover {
  transform: translateY(-2px);
  box-shadow: 0 6px 20px rgba(81, 207, 102, 0.4);
}

/* Pogu nospiešanas efekts */
.btn-no:active,
.btn-yes:active {
  transform: translateY(0);
}

/* Responsivitāte: planšetdatoriem */
@media (max-width: 768px) {
  .confirm-box {
    padding: 30px 40px;
    min-width: 250px;
  }

  .confirm-box h3 {
    font-size: 22px;
    margin-bottom: 25px;
  }

  .btn-no,
  .btn-yes {
    font-size: 16px;
    padding: 12px 30px;
    min-width: 90px;
  }
}

/* Responsivitāte: mobilajām ierīcēm */
@media (max-width: 480px) {
  .confirm-box {
    padding: 25px 35px;
    min-width: 220px;
  }

  .confirm-box h3 {
    font-size: 20px;
    margin-bottom: 20px;
  }

  .btn-no,
  .btn-yes {
    font-size: 15px;
    padding: 10px 25px;
    min-width: 80px;
  }

  .confirm-buttons {
    gap: 15px;
  }
}
</style>

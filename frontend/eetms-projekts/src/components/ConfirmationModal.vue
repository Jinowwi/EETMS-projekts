<template>
  <div v-if="isOpen" class="confirm-overlay">
    <div class="confirm-box">
      <h3>Submit?</h3>
      <div class="confirm-buttons">
        <button class="btn-no" @click="cancel">No</button>
        <button class="btn-yes" @click="navigateTo('/verification')">Yes</button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { useRouter } from 'vue-router';

const router = useRouter();

const navigateTo = (path) => {
    router.push(path);
};

const props = defineProps({
  isOpen: Boolean,
  shiftType: String
});

const emit = defineEmits(['confirm', 'cancel']);

const confirm = () => {
  emit('confirm');
};

const cancel = () => {
  emit('cancel');
};
</script>

<style scoped>
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

.confirm-box {
  background: #e8e8e8;
  border-radius: 16px;
  padding: 35px 50px;
  box-shadow: 0 8px 30px rgba(0, 0, 0, 0.2);
  text-align: center;
  min-width: 280px;
}

.confirm-box h3 {
  font-family: 'Inter';
  font-size: 24px;
  font-weight: 700;
  color: #333;
  margin: 0 0 30px 0;
}

.confirm-buttons {
  display: flex;
  gap: 20px;
  justify-content: center;
}

.btn-no,
.btn-yes {
  font-family: 'Inter';
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

.btn-no {
  background: linear-gradient(135deg, #ff6b6b, #ee5a6f);
  color: white;
}

.btn-no:hover {
  transform: translateY(-2px);
  box-shadow: 0 6px 20px rgba(255, 107, 107, 0.4);
}

.btn-yes {
  background: linear-gradient(135deg, #51cf66, #37b24d);
  color: white;
}

.btn-yes:hover {
  transform: translateY(-2px);
  box-shadow: 0 6px 20px rgba(81, 207, 102, 0.4);
}

.btn-no:active,
.btn-yes:active {
  transform: translateY(0);
}

/* Responsive */
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

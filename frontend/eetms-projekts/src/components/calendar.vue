<template>
  <div class="calendar-container">
    <VCalendar 
      :initial-page="initialPage"
      @dayclick="clickDay" 
      @update:from-page="changeDate"
      :attributes="calendarAttributes"
      :expanded="true"
    />
  </div>
</template>

<script setup>
import { Calendar as VCalendar } from 'v-calendar';
import 'v-calendar/style.css';
import { computed } from 'vue';

const props = defineProps({
  missedPunches: {
    type: Array,
    default: () => []
  },
  initialDate: {
    type: [Date, String, null],
    default: null
  }
});

const initialPage = computed(() => {
  if(!props.initialDate) return undefined;

  const d = props.initialDate instanceof Date
    ? props.initialDate
    : new Date(props.initialDate);

  return { month: d.getMonth() + 1, year: d.getFullYear()
  }; 
}); 

const emit = defineEmits(['punchClick']);

const clickDay = (day) => {
  const missedPunch = props.missedPunches.find(punch => {
    const punchDate = new Date(punch.date);
    const sameDay = punchDate.toDateString() === day.date.toDateString(); 
    return sameDay && punch.type !== null;
  });

  if (missedPunch) {
    emit('punchClick', missedPunch);
  }
};

const changeDate = (page) => {
  console.log(page);
};

const calendarAttributes = computed(() => {
  return props.missedPunches
  .filter(punch => punch.type !== null)
  .map(punch => ({
    dot: {
      color: 'red',
      class: 'missed-punch-dot'
    },
    dates: new Date(punch.date),
    customData: punch
  }));
});
</script>

<style scoped>
.calendar-container {
  width: calc(100% - 16px);
  max-width: 80%;
  margin: 0 0px;
  padding: 12px 8px 0 8px;
  background: var(--color-white);
  border-radius: 12px;
  box-shadow: 0 2px 12px rgba(0, 0, 0, 0.08);
  box-sizing: border-box;
  margin-left: 40px;
}

:deep(.vc-container) {
  width: 100%;
  background: transparent;
  font-family: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif;
  border: none;
}

:deep(.vc-pane) {
  background: transparent;
  width: 100%;
  padding-bottom: 0;
}

:deep(.vc-header) {
  padding: 0 0 10px 0;
  margin-bottom: 10px;
  border-bottom: 2px solid #f0f0f0;
}

:deep(.vc-title) {
  font-size: 15px;
  font-weight: 700;
  background: var(--brand-gradient);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  background-clip: text;
  cursor: pointer;
  transition: opacity 0.2s ease;
  outline: none;
}

:deep(.vc-title:hover),
:deep(.vc-title:focus) {
  opacity: 0.8;
  outline: none;
}

:deep(.vc-arrow) {
  color: var(--brand-berry);
  width: 28px;
  height: 28px;
  border-radius: 8px;
  transition: all 0.2s ease;
  outline: none;
}

:deep(.vc-arrow:hover),
:deep(.vc-arrow:focus) {
  background: #f8f8f8;
  outline: none;
  box-shadow: 0 0 0 3px rgba(161, 41, 113, 0.2);
}

:deep(.vc-popover-content-wrapper) {
  background: var(--color-white);
  border-radius: 12px;
  box-shadow: 0 8px 30px rgba(161, 41, 113, 0.15);
  border: none;
  overflow: hidden;
  max-width: 90vw;
}

:deep(.vc-popover-content) {
  background: var(--color-white);
  border: none;
  padding: 12px;
}

:deep(.vc-popover-caret) {
  display: none;
}

:deep(.vc-nav-header) {
  padding: 8px 6px;
  border-bottom: 1px solid #f0f0f0;
  margin-bottom: 8px;
}

:deep(.vc-nav-title) {
  font-size: 13px;
  font-weight: 600;
  color: #333;
  outline: none;
}

:deep(.vc-nav-arrow) {
  color: var(--brand-berry);
  width: 28px;
  height: 28px;
  border-radius: 6px;
  transition: background 0.2s ease;
  outline: none;
}

:deep(.vc-nav-arrow:hover),
:deep(.vc-nav-arrow:focus) {
  background: #f5f5f5;
  outline: none;
  box-shadow: 0 0 0 2px rgba(161, 41, 113, 0.2);
}

:deep(.vc-nav-items) {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 6px;
  padding: 4px;
}

:deep(.vc-nav-item) {
  font-size: 11px;
  font-weight: 500;
  color: #333;
  background: #fafafa;
  border: 2px solid transparent;
  border-radius: 8px;
  padding: 8px 4px;
  transition: all 0.2s ease;
  cursor: pointer;
  outline: none;
  min-height: 36px;
  display: flex;
  align-items: center;
  justify-content: center;
}

:deep(.vc-nav-item:hover) {
  background: #f0f0f0;
  border-color: #e0e0e0;
}

:deep(.vc-nav-item:focus) {
  outline: none;
  box-shadow: 0 0 0 2px rgba(161, 41, 113, 0.3);
}

:deep(.vc-nav-item.is-current) {
  background: linear-gradient(135deg, #a12971, #96537b);
  color: var(--color-white);
  font-weight: 600;
  border-color: transparent;
}

:deep(.vc-nav-item.is-active) {
  background: rgba(161, 41, 113, 0.1);
  border-color: var(--brand-berry);
  color: var(--brand-berry);
  font-weight: 600;
}

:deep(.vc-weekdays) {
  padding: 0 0 6px 0;
}

:deep(.vc-weekday) {
  font-size: 9px;
  font-weight: 600;
  color: #999;
  text-transform: uppercase;
  letter-spacing: 0.3px;
  padding: 4px 0;
}

:deep(.vc-weeks) {
  padding: 0;
  margin-bottom: 0;
}

:deep(.vc-day) {
  padding: 1px;
  min-height: 42px;
  position: relative;
}

:deep(.vc-day-content) {
  font-size: 11px;
  font-weight: 500;
  color: #333;
  background: linear-gradient(to right, #96537b0a, #2ba4920a);
  border-radius: 6px;
  width: 100%;
  height: 100%;
  min-height: 40px;
  display: flex;
  align-items: flex-start;
  justify-content: flex-start;
  padding: 4px 3px;
  transition: all 0.2s ease;
  border: 2px solid transparent;
  outline: none;
  cursor: pointer;
}

:deep(.vc-day-content:focus) {
  outline: none;
  box-shadow: 0 0 0 2px rgba(161, 41, 113, 0.2);
}

:deep(.vc-day:hover .vc-day-content) {
  background: linear-gradient(to left, #96537b0a, #2ba4920a);
  border-color: #a5698d50;
}

:deep(.vc-day.is-today .vc-day-content) {
  background: #fff;
  border-color: var(--brand-berry);
  color: var(--brand-berry);
  font-weight: 600;
}

:deep(.vc-day.is-selected .vc-day-content) {
  background: var(--header-gradient);
  color: var(--color-white);
  border-color: transparent;
  font-weight: 600;
}

:deep(.vc-day.is-not-in-month .vc-day-content) {
  background: transparent;
  color: #d0d0d0;
}

:deep(.vc-day.is-not-in-month:hover .vc-day-content) {
  background: transparent;
  border-color: transparent;
}

:deep(.vc-dots) {
  position: absolute;
  top: 3px;
  right: 3px;
  display: flex;
  gap: 3px;
}

:deep(.vc-dot) {
  width: 6px !important;
  height: 6px !important;
  border-radius: 50%;
  background-color: var(--reminder-header) !important;
  box-shadow: 0 2px 4px rgba(220, 53, 69, 0.4);
}

:deep(.vc-highlight) {
  display: none;
}

@media (hover: none) and (pointer: coarse) {
  :deep(.vc-day-content:active) {
    transform: scale(0.98);
  }
}

@media (min-width: 375px) {
  .calendar-container {
    padding: 14px 10px 0 10px;
  }

  :deep(.vc-title) {
    font-size: 16px;
  }

  :deep(.vc-arrow) {
    width: 30px;
    height: 30px;
  }

  :deep(.vc-day) {
    min-height: 46px;
    padding: 2px;
  }

  :deep(.vc-day-content) {
    min-height: 42px;
    padding: 5px 4px;
    font-size: 11px;
  }
}

@media (min-width: 481px) {
  .calendar-container {
    width: calc(100% - 20px);
    margin: 0 10px;
    padding: 16px 12px 0 12px;
    border-radius: 14px;
  }

  :deep(.vc-header) {
    padding: 0 0 12px 0;
    margin-bottom: 12px;
  }

  :deep(.vc-title) {
    font-size: 17px;
  }

  :deep(.vc-arrow) {
    width: 32px;
    height: 32px;
  }

  :deep(.vc-weekday) {
    font-size: 10px;
    padding: 5px 0;
  }

  :deep(.vc-weekdays) {
    padding: 0 0 8px 0;
  }

  :deep(.vc-day) {
    min-height: 52px;
    padding: 2px;
  }

  :deep(.vc-day-content) {
    min-height: 48px;
    font-size: 12px;
    padding: 6px 5px;
    border-radius: 8px;
  }

  :deep(.vc-dot) {
    width: 7px !important;
    height: 7px !important;
  }

  :deep(.vc-dots) {
    top: 4px;
    right: 4px;
  }

  :deep(.vc-nav-item) {
    font-size: 12px;
    padding: 9px 5px;
  }
}

@media (min-width: 768px) {
  .calendar-container {
    width: calc(100% - 140px);
    max-width: 750px;
    margin-left: 110px;
    padding: 18px 14px 0 14px;
    border-radius: 16px;
  }

  :deep(.vc-header) {
    padding: 0 0 14px 0;
    margin-bottom: 14px;
  }

  :deep(.vc-title) {
    font-size: 18px;
  }

  :deep(.vc-arrow) {
    width: 34px;
    height: 34px;
    border-radius: 10px;
  }

  :deep(.vc-weekday) {
    font-size: 10px;
    letter-spacing: 0.6px;
    padding: 6px 0;
  }

  :deep(.vc-day) {
    min-height: 58px;
    padding: 3px;
  }

  :deep(.vc-day-content) {
    min-height: 52px;
    font-size: 13px;
    padding: 8px 6px;
    border-radius: 9px;
  }

  :deep(.vc-dot) {
    width: 7px !important;
    height: 7px !important;
  }

  :deep(.vc-nav-title) {
    font-size: 14px;
  }

  :deep(.vc-nav-item) {
    font-size: 12px;
    padding: 10px 6px;
  }
}


@media (min-width: 1024px) {
  .calendar-container {
    max-width: 850px;
    width: 90%;
    margin: 0 auto;
    padding: 18px 16px 0 16px;

    /* margin-left: 100px; */
  }

  :deep(.vc-title) {
    font-size: 19px;
  }

  :deep(.vc-arrow) {
    width: 36px;
    height: 36px;
  }

  :deep(.vc-weekday) {
    font-size: 11px;
    letter-spacing: 0.7px;
  }

  :deep(.vc-day) {
    min-height: 64px;
    padding: 3px;
  }

  :deep(.vc-day-content) {
    min-height: 58px;
    font-size: 13px;
    padding: 9px 7px;
    border-radius: 10px;
  }

  :deep(.vc-dot) {
    width: 8px !important;
    height: 8px !important;
  }

  :deep(.vc-dots) {
    top: 5px;
    right: 5px;
  }

  :deep(.vc-nav-items) {
    gap: 8px;
  }

  :deep(.vc-nav-item) {
    font-size: 13px;
    padding: 10px 6px;
    border-radius: 10px;
  }
}

@media (min-width: 1200px) {
  .calendar-container {
    max-width: 900px;
    padding: 20px 18px 0 18px;
  }

  :deep(.vc-header) {
    padding: 0 0 16px 0;
    margin-bottom: 16px;
  }

  :deep(.vc-title) {
    font-size: 20px;
  }

  :deep(.vc-weekday) {
    font-size: 11px;
    letter-spacing: 0.8px;
    padding: 6px 0;
  }

  :deep(.vc-weekdays) {
    padding: 0 0 10px 0;
  }

  :deep(.vc-day) {
    min-height: 70px;
    padding: 3px;
  }

  :deep(.vc-day-content) {
    min-height: 64px;
    font-size: 14px;
    padding: 10px 8px;
  }

  :deep(.vc-popover-content) {
    padding: 16px;
  }

  :deep(.vc-nav-header) {
    padding: 12px 8px;
    margin-bottom: 12px;
  }

  :deep(.vc-nav-title) {
    font-size: 15px;
  }

  :deep(.vc-nav-arrow) {
    width: 30px;
    height: 30px;
    border-radius: 8px;
  }
}

@media (min-width: 1400px) {
  .calendar-container {
    max-width: 1000px;
    padding: 22px 20px 0 20px;
  }

  :deep(.vc-title) {
    font-size: 22px;
  }

  :deep(.vc-day) {
    min-height: 76px;
  }

  :deep(.vc-day-content) {
    min-height: 70px;
    font-size: 15px;
    padding: 11px 9px;
  }
}
</style>
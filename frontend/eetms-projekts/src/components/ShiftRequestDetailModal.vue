<template>
  <div class="modal-overlay" @click.self="$emit('close')">
    <div class="modal">
      <button class="modal-close" @click="$emit('close')">✕</button>

      <h2>Shift Request Details</h2>

      <div class="detail-row">
        <span class="label">Reason:</span>
        <span class="value">{{ request.reason?.name ?? '—' }}</span>
      </div>

      <div class="detail-row">
        <span class="label">Status:</span>
        <span class="value status-badge" :class="statusInfo.cls">{{ statusInfo.label }}</span>
      </div>

      <div class="detail-row">
        <span class="label">Description:</span>
        <span class="value">{{ request.description ?? '—' }}</span>
      </div>

      <div class="detail-row">
        <span class="label">Company:</span>
        <span class="value">{{ request.company?.name ?? '—' }}</span>
      </div>

      <div class="detail-row">
        <span class="label">Assigned to:</span>
        <span class="value">{{ request.rem?.name ?? '—' }}</span>
      </div>

    </div>
  </div>
</template>

<script setup>
import { computed } from 'vue'

const props = defineProps({
  request: {
    type: Object,
    required: true
  }
})

defineEmits(['close'])

const statusMap = {
  1: { label: 'Sent',        cls: 'status-sent'       },
  2: { label: 'Approved',    cls: 'status-approved'   },
  3: { label: 'In Progress', cls: 'status-inprogress' },
  4: { label: 'Done',        cls: 'status-done'       },
}

const statusInfo = computed(() => statusMap[props.request.status] ?? { label: props.request.status, cls: '' })
</script>

<style scoped>
.modal-overlay {
  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, 0.45);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
}

.modal {
  background: var(--color-surface, #fff);
  border-radius: 12px;
  padding: 2rem;
  min-width: 340px;
  max-width: 480px;
  width: 100%;
  position: relative;
  box-shadow: 0 8px 32px rgba(0, 0, 0, 0.18);
}

.modal-close {
  position: absolute;
  top: 1rem;
  right: 1rem;
  background: none;
  border: none;
  font-size: 1.1rem;
  cursor: pointer;
  color: var(--brand-berry);
  line-height: 1;
}

.modal-close:hover {
  color: var(--color-text, #222);
}

h2 {
  margin-bottom: 1.25rem;
  font-size: 1.2rem;
}

.detail-row {
  display: flex;
  gap: 0.75rem;
  padding: 0.5rem 0;
  border-bottom: 1px solid var(--color-divider, #eee);
}

.detail-row:last-child {
  border-bottom: none;
}

.label {
  font-weight: 600;
  min-width: 100px;
  color: var(--color-text-muted, #666);
}

.value {
  color: var(--color-text, #222);
}

.status-badge {
  padding: 0.15rem 0.6rem;
  border-radius: 999px;
  font-size: 0.85rem;
  font-weight: 600;
}

.status-sent       { background: #ddeeff; color: #1a4a8b; }
.status-approved   { background: #d4efcc; color: #2a6e1a; }
.status-inprogress { background: #fff0cc; color: #7a5500; }
.status-done       { background: #e0e0e0; color: #444;    }
</style>
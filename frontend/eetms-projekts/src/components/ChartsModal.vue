<template>
    <div v-if="isOpen" class="ChartOverlay">
        <div class="form-group">
          <label>Date</label>
          <input type="date" v-model="editedDate" class="form-input" />
        </div> 
    </div>
</template>

<script setup>

</script>

<style scoped></style>
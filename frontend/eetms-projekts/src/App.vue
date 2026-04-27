<template>

  <router-view/>
  <Navbar v-if="$route.meta.showNavbar" />
</template>

<script setup>
import Navbar from '../src/components/navbar.vue';
</script>

<style>
html, body {
  margin: 0;
  padding: 0;
  height: 100%;
  overflow-x: hidden;
}

#app {
  height: 100%;
  position: relative;
}
</style>

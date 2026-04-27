<template>
  <div class="lang-switcher" ref="switcherRef">
    <button class="lang-btn" @click="isOpen = !isOpen">
      <img :src="flagUrl(currentCountry)" :alt="currentLang" class="flag-img" />
      <span class="lang-code">{{ currentLang.toUpperCase() }}</span>
    </button>
    <div v-if="isOpen" class="lang-dropdown">
      <button
        v-for="lang in languages"
        :key="lang.code"
        class="lang-option"
        :class="{ active: currentLang === lang.code }"
        @click="selectLang(lang.code)"
      >
        <img :src="flagUrl(lang.country)" :alt="lang.label" class="flag-img" />
        <span class="lang-code">{{ lang.label }}</span>
      </button>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, onUnmounted } from 'vue'
import { useI18n } from 'vue-i18n'

const { locale } = useI18n()
const currentLang = ref(locale.value)
const isOpen = ref(false)
const switcherRef = ref(null)

const languages = [
  { code: 'en', country: 'gb', label: 'EN' },
  { code: 'lt', country: 'lt', label: 'LT' },
  { code: 'lv', country: 'lv', label: 'LV' },
  { code: 'et', country: 'ee', label: 'ET' },
  { code: 'ru', country: 'ru', label: 'RU' },
]

const currentCountry = computed(
  () => languages.find(l => l.code === currentLang.value)?.country
)

function flagUrl(countryCode) {
  return `https://flagcdn.com/w40/${countryCode}.png`
}

function selectLang(code) {
  currentLang.value = code
  locale.value = code
  localStorage.setItem('lang', code)
  isOpen.value = false
}

function handleClickOutside(e) {
  if (switcherRef.value && !switcherRef.value.contains(e.target)) {
    isOpen.value = false
  }
}

onMounted(() => document.addEventListener('click', handleClickOutside))
onUnmounted(() => document.removeEventListener('click', handleClickOutside))
</script>

<style scoped>
.lang-switcher {
  position: relative;
  display: inline-block;
}

.flag-img {
  width: 40px;
  height: auto;
  display: block;
  border-radius: 2px;
}

.lang-btn {
  background: transparent;
  border: 1px solid #ccc;
  border-radius: 6px;
  padding: 5px 8px;
  cursor: pointer;
  line-height: 1;
  transition: border-color 0.2s;
  display: flex;
  align-items: center;
  gap: 8px
}

.lang-btn:hover {
  border-color: #888;
}

.lang-dropdown {
  position: absolute;
  top: calc(100% + 6px);
  background: #fff;
  border: 1px solid #ddd;
  border-radius: 8px;
  padding: 6px;
  display: flex;
  flex-direction: column;
  gap: 2px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.12);
  z-index: 100;
}

.lang-option {
  background: transparent;
  border: none;
  border-radius: 6px;
  padding: 5px 8px;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: background 0.15s;
  gap: 8px;
  justify-content: flex-start;
}

.lang-option:hover {
  background: #f0f0f0;
}

.lang-option.active {
  background: #e8f0fe;
}

.lang-code {
  font-family: 'Inter', sans-serif;
  font-size: 13px;
  font-weight: 600;
  color: black;
  min-width: 24px;
  text-align: left;
}
</style>
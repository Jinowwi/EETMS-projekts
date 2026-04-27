import { createI18n } from 'vue-i18n'

import en from './locales/en.json'
import lt from './locales/lt.json'
import lv from './locales/lv.json'
import et from './locales/et.json'
import ru from './locales/ru.json'

const i18n = createI18n({
  legacy: false,
  locale: localStorage.getItem('lang') || 'en',
  fallbackLocale: 'en',
  messages: {
    en,
    lt,
    lv,
    et,
    ru
  }
})

export default i18n
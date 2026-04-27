import { createApp } from 'vue'
import { createPinia } from 'pinia'
import App from './App.vue'
import router from './router'
import './styles/app.css' 
import { library } from '@fortawesome/fontawesome-svg-core'
import { FontAwesomeIcon } from '@fortawesome/vue-fontawesome'
import { faHouse, faList, faCalendar, faMessage, faChartArea, faSliders, faPen, faPlus, faEllipsisV, faTimes, fas, faCheck, faBook } from '@fortawesome/free-solid-svg-icons'
import i18n from './i18n'

library.add(faHouse, 
    faList, 
    faCalendar, 
    faMessage, 
    faChartArea,
    faSliders,
    faPen,
    faPlus,
    faEllipsisV, 
    faTimes, 
    fas,
    faCheck,
    faBook
)

const app = createApp(App)

app.use(createPinia())
app.use(i18n)
app.use(router)
app.component('FontAwesomeIcon', FontAwesomeIcon)
app.mount('#app') 

window.alert = function(message) {
  
  const box = document.createElement('div')
  box.className = 'alert-box'
  box.style.fontFamily = getComputedStyle(document.documentElement).getPropertyValue('Inter').trim() || 'sans-serif'

  const text = document.createElement('span')
  text.textContent = message

  const btn = document.createElement('button')
  btn.className = 'alert-btn'
  btn.textContent = 'OK'
  btn.onclick = () => document.body.removeChild(box)

  box.appendChild(text)
  box.appendChild(btn)
  document.body.appendChild(box)
}
import { createRouter, createWebHistory } from 'vue-router'
import { isLoggedIn } from '@/services/auth.js'
import { getAdminRoleLevel } from '@/services/auth.js'

import HomeView from '../views/HomeView.vue'; 
import ListView from '../views/ListView.vue';
import CalendarView from '../views/CalendarView.vue';
import RemindersView from '../views/RemindersView.vue';
import ChartsView from '../views/Charts.vue';
import LoginView from '../views/LoginView.vue';
import HomeREM from '@/views/HomeREM.vue';
import LoginForCompanyView from '@/views/LoginForCompanyView.vue';
import CompanyProfileView from '@/views/CompanyProfileView.vue';
import RoleSelect from '../views/RoleSelect.vue';
import LoginForShop from '@/views/LoginForShop.vue';
import ShopDirectorHome from '@/views/ShopDirectorHome.vue';

import HomePageTablet from '../views-tablet/HomePageTablet.vue'
import ShiftManager from '../views-tablet/ShiftManager.vue'
import CodeView from '../views-tablet/CodeView.vue'
import Verification from '../views-tablet/Verification.vue'
import SuccessView from '../views-tablet/SuccessView.vue'
import PhoneNumber from '../views-tablet/PhoneNumber.vue'
import Reason from '../views-tablet/Reason.vue'
import SelectRoleView from '../views/RoleSelect.vue'
import SetPassword from '@/views/SetPassword.vue';

const routes = [
  {
    path: '/login',
    name: 'login',
    component: LoginView,
    meta: { showNavbar: false }
  },
  {
    path: '/home',
    name: 'home', 
    component: HomeView, 
    meta: { showNavbar: false, requiresAuth: true, roles: [3] }
  },
  {
    path: '/list',
    name: 'list',
    component: ListView,
    meta: { showNavbar: true, requiresAuth: true, roles: [2, 3] }
  }, 
  {
    path: '/calendar',
    name: 'calendar',
    component: CalendarView,
    meta: { showNavbar: true, requiresAuth: true, roles: [1, 3] }
  }, 
  {
    path: '/reminders',
    name: 'reminders',
    component: RemindersView,
    meta: { showNavbar: true, requiresAuth: true, roles: [1, 2, 3] }
  },
  {
    path: '/charts',
    name: 'charts',
    component: ChartsView,
    meta: { showNavbar: true, requiresAuth: true, roles: [1, 3], allowCompany: true}
  },
  {
    path: '/hometab', 
    name: 'homet',
    component: HomePageTablet, 
    meta: { showNavbar: false }
  }, 
  {
    path: '/shifts', 
    name: 'shiftmng', 
    component: ShiftManager, 
    meta: { showNavbar: false }
  }, 
  {
    path: '/code', 
    name: 'code', 
    component: CodeView, 
    meta: { showNavbar: false }
  }, 
  {
    path: '/verification', 
    name: 'verification', 
    component: Verification, 
    meta: { showNavbar: false }
  }, 
  {
    path: '/successful', 
    name: 'successful', 
    component: SuccessView, 
    meta: { showNavbar: false }
  },
  {
    path: '/phone',
    name: 'phone',
    component: PhoneNumber, 
    meta: { showNavbar: false }
  },
  {
    path: '/reason',
    name: 'reason',
    component: Reason,
    meta: {showNavbar: false}
  },
  {
    path: '/',
    name: 'select-role',
    component: SelectRoleView,
    meta: { showNavbar: false }
  },
  {
    path: '/:pathMatch(.*)*',
    redirect: '/home'
  },
  {
    path: '/homerem',
    name: 'homerem',
    component: HomeREM,
    meta: { showNavbar: false }
  },
  {
    path: '/login-comp',
    name: 'login-comp',
    component: LoginForCompanyView, 
    meta: { showNavbar: false }
  }, 
  {
    path: '/profile-comp',
    name: 'profile-comp',
    component: CompanyProfileView, 
    meta: { showNavbar: true }
  },
  {
    path: '/roleselect',
    name: 'roleselect',
    component: RoleSelect,
    meta: {showNavbar: false}
  },
  {
    path: '/login-shop',
    name: 'login-shop',
    component: LoginForShop,
    meta: { showNavbar: false}
  },
  {
    path:'/shop-home',
    name: 'shop-home',
    component: ShopDirectorHome,
    meta: { showNavbar: false, requiresAuth: true }
  },
  {
    path: '/set-password',
    name: 'SetPassword',
    component: SetPassword,
    meta: { showNavbar: false }
  }
];

const router = createRouter({ history: createWebHistory(), routes })

router.beforeEach((to, from, next) => {
  const isLoggedInAsAdmin = !!localStorage.getItem('admin')
  const isLoggedInAsCompany = !!localStorage.getItem('companyId')
  const isLoggedInAsShop = !!localStorage.getItem('shopId') 
  const userRole = getAdminRoleLevel()

  if (to.meta.requiresAuth) {
    if (!isLoggedInAsAdmin && !isLoggedInAsCompany && !isLoggedInAsShop) { 
      return next('/login')
    }
  }

  if (to.meta.roles) {
    if (isLoggedInAsAdmin) {
      if (!to.meta.roles.includes(userRole)) {
        return next('/home')
      }
    } else if (isLoggedInAsCompany) {
      if (!to.meta.allowCompany) {
        return next('/profile-comp')
      }
    }
  }

  next()
})

export default router; 
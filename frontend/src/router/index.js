import { createRouter, createWebHistory } from 'vue-router';
import authService from '../services/authService';
import LoginView from '../views/LoginView.vue';
import DashboardView from '../views/DashboardView.vue';
import PlanesView from '../views/PlanesView.vue';
import AppLayout from '../components/AppLayout.vue';

const routes = [
  {
    path: '/login',
    name: 'Login',
    component: LoginView,
    meta: { isPublic: true },
  },
  {
    path: '/',
    component: AppLayout,
    children: [
      {
        path: '',
        redirect: '/dashboard',
      },
      {
        path: 'dashboard',
        name: 'Dashboard',
        component: DashboardView,
      },
      {
        path: 'planes',
        name: 'Planes',
        component: PlanesView,
      },
    ],
  },
  {
    path: '/:pathMatch(.*)*',
    redirect: '/dashboard',
  },
];

const router = createRouter({
  history: createWebHistory(),
  routes,
});

router.beforeEach((to, from, next) => {
  const isAuthenticated = authService.isAuthenticated();

  if (to.meta.isPublic) {
    if (isAuthenticated) {
      next({ name: 'Dashboard' });
    } else {
      next();
    }
  } else {
    if (!isAuthenticated) {
      next({ name: 'Login' });
    } else {
      next();
    }
  }
});

export default router;

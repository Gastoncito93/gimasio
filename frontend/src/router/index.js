import { createRouter, createWebHistory } from 'vue-router';
import authService from '../services/authService';
import LoginView from '../views/LoginView.vue';
import RegisterView from '../views/RegisterView.vue';
import DashboardView from '../views/DashboardView.vue';
import PlanesView from '../views/PlanesView.vue';
import ActividadesView from '../views/ActividadesView.vue';
import CoachesView from '../views/CoachesView.vue';
import SociosView from '../views/SociosView.vue';
import CuotasView from '../views/CuotasView.vue';
import ProfileView from '../views/ProfileView.vue';
import CoachAlumnosView from '../views/CoachAlumnosView.vue';
import CoachAlumnoDetalleView from '../views/CoachAlumnoDetalleView.vue';
import ConfiguracionView from '../views/ConfiguracionView.vue';
import AppLayout from '../components/AppLayout.vue';

const routes = [
  {
    path: '/login',
    name: 'Login',
    component: LoginView,
    meta: { isPublic: true },
  },
  {
    path: '/register',
    name: 'Register',
    component: RegisterView,
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
        path: 'coach/alumnos',
        name: 'CoachAlumnos',
        component: CoachAlumnosView,
        meta: { roles: ['Administrador', 'Coach'] },
      },
      {
        path: 'coach/alumnos/:id',
        name: 'CoachAlumnoDetalle',
        component: CoachAlumnoDetalleView,
        meta: { roles: ['Administrador', 'Coach'] },
      },
      {
        path: 'coaches',
        name: 'Coaches',
        component: CoachesView,
        meta: { roles: ['Administrador'] },
      },
      {
        path: 'planes',
        name: 'Planes',
        component: PlanesView,
        meta: { roles: ['Administrador'] },
      },
      {
        path: 'actividades',
        name: 'Actividades',
        component: ActividadesView,
        meta: { roles: ['Administrador'] },
      },
      {
        path: 'socios',
        name: 'Socios',
        component: SociosView,
        meta: { roles: ['Administrador'] },
      },
      {
        path: 'cuotas',
        name: 'Cuotas',
        component: CuotasView,
        meta: { roles: ['Administrador'] },
      },
      {
        path: 'profile',
        name: 'Profile',
        component: ProfileView,
      },
      {
        path: 'configuracion',
        name: 'Configuracion',
        component: ConfiguracionView,
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
  if (to.meta.isPublic) {
    next();
    return;
  }

  const isAuthenticated = authService.isAuthenticated();
  if (!isAuthenticated) {
    next({ name: 'Login' });
    return;
  }

  const user = authService.getUsuario();
  if (to.meta.roles && (!user || !to.meta.roles.includes(user.rol))) {
    next({ name: 'Dashboard' });
    return;
  }

  next();
});

export default router;

import { createRouter, createWebHistory } from 'vue-router';
import Login from '../views/LoginView.vue'; // 登入頁面
import HomeView from '../views/HomeView.vue'; //後台

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/login',
      name: 'Login',
      component: Login, // 登入頁面
    },
    {
      path: '/get-otp',
      name: 'GetOtp',
      component: () => import('../views/GetOtpPage/GetOtpPage.vue'),
    },
    {
      path: '/',
      name: 'home',
      component: HomeView, // 後台首頁
      children: [
        {
          path: '',
          name: 'homePage',
          component: () => import('../views/HomeInfo.vue'), //首頁顯示內容(歡迎頁)
        },
        // {
        //   path: '/member/list',
        //   name: 'MemberList',
        //   component: () => import('../views/MemberData/MemberList.vue')
        // },
        // {
        //   path: '/member/data',
        //   name: 'MemberData',
        //   component: () => import('../views/MemberData/MemberEdit.vue')
        // },
        // {
        //   path: '/market/list',
        //   name: 'MarketList',
        //   component: () => import('../views/MarketPage/MarketList.vue')
        // },
        // {
        //   path: '/market/data',
        //   name: 'MarketEdit',
        //   component: () => import('../views/MarketPage/MarketEdit.vue')
        // },
        {
          path: '/admin-manager/list',
          name: 'AdminManager',
          component: () => import('../views/AdminManager/AdminManager.vue')
        },
        {
          path: '/admin-manager/data',
          name: 'AdminManagerEdit',
          component: () => import('../views/AdminManager/AdminManagerEdit.vue')
        },
        {
          path: '/admin-role/list',
          name: 'AdminRole',
          component: () => import('../views/AdminRole/AdminRole.vue'),
        },
        {
          path: '/admin-permission/list',
          name: 'adminPermission',
          component: () => import('../views/AdminPermission/AdminPermission.vue')
        },
        {
          path: '/system-parameters/list',
          name: 'SystemParameters',
          component: () => import('../views/SystemParameters/SystemParametersList.vue')
        },
        {
          path: '/menu/list',
          name: 'MenuPage',
          component: () => import('../views/MenuPage/MenuPage.vue')
        },
      ]
    }
  ]
});

export default router;

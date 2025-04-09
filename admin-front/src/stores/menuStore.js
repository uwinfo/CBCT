import { defineStore } from 'pinia';
import api from '../js/api';
import router from '../router';

export const useMenuStore = defineStore('menu', {
  state: () => ({
    menuItems: [], // 存儲菜單資料
  }),
  actions: {
    async loadData() {
      try {
        const getMenuData = await api.axiosGetAsync('/admin-menu/login-user-menu');
        this.menuItems = getMenuData.data || []; // 如果 API 返回 null，設為空陣列
      } catch (error) {
        console.error('錯誤:', error);
        this.menuItems = [];
        router.push('/login'); 
      }
    },
  },
});
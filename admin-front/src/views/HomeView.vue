<template>
  <el-container>
    <!-- 主體結構 -->
    <!-- 側欄 -->
    <el-aside width="220px" v-show="isDesktop" class="hidden md:block sidebar">
      <div class="pt-5 pb-2"><img class="m-auto rounded-xl" style="height: 100px" src="@/assets/images/logo.jpg" alt="Element logo" /></div>
      <el-menu :default-active="activeMenu" @select="menuSelect">
        <!-- <RouterLink :to="{ name: 'ShoplineControl' }">
            <el-menu-item index="1">Shopline</el-menu-item>
          </RouterLink>
          <RouterLink :to="{ name: 'OrderMain' }">
            <el-menu-item index="2">訂單列表</el-menu-item>
          </RouterLink>
          <RouterLink :to="{ name: 'BulkPrint' }">
            <el-menu-item index="3">大宗列表</el-menu-item>
          </RouterLink>
          <RouterLink :to="{ name: 'MenuPage' }">
            <el-menu-item index="4">Settings</el-menu-item>
          </RouterLink> -->
      </el-menu>
      <!--新目錄-->
      <el-menu :default-active="activeIndex" :open-keys="openedKeys" class="el-menu-vertical-demo" @select="handleSelect">
        <!-- 遍歷處理頂層菜單項 -->
        <template v-for="item in structuredMenu" :key="item.id">
          <template v-if="item.children && item.children.length">
            <!-- 父層 -->
            <el-sub-menu :index="String(item.id)" :key="item.id">
              <template #title>{{ item.text }}</template>
              <!-- 子層 -->
              <el-menu-item v-for="child in item.children" :key="child.id" :index="`/${child.link.split('/')[1]}`" @click="$router.push({ path: child.link })">
                {{ child.text }}
              </el-menu-item>
            </el-sub-menu>
          </template>
          <template v-else>
            <!-- 一層 -->
            <el-menu-item :index="`/${item.link.split('/')[1]}`" :key="item.id" @click="$router.push({ path: item.link })">
              {{ item.text }}
            </el-menu-item>
          </template>
        </template>
      </el-menu>
    </el-aside>
    <el-container>
      <!-- 頂欄 -->
      <el-header height="60px">
        <el-menu
          :default-active="activeMenu2"
          class="el-menu-demo justify-between md:justify-end"
          mode="horizontal"
          background-color="#545c64"
          text-color="#fff"
          active-text-color="#ffd04b"
          @select="menuSelect">
          <!-- 菜單按鈕（平板和手機尺寸） -->
          <button class="md:hidden bg-gray-800 text-white px-3 py-2 rounded" @click="toggleDrawer">
            <el-icon><Menu /></el-icon>
          </button>
          <el-sub-menu index="1">
            <template #title>
              <el-icon><User /></el-icon>
            </template>
            <el-menu-item index="1-1" @click="logout">登出</el-menu-item>
          </el-sub-menu>
        </el-menu>
      </el-header>
      <!-- 主內容區 -->
      <el-main class="bg-slate-50 bg-img">
        <RouterView />
        <el-backtop :right="toTopWidth" :bottom="toTopHeight" style="background-color: #1989fa; color: #fff" />
      </el-main>
    </el-container>
    <!-- 側邊欄（平板和手機，用 el-drawer） -->
    <el-drawer v-model="isDrawerVisible" direction="ltr" size="200px" class="md:hidden">
      <!-- 行動裝置：頂部區塊，包含 X 按鈕和 Logo -->
      <div class="flex items-center justify-between px-4 py-2">
        <img style="width: 50px" src="@/assets/images/logo.jpg" alt="Element logo" />
        <el-icon @click="toggleDrawer" class="cursor-pointer">
          <Close />
        </el-icon>
      </div>
      <el-menu :default-active="activeMenu" @select="handleSelect">
        <!-- 遍歷目錄 -->
        <template v-for="item in structuredMenu" :key="item.id">
          <template v-if="item.children && item.children.length">
            <el-sub-menu :index="String(item.id)">
              <template #title>{{ item.text }}</template>
              <el-menu-item v-for="child in item.children" :key="child.id" :index="child.link">
                {{ child.text }}
              </el-menu-item>
            </el-sub-menu>
          </template>
          <template v-else>
            <el-menu-item :index="item.link">
              {{ item.text }}
            </el-menu-item>
          </template>
        </template>
      </el-menu>
    </el-drawer>
  </el-container>
</template>

<script setup>
import { ElMessageBox } from 'element-plus';
import { RouterView, useRouter, useRoute } from 'vue-router';
import { ref, onMounted, computed, watch } from 'vue';
//import axios from 'axios';
import api from '../js/api';
import { useMenuStore } from '@/stores/menuStore';

const router = useRouter();
const activeMenu = ref('1');
const activeMenu2 = '';
//const menuItems = ref([]);
const activeIndex = ref('');
const openedKeys = ref([]); // 儲存展開的父級選單
const route = useRoute(); // 獲取當前路徑
const menuStore = useMenuStore();
const isDrawerVisible = ref(false); // 控制 el-drawer 的顯示
const isDesktop = computed(() => window.innerWidth >= 768); // 判斷是否為桌面尺寸

// 監聽視窗大小變化
window.addEventListener('resize', () => {
  isDrawerVisible.value = false;
});

const toggleDrawer = () => {
  isDrawerVisible.value = !isDrawerVisible.value;
};

watch(
  () => route.path,
  () => syncMenuWithRoute(route)
);

onMounted(async () => {
  checkIsLogin();
  await menuStore.loadData();
  syncMenuWithRoute(route);
});

const toTopWidth = computed(() => {
  return window.innerWidth <= 768 ? 30 : 20;
});

const toTopHeight = computed(() => {
  return window.innerHeight <= 768 ? 15 : 100;
});

const structuredMenu = computed(() => {
  const menuMap = {};
  const topLevelMenus = [];

  menuStore.menuItems.forEach((item) => {
    menuMap[item.id] = { ...item, children: [], parentId: item.parent };
  });

  menuStore.menuItems.forEach((item) => {
    if (item.parent === '#') {
      topLevelMenus.push(menuMap[item.id]);
    } else {
      if (menuMap[item.parent]) {
        menuMap[item.parent].children.push(menuMap[item.id]);
      }
    }
  });

  return topLevelMenus;
});

//左側選單的當前頁面顯示功能
const syncMenuWithRoute = (route) => {
  activeIndex.value = `/${route.path.split('/')[1]}`;

  // 計算展開的父層選單節點
  const findParentIds = (id) => {
    const parents = [];
    let currentId = id;
    while (currentId) {
      const parentId = menuStore.menuItems.find((item) => item.id === currentId)?.parent;
      if (parentId && parentId !== '#') {
        parents.push(parentId);
        currentId = parentId;
      } else {
        currentId = null;
      }
    }
    return parents;
  };

  const matchedItem = menuStore.menuItems.find((item) => item.link === route.path);
  if (matchedItem) {
    openedKeys.value = findParentIds(matchedItem.id);
  }
};

// menu點選
const handleSelect = (index) => {
  activeIndex.value = index;
  console.log('Selected menu index:', index);
  // router.push(index); //因為會警告找不到index的路徑，也不需在這直接連去頁面，所以先註解掉
};

const menuSelect = (key, keyPath) => {
  console.log(key, keyPath);
};

const logout = () => {
  api.axiosGetAsync('/admin-auth/log-out');
  router.push('../../login');
};

const checkIsLogin = async () => {
  await api.axiosGetAsync('/admin-auth/is-login').then((response) => {
    if (response.data.isLogin === false) {
      console.log('使用者未登入', response);
      ElMessageBox.alert('請先登入', '錯誤', {
        confirmButtonText: '確認',
        type: 'error',
        showClose: false, // 右上角X
      }).then(() => {
        router.push('./login');
      });
    }
  });
};
</script>

<style>
html,
body {
  height: 100%;
  margin: 0;
  padding: 0;
}

.el-header {
  width: calc(100% - 220px);
  position: fixed;
  top: 0;
  padding: 0;
  z-index: 2003;
}

@media screen and (max-width: 767px) {
  .el-header {
    width: 100%;
  }
}

.el-main {
  margin-top: 60px;
  padding: 20px 20px 60px;
}

.el-aside {
  border-right: 1px solid var(--el-menu-border-color);
}

@media screen and (min-width: 768px){
  .el-aside {
    position: fixed;
    top: 0;
    left: 0;
  }
}

.el-menu {
  border: none;
}

.el-container{
  min-height: 100vh;
}

@media screen and (min-width: 768px){
  .el-aside + .el-container{
    padding-left: 220px; /*el-aside的寬度*/
  }
}

.el-drawer__header {
  display: none; /* 隱藏標題區塊 */
}

.sidebar {
  height: 100vh;
  overflow-y: auto;
}

::-webkit-scrollbar {
  width: 7px;
}

::-webkit-scrollbar-thumb {
  border-radius: 4px;
  background-color: rgba(84, 92, 100, 0.4);
}

.bg-img {
  background-image: url('@/assets/images/background.svg');
  background-position: center;
  background-attachment: fixed;
  background-size: cover;
}

.el-overlay{
  z-index: 2003 !important;
}

.el-popup-parent--hidden .el-select__popper, .el-popup-parent--hidden .el-picker__popper{
  z-index: 2004 !important;
}

.el-popper{
  z-index: 2001 !important;
}

.text-center .cell{
 text-align: center;
}

.whitespace-nowrap .cell{
  white-space: nowrap;
}

.line-height-130p, .line-height-130p .cell{
  line-height: 130% !important;
}

.el-loading-mask{
  z-index: 2006 !important;
}

.el-loading-spinner .path{
  stroke-width: 3px;
  stroke: #fff;
}

.el-loading-spinner .el-loading-text{
  color: #fff;
  font-size: 16px;
}

.el-popup-parent--hidden .el-loading-mask{
  background: none !important;
}

.el-loading-parent--relative .el-dialog{
  opacity: 0;
}

.drag-handle{
  cursor: grab;
}

.form-title{
  margin-right: 5px;
  flex: 0 0 auto;
  line-height: 125%;
  word-break: break-word;
}
</style>

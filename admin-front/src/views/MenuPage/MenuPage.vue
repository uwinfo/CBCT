<template>
  <div class="text-2xl pb-5">後台目錄管理</div>
  <div class="flex gap-x-2.5 menu-wrap">
    <div class="p-6 bg-white w-2/3 mb-6 border border-solid border-slate-200 structured-menu">
      <!-- 表格 -->
      <el-table :data="structuredMenu" stripe style="width: 100%">
        <el-table-column prop="name" label="名稱" min-width="200">
          <template #default="{ row }">
            <!-- 判斷 parent 值來設置樣式 -->
            <span class="cursor-pointer" :class="row.parentUid !== '#' ? 'is-child' : row.children != null && row.children.length > 0 ? 'is-parent' : ''" @click="editMenu(row.uid)">
              {{ row.name }}
            </span>
          </template>
        </el-table-column>
        <!-- <el-table-column prop="text" label="名稱"/> -->
        <!-- <el-table-column prop="parent" label="父UID" class-name="whitespace-nowrap" />
        <el-table-column prop="id" label="UID" class-name="whitespace-nowrap" /> -->
        <el-table-column prop="link" label="連結" min-width="200" />
        <el-table-column prop="sort" label="排序" width="90" />
        <el-table-column fixed="right" label="操作" width="100" label-class-name="text-center">
          <template #default="item">
            <el-button type="primary" :icon="Edit" circle @click="editMenu(item.row.uid)" />
            <el-popconfirm title="請問是否刪除?" confirm-button-text="是" cancel-button-text="否" @confirm="deleteMenu(item.row.uid)">
                <template #reference>
                    <el-button type="danger" :icon="Delete" circle/>
                </template>
            </el-popconfirm>
          </template>
        </el-table-column>
      </el-table>
    </div>
    <div class="p-6 bg-white w-1/3 mb-6 border border-solid border-slate-200 setting-menu">
      <!-- 表单 -->
       <div class="pb-2">
          <h2>新增/修改區塊</h2>
       </div>
      <div class="pb-4">
        <div class="form-title pb-2">名稱：</div>
        <div class="w-full">
          <el-input v-model="editData.name" placeholder="請輸入..." :class="errors.name ? 'is-error' : ''" clearable/>
          <div v-if="errors.name" class="text-red-500 text-sm">{{ errors.name[0] }}</div>
        </div>
      </div>
      <div class="pb-4">
        <div class="form-title pb-2">目錄路徑：</div>
        <el-select v-model="editData.parentUid" placeholder="Select">
          <el-option label="第一層" :value="'#'" />
          <el-option v-for="item in parentList" :key="item.uid" :label="item.name" :value="item.uid" />
        </el-select>
      </div>
      <div class="pb-4">
        <div class="form-title pb-2">連結：</div>
        <el-input v-model="editData.link" placeholder="請輸入" />
      </div>
      <div class="pb-4">
        <div class="form-title pb-2">排序：</div>
        <el-input type="number" v-model="editData.sort" placeholder="請輸入" />
      </div>
      <div class="pb-4">
        <div class="form-title pb-2">權限：</div>
        <el-select v-model="editData.permissionUid" placeholder="Select">
          <el-option v-for="item in dataAdminPermission" :key="item.uid" :label="item.description + ' ' + item.code" :value="item.uid" />
        </el-select>
      </div>
      <div class="flex justify-end">
        <el-button v-if="!isEdit" type="primary" @click="addNewMenu">新增</el-button>
        <el-button v-if="isEdit" type="success" @click="addNewMenu">修改</el-button>
        <el-popconfirm v-if="isEdit" title="請問是否刪除?" confirm-button-text="是" cancel-button-text="否" @confirm="deleteMenu(editData.uid)">
          <template #reference>
              <el-button type="danger">刪除</el-button>
          </template>
        </el-popconfirm>
        <el-button v-if="isEdit" type="warning" @click="cancel">取消</el-button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ElMessageBox } from 'element-plus';
import { ref, computed, onMounted} from 'vue';
import api from '../../js/api';
import {Delete,Edit} from '@element-plus/icons-vue';
import { ElNotification } from 'element-plus';
import { useMenuStore } from '@/stores/menuStore'; // 引入 menuStore

//其他參數
const menuItems = ref([]);
const dataAdminPermission = ref([]);
const parentList =  ref([]);
const editData= ref({
  uid:'',
  name:'',
  link:'',
  sort:5000,
  parentUid:'#',
  permissionUid:'',
});
const isEdit= ref(false);
const menuStore = useMenuStore();
const errors = ref({});

onMounted(async() => {
  await loadData();
  getParentList();
});

const scrollToTop = () => {
window.scrollTo({
  top: 0,
  //behavior: 'smooth'
})
}

const loadData = async()=>{
  try {
      const menuResponse = await api.axiosGetAsync('/admin-menu/list');
      const permissionResponse = await api.axiosGetAsync('/universal/permissions');
      menuItems.value = menuResponse.data
      dataAdminPermission.value = permissionResponse.data
  } catch (error) {
    const payloadErrors = error.response?.data?.invalidatedPayload;
    let errorMessage = '請求失敗';
    if (payloadErrors == null) {
      errorMessage = error.response?.data?.message;
    }
    ElMessageBox.alert(errorMessage, '錯誤', {
      confirmButtonText: '確認',
      type: 'error',
      message: `${error.message}`,
    });
  }
}

const structuredMenu = computed(() => {
  const menuMap = {};
  const topLevelMenus = [];

  menuItems.value.forEach((item) => {
      menuMap[item.uid] = { ...item, children: []};
  });

  menuItems.value.forEach((item) => {
      if (item.parentUid === "#") {
        topLevelMenus.push(menuMap[item.uid]);
      } else {
        if (menuMap[item.parentUid]) {
            menuMap[item.parentUid].children.push(menuMap[item.uid]);
        }
      }
  });
  const flattenMenu = [];
  const flatten = items => {
      items.forEach(item => {
      flattenMenu.push({ ...item});
      if (item.children && item.children.length > 0) {
          flatten(item.children);
      }
      });
  };

  flatten(topLevelMenus);
  return flattenMenu; // 返回展平後的結構
});

const editMenu = async (id) => {
  try {
    errors.value = {};
    isEdit.value = true;
    const response = await api.axiosGetAsync(`/admin-menu?uid=${id}`);

    // 保留原有的 editData 屬性，僅更新從 API 獲取的部分
    editData.value = {
      ...editData.value, // 保留原有的值
      ...response.data, // 更新 API 返回的數據
    };

    scrollToTop(); //移至頁面頂端
  } catch (error) {
    const payloadErrors = error.response?.data?.invalidatedPayload;
    let errorMessage = '請求失敗';
    if (payloadErrors == null) {
      errorMessage = error.response?.data?.message;
    }
    ElMessageBox.alert(errorMessage, '錯誤', {
      confirmButtonText: '確認',
      type: 'error',
      message: `${error.message}`,
    });
  }
};

const getParentList = async () =>{
  try{
    const response = await api.axiosGetAsync('/admin-menu/list');
    parentList.value = response.data.filter((item) =>
        item.parentUid === "#"
    );
  } catch (error) {
    const payloadErrors = error.response?.data?.invalidatedPayload;
    let errorMessage = '請求失敗';
    if (payloadErrors == null) {
      errorMessage = error.response?.data?.message;
    }
    ElMessageBox.alert(errorMessage, '錯誤', {
      confirmButtonText: '確認',
      type: 'error',
      message: `${error.message}`,
    });
  }
};

const addNewMenu = async () => {
  try {
    await api.axiosPostAsync('/admin-menu',editData.value);
    
    ElNotification({
      title: '確認',
      type: 'success',
      message: '已完成',
    });

    // 清除錯誤狀態
    errors.value = {};

    loadData();
    cancel();
    getParentList();
    menuStore.loadData();
  } catch (error) {
    if (error.validationErrors) {
      errors.value = error.validationErrors; // 存 API 回傳的錯誤欄位
    }

    ElNotification({
      title: '錯誤',
      message: '請確認輸入內容',
      type: 'error',
    });
  }
};

const cancel = ()=>{
  editData.value={
      uid:'',
      name:'',
      link:'',
      sort:5000,
      parentUid:'#',
      permissionUid:'',
  }
  isEdit.value = false;
}

const deleteMenu = async (uid) => {
  try {
    await api.axiosDeleteAsync(`/admin-menu?uid=${uid}`);
    await loadData();
    await menuStore.loadData();
  } catch (error) {
    const payloadErrors = error.response?.data?.invalidatedPayload;
    let errorMessage = '請求失敗';
    if (payloadErrors == null) {
      errorMessage = error.response?.data?.message;
    }
    ElMessageBox.alert(errorMessage, '錯誤', {
      confirmButtonText: '確認',
      type: 'error',
      message: `${error.message}`,
    });
  }
};

</script>

<style>
.is-parent{
color: #409eff;
}
.is-parent::after{
content: '';
display: inline-block;
width: 8px;
height: 8px;
margin-left: 12px;
border-top: 1px solid #409eff;
border-right: 1px solid #409eff;
transform: rotate(135deg) translate(0, 5px);
}

.is-child{
display: flex;
}

.is-child::before{
content: '';
display: inline-block;
width: 10px;
height: 1px;
margin: 11px 8px 0 15px;
flex: 0 0 auto;
background: #000;
opacity: .3;
}

@media screen and (max-width: 1024px) {
.menu-wrap{
  flex-direction: column-reverse;
}
.structured-menu, .setting-menu{
  width: 100%;
}
}

</style>
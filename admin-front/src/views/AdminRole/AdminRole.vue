<template>
  <div class="text-2xl pb-5">角色管理</div>
  <div class="p-6 bg-white w-full mb-6 border border-solid border-slate-200">
      <div class="block md:flex">
          <div class="w-full text-left md:w-1/2">
              <el-button type="primary" @click="upsertData()">新增</el-button>
          </div>
      </div>
  </div>
  <div class="p-6 bg-white w-full mb-6 border border-solid border-slate-200">
      <el-table :data="data" stripe style="width: 100%">
        <el-table-column prop="name" label="名稱" min-width="150" />
        <el-table-column prop="sort" label="排序" min-width="60" />
        <el-table-column prop="description" label="說明" min-width="200" />
        <!-- <el-table-column prop="uid" label="UID" min-width="150" class-name="whitespace-nowrap" /> -->
        <el-table-column fixed="right" label="操作" width="60">
          <template #default="item">
              <el-button type="primary" :icon="Edit" circle @click="upsertData(item.row.uid)" />
              <el-popconfirm class="hidden" title="請問是否刪除?" confirm-button-text="是" cancel-button-text="否" @confirm="deleteAdmin(item.row.uid)">
                  <template #reference>
                      <el-button class="hidden" type="danger" :icon="Delete" circle/>
                  </template>
              </el-popconfirm>
          </template>
        </el-table-column>
      </el-table>
      <!-- <page v-if="data.list != null && data.list.length > 0" class="pt-5 text-right" :currentPage="data.currentPage" :total="data.totalRecord" :pageSize="data.pageSize" @updateCurrentPage="handleCurrentPageChange" @updatePageSize="handlePageSizeChange"/> -->
  </div>
  <popup :popppTable="popppTable" @popppTable="updatePopppTable" :isEdit="isEdit" :data="itemData" @update-data="loadList"/>
</template>

<script setup>
import { ElMessageBox } from 'element-plus';
import {ref, onMounted} from 'vue';
import api from '../../js/api';
import {Delete, Edit} from '@element-plus/icons-vue';
import popup from './AdminRoleEdit.vue'; //popup

const data = ref([]);
const popppTable = ref(false);
const isEdit = ref(false);
const itemData = ref([]);

// //頁碼
// const currentPage = ref(1);
// const pageSize = ref(20);

onMounted(async()=>{
  await loadList();
})

const loadList = async()=>{
  try {
      const params = new URLSearchParams();
      // params.append('currentPage', currentPage.value);
      // params.append('pageSize', pageSize.value);

      const dataResponse = await api.axiosGetAsync(`/admin-role/list?${params.toString()}`);
      data.value = dataResponse.data;

      //console.log(data.value);
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

const upsertData= async(uid)=>{
  if(uid == undefined){
    itemData.value= {};
    isEdit.value = false;
  }
  else{
    const dataResponse = await api.axiosGetAsync(`/admin-role?uid=${uid}`);
    itemData.value = dataResponse.data;
    isEdit.value = true;
  }
  popppTable.value = true;
}

const updatePopppTable = (value) => {
  popppTable.value = value;
};

// const handleCurrentPageChange = (newPage) => {
//   currentPage.value = newPage;
//   loadList();
// };

// const handlePageSizeChange = (newSize) => {
//   pageSize.value = newSize;
//   currentPage.value = 1; // 切換頁大小後重置到第一頁
//   loadList(); 
// };

</script>
<template>
  <div class="text-2xl pb-5">權限管理</div>
  <div class="p-6 bg-white w-full mb-6 border border-solid border-slate-200">
      <div class="block md:flex">
          <div class="w-full text-left md:w-1/2">
              <el-button type="primary" @click="upsertData()">新增</el-button>
          </div>
      </div>
  </div>
  <div class="p-6 bg-white w-full mb-6 border border-solid border-slate-200">
      <el-table :data="data.list" stripe style="width: 100%">
        <el-table-column prop="description" label="名稱" min-width="150" />
        <el-table-column prop="code" label="Code" min-width="200" />
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
      <page v-if="data.list != null && data.list.length > 0" class="pt-5 text-right" :currentPage="data.currentPage" :total="data.totalRecord" :pageSize="data.pageSize" @updateCurrentPage="handleCurrentPageChange" @updatePageSize="handlePageSizeChange"/>
  </div>
  <popup :popppTable="popppTable" @popppTable="updatePopppTable" :isEdit="isEdit" :data="itemData" @update-data="loadData"/>
</template>

<script setup>
import {ref, onMounted} from 'vue';
import api from '../../js/api';
import {Delete, Edit} from '@element-plus/icons-vue';
import page from '../../components/PaginationBlock.vue'; //頁碼元件
import popup from './AdminPermissionEdit.vue'; //popup

const data = ref([]);
const popppTable = ref(false);
const isEdit = ref(false);
const itemData = ref([]);

//頁碼
const currentPage = ref(1);
const pageSize = ref(20);

onMounted(async()=>{
  await loadData();
})

const loadData = async()=>{
  try {
      const params = new URLSearchParams();
      params.append('currentPage', currentPage.value);
      params.append('pageSize', pageSize.value);

      const dataResponse = await api.axiosGetAsync(`/admin-permission/list?${params.toString()}`);
      data.value = dataResponse.data;
  } catch (error) {
      console.error('Error loading data:', error);
  }
}

const upsertData= async(uid)=>{
  if(uid == undefined){
    itemData.value= {};
    isEdit.value = false;
  }
  else{
    const dataResponse = await api.axiosGetAsync(`/admin-permission?uid=${uid}`);
    itemData.value = dataResponse.data;
    isEdit.value = true;
  }
  popppTable.value = true;
}

const updatePopppTable = (value) => {
  popppTable.value = value;
};

const handleCurrentPageChange = (newPage) => {
currentPage.value = newPage;
loadData();
};

const handlePageSizeChange = (newSize) => {
pageSize.value = newSize;
currentPage.value = 1; // 切換頁大小後重置到第一頁
loadData(); 
};

</script>
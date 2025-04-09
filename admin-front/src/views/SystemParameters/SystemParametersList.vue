<template>
  <div class="text-2xl pb-2">系統參數</div>
  <div class="text-red-600 pb-5">
    <el-icon ><WarningFilled /></el-icon>
      請勿隨意更動該頁面的參數
  </div>
  <div class="p-6 bg-white w-full mb-6 border border-solid border-slate-200">
      <el-form class="block md:grid grid-cols-2 gap-6">
        <el-form-item label="搜尋：">
          <el-input v-model="keyword" placeholder="請輸入名稱..." clearable/>
        </el-form-item>
        <div class="w-full text-right">
            <el-button color="#0162e8" type="primary" @click="search(keyword)"><el-icon><Search /></el-icon>查詢</el-button>
            <el-button type="primary" @click="upsertData()">新增</el-button>
        </div>
      </el-form>
      <!-- <div class="block md:flex">
          <div class="w-full text-left md:w-1/2">
              <el-button type="primary" @click="upsertData()">新增</el-button>
          </div>
      </div> -->
  </div>
  <div class="p-6 bg-white w-full mb-6 border border-solid border-slate-200">
      <el-table :data="data.list" stripe style="width: 100%">
        <el-table-column prop="name" label="名稱" min-width="200" />
        <el-table-column prop="content" label="參數" min-width="200" />
        <el-table-column prop="description" label="敘述" min-width="200" />
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
import popup from './SystemParametersEdit.vue'; //popup

const data = ref([]);
const popppTable = ref(false);
const isEdit = ref(false);
const itemData = ref([]);
const keyword = ref('');

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

      if (keyword.value) {
          params.append('keyword', keyword.value);
      }

      const dataResponse = await api.axiosGetAsync(`/sys_config/list?${params.toString()}`);
      data.value = dataResponse.data;

      //console.log(data.value);
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
    const dataResponse = await api.axiosGetAsync(`/sys_config?uid=${uid}`);
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

const search = async(item)=>{
    keyword.value = item;
    currentPage.value = 1;
    await loadData();
}

</script>

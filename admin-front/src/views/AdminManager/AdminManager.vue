<template>
    <div class="text-2xl pb-5">管理員管理</div>
    <div class="p-6 bg-white w-full mb-6 border border-solid border-slate-200">
      <el-form class="block md:grid grid-cols-2 gap-6">
        <el-form-item label="搜尋：">
          <el-input v-model="searchInput" placeholder="請輸入姓名/Email..." clearable/>
        </el-form-item>
        <div class="w-full text-right">
            <el-button color="#0162e8" type="primary" @click="search(searchInput)"><el-icon><Search /></el-icon>查詢</el-button>
            <el-button type="primary" @click="addNewData">新增</el-button>
        </div>
      </el-form>
    </div>
    <div class="p-6 bg-white w-full mb-6 border border-solid border-slate-200">
        <el-table :data="data.list" stripe style="width: 100%">
          <!-- <el-table-column prop="uid" label="UID" min-width="150" class-name="whitespace-nowrap" /> -->
          <el-table-column prop="name" label="姓名" />
          <el-table-column prop="email" label="Email" min-width="250" />
          <el-table-column prop="mobile" label="手機" min-width="120" />
          <el-table-column prop="enStatus" label="狀態" min-width="120" />
          <el-table-column fixed="right" label="操作" width="100" label-class-name="text-center">
            <template #default="item">
                <el-button type="primary" :icon="Edit" circle @click="editAdmin(item.row.uid)" />
                <!-- <el-button type="danger" :icon="Delete" circle @click="deleteAdmin(item.row.uid)"/> -->
                <el-popconfirm title="請問是否刪除?" confirm-button-text="是" cancel-button-text="否" @confirm="deleteAdmin(item.row.uid)">
                    <template #reference>
                        <el-button type="danger" :icon="Delete" circle/>
                    </template>
                </el-popconfirm>
            </template>
          </el-table-column>
        </el-table>
        <page v-if="data.list != null && data.list.length > 0" class="pt-5 text-right" :currentPage="data.currentPage" :total="data.totalRecord" :pageSize="data.pageSize" @updateCurrentPage="handleCurrentPageChange" @updatePageSize="handlePageSizeChange"/>
    </div>
</template>

<script setup>
import { ElMessageBox } from 'element-plus';
import {ref, onMounted} from 'vue';
import api from '../../js/api';
import {Delete, Edit} from '@element-plus/icons-vue';
import {useRouter} from 'vue-router';
import page from '../../components/PaginationBlock.vue'; //頁碼元件

const router = useRouter();
const data = ref([]);
const keyword = ref('');
const searchInput = ref('');

const statusMapping = {
  100: '正常',
  '-100': '鎖定'
};

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

      const dataResponse = await api.axiosGetAsync(`/admin-user?${params.toString()}`);
      data.value = dataResponse.data;
      
      data.value.list = data.value.list.map((x) => {
        return {
          ...x,
          enStatus: statusMapping[String(x.enStatus)] || '',
        };
      });
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

const search = async(item)=>{
    keyword.value = item;
    currentPage.value = 1;
    await loadData();
}

const editAdmin = (uid)=>{
    console.log(uid);
    router.push(`/admin-manager/data?uid=${uid}`)
}

const addNewData = ()=>{
    router.push('/admin-manager/data');
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

const deleteAdmin = async (uid) => {
  try {
    await api.axiosDeleteAsync(`/admin-user?uid=${uid}`);
    await loadData();
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

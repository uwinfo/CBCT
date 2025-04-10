<template>
    <h2 class="text-xl pb-4">管理員帳號管理</h2>
    <div class="block md:flex gap-x-2.5">
        <div class="p-6 bg-white w-full md:w-1/2 mb-6 border border-solid border-slate-200">
            <div class="flex pb-4 items-start">
              <div class="form-title">Uid：</div>
                <div class="w-full flex items-center">
                  <div class="text-sm" style="color: #606266;" v-if="data.uid == '' || data.uid == null">該欄位為自動產生</div>
                  <div class="text-sm" style="color: #606266;" v-if="data.uid != null && data.uid != ''">{{ data.uid }}</div>
                </div>
            </div>
            <div class="pb-4">
                <div class="form-title pb-2"><span class="text-red-600">*</span>姓名：</div>
                <div class="w-full">
                  <el-input v-model="data.name" placeholder="請輸入..." :class="errors.name ? 'is-error' : ''" clearable/>
                  <div v-if="errors.name" class="text-red-500 text-sm">{{ errors.name[0] }}</div>
                </div>
            </div>
            <div class="pb-4">
                <div class="form-title pb-2">Email：</div>
                <div class="w-full">
                  <el-input v-model="data.email" placeholder="請輸入..." :class="errors.email ? 'is-error' : ''" clearable/>
                  <div v-if="errors.email" class="text-red-500 text-sm">{{ errors.email[0] }}</div>
                </div>
            </div>
            <div class="pb-4">
                <div class="form-title pb-2">手機：</div>
                <div class="w-full">
                  <el-input type="number" oninput="if(value.length > 10) value=value.slice(0, 10)" v-model="data.mobile" placeholder="請輸入..." :class="errors.mobile ? 'is-error' : ''" clearable/>
                  <div v-if="errors.mobile" class="text-red-500 text-sm">{{ errors.mobile[0] }}</div>
                </div>
            </div>
            <div class="flex pb-4 items-center">
                <div class="form-title mr-2">狀態：</div>
                <el-switch v-model="enStatus" size="large" inline-prompt style="--el-switch-on-color: #13ce66; --el-switch-off-color: #ff4949; height: 32px;" active-text="正常" inactive-text="鎖定" />
            </div>
            <div class="pb-4">
                <div class="form-title pb-2"><span class="text-red-600">*</span>密碼：</div>
                <div class="w-full">
                  <el-input v-model="data.secret" type="password" placeholder="8~20碼，需包含大小寫英文字母、數字 (如不需修改密碼可不用輸入)" :class="errors.secret ? 'is-error' : ''" show-password/>
                  <div v-if="errors.secret" class="text-red-500 text-sm">{{ errors.secret[0] }}</div>
                </div>
            </div>
            <div class="pb-4">
                <div class="form-title pb-2">備註：</div>
                <el-input v-model="data.backMemo" :autosize="{ minRows: 3, maxRows: 5 }" type="textarea" placeholder="請輸入..."/>
            </div>
        </div>
        <div class="p-6 bg-white w-full md:w-1/2 mb-6 border border-solid border-slate-200">
            <div class="pb-2">角色</div>
            <el-table :data="roleListWithStatus" stripe style="width: 100%">
            <el-table-column prop="id" label="Id" width="60"/>
            <el-table-column prop="name" label="名稱" />
            <el-table-column prop="description" label="簡述" />
            <el-table-column fixed="right" width="65">
                    <template #header>
                        <el-switch v-model="roleAll" @change="allRoleChange(roleAll)"/>
                    </template>
                    <template #default="scope">
                        <el-switch
                        v-model="scope.row.role"
                        @change="updateRoleStatus(scope.row, scope.row.role)"
                        />
                    </template>
            </el-table-column>
            </el-table>
            <div class="flex justify-end pt-5">
                <el-button v-if="!isEdit" type="primary" @click="addNewData">新增</el-button>
                <el-button v-if="isEdit" type="success" @click="addNewData">修改</el-button>
                <el-popconfirm v-if="isEdit" title="請問是否刪除?" confirm-button-text="是" cancel-button-text="否" @confirm="deleteData(data.uid)">
                    <template #reference>
                        <el-button type="danger">刪除</el-button>
                    </template>
                </el-popconfirm>
                <el-button type="warning" @click="cancel">取消</el-button>
            </div>
        </div>
    </div>
</template>

<script setup>
import { ElMessageBox } from 'element-plus';
import { ref, onMounted, computed } from 'vue';
import { useRoute ,useRouter} from 'vue-router'; 
import api from '../../js/api';
import { ElNotification } from 'element-plus';

const route = useRoute();
const router = useRouter();
const roleList = ref([]);
const data = ref({
    uid :'',
    name:'',
    email:'',
    mobile:'',
    enStatus: -100,
    adminRoleUids:[],
    backMemo:''
});
const isEdit= ref(false);
const roleAll = ref(false);
const errors = ref({});

const userId = computed(()=>{
    return route.query.uid;
})

const enStatus = computed({
  get() {
    return data.value.enStatus === 100;
  },
  set(value) {
    data.value.enStatus = value ? 100 : -100;
  },
});

const roleListWithStatus = computed(() =>
  roleList.value.map((role) => ({
    ...role,
    role: data.value.adminRoleUids.includes(role.uid)
  }))
);

onMounted(async()=>{
    await loadData();
    await loadRole();
})

const loadData = (async()=>{
    const params = new URLSearchParams(window.location.search);
    const uid = params.get('uid');
    if(uid !== null){
        isEdit.value = true;
    }
    else{
        isEdit.value = false;
    }

    if(isEdit.value == false){
        return;
    }

    const dataResponse = await api.axiosGetAsync(`/admin-user/get?uid=${userId.value}`);
    data.value = dataResponse.data;
})

const loadRole=(async()=>{
    const rolesResponse = await api.axiosGetAsync('/universal/roles');
    roleList.value = rolesResponse.data;
})

const updateRoleStatus = (row, value) => {
  if (value) {
    // 如果開啟，新增 UID
    if (!data.value.adminRoleUids.includes(row.uid)) {
      data.value.adminRoleUids.push(row.uid);
    }
  } else {
    // 如果關閉，移除 UID
    data.value.adminRoleUids = data.value.adminRoleUids.filter((uid) => uid !== row.uid);
  }
};

const allRoleChange = (item)=>{
    if(item){
        data.value.adminRoleUids = roleList.value.map(x => x.uid);
    }
    else{
        data.value.adminRoleUids = []
    }
}

const addNewData = async () => {
  try {
    const postData = data.value;
    await api.axiosPostAsync('/admin-user',postData)
    
    ElNotification({
      title: '確認',
      type: 'success',
      message: '已完成',
    });

    // 清除錯誤狀態
    errors.value = {};

    loadData();
    cancel();
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

const deleteData = async (uid) => {
  try {
    await api.axiosDeleteAsync(`/admin-user?uid=${uid}`);
    cancel();
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

const cancel = ()=>{
    router.push('/admin-manager/list');
}
</script>
<template>
  <el-dialog v-model="localPopppTable" title="角色設定" width="auto" :before-close="handleClose" @close="closePopup" class="responsive-dialog">
    <div class="block">
      <div class="flex pb-4 items-start">
          <div class="form-title">Uid：</div>
          <div class="w-full flex items-center">
            <div class="text-sm" style="color: #606266;" v-if="localData.uid == '' || localData.uid == null">該欄位為自動產生</div>
            <div class="text-sm" style="color: #606266;" v-if="localData.uid != null && localData.uid != ''">{{ localData.uid }}</div>
          </div>
      </div>
      <div class="pb-4">
          <div class="form-title pb-2">名稱：</div>
          <div class="w-full">
            <el-input v-model="localData.name" placeholder="請輸入..." :class="errors.name ? 'is-error' : ''" clearable/>
            <div v-if="errors.name" class="text-red-500 text-sm">{{ errors.name[0] }}</div>
          </div>
      </div>
      <div class="pb-4">
          <div class="form-title pb-2">排序：</div>
          <div class="w-full">
            <el-input type="number" v-model="localData.sort" placeholder="請輸入..." :class="errors.sort ? 'is-error' : ''" clearable/>
            <div v-if="errors.sort" class="text-red-500 text-sm">{{ errors.sort[0] }}</div>
          </div>
      </div>
      <div class="pb-4">
          <div class="form-title pb-2">說明：</div>
          <div class="w-full">
            <el-input v-model="localData.description" placeholder="請輸入..." :class="errors.description ? 'is-error' : ''" clearable/>
            <div v-if="errors.description" class="text-red-500 text-sm">{{ errors.description[0] }}</div>
          </div>
      </div>
      <div class="pb-4">
        <div class="form-title pb-2">權限：</div>
        <div class="w-full">
          <el-select
            v-model="localData.permissions"
            multiple
            clearable
            collapse-tags
            placeholder="請選擇..."
            popper-class="custom-header"
            :max-collapse-tags="3"
          >
            <template #header>
              <el-checkbox class="flex" v-model="checkAll" :indeterminate="isIndeterminate" @change="handleCheckAllChange">All</el-checkbox>
            </template>
            <el-option v-for="item in dataAdminPermission" :key="item.uid" :label="item.description" :value="item.code" />
          </el-select>
          <div v-if="errors.description" class="text-red-500 text-sm">{{ errors.description[0] }}</div>
        </div>
      </div>
      <div class="flex justify-end pt-5">
          <el-button v-if="!props.isEdit" type="primary" @click="upsertData">新增</el-button>
          <el-button v-if="props.isEdit" type="success" @click="upsertData">修改</el-button>
          <el-button type="warning" @click="closePopup">取消</el-button>
      </div>
    </div>
  </el-dialog>
</template>

<script setup>
import {ref, onMounted, watch} from 'vue';
import { ElNotification } from 'element-plus';
import api from '../../js/api';

const props = defineProps({
  data: Object,
  popppTable: Boolean,
  isEdit: Boolean,
})

const prepareData = async () => {
  const permissionResponse = await api.axiosGetAsync('/universal/permissions');
  dataAdminPermission.value = permissionResponse.data;
};

onMounted(() => {
  prepareData().then(() => {
    watch(() => props.popppTable, (newVal) => {
      localPopppTable.value = newVal;
    });

    watch(
      () => props.data,
      (newVal) => {
        localData.value = { ...newVal };
        if (localData.value && Object.keys(localData.value).length > 0) {
          // console.log(localData.value);
          const permissionsArray = Array.isArray(localData.value.permissions)
            ? localData.value.permissions
            : localData.value.permissions.split(',');

          localData.value.permissions = permissionsArray.filter((perm) =>
            dataAdminPermission.value.some((item) => item.code === perm)
          );
        }
      },
      { immediate: true } // 確保初始化時也同步一次
    );
  });
});

//全選下拉
const checkAll = ref(false);
const isIndeterminate = ref(false);

const dataAdminPermission = ref([]);
const errors = ref({});
const localPopppTable = ref(props.popppTable);
const localData = ref({ ...props.data });

const emit = defineEmits(['popppTable','update-data']);

const handleCheckAllChange = (val) => {
  localData.value.permissions = val ? dataAdminPermission.value.map(item => item.code) : [];
  // console.log(localData.value.permissions);
}

watch(
  () => localData.value.permissions,
  (newVal) => {
    //當newVal是undefined的時候
    if (!Array.isArray(newVal)) {
      isIndeterminate.value = false;
      checkAll.value = false;
      return;
    }
    const total = dataAdminPermission.value.length;
    const checkedCount = newVal.length;
    isIndeterminate.value = checkedCount > 0 && checkedCount < total;
    checkAll.value = checkedCount === total;
  }
)


const closePopup =()=>{
  emit('popppTable', false);
  errors.value = {};
};

const handleClose=(done)=>{
  done()
};

const upsertData = async () => {
  try {
    const postData = localData.value;
    if(postData.uid === undefined){
      postData.uid = '';
    }
    if(postData.permissions!=='' ){
        postData.permissions = postData.permissions.join();
    }
    await api.axiosPostAsync('/admin-role',postData)
    
    ElNotification({
      title: '確認',
      type: 'success',
      message: '已完成',
    });

    // 清除錯誤狀態
    errors.value = {};

    emit('update-data'); 
    closePopup();
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
</script>

<style>
.responsive-dialog {
  max-width: 1000px;
}
@media screen and (max-width: 768px) {
  .responsive-dialog {
    max-width: 85%;
  }
}
</style>
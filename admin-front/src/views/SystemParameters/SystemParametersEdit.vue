<template>
  <el-dialog v-model="localPopppTable" title="系統參數設定" width="auto" :before-close="handleClose" @close="closePopup" class="responsive-dialog">
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
          <div class="form-title pb-2">參數：</div>
          <div class="w-full">
            <el-input v-model="localData.content" placeholder="請輸入..." :class="errors.content ? 'is-error' : ''" clearable/>
            <div v-if="errors.content" class="text-red-500 text-sm">{{ errors.content[0] }}</div>
          </div>
      </div>
      <div class="pb-4">
          <div class="form-title pb-2">敘述：</div>
          <div class="w-full">
            <el-input v-model="localData.description" placeholder="請輸入..." :class="errors.description ? 'is-error' : ''" clearable/>
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
import {ref, watch} from 'vue';
import { ElNotification } from 'element-plus';
import api from '../../js/api';

const errors = ref({});
const props = defineProps({
  data: Object,
  popppTable: Boolean,
  isEdit: Boolean,
})

const localPopppTable = ref(props.popppTable);
const localData = ref({ ...props.data });

const emit = defineEmits(['popppTable','update-data']);

watch(() => props.popppTable, (newVal) => {
  localPopppTable.value = newVal;
});

watch(
  () => props.data,
  (newVal) => {
    localData.value = { ...newVal };
  },
  { immediate: true } // 確保初始化時也同步一次
);

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
    await api.axiosPatchAsync('/sys_config',postData)
    
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
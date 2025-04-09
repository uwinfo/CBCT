<template>
  <div class="pagination-block">
    <el-pagination
      class="flex pagination-wrapper justify-end flex-wrap"
      v-model:current-page="localCurrentPage"
      v-model:page-size="localPageSize"
      :page-sizes="[20, 50, 100, 500]"
      :size="size"
      :background="background"
      :pager-count="5"
      layout="total, sizes, prev, pager, next"
      :total="total"
      @size-change="handleSizeChange"
      @current-change="handleCurrentChange"
    />
  </div>
</template>

<script setup>
import {ref, watch, defineProps, defineEmits, onMounted} from 'vue';
const background = ref(true);
const size = ref(window.innerWidth < 767 ? 'small' : 'default');
const props = defineProps({
  currentPage: {
    type: Number,
    required: true,
    default: 1
  },
  pageSize: {
    type: Number,
    required: true,
    default: 20
  },
  total: {
    type: Number,
    required: true,
    default: 1000
  }
});

const emit = defineEmits(['updateCurrentPage', 'updatePageSize']);

onMounted(() => {
  window.addEventListener('resize', size);
});

//用於同步 props
const localCurrentPage = ref(props.currentPage);
const localPageSize = ref(props.pageSize);

watch(() => props.currentPage, (newVal) => {
  localCurrentPage.value = newVal;
});

watch(() => props.pageSize, (newVal) => {
  localPageSize.value = newVal;
});

// 當分頁狀態改變時，觸發通知父組件
function handleSizeChange(newSize) {
  emit('updatePageSize', newSize);
}

function handleCurrentChange(newPage) {
  emit('updateCurrentPage', newPage);
}

</script>

<style>
.pagination-block{
  margin: 0 -25px;
  padding-left: 25px;
  padding-right: 25px;
}
@media screen and (max-width: 767px){
  .pagination-block{
    margin: 0 -25px;
    padding-left: 20px;
    padding-right: 20px;
  }
}
@media screen and (max-width: 768px){
  .pagination-wrapper {
    display: flex;
    flex-wrap: wrap;
    gap: 8px;
    justify-content: flex-end;
  }
  .pagination-wrapper .el-pagination__sizes {
    flex-basis: 100%; 
    text-align: right; 
  }

  .pagination-wrapper .el-pagination__total {
    flex-basis: 100%;
    text-align: right;
  }

  .pagination-wrapper .el-pagination__prev,
  .pagination-wrapper .el-pagination__next {
    flex-basis: 100%;
    text-align: right;
  }
}
</style>
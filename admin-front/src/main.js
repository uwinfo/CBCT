import './assets/main.css'

import { createApp } from 'vue'
import { createPinia } from 'pinia'
import axios from 'axios'
import VueAxios from 'vue-axios'
import ElementPlus from 'element-plus'
import 'element-plus/dist/index.css'
import App from './App.vue'
import router from './router'
import * as ElementPlusIconsVue from '@element-plus/icons-vue';
import './index.css'
import $ from 'jquery'
window.$ = window.jQuery = $
import moment from 'moment'
import zhTw from 'element-plus/es/locale/lang/zh-tw'; // 引入繁體中文語言包

const app = createApp(App)

for (const [key, component] of Object.entries(ElementPlusIconsVue)) {
    app.component(key, component)
  }

  
axios.defaults.withCredentials = true;
app.config.globalProperties.$moment = moment;
app.use(ElementPlus, {
  locale: zhTw, // 設定為繁體中文
});
app.use(createPinia())
app.use(router)
app.use(VueAxios, axios)
app.mount('#app')

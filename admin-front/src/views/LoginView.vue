<template>
  <div class="flex justify-center items-center min-h-screen bg-gray-100 bg-img">
    <el-card class="w-full max-w-md shadow-lg">
      <img class="w-28 m-auto mb-5" src="@/assets/images/logo.jpg" alt="Element logo" />
      <h2 class="text-center text-2xl font-bold mb-5">CBCT Diagnostic<br>Management Platform</h2>
      <el-form class="login-form" label-position="top">
        <!-- 帳號 -->
        <el-form-item label="帳號">
          <el-input v-model="username" placeholder="請輸入電子信箱"></el-input>
        </el-form-item>

        <!-- 密碼 -->
        <el-form-item label="密碼">
          <el-input v-model="password" placeholder="請輸入密碼" show-password></el-input>
        </el-form-item>

        <!-- 動態密碼 -->
        <el-form-item label="動態密碼">
          <el-input v-model="otp" placeholder="請輸入動態密碼"></el-input>
        </el-form-item>

        <!-- 登入按鈕 -->
        <el-form-item>
          <el-button type="primary" class="w-full" @click="login">登入</el-button>
        </el-form-item>
        <el-divider>動態密碼</el-divider>
        <div class="text-purple-800">首次啟動動態密碼，請先輸入"帳號"及"密碼"，再點選"取得動態密碼"</div>
        <el-form-item>
          <el-button color="#626aef" :dark="isDark" class="w-full" @click="getOpt">取得動態密碼</el-button>
        </el-form-item>
      </el-form>
    </el-card>
  </div>
</template>

<script setup>
import { ElMessageBox } from 'element-plus';
import { ref,onMounted } from 'vue';
import { useRouter } from 'vue-router';
import api from '../js/api'

const username = ref('');
const password = ref('');
const otp = ref('');
const router = useRouter();

onMounted(async()=>{
  await checkIsLogin();
  await useKeycodeToLogin();
})

const useKeycodeToLogin = () => {
  const inputs = document.querySelectorAll("input");
  
  inputs.forEach((input) => {
      input.addEventListener("focus", () => {
        if (!document.body.classList.contains("el-popup-parent--hidden")) {
          document.addEventListener("keydown", handleKeydown);
        }
      });

      input.addEventListener("blur", () => {
        document.removeEventListener("keydown", handleKeydown);
      });

      input.addEventListener("input", () => {
        document.addEventListener("keydown", handleKeydown);
    });
  });

  function handleKeydown(event) {
      // console.log("按鍵值 (key):", event.key);
      if (event.key == 'Enter') {
        login();
      }
  }
}

const login = async () => {
  const postData = {
    email: username.value,
    secret: password.value,
    otp: otp.value,
  };
  //console.log(postData);
  await api.axiosPostAsync('/admin-auth/log-in', postData)
  .then(() => {
    //console.log(response);
    router.push('/');
  })
  .catch((error) => {
    ElMessageBox.alert(error.message, '錯誤', {
      confirmButtonText: '確認',
      type: 'error',
      message: `${error.message}`,
    });
  });
};

const getOpt = function(){
  const postData = {
    email: username.value,
    secret: password.value,
    otp: '',
  };
  api.axiosPostAsync('/admin-auth/get-otp', postData)
  .then((response) => {
      console.log(response);
      ElMessageBox.alert(response, '通知', {
        confirmButtonText: '確認',
        type: 'success',
        message: `信件已發送到該信箱，請前去確認。`,
      });
    })
    .catch((error) => {
      const payloadErrors = error.response?.data?.invalidatedPayload;
      let errorMessage = '請求失敗';
      if (payloadErrors == null) {
        errorMessage = error.response?.data?.message;
      }

      if (payloadErrors) {
        const usernameError = payloadErrors.username?.[0] || '';
        const passwordError = payloadErrors.password?.[0] || '';
        errorMessage = `${usernameError} ${passwordError}`.trim();
      }
      ElMessageBox.alert(errorMessage, '錯誤', {
        confirmButtonText: '確認',
        type: 'error',
        message: `${error.message}`,
      });
    });
}

const checkIsLogin = async () => {
  //console.log(postData);
  await api.axiosGetAsync('/admin-auth/is-login')
  .then((response) => {
    if(response.data === true){
      router.push('/');
    }
  })
  .catch((error) => {
    ElMessageBox.alert(error.message, '錯誤', {
      confirmButtonText: '確認',
      type: 'error',
      message: `${error.message}`,
    });
  });
};
</script>

<style>
body {
  margin: 0;
}

.el-card {
  padding: 20px;
}

.login-form {
  width: 100%;
}

</style>

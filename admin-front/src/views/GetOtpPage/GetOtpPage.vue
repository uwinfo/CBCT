<template>
    <div class="py-10 bg-gray-100 min-h-screen h-full">
        <el-card class="w-5/6 md:w-1/2 mx-auto bg-white p-3 is-always-shadow">
            <div class="text-xl">請用手機掃描QR Code</div>
            <div class="text-red-600">注意，開啟連結後，舊的動態驗証碼會自動失效。</div>
            <div class="mx-auto">
                <img id="qrcode" class="w-1/2 mx-auto" :src="otpData">
            </div>
            <div class="text-center"><el-button type="primary" @click="backToLoginPage">回登入頁</el-button></div>
            <div class="pt-4">動態密碼設定教學</div>
            <div>Step1.</div>
            <div>用手機下載Google Authenticator</div>
            <div class="flex items-baseline py-1.5">
                <a href="https://itunes.apple.com/tw/app/google-authenticator/id388497605?mt=8" target="_blank" class="w-48">
                    <img src="../../assets/images/app-store-badge.png">
                </a>
                <a href="https://play.google.com/store/apps/details?id=com.google.android.apps.authenticator2" target="_blank" class="w-48">
                    <img src="../../assets/images/google-play-badge.png">
                </a>
            </div>
            <div class="pt-2">Step2.</div>
            <div>開啟Google Authenticator，點選右下角的 "+"，再點選"掃描QR Code"。</div>
            <div class="pt-2">Step3.</div>
            <div>掃描上方QR Code，即可設定完成。</div>
            <div class="pt-2">Step4.</div>
            <div>點選"回登入頁"，即可在動態密碼輸入框輸入6位數字密碼</div>
        </el-card>
    </div>
</template>

<script setup>
import { ref,onMounted } from 'vue';
import { useRouter } from 'vue-router';
import api from '../../js/api'
const router = useRouter();
const uid = ref('');
const otpConfirm = ref('');
const otpData = ref('');

const backToLoginPage= () =>{
    router.push('/login');
};

onMounted(async() => {
    getUrl();
    const data = await api.axiosGetAsync(`/admin-auth/get-otp-qrcode-image?uid=${uid.value}&otpConfirm=${otpConfirm.value}`);
    otpData.value = data.data.imageData;
});

const getUrl= () =>{
    const urlParams = new URLSearchParams(window.location.search);
    uid.value = urlParams.get('uid');
    otpConfirm.value = urlParams.get('otpConfirm');
};

</script>
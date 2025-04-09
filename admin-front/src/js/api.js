import $ from 'jquery';
import axios from 'axios';
//import uwTool from './uw-tool.js';
axios.defaults.withCredentials = true
import { ElNotification } from 'element-plus';
import { ElLoading } from 'element-plus'

let requestCount = 0; // 計數器
let loadingInstance = null;
const defaultObj = {
  async axiosGetAsync(url, noloading) {
    console.log("axiosGetAsync: " + url);
    const startTime = Date.now();
    //Loading畫面
    if (requestCount === 0 && (!noloading || noloading !== 'isNoLoading')) { 
      //參數noloading為'isNoLoading'，才不會有loading畫面，叫其他名稱或設定null等都會有loading畫面
      loadingInstance = ElLoading.service({
        lock: true,
        text: '載入中',
        background: 'rgba(0, 0, 0, 0.5)',
      });
    }
    requestCount++;
    
    let date
    if (url.indexOf('?') > -1) {
      date = `&${(new Date()).getTime()}`
    } else {
      date = `?${(new Date()).getTime()}`
    }

    if (url.startsWith('/') && !url.startsWith(`${import.meta.env.VITE_API_ROOT_URL}/`)) {
      url = `${import.meta.env.VITE_API_ROOT_URL}${url}${date}`;
    }
    try {
      return await axios.get(url, {
        // headers: {
        //   'Corp-Code': uwTool.getCoId()
        // },
        withCredentials: true
      });
    }
    catch (error) {
      if (error.response?.data) {
        const responseData = error.response.data;

        // 包含 message & validationErrors
        throw {
          message: responseData.message || "發生未知錯誤",
          validationErrors: responseData.invalidatedPayload || null,
          status: error.response.status || 500
        };
      }

      console.error('API Error:', error);
      throw error;
    }
    finally {
      requestCount--;
      const elapsedTime = Date.now() - startTime; // 請求耗時
      const MIN_LOADING_TIME = 500; // 顯示時間(毫秒)

      const delay = Math.max(0, MIN_LOADING_TIME - elapsedTime);
      setTimeout(() => {
        if (requestCount === 0 && loadingInstance) {
          loadingInstance.close();
          loadingInstance = null;
        }
      }, delay);
    }

  },

  async axiosPostAsync(url, data, showLoading = false) {
    if (url.startsWith('/') && !url.startsWith(`${import.meta.env.VITE_API_ROOT_URL}/`)) {
      url = `${import.meta.env.VITE_API_ROOT_URL}${url}`;
    }

    const startTime = Date.now();
    if (showLoading) {
      //Loading畫面
      if (requestCount === 0) {
        loadingInstance = ElLoading.service({
          lock: true,
          text: 'Loading',
          background: 'rgba(0, 0, 0, 0.35)',
        });
      }
      requestCount++;
    }

    try {
      return await axios.post(url, data, { withCredentials: true });
    } catch (error) {
      if (error.response?.data) {
        const responseData = error.response.data;

        // 包含 message & validationErrors
        throw {
          message: responseData.message || "發生未知錯誤",
          validationErrors: responseData.invalidatedPayload || null,
          status: error.response.status || 500
        };
      }
      throw error;
    }
    finally {
      if (showLoading) {
        requestCount--;
        const elapsedTime = Date.now() - startTime; // 請求耗時
        const MIN_LOADING_TIME = 500; // 顯示時間(毫秒)

        const delay = Math.max(0, MIN_LOADING_TIME - elapsedTime);
        setTimeout(() => {
          if (requestCount === 0 && loadingInstance) {
            loadingInstance.close();
            loadingInstance = null;
          }
        }, delay);
      }
    }
  },

  // async axiosPatchAsync(url, data) {
  //   if (url.startsWith('/') && !url.startsWith(`${import.meta.env.VITE_API_ROOT_URL}/`)) {
  //     url = `${import.meta.env.VITE_API_ROOT_URL}${url}`;
  //   }

  //   return await axios.patch(url, data, {
  //     // headers: {
  //     //   'Corp-Code': uwTool.getCoId()
  //     // },
  //     withCredentials: true
  //   });
  // },

  async axiosPatchAsync(url, data) {
    if (url.startsWith('/') && !url.startsWith(`${import.meta.env.VITE_API_ROOT_URL}/`)) {
      url = `${import.meta.env.VITE_API_ROOT_URL}${url}`;
    }
    try {
      return await axios.patch(url, data, { withCredentials: true });
    } catch (error) {
      if (error.response?.data) {
        const responseData = error.response.data;

        // 包含 message & validationErrors
        throw {
          message: responseData.message || "發生未知錯誤",
          validationErrors: responseData.invalidatedPayload || null,
          status: error.response.status || 500
        };
      }
      throw error;
    }
  },

  async axiosDeleteAsync(url) {
    if (url.startsWith('/') && !url.startsWith(`${import.meta.env.VITE_API_ROOT_URL}/`)) {
      url = `${import.meta.env.VITE_API_ROOT_URL}${url}`;
    }
    return await axios.delete(url, {
      withCredentials: true
    })
      .then(res => {
        ElNotification({
          title: '成功',
          message: '已刪除',
          type: 'success',
        });
        return res;
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
        ElNotification({
          title: '錯誤',
          message: errorMessage,
          type: 'error',
        });
        return Promise.reject(error);
      });
  },

  sendRequest(newParameters) {
    var parameters = {
      url: "",
      method: "POST",
      postData: null,
      success: null,
      failure: null,
    };

    $.extend(parameters, newParameters);

    let data = null;
    if (parameters.method == "GET") {
      data = parameters.postData;
    } else {
      if (parameters.postData != null) {
        data = JSON.stringify(parameters.postData);
      }
    }

    return $.ajax({
      method: parameters.method,
      url: parameters.url,
      data: data,
      dataType: "json", // 保留 dataType: "json"
      success: parameters.success,
      error: parameters.failure
    });
  },

  getFullApiUrl(base, relativeUrl) {
    if (relativeUrl != null) {
      relativeUrl = String(relativeUrl);
    }

    var url = base;
    if (relativeUrl) {
      if (relativeUrl.startsWith("?")) {
        // 直接併, 不要加 /
      } else if (!relativeUrl.startsWith("/") && !url.endsWith("/")) {
        // 補一個 "/"
        url += "/";
      } else if (relativeUrl.startsWith("/") && url.endsWith("/")) {
        // 拿掉一個 "/"
        url = url.substring(0, url.length - 1);
      }
      url += relativeUrl;
    }

    if (url.includes("?")) {
      url += "&";
    } else {
      url += "?"
    }
    url += "tf=" + (new Date()).getTime();

    return url;
  }
}

export default defaultObj;

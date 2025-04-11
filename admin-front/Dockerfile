FROM node:18 AS builder
WORKDIR /app

# 複製依賴定義並安裝
COPY package*.json ./
RUN npm install

# 複製其餘檔案並 build
COPY . .
RUN npm run build

# ====== Production Stage ======
FROM nginx:alpine

# 複製 Vite 打包後的靜態檔案到 Nginx 目錄
COPY --from=builder /app/dist /usr/share/nginx/html

# 如果你有自定 Nginx 設定（例如處理 Vue router 的 history mode）
# 可以取消以下註解並放入你的 nginx.conf 檔
# COPY nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]
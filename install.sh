#!/bin/bash

set -e

echo "🔧 更新系統套件..."
sudo apt update -y
sudo apt upgrade -y

echo "🐙 安裝 Git..."
sudo apt install -y git

echo "🌐 安裝 Nginx..."
sudo apt install -y nginx

echo "🐳 安裝 Docker..."
sudo apt install -y docker.io
sudo systemctl enable docker
sudo systemctl start docker
sudo usermod -aG docker $USER

echo "📦 安裝 NVM..."
export NVM_DIR="$HOME/.nvm"
if [ ! -d "$NVM_DIR" ]; then
  curl -o- https://raw.githubusercontent.com/nvm-sh/nvm/v0.40.2/install.sh | bash
fi

# 使 nvm 立即可用
export NVM_DIR="$HOME/.nvm"
source "$NVM_DIR/nvm.sh"

echo "🟢 安裝 Node.js v18.16.1 via NVM..."
nvm install 18.16.1
nvm alias default 18.16.1

echo "🔐 安裝 Certbot 與 Nginx 插件..."
sudo apt install -y certbot python3-certbot-nginx

echo "✅ 所有工具安裝完成！請重新登入或執行以下命令啟用 Docker 權限："
echo "newgrp docker"

read -p "please type your domain? " domain
sudo sed "s/__DOMAIN__/${domain}/g" ./devops/cbct.conf > /etc/nginx/conf.d/${domain}.conf
echo "✅ 已複製並替換 domain，路徑：/etc/nginx/conf.d/${domain}.conf"
mkdir for-nginx

sudo nginx -s reload

nginx -v
node -v
docker --version
git --version
certbot --version
# Ubuntu 24.04.2 LTS 系統安裝指南

此指南適用於 Ubuntu 24.04.2 LTS，將自動安裝以下套件與工具：

- nginx
- nvm
- node.js 18.16.1（透過 nvm 安裝）
- docker
- git
- certbot
- python3-certbot-nginx

## Prerequisites

- check github permission `ssh -vT git@github.com`
- clone repo to local `sudo git clone git@github.com:uwinfo/CBCT.git /var/project`

## 安裝基本軟體

您可以直接執行附帶的 `sh ./install.sh` 腳本，
安裝後的檢查請輸入以下的指令來確認 :

```sh
nginx -v
node -v
docker --version
git --version
certbot --version
```

## 部署 app

執行部署的腳本 `sh ./deploy.sh`

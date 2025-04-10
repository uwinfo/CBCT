git checkout -f;
git fetch --all;
git pull origin staging;

docker build -t cbct-admin-api ./ --rm --no-cache

if [ "$(sudo docker ps -q -f name=cbct-admin-api)" ]; then
   sudo docker stop cbct-admin-api
fi
if [ "$(sudo docker container ls -f name=cbct-admin-api)" ]; then
   sudo docker rm cbct-admin-api
fi

docker run -d \
-e ASPNETCORE_URLS=http://\*:7000 \
-p 7000:7000 \
--name cbct-admin-api \
-v /var/project/CBCT/AdminApi/secrets.json:/app/secrets.json \
-v /var/project/CBCT/data:/data \
cbct-admin-api

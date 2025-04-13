git checkout -f;
git fetch --all;
git pull origin main;
echo "start deployment" > for-nginx/status.txt
docker build -t cbct-admin-api ./ --rm --no-cache
echo "build api complete" > for-nginx/status.txt
if [ "$(docker ps -q -f name=cbct-admin-api)" ]; then
   docker stop cbct-admin-api
fi
if [ "$(docker container ls -f name=cbct-admin-api)" ]; then
   docker rm cbct-admin-api
fi

docker run -d \
-e ASPNETCORE_URLS=http://\*:7000 \
-p 7000:7000 \
--name cbct-admin-api \
-v /var/project/cbct/AdminApi/secrets.json:/app/secrets.json \
-v /var/project/cbct/data:/data \
cbct-admin-api
echo "run api container complete" > for-nginx/status.txt

cd admin-front && docker build -t cbct-admin-f2e ./ --rm --no-cache
echo "build f2e complete" > ../for-nginx/status.txt
if [ "$(docker ps -q -f name=cbct-admin-f2e)" ]; then
   docker stop cbct-admin-f2e
fi
if [ "$(docker container ls -f name=cbct-admin-f2e)" ]; then
   docker rm cbct-admin-f2e
fi

docker run -d \
-p 8080:80 \
--name cbct-admin-f2e \
-v /var/project/cbct/data:/data \
cbct-admin-f2e
echo "run f2e container complete" > ../for-nginx/status.txt
cd ../
git rev-parse HEAD > for-nginx/version.txt
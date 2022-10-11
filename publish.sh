docker build --target zserver -f Dockerfile -t zlzforever/zserver .
docker push zlzforever/zserver

docker build --target zserver-api -f API.Dockerfile -t zlzforever/zserver-api .
docker push zlzforever/zserver-api

docker buildx build --platform linux/arm64 --target zserver-api -f API_ARM.Dockerfile -t zlzforever/zserver-api:arm64-20221011 .
docker push zlzforever/zserver-api:arm64
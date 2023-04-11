docker build --target zserver -f Dockerfile -t zlzforever/zserver .
docker push zlzforever/zserver

docker buildx build --platform linux/arm64 --target zserver-api -f API.Dockerfile -t zlzforever/zserver-api:20230410 .
docker push zlzforever/zserver-api:20230410

docker build --target zserver-api -f API.Dockerfile -t zlzforever/zserver-api:20230410 .
docker push zlzforever/zserver-api:20230410

docker buildx build --platform linux/arm64 --target zserver-api -f API_ARM.Dockerfile -t zlzforever/zserver-api:arm64-20221211 .
docker push zlzforever/zserver-api:arm64-20221211
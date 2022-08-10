docker buildx build --platform linux/amd64 --target zserver -f Dockerfile -t zlzforever/zserver .
docker push zlzforever/zserver
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS api-builder
WORKDIR /app
COPY . .
RUN cd src/ZServer.API && dotnet publish -c Release -o out
RUN rm -rf src/ZServer.API/out/nacos.json
RUN rm -rf src/ZServer.API/out/zserver.json
RUN rm -rf src/ZServer.API/out/runtimes/linux-arm
RUN rm -rf src/ZServer.API/out/runtimes/linux-arm64
RUN rm -rf src/ZServer.API/out/runtimes/linux-musl-x64
RUN rm -rf src/ZServer.API/out/runtimes/osx
RUN rm -rf src/ZServer.API/out/runtimes/osx-x64
RUN rm -rf src/ZServer.API/out/runtimes/win-arm64
RUN rm -rf src/ZServer.API/out/runtimes/win-x64
RUN rm -rf src/ZServer.API/out/runtimes/win-x86
RUN rm -rf src/ZServer.API/out/shapes
RUN rm -rf src/ZServer.API/out/Fonts

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS zserver-api
WORKDIR /app
EXPOSE 8200
ENV LANG zh_CN.UTF-8
RUN mkdir /app/shapes && mkdir /app/Fonts
RUN echo "deb https://mirrors.aliyun.com/debian/ bookworm main non-free non-free-firmware contrib" > /etc/apt/sources.list && \
    echo "deb-src https://mirrors.aliyun.com/debian/ bookworm main non-free non-free-firmware contrib" >> /etc/apt/sources.list && \
    echo "deb https://mirrors.aliyun.com/debian-security/ bookworm-security main" >> /etc/apt/sources.list && \
    echo "deb-src https://mirrors.aliyun.com/debian-security/ bookworm-security main" >> /etc/apt/sources.list
RUN apt-get update &&\
    apt-get install -y fontconfig && apt-get clean
COPY docker-entrypoint.sh /usr/local/bin/
RUN chmod +x /usr/local/bin/docker-entrypoint.sh
ADD ./src/ZServer.API/fonts/* /usr/share/fonts/truetype/deng/
COPY --from=api-builder /app/src/ZServer.API/out .
ENTRYPOINT ["docker-entrypoint.sh"]
CMD ["dotnet", "zapi.dll"]


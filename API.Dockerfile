FROM mcr.microsoft.com/dotnet/sdk:10.0 AS api-builder
WORKDIR /app
COPY . .
RUN cd src/ZServer.API && dotnet publish -c Release -o out
RUN rm -rf src/ZServer.API/out/nacos.json && \
    rm -rf src/ZServer.API/out/zserver.json &&  \
    rm -rf src/ZServer.API/out/runtimes/linux-arm &&  \
    rm -rf src/ZServer.API/out/runtimes/linux-arm64 &&  \
    rm -rf src/ZServer.API/out/runtimes/linux-musl-x64  && \
    rm -rf src/ZServer.API/out/runtimes/osx &&  \
    rm -rf src/ZServer.API/out/runtimes/osx-x64 &&  \
    rm -rf src/ZServer.API/out/runtimes/win-arm64 &&  \
    rm -rf src/ZServer.API/out/runtimes/win-x64 &&  \
    rm -rf src/ZServer.API/out/runtimes/win-x86 &&  \
    rm -rf src/ZServer.API/out/shapes &&  \
    rm -rf src/ZServer.API/out/fonts

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS zserver-api
WORKDIR /app
EXPOSE 8200
ENV LANG zh_CN.UTF-8
RUN mkdir /app/shapes && mkdir /app/Fonts
#RUN echo "deb https://mirrors.aliyun.com/debian/ noble main non-free non-free-firmware contrib" > /etc/apt/sources.list && \
#    echo "deb-src https://mirrors.aliyun.com/debian/ noble main non-free non-free-firmware contrib" >> /etc/apt/sources.list && \
#    echo "deb https://mirrors.aliyun.com/debian-security/ noble-security main" >> /etc/apt/sources.list && \
#    echo "deb-src https://mirrors.aliyun.com/debian-security/ noble-security main" >> /etc/apt/sources.list
RUN apt-get update &&\
    apt-get install -y fontconfig iputils-ping net-tools curl && apt-get clean
ADD ./src/ZServer.API/fonts/* /usr/share/fonts/truetype/deng/    
COPY docker-entrypoint.sh /usr/local/bin/
RUN chmod +x /usr/local/bin/docker-entrypoint.sh
COPY --from=api-builder /app/src/ZServer.API/out .
ENTRYPOINT ["docker-entrypoint.sh"]
CMD ["dotnet", "zapi.dll"]


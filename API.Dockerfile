FROM mcr.microsoft.com/dotnet/sdk:7.0 AS api-builder
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

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS zserver-api
WORKDIR /app
RUN sed -i "s@http://deb.debian.org@http://mirrors.aliyun.com@g" /etc/apt/sources.list 
RUN apt-get update -y && apt-get install -y libgdiplus locales fontconfig && apt-get clean && ln -s /usr/lib/libgdiplus.so /usr/lib/gdiplus.dll
RUN sed -ie 's/# zh_CN.UTF-8 UTF-8/zh_CN.UTF-8 UTF-8/g' /etc/locale.gen && locale-gen && mkdir /usr/share/fonts/truetype/deng/
ADD ./src/ZServer.API/fonts/* /usr/share/fonts/truetype/deng/
RUN fc-cache -vf && fc-list
ENV LANG zh_CN.UTF-8
COPY --from=api-builder /app/src/ZServer.API/out .
RUN mkdir /app/shapes && mkdir /app/Fonts
ENTRYPOINT ["dotnet", "zapi.dll"]


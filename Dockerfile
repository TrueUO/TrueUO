FROM mcr.microsoft.com/dotnet/runtime:10.0

WORKDIR /trueuo
RUN apt-get update && apt-get install -y zlib1g

COPY . .

EXPOSE 2593

# Environment variables
ENV DOTNET_GC_SERVER=1 \
    DOTNET_USE_POLLING_FILE_WATCHER=1 \
    LC_ALL=en_US.UTF-8 \
   DataPath.CustomPath=/trueuo/uo_data \
   Containered=1 \
   Server.Relay=127.0.0.1

ENTRYPOINT ["dotnet", "TrueUO.dll"]
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS publish
WORKDIR /src
COPY ["backend.csproj", "./"]
RUN dotnet restore "backend.csproj"
COPY . .
RUN dotnet publish "backend.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM publish AS dev
RUN dotnet tool install --global dotnet-ef
ENV PATH="$PATH:/root/.dotnet/tools"
RUN apt-get update \
    && apt-get install -y --no-install-recommends gosu \
    && rm -rf /var/lib/apt/lists/*
# COPY docker-entrypoint.dev.sh /usr/local/bin/
# RUN chmod +x /usr/local/bin/docker-entrypoint.dev.sh
# ENTRYPOINT ["docker-entrypoint.dev.sh"]

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
RUN useradd -m appuser
USER appuser
ENTRYPOINT ["dotnet", "backend.dll"]
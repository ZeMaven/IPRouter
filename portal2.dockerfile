FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base

# Install NGINX
RUN apt-get update && \
    apt-get install -y nginx && \
    apt-get clean && \
    rm -rf /var/lib/apt/lists/* /tmp/* /var/tmp/*

# Configure NGINX
COPY MomoSwitchPortal/nginx.conf /etc/nginx/nginx.conf

WORKDIR /app

# NGINX will listen on 80, your application on 8080
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["MomoSwitchPortal/MomoSwitchPortal.csproj", "MomoSwitchPortal/"]
RUN dotnet restore "./MomoSwitchPortal/MomoSwitchPortal.csproj"
COPY . .
WORKDIR "/src/MomoSwitchPortal"
RUN dotnet build "./MomoSwitchPortal.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./MomoSwitchPortal.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
COPY --from=publish /app/publish .
COPY MomoSwitchPortal/start.sh /start.sh
RUN chmod +x /start.sh
ENTRYPOINT ["/start.sh"]
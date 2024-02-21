# Stage 1: Build the application
FROM mcr.microsoft.com/dotnet/framework/sdk:4.8 AS build
WORKDIR /src

# Restore NuGet packages
COPY *.sln .
COPY NipInwardProxy/NipInwardProxy.csproj NipInwardProxy/
RUN nuget restore

# Copy project files and build the application
COPY NipInwardProxy/ NipInwardProxy/
WORKDIR /src/NipInwardProxy
RUN msbuild /p:Configuration=Release /p:OutputPath=/app/out

# Stage 2: Runtime image
FROM mcr.microsoft.com/dotnet/framework/aspnet:4.8 AS runtime
WORKDIR /inetpub/wwwroot

# Remove default web application
RUN powershell -Command "Remove-Item -Recurse C:\inetpub\wwwroot\*"

# Copy the build output to wwwroot
COPY --from=build /app/out .

# Configure IIS to host the WCF service
# Note: Additional IIS configuration might be required depending on your application's needs

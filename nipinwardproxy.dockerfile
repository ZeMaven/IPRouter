# Stage 1: Build the application
FROM mcr.microsoft.com/dotnet/framework/sdk:4.8 AS build
WORKDIR /src

# Copy solution and project files and restore NuGet packages
COPY *.sln .
COPY NipInwardProxy/*.csproj NipInwardProxy/
COPY NipInwardProxy/packages.config NipInwardProxy/
RUN nuget restore NipInwardProxy/packages.config -SolutionDirectory ../

# Copy the rest of the source code and build the project
COPY NipInwardProxy/ NipInwardProxy/
WORKDIR /src/NipInwardProxy
RUN msbuild /p:Configuration=Release /p:OutputPath=/app/out

# Stage 2: Runtime image
FROM mcr.microsoft.com/dotnet/framework/aspnet:4.8 AS runtime
WORKDIR /inetpub/wwwroot

# Clean default web app
RUN powershell -Command "Remove-Item -Recurse C:\inetpub\wwwroot\*"

# Copy the build output
COPY --from=build /app/out .

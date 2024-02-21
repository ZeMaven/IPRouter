# Use an official Microsoft Windows Server Core image.
# https://hub.docker.com/_/microsoft-windowsservercore
FROM mcr.microsoft.com/windows/servercore:ltsc2019

# Set the working directory in the container to /app.
WORKDIR /app

# Copy the current directory contents into the container at /app.
COPY . .

# Install the .NET Framework 4.8 runtime.
RUN Invoke-WebRequest -Uri 'https://download.visualstudio.microsoft.com/download/pr/3a6f62ee-987b-40b6-a5f4-869e56d5a697/9d98e0d2f0b0e306f678b2d08f8echki/dotnetfx_48_full_x86_x64.exe' -OutFile 'dotnetfx_48_full_x86_x64.exe' && \
    Start-Process -FilePath 'dotnetfx_48_full_x86_x64.exe' -Wait -NoNewWindow

# Install NuGet packages.
RUN Install-PackageProvider -Name NuGet -MinimumVersion 2.8.5.201 -Force
RUN nuget restore

# Compile the project.
RUN msbuild /t:Build /p:Configuration=Release

# Set the entry point for the container.
ENTRYPOINT ["cmd.exe", "/k", "cscript.exe //T:30 //NoLogo NipInward.svc /r:bin/Release"]
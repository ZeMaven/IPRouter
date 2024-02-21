# Use a Windows Server Core image with .NET Framework 4.8 as the base
FROM mcr.microsoft.com/dotnet/framework/aspnet:4.8-windowsservercore-ltsc2019 as builder

# Set the working directory inside the container
WORKDIR /inetpub/wwwroot

# Copy the project files into the container
COPY . .

# Assuming you might have a build script or need to restore packages,
# you can run MSBuild commands here. For simplicity, this step is omitted,
# but you would typically restore NuGet packages and build your solution here.

# Now, using a runtime image for the actual deployment
FROM mcr.microsoft.com/dotnet/framework/aspnet:4.8-windowsservercore-ltsc2019

# Copy the built application from the builder stage to the runtime container
WORKDIR /inetpub/wwwroot
COPY --from=builder /inetpub/wwwroot .

# Configure additional environment variables, IIS settings, or install additional components here

# Inform Docker that the container listens on the specified port at runtime.
EXPOSE 80

# The ENTRYPOINT or CMD to run your application can be customized as needed.
# For IIS-based applications, there is no need to define an ENTRYPOINT or CMD
# as IIS is already configured to start and host your application.

# Use the official .NET SDK image for .NET 8.0 to build the project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS buildgit
WORKDIR /src

# Copy csproj and restore any dependencies (via NuGet)
COPY ["blog-website-api.csproj", "./"]
RUN dotnet restore "blog-website-api.csproj"

# Copy the rest of your project's files into the Docker image
COPY . .

# Build your application in the Release configuration
RUN dotnet build "blog-website-api.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "blog-website-api.csproj" -c Release -o /app/publish

# Use the official .NET runtime image for .NET 8.0 to run the application
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "blog-website-api.dll"]

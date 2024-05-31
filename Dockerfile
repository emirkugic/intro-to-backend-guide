FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

COPY ["blog-website-api.csproj", "./"]
RUN dotnet restore "blog-website-api.csproj"

COPY . .

RUN dotnet build "blog-website-api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "blog-website-api.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "blog-website-api.dll"]

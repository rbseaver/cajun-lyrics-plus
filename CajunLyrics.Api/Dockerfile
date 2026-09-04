FROM node:22-alpine AS frontend-build
WORKDIR /src/CajunLyrics.Web
COPY CajunLyrics.Web/package*.json ./
RUN npm ci
COPY CajunLyrics.Web .
RUN npm run build

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["CajunLyrics.Api/CajunLyrics.Api.csproj", "CajunLyrics.Api/"]
COPY ["CajunLyrics.Lib/CajunLyrics.Lib.csproj", "CajunLyrics.Lib/"]
RUN dotnet restore "./CajunLyrics.Api/CajunLyrics.Api.csproj"
COPY . .
COPY --from=frontend-build /src/CajunLyrics.Web/dist /src/CajunLyrics.Api/wwwroot
WORKDIR "/src/CajunLyrics.Api"
RUN dotnet publish "./CajunLyrics.Api.csproj" -c ${BUILD_CONFIGURATION} -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
ENV ASPNETCORE_ENVIRONMENT=Docker
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "CajunLyrics.Api.dll"]
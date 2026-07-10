FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["MediTrack.IdentityService.API/MediTrack.IdentityService.API.csproj", "MediTrack.IdentityService.API/"]
RUN dotnet restore "MediTrack.IdentityService.API/MediTrack.IdentityService.API.csproj"
COPY . .
WORKDIR "/src/MediTrack.IdentityService.API"
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MediTrack.IdentityService.API.dll"]

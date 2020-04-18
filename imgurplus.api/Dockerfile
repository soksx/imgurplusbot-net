#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["imgurplus.api/imgurplus.api.csproj", "imgurplus.api/"]
COPY ["imgurplusbot.bll/imgurplusbot.bll.csproj", "imgurplusbot.bll/"]
COPY ["imgurplusbot.dal/imgurplusbot.dal.csproj", "imgurplusbot.dal/"]
COPY ["Imgur.API/src/Imgur.API/Imgur.API.csproj", "Imgur.API/src/Imgur.API/"]
RUN dotnet restore "imgurplus.api/imgurplus.api.csproj"
COPY . .
WORKDIR "/src/imgurplus.api"
RUN dotnet build "imgurplus.api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "imgurplus.api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
RUN touch imgurplus.db
ENTRYPOINT ["dotnet", "imgurplus.api.dll"]

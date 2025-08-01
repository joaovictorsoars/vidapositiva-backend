﻿FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine3.22 AS build
WORKDIR /src

COPY . ./

RUN dotnet restore
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine3.22 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "VidaPositiva.Api.dll"]
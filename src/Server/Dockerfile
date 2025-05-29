FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /source
COPY . .
RUN dotnet publish -c release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app ./

ENV ASPNETCORE_URLS=http://+:8080
ENV DOTNET_ENVIRONMENT=production
EXPOSE 8080
ENTRYPOINT ["dotnet", "GoodFriend.Server.dll"]

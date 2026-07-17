FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY . .
RUN dotnet restore FrotaGo.sln
RUN dotnet publish src/FrotaGo.Api/FrotaGo.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:5073
EXPOSE 5073

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "FrotaGo.Api.dll"]

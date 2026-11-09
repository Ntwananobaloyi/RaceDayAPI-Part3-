DOCKERFILE

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["RaceDayMVC.csproj", "./"]
RUN dotnet restore "RaceDayMVC.csproj"
COPY . .
RUN dotnet publish "RaceDayMVC.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 80
ENTRYPOINT ["dotnet", "RaceDayMVC.dll"]

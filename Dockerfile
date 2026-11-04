FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY EcoCiclo.csproj .
RUN dotnet restore
COPY . .
RUN dotnet publish EcoCiclo.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .
RUN mkdir -p /app/database && chown -R $APP_UID /app/database
USER $APP_UID
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080
ENTRYPOINT ["dotnet", "EcoCiclo.dll"]

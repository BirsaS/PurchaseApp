FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY PurchaseApp.csproj ./
RUN dotnet restore
COPY . ./
RUN dotnet publish PurchaseApp.csproj -c Release --no-restore -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish ./
ENV ASPNETCORE_HTTP_PORTS=10000
EXPOSE 10000
ENTRYPOINT ["dotnet", "PurchaseApp.dll"]

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

WORKDIR /src

COPY ["src/MultiTenantEmployeeApi.Api/MultiTenantEmployeeApi.Api.csproj", "src/MultiTenantEmployeeApi.Api/"]

RUN dotnet restore "src/MultiTenantEmployeeApi.Api/MultiTenantEmployeeApi.Api.csproj"

COPY . .

WORKDIR "/src/src/MultiTenantEmployeeApi.Api"

RUN dotnet publish "MultiTenantEmployeeApi.Api.csproj" \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final

WORKDIR /app

EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "MultiTenantEmployeeApi.Api.dll"]
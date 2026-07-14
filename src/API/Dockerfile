# ---- Build stage ----
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/API/ProductApi.API.csproj", "src/API/"]
COPY ["src/Application/ProductApi.Application.csproj", "src/Application/"]
COPY ["src/Domain/ProductApi.Domain.csproj", "src/Domain/"]
COPY ["src/Infrastructure/ProductApi.Infrastructure.csproj", "src/Infrastructure/"]
RUN dotnet restore "src/API/ProductApi.API.csproj"

COPY . .
WORKDIR /src/src/API
RUN dotnet build "ProductApi.API.csproj" -c Release -o /app/build

# ---- Publish stage ----
FROM build AS publish
RUN dotnet publish "ProductApi.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ---- Runtime stage ----
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

RUN addgroup --system appgroup && adduser --system --ingroup appgroup appuser
USER appuser

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ProductApi.API.dll"]

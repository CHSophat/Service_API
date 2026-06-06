# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["src/ApartmentManagementSystem.API/ApartmentManagementSystem.API.csproj", "src/ApartmentManagementSystem.API/"]
RUN dotnet restore "src/ApartmentManagementSystem.API/ApartmentManagementSystem.API.csproj"

# Copy the rest of the source code
COPY . .

# Publish the application
WORKDIR /src/ApartmentManagementSystem.API
RUN dotnet publish "ApartmentManagementSystem.API.csproj" -c Release -o /app/publish /p:UseAppHost=false /p:WarningsNotAsErrors=true

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 80
ENTRYPOINT ["dotnet", "ApartmentManagementSystem.API.dll"]

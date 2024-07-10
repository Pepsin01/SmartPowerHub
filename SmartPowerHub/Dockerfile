#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy the solution file and the necessary project files
COPY SmartPowerHub.sln .
COPY SmartPowerHub/SmartPowerHub.csproj SmartPowerHub/
COPY IoTControllerContracts/IoTControllerContracts.csproj IoTControllerContracts/

# Restore dependencies for the main project
RUN dotnet restore "SmartPowerHub/SmartPowerHub.csproj"

# Copy the remaining source code
COPY . .

# Build the project
WORKDIR /src/SmartPowerHub
RUN dotnet build -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /src/SmartPowerHub/IoTControllers ./IoTControllers
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SmartPowerHub.dll"]

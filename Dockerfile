# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["src/ParthBE/ParthBE.csproj", "ParthBE/"]
RUN dotnet restore "ParthBE/ParthBE.csproj"

# Copy source code and build
COPY src/ParthBE/ ParthBE/
WORKDIR "/src/ParthBE"
RUN dotnet build "ParthBE.csproj" -c Release -o /app/build

# Stage 2: Publish
FROM build AS publish
RUN dotnet publish "ParthBE.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Expose port 5000 for HTTP
EXPOSE 5000

# Copy published app
COPY --from=publish /app/publish .

# Set environment variables
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:5000

# Run the application
ENTRYPOINT ["dotnet", "ParthBE.dll"]

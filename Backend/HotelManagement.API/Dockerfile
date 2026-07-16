# ─── Build Stage ───
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files for dependency caching
COPY ["Backend/HotelManagement.API/HotelManagement.API.csproj", "Backend/HotelManagement.API/"]
COPY ["Backend/HotelManagement.BLL/HotelManagement.BLL.csproj", "Backend/HotelManagement.BLL/"]
COPY ["Backend/HotelManagement.DAL/HotelManagement.DAL.csproj", "Backend/HotelManagement.DAL/"]
COPY ["Backend/HotelManagement.Repository/HotelManagement.Repository.csproj", "Backend/HotelManagement.Repository/"]

RUN dotnet restore "Backend/HotelManagement.API/HotelManagement.API.csproj"

# Copy all source and publish
COPY . .
WORKDIR "/src/Backend/HotelManagement.API"
RUN dotnet publish -c Release -o /app/publish

# ─── Runtime Stage ───
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080

# Copy published artifacts
COPY --from=build /app/publish .

# Entrypoint
ENTRYPOINT ["dotnet", "HotelManagement.API.dll"]
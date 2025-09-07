# Use the official .NET image as a build environment

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
# appsettings.json is excluded from build context using .dockerignore
COPY ["DiscordBGGCollection/DiscordBGGCollection.csproj", "DiscordBGGCollection/"]
COPY ["TestCases/TestCases.csproj", "TestCases/"]
COPY . .
RUN dotnet restore "DiscordBGGCollection/DiscordBGGCollection.csproj"
RUN dotnet build "DiscordBGGCollection/DiscordBGGCollection.csproj" -c Release -o /app/build
RUN dotnet publish "DiscordBGGCollection/DiscordBGGCollection.csproj" -c Release -o /app/publish

# Use the official .NET runtime image for the final container
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
# appsettings.json will be mounted at runtime from the host system
ENTRYPOINT ["dotnet", "DiscordBGGCollection.dll"]

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 5144
ENV ASPNETCORE_URLS=http://+:5144

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["./", "./"]
RUN dotnet restore "auth-dotnet-api.csproj"
RUN dotnet build "auth-dotnet-api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "auth-dotnet-api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Set the environment PROD
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "auth-dotnet-api.dll"]
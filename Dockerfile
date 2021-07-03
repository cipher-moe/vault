FROM mcr.microsoft.com/dotnet/aspnet:5.0.7-alpine3.13-amd64 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0.301-alpine3.13-amd64 AS build
WORKDIR /src
COPY ["vault.csproj", "./"]
RUN dotnet restore "vault.csproj"
COPY . .
WORKDIR "/src/"
RUN dotnet build "vault.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "vault.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "vault.dll"]

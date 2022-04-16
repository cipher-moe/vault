FROM mcr.microsoft.com/dotnet/aspnet:6.0.4-alpine3.15 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0.202-alpine3.15 AS build
WORKDIR /src
COPY ["vault.csproj", "./"]
RUN dotnet restore "vault.csproj"
COPY . .
WORKDIR "/src/"
RUN apk add --no-cache npm
RUN npm install --global yarn
RUN yarn install --frozen-lockfile
RUN dotnet build "vault.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "vault.csproj" -c Release -o /app/publish

FROM base AS final
ARG PORT
ENV PORT=$PORT
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "vault.dll"]

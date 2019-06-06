FROM mcr.microsoft.com/dotnet/core/aspnet:2.1-stretch-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:2.1-stretch AS build
WORKDIR /src
COPY ["Blogroll.Web/Blogroll.Web.csproj", "Blogroll.Web/"]
COPY ["Blogroll.Common/Blogroll.Common.csproj", "Blogroll.Common/"]
COPY ["Blogroll.Persistence.SQLite/Blogroll.Persistence.SQLite.csproj", "Blogroll.Persistence.SQLite/"]
COPY ["Blogroll.Web.Common/Blogroll.Web.Common.csproj", "Blogroll.Web.Common/"]
COPY ["Blogroll.Persistence.LiteDB/Blogroll.Persistence.LiteDB.csproj", "Blogroll.Persistence.LiteDB/"]
RUN dotnet restore "Blogroll.Web/Blogroll.Web.csproj"
COPY . .
WORKDIR "/src/Blogroll.Web"
RUN dotnet build "Blogroll.Web.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "Blogroll.Web.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
RUN mkdir -p Data/LiteDb
RUN mkdir -p Data/SQLite
ENTRYPOINT ["dotnet", "Blogroll.Web.dll"]
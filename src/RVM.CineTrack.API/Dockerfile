FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS base
WORKDIR /app
EXPOSE 8080
RUN apk add --no-cache icu-libs
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /src
COPY ["src/RVM.CineTrack.API/RVM.CineTrack.API.csproj", "src/RVM.CineTrack.API/"]
COPY ["src/RVM.CineTrack.Domain/RVM.CineTrack.Domain.csproj", "src/RVM.CineTrack.Domain/"]
COPY ["src/RVM.CineTrack.Infrastructure/RVM.CineTrack.Infrastructure.csproj", "src/RVM.CineTrack.Infrastructure/"]
RUN dotnet restore "./src/RVM.CineTrack.API/RVM.CineTrack.API.csproj"
COPY . .
WORKDIR "/src/src/RVM.CineTrack.API"
RUN dotnet publish "./RVM.CineTrack.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "RVM.CineTrack.API.dll"]

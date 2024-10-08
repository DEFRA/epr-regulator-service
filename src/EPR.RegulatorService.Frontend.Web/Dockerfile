FROM defradigital/dotnetcore-development:dotnet8.0 AS build-env
USER root

# Expose the app on a defined port, configurable via a build argument
ARG PORT=3000
ENV ASPNETCORE_URLS=http://*:${PORT}
EXPOSE ${PORT}

# Copy everything
COPY EPR.RegulatorService.Frontend.Web/*.csproj ./EPR.RegulatorService.Frontend.Web/
COPY EPR.RegulatorService.Frontend.Core/*.csproj ./EPR.RegulatorService.Frontend.Core/
# Move up 2 directorys as Nuget.config is located in src
COPY NuGet.Config ./NuGet.Config

# Use Build Argument to insert PAT into Nuget file
ARG PAT=localhost
RUN sed -i "s|</configuration>|<packageSourceCredentials><epr-packaging-common><add key=\"Username\" value=\"PAT\" /><add key=\"ClearTextPassword\" value=\"${PAT}\" /></epr-packaging-common></packageSourceCredentials></configuration>|" NuGet.Config

RUN find . -name "*.csproj" -type f -print

# Restore as distinct layers
RUN dotnet restore "./EPR.RegulatorService.Frontend.Web/EPR.RegulatorService.Frontend.Web.csproj" --configfile "NuGet.Config"

# Install NPM
USER root
RUN apk update && apk add --no-cache npm

ENV NODE_EXTRA_CA_CERTS=/usr/local/share/ca-certificates/internal-ca.crt

# Build and publish a release
COPY EPR.RegulatorService.Frontend.Web/. ./EPR.RegulatorService.Frontend.Web/.
COPY EPR.RegulatorService.Frontend.Core/. ./EPR.RegulatorService.Frontend.Core/.
WORKDIR /home/dotnet/EPR.RegulatorService.Frontend.Web

RUN dotnet publish -c Release -o out

# Build runtime image
FROM defradigital/dotnetcore:dotnet8.0
USER dotnet
COPY --from=build-env /home/dotnet/EPR.RegulatorService.Frontend.Web/out .

# Add internationalisation support
USER root
RUN apk add --no-cache icu-libs icu-data-full libintl tzdata
USER dotnet

ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=0

# Expose the app on a defined port, configurable via a build argument
ARG PORT=3000
ENV ASPNETCORE_URLS=http://*:${PORT}
EXPOSE ${PORT}

CMD dotnet EPR.RegulatorService.Frontend.Web.dll

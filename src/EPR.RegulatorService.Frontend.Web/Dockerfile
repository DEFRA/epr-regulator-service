FROM defradigital/dotnetcore-development:dotnet8.0 AS build-env
USER root

# Expose the app on a defined port, configurable via a build argument
ARG PORT=3000
ENV ASPNETCORE_URLS=http://*:${PORT}
EXPOSE ${PORT}

# Copy csproj files for restore
COPY EPR.RegulatorService.Frontend.Web/*.csproj ./EPR.RegulatorService.Frontend.Web/
COPY EPR.RegulatorService.Frontend.Core/*.csproj ./EPR.RegulatorService.Frontend.Core/
COPY git-subtree-epr-common/src/EPR.Common/EPR.Common.Authorization/*.csproj ./git-subtree-epr-common/src/EPR.Common/EPR.Common.Authorization/

# Restore as distinct layers
RUN dotnet restore "./EPR.RegulatorService.Frontend.Web/EPR.RegulatorService.Frontend.Web.csproj"

# Install NPM
USER root
RUN apk update && apk add --no-cache npm

ENV NODE_EXTRA_CA_CERTS=/usr/local/share/ca-certificates/internal-ca.crt

# Build and publish a release
COPY EPR.RegulatorService.Frontend.Web/. ./EPR.RegulatorService.Frontend.Web/.
COPY EPR.RegulatorService.Frontend.Core/. ./EPR.RegulatorService.Frontend.Core/.
COPY git-subtree-epr-common/. ./git-subtree-epr-common/.
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

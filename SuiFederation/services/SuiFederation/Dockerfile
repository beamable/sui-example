# use the dotnet sdk as a build stage
FROM mcr.microsoft.com/dotnet/sdk:7.0-alpine as build-env
RUN apk add --no-cache nodejs npm
# <BEAM-CLI-COPY-ENV> this line signals the start of environment variables copies into the built container. Do not remove it. This will be overwritten every time a variable changes in the execution of the CLI.
ENV BEAM_CSPROJ_PATH="/subsrc/services/SuiFederation/SuiFederation.csproj"
# </BEAM-CLI-COPY-ENV> this line signals the end of environment variables copies into the built container. Do not remove it.

# <BEAM-CLI-COPY-SRC> this line signals the start of Beamable Project Src copies into the built container. Do not remove it. The content between here and the closing tag will change anytime the Beam CLI modifies dependencies.
RUN mkdir -p /subsrc/services/SuiFederationStorage
COPY services/SuiFederationStorage /subsrc/services/SuiFederationStorage

RUN mkdir -p /subsrc/services/SuiFederationCommon
COPY services/SuiFederationCommon /subsrc/services/SuiFederationCommon

RUN mkdir -p /subsrc/services/SuiFederation
COPY services/SuiFederation /subsrc/services/SuiFederation
# </BEAM-CLI-COPY-SRC> this line signals the end of Beamable Project Src copies. Do not remove it.

# build the dotnet program
WORKDIR /

RUN dotnet publish ${BEAM_CSPROJ_PATH} -c release -o /beamApp

COPY SuiFederation/js/. /beamApp/js

# use the dotnet runtime as the final stage
FROM mcr.microsoft.com/dotnet/runtime:7.0-alpine
RUN apk add --no-cache nodejs npm
WORKDIR /beamApp

# expose the health port
EXPOSE 6565 

# copy the built program
COPY --from=build-env /beamApp .

# when starting the container, run dotnet with the built dll
ENTRYPOINT ["dotnet", "/beamApp/SuiFederation.dll"]

# Swap entrypoints if the container is exploding and you want to keep it alive indefinitely so you can go look into it.
#ENTRYPOINT ["tail", "-f", "/dev/null"]
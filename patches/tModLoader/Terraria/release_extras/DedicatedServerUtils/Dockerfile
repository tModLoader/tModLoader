FROM sonroyaalmerol/steamcmd-arm64:root-trixie AS arm64-base
FROM cm2network/steamcmd:root-trixie AS amd64-base

FROM ${BUILDARCH}-base
ARG DEBIAN_FRONTEND=noninteractive
# libicu is dotnet dependency
# unzip is "./manage-tModLoaderServer.sh install-tml --github" dependency
RUN apt-get update && \
    libicu_pkgname=$(apt-cache show 'libicu[0-9]*' | awk -F: '/Package: /{print $2}') && \
    apt-get install -y --no-install-recommends ${libicu_pkgname} unzip && rm -rf /var/lib/apt/lists/*

ENV UMASK=0002

# Set a specific tModLoader version, defaults to the latest Github release
ARG TMLVERSION
ARG UID=1000
ARG GID=1000

RUN usermod -u $UID steam && groupmod -g $GID steam && usermod -g $GID steam && chown -R $UID:$GID /home/steam
USER steam
ENV HOME=/home/steam
ENV PATH="$PATH:$HOME/.bin"
WORKDIR $HOME

# Create an easy to use steamcmd executable
COPY --chown=steam:steam --chmod=0755 <<EOF ./.bin/steamcmd
#\!/bin/bash
exec ~/steamcmd/steamcmd.sh "\$@"
EOF

# Update SteamCMD and verify latest version
RUN steamcmd +quit

# To make local edits to the management script, change this URL to a local path for manage-tModLoaderServer.sh
COPY --chown=steam:steam --chmod=0755 manage-tModLoaderServer.sh .
#ADD --chown=steam:steam --chmod=0755 https://raw.githubusercontent.com/tModLoader/tModLoader/1.4.5/patches/tModLoader/Terraria/release_extras/DedicatedServerUtils/manage-tModLoaderServer.sh .


# Make management script executable and manually add the logs directory to fix the "Permission Denied" error on most systems.
# Adding write permissions for the logs is necessary because when Docker mounts a volume that doesn't exist on the host it mounts both host and container as root
RUN ISDOCKER=1 ./manage-tModLoaderServer.sh install-tml --github

EXPOSE 7777

ENTRYPOINT [ "./manage-tModLoaderServer.sh", "start", "--folder", "/tModLoader" ]

FROM ubuntu:22.04 as builder

ARG DEBIAN_FRONTEND=noninteractive
RUN dpkg --add-architecture i386 \
 && apt-get update -y \
 && apt-get install -y --no-install-recommends libc6:i386 \
 && rm -rf /var/lib/apt/lists/*

FROM ubuntu:22.04
ARG DEBIAN_FRONTEND=noninteractive
RUN dpkg --add-architecture i386 \
    && apt-get update -y \
    && apt-get install -y --no-install-recommends \
        bash curl nano file libgcc-s1 libstdc++6 \
        libicu70 ca-certificates \
    && rm -rf /var/lib/apt/lists/*

# Copy i386 libs from builder (for steamcmd)
COPY --from=builder \
    /lib/i386-linux-gnu/ld-linux.so.2 \
    /lib/i386-linux-gnu/libc.so.6 \
    /lib/i386-linux-gnu/libdl.so.2 \
    /lib/i386-linux-gnu/libm.so.6 \
    /lib/i386-linux-gnu/libpthread.so.0 \
    /lib/i386-linux-gnu/librt.so.1 \
    /lib/i386-linux-gnu/

RUN mkdir -p /lib/i386-linux-gnu \
    && ln -sf /lib/i386-linux-gnu/ld-linux.so.2 /lib/ld-linux.so.2

# TODO: Currently it is too arduous to allow UID/GID to be set at runtime via ENV variables for a few reasons
# 1) The entrypoint would need to run as root because of root requirements for usermod and groupmod
# 2) Current tools for switching the running user (su-exec and gosu) mid-script have their own issues with TTY in a Docker container
# See: https://github.com/ncopa/su-exec/issues/33 and https://github.com/tianon/gosu/pull/8   
ARG UID=1000
ARG GID=1000
ENV UMASK=0002

# Set a specific tModLoader version, defaults to the latest Github release
ARG TMLVERSION

# Create tModLoader user and drop root permissions
# BusyBox uses an old adduser without the --user-group option so create a group first
RUN groupadd -g $GID tml \
    && useradd -m -d /home/tml -u $UID -g tml tml

USER tml
ENV HOME /home/tml
ENV USER tml
ENV PATH="$PATH:$HOME/.bin"
WORKDIR $HOME

# Setup the steam directory and steamcmd for the local user
RUN mkdir -p ~/Steam ~/.bin \
    && curl -sqL "https://steamcdn-a.akamaihd.net/client/installer/steamcmd_linux.tar.gz" | tar zxvf - -C ~/Steam

# Create an easy to use steamcmd executable
COPY --chown=tml:tml --chmod=0755 <<EOF ./.bin/steamcmd
#\!/bin/bash

exec ~/Steam/steamcmd.sh "\$@"
EOF

# Update SteamCMD and verify latest version
RUN steamcmd +quit

# To make local edits to the management script, change this URL to a local path for manage-tModLoaderServer.sh
ADD --chown=tml:tml --chmod=0755 https://raw.githubusercontent.com/tModLoader/tModLoader/1.4.5/patches/tModLoader/Terraria/release_extras/DedicatedServerUtils/manage-tModLoaderServer.sh .

# Make management script executable and manually add the logs directory to fix the "Permission Denied" error on most systems.
# Adding write permissions for the logs is necessary because when Docker mounts a volume that doesn't exist on the host it mounts both host and container as root
RUN ISDOCKER=1 ./manage-tModLoaderServer.sh install-tml --github

EXPOSE 7777

ENTRYPOINT [ "./manage-tModLoaderServer.sh", "start", "--folder", "/tModLoader" ]

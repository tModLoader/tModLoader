FROM ubuntu:22.04 as builder

# Update and install the libc requirement that Alpine does not support
ARG DEBIAN_FRONTEND=noninteractive
RUN dpkg --add-architecture i386 \
 && apt-get update -y \
 && apt-get install -y --no-install-recommends libc6:i386 \
 && rm -rf /var/lib/apt/lists/*

FROM alpine:3.20

# TODO try to get gcompat working
RUN apk update \
    && apk add --no-cache bash curl nano file libgcc libstdc++ icu-libs \
    # && echo "x86" > /etc/apk/arch \
    # && apk add --no-cache gcompat \
    # && echo "x86_64" > /etc/apk/arch \
    && rm -rf /var/cache/apk/*

# Copy the required libc files since gcompat does not work with steamcmd
COPY --from=builder \
    /lib/i386-linux-gnu/ld-linux.so.2 \
    /lib/i386-linux-gnu/libc.so.6 \
    /lib/i386-linux-gnu/libdl.so.2 \
    /lib/i386-linux-gnu/libm.so.6 \
    /lib/i386-linux-gnu/libpthread.so.0 \
    /lib/i386-linux-gnu/librt.so.1 \
    /lib/

# TODO: Currently it is too arduous to allow UID/GID to be set at runtime via ENV variables for a few reasons
# 1) The entrypoint would need to run as root because of root requirements for usermod and groupmod
# 2) Current tools for switching the running user (su-exec and gosu) mid-script have their own issues with TTY in a Docker container
# See: https://github.com/ncopa/su-exec/issues/33 and https://github.com/tianon/gosu/pull/8   
ARG UID=1000
ARG GID=1000
ENV UMASK=0002

# Set a specific tModLoader version, defaults to the latest Github release
ARG TMLVERSION

# How long it takes the server to shutdown when Ctrl+C or `docker stop` is used.
ENV SHUTDOWN_TIME=10s

# Create tModLoader user and drop root permissions
# BusyBox uses an old adduser without the --user-group option so we need to create a group first
RUN addgroup -g $GID tml \
    && adduser -D --home /home/tml -u $UID -G tml tml

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
# Currently using turtletowerz/tModLoader to fetch management script (required until changes merged into upstream)
ADD --chown=tml:tml --chmod=0755 https://raw.githubusercontent.com/turtletowerz/tModLoader/1.4.4/patches/tModLoader/Terraria/release_extras/DedicatedServerUtils/manage-tModLoaderServer.sh .

# Make management script executable and manually add the logs directory to fix the "Permission Denied" error on most systems.
# Adding write permissions for the logs is necessary because when Docker mounts a volume that doesn't exist on the host it mounts both host and container as root
RUN ISDOCKER=1 ./manage-tModLoaderServer.sh install-tml --github

# Use stream editor to fix a self-process-reference in ScriptCaller.sh to work with source/exec (required until changes bundled into release)
RUN sed -i 's|cd "$(dirname "$0")"|cd "$(dirname "${BASH_SOURCE[0]}")"|' ./server/LaunchUtils/ScriptCaller.sh

EXPOSE 7777

ENTRYPOINT [ "./manage-tModLoaderServer.sh", "start", "--folder", "/tModLoader" ]

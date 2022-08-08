# Partly from https://github.com/steamcmd/docker/blob/master/dockerfiles/alpine-3/Dockerfile

######## BUILDER ########

# Set the base image
FROM steamcmd/steamcmd:ubuntu-18 as builder

# Set environment variables
ENV USER root
ENV HOME /root/installer

# Set working directory
WORKDIR $HOME

# Install prerequisites
RUN apt-get update \
 && apt-get install -y --no-install-recommends curl tar

# Donload and unpack installer
RUN curl http://media.steampowered.com/installer/steamcmd_linux.tar.gz \
 --output steamcmd.tar.gz --silent
RUN tar -xvzf steamcmd.tar.gz && rm steamcmd.tar.gz

######## INSTALL ########

# Set the base image
FROM alpine:3

# Set environment variables
ENV USER root
ENV HOME /root

# Set working directory
WORKDIR $HOME

# Install prerequisites
RUN apk update \
 && apk add --no-cache bash curl tmux libstdc++ libgcc icu-libs \
 && rm -rf /var/cache/apk/*

# Copy steamcmd files from builder
COPY --from=builder /root/installer/steamcmd.sh /usr/lib/games/steam/
COPY --from=builder /root/installer/linux32/steamcmd /usr/lib/games/steam/
COPY --from=builder /usr/games/steamcmd /usr/bin/steamcmd

# Copy required files from builder
COPY --from=builder /etc/ssl/certs /etc/ssl/certs
COPY --from=builder /lib/i386-linux-gnu /lib/
COPY --from=builder /root/installer/linux32/libstdc++.so.6 /lib/

# Fixes conflicting libraries between steamcmd and dotnet
RUN mkdir /steamlib \
 && mv /lib/libstdc++.so.6 /steamlib \
 && mv /lib/libgcc_s.so.1 /steamlib
ENV LD_LIBRARY_PATH /steamlib

# Create tModLoader user and drop root permissions
RUN adduser tml -h /home/tml -D
USER tml
ENV USER tml
ENV HOME /home/tml
WORKDIR $HOME

# Update SteamCMD and verify latest version
RUN steamcmd +quit

#NOTE: Update url before merging
RUN curl -O https://raw.githubusercontent.com/pollen00/tModLoader/serversetup/patches/tModLoader/Terraria/release_extras/DedicatedServerUtils/Setup_tModLoaderServer.sh \
 && chmod u+x Setup_tModLoaderServer.sh \
 && ./Setup_tModLoaderServer.sh -i -g

#NOTE: Temp step to download the no steam tML build. Remove before merging.
RUN cd $HOME/tModLoader \
 && rm -r * \
 && curl -O https://ppeb.me/tml/nosteam.zip \
 && unzip nosteam.zip

# Download entrypoint script
#NOTE: Update url before merging
RUN curl -O https://raw.githubusercontent.com/pollen00/tModLoader/serversetup/patches/tModLoader/Terraria/release_extras/DedicatedServerUtils/Docker/Launch.sh \
 && chmod u+x Launch.sh

#NOTE: Temp step to download modified launch script. Remove before merging.
RUN curl -O https://raw.githubusercontent.com/pollen00/tModLoader/serversetup/patches/tModLoader/Terraria/release_extras/start-tModLoaderServer.sh \
 && chmod u+x start-tModLoaderServer.sh \
 && mv start-tModLoaderServer.sh $HOME/tModLoader/start-tModLoaderServer.sh

EXPOSE 7777

ENTRYPOINT [ "/bin/bash" ]

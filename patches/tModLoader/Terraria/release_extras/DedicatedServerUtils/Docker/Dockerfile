FROM steamcmd/steamcmd:alpine-3

# Install prerequisites
RUN apk update \
 && apk add --no-cache bash curl tmux libstdc++ libgcc icu-libs \
 && rm -rf /var/cache/apk/*

# Fix 32 and 64 bit library conflicts
RUN mkdir /steamlib \
 && mv /lib/libstdc++.so.6 /steamlib \
 && mv /lib/libgcc_s.so.1 /steamlib
ENV LD_LIBRARY_PATH /steamlib

# Create tModLoader user and drop root permissions
ARG UID
ARG GID
RUN addgroup -g $GID tml \
 && adduser tml -u $UID -G tml -h /home/tml -D
USER tml
ENV USER tml
ENV HOME /home/tml
WORKDIR $HOME

# Update SteamCMD and verify latest version
RUN steamcmd +quit

#NOTE: Update url before merging
RUN curl -O https://raw.githubusercontent.com/pollen00/tModLoader/serversetup/patches/tModLoader/Terraria/release_extras/DedicatedServerUtils/manage-tModLoaderServer.sh \
 && chmod u+x manage-tModLoaderServer.sh \
 && ./manage-tModLoaderServer.sh -i -g --no-mods

# Download entrypoint script
#NOTE: Update url before merging
RUN curl -O https://raw.githubusercontent.com/pollen00/tModLoader/serversetup/patches/tModLoader/Terraria/release_extras/DedicatedServerUtils/Docker/Launch.sh \
 && chmod u+x Launch.sh

#NOTE: Temp step to download modified launch script. Remove before merging.
RUN curl -O https://raw.githubusercontent.com/pollen00/tModLoader/serversetup/patches/tModLoader/Terraria/release_extras/start-tModLoaderServer.sh \
 && chmod u+x start-tModLoaderServer.sh \
 && mv start-tModLoaderServer.sh $HOME/tModLoader/start-tModLoaderServer.sh

EXPOSE 7777

ENTRYPOINT [ "/bin/bash", "-c", "./Launch.sh" ]

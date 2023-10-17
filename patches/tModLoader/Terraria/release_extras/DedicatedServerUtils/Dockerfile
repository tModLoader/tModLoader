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

# Set a specific tModLoader version, defaults to the latest Github release
ARG TML_VERSION

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

RUN curl -O https://raw.githubusercontent.com/tModLoader/tModLoader/1.4.4/patches/tModLoader/Terraria/release_extras/DedicatedServerUtils/manage-tModLoaderServer.sh \
  && chmod u+x manage-tModLoaderServer.sh \
  && /bin/bash ./manage-tModLoaderServer.sh install-tml --github --tml-version $TML_VERSION

EXPOSE 7777

ENTRYPOINT [ "/bin/bash", "-c", "./manage-tModLoaderServer.sh docker --folder ~/.local/share/Terraria/tModLoader" ]

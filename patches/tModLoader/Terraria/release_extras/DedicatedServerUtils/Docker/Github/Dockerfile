FROM alpine:3.16.1

# Install prerequisites
RUN apk update \
 && apk add --no-cache bash curl tmux \
 && rm -rf /var/cache/apk/*

# Create tml user and drop root permissions
RUN adduser tml -h /home/tml -D
USER tml
ENV HOME /home/tml
WORKDIR $HOME

# Install tModLoader
RUN mkdir -p $HOME/.local/share/Terraria/scriptdir \
 && cd $HOME/.local/share/Terraria/scriptdir \
 && curl -O https://raw.githubusercontent.com/pollen00/tModLoader/serversetup/patches/tModLoader/Terraria/release_extras/DedicatedServerUtils/Setup_tModLoaderServer.sh \
 && chmod u+x Setup_tModLoaderServer.sh \
 && ./Setup_tModLoaderServer.sh -i -g

# Prepare launch utils
RUN curl -O https://raw.githubusercontent.com/pollen00/tModLoader/serversetup/patches/tModLoader/Terraria/release_extras/DedicatedServerUtils//Docker/Launch.sh \
 && chmod u+x Launch.sh

# Replace start-tModLoaderServer.sh
RUN curl -O https://raw.githubusercontent.com/pollen00/tModLoader/serversetup/patches/tModLoader/Terraria/release_extras/start-tModLoaderServer.sh \
 && chmod u+x start-tModLoaderServer.sh \
 && mv start-tModLoaderServer.sh $HOME/tModLoader

EXPOSE 7777

ENTRYPOINT [ "./Launch.sh", "-g", "--location", "~/tModLoader", "--nosteamserver" ]

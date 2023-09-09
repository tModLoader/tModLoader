FROM steamcmd/steamcmd:ubuntu-22

# Install prerequisites
RUN apt-get update && apt-get -y upgrade \
 && apt-get -y install bash curl unzip tmux libstdc++-10-dev libgcc-10-dev icu-devtools \
 && rm -rf /var/lib/apt/lists/*

# Create tModLoader user and drop root permissions
ARG UID
ARG GID
RUN groupadd -g $GID tml \
 && useradd --gid $GID --uid $UID --create-home --home-dir /home/tml tml

USER tml
ENV USER tml
ENV HOME /home/tml
WORKDIR $HOME

# Update SteamCMD and verify latest version
RUN steamcmd +quit

RUN curl -O https://raw.githubusercontent.com/tModLoader/tModLoader/1.4.4/patches/tModLoader/Terraria/release_extras/DedicatedServerUtils/manage-tModLoaderServer.sh \
  && chmod u+x manage-tModLoaderServer.sh \
  && ./manage-tModLoaderServer.sh install --github --skip-mods

EXPOSE 7777

ENTRYPOINT [ "/bin/bash", "-c", "./manage-tModLoaderServer.sh docker --folder ~/.local/share/Terraria/tModLoader" ]

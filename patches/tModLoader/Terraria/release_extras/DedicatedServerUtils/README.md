# Dedicated Server Utils
This directory contains utilities for managing a dedicated ***Linux*** server. There is the `manage-tModLoaderServer.sh` script for direct management of an installation or a Docker container for automatic management.

## Using Docker or the Script
Docker is a service used to containerize, or isolate, a specific application from a host system. This is beneficial for both security and ease of use with more complicated services. This will allow you to use tModLoader without affecting the host system. If you would prefer more control over your server or would prefer not to use Docker, then make use of `manage-tModLoaderServer.sh`.

## Using The Script
The `manage-tModLoaderServer.sh` script can be used to install tML either directly from the Github release or from SteamCMD. The script is made to run fully standalone, so just download it to your server and run it. No other files from the repo are needed.

### Installing tModLoader
#### Via SteamCMD (recommended)
* Installation via SteamCMD can be performed using `./manage-tModLoaderServer.sh --install --username your_steam_username`.
* You will be prompted for your password (and your 2fa code if applicable).
* By default, tModLoader will be installed to `~/Steam/steamapps/common/tModLoader`. To specify an installation directory, use `--folder /full/path/from/root`

#### From GitHub
* Installation from the latest GitHub release can be performed using `./manage-tModLoaderServer.sh --install --github`.
* By default, tModLoader will be installed to `~/tModLoader`. To specify an installation directory, use `--folder /path/to/install`.

Aditionally, you can avoid updating or installing mods with the `--no-mods` argument.

### Installing Mods
Mods will be automatically installed during the tModLoader installation step, but can also be installed separately using the `--mod-only` argument. Simply place any `.tmod` files, and `enabled.json` into the same directory as the script.

#### Via Modpack
Because the steam workshop uses UUIDs, to install mods from the workshop you must create a modpack. To get an `install.txt` and its accompanying `enabled.json`:
* Go to Workshop
* Go to Mod Packs
* Click `Save Enabled as New Mod Pack`
* Click `Open Mod Pack Folder`.
* Enter the folder with the name of your modpack

You can copy `enabled.json` and `install.txt` to your script directory and they will be used next time the script is run.

### Launching
To run tModLoader, you just need to navigate to your install directory (`~/tModLoader` for GitHub, `~/Steam/steamapps/common/tModLoader` for SteamCMD, by default), and run `./start-tModLoaderServer.sh`.

#### Automatically Selecting A World
If you want to run tModLoader without needing any input on startup (such as from an init system), then all you need to do is copy the example [serverconfig.txt](https://github.com/tModLoader/tModLoader/tree/1.4/patches/tModLoader/Terraria/release_extras/DedicatedServerUtils/serverconfig.txt) and change the settings how you like. Additional options can be found [on the Terraria wiki](https://terraria.wiki.gg/wiki/Server#Server_config_file)

## Updating
If an update for `manage-tModLoaderServer.sh` is availble, a message will be printed letting you know one is available. It can be updated using `./manage-tModLoaderServer.sh --update-script`. An outdated script may contain bugs or lack features, so it is usually a good idea to update.

When using `manage-tModLoaderServer.sh`, tModLoader updates can be performed with `./manage-tModLoaderServer.sh --update`. When using a Github install, use `--github` and `--folder` if your install is in a non-standard location. Mods will be updated as well.

When using the Docker container, simply rebuild the container using `docker-compose build` to update tModLoader. Mods will be updated as well.

## Using The Docker Container
Before building and running the container, you first need to prepare a directory to pass to the container. This directory will link to `~/.local/share/Terraria` inside of the container. Within the directory, place your `enabled.json`, [install.txt](#via-modpack), [serverconfig.txt](#automatically-selecting-a-world), and any `.tmod` files. To manually add worlds, place them in `tModLoader/Worlds` inside of your directory (you may have to create the directory yourself).

To install and run the container:
* Ensure `docker` and `docker-compose` are installed
* Download [docker-compose.yml](https://github.com/tModLoader/tModLoader/tree/1.4/patches/tModLoader/Terraria/release_extras/DedicatedServerUtils/Docker/docker-compose.yml) and the [Dockerfile](https://github.com/tModLoader/tModLoader/tree/1.4/patches/tModLoader/Terraria/release_extras/DedicatedServerUtils/Docker/Dockerfile) next to the folder you created earlier
* Edit `docker-compose.yml` with your GID and UID. These can be found by running `id`.
* Run `docker-compose build`
* Run `docker-compose up`

The server will be available on port 7777. You can change this by editing `docker-compose.yml`

To run without any interactivity, use `docker-compose up -d`, and include [serverconfig.txt](#automatically-selecting-a-world) in the base of the directory you created earlier.

## Autostarting
Docker: Add `restart: always` within `services.tml` inside of `docker-compose.yml`, then rebuild with `docker-comopose build`.

`manage-tModLoaderServer.sh`: This is platform dependent. You can use your system's init system to automatically start the server on launch, although you'll want to provide a [serverconfig.txt](#serverconfig.txt) due to the headless nature of this approach. An example for a common init system, `systemd`, is below. Lines with placeholder that need to be changed are marked with a `💀`.

```
[Unit]
Description=tML Server
After=network-online.target
Wants=network-online.target systemd-networkd-wait-online.service

StartLimitIntervalSec=500
StartLimitBurst=5

[Service]
User=youruser 💀
Group=yourgroup 💀
Restart=on-failure
RestartSec=5s

WorkingDirectory=/home/youruser/tModLoader 💀
ExecStart=bash -c ./start-tModLoaderServer.sh -nosteam

[Install]
WantedBy=multi-user.target
```
The server can then be started using `systemctl start YourServiceFile`. Autostart can be enabled by running `systemctl enable YourServiceFile`. To find out where to place the service file, refer to your distro's documentation.

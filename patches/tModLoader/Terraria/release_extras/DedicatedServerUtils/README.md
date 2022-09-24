# Dedicated Server Utils
This directory contains utilities for managing a dedicated ***Linux*** server.

## Docker vs Management Script
While both the Docker container and the management script can install and update tModLoader and any mods, there are a few key differences. Docker isolates tModLoader from your host system and increases security. The management script allows for direct access to your server and increased control as a result. If you are a public server operator or just prefer Docker, then go with the [Docker Container](#using-the-docker-container), otherwise make use of the [management script](#using-the-management-script).

## Using The Management Script
The `manage-tModLoaderServer.sh` script can be used to install tModLoader either directly from the GitHub release or from SteamCMD. The script is made to run fully standalone, so just download it to your server and run it. No other files from the repo are needed.

### Installing tModLoader
#### Via SteamCMD (recommended)
* Ensure SteamCMD is installed and on your PATH. You can install SteamCMD from your package manager or [Valve's Wiki](https://developer.valvesoftware.com/wiki/SteamCMD).
* Run `./manage-tModLoaderServer.sh --install --username your_steam_username`.
* You will be prompted for your password (and your 2fa code if applicable).
* By default, tModLoader will be installed to `~/Steam/steamapps/common/tModLoader`. To specify an installation directory, use `--folder /full/path/from/root`

#### From GitHub
* Run `./manage-tModLoaderServer.sh --install --github`.
* By default, tModLoader will be installed to `~/tModLoader`. To specify an installation directory, use `--folder /path/to/install`.
* This will install the latest GitHub release, which is the same version as released on Steam.

### Installing Mods
Mods will be automatically installed during the tModLoader installation step, but can also be installed separately using the `--mods-only` argument. Simply place any `.tmod` files, `install.txt` for workshop mods, and `enabled.json` into the same directory as the script. Additionally, you can avoid updating or installing mods with the `--no-mods` argument.

#### Obtaining install.txt
Because the steam workshop does not use mod names to identify mods, you must create a modpack to install mods from the workshop. To get an `install.txt` file and its accompanying `enabled.json`:
* Go to Workshop
* Go to Mod Packs
* Click `Save Enabled as New Mod Pack`
* Click `Open Mod Pack Folder`.
* Enter the folder with the name of your modpack

You can copy `enabled.json` and `install.txt` to your script directory and they will be used next time the script is run (run `./manage-tModLoaderServer.sh --mods-only` to install mods immediately).

### Launching
To run tModLoader, you just need to navigate to your install directory (`~/tModLoader` for GitHub, `~/Steam/steamapps/common/tModLoader` for SteamCMD, by default), and run `./start-tModLoaderServer.sh`. There is also a `--start` argument that will launch the game.

#### Automatically Selecting A World
If you want to run tModLoader without needing any input on startup (such as from an init system), then all you need to do is copy the example [serverconfig.txt](https://github.com/tModLoader/tModLoader/tree/1.4/patches/tModLoader/Terraria/release_extras/DedicatedServerUtils/serverconfig.txt) and change the settings how you like. Additional options can be found [on the Terraria wiki](https://terraria.wiki.gg/wiki/Server#Server_config_file)

## Updating
If an update for `manage-tModLoaderServer.sh` is available, a message will be printed letting you know one is available. It can be updated using `./manage-tModLoaderServer.sh --update-script`. An outdated script may contain bugs or lack features, so it is usually a good idea to update.

When using `manage-tModLoaderServer.sh`, tModLoader updates can be performed with `./manage-tModLoaderServer.sh --update`. When using a GitHub install, use `--github`. Use`--folder` if your install is in a non-standard location. Mods will be updated as well.

When using the Docker container, simply rebuild the container using `docker-compose build` to update tModLoader. Mods will be updated as well.

## Using The Docker Container
To install and run the container:
* Ensure `docker` and `docker-compose` are installed. They can be installed from your package manager or [Docker's Documentation](https://docs.docker.com/engine/install/)
* Download [docker-compose.yml](https://github.com/tModLoader/tModLoader/tree/1.4/patches/tModLoader/Terraria/release_extras/DedicatedServerUtils/Docker/docker-compose.yml) and the [Dockerfile](https://github.com/tModLoader/tModLoader/tree/1.4/patches/tModLoader/Terraria/release_extras/DedicatedServerUtils/Docker/Dockerfile).
* Next to those docker files, create a folder named `Terraria`, and place `enabled.json`, [install.txt](#obtaining-install.txt), [serverconfig.txt](#automatically-selecting-a-world), your worlds, and any `.tmod` files inside.
* Edit `docker-compose.yml` with your GID and UID. These can be found by running `id`.
* Run `docker-compose build`
* Run `docker-compose up`

The server will be available on port 7777.

To run without any interactivity, use `docker-compose up -d`, and include [serverconfig.txt](#automatically-selecting-a-world) in the `Terraria` directory.

## Autostarting On Boot
When using `manage-tModLoaderServer.sh`, refer to your distro's documentation. You can likely use a startup script with your init system.

When using the Docker container, add `restart: always` within `services.tml` inside of `docker-compose.yml`, then rebuild with `docker-compose build`.

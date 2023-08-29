# Dedicated Server Utils
This directory contains utilities for a dedicated server on Linux or Docker

### Installation Options
**Currently ARM support does not exist for TML**. While both the Docker container and the management script can install and update tModLoader and any mods, there are a few key differences. Docker isolates tModLoader from your host system, allowing for less manual management and increased security. The management script allows for direct access to your server and increased control as a result.

## Quick Links
[Pre-installation steps](#pre-install)

[Docker Installation (recommended)](#using-the-docker-container)

[Management Script Installation](#using-the-management-script)

[Server Configuration](#server-configuration)

[Updating the Management Script](#updating-the-management-script)

## 1.4.4 Migration
The 1.4.4 script is a **backwards-incompatible update** that introduces a new [folder structure](#folder-structure) that both the management script and the Docker container need to conform to. Additionally, the management script has moved from flags to commands for installing, updating and starting the server. Run `./manage-tModLoaderServer.sh help` to see a list of available commands/flags.

---

## Pre-install
Both the Docker and Management script use the same folder structure, so make sure it's setup properly before installing. Here is an example:

```
├── Mods
│   ├── localmod1.tmod
│   ├── localmod2.tmod
│   └── enabled.json
├── Worlds
│   ├── world1.twld
│   ├── world1.wld
│   ├── world2.twld
│   └── world2.wld
├── server
│   └── * contains tModLoader installation and Steam workshop mods *
├── install.txt
├── manage-tModLoaderServer.sh
├── serverconfig.txt
└── tmlversion.txt
```
The `server` folder is created by the management script and do not need to be edited unless you want to clear your installation.

### Server Configuration
If you want to run tModLoader without needing any input on startup (such as from an init system), copy the example [serverconfig.txt](https://github.com/tModLoader/tModLoader/tree/1.4.4/patches/tModLoader/Terraria/release_extras/serverconfig.txt) and change the settings how you like. Key options are defined below, and other options can be found [on the Terraria wiki](https://terraria.wiki.gg/wiki/Server#Server_config_file).
* `worldname` changes the default world name when creating a new world using autocreate **You do not need to include .wld in your world name**. This setting **will not** work with an existing world, see the `world` option for an existing world.
* `world` sets the exact path to an existing or new terraria world, ex. `worldpath/to/your/world.wld`. **For Docker installations**, the world path must follow `/home/tml/.local/share/Terraria/tModLoader/Worlds/your_world.wld`.
* `autocreate=1` will enable autocreating, which creates a new world at your provided location if one does not already exist.

---

## Using The Docker Container
To install and run the container:
1. Install `docker` from your package manager or [Docker's Official Page](https://docs.docker.com/engine/install/)
   * **To check if Compose V2 is installed in this package**, run `docker compose version`. If the command errors, your manager still uses V1 and will need to additionally install the `docker-compose` package. All commands below assume Compose V2 is installed, so if you have V1 replace any `docker compose` commands with `docker-compose`
2. Download [docker-compose.yml](https://github.com/tModLoader/tModLoader/tree/1.4.4/patches/tModLoader/Terraria/release_extras/DedicatedServerUtils/docker-compose.yml) and the [Dockerfile](https://github.com/tModLoader/tModLoader/tree/1.4.4/patches/tModLoader/Terraria/release_extras/DedicatedServerUtils/Dockerfile).
3. Next to those docker files, create a folder named `Terraria`, and create a proper [folder structure](#folder-structure) for your server in that folder.
4. Edit `docker-compose.yml` with your GID and UID. These can be found by running `id`, and generally default to 1000.
5. Run `docker compose up`
   * If [serverconfig.txt](#server-configuration) exists in the `Terraria` directory add `-d` to the end to run without interactivity.

To attach to the server console run `docker attach tml`. To detach from the console press `Ctrl-P Ctrl-Q` to avoid shutting down or `Ctrl-C` to detach and shutdown the server.

### Updating
To update, download the newest container and rebuild it using `docker compose build` to update tModLoader. Mods will be updated as well.

---

## Using The Management Script
The `manage-tModLoaderServer.sh` script can be used to install tModLoader either directly from the GitHub release or from SteamCMD. The script is made to run fully standalone, so just download it to your server and run it. No other files from the repo are needed.

### Installing tModLoader
#### SteamCMD (recommended)
1. Ensure SteamCMD is installed and on your PATH. You can install SteamCMD from your package manager or [Valve's Wiki](https://developer.valvesoftware.com/wiki/SteamCMD).
2. Run `./manage-tModLoaderServer.sh install --username your_steam_username` and enter any password/2fa if necessary. tModLoader will install to the `server` directory in your installation folder.

#### GitHub
1. Run `./manage-tModLoaderServer.sh install --github`. This will install the latest GitHub release, which is the same version as released on Steam. 
   * If you wish to use a custom/legacy tModLoader version from Github, provide either a `tmlversion.txt` file from a modpack or pass the `--tml-version` flag with a specified version, ex. `v2022.06.96.4`

### Installing Mods
Mods will be automatically installed during the tModLoader installation step, but can also be installed separately by running `./manage-tModLoaderServer.sh install --skip-tml`. Provide `install.txt` for workshop mods, and make a `Mods` folder with `enabled.json` to enable any necessary mods. **You will need a `Mods/enabled.json` for any mods to be enabled on your server**. Any local mod files can also be copied into the Mods directory.

#### Obtaining install.txt
Because the steam workshop does not use mod names to identify mods, you must create a modpack to install mods from the workshop. To get an `install.txt` file and its accompanying `enabled.json`
1. Go to Workshop
2. Go to Mod Packs
3. Click `Save Enabled as New Mod Pack`
4. Click `Open Mod Pack Folder`.
5. Enter the folder with the name of your modpack
6. Copy `install.txt` to your script directory. Then make a `Mods` folder and copy the `enabled.json` file into the Mods folder.
7. Run `./manage-tModLoaderServer.sh install --skip-tml` to install the mods on your server

### Launching
To start a server, run `./manage-tModLoaderServer.sh start`. Be sure to pass in `--folder` again if you used a custom location during installation.

### Updating
Run `./manage-tModLoaderServer.sh update` to update both Mods and TML to their latest versions. You can optionally skip mod updating with `--skip-mods`, or skip tModLoader updating with `--skip-tml`

---

## Updating the Management Script
If an update for `manage-tModLoaderServer.sh` is available, a message will be printed letting you know one is available. It can be updated using `./manage-tModLoaderServer.sh update-script`. An outdated script may contain bugs or lack features, so it is usually a good idea to update.

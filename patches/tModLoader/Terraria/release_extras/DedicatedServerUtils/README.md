# Dedicated Server Utils
This directory contains utilities for a dedicated server on Linux or Docker

---

## ARM64 Linux Support

Linux ARM64 (aarch64) is natively supported as of tModLoader 1.4.4. No extra workarounds are required — you can install directly from Steam or GitHub releases.

If you are using **tModLoader 1.4.3 or earlier**, the Steamworks.NET library did not support ARM in its managed code. For those legacy versions only, use the [20.1.0 Release build on this fork](https://github.com/MikolajKolek/Steamworks.NET-arm64/releases/tag/20.1.0) by [MikolajKolek](https://github.com/MikolajKolek) and replace the existing `.dll` in `Libraries/Steamworks.net/20.1.0/lib/netstandard2.1`.
---

## Quick Links

### Preinstall Steps (REQUIRED)
* [Specific Folder Structure](#preinstall-folder-structure)

### Installation
* [Docker (recommended)](#using-the-docker-container)

* [Management Script](#using-the-management-script)

* [Installing Mods](#mod-installation)


[Server Configuration](#server-configuration)

[Updating the Management Script](#updating-the-management-script)

## Preinstall Folder Structure
Both the Docker and Management script **require** the same folder structure, so make sure it's setup properly before installing. Here is what is should look like:

```
docker-compose.yml (Docker version only)
Dockerfile (Docker version only)
tModLoader
├── Mods
│   ├── localmod1.tmod
│   ├── localmod2.tmod
│   ├── enabled.json
│   └── install.txt
├── Worlds
│   ├── world1.twld
│   ├── world1.wld
│   ├── world2.twld
│   └── world2.wld
├── server (automatically created, management script version only)
│   └── * contains tModLoader installation *
├── steamapps (automatically created)
│   └── * contains Steam workshop mods *
├── manage-tModLoaderServer.sh (management script version only)
├── serverconfig.txt (optional)
└── tmlversion.txt (optional)
```

### Obtaining install.txt and enabled.json
`install.txt` and `enabled.json` are needed for any mods you wish to install from the Steam Workshop.
The steam workshop does not use mod names to identify mods, so a modpack should be made to install mods from the workshop
1. From the TML main menu, go to Workshop -> Mod Packs
2. Click `Save Enabled as New Mod Pack`
3. Click `Open Mod Pack Folder`
4. Enter the folder with the name of your modpack
5. Make a `Mods` folder and copy `install.txt` and `enabled.json` file into it
6. **Management Script Version Only**: Run `./manage-tModLoaderServer.sh install-mods` to install the mods on your server

---

## Using The Docker Container
### Installation
1. First, ensure you have a proper [folder structure](#preinstall-folder-structure)
   * It is also **highly** recommended to create a [serverconfig.txt](#server-configuration) file. This makes it so when you start the container you don't have to attach to manually configure the server
2. Install `docker` from your package manager or [Docker's Official Page](https://docs.docker.com/engine/install/)
   * **To check if Compose V2 is installed in this package**, run `docker compose version`. If the command errors, your manager still uses V1 and will need to additionally install the `docker-compose` package. All commands below assume Compose V2 is installed, so if you have V1 replace any `docker compose` commands with `docker-compose`
3. Download [docker-compose.yml](https://github.com/tModLoader/tModLoader/tree/1.4.4/patches/tModLoader/Terraria/release_extras/DedicatedServerUtils/docker-compose.yml) and the [Dockerfile](https://github.com/tModLoader/tModLoader/tree/1.4.4/patches/tModLoader/Terraria/release_extras/DedicatedServerUtils/Dockerfile)
4. Edit `docker-compose.yml` with your GID and UID. These can be found by running `id`, and generally default to 1000
   * **IMPORTANT:** Make sure all files in the `tModLoader` folder have this same UID/GID. This can be set by running `chown -R UID:GID tModLoader`
   * You can optionally also set the `TMLVERSION` arg to get a specific tModLoader version
5. Run `docker compose up -d`. This command will create the `Mods` and `Worlds` directories if they don't already exist
   * To attach to the server console run `docker attach tml`. To detach from the console press `Ctrl-P Ctrl-Q` to avoid shutting down or `Ctrl-C` to detach and shutdown the server

### Running Commands
To run commands inside the container, run `docker exec -it tml execute "YOUR COMMAND"`. An example hello world would be `docker exec -it tml execute "say Hello World!"`. The quotes are required around the entire command because tmux can only accept one argument to be passed, otherwise the command is sent as a single word without spaces

### Updating
To update, download the newest container and rebuild it using `docker compose build` to update tModLoader. Mods will be updated as well. If the container doesn't rebuild you can force rebuilding by running `docker compose build --no-cache`

---

## Using The Management Script
The `manage-tModLoaderServer.sh` script can be used to install tModLoader either directly from the GitHub release or from SteamCMD. The script is made to run fully standalone, so just download it to your server and run it. This is not the recommended way to install the dedicated server, and typically is only used to access the server's files

To explore all the options before continuing, run `./manage-tModLoaderServer.sh -h` to get a list of all environment variables and command line parameters

### Installation
1. Ensure you have a proper [folder structure](#preinstall-folder-structure)
2. Install either the SteamCMD or Github release
   * **SteamCMD (recommended)** 
      1. Ensure SteamCMD is installed and on your PATH. You can install SteamCMD from your package manager or [Valve's Wiki](https://developer.valvesoftware.com/wiki/SteamCMD). If your distribution cannot install SteamCMD the standard way, download it manually and pass the `STEAMCMDPATH` environment variable to the management script
      2. Run `./manage-tModLoaderServer.sh install-tml --username your_steam_username` and enter any password/2fa if necessary. tModLoader will install to the `server` directory in your installation folder
   
   * **Github**
      1. Run `./manage-tModLoaderServer.sh install-tml --github`. This will install the latest GitHub release, which is the same version as released on Steam
         * To use a specific/legacy tModLoader version from Github, provide either a `tmlversion.txt` file from a modpack or pass the `TMLVERSION` environment variable with a specific version, e.g. `v2022.06.96.4`
3. Install any necessary mods with `./manage-tModLoaderServer.sh install-mods`
   * No mods will be installed if `install.txt` is missing, and no mods will be enabled if `enabled.json` is missing. **You will need a `Mods/enabled.json` to contain all Mods that you want enabled, including local mods**
4. Start the server with `./manage-tModLoaderServer.sh start`. Be sure to pass in `--folder` again if you used a custom location during installation

**NOTE**: `install-mods` and `install-tml` can be run together with the `install` command, e.g. `./manage-tModLoaderServer.sh install ...`

### Updating
`./manage-tModLoaderServer.sh install` will update both TML and installed mods. To update just mods, run the `install-mods` command. To only update TML, run the `install-tml` command

---

## Server Configuration
To run tModLoader without needing any input on startup (such as from an init system), copy the example [serverconfig.txt](https://github.com/tModLoader/tModLoader/tree/1.4.4/patches/tModLoader/Terraria/release_extras/serverconfig.txt) and change the settings how you like. Key options are defined below, and other options can be found [on the Terraria wiki](https://terraria.wiki.gg/wiki/Server#Server_config_file)
* `worldname` changes the default world name when creating a new world using autocreate **You do not need to include .wld in your world name**. This setting **will not** work with an existing world, see the `world` option for an existing world
* `world` sets the exact path to an existing or new terraria world, ex. `/path/to/your_world.wld`. **For Docker installations**, the world path must follow `/tModLoader/Worlds/your_world.wld`
* `autocreate=1` will enable autocreating, which creates a new world at your provided location if one does not already exist

---

## Updating the Management Script
If an update for `manage-tModLoaderServer.sh` is available, a message will be printed letting you know one is available. It can be updated using `./manage-tModLoaderServer.sh update-script`. An outdated script may contain bugs or lack features, so it is usually a good idea to update when possible

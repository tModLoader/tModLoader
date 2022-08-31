# Dedicated Server Utils

This directory contains utilities for managing a dedicated ***Linux*** server. There is the `manage-tModLoaderServer.sh` script for directly managing an installation or a Docker container for automatic management.

## Using the script

The `manage-tModLoaderServer.sh` script can be used to install tML either directly from the Github release or from SteamCMD. The script is made to run fully standalone, so just download it to your server and run it. No other files from the repo are needed.

Github installations can be done using `./manage-tModLoaderServer.sh --install --github`. SteamCMD is the default so just leave out `--github` to use it instead. By default Github installations will be in `~/tModLoader`, and SteamCMD installations will be in `~/Steam/steamapps/common/tModLoader`.

During installation, [enabled.json](#install.txt-and-enabled.json), [install.txt](#install.txt-and-enabled.json), and any `.tmod` files will be checked for in the directory of the script or in the directory specified by `--checkdir`.

Note that for Github installations `unzip` must be on your `PATH`, and for SteamCMD installations `SteamCMD` must be on your `PATH`. Install it from your package manager or from [steam](https://developer.valvesoftware.com/wiki/SteamCMD).

For additional information, run `./manage-tModLoaderServer.sh --help` to view the available arguments.

## Using the Docker container

Before building and running the container first you need to prepare a directory to pass to the container. This applies to `docker` and `docker-compose` This directory will link to `~/.local/share/Terraria` inside of the container. Within the directory, place your [enabled.json](#install.txt-and-enabled.json), [install.txt](#install.txt-and-enabled.json), `serverconfig.txt`, and any `.tmod` files. This directory will be used in the `docker run` step. To manually add worlds, place them in `tModLoader/Worlds` inside of your directory (you may have to create the directory yourself).

### Building and running the container
 * `docker build .`
 * When building finishes, a 12 character UUID, such as `c7135972bd7c`, will be printed. Use this in the next step.
 * `docker run -v full_path_to_your_dir:/home/tml/.local/share/Terraria -p 7777:7777 your_uuid`

Note that a human-friendly name can be given to your container by passing in `--name container_name` to your `docker-run` command. To run the container on a different port than `7777`, simply change it to be `your_port:7777` in your `docker-run` command. To allow the container to persist after closing your terminal or ssh session, add `-d` to your `docker-run` command.

### Docker Compose
 * Copy `docker-compose.yml` and `Dockerfile` to the same directory
 * `docker-compose build`
 * `docker-compose up`

To allow the container to persist after closing your terminal or ssh session, run with `docker-compose up -d`.


### serverconfig.txt
Because the container is meant to be headless, you can't select a world or any other options using the normal interactive method. They must be specified in `serverconfig.txt`. There are only three things required in your config, but you'll likely want to add more. Additional options can be found [on the Terraria wiki](https://terraria.wiki.gg/wiki/Server#Server_config_file)

```
# The world file to load. If you already have a world, set this to the filename. Only change the last segment of the path.
world=/home/tml/.local/share/Terraria/tModLoader/Worlds/YourWorld.wld
# Allow the world to be created if none are found. World size is specified by 1 (small), 2 (medium), and 3 (large).
autocreate=1
# Sets the name of the world when using autocreate
worldname=YourWorld
```

## Updating
To update the script, run `./manage-tModLoaderServer.sh --updatescript`

When using `manage-tModLoaderServer.sh`, updating can be done by running `./manage-tModLoaderServer.sh --update`. When using a Github install, use `--github` and `--folder` if your install is in a non-standard location.

When using the Docker container, simply rebuild the container to update tModLoader. Use `docker build .` or `docker-compose build` depending on your setup.

## Autostarting
Docker: Start the container using `--restart always`. For additional options view the [documentation](https://docs.docker.com/config/containers/start-containers-automatically/).

Docker Compose: Add `restart: always` within `services.tml` inside of `docker-compose.yml`.

Server Management Script: This is platform dependent. You can use your system's init system to automatically start the server on launch, although you'll want to provide a [config](#serverconfig.txt) due to the headless nature of this approach. An example for a common init system, `systemd`, is below. Lines with placeholder that need to be changed are marked with a `💀`.

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

## install.txt and enabled.json

Due to the steam workshop using UUIDs, to get a list of mods to install from the workshop you must create a modpack. To get an `install.txt` and its accompanying `enabled.json`, go to Workshop > Mod Packs, and then click `Save Enabled as New Mod Pack`. Then click `Open Mod Pack Folder`. You'll see a folder with the name of your modpack. Enter it and both files should be there.

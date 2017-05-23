# tModLoader, a Terraria modding API [![Build Status](https://travis-ci.org/bluemagic123/tModLoader.svg?branch=master)](https://travis-ci.org/bluemagic123/tModLoader) [![Discord](https://discordapp.com/api/guilds/103110554649894912/widget.png?style=shield)](https://discord.me/tmodloader)

## About

tModLoader is an API for Terraria that provides a way to load your own mods without having to work directly with Terraria's source code. This means you can easily make mods that are compatible with other people's mods and save yourself the trouble of having to decompile then recompile Terraria.exe. It is made to work for Terraria version 1.3 and above.

Our goal for tModLoader is to make it simple as possible to mod while giving the modder powerful control over the game. It is designed in a way as to minimize the effort required for us to update to future Terraria versions. If you either don't want to commit to this project or are not able to decompile Terraria, we are open to suggestions for hooks and/or modifications.

Download and installation instructions are on the [forums thread](http://forums.terraria.org/index.php?threads/1-3-tmodloader-a-modding-api.23726/).

**Note**: this repository will be ahead of the current released version.

## How-to install or uninstall

### Installation
___
Installing tModLoader is relatively easy. If you want to ensure you can easily revert back to vanilla, you should make a backup copy of your Terraria.exe and TerrariaServer.exe

1. Goto the **[releases](https://github.com/bluemagic123/tModLoader/releases)** page and download the tML release you want. (usually the **[latest](https://github.com/bluemagic123/tModLoader/releases/latest)**)
2. Unzip the contents somewhere (usually documents or downloads folder)
3. Open the extracted folder, **copy** the contents to your Terraria folder and let it overwrite files when asked. (replace files)
4. Done. You can launch Terraria as usual.

### Uninstallation
___
Uninstallation of tModLoader is even easier. This part covers how to do it when using Steam.

1. Open Steam, go to your game library section and locate Terraria.
2. Let Steam **[verify the integrity of game files](https://support.steampowered.com/kb_article.php?ref=2037-QEUH-3335)** for Terraria, this will redownload vanilla files.
4. Done. You can launch Terraria as usual.

## Contributing

**Huge thanks to [ChickenBones](https://github.com/Chicken-Bones) for creating the code patcher!**

**NOTE: The decompilation doesn't work on Mac. You need Windows.**

### Brief introduction
___
tModLoader uses its own code patcher. If you want to contribute to tModLoader, you will have to use this tool. We need to use a patches system, because we are not allowed to upload vanilla source code publicly. It also allows for relatively easy code maintenance. Here's what the tool looks like: [url](https://i.imgur.com/u9Yy1rl.png)

Before you're about to make a contribution, please check [this article](https://github.com/bluemagic123/tModLoader/blob/master/CONTRIBUTING.md). Thanks in advance.

### Getting the tModLoader code for the first time
___
1. Clone this repository
2. Open setup.bat in the root folder
    * If setup.bat won't open, you must unblock all the files in the cloned repository
3. Select your vanilla terraria.exe (must be vanilla) ([img](https://i.imgur.com/MccGyvB.png))
4. Click on 'Setup' (top left button)
    * **Warning:** decompilation can take several hours to complete depending on your hardware. It's also likely that your computer **completely freezes** during the process, mainly once it hits NPC.cs It is recommended that you enable the 'Single Decompile Thread' option ([img](https://i.imgur.com/6mBbZnQ.png)) if you don't have very high end hardware. It's unwise to even attempt a decompile if you have less than 8 GB RAM. Having an SSD, powerful CPU and a high amount of RAM will significantly speed up the decompilation process.
5. When decompilation is complete, verify that you have these folders:
    * src/decompiled/
    * src/merged/
    * src/Terraria/
    * src/tModLoader/
6. To open up the tModLoader workspace, navigate to solutions/ and open tModLoader.sln
7. ???
8. Profit

### Keeping your code up-to-date
___
**Warning:** it is wise that you backup your edits before pulling latest patches, if you have any that you haven't committed yet. Applying the latest patches **will** delete any of your work not included in them.

1. Pull all newer commits from this repository
   * You should verify that you now have the latest patches, located in patches/
2. Open setup.bat in the root folder
3. Click on 'Regenerate Source' (bottom right corner)
   * After this process you can open solutions/tModLoader.sln as usual with the updated code
4. ???
5. Profit

### Committing your changes
___
1. Open setup.bat in the root folder
2. Click on 'Diff x' where x is your workspace
    * Your workspace is tModLoader 99% of the time. If it isn't, we imply you know what you're doing.
3. Create a new commit to commit the patches/ folder
    * Before you push your commit, please check our [contribution article](https://github.com/bluemagic123/tModLoader/blob/master/CONTRIBUTING.md). Thanks.
4. ???
5. Profit

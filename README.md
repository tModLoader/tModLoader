# tModLoader, a Terraria modding API [![Build Status](https://travis-ci.org/blushiemagic/tModLoader.svg?branch=master)](https://travis-ci.org/blushiemagic/tModLoader) [![Discord](https://discordapp.com/api/guilds/103110554649894912/widget.png?style=shield)](https://discord.me/tmodloader)

## About

tModLoader is an API for Terraria that provides a way to create and load mods without having to work with Terraria's source code directly. This means you can easily make mods that are compatible with other mods and save yourself the trouble of having to decompile then recompile Terraria.exe. This also means players can use multiple mods at the same time without having to worry about compatibility issues. Traditionally most mods (standalone mods) that don't a sort of API like tModLoader to handle them, will be incompatible with each other. tModLoader is made to work for Terraria version 1.3 and above. To view _most_ of our releases you can go to the [releases page](https://github.com/blushiemagic/tModLoader/releases) which will include the binaries required to install tModLoader. Certain older releases are not included on this page. (there is no real reason to use them either)

Our goal for tModLoader is to make it simple as possible to mod while giving the modder powerful control over the game. It is designed in a way as to minimize the effort required for us to update to future Terraria versions. If you either don't want to commit to this project or are not able to decompile Terraria, we are open to suggestions for hooks and/or modifications. Please use our [github issues section](https://github.com/blushiemagic/tModLoader/issues) to do this (you can ignore the default format for suggestions) You can also contact us through our Discord server.

**NOTE**: this repository will be ahead of the current released version.

___
Download and installation instructions are on the [TCF thread](http://forums.terraria.org/index.php?threads/1-3-tmodloader-a-modding-api.23726/).

If you are a modder, the following links might be useful for you:

___
1. [tModLoader documentation](http://blushiemagic.github.io/tModLoader/html/annotated.html)
2. [tModLoader WIKI](https://github.com/blushiemagic/tModLoader/wiki) (useful information and guides)
3. [Join our Discord server](https://discord.me/tmodloader) (requires a DiscordApp account)
4. [Mod skeleton generator](http://javid.ddns.net/tModLoader/generator/ModSkeletonGenerator.html)
 This tool, made by Jopojelly, can quickly setup a .csproj for you with the barebones of a mod.
5. [tModLoader's official release thread on TCF](http://forums.terraria.org/index.php?threads/.23726/)
6. [tModLoaders help & resources thread](http://forums.terraria.org/index.php?threads/tmodloader-code-examples-handy-code-snippets.28901)


## Support us

The best way to support us is by becoming a patron on our [Patreon page](https://www.patreon.com/tModLoader).
You can choose whatever amount of money you'd like to support us with (starting from $1, capping at $15000) either as a monthly 'subscription' or as a one-time payment. We highly appreciate it if you do. Supporting us also comes with various rewards, such as being included in our credits section.

The money that we earn from our Patreon will be initially used to pay for server costs (for the Mod Browser), if the amount grows big enough we might get our own web host for a website. For now, the revenue will be used just to pay for Mod Browser uptime and availability. Thank you in advance for supporting us, whether you decide to become a patron or not!

## How to get in touch

If you would like to contact us or tModLoader users, it's best to join our [Discord server](https://discord.me/tmodloader). Discord is a chat and voice application we've used for over 2 years to communicate with each other. It's similar to IRC chats. To use Discord you will need to register a new account on [their website](https://discordapp.com/).

## How-to install or uninstall

### Installation
___
Installing tModLoader is relatively easy. If you want to ensure you can easily revert back to vanilla, you should make a backup copy of your Terraria.exe and TerrariaServer.exe

1. Goto the **[releases](https://github.com/blushiemagic/tModLoader/releases)** page and download the tML release you want. (usually the **[latest](https://github.com/blushiemagic/tModLoader/releases/latest)**)
2. Unzip the contents somewhere (usually documents or downloads folder)
3. Open the extracted folder, **copy** the contents to your Terraria folder and let it overwrite files when asked. (replace files)
4. Done. You can launch Terraria as usual.

Tip: Here is an easy way to find where your Terraria files are located:

1. Locate Terraria in your Steam game library, right click it and click 'Properties'
2. Browse to the 'Local Files' tab and click on the 'Browse local files...' button
3. You are now in your Terraria folder (this is where you should install tModLoader)

### Uninstallation
___
Uninstallation of tModLoader is even easier than installation. This part covers how to do it when using Steam.

1. Open Steam, go to your game library section and locate Terraria.
2. Let Steam **[verify the integrity of game files](https://support.steampowered.com/kb_article.php?ref=2037-QEUH-3335)** for Terraria, this will reconfigure your game files to run vanilla.
4. Done. You can launch Terraria as usual.

### HELP! I've lost my Players / Worlds when I installed tModLoader, what do I do?
___
tModLoader uses separate folders to store player (.plr) and world (.wld) files, mainly because it will store additional data for them. Your vanilla players and worlds will be stored in: _%UserProfile%\Documents\My Games\Terraria_ (for Windows)
in the _Players_ and _Worlds_ folders respectively. You can select, then copy and paste these folders into the ModLoader directory to migrate your players and worlds to tModLoader. The ModLoader directory is used my tModLoader to store all kinds of files, such as your installed mods. **Please note** that once you play tModLoader, it will create .tplr and .twld files as well as backup files (.bak) for your player and world files. To revert to a backup, simply remove the .plr or .wld file, then rename the .bak file to not include the .bak extension. (If your backup looks like this: playername.plr.bak, then it should be renamed to: playername.plr) If your explorer view is not showing file extensions, you can enable this by opening your 'View' menu and checking the 'File name extensions' checkbox. ([Example](https://i.imgur.com/CzP5yMA.png)) If you can not see this menu, you may need to unfold it by [clicking the down arrow](https://i.imgur.com/O8LqfGz.png) in the top right corner of your explorer view.

## Contributing

**Huge thanks to [ChickenBones](https://github.com/Chicken-Bones) for creating the code patcher!**

**NOTE: The decompilation doesn't work on Mac or Linux. You need Windows.**

### Brief introduction
___
tModLoader uses its own code patcher. If you want to contribute to tModLoader, you will have to use this tool. We need to use a patches system, because we are not allowed to upload vanilla source code publicly. It also allows for relatively easy code maintenance. Here's what the tool looks like: [url](https://i.imgur.com/u9Yy1rl.png)

Before you're about to make a contribution, please check [this article](https://github.com/blushiemagic/tModLoader/blob/master/CONTRIBUTING.md). Thanks in advance.

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

### Keeping your code up-to-date
___
**NOTE:** it is wise that you backup your edits before pulling latest patches, if you have any that you haven't committed yet. Applying the latest patches **will** delete any of your work not included in them.

1. Pull all newer commits from this repository
   * You should verify that you now have the latest patches, located in patches/
2. Open setup.bat in the root folder
3. Click on 'Regenerate Source' (bottom right corner)
   * After this process you can open solutions/tModLoader.sln as usual with the updated code

### Committing your changes
___
1. Open setup.bat in the root folder
2. Click on 'Diff x' where x is your workspace
    * Your workspace is tModLoader 99% of the time. If it isn't, we imply you know what you're doing.
3. Create a new commit to commit the patches/ folder
    * Before you push your commit, please check our [contribution article](https://github.com/bluemagic123/tModLoader/blob/master/CONTRIBUTING.md). Thanks.

### HELP! I accidentally committed on a wrong branch!
Simply stash changes and checkout.
___
1. Open in git shell/bash or whatever
2. Run `git stash save` or `git stash` (should default to save)
3. Run `git checkout -b xxxx`
    * Replace xxxx by branch name
    * Omit -b if not creating a new branch
4. Run `git stash pop`

# tModLoader #
## By: bluemagic123 ##

tModLoader is essentially a mod for Terraria that provides a way to load your own mods without having to work directly with Terraria's source code itself. This means you can easily make mods that are compatible with other people's mods and save yourself the trouble of having to decompile then recompile Terraria.exe. It is made to work for Terraria 1.3.

tModLoader saves worlds and players separately from vanilla worlds and players. Due to specific differences in save file formats, vanilla worlds and players can be ported to tModLoader worlds and players simply by copying over save files, but tModLoader worlds and players cannot be ported back to vanilla worlds and players. (Currently worlds that do not contain modded items can be ported back, but this will change in the future.)

My goal for tModLoader it to make it simple as possible while giving the modder powerful control over the game. It is designed in a way as to minimize the effort required for me to update to future Terraria versions. Once I make substantial progress on tModLoader, I will also become open to everyone's suggestions for more hooks.

Download and installation instructions are on the forums thread.

[Forums Thread](http://forums.terraria.org/index.php?threads/1-3-tmodloader-a-modding-api.23726/)

### Current Version ###
The current released version is: 0.1.2
Changelog:
-Mods that crash the game while loading are now auto-disabled
-Mods are now auto-enabled when they are built
-Added a button to the Mod Sources menu to open the Mod Sources folder
-In-game error messages now appear when the game would have crashed and when a build fails
-Item display names can now be separated from internal names in the SetDefault hook
-Added a ton of hooks for ModItem and GlobalItem
--CanUseItem, UseStyle, UseItemFrame, UseItem, and ConsumeItem
--HoldStyle, HoldItem, and HoldItemFrame
--Shoot, ConsumeAmmo, UseItemHitbox, and MeleeEffects
--ModifyHitNPC, OnHitNPC, ModifyHitPvp, and OnHitPvp
--UpdateInventory, UpdateEquip, UpdateAccessory, IsArmorSet, and UpdateArmorSet
--CanRightClick, RightClick, and Update
--VerticalWingSpeeds and HorizontalWingSpeeds
--GetAnimation (ModItem only), GetAlpha, PreDrawInWorld, and PostDrawInWorld
-Added support for armors and accessories
-Fixed decompile bug that caused minimaps to not save
-Updated to Terraria v1.3.0.6
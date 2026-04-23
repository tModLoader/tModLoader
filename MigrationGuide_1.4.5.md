Note: Once 1.4.5 is stable, the contents of this file will be used to update https://github.com/tModLoader/tModLoader/wiki/Update-Migration-Guide. This page is a work in progress and modders should not consider it a complete guide for porting until tModLoader is approaching a stable release for 1.4.5.

# v2026.?? (1.4.5)

The Terraria 1.4.5 update included major changes to the source code. As such, updating tModLoader to the 1.4.5 content also included many major breaking changes. This release represents a new generation of mods as well, mods made for 1.4.4 will need to be reworked for 1.4.5, they are not backwards compatible due to how drastic many of the changes are. 

Modders should follow this guide to migrate their mod from 1.4.4 to 1.4.5. This migration guide assumes the mod has already been migrated to 1.4.4. If that is not the case, do that first. As with 1.4.3, 1.4.4 mods will continue to be available on the Steam Workshop, even after the modder has published an update on 1.4.5. Modders can continue to publish updates for their mods on 1.4.4 as well as 1.4.5 concurrently, the workshop handles this.

## Porting Prerequisites

This tModLoader release updates .NET from .NET 8 to .NET 10. Modders will need to download and install the [.NET 10 SDK](https://dotnet.microsoft.com/en-us/download). Visual Studio users will need to [update to Visual Studio 2026](https://learn.microsoft.com/en-us/visualstudio/install/update-visual-studio?view=visualstudio) as well. Visual Studio 2022 will not work anymore. Rider and Visual Studio Code users should make sure they are updated as well.

The porting process will change your source code. If you are not yet using and [source code version control](https://github.com/tModLoader/tModLoader/wiki/Intermediate-Git-&-mod-management) like a GitHub repository, now might be the time to learn how to do that. If you are not ready to learn that yet, please at least make a backup of your source code.

## Porting Instructions

To port a mod, the first step is to run tModPorter. Follow [these instructions](https://github.com/tModLoader/tModLoader/wiki/Update-Migration-Guide#tmodporter) to run tModPorter on your mod. You should see many files changed, if that is not the case, something might have gone wrong. 

After that is complete, you are ready to open your IDE and begin working on updating parts of the code that tModPorter either couldn't port or left a comment with instructions for. These remaining items are discussed in more detail below. 

To find all comments, first go to `Tools -> Options -> Environment -> Task List` and add `tModPorter` as a custom token. Then close it and open `View -> Task List`, which will list all comments. You can also use the search bar to filter specific comments. As 1.4.5 continues to update, you might need to "Run tModPorter" again if an update breaks something.

### Legend

In the following sections, many changes will be annotated with icons indicating how much is covered by tModPorter:    
    
🤖 - tModPorter should apply a full fix this change.     
⚙️ - tModPorter will have applied a partial fix or left a comment with instructions for the modder.    
💀 - No automation. The modder will have to determine if this issue affects their mod and make their own fixes.  

## Major Vanilla Changes

These are major structural changes that all modders should be aware of.

### WorldItem

The `Item` class has had the in-world functionality split into a new `WorldItem` class. All items in the game world, such as `Main.item` entries, are now `WorldItem`. A `WorldItem` includes a reference to the underlying `Item` class (`WorldItem.inner`). All fields related to the behavior of an item in the game world, such as the `position`, `velocity`, `shimmered`, `instanced`, and others now belong to the `WorldItem` class. `WorldItem` inherits `Entity`. `Item` no longer inherits `Entity`, but now implements `IEntitySourceTarget`. `Entity` implements `IEntitySourceTarget` as well. All `Entity` fields in `IEntitySource` classes are now `IEntitySourceTarget`.

Hooks that deal with items in the game world will now have a `WorldItem item` parameter as well. This will require modders to switch from `Item` to `item` in various `ModItem` classes if dealing with the fields that are now on `WorldItem`.

### Projectile Draw Changes

There have been several changes to projectile drawing in this update.

To support drawing projectiles on Mannequins and other custom `Player` instances, `(ModProjectile|GlobalProjectile).PreDraw/PreDrawExtras/PostDraw` now has a `Player` parameter. Use this instead of `Main.player[Projectile.owner]` in those methods.

Projectile draw ordering code has also been reworked. Previously, `Projectile.hide` would be used for projectiles that shouldn't be drawn at all, projectiles held by the player, and projectiles drawn with a different draw ordering using `DrawBehind`. Held projectiles and projectiles drawn with a different draw ordering are now handled by `Projectile.drawLayer`, leaving `Projectile.hide` to only indicated if the projectile shouldn't be drawn at all. `(ModProjectile|GlobalProjectile).DrawBehind` has been removed.

Similarly, `Projectile.usesOwnerLight` has been added to indicate if a projectile should draw using lighting values at the player location rather than the projectile location. Previously, `Projectile.hide` being true and `ProjectileID.Sets.DontAttachHideToAlpha` being false would cause the projectile to have this behavior.

Finally, `ProjectileID.Sets.HeldProjDoesNotUsePlayerGfxOffY` has been removed. `AI()` should use `master.RotatedRelativePoint(master.MountedCenter + ...)` to position held projectiles now. (TODO: More information, more samples)

**Sample Migrations:**    
* Projectile using `ModProjectile.DrawBehind`: Remove `DrawBehind` hook, remove `Projectile.hide = true;`, remove `ProjectileID.Sets.DontAttachHideToAlpha[Type] = true;`. Set `Projectile.drawLayer` in `ModProjectile.SetDefaults` to the `ProjectileDrawLayerID` entry matching the desired draw layer.
  * If using `ModProjectile.DrawBehind` for a dynamic draw layer, such as "Bone Javelin"-style sticking projectiles, move that logic to `ModProjectile.AI` and use it to set `Projectile.drawLayer`.
* Held projectile
  * Held in hand: Remove `Projectile.hide = true;`. Set `Projectile.usesOwnerLight = true;` and `Projectile.drawLayer = ProjectileDrawLayerID.HeldProj;` (or `ProjectileDrawLayerID.HeldProjOverHand` if `ModProjectile.DrawHeldProjInFrontOfHeldItemAndArms` was used before) in `ModProjectile.SetDefaults`.
  * Not held in hand (flail): Set `Projectile.drawLayer = ProjectileDrawLayerID.HeldProj;`
* Projectile that shouldn't be drawn at all: No adjustments necessary.

### Town NPC Chat Buttons

Chat buttons for Town NPCs have changed significantly in 1.4.5. The new system uses an `NPCInteraction` to define what the button says, when it should be shown, and what happens when it is clicked. Town NPCs can now support up to 32 buttons visible at one time (no longer limited to firstButton and secondButton).

#### Registering Chat Buttons

#### Creating a Custom NPCInteraction

## New Vanilla Features

The [Terraria 1.4.5 changelog](https://terraria.wiki.gg/wiki/1.4.5.0) lists many vanilla changes made in the 1.4.5 update. A select portion of the changes relevant to modding will be detailed here as well.

### NPC Portraits

Town NPC now have portraits shown while talking to them. Modders will need to make portrait sprites for each Town NPC to support this feature. Adding a detailed portrait is technically optional. Do note that if a detailed portrait is not provided, the NPC will display the profile portraits as a fallback if the detailed portrait setting is selected.

#### Detailed Portrait sprites
The detailed portrait sprites are expected to be a resolution of 200x200 pixels. The background is 112x112 and the center 96x96 is where the head of the NPC should be. This means most portraits will only take up the center of the sprite and will have a lot of empty space around them. The head of the NPC should be facing forward and to the right to match the vanilla NPCs. Modders should create a portrait for both the normal and shimmered variant if applicable.

Sample portraits
<img width="200" height="200" alt="SampleMale_Portrait" src="https://github.com/user-attachments/assets/2deda86f-232f-4fa2-a63b-a95c4e3bc78d" />
<img width="200" height="200" alt="SampleFemale_Portrait" src="https://github.com/user-attachments/assets/154a4194-9322-4720-8f44-8a2e0ecefb44" />
Portrait window for reference <img width="112" height="112" alt="Portrait_Window" src="https://github.com/user-attachments/assets/73897b78-bb92-43ed-bdcd-7af9392159fa" />

#### Detailed Portrait code
To assign a portrait to an NPC, use `NPCID.Sets.NPCPortraits` dictionary in the NPC's `SetStaticDefaults()`.
* `NPCID.Sets.NPCPortraits.Add(int key, NPCID.Sets.NPCPortraitProvider value)`
  * The key is the NPC type. Pass `NPC.type` or `Type`.
  * The value is the `NPCPortraitProvider`. All vanilla NPCs use `NPCID.Sets.PrioritizedPortrait()` which allows for multiple portraits with conditions. Though, `NPCID.Sets.BasicPortrait(string texturePath)` could be used instead for only supporting a single portrait.
* Using `NPCID.Sets.PrioritizedPortrait()` allows for assigning multiple portraits with conditions.
  * Use `.With(SelectionCondition condition, NPCPortraitProvider portrait)` to add a portrait with a condition (see below for more details).
  * Use `.Default(NPCPortraitProvider portrait)` for the default portrait if none of the other conditional portraits were met.
* Vanilla has two SelectionConditions predefined:
  * `NPCID.Sets.ShimmeredPortraitCondition` will return true if the NPC is `NPC.IsShimmerVariant`.
  * `NPCID.Sets.VariantPortraitCondition(int variantIndex)` will return true if the NPC is `NPC.townNpcVariationIndex == varientIndex`. Used for the Town Pets and Town Slimes.
  * Delegating a method of bool type or creating a `Func<bool>` (or lambda expression `() => ...`) can be used to create a different condition.
    * For example, the Zoologist uses `() => NPCID.Sets.ShimmeredPortraitCondition() && NPC.ShouldBestiaryGirlBeLycantrope()` for one of its conditions.
* `NPCID.Sets.BasicPortrait(string texturePath)` is what vanilla uses to define the texture path of the portrait.
  * `texturePath` is the path to the portrait texture.
    * Example: `"ExampleMod/Content/NPCs/ExamplePerson_Portrait"`
	* If the portrait texture is placed in the same folder as the NPC's texture name prefixed with the class name of the NPC, something like `$"{Texture}_Portrait"` can be used for convenience.
	* BasicPortrait has many optional parameters for choosing a specific frame out of a texture sheet if the modder would desire combining all of the portraits into one big texture. This is just for choosing a specific frame to show; not for animation.

Full example:
```cs
// In the ModNPC's SetStaticDefaults()
// This example assumes the portraits are in the same folder as the NPC and are named NPCClassName_Portrait and NPCClassName_Shimmer_Portrait

// Here we define which portrait to use for the Town NPC when the portrait style setting is set to detailed.
NPCID.Sets.NPCPortraits.Add(Type, NPCID.Sets.PrioritizedPortrait()
	.With(NPCID.Sets.ShimmeredPortraitCondition, NPCID.Sets.BasicPortrait($"{Texture}_Shimmer_Portrait")) // This is the portrait to use while the Town NPC is shimmered.
	.Default(NPCID.Sets.BasicPortrait($"{Texture}_Portrait"))); // Default portrait to use (not shimmered).
```

If the Town NPC has many variants, such as a Town Pet, something like this can be used:
```cs
// Here we define which portrait to use for the Town NPC when the portrait style setting is set to detailed.
NPCID.Sets.NPCPortraits.Add(Type, NPCID.Sets.PrioritizedPortrait()
	.With(NPCID.Sets.VariantPortraitCondition(0), NPCID.Sets.BasicPortrait($"{Texture}_Portrait")) // Each variant of the NPC gets its own portrait.
	.With(NPCID.Sets.VariantPortraitCondition(1), NPCID.Sets.BasicPortrait($"{Texture}_1_Portrait"))
	.With(NPCID.Sets.VariantPortraitCondition(2), NPCID.Sets.BasicPortrait($"{Texture}_2_Portrait"))
	.With(NPCID.Sets.VariantPortraitCondition(3), NPCID.Sets.BasicPortrait($"{Texture}_3_Portrait"))
	.Default(NPCID.Sets.BasicPortrait($"{Texture}_Portrait"))); // The default portrait to use.
```

#### Profile and Retro Portraits
When the portrait setting is set to profile, a close up of the NPC's normal sprite will be shown. Retro will show the NPC's sprite in its entirety. The offsets of these can be changed with two sets if needed.
* `NPCID.Sets.NPCPortraitsCloseUpOffsets.Add(int key, Vector2 value)` for the Profile setting.
  * Example: `NPCID.Sets.NPCPortraitsCloseUpOffsets.Add(Type, new Vector2(-3f, 0f))`
* `NPCID.Sets.NPCPortraitsFullBodyRetroOffsets.Add(int key, Vector2 value)` for the Retro setting.
  * Example: `NPCID.Sets.NPCPortraitsFullBodyRetroOffsets.Add(Type, new Vector2(0f, 0f));`
  
Note: As stated above, if a detailed portrait is not provided, the profile portrait will display if the user has the detailed portrait setting enabled.

## Other Changes

* Fishing power bonus now applies to any chair, not just toilets.
* `ICameraModifier` now has a `IsAScreenShake` property to support the user's screen shake accessibility setting (`Main.UseScreenShake`). Update your `ICameraModifier` and other camera movements to support `Main.UseScreenShake`.
* `Main.sign` length changed from 1000 to 32000.
* Shaders no longer need to declare every possible input, missing inputs will be ignored now.
* Dungeon generation has changed. Multiple dungeons can now generate under some secret seeds. Most dungeon related fields that used to be static fields in `Terraria.WorldBuilding.GenVars` are now instance fields in `Terraria.GameContent.Generation.Dungeon.DungeonGenVars`, accessed through the `GenVars.CurrentDungeonGenVars` property to access the data for the currently generating dungeon index.
  * For example: `GenVars.dungeonSide` -> `GenVars.CurrentDungeonGenVars.dungeonSide`. Many of the fields have been renamed or have changed meaning, it would be wise to study the decompiled code if in doubt about any of the changes.

## Renamed, Moved, or Removed Members

### Static Methods

* 💀: `Main.GetPlayerArmPosition` now has a `Player` parameter.
* ⚙️: `Utils.PlotTileArea` -> `Utils.FloodFillTile`. No longer returns `bool` and parameters are now `Point point, float maxDist, TileActionAttempt plot` instead of `int x, int y, TileActionAttempt plot`.
* 🤖: `WorldGen.CheckTight` -> `WorldGen.CheckStalactite`

### Static Fields / Constants / Properties

All classes are in the `Terraria` or `Terraria.ID` namespaces unless otherwise indicated.

* ⚙️: `BuffID.Sets.BasicMountData` removed. Replace with `BuffID.Sets.MountType[Type] = ModContent.MountType<MyMount>();`.
* 🤖: `BuffID.Sets.LongerExpertDebuff` -> `BuffID.Sets.BuffTimeIsExtendedWithGameDifficulty`
* 💀: `Chest.maxItems` is no longer static.
* 🤖: `GoreID.Sets.LiquidDroplet` -> `GoreID.Sets.IsDrip`
* 🤖: `ImmunityCooldownID.Bosses` -> `ImmunityCooldownID.BossNoCheese`
* ⚙️: `ItemID.Sets.ItemSpawnDecaySpeed` removed. No longer used.
* 🤖: `ItemID.Sets.SortingPriorityBossSpawns` -> `ItemID.Sets.SortingPriorityMiscImportants`
* 🤖: `ItemID.Sets.BonusAttackSpeedMultiplier` -> `ItemID.Sets.BonusMeleeSpeedMultiplier`
* ⚙️: `Main.ActiveItems` now iterates over `WorldItem` instead of `Item`.
* ⚙️: `Main.GameModeInfo` removed. 
  * 🤖: `Main.GameModeInfo.IsJourneyMode` -> `Main.IsJourneyMode`
* 🤖: `Main.LogicCheckScreenHeight` -> `Main.MaxWorldViewSize.Y`
* 🤖: `Main.LogicCheckScreenWidth` -> `Main.MaxWorldViewSize.X`
* ⚙️: `Main.musicBox2` removed. Use `Player.musicBox` instead.
* 🤖: `Main.popupText` -> `PopupText.popupText`
* 🤖: `Main.recBigList` -> `Main.PopsUseGrid`
* 🤖: `Main.recFastScroll` -> `Main.PipsFastScroll`
* ⚙️: `Main.item` is now `WorldItem[]` instead of `Item[]`.
* 🤖: `MessageID` entry changes: `TileSquare` -> `AreaTileChange`, `ShotAnimationAndSound` -> `ItemRotationAndAnimation`, `PlayerTeam` -> `TeamChange`, `RequestReadSign` -> `OpenSignRequest`, `ReadSign` -> `OpenSignResponse`, `AddPlayerBuff` -> `AddPlayerBuffPvP`, `PaintTile` -> `SyncTilePaintOrCoating`, `PaintWall` -> `SyncWallPaintOrCoating`, `NPCKillCountDeathTally` -> `Unused83`, `TEDisplayDollItemSync` -> `TEDisplayDollDataSync`
* ⚙️: `MountID.Sets.FacePlayersVelocity` removed. Now automatic for all minecarts.
* 🤖: `MusicId` entry changes: `Night` -> `OverworldNight`, `Title` -> `TitleClassic`, `Jungle` -> `JungleDay`, `TheHallow` -> `Hallow`, `Space` -> `SpaceNight`, `Boss4` -> `Golem`, `AltOverworldDay` -> `OverworldDayAlt`, `Ocean` -> `OceanDay`, `RainSoundEffect` -> `RainAmbience`, `Mushrooms` -> `Mushroom`, `AltUnderground` -> `UndergroundAlt`, `TheTowers` -> `LunarPillars`, `Hell` -> `Underworld`, `LunarBoss` -> `MoonLord`, `GoblinInvasion` -> `GoblinArmy`, `DayRemix` -> `OverworldDayRemix`, `MenuMusic` -> `TitleJourneysBeginningWithIntro`, `Monsoon` -> `Storm`, `JungleUnderground` -> `UndergroundJungle`, `ConsoleMenu` -> `TitleAlt`, `OtherworldlyRain` -> `OtherworldRain`, `OtherworldlyDay` -> `OtherworlddDay`, `OtherworldlyNight` -> `OtherworldNight`, `OtherworldlyUnderground` -> `OtherworldUnderground`, `OtherworldlyDesert` -> `OtherworldDesert`, `OtherworldlyOcean` -> `OtherworldOcean`, `OtherworldlyMushrooms` -> `OtherworldMushroom`, `OtherworldlyDungeon` -> `OtherworldDungeon`, `OtherworldlySpace` -> `OtherworldSpace`, `OtherworldlyUnderworld` -> `OtherworldUnderworld`, `OtherworldlySnow` -> `OtherworldSnow`, `OtherworldlyCorruption` -> `OtherworldCorruption`, `OtherworldlyUGCorrption` -> `OtherworldUndergroundCorruption`, `OtherworldlyCrimson` -> `OtherworldCrimson`, `OtherworldlyUGCrimson` -> `OtherworldUndergroundCrimson`, `OtherworldlyIce` -> `OtherworldIce`, `OtherworldlyUGHallow` -> `OtherworldUndergroundHallow`, `OtherworldlyEerie` -> `OtherworldEerie`, `OtherworldlyBoss2` -> `OtherworldBoss2`, `OtherworldlyBoss1` -> `OtherworldBoss1`, `OtherworldlyInvasion` -> `OtherworldInvasion`, `OtherworldlyTowers` -> `OtherworldLunarPillars`, `OtherworldlyLunarBoss` -> `OtherworldMoonLord`, `OtherworldlyPlantera` -> `OtherworldPlantera`, `OtherworldlyJungle` -> `OtherworldJungle`, `OtherworldlyWoF` -> `OtherworldWallOfFlesh`, `OtherworldlyHallow` -> `OtherworldHallow`, `Credits` -> `JourneysEnd`, `Shimmer` -> `Aether`
* 🤖: `NPCID.Sets.UsesNewTargetting` -> `NPCID.Sets.UsesNewTargeting`
* 🤖: `NPCID.Sets.GoldCrittersCollection` -> `NPCID.Sets.IsGoldCritter`. Also changed from `List` to typical to typical ID set.
* 🤖: `NPCID.Sets.ShouldBeCountedAsBoss` -> `NPCID.Sets.ShouldBeCountedAsBossForBestiary`
* 🤖: `NPCID.Sets.SpawnFromLastEmptySlot` -> `NPCID.Sets.SearchSpawnSlotsInReverse`
* 🤖: `ProjectileID.Web` -> `ProjectileID.WebSlingerHook`
* ⚙️: `ProjectileID.Sets.DontAttachHideToAlpha` removed. Now true by default. See `Projectile.usesOwnerLight` and `Projectile.drawLayer` for more details.
* ⚙️: `ProjectileID.Sets.HeldProjDoesNotUsePlayerGfxOffY` removed. `AI()` should use `master.RotatedRelativePoint(master.MountedCenter + ...)` to position held projectiles.
* 🤖: `ProjectileID.Sets.MinionTargettingFeature` -> `ProjectileID.Sets.MinionTargetingFeature`
* 🤖: `TileID.Sets.CountsAsWaterSource` -> `TileID.Sets.CountsAsWaterForCrafting`
* 🤖: `TileID.Sets.CountsAsHoneySource` -> `TileID.Sets.CountsAsHoneyForCrafting`
* 🤖: `TileID.Sets.CountsAsLavaSource` -> `TileID.Sets.CountsAsLavaForCrafting`
* 🤖: `TileID.Sets.CountsAsShimmerSource` -> `TileID.Sets.CountsAsShimmerForCrafting`
* ⚙️: `TileID.Sets.IsAMechanism` -> `TileID.Sets.Wiring.IsAMechanism`
  * 💀: The meaning of `IsAMechanism` has changed, it is now used for all wireable tiles and is how the items that place the tiles automatically get the "Wireable" tooltip. Add `TileID.Sets.Wiring.IsAMechanism[Type] = true;` to all tiles that do something when wired and add `TileID.Sets.Wiring.IgnoreWhenValidatingTraps[Type] = true;` to wireable tiles that aren't traps.
* 🤖: `TileID.Sets.IsATrigger` -> `TileID.Sets.Wiring.IsATrigger`
* 🤖: `TileID.Sets.InteractibleByNPCs` -> `TileID.Sets.InteractableByNPCs`
* 🤖: `TileID.Sets.Torch` -> `TileID.Sets.Torches`
* 🤖: `TileID.Sets.Campfire` -> `TileID.Sets.Campfires`
* 🤖: `WallID.Sets.Corrupt` -> `WallID.Sets.SpreadsCorruption`
* 🤖: `WallID.Sets.Crimson` -> `WallID.Sets.SpreadsCrimson`
* 🤖: `WallID.Sets.Hallow` -> `WallID.Sets.SpreadsHallow`
* 🤖: `Main.DisableIntenseVisualEffects` -> `Main.FlashyEffectsWorld`. The new field has the opposite meaning of the old field.
* 🤖: `Main.gameInactive` -> `Terraria.FocusHelper.GameplayActive`. The new field has the opposite meaning of the old field.
* 🤖: `NPC.killCount` -> `Terraria.GameContent.BannerSystem.killCount`
* 🤖: `WorldGen.gen` -> `WorldGen.isGeneratingOrLoadingWorld`

### Non-Static Methods

* 🤖: `Item.BannerToItem` -> `Terraria.GameContent.BannerSystem.BannerToItem`
* 🤖: `Item.BannerToNPC` -> `Terraria.GameContent.BannerSystem.BannerToNPC`
* 🤖: `Item.NPCtoBanner` -> `Terraria.GameContent.BannerSystem.NPCtoBanner`
* 🤖: `Item.SetDefaults` changed. The `noMatCheck` parameter has been removed.
* 🤖: `Localization.LocalizedText.CanFormatWith` -> `Localization.LocalizedText.ConditionsMetWith`
* 🤖: `Main.ShouldShowInvisibleWalls` -> `Main.ShouldShowInvisibleBlocksAndWalls`
* 🤖: `NPC.ShouldBestiaryGirlBeLycantrope` now static.
* ⚙️: `NPC.SpawnWithHigherTime` removed. No longer used.
* 🤖: `Player.IsProjectileInteractibleAndInInteractionRange` -> `Player.IsProjectileInteractableAndInInteractionRange`
* ⚙️: `Player.CheckForGoodTeleportationSpot` removed. Use `Utils.CheckForGoodTeleportationSpot` instead.
* 💀: `Player.DropItems` now has a `gemsOnly` parameter indicating a softcore or creative player that should only drop large gems.
* ⚙️: `Recipe.FindRecipes` removed. No longer used.

### Non-Static Fields / Constants / Properties

* 🤖: `Dust.noLightEmittence` -> `Dust.noLightEmittance`
* 💀: `Entity.active` removed. `Player.active`, `Projectile.active`, and `WorldItem.active` have been added.
* 🤖: `Item.netID` -> `Item.type`. `netID` has been removed.
* ⚙️: `Item` fields moved to `WorldItem`: `beingGrabbed`, `whoAmI`. Moved to `WorldItem`, relevant hooks will provide a `WorldItem` instance to use.
* 🤖: `Main.HasInteractibleObjectThatIsNotATile` -> `Main.HasInteractableObjectThatIsNotATile`
* 🤖: `Main.CurrentFrameFlags.HadAnActiveInteractibleProjectile` -> `Main.HadAnActiveInteractableProjectile`
* ⚙️: `NPC.netSkip` removed. No longer necessary when setting `life <= 0` and was never necessary when setting `active = false`.
* 🤖: `Player.adjWater` -> `Player.adjWaterSource`
* 🤖: `Player.oldAdjWater` -> `Player.oldAdjWaterSource`
* 🤖: `Player.isPettingAnimal` -> `Terraria.GameContent.PlayerPettingInfo.isPetting`. Just change `Player.isPettingAnimal` to `Player.petting.isPetting`.

### Classes
* ⚙️: `Player.RandomTeleportationAttemptSettings` is now `Utils.RandomTeleportationAttemptSettings`. Modder will need to populate all relevant new fields (`teleporteeSize,  `teleporteeVelocity`, `teleporteeGravityDirection`).

## tModLoader changes

All classes are in the `Terraria.ModLoader` or `Terraria` namespaces unless otherwise indicated.

* ⚙️: `(ModItem|GlobalItem).OnSpawn/CanStackInWorld/Update/PostUpdate/GrabRange/GrabStyle/CanPickup/OnPickup/PreDrawInWorld/PostDrawInWorld` now has a `WorldItem` parameter. For `ModItem` code, switch from `Item` to `item` to access fields on the `WorldItem`. For `GlobalItem` code, you might need to access `item.inner` to access the underlying `Item` instance if accessing a field not exposed as a getter on `WorldItem.
* ⚙️: `(ModNPC|GlobalNPC).SpawnChance` changed and renamed the parameter from `NPCSpawnInfo spawnInfo` to `NPC.Spawner spawner`.
* ⚙️: `(ModProjectile|GlobalProjectile).DrawBehind` has been removed. Set `Projectile.drawLayer` instead. 
* ⚙️: `ModProjectile.DrawHeldProjInFrontOfHeldItemAndArms` has been removed. Set `Projectile.drawLayer` to `ProjectileDrawLayerID.HeldProjOverHand` instead. 
* ⚙️: `(ModProjectile|GlobalProjectile).PreDraw/PreDrawExtras/PostDraw` now has a `Player` parameter. Use this instead of `Main.player[Projectile.owner]` to properly support rendering projectiles to custom `Player` instances, such as Mannequins.
* 🤖: `ModTile.AddToArray` is no longer used for `TileID.Sets.RoomNeeds` entries since `TileID.Sets.RoomNeeds` fields have changed to typical ID sets.
* 💀: `NPCSpawnInfo` is no longer used, it has been replaced by `NPC.Spawner` in functionality.
  * ⚙️: The following fields changed from `NPCSpawnInfo` to `NPC.Spawner`: `Sky` -> `skyMob`, `Lihzahrd` -> `ZoneLihzhardTemple`, `PlayerSafe` -> `noWorms`, `Invasion` -> `invaders`,`Water` -> `waterTile`, `Granite` -> `nearGranite`, `Marble` -> `nearMarble`, `SpiderCave` -> `spawnSpider`, `PlayerInTown` -> `spawnFriendly`, `DesertCave` -> `spawnUndergroundDesert`

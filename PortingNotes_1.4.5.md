# TODOs

Once all patches are fixed, these items need to be fixed or double checked:

- GameModeData.cs no longer exists, patches need to be redistributed
- NPCSpawnParams.gameModeData no longer exists. This was potentially used in IBestiaryInfoElement.
- NPCSpawnParams.strengthMultiplierOverride renamed to difficultyOverride. Investigate if behavior changed.
- RecipeGroupID.cs no longer exists, need to adjust documentation accordingly.
- NPCHitCount = 58 --> (and others) needs comment explaining what the value should be. Why is it 1 more when no sound 0?
- Remove all Obsolete methods, including hooks and vanilla changes.
- Doublecheck methods marked as "Unused": SwitchTilesNew, AddStructure/AddProtectedStructure
- Remove TileID.Sets.WallsMergeWith (see TileID.Sets.TruncatesWalls)
- NetDiagnosticsUI Interlocked changes, should they include modded?
- Test if https://github.com/tModLoader/tModLoader/pull/4626 is fixed in vanilla. I think you need to test with non-player chat to test properly, or maybe another player?
- ItemSourceID is now static readonly, not a const. Do any other IDs change?
- Need to update ItemID.Sets.OreDropsFromSlime with new entries.
  - Also AI_001_Slimes_GenerateItemInsideBody has additional logic now for skyblock that adjusts the drops. Investigate these and see if tmod needs more support or a TODO.
- Need to update TileID.Sets.DisableSmartInteract with new entries from BlockBecauseYouAreOverAnImportantTile.ShouldBlockSmartInteract. Remove torches/4 and update docs. Add 698, 720, 721, 725, 725, 733. (TileID.Sets.Torches checked separately.)
- We need to add ModItem.SummonPrefix(), add ModPrefix.Summon
- ShimmerTransforms.IsItemTransformLocked seems to have been split, need to verify RecipeLoader.DecraftAvailable and other logic still applies.
- Need to update TileID.Sets.DisableSmartCursor
- Update ModPylon docs to removed danger check from check listing.
- Need to add anyWire, water, anyWater, anyLava, anyHoney, anyShimmer to Tile.TML.VanillaRemapping, as well as new properly cased properties for each.
- Remove totalWeight parameter from ModifyWorldGenTasks
- WorldGenerator._seed needs to be internal again. The patch was lost
- Consider updating FlexibleTileWand.Reload
- NPCID.Sets.IsGoldCritter has been added, seems to be an exact clone of GoldCrittersCollection. Link each other in documentation?
- Move NPCID.Sets.SpawnFromLastEmptySlot docs to SearchSpawnSlotsInReverse and delete from .TML.cs
- https://github.com/tModLoader/tModLoader/pull/1675 seemed to fix a bug that is apparently now fixed in vanilla. Patches in AWorkshopPublishInfoState deleted. Verify that existing workshop publicity still correctly updates UI without requiring a click.
- RecipeGroup has changed dramatically. We'll need to adjust how modded groups merge and document the new behaviors and new ctors. The tml added methods might also be superfluous now. 
- AmmoID.SEts.IsArrow/IsBullet/IsSpecialist can be removed from .TML.cs and docs moved over.
- Mount.Dismount now has a ignoreEffect parameter, this might duplicate the skipDust variable used in MountLoader.Dismount. Adjust patches (and docs) accordingly if they should be the same. When is it set? Do modded mounts need to care about when ignoreEffect was true or false?
- Need to add `mountedPlayer.allDamage += 0.1f;` patch to `Mount.UpdateEffects` for Mounts 62 and 63 (Chillet)
- Check for new `XDamage += ` results and fix them all to use `allDamage`.
- NPC now has various spawn flags like ZoneSnow, ZoneDungeon, etc. What are they for?
- Use InitData.MaxNPCs instead of Main.maxNPCs? Or not? When to use one or the other?
- NPCLoader.BuffTownNPC will need to be reworked to facilitate new functionality. "Defeating a boss now also gives each villager a 1.5% attack speed bonus." is a new vanilla effect. Similarly the Advanced Combat Techniques increases health by 250. Dryad immortal on infectedSeed.
- Check for any remaining TML added ID sets that aren't in TML.cs files.
- BelongsToInvasionPirate now has 492, 252, 662 (492 present in NPC, but not in Main.UpdateAudio_DecideOnNewMusic. Is this an oversight or intentional?)
- VelongsToInvasionMartianMadness now has 394, 520
- BuffLoader.ReApply (NPC) logic seems changed, likely to fix desync issues. The server sync for MessageID.NPCBuffs when !quiet now happens after the reapply logic. Modded ReApply will need doc updates or maybe new parameters to properly adjust to these changes. Maybe a ref time parameter instead?
- NPC.TryAddingRepeatedBuff added. Might be useful to document and make public.
- Recipe.requiredTile no longer supports multiple tiles. Only a single crafting station is the new approach. Should we restore the old functionality? Is this necessary for the new crafting menu features?
- Zone calculations seem to have been reorganized a bit. Verify functionality of hooks (TileCountsAvailable, ResetNearbyTileEffects, UpdateSceneEffect)
- TileObjectData.addSubTileRange needs to be public. TryGetTileBounds needs docs. DrawFrameOffsets needs docs and maybe example, not sure what it is for yet.
- FileUtilities.Copy and Move no longer have an `overwrite` parameter.
- LegacyAudioSystem now has TrackLoopCounts and PlayCallbacks. They seem to involve counting how many times a specific music has looped. Investigate. Modders might be interested. Used with RainbowBoulderMusicPlayCallback
- SoundEngine.Initialize now returns the IAudioSystem. We should test if it is still necessary to show an error message for !IsAudioSupported. 1.4.5 change log claims "Terraria no longer fails to launch when it fails to detect an available audio device.", we should see if tModLoader can work without audio support too.
- Commented out SoundEngine.PlaySound methods now have a new pitchOffset optional parameter. We may need to adjust SoundStyle or SoundID entries to account for this.
- SoundPlayOverrides is also new (used in PlayTrackedSound and SoundPlayer.Play), contains an override volume. Will also need to be accounted for in our unified approach.
- tModLoaderTitleLinkButtonsTexture.png needs to be adjusted with new bluesky link
- Update https://github.com/tModLoader/tModLoader/wiki/Vanilla-Content-IDs#achievement-identifiers with new achievements
- https://github.com/tModLoader/tModLoader/pull/3500 seems to have changed ItemSourceID.PlayerDropItemCheck to ThrowItem. In 1.4.5 it was renamed to PlayerDrop and there is a new InventoryOverflow. Double check that the #3500 logic applies to the fixed 1.4.5 code. Maybe see if ThrowItem is a better name than PlayerDrop and can be fixed in Terraria, otherwise tModPorter it or double check that it is patched everywhere to use the new names.
- Also, ItemSourceID.SortingWithNoSpace has been removed.
- TileLoader.IsTileDangerous (and other methods?) now takes `Main.SceneMetrics.PerspectivePlayer` as input, not necessarily the LocalPlayer. I think this means dangersense should work when spectating. Need docs updates.
- TileLoader.IsTileSpelunkable also takes `Main.SceneMetrics.PerspectivePlayer`. These hooks now need a Player parameter, they don't currently have one.
- TileLoader.IsTileBiomeSightable as well.
- TileLoader.SpecialDraw (and other tile methods I assume) now takes a TileBatch instead of Main.spriteBatch. What does this affect? How will mods need to change? Why do some methods in TileDrawing still use Main.spriteBatch?
- DuplicationMenuToolsFilter needs: 213, 5295, 5667
- Make sure ItemFilters.MiscFilter is properly resizing.
- Does the new `uLightSource.SetValue(Vector3.Zero);` (`EffectParameter` class) do what `base.Shader.Parameters["uLightSource"]?.SetValue(Vector3.Zero);` used to do? "Allow shaders to omit parameters they don't use, no longer throw exception" (https://github.com/tModLoader/tModLoader/commit/30b2b9b1e3347a1c98ebe6924811ba5e82391dc3). Check ReflectiveArmorShaderData and other usages.
- ShaderData classes now have `if (Main.dedServ)` checks. Are these overzealous, or do we need to adjust other places or inform modders that shader code might attempt to run on servers.
- Player.voiceOverride. Currently an sbyte, might need to be an int like the other equipment slot IDs. Also an example would be nice.
- Player.faceMask, another new equipment slot field. Will need to document ArmorIDs.Face.Sets.DrawInFaceMaskLayer as well
- Player.revolverCritChanceBonus, is this a Stat? should it be a StatModifier?
- Player.adjTile patches are weird. It shouldn't be necessary to resize, they should be correct when the Player is initialized anyway.
- player.oldAdjTile has been removed. Did modders depend on this for any reason? Tracking previous frames?
- Player.coat added. It might also need and EquipType
- What is Player._pendingRefunds? Does it require modded item support?
- Player.ApplyEquipVanity now calls RefreshInfoAccsFromItemType. Is this new behavior, will our existing hooks now call things multiple times by accident?
- Player.meleeArmorPenetration is new, need to hook it up
- Player.revolverCritChanceBonus is new, need to hook it up
- Player.ApplyItemTime has been updated, we might not need as many patches?
- Integrate new `private void SetItemAnimation(int baseFrames, float multiplier)` method into our usetime hooks. Make public.
- Player.AddBuff parameters changed. Will need to adjust docs and maybe inform modders of any behavior changes.
- What does `Main.item[num].OverrideWith(theItemWeDrop);` do differently than `Main.item[num] = theItemWeDrop;`? Do we need to document or adjust how modders interact with Main.item[]?
- ProjectileLoader.CanUseGrapple can be reworked. The vanilla code now consolidates "max hooks" checks, so we should be able to make the logic for most modded grappling hooks easier by supplying those parameters to the hook or using a set.
- https://github.com/tModLoader/tModLoader/issues/4494 should be easily fixable with the new QuickGrapple code organization
- Should Ram Rune be a VanillaExtraJump? Adjust Player.isPerformingJump_DownDash if necessary. What is CanUseBootFlyingAbilities? Also double check CancelAllJumpVisualEffects logic
- TileLoader.HasWalkDust and WalkDust never pass in the Tile or Frame values. New vanilla logic sometimes checks frameX/Y, so hook could be updated with more parameters to facilitate.
- Collision.SwitchTiles `objType` parameter in Player.Update changed from 1 to 5. Why, isn't it still Player? Adjust docs accordingly after investigating.
- Rename TileID.Sets.CountsAsXSource to CountsAsXForCrafting to match CountsAsWaterForCrafting.
- Adjacent tiles are now contained in Recipe.TileCountsAs rather than hard-coded. Need to adjust TileLoader.AdjTiles hooks/docs/exampels to prioritize using Recipe.TileCountsAs
  - Restore `TileLoader.AdjTiles(this, Main.tile[j, k].type);` patch to new Player.SetAdjTile method
- Add 697 to TileID.Sets.CanPlaceNextToNonSolidTile
- ItemID.Sets.ExtractinatorMode entries seem to have changed a lot. There is also a new CanBeExtractinated set. Need to investigate and adjust things if necessary.
- Terraria added a CanConsumeConsumableItem stub method. Should it check item.consumable or only ItemLoader.ConsumeItem.
- We'll need to check all bag drops and update the drop database.
- A lot of the GetItemSettings presets have been renamed or removed. Might be something to document.
- SystemLoader.ModifyLightingBrightness might need `_negLight3` parameter.
- SystemLoader.ModifyLightingBrightness and LoaderManager.Get<WaterStylesLoader>().LightColorMultiplier might need perspectivePlayer parameter or docs to use Main.SceneMetrics.PerspectivePlayer. This is for supporting spectator mode I believe.
- There seems to be a new `flag` we should add to TileLoader.ModifyLight. It seems to determine if paint should override the native light color from a tile.
- What is ArmorIDs.Wing.Sets.AlwaysAnimated?
- BiomeConversionID.PurificationPowder (8) and Chlorophyte (9) might not match up with Terraria-added values. Need to double check where these were used against the new ID values. Chlorophyte is now either 8/9/10 and PurificationPowder is 11.
- Several MessageID were renamed. Should we keep the new vanilla name, or rename vanilla? Probably best to stay up to date with vanilla.
- Lange.CreateDialogFilter now has a checkConditions parameter. It seems that there is a new system for object substitutions. We'll need to document these and make sure they work for modded substitutions. LocalizedText.CanFormatWith usages seem to be replaced with ConditionsMetWith. Some Language.GetTextValueWith usages changed to GetTextValue but still somehow support substitutions.
- Should PlayerLoader.SyncPlayer in SyncOnePlayer be after syncing owner Projectiles?
- ItemID.ItemSpawnDecaySpeed gone. IsBasicFish added. IsQuestFish added (adjust ModItem.IsQuestFish?)
- TEDeadCellsDisplayJar and associated net messages need to be updated. Are there other new TEs?
- ItemID's #endif needs to be moved down.
- ItemID.BannerEffect changed, might need an example. Docs need to be updated for LinearCurve.
- Chest.maxItems no longer const. DefaultMaxItems/AbsoluteMaxItemsWeCanEverReachInAChestForNow added. Might need to find for loops to 40 and change if we want to support this. Chest ctors changed.
- Verify that https://github.com/tModLoader/tModLoader/issues/4383 is fixed: `if (Language.GetText("CLI.NewWorld_Command").EqualsCommand(text3))` in vanilla replaced `if (text2 == "n" || text2 == "N" || string.Equals(text2, Language.GetTextValue("CLI.NewWorld_Command"), StringComparison.CurrentCultureIgnoreCase))` fix in tmod. (New world command)
- Main.ExecuteCommand changed, doesn't have both a lowercase and raw input string anymore. Double check how capitalization is handled now, such as in CommandLoader.HandleCommand. 
- Main menu music logic now checks Main.titleMusicStyle and titleMusicStyleRandom, document and adjust ModMenu logic if necessary.
- Projectile draw code now supports an OverridePlayer. What is it used for in vanilla? (why not owner?) Hooks probably will need it. ModifyFishingLine as well
- Projectile.drawLayer will simplify Projectile.hide and ProjectileLoader.DrawBehind usage
- Move Item.instanced failed patch to WorldItem. Also noGrabDelay, beingGrabbed, timeSinceItemSpawned patch.
- Item.armorPenetration added, should we keep tml-added ArmorPenetration property?
  - "This is unused, replaced with this.ArmorPenetration." patch might be incorrect as well. Nearby switch table also changed a lot, might need to apply them elsewhere.
- Vanilla CanHavePrefixes logic changed, might be able to use it rather than tml changes.
- Item banner related methods moved to GameContent.BannerSystem. Need to move docs over.
- Item Shimmer/Update/CheckLavaDeath/MoveInWorld/GetPickedUpByMonsters_Special/FindOwner/getRect/GetShimmered/related methods have moved to World Item. Need to move docs/patches over.
- ModPylon.DrawMapIcon needs to support new vanilla options (DrawClamped when fullscreen it seems.)
- ItemSlot has new flip parameter, what is it used for? PreDrawInInventory needs flip parameter. (and itemFade parameter? And secondColor?)
- "// Sound is played on animation start #ItemTimeOnAllClients" comments around "SoundEngine.PlaySound(item6.UseSound" in MessageBuffer's `ShotAnimationAndSound` code. ShotAnimationAndSound was renamed, we might need to verify that this is still fixed in tmod.
- ApplyDifficultyAndPlayerScaling needs to be revisited.
- Need to restore rejected PopupText.rare patch logic in Item.GetPopupRarityColor
- Add 26 to TileID.Sets.AvoidedByMeteorLanding
- Check for ` = new Tile();` not gated by null checks. These will all throw exception. Change to `Tile.Clear(TileDataType.All);`
- WorldGen.StopWaterfallAmbienceAudio might be a better place for some existing patches. Need to verify save and quit stopping waterfall sounds properly.
- TileLoader.DropCritterChance could be updated with LuckyClover chance. Also Lavafly/HellButterfly chance
- TileID.Sets.SpreadsCrimson added. Need docs and possibly adjust biome spread logic. SpreadsHallow
- OreRunner changed, new parameters should make the method more useful, need docs. Also in 1.4.4 OreRunner was missing a tileMoss check, so that might affect mods when fixed.
- Update ExampleExposedGems.TileFrame to use the new CheckAndAdjustMultiDirectionalTile method. Document CheckAndAdjustMultiDirectionalTile as well.
- NewProjectile now has a NewProjectileModifier parameter. How is it used? How should modders use it? Need to add it to Docs for each overload.
- Code in Projectile claiming "// Moved to CombinedHooks.ModifyHitByProjectile" will need to be copied over again if that is still the intention. It seems that deadMansSweater is also nearby, should it also be commented?
- Projectile.bonusCritChance added. Does this do the same as tml-added Projectile.CritChance? Double check if `if (DamageType.UseStandardCritCalcs && Main.rand.Next(100) < CritChance + num10)` in new code is double applying projectile crit chance or if it is correct.
- Not sure about the order for "VanillaOnHitEffectsResume:" and other labels. SpawnHitVisuals method added in between existing patches.
- It seems like bomb damage logic has been reworked. Maybe many of our patches are no longer necessary or our explosive projectile examples need fixing. 
- CombinedHooks.CanHitNPCWithProj patches might need to be reworked, it seems like they should be able to be simplified
- Whip tag damage changed. Player.TagEffectState. Need to reapply "float num13 = ProjectileID.Sets.SummonTagDamageMultiplier[type];" patch somewhere.
- Update ProjectileID.Sets.IsInteractable: 1093, 1094, 1098
- Looks like we might want to split out the collision hitbox modification from TileCollideStyle. There is a new Projectile.GetCollisionParams method.
- Biome conversion patches will need to be fixed, or maybe the vanilla changes will make it much easier to implement. Projectile, WorldGen
- Double check new DoScrollingInInventory logic against PlayerInput.MouseInModdedUI
- Patches checking ActiveWorldFileData being null and initializing it might be superfluous now. Seems like there were some vanilla changes.
- MapRenderer class now contains what was Main.mapSectionTexture and mapTarget. Most static fields there should probably be public.
- New vanilla TooltipLine options. Need to add to docs and decide on name (is there a wiki page as well?): CommonItemTooltip.ItemUnlockedByTeammate, armorPenetration, bonusTagDamage, check for others.
- DrawBlockReplacementIcon return changed from void to bool. Does that affect how the builders toggle works? Did vanilla behavior change? New state bool in logic, and DoStatefulTickSound
- StartRain method now seems to be more controllable. Update docs accordingly. (StopRain as well)
- New guns that use Player.spaceGun? See updated `toolTipNames[numLines] = "UseMana";` patch. Look into IsSpaceGun and GetManaCost, might need updates. 4347, 4348, 514
- Double check PlayerLoader.ModifyZoom logic. Seems like there is only 1 callsite now, code was cleaned up?
- What is Main.boulderLogo? Seems like MenuLoader needs to be updated with a new vanilla menu option?
- There are new music tracks and some might have been moved. Update SceneEffectPriority enum docs and double check that they are correct for both methods.

# New Fields that might need more documentation

- UIElement.PassThroughMouseInteraction --> What does it do? How does it differ from IgnoresMouseInteraction?
- FishingAttempt.junk added --> How does this change things?
- TileEntity.RequiresUpdates (and static List<TileEntity> UpdateEntities) added -> Do all mods need to update their TEs? What is the default? What are Add and Remove methods? ModTileEntity will likely require updates.
- TileEntity.Read now has a gameversion parameter. For modded tiles, I don't think this affects anything. Vanilla TEs have updated save and load code, need to verify poses and other changes work with modded items.
- AnchorType.AllFlatHeight added -> What is it used for, what does it represent? Which tiles use it that didn't before?
- Dust.fullBright added -> Seems to force GetAlpha to return White
- Dust.HackFrame added -> Seems to retrieve a vanilla dust frame value from a dustID, similar to ExampleCustomDrawDust.OnSpawn logic.
- UserInterface.MouseCaptured -> could be useful
- Tile.ClearSlope and ClearTileAndPaint
- FlexibleTileWand is now used to place many other tiles that used to rely solely on RandomStyleRange. We should add an example of a custom FlexibleTileWand item/tile and document when to use it.
- NPCID.Sets.NPCPortraits
- NPCID.Sets: SpawnOnPlayerCanSpawnInMidairOnSkyblock, DontDropDungeonKeysOrSouls, HunterPotionFriendlyOverride, others.
- UIScrollbar.AutoHide and CanScroll
- NPC.defLifeMax
- NPC.DelBuff has new quiet parameter
- TileID.Sets.DontDrawTileSlopes.
- Player.selectedItem is not a getter property instead of a field. We might need to document selectedItemState and other related new fields.
- BuffID.Sets.AddBuffTimeAdditivelyToCap. Also need to update Mod/GlobalBuff.ReApply docs to mention AddBuffTimeAdditivelyToCap as a streamlined alternative for this use-case.
- Mount.DismountOnItemUse and MountID.Sets.CanUseHooks
- Add docs for new GetItemSettings parameters
- Main.menuChat
- Need to fix documentation for various secret and special seeds, like Main.specialSeedWorld. Need to change secret to special in most cases, and fix wiki links.

# Changes that need to be communicated to modders

- UIElement.OnDraw is now a DrawEvent not a ElementEvent (can this be tModPorted?)
- Point16.X and Y are no longer readonly
- Entity.active removed, active field added to Player, Projectile, WorldItem
- WorldItem added, represents an Item in the world
- Item no longer inherits from Entity
- Item and Entity implement IEntitySourceTarget. All Entity fields in IEntitySource are now IEntitySourceTarget
- Entity.Center changed, taking into account 0.5 from odd widths and heights now instead of using integer division
	new Vector2(position.X + (float)(width / 2), position.Y + (float)(height / 2)); 
	to 
	new Vector2(position.X + (float)width / 2f, position.Y + (float)height / 2f);
- Many of the unique vanilla hairstyles are no longer available at character creation. Set ModHair.AvailableDuringCharacterCreation to false if you wish to follow suit.
- SlimeBodyItemDropRule can be applied to any NPCID.Sets.SlimeCanContainItems now and ItemID.Sets.OreDropsFromSlime has been updated to include the alternate ores as well as Hellstone and DesertFossil
- Magic and Summon prefixes have been split into separate categories
- Pylons no longer check for "danger". ModPylon.ValidTeleportCheck_AnyDanger removed.
- Item.netID removed (tModPorter?)
- Item.SetDefaults(int Type = 0) no longer exists
- Item.SetDefaults(int Type, bool noMatCheck = false, ItemVariant variant = null) change to SetDefaults(int Type, ItemVariant variant = null) (noMatCheck parameter removed)
- UnifiedRandom.Next methods are no longer virtual
- ICameraModifier now has a IsAScreenShake property to support the user's screen shake accesibility setting (Main.UseScreenShake). Update your ICameraModifier and other camera movements to support Main.UseScreenShake.
- TownNPC can now have portraits. Use the following to implement: NPCID.Sets.NPCPortraits (todo, example)
- UIWrappedSearchBar, is it useful to modders?
- Main.sign length changed from 1000 to 32000. (Adjust docs.)
- Lots of new methods in Utils. Check if any duplicate TML.cs methods.
- Various text rendering methods have been changed or improved. Investigate new functionality and previous bug fixes.
- Player.IsAllowedToHoldItems
- All chairs give fishing bonus now, not just toilets.
- MountID.Sets.DoesNotOverrideLegFrames seems like something a lot of modded mounts might want to use. (All other new MountID.Sets sets as well)
- BuffID.Sets.BasicMountData is now just BuffID.Sets.MountType. It no longer stores a `BuffID.Sets.BuffMountData` since mounts no longer "faceLeft" or right. It now just stores the MountID directly.
- BuffID.IsAnNPCWhipDebuff, which tModLoader renamed to IsATagBuff, has changed a lot. Need to document the new behavior. Do we want to revert the name change? Also CanBeRemovedByNetMessage docs are now wrong.
- ProjectileID.Sets.HeldProjDoesNotUsePlayerGfxOffY removed. How has this been fixed? I thought it wouldn't be fixed in vanilla.
- ProjectileID.Sets.DontAttachHideToAlpha removed. What replaced it? Is it usesOwnerLight? Update Projectile.hide docs as well.
- Need to determine if hooks need to act on ModItem or WorldItem. For example: `ItemIO.SendModData(item3, writer);`
- The number3 parameter of the SyncEquipment message seems to have changed meaning. Docs needed.
- EntitySource_FishedOut will now apply to fishing item spawns instead of just npc spawns.
- Main.blackTarget removed. All other render targets are now static and WorldSceneLayerTarget instead of RenderTarget2D. (What does WorldSceneLayerTarget do?)

# tModPorter TODOs

- TileID.Sets.WallsMergeWith usages in Framing.WallFrame changed to newly added TileID.Sets.TruncatesWalls (TODO: New set contains several new vanilla tiles, does that make sense? tModPorter?)
- Main.ShouldShowInvisibleWalls changed to Main.ShouldShowInvisibleBlocksAndWalls
- ItemVariants.EverythingWorld renamed to MechdusaWorld
- NPCID.Sets.SpawnFromLastEmptySlot renamed to SearchSpawnSlotsInReverse
- NPCID.Sets.ShouldBeCountedAsBossForBestiary renamed to ShouldBeCountedAsBoss (TODO: Verify where it is now used and update docs if necessary)
- TileID.Sets.InteractibleByNPCs renamed to InteractableByNPCs
- TileID.Sets.Torch renamed to Torches
- TileID.Sets.Campfire renamed to Campfires
- TileID.Sets.IsATrigger changed to TileID.Sets.Wiring.IsATrigger
- TileID.Sets.IsAMechanism changed to TileID.Sets.Wiring.IsAMechanism. The function of the set might have changed, investigate and update docs
- NPCID.Sets.UsesNewTargetting renamed to UsesNewTargeting
- GoreID.Sets.LiquidDroplet renamed to IsDrip
- WorldGen.gen renamed to isGeneratingOrLoadingWorld
- Player.adjWater -> addWaterSource
- Player.oldAdjWater -> oldAdjWaterSource
- TileID.Sets.CountsAsWaterSource -> TileID.Sets.CountsAsWaterForCrafting (And TML added lava, shimmer, honey?)
- Player.GetItem no longer has plr parameter
- BuffID.Sets.LongerExpertDebuff -> BuffID.Sets.BuffTimeIsExtendedWithGameDifficulty. Docs remarks might also now be wrong. Also doc BuffTimeIsExtendedByDeadCellsPotionStationBuff
- MessageID.TileSquare -> AreaTileChange
- MessageID.ShotAnimationAndSound -> ItemRotationAndAnimation
- MessageID.PlayerTeam -> TeamChange
- MessageID.RequestReadSign -> OpenSignRequest
- MessageID.ReadSign -> OpenSignResponse
- MessageID.AddPlayerBuff -> AddPlayerBuffPvP
- MessageID.PaintTile -> SyncTilePaintOrCoating
- MessageID.PaintWall -> SyncWallPaintOrCoating
- MessageID.NPCKillCountDeathTally -> Unused83 (Also deprecated)
- MessageID.TEDisplayDollItemSync -> TEDisplayDollDataSync
- MountID.Sets.FacePlayersVelocity removed. Now automatic for all minecarts
- ItemID.Sets.SortingPriorityBossSpawns renamed to SortingPriorityMiscImportants
- ItemID.Sets.BonusAttackSpeedMultiplier renamed to BonusMeleeSpeedMultiplier (double check that this doesn't only apply to melee weapons. I think it isn't limited currently)

# Terraria update requests

These are simple changes that we'd like Terraria to implement, mainly to reduce large or error-prone patches.

- Use BlockBecauseYouAreOverAnImportantTile.ShouldBlockSmartInteract to make TileID.Sets.DisableSmartInteract
- Use SmartCursorHelper.IsHoveringOverAnInteractibleTileThatBlocksSmartCursor to make TileID.Sets.DisableSmartCursor
- Typo: SmartCursorHelper.IsHoveringOverAnInteractibleTileThatBlocksSmartCursor -> Interactable
- In Step_Torch, change "bool flag = !ItemID.Sets.WaterTorches[type];" to "bool flag = !ItemID.Sets.WaterTorches[providedInfo.player.BiomeTorchHoldStyle(providedInfo.item.type)];" to "Allow underwater biome torches to work" (Coral Torches don't work while in the ocean)
- de-DE/Main.json has a lot of fixes in it, should some of these fixes be brought into Terraria? Are they actually correct? Some English.
- https://github.com/tModLoader/tModLoader/commit/fced57a0725a6fabc171616487adf5166cbb89ef has several changes similar to `new MemoryStream(FileUtilities.ReadAllBytes(text, isCloudSave));` -> `FileUtilities.ReadAllBytes(text, isCloudSave).ToMemoryStream();`, to "Fix bug where BinaryIO failed to access the buffer of non-public MemoryStream". Do these need to be fixed in Terraria as well?
- https://github.com/tModLoader/tModLoader/commit/a532a537df39d3787829299e0835a3e29263fe7d has a fix for "Fix map saving if loading corrupted map file.". Check if this is still the case and suggest a fix in Terraria.
- https://github.com/tModLoader/tModLoader/commit/a532a537df39d3787829299e0835a3e29263fe7d has a fix for "Fix map saving if loading old map file.". Check if this is still the case and suggest a fix in Terraria.
- NPC.catchItem, change from short to int?
- Add `Color color = new Color(175, 75, 255);` to the start of `DoDeathEvents_CelebrateBossDeath` and use it in all the ChatHelper.BroadcastChatMessage and Main.NewText method calls. (Big patches)
- `private static readonly bool[] SafeDust` and `private static readonly bool[] SafeGore` being private and readonly, unlike every other set, is a bit odd.
- Typos: "editting" -> "editing". `UIVirtualKeyboard._edittingSign`, `UIVirtualKeyboard._edittingChest`, `PlayerInputProfile.AllowEditting`. Also, "Edittable" in input profiles.json, but probably don't fix that one since it'll break saved data.
- More simple typos: GemTree_Sappphire, garenteeNewStyle, caveOpenningSize, IsTileReplacable, DrawUnderworldBackgroudLayer, Dodgable, MakeHairsylesMenu, `_requiredObjecsForCraftingText`, pointPoisition, "GameUI.PrecentFishingPower", GemTree_Sappphire, Sillouette->Silhouette, WhoAmIToTargettingIndex, Emittence->Emittance, actuatorsLeftToConstume
- DrawUnderworldBackgroudLayer -> DrawUnderworldBackgroundLayer
- NPCInteraction.ShowExcalmation -> ShowExclamation

# Longer Patch issues:

Longer TODOs that would clutter above

- See what happens when a worldgen step fails now with the new world generator code. We removed this patch since the new code seems to handle it, but maybe we need to restore the error reporting (Utils.ShowFancyErrorMessage)
```diff
@@ -42,7 +42,24 @@
 			Main.rand = new UnifiedRandom(_seed);
 			stopwatch.Start();
 			progress.Start(pass2.Weight);
+
+			try {
-			pass2.Apply(progress, _configuration.GetPassConfiguration(pass2.Name));
+				pass2.Apply(progress, _configuration.GetPassConfiguration(pass2.Name));
+			}
+			catch(Exception e) {
+				string message = string.Join(
+					"\n",
+					Language.GetTextValue("tModLoader.WorldGenError"),
+					pass2.Name,
+					e
+				);
+				Utils.ShowFancyErrorMessage(message, 0);
+
+				// We need to shutdown the thread without it saving.
+				//TODO: Allow returning a bool to signify if it should save or not.
+				throw;
+			}
+
 			progress.End();
 			stopwatch.Reset();
 		}
```

- Need to restore this patch. The code has been moved into ItemSorting.AddSortingPrioritiesBasedOnPlayerDamage and DamageTypeSortingLayerEntry is also new:
```diff
 	{
 		Player player = Main.player[Main.myPlayer];
 		_layerList.Clear();
-		List<float> list = new List<float> {
+		List<StatModifier> list = new List<StatModifier> {
 			player.meleeDamage,
 			player.rangedDamage,
 			player.magicDamage,
 			player.minionDamage
 		};
 
+
+		/*
 		list.Sort((float x, float y) => y.CompareTo(x));
+		*/
+		list.Sort((x, y) => (y.Additive * y.Multiplicative).CompareTo(x.Additive * x.Multiplicative));
+
 		for (int i = 0; i < 5; i++) {
 			if (!_layerList.Contains(ItemSortingLayers.WeaponsMelee) && player.meleeDamage == list[0]) {
 				list.RemoveAt(0);
```

- NPC.killCount no longer exists? Need to update docs and examples.
```diff
+	/// <summary>
+	/// Indexed by BannerIDs, counts how many kills a specific enemy (or group of enemies with a shared BannerID) has in this world. Kill counts are stored on the world and are synced in multiplayer. Used by the <see cref="ItemID.TallyCounter"/> and for dropping banners. See also <see cref="ItemID.Sets.KillsToBanner"/>.
+	/// <para/> Note that Bestiary kill counts are tracked separately and per each NPC type instead of sharing a kill count with all other NPC types using the same BannerID.
+	/// </summary>
 	public static int[] killCount = new int[NPCID.Count];
```

- NPC.netUpdate field moved, need to restore this patch in a new patch after patches fixed.
```diff
+	/// <summary>
+	/// Set to true in <see cref="ModNPC.AI"/> or other suitable places to trigger the NPC syncing code (<see cref="MessageID.SyncNPC"/>). This will sync position, life, and other data about this NPC from the server to the clients. Modded data from <see cref="ModNPC.SendExtraAI(System.IO.BinaryWriter)"/> and <see cref="GlobalNPC.SendExtraAI(NPC, ModLoader.IO.BitWriter, System.IO.BinaryWriter)"/> will be included.
+	/// <para/> Use this to sync changes so that the client's NPC instances stay in sync with the server's. Only changes that are non-Deterministic on the client's side, such as random decisions or code only running on the server, need to be synced. The <see href="https://github.com/tModLoader/tModLoader/wiki/Basic-Netcode">Basic Netcode wiki page</see> goes into more details and links to examples.
+	/// <para/> As the server is in charge of NPC, changes to NPC data should only happen on the server in multiplayer.
+	/// </summary>
 	public bool netUpdate;
 	public bool netUpdate2;
```

- These buffimmune change are new and likely need to be used to update TML-specific sets.
```cs
		if (buffImmune[20]) {
			buffImmune[30] = true;
			buffImmune[375] = buffImmune[30];
		}

		if (buffImmune[69])
			buffImmune[36] = true;
```

- Move this patch to PrepareAliases, or adjust ModCommand to use new system:
```diff
@@ -56,6 +58,13 @@
 			string name = EmoteID.Search.GetName(i);
 			string key = "EmojiCommand." + name;
 			ChatManager.Commands.AddAlias(Language.GetText(key), NetworkText.FromFormattable("{0} {1}", Language.GetText("ChatCommand.Emoji_1"), Language.GetText("EmojiName." + name)));
+		}
+
+		foreach (var modEmoteBubble in EmoteBubbleLoader.emoteBubbles)
+		{
+			// Vanilla uses 2 keys, one with the name and the other with /${namekey}. Since the name is only used for the command, we can avoid that for simplicity
+			var command = new LocalizedText(modEmoteBubble.Command.Key, $"/{modEmoteBubble.Command.Value.ToLower()}");
+			ChatManager.Commands.AddAlias(command, NetworkText.FromFormattable("{0} {1}", Language.GetText("ChatCommand.Emoji_1"), modEmoteBubble.Command));
 		}
 	}
 }
```

- PatchReviewer crashes when trying to move this patch "@@ -30673,9 +32745,14 @@":
```
	private void PlaceThing_Tiles_TryPlacing(int tileToCreate, bool? overrideCanPlace, int? forcedRandom, TileObject data, int placeStyle)
	{
		bool canPlace = false;
		bool newObjectType = false;
+		PlantLoader.CheckAndInjectModSapling(tileTargetX, tileTargetY, ref tileToCreate, ref previewPlaceStyle);
		if (overrideCanPlace.HasValue) {
			canPlace = overrideCanPlace.Value;
		}
+		else if (!TileLoader.CanPlace(tileTargetX, tileTargetY, tileToCreate)) {
+			canPlace = false;
+			// TODO: CanPlace hook that allows forcing canPlace to true, rather than just preventing placement.
+		}
		else if (TileObjectData.CustomPlace(tileToCreate, placeStyle) && tileToCreate != 82 && tileToCreate != 227 && tileToCreate != 4) {
```

- Banner syncing has changed. Check what `NetManager.Instance.SendToClient(BannerSystem.NetBannersModule.WriteFullState(), whoAmI);` does and make sure it supports modded bannerids.
```diff
@@ -745,7 +803,7 @@
 						NetMessage.TrySendData(27, whoAmI, -1, null, num90);
 				}
 
-				for (int num91 = 0; num91 < 290; num91++) {
+				for (int num91 = 0; num91 < NPCLoader.NPCCount; num91++) {
 					NetMessage.TrySendData(83, whoAmI, -1, null, num91);
 				}
 
```

- Chest.chestItemSpawn/chestItemSpawn2/dresserItemSpawn removed. Document whatever replaced it.
+	/// <summary>
+	/// Associates a <see cref="TileID.Containers"/> style with the item type (<see cref="Item.type"/>) that is dropped when the chest is destroyed.
+	/// <br/> <see cref="maxChestTypes"/> elements long.
+	/// </summary>
+
+	/// <summary>
+	/// Associates a <see cref="TileID.Containers2"/> style with the item type (<see cref="Item.type"/>) that is dropped when the chest is destroyed.
+	/// <br/> <see cref="maxChestTypes2"/> elements long.
+	/// </summary>
+
+	/// <summary>
+	/// Associates a <see cref="TileID.Dressers"/> style with the item type (<see cref="Item.type"/>) that is dropped when the dresser is destroyed.
+	/// <br/> <see cref="maxDresserTypes"/> elements long.
+	/// </summary>



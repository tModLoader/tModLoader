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
- BelongsToInvasionPirate now has 492, 252, 662
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

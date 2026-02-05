# TODOs

Once all patches are fixed, these items need to be fixed or double checked:

- GameModeData.cs no longer exists, patches need to be redistributed
- RecipeGroupID.cs no longer exists, need to adjust documentation accordingly.
- NPCHitCount = 58 --> (and others) needs comment explaining what the value should be. Why is it 1 more when no sound 0?
- Remove all Obsolete methods, including hooks and vanilla changes.
- Doublecheck methods marked as "Unused": SwitchTilesNew, AddStructure/AddProtectedStructure
- Remove TileID.Sets.WallsMergeWith (see TileID.Sets.TruncatesWalls)
- NetDiagnosticsUI Interlocked changes, should they include modded?
- Test if https://github.com/tModLoader/tModLoader/pull/4626 is fixed in vanilla. I think you need to test with non-player chat to test properly, or maybe another player?
- ItemSourceID is now static readonly, not a const. Do any other IDs change?
- Need to update ItemID.Sets.OreDropsFromSlime with new entries.
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

# Changes that need to be communicated to modders

- UIElement.OnDraw is now a DrawEvent not a ElementEvent (can this be tModPorted?)
- Point16.X and Y are no longer readonly
- Entity.active removed, active field added to Player, Projectile, WorldItem
- WorldItem added, represents an Item in the world
- Item no longer inherits from Entity
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

# tModPorter TODOs

- TileID.Sets.WallsMergeWith changed to vanilla added TileID.Sets.TruncatesWalls (TODO: New set contains several new vanilla tiles, does that make sense? tModPorter?)
- Main.ShouldShowInvisibleWalls changed to Main.ShouldShowInvisibleBlocksAndWalls
- ItemVariants.EverythingWorld renamed to MechdusaWorld
- NPCID.Sets.SpawnFromLastEmptySlot renamed to SearchSpawnSlotsInReverse
- NPCID.Sets.ShouldBeCountedAsBossForBestiary renamed to ShouldBeCountedAsBoss (TODO: Verify where it is now used and update docs if necessary)

# Terraria update requests

These are simple changes that we'd like Terraria to implement, mainly to reduce large or error-prone patches.

- Add Entity to SwitchTiles -> public static bool SwitchTiles(Entity entity, Vector2 Position, int Width, int Height, Vector2 oldPosition, int objType)
- public static bool[] IgnoredByNpcStepUp = Factory.CreateBoolSet(14, 16, 18, 134, 469); // or (Tables, Anvils, WorkBenches, MythrilAnvil, Tables2)
- Use BlockBecauseYouAreOverAnImportantTile.ShouldBlockSmartInteract to make TileID.Sets.DisableSmartInteract
- Use SmartCursorHelper.IsHoveringOverAnInteractibleTileThatBlocksSmartCursor to make TileID.Sets.DisableSmartInteract
- Typo: SmartCursorHelper.IsHoveringOverAnInteractibleTileThatBlocksSmartCursor -> Interactable
- In Step_Torch, change "bool flag = !ItemID.Sets.WaterTorches[type];" to "bool flag = !ItemID.Sets.WaterTorches[providedInfo.player.BiomeTorchHoldStyle(providedInfo.item.type)];" to "Allow underwater biome torches to work" (Coral Torches don't work while in the ocean)
- Typo: UsesNewTargetting -> UsesNewTargeting

# Longer Patch issues:

Longer TODOs that would clutter above

- See what happens when a worldgen step fails now with the new world generator code. We removed this patch since the new code seems to handle it, but maybe we need to restore the error reporting (Utils.ShowFancyErrorMessage)
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
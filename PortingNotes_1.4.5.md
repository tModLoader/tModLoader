# TODOs

Once all patches are fixed, these items need to be fixed or double checked:

- NPCSpawnParams.gameModeData no longer exists. This was potentially used in IBestiaryInfoElement.
- NPCSpawnParams.strengthMultiplierOverride renamed to difficultyOverride. Investigate if behavior changed.
- NPCHitCount = 58 --> (and others) needs comment explaining what the value should be. Why is it 1 more when no sound 0?
- Remove all Obsolete methods, including hooks and vanilla changes.
- Doublecheck methods marked as "Unused": SwitchTilesNew, AddStructure/AddProtectedStructure
- Test if https://github.com/tModLoader/tModLoader/pull/4626 is fixed in vanilla. I think you need to test with non-player chat to test properly, or maybe another player?
- ItemSourceID is now static readonly, not a const. Do any other IDs change?
- We need to add ModItem.SummonPrefix(), add ModPrefix.Summon
- ShimmerTransforms.IsItemTransformLocked seems to have been split, need to verify RecipeLoader.DecraftAvailable and other logic still applies.
- Consider updating FlexibleTileWand.Reload
- https://github.com/tModLoader/tModLoader/pull/1675 seemed to fix a bug that is apparently now fixed in vanilla. Patches in AWorkshopPublishInfoState deleted. Verify that existing workshop publicity still correctly updates UI without requiring a click.
- Mount.Dismount now has a ignoreEffect parameter, this might duplicate the skipDust variable used in MountLoader.Dismount. Adjust patches (and docs) accordingly if they should be the same. When is it set? Do modded mounts need to care about when ignoreEffect was true or false?
- Check for any remaining TML added ID sets that aren't in TML.cs files.
- BuffLoader.ReApply (NPC) logic seems changed, likely to fix desync issues. The server sync for MessageID.NPCBuffs when !quiet now happens after the reapply logic. Modded ReApply will need doc updates or maybe new parameters to properly adjust to these changes. Maybe a ref time parameter instead?
- NPC.TryAddingRepeatedBuff added. Might be useful to document and make public.
- Recipe.requiredTile no longer supports multiple tiles. Only a single crafting station is the new approach. In theory it is possible to restore multiple required tiles, but we'd have to rule that recipes can show in the filtered crafting station UI if _any_ of their required tiles meet the filter.
- Zone calculations seem to have been reorganized a bit. Verify functionality of hooks (TileCountsAvailable, ResetNearbyTileEffects, UpdateSceneEffect)
- FileUtilities.Copy and Move no longer have an `overwrite` parameter.
- LegacyAudioSystem now has TrackLoopCounts and PlayCallbacks. They seem to involve counting how many times a specific music has looped. Investigate. Modders might be interested. Used with RainbowBoulderMusicPlayCallback
- SoundEngine.Initialize now returns the IAudioSystem. We should test if it is still necessary to show an error message for !IsAudioSupported. 1.4.5 change log claims "Terraria no longer fails to launch when it fails to detect an available audio device.", we should see if tModLoader can work without audio support too.
- https://github.com/tModLoader/tModLoader/pull/3500 seems to have changed ItemSourceID.PlayerDropItemCheck to ThrowItem. In 1.4.5 it was renamed to PlayerDrop and there is a new InventoryOverflow. Double check that the #3500 logic applies to the fixed 1.4.5 code. Maybe see if ThrowItem is a better name than PlayerDrop and can be fixed in Terraria, otherwise tModPorter it or double check that it is patched everywhere to use the new names.
- Also, ItemSourceID.SortingWithNoSpace has been removed.
- TileLoader.SpecialDraw (and other tile methods I assume) now takes a TileBatch instead of Main.spriteBatch. What does this affect? How will mods need to change? Why do some methods in TileDrawing still use Main.spriteBatch?
- ShaderData classes now have `if (Main.dedServ)` checks. Are these overzealous, or do we need to adjust other places or inform modders that shader code might attempt to run on servers.
- Player.voiceOverride. Currently an sbyte, might need to be an int like the other equipment slot IDs. Also an example would be nice.
- Need to document ArmorIDs.Face.Sets.DrawInFaceMaskLayer as well
- Player.revolverCritChanceBonus needs a quick test now that it has been implemented as a Projectile.CritChance bonus. Need to hookup `Item.GetVisualCritChance`
- Player.adjTile patches are weird. It shouldn't be necessary to resize, they should be correct when the Player is initialized anyway.
- Player.coat added. It might also need and EquipType
- What is Player._pendingRefunds? Does it require modded item support?
- Player.ApplyEquipVanity now calls RefreshInfoAccsFromItemType. Is this new behavior, will our existing hooks now call things multiple times by accident?
- Player.meleeArmorPenetration is new, need to hook it up
- Player.ApplyItemTime has been updated, we might not need as many patches?
- Integrate new `private void SetItemAnimation(int baseFrames, float multiplier)` method into our usetime hooks. Make public.
- What does `Main.item[num].OverrideWith(theItemWeDrop);` do differently than `Main.item[num] = theItemWeDrop;`? Do we need to document or adjust how modders interact with Main.item[]?
- ProjectileLoader.CanUseGrapple can be reworked. The vanilla code now consolidates "max hooks" checks, so we should be able to make the logic for most modded grappling hooks easier by supplying those parameters to the hook or using a set.
- https://github.com/tModLoader/tModLoader/issues/4494 should be easily fixable with the new QuickGrapple code organization
- Should Ram Rune be a VanillaExtraJump? Adjust Player.isPerformingJump_DownDash if necessary. What is CanUseBootFlyingAbilities? Also double check CancelAllJumpVisualEffects logic
- TileLoader.HasWalkDust and WalkDust never pass in the Tile or Frame values. New vanilla logic sometimes checks frameX/Y, so hook could be updated with more parameters to facilitate.
- Collision.SwitchTiles `objType` parameter in Player.Update changed from 1 to 5. Why, isn't it still Player? Adjust docs accordingly after investigating.
  - Seems to control MessageID.PlayerControls being sent but I don't know why. 1 is unused.
- Adjacent tiles are now contained in Recipe.TileCountsAs rather than hard-coded. Need to adjust TileLoader.AdjTiles hooks/docs/exampels to prioritize using Recipe.TileCountsAs
  - Restore `TileLoader.AdjTiles(this, Main.tile[j, k].type);` patch to new Player.SetAdjTile method
- Terraria added a CanConsumeConsumableItem stub method. Should it check item.consumable or only ItemLoader.ConsumeItem.
- We'll need to check all bag drops and update the drop database.
- A lot of the GetItemSettings presets have been renamed or removed. Might be something to document.
- SystemLoader.ModifyLightingBrightness might need `_negLight3` parameter.
- SystemLoader.ModifyLightingBrightness and LoaderManager.Get<WaterStylesLoader>().LightColorMultiplier might need perspectivePlayer parameter or docs to use Main.SceneMetrics.PerspectivePlayer. This is for supporting spectator mode I believe.
- There seems to be a new `flag` we should add to TileLoader.ModifyLight. It seems to determine if paint should override the native light color from a tile.
- BiomeConversionID.PurificationPowder (8) and Chlorophyte (9) might not match up with Terraria-added values. Need to double check where these were used against the new ID values. Chlorophyte is now either 8/9/10 and PurificationPowder is 11.
- Lange.CreateDialogFilter now has a checkConditions parameter. It seems that there is a new system for object substitutions. We'll need to document these and make sure they work for modded substitutions. LocalizedText.CanFormatWith usages seem to be replaced with ConditionsMetWith. Some Language.GetTextValueWith usages changed to GetTextValue but still somehow support substitutions.
- Should PlayerLoader.SyncPlayer in SyncOnePlayer be after syncing owner Projectiles?
- ItemID.ItemSpawnDecaySpeed gone.
- Vanilla Fishing drops are now declarative
  - Mods should be encouraged to use the new system, so new fishing features work.
    - "Fish now appear visually in the water while you are fishing" - (FishDropsDB.GetDisplayableDrops)
	- The fish you are catching is drawn (FishingCheck_RollItemDrop)
  - Is there any use for ModPlayer.CatchFish once mods are using the new system? It would need to be moved into FishingCheck_RollItemDrop to work with the new systems.
- TELeashedEntityAnchorWithItem (used by TECritterAnchor and TEKiteAnchor) will need to be updated to support modded items.
  - We'll likely need an ExampleKite.
  - TECritterAnchor will probably require more changes as well.
- ItemID.BannerEffect changed, might need an example. Docs need to be updated for LinearCurve.
- Chest.maxItems no longer const. DefaultMaxItems/AbsoluteMaxItemsWeCanEverReachInAChestForNow added. Might need to find for loops to 40 and change if we want to support this. Chest ctors changed.
- Verify that https://github.com/tModLoader/tModLoader/issues/4383 is fixed: `if (Language.GetText("CLI.NewWorld_Command").EqualsCommand(text3))` in vanilla replaced `if (text2 == "n" || text2 == "N" || string.Equals(text2, Language.GetTextValue("CLI.NewWorld_Command"), StringComparison.CurrentCultureIgnoreCase))` fix in tmod. (New world command)
- Main.ExecuteCommand changed, doesn't have both a lowercase and raw input string anymore. Double check how capitalization is handled now, such as in CommandLoader.HandleCommand. 
- Main menu music logic now checks Main.titleMusicStyle and titleMusicStyleRandom, document and adjust ModMenu logic if necessary.
- Item.armorPenetration added, should we keep tml-added ArmorPenetration property?
  - "This is unused, replaced with this.ArmorPenetration." patch might be incorrect as well. Nearby switch table also changed a lot, might need to apply them elsewhere.
- Vanilla CanHavePrefixes logic changed, might be able to use it rather than tml changes.
  - #StackablePrefixWeapons needs to be searched for and removed
- Item Shimmer/CheckLavaDeath/GetPickedUpByMonsters_Special/FindOwner/getRect/GetShimmered/CombineWithNearbyItems/related methods have moved to World Item. Need to move docs/patches over.
- ModPylon.DrawMapIcon needs to support new vanilla options (DrawClamped when fullscreen it seems.)
- ItemSlot has new flip parameter, what is it used for? PreDrawInInventory needs flip parameter. (and itemFade parameter? And secondColor?)
- "// Sound is played on animation start #ItemTimeOnAllClients" comments around "SoundEngine.PlaySound(item6.UseSound" in MessageBuffer's `ShotAnimationAndSound` code. ShotAnimationAndSound was renamed, we might need to verify that this is still fixed in tmod.
- ApplyDifficultyAndPlayerScaling needs to be revisited.
- WorldGen.StopWaterfallAmbienceAudio might be a better place for some existing patches. Need to verify save and quit stopping waterfall sounds properly.
- TileLoader.DropCritterChance could be updated with LuckyClover chance. Also Lavafly/HellButterfly chance
- TileID.Sets.SpreadsCrimson added. Need docs and possibly adjust biome spread logic. SpreadsHallow
- OreRunner changed, new parameters should make the method more useful, need docs. Also in 1.4.4 OreRunner was missing a tileMoss check, so that might affect mods when fixed.
- NewProjectile now has a NewProjectileModifier parameter. How is it used? How should modders use it? Need to add it to Docs for each overload.
- Code in Projectile claiming "// Moved to CombinedHooks.ModifyHitByProjectile" will need to be copied over again if that is still the intention. It seems that deadMansSweater is also nearby, should it also be commented?
- Not sure about the order for "VanillaOnHitEffectsResume:" and other labels. SpawnHitVisuals method added in between existing patches.
- It seems like bomb damage logic has been reworked. Maybe many of our patches are no longer necessary or our explosive projectile examples need fixing. 
- CombinedHooks.CanHitNPCWithProj patches might need to be reworked, it seems like they should be able to be simplified
- Looks like we might want to split out the collision hitbox modification from TileCollideStyle. There is a new Projectile.GetCollisionParams method.
- Double check new DoScrollingInInventory logic against PlayerInput.MouseInModdedUI
- Patches checking ActiveWorldFileData being null and initializing it might be superfluous now. Seems like there were some vanilla changes.
- MapRenderer class now contains what was Main.mapSectionTexture and mapTarget. Most static fields there should probably be public.
- DrawBlockReplacementIcon return changed from void to bool. Does that affect how the builders toggle works? Did vanilla behavior change? New state bool in logic, and DoStatefulTickSound
- See updated `toolTipNames[numLines] = "UseMana";` patch. Look into IsSpaceGun and GetManaCost, might need updates. 
- Double check PlayerLoader.ModifyZoom logic. Seems like there is only 1 callsite now, code was cleaned up?
- There are new music tracks and some might have been moved. Update SceneEffectPriority enum docs and double check that they are correct for both methods.
- DrawPlayer_14_2_GlassSlipperSparkles gone?
- Need to find where ProjectileLoader.DrawHeldProjInFrontOfHeldItemAndArms (ModProjectile.DrawHeldProjInFrontOfHeldItemAndArms) should go. PlayerDrawSet removed heldProjOverHand and there are new fields as well. Seems like `SelectedDrawnProjectile.drawLayer == 8` replaced it in DrawPlayer_31_ProjectileOverArm? ProjectileDrawLayerID.HeldProjOverHand exists.
- SoundID.TML: NPCHit58, NPCDeath67, NPCDeath68, Item179-199
- CreateTrackable now has maxInstances parameter, need to make sure they are applied to our changes.
- Item.CanStack has been added. ItemLoader.CanStack patches should probably be moved into it.
- Item.IsTheSameAs removed
- RefreshInfoAccsFromItemType is now being called on vanity equipment too. Does this affect any of our changes? Is any slot now being checked twice? Do we need to adjust ModAccessorySlotPlayer for the same behavior?
- `//TML: Eventide and nightglow handled by Item.useLimitPerAnimation.` comment now commenting out item 5669. Might need to make changes to that item similar to 4956
- ItemLoader.UseItemHitbox callsite useStyle == 3 needs adjustment to call hook reliably.
- clientClone changed. I think the `_clientClone` field is no longer needed, or extraneous.
- Player.nonTorch removed
- What is ApplyRapidAttackBonus?
- GameTipsDisplay patches need to be redone
- Main.OpenPlayerSelectFromNet changed how our patches can be implemented for invite joining. Need to be reimplemented.
- DrawColorCodedStringWithShadow methods no longer return Vector2 string size. Is this because of some reason? All patches in ChatManager need to be revisited.
- Paladin shield patches might have been mixed up. Double Check.
- New GetItemManaUsageDetails and ItemCheck_PayMana_X methods split mana costs into multiple methods. Should be able to remove a lot of Player.TML.cs patches and use them directly.
- Test TryDroppingSingleItem with stacks (hardcore death). Modded data should be preserved with Item.NewItem overload taking Item instance, but not sure about how that handled stack in the past.
- Vanilla now uses Player.clientCloneItem() instead of Item.Clone(). I think we can just use that instead of swapping them for CopyNetStateTo and adjust `clientCloneItem` with `NetStateVersion`, but this may need more testing.
- Recipe Changes:
  - anyX (anyWood, anySand, etc) all removed. We should no longer need to maintain those old recipe group approaches.
  - useX (useWood, ext) also removed. Same.
  - Need needTorchGodsFavor condition
  - needEverythingSeed seems to be replaced by needMechdusa. TODo: Rename Condition.ZenithWorld?
  - Recipe item consumption seems to be in another class now, patches need to be moved. GetIngredientCraftingDiscount also needs to be tweaked to work again for modded RecipeLoader.ConsumeIngredient
    - Hook needs rework to use `Recipe.RequiredItemEntry`
  - CraftViaRequest complicates `RecipeItemCreationContext.DestinationStack` I think. Removed for compile, restore if possible. OnCraftHooks also not hooked up.
    - In theory it can be restored, since the `_pendingCrafts` queue remains on the client, and changes to `Main.mouseItem` are forbidden while a craft is pending. Documentation needs to note that the craft could be refunded though, so we likely need `OnCraft` hoook to be in `CraftItem_GrantItem`. We could amend the response packet from the server to send the consumed items, at the cost of quite some bandwidth when rapid crafting
- ItemSlot flow changed a lot. AccCheck no longer exists, replaced by CanEquipAccessoryInSlot?
- DyeSwap/ModSlotDyeSwap needs new approach
- There are still a lot of places checking for TileID.ClosedDoor that need to be TileLoader.IsClosedDoor.
- Item192 uses Projectile.kiteSoundPitch. How do we do that?
- New AmmoID.Sets.IsSpecialist doesn't contain Sand anymore. Is that expected?
- Everything in NPCSpawnHelper will need to be checked against any 1.4.5 changes, as well as any new conditions that are still missing.
- TileSnapshot will need more thought to restore functionality. Commented out erroring code for now.
- Vanilla now has a NativeLibraries class, conflicting with our own.
- Modded tip logic might need changes. There can now be player creation specific tips in UICharacterCreation._tips (a GameTipsDisplay class with a different `ITipProvider`. Supporting this would require more thought.
- How do we update ProjectileID.Sets.PlayerHurtDamageIgnoresDifficultyScaling? Needs dev comment and to be updated.
- SurfaceBackgroundStylesLoader.DrawCloseBackground needs to be fixed or recreated from new vanilla logic/math
- The "#4640"/PostTileFrame fix caused the game to get stuck on settling liquids 50%, it has been commented out.
- Initialize_AlmostEverything has many new methods we'll likely need to integrate into mod reloading. We might want to see if there is a "cleaner" way than just copying over specific method calls into ModContent.Load.
  - For example: ArmorSetBonuses.BuildLookup, ItemID.Sets.PostSetupContent
  - Someone should "find all references" on each XID.Count field to find any remaining content arrays that need to be resized (CB done ItemID, TileID, WallID).
- Make a checklist of all TML hooks and have others QC each method behavior?
- ItemID.Sets.BonusAttackSpeedMultiplier renamed to BonusMeleeSpeedMultiplier. tModPorter done. (double check that this doesn't only apply to melee weapons. I think it isn't limited currently)
- Run NPCShopDatabase.Test tests.
- Player.setBonus is unused by vanilla. We will likely remove it (and comment out UpdateArmorSetsOld since it is misleading) and migrate all ExampleMod set bonuses to the new system. We'll need examples of various common set bonus setups (multiple helments, partial sets, typical head/chest/leg set, etc.)
- Check ModifyEquipTextureDraw to determine if there is any other locations where the hook needs to be applied in PlayerDrawLayers.cs
- PlayerDrawLayers.cs DrawPlayer_13_Leggings

# New Fields that might need more documentation

- UIElement.PassThroughMouseInteraction --> What does it do? How does it differ from IgnoresMouseInteraction?
- TileEntity.Read now has a gameversion parameter. For modded tiles, I don't think this affects anything. Vanilla TEs have updated save and load code, need to verify poses and other changes work with modded items.
- UserInterface.MouseCaptured -> could be useful
- FlexibleTileWand is now used to place many other tiles that used to rely solely on RandomStyleRange. We should add an example of a custom FlexibleTileWand item/tile and document when to use it.
- UIScrollbar.AutoHide and CanScroll
- NPC.defLifeMax
- NPC.DelBuff has new quiet parameter
- TileID.Sets.DontDrawTileSlopes.
- Player.selectedItem is not a getter property instead of a field. We might need to document selectedItemState and other related new fields.
- Add docs for new GetItemSettings parameters
- Main.menuChat
- Need to fix documentation for various secret and special seeds, like Main.specialSeedWorld. Need to change secret to special in most cases, and fix wiki links.

# Changes that need to be communicated to modders

- UIElement.OnDraw is now a DrawEvent not a ElementEvent (can this be tModPorted?)
- Point16.X and Y are no longer readonly
- Entity.Center changed, taking into account 0.5 from odd widths and heights now instead of using integer division
	new Vector2(position.X + (float)(width / 2), position.Y + (float)(height / 2)); 
	to 
	new Vector2(position.X + (float)width / 2f, position.Y + (float)height / 2f);
- Many of the unique vanilla hairstyles are no longer available at character creation. Set ModHair.AvailableDuringCharacterCreation to false if you wish to follow suit.
- SlimeBodyItemDropRule can be applied to any NPCID.Sets.SlimeCanContainItems now and ItemID.Sets.OreDropsFromSlime has been updated to include the alternate ores as well as Hellstone and DesertFossil
- Magic and Summon prefixes have been split into separate categories
- Pylons no longer require happiness to be sold. Remove Condition.HappyEnoughToSellPylons from ModPylon.GetNPCShopEntry() to match vanilla.
- Removed Condition.HappyEnough and Condition.HappyEnoughToSellPylons. Replaced with Condition.CurrentPriceAdjustmentUnder(float priceModifier) and Condition.CurrentPriceAdjustmentOver(float priceModifier).
- Item.SetDefaults(int Type = 0) no longer exists
- Item.SetDefaults(int Type, bool noMatCheck = false, ItemVariant variant = null) change to SetDefaults(int Type, ItemVariant variant = null) (noMatCheck parameter removed)
- UnifiedRandom.Next methods are no longer virtual
- UIWrappedSearchBar, is it useful to modders?
- Various text rendering methods have been changed or improved. Investigate new functionality and previous bug fixes.
- Player.IsAllowedToHoldItems
- Need to determine if hooks need to act on ModItem or WorldItem. For example: `ItemIO.SendModData(item3, writer);`
  - `public EntityGlobalsEnumerator<TGlobal> Enumerate(IEntityWithGlobals<TGlobal> entity) => new(ForType(entity.Type), entity);` doesn't work as-is for hooks that are now WorldItem. I've changed them to `.Enumerate(item.inner)`, but I'm not positive what design we want for these hooks now. (WorldItem points to Item, but Item doesn't point to WorldItem.)
- The number3 parameter of the SyncEquipment message seems to have changed meaning. Docs needed.
- EntitySource_FishedOut will now apply to fishing item spawns instead of just npc spawns.
- Main.blackTarget removed. All other render targets are now static and WorldSceneLayerTarget instead of RenderTarget2D. (What does WorldSceneLayerTarget do?)
- ArmorIDs.Head.Sets.DrawHead renamed to ArmorIDs.Head.Sets.HidesHead and values inverted
- HardmodeAnnouncementTask is no longer a HardmodeTask.
- AssetRepository a huge mess of patches. _changeWatcher patch might need to be restored.
- LanguageManager.GetText changed, now it stores on miss. Before it didn't and we kept that behavior. Do we want the old behavior still?
- Replace Main.hasFocus with FocusHelper.AllowUIInputs (or another property)
- Player.QuickSpawnItem no longer returns an int indicating the index of the item in Main.item. This is because the spawned item can now potentially go directly into player inventory.
- Item.width and height no longer have any relation to the in-world hitbox of dropped items. All items now have a 16x16 hitbox in the game world.

# tModPorter TODOs

- ItemVariants.EverythingWorld renamed to MechdusaWorld
- Main.GameModeInfo.IsJourneyMode -> Main.IsJourneyMode
- Item.SetDefaults() -> Item.SetDefaults(0)
- Item.SetDefaults(int, bool) -> Item.SetDefaults(int)

# ExampleMod TODOs
- Verify that ExampleZombieThief still works with changes

# Terraria update requests

These are simple changes that we'd like Terraria to implement, mainly to reduce large or error-prone patches.

- In Step_Torch, change "bool flag = !ItemID.Sets.WaterTorches[type];" to "bool flag = !ItemID.Sets.WaterTorches[providedInfo.player.BiomeTorchHoldStyle(providedInfo.item.type)];" to "Allow underwater biome torches to work" (Coral Torches don't work while in the ocean)
- de-DE/Main.json has a lot of fixes in it, should some of these fixes be brought into Terraria? Are they actually correct? Some English.
- https://github.com/tModLoader/tModLoader/commit/fced57a0725a6fabc171616487adf5166cbb89ef has several changes similar to `new MemoryStream(FileUtilities.ReadAllBytes(text, isCloudSave));` -> `FileUtilities.ReadAllBytes(text, isCloudSave).ToMemoryStream();`, to "Fix bug where BinaryIO failed to access the buffer of non-public MemoryStream". Do these need to be fixed in Terraria as well?
- https://github.com/tModLoader/tModLoader/commit/a532a537df39d3787829299e0835a3e29263fe7d has a fix for "Fix map saving if loading corrupted map file.". Check if this is still the case and suggest a fix in Terraria.
- https://github.com/tModLoader/tModLoader/commit/a532a537df39d3787829299e0835a3e29263fe7d has a fix for "Fix map saving if loading old map file.". Check if this is still the case and suggest a fix in Terraria.
- NPC.catchItem, change from short to int?
- `private static readonly bool[] SafeDust` and `private static readonly bool[] SafeGore` being private and readonly, unlike every other set, is a bit odd.
- More simple typos: "GameUI.PrecentFishingPower"
- NPCInteraction.ShowExcalmation -> ShowExclamation
- PlayerDrawSet.missingHand and missingArm are the opposite of what they sound like apparently. tModLoader changes them as follows:
```diff
+	// Renames for less confusion [
-	public bool missingHand;
+	public bool armorHidesHands;
-	public bool missingArm;
+	public bool armorHidesArms;
+
+	internal bool missingHand {
+		get => !armorHidesHands;
+		set => armorHidesHands = !value;
+	}
+	internal bool missingArm {
+		get => !armorHidesArms;
+		set => armorHidesArms = !value;
+	}
+	// ]
```
- There are still several TileID.Sets.Torches missing: TorchAttack (x2), UpdateTorchLuck_ConsumeCountersAndCalculate, TryRecalculatingTorchLuck, PlaceTile

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

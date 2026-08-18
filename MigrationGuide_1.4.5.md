Note: Once 1.4.5 is stable, the contents of this file will be used to update https://github.com/tModLoader/tModLoader/wiki/Update-Migration-Guide. This page is a work in progress and modders should not consider it a complete guide for porting until tModLoader is approaching a stable release for 1.4.5.

# v2026.?? (1.4.5)

The Terraria 1.4.5 update included major changes to the source code. As such, updating tModLoader to the 1.4.5 content also included many major breaking changes. This release represents a new generation of mods as well, mods made for 1.4.4 will need to be reworked for 1.4.5, they are not backwards compatible due to how drastic many of the changes are. 

Modders should follow this guide to migrate their mod from 1.4.4 to 1.4.5. This migration guide assumes the mod has already been migrated to 1.4.4. If that is not the case, do that first. As with 1.4.3, 1.4.4 mods will continue to be available on the Steam Workshop, even after the modder has published an update on 1.4.5. Modders can continue to publish updates for their mods on 1.4.4 as well as 1.4.5 concurrently, the workshop handles this.

## Porting Prerequisites

This tModLoader release updates .NET from .NET 8 to .NET 10. Modders will need to download and install the [.NET 10 SDK](https://dotnet.microsoft.com/en-us/download). Visual Studio users will need to [update to Visual Studio 2026](https://learn.microsoft.com/en-us/visualstudio/install/update-visual-studio?view=visualstudio) as well. Visual Studio 2022 will not work anymore. Rider and Visual Studio Code users should make sure they are updated as well.

The porting process will change your source code. If you are not yet using [source code version control](https://github.com/tModLoader/tModLoader/wiki/Intermediate-Git-&-mod-management) like a GitHub repository, now might be the time to learn how to do that. If you are not ready to learn that yet, please at least make a backup of your source code.

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

Chat buttons are now registered at mod load time instead of when interacting with the NPC. Registration implicitly assigns the button to the NPC.

* `ModNPC.SetChatButtons` has been replaced with `ModNPC.RegisterChatButtons(NPCInteractionList interactions)`
* The Close, Happiness, and Housing buttons are automatically registered before RegisterChatButtons runs.
  * If the NPC is a Town Pet, the Pet button will be added, too.
  * The Close, Happiness, Housing, and Pet buttons are already predefined and can be used with `NPCInteractionDatabase.CloseButton`, etc.
* Register buttons with the supplied `interactions` parameter.
  * `interactions.Append(NPCInteraction interaction)` or `interactions.Prepend(NPCInteraction interaction)`
    * Append will add the button to the end of the list (after the Happiness and Housing buttons, too).
	* Prepend will add the button to the beginning of the list.
  * `interactions.InsertAfter(NPCInteraction interactionToRegister, NPCInteraction interactionAfter)` or `interactions.InsertBefore(NPCInteraction interactionToRegister, NPCInteraction interactionBefore)`
    * InsertAfter and InsertBefore will add the button after or before another specified button.
  * Each method returns the supplied `NPCInteraction` as an `NPCInteractionList.Entry` if caching the result is desired for use in another method.
* Use `interactions.X(NPCInteractions.Shop(string shopName = "Shop", string customTextKey = null))` to assign shops.
  * The shopName must match the shopName for the `NPCShop`.
  * Example: ModNPC calling `interactions.Append(NPCInteractions.Shop())` will register a button for the shop with full name "ModName/ModNPCName/Shop".
  * Vanilla shops can easily be added with the full name string "Terraria/Merchant/Shop". ("Decor" for the Painter's second shop).
  * Shops from other modded Town NPCs can be added with the full name string "ModName/ModNPCName/ShopName". For example: "ExampleMod/ExamplePerson/Shop".
  * Alternatively, use `NPCShopDatabase.GetShopName(NPCID, "Shop")` to get the full shop name for an NPC.
* Use `interactions.X(new NPCInteraction...)` to register other buttons.
  * Example: `interactions.Append(new NPCInteractions.Actions.CloseChat());`

Previously, `ModNPC.OnChatButtonClicked` was used to assign a shop to a button. This is no longer needed because shops are assigned directly to the button in `RegisterChatButtons`. Most modders can probably remove this hook.

Full example:
```cs
public override void RegisterChatButtons(NPCInteractionList interactions) {
	// Here is how to register chat buttons to your NPC.
	// There are many method that you can use to change the order of the buttons.
	// interactions.Append(NPCInteraction interaction)	This will add the button to the end of the list (after the Happiness and Housing buttons, too).
	// interactions.Prepend(NPCInteraction interaction)	This will add the button to the beginning of the list.
	// interactions.InsertAfter(NPCInteraction interactionToRegister, NPCInteraction interactionAfter)		This will add the button after another specified button.
	// interactions.InsertBefore(NPCInteraction interactionToRegister, NPCInteraction interactionBefore)	This will add the button before another specified button.
	
	// In this example we are registering our Shop button to before the Close button.
	// The Close button instance is provided for us for convenience.
	interactions.InsertBefore(NPCInteractions.Shop(ShopName), NPCInteractionDatabase.CloseButton); // NPCInteractions.Shop() is a helper that creates a Shop button.
	
	// Next, add the rest of our buttons before the Happiness button (which is before the Housing button).
	interactions.InsertBefore(new AwesomeifyButton(), NPCInteractionDatabase.HappinessButton); // These are custom buttons.
	interactions.InsertBefore(new UpgradeButton(), NPCInteractionDatabase.HappinessButton);
	interactions.InsertBefore(new OpenShopOnlyAvailableDuringDay(ShopName, "Day Only Shop"), NPCInteractionDatabase.HappinessButton);

	// Showcase of other things you can do
	NPCInteractionList.Entry awesomeifyButton = interactions.InsertBefore(new AwesomeifyButton(), NPCInteractionDatabase.HappinessButton); // Return the interaction instance
	interactions.InsertAfter(new OpenShopOnlyAvailableDuringDay(ShopName, "Day Only Shop"), awesomeifyButton); // Insert after the instance we saved above.
	interactions.Prepend(NPCInteractions.Shop(ShopName)); // Insert at the beginning
	interactions.Append(NPCInteractions.Shop(ShopName)); // Insert at the end (after the happiness and housing buttons, too)
	
	// Don't want a close, happiness, or housing button? Just disable it!
	interactions.Disable(NPCInteractionDatabase.HousingButton);

	// Adding existing shops from other NPCs
	interactions.Append(NPCInteractions.Shop("Terraria/Painter/Shop", "Painter Shop"));
	interactions.Append(NPCInteractions.Shop("Terraria/Painter/Decor", "Painter Decor"));
	interactions.Append(NPCInteractions.Shop("ExampleMod/ExamplePerson/Shop", "Example Person"));
}
```

`GlobalNPC.RegisterChatButtons` is a new hook that allows you to add chat buttons to other Town NPCs, including vanilla Town NPCs.

```cs
// In GlobalNPC
public override void RegisterChatButtons(NPC npc, NPCInteractionList interactions) {
	// Here we can add additional chat buttons to Town NPCs.
	if (npc.type == NPCID.Guide) {
		// Add a shop button that open the Zoologist's shop.
		// Vanilla shops can specified with "Terraria/NPCName/Shop" ("Decor" for the Painter's second shop)
		// Modded shops can be specified with "ModName/NPCName/ShopName"
		interactions.InsertBefore(NPCInteractions.Shop("Terraria/BestiaryGirl/Shop", "Shop"), NPCInteractionDatabase.CloseButton);

		// Here we are going to disable the Guide's tips button.
		// This way matches the type of the interaction and returns the first that matches or null if not found.
		// If the interaction wasn't found, nothing happens.
		NPCInteraction guideTipNPCInteraction = interactions.Interactions.OfType<NPCInteractions.Actions.GuideTip>().FirstOrDefault();
		interactions.Disable(guideTipNPCInteraction); // If the instance is null (aka not found), Disable won't do anything.

		// Alternate way: this way does the same thing, but searches the Entries instead and returns the NPCInteractionList.Entry if found.
		// NPCInteractionList.Entry guideTipEntry = interactions.Entries.Where(e => e.NPCInteraction.GetType() == typeof(NPCInteractions.Actions.GuideTip)).FirstOrDefault();
		// interactions.Disable(guideTipEntry); // If the instance is null (aka not found), Disable won't do anything.
	}
}
```

#### Creating a Custom NPCInteraction

Custom chat buttons can easily be made by creating a new class that inherits `NPCInteraction`.
* The `GetText()`, `Condition()`, and `Interact()` must be defined.
  * `GetText()` is the text of the button.
  * `Condition()` determines when this NPCInteraction can/will be shown.
	* If you want the button to always be shown, return true.
    * Since the buttons are assigned to NPCs, there is no need to set the Condition to be only for specific NPC (no need for `TalkNPCType == ...`)
  * `Interact()` is the action that happens when the button is clicked such as opening a shop.
* `ShowExcalmation` can be set to true to display a for a small exclamation point to be shown next to the button.
* `TryAddCoins(ref Color chatColor, out int coinValue)` can be used to display a coin count next to the button.
* `TextColor(ref Color chatColor, ref Color chatColorShadow, bool hoveringOverButton)` can be used to modify the color of the button.
* The properties `LocalPlayer`, `TalkNPC`, and `TalkNPCType` can be used as shortcuts for `Main.LocalPlayer`, `Main.npc[Main.LocalPlayer.talkNPC]`, and `Main.npc[Main.LocalPlayer.talkNPC].type` respectively.

Full Example:
```cs
// Here is simple example of a custom button that is labeled "Awesomeify".
public class AwesomeifyButton : NPCInteraction {
	// This is the label of the button. This points to a localization key that translates to "Awesomeify".
	public override string GetText() => Language.GetTextValue("Mods.ExampleMod.NPCs.ExamplePerson.AwesomeifyButton");

	// Here you can change when this button will show up.
	// We want the button to always be shown, so we return true.
	// Chat buttons are assigned per NPC, so we don't have to worry about specifying this button should only show for our NPC.
	// (No need to do something like this: TalkNPCType == ModContent.NPCType<ExamplePerson>();)
	public override bool Condition() => true;

	// When the button is clicked, this will run.
	public override void Interact() {
		Main.npcChatText = "Awesome!";
	}
}

// A custom interaction that inherits OpenShop with the condition changed to only show during the day.
public class OpenShopOnlyAvailableDuringDay(string shopName, string customTextKey = null) : NPCInteractions.Actions.OpenShop(shopName, customTextKey)
{
	// base.Condition() will run the base class' condition, so we don't have to copy that ourselves.
	// Then we also add && Main.dayTime to make this button only show up during the day time.
	public override bool Condition() => base.Condition() && Main.dayTime;

	public override bool ShowExcalmation => true; // Show an exclamation point next to the button.

	public override void TextColor(ref Color chatColor, ref Color chatColorShadow, bool hoveringOverButton) { // Edit the color of the button.
		chatColor = Color.Black * (Main.mouseTextColor / 255f); // * (Main.mouseTextColor / 255f) makes it pulse like the normal buttons.
		chatColorShadow = Color.LightGray;
		if (hoveringOverButton)
		{
			chatColor = Color.DarkGray * (Main.mouseTextColor / 255f);
			chatColorShadow = Color.White;
		}
	}
}
```

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

### RecipeGroup

`RecipeGroup`s have changed. 
* 🤖: There is no longer a `RecipeGroupID` class. Instead, the `RecipeGroups` class stores vanilla `RecipeGroup` objects.
* ⚙️: Creating `RecipeGroup`s has also changed. 
  * The `RecipeGroup` constructor and `RecipeGroup.RegisterGroup` have been removed. Creating and registering a `RecipeGroup` is now consolidated into the `RecipeGroup.Register` method.
  * Rather than manually using "LegacyMisc.37" to create the string "Any ItemName", the default way of creating `RecipeGroup`s will now automatically generate that string from a passed in localization key. Custom recipe group display names are also still possible with the other `RecipeGroup.Register` overload.
* There are several new vanilla `RecipeGroup`s. Replace custom groups with these vanilla groups: `Seashells`, `Stone`, `CobaltBar`, `MythrilBar`, `AdamantiteBar`, `GemCritter`, `MagicMirror`, and `Jellyfish`. If you were referencing vanilla groups by name/key, note that these no longer have spaces: `CloudBalloons`, `BlizzardBalloons, `SandstormBalloons`, `CritterGuides, and `NatureGuides`.

Examples:
```cs
// Old
CreateRecipe()
	.AddRecipeGroup(RecipeGroupID.Wood, 9)
	.Register();

RecipeGroup.recipeGroups[RecipeGroupID.Sand].ValidItems.Add(ModContent.ItemType<ExampleSandBlock>());

RecipeGroup SilverBarRecipeGroup = new RecipeGroup(
	() => $"{Language.GetTextValue("LegacyMisc.37")} {Lang.GetItemNameValue(ItemID.SilverBar)}",
	ItemID.SilverBar, ItemID.TungstenBar, ModContent.ItemType<Items.Placeable.ExampleBar>()
);
RecipeGroup.RegisterGroup(nameof(ItemID.SilverBar), SilverBarRecipeGroup);

// New
CreateRecipe()
	.AddRecipeGroup(RecipeGroups.Wood, 9)
	.Register();

RecipeGroups.Sand.ValidItems.Add(ModContent.ItemType<ExampleSandBlock>());

SilverBarRecipeGroup = RecipeGroup.Register(
	nameof(ItemID.SilverBar),
	"ItemName.SilverBar", // Lang.GetItemName(ItemID.SilverBar).Key would also work.
	ItemID.SilverBar, ItemID.TungstenBar, ModContent.ItemType<ExampleBar>()
);
```

### Whips and Tag Effects

#### Tag Effects

The way whip tag damage and effects are applied have been redone. Most functionally is no longer applied with a debuff and is instead applied with a `WhipTagEffect`. The tag effect is unique, which means effects from multiple tags cannot be stacked.
* In the whip item's SetStaticDefaults, add `ItemID.Sets.UniqueTagEffects[Type] = new WhipTagEffect() { ... };`
  * In the constructor, you can add the basic effects.
    * `TagDamage`, `CritChance`, `PlayerBuffId`, `PlayerBuffTime`, `PlayerBuffAppliedManually`, and `TagDuration` can be set.
* Additional functionality can be achieved by creating a new class that inherits `WhipTagEffect` and using that class in the SetStaticDefaults.
  * There are overrides for when the tagged NPC takes damage from from a minion, sentry, or one of their projectiles.
    * Hooks include `ModifyTaggedHit`, `ModifyProcHit`, `OnTaggedHit`, and `OnProcHit` as well as others.
	* For the Proc hooks to run, the tagged NPC must have procs enabled for itself with `TryEnableProcOnNPC`. See ExampleWhipProjectileAdvanced.OnHitNPC for how to apply that.
  * See Example Whip Advanced for some examples.
* Most buffs that were made to implement tag damage can be removed.
  * Any additional effects can be moved to a custom `WhipTagEffect` class.
  * Don't forget to remove the `AddBuff` in your whip projectile's `OnHitNPC`.

#### Whip Changes

Whip AI has changed slightly. `Projectile.ai[1]` is now used to set the swing direction.

* This means you'll need to override the item's `Shoot` and spawn the projectile manually now.
* The following code is exactly what you'll need and is what vanilla does.

```cs
public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
	// This gives some visual variance on how fast the whip swinging animation plays out.
	// This has no effect on the actual collision.
	float swingDirection = 0.6f + (0.4f * Main.rand.NextFloat());
	// 1/3 of the time, swing the whip from the bottom to top instead of from top to bottom.
	// The Dark Harvest is the only whip that doesn't have the chance of swinging from the bottom up.
	if (Main.rand.NextBool(3)) {
		swingDirection *= -2.5f;
	}
	// Set swingDirection to 1f for the pre-1.4.5 behavior.

	Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI, 0f, swingDirection);
	return false; // Return false because we've already spawned the projectile.
}
```

If you were using `Projectile.FillWhipControlPoints` to draw the projectile, you should now pass the player parameter as the third argument. Otherwise the whip won't draw correctly for mannequins.

Example Mod's whips have been updated with new examples and additional comments. See the *Example Mod* section below for the details.

See ExampleWhip, ExampleWhipAdvanced, ExampleWhipProjectile, and ExampleWhipProjectileAdvanced for more examples.

## Other Changes

* Fishing power bonus now applies to any chair, not just toilets.
* `ICameraModifier` now has a `IsAScreenShake` property to support the user's screen shake accessibility setting (`Main.UseScreenShake`). Update your `ICameraModifier` and other camera movements to support `Main.UseScreenShake`.
* `Main.sign` length changed from 1000 to 32000.
* Shaders no longer need to declare every possible input, missing inputs will be ignored now.
* Dungeon generation has changed. Multiple dungeons can now generate under some secret seeds. Most dungeon related fields that used to be static fields in `Terraria.WorldBuilding.GenVars` are now instance fields in `Terraria.GameContent.Generation.Dungeon.DungeonGenVars`, accessed through the `GenVars.CurrentDungeonGenVars` property to access the data for the currently generating dungeon index.
  * For example: `GenVars.dungeonSide` -> `GenVars.CurrentDungeonGenVars.dungeonSide`. Many of the fields have been renamed or have changed meaning, it would be wise to study the decompiled code if in doubt about any of the changes.
* Several item tooltip line changes:
  * The "SocialDesc" tooltip line no longer exists. The "Social" tooltip line (now "Equipped in social slot") will now only show for items that are neither `Item.vanity` or `Item.hasVanityEffects`.
    * `Item.hasVanityEffects` is now used. It was previously unused. Set this for accessories that have vanity effects to prevent the "Social" tooltip line from appearing and suggesting the item has no effect in vanity slots.
  * There are new tooltip lines: "Wireable", "Container", "WireTrigger", "WizardHatDuringAnniversary", "BurningBlock", "MechSummonDuringEverything", "MechdusaSummonNotDuringEverything", "PrefixArmorPenetration", "PrefixTagDamage", "SetBonusSinglePiece", "JourneyResearchTeammate", and "MissingRequirements".
  * The "SetBonus" tooltip has changed. It now automatically displays partial sets and adjusts the color to indicate if the set is complete.
  * The "SetBonusSinglePiece" tooltip shows the set bonus that would be applied if the unequipped equipment were equipped.
* Town NPCs who are homeless have a new "Housing" button that displays their "NoHome" dialogue as well as a hint on what valid housing is. The hint text can be customized through the localization file. If the key `Mods.ModName.NPCs.NPCName.HousingText.HousingRequirements` exists, it will automatically be used over the default text.
  * The `Mods.{ModName}.NPCs.{ModNPCName}.TownNPCMood.NoHome` localization key will now be generated for town pets as well.
* Town NPCs can now have specific happiness dialogue for other Town NPCs or biomes that work just like the previous `LikeNPC_Princess` and `Princess_LovesNPC`.
  * For Mod NPCs, the localization keys are scoped in `Mods.{ModName}.NPCs.{ModNPCName}.TownNPCMood`
    * `{AffectionLevel}NPC_{OtherNPCInternalName}` For specific dialogue for talking about other NPC. Other loved NPCs will use the generic `{AffectionLevel}NPC`.
      * Example: `LoveNPC_Guide` Would be a specific dialogue for talking about the Guide. 
      * Modded NPCs will need the full mod name as well. Example: `LoveNPC_ExampleMod/ExamplePerson`.
    * `{AffectionLevel}Biome_{BiomeName}` For biomes.
      * Modded Biomes will need the full name. Example: `LoveBiome_ExampleMod/ExampleSurfaceBiome`
    * `{OtherNPCInternalName}_{AffectionLevel}sNPC` For specific dialogue when another NPC is talking about your Mod NPC.
      * Example: `Guide_LovesNPC` Would be specific dialogue from the Guide when he is talking about your Mod NPC.
      * Modded NPCs will need the full mod name, too.  Example: `ExampleMod/ExamplePerson_LovesNPC`.
  * For vanilla NPCs talking about vanilla NPCs, the localization keys are scoped in `TownNPCMood_{NPCInternalName}` (outside of Mods.ModName)
    * `{AffectionLevel}NPC_{OtherVanillaNPCInternalName}`
	  * Example: `TownNPCMood_Guide.LikeNPC_BestiaryGirl` Would be a specific dialogue for when the Guide is talking about the Zoologist. 
    * `{AffectionLevel}Biome_{BiomeName}` for biomes.
  * Caveat for the Zoologist: She has two sets of happiness dialogue. A normal one and one for when she is transformed.
    * In the ModNPC, add Transformed beforehand: `Transformed.BestiaryGirl_{AffectionLevel}NPC`
	* For vanilla NPCs talking about vanilla NPCs, use BestiaryGirlTransformed: `TownNPCMood_BeastiaryGirlTransformed.{AffectionLevel}NPC_{OtherNPCInternalName}`
* `Item.maxStack` now defaults to `Item.CommonMaxStack` (9999) now instead of 1.
* `FishingAttempt.junk` now exists.
* Critters can now be leashed. While leashed, they are a new type of `Entity` called `LeashedCritter` rather than a `NPC`. This requires several changes to support.
  * Assign `TECritterAnchor.CritterPrototypes[Type]` in `ModNPC.SetStaticDefaults` to dictate the animation and AI to use while leashed. 
  * Add `ItemID.Sets.PlaceTileOnAltUse[Type] = true;` to `ModItem.SetStaticDefaults` and set `Item.createTile = TileID.CritterAnchor;` in `ModItem.SetDefaults`.
* Minion buffs can now have a counter for how many times the minion was summoned. Simply add `BuffID.Sets.BuffTextHandlers.Add(Type, new CachedProjectileCounterBuffTextHandler(ModContent.ProjectileType<YourMinionsProjectile>()));` to the buff's `SetStaticDefaults`.
  * Custom buff text handlers can be made by creating a class that inherits `IBuffTextHandler` if the vanilla `CachedProjectileCounterBuffTextHandler` doesn't suit your minion or if you want to display custom text on a buff for any other purpose.
* To support mannequins holding held projectiles, `(ModProjectile|GlobalProjectile).DisplayDollSettings(Player doll, TEDisplayDoll.DisplayDollPose pose, ref int aiStyle, ref int aiType)` has been added.
	* Many Example Mod held projectiles were updated to showcase the new hook.
	* As mentioned in the Projectile Draw Changes section, PreDraw/Draw/PostDraw code needs to be updated to use the new `player` parameter instead of using `Main.player[Projectile.owner]`.
	* `Projectile.drawLayer = ProjectileDrawLayerID.HeldProj` will likely need to be added to `SetDefaults` for the projectile to draw on the correct layer.
* `Condition.DownedEarlygameBoss` now includes King Slime and Deerclops
* Workarounds to get the correct mouse position for projectiles that draw during other draw layers are no longer required and should be removed for correct behavior. See the ExampleInteractableProjectile.cs changes.

### Example Mod

Several Example Mod examples have been updated to adapt to 1.4.5 changes and to fix other issues. If you used any of these as a template/guide, we recommend applying the same changes.

* `ExampleJoustingLanceProjectile` (https://github.com/tModLoader/tModLoader/pull/5145/changes)
  * Added `ProjectileID.Sets.AllowsContactDamageFromJellyfish` to allow for damage from a zapping jellyfish.
  * `SetDefaults`: Remove `Projectile.hide`, add `Projectile.drawLayer = ProjectileDrawLayerID.HeldProj` and `Projectile.usesOwnerLight = true`.
    * If you are using `Projectile.DefaultToSpear()`, these two will automatically be set.
  * The `rotationFactor` in `Colliding` has been updated.
  * `player.gfxOffY` in `PreDraw` has been replaced with `Projectile.gfxOffY` to fix the sprite bouncing when walking up blocks.
* `ExampleWhip`, `ExampleWhipProjectile`, `ExampleWhipAdvanced`, and `ExampleWhipProjectileAdvanced` (https://github.com/tModLoader/tModLoader/pull/5130/changes)
  * `ExampleWhip` and `ExampleWhipProjectile` have been simplified.
	* Added the `Shoot()` override to spawn the projectile manually for the swing direction. See the *Whip Changes* section above for details.
    * It no longer has the charging ability that it did before (`ExampleWhipAdvanced` still has it).
    * The draw code has been changed to be more generic.
	  * `Projectile.FillWhipControlPoints` has been updated to pass the player parameter.
      * It is almost an exact copy of the Leather Whip's drawing.
	  * It assumes each segment in the sprite are equal size, like most sprite sheets are.
	* See the *Whips and Tag Effects* section above for details on tag damage changes.
  * `ExampleWhipAdvanced` and `ExampleWhipProjectileAdvanced` have been updated.
    * Added the `Shoot()` override to spawn the projectile manually for the swing direction. See the *Whip Changes* section above for details.
    * If you weren't using `Projectile.DefaultToWhip()`, add `Projectile.drawLayer = ProjectileDrawLayerID.HeldProj` to the projectile's SetDefaults.
    * Replace `float swingTime = owner.itemAnimationMax * Projectile.MaxUpdates` with `Projectile.GetWhipSettings(Projectile, out float timeToFlyOut, out _, out _)`
	  * `Projectile.GetWhipSettings` has new functionality for when the whip is displayed on a mannequin.
    * Add `owner.MatchItemTimeToItemAnimation()` after setting the `heldProj` to match vanilla.
    * The draw code has been changed to work better for different segment amounts.
	  * `Projectile.FillWhipControlPoints` has been updated to pass the player parameter.
      * Previously, the draw code was specific for `ExampleWhipProjectileAdvanced`. Now it will work for any number of segments.
	  * Even if your whips seem to draw fine, double check the code because it is likely that the third segment of your whip wasn't being drawn.
	* See the *Whips and Tag Effects* section above for details on tag damage changes.
* `ExampleSimpleMinionBuff` now tracks how many minions were summoned with `BuffID.Sets.BuffTextHandlers`.
* `ExampleAdvancedFlailProjectile`, `ExampleCustomSwingProjectile`, `ExampleDrillProjectile`, `ExampleFailProjectile`, `ExampleHeldProjectileWeaponProjectile`, `ExampleJoustingLanceProjectile`, `ExampleShortswordProjectile`, `ExampleSpearProjectile`, `ExampleWhipProjectile`, `ExampleWhipProjectileAdvanced`, and `ExampleYoyoProjectile` have been updated to support being held by mannequins.
* `ExampleCustomUseStyleWeapon` (`ExampleCustomUseStyleGlobalItem`) has been updated to support being held by mannequins using `TEDisplayDoll.RegisterUsePose` and `player.isDisplayDollOrInanimate`.

## Renamed, Moved, or Removed Members

### Static Methods

* 💀: `Item.NewItem` methods no longer have the `bool reverseLookup` parameter. Remove it.
* 🤖: `Main.DrawWindowsIMEPanel` has been split into `Main.DrawIMEPanel` and `Main.SetIMEPanelAnchor`. `DrawIMEPanel` is automatically called each game update, so just replace  `DrawWindowsIMEPanel` calls with `SetIMEPanelAnchor` to customize the panel location.
* 💀: `Main.GetPlayerArmPosition` now has a `Player` parameter.
* ⚙️: `RecipeGroup.RegisterGroup` removed. See [RecipeGroup](#recipegroup) for more information.
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
* ⚙️: `NPCID.Sets.ImmuneToAllBuffs` was removed. Continue to use `NPCID.Sets.ImmuneToRegularBuffs` and if immunity to tags effects and tag buffs is desired, additionally set the new `NPCID.Sets.ImmuneToWhipTags`.
* 🤖: `NPCID.Sets.ShouldBeCountedAsBoss` -> `NPCID.Sets.ShouldBeCountedAsBossForBestiary`
* 🤖: `NPCID.Sets.SpawnFromLastEmptySlot` -> `NPCID.Sets.SearchSpawnSlotsInReverse`
* 🤖: `ProjectileID.Web` -> `ProjectileID.WebSlingerHook`
* ⚙️: `ProjectileID.Sets.DontAttachHideToAlpha` removed. Now true by default. See `Projectile.usesOwnerLight` and `Projectile.drawLayer` for more details.
* ⚙️: `ProjectileID.Sets.HeldProjDoesNotUsePlayerGfxOffY` removed. `AI()` should use `master.RotatedRelativePoint(master.MountedCenter + ...)` to position held projectiles.
* 🤖: `ProjectileID.Sets.MinionTargettingFeature` -> `ProjectileID.Sets.MinionTargetingFeature`
* 🤖: `RecipeGroupID` removed. `RecipeGroup` instances are now stored directly in `RecipeGroups`.
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
* 🤖: `TileID.Sets.WallsMergeWith` -> `TileID.Sets.TruncatesWalls`
* 🤖: `WallID.Sets.Corrupt` -> `WallID.Sets.SpreadsCorruption`
* 🤖: `WallID.Sets.Crimson` -> `WallID.Sets.SpreadsCrimson`
* 🤖: `WallID.Sets.Hallow` -> `WallID.Sets.SpreadsHallow`
* 🤖: `Main.DisableIntenseVisualEffects` -> `Main.FlashyEffectsWorld`. The new field has the opposite meaning of the old field.
* 💀: `Main.hasFocus` -> `Terraria.FocusHelper.AllowGameplayInputs`, most likely. Other options include `FocusHelper.AllowUIInputs`, `FocusHelper.UpdateVisualEffects`, and many more. Choose the property that best matches the intention of the code.
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
* 🤖: `Player.AddBuff` changed. The `quiet` and `foodHack` parameters have been removed.
* 🤖: `Player.GetItem` changed. The `plr` parameter has been removed.
* 🤖: `Player.IsProjectileInteractibleAndInInteractionRange` -> `Player.IsProjectileInteractableAndInInteractionRange`
* ⚙️: `Player.CheckForGoodTeleportationSpot` removed. Use `Utils.CheckForGoodTeleportationSpot` instead.
* 💀: `Player.DropItems` now has a `gemsOnly` parameter indicating a softcore or creative player that should only drop large gems.
* ⚙️: `Recipe.FindRecipes` removed. No longer used.
* ⚙️: `RecipeGroup` constructor removed. See [RecipeGroup](#recipegroup) for more information.

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

### Localization
* 💀: `<left>` and `<right>` strings that automatically localized into "Left Click" and "Right Click" have been removed.
  * Replace all `<left>` with `{InputTrigger_UseOrAttack}`.
  * Replace `<right>` with one of these three options:
    * `{InputTrigger_ToggleOrOpen}` Used by items that have right click functionality in the inventory. Example: Grab bags and items that transform into other items.
	  * `{$CommonItemTooltip.RightClickToOpen}` can still be used for "Right Click to open".
	* `{InputTrigger_InteractWithTile}` Used by things that have right click functionality with tiles or the world. Example: Placing items into a tile or right click alternate fire weapons.
	* `{InputTrigger_InteractWithTileUI}` Used by the crafting window to tell you can right click it to switch between the classic and modern styles.
  * These changes are to better support gamepad hint text and interactions. Also consider testing all items in your mod with right click interactions with a gamepad. You may need to add `ItemID.Sets.OpenableBag` or `ItemID.Sets.HasRightFire` if the gamepad instructions are missing or incorrect.

## tModLoader changes

All classes are in the `Terraria.ModLoader` or `Terraria` namespaces unless otherwise indicated.

* ⚙️: `(ModItem|GlobalItem).OnSpawn/CanStackInWorld/Update/PostUpdate/GrabRange/GrabStyle/CanPickup/OnPickup/PreDrawInWorld/PostDrawInWorld` now has a `WorldItem` parameter. For `ModItem` code, switch from `Item` to `item` to access fields on the `WorldItem`. For `GlobalItem` code, you might need to access `item.inner` to access the underlying `Item` instance if accessing a field not exposed as a getter on `WorldItem.
* 🤖: `ModNPC.SpawnChance` and `GlobalNPC.EditSpawnPool` changed and renamed the parameter from `NPCSpawnInfo spawnInfo` to `NPC.Spawner spawner`.
  * `GlobalNPC.EditSpawnFlags(NPC.Spawner spawner)` can be used to adjust player-level spawn flags before spawn rate, range, and tile selection. Use this when the changed flags should affect those later spawn calculations, such as biome, safe wall, or invasion state. For example, a modded safe-zone effect could set `spawner.noWorms = true` before vanilla spawn logic uses that flag.
  * `GlobalNPC.EditSpawnInfo(NPC.Spawner spawner)` can be used to adjust spawn information after the spawn tile has been selected and before `SpawnChance` and `EditSpawnPool` are evaluated. Use this for tile-level spawn context, such as water, granite, marble, spider cave, underground desert, spawn tile type, or spawn wall type. For example, a modded tile could set `spawner.nearGranite = true` so later spawn chance and spawn pool logic sees the chosen tile as granite-like.
* 🤖: `GlobalNPC.BuffTownNPC` has new parameters to adjust more stats and now honors `AppliesToEntity`.
* ⚙️: `(ModProjectile|GlobalProjectile).DrawBehind` has been removed. Set `Projectile.drawLayer` instead. 
* ⚙️: `ModProjectile.DrawHeldProjInFrontOfHeldItemAndArms` has been removed. Set `Projectile.drawLayer` to `ProjectileDrawLayerID.HeldProjOverHand` instead. 
* ⚙️: `(ModProjectile|GlobalProjectile).PreDraw/PreDrawExtras/PostDraw` now has a `Player` parameter. Use this instead of `Main.player[Projectile.owner]` to properly support rendering projectiles to custom `Player` instances, such as Mannequins.
* ⚙️: `ModPylon.ValidTeleportCheck_AnyDanger` and `GlobalPylon.ValidTeleportCheck_PreAnyDanger` have been removed. Pylons no longer check for danger when teleporting.
* 🤖: `ModTile.AddToArray` is no longer used for `TileID.Sets.RoomNeeds` entries since `TileID.Sets.RoomNeeds` fields have changed to typical ID sets.
* `NPCLoader.blockLoot` can now affect all drops such as coins, hearts, etherian mana, and skyblock specific drops. Before it could only affect loot table drops.
* ⚙️: `NPCSpawnInfo` is no longer used, it has been replaced by `NPC.Spawner` in functionality.
  * 🤖: The following fields changed from `NPCSpawnInfo` to `NPC.Spawner`: `DesertCave` -> `spawnUndergroundDesert`, `Granite` -> `nearGranite`, `Invasion` -> `invaders`, `Lihzahrd` -> `ZoneLihzhardTemple`, `Marble` -> `nearMarble`, `PlayerInTown` -> `spawnFriendly`, `PlayerSafe` -> `noWorms`, `Sky` -> `skyMob`, `SpiderCave` -> `spawnSpider`, `Water` -> `waterTile`
  * ⚙️: `PlanteraDefeated` removed, use `NPC.downedPlantBoss && Main.hardMode` instead.
  * 💀: `PlayerFloorX` and `PlayerFloorY` are no longer tracked by `NPC.Spawner`. Vanilla code no longer uses player floor tiles for spawning logic.
  * 💀: Using the `Player` fields such as `Player.ZoneJungle` is no longer recommended since `NPC.Spawner` contains its own version of those flags. These are used for custom spawning logic such as the dual dungeons secret seed. Failure to migrate to using these new fields will result in incorrect spawning logic.
  * There are many other new fields in `NPC.Spawner` that might prove useful, such as `hardDungeon`.
* `TooltipLine` and `DrawableTooltipLine` changes:
  * ⚙️: `(TooltipLine|DrawableTooltipLine).IsModifier` and `(TooltipLine|DrawableTooltipLine).IsModifierBad` are removed. Set `TooltipLine.Color` directly to `Terraria.ID.Colors.PrefixGood` or `Terraria.ID.Colors.PrefixBad`. If you previously needed to determine if tooltip lines were for prefixes, now check `TooltipLine.Color` against those colors or maybe see if `TooltipLine.Name` starts with "Prefix".
  * ⚙️: `TooltipLine.OverrideColor` has been renamed to `Color`. It is no longer nullable. `DrawableTooltipLine.OverrideColor` has been removed, leaving just `DrawableTooltipLine.Color`.
* ⚙️: `ModNPC.SetChatButtons` has been removed and replaced with `RegisterChatButtons(NPCInteractionList interactions)`
* ⚙️: `ModNPC.OnChatButtonClicked` changed parameters. `(bool firstButton, ref string shop)` -> `(NPCInteraction interaction)`
* ⚙️: `GlobalNPC.OnChatButtonClicked` and `GlobalNPC.PreChatButtonClicked` changed parameters. `(NPC npc, bool firstButton)` -> `(NPC npc, NPCInteraction interaction)`
* ⚙️: `ModItem.IsQuestFish` has been removed. Use `ItemID.Sets.IsQuestFish` instead.
* 🤖: Modded trees now support being grown directly with `Fertilizer` and `Infused Fertilizer`. This requires moving tree growing code in your sapling tiles. Move `WorldGen.GrowTree`/`WorldGen.GrowPalmTree` code from `ModTile.RandomUpdate` to the new `ModTile.GrowSapling` method and call `WorldGen.AttemptToGrowTreeFromSapling` in its place. See the [`ExampleSapling.cs` changes](https://github.com/tModLoader/tModLoader/pull/5229/changes) for an example.
* Localized mod display names, mod descriptions, and steam workshop descriptions are now supported. [More information](https://github.com/tModLoader/tModLoader/pull/5226).
  * Add `displayName.[languageCode] = LocalizedDisplayName` to `build.txt` to support other languages for the name of the mod.
  * Add `description_[languageCode].txt` and `description_workshop_[languageCode].txt` to support other languages for the in-game description and steam workshop description.
* `(Mod|Global)BlockType.RandomUpdate` (Tiles and Walls) now have an `underground` parameter to more easily support underground or overground-only logic and better support the "Don't dig up" special world seed behaviors. If that seed is active, tile locations technically underground (below `Main.worldSurface`) might be considered overground for random update purposes. Modders should trust the `underground` parameter rather than rely on checking `Main.worldSurface` to support the special characteristics of "Don't dig up".
* 🤖: `ModCloud.Draw` now has a `List<DrawData>` parameter replacing the `SpriteBatch` parameter. Manually drawing during this hook is no longer supported due to the new horizon visuals drawing clouds using a shader, but the new parameter allows supplying additional `DrawData` if desired.

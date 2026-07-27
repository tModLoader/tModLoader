using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Enums;
using Terraria.GameContent.Golf;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items.Tools
{
	/// <summary>
	/// An example golf club item demonstrating how to create a custom golf club.
	/// <br/>
	/// Register as a golf club via <see cref="ItemID.Sets.IsAGolfClub"/>,
	/// and specify club properties via <see cref="GetGolfClubProperties"/>.
	/// <br/>
	/// No longer requires On_ hooks (Detours) to intercept vanilla methods.
	/// </summary>
	public class ExampleGolfClub : ModItem
	{
		public override void SetStaticDefaults()
		{
			// Allow placement on weapon racks
			ItemID.Sets.CanBePlacedOnWeaponRacks[Type] = true;

			// Mark this item as a golf club so that Item.IsAGolfingItem and
			// GolfHelper.IsPlayerHoldingClub can recognize it
			ItemID.Sets.IsAGolfClub[Type] = true;
		}

		public override void SetDefaults()
		{
			// DefaultToGolfClub sets various properties common to golf clubs:
			// - width/height: item dimensions
			// - channel: shows hand animation on hover
			// - useStyle: 8 (GolfPlay)
			// - holdStyle: 4 (HoldGolfClub)
			// - shootSpeed: projectile speed
			// - shoot: projectile type (722 = GolfClubHelper)
			// - useAnimation/useTime: animation timing
			// - noMelee: disables melee damage
			Item.DefaultToGolfClub(20, 20);

			// Set rarity and shop value
			Item.SetShopValues(ItemRarityColor.Green2, Item.buyPrice(0, 10));
		}

		/// <summary>
		/// Specifies the <see cref="GolfHelper.ClubProperties"/> for this golf club.
		/// <br/>
		/// <list type="bullet">
		/// <item><term>MinimumStrength</term><description> Minimum shot power (Vector2, X=horizontal, Y=vertical)</description></item>
		/// <item><term>MaximumStrength</term><description> Maximum shot power (Vector2, X=horizontal, Y=vertical)</description></item>
		/// <item><term>RoughLandResistance</term><description> Resistance to rough landing (0=none, 1=full)</description></item>
		/// </list>
		/// <br/>
		/// Common club type references:
		/// <list type="bullet">
		/// <item><term>Iron</term><description> Minimum=(0.25,0.25), Maximum=(1,1), RoughLandResistance=0 </description></item>
		/// <item><term>Putter</term><description> Minimum=(0,0), Maximum=(0.25,0.25), RoughLandResistance=0 </description></item>
		/// <item><term>Driver</term><description> Minimum=(0.25,0.25), Maximum=(1.5,0.65), RoughLandResistance=0 </description></item>
		/// <item><term>Wedge</term><description> Minimum=(0.25,0.25), Maximum=(0.65,1.5), RoughLandResistance=1 </description></item>
		/// </list>
		/// </summary>
		/// <returns>The <see cref="ClubProperties"/> for this club, or null to use the default.</returns>
		public override GolfHelper.ClubProperties? GetGolfClubProperties()
		{
			// Return custom club properties, using Iron-like stats here
			return new GolfHelper.ClubProperties(
				minimumStrength: new Vector2(0.25f, 0.25f),
				maximumStrength: new Vector2(1.5f, 0.65f),
				roughLandResistance: 0f
			);
		}

		public override void ModifyResearchSorting(ref ContentSamples.CreativeHelper.ItemGroup itemGroup)
		{
			// Place this item in the Golf group in Journey Mode's duplication menu
			itemGroup = ContentSamples.CreativeHelper.ItemGroup.Golf;
		}
	}
}
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace ExampleMod.Items
{
	[AutoloadEquip(EquipType.Shoes)]
	public class ExampleDashAccessory : ModItem
	{
		public override void SetStaticDefaults()
		{
			Tooltip.SetDefault("This is a modded accessory." +
				"\nDouble tap in any cardinal direction to do a dash!");
		}

		public override void SetDefaults()
		{
			item.defense = 2;
			item.accessory = true;
			item.rare = ItemRarityID.Blue;
			item.value = Item.sellPrice(silver: 60);
		}

		public override void UpdateAccessory(Player player, bool hideVisual)
		{
			ExampleDashPlayer mp = player.GetModPlayer<ExampleDashPlayer>();

			//If the dash is active, decrement the timers and do other things
			if(mp.DashActive)
			{
				//This is where we set the afterimage effect.  You can replace these two lines with whatever you want to happen during the dash
				//Some examples include:  spawning dust where the player is, adding buffs, making the player immune, etc.
				//Here we take advantage of "player.eocDash" and "player.armorEffectDrawShadowEOCShield" to get the Shield of Cthulhu's afterimage effect
				player.eocDash = mp.DashTimer;
				player.armorEffectDrawShadowEOCShield = true;

				//If the dash has just started, apply the dash velocity in whatever direction we wanted to dash towards
				if(mp.DashTimer == ExampleDashPlayer.MAX_DASH_TIMER)
				{
					Vector2 newVelocity = player.velocity;
					
					//Only apply the dash velocity if our current speed in the wanted direction is less than DashVelocity
					if((mp.PressedUp && player.velocity.Y > -mp.DashVelocity) || (mp.PressedDown && player.velocity.Y < mp.DashVelocity))
					{
						//Y-velocity is set here
						//If the direction requested was Up, then we adjust the velocity to make the dash appear "faster" due to gravity being immediately in effect
						//This adjustment is roughly 1.3x the intended dash velocity
						float dashDirection = mp.PressedDown ? 1 : -1.3f;
						newVelocity.Y = dashDirection * mp.DashVelocity;
					}
					else if((mp.PressedLeft && player.velocity.X > -mp.DashVelocity) || (mp.PressedRight && player.velocity.X < mp.DashVelocity))
					{
						//X-velocity is set here
						int dashDirection = mp.PressedRight ? 1 : -1;
						newVelocity.X = dashDirection * mp.DashVelocity;
					}

					player.velocity = newVelocity;
				}

				mp.DashTimer--;
				mp.DashDelay--;

				if(mp.DashDelay == 0)
				{
					//The dash has ended.  Reset the fields
					mp.DashDelay = ExampleDashPlayer.MAX_DASH_DELAY;
					mp.DashTimer = ExampleDashPlayer.MAX_DASH_TIMER;
					mp.DashActive = false;
				}
			}
		}
	}

	public class ExampleDashPlayer : ModPlayer
	{
		//This array will be used to keep track of the previous values of "player.doubleTapCardinalTimer"
		private int[] oldCardinalTimers = new int[4];

		//These indicate what direction is what in the timer arrays used
		private readonly int Down = 0;
		private readonly int Up = 1;
		private readonly int Right = 2;
		private readonly int Left = 3;

		//Bools for if a direction was pressed in any of the cardinal directions
		public bool PressedDown = false;
		public bool PressedUp = false;
		public bool PressedRight = false;
		public bool PressedLeft = false;

		//These are properties that can be accessed from this ModPlayer which indicate if a double-tap happened in any of the cardinal directions
		public bool DoubleTapDown => PressedDown && player.doubleTapCardinalTimer[Down] < oldCardinalTimers[Down];
		public bool DoubleTapUp => PressedUp && player.doubleTapCardinalTimer[Up] < oldCardinalTimers[Up];
		public bool DoubleTapRight => PressedRight && player.doubleTapCardinalTimer[Right] < oldCardinalTimers[Right];
		public bool DoubleTapLeft => PressedLeft && player.doubleTapCardinalTimer[Left] < oldCardinalTimers[Left];

		//The fields related to the dash accessory
		public bool DashActive = false;
		public int DashDelay = MAX_DASH_DELAY;
		public int DashTimer = MAX_DASH_TIMER;
		public readonly float DashVelocity = 10f;
		//These two fields are the max values for the delay between dashes and the length of the dash in that order
		//The time is measured in frames
		public static readonly int MAX_DASH_DELAY = 50;
		public static readonly int MAX_DASH_TIMER = 35;

		public override void ResetEffects()
		{
			//ResetEffects() is called not long after player.doubleTapCardinalTimer's values have been set
			//The elements in player.doubleTapCardinalTimer are set to 15 when the player presses one of the cardinal keys, those
			// being W, A, S and D by default.

			//Check if the ExampleDashAccessory is equipped and also check against this priority:
			// If the Shield of Cthulhu, Master Ninja Gear, Tabi and/or Solar Armour set is equipped, prevent this accessory from doing its dash effect

			//The priority is used to prevent undesirable effects.
			//Without it, the player is able to use the ExampleDashAccessory's dash as well as the vanilla ones
			bool dashAccessoryEquipped = false;
			bool shieldOfCthulhuEquipped = false;
			bool masterNinjaGearEquipped = false;
			bool tabiEquipped = false;

			//This is the loop used in vanilla to update/check the not-vanity accessories
			for(int i = 3; i < 8 + player.extraAccessorySlots; i++)
			{
				Item item = player.armor[i];

				//Set the corresponding flag for each item if that item is equipped
				if(item.type == ItemType<ExampleDashAccessory>())
				{
					dashAccessoryEquipped = true;
				}
				else if(item.type == ItemID.EoCShield) {
					shieldOfCthulhuEquipped = true;
				}
				else if(item.type == ItemID.MasterNinjaGear) {
					masterNinjaGearEquipped = true;
				}
				else if(item.type == ItemID.Tabi) {
					tabiEquipped = true;
				}
			}

			//If we don't have the ExampleDashAccessory equipped or if any of the higher-priority accessories/armour mentioned above are equipped, return immediately
			//Also return if the player is currently on a mount, since dashes on a mount look weird
			if(!dashAccessoryEquipped || shieldOfCthulhuEquipped || masterNinjaGearEquipped || tabiEquipped || player.setSolar || player.mount.Active)
			{
				return;
			}

			//Set the "pressed" bools
			PressedDown = player.controlDown && player.releaseDown;
			PressedUp = player.controlUp && player.releaseUp;
			PressedRight = player.controlRight && player.releaseRight;
			PressedLeft = player.controlLeft && player.releaseLeft;

			//Finally, if any of the "DoubleTap" properties are true and the dash hasn't started, set the dash to being active
			//Examples for alternate checks are the following:
			// Right/Left only:  if(!DashActive && (DoubleTapRight || DoubleTapLeft))
			// Up only:          if(!DashActive && DoubleTapUp)
			if(!DashActive && (DoubleTapDown || DoubleTapUp || DoubleTapRight || DoubleTapLeft))
			{
				DashActive = true;

				//Here you'd be able to set an effect that happens when the dash first activates
				//Some examples include:  the larger smoke effect from the Master Ninja Gear and Tabi
			}
		}

		public override void PostUpdate()
		{
			//This hook will be used to assign the values in our oldCardinalTimers array to the player.doubleTapCardinalTimer array

			//Check if each direction was pressed.  If it was, store the old value
			//Using a helper method to shorten repetition
			UpdateTimer(Down,  PressedDown);
			UpdateTimer(Up,    PressedUp);
			UpdateTimer(Right, PressedRight);
			UpdateTimer(Left,  PressedLeft);
		}

		private void UpdateTimer(int timerIndex, bool condition)
		{
			if(condition)
			{
				//If the condition is true, update the timer
				oldCardinalTimers[timerIndex] = player.doubleTapCardinalTimer[timerIndex];
			}
			else if(player.doubleTapCardinalTimer[timerIndex] == 0) {
				//Otherwise, reset the old timer
				oldCardinalTimers[timerIndex] = 0;
			}
		}
	}
}

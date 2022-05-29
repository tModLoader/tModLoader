using System;
using Terraria;
using Terraria.ModLoader;

public class SimpleIdentifiersTest : Mod
{
	public void UseRenamedModXFields(ModType modType, ModItem modItem, ModNPC modNPC, ModPlayer modPlayer, ModProjectile modProjectile, ModMount modMount) {
		Console.Write(modType.Mod);
		Console.Write(modType.Mod != null);
		Console.Write(modItem.Item);
		Console.Write(modNPC.NPC);
		Console.Write(modPlayer.Player);
		Console.Write(modProjectile.Projectile);
		Console.Write(modMount.MountData);
	}

	public void UseRenamedModXFields(Item item, NPC npc, Projectile projectile, Mount.MountData mount) {
		Console.Write(item.ModItem);
		Console.Write(npc.ModNPC);
		Console.Write(projectile.ModProjectile);
		Console.Write(mount.ModMount);
	}

	public void UseRenamedFieldOnExpression(Func<Tile> f) {
		Console.Write(f().WallType);
		f().TileFrameX = 1;
	}

	public Mod NestedRenames(ModPlayer player) {
		return player.Player.inventory[0].ModItem.Item.ModItem.Mod;
	}

	public Mod ConditionalAccess(ModPlayer player) {
		Console.WriteLine(player?.Player.inventory);
		return player?.Player?.inventory[0]?.ModItem?.Item?.ModItem?.Mod;
	}

	public void UseRenamedMethods(ModTile modTile, Action<Func<int, int, bool>> accept) {
		modTile.RightClick(0, 0);
		modTile?.RightClick(0, 0);
		accept(modTile.RightClick);
		Console.Write(modTile.RightClick);
		((Func<int, int, bool>)modTile.RightClick)(0, 0);
		((Func<int, int, bool>)modTile.RightClick)?.Invoke(0, 0);
	}

#if COMPILE_ERROR
	public void NoChangeCompileErrors()
	{
		projectile.FieldA = 1;
		mod.FieldA = 1;
		player.FieldA = 1;
		item.FieldA = 1;
	}
#endif

	public void NoChangeOtherIdentifiers()
	{
		int item, mod, player, projectile;
		item = 1;
		mod = 2;
		player = 3;
		projectile = 4;
	}

	public void NoChangeTypeMismatch()
	{
		Dummy item = new();
		item.FieldA = 0;
	}
}
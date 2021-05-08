namespace Terraria.ModLoader
{
	/// <summary>
	/// This class allows you to modify the behavior of any wall in the game (although admittedly walls don't have much behavior). Create an instance of an overriding class then call Mod.AddGlobalWall to use this.
	/// </summary>
	public abstract class GlobalWall : GlobalBlockType
	{
		protected sealed override void Register() {
			ModTypeLookup<GlobalWall>.Register(this);
			WallLoader.globalWalls.Add(this);
		}

		public sealed override void SetupContent() => SetDefaults();

		/// <summary>
		/// Allows you to customize which items the wall at the given coordinates drops. Return false to stop the game from dropping the wall's default item (the dropType parameter). Returns true by default.
		/// </summary>
		public virtual bool Drop(int i, int j, int type, ref int dropType) {
			return true;
		}

		/// <summary>
		/// Allows you to determine what happens when the wall at the given coordinates is killed or hit with a hammer. Fail determines whether the wall is mined (whether it is killed).
		/// </summary>
		public virtual void KillWall(int i, int j, int type, ref bool fail) {
		}
	}
}

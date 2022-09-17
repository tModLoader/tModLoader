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

		public sealed override void SetupContent() => SetStaticDefaults();

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

		/// <summary>
		/// Called for every wall that updates due to being placed or being next to a wall that is changed. Return false to stop the game from carrying out its default WallFrame operations. Returns true by default.
		/// </summary>
		/// <param name="i">The x position in tile coordinates.</param>
		/// <param name="j">The y position in tile coordinates.</param>
		/// <param name="type">Type of the wall being framed</param>
		/// <param name="resetFrame"></param>
		/// <returns></returns>
		public virtual bool WallFrame(int i, int j, int type, ref bool resetFrame) {
			return true;
		}
	}
}

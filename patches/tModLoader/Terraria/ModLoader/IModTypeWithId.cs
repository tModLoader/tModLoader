namespace Terraria.ModLoader
{
	public interface IModTypeWithId : IModType
	{
		/// <summary> The Id of this mod type. </summary>
		public int Type { get; }
	}
}

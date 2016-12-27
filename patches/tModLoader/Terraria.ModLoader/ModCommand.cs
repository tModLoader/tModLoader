namespace Terraria.ModLoader
{
	public enum CommandType { Chat, Server }

	public abstract class ModCommand
	{
		public Mod Mod { get; internal set; }
		public string Name { get; internal set; }
		public abstract string Command { get; }
		public abstract CommandType Type { get; }
		public abstract string Usage { get; }
		public abstract bool Show { get; }
		public virtual bool Autoload(ref string name) => Mod.Properties.Autoload;
		public abstract bool VerifyArguments(string[] args);
		public abstract void Action(string[] args);
	}
}

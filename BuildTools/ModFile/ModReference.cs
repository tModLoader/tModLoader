using System;

namespace tModPackager.ModFile;

public class ModReference
{
	public string mod;
	public Version? target;

	public ModReference(string mod, Version? target)
	{
		this.mod = mod;
		this.target = target;
	}

	public override string ToString() => target == null ? mod : mod + '@' + target;

	public static ModReference Parse(string spec)
	{
		string[] split = spec.Split('@');
		switch (split.Length) {
			case 1:
				return new ModReference(split[0], null);
			case > 2:
				throw new Exception("Invalid mod reference: " + spec);
			default:
				try {
					return new ModReference(split[0], new Version(split[1]));
				}
				catch {
					throw new Exception("Invalid mod reference: " + spec);
				}
		}
	}
}
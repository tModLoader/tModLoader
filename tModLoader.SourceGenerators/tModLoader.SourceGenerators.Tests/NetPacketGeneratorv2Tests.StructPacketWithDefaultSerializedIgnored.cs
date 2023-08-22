﻿using GeneratorTest = tModLoader.SourceGenerators.Tests.tModLoaderSourceGeneratorVerifier<tModLoader.SourceGenerators.Tests.Adapter<tModLoader.SourceGenerators.NetPacketGeneratorv2>>.Test;

namespace tModLoader.SourceGenerators.Tests;

partial class NetPacketGeneratorv2Tests {
	[Fact]
	public async Task StructPacketWithDefaultSerializedIgnoredInt32() {
		const string CodeFile = @"
using Terraria.ModLoader;
using Terraria.ModLoader.Packets;

namespace GeneratedDemo;

public sealed class GeneratedMod : Mod {
}

[NetPacket(typeof(GeneratedMod))]
public partial struct GeneratedPacket {
	public const byte Id = GeneratedDemo.PacketRegistry.GeneratedPacketId;

	public int Default;

	[Serialize]
	public int Serialized;

	[Ignore]
	public int Ignored;

	public readonly void HandlePacket() {
	}

	public void HandleForAll() => throw new System.NotImplementedException();
}

[PacketRegistry]
static partial class PacketRegistry {
	[NetPacketIdOf(typeof(GeneratedPacket))]
	public const byte GeneratedPacketId = 0;
}
";

		const string GeneratedFile = @"// <auto-generated/>
#nullable disable
using System.IO;
using System.Runtime.CompilerServices;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace GeneratedDemo;

partial struct GeneratedPacket : Terraria.ModLoader.Packets.INetPacket {
	/// <inheritdoc cref=""Terraria.ModLoader.Packets.INetPacket.Send(int, int)""/>
	[CompilerGenerated]
	public void Send(int toClient = -1, int ignoreClient = -1) {
		var packet = ModContent.GetInstance<GeneratedDemo.GeneratedMod>().GetPacket();
		packet.Write(Id);

		var encoder_Default = default(Terraria.ModLoader.Packets.IntEncoder);
		encoder_Default.Send(packet, Default);
		var encoder_Serialized = default(Terraria.ModLoader.Packets.IntEncoder);
		encoder_Serialized.Send(packet, Serialized);

		packet.Send(toClient, ignoreClient);
	}

	/// <inheritdoc cref=""Terraria.ModLoader.Packets.INetPacket.Receive(BinaryReader, int)""/>
	[CompilerGenerated]
	public void Receive(BinaryReader reader, int sender) {
		// SetDefaults();

		var encoder_Default = default(Terraria.ModLoader.Packets.IntEncoder);
		Default = encoder_Default.Read(reader);
		var encoder_Serialized = default(Terraria.ModLoader.Packets.IntEncoder);
		Serialized = encoder_Serialized.Read(reader);

		if (Main.netMode == NetmodeID.Server) {
			Send(-1, sender);
		}
		HandlePacket();
	}
}

#nullable restore";

		await new GeneratorTest {
			TestState = {
				Sources = { CodeFile },
				GeneratedSources = {
					(typeof(Adapter<NetPacketGeneratorv2>), "GeneratedDemo.GeneratedPacket.g.cs", GeneratedFile),
				},
			},
		}.RunAsync();
	}

	[Fact]
	public async Task StructPacketWithDefaultSerializedIgnoredInt32EncodedAsIntBit7() {
		const string CodeFile = @"
using Terraria.ModLoader;
using Terraria.ModLoader.Packets;

namespace GeneratedDemo;

public sealed class GeneratedMod : Mod {
}

[NetPacket(typeof(GeneratedMod))]
public partial struct GeneratedPacket {
	public const byte Id = GeneratedDemo.PacketRegistry.GeneratedPacketId;

	[EncodedAs(typeof(Int32Bit7Encoder))]
	public int Default;

	[Serialize]
	[EncodedAs(typeof(Int32Bit7Encoder))]
	public int Serialized;

	[Ignore]
	[EncodedAs(typeof(Int32Bit7Encoder))]
	public int Ignored;

	public readonly void HandlePacket() {
	}

	public void HandleForAll() => throw new System.NotImplementedException();
}

[PacketRegistry]
static partial class PacketRegistry {
	[NetPacketIdOf(typeof(GeneratedPacket))]
	public const byte GeneratedPacketId = 0;
}
";

		const string GeneratedFile = @"// <auto-generated/>
#nullable disable
using System.IO;
using System.Runtime.CompilerServices;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace GeneratedDemo;

partial struct GeneratedPacket : Terraria.ModLoader.Packets.INetPacket {
	/// <inheritdoc cref=""Terraria.ModLoader.Packets.INetPacket.Send(int, int)""/>
	[CompilerGenerated]
	public void Send(int toClient = -1, int ignoreClient = -1) {
		var packet = ModContent.GetInstance<GeneratedDemo.GeneratedMod>().GetPacket();
		packet.Write(Id);

		var encoder_Default = default(Terraria.ModLoader.Packets.Int32Bit7Encoder);
		encoder_Default.Send(packet, Default);
		var encoder_Serialized = default(Terraria.ModLoader.Packets.Int32Bit7Encoder);
		encoder_Serialized.Send(packet, Serialized);

		packet.Send(toClient, ignoreClient);
	}

	/// <inheritdoc cref=""Terraria.ModLoader.Packets.INetPacket.Receive(BinaryReader, int)""/>
	[CompilerGenerated]
	public void Receive(BinaryReader reader, int sender) {
		// SetDefaults();

		var encoder_Default = default(Terraria.ModLoader.Packets.Int32Bit7Encoder);
		Default = encoder_Default.Read(reader);
		var encoder_Serialized = default(Terraria.ModLoader.Packets.Int32Bit7Encoder);
		Serialized = encoder_Serialized.Read(reader);

		if (Main.netMode == NetmodeID.Server) {
			Send(-1, sender);
		}
		HandlePacket();
	}
}

#nullable restore";

		await new GeneratorTest {
			TestState = {
				Sources = { CodeFile },
				GeneratedSources = {
					(typeof(Adapter<NetPacketGeneratorv2>), "GeneratedDemo.GeneratedPacket.g.cs", GeneratedFile),
				},
			},
		}.RunAsync();
	}

	[Fact]
	public async Task StructPacketWithDefaultSerializedIgnoredInt32EncodedAsByte() {
		const string CodeFile = @"
using Terraria.ModLoader;
using Terraria.ModLoader.Packets;

namespace GeneratedDemo;

public sealed class GeneratedMod : Mod {
}

[NetPacket(typeof(GeneratedMod))]
public partial struct GeneratedPacket {
	public const byte Id = GeneratedDemo.PacketRegistry.GeneratedPacketId;

	[EncodedAs(typeof(ByteEncoder))]
	public int Default;

	[Serialize]
	[EncodedAs(typeof(ByteEncoder))]
	public int Serialized;

	[Ignore]
	[EncodedAs(typeof(ByteEncoder))]
	public int Ignored;

	public readonly void HandlePacket() {
	}

	public void HandleForAll() => throw new System.NotImplementedException();
}

[PacketRegistry]
static partial class PacketRegistry {
	[NetPacketIdOf(typeof(GeneratedPacket))]
	public const byte GeneratedPacketId = 0;
}
";

		const string GeneratedFile = @"// <auto-generated/>
#nullable disable
using System.IO;
using System.Runtime.CompilerServices;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace GeneratedDemo;

partial struct GeneratedPacket : Terraria.ModLoader.Packets.INetPacket {
	/// <inheritdoc cref=""Terraria.ModLoader.Packets.INetPacket.Send(int, int)""/>
	[CompilerGenerated]
	public void Send(int toClient = -1, int ignoreClient = -1) {
		var packet = ModContent.GetInstance<GeneratedDemo.GeneratedMod>().GetPacket();
		packet.Write(Id);

		var encoder_Default = default(Terraria.ModLoader.Packets.ByteEncoder);
		encoder_Default.Send(packet, (byte)Default);
		var encoder_Serialized = default(Terraria.ModLoader.Packets.ByteEncoder);
		encoder_Serialized.Send(packet, (byte)Serialized);

		packet.Send(toClient, ignoreClient);
	}

	/// <inheritdoc cref=""Terraria.ModLoader.Packets.INetPacket.Receive(BinaryReader, int)""/>
	[CompilerGenerated]
	public void Receive(BinaryReader reader, int sender) {
		// SetDefaults();

		var encoder_Default = default(Terraria.ModLoader.Packets.ByteEncoder);
		Default = (int)encoder_Default.Read(reader);
		var encoder_Serialized = default(Terraria.ModLoader.Packets.ByteEncoder);
		Serialized = (int)encoder_Serialized.Read(reader);

		if (Main.netMode == NetmodeID.Server) {
			Send(-1, sender);
		}
		HandlePacket();
	}
}

#nullable restore";

		await new GeneratorTest {
			TestState = {
				Sources = { CodeFile },
				GeneratedSources = {
					(typeof(Adapter<NetPacketGeneratorv2>), "GeneratedDemo.GeneratedPacket.g.cs", GeneratedFile),
				},
			},
		}.RunAsync();
	}

	[Fact]
	public async Task StructPacketWithDefaultSerializedIgnoredInt32EncodedAsLong() {
		const string CodeFile = @"
using Terraria.ModLoader;
using Terraria.ModLoader.Packets;

namespace GeneratedDemo;

public sealed class GeneratedMod : Mod {
}

[NetPacket(typeof(GeneratedMod))]
public partial struct GeneratedPacket {
	public const byte Id = GeneratedDemo.PacketRegistry.GeneratedPacketId;

	[EncodedAs(typeof(LongEncoder))]
	public int Default;

	[Serialize]
	[EncodedAs(typeof(LongEncoder))]
	public int Serialized;

	[Ignore]
	[EncodedAs(typeof(LongEncoder))]
	public int Ignored;

	public readonly void HandlePacket() {
	}

	public void HandleForAll() => throw new System.NotImplementedException();
}

[PacketRegistry]
static partial class PacketRegistry {
	[NetPacketIdOf(typeof(GeneratedPacket))]
	public const byte GeneratedPacketId = 0;
}
";

		const string GeneratedFile = @"// <auto-generated/>
#nullable disable
using System.IO;
using System.Runtime.CompilerServices;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace GeneratedDemo;

partial struct GeneratedPacket : Terraria.ModLoader.Packets.INetPacket {
	/// <inheritdoc cref=""Terraria.ModLoader.Packets.INetPacket.Send(int, int)""/>
	[CompilerGenerated]
	public void Send(int toClient = -1, int ignoreClient = -1) {
		var packet = ModContent.GetInstance<GeneratedDemo.GeneratedMod>().GetPacket();
		packet.Write(Id);

		var encoder_Default = default(Terraria.ModLoader.Packets.LongEncoder);
		encoder_Default.Send(packet, (long)Default);
		var encoder_Serialized = default(Terraria.ModLoader.Packets.LongEncoder);
		encoder_Serialized.Send(packet, (long)Serialized);

		packet.Send(toClient, ignoreClient);
	}

	/// <inheritdoc cref=""Terraria.ModLoader.Packets.INetPacket.Receive(BinaryReader, int)""/>
	[CompilerGenerated]
	public void Receive(BinaryReader reader, int sender) {
		// SetDefaults();

		var encoder_Default = default(Terraria.ModLoader.Packets.LongEncoder);
		Default = (int)encoder_Default.Read(reader);
		var encoder_Serialized = default(Terraria.ModLoader.Packets.LongEncoder);
		Serialized = (int)encoder_Serialized.Read(reader);

		if (Main.netMode == NetmodeID.Server) {
			Send(-1, sender);
		}
		HandlePacket();
	}
}

#nullable restore";

		await new GeneratorTest {
			TestState = {
				Sources = { CodeFile },
				GeneratedSources = {
					(typeof(Adapter<NetPacketGeneratorv2>), "GeneratedDemo.GeneratedPacket.g.cs", GeneratedFile),
				},
			},
		}.RunAsync();
	}

	[Fact]
	public async Task StructPacketWithDefaultSerializedIgnoredInt32AutoSerializeOn() {
		const string CodeFile = @"
using Terraria.ModLoader;
using Terraria.ModLoader.Packets;

namespace GeneratedDemo;

public sealed class GeneratedMod : Mod {
}

[NetPacket(typeof(GeneratedMod), AutoSerialize = true)]
public partial struct GeneratedPacket {
	public const byte Id = GeneratedDemo.PacketRegistry.GeneratedPacketId;

	public int Default;

	[Serialize]
	public int Serialized;

	[Ignore]
	public int Ignored;

	public readonly void HandlePacket() {
	}

	public void HandleForAll() => throw new System.NotImplementedException();
}

[PacketRegistry]
static partial class PacketRegistry {
	[NetPacketIdOf(typeof(GeneratedPacket))]
	public const byte GeneratedPacketId = 0;
}
";

		const string GeneratedFile = @"// <auto-generated/>
#nullable disable
using System.IO;
using System.Runtime.CompilerServices;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace GeneratedDemo;

partial struct GeneratedPacket : Terraria.ModLoader.Packets.INetPacket {
	/// <inheritdoc cref=""Terraria.ModLoader.Packets.INetPacket.Send(int, int)""/>
	[CompilerGenerated]
	public void Send(int toClient = -1, int ignoreClient = -1) {
		var packet = ModContent.GetInstance<GeneratedDemo.GeneratedMod>().GetPacket();
		packet.Write(Id);

		var encoder_Default = default(Terraria.ModLoader.Packets.IntEncoder);
		encoder_Default.Send(packet, Default);
		var encoder_Serialized = default(Terraria.ModLoader.Packets.IntEncoder);
		encoder_Serialized.Send(packet, Serialized);

		packet.Send(toClient, ignoreClient);
	}

	/// <inheritdoc cref=""Terraria.ModLoader.Packets.INetPacket.Receive(BinaryReader, int)""/>
	[CompilerGenerated]
	public void Receive(BinaryReader reader, int sender) {
		// SetDefaults();

		var encoder_Default = default(Terraria.ModLoader.Packets.IntEncoder);
		Default = encoder_Default.Read(reader);
		var encoder_Serialized = default(Terraria.ModLoader.Packets.IntEncoder);
		Serialized = encoder_Serialized.Read(reader);

		if (Main.netMode == NetmodeID.Server) {
			Send(-1, sender);
		}
		HandlePacket();
	}
}

#nullable restore";

		await new GeneratorTest {
			TestState = {
				Sources = { CodeFile },
				GeneratedSources = {
					(typeof(Adapter<NetPacketGeneratorv2>), "GeneratedDemo.GeneratedPacket.g.cs", GeneratedFile),
				},
			},
		}.RunAsync();
	}

	[Fact]
	public async Task StructPacketWithDefaultSerializedIgnoredInt32EncodedAsIntBit7AutoSerializeOn() {
		const string CodeFile = @"
using Terraria.ModLoader;
using Terraria.ModLoader.Packets;

namespace GeneratedDemo;

public sealed class GeneratedMod : Mod {
}

[NetPacket(typeof(GeneratedMod), AutoSerialize = true)]
public partial struct GeneratedPacket {
	public const byte Id = GeneratedDemo.PacketRegistry.GeneratedPacketId;

	[EncodedAs(typeof(Int32Bit7Encoder))]
	public int Default;

	[Serialize]
	[EncodedAs(typeof(Int32Bit7Encoder))]
	public int Serialized;

	[Ignore]
	[EncodedAs(typeof(Int32Bit7Encoder))]
	public int Ignored;

	public readonly void HandlePacket() {
	}

	public void HandleForAll() => throw new System.NotImplementedException();
}

[PacketRegistry]
static partial class PacketRegistry {
	[NetPacketIdOf(typeof(GeneratedPacket))]
	public const byte GeneratedPacketId = 0;
}
";

		const string GeneratedFile = @"// <auto-generated/>
#nullable disable
using System.IO;
using System.Runtime.CompilerServices;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace GeneratedDemo;

partial struct GeneratedPacket : Terraria.ModLoader.Packets.INetPacket {
	/// <inheritdoc cref=""Terraria.ModLoader.Packets.INetPacket.Send(int, int)""/>
	[CompilerGenerated]
	public void Send(int toClient = -1, int ignoreClient = -1) {
		var packet = ModContent.GetInstance<GeneratedDemo.GeneratedMod>().GetPacket();
		packet.Write(Id);

		var encoder_Default = default(Terraria.ModLoader.Packets.Int32Bit7Encoder);
		encoder_Default.Send(packet, Default);
		var encoder_Serialized = default(Terraria.ModLoader.Packets.Int32Bit7Encoder);
		encoder_Serialized.Send(packet, Serialized);

		packet.Send(toClient, ignoreClient);
	}

	/// <inheritdoc cref=""Terraria.ModLoader.Packets.INetPacket.Receive(BinaryReader, int)""/>
	[CompilerGenerated]
	public void Receive(BinaryReader reader, int sender) {
		// SetDefaults();

		var encoder_Default = default(Terraria.ModLoader.Packets.Int32Bit7Encoder);
		Default = encoder_Default.Read(reader);
		var encoder_Serialized = default(Terraria.ModLoader.Packets.Int32Bit7Encoder);
		Serialized = encoder_Serialized.Read(reader);

		if (Main.netMode == NetmodeID.Server) {
			Send(-1, sender);
		}
		HandlePacket();
	}
}

#nullable restore";

		await new GeneratorTest {
			TestState = {
				Sources = { CodeFile },
				GeneratedSources = {
					(typeof(Adapter<NetPacketGeneratorv2>), "GeneratedDemo.GeneratedPacket.g.cs", GeneratedFile),
				},
			},
		}.RunAsync();
	}

	[Fact]
	public async Task StructPacketWithDefaultSerializedIgnoredInt32EncodedAsByteAutoSerializeOn() {
		const string CodeFile = @"
using Terraria.ModLoader;
using Terraria.ModLoader.Packets;

namespace GeneratedDemo;

public sealed class GeneratedMod : Mod {
}

[NetPacket(typeof(GeneratedMod), AutoSerialize = true)]
public partial struct GeneratedPacket {
	public const byte Id = GeneratedDemo.PacketRegistry.GeneratedPacketId;

	[EncodedAs(typeof(ByteEncoder))]
	public int Default;

	[Serialize]
	[EncodedAs(typeof(ByteEncoder))]
	public int Serialized;

	[Ignore]
	[EncodedAs(typeof(ByteEncoder))]
	public int Ignored;

	public readonly void HandlePacket() {
	}

	public void HandleForAll() => throw new System.NotImplementedException();
}

[PacketRegistry]
static partial class PacketRegistry {
	[NetPacketIdOf(typeof(GeneratedPacket))]
	public const byte GeneratedPacketId = 0;
}
";

		const string GeneratedFile = @"// <auto-generated/>
#nullable disable
using System.IO;
using System.Runtime.CompilerServices;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace GeneratedDemo;

partial struct GeneratedPacket : Terraria.ModLoader.Packets.INetPacket {
	/// <inheritdoc cref=""Terraria.ModLoader.Packets.INetPacket.Send(int, int)""/>
	[CompilerGenerated]
	public void Send(int toClient = -1, int ignoreClient = -1) {
		var packet = ModContent.GetInstance<GeneratedDemo.GeneratedMod>().GetPacket();
		packet.Write(Id);

		var encoder_Default = default(Terraria.ModLoader.Packets.ByteEncoder);
		encoder_Default.Send(packet, (byte)Default);
		var encoder_Serialized = default(Terraria.ModLoader.Packets.ByteEncoder);
		encoder_Serialized.Send(packet, (byte)Serialized);

		packet.Send(toClient, ignoreClient);
	}

	/// <inheritdoc cref=""Terraria.ModLoader.Packets.INetPacket.Receive(BinaryReader, int)""/>
	[CompilerGenerated]
	public void Receive(BinaryReader reader, int sender) {
		// SetDefaults();

		var encoder_Default = default(Terraria.ModLoader.Packets.ByteEncoder);
		Default = (int)encoder_Default.Read(reader);
		var encoder_Serialized = default(Terraria.ModLoader.Packets.ByteEncoder);
		Serialized = (int)encoder_Serialized.Read(reader);

		if (Main.netMode == NetmodeID.Server) {
			Send(-1, sender);
		}
		HandlePacket();
	}
}

#nullable restore";

		await new GeneratorTest {
			TestState = {
				Sources = { CodeFile },
				GeneratedSources = {
					(typeof(Adapter<NetPacketGeneratorv2>), "GeneratedDemo.GeneratedPacket.g.cs", GeneratedFile),
				},
			},
		}.RunAsync();
	}

	[Fact]
	public async Task StructPacketWithDefaultSerializedIgnoredInt32EncodedAsLongAutoSerializeOn() {
		const string CodeFile = @"
using Terraria.ModLoader;
using Terraria.ModLoader.Packets;

namespace GeneratedDemo;

public sealed class GeneratedMod : Mod {
}

[NetPacket(typeof(GeneratedMod), AutoSerialize = true)]
public partial struct GeneratedPacket {
	public const byte Id = GeneratedDemo.PacketRegistry.GeneratedPacketId;

	[EncodedAs(typeof(LongEncoder))]
	public int Default;

	[Serialize]
	[EncodedAs(typeof(LongEncoder))]
	public int Serialized;

	[Ignore]
	[EncodedAs(typeof(LongEncoder))]
	public int Ignored;

	public readonly void HandlePacket() {
	}

	public void HandleForAll() => throw new System.NotImplementedException();
}

[PacketRegistry]
static partial class PacketRegistry {
	[NetPacketIdOf(typeof(GeneratedPacket))]
	public const byte GeneratedPacketId = 0;
}
";

		const string GeneratedFile = @"// <auto-generated/>
#nullable disable
using System.IO;
using System.Runtime.CompilerServices;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace GeneratedDemo;

partial struct GeneratedPacket : Terraria.ModLoader.Packets.INetPacket {
	/// <inheritdoc cref=""Terraria.ModLoader.Packets.INetPacket.Send(int, int)""/>
	[CompilerGenerated]
	public void Send(int toClient = -1, int ignoreClient = -1) {
		var packet = ModContent.GetInstance<GeneratedDemo.GeneratedMod>().GetPacket();
		packet.Write(Id);

		var encoder_Default = default(Terraria.ModLoader.Packets.LongEncoder);
		encoder_Default.Send(packet, (long)Default);
		var encoder_Serialized = default(Terraria.ModLoader.Packets.LongEncoder);
		encoder_Serialized.Send(packet, (long)Serialized);

		packet.Send(toClient, ignoreClient);
	}

	/// <inheritdoc cref=""Terraria.ModLoader.Packets.INetPacket.Receive(BinaryReader, int)""/>
	[CompilerGenerated]
	public void Receive(BinaryReader reader, int sender) {
		// SetDefaults();

		var encoder_Default = default(Terraria.ModLoader.Packets.LongEncoder);
		Default = (int)encoder_Default.Read(reader);
		var encoder_Serialized = default(Terraria.ModLoader.Packets.LongEncoder);
		Serialized = (int)encoder_Serialized.Read(reader);

		if (Main.netMode == NetmodeID.Server) {
			Send(-1, sender);
		}
		HandlePacket();
	}
}

#nullable restore";

		await new GeneratorTest {
			TestState = {
				Sources = { CodeFile },
				GeneratedSources = {
					(typeof(Adapter<NetPacketGeneratorv2>), "GeneratedDemo.GeneratedPacket.g.cs", GeneratedFile),
				},
			},
		}.RunAsync();
	}

	[Fact]
	public async Task StructPacketWithDefaultSerializedIgnoredInt32AutoSerializeOff() {
		const string CodeFile = @"
using Terraria.ModLoader;
using Terraria.ModLoader.Packets;

namespace GeneratedDemo;

public sealed class GeneratedMod : Mod {
}

[NetPacket(typeof(GeneratedMod), AutoSerialize = false)]
public partial struct GeneratedPacket {
	public const byte Id = GeneratedDemo.PacketRegistry.GeneratedPacketId;

	public int Default;

	[Serialize]
	public int Serialized;

	[Ignore]
	public int Ignored;

	public readonly void HandlePacket() {
	}

	public void HandleForAll() => throw new System.NotImplementedException();
}

[PacketRegistry]
static partial class PacketRegistry {
	[NetPacketIdOf(typeof(GeneratedPacket))]
	public const byte GeneratedPacketId = 0;
}
";

		const string GeneratedFile = @"// <auto-generated/>
#nullable disable
using System.IO;
using System.Runtime.CompilerServices;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace GeneratedDemo;

partial struct GeneratedPacket : Terraria.ModLoader.Packets.INetPacket {
	/// <inheritdoc cref=""Terraria.ModLoader.Packets.INetPacket.Send(int, int)""/>
	[CompilerGenerated]
	public void Send(int toClient = -1, int ignoreClient = -1) {
		var packet = ModContent.GetInstance<GeneratedDemo.GeneratedMod>().GetPacket();
		packet.Write(Id);

		var encoder_Serialized = default(Terraria.ModLoader.Packets.IntEncoder);
		encoder_Serialized.Send(packet, Serialized);

		packet.Send(toClient, ignoreClient);
	}

	/// <inheritdoc cref=""Terraria.ModLoader.Packets.INetPacket.Receive(BinaryReader, int)""/>
	[CompilerGenerated]
	public void Receive(BinaryReader reader, int sender) {
		// SetDefaults();

		var encoder_Serialized = default(Terraria.ModLoader.Packets.IntEncoder);
		Serialized = encoder_Serialized.Read(reader);

		if (Main.netMode == NetmodeID.Server) {
			Send(-1, sender);
		}
		HandlePacket();
	}
}

#nullable restore";

		await new GeneratorTest {
			TestState = {
				Sources = { CodeFile },
				GeneratedSources = {
					(typeof(Adapter<NetPacketGeneratorv2>), "GeneratedDemo.GeneratedPacket.g.cs", GeneratedFile),
				},
			},
		}.RunAsync();
	}

	[Fact]
	public async Task StructPacketWithDefaultSerializedIgnoredInt32EncodedAsIntBit7AutoSerializeOff() {
		const string CodeFile = @"
using Terraria.ModLoader;
using Terraria.ModLoader.Packets;

namespace GeneratedDemo;

public sealed class GeneratedMod : Mod {
}

[NetPacket(typeof(GeneratedMod), AutoSerialize = false)]
public partial struct GeneratedPacket {
	public const byte Id = GeneratedDemo.PacketRegistry.GeneratedPacketId;

	[EncodedAs(typeof(Int32Bit7Encoder))]
	public int Default;

	[Serialize]
	[EncodedAs(typeof(Int32Bit7Encoder))]
	public int Serialized;

	[Ignore]
	[EncodedAs(typeof(Int32Bit7Encoder))]
	public int Ignored;

	public readonly void HandlePacket() {
	}

	public void HandleForAll() => throw new System.NotImplementedException();
}

[PacketRegistry]
static partial class PacketRegistry {
	[NetPacketIdOf(typeof(GeneratedPacket))]
	public const byte GeneratedPacketId = 0;
}
";

		const string GeneratedFile = @"// <auto-generated/>
#nullable disable
using System.IO;
using System.Runtime.CompilerServices;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace GeneratedDemo;

partial struct GeneratedPacket : Terraria.ModLoader.Packets.INetPacket {
	/// <inheritdoc cref=""Terraria.ModLoader.Packets.INetPacket.Send(int, int)""/>
	[CompilerGenerated]
	public void Send(int toClient = -1, int ignoreClient = -1) {
		var packet = ModContent.GetInstance<GeneratedDemo.GeneratedMod>().GetPacket();
		packet.Write(Id);

		var encoder_Serialized = default(Terraria.ModLoader.Packets.Int32Bit7Encoder);
		encoder_Serialized.Send(packet, Serialized);

		packet.Send(toClient, ignoreClient);
	}

	/// <inheritdoc cref=""Terraria.ModLoader.Packets.INetPacket.Receive(BinaryReader, int)""/>
	[CompilerGenerated]
	public void Receive(BinaryReader reader, int sender) {
		// SetDefaults();

		var encoder_Serialized = default(Terraria.ModLoader.Packets.Int32Bit7Encoder);
		Serialized = encoder_Serialized.Read(reader);

		if (Main.netMode == NetmodeID.Server) {
			Send(-1, sender);
		}
		HandlePacket();
	}
}

#nullable restore";

		await new GeneratorTest {
			TestState = {
				Sources = { CodeFile },
				GeneratedSources = {
					(typeof(Adapter<NetPacketGeneratorv2>), "GeneratedDemo.GeneratedPacket.g.cs", GeneratedFile),
				},
			},
		}.RunAsync();
	}

	[Fact]
	public async Task StructPacketWithDefaultSerializedIgnoredInt32EncodedAsByteAutoSerializeOff() {
		const string CodeFile = @"
using Terraria.ModLoader;
using Terraria.ModLoader.Packets;

namespace GeneratedDemo;

public sealed class GeneratedMod : Mod {
}

[NetPacket(typeof(GeneratedMod), AutoSerialize = false)]
public partial struct GeneratedPacket {
	public const byte Id = GeneratedDemo.PacketRegistry.GeneratedPacketId;

	[EncodedAs(typeof(ByteEncoder))]
	public int Default;

	[Serialize]
	[EncodedAs(typeof(ByteEncoder))]
	public int Serialized;

	[Ignore]
	[EncodedAs(typeof(ByteEncoder))]
	public int Ignored;

	public readonly void HandlePacket() {
	}

	public void HandleForAll() => throw new System.NotImplementedException();
}

[PacketRegistry]
static partial class PacketRegistry {
	[NetPacketIdOf(typeof(GeneratedPacket))]
	public const byte GeneratedPacketId = 0;
}
";

		const string GeneratedFile = @"// <auto-generated/>
#nullable disable
using System.IO;
using System.Runtime.CompilerServices;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace GeneratedDemo;

partial struct GeneratedPacket : Terraria.ModLoader.Packets.INetPacket {
	/// <inheritdoc cref=""Terraria.ModLoader.Packets.INetPacket.Send(int, int)""/>
	[CompilerGenerated]
	public void Send(int toClient = -1, int ignoreClient = -1) {
		var packet = ModContent.GetInstance<GeneratedDemo.GeneratedMod>().GetPacket();
		packet.Write(Id);

		var encoder_Serialized = default(Terraria.ModLoader.Packets.ByteEncoder);
		encoder_Serialized.Send(packet, (byte)Serialized);

		packet.Send(toClient, ignoreClient);
	}

	/// <inheritdoc cref=""Terraria.ModLoader.Packets.INetPacket.Receive(BinaryReader, int)""/>
	[CompilerGenerated]
	public void Receive(BinaryReader reader, int sender) {
		// SetDefaults();

		var encoder_Serialized = default(Terraria.ModLoader.Packets.ByteEncoder);
		Serialized = (int)encoder_Serialized.Read(reader);

		if (Main.netMode == NetmodeID.Server) {
			Send(-1, sender);
		}
		HandlePacket();
	}
}

#nullable restore";

		await new GeneratorTest {
			TestState = {
				Sources = { CodeFile },
				GeneratedSources = {
					(typeof(Adapter<NetPacketGeneratorv2>), "GeneratedDemo.GeneratedPacket.g.cs", GeneratedFile),
				},
			},
		}.RunAsync();
	}

	[Fact]
	public async Task StructPacketWithDefaultSerializedIgnoredInt32EncodedAsLongAutoSerializeOff() {
		const string CodeFile = @"
using Terraria.ModLoader;
using Terraria.ModLoader.Packets;

namespace GeneratedDemo;

public sealed class GeneratedMod : Mod {
}

[NetPacket(typeof(GeneratedMod), AutoSerialize = false)]
public partial struct GeneratedPacket {
	public const byte Id = GeneratedDemo.PacketRegistry.GeneratedPacketId;

	[EncodedAs(typeof(LongEncoder))]
	public int Default;

	[Serialize]
	[EncodedAs(typeof(LongEncoder))]
	public int Serialized;

	[Ignore]
	[EncodedAs(typeof(LongEncoder))]
	public int Ignored;

	public readonly void HandlePacket() {
	}

	public void HandleForAll() => throw new System.NotImplementedException();
}

[PacketRegistry]
static partial class PacketRegistry {
	[NetPacketIdOf(typeof(GeneratedPacket))]
	public const byte GeneratedPacketId = 0;
}
";

		const string GeneratedFile = @"// <auto-generated/>
#nullable disable
using System.IO;
using System.Runtime.CompilerServices;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace GeneratedDemo;

partial struct GeneratedPacket : Terraria.ModLoader.Packets.INetPacket {
	/// <inheritdoc cref=""Terraria.ModLoader.Packets.INetPacket.Send(int, int)""/>
	[CompilerGenerated]
	public void Send(int toClient = -1, int ignoreClient = -1) {
		var packet = ModContent.GetInstance<GeneratedDemo.GeneratedMod>().GetPacket();
		packet.Write(Id);

		var encoder_Serialized = default(Terraria.ModLoader.Packets.LongEncoder);
		encoder_Serialized.Send(packet, (long)Serialized);

		packet.Send(toClient, ignoreClient);
	}

	/// <inheritdoc cref=""Terraria.ModLoader.Packets.INetPacket.Receive(BinaryReader, int)""/>
	[CompilerGenerated]
	public void Receive(BinaryReader reader, int sender) {
		// SetDefaults();

		var encoder_Serialized = default(Terraria.ModLoader.Packets.LongEncoder);
		Serialized = (int)encoder_Serialized.Read(reader);

		if (Main.netMode == NetmodeID.Server) {
			Send(-1, sender);
		}
		HandlePacket();
	}
}

#nullable restore";

		await new GeneratorTest {
			TestState = {
				Sources = { CodeFile },
				GeneratedSources = {
					(typeof(Adapter<NetPacketGeneratorv2>), "GeneratedDemo.GeneratedPacket.g.cs", GeneratedFile),
				},
			},
		}.RunAsync();
	}
}

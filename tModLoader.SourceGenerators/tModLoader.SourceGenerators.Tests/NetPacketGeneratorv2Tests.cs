using GeneratorTest = tModLoader.SourceGenerators.Tests.tModLoaderSourceGeneratorVerifier<tModLoader.SourceGenerators.Tests.Adapter<tModLoader.SourceGenerators.NetPacketGeneratorv2>>.Test;

namespace tModLoader.SourceGenerators.Tests;

public partial class NetPacketGeneratorv2Tests {
	[Fact]
	public async Task EmptyStructPacket() {
		const string CodeFile = @"
using Terraria.ModLoader;
using Terraria.ModLoader.Packets;

namespace GeneratedDemo;

public sealed class GeneratedMod : Mod {
}

[NetPacket(typeof(GeneratedMod))]
public partial struct GeneratedPacket {
	public const byte Id = GeneratedDemo.PacketRegistry.GeneratedPacketId;

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

		packet.Send(toClient, ignoreClient);
	}

	/// <inheritdoc cref=""Terraria.ModLoader.Packets.INetPacket.Receive(BinaryReader, int)""/>
	[CompilerGenerated]
	public void Receive(BinaryReader reader, int sender) {
		// SetDefaults();

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
	public async Task EmptyRefStructPacket() {
		const string CodeFile = @"
using Terraria.ModLoader;
using Terraria.ModLoader.Packets;

namespace GeneratedDemo;

public sealed class GeneratedMod : Mod {
}

[NetPacket(typeof(GeneratedMod))]
public ref partial struct GeneratedPacket {
	private const byte Id = PacketRegistry.GeneratedPacketId;

	public readonly void HandlePacket() {
	}
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

partial struct GeneratedPacket {
	/// <inheritdoc cref=""Terraria.ModLoader.Packets.INetPacket.Send(int, int)""/>
	[CompilerGenerated]
	public void Send(int toClient = -1, int ignoreClient = -1) {
		var packet = ModContent.GetInstance<GeneratedDemo.GeneratedMod>().GetPacket();
		packet.Write(Id);

		packet.Send(toClient, ignoreClient);
	}

	/// <inheritdoc cref=""Terraria.ModLoader.Packets.INetPacket.Receive(BinaryReader, int)""/>
	[CompilerGenerated]
	public void Receive(BinaryReader reader, int sender) {
		// SetDefaults();

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
	public async Task EmptyStructPacketWithSetDefaults() {
		const string CodeFile = @"
using Terraria.ModLoader;
using Terraria.ModLoader.Packets;

namespace GeneratedDemo;

public sealed class GeneratedMod : Mod {
}

[NetPacket(typeof(GeneratedMod))]
public partial struct GeneratedPacket {
	public const byte Id = GeneratedDemo.PacketRegistry.GeneratedPacketId;

	public readonly void HandlePacket() {
	}

	public readonly void SetDefaults() {
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

		packet.Send(toClient, ignoreClient);
	}

	/// <inheritdoc cref=""Terraria.ModLoader.Packets.INetPacket.Receive(BinaryReader, int)""/>
	[CompilerGenerated]
	public void Receive(BinaryReader reader, int sender) {
		SetDefaults();

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
	public async Task EmptyStructPacketModNameSameAsNamespace()
	{
		const string CodeFile = @"
using Terraria.ModLoader;
using Terraria.ModLoader.Packets;

namespace GeneratedDemo;

public sealed class GeneratedDemo : Mod {
}

[NetPacket(typeof(GeneratedDemo))]
public partial struct GeneratedPacket {
	public const byte Id = PacketRegistry.GeneratedPacketId;

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
		var packet = ModContent.GetInstance<GeneratedDemo>().GetPacket();
		packet.Write(Id);

		packet.Send(toClient, ignoreClient);
	}

	/// <inheritdoc cref=""Terraria.ModLoader.Packets.INetPacket.Receive(BinaryReader, int)""/>
	[CompilerGenerated]
	public void Receive(BinaryReader reader, int sender) {
		// SetDefaults();

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

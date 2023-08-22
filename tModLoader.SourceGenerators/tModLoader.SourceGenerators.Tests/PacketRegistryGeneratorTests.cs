﻿using GeneratorTest = tModLoader.SourceGenerators.Tests.tModLoaderSourceGeneratorVerifier<tModLoader.SourceGenerators.Tests.Adapter<tModLoader.SourceGenerators.PacketRegistryGenerator>>.Test;

namespace tModLoader.SourceGenerators.Tests;

public class PacketRegistryGeneratorTests {
	[Fact]
	public async Task EmptyPacketRegistry() {
		const string CodeFile = @"
using Terraria.ModLoader.Packets;

namespace GeneratedDemo;

[PacketRegistry]
public static partial class PacketRegistry {
}
";

		const string GeneratedFile = @"// <auto-generated/>
using System.IO;
using System.Runtime.CompilerServices;

namespace GeneratedDemo;

partial class PacketRegistry {
	[CompilerGenerated]
	public static void Handle(BinaryReader reader, int whoAmI) {
		byte type = reader.ReadByte();

		switch (type) {
		}
	}
}
";

		await new GeneratorTest {
			TestState = {
				Sources = { CodeFile },
				GeneratedSources = {
					(typeof(Adapter<PacketRegistryGenerator>), "GeneratedDemo.PacketRegistry.g.cs", GeneratedFile),
				},
			},
		}.RunAsync();
	}

	[Fact]
	public async Task PacketRegistryWithId() {
		const string CodeFile = @"
using Terraria.ModLoader.Packets;

namespace GeneratedDemo;

public sealed class GeneratedMod {
}

[NetPacket(typeof(GeneratedMod))]
public partial struct GeneratedPacket {
	public readonly void HandlePacket() {
	}

	public void Receive(System.IO.BinaryReader reader, int sender) => throw new System.NotImplementedException();

	public void HandleForAll() => throw new System.NotImplementedException();
}

[PacketRegistry]
public static partial class PacketRegistry {
	[NetPacketIdOf(typeof(GeneratedPacket))]
	public const byte GeneratedPacketId = 0;
}
";

		const string GeneratedFile = @"// <auto-generated/>
using System.IO;
using System.Runtime.CompilerServices;

namespace GeneratedDemo;

partial class PacketRegistry {
	[CompilerGenerated]
	public static void Handle(BinaryReader reader, int whoAmI) {
		byte type = reader.ReadByte();

		switch (type) {
			case GeneratedPacketId: {
				var packet = default(GeneratedDemo.GeneratedPacket);
				packet.Receive(reader, whoAmI);
				break;
			}
		}
	}
}
";

		const string GeneratedFile2 = @"// <auto-generated/>
using System.Runtime.CompilerServices;
using PacketRegistry = GeneratedDemo.PacketRegistry;

namespace GeneratedDemo;

partial struct GeneratedPacket {
	[CompilerGenerated]
	public const byte Id = PacketRegistry.GeneratedPacketId;
}
";

		await new GeneratorTest {
			TestState = {
				Sources = { CodeFile },
				GeneratedSources = {
					(typeof(Adapter<PacketRegistryGenerator>), "GeneratedDemo.PacketRegistry.g.cs", GeneratedFile),
					(typeof(Adapter<PacketRegistryGenerator>), "GeneratedDemo.GeneratedPacket.g.cs", GeneratedFile2),
				},
			},
		}.RunAsync();
	}
}

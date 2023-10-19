using System.IO;

namespace Terraria.ModLoader.Packets;

public interface INetEncoder {
	void Write<T>(ModPacket packet, T value);

	T Read<T>(BinaryReader reader);
}

public interface INetEncoder<T> : INetEncoder {
	void Write(ModPacket packet, T value);

	T Read(BinaryReader reader);

	void INetEncoder.Write<TMethod>(ModPacket packet, TMethod value) {
		Write(packet, (T)(object)value);
	}

	TMethod INetEncoder.Read<TMethod>(BinaryReader reader) {
		return (TMethod)(object)Read(reader);
	}
}

using System.IO;

namespace Terraria.ModLoader.Packets;

public interface INetEncoder {
	void Send<T>(ModPacket packet, T value);

	T Read<T>(BinaryReader reader);
}

public interface INetEncoder<T> : INetEncoder {
	void Send(ModPacket packet, T value);

	T Read(BinaryReader reader);

	void INetEncoder.Send<TMethod>(ModPacket packet, TMethod value) {
		Send(packet, (T)(object)value);
	}

	TMethod INetEncoder.Read<TMethod>(BinaryReader reader) {
		return (TMethod)(object)Read(reader);
	}
}

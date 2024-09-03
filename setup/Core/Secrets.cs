using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text.Json;
using Terraria.ModLoader.Setup.Core.Abstractions;

namespace Terraria.ModLoader.Setup.Core;

// We don't bother with IVs because we aren't really providing an encryption service. We don't care about various attacks involving lots of different encrypted messages.
public class Secrets
{
	private static readonly byte[] KCV = Convert.FromHexString("4652ab9b391c605fb20cd2446796edf7");
	private static readonly string DerivedKeyStorePath = Path.Combine("setup", "SecretAssets", "keys.json");

	private readonly byte[] key;

	private static string SecretFilePath(string name) => Path.Combine("setup", "SecretAssets", name + ".enc");

	public Secrets(byte[] key)
	{
		VerifyKey(key);
		this.key = key;
	}

	public static byte[] DeriveKey(string path)
	{
		if (!TryDeriveKey(path, out var key))
			throw new UnauthorizedAccessException($"The provided {Path.GetFileName(path)} is not a valid proof of ownership. Perhaps it's an older or newer version");

		return key;
	}

	private static byte[] HashFile(string file)
	{
		using var mySHA256 = SHA256.Create();
		using var fs = File.OpenRead(file);
		return mySHA256.ComputeHash(fs);
	}

	public static bool TryDeriveKey(string file, [NotNullWhen(true)] out byte[]? key)
	{
		var hash = HashFile(file);
		var json = File.ReadAllText(DerivedKeyStorePath);
		var derivedKeys = JsonSerializer.Deserialize<Dictionary<string, byte[]>>(json)!;
		key = derivedKeys.Values.Select(k => Decrypt(hash, k)).Where(CheckKey).FirstOrDefault();
		return key != null;
	}

	public void AddProofOfOwnershipFile(string identifier, string file)
	{
		var hash = HashFile(file);
		var derivedKey = Encrypt(hash, key);

		var json = File.ReadAllText(DerivedKeyStorePath);
		var derivedKeys = JsonSerializer.Deserialize<Dictionary<string, byte[]>>(json)!;
		derivedKeys[identifier] = derivedKey;

		json = JsonSerializer.Serialize(derivedKeys, options: new() { WriteIndented = true });
		File.WriteAllText(DerivedKeyStorePath, json);
	}

	public static bool CheckKey(byte[] key)
	{
		using var aes = Aes.Create();
		aes.Key = key;
		var encoded = aes.EncryptCbc(new byte[32], new byte[16], PaddingMode.None);
		return Enumerable.SequenceEqual(encoded[..KCV.Length], KCV);
	}

	public static void VerifyKey(byte[] key)
	{
		if (!CheckKey(key))
			throw new Exception("Key verification failed, wrong key");
	}

	private static byte[] Decrypt(byte[] key, byte[] data)
	{
		using var aes = Aes.Create();
		aes.Key = key;
		return aes.DecryptCbc(data, new byte[16]);
	}

	private static byte[] Encrypt(byte[] key, byte[] data)
	{
		using var aes = Aes.Create();
		aes.Key = key;
		return aes.EncryptCbc(data, new byte[16]);
	}

	private static byte[] Compress(byte[] data)
	{
		using var ms = new MemoryStream();
		using var ds = new DeflateStream(ms, CompressionMode.Compress);
		new MemoryStream(data).CopyTo(ds);
		return ms.ToArray();
	}

	private static byte[] Decompress(byte[] data)
	{
		using var ds = new DeflateStream(new MemoryStream(data), CompressionMode.Decompress);
		using var ms = new MemoryStream();
		ds.CopyTo(ms);
		return ms.ToArray();
	}

	public void UpdateFile(string path)
	{
		var data = File.ReadAllBytes(path);
		data = Compress(data);
		data = Encrypt(key, data);
		File.WriteAllBytes(SecretFilePath(Path.GetFileName(path)), data);
	}

	public byte[] ReadFile(string name)
	{
		var data = File.ReadAllBytes(SecretFilePath(name));
		data = Decrypt(key, data);
		data = Decompress(data);
		return data;
	}
}

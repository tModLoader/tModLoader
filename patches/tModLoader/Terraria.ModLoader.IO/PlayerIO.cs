using System;
using System.IO;
using System.Security.Cryptography;
using Terraria;
using Terraria.Social;
using Terraria.Utilities;

namespace Terraria.ModLoader.IO
{
	internal static class PlayerIO
	{
		private const byte numFlagBytes = 1;
		//make Terraria.Player.ENCRYPTION_KEY internal
		//add to end of Terraria.Player.SavePlayer
		internal static void WriteModFile(Player player, string path, bool isCloudSave)
		{
			path = Path.ChangeExtension(path, ".tplr");
			if (FileUtilities.Exists(path, isCloudSave))
			{
				FileUtilities.Copy(path, path + ".bak", isCloudSave, true);
			}
			byte[] flags;
			byte[] data;
			using (MemoryStream stream = new MemoryStream())
			{
				using (BinaryWriter writer = new BinaryWriter(stream))
				{
					flags = WriteModPlayer(player, writer);
					writer.Flush();
					data = stream.ToArray();
				}
			}
			RijndaelManaged rijndaelManaged = new RijndaelManaged();
			using (Stream stream = isCloudSave ? (Stream)new MemoryStream() : (Stream)new FileStream(path, FileMode.Create))
			{
				using (CryptoStream cryptoStream = new CryptoStream(stream, rijndaelManaged.CreateEncryptor(Player.ENCRYPTION_KEY, Player.ENCRYPTION_KEY), CryptoStreamMode.Write))
				{
					using (BinaryWriter writer = new BinaryWriter(cryptoStream))
					{
						byte limit;
						for (limit = (byte)flags.Length; limit > 0; limit--)
						{
							if (flags[limit - 1] != 0)
							{
								break;
							}
						}
						writer.Write(limit);
						if (limit > 0)
						{
							writer.Write(flags, 0, limit);
							writer.Write(data);
						}
						writer.Flush();
						cryptoStream.FlushFinalBlock();
						stream.Flush();
						if (isCloudSave && SocialAPI.Cloud != null)
						{
							SocialAPI.Cloud.Write(path, ((MemoryStream)stream).ToArray());
						}
					}
				}
			}
		}
		//add near end of Terraria.Player.LoadPlayer before accessory check
		internal static void ReadModFile(Player player, string path, bool isCloudSave)
		{
			path = Path.ChangeExtension(path, ".tplr");
			RijndaelManaged rijndaelManaged = new RijndaelManaged();
			rijndaelManaged.Padding = PaddingMode.None;
			if (!FileUtilities.Exists(path, isCloudSave))
			{
				return;
			}
			byte[] buffer = FileUtilities.ReadAllBytes(path, isCloudSave);
			using (MemoryStream stream = new MemoryStream(buffer))
			{
				using (CryptoStream cryptoStream = new CryptoStream(stream, rijndaelManaged.CreateDecryptor(Player.ENCRYPTION_KEY, Player.ENCRYPTION_KEY), CryptoStreamMode.Read))
				{
					using (BinaryReader reader = new BinaryReader(cryptoStream))
					{
						byte limit = reader.ReadByte();
						if (limit == 0)
						{
							return;
						}
						byte[] flags = reader.ReadBytes(limit);
						if (flags.Length < numFlagBytes)
						{
							Array.Resize(ref flags, numFlagBytes);
						}
						ReadModPlayer(player, flags, reader);
					}
				}
			}
		}

		internal static byte[] WriteModPlayer(Player player, BinaryWriter writer)
		{
			byte[] flags = new byte[numFlagBytes];
			if (WriteInventory(player.armor, writer))
			{
				flags[0] |= 1;
			}
			if (WriteInventory(player.dye, writer))
			{
				flags[0] |= 2;
			}
			if (WriteInventory(player.inventory, writer, true, true))
			{
				flags[0] |= 4;
			}
			if (WriteInventory(player.miscEquips, writer))
			{
				flags[0] |= 8;
			}
			if (WriteInventory(player.miscDyes, writer))
			{
				flags[0] |= 16;
			}
			if (WriteInventory(player.bank.item, writer, true))
			{
				flags[0] |= 32;
			}
			if (WriteInventory(player.bank2.item, writer, true))
			{
				flags[0] |= 64;
			}
			return flags;
		}

		internal static void ReadModPlayer(Player player, byte[] flags, BinaryReader reader)
		{
			if ((flags[0] & 1) == 1)
			{
				ReadInventory(player.armor, reader);
			}
			if ((flags[0] & 2) == 2)
			{
				ReadInventory(player.dye, reader);
			}
			if ((flags[0] & 4) == 4)
			{
				ReadInventory(player.inventory, reader, true, true);
			}
			if ((flags[0] & 8) == 8)
			{
				ReadInventory(player.miscEquips, reader);
			}
			if ((flags[0] & 16) == 16)
			{
				ReadInventory(player.miscDyes, reader);
			}
			if ((flags[0] & 32) == 32)
			{
				ReadInventory(player.bank.item, reader, true);
			}
			if ((flags[0] & 64) == 64)
			{
				ReadInventory(player.bank2.item, reader, true);
			}
		}

		internal static bool WriteInventory(Item[] inv, BinaryWriter writer, bool writeStack = false, bool writeFavorite = false)
		{
			ushort count = 0;
			byte[] data;
			using (MemoryStream stream = new MemoryStream())
			{
				using (BinaryWriter invWriter = new BinaryWriter(stream))
				{
					for (int k = 0; k < inv.Length; k++)
					{
						if (ItemIO.WriteModItemSlot(inv, k, invWriter, writeStack, writeFavorite))
						{
							count++;
						}
					}
					invWriter.Flush();
					data = stream.ToArray();
				}
			}
			if (count > 0)
			{
				writer.Write(count);
				writer.Write(data);
				return true;
			}
			return false;
		}

		internal static void ReadInventory(Item[] inv, BinaryReader reader, bool readStack = false, bool readFavorite = false)
		{
			int count = reader.ReadUInt16();
			for (int k = 0; k < count; k++)
			{
				ItemIO.ReadModItemSlot(inv, reader, readStack, readFavorite);
			}
		}
		//add to end of Terraria.IO.PlayerFileData.MoveToCloud
		internal static void MoveToCloud(string localPath, string cloudPath)
		{
			localPath = Path.ChangeExtension(localPath, ".tplr");
			cloudPath = Path.ChangeExtension(cloudPath, ".tplr");
			if (File.Exists(localPath))
			{
				FileUtilities.MoveToCloud(localPath, cloudPath);
			}
		}
		//add to end of Terraria.IO.PlayerFileData.MoveToLocal
		internal static void MoveToLocal(string cloudPath, string localPath)
		{
			cloudPath = Path.ChangeExtension(cloudPath, ".tplr");
			localPath = Path.ChangeExtension(localPath, ".tplr");
			if (FileUtilities.Exists(cloudPath, true))
			{
				FileUtilities.MoveToLocal(cloudPath, localPath);
			}
		}
		//add to Terraria.Player.GetFileData after moving vanilla .bak file
		internal static void LoadBackup(string path, bool cloudSave)
		{
			path = Path.ChangeExtension(path, ".tplr");
			if (FileUtilities.Exists(path + ".bak", cloudSave))
			{
				FileUtilities.Move(path + ".bak", path, cloudSave, true);
			}
		}
	}
}

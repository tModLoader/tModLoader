using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content
{
	//This is less of an example and more of a debugging tool
	//This will test for;
	//	Initialize being called before NetSend
	//	Initialize being called before NetRecieve
	class DebugNetworkingModWorld : ModWorld
	{
		public static Dictionary<string, string> table;
		public static bool _tableChanged; //To avoid spamming the server with NetSend
		public override void Initialize() {
			table = new Dictionary<string, string>();
			table["Initialized"] = "true";
			_tableChanged = true;
		
		}

		public override void NetSend(BinaryWriter writer) {
			if(!table.TryGetValue("HELLO", out var result) || result != "World") { //Avoid sending info to the server if this didnt change.
				table["HELLO"] = "WORLD"; //This will test that Initialize was run before NetSend, table will be null if it wasn't
				_tableChanged = true;
			}

			if (!_tableChanged)
				return;
			//Send over all keys of the table
			writer.Write(table.Count); //this should send 2
			foreach(var pair in table) {
				writer.Write(pair.Key);
				writer.Write(pair.Value);
			}
			_tableChanged = false;

		}
		public override void NetReceive(BinaryReader reader) {
			var count = reader.ReadInt32(); //Read the number of key-value-pairs to read
			for(var i = 0; i < count; i++) {
				var key = reader.ReadString();
				var value = reader.ReadString();
				table[key] = value;
			}
		}

	}
}
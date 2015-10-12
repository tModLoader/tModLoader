using System;
using System.IO;
using System.Windows.Forms;
using Ionic.Zlib;

namespace Installer
{
    class SetupTask
    {
        internal static bool Setup()
        {
            Directory.CreateDirectory("Setup");
            string[] files =
            {
                "Windows.exe",
                "Mac.exe",
                "Linux.exe",
                "Microsoft.Xna.Framework.dll",
                "Microsoft.Xna.Framework.Game.dll",
                "Microsoft.Xna.Framework.Graphics.dll",
                "Microsoft.Xna.Framework.Xact.dll",
                "FNA.dll",
                "MP3Sharp.dll",
                "Content" + Path.DirectorySeparatorChar + "MysteryItem.png"
            };
            for (int k = 0; k < files.Length; k++)
            {
                files[k] = "Setup" + Path.DirectorySeparatorChar + files[k];
            }
            foreach (string file in files)
            {
                if (!File.Exists(file))
                {
                    MessageBox.Show("The file " + file + " does not exist.", "Setup Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }
            Directory.CreateDirectory("Resources");
            try
            {
                using (FileStream fileStream = File.Create("Resources" + Path.DirectorySeparatorChar + "Resources"))
                {
                    using (DeflateStream zip = new DeflateStream(fileStream, CompressionMode.Compress))
                    {
                        using (BinaryWriter writer = new BinaryWriter(zip))
                        {
                            writer.Write((byte)files.Length);
                            foreach (string file in files)
                            {
                                WriteFile(file, writer);
                            }
                        }
                    }
                }
            }
            catch(Exception e)
            {
                MessageBox.Show(e.Message + Environment.NewLine + e.StackTrace, "Setup Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            MessageBox.Show("Success!", "Setup", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return true;
        }

        private static void WriteFile(string file, BinaryWriter writer)
        {
            byte[] buffer = File.ReadAllBytes(file);
            writer.Write(Path.GetFileName(file));
            writer.Write(buffer.Length);
            writer.Write(buffer);
        }
    }
}

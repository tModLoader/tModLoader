using System;
using System.Collections.Generic;
using System.IO;
using Ionic.Zlib;

namespace Installer
{
    class ZipFile
    {
        public readonly string name;
        private IDictionary<string, byte[]> files = new Dictionary<string, byte[]>();
        public ZipFile(string name)
        {
            this.name = name;
        }

        public byte[] this[string file]
        {
            get
            {
                return this.files[file];
            }
            set
            {
                this.files[file] = value;
            }
        }

        public void Write(Task task = null, DoWorkArgs progress = default(DoWorkArgs))
        {
            using(FileStream fileStream = File.Create(this.name))
            {
                using(DeflateStream compress = new DeflateStream(fileStream, CompressionMode.Compress))
                {
                    using(BinaryWriter writer = new BinaryWriter(compress))
                    {
                        writer.Write((byte)this.files.Count);
                        foreach(string file in this.files.Keys)
                        {
                            writer.Write(file);
                            writer.Write(this.files[file].Length);
                            writer.Write(this.files[file]);
                            if(task != null)
                            {
                                task.ReportProgress(progress);
                            }
                        }
                    }
                }
            }
        }

        public static ZipFile Read(string name)
        {
            ZipFile zip = new ZipFile(name);
            using(FileStream fileStream = File.OpenRead(name))
            {
                using(DeflateStream decompress = new DeflateStream(fileStream, CompressionMode.Decompress))
                {
                    using(BinaryReader reader = new BinaryReader(decompress))
                    {
                        int count = reader.ReadByte();
                        for(int k = 0; k < count; k++)
                        {
                            string fileName = reader.ReadString();
                            byte[] buffer = reader.ReadBytes(reader.ReadInt32());
                            zip[fileName] = buffer;
                        }
                    }
                }
            }
            return zip;
        }
    }
}

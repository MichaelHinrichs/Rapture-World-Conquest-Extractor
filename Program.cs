//Written for Rapture: World Conquest. https://store.steampowered.com/app/547520/
using System;
using System.IO;
using System.IO.Compression;

namespace Rapture_World_Conquest_Extractor
{
    class Program
    {
        static BinaryReader br;
        static void Main(string[] args)
        {
            FileStream input = File.OpenRead(args[0]);
            br = new(input);
            if (new string(br.ReadChars(4)) != "HEAD")
                throw new Exception("Not a \"Rapture: World Conquest\" moj file.");

            br.ReadInt32();
            br.ReadInt32();

            string fileExtention = new(br.ReadChars(4));
            Directory.CreateDirectory(Path.GetDirectoryName(args[0]) + "//" + Path.GetFileNameWithoutExtension(args[0]));
            int n = 0;
            while (fileExtention != "FOOT")
            {
                int sizeCompressed = br.ReadInt32();
                int isCompressed = br.ReadInt32();//This is just a guess.
                int sizeUncompressed = br.ReadInt32();

                MemoryStream ms = new();
                if (isCompressed == 65536)
                {
                    br.ReadInt16();
                    using var ds = new DeflateStream(new MemoryStream(br.ReadBytes(sizeCompressed - 18)), CompressionMode.Decompress);
                    ds.CopyTo(ms);
                }
                else if (isCompressed == 0)//Probably
                    ms.Write(br.ReadBytes(sizeUncompressed));
                else
                    throw new Exception("Fuck!");

                BinaryReader msr = new(ms);
                msr.BaseStream.Position = 16;//what is this data? Is it in both the decompressed, and the never compressed files?

                using FileStream FS = File.Create(Path.GetDirectoryName(args[0]) + "//" + Path.GetFileNameWithoutExtension(args[0]) + "//" + n + '.' + fileExtention);
                BinaryWriter bw = new(FS);
                bw.Write(msr.ReadBytes(sizeUncompressed - 16));
                bw.Close();
                FS.Close();
                msr.Close();
                ms.Close();

                n++;
                fileExtention = new(br.ReadChars(4));
            }
        }
    }
}

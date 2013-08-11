using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace VoxelLandscapeEditor
{
    public static class LoadSave
    {
        public static void Save(string fn, World gameWorld)
        {
            StringBuilder sb = new StringBuilder();

            using (StringWriter str = new StringWriter(sb))
            {
                str.Write(gameWorld.X_CHUNKS + "," + gameWorld.Y_CHUNKS + "," + gameWorld.Z_CHUNKS + "\n");

                for (int z = 0; z < gameWorld.Z_CHUNKS; z++)
                {
                    for (int y = 0; y < gameWorld.Y_CHUNKS; y++)
                    {
                        for (int x = 0; x < gameWorld.X_CHUNKS; x++)
                        {
                            str.Write("C\n");
                            Chunk c = gameWorld.Chunks[x,y,z];
                            for(int vx=0;vx<Chunk.X_SIZE;vx++)
                                for(int vy=0;vy<Chunk.Y_SIZE;vy++)
                                    for (int vz = 0; vz < Chunk.Z_SIZE; vz++)
                                    {
                                        if (!c.Voxels[vx, vy, vz].Active) continue;

                                        string vox = vx + "," + vy + "," + vz + ",";
                                        vox += ((int)c.Voxels[vx, vy, vz].Type);
                                        str.Write(vox + "\n");
                                    }
                        }
                    }
                }
                str.Flush();
            }

            using (StreamWriter fs = new StreamWriter(fn))
            {
                fs.Write(Compress(sb.ToString()));
                fs.Flush();
            }

            sb.Clear();
            sb = null;

            GC.Collect();
        }

        public static void Load(string fn, ref World gameWorld)
        {
            string fileString = "";

            using (StreamReader fs = new StreamReader(fn))
            {
                fileString = fs.ReadToEnd();
            }

            fileString = Decompress(fileString);

            string[] fileSplit = fileString.Split('\n');
            
            int cl = 0;

            string line;
            string[] split;

            line = fileSplit[0];
            split = line.Split(',');

            gameWorld = new World(Convert.ToInt16(split[0]), Convert.ToInt16(split[1]), Convert.ToInt16(split[2]), false);
            cl+=2;

            for (int z = 0; z < gameWorld.Z_CHUNKS; z++)
            {
                for (int y = 0; y < gameWorld.Y_CHUNKS; y++)
                {
                    for (int x = 0; x < gameWorld.X_CHUNKS; x++)
                    {
                        Chunk c = gameWorld.Chunks[x, y, z];

                        while (cl<fileSplit.Length)
                        {
                            line = fileSplit[cl];
                            if (line == "C") break;
                            if (line == "") break;

                            split = line.Split(',');

                            c.SetVoxel(Convert.ToInt16(split[0]), Convert.ToInt16(split[1]), Convert.ToInt16(split[2]), true, (VoxelType)(Convert.ToInt16(split[3])), GetTopColor((VoxelType)(Convert.ToInt16(split[3]))), GetSideColor((VoxelType)(Convert.ToInt16(split[3]))));

                            cl++;
                        }

                        cl++;
                    }
                }
            }

            fileSplit = null;
            fileString = null;

            GC.Collect();

        }

        public static string Compress(string text)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(text);
            MemoryStream ms = new MemoryStream();

            using (GZipStream zip = new GZipStream(ms, CompressionMode.Compress, true))
            {
                zip.Write(buffer, 0, buffer.Length);
            }

            ms.Position = 0;
            MemoryStream outStream = new MemoryStream();

            byte[] compressed = new byte[ms.Length];
            ms.Read(compressed, 0, compressed.Length);

            byte[] gzBuffer = new byte[compressed.Length + 4];
            System.Buffer.BlockCopy(compressed, 0, gzBuffer, 4, compressed.Length);
            System.Buffer.BlockCopy(BitConverter.GetBytes(buffer.Length), 0, gzBuffer, 0, 4);

            return Convert.ToBase64String (gzBuffer);
        }

        public static string Decompress(string compressedText)
        {
            byte[] gzBuffer = Convert.FromBase64String(compressedText);

            using (MemoryStream ms = new MemoryStream())
            {
                int msgLength = BitConverter.ToInt32(gzBuffer, 0);
                ms.Write(gzBuffer, 4, gzBuffer.Length - 4);

                byte[] buffer = new byte[msgLength];

                ms.Position = 0;

                using (GZipStream zip = new GZipStream(ms, CompressionMode.Decompress))
                {
                    zip.Read(buffer, 0, buffer.Length);
                }

                return Encoding.UTF8.GetString(buffer);
            } 
        }


        static Color GetTopColor(VoxelType type)
        {
            switch (type)
            {
                case VoxelType.Ground:
                    return new Color(0f, 0.5f + ((float)Helper.Random.NextDouble() * 0.1f), 0f);
                case VoxelType.Tree:
                    return new Color(0.4f + ((float)Helper.Random.NextDouble() * 0.1f), 0.1f + ((float)Helper.Random.NextDouble() * 0.05f), 0.05f);
                case VoxelType.Leaf:
                    return new Color(0.2f, 0.8f + ((float)Helper.Random.NextDouble() * 0.1f), 0.2f);
            }

            return Color.White;
        }

        static Color GetSideColor(VoxelType type)
        {
            switch (type)
            {
                case VoxelType.Ground:
                    return new Color(0f, 0.3f, 0f);
                case VoxelType.Tree:
                    return new Color(0.4f + ((float)Helper.Random.NextDouble() * 0.1f), 0.1f + ((float)Helper.Random.NextDouble() * 0.05f), 0.05f);
                case VoxelType.Leaf:
                    return new Color(0.2f, 0.5f + ((float)Helper.Random.NextDouble() * 0.1f), 0.2f);
            }

            return Color.White;
        }
        
    }
}

using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.Windows.Forms;

namespace VoxelLandscapeEditor
{
    public static class LoadSave
    {
        public static void Save(World gameWorld)
        {
            //StringBuilder sb = new StringBuilder();

            //using (StringWriter str = new StringWriter(sb))
            //{

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.AddExtension = true;
            sfd.DefaultExt = ".vxl";
            sfd.Filter = "Voxel Landscape|*.vxl";
            DialogResult dr = sfd.ShowDialog();

            if (string.IsNullOrEmpty(sfd.FileName) || dr != DialogResult.OK) return;


            byte[] memBytes;
            //using (FileStream str = new FileStream(sfd.FileName, FileMode.Create))
            using (MemoryStream str = new MemoryStream())
            {
                var sw = new StreamWriter(str);
                sw.WriteLine(gameWorld.CodeName);
                sw.WriteLine(gameWorld.DisplayName);
                sw.Flush();

                str.WriteByte((byte)gameWorld.Type);
                str.WriteByte((byte)gameWorld.Theme);
                //str.Write(gameWorld.X_CHUNKS + "," + gameWorld.Y_CHUNKS + "," + gameWorld.Z_CHUNKS + "\n");
                //using (GZipStream gzstr = new GZipStream(str, CompressionMode.Compress))
                //{
                
                    str.WriteByte(Convert.ToByte(gameWorld.X_CHUNKS));
                    str.WriteByte(Convert.ToByte(gameWorld.Y_CHUNKS));
                    str.WriteByte(Convert.ToByte(gameWorld.Z_CHUNKS));

                    str.WriteByte(Convert.ToByte(gameWorld.Spawns.Count));
                    for (int i = 0; i < gameWorld.Spawns.Count; i++)
                    {
                        str.Write(BitConverter.GetBytes((int)gameWorld.Spawns[i].Position.X),0,4);
                        str.Write(BitConverter.GetBytes((int)gameWorld.Spawns[i].Position.Y), 0, 4);
                        str.Write(BitConverter.GetBytes((int)gameWorld.Spawns[i].Position.Z), 0, 4);
                        str.WriteByte(Convert.ToByte(gameWorld.Spawns[i].Type));
                        str.WriteByte(Convert.ToByte(gameWorld.Spawns[i].Rotation));
                    }

                    for (int z = 0; z < gameWorld.Z_CHUNKS; z++)
                    {
                        for (int y = 0; y < gameWorld.Y_CHUNKS; y++)
                        {
                            for (int x = 0; x < gameWorld.X_CHUNKS; x++)
                            {
                                //str.Write("C\n");

                                Chunk c = gameWorld.Chunks[x, y, z];
                                for (byte vx = 0; vx < Chunk.X_SIZE; vx++)
                                    for (byte vy = 0; vy < Chunk.Y_SIZE; vy++)
                                        for (byte vz = 0; vz < Chunk.Z_SIZE; vz++)
                                        {
                                            if (!c.Voxels[vx, vy, vz].Active) continue;

                                            //string vox = vx + "," + vy + "," + vz + ",";
                                            //vox += ((int)c.Voxels[vx, vy, vz].Type);
                                            //str.Write(vox + "\n");

                                            str.WriteByte(vx);
                                            str.WriteByte(vy);
                                            str.WriteByte(vz);
                                            str.WriteByte((byte)c.Voxels[vx, vy, vz].Type);
                                            str.WriteByte(Convert.ToByte(c.Voxels[vx, vy, vz].Destructable));
                                            str.WriteByte(c.Voxels[vx, vy, vz].TR);
                                            str.WriteByte(c.Voxels[vx, vy, vz].TG);
                                            str.WriteByte(c.Voxels[vx, vy, vz].TB);
                                            str.WriteByte(c.Voxels[vx, vy, vz].SR);
                                            str.WriteByte(c.Voxels[vx, vy, vz].SG);
                                            str.WriteByte(c.Voxels[vx, vy, vz].SB);
                                        }
                                str.WriteByte(Convert.ToByte('c'));
                            }
                        }

                    }

                //}
                str.Flush();
                memBytes = str.ToArray();
            }

            using (FileStream str = new FileStream(sfd.FileName, FileMode.Create))
            {
                using (GZipStream gzstr = new GZipStream(str, CompressionMode.Compress))
                {
                    gzstr.Write(memBytes, 0, memBytes.Length);
                    gzstr.Flush();
                }
            }
            //}

            //using (StreamWriter fs = new StreamWriter(fn))
            //{
            //    fs.Write(Compress(sb.ToString()));
            //    fs.Flush();
            //}

            //sb.Clear();
            //sb = null;

            GC.Collect();
        }

      

        public static void Load(ref World gameWorld)
        {
            //string fileString = "";

            //using (StreamReader fs = new StreamReader(fn))
            //{
            //    fileString = fs.ReadToEnd();
            //}

            //fileString = Decompress(fileString);

            //string[] fileSplit = fileString.Split('\n');
            
            //int cl = 0;

            //string line;
            //string[] split;

            //line = fileSplit[0];
            //split = line.Split(',');

            //cl+=2;
            OpenFileDialog sfd = new OpenFileDialog();
            sfd.AddExtension = true;
            sfd.DefaultExt = ".vxl";
            sfd.Filter = "Voxel Landscape|*.vxl";
            DialogResult dr = sfd.ShowDialog();

            if (string.IsNullOrEmpty(sfd.FileName) || dr != DialogResult.OK) return;


            byte[] buffer;

            using (FileStream gstr = new FileStream(sfd.FileName, FileMode.Open))
            {
                byte[] lb = new byte[4];
                gstr.Position = gstr.Length - 4;
                gstr.Read(lb, 0,4);
                int msgLength = BitConverter.ToInt32(lb, 0);

                buffer = new byte[msgLength];

                gstr.Position = 0;

                using (GZipStream str = new GZipStream(gstr, CompressionMode.Decompress))
                {
                    
                    str.Read(buffer, 0, msgLength);
                }
            }

            int pos = 2;

            var sw = new StreamReader(new MemoryStream(buffer));
            string codename = sw.ReadLine();
            string dispname = sw.ReadLine();
            sw.Close();

            int foundcount = 0;
            while (foundcount < 2)
            {
                pos++;
                if (buffer[pos - 2] == 13 && buffer[pos - 1] == 10) foundcount++;
            }

            int xs = buffer[pos + 2];
            int ys = buffer[pos + 3];
            int zs = buffer[pos + 4];
            gameWorld = new World(50, 50, 1, false);
            gameWorld.X_CHUNKS = xs;
            gameWorld.Y_CHUNKS = ys;
            gameWorld.Z_CHUNKS = 1;

            gameWorld.Type = (MapType)buffer[pos];
            gameWorld.Theme = (Theme)buffer[pos + 1];

            int numSpawns = buffer[pos + 5];

            pos += 6;

            for (int i = 0; i < numSpawns; i++)
            {
                Spawn s = new Spawn();
                s.Position = new Vector3(BitConverter.ToInt32(buffer, pos), BitConverter.ToInt32(buffer, pos + 4), BitConverter.ToInt32(buffer, pos + 8));
                s.Type = (SpawnType)buffer[pos + 12];
                s.Rotation = buffer[pos + 13];
                gameWorld.Spawns.Add(s);
                pos += 14;
            }

            for (int z = 0; z < gameWorld.Z_CHUNKS; z++)
            {
                for (int y = 0; y < gameWorld.Y_CHUNKS; y++)
                {
                    for (int x = 0; x < gameWorld.X_CHUNKS; x++)
                    {
                        Chunk c = gameWorld.Chunks[x, y, z];

                        while (pos < buffer.Length)
                        {
                            if (Convert.ToChar(buffer[pos]) != 'c')
                            {
                                //str.Seek(-1, SeekOrigin.Current);
                                //str.Read(ba, 0, 10);
                                int vx = buffer[pos];
                                int vy = buffer[pos + 1];
                                int vz = buffer[pos + 2];
                                VoxelType type = (VoxelType)buffer[pos + 3];
                                short destruct = buffer[pos + 4];
                                Color top = new Color(buffer[pos + 5], buffer[pos + 6], buffer[pos + 7]);
                                Color side = new Color(buffer[pos + 8], buffer[pos + 9], buffer[pos + 10]);

                                c.SetVoxel(vx, vy, vz, true, destruct, type, top, side);
                                pos += 11;
                            }
                            else
                            {
                                pos++;
                                break;
                            }

                        }

                        //while (cl<fileSplit.Length)
                        //{
                        //    line = fileSplit[cl];
                        //    if (line == "C") break;
                        //    if (line == "") break;

                        //    split = line.Split(',');

                        //    c.SetVoxel(Convert.ToInt16(split[0]), Convert.ToInt16(split[1]), Convert.ToInt16(split[2]), true, (VoxelType)(Convert.ToInt16(split[3])), GetTopColor((VoxelType)(Convert.ToInt16(split[3]))), GetSideColor((VoxelType)(Convert.ToInt16(split[3]))));

                        //    cl++;
                        //}

                        //cl++;
                    }
                }

            }

            //fileSplit = null;
            //fileString = null;

            gameWorld.UpdateWorldMeshes();

            GC.Collect();

        }

        public static PrefabChunk LoadPrefab(string fn)
        {
            PrefabChunk prefab;

            byte[] buffer;

            using (FileStream gstr = new FileStream(fn, FileMode.Open))
            {
                byte[] lb = new byte[4];
                gstr.Position = gstr.Length - 4;
                gstr.Read(lb, 0, 4);
                int msgLength = BitConverter.ToInt32(lb, 0);

                buffer = new byte[msgLength];

                gstr.Position = 0;

                using (GZipStream str = new GZipStream(gstr, CompressionMode.Decompress))
                {

                    str.Read(buffer, 0, msgLength);
                }
            }

            int pos = 0;

            int xs = buffer[0];
            int ys = buffer[1];
            int zs = buffer[2];
            int frames = buffer[3];
            prefab = new PrefabChunk(xs, ys, zs);

            pos = 4;

            for (int i = 0; i < 10; i++)
            {
                pos += 3;
            }

            while (pos < buffer.Length)
            {
                if (Convert.ToChar(buffer[pos]) != 'c')
                {
                    int vx = buffer[pos];
                    int vy = buffer[pos + 1];
                    int vz = buffer[pos + 2];
                    Color top = new Color(buffer[pos + 3], buffer[pos + 4], buffer[pos + 5]);

                    prefab.SetVoxel(vx, (zs-1)-vz, vy, true, VoxelType.Prefab, top);
                    pos += 6;

                }
                else
                {
                    pos++;
                    break;
                }

            }

            prefab.UpdateMesh();

            GC.Collect();

            return prefab;
        }

        public static void LoadAnim(ref VoxelSprite sprite, string fn)
        {
          
           
            byte[] buffer;

            using (FileStream gstr = new FileStream(fn, FileMode.Open))
            {
                byte[] lb = new byte[4];
                gstr.Position = gstr.Length - 4;
                gstr.Read(lb, 0, 4);
                int msgLength = BitConverter.ToInt32(lb, 0);

                buffer = new byte[msgLength];

                gstr.Position = 0;

                using (GZipStream str = new GZipStream(gstr, CompressionMode.Decompress))
                {

                    str.Read(buffer, 0, msgLength);
                }
            }

            int pos = 0;

            int xs = buffer[0];
            int ys = buffer[1];
            int zs = buffer[2];
            int frames = buffer[3];
            sprite = new VoxelSprite(xs, ys, zs);
            sprite.AnimChunks.Clear();
            sprite.ChunkRTs.Clear();

            pos = 4;

            for (int i = 0; i < 10; i++)
            {
                
                pos += 3;
            }


            for (int frame = 0; frame < frames; frame++)
            {
                sprite.AddFrame(false);

                AnimChunk c = sprite.AnimChunks[frame];

                while (pos < buffer.Length)
                {
                    if (Convert.ToChar(buffer[pos]) != 'c')
                    {
                        //str.Seek(-1, SeekOrigin.Current);
                        //str.Read(ba, 0, 10);
                        int vx = buffer[pos];
                        int vy = buffer[pos + 1];
                        int vz = buffer[pos + 2];
                        Color top = new Color(buffer[pos + 3], buffer[pos + 4], buffer[pos + 5]);

                        c.SetVoxel(vx, (zs-1)-vz, vy, true, top);
                        pos += 6;

                    }
                    else
                    {
                        pos++;
                        break;
                    }

                }

                c.UpdateMesh();

            }

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

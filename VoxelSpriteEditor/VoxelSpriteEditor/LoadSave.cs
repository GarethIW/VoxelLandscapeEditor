using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.Windows.Forms;
using Microsoft.Xna.Framework.Graphics;

namespace VoxelSpriteEditor
{
    public static class LoadSave
    {
        public static void Save(VoxelSprite sprite, ref Color[] swatches)
        {
            //StringBuilder sb = new StringBuilder();

            //using (StringWriter str = new StringWriter(sb))
            //{

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.AddExtension = true;
            sfd.DefaultExt = ".vxl";
            sfd.Filter = "Voxel Sprite|*.vxs";
            DialogResult dr = sfd.ShowDialog();

            if (string.IsNullOrEmpty(sfd.FileName) || dr != DialogResult.OK) return;


            using (FileStream str = new FileStream(sfd.FileName, FileMode.Create))
            {
                //str.Write(gameWorld.X_CHUNKS + "," + gameWorld.Y_CHUNKS + "," + gameWorld.Z_CHUNKS + "\n");
                using (GZipStream gzstr = new GZipStream(str, CompressionMode.Compress))
                {

                    gzstr.WriteByte(Convert.ToByte(sprite.X_SIZE));
                    gzstr.WriteByte(Convert.ToByte(sprite.Y_SIZE));
                    gzstr.WriteByte(Convert.ToByte(sprite.Z_SIZE));
                    gzstr.WriteByte(Convert.ToByte(sprite.AnimChunks.Count));

                    for (int i = 0; i < 10; i++)
                    {
                        gzstr.WriteByte(swatches[i].R);
                        gzstr.WriteByte(swatches[i].G);
                        gzstr.WriteByte(swatches[i].B);
                    }

                    foreach (AnimChunk c in sprite.AnimChunks)
                    {
                        //str.Write("C\n");

                        //Chunk c = gameWorld.Chunks[x, y, z];
                        for (int vx = 0; vx < sprite.X_SIZE; vx++)
                            for (int vy = 0; vy < sprite.Y_SIZE; vy++)
                                for (int vz = 0; vz < sprite.Z_SIZE; vz++)
                                {
                                    if (!c.Voxels[vx, vy, vz].Active) continue;

                                    //string vox = vx + "," + vy + "," + vz + ",";
                                    //vox += ((int)c.Voxels[vx, vy, vz].Type);
                                    //str.Write(vox + "\n");

                                    gzstr.WriteByte(Convert.ToByte(vx));
                                    gzstr.WriteByte(Convert.ToByte(vy));
                                    gzstr.WriteByte(Convert.ToByte(vz));
                                    gzstr.WriteByte(c.Voxels[vx, vy, vz].Color.R);
                                    gzstr.WriteByte(c.Voxels[vx, vy, vz].Color.G);
                                    gzstr.WriteByte(c.Voxels[vx, vy, vz].Color.B);
                                }
                        gzstr.WriteByte(Convert.ToByte('c'));


                    }
                    //str.Flush();

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



        public static void Load(ref VoxelSprite sprite, GraphicsDevice gd, ref Color[] swatches)
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
            sfd.DefaultExt = ".vxs";
            sfd.Filter = "Voxel Sprite|*.vxs";
            DialogResult dr = sfd.ShowDialog();

            if (string.IsNullOrEmpty(sfd.FileName) || dr != DialogResult.OK) return;


            byte[] buffer;

            using (FileStream gstr = new FileStream(sfd.FileName, FileMode.Open))
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
            sprite = new VoxelSprite(xs, ys, zs, gd);
            sprite.AnimChunks.Clear();
            sprite.ChunkRTs.Clear();

            pos = 4;

            for (int i = 0; i < 10; i++)
            {
                swatches[i] = new Color(buffer[pos], buffer[pos+1],buffer[pos+2]);
                pos += 3;
            }


            for(int frame = 0;frame<frames;frame++)
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

                        c.SetVoxel(vx, vy, vz, true, top);
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

            return Convert.ToBase64String(gzBuffer);
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

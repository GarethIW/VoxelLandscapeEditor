using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace VoxelSpriteEditor
{
    public static class TileSheetConverter
    {

        public static void Load(ref VoxelSprite vs, GraphicsDevice gd)
        {
            OpenFileDialog sfd = new OpenFileDialog();
            sfd.AddExtension = true;
            sfd.DefaultExt = ".png";
            sfd.Filter = "Tilesheet|*.png";
            DialogResult dr = sfd.ShowDialog();

            if (string.IsNullOrEmpty(sfd.FileName) || dr != DialogResult.OK) return;

            Texture2D inTex;

            try
            {
                using (Stream fs = sfd.OpenFile())
                {
                    inTex = Texture2D.FromStream(gd, fs);
                }
            }
            catch (Exception ex)
            {
                return;
            }

            vs.AnimChunks.Clear();

            Color[] colData = new Color[inTex.Width * inTex.Height];
            inTex.GetData(colData);

            int xTiles = inTex.Width / vs.X_SIZE;
            int yTiles = inTex.Height / vs.Y_SIZE;

            int depthIntensity = 7;

            for (int yt = 0; yt < yTiles; yt++)
            {
                for (int xt = 0; xt < xTiles; xt++)
                {
                
                    vs.AddFrame(false);
                    for (int vx = 0; vx < vs.X_SIZE; vx++)
                    {
                        for (int vy = 0; vy < vs.Y_SIZE; vy++)
                        {
                            Color col = colData[((vy + (yt * vs.Y_SIZE)) * inTex.Width) + (vx + (xt*vs.X_SIZE))];
                            if (col.A > 0)
                            {
                                int depth = depthIntensity - (int)(((float)depthIntensity / (255f + 255f + 255f)) * ((float)col.R + (float)col.G + (float)col.B));
                                for (int vz = vs.Z_SIZE-1; vz>(depth-3); vz--)
                                    vs.AnimChunks[vs.AnimChunks.Count-1].SetVoxel(vx, vz, vs.Y_SIZE-1-vy, true, new Color(col.R, col.G, col.B));
                            }
                        }
                    }
                    vs.AnimChunks[vs.AnimChunks.Count - 1].UpdateMesh();
                }
            }

        }
    }
}

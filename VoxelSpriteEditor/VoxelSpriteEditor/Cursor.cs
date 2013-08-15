using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VoxelSpriteEditor
{
    public class Cursor
    {
        const float CURSOR_SIZE = Voxel.SIZE +0.1f;
        const float CURSOR_HALF_SIZE = CURSOR_SIZE / 2f;

        public Voxel[,,] Voxels = new Voxel[32,32,32];

        public List<VertexPositionNormalColor> Vertices = new List<VertexPositionNormalColor>();
        public List<short> Indexes = new List<short>();

        public VertexPositionNormalColor[] VertexArray;
        public short[] IndexArray;

        public Vector3 Position;

        Color cursorColor = Color.White;

        public Vector3 Size = Vector3.One;

        public Cursor()
        {
            UpdateMesh();
        }

        public void Update(GameTime gameTime, Color col)
        {
            if (cursorColor != col)
            {
                cursorColor = col;
                UpdateMesh();
            }
        }

        public void UpdateMesh()
        {
            Vector3 meshCenter = (Vector3.One * Voxel.SIZE) / 2f;
            Vertices.Clear();
            Indexes.Clear();

            for (int x = 0; x < 32; x++)
                for (int y = 0; y < 32; y++)
                    for (int z = 0; z < 32; z++) Voxels[x, y, z].Active = false;

            for(int x=0;x<(int)Size.X;x++)
                for(int y=0;y<(int)Size.Y;y++)
                    for (int z = 0; z < (int)Size.Z; z++)
                    {
                        Voxels[x, y, z].Active = true;

                        Vector3 worldOffset = ((new Vector3(x, y, z) * Voxel.SIZE)) + meshCenter;

                        MakeQuad(worldOffset, new Vector3(-CURSOR_HALF_SIZE, -CURSOR_HALF_SIZE, -CURSOR_HALF_SIZE), new Vector3(CURSOR_HALF_SIZE, -CURSOR_HALF_SIZE, -CURSOR_HALF_SIZE), new Vector3(CURSOR_HALF_SIZE, CURSOR_HALF_SIZE, -CURSOR_HALF_SIZE), new Vector3(-CURSOR_HALF_SIZE, CURSOR_HALF_SIZE, -CURSOR_HALF_SIZE), new Vector3(0f, 0f, -1f),cursorColor*0.5f);
                        MakeQuad(worldOffset, new Vector3(CURSOR_HALF_SIZE, CURSOR_HALF_SIZE, CURSOR_HALF_SIZE), new Vector3(CURSOR_HALF_SIZE, -CURSOR_HALF_SIZE, CURSOR_HALF_SIZE), new Vector3(-CURSOR_HALF_SIZE, -CURSOR_HALF_SIZE, CURSOR_HALF_SIZE), new Vector3(-CURSOR_HALF_SIZE, CURSOR_HALF_SIZE, CURSOR_HALF_SIZE), new Vector3(0f, 0f, 1f), cursorColor * 0.5f);
                        MakeQuad(worldOffset, new Vector3(-CURSOR_HALF_SIZE, -CURSOR_HALF_SIZE, -CURSOR_HALF_SIZE), new Vector3(-CURSOR_HALF_SIZE, CURSOR_HALF_SIZE, -CURSOR_HALF_SIZE), new Vector3(-CURSOR_HALF_SIZE, CURSOR_HALF_SIZE, CURSOR_HALF_SIZE), new Vector3(-CURSOR_HALF_SIZE, -CURSOR_HALF_SIZE, CURSOR_HALF_SIZE), new Vector3(-1f, 0f, 0f), cursorColor * 0.5f);
                        MakeQuad(worldOffset, new Vector3(CURSOR_HALF_SIZE, CURSOR_HALF_SIZE, CURSOR_HALF_SIZE), new Vector3(CURSOR_HALF_SIZE, CURSOR_HALF_SIZE, -CURSOR_HALF_SIZE), new Vector3(CURSOR_HALF_SIZE, -CURSOR_HALF_SIZE, -CURSOR_HALF_SIZE), new Vector3(CURSOR_HALF_SIZE, -CURSOR_HALF_SIZE, CURSOR_HALF_SIZE), new Vector3(1f, 0f, 0f), cursorColor * 0.5f);
                        MakeQuad(worldOffset, new Vector3(-CURSOR_HALF_SIZE, CURSOR_HALF_SIZE, -CURSOR_HALF_SIZE), new Vector3(CURSOR_HALF_SIZE, CURSOR_HALF_SIZE, -CURSOR_HALF_SIZE), new Vector3(CURSOR_HALF_SIZE, CURSOR_HALF_SIZE, CURSOR_HALF_SIZE), new Vector3(-CURSOR_HALF_SIZE, CURSOR_HALF_SIZE, CURSOR_HALF_SIZE), new Vector3(0f, 0f, 1f), cursorColor * 0.5f);
                        MakeQuad(worldOffset, new Vector3(CURSOR_HALF_SIZE, -CURSOR_HALF_SIZE, CURSOR_HALF_SIZE), new Vector3(CURSOR_HALF_SIZE, -CURSOR_HALF_SIZE, -CURSOR_HALF_SIZE), new Vector3(-CURSOR_HALF_SIZE, -CURSOR_HALF_SIZE, -CURSOR_HALF_SIZE), new Vector3(-CURSOR_HALF_SIZE, -CURSOR_HALF_SIZE, CURSOR_HALF_SIZE), new Vector3(0f, 0f, -1f), cursorColor * 0.5f);   
                    }

          

            VertexArray = Vertices.ToArray();
            IndexArray = new short[Indexes.Count];

            for (int ind = 0; ind < Indexes.Count / 6; ind++)
            {
                for (int i = 0; i < 6; i++)
                {
                    IndexArray[(ind * 6) + i] = (short)(Indexes[(ind * 6) + i] + (ind * 4));
                }
            }

            Vertices.Clear();
            Indexes.Clear();
        }

        public void ChangeSize(Vector3 amount, int limit)
        {
            Size += amount;
            Size = Vector3.Clamp(Size, Vector3.One, Vector3.One * limit);
            UpdateMesh();
        }

        void MakeQuad(Vector3 offset, Vector3 tl, Vector3 tr, Vector3 br, Vector3 bl, Vector3 norm, Color col)
        {
            Vertices.Add(new VertexPositionNormalColor(offset + tl, norm, col));
            Vertices.Add(new VertexPositionNormalColor(offset + tr, norm, col));
            Vertices.Add(new VertexPositionNormalColor(offset + br, norm, col));
            Vertices.Add(new VertexPositionNormalColor(offset + bl, norm, col));
            Indexes.Add(0);
            Indexes.Add(1);
            Indexes.Add(2);
            Indexes.Add(2);
            Indexes.Add(3);
            Indexes.Add(0);
        }
    }
}

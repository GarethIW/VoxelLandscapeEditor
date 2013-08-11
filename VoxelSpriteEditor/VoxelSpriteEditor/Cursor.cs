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

        public Voxel Voxel = new Voxel(true,Color.Red * 0.5f);

        public List<VertexPositionNormalColor> Vertices = new List<VertexPositionNormalColor>();
        public List<short> Indexes = new List<short>();

        public VertexPositionNormalColor[] VertexArray;
        public short[] IndexArray;

        public Vector3 Position;

        public Cursor()
        {
            Voxel.Active = true;
            UpdateMesh();
        }

        public void Update(GameTime gameTime)
        {

        }

        public void UpdateMesh()
        {
            Vector3 meshCenter = (Vector3.One * Voxel.SIZE) / 2f;
            Vertices.Clear();
            Indexes.Clear();

            Vector3 worldOffset = meshCenter;

            MakeQuad(worldOffset, new Vector3(-CURSOR_HALF_SIZE, -CURSOR_HALF_SIZE, -CURSOR_HALF_SIZE), new Vector3(CURSOR_HALF_SIZE, -CURSOR_HALF_SIZE, -CURSOR_HALF_SIZE), new Vector3(CURSOR_HALF_SIZE, CURSOR_HALF_SIZE, -CURSOR_HALF_SIZE), new Vector3(-CURSOR_HALF_SIZE, CURSOR_HALF_SIZE, -CURSOR_HALF_SIZE), new Vector3(0f, 0f, -1f), Color.Red*0.5f);
            MakeQuad(worldOffset, new Vector3(CURSOR_HALF_SIZE, CURSOR_HALF_SIZE, CURSOR_HALF_SIZE), new Vector3(CURSOR_HALF_SIZE, -CURSOR_HALF_SIZE, CURSOR_HALF_SIZE), new Vector3(-CURSOR_HALF_SIZE, -CURSOR_HALF_SIZE, CURSOR_HALF_SIZE), new Vector3(-CURSOR_HALF_SIZE, CURSOR_HALF_SIZE, CURSOR_HALF_SIZE), new Vector3(0f, 0f, 1f), Color.Red * 0.5f);
            MakeQuad(worldOffset, new Vector3(-CURSOR_HALF_SIZE, -CURSOR_HALF_SIZE, -CURSOR_HALF_SIZE), new Vector3(-CURSOR_HALF_SIZE, CURSOR_HALF_SIZE, -CURSOR_HALF_SIZE), new Vector3(-CURSOR_HALF_SIZE, CURSOR_HALF_SIZE, CURSOR_HALF_SIZE), new Vector3(-CURSOR_HALF_SIZE, -CURSOR_HALF_SIZE, CURSOR_HALF_SIZE), new Vector3(-1f, 0f, 0f), Color.Red * 0.5f);
            MakeQuad(worldOffset, new Vector3(CURSOR_HALF_SIZE, CURSOR_HALF_SIZE, CURSOR_HALF_SIZE), new Vector3(CURSOR_HALF_SIZE, CURSOR_HALF_SIZE, -CURSOR_HALF_SIZE), new Vector3(CURSOR_HALF_SIZE, -CURSOR_HALF_SIZE, -CURSOR_HALF_SIZE), new Vector3(CURSOR_HALF_SIZE, -CURSOR_HALF_SIZE, CURSOR_HALF_SIZE), new Vector3(1f, 0f, 0f), Color.Red * 0.5f);
            MakeQuad(worldOffset, new Vector3(-CURSOR_HALF_SIZE, CURSOR_HALF_SIZE, -CURSOR_HALF_SIZE), new Vector3(CURSOR_HALF_SIZE, CURSOR_HALF_SIZE, -CURSOR_HALF_SIZE), new Vector3(CURSOR_HALF_SIZE, CURSOR_HALF_SIZE, CURSOR_HALF_SIZE), new Vector3(-CURSOR_HALF_SIZE, CURSOR_HALF_SIZE, CURSOR_HALF_SIZE), new Vector3(0f, 0f, 1f), Color.Red * 0.5f);
            MakeQuad(worldOffset, new Vector3(CURSOR_HALF_SIZE, -CURSOR_HALF_SIZE, CURSOR_HALF_SIZE), new Vector3(CURSOR_HALF_SIZE, -CURSOR_HALF_SIZE, -CURSOR_HALF_SIZE), new Vector3(-CURSOR_HALF_SIZE, -CURSOR_HALF_SIZE, -CURSOR_HALF_SIZE), new Vector3(-CURSOR_HALF_SIZE, -CURSOR_HALF_SIZE, CURSOR_HALF_SIZE), new Vector3(0f, 0f, -1f), Color.Red * 0.5f);   

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

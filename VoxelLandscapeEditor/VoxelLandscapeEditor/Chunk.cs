using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace VoxelLandscapeEditor
{
    public class Chunk
    {
        public const int X_SIZE = 32, Y_SIZE = 32, Z_SIZE = 32;

        public Voxel[, ,] Voxels = new Voxel[X_SIZE,Y_SIZE,Z_SIZE];

        public List<VertexPositionNormalColor> Vertices = new List<VertexPositionNormalColor>();
        public List<short> Indexes = new List<short>();

        public VertexPositionNormalColor[] VertexArray;
        public short[] IndexArray;

        World parentWorld;
        public int worldX, worldY, worldZ;

        public BoundingSphere boundingSphere;

        public bool Visible = false;
        public bool Updated = false;

        
        public Chunk(World world, int wx, int wy, int wz, bool createGround)
        {
            parentWorld = world;
            worldX = wx;
            worldY = wy;
            worldZ = wz;

            boundingSphere = new BoundingSphere(new Vector3(worldX * (X_SIZE * Voxel.SIZE), worldY * (Y_SIZE * Voxel.SIZE), worldZ * (Z_SIZE * Voxel.SIZE)) + (new Vector3(X_SIZE * Voxel.SIZE, Y_SIZE * Voxel.SIZE, Z_SIZE * Voxel.SIZE) / 2f), (X_SIZE * Voxel.SIZE)/1.5f);

            if (createGround)
            {
                for (int y = 0; y < Y_SIZE; y++)
                    for (int x = 0; x < X_SIZE; x++)
                    {
                        for (int z = Chunk.Z_SIZE - 1; z >= Chunk.Z_SIZE - 5; z--)
                        {
                            SetVoxel(x, y, z, true, VoxelType.Ground, new Color(0f, 0.5f + ((float)Helper.Random.NextDouble() * 0.1f), 0f), new Color(0f, 0.3f, 0f));
                        }
                    }
            }
        }

        public void SetVoxel(int x, int y, int z, bool active, VoxelType type, Color top, Color side)
        {
            if (x < 0 || y < 0 || z < 0 || x >= X_SIZE || y >= Y_SIZE || z >= Z_SIZE) return;

            Voxels[x, y, z].Active = active;
            Voxels[x, y, z].Type = type;
            Voxels[x, y, z].TR = top.R;
            Voxels[x, y, z].TG = top.G;
            Voxels[x, y, z].TB = top.B;
            Voxels[x, y, z].SR = side.R;
            Voxels[x, y, z].SG = side.G;
            Voxels[x, y, z].SB = side.B;
            //= new Voxel(active, type, top, side);

            Updated = true;
        }

        public void UpdateMesh()
        {
            Vector3 meshCenter = (new Vector3(X_SIZE, Y_SIZE, Z_SIZE) * Voxel.SIZE) / 2f;
            Vertices.Clear();
            Indexes.Clear();

            for (int z = Z_SIZE - 1; z >= 0; z--)
                for (int y = Y_SIZE-1; y >=0; y--)
                     for(int x=0;x<X_SIZE;x++)
                    {
                        if (Voxels[x, y, z].Active == false) continue;

                        Vector3 worldOffset = new Vector3(worldX*(X_SIZE*Voxel.SIZE), worldY*(Y_SIZE*Voxel.SIZE),worldZ*(Z_SIZE*Voxel.SIZE)) + ((new Vector3(x, y, z) * Voxel.SIZE));// - meshCenter);

                        if (!IsVoxelAt(x, y, z - 1)) MakeQuad(worldOffset, new Vector3(-Voxel.HALF_SIZE, -Voxel.HALF_SIZE, -Voxel.HALF_SIZE), new Vector3(Voxel.HALF_SIZE, -Voxel.HALF_SIZE, -Voxel.HALF_SIZE), new Vector3(Voxel.HALF_SIZE, Voxel.HALF_SIZE, -Voxel.HALF_SIZE), new Vector3(-Voxel.HALF_SIZE, Voxel.HALF_SIZE, -Voxel.HALF_SIZE), new Vector3(0f, 0f, -1f), CalcLighting(x,y,z, new Color(Voxels[x, y, z].TR, Voxels[x, y, z].TG,Voxels[x, y, z].TB)));
                        //if (!IsVoxelAt(x, y, z + 1)) MakeQuad(worldOffset, new Vector3(Voxel.HALF_SIZE, Voxel.HALF_SIZE, Voxel.HALF_SIZE), new Vector3(Voxel.HALF_SIZE, -Voxel.HALF_SIZE, Voxel.HALF_SIZE), new Vector3(-Voxel.HALF_SIZE, -Voxel.HALF_SIZE, Voxel.HALF_SIZE), new Vector3(-Voxel.HALF_SIZE, Voxel.HALF_SIZE, Voxel.HALF_SIZE), new Vector3(0f, 0f, 1f), Voxels[x, y, z].Color);
                        if (!IsVoxelAt(x - 1, y, z)) MakeQuad(worldOffset, new Vector3(-Voxel.HALF_SIZE, -Voxel.HALF_SIZE, -Voxel.HALF_SIZE), new Vector3(-Voxel.HALF_SIZE, Voxel.HALF_SIZE, -Voxel.HALF_SIZE), new Vector3(-Voxel.HALF_SIZE, Voxel.HALF_SIZE, Voxel.HALF_SIZE), new Vector3(-Voxel.HALF_SIZE, -Voxel.HALF_SIZE, Voxel.HALF_SIZE), new Vector3(-1f, 0f, 0f), CalcLighting(x - 1, y, z, new Color(Voxels[x, y, z].SR, Voxels[x, y, z].SG, Voxels[x, y, z].SB)));
                        if (!IsVoxelAt(x + 1, y, z)) MakeQuad(worldOffset, new Vector3(Voxel.HALF_SIZE, Voxel.HALF_SIZE, Voxel.HALF_SIZE), new Vector3(Voxel.HALF_SIZE, Voxel.HALF_SIZE, -Voxel.HALF_SIZE), new Vector3(Voxel.HALF_SIZE, -Voxel.HALF_SIZE, -Voxel.HALF_SIZE), new Vector3(Voxel.HALF_SIZE, -Voxel.HALF_SIZE, Voxel.HALF_SIZE), new Vector3(1f, 0f, 0f), CalcLighting(x + 1, y, z, new Color(Voxels[x, y, z].SR, Voxels[x, y, z].SG, Voxels[x, y, z].SB)));
                        if (!IsVoxelAt(x, y + 1, z)) MakeQuad(worldOffset, new Vector3(-Voxel.HALF_SIZE, Voxel.HALF_SIZE, -Voxel.HALF_SIZE), new Vector3(Voxel.HALF_SIZE, Voxel.HALF_SIZE, -Voxel.HALF_SIZE), new Vector3(Voxel.HALF_SIZE, Voxel.HALF_SIZE, Voxel.HALF_SIZE), new Vector3(-Voxel.HALF_SIZE, Voxel.HALF_SIZE, Voxel.HALF_SIZE), new Vector3(0f, 0f, 1f), CalcLighting(x, y + 1, z, new Color(Voxels[x, y, z].SR, Voxels[x, y, z].SG, Voxels[x, y, z].SB)));
                        //if (!IsVoxelAt(x, y - 1, z)) MakeQuad(worldOffset, new Vector3(Voxel.HALF_SIZE, -Voxel.HALF_SIZE, Voxel.HALF_SIZE), new Vector3(Voxel.HALF_SIZE, -Voxel.HALF_SIZE, -Voxel.HALF_SIZE), new Vector3(-Voxel.HALF_SIZE, -Voxel.HALF_SIZE, -Voxel.HALF_SIZE), new Vector3(-Voxel.HALF_SIZE, -Voxel.HALF_SIZE, Voxel.HALF_SIZE), new Vector3(0f, 0f, -1f), CalcLighting(x, y-1, z, Voxels[x, y, z].SideColor)); 
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

            Updated = false;
        }

        bool[] lightingDirs = new bool[9];
        Color CalcLighting(int x, int y, int z, Color currentColor)
        {
            if (x < 0 || y < 0 || z - 1 < 0 || x >= X_SIZE || y >= Y_SIZE || z - 1 >= Z_SIZE) return currentColor;

            Vector3 colVect = currentColor.ToVector3();
            float intensity = 0.05f;
            float light = 1f;

            for (int i = 0; i < 9; i++) lightingDirs[i] = false;

            for (int zz = 1; zz < 7; zz++)
            {
                intensity = (0.11f / 7f) * (7f - (float)zz);
                if ((!lightingDirs[0]) && IsVoxelAt(x, y, z - zz)) { light -= (intensity * 4f); lightingDirs[0] = true; }
                if ((!lightingDirs[1]) && IsVoxelAt(x - zz, y - zz, z - zz)) { light -= intensity; lightingDirs[1] = true; }
                if ((!lightingDirs[2]) && IsVoxelAt(x, y - zz, z - zz)) { light -= intensity; lightingDirs[2] = true; }
                if ((!lightingDirs[3]) && IsVoxelAt(x + zz, y - zz, z - zz)) { light -= intensity; lightingDirs[3] = true; }
                if ((!lightingDirs[4]) && IsVoxelAt(x - zz, y, z - zz)) { light -= intensity; lightingDirs[4] = true; }
                if ((!lightingDirs[5]) && IsVoxelAt(x + zz, y, z - zz)) { light -= intensity; lightingDirs[5] = true; }
                if ((!lightingDirs[6]) && IsVoxelAt(x - zz, y + zz, z - zz)) { light -= intensity; lightingDirs[6] = true; }
                if ((!lightingDirs[7]) && IsVoxelAt(x, y + zz, z - zz)) { light -= intensity; lightingDirs[7] = true; }
                if ((!lightingDirs[8]) && IsVoxelAt(x + zz, y + zz, z - zz)) { light -= intensity; lightingDirs[8] = true; }
            }

            light = MathHelper.Clamp(light, 0f, 1f);

            return new Color(colVect * light);
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

        public bool IsVoxelAt(int x, int y, int z)
        {
            if (x >= 0 && x < X_SIZE && y >= 0 && y < Y_SIZE && z >= 0 && z < Z_SIZE) return Voxels[x, y, z].Active;

            if (x < 0)
                if (worldX == 0) return false;
                else return parentWorld.Chunks[worldX - 1, worldY, worldZ].IsVoxelAt(X_SIZE + x, y, z);

            if (x >= X_SIZE)
                if (worldX >= parentWorld.X_CHUNKS - 1) return false;
                else return parentWorld.Chunks[worldX + 1, worldY, worldZ].IsVoxelAt(X_SIZE - x, y, z);

            if (y < 0)
                if (worldY == 0) return false;
                else return parentWorld.Chunks[worldX, worldY - 1, worldZ].IsVoxelAt(x, Y_SIZE + y, z);

            if (y >= Y_SIZE)
                if (worldY >= parentWorld.Y_CHUNKS - 1) return false;
                else return parentWorld.Chunks[worldX, worldY + 1, worldZ].IsVoxelAt(x, Y_SIZE - y, z);

            if (z < 0)
                if (worldZ == 0) return false;
                else return parentWorld.Chunks[worldX, worldY, worldZ - 1].IsVoxelAt(x, y, Z_SIZE + z);

            if (z >= Z_SIZE)
                if (worldZ >= parentWorld.Z_CHUNKS - 1) return false;
                else return parentWorld.Chunks[worldX, worldY, worldZ + 1].IsVoxelAt(x, y, Z_SIZE - z);

            return false;
        }

        public void ClearMem()
        {
            VertexArray = null;
            IndexArray = null;
            
        }
    }
}

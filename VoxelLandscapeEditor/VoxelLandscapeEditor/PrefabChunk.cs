using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace VoxelLandscapeEditor
{
    public class PrefabChunk
    {
        public int X_SIZE = 32, Y_SIZE = 32, Z_SIZE = 32;

        public Voxel[, ,] Voxels;

        public List<VertexPositionNormalColor> Vertices = new List<VertexPositionNormalColor>();
        public List<short> Indexes = new List<short>();

        public VertexPositionNormalColor[] VertexArray;
        public short[] IndexArray;

        World parentWorld;
        public int worldX, worldY, worldZ;

        public BoundingSphere boundingSphere;

        public bool Visible = false;
        public bool Updated = false;

        int rot = 0;

        public PrefabChunk(int xs, int ys, int zs)
        {
            X_SIZE = xs;
            Y_SIZE = ys;
            Z_SIZE = zs;

            Voxels = new Voxel[X_SIZE,Y_SIZE,Z_SIZE];

        }

        public void SetVoxel(int x, int y, int z, bool active, VoxelType type, Color col)
        {
            if (x < 0 || y < 0 || z < 0 || x >= X_SIZE || y >= Y_SIZE || z >= Z_SIZE) return;

            Color top = col;
            Color side = new Color(col.ToVector3() * 0.75f);

            Voxels[x, y, z].Active = active;
            Voxels[x, y, z].Type = type;
            Voxels[x, y, z].TR = top.R;
            Voxels[x, y, z].TG = top.G;
            Voxels[x, y, z].TB = top.B;
            Voxels[x, y, z].SR = side.R;
            Voxels[x, y, z].SG = side.G;
            Voxels[x, y, z].SB = side.B;

            Updated = true;
        }

        public void UpdateMesh()
        {
            Vector3 meshCenter = new Vector3(X_SIZE * Voxel.HALF_SIZE, Y_SIZE * Voxel.HALF_SIZE, Z_SIZE*Voxel.SIZE) - new Vector3(Voxel.HALF_SIZE,Voxel.HALF_SIZE,0f);
            Vertices.Clear();
            Indexes.Clear();

            for (int z = Z_SIZE - 1; z >= 0; z--)
                for (int y = Y_SIZE-1; y >=0; y--)
                     for(int x=0;x<X_SIZE;x++)
                    {
                        if (Voxels[x, y, z].Active == false) continue;

                        Vector3 worldOffset = new Vector3(worldX*(X_SIZE*Voxel.SIZE), worldY*(Y_SIZE*Voxel.SIZE),worldZ*(Z_SIZE*Voxel.SIZE)) + ((new Vector3(x, y, z) * Voxel.SIZE)) - meshCenter;

                        if (!IsVoxelAt(x, y, z - 1)) MakeQuad(worldOffset, new Vector3(-Voxel.HALF_SIZE, -Voxel.HALF_SIZE, -Voxel.HALF_SIZE), new Vector3(Voxel.HALF_SIZE, -Voxel.HALF_SIZE, -Voxel.HALF_SIZE), new Vector3(Voxel.HALF_SIZE, Voxel.HALF_SIZE, -Voxel.HALF_SIZE), new Vector3(-Voxel.HALF_SIZE, Voxel.HALF_SIZE, -Voxel.HALF_SIZE), new Vector3(0f, 0f, -1f), new Color(Voxels[x, y, z].TR, Voxels[x, y, z].TG,Voxels[x, y, z].TB));
                        if (!IsVoxelAt(x, y, z + 1)) MakeQuad(worldOffset, new Vector3(Voxel.HALF_SIZE, Voxel.HALF_SIZE, Voxel.HALF_SIZE), new Vector3(Voxel.HALF_SIZE, -Voxel.HALF_SIZE, Voxel.HALF_SIZE), new Vector3(-Voxel.HALF_SIZE, -Voxel.HALF_SIZE, Voxel.HALF_SIZE), new Vector3(-Voxel.HALF_SIZE, Voxel.HALF_SIZE, Voxel.HALF_SIZE), new Vector3(0f, 0f, 1f), new Color(Voxels[x, y, z].SR, Voxels[x, y, z].SG, Voxels[x, y, z].SB));
                        if (!IsVoxelAt(x - 1, y, z)) MakeQuad(worldOffset, new Vector3(-Voxel.HALF_SIZE, -Voxel.HALF_SIZE, -Voxel.HALF_SIZE), new Vector3(-Voxel.HALF_SIZE, Voxel.HALF_SIZE, -Voxel.HALF_SIZE), new Vector3(-Voxel.HALF_SIZE, Voxel.HALF_SIZE, Voxel.HALF_SIZE), new Vector3(-Voxel.HALF_SIZE, -Voxel.HALF_SIZE, Voxel.HALF_SIZE), new Vector3(-1f, 0f, 0f), new Color(Voxels[x, y, z].SR, Voxels[x, y, z].SG, Voxels[x, y, z].SB));
                        if (!IsVoxelAt(x + 1, y, z)) MakeQuad(worldOffset, new Vector3(Voxel.HALF_SIZE, Voxel.HALF_SIZE, Voxel.HALF_SIZE), new Vector3(Voxel.HALF_SIZE, Voxel.HALF_SIZE, -Voxel.HALF_SIZE), new Vector3(Voxel.HALF_SIZE, -Voxel.HALF_SIZE, -Voxel.HALF_SIZE), new Vector3(Voxel.HALF_SIZE, -Voxel.HALF_SIZE, Voxel.HALF_SIZE), new Vector3(1f, 0f, 0f), new Color(Voxels[x, y, z].SR, Voxels[x, y, z].SG, Voxels[x, y, z].SB));
                        if (!IsVoxelAt(x, y + 1, z)) MakeQuad(worldOffset, new Vector3(-Voxel.HALF_SIZE, Voxel.HALF_SIZE, -Voxel.HALF_SIZE), new Vector3(Voxel.HALF_SIZE, Voxel.HALF_SIZE, -Voxel.HALF_SIZE), new Vector3(Voxel.HALF_SIZE, Voxel.HALF_SIZE, Voxel.HALF_SIZE), new Vector3(-Voxel.HALF_SIZE, Voxel.HALF_SIZE, Voxel.HALF_SIZE), new Vector3(0f, 0f, 1f), new Color(Voxels[x, y, z].SR, Voxels[x, y, z].SG, Voxels[x, y, z].SB));
                        if (!IsVoxelAt(x, y - 1, z)) MakeQuad(worldOffset, new Vector3(Voxel.HALF_SIZE, -Voxel.HALF_SIZE, Voxel.HALF_SIZE), new Vector3(Voxel.HALF_SIZE, -Voxel.HALF_SIZE, -Voxel.HALF_SIZE), new Vector3(-Voxel.HALF_SIZE, -Voxel.HALF_SIZE, -Voxel.HALF_SIZE), new Vector3(-Voxel.HALF_SIZE, -Voxel.HALF_SIZE, Voxel.HALF_SIZE), new Vector3(0f, 0f, -1f), new Color(Voxels[x, y, z].SR, Voxels[x, y, z].SG, Voxels[x, y, z].SB)); 
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

        public void Rotate()
        {
            Voxel[,,] temp = new Voxel[X_SIZE,Y_SIZE,Z_SIZE];

            rot ++;
            if(rot>3) rot = 0;

            for (int x = 0; x < X_SIZE; x++)
            {
                for (int y = 0; y < Y_SIZE; y++)
                {
                    for (int z = 0; z < Z_SIZE; z++)
                    {
                        if (Voxels[x, y, z].Active)
                        {
                            
                            //if (rot == 0)
                            //{
                            //    temp[y, x, z].Active = true;
                            //    temp[y, x, z].Type = VoxelType.Ground;
                            //    temp[y, x, z].TR = Voxels[x, y, z].TR;
                            //    temp[y, x, z].TG = Voxels[x, y, z].TG;
                            //    temp[y, x, z].TB = Voxels[x, y, z].TB;
                            //    temp[y, x, z].SR = Voxels[x, y, z].SR;
                            //    temp[y, x, z].SG = Voxels[x, y, z].SG;
                            //    temp[y, x, z].SB = Voxels[x, y, z].SB;
                            //}
                            //if (rot == 3)
                            //{
                            //    temp[(Y_SIZE-1)-y, x, z].Active = true;
                            //    temp[(Y_SIZE - 1)-y, x, z].Type = VoxelType.Ground;
                            //    temp[(Y_SIZE - 1) - y, x, z].TR = Voxels[x, y, z].TR;
                            //    temp[(Y_SIZE - 1) - y, x, z].TG = Voxels[x, y, z].TG;
                            //    temp[(Y_SIZE - 1) - y, x, z].TB = Voxels[x, y, z].TB;
                            //    temp[(Y_SIZE - 1) - y, x, z].SR = Voxels[x, y, z].SR;
                            //    temp[(Y_SIZE - 1) - y, x, z].SG = Voxels[x, y, z].SG;
                            //    temp[(Y_SIZE - 1) - y, x, z].SB = Voxels[x, y, z].SB;
                            //}
                            //if (rot == 1 || rot==3)
                            //{
                                temp[y, (X_SIZE - 1)-x, z].Active = true;
                                temp[y, (X_SIZE - 1)-x, z].Type = VoxelType.Prefab;
                                temp[y, (X_SIZE - 1)-x, z].TR = Voxels[x, y, z].TR;
                                temp[y, (X_SIZE - 1)-x, z].TG = Voxels[x, y, z].TG;
                                temp[y, (X_SIZE - 1)- x, z].TB = Voxels[x, y, z].TB;
                                temp[y, (X_SIZE - 1)-x,z].SR = Voxels[x, y, z].SR;
                                temp[y, (X_SIZE - 1) - x, z].SG = Voxels[x, y, z].SG;
                                temp[y, (X_SIZE - 1) - x, z].SB = Voxels[x, y, z].SB;
                            //}
                            //if (rot == 2 || rot==0)
                            //{
                            //    temp[(Y_SIZE - 1) - y, (X_SIZE - 1) - x, z].Active = true;
                            //    temp[(Y_SIZE - 1) - y, (X_SIZE - 1) - x, z].Type = VoxelType.Prefab;
                            //    temp[(Y_SIZE - 1) - y, (X_SIZE - 1) - x, z].TR = Voxels[x, y, z].TR;
                            //    temp[(Y_SIZE - 1) - y, (X_SIZE - 1) - x, z].TG = Voxels[x, y, z].TG;
                            //    temp[(Y_SIZE - 1) - y, (X_SIZE - 1) - x, z].TB = Voxels[x, y, z].TB;
                            //    temp[(Y_SIZE - 1) - y, (X_SIZE - 1) - x, z].SR = Voxels[x, y, z].SR;
                            //    temp[(Y_SIZE - 1) - y, (X_SIZE - 1) - x, z].SG = Voxels[x, y, z].SG;
                            //    temp[(Y_SIZE - 1) - y, (X_SIZE - 1) - x, z].SB = Voxels[x, y, z].SB;
                            //}
                        }
                    }
                }
            }

            for (int x = 0; x < X_SIZE; x++)
            {
                for (int y = 0; y < Y_SIZE; y++)
                {
                    for (int z = 0; z < Z_SIZE; z++)
                    {
                        Voxels[x, y, z].Active = temp[x, y, z].Active;
                        Voxels[x, y, z].Type = temp[x, y, z].Type;
                        Voxels[x, y, z].TR = temp[x, y, z].TR;
                        Voxels[x, y, z].TG = temp[x, y, z].TG;
                        Voxels[x, y, z].TB = temp[x, y, z].TB;
                        Voxels[x, y, z].SR = temp[x, y, z].SR;
                        Voxels[x, y, z].SG = temp[x, y, z].SG;
                        Voxels[x, y, z].SB = temp[x, y, z].SB;
                    }
                }
            }

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

        public bool IsVoxelAt(int x, int y, int z)
        {
            if (x >= 0 && x < X_SIZE && y >= 0 && y < Y_SIZE && z >= 0 && z < Z_SIZE) return Voxels[x, y, z].Active;

            return false;
        }

    }
}

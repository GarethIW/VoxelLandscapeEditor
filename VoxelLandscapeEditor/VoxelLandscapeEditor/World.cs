using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VoxelLandscapeEditor
{
    public class World
    {
        public int X_CHUNKS = 25, Y_CHUNKS = 25, Z_CHUNKS = 1;
        public int X_SIZE;
        public int Y_SIZE;
        public int Z_SIZE;

        const double REDRAW_INTERVAL = 60;

        public Chunk[, ,] Chunks;

        double redrawTime = 0;

        public World()
        {
            Init(true);
        }
        public World(int xs, int ys, int zs, bool createGround)
        {
            X_CHUNKS = xs;
            Y_CHUNKS = ys;
            Z_CHUNKS = zs;

            Init(createGround);
        }

        void Init(bool createGround)
        {
            Chunks = new Chunk[X_CHUNKS, Y_CHUNKS, Z_CHUNKS];

            for (int x = 0; x < X_CHUNKS; x++)
            {
                for (int y = 0; y < Y_CHUNKS; y++)
                {
                    for (int z = 0; z < Z_CHUNKS; z++)
                    {
                        Chunks[x, y, z] = new Chunk(this, x, y, z, createGround);
                    }
                }
            }
        }

        public void Update(GameTime gameTime, Camera gameCamera)
        {
            X_SIZE = X_CHUNKS * Chunk.X_SIZE;
            Y_SIZE = Y_CHUNKS * Chunk.Y_SIZE;
            Z_SIZE = Z_CHUNKS * Chunk.Z_SIZE;

            redrawTime += gameTime.ElapsedGameTime.TotalMilliseconds;
            if (redrawTime > REDRAW_INTERVAL)
            {
                redrawTime = 0;
                UpdateWorldMeshes();
            }

            for (int y = 0; y < Y_CHUNKS; y++)
            {
                for (int x = 0; x < X_CHUNKS; x++)
                {
                    Chunk c = Chunks[x, y, 0];
                    if (!gameCamera.boundingFrustum.Intersects(c.boundingSphere.Transform(Matrix.CreateTranslation(gameCamera.Position))))
                    {
                        if (c.Visible)
                        {
                            c.Visible = false;
                            //c.ClearMem();
                        }
                    }
                    else
                    {
                        if (!c.Visible)
                        {
                            c.Visible = true;
                            if (c.Updated) c.UpdateMesh();
                            //c.UpdateMesh();
                        }
                    }
                }
            }
        }

        public void UpdateWorldMeshes()
        {
            foreach (Chunk c in Chunks) if(c.Updated) c.UpdateMesh();
        }

        public void SetVoxel(int x, int y, int z, bool active, VoxelType type, Color top, Color side)
        {
            if (x < 0 || y < 0 || z < 0 || x >= X_SIZE || y >= Y_SIZE || z >= Z_SIZE) return;

            Chunk c = GetChunkAtWorldPosition(x, y, z);

            c.Voxels[x - ((x / Chunk.X_SIZE) * Chunk.X_SIZE), y - ((y / Chunk.Y_SIZE) * Chunk.Y_SIZE), z - ((z / Chunk.Z_SIZE) * Chunk.Z_SIZE)].Active = active;
            c.Voxels[x - ((x / Chunk.X_SIZE) * Chunk.X_SIZE), y - ((y / Chunk.Y_SIZE) * Chunk.Y_SIZE), z - ((z / Chunk.Z_SIZE) * Chunk.Z_SIZE)].Type = type;
            c.Voxels[x - ((x / Chunk.X_SIZE) * Chunk.X_SIZE), y - ((y / Chunk.Y_SIZE) * Chunk.Y_SIZE), z - ((z / Chunk.Z_SIZE) * Chunk.Z_SIZE)].TopColor = top;
            c.Voxels[x - ((x / Chunk.X_SIZE) * Chunk.X_SIZE), y - ((y / Chunk.Y_SIZE) * Chunk.Y_SIZE), z - ((z / Chunk.Z_SIZE) * Chunk.Z_SIZE)].SideColor = side;
            //= new Voxel(active, type, top, side);

            c.Updated = true;
        }

        public void SetVoxelActive(int x, int y, int z, bool active)
        {
            if (x < 0 || y < 0 || z < 0 || x >= X_SIZE || y >= Y_SIZE || z >= Z_SIZE) return;

            Chunk c = GetChunkAtWorldPosition(x, y, z);

            c.Voxels[x - ((x / Chunk.X_SIZE) * Chunk.X_SIZE), y - ((y / Chunk.Y_SIZE) * Chunk.Y_SIZE), z - ((z / Chunk.Z_SIZE) * Chunk.Z_SIZE)].Active = active;

            c.Updated = true;
        }

        public Voxel GetVoxel(int x, int y, int z)
        {
            if (x < 0 || y < 0 || z < 0 || x >= X_SIZE || y >= Y_SIZE || z >= Z_SIZE) return new Voxel();

            Chunk c = GetChunkAtWorldPosition(x, y, z);

            return c.Voxels[x - ((x / Chunk.X_SIZE) * Chunk.X_SIZE), y - ((y / Chunk.Y_SIZE) * Chunk.Y_SIZE), z - ((z / Chunk.Z_SIZE) * Chunk.Z_SIZE)];
        }

        public Chunk GetChunkAtWorldPosition(int x, int y, int z)
        {
            if (x < 0 || y < 0 || z < 0 || x >= X_SIZE || y >= Y_SIZE || z >= Z_SIZE) return null;

            return Chunks[x / Chunk.X_SIZE, y / Chunk.Y_SIZE, z / Chunk.Z_SIZE];
        }

        internal void MakeTree(int x, int y, int z)
        {
            int height = 3 + Helper.Random.Next(7);
            for (int h = z; h > z - height; h--)
            {
                SetVoxel(x, y, h, true, VoxelType.Tree, new Color(0.4f + ((float)Helper.Random.NextDouble() * 0.1f), 0.1f + ((float)Helper.Random.NextDouble() * 0.05f), 0.05f), new Color(0.5f + ((float)Helper.Random.NextDouble() * 0.1f), 0.1f + ((float)Helper.Random.NextDouble() * 0.05f), 0.05f));

                if (h < z - 4)
                {
                    for (int xx = x - 1; xx <= x + 1; xx++)
                    {
                        for (int yy = y - 1; yy <= y + 1; yy++)
                        {
                            if (xx == x && yy == y) continue;
                            if (Helper.Random.Next(2) == 0)
                            {
                                SetVoxel(xx, yy, h, true, VoxelType.Leaf, new Color(0.2f, 0.8f + ((float)Helper.Random.NextDouble() * 0.1f), 0.2f), new Color(0.2f, 0.5f + ((float)Helper.Random.NextDouble() * 0.1f), 0.2f));
                            }
                        }
                    }
                }
            }

            SetVoxel(x, y, z - (height), true, VoxelType.Leaf, new Color(0.2f, 0.8f + ((float)Helper.Random.NextDouble() * 0.1f), 0.2f), new Color(0.2f, 0.5f + ((float)Helper.Random.NextDouble() * 0.1f), 0.2f));
           
        }
    }
}

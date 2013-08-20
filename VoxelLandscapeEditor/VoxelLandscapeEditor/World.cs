using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VoxelLandscapeEditor
{
    public class World
    {
        public int X_CHUNKS = 40, Y_CHUNKS = 40, Z_CHUNKS = 1;
        public int X_SIZE;
        public int Y_SIZE;
        public int Z_SIZE;

        const double REDRAW_INTERVAL = 60;

        public Chunk[, ,] Chunks;

        public List<Spawn> Spawns = new List<Spawn>();

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

        public void SetVoxel(int x, int y, int z, bool active, short destructable, VoxelType type, Color top, Color side)
        {
            if (x < 0 || y < 0 || z < 0 || x >= X_SIZE || y >= Y_SIZE || z >= Z_SIZE) return;

            Chunk c = GetChunkAtWorldPosition(x, y, z);

            c.SetVoxel(x - ((x / Chunk.X_SIZE) * Chunk.X_SIZE), y - ((y / Chunk.Y_SIZE) * Chunk.Y_SIZE), z - ((z / Chunk.Z_SIZE) * Chunk.Z_SIZE), active, destructable, type, top, side);

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
            int height = 6 + Helper.Random.Next(10);
            if (z - height < 0) height -= ((z - height)-1);
            for (int h = z; h > z - height; h--)
            {
              
                SetVoxel(x, y, h, true, 0, VoxelType.Tree, new Color(0.4f + ((float)Helper.Random.NextDouble() * 0.1f), 0.1f + ((float)Helper.Random.NextDouble() * 0.05f), 0.05f), new Color(0.5f + ((float)Helper.Random.NextDouble() * 0.1f), 0.1f + ((float)Helper.Random.NextDouble() * 0.05f), 0.05f));
                SetVoxel(x+1, y, h, true, 0, VoxelType.Tree, new Color(0.4f + ((float)Helper.Random.NextDouble() * 0.1f), 0.1f + ((float)Helper.Random.NextDouble() * 0.05f), 0.05f), new Color(0.5f + ((float)Helper.Random.NextDouble() * 0.1f), 0.1f + ((float)Helper.Random.NextDouble() * 0.05f), 0.05f));
                SetVoxel(x, y+1, h, true, 0, VoxelType.Tree, new Color(0.4f + ((float)Helper.Random.NextDouble() * 0.1f), 0.1f + ((float)Helper.Random.NextDouble() * 0.05f), 0.05f), new Color(0.5f + ((float)Helper.Random.NextDouble() * 0.1f), 0.1f + ((float)Helper.Random.NextDouble() * 0.05f), 0.05f));
                SetVoxel(x+1, y+1, h, true, 0, VoxelType.Tree, new Color(0.4f + ((float)Helper.Random.NextDouble() * 0.1f), 0.1f + ((float)Helper.Random.NextDouble() * 0.05f), 0.05f), new Color(0.5f + ((float)Helper.Random.NextDouble() * 0.1f), 0.1f + ((float)Helper.Random.NextDouble() * 0.05f), 0.05f));


                if (h < z - 6)
                {
                    for (int xx = x - 2; xx <= x + 3; xx++)
                    {
                        for (int yy = y - 2; yy <= y + 3; yy++)
                        {
                            if (xx == x && yy == y) continue;
                            if (Helper.Random.Next(2) == 0)
                            {
                                SetVoxel(xx, yy, h, true, 0, VoxelType.Leaf, new Color(0.2f, 0.8f + ((float)Helper.Random.NextDouble() * 0.1f), 0.2f), new Color(0.2f, 0.5f + ((float)Helper.Random.NextDouble() * 0.1f), 0.2f));
                            }
                        }
                    }
                }
            }

            SetVoxel(x, y, z - (height), true, 0, VoxelType.Leaf, new Color(0.2f, 0.8f + ((float)Helper.Random.NextDouble() * 0.1f), 0.2f), new Color(0.2f, 0.5f + ((float)Helper.Random.NextDouble() * 0.1f), 0.2f));
            SetVoxel(x+1, y, z - (height), true, 0, VoxelType.Leaf, new Color(0.2f, 0.8f + ((float)Helper.Random.NextDouble() * 0.1f), 0.2f), new Color(0.2f, 0.5f + ((float)Helper.Random.NextDouble() * 0.1f), 0.2f));
            SetVoxel(x, y+1, z - (height), true, 0, VoxelType.Leaf, new Color(0.2f, 0.8f + ((float)Helper.Random.NextDouble() * 0.1f), 0.2f), new Color(0.2f, 0.5f + ((float)Helper.Random.NextDouble() * 0.1f), 0.2f));
            SetVoxel(x+1, y+1, z - (height), true, 0, VoxelType.Leaf, new Color(0.2f, 0.8f + ((float)Helper.Random.NextDouble() * 0.1f), 0.2f), new Color(0.2f, 0.5f + ((float)Helper.Random.NextDouble() * 0.1f), 0.2f));

            
        }

        public void SwitchTheme(Theme t)
        {
            foreach (Chunk c in Chunks)
            {
                for (int x = 0; x < Chunk.X_SIZE; x++)
                    for (int y = 0; y < Chunk.Y_SIZE; y++)
                        for (int z = 0; z < Chunk.Z_SIZE; z++)
                            if (c.Voxels[x,y,z].Active && c.Voxels[x, y, z].Type == VoxelType.Ground) c.SetVoxel(x, y, z, true, 0, VoxelType.Ground, ThemeTopColor(t), ThemeSideColor(t));
            }
        }

        public Color ThemeTopColor(Theme t)
        {
            switch(t)
            {
                case Theme.Jungle:
                    return new Color(0f, 0.5f + ((float)Helper.Random.NextDouble() * 0.1f), 0f);
                case Theme.Snow:
                    float rg = 0.8f + ((float)Helper.Random.NextDouble() * 0.2f);
                    return new Color(rg, rg, 1f);
                default:
                    return new Color(0f, 0.5f + ((float)Helper.Random.NextDouble() * 0.1f), 0f);
            }
        }
        public Color ThemeSideColor(Theme t)
        {
            switch (t)
            {
                case Theme.Jungle:
                    return new Color(0f, 0.3f, 0f);
                case Theme.Snow:
                    return new Color(0.5f, 0.5f, 0.6f);
                default:
                    return new Color(0f, 0.3f, 0f);
            }
        }
    }
}

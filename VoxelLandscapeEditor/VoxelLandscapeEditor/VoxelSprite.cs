﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VoxelLandscapeEditor
{
    public class VoxelSprite
    {
        public List<AnimChunk> AnimChunks = new List<AnimChunk>();
        public List<RenderTarget2D> ChunkRTs = new List<RenderTarget2D>();

        public int CurrentFrame = 0;

        public int X_SIZE, Y_SIZE, Z_SIZE;

        GraphicsDevice graphicsDevice;

        public VoxelSprite(int xs, int ys, int zs)
        {
            X_SIZE = xs;
            Y_SIZE = ys;
            Z_SIZE = zs;
            AnimChunks.Add(new AnimChunk(xs, ys, zs, false));
        }
        public VoxelSprite(int xs, int ys, int zs, GraphicsDevice gd)
        {
            graphicsDevice = gd;

            X_SIZE = xs;
            Y_SIZE = ys;
            Z_SIZE = zs;

            AnimChunks.Add(new AnimChunk(xs, ys, zs, true));
            ChunkRTs.Add(new RenderTarget2D(gd,200,150,false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8));
        }

        public void AddFrame(bool guides)
        {
            AnimChunks.Add(new AnimChunk(X_SIZE, Y_SIZE, Z_SIZE, guides));
        }
    }
}

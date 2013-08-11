using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VoxelSpriteEditor
{
    public enum VoxelType
    {
        Ground,
        Tree,
        Leaf,
        Water
    }

    public struct Voxel
    {
        public const float SIZE = 0.5f;
        public const float HALF_SIZE = SIZE / 2f;

        public bool Active;// = false;
        public Color Color;// = Color.White;

        public Voxel(bool active, Color col)
        {
            Active = active;
            Color = col;
        }
    }
}

using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VoxelLandscapeEditor
{
    public struct Voxel
    {
        public const float SIZE = 0.5f;
        public const float HALF_SIZE = SIZE / 2f;

        public bool Active;// = false;
        public Color TopColor;// = Color.White;
        public Color SideColor;// = Color.Gray;

        public Voxel(bool active, Color top, Color side)
        {
            Active = active;
            TopColor = top;
            SideColor = side;
        }
    }
}

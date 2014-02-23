﻿using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VoxelLandscapeEditor
{
    public enum SpawnType : byte
    {
        Squad,
        Single,
        HutDoor,
        RedFlag,
        BlueFlag,
        Hill
    }

    public class Spawn
    {
        public SpawnType Type;
        public Vector3 Position;
        public byte Rotation;

       
    }
}

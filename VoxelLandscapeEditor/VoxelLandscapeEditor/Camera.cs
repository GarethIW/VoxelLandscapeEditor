using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VoxelLandscapeEditor
{
    public class Camera
    {
        public Matrix worldMatrix;
        public Matrix viewMatrix;
        public Matrix projectionMatrix;

        public BoundingFrustum boundingFrustum;

        public Vector3 Position = new Vector3(0, 0, 0);
        public float Yaw = MathHelper.Pi;
        public float Roll = MathHelper.Pi;
        public float Pitch = -0.2f;

        public bool Zoom = false;

        public float Rot = 0f;

        const float moveSpeed = 0.1f;


        public Camera(GraphicsDevice gd, Viewport vp)
        {
            worldMatrix = Matrix.CreateWorld(Vector3.Zero, Vector3.Forward, Vector3.Up);
            Matrix cameraRotation = Matrix.CreateRotationZ(Roll) * Matrix.CreateRotationX(Pitch) * Matrix.CreateRotationY(Yaw);
            viewMatrix = Matrix.CreateLookAt(new Vector3(0,20,Zoom?-50:-30), new Vector3(0, 0, Zoom?-20:0), Vector3.Down);
            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, vp.AspectRatio, 0.001f, 300f);

            boundingFrustum = new BoundingFrustum(viewMatrix * projectionMatrix);
        }

        public void AddToPosition(Vector3 vectorToAdd)
        {
            Position += vectorToAdd;
            UpdateViewMatrix();
        }

        public void UpdateViewMatrix()
        {
            worldMatrix = Matrix.CreateWorld(Vector3.Zero, Vector3.Forward, Vector3.Up) * Matrix.CreateRotationZ(Rot) * Matrix.CreateTranslation(Position);
            viewMatrix = Matrix.CreateLookAt(new Vector3(0, 20, Zoom ? -150 : -30), new Vector3(0, 0, Zoom ? -120 : 0), Vector3.Down);
            boundingFrustum = new BoundingFrustum(viewMatrix * projectionMatrix);

        }
        
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace VoxelSpriteEditor
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class SpriteEditor : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        VoxelSprite sprite;

        Matrix worldMatrix;
        Matrix viewMatrix;
        Matrix projectionMatrix;

        BasicEffect drawEffect;
        BasicEffect cursorEffect;

        KeyboardState lks;
        MouseState lms;

        SpriteFont font;

        Cursor cursor = new Cursor();

        float viewPitch = 0f;
        float viewYaw = 0f;
        float viewZoom = -20f;

        RenderTarget2D viewRT;

        public SpriteEditor()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            graphics.PreferredBackBufferWidth = 1024;
            graphics.PreferredBackBufferHeight = 768;
            IsMouseVisible = true;
            graphics.ApplyChanges();

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            font = Content.Load<SpriteFont>("hudfont");

            viewRT = new RenderTarget2D(GraphicsDevice, 800,600, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8);

            worldMatrix = Matrix.CreateWorld(Vector3.Zero, Vector3.Forward, Vector3.Down);
            viewMatrix = Matrix.CreateLookAt(new Vector3(0, 0, -10), new Vector3(0, 0, 0), Vector3.Up);
            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, GraphicsDevice.Viewport.AspectRatio, 0.001f, 100f);

            sprite = new VoxelSprite(9, 9, 9, GraphicsDevice);
            sprite.AddFrame();
            sprite.AddFrame();
            sprite.AddFrame();
            sprite.AddFrame();
            sprite.AddFrame();

            drawEffect = new BasicEffect(GraphicsDevice)
            {
                World = worldMatrix,
                View = viewMatrix,
                Projection = projectionMatrix,
                VertexColorEnabled = true,
                //LightingEnabled = true
            };
            drawEffect.EnableDefaultLighting();

            cursorEffect = new BasicEffect(GraphicsDevice)
            {
                World = worldMatrix,
                View = viewMatrix,
                Projection = projectionMatrix,
                VertexColorEnabled = true
            };

            
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            AnimChunk currentChunk = sprite.AnimChunks[sprite.CurrentFrame];

            KeyboardState cks = Keyboard.GetState();
            MouseState cms = Mouse.GetState();
            Vector2 mousePos = new Vector2(cms.X, cms.Y);
            Vector2 mousedelta = mousePos - new Vector2(lms.X, lms.Y);
            int wheelDelta = cms.ScrollWheelValue - lms.ScrollWheelValue;

            Vector3 moveVector = new Vector3(0, 0, 0);
            if (cks.IsKeyDown(Keys.Up) && !lks.IsKeyDown(Keys.Up))
                moveVector += new Vector3(0, -1, 0);
            if (cks.IsKeyDown(Keys.Down) && !lks.IsKeyDown(Keys.Down))
                moveVector += new Vector3(0, 1, 0);
            if (cks.IsKeyDown(Keys.Right) && !lks.IsKeyDown(Keys.Right))
                moveVector += new Vector3(1, 0, 0);
            if (cks.IsKeyDown(Keys.Left) && !lks.IsKeyDown(Keys.Left))
                moveVector += new Vector3(-1, 0, 0);
            if (cks.IsKeyDown(Keys.PageDown) && !lks.IsKeyDown(Keys.PageDown))
                moveVector += new Vector3(0, 0, 1);
            if (cks.IsKeyDown(Keys.PageUp) && !lks.IsKeyDown(Keys.PageUp))
                moveVector += new Vector3(0, 0,-1);
            cursor.Position += moveVector;

            cursor.Position = Vector3.Clamp(cursor.Position, Vector3.Zero, Vector3.One * (currentChunk.X_SIZE-1));

            if (cks.IsKeyDown(Keys.Space) && !lks.IsKeyDown(Keys.Space))
            {
                currentChunk.SetVoxel((int)cursor.Position.X, (int)cursor.Position.Y, (int)cursor.Position.Z, true, Color.Green);
                currentChunk.UpdateMesh();
            }

            if (cks.IsKeyDown(Keys.Tab) && !lks.IsKeyDown(Keys.Tab))
            {
                currentChunk.SetVoxel((int)cursor.Position.X, (int)cursor.Position.Y, (int)cursor.Position.Z, false, currentChunk.Voxels[(int)cursor.Position.X, (int)cursor.Position.Y, (int)cursor.Position.Z].Color);
                currentChunk.UpdateMesh();
            }

            if (wheelDelta != 0)
            {
                if (wheelDelta > 0)
                {
                    viewZoom += 1f;
                }
                else
                {
                    viewZoom -= 1f;
                }
            }


            if (cms.LeftButton == ButtonState.Pressed)
            {
                viewPitch -= mousedelta.Y * 0.01f;
                viewYaw += mousedelta.X * 0.01f;
            }

            

            lks = cks;
            lms = cms;

            drawEffect.World = worldMatrix * Matrix.CreateRotationY(viewYaw) * Matrix.CreateRotationX(viewPitch);
            drawEffect.View = Matrix.CreateLookAt(new Vector3(0, 0, viewZoom), new Vector3(0, 0, 0), Vector3.Up);
            cursorEffect.World = Matrix.CreateTranslation((-((Vector3.One * Voxel.SIZE) * (currentChunk.X_SIZE / 2f)) + (cursor.Position * Voxel.SIZE))) * drawEffect.World;
            cursorEffect.View = drawEffect.View;

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            // Frames
            for (int i = 0; i < sprite.AnimChunks.Count;i++ )
            {
                GraphicsDevice.SetRenderTarget(sprite.ChunkRTs[i]);
                GraphicsDevice.Clear(Color.Black);
                GraphicsDevice.DepthStencilState = DepthStencilState.Default;
                GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
                GraphicsDevice.BlendState = BlendState.Opaque;
                foreach (EffectPass pass in drawEffect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    AnimChunk c = sprite.AnimChunks[i];
                    if (c.VertexArray.Length>0) 
                        GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionNormalColor>(PrimitiveType.TriangleList, c.VertexArray, 0, c.VertexArray.Length, c.IndexArray, 0, c.VertexArray.Length / 2);
                }
                spriteBatch.Begin();
                spriteBatch.DrawString(font, (i+1).ToString(), Vector2.One * 5, Color.White);
                spriteBatch.End();
            }
            // End frames


            // Main editor viewport
            GraphicsDevice.SetRenderTarget(viewRT);

            GraphicsDevice.Clear(Color.Black);

            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
            GraphicsDevice.BlendState = BlendState.Opaque;

            foreach (EffectPass pass in drawEffect.CurrentTechnique.Passes)
            {
                pass.Apply();

                AnimChunk c = sprite.AnimChunks[sprite.CurrentFrame];
                GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionNormalColor>(PrimitiveType.TriangleList, c.VertexArray, 0, c.VertexArray.Length, c.IndexArray, 0, c.VertexArray.Length / 2);
                
            }

            GraphicsDevice.BlendState = BlendState.AlphaBlend;
            foreach (EffectPass pass in cursorEffect.CurrentTechnique.Passes)
            {
                pass.Apply();

                Cursor c = cursor;
                GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionNormalColor>(PrimitiveType.TriangleList, c.VertexArray, 0, c.VertexArray.Length, c.IndexArray, 0, c.VertexArray.Length / 2);
            }

            spriteBatch.Begin();
            spriteBatch.DrawString(font, cursor.Position.ToString(), Vector2.One * 20, Color.White);
            spriteBatch.End();

            GraphicsDevice.SetRenderTarget(null);
            // End main editor viewport


            // Screen layout
            GraphicsDevice.Clear(Color.DarkGray);

            spriteBatch.Begin();
            spriteBatch.Draw(viewRT, new Vector2(0, GraphicsDevice.Viewport.Height - viewRT.Height), null, Color.White);
            for (int i = 0; i < sprite.AnimChunks.Count; i++)
            {
                spriteBatch.Draw(sprite.ChunkRTs[i], new Vector2(5+(i * (sprite.ChunkRTs[i].Width + 5)), 5), null, Color.White);
            }
            spriteBatch.End();
            // End screen layout

            base.Draw(gameTime);
        }
    }
}

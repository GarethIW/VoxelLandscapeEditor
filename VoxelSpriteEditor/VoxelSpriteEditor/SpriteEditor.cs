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

        AnimChunk clipBoard = new AnimChunk(32, 32, 32, false);

        Vector2 paintPos;
        Rectangle redRect;
        Rectangle greenRect;
        Rectangle blueRect;
        Color selectedColor = new Color(255, 255, 255);

        Color[] swatches = new Color[10];
        Rectangle[] swatchRects = new Rectangle[10];
        int selectedSwatch = 0;

        Rectangle viewRect;

        Rectangle nextFrameRect;
        Rectangle prevFrameRect;

        float viewPitch = 0f;
        float viewYaw = 0f;
        float viewZoom = -10f;

        RenderTarget2D viewRT;

        Dictionary<string, Texture2D> texList = new Dictionary<string, Texture2D>();

        double animTime = 0;
        bool playingAnim = false;

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
            graphics.PreferredBackBufferWidth = 1235;
            graphics.PreferredBackBufferHeight = 768;
            graphics.GraphicsProfile = GraphicsProfile.HiDef;
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

            LoadTex("arrow");
            LoadTex("colors");
            LoadTex("triangles");
            LoadTex("square");

            paintPos = new Vector2(GraphicsDevice.Viewport.Width - 215, GraphicsDevice.Viewport.Height-100);

            viewRT = new RenderTarget2D(GraphicsDevice, 800,600, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8);
            viewRect = new Rectangle(0, GraphicsDevice.Viewport.Height - 600, 800, 600);

            worldMatrix = Matrix.CreateWorld(Vector3.Zero, Vector3.Forward, Vector3.Down);
            viewMatrix = Matrix.CreateLookAt(new Vector3(0, 0, -10), new Vector3(0, 0, 0), Vector3.Up);
            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, 800f/600f, 0.001f, 100f);

            sprite = new VoxelSprite(16, 16,16, GraphicsDevice);

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

            redRect = new Rectangle((int)paintPos.X - (texList["colors"].Width / 2), (int)paintPos.Y - (texList["colors"].Height / 2), texList["colors"].Width, 40);
            greenRect = redRect;
            greenRect.Offset(0, 60);
            blueRect = greenRect;
            blueRect.Offset(0, 60);
            
            swatches[0]= new Color(255, 255, 255);
            swatches[1]= new Color(255, 0, 0);
            swatches[2]= new Color(0, 255, 0);
            swatches[3]= new Color(0, 0, 255);
            swatches[4]= new Color(255, 255, 0);
            swatches[5]= new Color(255, 0, 255);
            swatches[6]= new Color(0, 255, 255);
            swatches[7]= new Color(81, 81, 81);
            swatches[8]= new Color(183, 183, 183);
            swatches[9]= new Color(0, 0, 0);

            Rectangle swRect = new Rectangle(GraphicsDevice.Viewport.Width - 420, 170, 410, 30);
            for (int i = 0; i < 10; i++)
            {
                swatchRects[i] = swRect;
                swRect.Offset(0, 40);
            }

            nextFrameRect = new Rectangle(GraphicsDevice.Viewport.Width - 75, 25, 50, 100);
            prevFrameRect = new Rectangle(25, 25, 50, 100);

            selectedColor = swatches[0];
        }

        void LoadTex(string name)
        {
            texList.Add(name, Content.Load<Texture2D>(name));
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
            {
                moveVector += new Vector3(0, -1, 0);
                
            }
            if (cks.IsKeyDown(Keys.Down) && !lks.IsKeyDown(Keys.Down))
            {
                moveVector += new Vector3(0, 1, 0);
            }
            if (cks.IsKeyDown(Keys.Right) && !lks.IsKeyDown(Keys.Right))
            {
                moveVector += new Vector3(1, 0, 0);
            }
            if (cks.IsKeyDown(Keys.Left) && !lks.IsKeyDown(Keys.Left))
            {
                moveVector += new Vector3(-1, 0, 0);
            }
            if (cks.IsKeyDown(Keys.PageDown) && !lks.IsKeyDown(Keys.PageDown))
            {
                moveVector += new Vector3(0, 0, -1);
            }
            if (cks.IsKeyDown(Keys.PageUp) && !lks.IsKeyDown(Keys.PageUp))
            {
                moveVector += new Vector3(0, 0, 1);
            }
            if (cks.IsKeyDown(Keys.LeftShift)) cursor.ChangeSize(moveVector, currentChunk.X_SIZE);
            if(!cks.IsKeyDown(Keys.LeftShift)) cursor.Position += moveVector;

            cursor.Position = Vector3.Clamp(cursor.Position, Vector3.Zero, (Vector3.One * (currentChunk.X_SIZE)) - (cursor.Size));

            if (cks.IsKeyDown(Keys.Space) && !lks.IsKeyDown(Keys.Space))
            {
                for (int x = (int)cursor.Position.X; x < (int)cursor.Position.X + (int)cursor.Size.X; x++)
                    for (int y = (int)cursor.Position.Y; y < (int)cursor.Position.Y + (int)cursor.Size.Y; y++)
                        for (int z = (int)cursor.Position.Z; z < (int)cursor.Position.Z + (int)cursor.Size.Z; z++)
                            currentChunk.SetVoxel(x, y, z, true, selectedColor);
                currentChunk.UpdateMesh();
            }

            if (cks.IsKeyDown(Keys.Tab) && !lks.IsKeyDown(Keys.Tab))
            {
                for(int x=(int)cursor.Position.X;x<(int)cursor.Position.X + (int)cursor.Size.X;x++)
                    for (int y = (int)cursor.Position.Y; y < (int)cursor.Position.Y + (int)cursor.Size.Y; y++)
                        for (int z = (int)cursor.Position.Z; z < (int)cursor.Position.Z + (int)cursor.Size.Z; z++)
                            currentChunk.SetVoxel(x, y, z, false, currentChunk.Voxels[x, y, z].Color);
                currentChunk.UpdateMesh();
            }

            if (cks.IsKeyDown(Keys.LeftControl) && (cks.IsKeyDown(Keys.C) && !lks.IsKeyDown(Keys.C)))
            {
                ClearClipboard();
                for (int x = (int)cursor.Position.X; x < (int)cursor.Position.X + (int)cursor.Size.X; x++)
                    for (int y = (int)cursor.Position.Y; y < (int)cursor.Position.Y + (int)cursor.Size.Y; y++)
                        for (int z = (int)cursor.Position.Z; z < (int)cursor.Position.Z + (int)cursor.Size.Z; z++)
                            clipBoard.SetVoxel(x - (int)cursor.Position.X, y - (int)cursor.Position.Y, z - (int)cursor.Position.Z, currentChunk.Voxels[x,y,z].Active, currentChunk.Voxels[x,y,z].Color);
            }
            if (cks.IsKeyDown(Keys.LeftControl) && (cks.IsKeyDown(Keys.X) && !lks.IsKeyDown(Keys.X)))
            {
                ClearClipboard();
                for (int x = (int)cursor.Position.X; x < (int)cursor.Position.X + (int)cursor.Size.X; x++)
                    for (int y = (int)cursor.Position.Y; y < (int)cursor.Position.Y + (int)cursor.Size.Y; y++)
                        for (int z = (int)cursor.Position.Z; z < (int)cursor.Position.Z + (int)cursor.Size.Z; z++)
                            clipBoard.SetVoxel(x - (int)cursor.Position.X, y - (int)cursor.Position.Y, z - (int)cursor.Position.Z, currentChunk.Voxels[x, y, z].Active, currentChunk.Voxels[x, y, z].Color);

                for (int x = (int)cursor.Position.X; x < (int)cursor.Position.X + (int)cursor.Size.X; x++)
                    for (int y = (int)cursor.Position.Y; y < (int)cursor.Position.Y + (int)cursor.Size.Y; y++)
                        for (int z = (int)cursor.Position.Z; z < (int)cursor.Position.Z + (int)cursor.Size.Z; z++)
                            currentChunk.SetVoxel(x, y, z, false, currentChunk.Voxels[x, y, z].Color);
                currentChunk.UpdateMesh();
            }

            if (cks.IsKeyDown(Keys.LeftControl) && (cks.IsKeyDown(Keys.V) && !lks.IsKeyDown(Keys.V)))
            {
                for (int x = (int)cursor.Position.X; x < (int)cursor.Position.X + (int)cursor.Size.X; x++)
                    for (int y = (int)cursor.Position.Y; y < (int)cursor.Position.Y + (int)cursor.Size.Y; y++)
                        for (int z = (int)cursor.Position.Z; z < (int)cursor.Position.Z + (int)cursor.Size.Z; z++)
                            currentChunk.SetVoxel(x,y,z, clipBoard.Voxels[x - (int)cursor.Position.X, y - (int)cursor.Position.Y, z - (int)cursor.Position.Z].Active, clipBoard.Voxels[x - (int)cursor.Position.X, y - (int)cursor.Position.Y, z - (int)cursor.Position.Z].Color);
                currentChunk.UpdateMesh();
            }

            if (cks.IsKeyDown(Keys.NumPad4) && !lks.IsKeyDown(Keys.NumPad4))
            {
                sprite.CurrentFrame--;
                if (sprite.CurrentFrame < 0) sprite.CurrentFrame = sprite.AnimChunks.Count - 1;

            }
            if (cks.IsKeyDown(Keys.NumPad6) && !lks.IsKeyDown(Keys.NumPad6))
            {
                sprite.CurrentFrame++;
                if (sprite.CurrentFrame >= sprite.AnimChunks.Count) sprite.CurrentFrame = 0;
            }

            if (cks.IsKeyDown(Keys.NumPad5) && !lks.IsKeyDown(Keys.NumPad5))
            {
                playingAnim = !playingAnim;
                animTime = 0;

            }

            if (cks.IsKeyDown(Keys.Insert) && !lks.IsKeyDown(Keys.Insert)) sprite.InsertFrame();
            if (cks.IsKeyDown(Keys.Home) && !lks.IsKeyDown(Keys.Home)) sprite.CopyFrame();
            if (cks.IsKeyDown(Keys.End) && !lks.IsKeyDown(Keys.End)) sprite.AddFrame(true);
            if (cks.IsKeyDown(Keys.Delete) && !lks.IsKeyDown(Keys.Delete)) sprite.DeleteFrame();

            if (cks.IsKeyDown(Keys.F2) && !lks.IsKeyDown(Keys.F2)) LoadSave.Save(sprite, ref swatches);
            if (cks.IsKeyDown(Keys.F5) && !lks.IsKeyDown(Keys.F5)) LoadSave.Load(ref sprite, GraphicsDevice, ref swatches);

            if (cks.IsKeyDown(Keys.F8) && !lks.IsKeyDown(Keys.F8)) TileSheetConverter.Load(ref sprite, GraphicsDevice);


            if (cks.IsKeyDown(Keys.D1)) { selectedSwatch = 0; selectedColor = swatches[0]; }
            if (cks.IsKeyDown(Keys.D2)) { selectedSwatch = 1; selectedColor = swatches[1]; }
            if (cks.IsKeyDown(Keys.D3)) { selectedSwatch = 2; selectedColor = swatches[2]; }
            if (cks.IsKeyDown(Keys.D4)) { selectedSwatch = 3; selectedColor = swatches[3]; }
            if (cks.IsKeyDown(Keys.D5)) { selectedSwatch = 4; selectedColor = swatches[4]; }
            if (cks.IsKeyDown(Keys.D6)) { selectedSwatch = 5; selectedColor = swatches[5]; }
            if (cks.IsKeyDown(Keys.D7)) { selectedSwatch = 6; selectedColor = swatches[6]; }
            if (cks.IsKeyDown(Keys.D8)) { selectedSwatch = 7; selectedColor = swatches[7]; }
            if (cks.IsKeyDown(Keys.D9)) { selectedSwatch = 8; selectedColor = swatches[8]; }
            if (cks.IsKeyDown(Keys.D0)) { selectedSwatch = 9; selectedColor = swatches[9]; }

            if (cks.IsKeyDown(Keys.F12) && !lks.IsKeyDown(Keys.F12))
            {
                if(sprite.X_SIZE<32)
                    sprite = new VoxelSprite(sprite.X_SIZE + 1, sprite.Y_SIZE + 1, sprite.Z_SIZE + 1, GraphicsDevice);
            }
            if (cks.IsKeyDown(Keys.F11) && !lks.IsKeyDown(Keys.F11))
            {
                if (sprite.X_SIZE > 3)
                    sprite = new VoxelSprite(sprite.X_SIZE - 1, sprite.Y_SIZE - 1, sprite.Z_SIZE - 1, GraphicsDevice);
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
                Point mp = Helper.VtoP(mousePos);

                if (viewRect.Contains(new Point((int)mousePos.X, (int)mousePos.Y)))
                {
                    viewPitch -= mousedelta.Y * 0.01f;
                    viewYaw += mousedelta.X * 0.01f;
                }

                Color prevColor = selectedColor;
                if (redRect.Contains(mp))
                {
                    selectedColor.R = (byte)MathHelper.Clamp(((256f / 400f) * ((mp.X - (float)redRect.Left))), 0f, 255f);
                    swatches[selectedSwatch] = selectedColor;
                    if (cks.IsKeyDown(Keys.LeftShift)) sprite.ReplaceColor(prevColor, selectedColor);
                }
                if (greenRect.Contains(mp))
                {
                    selectedColor.G = (byte)MathHelper.Clamp(((256f / 400f) * ((mp.X - (float)greenRect.Left))), 0f, 255f);
                    swatches[selectedSwatch] = selectedColor;
                    if (cks.IsKeyDown(Keys.LeftShift)) sprite.ReplaceColor(prevColor, selectedColor);

                }
                if (blueRect.Contains(mp))
                {
                    selectedColor.B = (byte)MathHelper.Clamp(((256f / 400f) * ((mp.X - (float)blueRect.Left))), 0f, 255f);
                    swatches[selectedSwatch] = selectedColor;
                    if (cks.IsKeyDown(Keys.LeftShift)) sprite.ReplaceColor(prevColor, selectedColor);

                }

                if (lms.LeftButton != ButtonState.Pressed)
                {
                    for (int i = 0; i < 10; i++)
                    {
                        if (swatchRects[i].Contains(mp))
                        {
                            selectedColor = swatches[i];
                            selectedSwatch = i;
                        }
                    }

                    if (prevFrameRect.Contains(mp))
                    {
                        sprite.CurrentFrame--;
                        if (sprite.CurrentFrame < 0) sprite.CurrentFrame = sprite.AnimChunks.Count-1;

                    }
                    if (nextFrameRect.Contains(mp))
                    {
                        sprite.CurrentFrame++;
                        if (sprite.CurrentFrame >= sprite.AnimChunks.Count) sprite.CurrentFrame = 0;
                    }

                    

                }
            }

            cursor.Update(gameTime, selectedColor);

            lks = cks;
            lms = cms;

            if (playingAnim)
            {
                animTime += gameTime.ElapsedGameTime.TotalMilliseconds;
                if (animTime > 100)
                {
                    animTime = 0;
                    sprite.CurrentFrame++;
                    if (sprite.CurrentFrame > sprite.AnimChunks.Count - 1) sprite.CurrentFrame = 0;
                }
            }

            drawEffect.World = worldMatrix * Matrix.CreateRotationY(viewYaw) * Matrix.CreateRotationX(viewPitch);
            drawEffect.View = Matrix.CreateLookAt(new Vector3(0, 0, viewZoom), new Vector3(0, 0, 0), Vector3.Up);
            cursorEffect.World = Matrix.CreateTranslation((-((Vector3.One * Voxel.SIZE) * (currentChunk.X_SIZE / 2f)) + (cursor.Position * Voxel.SIZE))) * drawEffect.World;
            cursorEffect.View = drawEffect.View;

            base.Update(gameTime);
        }

        private void ClearClipboard()
        {
            for (int x = 0; x < 32; x++)
                for (int y = 0; y < 32; y++)
                    for (int z = 0; z < 32; z++)
                        clipBoard.SetVoxel(x, y, z, false, selectedColor);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            // Frames
            for (int i =0; i < sprite.AnimChunks.Count;i++ )
            {
               
                GraphicsDevice.SetRenderTarget(sprite.ChunkRTs[i]);
                GraphicsDevice.Clear(i!=sprite.CurrentFrame?Color.Black:new Color(0.2f,0.2f,0.2f));
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
                if(c.VertexArray.Length>0)
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
            GraphicsDevice.Clear(new Color(0.1f,0.1f,0.1f));

            spriteBatch.Begin();
            spriteBatch.Draw(viewRT, new Vector2(0, GraphicsDevice.Viewport.Height - viewRT.Height), null, Color.White);
            int viewBarScroll = sprite.CurrentFrame-2;
            if (viewBarScroll > sprite.AnimChunks.Count - 5) viewBarScroll = sprite.AnimChunks.Count - 5;
            if (viewBarScroll < 0) viewBarScroll = 0;

            int x = 0;
            for (int i = viewBarScroll; i < viewBarScroll+5; i++)
            {
                if (i > sprite.AnimChunks.Count - 1) break;
                spriteBatch.Draw(sprite.ChunkRTs[i], new Vector2(105+(x * (sprite.ChunkRTs[i].Width + 5)), 5), null, Color.White);
                x++;
            }

            

            for (int i = 0; i < 10; i++)
            {
                spriteBatch.Draw(texList["square"], swatchRects[i], swatches[i]);
                spriteBatch.DrawString(font, (i<9?i + 1:0).ToString() + (selectedSwatch==i?" > ":""), new Vector2(swatchRects[i].X + 3, swatchRects[i].Y) + Vector2.One, Color.Black);
                spriteBatch.DrawString(font, (i < 9 ? i + 1 : 0).ToString() + (selectedSwatch == i ? " > " : ""), new Vector2(swatchRects[i].X + 3, swatchRects[i].Y), Color.White);
            }

            spriteBatch.Draw(texList["colors"], paintPos, null, Color.White, 0f, new Vector2(texList["colors"].Width, texList["colors"].Height) / 2, 1f, SpriteEffects.None, 1);

            spriteBatch.Draw(texList["triangles"], new Vector2(redRect.Left + 4, redRect.Top + 24) + new Vector2((400f / 256f) * (float)(selectedColor.R), 0f), null, Color.White, 0f, new Vector2(texList["triangles"].Width, texList["triangles"].Height) / 2, 1f, SpriteEffects.None, 1);
            spriteBatch.Draw(texList["triangles"], new Vector2(greenRect.Left + 4, greenRect.Top + 24) + new Vector2((400f / 256f) * (float)(selectedColor.G ), 0f), null, Color.White, 0f, new Vector2(texList["triangles"].Width, texList["triangles"].Height) / 2, 1f, SpriteEffects.None, 1);
            spriteBatch.Draw(texList["triangles"], new Vector2(blueRect.Left + 4, blueRect.Top + 24) + new Vector2((400f / 256f) * (float)(selectedColor.B), 0f), null, Color.White, 0f, new Vector2(texList["triangles"].Width, texList["triangles"].Height) / 2, 1f, SpriteEffects.None, 1);

            spriteBatch.Draw(texList["arrow"], nextFrameRect, null, Color.White, 0f, Vector2.Zero, SpriteEffects.None, 1);
            spriteBatch.Draw(texList["arrow"], prevFrameRect, null, Color.White, 0f, Vector2.Zero, SpriteEffects.FlipHorizontally, 1);

            spriteBatch.End();
            // End screen layout

            base.Draw(gameTime);
        }
    }
}

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
using System.IO;

namespace VoxelLandscapeEditor
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Editor : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        World gameWorld;
        Camera gameCamera;

        BasicEffect drawEffect;
        BasicEffect cursorEffect;

        KeyboardState lks;
        MouseState lms;

        SpriteFont font;

        Cursor cursor;

        List<PrefabChunk> Prefabs = new List<PrefabChunk>();

        VoxelSprite spawnSprites = new VoxelSprite(15, 15, 15);

        SaveForm saveForm;

        double brushTime = 0;

        int spawnRot;

        int selectedPrefab = 0;
        int selectedSpawn = 0;

        bool zoom = false;

        public Editor()
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
            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 720;
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
            spriteBatch = new SpriteBatch(GraphicsDevice);

            font = Content.Load<SpriteFont>("hudfont");

            gameWorld = new World(50,50,1,true);
            gameCamera = new Camera(GraphicsDevice, GraphicsDevice.Viewport);
            cursor = new Cursor();

            this.EnsurePrefabs();
            List<string> pfs = new List<String>(Directory.EnumerateFiles(Path.Combine(Content.RootDirectory, "prefabs/")));
            LoadSave.LoadAnim(ref spawnSprites, Path.Combine(Content.RootDirectory, "spawns.vxs"));

            foreach (string fn in pfs)
            {
                Prefabs.Add(LoadSave.LoadPrefab(fn));
            }

            drawEffect = new BasicEffect(GraphicsDevice)
            {
                World = gameCamera.worldMatrix,
                View = gameCamera.viewMatrix,
                Projection = gameCamera.projectionMatrix,
                VertexColorEnabled = true,
                //LightingEnabled = true // turn on the lighting subsystem.

            };

            cursorEffect = new BasicEffect(GraphicsDevice)
            {
                World = gameCamera.worldMatrix,
                View = gameCamera.viewMatrix,
                Projection = gameCamera.projectionMatrix,
                VertexColorEnabled = true
                
            };

            saveForm = new SaveForm(gameWorld);

            GC.Collect();
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
            
            if (!this.IsActive) return;
            if (saveForm != null && saveForm.Visible) return;
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            KeyboardState cks = Keyboard.GetState();
            MouseState cms = Mouse.GetState();
            Vector2 mousePos = new Vector2(cms.X, cms.Y);
            Vector2 mousedelta = mousePos - new Vector2(lms.X, lms.Y);
            int wheelDelta = cms.ScrollWheelValue - lms.ScrollWheelValue;

            Vector3 moveVector = new Vector3(0, 0, 0);
            if (cks.IsKeyDown(Keys.Up) || cks.IsKeyDown(Keys.W))
                moveVector += new Vector3(0, 1, 0);
            if (cks.IsKeyDown(Keys.Down) || cks.IsKeyDown(Keys.S))
                moveVector += new Vector3(0, -1, 0);
            if (cks.IsKeyDown(Keys.Right) || cks.IsKeyDown(Keys.D))
                moveVector += new Vector3(-1, 0, 0);
            if (cks.IsKeyDown(Keys.Left) || cks.IsKeyDown(Keys.A))
                moveVector += new Vector3(1, 0, 0);
            gameCamera.AddToPosition(moveVector);
            gameCamera.UpdateViewMatrix();
            //gameCamera.Rotate(mousedelta.X, mousedelta.Y);

            if (cks.IsKeyDown(Keys.Z) && !lks.IsKeyDown(Keys.Z)) gameCamera.Zoom = !gameCamera.Zoom;

            if (cks.IsKeyDown(Keys.PageUp) && !lks.IsKeyDown(Keys.PageUp))
            {
                if(cursor.Mode == CursorMode.LandScape) cursor.Height++;
                if (cursor.Mode == CursorMode.Prefab) cursor.destructable++;

            }
            if (cks.IsKeyDown(Keys.PageDown) && !lks.IsKeyDown(Keys.PageDown))
            {
                if (cursor.Mode == CursorMode.LandScape) cursor.Height--;
                if (cursor.Mode == CursorMode.Prefab) cursor.destructable--;

            }
            if (cks.IsKeyDown(Keys.Tab) && !lks.IsKeyDown(Keys.Tab)) cursor.Mode++;

            if (cks.IsKeyDown(Keys.F2) && !lks.IsKeyDown(Keys.F2))
            {
                saveForm = new SaveForm(gameWorld);
                saveForm.Show(); //LoadSave.Save(gameWorld);
            }
            if (cks.IsKeyDown(Keys.F5) && !lks.IsKeyDown(Keys.F5)) LoadSave.Load(ref gameWorld);

            if (cks.IsKeyDown(Keys.F8) && !lks.IsKeyDown(Keys.F8)) cursor.MirrorMode = !cursor.MirrorMode;


            if (cks.IsKeyDown(Keys.Back) && cursor.Mode == CursorMode.Prefab) cursor.CutPrefabSapce(gameWorld, Prefabs[selectedPrefab]); 

            if (cks.IsKeyDown(Keys.F12) && !lks.IsKeyDown(Keys.F12))
            {
                if (cks.IsKeyDown(Keys.LeftShift))
                {
                    if (gameWorld.Y_CHUNKS > 5)
                    {
                        gameWorld.Y_CHUNKS--;
                    }
                }
                else
                {
                    if (gameWorld.Y_CHUNKS < 50)
                    {
                        gameWorld.Y_CHUNKS++;
                    }
                }
            }
            if (cks.IsKeyDown(Keys.F11) && !lks.IsKeyDown(Keys.F11))
            {
                if (cks.IsKeyDown(Keys.LeftShift))
                {
                    if (gameWorld.X_CHUNKS > 5)
                    {
                        gameWorld.X_CHUNKS--;
                    }
                }
                else if (cks.IsKeyDown(Keys.LeftControl))
                {
                    if (gameWorld.X_CHUNKS < 50)
                    {
                        gameWorld.X_CHUNKS++;
                        gameWorld.PadLeft();
                    }
                }
                else
                {
                    if (gameWorld.X_CHUNKS < 50)
                    {
                        gameWorld.X_CHUNKS++;
                    }
                }
            }

            if (cks.IsKeyDown(Keys.F10) && !lks.IsKeyDown(Keys.F10))
            {
                cursor.Theme++;
                if ((int)cursor.Theme > Enum.GetValues(typeof(Theme)).Length - 1) cursor.Theme = 0;
                gameWorld.Theme = cursor.Theme;
                if (cks.IsKeyDown(Keys.LeftShift)) gameWorld.SwitchTheme(cursor.Theme);
            }
            if (cks.IsKeyDown(Keys.F9) && !lks.IsKeyDown(Keys.F9))
            {
                cursor.Theme--;
                if (cursor.Theme < 0) cursor.Theme = (Theme)Enum.GetValues(typeof(Theme)).Length - 1;
                gameWorld.Theme = cursor.Theme;
                if (cks.IsKeyDown(Keys.LeftShift)) gameWorld.SwitchTheme(cursor.Theme);
            }

            if (wheelDelta != 0)
            {
               
                if (cursor.Mode != CursorMode.Prefab && cursor.Mode !=CursorMode.Spawn)
                {
                    if (wheelDelta > 0)
                    {
                        if (cks.IsKeyDown(Keys.LeftShift)) cursor.Height++;

                        else cursor.Size += 2;
                        cursor.UpdateMesh();
                    }
                    else
                    {
                        if (cks.IsKeyDown(Keys.LeftShift)) cursor.Height--;
                        else cursor.Size -= 2;
                        cursor.UpdateMesh();
                    }
                }
                else if(cursor.Mode== CursorMode.Prefab)
                {
                    if (wheelDelta > 0)
                    {
                        if (cks.IsKeyDown(Keys.LeftShift)) cursor.Height++;
                        else selectedPrefab++;
                        if (selectedPrefab >= Prefabs.Count) selectedPrefab = 0;
                    }
                    else
                    {
                        if (cks.IsKeyDown(Keys.LeftShift)) cursor.Height--;
                        else selectedPrefab--;
                        if (selectedPrefab < 0) selectedPrefab = Prefabs.Count - 1;
                    }
                }
                else if (cursor.Mode == CursorMode.Spawn)
                {
                    if (wheelDelta > 0)
                    {
                        if (cks.IsKeyDown(Keys.LeftShift)) cursor.Height++;
                        else selectedSpawn++;
                        if (selectedSpawn >= spawnSprites.AnimChunks.Count) selectedSpawn = 0;
                    }
                    else
                    {
                        if (cks.IsKeyDown(Keys.LeftShift)) cursor.Height--;
                        else selectedSpawn--;
                        if (selectedSpawn < 0) selectedSpawn = spawnSprites.AnimChunks.Count - 1;
                    }
                }
            }

            if (cms.RightButton == ButtonState.Pressed && lms.RightButton != ButtonState.Pressed)
            {
                if (cursor.Mode == CursorMode.Prefab) Prefabs[selectedPrefab].Rotate();
                if (cursor.Mode == CursorMode.Spawn)
                {
                    spawnRot++;
                    if (spawnRot > 3) spawnRot = 0;
                    //for(int i=0;i<spawnSprites.AnimChunks.Count;i++)
                      //  spawnSprites.AnimChunks[i].Rotate();
                }
            }

            if (cms.LeftButton == ButtonState.Pressed)
            {
                if (brushTime == 0) cursor.PerformAction(gameWorld, Prefabs[selectedPrefab], selectedSpawn, spawnRot);

                brushTime += gameTime.ElapsedGameTime.TotalMilliseconds;
                if (cursor.Mode!= CursorMode.Spawn && brushTime > 50) brushTime = 0;
            }
            if (cms.LeftButton == ButtonState.Released) brushTime = 0;

            Viewport vp = GraphicsDevice.Viewport;
            //  Note the order of the parameters! Projection first.
            Vector3 pos1 = vp.Unproject(new Vector3(mousePos.X, mousePos.Y, 0), gameCamera.projectionMatrix, gameCamera.viewMatrix, gameCamera.worldMatrix);
            Vector3 pos2 = vp.Unproject(new Vector3(mousePos.X, mousePos.Y, 1), gameCamera.projectionMatrix, gameCamera.viewMatrix, gameCamera.worldMatrix);
            Ray pickRay = new Ray(pos1, Vector3.Normalize(pos2 - pos1));
            int wpx = 0;
            int wpy = 0;
            int wpz = 0;
            foreach (Chunk c in gameWorld.Chunks)
            {
                if (!c.Visible) continue;
                if (!pickRay.Intersects(c.boundingSphere).HasValue) continue;
                for (int y = 0; y < Chunk.Y_SIZE; y++)
                    for (int x = 0; x < Chunk.X_SIZE; x++)
                    {

                        //for (int z = Chunk.Z_SIZE - 1; z >= 0; z--)
                        //{
                        //if (c.Voxels[x, y, z].Active == false) continue;

                        int z = Chunk.Z_SIZE - 1;

                        Vector3 worldOffset = new Vector3(c.worldX * (Chunk.X_SIZE * Voxel.SIZE), c.worldY * (Chunk.Y_SIZE * Voxel.SIZE), c.worldZ * (Chunk.Z_SIZE * Voxel.SIZE)) + ((new Vector3(x, y, z) * Voxel.SIZE));
                        BoundingBox box = new BoundingBox(worldOffset + new Vector3(-Voxel.HALF_SIZE, -Voxel.HALF_SIZE, -Voxel.HALF_SIZE), worldOffset + new Vector3(Voxel.HALF_SIZE, Voxel.HALF_SIZE, Voxel.HALF_SIZE));
                        if (pickRay.Intersects(box).HasValue)
                        {
                            wpx = (c.worldX * Chunk.X_SIZE) + x;
                            wpy = (c.worldY * Chunk.Y_SIZE) + y;
                            wpz = (c.worldZ * Chunk.Z_SIZE) + z;
                            for (int zz = Chunk.Z_SIZE - 1; zz >= 0; zz--) if (!gameWorld.GetVoxel(wpx, wpy, zz).Active || gameWorld.GetVoxel(wpx, wpy, zz).Type != VoxelType.Ground) { wpz = zz; break; }
                            //if (cursor.Mode == CursorMode.Prefab || cursor.Mode == CursorMode.Spawn) 
                            wpz = (Chunk.Z_SIZE) - cursor.Height;
                            cursor.Position = new Vector3(wpx, wpy, wpz);
                            //gameWorld.SetVoxelActive(wpx, wpy, wpz, false);
                            break;
                        }
                    }
                        //}
            }

            lks = cks;
            lms = cms;

            drawEffect.View = gameCamera.viewMatrix;
            drawEffect.World = gameCamera.worldMatrix;

            cursorEffect.World = Matrix.CreateTranslation(cursor.Position * Voxel.SIZE) * gameCamera.worldMatrix;

            gameWorld.Update(gameTime, gameCamera);
            cursor.Update(gameTime);

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            //GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            //GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
            GraphicsDevice.BlendState = BlendState.AlphaBlend;

            foreach (EffectPass pass in drawEffect.CurrentTechnique.Passes)
            {
                pass.Apply();

                for (int y = 0; y < gameWorld.Y_CHUNKS; y++)
                {
                    for (int x = 0; x < gameWorld.X_CHUNKS; x++)
                    {
                        Chunk c = gameWorld.Chunks[x, y, 0];
                        if (!c.Visible) continue;

                        if (c == null || c.VertexArray==null || c.VertexArray.Length == 0) continue;
                        if (!gameCamera.boundingFrustum.Intersects(c.boundingSphere.Transform(Matrix.CreateTranslation(gameCamera.Position)))) continue;
                        GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionNormalColor>(PrimitiveType.TriangleList, c.VertexArray, 0, c.VertexArray.Length, c.IndexArray, 0, c.VertexArray.Length / 2);
                    }
                }
            }

            foreach (Spawn s in gameWorld.Spawns)
            {
                drawEffect.World = Matrix.CreateRotationZ( -(MathHelper.PiOver2 * s.Rotation)) * Matrix.CreateTranslation(s.Position * Voxel.SIZE) * gameCamera.worldMatrix;
                foreach (EffectPass pass in drawEffect.CurrentTechnique.Passes)
                {
                    pass.Apply();

                    AnimChunk c = spawnSprites.AnimChunks[(int)s.Type];

                    if (c.VertexArray != null)
                        GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionNormalColor>(PrimitiveType.TriangleList, c.VertexArray, 0, c.VertexArray.Length, c.IndexArray, 0, c.VertexArray.Length / 2);

                }
            }

            if (cursor.Mode == CursorMode.Prefab)
            {
                if (Prefabs.Count > 0)
                {
                    cursorEffect.Alpha = 0.5f;
                    foreach (EffectPass pass in cursorEffect.CurrentTechnique.Passes)
                    {
                        pass.Apply();

                        PrefabChunk c = Prefabs[selectedPrefab];

                        if (c.VertexArray != null)
                            GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionNormalColor>(PrimitiveType.TriangleList, c.VertexArray, 0, c.VertexArray.Length, c.IndexArray, 0, c.VertexArray.Length / 2);

                    }
                }
            }
            else if (cursor.Mode == CursorMode.Spawn)
            {
                cursorEffect.Alpha = 0.5f;
                cursorEffect.World = Matrix.CreateRotationZ(-(MathHelper.PiOver2 * spawnRot)) * Matrix.CreateTranslation(cursor.Position * Voxel.SIZE) * gameCamera.worldMatrix;

                foreach (EffectPass pass in cursorEffect.CurrentTechnique.Passes)
                {
                    pass.Apply();

                    AnimChunk c = spawnSprites.AnimChunks[selectedSpawn];

                    if (c.VertexArray != null)
                        GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionNormalColor>(PrimitiveType.TriangleList, c.VertexArray, 0, c.VertexArray.Length, c.IndexArray, 0, c.VertexArray.Length / 2);

                }
            }
            else
            {
                cursorEffect.Alpha = 1f;
                foreach (EffectPass pass in cursorEffect.CurrentTechnique.Passes)
                {
                    pass.Apply();

                    if (cursor.VertexArray != null)
                        GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionNormalColor>(PrimitiveType.TriangleList, cursor.VertexArray, 0, cursor.VertexArray.Length, cursor.IndexArray, 0, cursor.VertexArray.Length / 2);

                }
            }

            spriteBatch.Begin();
            spriteBatch.DrawString(font, cursor.Mode.ToString(), Vector2.One * 20, Color.White);
            if (cursor.Mode == CursorMode.LandScape ||cursor.Mode ==  CursorMode.Water)
            {
                spriteBatch.DrawString(font, "Brush height: " + cursor.Height, new Vector2(20, 40), Color.White);
                spriteBatch.DrawString(font, "Theme: " + cursor.Theme.ToString(), new Vector2(20, 60), Color.White);
            }
            if (cursor.Mode == CursorMode.Prefab)
            {
                spriteBatch.DrawString(font, "Brush height: " + cursor.Height, new Vector2(20, 40), Color.White);
                spriteBatch.DrawString(font, "Destructable level: " + cursor.destructable, new Vector2(20, 60), Color.White);
            }
            if (cursor.Mode == CursorMode.Spawn)
            {
                spriteBatch.DrawString(font, "Spawn height: " + cursor.Height, new Vector2(20, 40), Color.White);
            }

            if (cursor.MirrorMode) spriteBatch.DrawString(font, "Mirror mode", new Vector2(300, 40), Color.White);
            spriteBatch.End();

            base.Draw(gameTime);
        }

        void EnsurePrefabs()
        {
            var currentdir = Environment.CurrentDirectory;
            var prefabsdir = Path.Combine(currentdir, @"Content\Prefabs\");

            if (!Directory.Exists(prefabsdir))
                Directory.CreateDirectory(prefabsdir);
        }
    }
}

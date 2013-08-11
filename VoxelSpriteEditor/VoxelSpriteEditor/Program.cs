using System;

namespace VoxelSpriteEditor
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (SpriteEditor game = new SpriteEditor())
            {
                game.Run();
            }
        }
    }
#endif
}


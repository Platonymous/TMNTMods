using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CustomSkins
{
    public class SkinTexture : Texture2D
    {
        public Texture2D Skin { get; private set; }

        public Texture2D Original { get; set; }

        public SkinTexture(Texture2D texture)
            : base(texture.GraphicsDevice, texture.Width, texture.Height)
        {
            Color[] colors = new Color[texture.Width * texture.Height];
            texture.GetData(colors);
            SetData(colors);

            Skin = texture;
            Original = texture;
        }

        public void Reset()
        {
            Skin = Original;
        }

        public void SetSkin(Texture2D texture)
        {
            Skin = texture;
        }

        public SkinTexture(GraphicsDevice graphicsDevice, int width, int height) : base(graphicsDevice, width, height)
        {
        }

        public SkinTexture(GraphicsDevice graphicsDevice, int width, int height, bool mipMap, SurfaceFormat format) : base(graphicsDevice, width, height, mipMap, format)
        {
        }
    }
}

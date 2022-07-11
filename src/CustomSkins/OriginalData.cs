using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CustomSkins
{
    public class OriginalData
    {
        public Texture2D Texture { get; set; }
        public Color[] Data { get; set; }

        public OriginalData(Texture2D texture)
        {
            Texture = texture;
            Data = new Color[texture.Width * texture.Height];
            texture.GetData(Data);
        }
    }
}

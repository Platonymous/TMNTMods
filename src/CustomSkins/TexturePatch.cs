using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CustomSkins
{
    public class TexturePatch
    {
        public string Asset { get; set; }
        public string Patch { get; set; }

        public string PatchType { get; set; }

        internal Texture2D PatchData { get; set; }

        internal bool ShouldMerge => PatchType.Equals("Merge",System.StringComparison.InvariantCultureIgnoreCase);

        internal void Apply()
        {
            if (CustomSkinsMod.Singleton.Textures.TryGetValue(Asset, out SkinTexture value) && value != null)
                if (!ShouldMerge)
                    value.SetSkin(PatchData);
                else
                {
                    Texture2D merged = new Texture2D(value.GraphicsDevice, value.Width, value.Height);
                    Color[] colorDataPatch = new Color[value.Width * value.Height];
                    Color[] colorDataOrg = new Color[value.Width * value.Height];
                    value.Skin.GetData(colorDataOrg);
                    PatchData.GetData(colorDataPatch);
                    for (int y = 0; y < value.Height; y++)
                        for (int x = 0; x < value.Width; x++)
                            if (colorDataPatch[y * value.Width + x] is Color c && c.A != 0)
                            {
                                colorDataOrg[y * value.Width + x] = c;
                                if (c.A == 255 && c.R == 255 && c.G == 0 && c.B == 255)
                                    colorDataOrg[y * value.Width + x] = Color.Transparent;
                            }
                    merged.SetData(colorDataOrg);

                    value.SetSkin(merged);
                }
        }

        internal void Reset()
        {
            if (CustomSkinsMod.Singleton.Textures.TryGetValue(Asset, out SkinTexture value) && value != null)
                value.Reset();
        }
    }
}

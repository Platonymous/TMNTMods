using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ModLoader.Content;
using Paris.Engine.Context;
using Paris.Engine.System.Localisation;
using Paris.Game.Data;
using Paris.Game.System;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace CustomSkins
{
    public class SkinModData
    {
        public List<CustomSkin> SkinsData = new List<CustomSkin>();

        public Dictionary<string,OriginalData> OriginalData = new Dictionary<string, OriginalData>();

        public string OriginalName;

        public string CharacterId;

        public CharacterInfo Character;

        public Color MenuColor;

        public Color MenuStarAColor;

        public Color MenuStarBColor;

        public bool initialized = false;

        public int CurrentSkin { get; set; } = -1;

        public SkinModData(string charaterName)
        {
            CharacterId = charaterName;
        }

        public void SetName(string name)
        {
            typeof(LocManager).GetMethod("AddLocToLanguage", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(LocManager.Singleton, new object[] { LocManager.Singleton.CurrentLanguage, Character.FullName.ID, name });
        }

        public void PatchTexture(string asset, TexturePatch patch)
        {
            try
            {
                if (OriginalData[asset] == null)
                    OriginalData[asset] = new OriginalData(ContextManager.Singleton.LoadContent<Texture2D>(asset, false));

                if (!patch.ShouldMerge)
                {
                    OriginalData[asset].Texture?.SetData(patch.PatchData);
                    return;
                }

                Texture2D org = OriginalData[asset].Texture;
                lock (org)
                {
                    Color[] colorDataOrg = new Color[org.Width * org.Height];
                    org.GetData(colorDataOrg);
                    for (int y = 0; y < org.Height; y++)
                        for (int x = 0; x < org.Width; x++)
                            if (patch.PatchData[y * org.Width + x] is Color c && c.A != 0)
                            {
                                colorDataOrg[y * org.Width + x] = c;
                                if (c.A == 255 && c.R == 255 && c.G == 0 && c.B == 255)
                                    colorDataOrg[y * org.Width + x] = Color.Transparent;
                            }
                    org.SetData(colorDataOrg);
                }
            }
            catch(Exception ex)
            {
                
            }
        }

        public void Init()
        {
            if (initialized)
                return;

            if (CharacterManager.Singleton.Characters.FirstOrDefault(chr => chr.InternalName == CharacterId) is CharacterInfo c)
                Character = c;
            
            foreach (var skin in SkinsData)
                foreach (var patch in skin.Patches)
                {
                    if (!OriginalData.ContainsKey(patch.Asset))
                        OriginalData.Add(patch.Asset, null);

                    var patchTexture = skin.Content.LoadContent<Texture2D>(patch.Patch.Replace(Path.GetExtension(patch.Patch),""));
                    patch.PatchData = new Color[patchTexture.Width * patchTexture.Height];
                    patchTexture.GetData(patch.PatchData);
                }

            MenuColor = new Color(Character.MenuColor.R, Character.MenuColor.G, Character.MenuColor.B, Character.MenuColor.A);
            MenuStarAColor = new Color(Character.MenuStarAColor.R, Character.MenuStarAColor.G,Character.MenuStarAColor.B,Character.MenuStarAColor.A);
            MenuStarBColor = new Color(Character.MenuStarBColor.R, Character.MenuStarBColor.G, Character.MenuStarBColor.B, Character.MenuStarBColor.A);

            initialized = true;
        }

        public void InitName()
        {
            if(OriginalName == null)
                OriginalName = LocManager.Singleton.GetStringForLanguage(Character.FullName.ID, LocManager.Singleton.CurrentLanguage);

        }

        public void Next()
        {
            CurrentSkin++;
            if(CurrentSkin >= SkinsData.Count)
                CurrentSkin = -1;

            ApplySkin();
        }

        public void Previous()
        {
            CurrentSkin--;
            if(CurrentSkin < -1)
                CurrentSkin = SkinsData.Count - 1;

            ApplySkin();
        }

        public void ApplySkin()
        {
            Reset();
            if (CurrentSkin >= 0)
            {
                var skin = SkinsData[CurrentSkin];
                foreach (var patch in skin.Patches)
                    PatchTexture(patch.Asset,patch);

                SetName(skin.Name);
                Character.MenuColor = GetColorByName(skin.MenuColor);
                Character.MenuStarAColor = Color.White * 0.4f;
                Character.MenuStarBColor = Color.White * 0.8f;
            }
        }

        public Color GetColorByName(string name)
        {
            if (typeof(Color).GetProperty(name) is PropertyInfo p && p.GetValue(null) is Color c)
                return c;

            return default(Color);
        }

        public void Reset()
        {
            foreach (string org in OriginalData.Keys.ToList())
                if (OriginalData[org] != null)
                {
                    OriginalData[org].Texture.SetData(OriginalData[org].Data);
                    OriginalData[org] = null;
                }
            



            if (OriginalName != null)
                SetName(OriginalName);
            Character.MenuColor = new Color(MenuColor.R, MenuColor.G, MenuColor.B, MenuColor.A);
            Character.MenuStarAColor = new Color(MenuStarAColor.R, MenuStarAColor.G, MenuStarAColor.B, MenuStarAColor.A);
            Character.MenuStarAColor = new Color(MenuStarBColor.R, MenuStarBColor.G, MenuStarBColor.B, MenuStarBColor.A);

        }
    }
}

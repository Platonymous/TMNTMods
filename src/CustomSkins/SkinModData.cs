using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Paris.Engine.System.Localisation;
using Paris.Game.Data;
using Paris.Game.System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace CustomSkins
{
    public class SkinModData
    {
        public List<CustomSkin> SkinsData = new List<CustomSkin>();

        public string CharacterId;

        public CharacterInfo Character;

        string OriginalName;

        public Color MenuColor;

        public Color MenuStarAColor;

        public Color MenuStarBColor;

        public bool initialized = false;

        public int CurrentSkin { get; set; } = -1;

        public SkinModData(string charaterName)
        {
            CharacterId = charaterName == "´Leonardo" ? "Leo" : charaterName;
        }

        public void SetName(string name)
        {
            typeof(LocManager).GetMethod("AddLocToLanguage", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(LocManager.Singleton, new object[] { LocManager.Singleton.CurrentLanguage, Character.FullName.ID, name });
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
                    var patchTexture = skin.Content.LoadContent<Texture2D>(patch.Patch.Replace(Path.GetExtension(patch.Patch),""));
                    patch.PatchData = patchTexture;
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

            Reset();
            CustomSkinsMod.Singleton.ApplyAll(this);
            ApplySkin();
        }

        public void Previous()
        {
            CurrentSkin--;
            if(CurrentSkin < -1)
                CurrentSkin = SkinsData.Count - 1;

            Reset();
            CustomSkinsMod.Singleton.ApplyAll(this);
            ApplySkin();
        }

        public void ApplySkin()
        {
            if (CurrentSkin >= 0)
            {
                var skin = SkinsData[CurrentSkin];
                foreach (var patch in skin.Patches)
                    patch.Apply();

                foreach (var patch in skin.AudioPatches)
                    patch.Apply();

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
            foreach (var skin in SkinsData)
                foreach (var patch in skin.AudioPatches)
                    patch.Reset();

            foreach (var skin in SkinsData)
                foreach (var patch in skin.Patches)
                    patch.Reset();

            if (OriginalName != null)
                SetName(OriginalName);
            Character.MenuColor = new Color(MenuColor.R, MenuColor.G, MenuColor.B, MenuColor.A);
            Character.MenuStarAColor = new Color(MenuStarAColor.R, MenuStarAColor.G, MenuStarAColor.B, MenuStarAColor.A);
            Character.MenuStarAColor = new Color(MenuStarBColor.R, MenuStarBColor.G, MenuStarBColor.B, MenuStarBColor.A);
        }


    }
}

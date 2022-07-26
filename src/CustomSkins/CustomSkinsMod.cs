using HarmonyLib;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ModLoader;
using ModLoader.Content;
using Paris.Engine.Audio;
using Paris.Engine.Context;
using Paris.Engine.Graphics;
using Paris.Engine.Menu;
using Paris.Engine.Menu.Control;
using Paris.Game.Data;
using Paris.Game.Menu;
using Paris.Game.Menu.Control;
using Paris.Game.System;
using Paris.System.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace CustomSkins
{
    public class CustomSkinsMod : IMod
    {
        public Dictionary<string, SkinModData> Skins = new Dictionary<string, SkinModData>();
        public Dictionary<string, SkinTexture> Textures = new Dictionary<string, SkinTexture>();
        public IModHelper Helper;
        static bool pressedRight;
        static bool pressedLeft;

        public static CustomSkinsMod Singleton;

        public CustomSkinsMod()
        {
            Singleton = this;
        }

        public void ApplyAll(SkinModData sender)
        {
            foreach (var skin in Skins.Values)
                if (skin != sender)
                    skin.Reset();

            foreach (var skin in Skins.Values)
                if (skin != sender)
                    skin.ApplySkin();
        }

        public void ModEntry(IModHelper helper)
        {
            Helper = helper;

            var harmony = new Harmony("Platonymous.CustomSkins");

            harmony.Patch(
                original: typeof(MenuBase).GetMethod("PressedRight", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance),
                postfix: new HarmonyMethod(typeof(CustomSkinsMod), nameof(PressedRight))
                );


            harmony.Patch(
                original: typeof(MenuBase).GetMethod("PressedLeft", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance),
                postfix: new HarmonyMethod(typeof(CustomSkinsMod), nameof(PressedLeft))
                );

            harmony.Patch(
               original: typeof(AudioManager).GetMethod("Init", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance),
               postfix: new HarmonyMethod(typeof(CustomSkinsMod), nameof(PreloadAudio))
               );

            foreach(var method in typeof(Renderer).GetMethods(BindingFlags.Public | BindingFlags.Instance).Where(m => m.Name == "DrawTexture" && m.GetParameters().Length >= 9))
                harmony.Patch(
               original: method,
               prefix: new HarmonyMethod(typeof(CustomSkinsMod), nameof(DrawTexture))
               );


            foreach (var pack in helper.GetContentPacks())
                try
                {
                    var contentPack = pack.Content.LoadJson<ContentPack>("content.json");
                    foreach (var skin in contentPack.Skins) {
                        skin.Content = pack.Content;
                        var skinId = skin.Character == "Leonardo" ? "Leo" : skin.Character;

                        for (int i = 0; i < skin.Patches.Count; i++)
                            if (skin.Patches[i] is TexturePatch tp && tp.Asset.StartsWith("Audio\\"))
                                skin.AudioPatches.Add(new SoundPatch(tp.Asset.Substring("Audio\\".Length), File.ReadAllBytes(Path.Combine(pack.Manifest.Folder, tp.Patch))));

                        skin.Patches.RemoveAll(p => p.Asset.StartsWith("Audio\\"));

                        if (!Skins.ContainsKey(skinId))
                            Skins.Add(skinId, new SkinModData(skinId));

                        foreach (var patch in skin.Patches)
                            if (!Textures.ContainsKey(patch.Asset))
                                Textures.Add(patch.Asset, null);

                        if (!Skins[skinId].SkinsData.Contains(skin))
                            Skins[skinId].SkinsData.Add(skin);
                    }
                }
                catch (Exception e)
                {
                    helper.Console.Error(e.Message);
                    helper.Console.Trace(e.StackTrace);
                }

            try
            {
                helper.Events.ContextSwitched += Events_ContextSwitched;
                helper.Events.AssetLoaded += Events_AssetLoaded;

                helper.Events.UpdateTicked += Events_UpdateTicked;
            }
            catch (Exception e)
            {
                helper.Console.Error(e.Message);
                helper.Console.Trace(e.StackTrace);
            }

        }

        public static void DrawTexture(ref Texture2D texture)
        {
            while (texture is SkinTexture st && st.Skin is Texture2D t)
                texture = t;
        }

        public static void PreloadAudio()
        {
           foreach(var skin in Singleton.Skins.Values)
                foreach(var data in skin.SkinsData)
                    foreach(var audio in data.AudioPatches)
                        audio.Init();
        }

        public  static void PressedRight()
        {
            pressedRight = true;
        }



        public static void PressedLeft()
        {
            pressedLeft = true;
        }


        private void Events_AssetLoaded(object sender, ModLoader.Events.AssetLoadedEventArgs e)
        {
            if (e.Asset is Texture2D t && Textures.ContainsKey(e.AssetName))
                if (Textures[e.AssetName] is SkinTexture st)
                {
                    st.Original = t;
                    e.SetAsset(st);
                }
                else
                {
                    Textures[e.AssetName] = new SkinTexture(t);
                    e.SetAsset(Textures[e.AssetName]);
                }
        }

        private void Events_ContextSwitched(object sender, ModLoader.Events.ContextSwitchedEventArgs e)
        {
            if (e.NewContext.Contains("Paris.Game.Menu.MainMenu"))
            {
                foreach (var t in Textures.Keys.ToList())
                {
                    ContextManager.Singleton.ContentManager.UnloadAsset(t);
                    ContextManager.Singleton.LoadContent<Texture2D>(t,true,true);
                }

                foreach (var skin in Skins.Values)
                    skin.Init();
                
            }

            if (e.NewContext.Contains("Paris.Game.Menu.CharacterSelect"))
                foreach (var skin in Skins.Values)
                    skin.InitName();
        }

        private void Events_UpdateTicked(object sender, ModLoader.Events.UpdateTickEventArgs e)
        {
            if ((pressedLeft || pressedRight) && ContextManager.Singleton.CurrentContext is CharacterSelect context)
            {
                CharacterSelectionContainer container = (CharacterSelectionContainer)context.GetType().GetField("_charContainer", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(context);
                if (container != null)
                {
                    int selection = container.Panels[0].CurrentSelection;
                    if (selection >= 0 && selection < CharacterManager.Singleton.Characters.Count)
                    {
                        CharacterInfo character = CharacterManager.Singleton.Characters[selection];
                        string charId = character.InternalName;

                        if (Skins.ContainsKey(charId))
                        {
                            if (pressedRight)
                                Skins[charId].Next();
                            else
                                Skins[charId].Previous();

                            foreach(var panel in container.Panels)
                                typeof(CharacterSelectionPanel).GetMethod("UpdateCharacterSelection", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(panel, new object[0]);
                        }
                    }
                }

            }

            pressedLeft = false;
            pressedRight = false;
        }
    }
}

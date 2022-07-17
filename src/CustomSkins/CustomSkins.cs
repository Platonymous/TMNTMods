using HarmonyLib;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ModLoader;
using ModLoader.Content;
using Paris.Engine.Audio;
using Paris.Engine.Context;
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

            
            foreach (var pack in helper.GetContentPacks())
                try
                {
                    var contentPack = pack.Content.LoadJson<ContentPack>("content.json");
                    foreach (var skin in contentPack.Skins) {
                        skin.Content = pack.Content;
                        var skinId = skin.Character == "Leonardo" ? "Leo" : skin.Character;

                        for (int i = 0; i < skin.Patches.Count; i++)
                            if (skin.Patches[i] is TexturePatch tp && tp.Asset.StartsWith("Audio\\"))
                            {
                                var ac = File.ReadAllBytes(Path.Combine(pack.Manifest.Folder, tp.Patch));
                                skin.AudioPatches.Add(new SoundPatch(tp.Asset.Substring("Audio\\".Length),ac));
                            }

                        skin.Patches.RemoveAll(p => p.Asset.StartsWith("Audio\\"));

                        if (!Skins.ContainsKey(skinId))
                        {
                            var skindata = new SkinModData(skinId);
                            Skins.Add(skinId, skindata);
                        }

                        if(!Skins[skinId].SkinsData.Contains(skin))
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
                foreach (var skin in Skins.Values)
                    if (e.Asset is Texture2D t && skin.OriginalData.ContainsKey(e.AssetName))
                        if (skin.OriginalData[e.AssetName] == null)
                            skin.OriginalData[e.AssetName] = new OriginalData(t);
                        else
                        {
                            skin.OriginalData[e.AssetName] = new OriginalData(t);
                            foreach (var skindata in Skins.Values)
                                if (skindata.OriginalData.ContainsKey(e.AssetName))
                                    skindata.ApplySkin();
                        }
        }

        private void Events_ContextSwitched(object sender, ModLoader.Events.ContextSwitchedEventArgs e)
        {
            if(e.NewContext.Contains("Paris.Game.Menu.MainMenu"))
                foreach (var skin in Skins.Values)
                    skin.Init();

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
                        string charId = character.InternalName == "Leo" ? "Leonardo" : character.InternalName;

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

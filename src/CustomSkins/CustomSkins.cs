using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ModLoader;
using Paris.Engine.Context;
using Paris.Game.Data;
using Paris.Game.Menu;
using Paris.Game.Menu.Control;
using Paris.Game.System;
using Paris.System.Input;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace CustomSkins
{
    public class CustomSkinsMod : IMod
    {
        public Dictionary<string, SkinModData> Skins = new Dictionary<string, SkinModData>();
        public IModHelper Helper;
        public void ModEntry(IModHelper helper)
        {
            Helper = helper;
            foreach (var pack in helper.GetContentPacks())
                try
                {
                    var contentPack = pack.Content.LoadJson<ContentPack>("content.json");
                    foreach (var skin in contentPack.Skins) {
                        skin.Content = pack.Content;
                        var skinId = skin.Character == "Leonardo" ? "Leo" : skin.Character;
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

        private void Events_AssetLoaded(object sender, ModLoader.Events.AssetLoadedEventArgs e)
        {
            foreach (var skin in Skins.Values)
                if (skin.OriginalData.ContainsKey(e.AssetName))
                    if (skin.OriginalData[e.AssetName] == null)
                        skin.OriginalData[e.AssetName] = new OriginalData((Texture2D)e.Asset);
                    else
                        e.SetAsset(skin.OriginalData[e.AssetName].Texture);
        }

        private void Events_ContextSwitched(object sender, ModLoader.Events.ContextSwitchedEventArgs e)
        {
            if(e.NewContext.Contains("Paris.Game.Menu.MainMenu"))
            {
                foreach (var skin in Skins.Values)
                {
                    skin.Init();
                    skin.CurrentSkin = -1;
                    skin.Reset();
                }
            }

            if (e.NewContext.Contains("Paris.Game.Menu.CharacterSelect"))
            {
                foreach (var skin in Skins.Values)
                    skin.InitName();
            }
        }

        private void Events_UpdateTicked(object sender, ModLoader.Events.UpdateTickEventArgs e)
        {
            if ((InputManager.Singleton.IsKeyJustPressed(Keys.Right) || InputManager.Singleton.IsKeyJustPressed(Keys.Left)) && ContextManager.Singleton.CurrentContext is CharacterSelect context)
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
                            if (InputManager.Singleton.IsKeyJustPressed(Keys.Right))
                                Skins[charId].Next();
                            else
                                Skins[charId].Previous();

                            foreach(var panel in container.Panels)
                                typeof(CharacterSelectionPanel).GetMethod("UpdateCharacterSelection", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(panel, new object[0]);
                        }
                    }
                }

            }
        }
    }
}

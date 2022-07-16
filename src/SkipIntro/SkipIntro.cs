using Microsoft.Xna.Framework;
using ModLoader;
using Paris.Engine.Context;
using Paris.Engine.Cutscenes;
using Paris.Engine.Scene;
using Paris.Game.HUD;

namespace SkipIntro
{
    public class SkipIntroMod : IMod
    {
        IModHelper Helper;
        Config ModConfig;

        public void ModEntry(IModHelper helper)
        {
            Helper = helper;
            ModConfig = helper.Config.LoadConfig<Config>();
            
            helper.Events.ContextSwitched += Events_ContextSwitched;
            helper.Events.GameInitialized += Events_GameInitialized;
        }

        private void Events_GameInitialized(object sender, ModLoader.Events.GameInitializedEventArgs e)
        {
            Helper.Config.SetOptionsMenuEntry("logos", "Skip Logos",
                (s) =>
                {
                    ModConfig.Logos = (s.Choice == "ON");
                    Helper.Config.SaveConfig(ModConfig);
                }, () => ModConfig.Logos ? "ON" : "OFF", "ON", "OFF");

            Helper.Config.SetOptionsMenuEntry("howto", "Skip HowTo",
                (s) =>
                {
                    ModConfig.HowTo = (s.Choice == "ON");
                    Helper.Config.SaveConfig(ModConfig);
                }, () => ModConfig.HowTo ? "ON" : "OFF", "ON", "OFF");

            Helper.Config.SetOptionsMenuEntry("video", "Skip Video",
                (s) =>
                {
                    ModConfig.Video = (s.Choice == "ON");
                    Helper.Config.SaveConfig(ModConfig);
                }, () => ModConfig.Video ? "ON" : "OFF", "ON", "OFF");
        }

        private void Events_ContextSwitched(object sender, ModLoader.Events.ContextSwitchedEventArgs e)
        {
            if (ModConfig.Logos && e.NewContext.Contains("Paris.Game.Menu.LogoScreen"))
            {
                e.SwitchContext(ModConfig.Video ? "Paris.Game.Menu.MainMenu" : "Paris.Engine.Cutscenes.VideoContext");
                if (!ModConfig.Video)
                {
                    VideoContext.VideoPath = "Videos\\TMNT_GameIntro";
                    VideoContext.NextContext = "Paris.Game.Menu.MainMenu";
                }
            }
            else if (ModConfig.Video && e.NewContext.Contains("Paris.Engine.Cutscenes.VideoContext"))
                e.SwitchContext("Paris.Game.Menu.MainMenu");
            
            if (ModConfig.HowTo && e.NewContext.Equals(@"2d\Level\Scene2d\HowToPlay", System.StringComparison.OrdinalIgnoreCase))
                Helper.Events.UpdateTicked += Events_UpdateTicked1;
            else
                Helper.Events.UpdateTicked -= Events_UpdateTicked1;
        }

        private void Events_UpdateTicked1(object sender, ModLoader.Events.UpdateTickEventArgs e)
        {
            if (ContextManager.Singleton.CurrentContext is Scene2d context && context.HUD is HowToPlay hud && !HowToPlay.ReturnToMainMenu)
                ContextManager.Singleton.SwitchToContext("Paris.Game.Menu.CharacterSelect", 0.1f, Color.Black);
        }

    }
}

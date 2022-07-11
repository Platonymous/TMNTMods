using Microsoft.Xna.Framework;
using ModLoader;
using Paris.Engine.Context;
using Paris.Engine.Scene;
using Paris.Engine.Scene.HUD;
using Paris.Game.Data;
using Paris.Game.HUD;
using System.Reflection;

namespace SkipIntro
{
    public class SkipIntroMod : IMod
    {
        static IModHelper Helper;

        public void ModEntry(IModHelper helper)
        {
            Helper = helper;
            helper.Events.ContextSwitched += Events_ContextSwitched;
        }

        private void Events_ContextSwitched(object sender, ModLoader.Events.ContextSwitchedEventArgs e)
        {
            if (e.NewContext.Contains("Paris.Game.Menu.LogoScreen"))
                e.SwitchContext("Paris.Game.Menu.MainMenu");
            else if (e.NewContext.Equals(@"2d\Level\Scene2d\HowToPlay", System.StringComparison.OrdinalIgnoreCase))
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

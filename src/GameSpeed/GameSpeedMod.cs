using HarmonyLib;
using ModLoader;
using Paris.Engine.Scene;
using System.Collections.Generic;

namespace GameSpeed
{
    public class GameSpeedMod : IMod
    {
        Harmony harmony;
        static IModHelper Helper;
        static Config SpeedConfig;


        public void ModEntry(IModHelper helper)
        {
            Helper = helper;
            harmony = new Harmony("Platonymous.GameSpeed");


            SpeedConfig = helper.Config.LoadConfig<Config>();

            List<string> values = new List<string>();

            for(int i = 50; i <= 500; i+= 5)
                if(((double) i / 100d).ToString("0.00", System.Globalization.CultureInfo.InvariantCulture) is string s)
                    values.Add(s);

            helper.Config.SetOptionsMenuEntry("Platonymoud.GameSpeed.Speed", "Speed", (c) =>
            {
                SpeedConfig.GameSpeed = float.Parse(c.Choice);
                helper.Config.SaveConfig(SpeedConfig);
            }, () => SpeedConfig.GameSpeed.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture), values.ToArray());

            harmony.Patch(
                            original: typeof(Scene2d).GetMethod("Tick", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance),
                            prefix: new HarmonyMethod(typeof(GameSpeedMod), nameof(Tick))
                            );
        }

        public static void Tick(Scene2d __instance)
        {
            __instance.SpeedRatio = SpeedConfig.GameSpeed;
        }
    }
}

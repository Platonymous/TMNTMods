using HarmonyLib;
using Microsoft.Xna.Framework;
using ModLoader;
using Paris.Engine;
using Paris.Engine.Context;
using Paris.Engine.Menu.Control;
using Paris.Engine.System.Localisation;
using Paris.Game.Menu;
using Paris.System.FSM;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using static Paris.System.FSM.FSM;

namespace StageSelect
{
    public class StageSelectMod : IMod
    {
        Harmony harmony;

        public void ModEntry(IModHelper helper)
        {
            harmony = new Harmony("Platonymous.Arena");
            harmony.Patch(
                original: typeof(Enum).GetMethod(nameof(Enum.GetNames), System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static),
                postfix:new HarmonyMethod(typeof(StageSelectMod),nameof(GetEnumNames))
                );

            harmony.Patch(
                original: typeof(MainMenu).GetMethod("Accept", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance),
                postfix: new HarmonyMethod(typeof(StageSelectMod), nameof(Accept))
                );
        }

        public static MainMenu mainMenu;
        public static GameMenu menu;
        public static State SelectMenuState;
        public static int FirstStage = -1;
        public static int LastStage = -1;
        public static List<string> Stages = new List<string>();
        public static void Accept(MainMenu __instance)
        {

            FSM fsm = (FSM) typeof(MainMenu).GetField("_fsm", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
            SelectionMenuControl start = (SelectionMenuControl)typeof(MainMenu).GetField("_startGameSelection", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
            start.MaxShownItems = 7;
            if (fsm.CurrentState == 3 && start.Selection >= FirstStage && start.Selection < LastStage)
            {
                int si = start.Selection - FirstStage;
                if (Stages.Count > si)
                {
                    Console.WriteLine(Stages[si]);
                    string[] n = Stages[si].Split(':');
                    ContextManager.Singleton.SwitchToContext(@"2d\Level\Scene2d\Stage\" + n[0] + @"\" + n[1], 0.1f, Color.Black);
                }
            }
        }
        public static void GetEnumNames(Enum __instance, Type enumType, ref string[] __result)
        {
            if (enumType.Name == "StartGameItems")
            {
                var list = new List<string>(__result);
                FirstStage = list.Count;
                DirectoryInfo stages = new DirectoryInfo(Path.Combine("Content","2d","Level","Scene2d","Stage"));
                foreach (var directory in stages.EnumerateDirectories())
                    foreach (var file in directory.GetFiles().Where(f => f.Name.Contains("_complete.zpbn")))
                    {
                        string n = file.Name.Replace("_complete.zpbn","");
                        Stages.Add(directory.Name + ":" + file.Name.Replace(".zpbn", ""));
                        string id = directory.Name + ":" + n;
                        list.Add(id);
                        typeof(LocManager).GetMethod("AddLocToLanguage", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(LocManager.Singleton, new object[] { LocManager.Singleton.CurrentLanguage, "mnu" + id, n.Replace("_", " ").Replace("Level","Stage").Replace("part","Pt. ").Replace("Boss","Boss ") });
                    }

                LastStage = list.Count;

                __result = list.ToArray();
            }
        }
    }
}

using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ModLoader;
using Paris.Engine.Context;
using Paris.Engine.Menu.Control;
using Paris.Engine.System.Localisation;
using Paris.Game.Menu;
using Paris.System.FSM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Arena
{
    public class ArenaMod : IMod
    {
        Harmony harmony;
        static int ArenaIndex;
        static IModHelper Helper;

        public void ModEntry(IModHelper helper)
        {
            Helper = helper;
            harmony = new Harmony("Platonymous.Arena");
            harmony.Patch(
                original: typeof(Enum).GetMethod(nameof(Enum.GetNames), System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static),
                postfix:new HarmonyMethod(typeof(ArenaMod),nameof(GetEnumNames))
                );

            harmony.Patch(
                original: typeof(MainMenu).GetMethod("Accept", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance),
                postfix: new HarmonyMethod(typeof(ArenaMod), nameof(Accept))
                           );
        }


        public static void Accept(MainMenu __instance)
        {
            FSM fsm = (FSM) typeof(MainMenu).GetField("_fsm", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
            SelectionMenuControl start = (SelectionMenuControl) typeof(MainMenu).GetField("_startGameSelection", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);

            if (fsm.CurrentState == 3 && start.Selection == ArenaIndex)
                ContextManager.Singleton.SwitchToContext(@"2d\Level\Scene2d\Arena", 0.1f, Color.Black);
        }

        public static void GetEnumNames(Enum __instance, Type enumType, ref string[] __result)
        {
            if (enumType.Name == "StartGameItems")
            {
                var list = new List<string>(__result);
                ArenaIndex = list.Count();
                list.Add("Arena");
                __result = list.ToArray();
                typeof(LocManager).GetMethod("AddLocToLanguage", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(LocManager.Singleton, new object[] { LocManager.Singleton.CurrentLanguage, "mnuArena", "Arena" });
            }
        }
    }
}

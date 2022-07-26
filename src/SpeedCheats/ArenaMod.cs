using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ModLoader;
using Paris;
using Paris.Engine.Context;
using Paris.Engine.Cutscenes;
using Paris.Engine.Graphics;
using Paris.Engine.Menu.Control;
using Paris.Engine.Scene;
using Paris.Engine.System.Achievements;
using Paris.Engine.System.Localisation;
using Paris.Game;
using Paris.Game.HUD;
using Paris.Game.Menu;
using Paris.Game.System;
using Paris.System.FSM;
using Paris.System.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Arena
{
    public class ArenaMod : IMod
    {
        Harmony harmony;
        static int ArenaIndex;
        static Effect crt = null;
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

            harmony.Patch(
                            original: typeof(CheatManager).GetMethod("IsActive", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance),
                            postfix: new HarmonyMethod(typeof(ArenaMod), nameof(IsActive))
                            );

            harmony.Patch(
                            original: typeof(Scene2d).GetMethod("Tick", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance),
                            prefix: new HarmonyMethod(typeof(ArenaMod), nameof(Tick))
                            );
        }

        public static void Tick(Scene2d __instance)
        {
            __instance.SpeedRatio = 3f;
        }

        public static void IsActive(int cheatId, ref bool __result)
        {
            if (cheatId == 42)
                __result = true;

            if (cheatId == 42)
                __result = true;
        }

        public static void Begin(Renderer __instance, ref Effect effect)
        {
           
            effect = crt;
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

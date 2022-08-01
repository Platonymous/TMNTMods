using HarmonyLib;
using Microsoft.Xna.Framework;
using ModLoader;
using Paris;
using Paris.Engine.Context;
using Paris.Engine.Data;
using Paris.Engine.Game.Data;
using Paris.Engine.Menu.Control;
using Paris.Engine.System;
using Paris.Engine.System.Localisation;
using Paris.Game;
using Paris.Game.Actor;
using Paris.Game.Damage;
using Paris.Game.Data;
using Paris.Game.Menu;
using Paris.Game.System;
using Paris.System.FSM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace OnePunch
{
    public class OnePunchMod : IMod
    {
        static Harmony HarmonyMod { get; set; }

        static IModHelper Helper { get; set; }

        static int OnePunchIndex { get; set; } = -1;

        public void ModEntry(IModHelper helper)
        {
                Helper = helper;

            
            try
            {
                HarmonyMod = new Harmony("Platonymous.OnePunchTurtle");
                HarmonyMod.Patch(
                    original: typeof(GameInfo).GetMethod("Init", BindingFlags.Public | BindingFlags.Instance),
                    postfix: new HarmonyMethod(typeof(OnePunchMod), nameof(Init))
                    );
            }
            catch(Exception ex)
            {
                helper.Console.Error(ex.Message);
                helper.Console.Trace(ex.StackTrace);
            }
        }

        public static void Init()
        {
            try
            {
                HarmonyMod.Patch(
                original: typeof(Enum).GetMethod(nameof(Enum.GetNames), BindingFlags.Public | BindingFlags.Static),
                postfix: new HarmonyMethod(typeof(OnePunchMod), nameof(GetEnumNames))
                );

            HarmonyMod.Patch(
                original: typeof(MainMenu).GetMethod("Accept", BindingFlags.Public | BindingFlags.Instance),
                postfix: new HarmonyMethod(typeof(OnePunchMod), nameof(Accept))
            );

            HarmonyMod.Patch(
               original: typeof(GameOverMenu).GetMethod("Accept", BindingFlags.Public | BindingFlags.Instance),
               prefix: new HarmonyMethod(typeof(OnePunchMod), nameof(AcceptGO))
           );

            HarmonyMod.Patch(
                    original: typeof(DifficultyFloat).GetMethod(nameof(DifficultyFloat.Get), BindingFlags.Public | BindingFlags.Instance),
                    postfix: new HarmonyMethod(typeof(OnePunchMod), nameof(GetFloat))
                );

            HarmonyMod.Patch(
                   original: typeof(DifficultyInt).GetMethod(nameof(DifficultyInt.Get), BindingFlags.Public | BindingFlags.Instance),
                   postfix: new HarmonyMethod(typeof(OnePunchMod), nameof(GetInt))
               );


                HarmonyMod.Patch(
                   original: typeof(StageData).GetMethod("GetChallenges", new Type[] { typeof(Difficulty) }),
                   postfix: new HarmonyMethod(typeof(OnePunchMod), nameof(GetChallenges))
               );

                HarmonyMod.Patch(
                   original: typeof(EnemySpawn).GetMethod("Init", BindingFlags.Public | BindingFlags.Instance),
                   postfix: new HarmonyMethod(typeof(OnePunchMod), nameof(EnemySpawnInit))
               );


                HarmonyMod.Patch(
                   original: typeof(Boss).GetMethod("ComputeMaxHP", BindingFlags.NonPublic | BindingFlags.Instance),
                   prefix: new HarmonyMethod(typeof(OnePunchMod), nameof(ComputeMaxHP))
               );

            HarmonyMod.Patch(
                   original: typeof(DamageInfoEx).GetMethod("GetDamage", BindingFlags.Public | BindingFlags.Instance),
                   postfix: new HarmonyMethod(typeof(OnePunchMod), nameof(GetDamage))
               );

                HarmonyMod.Patch(
                   original: typeof(GameInfo).GetMethod("PostArcadeHighScore", BindingFlags.Public | BindingFlags.Instance),
                   prefix: new HarmonyMethod(typeof(OnePunchMod), nameof(PostArcadeHighScore))
               );

                HarmonyMod.Patch(
                   original: typeof(Player).GetMethod("IncreaseLifeCount", BindingFlags.Public | BindingFlags.Instance),
                   prefix: new HarmonyMethod(typeof(OnePunchMod), nameof(IncreaseLifeCount))
               );

            }
            catch (Exception ex)
            {
                Helper.Console.Error(ex.Message);
                Helper.Console.Trace(ex.StackTrace);
            }
        }


        public static void EnemySpawnInit(EnemySpawn __instance)
        {
            if (OnePunchIndex == (int)GameInfo.Singleton.DifficultySetting)
                __instance.MaxHP = 5;
        }
        public static void GetChallenges(StageData __instance, Difficulty difficulty, ref List<ChallengeInfo> __result)
        {
            if (OnePunchIndex == (int)difficulty)
                __result = __instance.GetChallenges(Difficulty.Hard);
        }


            public static void GetFloat(DifficultyFloat __instance, Difficulty difficulty, ref float __result)
        {
            if (OnePunchIndex == (int)difficulty)
                __result = __instance.Get(Difficulty.Hard);
        }

        public static void GetDamage(ref float __result)
        {
            if (OnePunchIndex == (int)GameInfo.Singleton.DifficultySetting)
                __result = 100000f;
        }

        public static bool ComputeMaxHP(Boss __instance)
        {
            if (__instance.HasAuthority && OnePunchIndex == (int)GameInfo.Singleton.DifficultySetting)
            {
                __instance.MaxHP = 5;
                __instance.HP = __instance.MaxHP;
                __instance.GetType().GetField("_replicateMaxHP", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(__instance, __instance.CanReplicate);
                return false;
            }

            return true;
        }

        public static bool IncreaseLifeCount()
        {
            if (OnePunchIndex == (int)GameInfo.Singleton.DifficultySetting)
                return false;

            return true;
        }

        public static bool PostArcadeHighScore()
        {
            if (OnePunchIndex == (int)GameInfo.Singleton.DifficultySetting)
            {
                ContextManager.Singleton.SwitchToContext(GameSettings.Singleton.GameContextMainMenu, 1f, Color.Black);
                return false;
            }

            return true;
        }

        public static void GetInt(DifficultyInt __instance, ref Difficulty difficulty, ref int __result)
        {
            if (OnePunchIndex == (int)difficulty)
                if (__instance.Easy == 5 && __instance.Normal == 3 && __instance.Hard == 3)
                    __result = 1;
                else if (__instance.Easy == 5 && __instance.Normal == 4 && __instance.Hard == 3)
                    __result = 0;
                else
                    __result = __instance.Hard;
        }
        public static bool AcceptGO()
        {
            if (OnePunchIndex == (int)GameInfo.Singleton.DifficultySetting)
            {
                ContextManager.Singleton.SwitchToContext(GameSettings.Singleton.GameContextMainMenu, 1f, Color.Black);
                return false;
            }

            return true;
        }

        public static void Accept(MainMenu __instance)
        {
            FSM fsm = (FSM)typeof(MainMenu).GetField("_fsm", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
            SelectionMenuControl diff = (SelectionMenuControl)typeof(MainMenu).GetField("_difficultySelection", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
            diff.MaxShownItems = Math.Max(diff.MaxShownItems, 7);
            if (fsm.CurrentState == 5 && diff.Selection == OnePunchIndex)
            {
                if (GameInfo.Singleton.GameMode == GameMode.Arcade)
                    GameInfoBase.Singleton.DifficultySetting = (Paris.Engine.Game.Data.Difficulty)OnePunchIndex;
                else if (GameInfo.Singleton.GameMode == GameMode.Story && PlayerManager.Singleton.LocalHostPlayer.Save is SaveGame save3)
                {
                    GameInfoBase.Singleton.DifficultySetting = Paris.Engine.Game.Data.Difficulty.Hard;
                    save3.StoryDifficulty = GameInfoBase.Singleton.DifficultySetting;
                }

                GameInfo.Singleton.ResetArcade();
                ContextManager.Singleton.SwitchToContext(GameSettings.Singleton.GameContextHowToPlay, 1f, Color.Black);
            }
        }

        public static void GetEnumNames(Enum __instance, Type enumType, ref string[] __result)
        {
            if (enumType.Name == "DifficultyItems")
            {
                var list = new List<string>(__result);
                OnePunchIndex = list.Count();
                list.Add("OnePunchTurtle");
                __result = list.ToArray();
                typeof(LocManager).GetMethod("AddLocToLanguage", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(LocManager.Singleton, new object[] { LocManager.Singleton.CurrentLanguage, "mnuOnePunchTurtle", "One Punch Turtle" });
            }
        }
    }
}

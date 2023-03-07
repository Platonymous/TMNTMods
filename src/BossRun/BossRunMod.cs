using HarmonyLib;
using Microsoft.Xna.Framework;
using ModLoader;
using Paris;
using Paris.Engine;
using Paris.Engine.Graphics.Playfield;
using Paris.Engine.Menu.Control;
using Paris.Engine.Messaging;
using Paris.Engine.Scene;
using Paris.Engine.System.Localisation;
using Paris.Engine.Types;
using Paris.Game;
using Paris.Game.Actor;
using Paris.Game.Actor.Camera;
using Paris.Game.HUD;
using Paris.Game.Menu;
using Paris.Game.System;
using Paris.System.FSM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace BossRun
{
    public class BossRunMod : IMod
    {
        Harmony harmony;
        static int BossIndex;
        static IModHelper Helper;
        static private bool _isBossRun = false;
        static Config config = new Config();

        static Scene2d lastSkip = null;

        public static bool IsBossRun
        {
            get
            {
                return _isBossRun && GameInfo.Singleton != null && GameInfo.Singleton.GameMode == GameMode.Arcade;
            }
            set
            {
                _isBossRun = value;
            }
        }

        public void ModEntry(IModHelper helper)
        {
            Helper = helper;

            config = Helper.Config.LoadConfig<Config>();

            helper.Config.SetOptionsMenuEntry("Platonymoud.BossRun.Skip", "Skip Autorunner", (c) =>
            {
                config.SkipAutrorunner = c.Choice == "ON";
                helper.Config.SaveConfig(config);
            }, () => config.SkipAutrorunner ? "ON" : "OFF", new string[] {"ON", "OFF"});

            harmony = new Harmony("Platonymous.Arena");
            harmony.Patch(
                original: typeof(Enum).GetMethod(nameof(Enum.GetNames), System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static),
                postfix: new HarmonyMethod(typeof(BossRunMod), nameof(GetEnumNames))
                );

            harmony.Patch(
                original: typeof(Enum).GetMethod(nameof(Enum.GetValues), System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static),
                postfix: new HarmonyMethod(typeof(BossRunMod), nameof(GetEnumValues))
                );

            harmony.Patch(
                original: typeof(MainMenu).GetMethod("Accept", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance),
                postfix: new HarmonyMethod(typeof(BossRunMod), nameof(Accept))
                           );

            harmony.Patch(
                original: typeof(CheatManager).GetMethod("IsActive", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance),
                postfix: new HarmonyMethod(typeof(BossRunMod), nameof(IsActive))
                           );

            harmony.Patch(
                original: typeof(Playfield).GetMethod("PostTick", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public),
                postfix: new HarmonyMethod(typeof(BossRunMod), nameof(PostTick))
                           );

            harmony.Patch(
                 original: typeof(StageData).GetProperty("ScenePaths", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public).GetGetMethod(),
                 postfix: new HarmonyMethod(typeof(BossRunMod), nameof(ScenePaths))
                            );
        }

        static string last = "";

        static int dTime = 0;

        public static void ScenePaths(StageData __instance, ref List<string> __result)
        {
            if (!IsBossRun)
                return;

            if (__instance.ArcadeNextScenePath.Contains("part1"))
            {
                IsBossRun = false;
                var data = StageList.Singleton.GetStageDataByScenePath(__instance.ArcadeNextScenePath);
                IsBossRun = true;
                __instance.ArcadeNextScenePath = data.ScenePaths.Last();
            }

            if (!IsBossRun || !__result.Any(s => s.Contains("part2")))
                return;
            var l = new List<string>();
            l.Add(__result.Last());

            __result = l;
        }

        public static void PostTick(float deltaTime)
        {
            if (!IsBossRun)
                return;

            if (deltaTime == 0f)
                dTime = 0;

            dTime += 1;



            var auto = SpawnAtBossAutoRunner();

            if (dTime > 60 || auto)
                return;

            SpawnAtBoss();
        }

        public static bool SpawnAtBossAutoRunner()
        {
            try
            {
                if (Scene2d.Active != null && Scene2d.Active.GetGameObjectsOfType<AutoScrollController>().FirstOrDefault<GameObject2d>() is AutoScrollController c && c != null)
                {
                    if (config.SkipAutrorunner)
                    {
                        SkipLevel();
                        return true;
                    }

                    Vector3Displacement _scrollDisplacement = (Vector3Displacement)typeof(AutoScrollController).GetField("_scrollDisplacement", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(c);
                    HashSet<GameObject2d> _encounteredObjects = (HashSet<GameObject2d>)typeof(AutoScrollController).GetField("_encounteredObjects", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(c);

                    List<GameObjectData> gameObject2dList = Scene2d.Active.GameObject2dList.ToList();
                    bool hasboss = false;

                    foreach (GameObjectData gameObjectData in gameObject2dList.ToList())
                    {
                        GameObject2d gameObject = gameObjectData.GameObject;
                        if (gameObject != null)
                        {
                            _encounteredObjects.Add(gameObject);
                            if (!hasboss && gameObject is Boss b && !b.MidBoss)
                            {
                                hasboss = true;
                                gameObject.Active = true;
                            }
                            else if (gameObject is Enemy || gameObject is Hazard)
                            {
                                gameObject.Active = false;
                                gameObject.Despawn();
                                gameObject2dList.Remove(gameObjectData);
                            }
                        }
                    }
                    return true;
                }

            }
            catch
            {

            }

            return false;
        }

        public static void SpawnAtBoss()
        {
            try
            {
                if (Scene2d.Active != null && Scene2d.Active.BaseScene != null && Scene2d.Active.BaseScene.Contains("_16"))
                    return;

                {
                    if (last != Scene2d.Active?.BaseScene)
                        last = Scene2d.Active.BaseScene;


                    Vector3 lastV = Vector3.Zero;

                    foreach (Enemy enemy in Scene2d.Active.GetGameObjectsOfType<Enemy>().OrderBy(enemy => enemy.Position2D.X).ToList())
                    {

                        try
                        {
                            Vector3 vector3 = enemy.Position;
                            short height = Scene2d.Active.Playfield.GetHeightAtPos(vector3.ToVector2()).Height;
                            var nextV = vector3.WithZ((float)height);
                            Scene2d.ForcedSpawnPos = nextV;
                            lastV = nextV.WithX(Scene2d.Active.Playfield.CollisionLayer.Width - 200);
                        }
                        catch
                        {
                            Scene2d.ForcedSpawnPos = lastV;
                        }

                        if (!(enemy is Boss boss && !boss.MidBoss))
                            enemy.Kill();

                    }

                }
            }
            catch
            {

            }
        }

        public static void IsActive(int cheatId, ref bool __result)
        {
            if (!IsBossRun)
                return;

            if (Scene2d.Active != null && Scene2d.Active.BaseScene != null && (Scene2d.Active.BaseScene.Contains("_16")))
                return;

            if (IsBossRun && cheatId == 42)
                __result = true;
        }


        public static void SkipLevel()
        {
            try
            {
                if (Scene2d.Active != null && Scene2d.Active != lastSkip && Scene2d.Active.Players != null && Scene2d.Active.Players.Values.First() is ParisObject player)
                {
                    MessageSystem.Singleton.SendMessage(player, 66, GameInfo.Singleton.CreateLevelCompleteInfo(), true, true);
                    (Scene2d.Active.HUD as MainHUD).ShowCompletionHUD();

                    lastSkip = Scene2d.Active;
                }
            }
            catch
            {

            }
        }


        public static void Accept(MainMenu __instance)
        {
            FSM fsm = (FSM)typeof(MainMenu).GetField("_fsm", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
            SelectionMenuControl start = (SelectionMenuControl)typeof(MainMenu).GetField("_arcadeGameSelection", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
            start.MaxShownItems = 7;
            if (fsm.CurrentState == 5)
                if (start.Selection == BossIndex)
                {
                    IsBossRun = true;
                    GameInfo.Singleton.IsCustomGameMode = false;
                    SelectionMenuControl _difficultySelection = (SelectionMenuControl) __instance.GetType().GetField("_difficultySelection", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
                    _difficultySelection.Selection = 1;
                    fsm.ChangeState((byte)6);
                    lastSkip = null;
                }
                else
                {
                    IsBossRun = false;
                    lastSkip = null;
                }

        }

        public static void GetEnumNames(Enum __instance, Type enumType, ref string[] __result)
        {
            if (enumType.Name == "ArcadeItems")
            {
                var list = new List<string>(__result);
                BossIndex = list.Count();
                list.Add("BossRun");
                __result = list.ToArray();
                typeof(LocManager).GetMethod("AddLocToLanguage", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(LocManager.Singleton, new object[] { LocManager.Singleton.CurrentLanguage, "mnuArcadeBossRun", "Boss Run" });

            }
        }

        public static void GetEnumValues(Enum __instance, Type enumType, ref Array __result)
        {
            if (enumType.Name == "ArcadeItems")
            {
                var list = new List<object>();

                foreach (var obj in __result)
                    list.Add(obj);

                BossIndex = list.Count();
                list.Add(BossIndex);
                __result = list.ToArray();
                typeof(LocManager).GetMethod("AddLocToLanguage", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(LocManager.Singleton, new object[] { LocManager.Singleton.CurrentLanguage, "mnuArcade" + BossIndex, "Boss Run" });

            }
        }
    }

}

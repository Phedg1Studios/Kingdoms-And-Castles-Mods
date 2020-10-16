﻿using Harmony;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using Assets;

namespace Phedg1Studios {
    namespace PauseMenuRememberSpeed {
        public class PauseMenuRememberSpeed : MonoBehaviour {
            static public KCModHelper helper;
            static private List<int> speeds = new List<int>() { 1, 1, 1, 1, 1 };
            static private int resetSpeed = 0;

            void Preload(KCModHelper helper) {
                PauseMenuRememberSpeed.helper = helper;
                var harmony = HarmonyInstance.Create("harmony");
                harmony.PatchAll(Assembly.GetExecutingAssembly());
                Assembly assembly = Assembly.GetAssembly(typeof(FreeResource));
                System.Type type = assembly.GetType("Assets.TimeManager");
                MethodInfo methodInfoA = type.GetMethod("TrySetSpeed", BindingFlags.NonPublic | BindingFlags.Instance);
                MethodInfo methodInfoB = typeof(TimeManagerTrySetSpeed).GetMethod("Postfix", BindingFlags.NonPublic | BindingFlags.Static);
                harmony.Patch(methodInfoA.GetBaseDefinition(), null, new HarmonyMethod(methodInfoB), null);
            }

            static void UpdateSpeed(int speed) {
                speeds.Add(speed);
                speeds.RemoveAt(0);
            }

            [HarmonyPatch(typeof(SpeedControlUI))]
            [HarmonyPatch("SetSpeed")]
            public static class SpeedControlUISetSpeed {
                static void Prefix(SpeedControlUI __instance, int idx) {
                    __instance.pauseButton.transform.parent.GetComponent<UnityEngine.UI.ToggleGroup>().allowSwitchOff = true;
                }

                static void Postfix(SpeedControlUI __instance, int idx) {
                    __instance.pauseButton.transform.parent.GetComponent<UnityEngine.UI.ToggleGroup>().allowSwitchOff = false;
                    UpdateSpeed(idx);
                }
            }

            public static class TimeManagerTrySetSpeed {
                static void Postfix(int idx) {
                    UpdateSpeed(idx);
                }
            }

            [HarmonyPatch(typeof(MainMenuMode))]
            [HarmonyPatch("Shutdown")]
            public static class MainMenuModeShutdown {
                static void Postfix() {
                    resetSpeed = speeds[0];
                    SpeedControlUI.inst.SetSpeed(resetSpeed);
                }
            }

            [HarmonyPatch(typeof(MainMenuMode))]
            [HarmonyPatch("OnClickedReturnToGame")]
            public static class MainMenuModeOnClickedReturnToGame {
                static void Postfix() {
                    SpeedControlUI.inst.SetSpeed(resetSpeed);
                }
            }
        }
    }
}
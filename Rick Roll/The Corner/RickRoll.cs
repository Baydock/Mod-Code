using Assets.Scripts.Models.Map;
using Assets.Scripts.Unity;
using Assets.Scripts.Unity.Map;
using Assets.Scripts.Unity.UI_New;
using Assets.Scripts.Unity.UI_New.Main.MapSelect;
using Assets.Scripts.Utils;
using HarmonyLib;
using MelonLoader;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using NinjaKiwi.Common;
using Assets.Scripts.Data;
using Assets.Scripts.Data.MapSets;
using System.Collections.Generic;

namespace RickRoll {

    public class RickRollMod : MelonMod {
        public static AssetBundle SceneBundle = null;
        public static AssetBundle AssetBundle = null;

        public override void OnApplicationStart() {
            SceneBundle = AssetBundle.LoadFromMemory(Properties.Resources.RickRollScene);
            AssetBundle = AssetBundle.LoadFromMemory(Properties.Resources.RickRollAssets);
        }
    }

    [HarmonyPatch(typeof(Game), nameof(Game.Awake))]
    public class Awake_Patch {
        [HarmonyPostfix]
        public static void Postfix(ref Game __instance) {
            GameData gameData = GameData.Instance;
            List<MapDetails> mapDetails = new List<MapDetails>(gameData.mapSet.Maps.items);
            mapDetails.Add(RickRollMap.Details);
            gameData.mapSet.Maps.items = mapDetails.ToArray();
        }
    }

    [HarmonyPatch(typeof(MapSelectScreen), nameof(MapSelectScreen.Open))]
    public class Open_Patch {
        [HarmonyPostfix]
        public static void Postfix(Il2CppSystem.Object data) => MelonLogger.Msg(data is null);
    }

    [HarmonyPatch(typeof(MapLoader), nameof(MapLoader.Load))]
    public class MapLoad_Patch {
        [HarmonyPrefix]
        public static bool Prefix(MapLoader __instance, string map, Il2CppSystem.Action<MapModel> loadedCallback) {
            if (map.Equals(RickRollMap.Name)) {
                __instance.currentMapName = map;

                loadedCallback.Invoke(RickRollMap.Model);

                SceneManager.LoadScene("Rick Roll", new LoadSceneParameters {
                    loadSceneMode = LoadSceneMode.Additive,
                    localPhysicsMode = LocalPhysicsMode.None
                });
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(UI), nameof(UI.DestroyAndUnloadMapScene))]
        public class MapClear_Patch {
            [HarmonyPrefix]
            public static bool Prefix(UI __instance) {
                if (__instance.mapLoader.currentMapName.Equals(RickRollMap.Name)) {
                    SceneManager.UnloadScene("Rick Roll");
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(ResourceLoader), "LoadSpriteFromSpriteReferenceAsync")]
        public class ResourceLoader_Patch {
            [HarmonyPrefix]
            public static bool Prefix(SpriteReference reference, Image image) {
                if (reference != null && reference.guidRef == RickRollMap.Name) {
                    Texture2D texture = RickRollMod.AssetBundle.LoadAsset("Rick Roll").Cast<Texture2D>();
                    image.canvasRenderer.SetTexture(texture);
                    image.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2());
                    return false;
                }
                return true;
            }
        }
    }
}
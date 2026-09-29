using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ButterflyStep.EditorTools
{
    public static partial class GameBuilder
    {
        private static readonly string[] Developers =
        {
            "Gabriel Trindade Santana",
            "Nathan Marques Credidio Costa",
            "Jefferson Moisés dos Santos Souza"
        };

        [MenuItem("Butterfly Step/Construir Cutscenes (Intro e Final)")]
        public static string BuildCutscenes()
        {
            var cutsceneArt = LoadCutsceneArt();
            BuildCutsceneScene("Intro", false, "Level01", cutsceneArt);
            BuildCutsceneScene("Ending", true, "MainMenu", cutsceneArt);
            SetupBuildSettings();
            AssetDatabase.SaveAssets();
            return "Cutscenes construídas.";
        }

        private static void BuildCutsceneScene(string name, bool isEnding, string next, CutsceneArt cutsceneArt)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var camGo = new GameObject("Camera") { tag = "MainCamera" };
            var cam = camGo.AddComponent<Camera>();
            cam.orthographic = true;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Color.black;
            camGo.transform.position = new Vector3(0f, 0f, -10f);
            camGo.AddComponent<AudioListener>();
            var go = new GameObject("Cutscene");
            go.AddComponent<Cutscene>().Setup(isEnding, next, cutsceneArt, Developers);
            EditorSceneManager.SaveScene(scene, ScenePath(name));
        }

        private static CutsceneArt LoadCutsceneArt()
        {
            string fx = $"{Root}/Art/CC0/FX";
            string hooded = $"{ArtRoot}/Penzilla/HoodedProtagonist.png";
            string treesBg = $"{LegacyRoot}/Trees/Background.png";
            string green = $"{LegacyRoot}/Trees/Green-Tree.png";
            string tilesPath = $"{LegacyRoot}/Assets/Tiles.png";
            return new CutsceneArt
            {
                font = AssetDatabase.LoadAssetAtPath<Font>($"{Root}/Art/Fonts/VT323-Regular.ttf"),
                ecoIdle = SpritesAt(hooded, "HoodedProtagonist_r0_"),
                ecoWalk = SpritesAt(hooded, "HoodedProtagonist_r2_"),
                ecoVanish = SpritesAt(hooded, "HoodedProtagonist_r6_"),
                cronoRun = SpritesAt($"{LegacyRoot}/Mob/Boar/Run/Run-Sheet.png", ""),
                timeWave = SpritesAt($"{fx}/TimeWave.png", ""),
                burst = SpritesAt($"{fx}/Burst.png", ""),
                sparkle = SpritesAt($"{fx}/Sparkle.png", ""),
                butterflies = SpritesAt($"{fx}/Butterflies.png", ""),
                birds = new[] { SpriteAt($"{Root}/Art/CC0/Bird/Bird_1.png", "bird0"), SpriteAt($"{Root}/Art/CC0/Bird/Bird_2.png", "bird1") }.Where(s => s != null).ToArray(),
                farTrees = new[] { "treeD0", "treeD1", "treeD2" }.Select(n => SpriteAt(treesBg, n)).Where(s => s != null).ToArray(),
                pines = new[] { "pineBig", "pineMid", "pineSmall" }.Select(n => SpriteAt(green, n)).Where(s => s != null).ToArray(),
                clock = SpriteAt($"{fx}/TimeClock.png", "TimeClock"),
                swirl = SpriteAt($"{fx}/TimeSwirl.png", "TimeSwirl"),
                clouds = SpriteAt($"{LegacyRoot}/Background/Background.png", "clouds"),
                mountDark = SpriteAt(treesBg, "mountDark"),
                mountLight = SpriteAt(treesBg, "mountLight"),
                groundFill = SpriteAt(tilesPath, "groundFill"),
                grassTop = SpriteAt(tilesPath, "grassTop")
            };
        }

        private static Sprite SpriteAt(string path, string name)
        {
            return AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().FirstOrDefault(s => s.name == name);
        }

        private static Sprite[] SpritesAt(string path, string prefix)
        {
            var list = new List<Sprite>(AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().Where(s => s.name.StartsWith(prefix)));
            list.Sort((a, b) => TrailingNumber(a.name).CompareTo(TrailingNumber(b.name)));
            return list.ToArray();
        }

        private static int TrailingNumber(string name)
        {
            int i = name.Length;
            while (i > 0 && char.IsDigit(name[i - 1])) i--;
            return i < name.Length && int.TryParse(name.Substring(i), out int n) ? n : 0;
        }
    }
}

using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ButterflyStep.EditorTools
{
    public static partial class GameBuilder
    {
        private static readonly string[] SceneOrder = { "MainMenu", "Level01", "Level02", "Level03", "Level04", "Level05", "Level06", "Level07", "Level08", "Level09", "Level10", "Ending" };

        private static Transform groupEnv;
        private static Transform groupBackground;
        private static Transform groupTemporal;
        private static Transform groupInteract;
        private static Transform groupEnemies;
        private static Transform groupRules;
        private static Transform groupTriggers;

        [MenuItem("Butterfly Step/Construir Projeto Completo (sobrescreve cenas)")]
        private static void BuildAllMenu()
        {
            if (!EditorUtility.DisplayDialog("Butterfly Step", "Isto recria prefabs e as 7 cenas do jogo. Alterações manuais nessas cenas serão perdidas. Continuar?", "Construir", "Cancelar")) return;
            BuildAll();
        }

        public static string BuildAll()
        {
            EnsureFolders();
            EnsureLayers();
            CreateSprites();
            LoadArt();
            LoadTiles();
            LoadFx();
            sceneryProps = 0;
            uiFont = null;
            CreateMaterials();
            CreateInput();
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            CreatePrefabs();
            BuildMenuScene();
            BuildLevel01();
            BuildLevel02();
            BuildLevel03();
            BuildLevel04();
            BuildLevel05();
            BuildLevel06();
            BuildLevel07();
            BuildLevel08();
            BuildLevel09();
            BuildLevel10();
            BuildEndingScene();
            SetupBuildSettings();
            AssetDatabase.SaveAssets();
            EditorSceneManager.OpenScene(ScenePath("Level01"));
            return "Butterfly Step construído com sucesso.";
        }

        private static string ScenePath(string name) => $"{Root}/Scenes/{name}.unity";

        private static void SetupBuildSettings()
        {
            var scenes = new List<EditorBuildSettingsScene>();
            foreach (var s in SceneOrder) scenes.Add(new EditorBuildSettingsScene(ScenePath(s), true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }

        private static Scene NewScene() => EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        private static void Save(Scene scene, string name)
        {
            if (name.StartsWith("Level") && int.TryParse(name.Substring(5), out int level))
            {
                var flow = Object.FindFirstObjectByType<LevelFlow>();
                if (flow != null) Set(flow, "levelNumber", level);
                PlaceExtras(level);
            }
            foreach (var temporal in Object.FindObjectsByType<TemporalObject>(FindObjectsInactive.Include, FindObjectsSortMode.None)) temporal.PreviewFirstState();
            EditorSceneManager.SaveScene(scene, ScenePath(name));
        }

        private static T Spawn<T>(GameObject prefab, Vector3 position, Transform parent = null) where T : Component
        {
            var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            if (parent != null) go.transform.SetParent(parent, false);
            go.transform.position = position;
            return go.GetComponent<T>();
        }

        private static GameObject SpawnGo(GameObject prefab, Vector3 position, Transform parent = null)
        {
            var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            if (parent != null) go.transform.SetParent(parent, false);
            go.transform.position = position;
            return go;
        }

        private struct LevelSpec
        {
            public string sceneName;
            public string title;
            public string intro;
            public string complete;
            public string next;
            public Season season;
            public int maxDay;
            public int step;
            public int startIndex;
            public bool stasis;
            public Vector2 spawn;
            public Vector2 camMin;
            public Vector2 camMax;
        }

        private static Scene BeginLevel(LevelSpec spec)
        {
            var scene = NewScene();

            var systems = SpawnGo(systemsPrefab, Vector3.zero);
            systems.name = "LevelSystems";
            var context = systems.GetComponent<LevelContext>();
            var time = systems.GetComponent<TimeManager>();
            var flow = systems.GetComponent<LevelFlow>();
            Set(time, "maxDay", spec.maxDay);
            Set(time, "dayStep", spec.step);
            Set(time, "calendar.startSeason", (int)spec.season);
            Set(time, "calendar.daysPerSeason", 30);
            Set(time, "calendar.startDayOfSeason", 1);
            Set(time, "startIndex", spec.startIndex);
            Set(systems.GetComponent<TimeStasis>(), "unlocked", spec.stasis);
            Set(flow, "levelTitle", spec.title);
            Set(flow, "introText", spec.intro);
            Set(flow, "completeText", spec.complete);
            Set(flow, "nextScene", spec.next);

            var cam = Spawn<CameraFollow>(cameraPrefab, new Vector3(spec.spawn.x, spec.spawn.y, -10f));
            cam.name = "GameCamera";
            var player = Spawn<PlayerController>(playerPrefab, spec.spawn);
            player.name = "Player";
            var hud = Spawn<GameHUD>(hudPrefab, Vector3.zero);
            hud.name = "HUD";

            Set(context, "player", player);
            Set(context, "hud", hud);
            Set(cam, "target", player.transform);
            Set(cam, "minBounds", spec.camMin);
            Set(cam, "maxBounds", spec.camMax);
            Set(hud, "cameraFollow", cam);
            var atmosphere = systems.GetComponent<TimeAtmosphere>();
            Set(atmosphere, "targetCamera", cam.GetComponent<Camera>());
            string[] weatherNames = { "Weather_Primavera", "Weather_Verao", "Weather_Outono", "Weather_Inverno" };
            for (int i = 0; i < 4; i++)
            {
                var weather = cam.transform.Find(weatherNames[i]);
                if (weather != null) Set(atmosphere, $"looks.Array.data[{i}].weather", weather.GetComponent<ParticleSystem>());
            }

            groupBackground = new GameObject("--- Background").transform;
            groupEnv = new GameObject("--- Environment").transform;
            groupTemporal = new GameObject("--- Temporal Objects").transform;
            groupInteract = new GameObject("--- Interactables").transform;
            groupEnemies = new GameObject("--- Enemies").transform;
            groupRules = new GameObject("--- Consequence Rules").transform;
            groupTriggers = new GameObject("--- Triggers").transform;

            Background(spec.camMin.x, spec.camMax.x, spec.camMin.y);
            Kill(spec.camMin.x - 5f, spec.camMin.y - 3f, spec.camMax.x - spec.camMin.x + 10f);
            return scene;
        }

        private static void Silhouettes(string name, Sprite[] sprites, float parallax, float xMin, float xMax, float y, float height, float spacing, Color color, int order)
        {
            var layer = new GameObject(name).transform;
            layer.SetParent(groupBackground, false);
            layer.gameObject.AddComponent<ParallaxLayer>().Setup(parallax);
            var rng = new System.Random(Mathf.RoundToInt(xMin * 7 + xMax * 3 + order));
            if (sprites.Length < 3 || sprites[0] == null || sprites[1] == null || sprites[2] == null) return;
            int i = 0;
            float cursor = xMin - 25f;
            while (cursor < xMax + 25f)
            {
                float scale = height / sprites[1].bounds.size.y * (0.85f + (float)rng.NextDouble() * 0.3f);
                int middles = 1 + rng.Next(4);
                for (int p = 0; p < middles + 2; p++)
                {
                    var sprite = p == 0 ? sprites[0] : p == middles + 1 ? sprites[2] : sprites[1];
                    var go = Go(name + "_" + i++, layer, new Vector3(cursor - sprite.bounds.min.x * scale, y, 0f));
                    AddSprite(go, sprite, color, order);
                    go.transform.localScale = new Vector3(scale, scale, 1f);
                    cursor += sprite.bounds.size.x * scale;
                }
                cursor += spacing * ((float)rng.NextDouble() * 1.4f - 0.9f);
            }
        }

        private static void ArtLayer(string name, Sprite sprite, float parallax, float xMin, float xMax, float y, float height, Color color, int order)
        {
            var layer = new GameObject(name).transform;
            layer.SetParent(groupBackground, false);
            layer.gameObject.AddComponent<ParallaxLayer>().Setup(parallax, name == "Sky");
            float width = xMax - xMin + 60f;
            var go = Go(name + "_Sprite", layer, new Vector3((xMin + xMax) * 0.5f, y, 0f));
            var sr = AddSprite(go, sprite, color, order);
            sr.drawMode = SpriteDrawMode.Tiled;
            sr.tileMode = SpriteTileMode.Continuous;
            float scale = height / sprite.bounds.size.y;
            go.transform.localScale = new Vector3(scale, scale, 1f);
            sr.size = new Vector2(width / scale, sprite.bounds.size.y);
        }

        private static void Background(float xMin, float xMax, float yMin)
        {
            if (skySprite != null && tiles != null && tiles.mountDark != null)
            {
                ArtLayer("Sky", skySprite, 0.97f, xMin, xMax, yMin - 8f, 30f, Color.white, -100);
                ArtLayer("Mountains_Far", tiles.mountLight, 0.88f, xMin, xMax, -10f, 18f, new Color(0.9f, 0.96f, 1f, 0.6f), -98);
                Silhouettes("Trees_Far", tiles.treeLight, 0.72f, xMin, xMax, -2f, 10f, 5f, new Color(0.92f, 0.98f, 1f, 0.8f), -96);
                ArtLayer("Mountains_Near", tiles.mountDark, 0.58f, xMin, xMax, -12f, 16f, new Color(0.85f, 0.9f, 0.95f), -94);
                Silhouettes("Trees_Near", tiles.treeDark, 0.45f, xMin, xMax, -2.5f, 8f, 4f, Color.white, -92);
                DarkForest(0.3f, xMin, xMax, -1.5f);
                if (yMin < -6f)
                {
                    ArtLayer("Mountains_Low", tiles.mountDark, 0.5f, xMin, xMax, yMin - 6f, 8f, new Color(0.8f, 0.85f, 0.9f), -93);
                }
                return;
            }
            var far = new GameObject("Far_Mountains").transform;
            far.SetParent(groupBackground, false);
            far.gameObject.AddComponent<ParallaxLayer>().Setup(0.8f);
            var mid = new GameObject("Mid_Hills").transform;
            mid.SetParent(groupBackground, false);
            mid.gameObject.AddComponent<ParallaxLayer>().Setup(0.55f);
            var clouds = new GameObject("Clouds").transform;
            clouds.SetParent(groupBackground, false);
            clouds.gameObject.AddComponent<ParallaxLayer>().Setup(0.9f);

            var rng = new System.Random(Mathf.RoundToInt(xMin * 13 + xMax));
            for (float x = xMin - 10f; x < xMax + 10f; x += 7f)
            {
                float h = 7f + (float)rng.NextDouble() * 5f;
                Shape("Mountain", far, new Vector2(x, yMin + 2f + h * 0.5f), new Vector2(10f + (float)rng.NextDouble() * 4f, h), triangleSprite, new Color(0.35f, 0.4f, 0.55f, 0.55f), -100);
            }
            for (float x = xMin - 6f; x < xMax + 6f; x += 5f)
            {
                float s = 6f + (float)rng.NextDouble() * 4f;
                Shape("Hill", mid, new Vector2(x, yMin + 1.5f), new Vector2(s * 1.6f, s), circleSprite, new Color(0.3f, 0.5f, 0.35f, 0.5f), -90);
            }
            for (float x = xMin; x < xMax; x += 9f)
            {
                float y = yMin + 13f + (float)rng.NextDouble() * 4f;
                Shape("Cloud", clouds, new Vector2(x, y), new Vector2(3.5f, 1.4f), circleSprite, new Color(1f, 1f, 1f, 0.6f), -95);
                Shape("Cloud", clouds, new Vector2(x + 1.3f, y + 0.4f), new Vector2(2.5f, 1.5f), circleSprite, new Color(1f, 1f, 1f, 0.6f), -95);
            }
        }

        private static GameObject Ground(float xMin, float yMin, float w, float h, bool grass = true)
        {
            var go = Rect("Ground", groupEnv, xMin, yMin, w, h, GroundColor, 0, true);
            if (grass && tiles != null && tiles.grassTop != null) Rect("Grass", go.transform, xMin, yMin + h - 0.5f, w, tiles.grassTop.bounds.size.y, Color.white, 1, false, tiles.grassTop);
            else if (grass) Rect("Grass", go.transform, xMin, yMin + h - 0.25f, w, 0.25f, GrassColor, 1, false, squareSprite);
            if (grass) SnowCover(go.transform, xMin, yMin + h, w);
            return go;
        }

        private static GameObject Wall(float xMin, float yMin, float w, float h, string name = "Wall")
        {
            if (name == "Limite" || name.StartsWith("Muro") || name.StartsWith("Pared") || name.StartsWith("Muralha")) h = Mathf.Max(h, 60f - yMin);
            return Rect(name, groupEnv, xMin, yMin, w, h, RockColor, 0, true);
        }

        private static GameObject OneWay(string name, Transform parent, float xMin, float yTop, float w, Color color, int order = 3)
        {
            var go = Rect(name, parent, xMin, yTop - 0.35f, w, 0.35f, color, order, true);
            go.GetComponent<BoxCollider2D>().usedByEffector = true;
            var effector = go.AddComponent<PlatformEffector2D>();
            effector.useOneWay = true;
            effector.surfaceArc = 160f;
            return go;
        }

        private static void Sign(float x, float y, string text)
        {
            var sign = Spawn<StorySign>(signPrefab, new Vector3(x, y, 0f), groupTriggers);
            Set(sign, "text", text);
        }

        private static void Exit(float x, float y) => SpawnGo(exitPrefab, new Vector3(x, y, 0f), groupTriggers);

        private static void Kill(float xMin, float y, float w)
        {
            var go = SpawnGo(deathZonePrefab, new Vector3(xMin + w * 0.5f, y, 0f), groupTriggers);
            Set(go.GetComponent<BoxCollider2D>(), "m_Size", new Vector2(w, 2f));
        }

        private static PushableBox Box(float x, float y, string id)
        {
            var box = Spawn<PushableBox>(boxPrefab, new Vector3(x, y, 0f), groupInteract);
            box.name = id;
            Set(box, "timelineId", id);
            return box;
        }

        private static TemporalEnemy Enemy(GameObject prefab, string name, float x, float y, float left, float right, string deathFlag = "")
        {
            var enemy = Spawn<TemporalEnemy>(prefab, new Vector3(x, y, 0f), groupEnemies);
            enemy.name = name;
            Set(enemy, "displayName", name);
            Set(enemy, "patrolLeft", left);
            Set(enemy, "patrolRight", right);
            Set(enemy, "deathFlag", deathFlag);
            return enemy;
        }

        private static TemporalEnemy Offspring(GameObject prefab, string name, float x, float y, float left, float right, int birthDay, string parentFlag)
        {
            var enemy = Enemy(prefab, name, x, y, left, right);
            Set(enemy, "birthDay", birthDay);
            Set(enemy, "parentDeathFlag", parentFlag);
            return enemy;
        }

        private static void Tameable(TemporalEnemy enemy, string flag, int afterDays, bool platform)
        {
            Set(enemy, "tameFlag", flag);
            Set(enemy, "tameAfterDays", afterDays);
            Set(enemy, "tamePlatform", platform);
        }

        private static void Rule(string flag, string description, params TemporalCondition[] causes)
        {
            var go = new GameObject($"Rule_{flag}");
            go.transform.SetParent(groupRules, false);
            go.AddComponent<ConsequenceRule>().Setup(flag, description, causes);
        }

        private static TemporalObject Temporal(string name, Vector2 position, SpriteRenderer visual = null, params Collider2D[] colliders)
        {
            var go = Go(name, groupTemporal, position);
            var t = go.AddComponent<TemporalObject>();
            t.Setup(name, visual, colliders);
            return t;
        }

        private static GameObject Part(Transform parent, string name, float xMin, float yMin, float w, float h, Color color, int order, bool solid, Sprite sprite = null)
        {
            return Rect(name, parent, xMin, yMin, w, h, color, order, solid, sprite);
        }

        private static Interactable Interact(Transform parent, Vector2 position, float radius, string flag, string prompt, string message, params TemporalCondition[] conditions)
        {
            var go = Go($"Interact_{flag}", parent, position, interactableLayer);
            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = radius;
            var highlight = tiles != null && tiles.uiArrowDown != null
                ? Shape("Indicador", go.transform, position + Vector2.up * (radius + 0.5f), new Vector2(1.6f, 1.6f), tiles.uiArrowDown, InteractColor, 30)
                : Shape("Highlight", go.transform, position, new Vector2(radius * 1.6f, radius * 1.6f), circleSprite, new Color(InteractColor.r, InteractColor.g, InteractColor.b, 0.3f), 4);
            highlight.layer = interactableLayer;
            var sr = highlight.GetComponent<SpriteRenderer>();
            sr.enabled = false;
            var interactable = go.AddComponent<Interactable>();
            interactable.Setup(flag, prompt, message, conditions);
            interactable.SetHighlight(sr);
            return interactable;
        }

        private static void BuildEndingScene()
        {
            var scene = NewScene();
            BuildStoryScene("Ending", "MainMenu", new Color(0.08f, 0.06f, 0.14f),
                "FIM DO PROTÓTIPO",
                "“Toda escolha deixa uma marca no futuro.”",
                "Com os dez fragmentos no lugar, o Relógio de Eco volta a bater.\n" +
                "O caminho de casa se abre... mas o vale que Eco deixa para trás não é o mesmo que encontrou.\n\n" +
                "A muda regada virou floresta. O pássaro libertado virou um bando.\n" +
                "A caixa esquecida ainda segura a porta. A pedra solta no inverno abriu um rio.\n" +
                "Cada pequeno passo mudou o futuro: esse é o efeito borboleta.\n\n" +
                "Obrigado por jogar!",
                "Pressione ENTER para voltar ao menu");
            Save(scene, "Ending");
        }

        private static void BuildStoryScene(string name, string next, Color background, string title, string subtitle, string body, string hint)
        {
            var camGo = new GameObject("Camera") { tag = "MainCamera" };
            var cam = camGo.AddComponent<Camera>();
            cam.orthographic = true;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = background;
            camGo.transform.position = new Vector3(0f, 0f, -10f);
            camGo.AddComponent<AudioListener>();

            var canvas = MakeCanvas("StoryCanvas", 0);
            var root = Stretch("Content", canvas.transform);
            var group = Group(root, 0f);

            var butterfly = UIRect("Butterfly", root, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 0.5f), new Vector2(0f, -120f), new Vector2(160f, 100f));
            UIImage(UIRect("WingL", butterfly, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-4f, 0f), new Vector2(70f, 90f)), new Color(1f, 0.8f, 0.3f), circleSprite);
            UIImage(UIRect("WingR", butterfly, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 0.5f), new Vector2(4f, 0f), new Vector2(70f, 90f)), new Color(0.75f, 0.55f, 1f), circleSprite);
            UIImage(UIRect("Body", butterfly, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(12f, 80f)), new Color(0.2f, 0.15f, 0.3f));

            UIText(UIRect("Title", root, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -190f), new Vector2(1600f, 110f)), title, 90, TextAnchor.MiddleCenter, new Color(1f, 0.87f, 0.45f), FontStyle.Bold);
            UIText(UIRect("Subtitle", root, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -300f), new Vector2(1600f, 50f)), subtitle, 32, TextAnchor.MiddleCenter, new Color(0.85f, 0.8f, 1f), FontStyle.Italic);
            UIText(UIRect("Body", root, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -70f), new Vector2(1400f, 420f)), body, 28, TextAnchor.MiddleCenter, Color.white);
            var hintText = UIText(UIRect("Hint", root, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 60f), new Vector2(1000f, 50f)), hint, 30, TextAnchor.MiddleCenter, new Color(1f, 0.9f, 0.5f), FontStyle.Bold);

            var screen = canvas.gameObject.AddComponent<StoryScreen>();
            screen.Setup(group, hintText, next, controls);
        }
    }
}

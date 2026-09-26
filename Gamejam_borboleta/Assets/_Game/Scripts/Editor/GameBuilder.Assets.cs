using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

namespace ButterflyStep.EditorTools
{
    public static partial class GameBuilder
    {
        private static Sprite squareSprite;
        private static Sprite blockSprite;
        private static Sprite circleSprite;
        private static Sprite triangleSprite;
        private static Sprite diamondSprite;
        private static Material particleMaterial;
        private static PhysicsMaterial2D noFriction;
        private static PhysicsMaterial2D boxMaterial;
        private static InputActionAsset controls;

        private static GameObject playerPrefab;
        private static GameObject hudPrefab;
        private static GameObject cameraPrefab;
        private static GameObject systemsPrefab;
        private static GameObject boxPrefab;
        private static GameObject exitPrefab;
        private static GameObject signPrefab;
        private static GameObject deathZonePrefab;
        private static GameObject slimePrefab;
        private static GameObject chronoferaPrefab;
        private static GameObject mothPrefab;
        private static GameObject waspPrefab;
        private static GameObject thornPrefab;
        private static EnemyProjectile projectilePrefab;
        private static EnemyProjectile shockwavePrefab;

        private static int playerLayer;
        private static int enemyLayer;
        private static int interactableLayer;

        private static readonly Color PlayerColor = new Color(0.3f, 0.6f, 1f);
        private static readonly Color InteractColor = new Color(1f, 0.85f, 0.25f);
        private static readonly Color TemporalColor = new Color(0.65f, 0.45f, 0.95f);
        private static readonly Color DoorColor = new Color(0.55f, 0.56f, 0.62f);
        private static readonly Color ConsequenceColor = new Color(0.35f, 0.85f, 0.45f);
        private static readonly Color GroundColor = new Color(0.42f, 0.33f, 0.26f);
        private static readonly Color GrassColor = new Color(0.36f, 0.62f, 0.3f);
        private static readonly Color RockColor = new Color(0.5f, 0.48f, 0.5f);
        private static readonly Color WaterColor = new Color(0.3f, 0.62f, 1f, 0.85f);
        private static readonly Color WoodColor = new Color(0.62f, 0.44f, 0.26f);

        private static void EnsureFolders()
        {
            foreach (var f in new[]
            {
                "Art/Placeholders", "Audio", "Materials", "Prefabs/Player", "Prefabs/Enemies", "Prefabs/Temporal",
                "Prefabs/Environment", "Prefabs/UI", "Scenes", "Settings", "ScriptableObjects"
            })
            {
                EnsureFolder($"{Root}/{f}");
            }
        }

        private static void EnsureLayers()
        {
            var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            var layers = tagManager.FindProperty("layers");
            SetLayer(layers, 6, "Player");
            SetLayer(layers, 7, "Enemy");
            SetLayer(layers, 8, "Interactable");
            tagManager.ApplyModifiedPropertiesWithoutUndo();
            playerLayer = LayerMask.NameToLayer("Player");
            enemyLayer = LayerMask.NameToLayer("Enemy");
            interactableLayer = LayerMask.NameToLayer("Interactable");
        }

        private static void SetLayer(SerializedProperty layers, int index, string name)
        {
            var p = layers.GetArrayElementAtIndex(index);
            if (string.IsNullOrEmpty(p.stringValue)) p.stringValue = name;
            else if (p.stringValue != name) Debug.LogWarning($"[GameBuilder] Layer {index} já usado por '{p.stringValue}'.");
        }

        private static void CreateSprites()
        {
            squareSprite = MakeSprite("Square", 32, 32, (x, y, s) => Color.white);
            blockSprite = MakeSprite("Block", 32, 32, (x, y, s) =>
            {
                bool edge = x == 0 || y == 0 || x == s - 1 || y == s - 1;
                return edge ? new Color(0.78f, 0.78f, 0.78f) : Color.white;
            });
            circleSprite = MakeSprite("Circle", 64, 64, (x, y, s) =>
            {
                float r = s * 0.5f;
                float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(r, r));
                return new Color(1f, 1f, 1f, Mathf.Clamp01(r - d));
            });
            triangleSprite = MakeSprite("Triangle", 64, 64, (x, y, s) =>
            {
                float half = (1f - (y + 0.5f) / s) * s * 0.5f;
                float dx = Mathf.Abs(x + 0.5f - s * 0.5f);
                return new Color(1f, 1f, 1f, Mathf.Clamp01(half - dx + 0.5f));
            });
            diamondSprite = MakeSprite("Diamond", 64, 64, (x, y, s) =>
            {
                float c = s * 0.5f;
                float d = Mathf.Abs(x + 0.5f - c) + Mathf.Abs(y + 0.5f - c);
                return new Color(1f, 1f, 1f, Mathf.Clamp01(c - d));
            });
        }

        private static Sprite MakeSprite(string name, int size, int ppu, Func<int, int, int, Color> pixel)
        {
            string path = $"{Root}/Art/Placeholders/{name}.png";
            if (!File.Exists(path))
            {
                var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
                for (int y = 0; y < size; y++)
                {
                    for (int x = 0; x < size; x++) tex.SetPixel(x, y, pixel(x, y, size));
                }
                tex.Apply();
                File.WriteAllBytes(path, tex.EncodeToPNG());
                UnityEngine.Object.DestroyImmediate(tex);
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            }

            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = ppu;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.filterMode = name == "Block" || name == "Square" || name.StartsWith("Pixel") ? FilterMode.Point : FilterMode.Bilinear;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.wrapMode = TextureWrapMode.Clamp;
            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect;
            settings.spriteAlignment = (int)SpriteAlignment.Center;
            importer.SetTextureSettings(settings);
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static void CreateMaterials()
        {
            string matPath = $"{Root}/Materials/ParticleUnlit.mat";
            particleMaterial = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (particleMaterial == null)
            {
                var shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
                particleMaterial = new Material(shader) { name = "ParticleUnlit" };
                AssetDatabase.CreateAsset(particleMaterial, matPath);
            }
            particleMaterial.mainTexture = circleSprite.texture;
            EditorUtility.SetDirty(particleMaterial);

            noFriction = LoadOrCreatePhysics("NoFriction", 0f);
            boxMaterial = LoadOrCreatePhysics("BoxFriction", 0.5f);
        }

        private static PhysicsMaterial2D LoadOrCreatePhysics(string name, float friction)
        {
            string path = $"{Root}/Materials/{name}.physicsMaterial2D";
            var mat = AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>(path);
            if (mat == null)
            {
                mat = new PhysicsMaterial2D(name);
                AssetDatabase.CreateAsset(mat, path);
            }
            mat.friction = friction;
            mat.bounciness = 0f;
            EditorUtility.SetDirty(mat);
            return mat;
        }

        private static void CreateInput()
        {
            string path = $"{Root}/Settings/ButterflyControls.inputactions";
            {
                var asset = ScriptableObject.CreateInstance<InputActionAsset>();
                var map = asset.AddActionMap("Gameplay");
                var move = map.AddAction("Move", InputActionType.Value, expectedControlLayout: "Vector2");
                move.AddCompositeBinding("2DVector").With("Up", "<Keyboard>/w").With("Down", "<Keyboard>/s").With("Left", "<Keyboard>/a").With("Right", "<Keyboard>/d");
                move.AddCompositeBinding("2DVector").With("Up", "<Keyboard>/upArrow").With("Down", "<Keyboard>/downArrow").With("Left", "<Keyboard>/leftArrow").With("Right", "<Keyboard>/rightArrow");
                move.AddBinding("<Gamepad>/leftStick");
                move.AddBinding("<Gamepad>/dpad");
                AddButton(map, "Jump", "<Keyboard>/space", "<Gamepad>/buttonSouth");
                AddButton(map, "Interact", "<Keyboard>/f", "<Gamepad>/buttonNorth");
                AddButton(map, "Attack", "<Keyboard>/j", "<Mouse>/leftButton", "<Gamepad>/buttonWest");
                AddButton(map, "TimeBack", "<Keyboard>/q", "<Gamepad>/leftShoulder");
                AddButton(map, "TimeForward", "<Keyboard>/e", "<Gamepad>/rightShoulder");
                AddButton(map, "Restart", "<Keyboard>/r", "<Gamepad>/select");
                AddButton(map, "Peek", "<Keyboard>/leftShift", "<Keyboard>/rightShift", "<Gamepad>/leftTrigger");
                AddButton(map, "Stasis", "<Keyboard>/c", "<Gamepad>/rightTrigger");
                AddButton(map, "Shoot", "<Keyboard>/k", "<Mouse>/rightButton", "<Gamepad>/buttonEast");
                AddButton(map, "Pause", "<Keyboard>/escape", "<Gamepad>/start");
                AddButton(map, "Submit", "<Keyboard>/enter", "<Keyboard>/space", "<Gamepad>/start", "<Gamepad>/buttonSouth");
                File.WriteAllText(path, asset.ToJson());
                UnityEngine.Object.DestroyImmediate(asset);
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            }
            controls = AssetDatabase.LoadAssetAtPath<InputActionAsset>(path);
        }

        private static void AddButton(InputActionMap map, string name, params string[] bindings)
        {
            var action = map.AddAction(name, InputActionType.Button);
            foreach (var b in bindings) action.AddBinding(b);
        }

        private static GameObject SavePrefab(GameObject go, string folder)
        {
            string path = $"{Root}/Prefabs/{folder}/{go.name}.prefab";
            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            UnityEngine.Object.DestroyImmediate(go);
            return prefab;
        }

        private static void CreatePrefabs()
        {
            playerPrefab = SavePrefab(BuildPlayer(), "Player");
            hudPrefab = SavePrefab(BuildHud(), "UI");
            cameraPrefab = SavePrefab(BuildCamera(), "Environment");
            systemsPrefab = SavePrefab(BuildSystems(), "Environment");
            boxPrefab = SavePrefab(BuildBox(), "Temporal");
            exitPrefab = SavePrefab(BuildExit(), "Environment");
            signPrefab = SavePrefab(BuildSign(), "Environment");
            deathZonePrefab = SavePrefab(BuildDeathZone(), "Environment");
            slimePrefab = SavePrefab(BuildSlime(), "Enemies");
            chronoferaPrefab = SavePrefab(BuildChronofera(), "Enemies");
            mothPrefab = SavePrefab(BuildMoth(), "Enemies");
            projectilePrefab = BuildProjectile();
            waspPrefab = SavePrefab(BuildWasp(projectilePrefab), "Enemies");
            shockwavePrefab = BuildShockwave();
            thornPrefab = SavePrefab(BuildThornPlant(projectilePrefab), "Enemies");
        }

        private static LayerMask Mask(params int[] layers)
        {
            int m = 0;
            foreach (int l in layers) m |= 1 << l;
            return m;
        }

        private static GameObject BuildPlayer()
        {
            var root = new GameObject("Player") { layer = playerLayer };
            var body = root.AddComponent<Rigidbody2D>();
            body.gravityScale = 3.5f;
            body.freezeRotation = true;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            body.sharedMaterial = noFriction;
            var capsule = root.AddComponent<CapsuleCollider2D>();
            capsule.size = new Vector2(0.8f, 1.4f);
            capsule.sharedMaterial = noFriction;

            var visual = new GameObject("Visual");
            visual.transform.SetParent(root.transform, false);
            SpriteRenderer sr;
            SpriteAnimator spriteAnimator = null;
            if (playerArt != null)
            {
                visual.transform.localPosition = new Vector3(0f, -0.7f, 0f);
                sr = AddSprite(visual, playerArt.First, Color.white, 20);
                spriteAnimator = AttachAnimator(visual, sr, playerArt, "Idle");
            }
            else
            {
                visual.transform.localScale = new Vector3(0.8f, 1.4f, 1f);
                sr = AddSprite(visual, squareSprite, PlayerColor, 20);
                var core = new GameObject("Core");
            core.transform.SetParent(visual.transform, false);
            core.transform.localPosition = new Vector3(0f, -0.05f, 0f);
            core.transform.localScale = new Vector3(0.45f, 0.45f * 0.8f / 1.4f, 1f);
            AddSprite(core, circleSprite, new Color(1f, 0.9f, 0.4f), 21);

            var eye = new GameObject("Eye");
            eye.transform.SetParent(visual.transform, false);
            eye.transform.localPosition = new Vector3(0.22f, 0.28f, 0f);
            eye.transform.localScale = new Vector3(0.22f, 0.1f, 1f);
                AddSprite(eye, squareSprite, Color.white, 22);
            }

            var slash = new GameObject("Slash");
            slash.transform.SetParent(root.transform, false);
            slash.transform.localScale = new Vector3(1.2f, 1f, 1f);
            var slashSr = AddSprite(slash, circleSprite, new Color(1f, 1f, 1f, 0.6f), 25);
            slashSr.enabled = false;

            var dust = MakeParticles("Poeira", root.transform, new Color(0.85f, 0.8f, 0.7f, 0.8f), 8, 2.2f, 0.35f, 0.18f, true, 0.3f, 0.2f);
            dust.transform.localPosition = new Vector3(0f, -0.65f, 0f);
            var dustShape = dust.shape;
            dustShape.shapeType = ParticleSystemShapeType.Box;
            dustShape.scale = new Vector3(0.6f, 0.05f, 0.1f);
            var timeFx = MakeParticles("TimeParticles", root.transform, new Color(0.75f, 0.6f, 1f), 30, 6f, 0.6f, 0.3f, true, 0.6f);
            var hurtFx = MakeParticles("HurtParticles", root.transform, new Color(1f, 0.3f, 0.3f), 20, 5f, 0.5f, 0.25f, true, 0.3f, 1f);

            var input = root.AddComponent<PlayerInputReader>();
            Set(input, "controls", controls);

            var controller = root.AddComponent<PlayerController>();
            Set(controller, "bodyCollider", capsule);
            Set(controller, "visual", visual.transform);
            Set(controller, "groundMask", Mask(0));
            Set(controller, "dust", dust);

            var health = root.AddComponent<PlayerHealth>();
            Set(health, "visual", sr);
            Set(health, "hurtParticles", hurtFx);

            var combat = root.AddComponent<PlayerCombat>();
            Set(combat, "slashVisual", slashSr);
            Set(combat, "hitMask", Mask(0, enemyLayer));

            var shooter = root.AddComponent<PlayerShooter>();
            Set(shooter, "boltPrefab", BuildBolt());
            var timeControl = root.AddComponent<PlayerTimeControl>();
            Set(timeControl, "timeParticles", timeFx);

            root.AddComponent<PlayerInventory>();
            var interaction = root.AddComponent<PlayerInteraction>();
            Set(interaction, "interactableMask", Mask(interactableLayer));
            if (spriteAnimator != null)
            {
                var anim = root.AddComponent<PlayerSpriteAnimation>();
                Set(anim, "animator", spriteAnimator);
            }
            return root;
        }

        private static GameObject BuildCamera()
        {
            var go = new GameObject("GameCamera") { tag = "MainCamera" };
            var cam = go.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 7f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.55f, 0.72f, 0.85f);
            cam.transform.position = new Vector3(0f, 0f, -10f);
            go.AddComponent<AudioListener>();
            go.AddComponent<UniversalAdditionalCameraData>();
            go.AddComponent<CameraFollow>();
            MakeWeather("Weather_Primavera", go.transform, new Color(1f, 0.7f, 0.85f, 0.9f), 10f, -1.2f, 0.12f, 0.6f, false);
            MakeWeather("Weather_Verao", go.transform, new Color(1f, 0.95f, 0.5f, 0.7f), 8f, 0.25f, 0.08f, 0.4f, true);
            MakeWeather("Weather_Outono", go.transform, new Color(0.95f, 0.5f, 0.2f, 0.95f), 14f, -1.6f, 0.18f, 1.2f, false);
            MakeWeather("Weather_Inverno", go.transform, new Color(1f, 1f, 1f, 0.95f), 45f, -2.2f, 0.12f, 0.5f, false);
            return go;
        }

        private static ParticleSystem MakeWeather(string name, Transform parent, Color color, float rate, float fallSpeed, float size, float sway, bool fillView)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = new Vector3(0f, fillView ? 0f : 8.5f, 10f);
            var ps = go.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            var main = ps.main;
            main.playOnAwake = false;
            main.loop = true;
            main.startLifetime = fillView ? 5f : 9f;
            main.startSpeed = 0f;
            main.startSize = new ParticleSystem.MinMaxCurve(size * 0.6f, size);
            main.startColor = color;
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 600;

            var emission = ps.emission;
            emission.rateOverTime = rate;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = fillView ? new Vector3(30f, 16f, 1f) : new Vector3(34f, 0.5f, 1f);

            var vel = ps.velocityOverLifetime;
            vel.enabled = true;
            vel.space = ParticleSystemSimulationSpace.World;
            vel.x = new ParticleSystem.MinMaxCurve(-sway, sway);
            vel.y = new ParticleSystem.MinMaxCurve(fallSpeed * 1.3f, fallSpeed * 0.7f);
            vel.z = new ParticleSystem.MinMaxCurve(0f, 0f);

            var noise = ps.noise;
            noise.enabled = true;
            noise.strength = sway * 0.6f;
            noise.frequency = 0.4f;

            var col = ps.colorOverLifetime;
            col.enabled = true;
            var grad = new Gradient();
            grad.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.15f), new GradientAlphaKey(1f, 0.8f), new GradientAlphaKey(0f, 1f) });
            col.color = grad;

            var r = go.GetComponent<ParticleSystemRenderer>();
            r.sharedMaterial = particleMaterial;
            r.sortingOrder = 35;
            return ps;
        }

        private static GameObject BuildSystems()
        {
            var go = new GameObject("LevelSystems");
            go.AddComponent<LevelContext>();
            var time = go.GetComponent<TimeManager>();
            Set(time, "solidMask", Mask(0));

            go.AddComponent<TimeStasis>();
            var fx = go.AddComponent<FeedbackFX>();
            var burst = MakeParticles("FeedbackParticles", go.transform, Color.white, 0, 5f, 0.8f, 0.3f, true, 0.4f);
            Set(fx, "burstParticles", burst);
            ConfigureFeedback(fx);
            fx.SetFont(UIFont);

            var lightGo = new GameObject("GlobalLight2D");
            lightGo.transform.SetParent(go.transform, false);
            var light = lightGo.AddComponent<Light2D>();
            light.lightType = Light2D.LightType.Global;
            light.intensity = 1f;

            var atmosphere = go.AddComponent<TimeAtmosphere>();
            Set(atmosphere, "globalLight", light);
            return go;
        }

        private static GameObject BuildBox()
        {
            var go = new GameObject("PushBox");
            var body = go.AddComponent<Rigidbody2D>();
            body.mass = 2f;
            body.gravityScale = 3f;
            body.freezeRotation = true;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            var col = go.AddComponent<BoxCollider2D>();
            col.size = new Vector2(0.98f, 0.98f);
            col.sharedMaterial = boxMaterial;

            var visual = new GameObject("Visual");
            visual.transform.SetParent(go.transform, false);
            if (tiles != null && tiles.crate != null)
            {
                AddSprite(visual, tiles.crate, Color.white, 8);
            }
            else
            {
                var sr = AddSprite(visual, blockSprite, new Color(0.85f, 0.62f, 0.3f), 8);
                sr.drawMode = SpriteDrawMode.Tiled;
                sr.size = Vector2.one;
                var cross = new GameObject("Mark");
            cross.transform.SetParent(visual.transform, false);
            cross.transform.localScale = new Vector3(0.7f, 0.12f, 1f);
            cross.transform.localRotation = Quaternion.Euler(0f, 0f, 45f);
            AddSprite(cross, squareSprite, new Color(0.55f, 0.36f, 0.15f), 9);
            var cross2 = UnityEngine.Object.Instantiate(cross, visual.transform);
            cross2.name = "Mark2";
                cross2.transform.localRotation = Quaternion.Euler(0f, 0f, -45f);
            }

            go.AddComponent<PushableBox>();
            return go;
        }

        private static GameObject BuildExit()
        {
            var go = new GameObject("LevelExit_FragmentoDoRelogio");
            var col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(1.4f, 2.2f);

            var glow = new GameObject("Glow");
            glow.transform.SetParent(go.transform, false);
            glow.transform.localScale = new Vector3(3.2f, 3.2f, 1f);
            var glowSr = AddSprite(glow, glowSprite != null ? glowSprite : circleSprite, new Color(0.6f, 0.75f, 1f, 0.55f), 6);
            glowSr.sharedMaterial = unlitSprite;
            glow.AddComponent<PulseGlow>();
            if (fxLibrary != null && fxLibrary.timeSwirl != null)
            {
                var swirl = new GameObject("Redemoinho");
                swirl.transform.SetParent(go.transform, false);
                swirl.transform.localScale = new Vector3(2.3f, 2.3f, 1f);
                var swSr = AddSprite(swirl, fxLibrary.timeSwirl, new Color(1f, 1f, 1f, 0.55f), 6);
                swSr.sharedMaterial = unlitSprite;
            }

            var visual = new GameObject("Visual");
            visual.transform.SetParent(go.transform, false);
            if (fxLibrary != null && fxLibrary.timeClock != null)
            {
                var clockSr = AddSprite(visual, fxLibrary.timeClock, Color.white, 7);
                clockSr.sharedMaterial = unlitSprite;
                visual.transform.localScale = Vector3.one * 1.9f;
            }
            else
            {
                AddSprite(visual, diamondSprite, new Color(1f, 0.85f, 0.35f), 7);
                var inner = new GameObject("Inner");
                inner.transform.SetParent(visual.transform, false);
                inner.transform.localScale = new Vector3(0.5f, 0.5f, 1f);
                AddSprite(inner, diamondSprite, Color.white, 8);
            }

            var sparkle = MakeParticles("Sparkles", go.transform, new Color(0.7f, 0.8f, 1f), 0, 0.6f, 1.5f, 0.15f, true, 0.8f, -0.1f);
            var em = sparkle.emission;
            em.rateOverTime = 8f;
            var main = sparkle.main;
            main.loop = true;
            main.playOnAwake = true;

            var exit = go.AddComponent<LevelExit>();
            Set(exit, "spinVisual", visual.transform);
            Set(exit, "spinSpeed", -35f);
            return go;
        }

        private static GameObject BuildSign()
        {
            var go = new GameObject("StorySign");
            var col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(3f, 3f);
            col.offset = new Vector2(0f, 1f);

            var post = new GameObject("Post");
            post.transform.SetParent(go.transform, false);
            post.transform.localPosition = new Vector3(0f, 0.5f, 0f);
            post.transform.localScale = new Vector3(0.15f, 1f, 1f);
            AddSprite(post, squareSprite, new Color(0.45f, 0.32f, 0.2f), -2);

            var board = new GameObject("Board");
            board.transform.SetParent(go.transform, false);
            board.transform.localPosition = new Vector3(0f, 1.1f, 0f);
            if (tiles != null && tiles.plaque != null)
            {
                AddSprite(board, tiles.plaque, Color.white, -1);
                board.transform.localScale = new Vector3(0.9f, 0.9f, 1f);
            }
            else
            {
                board.transform.localScale = new Vector3(1f, 0.6f, 1f);
                AddSprite(board, squareSprite, new Color(0.8f, 0.65f, 0.42f), -1);
                var text = new GameObject("Mark");
                text.transform.SetParent(board.transform, false);
                text.transform.localScale = new Vector3(0.6f, 0.15f, 1f);
                AddSprite(text, squareSprite, new Color(0.45f, 0.32f, 0.2f), 0);
            }

            go.AddComponent<StorySign>();
            return go;
        }

        private static GameObject BuildDeathZone()
        {
            var go = new GameObject("DeathZone");
            var col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(10f, 2f);
            go.AddComponent<DeathZone>();
            return go;
        }

        private static EnemyProjectile BuildProjectile()
        {
            if (sporeFrames.Length > 0) return BuildProjectile("Projectile_Espinho", sporeFrames[0], Color.white, 1.1f, 0.2f, false, 90f, sporeFrames, new Color(0.85f, 1f, 0.45f));
            return BuildProjectile("Projectile_Espinho", tiles != null ? tiles.uiSpore : diamondSprite, tiles != null ? new Color(0.75f, 1f, 0.5f) : new Color(0.7f, 1f, 0.35f), 0.45f, 0.18f, false, 540f);
        }

        private static EnemyProjectile BuildShockwave()
        {
            if (timeWaveFrames.Length > 0) return BuildProjectile("Projectile_Pedra", timeWaveFrames[0], Color.white, 1.6f, 0.35f, true, 0f, timeWaveFrames, new Color(0.85f, 0.5f, 1f));
            return BuildProjectile("Projectile_Pedra", tiles != null ? tiles.rockSmall : circleSprite, Color.white, 0.45f, 0.35f, true, -720f);
        }

        private static EnemyProjectile BuildProjectile(string name, Sprite sprite, Color color, float size, float radius, bool passThrough, float spin, Sprite[] frames = null, Color? trail = null)
        {
            var go = new GameObject(name);
            var body = go.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            body.useFullKinematicContacts = true;
            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = radius;
            var visual = new GameObject("Visual");
            visual.transform.SetParent(go.transform, false);
            float scale = sprite != null ? size / Mathf.Max(sprite.bounds.size.x, sprite.bounds.size.y) : size;
            visual.transform.localScale = new Vector3(scale, scale, 1f);
            var sr = AddSprite(visual, sprite, color, 14);
            if (sprite != null) visual.transform.localPosition = -sprite.bounds.center * scale;
            if (frames != null && frames.Length > 1)
            {
                sr.sharedMaterial = unlitSprite;
                var anim = visual.AddComponent<SpriteAnimator>();
                anim.Setup(sr, "Voo");
                anim.AddClip("Voo", frames, 40f, true);
                anim.SetAffectedByStasis(true);
            }
            var glow = MakeParticles("Rastro", go.transform, trail ?? color, 0, 0.3f, 0.35f, 0.12f, true, 0.05f);
            var em = glow.emission;
            em.rateOverTime = passThrough ? 0f : 18f;
            var main = glow.main;
            main.loop = true;
            main.playOnAwake = true;
            var projectile = go.AddComponent<EnemyProjectile>();
            Set(projectile, "solidMask", Mask(0));
            Set(projectile, "visual", visual.transform);
            Set(projectile, "spinSpeed", spin);
            Set(projectile, "destroyOnSolid", !passThrough);
            if (passThrough) Set(projectile, "lifetime", 2.2f);
            string path = $"{Root}/Prefabs/Enemies/{go.name}.prefab";
            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            UnityEngine.Object.DestroyImmediate(go);
            return prefab.GetComponent<EnemyProjectile>();
        }

        private static GameObject BuildEnemyBase(string name, Color color, Sprite sprite, Vector2 bodySize, Action<Transform> buildRemains, ArtSet art, string startClip, EnemyProjectile projectile = null)
        {
            var root = new GameObject(name) { layer = enemyLayer };
            var body = root.AddComponent<Rigidbody2D>();
            body.gravityScale = 3f;
            body.freezeRotation = true;
            body.sharedMaterial = noFriction;
            var col = root.AddComponent<BoxCollider2D>();
            col.size = bodySize;
            col.offset = new Vector2(0f, bodySize.y * 0.5f);
            col.sharedMaterial = noFriction;

            var visual = new GameObject("Visual") { layer = enemyLayer };
            visual.transform.SetParent(root.transform, false);
            SpriteRenderer sr;
            SpriteAnimator animator = null;
            if (art != null)
            {
                sr = AddSprite(visual, art.First, Color.white, 12);
                animator = AttachAnimator(visual, sr, art, startClip);
                animator.SetAffectedByStasis(true);
            }
            else
            {
                visual.transform.localPosition = new Vector3(0f, bodySize.y * 0.5f, 0f);
                visual.transform.localScale = new Vector3(bodySize.x, bodySize.y, 1f);
                sr = AddSprite(visual, sprite, color, 12);
                for (int i = 0; i < 2; i++)
                {
                    var eye = new GameObject($"Eye{i}");
                    eye.transform.SetParent(visual.transform, false);
                    eye.transform.localPosition = new Vector3(i == 0 ? 0.12f : 0.32f, 0.12f, 0f);
                    eye.transform.localScale = new Vector3(0.14f, 0.2f, 1f);
                    AddSprite(eye, squareSprite, Color.white, 13);
                }
            }

            var hurt = new GameObject("Hurtbox") { layer = 0 };
            hurt.transform.SetParent(root.transform, false);
            var hurtCol = hurt.AddComponent<BoxCollider2D>();
            hurtCol.isTrigger = true;
            hurtCol.size = bodySize * 0.92f;
            hurtCol.offset = new Vector2(0f, bodySize.y * 0.5f);

            var platform = new GameObject("PlatformCollider") { layer = 0 };
            platform.transform.SetParent(root.transform, false);
            var platCol = platform.AddComponent<BoxCollider2D>();
            platCol.size = bodySize;
            platCol.offset = new Vector2(0f, bodySize.y * 0.5f);
            platCol.enabled = false;

            var remains = new GameObject("Remains");
            remains.transform.SetParent(root.transform, false);
            if (art != null && art.remains != null)
            {
                var r = new GameObject("Restos");
                r.transform.SetParent(remains.transform, false);
                var rs = AddSprite(r, art.remains, art.remainsFlipY ? new Color(0.55f, 0.5f, 0.5f) : new Color(0.85f, 0.85f, 0.85f), 11);
                rs.flipX = art.facesLeft;
                rs.flipY = art.remainsFlipY;
                if (art.remainsFlipY) r.transform.localPosition = new Vector3(0f, ContentHeight(art.remains), 0f);
            }
            else buildRemains(remains.transform);
            if (tiles != null)
            {
                var growth = new GameObject("Vida Nova");
                growth.transform.SetParent(remains.transform, false);
                Decor(growth.transform, tiles.blueFlower, new Vector2(-0.35f, 0f), 0.7f, 12);
                Decor(growth.transform, tiles.sprout, new Vector2(0.1f, 0f), 0.8f, 12);
                Decor(growth.transform, tiles.lavender, new Vector2(0.45f, 0f), 0.7f, 12);
                if (fxLibrary != null && fxLibrary.butterflies.Length > 4)
                {
                    var fly = new GameObject("Borboleta");
                    fly.transform.SetParent(growth.transform, false);
                    fly.transform.localPosition = new Vector3(0f, 0.9f, 0f);
                    var wings = new GameObject("Asas");
                    wings.transform.SetParent(fly.transform, false);
                    var wsr = AddSprite(wings, fxLibrary.butterflies[4], Color.white, 13);
                    fly.AddComponent<Butterfly>().Setup(wsr, 0.45f, new Vector2(0.6f, 0.25f), 0.9f);
                }
                growth.SetActive(false);
            }
            remains.SetActive(false);

            var enemy = root.AddComponent<TemporalEnemy>();
            enemy.Setup(name, sr, col, hurtCol, platCol, remains, projectile);
            Set(enemy, "groundMask", Mask(0));
            if (animator != null)
            {
                Set(enemy, "animator", animator);
                Set(enemy, "spriteFacesLeft", art.facesLeft);
            }
            return root;
        }

        private static float ContentHeight(Sprite sprite)
        {
            string path = AssetDatabase.GetAssetPath(sprite.texture);
            if (string.IsNullOrEmpty(path) || !System.IO.File.Exists(path)) return sprite.bounds.max.y;
            var tex = new Texture2D(2, 2);
            tex.LoadImage(System.IO.File.ReadAllBytes(path));
            var rect = sprite.rect;
            int top = -1;
            for (int y = (int)rect.yMax - 1; y >= (int)rect.y && top < 0; y--)
            {
                for (int x = (int)rect.x; x < (int)rect.xMax; x++)
                {
                    if (tex.GetPixel(x, y).a > 0.1f) { top = y; break; }
                }
            }
            UnityEngine.Object.DestroyImmediate(tex);
            if (top < 0) return 0f;
            return (top + 1 - rect.y - sprite.pivot.y) / sprite.pixelsPerUnit;
        }

        private static void RemainsPart(Transform parent, Vector2 localCenter, Vector2 size, Sprite sprite, Color color, float angle = 0f)
        {
            var go = new GameObject("Parte");
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localCenter;
            go.transform.localRotation = Quaternion.Euler(0f, 0f, angle);
            go.transform.localScale = new Vector3(size.x, size.y, 1f);
            AddSprite(go, sprite, color, 11);
        }

        private static Color Tint(Color placeholder, Color art) => slimeArt != null ? art : placeholder;

        private static GameObject BuildSlime()
        {
            var root = BuildEnemyBase("Enemy_Lodo", new Color(0.95f, 0.35f, 0.35f), circleSprite, new Vector2(1f, 0.8f), r =>
            {
                RemainsPart(r, new Vector2(0f, 0.08f), new Vector2(0.9f, 0.16f), circleSprite, new Color(0.6f, 0.4f, 0.4f, 0.8f));
            }, slimeArt, "Walk");
            var enemy = root.GetComponent<TemporalEnemy>();
            enemy.AddStage(new EnemyStage { name = "Filhote", size = 0.6f, speed = 1.5f, health = 1, color = Tint(new Color(1f, 0.5f, 0.5f), new Color(0.8f, 1f, 0.8f)), movement = EnemyMovement.Patrulha, animation = "Walk" });
            enemy.AddStage(new EnemyStage { name = "Adulto (pula)", conditions = { TemporalCondition.Since(20) }, size = 1f, speed = 3f, health = 2, color = Tint(new Color(0.9f, 0.3f, 0.3f), Color.white), movement = EnemyMovement.Pula, animation = "Walk", message = "O lodo cresceu e aprendeu a pular." });
            enemy.AddStage(new EnemyStage { name = "Ressecado (verão)", conditions = { TemporalCondition.In(Season.Verao) }, size = 0.7f, speed = 0.8f, health = 1, color = Tint(new Color(0.75f, 0.55f, 0.4f), new Color(0.85f, 0.7f, 0.45f)), movement = EnemyMovement.Patrulha, animation = "Walk", message = "O calor ressecou o lodo: ficou pequeno e lento." });
            enemy.AddStage(new EnemyStage { name = "Congelado (inverno)", conditions = { TemporalCondition.In(Season.Inverno) }, size = 1f, color = Tint(new Color(0.7f, 0.9f, 1f), new Color(0.6f, 0.85f, 1f)), movement = EnemyMovement.Parado, animation = "Idle", solidPlatform = true, harmless = true, message = "O lodo congelou no inverno: agora é uma plataforma." });
            EditorUtility.SetDirty(enemy);
            return root;
        }

        private static GameObject BuildChronofera()
        {
            var bone = new Color(0.9f, 0.88f, 0.8f);
            var root = BuildEnemyBase("Enemy_Cronofera", new Color(1f, 0.5f, 0.2f), squareSprite, new Vector2(1.1f, 0.8f), r =>
            {
                RemainsPart(r, new Vector2(0f, 0.1f), new Vector2(0.9f, 0.1f), squareSprite, bone);
                RemainsPart(r, new Vector2(-0.2f, 0.2f), new Vector2(0.5f, 0.08f), squareSprite, bone, 35f);
                RemainsPart(r, new Vector2(0.25f, 0.2f), new Vector2(0.5f, 0.08f), squareSprite, bone, -35f);
                RemainsPart(r, new Vector2(0.42f, 0.22f), new Vector2(0.22f, 0.2f), circleSprite, bone);
            }, boarArt, "Run");
            bool art = boarArt != null;
            var enemy = root.GetComponent<TemporalEnemy>();
            enemy.AddStage(new EnemyStage { name = "Jovem (rápida)", size = 0.6f, speed = 8f, health = 3, color = art ? new Color(1f, 0.85f, 0.7f) : new Color(1f, 0.55f, 0.2f), movement = EnemyMovement.Persegue, animation = "Run", stompable = false });
            enemy.AddStage(new EnemyStage { name = "Adulta", conditions = { TemporalCondition.Since(20) }, size = 1.1f, speed = 4f, health = 3, color = art ? Color.white : new Color(0.9f, 0.3f, 0.25f), movement = EnemyMovement.Persegue, animation = "Run", stompable = false, message = "A cronofera cresceu. Ainda é rápida." });
            enemy.AddStage(new EnemyStage { name = "Anciã (lenta e pesada)", conditions = { TemporalCondition.Since(40) }, size = 2.2f, speed = 1.2f, health = 8, color = art ? new Color(0.75f, 0.65f, 0.7f) : new Color(0.55f, 0.15f, 0.3f), movement = EnemyMovement.Patrulha, animation = "Walk", heavy = true, stompable = false, message = "A cronofera envelheceu: enorme, lenta... e pesada." });
            enemy.AddStage(new EnemyStage { name = "Morta de velhice", conditions = { TemporalCondition.Since(60) }, size = 2.2f, present = false, showRemains = true, color = Color.gray, message = "A cronofera morreu de velhice. Restam só os ossos." });
            EditorUtility.SetDirty(enemy);
            return root;
        }

        private static GameObject BuildMoth()
        {
            var root = BuildEnemyBase("Enemy_Mariposa", new Color(0.5f, 0.85f, 0.35f), circleSprite, new Vector2(0.9f, 0.6f), r =>
            {
                RemainsPart(r, new Vector2(-0.25f, 0.06f), new Vector2(0.5f, 0.12f), circleSprite, new Color(0.7f, 0.55f, 0.85f, 0.8f), 15f);
                RemainsPart(r, new Vector2(0.25f, 0.06f), new Vector2(0.5f, 0.12f), circleSprite, new Color(0.7f, 0.55f, 0.85f, 0.8f), -15f);
            }, mothArt, "Walk");
            bool art = mothArt != null;
            var enemy = root.GetComponent<TemporalEnemy>();
            enemy.AddStage(new EnemyStage { name = "Lagarta", size = 0.8f, speed = 1f, health = 1, color = art ? Color.white : new Color(0.5f, 0.85f, 0.35f), movement = EnemyMovement.Patrulha, animation = "Walk", stompable = true });
            enemy.AddStage(new EnemyStage { name = "Casulo", conditions = { TemporalCondition.Since(20) }, size = 0.9f, health = 2, color = art ? new Color(0.95f, 0.85f, 0.65f) : new Color(0.85f, 0.72f, 0.45f), movement = EnemyMovement.Parado, animation = "Hide", harmless = true, stompable = false, message = "A lagarta se fechou num casulo." });
            enemy.AddStage(new EnemyStage { name = "Mariposa (voa)", conditions = { TemporalCondition.Since(30) }, size = 0.9f, speed = 3.2f, health = 1, color = art ? new Color(0.85f, 0.75f, 1f) : new Color(0.8f, 0.55f, 1f), movement = EnemyMovement.Voa, animation = "Fly", stompable = true, message = "Do casulo saiu uma criatura alada — e ela está com fome!" });
            enemy.AddStage(new EnemyStage { name = "Fim do ciclo", conditions = { TemporalCondition.Since(60) }, size = 0.9f, present = false, showRemains = true, color = Color.gray, message = "A criatura viveu seu ciclo e se foi. Restou a casca." });
            EditorUtility.SetDirty(enemy);
            return root;
        }

        private static GameObject BuildWasp(EnemyProjectile projectile)
        {
            var root = BuildEnemyBase("Enemy_VespaDoTempo", new Color(1f, 0.75f, 0.3f), circleSprite, new Vector2(0.9f, 0.6f), r =>
            {
                RemainsPart(r, new Vector2(0f, 0.06f), new Vector2(0.6f, 0.14f), circleSprite, new Color(0.8f, 0.6f, 0.3f, 0.8f));
            }, mothArt, "Walk", projectile);
            bool art = mothArt != null;
            var enemy = root.GetComponent<TemporalEnemy>();
            enemy.AddStage(new EnemyStage { name = "Larva (morde de perto)", size = 0.9f, speed = 3.4f, health = 2, color = art ? new Color(1f, 0.85f, 0.55f) : new Color(1f, 0.75f, 0.3f), movement = EnemyMovement.Persegue, animation = "Walk", stompable = true });
            enemy.AddStage(new EnemyStage { name = "Vespa adulta (ataca de longe)", conditions = { TemporalCondition.Since(20) }, size = 1.1f, speed = 2.4f, health = 3, color = art ? new Color(1f, 0.9f, 0.6f) : new Color(1f, 0.6f, 0.2f), movement = EnemyMovement.Voa, animation = "Fly", attackAnimation = "FlyAttack", stompable = true, shootInterval = 1.9f, projectileSpeed = 7f, message = "A larva virou vespa: agora ela ataca de longe!" });
            enemy.AddStage(new EnemyStage { name = "Vespa velha", conditions = { TemporalCondition.Since(50) }, size = 1.1f, speed = 1.2f, health = 2, color = art ? new Color(0.75f, 0.7f, 0.7f) : Color.gray, movement = EnemyMovement.Voa, animation = "Fly", attackAnimation = "FlyAttack", stompable = true, shootInterval = 3.5f, projectileSpeed = 5f, message = "A vespa envelheceu: lenta e cansada." });
            enemy.AddStage(new EnemyStage { name = "Fim do ciclo", conditions = { TemporalCondition.Since(70) }, size = 1f, present = false, showRemains = true, color = Color.gray, message = "A vespa viveu seu ciclo." });
            EditorUtility.SetDirty(enemy);
            return root;
        }

        private static GameObject BuildThornPlant(EnemyProjectile projectile)
        {
            var root = BuildEnemyBase("Enemy_Espinheiro", new Color(0.35f, 0.65f, 0.3f), triangleSprite, new Vector2(0.8f, 1.1f), r =>
            {
                RemainsPart(r, new Vector2(0f, 0.2f), new Vector2(0.15f, 0.4f), squareSprite, new Color(0.5f, 0.4f, 0.25f));
                RemainsPart(r, new Vector2(0.12f, 0.3f), new Vector2(0.25f, 0.06f), squareSprite, new Color(0.5f, 0.4f, 0.25f), 30f);
            }, plantArt, "Idle", projectile);
            bool art = plantArt != null;
            var enemy = root.GetComponent<TemporalEnemy>();
            Set(enemy, "chaseRange", 7f);
            enemy.AddStage(new EnemyStage { name = "Broto", size = 0.5f, health = 1, color = art ? new Color(0.75f, 1f, 0.7f) : new Color(0.5f, 0.8f, 0.4f), movement = EnemyMovement.Parado, animation = "Idle", harmless = true, stompable = true });
            enemy.AddStage(new EnemyStage { name = "Madura (atira espinhos)", conditions = { TemporalCondition.Since(20) }, size = 1.1f, health = 3, color = art ? Color.white : new Color(0.3f, 0.6f, 0.28f), movement = EnemyMovement.Parado, animation = "Idle", attackAnimation = "Attack", stompable = false, shootInterval = 2.2f, projectileSpeed = 6f, message = "O espinheiro amadureceu e começou a cuspir espinhos." });
            enemy.AddStage(new EnemyStage { name = "Murcha (inverno)", conditions = { TemporalCondition.In(Season.Inverno) }, size = 1f, present = false, showRemains = true, color = Color.gray, message = "O inverno fez o espinheiro murchar." });
            EditorUtility.SetDirty(enemy);
            return root;
        }

        private static GameObject BuildHud()
        {
            var canvas = MakeCanvas("HUD", 10);
            var root = canvas.transform;
            var hud = canvas.gameObject.AddComponent<GameHUD>();
            var debug = canvas.gameObject.AddComponent<DebugOverlay>();

            var timePanel = UIRect("TimePanel", root, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -16f), new Vector2(560f, 160f));
            if (tiles != null) UIImage(timePanel, Color.white, tiles.uiWood); else UIImage(timePanel, new Color(0f, 0f, 0f, 0.35f));
            var season = UIText(UIRect("Season", timePanel, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -6f), new Vector2(0f, 30f)), "PRIMAVERA  ·  ANO 1", 22, TextAnchor.MiddleCenter, new Color(0.6f, 1f, 0.6f), FontStyle.Bold);
            var day = UIText(UIRect("Day", timePanel, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 8f), new Vector2(300f, 90f)), "DIA 1", 64, TextAnchor.MiddleCenter, Color.white, FontStyle.Bold);
            var back = UIText(UIRect("Back", timePanel, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(20f, 8f), new Vector2(120f, 70f)), tiles != null ? "< Q" : "◄ Q", 40, TextAnchor.MiddleLeft, Color.white, FontStyle.Bold);
            var forward = UIText(UIRect("Forward", timePanel, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-20f, 8f), new Vector2(120f, 70f)), tiles != null ? "E >" : "E ►", 40, TextAnchor.MiddleRight, Color.white, FontStyle.Bold);
            var dots = UIRect("Periods", timePanel, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 12f), new Vector2(480f, 24f));
            var dotsLayout = dots.gameObject.AddComponent<HorizontalLayoutGroup>();
            dotsLayout.spacing = 12f;
            dotsLayout.childAlignment = TextAnchor.MiddleCenter;
            dotsLayout.childControlWidth = false;
            dotsLayout.childControlHeight = false;
            dotsLayout.childForceExpandWidth = false;
            dotsLayout.childForceExpandHeight = false;

            var level = UIText(UIRect("LevelLabel", root, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(30f, -24f), new Vector2(760f, 40f)), "Fase", 28, TextAnchor.UpperLeft, Color.white, FontStyle.Bold);
            var hearts = UIRect("Hearts", root, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(30f, -76f), new Vector2(300f, 30f));
            var heartsLayout = hearts.gameObject.AddComponent<HorizontalLayoutGroup>();
            heartsLayout.spacing = 8f;
            heartsLayout.childControlWidth = false;
            heartsLayout.childControlHeight = false;
            heartsLayout.childForceExpandWidth = false;
            heartsLayout.childForceExpandHeight = false;

            UIText(UIRect("Help", root, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-26f, -20f), new Vector2(520f, 150f)),
                "A/D  andar     ESPAÇO  pular\nF  interagir     J  atacar\nQ / E  voltar / avançar no tempo\nSHIFT + Q/E  espiar     C  pausar o tempo\nR  reiniciar     F1  debug", 19, TextAnchor.UpperRight, new Color(1f, 1f, 1f, 0.75f));

            var prompt = UIRect("Prompt", root, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 180f), new Vector2(560f, 58f));
            if (tiles != null) UIImage(prompt, Color.white, tiles.uiButton); else UIImage(prompt, new Color(0f, 0f, 0f, 0.6f));
            var promptGroup = Group(prompt, 0f);
            var promptText = UIText(Stretch("Text", prompt), "[F] Interagir", 28, TextAnchor.MiddleCenter, InteractColor, FontStyle.Bold);

            var message = UIRect("Message", root, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -268f), new Vector2(1300f, 64f));
            if (tiles != null) UIImage(message, new Color(1f, 1f, 1f, 0.92f), tiles.uiWood); else UIImage(message, new Color(0f, 0f, 0f, 0.45f));
            var messageGroup = Group(message, 0f);
            var messageText = UIText(Stretch("Text", message), "", 28, TextAnchor.MiddleCenter, Color.white);

            var sign = UIRect("Sign", root, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 40f), new Vector2(1150f, 120f));
            if (tiles != null) UIImage(sign, Color.white, tiles.uiParchment); else UIImage(sign, new Color(0.12f, 0.09f, 0.06f, 0.8f));
            var signGroup = Group(sign, 0f);
            var signTextRt = Stretch("Text", sign);
            signTextRt.offsetMin = new Vector2(24f, 10f);
            signTextRt.offsetMax = new Vector2(-24f, -10f);
            var signText = UIText(signTextRt, "", 26, TextAnchor.MiddleCenter, tiles != null ? new Color(0.28f, 0.17f, 0.08f) : new Color(1f, 0.95f, 0.82f), FontStyle.Italic, tiles == null);

            var debugPanel = UIRect("DebugPanel", root, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-20f, -70f), new Vector2(600f, 720f));
            UIImage(debugPanel, new Color(0f, 0f, 0f, 0.78f));
            var debugTextRt = Stretch("Text", debugPanel);
            debugTextRt.offsetMin = new Vector2(16f, 12f);
            debugTextRt.offsetMax = new Vector2(-16f, -12f);
            var debugText = UIText(debugTextRt, "", 18, TextAnchor.UpperLeft, Color.white, FontStyle.Normal, false);
            debugPanel.gameObject.SetActive(false);

            var stasis = UIRect("Stasis", root, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(30f, -116f), new Vector2(220f, 26f));
            var stasisGroup = Group(stasis, 1f);
            UIImage(stasis, new Color(0f, 0f, 0f, 0.5f), tiles != null ? tiles.uiWood : null);
            var stasisFillRt = Stretch("Fill", stasis);
            stasisFillRt.offsetMin = new Vector2(5f, 5f);
            stasisFillRt.offsetMax = new Vector2(-5f, -5f);
            var stasisFill = UIImage(stasisFillRt, new Color(0.45f, 0.75f, 1f), squareSprite);
            stasisFill.type = Image.Type.Filled;
            stasisFill.fillMethod = Image.FillMethod.Horizontal;
            UIText(Stretch("Label", stasis), "C  PAUSA DO TEMPO", 16, TextAnchor.MiddleCenter, Color.white, FontStyle.Bold);
            Set(hud, "stasisFill", stasisFill);
            Set(hud, "stasisGroup", stasisGroup);

            var bolt = UIRect("Bolt", root, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(30f, -148f), new Vector2(220f, 26f));
            var boltGroup = Group(bolt, 0f);
            UIImage(bolt, new Color(0f, 0f, 0f, 0.5f), tiles != null ? tiles.uiWood : null);
            var boltFillRt = Stretch("Fill", bolt);
            boltFillRt.offsetMin = new Vector2(5f, 5f);
            boltFillRt.offsetMax = new Vector2(-5f, -5f);
            var boltFill = UIImage(boltFillRt, new Color(0.75f, 0.6f, 1f), squareSprite);
            boltFill.type = Image.Type.Filled;
            boltFill.fillMethod = Image.FillMethod.Horizontal;
            UIText(Stretch("Label", bolt), "K  DISPARO DO TEMPO", 16, TextAnchor.MiddleCenter, Color.white, FontStyle.Bold);
            Set(hud, "boltFill", boltFill);
            Set(hud, "boltGroup", boltGroup);

            var pollenRt = UIRect("Pollen", root, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(30f, -182f), new Vector2(260f, 32f));
            var pollenText = UIText(pollenRt, "BORBOLETAS  0/5", 22, TextAnchor.MiddleLeft, new Color(0.6f, 0.85f, 1f), FontStyle.Bold);
            Set(hud, "pollenText", pollenText);

            var boss = UIRect("BossBar", root, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -190f), new Vector2(760f, 64f));
            var bossGroup = Group(boss, 0f);
            var bossName = UIText(UIRect("Name", boss, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), Vector2.zero, new Vector2(0f, 26f)), "", 24, TextAnchor.MiddleCenter, new Color(1f, 0.85f, 0.5f), FontStyle.Bold);
            var bossBack = UIRect("Back", boss, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 12f), new Vector2(0f, 24f));
            UIImage(bossBack, new Color(0f, 0f, 0f, 0.6f), tiles != null ? tiles.uiWood : null);
            var bossFillRt = Stretch("Fill", bossBack);
            bossFillRt.offsetMin = new Vector2(5f, 5f);
            bossFillRt.offsetMax = new Vector2(-5f, -5f);
            var bossFill = UIImage(bossFillRt, new Color(0.95f, 0.3f, 0.3f), squareSprite);
            bossFill.type = Image.Type.Filled;
            bossFill.fillMethod = Image.FillMethod.Horizontal;
            var bossState = UIText(UIRect("State", boss, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 1f), new Vector2(0f, 10f), new Vector2(0f, 24f)), "", 16, TextAnchor.MiddleCenter, Color.white);
            Set(hud, "bossGroup", bossGroup);
            Set(hud, "bossName", bossName);
            Set(hud, "bossFill", bossFill);
            Set(hud, "bossState", bossState);

            var peek = UIRect("Peek", root, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 150f), new Vector2(1000f, 56f));
            var peekGroup = Group(peek, 0f);
            UIImage(peek, new Color(0.2f, 0.4f, 0.6f, 0.75f));
            var peekText = UIText(Stretch("Text", peek), "", 24, TextAnchor.MiddleCenter, new Color(0.85f, 0.95f, 1f), FontStyle.Bold);
            Set(hud, "peekGroup", peekGroup);
            Set(hud, "peekText", peekText);

            var inventory = UIRect("Inventory", root, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(24f, 24f), new Vector2(200f, 240f));
            var invLayout = inventory.gameObject.AddComponent<VerticalLayoutGroup>();
            invLayout.spacing = 6f;
            invLayout.childAlignment = TextAnchor.LowerLeft;
            invLayout.childControlWidth = false;
            invLayout.childControlHeight = false;
            invLayout.childForceExpandWidth = false;
            invLayout.childForceExpandHeight = false;
            Set(hud, "inventoryContainer", inventory);
            if (tiles != null) Set(hud, "inventorySlot", tiles.uiWood);

            var flash = UIImage(Stretch("TimeFlash", root), Color.clear);

            var intro = Stretch("Intro", root);
            var introGroup = Group(intro, 0f);
            var band = UIRect("Band", intro, new Vector2(0f, 0.5f), new Vector2(1f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(0f, 280f));
            UIImage(band, new Color(0f, 0f, 0f, 0.6f));
            var introTitle = UIText(UIRect("Title", intro, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 60f), new Vector2(1400f, 80f)), "", 56, TextAnchor.MiddleCenter, new Color(1f, 0.87f, 0.45f), FontStyle.Bold);
            var introBody = UIText(UIRect("Body", intro, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -45f), new Vector2(1300f, 120f)), "", 28, TextAnchor.MiddleCenter, Color.white);

            var complete = Stretch("Complete", root);
            UIImage(complete, new Color(0f, 0f, 0f, 0.7f));
            var completeGroup = Group(complete, 0f);
            var completeTitle = UIText(UIRect("Title", complete, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 60f), new Vector2(1400f, 110f)), "FASE CONCLUÍDA", 84, TextAnchor.MiddleCenter, new Color(1f, 0.87f, 0.45f), FontStyle.Bold);
            var completeBody = UIText(UIRect("Body", complete, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -50f), new Vector2(1300f, 120f)), "", 30, TextAnchor.MiddleCenter, Color.white);

            var fade = Stretch("Fade", root);
            UIImage(fade, Color.black);
            var fadeGroup = Group(fade, 1f);

            var pause = Stretch("Pause", root);
            UIImage(pause, new Color(0f, 0f, 0f, 0.65f));
            var pauseGroup = Group(pause, 0f);
            var pauseBox = UIRect("Box", pause, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(520f, 440f));
            UIImage(pauseBox, tiles != null ? Color.white : new Color(0.1f, 0.08f, 0.06f, 0.9f), tiles != null ? tiles.uiParchment : null);
            UIText(UIRect("Title", pauseBox, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -30f), new Vector2(460f, 70f)), "PAUSA", 56, TextAnchor.MiddleCenter, new Color(0.35f, 0.2f, 0.1f), FontStyle.Bold, false);
            var resume = MenuButton(pauseBox, "Continuar", "Continuar", new Vector2(0.5f, 0.5f), new Vector2(0f, 40f), new Vector2(360f, 64f));
            var restart = MenuButton(pauseBox, "Reiniciar", "Reiniciar fase", new Vector2(0.5f, 0.5f), new Vector2(0f, -40f), new Vector2(360f, 64f));
            var menu = MenuButton(pauseBox, "Menu", "Menu principal", new Vector2(0.5f, 0.5f), new Vector2(0f, -120f), new Vector2(360f, 64f));
            canvas.gameObject.AddComponent<PauseMenu>().Setup(pauseGroup, resume, restart, menu, controls);
            EnsureEventSystem(root);

            Set(hud, "dayText", day);
            Set(hud, "seasonText", season);
            Set(hud, "backArrow", back);
            Set(hud, "forwardArrow", forward);
            Set(hud, "periodDots", dots);
            Set(hud, "flashOverlay", flash);
            Set(hud, "levelLabel", level);
            Set(hud, "promptText", promptText);
            Set(hud, "promptGroup", promptGroup);
            Set(hud, "messageText", messageText);
            Set(hud, "messageGroup", messageGroup);
            Set(hud, "signText", signText);
            Set(hud, "signGroup", signGroup);
            Set(hud, "heartsContainer", hearts);
            Set(hud, "introGroup", introGroup);
            Set(hud, "introTitle", introTitle);
            Set(hud, "introBody", introBody);
            Set(hud, "completeGroup", completeGroup);
            Set(hud, "completeTitle", completeTitle);
            Set(hud, "completeBody", completeBody);
            Set(hud, "fadeGroup", fadeGroup);
            Set(hud, "dotSprite", circleSprite);
            if (tiles != null)
            {
                Set(hud, "heartSprite", tiles.uiHeart);
                Set(hud, "heartEmptySprite", tiles.uiHeartEmpty);
            }

            Set(debug, "panel", debugPanel.gameObject);
            Set(debug, "output", debugText);
            return canvas.gameObject;
        }
    }
}

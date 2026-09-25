using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;

namespace ButterflyStep.EditorTools
{
    public static partial class GameBuilder
    {
        private const string FxRoot = Root + "/Art/CC0/FX";
        private const string UnlitSpritePath = "Packages/com.unity.render-pipelines.universal/Runtime/Materials/Sprite-Unlit-Default.mat";
        private const string LitSpritePath = "Packages/com.unity.render-pipelines.universal/Runtime/Materials/Sprite-Lit-Default.mat";

        private static FxLibrary fxLibrary;
        private static Material unlitSprite;
        private static Material litSprite;
        private static Material waterMaterial;
        private static Sprite glowSprite;
        private static Sprite snowFill, snowCap, iceTile, snowFlake;

        private static int PixelHash(int x, int y) => ((x * 73856093) ^ (y * 19349663) ^ 0x5bd1e995) & 0xffff;
        private static Sprite[] sporeFrames = new Sprite[0];
        private static Sprite[] timeWaveFrames = new Sprite[0];

        private static readonly Color RiverTop = new Color(0.25f, 0.58f, 0.92f, 0.8f);
        private static readonly Color RiverBottom = new Color(0.04f, 0.14f, 0.36f, 0.94f);
        private static readonly Color LakeTop = new Color(0.3f, 0.66f, 0.9f, 0.76f);
        private static readonly Color LakeBottom = new Color(0.05f, 0.2f, 0.38f, 0.92f);

        private static void LoadFx()
        {
            unlitSprite = AssetDatabase.LoadAssetAtPath<Material>(UnlitSpritePath);
            snowFill = MakeSprite("PixelSnow", 32, 32, (x, y, s) =>
            {
                int h = PixelHash(x / 2, y / 2) % 100;
                float k = h < 12 ? 0.86f : h < 18 ? 1f : 0.94f;
                float shade = 1f;
                return new Color(k * 0.95f * shade, k * 0.97f * shade, k * shade, 1f);
            });
            snowCap = MakeSprite("PixelSnowCap", 32, 32, (x, y, s) =>
            {
                int edge = 20 + Mathf.RoundToInt(Mathf.Sin(x * 0.55f) * 2f + Mathf.Sin(x * 1.3f + 1f));
                if (y > edge) return Color.clear;
                if (y >= edge - 1) return new Color(1f, 1f, 1f, 1f);
                int h = PixelHash(x / 2, y / 2) % 100;
                float k = h < 10 ? 0.88f : 0.97f;
                return new Color(k * 0.98f, k, 1f, 1f);
            });
            snowFlake = MakeSprite("PixelFlake", 8, 32, (x, y, s) =>
            {
                bool cross = (x == 3 || x == 4) && y > 0 && y < 7 || (y == 3 || y == 4) && x > 0 && x < 7;
                bool diag = (x == y || x == 7 - y) && x > 1 && x < 6;
                return cross || diag ? Color.white : Color.clear;
            });
            iceTile = MakeSprite("PixelIce", 32, 32, (x, y, s) =>
            {
                int d = (x + y) % 16;
                if (d == 0 || d == 1) return new Color(0.95f, 1f, 1f, 0.92f);
                int h = PixelHash(x / 2, y / 2) % 100;
                float k = h < 15 ? 0.82f : 0.9f;
                return new Color(0.72f * k / 0.9f, 0.9f * k / 0.9f, 1f, 0.82f);
            });
            glowSprite = MakeSprite("Glow", 64, 64, (x, y, s) =>
            {
                float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(s * 0.5f, s * 0.5f)) / (s * 0.5f);
                float a = Mathf.Clamp01(1f - d);
                return new Color(1f, 1f, 1f, a * a);
            });
            litSprite = AssetDatabase.LoadAssetAtPath<Material>(LitSpritePath);
            string waterPath = $"{Root}/Materials/Water.mat";
            waterMaterial = AssetDatabase.LoadAssetAtPath<Material>(waterPath);
            if (waterMaterial == null && litSprite != null)
            {
                waterMaterial = new Material(litSprite) { name = "Water" };
                AssetDatabase.CreateAsset(waterMaterial, waterPath);
            }
            if (waterMaterial != null)
            {
                waterMaterial.mainTexture = squareSprite.texture;
                EditorUtility.SetDirty(waterMaterial);
            }
            sporeFrames = SliceGrid($"{FxRoot}/Spore.png", 100, 100, 64f);
            timeWaveFrames = SliceGrid($"{FxRoot}/TimeWave.png", 100, 100, 64f);
            fxLibrary = new FxLibrary
            {
                slash = SliceGrid($"{FxRoot}/Slash.png", 64, 47, 32f),
                hitRing = SliceGrid($"{FxRoot}/HitRing.png", 100, 100, 64f),
                sparkle = SliceGrid($"{FxRoot}/Sparkle.png", 100, 100, 64f),
                bubbles = SliceGrid($"{FxRoot}/Bubbles.png", 100, 100, 64f),
                burst = SliceGrid($"{FxRoot}/Burst.png", 100, 100, 64f),
                soul = SliceGrid($"{FxRoot}/Soul.png", 100, 100, 64f),
                timeClock = SingleSprite($"{FxRoot}/TimeClock.png", 100f),
                timeSwirl = SingleSprite($"{FxRoot}/TimeSwirl.png", 100f),
                butterflies = SliceComponents($"{FxRoot}/Butterflies.png", 32f, 1)
            };
        }

        private static Sprite[] SliceComponents(string path, float ppu, int join)
        {
            if (!File.Exists(path)) return new Sprite[0];
            AssetDatabase.ImportAsset(path);
            var tex = new Texture2D(2, 2);
            tex.LoadImage(File.ReadAllBytes(path));
            int w = tex.width, h = tex.height;
            var solid = new bool[w, h];
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                    solid[x, y] = tex.GetPixel(x, y).a > 0.1f;
            Object.DestroyImmediate(tex);

            var seen = new bool[w, h];
            var boxes = new List<RectInt>();
            var stack = new Stack<Vector2Int>();
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    if (!solid[x, y] || seen[x, y]) continue;
                    int minX = x, maxX = x, minY = y, maxY = y;
                    stack.Push(new Vector2Int(x, y));
                    seen[x, y] = true;
                    while (stack.Count > 0)
                    {
                        var p = stack.Pop();
                        minX = Mathf.Min(minX, p.x); maxX = Mathf.Max(maxX, p.x);
                        minY = Mathf.Min(minY, p.y); maxY = Mathf.Max(maxY, p.y);
                        for (int dy = -join; dy <= join; dy++)
                        {
                            for (int dx = -join; dx <= join; dx++)
                            {
                                int nx = p.x + dx, ny = p.y + dy;
                                if (nx < 0 || ny < 0 || nx >= w || ny >= h || seen[nx, ny] || !solid[nx, ny]) continue;
                                seen[nx, ny] = true;
                                stack.Push(new Vector2Int(nx, ny));
                            }
                        }
                    }
                    if (maxX - minX >= 6 && maxY - minY >= 6) boxes.Add(new RectInt(minX, minY, maxX - minX + 1, maxY - minY + 1));
                }
            }
            boxes.Sort((a, b) =>
            {
                bool topA = a.center.y > h * 0.5f, topB = b.center.y > h * 0.5f;
                if (topA != topB) return topA ? -1 : 1;
                return a.x.CompareTo(b.x);
            });

            string prefix = Path.GetFileNameWithoutExtension(path) + "_";
            var rects = new List<SpriteRect>();
            foreach (var box in boxes)
            {
                rects.Add(new SpriteRect
                {
                    name = prefix + rects.Count,
                    rect = new Rect(box.x, box.y, box.width, box.height),
                    alignment = SpriteAlignment.Center,
                    pivot = new Vector2(0.5f, 0.5f),
                    spriteID = GUID.Generate()
                });
            }
            var importer = PrepareImporter(path, ppu);
            ApplyRects(importer, path, rects);
            return LoadSprites(path, prefix);
        }

        private static Sprite SingleSprite(string path, float ppu)
        {
            if (!File.Exists(path)) return null;
            AssetDatabase.ImportAsset(path);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = ppu;
            importer.filterMode = FilterMode.Bilinear;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static Sprite[] SliceGrid(string path, int frameW, int frameH, float ppu)
        {
            if (!File.Exists(path)) return new Sprite[0];
            AssetDatabase.ImportAsset(path);
            var tex = new Texture2D(2, 2);
            tex.LoadImage(File.ReadAllBytes(path));
            int cols = tex.width / frameW;
            int rows = tex.height / frameH;
            string prefix = Path.GetFileNameWithoutExtension(path) + "_";
            var rects = new List<SpriteRect>();
            for (int row = 0; row < rows; row++)
            {
                int y0 = tex.height - (row + 1) * frameH;
                for (int c = 0; c < cols; c++)
                {
                    int solid = 0;
                    for (int y = 0; y < frameH && solid < 6; y += 2)
                    {
                        for (int x = 0; x < frameW && solid < 6; x += 2)
                        {
                            if (tex.GetPixel(c * frameW + x, y0 + y).a > 0.1f) solid++;
                        }
                    }
                    if (solid < 6) continue;
                    rects.Add(new SpriteRect
                    {
                        name = prefix + rects.Count,
                        rect = new Rect(c * frameW, y0, frameW, frameH),
                        alignment = SpriteAlignment.Center,
                        pivot = new Vector2(0.5f, 0.5f),
                        spriteID = GUID.Generate()
                    });
                }
            }
            Object.DestroyImmediate(tex);
            var importer = PrepareImporter(path, ppu);
            ApplyRects(importer, path, rects);
            return LoadSprites(path, prefix);
        }

        private static void ConfigureFeedback(FeedbackFX fx)
        {
            if (fxLibrary == null) return;
            fx.SetLibrary(fxLibrary, unlitSprite);
            EditorUtility.SetDirty(fx);
        }

        private static WaterSurface WaterMesh(Transform parent, string name, float xMin, float yTop, float w, float depth, float flow, int order, Color top, Color bottom)
        {
            var go = Go(name, parent, new Vector2(xMin + w * 0.5f, yTop));
            go.AddComponent<MeshFilter>();
            var mr = go.AddComponent<MeshRenderer>();
            mr.sharedMaterial = waterMaterial != null ? waterMaterial : unlitSprite;
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            mr.receiveShadows = false;
            var surface = go.AddComponent<WaterSurface>();
            surface.Setup(w, depth, flow, order);
            surface.SetColors(top, bottom);
            return surface;
        }

        private static WaterBody Water(Transform parent, string name, float xMin, float yTop, float w, float depth, Vector2 flow, bool lake = false, bool deadly = true)
        {
            var holder = Go(name, parent, new Vector2(xMin + w * 0.5f, yTop));
            var surface = WaterMesh(holder.transform, "Superficie", xMin, yTop, w, depth, flow.x * 0.35f, 6, lake ? LakeTop : RiverTop, lake ? LakeBottom : RiverBottom);
            var volume = Go("Volume", holder.transform, new Vector2(xMin + w * 0.5f, yTop - depth * 0.5f));
            var box = volume.AddComponent<BoxCollider2D>();
            box.isTrigger = true;
            box.size = new Vector2(w, depth);
            var splash = MakeParticles("Respingos", holder.transform, new Color(0.85f, 0.95f, 1f, 0.9f), 0, 5f, 0.6f, 0.16f, true, 0.25f, 1.6f);
            var shape = splash.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 25f;
            shape.radius = 0.2f;
            splash.transform.rotation = Quaternion.Euler(-90f, 0f, 0f);
            var body = volume.AddComponent<WaterBody>();
            body.Setup(flow, surface, splash, "", deadly);
            if (Mathf.Abs(flow.x) > 0.1f) Current(holder.transform, xMin, yTop, w, depth, flow.x);
            return body;
        }

        private static void FrozenWater(Transform parent, float xMin, float yTop, float w, float depth, float thickness)
        {
            var surface = WaterMesh(parent, "Agua Congelada", xMin, yTop - thickness * 0.5f, w, depth - thickness * 0.5f, 0f, 5, new Color(0.55f, 0.78f, 0.92f, 0.85f), new Color(0.1f, 0.25f, 0.42f, 0.95f));
            surface.SetFrozen(true);
            var slab = Rect("Gelo", parent, xMin, yTop - thickness, w, thickness, iceTile != null ? Color.white : new Color(0.84f, 0.94f, 1f, 0.6f), 7, true, iceTile != null ? iceTile : squareSprite);
            if (snowCap != null) Rect("Geada", slab.transform, xMin, yTop - 0.78f, w, 1f, new Color(1f, 1f, 1f, 0.85f), 8, false, snowCap);
        }

        private static void Current(Transform parent, float xMin, float yTop, float w, float depth, float flowX)
        {
            var ps = MakeParticles("Correnteza", parent, new Color(0.85f, 0.95f, 1f, 0.35f), 0, 0.01f, 1.4f, 0.08f, true, 0.1f);
            ps.transform.position = new Vector3(xMin + w * 0.5f, yTop - depth * 0.45f, 0f);
            var main = ps.main;
            main.loop = true;
            main.playOnAwake = true;
            main.duration = 1f;
            main.startSpeed = 0f;
            main.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.12f);
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(w * 0.9f, depth * 0.8f, 0.1f);
            var em = ps.emission;
            em.rateOverTime = w * 2.5f;
            var vel = ps.velocityOverLifetime;
            vel.enabled = true;
            vel.space = ParticleSystemSimulationSpace.World;
            vel.x = new ParticleSystem.MinMaxCurve(flowX * 0.6f, flowX);
            vel.y = new ParticleSystem.MinMaxCurve(-0.1f, 0.1f);
            vel.z = new ParticleSystem.MinMaxCurve(0f, 0f);
            ps.GetComponent<ParticleSystemRenderer>().sortingOrder = 7;
        }

        private static GameObject Waterfall(Transform parent, string name, float xMin, float yBottom, float w, float h)
        {
            var go = Rect(name, parent, xMin, yBottom, w, h, tiles != null && tiles.waterfall != null ? new Color(1f, 1f, 1f, 0.95f) : WaterColor, 5, false, tiles != null && tiles.waterfall != null ? tiles.waterfall : squareSprite);
            var fall = Go("Fios", go.transform, new Vector2(xMin + w * 0.5f, yBottom + h));
            fall.AddComponent<WaterFall>().Setup(w, h, squareSprite, 6);
            var foam = MakeParticles("Espuma", go.transform, new Color(1f, 1f, 1f, 0.8f), 0, 2.2f, 0.7f, 0.35f, true, 0.2f, 0.6f);
            foam.transform.position = new Vector3(xMin + w * 0.5f, yBottom + 0.1f, 0f);
            var main = foam.main;
            main.loop = true;
            main.playOnAwake = true;
            main.duration = 1f;
            var shape = foam.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 55f;
            shape.radius = w * 0.4f;
            foam.transform.rotation = Quaternion.Euler(-90f, 0f, 0f);
            var em = foam.emission;
            em.rateOverTime = 18f * Mathf.Max(0.4f, w);
            foam.GetComponent<ParticleSystemRenderer>().sortingOrder = 8;
            return go;
        }

        private static GameObject Branch(string name, Transform parent, float trunkX, float xMin, float yTop, float w)
        {
            bool right = xMin + w * 0.5f >= trunkX;
            float start = right ? Mathf.Min(xMin, trunkX - 0.1f) : xMin;
            float end = right ? xMin + w : Mathf.Max(xMin + w, trunkX + 0.1f);
            var platform = OneWay(name, parent, start, yTop, end - start, ConsequenceColor, 3);
            var branchSr = platform.GetComponent<SpriteRenderer>();
            if (tiles != null && tiles.branch != null)
            {
                branchSr.size = new Vector2(end - start, tiles.branch.bounds.size.y);
                branchSr.transform.position += Vector3.down * (tiles.branch.bounds.size.y - 0.35f) * 0.5f;
                platform.GetComponent<BoxCollider2D>().offset = new Vector2(0f, (tiles.branch.bounds.size.y - 0.35f) * 0.5f);
            }
            float dir = right ? 1f : -1f;
            float reach = Mathf.Min(1.8f, (end - start) * 0.45f);
            Brace(platform.transform, new Vector2(trunkX, yTop - 1.4f), new Vector2(trunkX + dir * reach, yTop - 0.2f));
            return platform;
        }

        private static void Brace(Transform parent, Vector2 from, Vector2 to)
        {
            Vector2 d = to - from;
            var go = Go("Braço", parent, (from + to) * 0.5f);
            var skin = tiles != null && tiles.branch != null ? tiles.branch : squareSprite;
            var sr = AddSprite(go, skin, tiles != null ? new Color(0.8f, 0.75f, 0.7f) : WoodColor, 2);
            sr.drawMode = SpriteDrawMode.Tiled;
            sr.size = new Vector2(d.magnitude + 0.2f, 0.26f);
            go.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg);
        }
    }
}

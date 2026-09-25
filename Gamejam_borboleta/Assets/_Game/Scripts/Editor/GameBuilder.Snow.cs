using System.IO;
using UnityEditor;
using UnityEngine;

namespace ButterflyStep.EditorTools
{
    public static partial class GameBuilder
    {
        private const float MoundPower = 0.55f;

        private static float MoundProfile(float t) => Mathf.Pow(Mathf.Max(0f, 1f - t * t), MoundPower);

        private static float MoundHalfWidth(float yRatio)
        {
            yRatio = Mathf.Clamp01(yRatio);
            return Mathf.Sqrt(Mathf.Max(0f, 1f - Mathf.Pow(yRatio, 1f / MoundPower)));
        }

        private static Sprite MoundSprite(float w, float h)
        {
            int wp = Mathf.RoundToInt(w * 32f);
            int hp = Mathf.RoundToInt(h * 32f);
            string path = $"{Root}/Art/Placeholders/PixelMound_{wp}x{hp}.png";
            if (!File.Exists(path))
            {
                var tex = new Texture2D(wp, hp, TextureFormat.RGBA32, false);
                var clear = new Color(0f, 0f, 0f, 0f);
                for (int x = 0; x < wp; x++)
                {
                    float t = (x + 0.5f) / wp * 2f - 1f;
                    float bump = Mathf.Sin(x * 0.45f) * 0.6f + Mathf.Sin(x * 1.3f + 2f) * 0.4f;
                    int top = Mathf.Clamp(Mathf.RoundToInt(MoundProfile(t) * (hp - 3) + bump), 1, hp);
                    for (int y = 0; y < hp; y++)
                    {
                        if (y >= top) { tex.SetPixel(x, y, clear); continue; }
                        int depth = top - 1 - y;
                        int n = PixelHash(x / 2, y / 2) % 100;
                        float k = n < 10 ? 0.9f : n < 16 ? 1f : 0.96f;
                        Color c = new Color(0.95f * k, 0.97f * k, 1f * k, 1f);
                        if (depth == 0) c = Color.white;
                        else if (depth == 1) c = new Color(0.98f, 0.99f, 1f, 1f);
                        if (t > 0.15f && depth > 1) c = Color.Lerp(c, new Color(0.72f, 0.8f, 0.94f, 1f), Mathf.Clamp01((t - 0.15f) * 0.9f));
                        if (t < -0.2f && depth > 2 && depth < 5) c = Color.Lerp(c, Color.white, 0.5f);
                        if (y < 3) c = Color.Lerp(c, new Color(0.75f, 0.82f, 0.93f, 1f), 0.6f - y * 0.2f);
                        tex.SetPixel(x, y, c);
                    }
                }
                tex.Apply();
                File.WriteAllBytes(path, tex.EncodeToPNG());
                Object.DestroyImmediate(tex);
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            }
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 32f;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect;
            settings.spriteAlignment = (int)SpriteAlignment.BottomCenter;
            importer.SetTextureSettings(settings);
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static void SnowMound(Transform parent, float xMin, float yMin, float w, float h, bool climbable)
        {
            float cx = xMin + w * 0.5f;
            var visual = Go("Monte", parent, new Vector2(cx, yMin - 0.05f));
            AddSprite(visual, MoundSprite(w, h), Color.white, 2);
            if (climbable)
            {
                int steps = Mathf.Max(2, Mathf.CeilToInt(h / 0.9f));
                float stepH = h / steps;
                for (int i = 0; i < steps; i++)
                {
                    float top = (i + 1) * stepH;
                    float half = Mathf.Max(0.45f, MoundHalfWidth(Mathf.Min(0.97f, top / h)) * w * 0.5f);
                    var step = Go($"Degrau {i + 1}", parent, new Vector2(cx, yMin + i * stepH + stepH * 0.5f));
                    step.AddComponent<BoxCollider2D>().size = new Vector2(half * 2f, stepH);
                }
            }
            else
            {
                float half = MoundHalfWidth(0.55f) * w * 0.5f;
                var wall = Go("Bloqueio", parent, new Vector2(cx, yMin + h * 0.5f));
                wall.AddComponent<BoxCollider2D>().size = new Vector2(half * 2f, h);
            }
            var flakes = MakeParticles("Neve Soprada", parent, Color.white, 0, 0.5f, 1.6f, 0.08f, true, 0.2f, -0.02f);
            flakes.transform.position = new Vector3(cx, yMin + h, 0f);
            var main = flakes.main;
            main.loop = true;
            main.playOnAwake = true;
            main.duration = 1f;
            var shape = flakes.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(w * 0.5f, 0.2f, 0.1f);
            var em = flakes.emission;
            em.rateOverTime = 5f;
        }

        private static void SnowCover(Transform ground, float xMin, float yTop, float w)
        {
            if (snowCap == null) return;
            var holder = Go("Neve do Chão", ground, new Vector2(xMin + w * 0.5f, yTop));
            var snowSr = Rect("Neve", holder.transform, xMin - 0.05f, yTop - 0.4f, w + 0.1f, 1f, Color.white, 2, false, snowCap).GetComponent<SpriteRenderer>();
            snowSr.enabled = false;
            holder.AddComponent<SeasonalActive>().Setup(Season.Inverno);
        }
    }
}

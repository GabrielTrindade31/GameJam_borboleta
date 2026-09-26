using System.IO;
using UnityEditor;
using UnityEngine;

namespace ButterflyStep.EditorTools
{
    public static partial class GameBuilder
    {
        private const string DerivedRoot = ArtRoot + "/Derived";

        private static Sprite brambleSprite, vineSprite, bushDry, bushSnow;

        private static void LoadPlants()
        {
            brambleSprite = MakeSprite("PixelBramble", 32, 32, (x, y, s) =>
            {
                float cx = 16f + Mathf.Sin(y / 32f * Mathf.PI * 2f) * 4f;
                float d = x - cx;
                if (Mathf.Abs(d) <= 1.6f) return d < -0.5f ? new Color(0.62f, 0.4f, 0.6f) : new Color(0.38f, 0.22f, 0.38f);
                int k = y % 8;
                if (k == 2 && d > 1.6f && d < 4.5f) return new Color(0.9f, 0.86f, 0.8f);
                if (k == 6 && d < -1.6f && d > -4.5f) return new Color(0.9f, 0.86f, 0.8f);
                float cx2 = 16f - Mathf.Sin(y / 32f * Mathf.PI * 2f + 1f) * 7f;
                float d2 = x - cx2;
                if (Mathf.Abs(d2) <= 1f) return new Color(0.46f, 0.28f, 0.46f);
                if (y % 8 == 4 && d2 > 1f && d2 < 3.5f) return new Color(0.9f, 0.86f, 0.8f);
                return Color.clear;
            });
            vineSprite = MakeSprite("PixelVine", 32, 32, (x, y, s) =>
            {
                float cx = 16f + Mathf.Sin(y / 32f * Mathf.PI * 2f) * 3f;
                float d = x - cx;
                if (Mathf.Abs(d) <= 1.2f) return d < 0f ? new Color(0.5f, 0.72f, 0.3f) : new Color(0.28f, 0.48f, 0.2f);
                if (y % 16 == 5 && d > 1.2f && d < 4f) return new Color(0.4f, 0.62f, 0.26f);
                if (y % 16 == 13 && d < -1.2f && d > -4f) return new Color(0.4f, 0.62f, 0.26f);
                return Color.clear;
            });
            if (tiles == null || tiles.bush == null) return;
            bushDry = Recolor(tiles.bush, "BushDry", c =>
            {
                float l = c.r * 0.3f + c.g * 0.59f + c.b * 0.11f;
                return Color.Lerp(new Color(0.35f, 0.16f, 0.06f), new Color(1f, 0.62f, 0.2f), Mathf.Clamp01(l * 2.2f));
            });
            bushSnow = Recolor(tiles.bush, "BushSnow", c =>
            {
                float l = c.r * 0.3f + c.g * 0.59f + c.b * 0.11f;
                return Color.Lerp(new Color(0.62f, 0.72f, 0.86f), Color.white, Mathf.Clamp01(l * 2.4f));
            });
        }

        private static Sprite[][] SliceRows(string path, int frameW, int frameH, float ppu)
        {
            var tex = new Texture2D(2, 2);
            tex.LoadImage(File.ReadAllBytes(path));
            int cols = tex.width / frameW, rowCount = tex.height / frameH;
            int minY = frameH, feetMin = frameW, feetMax = -1;
            var used = new bool[rowCount, cols];
            for (int row = 0; row < rowCount; row++)
            {
                int y0 = tex.height - (row + 1) * frameH;
                for (int c = 0; c < cols; c++)
                    for (int y = 0; y < frameH; y++)
                        for (int x = 0; x < frameW; x++)
                        {
                            if (tex.GetPixel(c * frameW + x, y0 + y).a < 0.1f) continue;
                            used[row, c] = true;
                            if (row == 0 && y < minY) minY = y;
                        }
            }
            int y00 = tex.height - frameH;
            for (int c = 0; c < cols; c++)
                for (int y = minY; y < Mathf.Min(frameH, minY + 3); y++)
                    for (int x = 0; x < frameW; x++)
                    {
                        if (tex.GetPixel(c * frameW + x, y00 + y).a < 0.1f) continue;
                        feetMin = Mathf.Min(feetMin, x);
                        feetMax = Mathf.Max(feetMax, x);
                    }
            Object.DestroyImmediate(tex);
            var pivot = new Vector2(feetMax >= feetMin ? (feetMin + feetMax + 1) * 0.5f / frameW : 0.5f, (float)minY / frameH);
            string baseName = Path.GetFileNameWithoutExtension(path);
            var rects = new System.Collections.Generic.List<UnityEditor.SpriteRect>();
            for (int row = 0; row < rowCount; row++)
            {
                int n = 0;
                for (int c = 0; c < cols; c++)
                {
                    if (!used[row, c]) continue;
                    rects.Add(new UnityEditor.SpriteRect { name = $"{baseName}_r{row}_{n++}", rect = new Rect(c * frameW, (rowCount - 1 - row) * frameH, frameW, frameH), alignment = SpriteAlignment.Custom, pivot = pivot, spriteID = GUID.Generate() });
                }
            }
            var importer = PrepareImporter(path, ppu);
            ApplyRects(importer, path, rects);
            var result = new Sprite[rowCount][];
            for (int row = 0; row < rowCount; row++) result[row] = LoadSprites(path, $"{baseName}_r{row}_");
            return result;
        }

        private static PlayerBolt BuildBolt()
        {
            var go = new GameObject("PlayerBolt");
            var visual = new GameObject("Visual");
            visual.transform.SetParent(go.transform, false);
            var sprite = fxLibrary != null && fxLibrary.timeSwirl != null ? fxLibrary.timeSwirl : circleSprite;
            var sr = AddSprite(visual, sprite, new Color(0.8f, 0.7f, 1f), 30);
            sr.sharedMaterial = unlitSprite;
            float s = 0.9f / Mathf.Max(0.01f, sprite.bounds.size.x);
            visual.transform.localScale = new Vector3(s, s, 1f);
            var trail = MakeParticles("Rastro", go.transform, new Color(0.7f, 0.6f, 1f), 0, 0.3f, 0.35f, 0.18f, true, 0.1f);
            var em = trail.emission;
            em.rateOverTime = 40f;
            var main = trail.main;
            main.loop = true;
            main.playOnAwake = true;
            var bolt = go.AddComponent<PlayerBolt>();
            Set(bolt, "visual", visual.transform);
            Set(bolt, "hitMask", Mask(0, enemyLayer));
            Set(bolt, "solidMask", Mask(0));
            string path = $"{Root}/Prefabs/Player/{go.name}.prefab";
            if (!System.IO.Directory.Exists($"{Root}/Prefabs/Player")) System.IO.Directory.CreateDirectory($"{Root}/Prefabs/Player");
            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return prefab.GetComponent<PlayerBolt>();
        }

        private static Sprite Recolor(Sprite source, string name, System.Func<Color, Color> map)
        {
            string src = AssetDatabase.GetAssetPath(source.texture);
            if (!File.Exists(src)) return null;
            if (!Directory.Exists(DerivedRoot)) Directory.CreateDirectory(DerivedRoot);
            string path = $"{DerivedRoot}/{name}.png";
            var full = new Texture2D(2, 2);
            full.LoadImage(File.ReadAllBytes(src));
            var r = source.rect;
            var tex = new Texture2D((int)r.width, (int)r.height, TextureFormat.RGBA32, false);
            for (int y = 0; y < r.height; y++)
            {
                for (int x = 0; x < r.width; x++)
                {
                    var c = full.GetPixel((int)r.x + x, (int)r.y + y);
                    if (c.a < 0.05f) { tex.SetPixel(x, y, Color.clear); continue; }
                    var m = map(c);
                    m.a = c.a;
                    tex.SetPixel(x, y, m);
                }
            }
            tex.Apply();
            File.WriteAllBytes(path, tex.EncodeToPNG());
            Object.DestroyImmediate(full);
            Object.DestroyImmediate(tex);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = source.pixelsPerUnit;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect;
            settings.spriteAlignment = (int)SpriteAlignment.Custom;
            settings.spritePivot = new Vector2(source.pivot.x / r.width, source.pivot.y / r.height);
            importer.SetTextureSettings(settings);
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }
    }
}

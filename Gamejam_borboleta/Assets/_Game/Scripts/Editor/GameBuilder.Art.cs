using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;

namespace ButterflyStep.EditorTools
{
    public static partial class GameBuilder
    {
        private const string ArtRoot = Root + "/Art/ThirdParty";
        private const string LegacyRoot = ArtRoot + "/LegacyFantasy";
        private const string SlimeRoot = ArtRoot + "/CraftpixSlime";
        private const string PlantRoot = ArtRoot + "/CraftpixPlant";

        private class ClipDef
        {
            public string name;
            public Sprite[] frames;
            public float fps;
            public bool loop;
        }

        private class ArtSet
        {
            public readonly List<ClipDef> clips = new List<ClipDef>();
            public Sprite remains;
            public bool remainsFlipY;
            public bool facesLeft;

            public ArtSet Add(string name, Sprite[] frames, float fps, bool loop = true)
            {
                if (frames != null && frames.Length > 0) clips.Add(new ClipDef { name = name, frames = frames, fps = fps, loop = loop });
                return this;
            }

            public Sprite First => clips.Count > 0 ? clips[0].frames[0] : null;
        }

        private static ArtSet playerArt;
        private static ArtSet slimeArt;
        private static ArtSet boarArt;
        private static ArtSet mothArt;
        private static ArtSet plantArt;
        private static Sprite skySprite;

        private static bool HasArt => File.Exists($"{LegacyRoot}/Character/Idle/Idle-Sheet.png");

        private static void LoadArt()
        {
            playerArt = slimeArt = boarArt = mothArt = plantArt = null;
            skySprite = null;
            if (!HasArt) return;

            string hooded = $"{ArtRoot}/Penzilla/HoodedProtagonist.png";
            if (File.Exists(hooded))
            {
                const float ppu = 15f;
                var rows = SliceRows(hooded, 32, 32, ppu);
                var jump = rows[5];
                playerArt = new ArtSet()
                    .Add("Idle", rows[0], 3f)
                    .Add("Blink", rows[1], 6f, false)
                    .Add("Walk", rows[2], 8f)
                    .Add("Run", rows[3], 14f)
                    .Add("Duck", rows[4], 12f, false)
                    .Add("Jump", Range(jump, 1, 3), 10f, false)
                    .Add("Fall", Range(jump, 4, 4), 8f, false)
                    .Add("Vanish", rows[6], 12f, false)
                    .Add("Dead", rows[7], 10f, false)
                    .Add("Attack", rows[8], 24f, false);
            }
            else
            {
                string c = $"{LegacyRoot}/Character";
                var jumpAll = SliceSheet($"{c}/Jump-All/Jump-All-Sheet.png", 64, 64, 0, 32);
                playerArt = new ArtSet()
                    .Add("Idle", SliceSheet($"{c}/Idle/Idle-Sheet.png", 64, 80, 0, 32), 6f)
                    .Add("Run", SliceSheet($"{c}/Run/Run-Sheet.png", 80, 80, 0, 32), 12f)
                    .Add("Jump", Range(jumpAll, 2, 3), 10f, false)
                    .Add("Fall", Range(jumpAll, 7, 4), 8f, false)
                    .Add("Attack", SliceSheet($"{c}/Attack-01/Attack-01-Sheet.png", 96, 80, 0, 32), 20f, false)
                    .Add("Dead", SliceSheet($"{c}/Dead/Dead-Sheet.png", 64, 64, 0, 32), 10f, false);
            }

            string m = $"{LegacyRoot}/Mob";
            boarArt = new ArtSet { facesLeft = true }
                .Add("Walk", SliceSheet($"{m}/Boar/Walk/Walk-Base-Sheet.png", 48, 32, 0, 32), 10f)
                .Add("Run", SliceSheet($"{m}/Boar/Run/Run-Sheet.png", 48, 32, 0, 32), 14f)
                .Add("Idle", SliceSheet($"{m}/Boar/Idle/Idle-Sheet.png", 48, 32, 0, 32), 6f)
                .Add("Hurt", SliceSheet($"{m}/Boar/Hit-Vanish/Hit-Sheet.png", 48, 32, 0, 32), 14f, false);
            if (boarArt.clips.Count > 0)
            {
                boarArt.remains = boarArt.clips[0].frames[0];
                boarArt.remainsFlipY = true;
            }

            var snailDead = SliceSheet($"{m}/Snail/Dead-Sheet.png", 48, 32, 0, 32);
            var snailHide = SliceSheet($"{m}/Snail/Hide-Sheet.png", 48, 32, 0, 32);
            mothArt = new ArtSet { facesLeft = true, remains = snailDead.Length > 0 ? snailDead[snailDead.Length - 1] : null }
                .Add("Walk", SliceSheet($"{m}/Snail/walk-Sheet.png", 48, 32, 0, 32), 8f)
                .Add("Hide", snailHide, 10f, false)
                .Add("Hurt", snailHide, 18f, false)
                .Add("Fly", SliceSheet($"{m}/Small_Bee/Fly/Fly-Sheet.png", 64, 64, 0, 40), 14f)
                .Add("FlyHurt", SliceSheet($"{m}/Small_Bee/Hit/Hit-Sheet.png", 64, 64, 0, 40), 14f, false)
                .Add("FlyAttack", SliceSheet($"{m}/Small_Bee/Attack/Attack-Sheet.png", 64, 64, 0, 40), 14f, false);

            var slimeDeath = SliceSheet($"{SlimeRoot}/Slime_Death.png", 64, 64, 2, 24);
            slimeArt = new ArtSet { facesLeft = true, remains = slimeDeath.Length > 0 ? slimeDeath[slimeDeath.Length - 1] : null }
                .Add("Walk", SliceSheet($"{SlimeRoot}/Slime_Walk.png", 64, 64, 2, 24), 10f)
                .Add("Idle", SliceSheet($"{SlimeRoot}/Slime_Idle.png", 64, 64, 2, 24), 6f)
                .Add("Hurt", SliceSheet($"{SlimeRoot}/Slime_Hurt.png", 64, 64, 2, 24), 14f, false)
                .Add("Attack", SliceSheet($"{SlimeRoot}/Slime_Attack.png", 64, 64, 2, 24), 16f, false);

            var plantDeath = SliceSheet($"{PlantRoot}/Plant_Death.png", 64, 64, 2, 28);
            plantArt = new ArtSet { facesLeft = true, remains = plantDeath.Length > 0 ? plantDeath[plantDeath.Length - 1] : null }
                .Add("Idle", SliceSheet($"{PlantRoot}/Plant_Idle.png", 64, 64, 2, 28), 6f)
                .Add("Attack", SliceSheet($"{PlantRoot}/Plant_Attack.png", 64, 64, 2, 28), 14f, false)
                .Add("Hurt", SliceSheet($"{PlantRoot}/Plant_Hurt.png", 64, 64, 2, 28), 14f, false);

        }

        private static Sprite[] Range(Sprite[] source, int start, int count)
        {
            return source.Skip(start).Take(count).ToArray();
        }

        private static TextureImporter PrepareImporter(string path, float ppu)
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.spritePixelsPerUnit = ppu;
            importer.filterMode = FilterMode.Point;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.wrapMode = TextureWrapMode.Clamp;
            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect;
            importer.SetTextureSettings(settings);
            return importer;
        }

        private static void ApplyRects(TextureImporter importer, string path, List<SpriteRect> rects)
        {
            var factory = new SpriteDataProviderFactories();
            factory.Init();
            var provider = factory.GetSpriteEditorDataProviderFromObject(importer);
            provider.InitSpriteEditorDataProvider();
            provider.SetSpriteRects(rects.ToArray());
            var names = provider.GetDataProvider<ISpriteNameFileIdDataProvider>();
            names.SetNameFileIdPairs(rects.Select(r => new SpriteNameFileIdPair(r.name, r.spriteID)).ToList());
            provider.Apply();
            importer.SaveAndReimport();
        }

        private static Sprite[] LoadSprites(string path, string prefix)
        {
            return AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>()
                .Where(s => s.name.StartsWith(prefix))
                .OrderBy(s => int.Parse(s.name.Substring(prefix.Length)))
                .ToArray();
        }

        private static Sprite[] SliceSheet(string path, int frameW, int frameH, int row, float ppu)
        {
            if (!File.Exists(path)) return new Sprite[0];
            var tex = new Texture2D(2, 2);
            tex.LoadImage(File.ReadAllBytes(path));
            int cols = tex.width / frameW;
            int y0 = tex.height - (row + 1) * frameH;

            int minX = frameW, minY = frameH, maxX = -1, maxY = -1;
            var used = new bool[cols];
            for (int c = 0; c < cols; c++)
            {
                for (int y = 0; y < frameH; y++)
                {
                    for (int x = 0; x < frameW; x++)
                    {
                        if (tex.GetPixel(c * frameW + x, y0 + y).a < 0.1f) continue;
                        used[c] = true;
                        if (x < minX) minX = x;
                        if (x > maxX) maxX = x;
                        if (y < minY) minY = y;
                        if (y > maxY) maxY = y;
                    }
                }
            }

            int feetMin = frameW, feetMax = -1;
            for (int c = 0; c < cols; c++)
            {
                for (int y = minY; y < Mathf.Min(frameH, minY + 6); y++)
                {
                    for (int x = 0; x < frameW; x++)
                    {
                        if (tex.GetPixel(c * frameW + x, y0 + y).a < 0.1f) continue;
                        if (x < feetMin) feetMin = x;
                        if (x > feetMax) feetMax = x;
                    }
                }
            }
            Object.DestroyImmediate(tex);

            float pivotX = feetMax >= feetMin ? (feetMin + feetMax + 1) * 0.5f / frameW : 0.5f;
            float pivotY = maxY >= 0 ? (float)minY / frameH : 0f;

            string prefix = Path.GetFileNameWithoutExtension(path) + "_r" + row + "_";
            var rects = new List<SpriteRect>();
            for (int c = 0; c < cols; c++)
            {
                if (!used[c]) continue;
                rects.Add(new SpriteRect
                {
                    name = prefix + rects.Count,
                    rect = new Rect(c * frameW, y0, frameW, frameH),
                    alignment = SpriteAlignment.Custom,
                    pivot = new Vector2(pivotX, pivotY),
                    spriteID = GUID.Generate()
                });
            }

            var importer = PrepareImporter(path, ppu);
            ApplyRects(importer, path, rects);
            return LoadSprites(path, prefix);
        }

        private static SpriteAnimator AttachAnimator(GameObject visual, SpriteRenderer renderer, ArtSet art, string startClip)
        {
            var animator = visual.AddComponent<SpriteAnimator>();
            animator.Setup(renderer, startClip);
            foreach (var clip in art.clips) animator.AddClip(clip.name, clip.frames, clip.fps, clip.loop);
            renderer.sprite = art.First;
            EditorUtility.SetDirty(animator);
            return animator;
        }
    }
}

using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;

namespace ButterflyStep.EditorTools
{
    public static partial class GameBuilder
    {
        private struct Piece
        {
            public string name;
            public RectInt area;
            public bool trim;
            public Vector2 pivot;
            public Vector4 border;

            public Piece(string name, int x, int y, int w, int h, bool trim = false, float pivotY = 0.5f, int border = 0)
            {
                this.name = name;
                area = new RectInt(x, y, w, h);
                this.trim = trim;
                pivot = new Vector2(0.5f, pivotY);
                this.border = new Vector4(border, border, border, border);
            }
        }

        private class TileSet
        {
            public Sprite groundFill;
            public Sprite waterfall;
            public Sprite grassTop;
            public Sprite brick;
            public Sprite plank;
            public Sprite branch;
            public Sprite water;
            public Sprite crate;
            public Sprite door;
            public Sprite grate;
            public Sprite plaque;
            public Sprite rockBig;
            public Sprite rockSmall;
            public Sprite bush;
            public Sprite mushroom;
            public Sprite mushroomSmall;
            public Sprite sprout;
            public Sprite leafPlant;
            public Sprite lavender;
            public Sprite cattail;
            public Sprite blueFlower;
            public Sprite pineBig, pineBigRed, pineBigGold, pineBigBare;
            public Sprite pineMid, pineMidRed, pineMidGold, pineMidBare;
            public Sprite pineSmall, pineSmallRed, pineSmallGold;
            public Sprite[] bird;
            public Sprite uiParchment, uiWood, uiArrowRight, uiArrowLeft, uiClock, uiSkull, uiHeart, uiHeartEmpty, uiButton;
            public Font font;
            public Sprite key, hive, gear, gate, stump, uiArrowDown, uiSpore;
            public Sprite lilyPad, uiGold;
            public Sprite mountDark, mountLight;
            public Sprite[] treeDark, treeLight;
            public Sprite[] mossRocks, standingStones, runes;
            public Sprite logs, fence, boulders, mine, mushroomCluster;
            public Sprite[] darkPines;
            public Sprite[] yellowPines;
            public Sprite hollowLog, hollowStump;
            public Sprite leafGreen, leafGreen2, leafDry, vineStem;
            public Sprite hiveComb, hiveCombDark, hiveChestOpen, hiveChestClosed, hiveGem, hiveArch, hiveWindow, hiveDipper, honeyDrip;
        }

        private static TileSet tiles;

        private static Dictionary<string, Sprite> Cut(string path, float ppu, params Piece[] pieces)
        {
            var result = new Dictionary<string, Sprite>();
            if (!File.Exists(path)) return result;

            var tex = new Texture2D(2, 2);
            tex.LoadImage(File.ReadAllBytes(path));
            var rects = new List<SpriteRect>();
            foreach (var p in pieces)
            {
                var area = p.area;
                if (p.trim) area = TrimArea(tex, area);
                rects.Add(new SpriteRect
                {
                    name = p.name,
                    rect = new Rect(area.x, tex.height - area.y - area.height, area.width, area.height),
                    alignment = SpriteAlignment.Custom,
                    pivot = p.pivot,
                    border = p.border,
                    spriteID = GUID.Generate()
                });
            }
            Object.DestroyImmediate(tex);

            var importer = PrepareImporter(path, ppu);
            ApplyRects(importer, path, rects);
            foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(path))
            {
                if (asset is Sprite s) result[s.name] = s;
            }
            return result;
        }

        private static RectInt TrimArea(Texture2D tex, RectInt area)
        {
            int minX = int.MaxValue, minY = int.MaxValue, maxX = -1, maxY = -1;
            for (int y = area.y; y < area.y + area.height; y++)
            {
                for (int x = area.x; x < area.x + area.width; x++)
                {
                    if (x < 0 || y < 0 || x >= tex.width || y >= tex.height) continue;
                    if (tex.GetPixel(x, tex.height - 1 - y).a < 0.1f) continue;
                    if (x < minX) minX = x;
                    if (x > maxX) maxX = x;
                    if (y < minY) minY = y;
                    if (y > maxY) maxY = y;
                }
            }
            return maxX < 0 ? area : new RectInt(minX, minY, maxX - minX + 1, maxY - minY + 1);
        }

        private static Sprite Get(Dictionary<string, Sprite> map, string key) => map.TryGetValue(key, out var s) ? s : null;

        private static void LoadTiles()
        {
            tiles = null;
            if (!HasArt) return;
            tiles = new TileSet();
            string a = $"{LegacyRoot}/Assets";

            var t = Cut($"{a}/Tiles.png", 32,
                new Piece("groundFill", 64, 160, 32, 32),
                new Piece("grassTop", 16, 8, 48, 26),
                new Piece("brick", 160, 192, 48, 32),
                new Piece("plank", 80, 93, 48, 13),
                new Piece("branch", 171, 171, 68, 16),
                new Piece("water", 56, 300, 32, 24),
                new Piece("mushroom", 290, 240, 27, 32, false, 0f),
                new Piece("mushroomSmall", 321, 251, 14, 21, false, 0f),
                new Piece("sprout", 353, 274, 14, 14, false, 0f),
                new Piece("leafPlant", 320, 359, 32, 25, false, 0f),
                new Piece("lavender", 274, 272, 14, 32, false, 0f),
                new Piece("cattail", 257, 293, 15, 27, false, 0f),
                new Piece("blueFlower", 243, 272, 11, 32, false, 0f),
                new Piece("key", 242, 323, 12, 7),
                new Piece("hive", 194, 100, 30, 48),
                new Piece("stump", 160, 0, 32, 160, false, 0f),
                new Piece("lilyPad", 274, 315, 28, 37),
                new Piece("waterfall", 49, 272, 46, 64));
            tiles.groundFill = Get(t, "groundFill");
            tiles.waterfall = Get(t, "waterfall");
            tiles.grassTop = Get(t, "grassTop");
            tiles.brick = Get(t, "brick");
            tiles.plank = Get(t, "plank");
            tiles.branch = Get(t, "branch");
            tiles.water = Get(t, "water");
            tiles.mushroom = Get(t, "mushroom");
            tiles.mushroomSmall = Get(t, "mushroomSmall");
            tiles.sprout = Get(t, "sprout");
            tiles.leafPlant = Get(t, "leafPlant");
            tiles.lavender = Get(t, "lavender");
            tiles.cattail = Get(t, "cattail");
            tiles.blueFlower = Get(t, "blueFlower");
            tiles.key = Get(t, "key");
            tiles.hive = Get(t, "hive");
            tiles.stump = Get(t, "stump");
            tiles.lilyPad = Get(t, "lilyPad");

            var hv = Cut($"{a}/Hive.png", 32,
                new Piece("hiveComb", 0, 64, 48, 32), new Piece("hiveCombDark", 48, 64, 48, 32),
                new Piece("hiveChestClosed", 85, 42, 22, 21, true, 0f), new Piece("hiveChestOpen", 85, 9, 22, 22, true, 0f), new Piece("hiveGem", 97, 98, 13, 13, true),
                new Piece("hiveArch", 64, 160, 48, 64, false, 0f), new Piece("hiveWindow", 0, 208, 48, 48),
                new Piece("hiveDipper", 112, 120, 16, 50, true, 0f), new Piece("honeyDrip", 112, 0, 16, 46, true, 1f));
            tiles.hiveComb = Get(hv, "hiveComb");
            tiles.hiveCombDark = Get(hv, "hiveCombDark");
            tiles.hiveChestOpen = Get(hv, "hiveChestOpen");
            tiles.hiveChestClosed = Get(hv, "hiveChestClosed");
            tiles.hiveGem = Get(hv, "hiveGem");
            tiles.hiveArch = Get(hv, "hiveArch");
            tiles.hiveWindow = Get(hv, "hiveWindow");
            tiles.hiveDipper = Get(hv, "hiveDipper");
            tiles.honeyDrip = Get(hv, "honeyDrip");

            var bd = Cut($"{a}/Buildings.png", 32, new Piece("gate", 340, 116, 44, 66, true),
                new Piece("logs", 14, 158, 52, 20, true, 0f), new Piece("fence", 160, 174, 48, 36, true, 0f),
                new Piece("boulders", 30, 205, 84, 37, true, 0f), new Piece("mine", 272, 209, 128, 160, true, 0f));
            tiles.gate = Get(bd, "gate");
            tiles.logs = Get(bd, "logs");
            tiles.fence = Get(bd, "fence");
            tiles.boulders = Get(bd, "boulders");
            tiles.mine = Get(bd, "mine");

            var sky = Cut($"{LegacyRoot}/Background/Background.png", 16, new Piece("clouds", 0, 0, 480, 272, false, 0f));
            skySprite = Get(sky, "clouds");
            var bg = Cut($"{LegacyRoot}/Trees/Background.png", 24,
                new Piece("mountDark", 704, 0, 96, 256, false, 0f), new Piece("mountLight", 800, 0, 96, 256, false, 0f),
                new Piece("treeD0", 0, 0, 96, 256, true, 0f), new Piece("treeD1", 112, 0, 96, 256, true, 0f), new Piece("treeD2", 224, 0, 128, 256, true, 0f),
                new Piece("treeL0", 352, 0, 96, 256, true, 0f), new Piece("treeL1", 464, 0, 96, 256, true, 0f), new Piece("treeL2", 576, 0, 128, 256, true, 0f));
            tiles.mountDark = Get(bg, "mountDark");
            tiles.mountLight = Get(bg, "mountLight");
            tiles.treeDark = new[] { Get(bg, "treeD0"), Get(bg, "treeD1"), Get(bg, "treeD2") };
            tiles.treeLight = new[] { Get(bg, "treeL0"), Get(bg, "treeL1"), Get(bg, "treeL2") };

            var i = Cut($"{a}/Interior-01.png", 32,
                new Piece("crate", 96, 128, 32, 32),
                new Piece("door", 131, 68, 43, 60, true),
                new Piece("grate", 80, 192, 48, 48, true, 0f),
                new Piece("plaque", 80, 0, 32, 32, true));
            tiles.crate = Get(i, "crate");
            tiles.door = Get(i, "door");
            tiles.grate = Get(i, "grate");
            tiles.plaque = Get(i, "plaque");

            var r = Cut($"{a}/Props-Rocks.png", 32,
                new Piece("rockBig", 0, 0, 64, 78, true, 0f),
                new Piece("rockSmall", 132, 33, 45, 47, true, 0f),
                new Piece("mossTall", 0, 80, 64, 80, true, 0f), new Piece("mossWide", 64, 112, 64, 48, true, 0f),
                new Piece("mossRound", 128, 112, 48, 48, true, 0f), new Piece("mossLong", 208, 128, 66, 34, true, 0f),
                new Piece("stoneTall", 0, 164, 64, 94, true, 0f), new Piece("stoneMid", 66, 176, 44, 82, true, 0f), new Piece("stoneShort", 116, 192, 40, 64, true, 0f),
                new Piece("rune0", 144, 256, 16, 16, true), new Piece("rune1", 160, 256, 16, 16, true), new Piece("rune2", 176, 256, 16, 16, true),
                new Piece("rune3", 208, 256, 16, 16, true), new Piece("rune4", 224, 256, 16, 16, true), new Piece("rune5", 176, 288, 16, 16, true));
            tiles.mossRocks = new[] { Get(r, "mossTall"), Get(r, "mossWide"), Get(r, "mossRound"), Get(r, "mossLong") };
            tiles.standingStones = new[] { Get(r, "stoneTall"), Get(r, "stoneMid"), Get(r, "stoneShort") };
            tiles.runes = new[] { Get(r, "rune0"), Get(r, "rune1"), Get(r, "rune2"), Get(r, "rune3"), Get(r, "rune4"), Get(r, "rune5") };
            tiles.rockBig = Get(r, "rockBig");
            tiles.rockSmall = Get(r, "rockSmall");

            var b = Cut($"{a}/Tree-Assets.png", 32, new Piece("bush", 206, 0, 130, 98, true, 0f), new Piece("mushroomCluster", 129, 30, 46, 32, true, 0f),
                new Piece("hollowLog", 0, 0, 80, 80, true, 0f), new Piece("hollowStump", 64, 128, 64, 64, false, 0f),
                new Piece("leafGreen", 128, 48, 16, 16, true), new Piece("leafGreen2", 144, 48, 16, 16, true), new Piece("leafDry", 96, 48, 16, 16, true), new Piece("vineStem", 128, 64, 16, 32));
            tiles.bush = Get(b, "bush");
            tiles.mushroomCluster = Get(b, "mushroomCluster");
            tiles.hollowLog = Get(b, "hollowLog");
            tiles.hollowStump = Get(b, "hollowStump");
            tiles.leafGreen = Get(b, "leafGreen");
            tiles.leafGreen2 = Get(b, "leafGreen2");
            tiles.leafDry = Get(b, "leafDry");
            tiles.vineStem = Get(b, "vineStem");

            string tr = $"{LegacyRoot}/Trees";
            Piece[] PinePieces(bool bare) => bare
                ? new[]
                {
                    new Piece("pineBig", 0, 0, 107, 368, false, 0f), new Piece("pineMid", 4, 721, 88, 207, false, 0f),
                    new Piece("pineSmall", 5, 1092, 70, 108, false, 0f), new Piece("bareBig", 354, 0, 69, 368, false, 0f),
                    new Piece("bareMid", 309, 721, 53, 207, false, 0f)
                }
                : new[]
                {
                    new Piece("pineBig", 0, 0, 107, 368, false, 0f), new Piece("pineMid", 4, 721, 88, 207, false, 0f),
                    new Piece("pineSmall", 5, 1092, 70, 108, false, 0f)
                };
            var green = Cut($"{tr}/Green-Tree.png", 32, PinePieces(true));
            var red = Cut($"{tr}/Red-Tree.png", 32, PinePieces(false));
            var gold = Cut($"{tr}/Golden-Tree.png", 32, PinePieces(false));
            var dark = Cut($"{tr}/Dark-Tree.png", 32, PinePieces(false));
            var yellow = Cut($"{tr}/Yellow-Tree.png", 32, PinePieces(false));
            tiles.yellowPines = new[] { Get(yellow, "pineSmall"), Get(yellow, "pineMid"), Get(yellow, "pineBig") };
            tiles.darkPines = new[] { Get(dark, "pineBig"), Get(dark, "pineMid"), Get(dark, "pineSmall") };
            tiles.pineBig = Get(green, "pineBig");
            tiles.pineMid = Get(green, "pineMid");
            tiles.pineSmall = Get(green, "pineSmall");
            tiles.pineBigBare = Get(green, "bareBig");
            tiles.pineMidBare = Get(green, "bareMid");
            tiles.pineBigRed = Get(red, "pineBig");
            tiles.pineMidRed = Get(red, "pineMid");
            tiles.pineSmallRed = Get(red, "pineSmall");
            tiles.pineBigGold = Get(gold, "pineBig");
            tiles.pineMidGold = Get(gold, "pineMid");
            tiles.pineSmallGold = Get(gold, "pineSmall");

            var ui = Cut($"{LegacyRoot}/HUD/Base-01.png", 32,
                new Piece("parchment", 0, 0, 64, 64, false, 0.5f, 6),
                new Piece("wood", 16, 224, 48, 48, false, 0.5f, 6),
                new Piece("arrowRight", 2, 178, 12, 12, true),
                new Piece("arrowLeft", 2, 194, 12, 12, true),
                new Piece("clock", 2, 274, 13, 13, true),
                new Piece("skull", 2, 258, 13, 13, true),
                new Piece("heart", 32, 144, 17, 17, true),
                new Piece("heartEmpty", 32, 174, 17, 17, true),
                new Piece("button", 242, 198, 60, 34, true, 0.5f, 6),
                new Piece("gear", 48, 206, 17, 18, true),
                new Piece("arrowDown", 17, 160, 14, 14, true),
                new Piece("spore", 61, 111, 19, 19, true),
                new Piece("gold", 61, 157, 19, 19, true));
            tiles.uiParchment = Get(ui, "parchment");
            tiles.uiWood = Get(ui, "wood");
            tiles.uiArrowRight = Get(ui, "arrowRight");
            tiles.uiArrowLeft = Get(ui, "arrowLeft");
            tiles.uiClock = Get(ui, "clock");
            tiles.uiSkull = Get(ui, "skull");
            tiles.uiHeart = Get(ui, "heart");
            tiles.uiHeartEmpty = Get(ui, "heartEmpty");
            tiles.uiButton = Get(ui, "button");
            tiles.gear = Get(ui, "gear");
            tiles.uiArrowDown = Get(ui, "arrowDown");
            tiles.uiSpore = Get(ui, "spore");
            tiles.uiGold = Get(ui, "gold");

            string birdPath = $"{Root}/Art/CC0/Bird";
            var b1 = Cut($"{birdPath}/Bird_1.png", 200, new Piece("bird0", 0, 0, 256, 256, true, 0.5f));
            var b2 = Cut($"{birdPath}/Bird_2.png", 200, new Piece("bird1", 0, 0, 256, 256, true, 0.5f));
            tiles.bird = new[] { Get(b1, "bird0"), Get(b2, "bird1") };

            tiles.font = AssetDatabase.LoadAssetAtPath<Font>($"{Root}/Art/Fonts/PixelifySans.ttf");
        }

        private static bool Skin(ref Sprite sprite, ref Color color)
        {
            if (tiles == null) return false;
            if (sprite != null && sprite != blockSprite && sprite != squareSprite) return false;
            Sprite skin = null;
            Color tint = Color.white;
            if (color == GroundColor) skin = tiles.groundFill;
            else if (color == RockColor) { skin = tiles.groundFill; tint = new Color(0.7f, 0.72f, 0.78f); }
            else if (color == WoodColor) skin = tiles.plank;
            else if (color == ConsequenceColor) skin = tiles.branch;
            else if (color == DoorColor) skin = tiles.door;
            else if (color == LilyColor) skin = tiles.lilyPad;
            else if (color == WaterColor) { skin = tiles.water; tint = new Color(1f, 1f, 1f, 0.9f); }
            if (skin == null) return false;
            sprite = skin;
            color = tint;
            return true;
        }

        private static void Decor(Transform parent, Sprite sprite, Vector2 bottomCenter, float scale = 1f, int order = -20, Color? tint = null)
        {
            if (sprite == null) return;
            var go = Go(sprite.name, parent, bottomCenter);
            AddSprite(go, sprite, tint ?? Color.white, order);
            go.transform.localScale = new Vector3(scale, scale, 1f);
        }

        private static SeasonalSprite SeasonalTree(Transform parent, Vector2 bottomCenter, int size, float scale = 1f, int order = -25, bool yellowVariant = false)
        {
            if (tiles == null) return null;
            Sprite green = size == 2 ? tiles.pineBig : size == 1 ? tiles.pineMid : tiles.pineSmall;
            Sprite gold = size == 2 ? tiles.pineBigGold : size == 1 ? tiles.pineMidGold : tiles.pineSmallGold;
            Sprite red = size == 2 ? tiles.pineBigRed : size == 1 ? tiles.pineMidRed : tiles.pineSmallRed;
            Sprite bare = size == 2 ? tiles.pineBigBare : size == 1 ? tiles.pineMidBare : tiles.pineSmallRed;
            if (yellowVariant && tiles.yellowPines != null && tiles.yellowPines[size] != null)
            {
                red = gold;
                gold = tiles.yellowPines[size];
            }
            var go = Go("Pinheiro (estações)", parent, bottomCenter);
            var sr = AddSprite(go, green, Color.white, order);
            go.transform.localScale = new Vector3(scale, scale, 1f);
            var seasonal = go.AddComponent<SeasonalSprite>();
            seasonal.Setup(sr, green, gold, red, bare);
            return seasonal;
        }

        private static void Bird(Transform parent, Vector2 position, float scale, int order, bool flying = true, float radius = 2.5f)
        {
            var go = Go("Passaro", parent, position);
            var sr = AddSprite(go, tiles.bird[0], Color.white, order);
            go.transform.localScale = new Vector3(scale, scale, 1f);
            var anim = go.AddComponent<SpriteAnimator>();
            anim.Setup(sr, "Voo");
            anim.AddClip("Voo", tiles.bird, flying ? 10f : 4f, true);
            go.AddComponent<BirdFlight>().Setup(sr, flying, new Vector2(radius, radius * 0.3f), 0.6f + (position.x * 0.37f % 0.5f));
        }

        private static void HollowStump(Transform parent, Vector2 bottom, float scale)
        {
            if (tiles.hollowStump == null) return;
            var size = tiles.hollowStump.bounds.size * scale;
            Part(parent, "Fundo do Oco", bottom.x - size.x * 0.3f, bottom.y, size.x * 0.6f, size.y * 0.8f, new Color(0.08f, 0.07f, 0.05f), -13, false, squareSprite);
            Decor(parent, tiles.hollowStump, bottom, scale, -12);
        }

        private static void DarkForest(float parallax, float xMin, float xMax, float y)
        {
            if (tiles == null || tiles.darkPines == null || tiles.darkPines[0] == null) return;
            var layer = new GameObject("Trees_Dark").transform;
            layer.SetParent(groupBackground, false);
            layer.gameObject.AddComponent<ParallaxLayer>().Setup(parallax);
            var rng = new System.Random(Mathf.RoundToInt(xMin * 5 + xMax * 11));
            int i = 0;
            for (float x = xMin - 20f; x < xMax + 20f; x += 4f + (float)rng.NextDouble() * 6f)
            {
                var sprite = tiles.darkPines[rng.Next(2)];
                if (sprite == null) continue;
                var go = Go("Pinheiro Escuro " + i++, layer, new Vector2(x, y));
                AddSprite(go, sprite, new Color(0.75f, 0.8f, 0.85f, 0.95f), -60);
                float s = 0.55f + (float)rng.NextDouble() * 0.3f;
                go.transform.localScale = new Vector3(rng.Next(2) == 0 ? s : -s, s, 1f);
            }
        }

        private static int sceneryProps;

        private static void SceneryProp(Transform parent, int i, float x, float groundY)
        {
            i = sceneryProps++;
            switch (i % 6)
            {
                case 0:
                    if (tiles.mossRocks != null) Decor(parent, tiles.mossRocks[(i / 5) % tiles.mossRocks.Length], new Vector2(x + 1.9f, groundY), 0.8f, -12);
                    break;
                case 1:
                    if (tiles.standingStones == null) break;
                    var stone = tiles.standingStones[(i / 5) % tiles.standingStones.Length];
                    if (stone == null) break;
                    Decor(parent, stone, new Vector2(x - 1.8f, groundY), 0.75f, -12);
                    var rune = tiles.runes != null ? tiles.runes[i % tiles.runes.Length] : null;
                    if (rune == null) break;
                    var glyph = Go("Runa do Tempo", parent, new Vector2(x - 1.8f, groundY + stone.bounds.size.y * 0.75f * 0.55f));
                    var sr = AddSprite(glyph, rune, new Color(0.75f, 0.97f, 1f), -11);
                    sr.sharedMaterial = unlitSprite;
                    glyph.transform.localScale = new Vector3(1f, 1f, 1f);
                    glyph.AddComponent<PulseGlow>();
                    break;
                case 2:
                    Decor(parent, tiles.logs, new Vector2(x + 1.7f, groundY), 0.9f, -12);
                    break;
                case 3:
                    Decor(parent, tiles.fence, new Vector2(x - 2.1f, groundY), 0.9f, -12);
                    break;
                case 5:
                    Decor(parent, tiles.hollowLog, new Vector2(x + 2f, groundY), 0.75f, -12);
                    break;
                default:
                    Decor(parent, tiles.boulders, new Vector2(x + 2.2f, groundY), 0.8f, -13);
                    Decor(parent, tiles.mushroomCluster, new Vector2(x + 1.2f, groundY), 0.8f, -11);
                    break;
            }
        }

        private static void Scenery(float groundY, params float[] treeXs)
        {
            if (tiles == null) return;
            var parent = new GameObject("--- Scenery").transform;
            for (int i = 0; i < treeXs.Length; i++)
            {
                int size = i % 3 == 0 ? 1 : i % 3 == 1 ? 0 : 1;
                SeasonalTree(parent, new Vector2(treeXs[i], groundY), size, i % 2 == 0 ? 1f : 0.8f, -26 - i % 2, (i + Mathf.RoundToInt(treeXs[i])) % 2 == 0);
                if (i % 2 == 0) Decor(parent, tiles.mushroomSmall, new Vector2(treeXs[i] + 1.1f, groundY), 1f, -10);
                else Decor(parent, tiles.blueFlower, new Vector2(treeXs[i] - 1.2f, groundY), 0.8f, -10);
                SceneryProp(parent, i, treeXs[i], groundY);
            }
        }
    }
}

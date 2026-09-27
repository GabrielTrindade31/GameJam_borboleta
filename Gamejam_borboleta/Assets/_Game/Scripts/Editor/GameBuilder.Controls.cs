using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace ButterflyStep.EditorTools
{
    public static partial class GameBuilder
    {
        private static readonly Color ControlsInk = new Color(0.22f, 0.13f, 0.06f);
        private static Sprite keyCapSprite, sectionSprite, labelSprite, swordIcon, mouseIcon, mouseRightIcon;

        private struct ControlRow
        {
            public string keys;
            public string text;
            public string note;

            public ControlRow(string k, string t, string n = "")
            {
                keys = k;
                text = t;
                note = n;
            }
        }

        private static Sprite UISprite(string name, int w, int h, int border, System.Func<int, int, Color> pixel)
        {
            string path = $"{Root}/Art/Placeholders/{name}.png";
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                    tex.SetPixel(x, y, pixel(x, y));
            tex.Apply();
            File.WriteAllBytes(path, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 32f;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.spriteBorder = new Vector4(border, border, border, border);
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static Color MouseColor(int x, int y, bool right)
        {
            float nx = (x - 7.5f) / 5.5f, ny = (y - 7f) / 7.2f;
            float r = nx * nx + ny * ny;
            var ink = new Color(0.16f, 0.1f, 0.06f);
            if (r > 1f) return Color.clear;
            if (r > 0.72f) return ink;
            if (x == 7 || x == 8) return y >= 8 ? ink : new Color(0.95f, 0.95f, 0.95f);
            if (y >= 8 && (right ? x > 8 : x < 7)) return new Color(0.85f, 0.2f, 0.2f);
            return new Color(0.95f, 0.95f, 0.95f);
        }

        private static void CreateControlSprites()
        {
            var outline = new Color(0.16f, 0.1f, 0.06f);
            keyCapSprite = UISprite("UIKeyCap", 16, 16, 5, (x, y) =>
            {
                if ((x == 0 || x == 15) && (y == 0 || y == 15)) return Color.clear;
                if (x == 0 || x == 15 || y == 0 || y == 15) return outline;
                if (y <= 3) return new Color(0.55f, 0.52f, 0.48f);
                if (y == 14) return Color.white;
                return new Color(0.9f, 0.88f, 0.84f);
            });
            sectionSprite = UISprite("UISection", 16, 16, 5, (x, y) =>
            {
                if ((x == 0 || x == 15) && (y == 0 || y == 15)) return Color.clear;
                if (x == 0 || x == 15 || y == 0 || y == 15) return new Color(0.72f, 0.58f, 0.38f);
                return new Color(0.98f, 0.9f, 0.72f, 0.75f);
            });
            labelSprite = UISprite("UISectionLabel", 16, 16, 5, (x, y) =>
            {
                if ((x == 0 || x == 15) && (y == 0 || y == 15)) return Color.clear;
                if (x == 0 || x == 15 || y == 0 || y == 15) return new Color(0.7f, 0.55f, 0.36f);
                return new Color(0.86f, 0.72f, 0.5f);
            });
            swordIcon = UISprite("UIIconSword", 16, 16, 0, (x, y) =>
            {
                int d = x - y;
                if (x >= 4 && x <= 14 && y >= 4 && y <= 14 && Mathf.Abs(d) <= 1) return d == 0 ? new Color(0.9f, 0.93f, 1f) : new Color(0.55f, 0.6f, 0.7f);
                if (x + y >= 7 && x + y <= 9 && x >= 1 && x <= 7) return new Color(0.85f, 0.65f, 0.2f);
                if (x >= 1 && x <= 3 && y >= 1 && y <= 3 && Mathf.Abs(x - y) <= 1) return new Color(0.5f, 0.3f, 0.15f);
                return Color.clear;
            });
            mouseIcon = UISprite("UIIconMouseL", 16, 16, 0, (x, y) => MouseColor(x, y, false));
            mouseRightIcon = UISprite("UIIconMouseR", 16, 16, 0, (x, y) => MouseColor(x, y, true));
        }

        private static void BuildControlsPanel(RectTransform box)
        {
            CreateControlSprites();
            UIText(UIRect("Titulo", box, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -28f), new Vector2(1000f, 80f)), "CONTROLES", 62, TextAnchor.MiddleCenter, ControlsInk, FontStyle.Bold, false);
            float y = -120f;
            y = ControlSection(box, y, playerArt != null ? playerArt.First : null, "MOVIMENTO", new[]
            {
                new ControlRow("[A] / [D]", "Andar"),
                new ControlRow("[ESPAÇO]", "Pular"),
                new ControlRow("[W] / [S]", "Subir / descer trepadeiras")
            });
            y = ControlSection(box, y, swordIcon, "AÇÕES", new[]
            {
                new ControlRow("{mouseL} Clique Esq. / [J]", "Atacar", "(pule em cima de inimigos pequenos)"),
                new ControlRow("{mouseR} Clique Dir. / [K]", "Disparo do Tempo", "(depois do baú da colmeia)"),
                new ControlRow("[F]", "Interagir")
            });
            y = ControlSection(box, y, fxLibrary != null ? fxLibrary.timeClock : null, "TEMPO", new[]
            {
                new ControlRow("[Q] / [E]", "Voltar / Avançar no tempo"),
                new ControlRow("[SHIFT] + [Q] / [E]", "Espiar outro dia"),
                new ControlRow("[C]", "Pausar o tempo")
            });
            ControlSection(box, y, tiles != null ? tiles.gear : null, "SISTEMA", new[]
            {
                new ControlRow("[ESC]", "Pausa"),
                new ControlRow("[R]", "Reiniciar fase")
            });
            UIText(UIRect("Controle", box, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 104f), new Vector2(1450f, 36f)),
                "Controle: analógico anda · A pula · X ataca · B dispara · Y interage · LB/RB tempo · LT espia · RT pausa o tempo · START pausa",
                22, TextAnchor.MiddleCenter, new Color(0.35f, 0.22f, 0.1f), FontStyle.Normal, false);
        }

        private static float ControlSection(RectTransform box, float top, Sprite icon, string title, ControlRow[] rows)
        {
            const float rowH = 56f;
            float h = rows.Length * rowH + 28f;
            var section = UIRect(title, box, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, top), new Vector2(1500f, h));
            UIImage(section, Color.white, sectionSprite);
            var label = UIRect("Rotulo", section, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(16f, 0f), new Vector2(360f, h - 24f));
            UIImage(label, Color.white, labelSprite);
            if (icon != null)
            {
                var iconRt = UIRect("Icone", label, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(62f, 0f), new Vector2(100f, 100f));
                UIImage(iconRt, Color.white, icon).preserveAspect = true;
            }
            UIText(UIRect("Titulo", label, new Vector2(0f, 0.5f), new Vector2(1f, 0.5f), new Vector2(0f, 0.5f), new Vector2(118f, 0f), new Vector2(-124f, 60f)), title, 36, TextAnchor.MiddleLeft, ControlsInk, FontStyle.Bold, false);
            var divider = UIRect("Divisor", section, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(400f, 0f), new Vector2(4f, h - 40f));
            UIImage(divider, new Color(0.72f, 0.58f, 0.38f));
            for (int i = 0; i < rows.Length; i++)
            {
                float cy = h * 0.5f - 14f - rowH * (i + 0.5f);
                ControlLine(section, cy, rows[i]);
            }
            return top - h - 14f;
        }

        private static void ControlLine(RectTransform section, float cy, ControlRow row)
        {
            float x = 430f;
            foreach (var token in Tokenize(row.keys))
            {
                if (token.StartsWith("[") && token.EndsWith("]"))
                {
                    string k = token.Substring(1, token.Length - 2);
                    float w = Mathf.Max(56f, 26f + k.Length * 20f);
                    var cap = UIRect("Tecla " + k, section, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(x, cy), new Vector2(w, 50f));
                    UIImage(cap, Color.white, keyCapSprite);
                    var t = Stretch("Letra", cap);
                    t.offsetMin = new Vector2(0f, 6f);
                    UIText(t, k, 28, TextAnchor.MiddleCenter, ControlsInk, FontStyle.Bold, false);
                    x += w + 12f;
                }
                else if (token == "{mouseL}" || token == "{mouseR}")
                {
                    var m = UIRect("Mouse", section, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(x, cy), new Vector2(40f, 48f));
                    UIImage(m, Color.white, token == "{mouseL}" ? mouseIcon : mouseRightIcon).preserveAspect = true;
                    x += 50f;
                }
                else
                {
                    float w = token.Length * 17f + 8f;
                    UIText(UIRect("Texto", section, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(x, cy), new Vector2(w, 50f)), token, 28, TextAnchor.MiddleCenter, ControlsInk, FontStyle.Normal, false);
                    x += w + 6f;
                }
            }
            float textX = Mathf.Max(x + 24f, 860f);
            string text = string.IsNullOrEmpty(row.note) ? row.text : $"{row.text}  <size=22><color=#7a5a3a>{row.note}</color></size>";
            UIText(UIRect("Descricao", section, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(textX, cy), new Vector2(1480f - textX, 50f)), text, 30, TextAnchor.MiddleLeft, ControlsInk, FontStyle.Normal, false);
        }

        private static List<string> Tokenize(string spec)
        {
            var result = new List<string>();
            foreach (var part in spec.Split(' '))
            {
                if (part.Length == 0) continue;
                bool plain = !(part.StartsWith("[") || part.StartsWith("{") || part == "/" || part == "+");
                if (plain && result.Count > 0)
                {
                    var last = result[result.Count - 1];
                    bool lastPlain = !(last.StartsWith("[") || last.StartsWith("{") || last == "/" || last == "+");
                    if (lastPlain)
                    {
                        result[result.Count - 1] = last + " " + part;
                        continue;
                    }
                }
                result.Add(part);
            }
            return result;
        }
    }
}

using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace ButterflyStep.EditorTools
{
    public static partial class GameBuilder
    {
        public const string Root = "Assets/_Game";

        private static Font uiFont;

        private static Font UIFont
        {
            get
            {
                if (uiFont == null && tiles != null) uiFont = tiles.font;
                if (uiFont == null) uiFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                return uiFont;
            }
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = Path.GetDirectoryName(path).Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }

        private static void Set(Object target, string field, object value)
        {
            var so = new SerializedObject(target);
            var p = so.FindProperty(field);
            if (p == null)
            {
                Debug.LogError($"[GameBuilder] Campo '{field}' não encontrado em {target.GetType().Name}.");
                return;
            }

            switch (value)
            {
                case null: p.objectReferenceValue = null; break;
                case Object o: p.objectReferenceValue = o; break;
                case int i:
                    if (p.propertyType == SerializedPropertyType.Float) p.floatValue = i;
                    else if (p.propertyType == SerializedPropertyType.Enum) p.enumValueIndex = i;
                    else p.intValue = i;
                    break;
                case float f: p.floatValue = f; break;
                case bool b: p.boolValue = b; break;
                case string s: p.stringValue = s; break;
                case Color c: p.colorValue = c; break;
                case Vector2 v2: p.vector2Value = v2; break;
                case Vector3 v3: p.vector3Value = v3; break;
                case LayerMask m: p.intValue = m.value; break;
                default: Debug.LogError($"[GameBuilder] Tipo não suportado para '{field}'."); break;
            }
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static GameObject Go(string name, Transform parent, Vector3 worldPos, int layer = 0)
        {
            var go = new GameObject(name) { layer = layer };
            if (parent != null) go.transform.SetParent(parent, false);
            go.transform.position = worldPos;
            return go;
        }

        private static SpriteRenderer AddSprite(GameObject go, Sprite sprite, Color color, int order)
        {
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color = color;
            sr.sortingOrder = order;
            return sr;
        }

        private static GameObject Rect(string name, Transform parent, float xMin, float yMin, float w, float h, Color color, int order, bool solid, Sprite sprite = null)
        {
            var go = Go(name, parent, new Vector3(xMin + w * 0.5f, yMin + h * 0.5f, 0f));
            var skin = sprite != null ? sprite : blockSprite;
            Skin(ref skin, ref color);
            var sr = AddSprite(go, skin, color, order);
            sr.drawMode = SpriteDrawMode.Tiled;
            sr.tileMode = SpriteTileMode.Continuous;
            sr.size = new Vector2(w, h);
            if (solid) go.AddComponent<BoxCollider2D>().size = new Vector2(w, h);
            return go;
        }

        private static GameObject Shape(string name, Transform parent, Vector2 worldCenter, Vector2 size, Sprite sprite, Color color, int order)
        {
            var go = Go(name, parent, worldCenter);
            AddSprite(go, sprite, color, order);
            go.transform.localScale = new Vector3(size.x, size.y, 1f);
            return go;
        }

        private static RectTransform UIRect(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 pos, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.layer = LayerMask.NameToLayer("UI");
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = pivot;
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            return rt;
        }

        private static RectTransform Stretch(string name, Transform parent)
        {
            return UIRect(name, parent, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
        }

        private static Image UIImage(RectTransform rt, Color color, Sprite sprite = null)
        {
            var img = rt.gameObject.AddComponent<Image>();
            img.color = color;
            img.sprite = sprite;
            img.raycastTarget = false;
            if (sprite != null && sprite.border != Vector4.zero) img.type = Image.Type.Sliced;
            return img;
        }

        private static Text UIText(RectTransform rt, string text, int size, TextAnchor align, Color color, FontStyle style = FontStyle.Normal, bool shadow = true)
        {
            var t = rt.gameObject.AddComponent<Text>();
            t.font = UIFont;
            t.text = text;
            t.fontSize = size;
            t.alignment = align;
            t.color = color;
            t.fontStyle = style;
            t.raycastTarget = false;
            t.supportRichText = true;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            if (shadow)
            {
                var s = rt.gameObject.AddComponent<Shadow>();
                s.effectColor = new Color(0f, 0f, 0f, 0.7f);
                s.effectDistance = new Vector2(2f, -2f);
            }
            return t;
        }

        private static Button MenuButton(Transform parent, string name, string label, Vector2 anchor, Vector2 pos, Vector2 size, int fontSize = 28)
        {
            var rt = UIRect(name, parent, anchor, anchor, new Vector2(0.5f, 0.5f), pos, size);
            var img = UIImage(rt, tiles != null ? Color.white : new Color(0.15f, 0.12f, 0.1f, 0.85f), tiles != null ? tiles.uiWood : null);
            img.raycastTarget = true;
            var button = rt.gameObject.AddComponent<Button>();
            button.targetGraphic = img;
            var colors = button.colors;
            colors.normalColor = new Color(0.85f, 0.85f, 0.85f);
            colors.highlightedColor = Color.white;
            colors.selectedColor = new Color(1f, 0.95f, 0.75f);
            colors.pressedColor = new Color(0.7f, 0.7f, 0.7f);
            colors.disabledColor = new Color(0.4f, 0.4f, 0.4f, 0.7f);
            button.colors = colors;
            var textRt = Stretch("Label", rt);
            textRt.offsetMin = new Vector2(10f, 4f);
            textRt.offsetMax = new Vector2(-10f, -4f);
            var text = UIText(textRt, label, fontSize, TextAnchor.MiddleCenter, Color.white, FontStyle.Bold);
            rt.gameObject.AddComponent<MenuButtonFx>().Setup(text);
            return button;
        }

        private static void EnsureEventSystem(Transform parent)
        {
            var go = new GameObject("EventSystem");
            go.transform.SetParent(parent, false);
            go.AddComponent<UnityEngine.EventSystems.EventSystem>();
            go.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        }

        private static CanvasGroup Group(RectTransform rt, float alpha)
        {
            var g = rt.gameObject.AddComponent<CanvasGroup>();
            g.alpha = alpha;
            g.blocksRaycasts = false;
            g.interactable = false;
            return g;
        }

        private static Canvas MakeCanvas(string name, int order)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.layer = LayerMask.NameToLayer("UI");
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = order;
            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            go.AddComponent<GraphicRaycaster>();
            return canvas;
        }

        private static ParticleSystem MakeParticles(string name, Transform parent, Color color, int burst, float speed, float lifetime, float size, bool worldSpace, float radius, float gravity = 0f)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var ps = go.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            var main = ps.main;
            main.playOnAwake = false;
            main.loop = false;
            main.duration = 0.5f;
            main.startLifetime = new ParticleSystem.MinMaxCurve(lifetime * 0.6f, lifetime);
            main.startSpeed = new ParticleSystem.MinMaxCurve(speed * 0.4f, speed);
            main.startSize = new ParticleSystem.MinMaxCurve(size * 0.5f, size);
            main.startColor = color;
            main.gravityModifier = gravity;
            main.simulationSpace = worldSpace ? ParticleSystemSimulationSpace.World : ParticleSystemSimulationSpace.Local;
            main.maxParticles = 400;

            var emission = ps.emission;
            emission.rateOverTime = 0f;
            if (burst > 0) emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)burst) });

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = radius;

            var col = ps.colorOverLifetime;
            col.enabled = true;
            var grad = new Gradient();
            grad.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) });
            col.color = grad;

            var sol = ps.sizeOverLifetime;
            sol.enabled = true;
            sol.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, 0f));

            var r = go.GetComponent<ParticleSystemRenderer>();
            r.sharedMaterial = particleMaterial;
            r.sortingOrder = 40;
            return ps;
        }
    }
}

using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace ButterflyStep
{
    public class OptionsPanel : MonoBehaviour
    {
        public static bool IsOpen { get; private set; }
        public static int ClosedFrame { get; private set; } = -1;

        private Action onClose;
        private CanvasGroup blocked;
        private Font font;
        private Sprite buttonSprite;
        private Color labelColor = new Color(0.3f, 0.18f, 0.08f);
        private float openedAt;

        public static OptionsPanel Open(Transform canvas, Button template, CanvasGroup behind, Action closed)
        {
            var go = new GameObject("PainelOpcoes", typeof(RectTransform));
            var rt = (RectTransform)go.transform;
            rt.SetParent(canvas, false);
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            rt.SetAsLastSibling();
            var panel = go.AddComponent<OptionsPanel>();
            panel.onClose = closed;
            panel.blocked = behind;
            var templateText = template != null ? template.GetComponentInChildren<Text>() : null;
            panel.font = templateText != null ? templateText.font : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            panel.buttonSprite = template != null && template.image != null ? template.image.sprite : null;
            panel.Build(FindSprite(canvas, "parchment"));
            return panel;
        }

        private static Sprite FindSprite(Transform root, string name)
        {
            foreach (var img in root.GetComponentsInChildren<Image>(true))
                if (img.sprite != null && img.sprite.name == name) return img.sprite;
            return null;
        }

        private void Build(Sprite parchment)
        {
            IsOpen = true;
            openedAt = Time.unscaledTime;
            if (blocked != null) blocked.interactable = false;

            var dim = gameObject.AddComponent<Image>();
            dim.color = new Color(0f, 0f, 0f, 0.55f);

            var box = Rect("Caixa", transform, Vector2.zero, new Vector2(1000f, 760f));
            var boxImg = box.gameObject.AddComponent<Image>();
            boxImg.sprite = parchment;
            boxImg.type = parchment != null && parchment.border.sqrMagnitude > 0f ? Image.Type.Sliced : Image.Type.Simple;
            boxImg.color = parchment != null ? Color.white : new Color(0.12f, 0.09f, 0.07f, 0.95f);
            if (parchment == null) labelColor = Color.white;

            Label(box, "OPÇÕES", new Vector2(0f, 310f), new Vector2(800f, 80f), 58, FontStyle.Normal);

            var music = SliderRow(box, "Música", new Vector2(0f, 180f), GameSettings.Music, v => GameSettings.Music = v);
            var sfx = SliderRow(box, "Efeitos sonoros", new Vector2(0f, 80f), GameSettings.Sfx, v => { GameSettings.Sfx = v; GameAudio.Play(ButterflyStep.Sfx.UiMove, 0f); });
            var full = ToggleRow(box, "Tela cheia", new Vector2(0f, -30f), () => GameSettings.Fullscreen, v => GameSettings.Fullscreen = v);
            var shake = ToggleRow(box, "Tremor de tela", new Vector2(0f, -120f), () => GameSettings.ScreenShake, v => GameSettings.ScreenShake = v);
            var hints = ToggleRow(box, "Avisos na tela", new Vector2(0f, -210f), () => GameSettings.Hints, v => GameSettings.Hints = v);
            var back = Button(box, "Voltar", new Vector2(0f, -310f), new Vector2(320f, 70f), Close);

            Chain(music, sfx, full, shake, hints, back);
            if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(music.gameObject);
        }

        private void Update()
        {
            if (Time.unscaledTime - openedAt < 0.2f) return;
            bool cancel = (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame) ||
                          (Gamepad.current != null && (Gamepad.current.buttonEast.wasPressedThisFrame || Gamepad.current.startButton.wasPressedThisFrame));
            if (cancel) Close();
        }

        private void LateUpdate()
        {
            if (EventSystem.current == null) return;
            var selected = EventSystem.current.currentSelectedGameObject;
            if (selected == null || !selected.transform.IsChildOf(transform))
            {
                var first = GetComponentInChildren<Selectable>();
                if (first != null) EventSystem.current.SetSelectedGameObject(first.gameObject);
            }
        }

        public void Close()
        {
            if (!IsOpen) return;
            ClosedFrame = Time.frameCount;
            IsOpen = false;
            GameAudio.Play(ButterflyStep.Sfx.UiMove, 0f);
            if (blocked != null) blocked.interactable = true;
            onClose?.Invoke();
            Destroy(gameObject);
        }

        private void OnDestroy()
        {
            if (IsOpen) IsOpen = false;
        }

        private static void Chain(params Selectable[] items)
        {
            for (int i = 0; i < items.Length; i++)
            {
                var nav = items[i].navigation;
                nav.mode = Navigation.Mode.Explicit;
                nav.selectOnUp = items[(i + items.Length - 1) % items.Length];
                nav.selectOnDown = items[(i + 1) % items.Length];
                if (!(items[i] is Slider))
                {
                    nav.selectOnLeft = items[i];
                    nav.selectOnRight = items[i];
                }
                items[i].navigation = nav;
            }
        }

        private Slider SliderRow(RectTransform parent, string label, Vector2 pos, float value, Action<float> changed)
        {
            var text = Label(parent, label, pos + new Vector2(-230f, 0f), new Vector2(380f, 60f), 36, FontStyle.Normal);
            text.alignment = TextAnchor.MiddleLeft;
            var percent = Label(parent, Mathf.RoundToInt(value * 100f) + "%", pos + new Vector2(390f, 0f), new Vector2(120f, 60f), 34, FontStyle.Normal);

            var root = Rect(label, parent, pos + new Vector2(130f, 0f), new Vector2(360f, 36f));
            var bg = root.gameObject.AddComponent<Image>();
            bg.color = new Color(0.25f, 0.16f, 0.1f, 0.85f);
            var fillArea = Rect("Area", root, Vector2.zero, Vector2.zero);
            Stretch(fillArea, 6f);
            var fill = Rect("Preenchimento", fillArea, Vector2.zero, Vector2.zero);
            fill.gameObject.AddComponent<Image>().color = new Color(1f, 0.78f, 0.3f);
            var handleArea = Rect("AreaAlca", root, Vector2.zero, Vector2.zero);
            Stretch(handleArea, 12f);
            var handle = Rect("Alca", handleArea, Vector2.zero, new Vector2(30f, 52f));
            var handleImg = handle.gameObject.AddComponent<Image>();
            handleImg.color = new Color(1f, 0.95f, 0.8f);

            var slider = root.gameObject.AddComponent<Slider>();
            slider.fillRect = fill;
            slider.handleRect = handle;
            slider.targetGraphic = handleImg;
            slider.direction = Slider.Direction.LeftToRight;
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = value;
            var colors = slider.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1f, 0.85f, 0.5f);
            colors.selectedColor = new Color(1f, 0.75f, 0.3f);
            slider.colors = colors;
            slider.onValueChanged.AddListener(v =>
            {
                percent.text = Mathf.RoundToInt(v * 100f) + "%";
                changed(v);
            });
            return slider;
        }

        private Button ToggleRow(RectTransform parent, string label, Vector2 pos, Func<bool> get, Action<bool> set)
        {
            bool state = get();
            Button button = null;
            button = Button(parent, "", pos, new Vector2(620f, 70f), () =>
            {
                state = !state;
                set(state);
                Refresh(button, label, state);
            });
            Refresh(button, label, state);
            return button;
        }

        private static void Refresh(Button button, string label, bool on)
        {
            var t = button.GetComponentInChildren<Text>();
            if (t != null) t.text = $"{label}:  {(on ? "SIM" : "NÃO")}";
        }

        private Button Button(RectTransform parent, string label, Vector2 pos, Vector2 size, Action click)
        {
            var rt = Rect(label, parent, pos, size);
            var img = rt.gameObject.AddComponent<Image>();
            img.sprite = buttonSprite;
            img.type = buttonSprite != null && buttonSprite.border.sqrMagnitude > 0f ? Image.Type.Sliced : Image.Type.Simple;
            img.color = buttonSprite != null ? Color.white : new Color(0.4f, 0.26f, 0.14f);
            var button = rt.gameObject.AddComponent<Button>();
            button.targetGraphic = img;
            var colors = button.colors;
            colors.highlightedColor = new Color(1f, 0.88f, 0.6f);
            colors.selectedColor = new Color(1f, 0.8f, 0.45f);
            button.colors = colors;
            button.onClick.AddListener(() => { GameAudio.Play(ButterflyStep.Sfx.UiSelect, 0f); click(); });
            var text = Label(rt, label, Vector2.zero, size, 32, FontStyle.Normal);
            text.color = Color.white;
            text.gameObject.AddComponent<Shadow>().effectDistance = new Vector2(2f, -2f);
            return button;
        }

        private Text Label(RectTransform parent, string value, Vector2 pos, Vector2 size, int fontSize, FontStyle style)
        {
            var t = Rect("Texto", parent, pos, size).gameObject.AddComponent<Text>();
            t.font = font;
            t.text = value;
            t.fontSize = Mathf.RoundToInt(fontSize * 1.25f);
            t.fontStyle = style;
            t.color = labelColor;
            t.alignment = TextAnchor.MiddleCenter;
            t.horizontalOverflow = HorizontalWrapMode.Overflow;
            t.raycastTarget = false;
            return t;
        }

        private static RectTransform Rect(string name, Transform parent, Vector2 pos, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            return rt;
        }

        private static void Stretch(RectTransform rt, float inset)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(inset, 0f);
            rt.offsetMax = new Vector2(-inset, 0f);
        }
    }
}

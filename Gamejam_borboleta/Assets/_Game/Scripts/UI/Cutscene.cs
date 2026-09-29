using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ButterflyStep
{
    [Serializable]
    public class CutsceneArt
    {
        public Font font;
        public Sprite[] ecoIdle = new Sprite[0];
        public Sprite[] ecoWalk = new Sprite[0];
        public Sprite[] ecoVanish = new Sprite[0];
        public Sprite[] cronoRun = new Sprite[0];
        public Sprite[] timeWave = new Sprite[0];
        public Sprite[] burst = new Sprite[0];
        public Sprite[] sparkle = new Sprite[0];
        public Sprite[] butterflies = new Sprite[0];
        public Sprite[] birds = new Sprite[0];
        public Sprite[] farTrees = new Sprite[0];
        public Sprite[] pines = new Sprite[0];
        public Sprite clock, swirl, clouds, mountDark, mountLight, groundFill, grassTop;
    }

    public class Cutscene : MonoBehaviour
    {
        [SerializeField] private bool ending;
        [SerializeField] private string nextScene = "Level01";
        [SerializeField] private CutsceneArt art = new CutsceneArt();
        [SerializeField] private string[] developers = new string[0];

        private static readonly Color HomeSky = new Color(1f, 0.82f, 0.6f);
        private static readonly Color ValleySky = new Color(0.72f, 0.92f, 1f);
        private static readonly Color DarkSky = new Color(0.28f, 0.16f, 0.4f);
        private static readonly Color Gold = new Color(1f, 0.85f, 0.35f);
        private static readonly Color[] Seasons =
        {
            new Color(0.72f, 0.95f, 0.85f), new Color(1f, 0.9f, 0.6f), new Color(1f, 0.68f, 0.45f), new Color(0.8f, 0.88f, 1f)
        };

        private RectTransform stage, home, valley, fx;
        private Image sky, flash, fade, eco, clock, crono;
        private Text caption, title;
        private CanvasGroup captionGroup, finalCard;
        private bool finished, leaving, onFinalCard;
        private float startedAt;
        private Coroutine main;
        private readonly List<RectTransform> shaking = new List<RectTransform>();

        public void Setup(bool isEnding, string next, CutsceneArt cutsceneArt, string[] devs)
        {
            ending = isEnding;
            nextScene = next;
            art = cutsceneArt;
            developers = devs;
        }

        private void Start()
        {
            Time.timeScale = 1f;
            startedAt = Time.unscaledTime;
            BuildCanvas();
            main = StartCoroutine(ending ? Outro() : Intro());
        }

        private void Update()
        {
            if (leaving || Time.unscaledTime - startedAt < 0.6f) return;
            if (!Pressed()) return;
            if (ending && !onFinalCard)
            {
                if (main != null) StopCoroutine(main);
                StopAllCoroutines();
                StartCoroutine(FinalCard(true));
                return;
            }
            if (!ending || finished) StartCoroutine(Leave());
        }

        private static bool Pressed()
        {
            var k = Keyboard.current;
            if (k != null && (k.enterKey.wasPressedThisFrame || k.spaceKey.wasPressedThisFrame || k.escapeKey.wasPressedThisFrame || k.numpadEnterKey.wasPressedThisFrame)) return true;
            var m = Mouse.current;
            if (m != null && m.leftButton.wasPressedThisFrame) return true;
            var g = Gamepad.current;
            return g != null && (g.buttonSouth.wasPressedThisFrame || g.startButton.wasPressedThisFrame);
        }

        private IEnumerator Leave()
        {
            leaving = true;
            yield return Tween(0.8f, t => fade.color = new Color(0f, 0f, 0f, t));
            SceneManager.LoadScene(nextScene);
        }

        private IEnumerator Intro()
        {
            GameAudio.PlayMusic(Season.Primavera);
            ShowHome(true);
            clock = Img("Relogio", fx, art.clock, new Vector2(-620f, 60f), new Vector2(190f, 190f), Color.white);
            clock.gameObject.AddComponent<UIBob>();
            eco = Actor("Eco", art.ecoWalk, 9f, new Vector2(-620f, -262f));
            yield return Tween(1.2f, t => fade.color = new Color(0f, 0f, 0f, 1f - t));

            StartCoroutine(Caption("Eco vivia no fluxo dos dias, guiado pelo seu Relógio."));
            StartCoroutine(CycleSky(Seasons, 4.8f));
            yield return Tween(5f, t =>
            {
                float x = Mathf.Lerp(-620f, -160f, t);
                eco.rectTransform.anchoredPosition = new Vector2(x, -262f);
                clock.rectTransform.anchoredPosition = new Vector2(x, 60f);
                home.anchoredPosition = new Vector2(-t * 260f, 0f);
            });
            SetFrames(eco, art.ecoIdle, 3f);
            yield return Wait(0.6f);

            StartCoroutine(Caption("Até que o Cronófago, a criatura que devora estações, cruzou o seu caminho."));
            var swirl = Img("Fenda", fx, art.swirl, new Vector2(620f, -60f), new Vector2(10f, 10f), new Color(0.8f, 0.5f, 1f, 0.9f));
            swirl.gameObject.AddComponent<UISpin>().speed = -120f;
            StartCoroutine(Tween(1.2f, t => sky.color = Color.Lerp(HomeSky, DarkSky, t)));
            yield return Tween(1f, t => swirl.rectTransform.sizeDelta = Vector2.one * Mathf.Lerp(10f, 520f, Smooth(t)));
            crono = Actor("Cronofago", art.cronoRun, 12f, new Vector2(1150f, -270f), new Vector2(576f, 384f));
            crono.color = new Color(0.72f, 0.45f, 0.85f);
            GameAudio.Play(Sfx.Stomp, 0f);
            yield return Tween(1.6f, t => crono.rectTransform.anchoredPosition = new Vector2(Mathf.Lerp(1150f, 330f, Smooth(t)), -270f));
            StartCoroutine(Tween(0.6f, t => swirl.color = new Color(0.8f, 0.5f, 1f, 0.9f * (1f - t))));
            StartCoroutine(Shake(0.5f, 14f));
            yield return Wait(1.6f);

            StartCoroutine(Caption("Com um só golpe, o Relógio se partiu em dez fragmentos..."));
            yield return Tween(0.35f, t => crono.rectTransform.anchoredPosition = new Vector2(Mathf.Lerp(330f, 60f, t * t), -270f));
            GameAudio.Play(Sfx.BossHit, 0f);
            StartCoroutine(Flash(0.5f));
            StartCoroutine(Shake(0.6f, 24f));
            PlayOnce(art.burst, clock.rectTransform.anchoredPosition, 420f, 40f, Gold);
            Shatter(clock.rectTransform.anchoredPosition);
            clock.gameObject.SetActive(false);
            GameAudio.Play(Sfx.Erase, 0f);
            yield return Tween(0.5f, t => crono.rectTransform.anchoredPosition = new Vector2(Mathf.Lerp(60f, 330f, Smooth(t)), -270f));
            yield return Wait(2.2f);

            StartCoroutine(Caption("...e Eco foi arrastado para um tempo que não é o seu."));
            GameAudio.Play(Sfx.TimeBack, 0f);
            var vortex = Img("Vortex", fx, art.swirl, eco.rectTransform.anchoredPosition + new Vector2(0f, 120f), new Vector2(10f, 10f), new Color(0.75f, 0.9f, 1f, 0.95f));
            vortex.gameObject.AddComponent<UISpin>().speed = 360f;
            vortex.transform.SetAsFirstSibling();
            PlayOnce(art.timeWave, eco.rectTransform.anchoredPosition + new Vector2(0f, 120f), 700f, 30f, Color.white);
            StartCoroutine(Tween(1f, t => crono.color = new Color(0.72f, 0.45f, 0.85f, 1f - t)));
            yield return Tween(1f, t => vortex.rectTransform.sizeDelta = Vector2.one * Mathf.Lerp(10f, 560f, Smooth(t)));
            SetFrames(eco, art.ecoVanish, 10f, false);
            var start = eco.rectTransform.anchoredPosition;
            yield return Tween(1.6f, t =>
            {
                eco.rectTransform.localEulerAngles = new Vector3(0f, 0f, t * 720f);
                eco.rectTransform.localScale = Vector3.one * (1f - t);
                eco.rectTransform.anchoredPosition = Vector2.Lerp(start, start + new Vector2(0f, 120f), t);
            });
            yield return Flash(1.2f, 1f);

            ShowHome(false);
            vortex.gameObject.SetActive(false);
            crono.gameObject.SetActive(false);
            sky.color = ValleySky;
            eco.rectTransform.localEulerAngles = Vector3.zero;
            eco.rectTransform.localScale = Vector3.one;
            eco.rectTransform.anchoredPosition = new Vector2(-200f, -262f);
            SetFrames(eco, art.ecoIdle, 3f);
            GameAudio.Play(Sfx.Land, 0f);
            StartCoroutine(Caption("Preso num vale desconhecido, Eco precisa reunir os fragmentos para voltar para casa."));
            for (int i = 0; i < 6; i++) Butterfly(new Vector2(UnityEngine.Random.Range(-800f, 800f), UnityEngine.Random.Range(-150f, 300f)), 7f);
            yield return Wait(4.2f);

            StartCoroutine(Caption("Mas cuidado: aqui, cada pequeno gesto ecoa no futuro."));
            SetFrames(eco, art.ecoWalk, 9f);
            StartCoroutine(Tween(3f, t => eco.rectTransform.anchoredPosition = new Vector2(Mathf.Lerp(-200f, 260f, t), -262f)));
            yield return Wait(1.2f);
            title.text = "BUTTERFLY STEP";
            yield return Tween(1.2f, t => title.color = new Color(1f, 0.95f, 0.8f, t));
            SetFrames(eco, art.ecoIdle, 3f);
            yield return Wait(2.2f);
            finished = true;
            StartCoroutine(Leave());
        }

        private IEnumerator Outro()
        {
            GameAudio.PlayMusic(Season.Primavera);
            ShowHome(false);
            sky.color = new Color(0.45f, 0.5f, 0.75f);
            eco = Actor("Eco", art.ecoIdle, 3f, new Vector2(-120f, -262f));
            yield return Tween(1.2f, t => fade.color = new Color(0f, 0f, 0f, 1f - t));

            StartCoroutine(Caption("O último fragmento volta ao seu lugar."));
            var center = new Vector2(-120f, 110f);
            var pieces = new List<Image>();
            for (int i = 0; i < 10; i++)
            {
                float a = i * Mathf.PI * 2f / 10f;
                var from = new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * 1300f;
                var piece = Img("Fragmento", fx, art.clock, from, new Vector2(56f, 56f), Gold);
                pieces.Add(piece);
                StartCoroutine(FlyTo(piece.rectTransform, from, center + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * 30f, 2f + i * 0.12f, 540f));
            }
            yield return Wait(3.3f);
            foreach (var p in pieces) p.gameObject.SetActive(false);
            GameAudio.Play(Sfx.Complete, 0f);
            StartCoroutine(Flash(0.6f));
            clock = Img("Relogio", fx, art.clock, center, new Vector2(180f, 180f), Color.white);
            clock.gameObject.AddComponent<UIBob>();
            PlayOnce(art.burst, center, 480f, 40f, Gold);

            StartCoroutine(Caption("O Relógio de Eco volta a bater."));
            for (int i = 0; i < 3; i++)
            {
                PlayOnce(art.timeWave, center, 500f + i * 200f, 30f, new Color(1f, 1f, 1f, 0.8f));
                GameAudio.Play(Sfx.TimeForward, 0f);
                yield return Wait(0.8f);
            }
            yield return CycleSky(new[] { Seasons[0], Seasons[1], Seasons[2], Seasons[3], ValleySky }, 2.4f);

            StartCoroutine(Caption("O vale que ele deixa já não é o mesmo: a muda virou floresta, o pássaro virou um bando."));
            var grow = new List<RectTransform>();
            for (int i = 0; i < 5; i++)
            {
                var s = art.pines.Length > 0 ? art.pines[i % art.pines.Length] : null;
                var tree = Img("Arvore Nova", valley, s, new Vector2(-760f + i * 380f + (i % 2) * 60f, -262f), NativeSize(s, 1.9f), Color.white);
                tree.rectTransform.pivot = new Vector2(0.5f, 0f);
                tree.rectTransform.localScale = new Vector3(1f, 0f, 1f);
                tree.transform.SetSiblingIndex(valley.childCount - 3);
                grow.Add(tree.rectTransform);
            }
            for (int i = 0; i < grow.Count; i++)
            {
                var g = grow[i];
                StartCoroutine(Tween(1.1f, t => g.localScale = new Vector3(1f, Smooth(t), 1f)));
                GameAudio.Play(Sfx.Restore, 0.1f);
                yield return Wait(0.35f);
            }
            for (int i = 0; i < 7; i++) StartCoroutine(Bird(i * 0.25f, 380f + (i % 3) * 70f - i * 12f));
            yield return Wait(3.4f);

            StartCoroutine(Caption("Eco atravessa a fenda e retorna para a sua linha do tempo."));
            var portal = Img("Fenda", fx, art.swirl, new Vector2(620f, -80f), new Vector2(10f, 10f), new Color(1f, 0.9f, 0.6f, 0.95f));
            portal.gameObject.AddComponent<UISpin>().speed = 200f;
            portal.transform.SetAsFirstSibling();
            yield return Tween(0.9f, t => portal.rectTransform.sizeDelta = Vector2.one * Mathf.Lerp(10f, 480f, Smooth(t)));
            SetFrames(eco, art.ecoWalk, 9f);
            var clockStart = clock.rectTransform.anchoredPosition;
            yield return Tween(2.6f, t =>
            {
                float x = Mathf.Lerp(-120f, 600f, t);
                eco.rectTransform.anchoredPosition = new Vector2(x, -262f);
                clock.rectTransform.anchoredPosition = new Vector2(x, clockStart.y);
            });
            SetFrames(eco, art.ecoVanish, 10f, false);
            GameAudio.Play(Sfx.TimeForward, 0f);
            yield return Wait(0.8f);
            yield return Flash(1f, 1f);

            portal.gameObject.SetActive(false);
            foreach (var g in grow) g.gameObject.SetActive(false);
            ShowHome(true);
            home.anchoredPosition = Vector2.zero;
            sky.color = HomeSky;
            eco.rectTransform.anchoredPosition = new Vector2(-640f, -262f);
            clock.rectTransform.anchoredPosition = new Vector2(-640f, 60f);
            clock.rectTransform.sizeDelta = new Vector2(150f, 150f);
            SetFrames(eco, art.ecoWalk, 9f);
            StartCoroutine(Caption("Em casa, os dias voltam a correr como devem."));
            for (int i = 0; i < 4; i++) Butterfly(new Vector2(UnityEngine.Random.Range(-700f, 700f), UnityEngine.Random.Range(-100f, 300f)), 6f);
            yield return Tween(4.5f, t =>
            {
                float x = Mathf.Lerp(-640f, -80f, t);
                eco.rectTransform.anchoredPosition = new Vector2(x, -262f);
                clock.rectTransform.anchoredPosition = new Vector2(x, 60f);
                home.anchoredPosition = new Vector2(-t * 220f, 0f);
            });
            SetFrames(eco, art.ecoIdle, 3f);
            yield return Wait(1f);
            yield return FinalCard(false);
        }

        private IEnumerator FinalCard(bool instant)
        {
            onFinalCard = true;
            captionGroup.alpha = 0f;
            fade.color = Color.clear;
            flash.color = Color.clear;
            finalCard.gameObject.SetActive(true);
            if (!instant) yield return Tween(1.4f, t => finalCard.alpha = t);
            finalCard.alpha = 1f;
            yield return Wait(0.8f);
            finished = true;
        }

        private void BuildCanvas()
        {
            var canvasGo = new GameObject("CutsceneCanvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            var cam = Camera.main;
            canvas.renderMode = cam != null ? RenderMode.ScreenSpaceCamera : RenderMode.ScreenSpaceOverlay;
            canvas.worldCamera = cam;
            canvas.planeDistance = 5f;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            stage = Rt("Stage", canvasGo.transform, Vector2.zero, new Vector2(1920f, 1080f));
            var mask = stage.gameObject.AddComponent<RectMask2D>();
            mask.enabled = true;
            sky = Img("Ceu", stage, art.clouds, Vector2.zero, new Vector2(2200f, 1240f), HomeSky);
            sky.preserveAspect = false;

            home = Rt("Casa", stage, Vector2.zero, new Vector2(1920f, 1080f));
            Row(home, art.mountDark, 16, new Vector2(-1200f, -262f), 2f, new Color(0.95f, 0.75f, 0.7f));
            Row(home, art.farTrees, 18, new Vector2(-1200f, -262f), 1.5f, new Color(1f, 0.85f, 0.7f));
            Ground(home, new Color(1f, 0.9f, 0.75f));

            valley = Rt("Vale", stage, Vector2.zero, new Vector2(1920f, 1080f));
            Row(valley, art.mountLight, 16, new Vector2(-1200f, -262f), 2f, Color.white);
            Row(valley, art.pines, 14, new Vector2(-1100f, -262f), 1.4f, new Color(0.8f, 0.95f, 0.85f));
            Ground(valley, Color.white);

            fx = Rt("Cena", stage, Vector2.zero, new Vector2(1920f, 1080f));

            title = Txt("Titulo", stage, "", new Vector2(0f, 250f), new Vector2(1600f, 200f), 120, new Color(1f, 0.95f, 0.8f, 0f));
            title.gameObject.AddComponent<Outline>().effectDistance = new Vector2(5f, -5f);

            var capBox = Rt("Legenda", stage, new Vector2(0f, -440f), new Vector2(1920f, 200f));
            var capBg = capBox.gameObject.AddComponent<Image>();
            capBg.color = new Color(0f, 0f, 0f, 0.62f);
            captionGroup = capBox.gameObject.AddComponent<CanvasGroup>();
            captionGroup.alpha = 0f;
            caption = Txt("Texto", capBox, "", Vector2.zero, new Vector2(1700f, 180f), 44, Color.white);

            var skip = Txt("Pular", stage, "ENTER: pular", new Vector2(760f, 500f), new Vector2(400f, 60f), 26, new Color(1f, 1f, 1f, 0.7f));
            skip.alignment = TextAnchor.MiddleRight;

            BuildFinalCard();

            flash = Img("Flash", stage, null, Vector2.zero, new Vector2(2200f, 1300f), new Color(1f, 1f, 1f, 0f));
            flash.raycastTarget = false;
            fade = Img("Fade", stage, null, Vector2.zero, new Vector2(2200f, 1300f), Color.black);
            fade.raycastTarget = false;
        }

        private void BuildFinalCard()
        {
            var card = Rt("Final", stage, Vector2.zero, new Vector2(1920f, 1080f));
            card.gameObject.AddComponent<Image>().color = new Color(0.06f, 0.04f, 0.12f, 0.96f);
            finalCard = card.gameObject.AddComponent<CanvasGroup>();
            finalCard.alpha = 0f;
            Txt("Obrigado", card, "Obrigado por jogar!", new Vector2(0f, 360f), new Vector2(1600f, 120f), 86, Gold);
            Txt("Frase", card, "“Toda escolha deixa uma marca no futuro.”", new Vector2(0f, 260f), new Vector2(1600f, 70f), 40, new Color(0.85f, 0.85f, 1f));
            Txt("Titulo Devs", card, "DESENVOLVIDO POR", new Vector2(0f, 150f), new Vector2(1200f, 60f), 34, new Color(1f, 1f, 1f, 0.7f));
            Txt("Devs", card, string.Join("\n", developers), new Vector2(0f, 20f), new Vector2(1400f, 200f), 46, Color.white);
            Txt("Cimatec", card, "Agradecemos ao SENAI CIMATEC pela Game Jam!", new Vector2(0f, -150f), new Vector2(1600f, 70f), 38, new Color(0.7f, 1f, 0.8f));
            Txt("Penzilla", card, "Graphics created by Penzilla Design · arte Legacy Fantasy, Craftpix e CC0", new Vector2(0f, -240f), new Vector2(1700f, 60f), 26, new Color(1f, 1f, 1f, 0.6f));
            var hint = Txt("Voltar", card, "Pressione ENTER para voltar ao menu", new Vector2(0f, -380f), new Vector2(1200f, 60f), 32, Gold);
            hint.gameObject.AddComponent<UIPulse>();
            card.gameObject.SetActive(false);
        }

        private void ShowHome(bool atHome)
        {
            home.gameObject.SetActive(atHome);
            valley.gameObject.SetActive(!atHome);
            sky.color = atHome ? HomeSky : ValleySky;
        }

        private void Row(RectTransform parent, Sprite sprite, int count, Vector2 start, float scale, Color tint)
        {
            Row(parent, sprite != null ? new[] { sprite } : new Sprite[0], count, start, scale, tint);
        }

        private void Row(RectTransform parent, Sprite[] sprites, int count, Vector2 start, float scale, Color tint)
        {
            if (sprites == null || sprites.Length == 0) return;
            float x = start.x;
            for (int i = 0; i < count; i++)
            {
                var s = sprites[i % sprites.Length];
                if (s == null) continue;
                var size = NativeSize(s, scale);
                var img = Img(s.name, parent, s, new Vector2(x + size.x * 0.5f, start.y), size, tint);
                img.rectTransform.pivot = new Vector2(0.5f, 0f);
                x += size.x * 0.92f;
            }
        }

        private void Ground(RectTransform parent, Color tint)
        {
            var fill = Img("Chao", parent, art.groundFill, new Vector2(0f, -420f), new Vector2(3000f, 320f), tint);
            fill.type = Image.Type.Tiled;
            fill.pixelsPerUnitMultiplier = 0.8f;
            if (art.grassTop != null)
            {
                var grass = Img("Grama", parent, art.grassTop, new Vector2(0f, -268f), new Vector2(3000f, 60f), tint);
                grass.type = Image.Type.Tiled;
                grass.pixelsPerUnitMultiplier = 0.5f;
            }
        }

        private static Vector2 NativeSize(Sprite s, float scale) => s == null ? Vector2.one * 100f : new Vector2(s.rect.width, s.rect.height) * scale;

        private Image Actor(string name, Sprite[] frames, float fps, Vector2 pos, Vector2? size = null)
        {
            var img = Img(name, fx, frames != null && frames.Length > 0 ? frames[0] : null, pos, size ?? new Vector2(256f, 256f), Color.white);
            img.rectTransform.pivot = new Vector2(0.5f, 0f);
            var anim = img.gameObject.AddComponent<UIFrames>();
            anim.Play(frames, fps, true);
            return img;
        }

        private static void SetFrames(Image img, Sprite[] frames, float fps, bool loop = true)
        {
            var anim = img.GetComponent<UIFrames>();
            if (anim != null) anim.Play(frames, fps, loop);
        }

        private void PlayOnce(Sprite[] frames, Vector2 pos, float size, float fps, Color tint)
        {
            if (frames == null || frames.Length == 0) return;
            var img = Img("Efeito", fx, frames[0], pos, Vector2.one * size, tint);
            img.gameObject.AddComponent<UIFrames>().Play(frames, fps, false, true);
        }

        private void Shatter(Vector2 origin)
        {
            for (int i = 0; i < 10; i++)
            {
                float a = i * Mathf.PI * 2f / 10f + UnityEngine.Random.Range(-0.2f, 0.2f);
                var dir = new Vector2(Mathf.Cos(a), Mathf.Sin(a));
                var piece = Img("Fragmento", fx, art.clock, origin, new Vector2(56f, 56f), Gold);
                StartCoroutine(FlyTo(piece.rectTransform, origin, origin + dir * 1400f, 1.6f, UnityEngine.Random.Range(-900f, 900f)));
            }
        }

        private void Butterfly(Vector2 pos, float life)
        {
            if (art.butterflies.Length == 0) return;
            var img = Img("Borboleta", fx, art.butterflies[UnityEngine.Random.Range(0, art.butterflies.Length)], pos, new Vector2(64f, 64f), Color.white);
            StartCoroutine(Flutter(img.rectTransform, life));
        }

        private IEnumerator Flutter(RectTransform r, float life)
        {
            var origin = r.anchoredPosition;
            float seed = UnityEngine.Random.value * 10f;
            for (float t = 0f; t < life; t += Dt)
            {
                r.anchoredPosition = origin + new Vector2(Mathf.Sin((t + seed) * 0.9f) * 160f + t * 30f, Mathf.Sin((t + seed) * 2.1f) * 50f + t * 20f);
                r.localScale = new Vector3(Mathf.Abs(Mathf.Sin((t + seed) * 14f)) * 0.8f + 0.2f, 1f, 1f);
                yield return null;
            }
            Destroy(r.gameObject);
        }

        private IEnumerator Bird(float delay, float y)
        {
            yield return Wait(delay);
            if (art.birds.Length == 0) yield break;
            var img = Img("Passaro", fx, art.birds[0], new Vector2(-1100f, y), new Vector2(110f, 110f), new Color(0.25f, 0.2f, 0.3f));
            img.gameObject.AddComponent<UIFrames>().Play(art.birds, 6f, true);
            yield return Tween(4.5f, t => img.rectTransform.anchoredPosition = new Vector2(Mathf.Lerp(-1100f, 1100f, t), y + Mathf.Sin(t * 12f) * 18f));
            Destroy(img.gameObject);
        }

        private IEnumerator FlyTo(RectTransform r, Vector2 from, Vector2 to, float duration, float spin)
        {
            for (float t = 0f; t < duration; t += Dt)
            {
                float k = Smooth(t / duration);
                r.anchoredPosition = Vector2.Lerp(from, to, k);
                r.localEulerAngles = new Vector3(0f, 0f, spin * t);
                yield return null;
            }
            r.anchoredPosition = to;
        }

        private IEnumerator Caption(string text)
        {
            if (onFinalCard) yield break;
            yield return Tween(0.2f, t => captionGroup.alpha = Mathf.Min(captionGroup.alpha, 1f - t));
            caption.text = "";
            captionGroup.alpha = 1f;
            for (int i = 1; i <= text.Length; i++)
            {
                caption.text = text.Substring(0, i);
                yield return Wait(0.028f);
            }
        }

        private IEnumerator CycleSky(Color[] colors, float duration)
        {
            float step = duration / colors.Length;
            var from = sky.color;
            foreach (var c in colors)
            {
                var a = from;
                yield return Tween(step, t => sky.color = Color.Lerp(a, c, t));
                from = c;
            }
        }

        private IEnumerator Flash(float duration, float hold = 0f)
        {
            yield return Tween(duration * 0.3f, t => flash.color = new Color(1f, 1f, 1f, t));
            if (hold > 0f) yield return Wait(hold * 0.3f);
            yield return Tween(duration * 0.7f, t => flash.color = new Color(1f, 1f, 1f, 1f - t));
        }

        private IEnumerator Shake(float duration, float strength)
        {
            for (float t = 0f; t < duration; t += Dt)
            {
                float k = strength * (1f - t / duration);
                fx.anchoredPosition = new Vector2(UnityEngine.Random.Range(-k, k), UnityEngine.Random.Range(-k, k));
                yield return null;
            }
            fx.anchoredPosition = Vector2.zero;
        }

        private static IEnumerator Wait(float seconds)
        {
            for (float t = 0f; t < seconds; t += Dt) yield return null;
        }

        private static IEnumerator Tween(float duration, Action<float> step)
        {
            for (float t = 0f; t < duration; t += Dt)
            {
                step(Mathf.Clamp01(t / duration));
                yield return null;
            }
            step(1f);
        }

        private static float Smooth(float t) => t * t * (3f - 2f * t);

        private static float Dt => Mathf.Min(Time.unscaledDeltaTime, 0.05f);

        private static RectTransform Rt(string name, Transform parent, Vector2 pos, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            return rt;
        }

        private static Image Img(string name, Transform parent, Sprite sprite, Vector2 pos, Vector2 size, Color color)
        {
            var img = Rt(name, parent, pos, size).gameObject.AddComponent<Image>();
            img.sprite = sprite;
            img.color = color;
            img.raycastTarget = false;
            img.preserveAspect = sprite != null;
            return img;
        }

        private Text Txt(string name, Transform parent, string text, Vector2 pos, Vector2 size, int fontSize, Color color)
        {
            var t = Rt(name, parent, pos, size).gameObject.AddComponent<Text>();
            t.font = art.font != null ? art.font : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            t.text = text;
            t.fontSize = Mathf.RoundToInt(fontSize * 1.25f);
            t.color = color;
            t.alignment = TextAnchor.MiddleCenter;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            t.raycastTarget = false;
            return t;
        }
    }

    public class UIFrames : MonoBehaviour
    {
        private Image image;
        private Sprite[] frames;
        private float fps, time;
        private bool loop, destroyAtEnd;

        public void Play(Sprite[] sprites, float framesPerSecond, bool looping, bool destroy = false)
        {
            image = GetComponent<Image>();
            frames = sprites;
            fps = framesPerSecond;
            loop = looping;
            destroyAtEnd = destroy;
            time = 0f;
            if (frames != null && frames.Length > 0) image.sprite = frames[0];
        }

        private void Update()
        {
            if (frames == null || frames.Length == 0) return;
            time += Mathf.Min(Time.unscaledDeltaTime, 0.05f);
            int i = Mathf.FloorToInt(time * fps);
            if (i >= frames.Length)
            {
                if (destroyAtEnd) { Destroy(gameObject); return; }
                i = loop ? i % frames.Length : frames.Length - 1;
            }
            image.sprite = frames[i];
        }
    }

    public class UIBob : MonoBehaviour
    {
        private float seed;
        private RectTransform rt;

        private void Awake()
        {
            rt = (RectTransform)transform;
            seed = UnityEngine.Random.value * 5f;
        }

        private void LateUpdate()
        {
            float t = Time.unscaledTime + seed;
            rt.localEulerAngles = new Vector3(0f, 0f, Mathf.Sin(t * 1.5f) * 8f);
            rt.localScale = Vector3.one * (1f + Mathf.Sin(t * 3f) * 0.05f);
        }
    }

    public class UISpin : MonoBehaviour
    {
        public float speed = 90f;

        private void Update() => transform.Rotate(0f, 0f, speed * Time.unscaledDeltaTime);
    }

    public class UIPulse : MonoBehaviour
    {
        private Graphic graphic;

        private void Awake() => graphic = GetComponent<Graphic>();

        private void Update()
        {
            var c = graphic.color;
            c.a = 0.55f + Mathf.Sin(Time.unscaledTime * 3f) * 0.4f;
            graphic.color = c;
        }
    }
}

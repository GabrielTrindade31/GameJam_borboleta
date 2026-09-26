using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ButterflyStep
{
    public class GameHUD : MonoBehaviour
    {
        [Header("Tempo")]
        [SerializeField] private Text dayText;
        [SerializeField] private Text seasonText;
        [SerializeField] private Color springColor = new Color(0.6f, 1f, 0.6f);
        [SerializeField] private Color summerColor = new Color(1f, 0.85f, 0.35f);
        [SerializeField] private Color autumnColor = new Color(1f, 0.6f, 0.3f);
        [SerializeField] private Color winterColor = new Color(0.75f, 0.9f, 1f);
        [SerializeField] private Text backArrow;
        [SerializeField] private Text forwardArrow;
        [SerializeField] private RectTransform periodDots;
        [SerializeField] private Image flashOverlay;
        [SerializeField] private Color pastFlash = new Color(1f, 0.7f, 0.3f, 0.45f);
        [SerializeField] private Color futureFlash = new Color(0.3f, 0.9f, 1f, 0.45f);
        [SerializeField] private Color arrowEnabled = Color.white;
        [SerializeField] private Color arrowDisabled = new Color(1f, 1f, 1f, 0.15f);

        [Header("Textos")]
        [SerializeField] private Text levelLabel;
        [SerializeField] private Text promptText;
        [SerializeField] private CanvasGroup promptGroup;
        [SerializeField] private Text messageText;
        [SerializeField] private CanvasGroup messageGroup;
        [SerializeField] private Text signText;
        [SerializeField] private CanvasGroup signGroup;

        [Header("Vida")]
        [SerializeField] private RectTransform heartsContainer;
        [SerializeField] private Color heartFull = new Color(0.95f, 0.3f, 0.4f);
        [SerializeField] private Color heartEmpty = new Color(1f, 1f, 1f, 0.15f);
        [SerializeField] private Sprite heartSprite;
        [SerializeField] private Sprite heartEmptySprite;
        [SerializeField] private Sprite dotSprite;

        [Header("Inventário")]
        [SerializeField] private RectTransform inventoryContainer;
        [SerializeField] private Sprite inventorySlot;

        [Header("Pólen")]
        [SerializeField] private Text pollenText;

        [Header("Chefe")]
        [SerializeField] private CanvasGroup bossGroup;
        [SerializeField] private Text bossName;
        [SerializeField] private Image bossFill;
        [SerializeField] private Text bossState;

        [Header("Poderes do tempo")]
        [SerializeField] private Image stasisFill;
        [SerializeField] private CanvasGroup stasisGroup;
        [SerializeField] private Image boltFill;
        [SerializeField] private CanvasGroup boltGroup;
        private PlayerShooter shooter;
        [SerializeField] private CanvasGroup peekGroup;
        [SerializeField] private Text peekText;

        [Header("Telas")]
        [SerializeField] private CanvasGroup introGroup;
        [SerializeField] private Text introTitle;
        [SerializeField] private Text introBody;
        [SerializeField] private CanvasGroup completeGroup;
        [SerializeField] private Text completeTitle;
        [SerializeField] private Text completeBody;
        [SerializeField] private CanvasGroup fadeGroup;

        [Header("Câmera")]
        [SerializeField] private CameraFollow cameraFollow;

        private readonly List<Image> dots = new List<Image>();
        private readonly List<Image> hearts = new List<Image>();
        private Coroutine dayRoutine;
        private Coroutine flashRoutine;
        private Coroutine messageRoutine;
        private Coroutine arrowRoutine;
        private Coroutine introRoutine;
        private string currentSign;
        private TimeManager time;
        private PlayerHealth health;
        private PlayerInventory inventory;
        private Vector2 backBasePos;
        private Vector2 forwardBasePos;

        public void Wire(CameraFollow follow) => cameraFollow = follow;

        private void Start()
        {
            var ctx = LevelContext.Current;
            time = ctx.Time;
            time.TimeChanged += OnTimeChanged;
            time.TimeChangeBlocked += OnTimeBlocked;
            if (ctx.Player != null)
            {
                health = ctx.Player.GetComponent<PlayerHealth>();
                if (health != null)
                {
                    health.HealthChanged += OnHealthChanged;
                    OnHealthChanged(health.Current, health.Max);
                }
            }
            if (ctx.Player != null)
            {
                inventory = ctx.Player.GetComponent<PlayerInventory>();
                if (inventory != null) inventory.Changed += RebuildInventory;
            }
            RebuildInventory();
            if (levelLabel != null) levelLabel.text = ctx.Flow.LevelTitle;
            if (backArrow != null) backBasePos = backArrow.rectTransform.anchoredPosition;
            if (forwardArrow != null) forwardBasePos = forwardArrow.rectTransform.anchoredPosition;
            BuildDots();
            SetGroup(promptGroup, 0f);
            SetGroup(messageGroup, 0f);
            SetGroup(signGroup, 0f);
            SetGroup(completeGroup, 0f);
            if (flashOverlay != null) flashOverlay.color = Color.clear;
            if (fadeGroup != null) StartCoroutine(Fade(fadeGroup, 1f, 0f, 0.5f));
            UpdateTimeDisplay(time.State, false);
            GameAudio.PlayMusic(time.State.Season);
        }

        private void OnDestroy()
        {
            if (time != null)
            {
                time.TimeChanged -= OnTimeChanged;
                time.TimeChangeBlocked -= OnTimeBlocked;
            }
            if (health != null) health.HealthChanged -= OnHealthChanged;
            if (inventory != null) inventory.Changed -= RebuildInventory;
        }

        private void RebuildInventory()
        {
            if (inventoryContainer == null) return;
            foreach (Transform child in inventoryContainer) Destroy(child.gameObject);
            if (inventory == null) return;
            foreach (var item in inventory.Items)
            {
                var slot = new GameObject(item.id, typeof(RectTransform), typeof(Image));
                slot.transform.SetParent(inventoryContainer, false);
                ((RectTransform)slot.transform).sizeDelta = new Vector2(190f, 52f);
                var bg = slot.GetComponent<Image>();
                bg.sprite = inventorySlot;
                bg.type = inventorySlot != null && inventorySlot.border != Vector4.zero ? Image.Type.Sliced : Image.Type.Simple;
                bg.color = inventorySlot != null ? Color.white : new Color(0f, 0f, 0f, 0.5f);
                bg.raycastTarget = false;

                var iconGo = new GameObject("Icon", typeof(RectTransform), typeof(Image));
                iconGo.transform.SetParent(slot.transform, false);
                var iconRt = (RectTransform)iconGo.transform;
                iconRt.anchorMin = iconRt.anchorMax = new Vector2(0f, 0.5f);
                iconRt.pivot = new Vector2(0f, 0.5f);
                iconRt.anchoredPosition = new Vector2(8f, 0f);
                iconRt.sizeDelta = new Vector2(38f, 38f);
                var icon = iconGo.GetComponent<Image>();
                icon.sprite = item.icon;
                icon.preserveAspect = true;
                icon.raycastTarget = false;

                var textGo = new GameObject("Name", typeof(RectTransform), typeof(Text));
                textGo.transform.SetParent(slot.transform, false);
                var textRt = (RectTransform)textGo.transform;
                textRt.anchorMin = Vector2.zero;
                textRt.anchorMax = Vector2.one;
                textRt.offsetMin = new Vector2(52f, 0f);
                textRt.offsetMax = new Vector2(-6f, 0f);
                var text = textGo.GetComponent<Text>();
                text.font = levelLabel != null ? levelLabel.font : null;
                text.fontSize = 20;
                text.alignment = TextAnchor.MiddleLeft;
                text.color = Color.white;
                text.text = item.displayName;
                text.raycastTarget = false;
            }
        }

        private void BuildDots()
        {
            if (periodDots == null) return;
            foreach (Transform child in periodDots) Destroy(child.gameObject);
            dots.Clear();
            foreach (int day in time.Days)
            {
                var go = new GameObject($"Dot_{day}", typeof(RectTransform), typeof(Image));
                go.transform.SetParent(periodDots, false);
                var rt = (RectTransform)go.transform;
                rt.sizeDelta = new Vector2(14f, 14f);
                var img = go.GetComponent<Image>();
                img.sprite = dotSprite;
                img.raycastTarget = false;
                dots.Add(img);
            }
        }

        private void OnTimeChanged(TimeState state)
        {
            if (state.Direction != 0) GameAudio.Play(state.Direction > 0 ? Sfx.TimeForward : Sfx.TimeBack, 0.02f);
            GameAudio.PlayMusic(state.Season);
            UpdateTimeDisplay(state, state.Direction != 0);
        }

        private void UpdateTimeDisplay(TimeState state, bool animate)
        {
            if (backArrow != null) backArrow.color = state.CanGoBack ? arrowEnabled : arrowDisabled;
            if (forwardArrow != null) forwardArrow.color = state.CanGoForward ? arrowEnabled : arrowDisabled;

            for (int i = 0; i < dots.Count; i++)
            {
                bool current = i == state.Index;
                dots[i].color = current ? new Color(1f, 0.85f, 0.3f) : new Color(1f, 1f, 1f, 0.3f);
                dots[i].rectTransform.sizeDelta = current ? new Vector2(20f, 20f) : new Vector2(12f, 12f);
            }

            if (seasonText != null)
            {
                seasonText.text = $"{SeasonCalendar.Name(state.Season).ToUpper()}  ·  ANO {state.YearNumber}";
                seasonText.color = SeasonColor(state.Season);
            }
            if (dayText == null) return;
            if (!animate)
            {
                dayText.text = $"DIA {state.DisplayDay}";
                return;
            }

            if (dayRoutine != null) StopCoroutine(dayRoutine);
            dayRoutine = StartCoroutine(AnimateDay(state.PreviousDay + 1, state.DisplayDay));
            if (state.SeasonChanged) ShowMessage(SeasonMessage(state.Season));
            if (flashRoutine != null) StopCoroutine(flashRoutine);
            flashRoutine = StartCoroutine(Flash(state.Direction > 0 ? futureFlash : pastFlash));
            Shake(0.2f, 0.15f);
        }

        private void OnTimeBlocked(int direction)
        {
            GameAudio.Play(Sfx.Blocked);
            if (time == null) return;
            var state = time.State;
            bool atLimit = direction < 0 ? !state.CanGoBack : !state.CanGoForward;
            if (atLimit)
            {
                ShowMessage(direction < 0
                    ? "Não é possível voltar antes do Dia 1. A linha do tempo desta fase começa aqui."
                    : $"O Dia {state.MaxDay + 1} é o limite do futuro nesta fase.");
            }
            else
            {
                ShowMessage("Algo sólido ocuparia o seu lugar nesse dia. Mova-se primeiro.");
            }
            if (arrowRoutine != null) StopCoroutine(arrowRoutine);
            ResetArrows();
            if (backArrow != null) backArrow.rectTransform.anchoredPosition = backBasePos;
            if (forwardArrow != null) forwardArrow.rectTransform.anchoredPosition = forwardBasePos;
            arrowRoutine = StartCoroutine(ShakeArrow(direction < 0 ? backArrow : forwardArrow));
        }

        private void ResetArrows()
        {
            if (backArrow != null) backArrow.color = ArrowColor(backArrow);
            if (forwardArrow != null) forwardArrow.color = ArrowColor(forwardArrow);
        }

        private Color SeasonColor(Season season)
        {
            switch (season)
            {
                case Season.Primavera: return springColor;
                case Season.Verao: return summerColor;
                case Season.Outono: return autumnColor;
                default: return winterColor;
            }
        }

        private static string SeasonMessage(Season season)
        {
            switch (season)
            {
                case Season.Primavera: return "A primavera chegou: tudo volta a brotar.";
                case Season.Verao: return "Verão: dias quentes, rios mais baixos, insetos no ar.";
                case Season.Outono: return "Outono: as folhas caem e as criaturas se preparam.";
                default: return "Inverno: a água congela e muitas criaturas hibernam.";
            }
        }

        private IEnumerator AnimateDay(int from, int to)
        {
            const float duration = 0.4f;
            var rt = dayText.rectTransform;
            for (float t = 0f; t < duration; t += UnityEngine.Time.unscaledDeltaTime)
            {
                float k = t / duration;
                dayText.text = $"DIA {Mathf.RoundToInt(Mathf.Lerp(from, to, 1f - (1f - k) * (1f - k)))}";
                float s = 1f + Mathf.Sin(k * Mathf.PI) * 0.35f;
                rt.localScale = new Vector3(s, s, 1f);
                yield return null;
            }
            dayText.text = $"DIA {to}";
            rt.localScale = Vector3.one;
        }

        private IEnumerator Flash(Color color)
        {
            if (flashOverlay == null) yield break;
            const float duration = 0.45f;
            for (float t = 0f; t < duration; t += UnityEngine.Time.unscaledDeltaTime)
            {
                var c = color;
                c.a = color.a * (1f - t / duration);
                flashOverlay.color = c;
                yield return null;
            }
            flashOverlay.color = Color.clear;
        }

        private IEnumerator ShakeArrow(Text arrow)
        {
            if (arrow == null) yield break;
            var rt = arrow.rectTransform;
            Vector2 basePos = arrow == backArrow ? backBasePos : forwardBasePos;
            for (float t = 0f; t < 0.3f; t += UnityEngine.Time.unscaledDeltaTime)
            {
                rt.anchoredPosition = basePos + new Vector2(Mathf.Sin(t * 80f) * 6f, 0f);
                arrow.color = Color.Lerp(new Color(1f, 0.3f, 0.3f), ArrowColor(arrow), t / 0.3f);
                yield return null;
            }
            rt.anchoredPosition = basePos;
            arrow.color = ArrowColor(arrow);
            arrowRoutine = null;
        }

        private Color ArrowColor(Text arrow)
        {
            var state = time.State;
            bool enabled = arrow == backArrow ? state.CanGoBack : state.CanGoForward;
            return enabled ? arrowEnabled : arrowDisabled;
        }

        private void OnHealthChanged(int current, int max)
        {
            if (heartsContainer == null) return;
            while (hearts.Count < max)
            {
                var go = new GameObject("Heart", typeof(RectTransform), typeof(Image));
                go.transform.SetParent(heartsContainer, false);
                ((RectTransform)go.transform).sizeDelta = new Vector2(30f, 30f);
                var img = go.GetComponent<Image>();
                img.raycastTarget = false;
                hearts.Add(img);
            }
            for (int i = 0; i < hearts.Count; i++)
            {
                bool full = i < current;
                if (heartSprite != null)
                {
                    hearts[i].sprite = full ? heartSprite : (heartEmptySprite != null ? heartEmptySprite : heartSprite);
                    hearts[i].color = full ? Color.white : new Color(1f, 1f, 1f, 0.5f);
                }
                else hearts[i].color = full ? heartFull : heartEmpty;
            }
        }

        public void SetPollen(int count, int total)
        {
            if (pollenText != null) pollenText.text = $"BORBOLETAS  {count}/{total}";
        }

        public void SetBoss(string name, float health01, bool alive, bool vulnerable, bool stunned)
        {
            if (bossGroup == null) return;
            bool show = !string.IsNullOrEmpty(name);
            bossGroup.alpha = Mathf.MoveTowards(bossGroup.alpha, show ? 1f : 0f, UnityEngine.Time.deltaTime * 4f);
            if (!show) return;
            if (bossName != null) bossName.text = name;
            if (bossFill != null)
            {
                bossFill.fillAmount = Mathf.MoveTowards(bossFill.fillAmount, health01, UnityEngine.Time.deltaTime * 2f);
                bossFill.color = !alive ? new Color(0.5f, 0.5f, 0.5f) : vulnerable ? new Color(0.95f, 0.3f, 0.3f) : new Color(0.55f, 0.65f, 0.85f);
            }
            if (bossState != null) bossState.text = !alive ? "DERROTADO" : stunned && vulnerable ? "ATORDOADO — ATAQUE!" : vulnerable ? "VULNERÁVEL" : "INVULNERÁVEL NESTA ÉPOCA";
        }

        private void Update()
        {
            var ctx = LevelContext.Current;
            if (ctx == null) return;
            if (boltFill != null && ctx.Player != null)
            {
                if (shooter == null) shooter = ctx.Player.GetComponent<PlayerShooter>();
                if (shooter != null)
                {
                    boltFill.fillAmount = shooter.Charge;
                    boltFill.color = shooter.Charge >= 1f ? new Color(0.75f, 0.6f, 1f) : new Color(0.4f, 0.35f, 0.55f);
                    if (boltGroup != null) boltGroup.alpha = shooter.Unlocked ? 1f : 0f;
                }
            }
            if (stasisFill != null && ctx.Stasis != null)
            {
                stasisFill.fillAmount = TimeStasis.Active ? ctx.Stasis.Remaining / 3f : ctx.Stasis.Energy;
                stasisFill.color = TimeStasis.Active ? new Color(0.5f, 0.85f, 1f) : ctx.Stasis.Energy >= 1f ? new Color(0.45f, 0.75f, 1f) : new Color(0.35f, 0.45f, 0.6f);
                if (stasisGroup != null) stasisGroup.alpha = ctx.Stasis.Unlocked ? 1f : 0f;
            }
            if (peekGroup != null && time != null)
            {
                peekGroup.alpha = time.IsPeeking ? 1f : 0f;
                if (time.IsPeeking && peekText != null) peekText.text = $"ESPIANDO O DIA {time.State.DisplayDay}  —  solte SHIFT para voltar ao dia {time.PeekOriginDay + 1}";
            }
        }

        public void SetPrompt(string text)
        {
            if (promptGroup == null) return;
            bool show = !string.IsNullOrEmpty(text);
            if (show && promptText != null) promptText.text = text;
            promptGroup.alpha = Mathf.MoveTowards(promptGroup.alpha, show ? 1f : 0f, UnityEngine.Time.deltaTime * 8f);
        }

        public void ShowMessage(string text, float duration = 2.6f)
        {
            if (messageText == null || messageGroup == null) return;
            messageText.text = text;
            if (messageRoutine != null) StopCoroutine(messageRoutine);
            messageRoutine = StartCoroutine(MessageRoutine(duration));
        }

        private IEnumerator MessageRoutine(float duration)
        {
            yield return Fade(messageGroup, messageGroup.alpha, 1f, 0.15f);
            yield return new WaitForSeconds(duration);
            yield return Fade(messageGroup, 1f, 0f, 0.4f);
        }

        public void ShowSign(string text)
        {
            if (signGroup == null || signText == null) return;
            currentSign = text;
            signText.text = text;
            signGroup.alpha = 1f;
        }

        public void HideSign(string text)
        {
            if (signGroup == null || currentSign != text) return;
            currentSign = null;
            signGroup.alpha = 0f;
        }

        public void ShowIntro(string title, string body)
        {
            if (introGroup == null) return;
            if (introTitle != null) introTitle.text = title;
            if (introBody != null) introBody.text = body;
            introRoutine = StartCoroutine(IntroRoutine());
        }

        private IEnumerator IntroRoutine()
        {
            SetGroup(introGroup, 1f);
            yield return new WaitForSeconds(3.5f);
            yield return Fade(introGroup, 1f, 0f, 0.8f);
        }

        private void HideIntro()
        {
            if (introRoutine != null) StopCoroutine(introRoutine);
            introRoutine = null;
            SetGroup(introGroup, 0f);
        }

        public void ShowLevelComplete(string body)
        {
            if (completeGroup == null) return;
            HideIntro();
            if (completeTitle != null) completeTitle.text = "FASE CONCLUÍDA";
            if (completeBody != null) completeBody.text = body;
            StartCoroutine(Fade(completeGroup, 0f, 1f, 0.4f));
        }

        public void ShowDeath()
        {
            if (completeGroup == null) return;
            HideIntro();
            if (completeTitle != null) completeTitle.text = "O FIO SE ROMPEU";
            if (completeBody != null) completeBody.text = "Voltando à última Flor do Tempo...";
            StartCoroutine(Fade(completeGroup, 0f, 1f, 0.3f));
        }

        public void HideDeath() => SetGroup(completeGroup, 0f);

        public IEnumerator FadeIn(float duration)
        {
            if (fadeGroup == null) yield break;
            yield return Fade(fadeGroup, fadeGroup.alpha, 0f, duration);
        }

        public IEnumerator FadeOut(float duration)
        {
            if (fadeGroup == null) yield break;
            yield return Fade(fadeGroup, fadeGroup.alpha, 1f, duration);
        }

        public void Shake(float duration, float strength)
        {
            if (cameraFollow != null) cameraFollow.Shake(duration, strength);
        }

        private static void SetGroup(CanvasGroup group, float alpha)
        {
            if (group != null) group.alpha = alpha;
        }

        private static IEnumerator Fade(CanvasGroup group, float from, float to, float duration)
        {
            if (group == null) yield break;
            for (float t = 0f; t < duration; t += UnityEngine.Time.unscaledDeltaTime)
            {
                group.alpha = Mathf.Lerp(from, to, t / duration);
                yield return null;
            }
            group.alpha = to;
        }
    }
}

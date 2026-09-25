using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ButterflyStep
{
    public class MainMenu : MonoBehaviour
    {
        public static readonly string[] ChapterNames =
        {
            "O Primeiro Passo", "A Porta que Ficou", "Degelo", "A Criatura das Eras", "O Bater de Asas",
            "O Futuro Devorado", "O Nível da Água", "Estações em Sequência", "A Porta do Passado", "O Devorador do Tempo"
        };

        [Header("Painéis")]
        [SerializeField] private CanvasGroup mainPanel;
        [SerializeField] private CanvasGroup chaptersPanel;
        [SerializeField] private CanvasGroup controlsPanel;
        [SerializeField] private CanvasGroup creditsPanel;
        [SerializeField] private CanvasGroup storyPanel;
        [SerializeField] private CanvasGroup fade;

        [Header("Botões principais")]
        [SerializeField] private Button newGameButton;
        [SerializeField] private Button continueButton;
        [SerializeField] private Text continueLabel;
        [SerializeField] private Button chaptersButton;
        [SerializeField] private Button controlsButton;
        [SerializeField] private Button creditsButton;
        [SerializeField] private Button quitButton;

        [Header("Outros")]
        [SerializeField] private Button[] chapterButtons = new Button[10];
        [SerializeField] private Text[] chapterLabels = new Text[10];
        [SerializeField] private Button resetButton;
        [SerializeField] private Button startStoryButton;
        [SerializeField] private Button[] backButtons = new Button[0];

        private CanvasGroup current;
        private bool loading;

        public void Setup(CanvasGroup main, CanvasGroup chapters, CanvasGroup controlsGroup, CanvasGroup credits, CanvasGroup story, CanvasGroup fadeGroup)
        {
            mainPanel = main;
            chaptersPanel = chapters;
            controlsPanel = controlsGroup;
            creditsPanel = credits;
            storyPanel = story;
            fade = fadeGroup;
        }

        public void SetMainButtons(Button newGame, Button cont, Text contLabel, Button chapters, Button controlsBtn, Button credits, Button quit)
        {
            newGameButton = newGame;
            continueButton = cont;
            continueLabel = contLabel;
            chaptersButton = chapters;
            controlsButton = controlsBtn;
            creditsButton = credits;
            quitButton = quit;
        }

        public void SetOtherButtons(Button[] chapterBtns, Text[] chapterTexts, Button reset, Button startStory, Button[] backs)
        {
            chapterButtons = chapterBtns;
            chapterLabels = chapterTexts;
            resetButton = reset;
            startStoryButton = startStory;
            backButtons = backs;
        }

        private void Awake()
        {
            Time.timeScale = 1f;
            newGameButton.onClick.AddListener(() => Show(storyPanel));
            continueButton.onClick.AddListener(() => LoadLevel(GameProgress.LastLevel));
            chaptersButton.onClick.AddListener(() => { RefreshChapters(); Show(chaptersPanel); });
            controlsButton.onClick.AddListener(() => Show(controlsPanel));
            creditsButton.onClick.AddListener(() => Show(creditsPanel));
            quitButton.onClick.AddListener(Quit);
            startStoryButton.onClick.AddListener(() => LoadLevel(1));
            resetButton.onClick.AddListener(() => { GameProgress.ResetAll(); RefreshChapters(); RefreshContinue(); });
            foreach (var back in backButtons) back.onClick.AddListener(() => Show(mainPanel));
            for (int i = 0; i < chapterButtons.Length; i++)
            {
                int level = i + 1;
                chapterButtons[i].onClick.AddListener(() => LoadLevel(level));
            }
        }

        private void Start()
        {
            foreach (var p in new[] { chaptersPanel, controlsPanel, creditsPanel, storyPanel }) SetPanel(p, false);
            RefreshContinue();
            Show(mainPanel);
            if (fade != null) StartCoroutine(Fade(1f, 0f, 1f));
        }

        private void Update()
        {
            if (current == mainPanel || loading) return;
            bool cancel = (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame) ||
                          (Gamepad.current != null && Gamepad.current.buttonEast.wasPressedThisFrame);
            if (cancel)
            {
                GameAudio.Play(Sfx.UiMove, 0f);
                Show(mainPanel);
            }
        }

        private void RefreshContinue()
        {
            bool has = GameProgress.HasProgress;
            continueButton.gameObject.SetActive(has);
            if (has && continueLabel != null) continueLabel.text = $"Continuar — Cap. {GameProgress.LastLevel}";
        }

        private void RefreshChapters()
        {
            int unlocked = GameProgress.UnlockedLevel;
            for (int i = 0; i < chapterButtons.Length; i++)
            {
                int level = i + 1;
                bool open = level <= unlocked;
                chapterButtons[i].interactable = open;
                int pollen = GameProgress.PollenCount($"Level{level:00}");
                chapterLabels[i].text = open
                    ? $"{level}. {ChapterNames[i]}\n<size=18>Borboletas {pollen}/{GameProgress.PollenPerLevel}</size>"
                    : $"{level}. ???\n<size=18>Bloqueado</size>";
            }
        }

        private void Show(CanvasGroup panel)
        {
            foreach (var p in new[] { mainPanel, chaptersPanel, controlsPanel, creditsPanel, storyPanel }) SetPanel(p, p == panel);
            current = panel;
            var first = panel.GetComponentInChildren<Selectable>();
            if (panel == mainPanel) first = continueButton.gameObject.activeSelf ? continueButton : newGameButton;
            if (EventSystem.current != null && first != null) EventSystem.current.SetSelectedGameObject(first.gameObject);
        }

        private static void SetPanel(CanvasGroup panel, bool visible)
        {
            if (panel == null) return;
            panel.alpha = visible ? 1f : 0f;
            panel.interactable = visible;
            panel.blocksRaycasts = visible;
        }

        private void LoadLevel(int level)
        {
            if (loading) return;
            loading = true;
            StartCoroutine(LoadRoutine($"Level{level:00}"));
        }

        private IEnumerator LoadRoutine(string scene)
        {
            if (fade != null) yield return Fade(0f, 1f, 0.6f);
            SceneManager.LoadScene(scene);
        }

        private IEnumerator Fade(float from, float to, float duration)
        {
            for (float t = 0f; t < duration; t += Time.unscaledDeltaTime)
            {
                fade.alpha = Mathf.Lerp(from, to, t / duration);
                yield return null;
            }
            fade.alpha = to;
        }

        private static void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ButterflyStep
{
    public class PauseMenu : MonoBehaviour
    {
        [SerializeField] private CanvasGroup panel;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button menuButton;
        [SerializeField] private InputActionAsset controls;

        private InputAction pause;

        public static bool IsPaused { get; private set; }

        public void Setup(CanvasGroup group, Button resume, Button restart, Button menu, InputActionAsset asset)
        {
            panel = group;
            resumeButton = resume;
            restartButton = restart;
            menuButton = menu;
            controls = asset;
        }

        private void Awake()
        {
            IsPaused = false;
            if (controls != null) pause = controls.FindAction("Gameplay/Pause");
            if (resumeButton != null) resumeButton.onClick.AddListener(Resume);
            if (restartButton != null) restartButton.onClick.AddListener(RestartLevel);
            if (menuButton != null) menuButton.onClick.AddListener(GoToMenu);
            CreateOptionsButton();
            Show(false);
        }

        private void CreateOptionsButton()
        {
            if (restartButton == null || resumeButton == null || menuButton == null) return;
            var box = restartButton.transform.parent as RectTransform;
            if (box != null) box.sizeDelta += new Vector2(0f, 80f);
            var options = Instantiate(restartButton, restartButton.transform.parent);
            options.name = "Opcoes";
            options.onClick = new Button.ButtonClickedEvent();
            var label = options.GetComponentInChildren<Text>();
            if (label != null) label.text = "Opções";
            ((RectTransform)resumeButton.transform).anchoredPosition = new Vector2(0f, 70f);
            ((RectTransform)restartButton.transform).anchoredPosition = new Vector2(0f, -8f);
            ((RectTransform)options.transform).anchoredPosition = new Vector2(0f, -86f);
            ((RectTransform)menuButton.transform).anchoredPosition = new Vector2(0f, -164f);
            options.onClick.AddListener(() => OptionsPanel.Open(panel.GetComponentInParent<Canvas>().rootCanvas.transform, options, panel, () =>
            {
                if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(options.gameObject);
            }));
        }

        private void OnDestroy()
        {
            if (IsPaused) Time.timeScale = 1f;
            IsPaused = false;
        }

        private void Update()
        {
            if (pause != null && !pause.enabled) pause.Enable();
            bool pressed = pause != null ? pause.WasPressedThisFrame() : Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
            if (!pressed || OptionsPanel.IsOpen || OptionsPanel.ClosedFrame == Time.frameCount) return;
            var ctx = LevelContext.Current;
            if (!IsPaused && ctx != null && ctx.Flow.IsFinishing) return;
            if (IsPaused) Resume();
            else Pause();
        }

        private void Pause()
        {
            IsPaused = true;
            Time.timeScale = 0f;
            Show(true);
            GameAudio.Play(Sfx.UiSelect, 0f);
            if (EventSystem.current != null && resumeButton != null) EventSystem.current.SetSelectedGameObject(resumeButton.gameObject);
        }

        public void Resume()
        {
            IsPaused = false;
            Time.timeScale = 1f;
            Show(false);
            GameAudio.Play(Sfx.UiMove, 0f);
        }

        private void RestartLevel()
        {
            IsPaused = false;
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        private void GoToMenu()
        {
            IsPaused = false;
            Time.timeScale = 1f;
            SceneManager.LoadScene(0);
        }

        private void Show(bool visible)
        {
            if (panel == null) return;
            panel.alpha = visible ? 1f : 0f;
            panel.interactable = visible;
            panel.blocksRaycasts = visible;
        }
    }
}

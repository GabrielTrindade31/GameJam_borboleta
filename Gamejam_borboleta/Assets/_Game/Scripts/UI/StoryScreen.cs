using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ButterflyStep
{
    public class StoryScreen : MonoBehaviour
    {
        [SerializeField] private CanvasGroup content;
        [SerializeField] private Text continueHint;
        [SerializeField] private string nextScene = "Level01";
        [SerializeField] private float minDisplayTime = 1f;
        [SerializeField] private InputActionAsset controls;

        private InputAction submit;
        private float shownAt;
        private bool leaving;

        public void Setup(CanvasGroup group, Text hint, string next, InputActionAsset asset)
        {
            content = group;
            continueHint = hint;
            nextScene = next;
            controls = asset;
        }

        private void Awake()
        {
            if (controls != null) submit = controls.FindAction("Gameplay/Submit");
        }

        private void OnEnable() => submit?.Enable();

        private void OnDisable() => submit?.Disable();

        private void Start()
        {
            shownAt = UnityEngine.Time.time;
            if (content != null) StartCoroutine(Fade(0f, 1f, 1.2f));
        }

        private void Update()
        {
            if (submit != null && !submit.enabled) submit.Enable();
            if (continueHint != null)
            {
                var c = continueHint.color;
                c.a = 0.5f + Mathf.Sin(UnityEngine.Time.time * 3f) * 0.4f;
                continueHint.color = c;
            }

            if (leaving || UnityEngine.Time.time - shownAt < minDisplayTime) return;
            bool pressed = submit != null ? submit.WasPressedThisFrame() : Keyboard.current != null && Keyboard.current.enterKey.wasPressedThisFrame;
            if (pressed) StartCoroutine(Leave());
        }

        private IEnumerator Leave()
        {
            leaving = true;
            if (content != null) yield return Fade(1f, 0f, 0.6f);
            SceneManager.LoadScene(nextScene);
        }

        private IEnumerator Fade(float from, float to, float duration)
        {
            for (float t = 0f; t < duration; t += UnityEngine.Time.deltaTime)
            {
                content.alpha = Mathf.Lerp(from, to, t / duration);
                yield return null;
            }
            content.alpha = to;
        }
    }
}

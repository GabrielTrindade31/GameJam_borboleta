using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ButterflyStep
{
    [RequireComponent(typeof(Button))]
    public class MenuButtonFx : MonoBehaviour, ISelectHandler, IDeselectHandler, IPointerEnterHandler, ISubmitHandler, IPointerClickHandler
    {
        [SerializeField] private float selectedScale = 1.08f;
        [SerializeField] private Text label;

        private bool selected;
        private Color labelColor;

        public void Setup(Text text) => label = text;

        private void Awake()
        {
            if (label != null) labelColor = label.color;
        }

        public void OnSelect(BaseEventData eventData)
        {
            selected = true;
            if (label != null) label.color = new Color(1f, 0.9f, 0.45f);
            GameAudio.Play(Sfx.UiMove, 0f);
        }

        public void OnDeselect(BaseEventData eventData)
        {
            selected = false;
            if (label != null) label.color = labelColor;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            var button = GetComponent<Button>();
            if (button.interactable && EventSystem.current != null) EventSystem.current.SetSelectedGameObject(gameObject);
        }

        public void OnSubmit(BaseEventData eventData) => GameAudio.Play(Sfx.UiSelect, 0f);

        public void OnPointerClick(PointerEventData eventData) => GameAudio.Play(Sfx.UiSelect, 0f);

        private void Update()
        {
            float target = selected ? selectedScale : 1f;
            float s = Mathf.MoveTowards(transform.localScale.x, target, Time.unscaledDeltaTime * 1.5f);
            transform.localScale = new Vector3(s, s, 1f);
        }
    }
}

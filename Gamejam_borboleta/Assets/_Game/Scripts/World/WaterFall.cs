using UnityEngine;

namespace ButterflyStep
{
    public class WaterFall : MonoBehaviour
    {
        [SerializeField] private float width = 1f;
        [SerializeField] private float height = 7f;
        [SerializeField] private Sprite streakSprite;
        [SerializeField] private int streaks = 6;
        [SerializeField] private float speed = 9f;
        [SerializeField] private Color streakColor = new Color(0.9f, 0.97f, 1f, 0.55f);
        [SerializeField] private int sortingOrder = 7;

        private Transform[] items;
        private float[] offsets;
        private float[] lengths;

        public void Setup(float w, float h, Sprite sprite, int order)
        {
            width = w;
            height = h;
            streakSprite = sprite;
            sortingOrder = order;
            streaks = Mathf.Max(3, Mathf.RoundToInt(w * 5f));
        }

        private void Awake()
        {
            if (streakSprite == null) return;
            items = new Transform[streaks];
            offsets = new float[streaks];
            lengths = new float[streaks];
            for (int i = 0; i < streaks; i++)
            {
                var go = new GameObject("Fio");
                go.transform.SetParent(transform, false);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = streakSprite;
                sr.color = streakColor;
                sr.sortingOrder = sortingOrder;
                lengths[i] = Random.Range(0.6f, 1.6f);
                float sx = Random.Range(0.04f, 0.09f);
                go.transform.localScale = new Vector3(sx / Mathf.Max(0.01f, streakSprite.bounds.size.x), lengths[i] / Mathf.Max(0.01f, streakSprite.bounds.size.y), 1f);
                offsets[i] = Random.value * height;
                items[i] = go.transform;
                items[i].localPosition = new Vector3(-width * 0.5f + width * (i + 0.5f) / streaks + Random.Range(-0.05f, 0.05f), 0f, 0f);
            }
        }

        private void Update()
        {
            if (items == null) return;
            for (int i = 0; i < items.Length; i++)
            {
                offsets[i] = (offsets[i] + speed * Time.deltaTime * (0.8f + lengths[i] * 0.2f)) % Mathf.Max(0.1f, height - lengths[i]);
                var p = items[i].localPosition;
                p.y = -offsets[i] - lengths[i] * 0.5f;
                items[i].localPosition = p;
            }
        }
    }
}

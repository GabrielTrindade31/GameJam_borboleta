using System.Collections.Generic;
using UnityEngine;

namespace ButterflyStep
{
    [RequireComponent(typeof(Collider2D))]
    public class ItemPickup : TemporalBehaviour
    {
        [Header("Item")]
        [SerializeField] private string itemId = "chave";
        [SerializeField] private string displayName = "Chave";
        [SerializeField] private Sprite icon;
        [TextArea(1, 3)] [SerializeField] private string pickupMessage = "Você pegou um item. Ele continua com você em qualquer dia.";

        [Header("Quando o item existe no mundo")]
        [Tooltip("Condições para o item aparecer (dias, estação, flags). Depois de coletado ele some de todos os dias.")]
        [SerializeField] private List<TemporalCondition> existsWhen = new List<TemporalCondition>();

        [Header("Visual")]
        [SerializeField] private Transform visual;
        [SerializeField] private float bobHeight = 0.12f;

        private bool collected;
        private bool visible;
        private Vector3 visualBase;

        public void Setup(string id, string itemName, Sprite itemIcon, string message, Transform visualRoot, params TemporalCondition[] conditions)
        {
            itemId = id;
            displayName = itemName;
            icon = itemIcon;
            pickupMessage = message;
            visual = visualRoot;
            existsWhen = new List<TemporalCondition>(conditions);
        }

        private void Awake()
        {
            GetComponent<Collider2D>().isTrigger = true;
            if (visual != null) visualBase = visual.localPosition;
        }

        protected override void Refresh(bool instant)
        {
            visible = !collected && Check(existsWhen);
            if (visual != null) visual.gameObject.SetActive(visible);
        }

        private void Update()
        {
            if (!visible || visual == null) return;
            visual.localPosition = visualBase + Vector3.up * Mathf.Sin(UnityEngine.Time.time * 3f) * bobHeight;
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (!visible || collected) return;
            var player = other.GetComponentInParent<PlayerController>();
            if (player == null) return;
            var inventory = player.GetComponent<PlayerInventory>();
            if (inventory == null) return;

            collected = true;
            inventory.Add(new InventoryItem { id = itemId, displayName = displayName, icon = icon });
            GameAudio.Play(Sfx.Pickup);
            Refresh(false);
            var fx = FeedbackFX.Instance;
            if (fx != null) fx.Burst(transform.position, new Color(1f, 0.9f, 0.4f), 24);
            var hud = Context.Hud;
            if (hud != null) hud.ShowMessage(pickupMessage, 3.5f);
        }
    }
}

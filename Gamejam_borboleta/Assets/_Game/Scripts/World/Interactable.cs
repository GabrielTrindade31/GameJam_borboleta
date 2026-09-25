using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace ButterflyStep
{
    public class Interactable : MonoBehaviour
    {
        [Header("Texto")]
        [Tooltip("Texto do botão. Ex: Regar a planta.")]
        [SerializeField] private string prompt = "Interagir";
        [Tooltip("Texto quando a ação pode ser desfeita no mesmo dia.")]
        [SerializeField] private string undoPrompt = "Desfazer";
        [Tooltip("Mensagem exibida quando a ação é feita.")]
        [SerializeField] private string doneMessage = "Ação registrada na linha do tempo.";
        [Tooltip("Dica mostrada quando as condições não permitem a ação (ex: 'Congelado — volte para uma estação mais quente'). Vazio = não mostra nada.")]
        [SerializeField] private string blockedPrompt = "";

        [Header("Ação")]
        [Tooltip("Flag registrada no dia atual quando o jogador interage. Ex: L1_PlantaRegada")]
        [SerializeField] private string flag = "NovaAcao";
        [Tooltip("Permite desfazer a ação interagindo de novo no mesmo dia em que ela foi feita.")]
        [SerializeField] private bool allowUndoSameDay = true;
        [Tooltip("Condições para a interação estar disponível (além da flag ainda não estar ativa).")]
        [SerializeField] private List<TemporalCondition> availableWhen = new List<TemporalCondition>();

        [Header("Itens")]
        [Tooltip("Id do item que o jogador precisa carregar (ex: chave, pinha). Vazio = nenhum.")]
        [SerializeField] private string requiredItem = "";
        [Tooltip("Nome do item mostrado na dica.")]
        [SerializeField] private string requiredItemName = "";
        [Tooltip("O item é gasto ao interagir.")]
        [SerializeField] private bool consumeItem = true;

        [SerializeField] private UnityEvent onInteract = new UnityEvent();

        [Header("Visual")]
        [SerializeField] private SpriteRenderer highlight;

        private float pulse;
        private bool highlighted;
        private InventoryItem returnedItem;
        private Vector3 highlightBase;

        private void Awake()
        {
            if (highlight != null) highlightBase = highlight.transform.localPosition;
        }

        public string Flag => flag;

        public void Setup(string actionFlag, string actionPrompt, string message, params TemporalCondition[] conditions)
        {
            flag = actionFlag;
            prompt = actionPrompt;
            doneMessage = message;
            availableWhen = new List<TemporalCondition>(conditions);
        }

        public void RequireItem(string id, string itemName, bool consume, string blocked)
        {
            requiredItem = id;
            requiredItemName = itemName;
            consumeItem = consume;
            blockedPrompt = blocked;
            if (consume) allowUndoSameDay = false;
        }

        public void SetBlockedPrompt(string text) => blockedPrompt = text;

        public void SetHighlight(SpriteRenderer renderer) => highlight = renderer;

        private LevelContext Ctx => LevelContext.Current;

        private bool IsDoneNow => Ctx.World.IsActive(flag, Ctx.Now.Day);

        private bool CanUndo => allowUndoSameDay && Ctx.World.TryGetFlagDay(flag, out int y) && y == Ctx.Now.Day;

        private PlayerInventory Inventory => Ctx != null && Ctx.Player != null ? Ctx.Player.GetComponent<PlayerInventory>() : null;

        private bool HasRequiredItem
        {
            get
            {
                if (string.IsNullOrEmpty(requiredItem)) return true;
                var inventory = Inventory;
                return inventory != null && inventory.Has(requiredItem);
            }
        }

        private bool ConditionsMet => TemporalCondition.All(availableWhen, Ctx.Now.Day, Ctx.Time.StartDay, Ctx.Time.Calendar, Ctx.World);

        public bool CanInteract
        {
            get
            {
                if (Ctx == null || !isActiveAndEnabled) return false;
                if (IsDoneNow) return CanUndo;
                return ConditionsMet;
            }
        }

        public bool ShowsHint => Ctx != null && isActiveAndEnabled && !IsDoneNow && !string.IsNullOrEmpty(blockedPrompt) && (!ConditionsMet || !HasRequiredItem);

        public string CurrentPrompt
        {
            get
            {
                if (IsDoneNow) return undoPrompt;
                if (!ConditionsMet || !HasRequiredItem) return string.IsNullOrEmpty(blockedPrompt) ? prompt : blockedPrompt;
                return prompt;
            }
        }

        public void Interact()
        {
            var hud = Ctx != null ? Ctx.Hud : null;
            if (!CanInteract)
            {
                if (ShowsHint && hud != null) hud.ShowMessage(blockedPrompt);
                return;
            }
            var fx = FeedbackFX.Instance;

            if (IsDoneNow)
            {
                Ctx.World.ClearFlag(flag);
                if (returnedItem != null && Inventory != null) Inventory.Add(returnedItem);
                returnedItem = null;
                if (hud != null) hud.ShowMessage("Ação desfeita. O futuro foi recalculado.");
            }
            else
            {
                if (!HasRequiredItem)
                {
                    if (hud != null) hud.ShowMessage($"Você precisa de: {requiredItemName}.");
                    return;
                }
                if (!string.IsNullOrEmpty(requiredItem) && consumeItem) returnedItem = Inventory.Remove(requiredItem);
                Ctx.World.SetFlag(flag, Ctx.Now.Day);
                if (hud != null) hud.ShowMessage($"{doneMessage}  (Dia {Ctx.Now.DisplayDay})");
                GameAudio.Play(Sfx.Interact);
                onInteract.Invoke();
            }
            if (fx != null) fx.Burst(transform.position, new Color(1f, 0.9f, 0.3f), 16);
        }

        public void SetHighlighted(bool value)
        {
            highlighted = value;
            if (highlight != null && !value) highlight.enabled = false;
        }

        private void Update()
        {
            if (highlight == null) return;
            bool ready = CanInteract && HasRequiredItem;
            bool show = highlighted || ready;
            highlight.enabled = show;
            if (!show) return;
            pulse += UnityEngine.Time.deltaTime * (highlighted ? 6f : 3f);
            var c = highlight.color;
            c.a = highlighted ? 1f : 0.55f;
            highlight.color = c;
            highlight.transform.localPosition = highlightBase + Vector3.up * Mathf.Sin(pulse) * 0.15f;
        }
    }
}

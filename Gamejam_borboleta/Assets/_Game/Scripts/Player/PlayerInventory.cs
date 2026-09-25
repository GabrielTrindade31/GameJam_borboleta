using System;
using System.Collections.Generic;
using UnityEngine;

namespace ButterflyStep
{
    [Serializable]
    public class InventoryItem
    {
        public string id;
        public string displayName;
        public Sprite icon;
    }

    public class PlayerInventory : MonoBehaviour
    {
        [Tooltip("Itens que o jogador carrega. Eles NÃO pertencem a nenhum dia: viajam no tempo junto com o Eco.")]
        [SerializeField] private List<InventoryItem> items = new List<InventoryItem>();

        public event Action Changed;
        public IReadOnlyList<InventoryItem> Items => items;

        public bool Has(string id) => Find(id) != null;

        public InventoryItem Find(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            foreach (var item in items)
            {
                if (item.id == id) return item;
            }
            return null;
        }

        public void Add(InventoryItem item)
        {
            if (item == null || string.IsNullOrEmpty(item.id)) return;
            items.Add(item);
            Changed?.Invoke();
        }

        public InventoryItem Remove(string id)
        {
            var item = Find(id);
            if (item == null) return null;
            items.Remove(item);
            Changed?.Invoke();
            return item;
        }
    }
}

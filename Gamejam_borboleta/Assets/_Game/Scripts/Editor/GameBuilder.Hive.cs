using UnityEngine;

namespace ButterflyStep.EditorTools
{
    public static partial class GameBuilder
    {
        private static void PowerChestAt(Transform parent, Vector2 position)
        {
            var chestGo = Go("Baú do Tempo", parent, position);
            var closed = Go("Fechado", chestGo.transform, position);
            Decor(closed.transform, tiles.hiveChestClosed, position, 1.4f, 3);
            if (tiles.hiveGem != null)
            {
                var gem = Go("Gema Azul", closed.transform, position + new Vector2(0f, 1.25f));
                var gemSr = AddSprite(gem, tiles.hiveGem, Color.white, 4);
                gemSr.sharedMaterial = unlitSprite;
                gem.AddComponent<PulseGlow>();
            }
            var open = Go("Aberto", chestGo.transform, position);
            Decor(open.transform, tiles.hiveChestOpen, position, 1.4f, 3);
            open.SetActive(false);
            var chest = chestGo.AddComponent<PowerChest>();
            Set(chest, "closedVisual", closed);
            Set(chest, "openVisual", open);
            var interact = Interact(chestGo.transform, position + new Vector2(0f, 0.6f), 1.2f, "L5_BauDoTempo", "Abrir o baú do tempo", "O baú guardava um eco do relógio de Eco.");
            UnityEditor.Events.UnityEventTools.AddPersistentListener(GetOnInteract(interact), chest.Unlock);
        }

        private static UnityEngine.Events.UnityEvent GetOnInteract(Interactable interact)
        {
            var field = typeof(Interactable).GetField("onInteract", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return (UnityEngine.Events.UnityEvent)field.GetValue(interact);
        }

        private static void HiveArena(GameObject hiveWall)
        {
            if (tiles == null || tiles.hiveCombDark == null) return;
            var hive = new GameObject("--- Colmeia").transform;

            var back = Go("Favos do Fundo", hive, new Vector2(65.25f, 12f));
            var backSr = AddSprite(back, tiles.hiveCombDark, new Color(0.95f, 0.82f, 0.7f), -18);
            backSr.drawMode = SpriteDrawMode.Tiled;
            backSr.size = new Vector2(19.5f, 10f);

            var left = Go("Parede Esquerda da Colmeia", hive, new Vector2(55.85f, 12f));
            var leftSr = AddSprite(left, tiles.hiveCombDark, new Color(1f, 0.9f, 0.75f), -17);
            leftSr.drawMode = SpriteDrawMode.Tiled;
            leftSr.size = new Vector2(0.7f, 10f);
            var roof = Go("Teto da Colmeia", hive, new Vector2(65.25f, 16.65f));
            var roofSr = AddSprite(roof, tiles.hiveCombDark, new Color(1f, 0.9f, 0.75f), -17);
            roofSr.drawMode = SpriteDrawMode.Tiled;
            roofSr.size = new Vector2(19.5f, 0.7f);
            foreach (var x in new[] { 57f, 61f, 65f, 69f, 73f }) Decor(hive, tiles.honeyDrip, new Vector2(x, 16.3f), 1f, -16);

            var rim = Go("Borda de Favos", hive, new Vector2(65.25f, 7.35f));
            var rimSr = AddSprite(rim, tiles.hiveCombDark, new Color(1f, 0.9f, 0.75f), -17);
            rimSr.drawMode = SpriteDrawMode.Tiled;
            rimSr.size = new Vector2(19.5f, 0.7f);

            foreach (var p in new[] { new Vector2(58.5f, 12.2f), new Vector2(65f, 14f), new Vector2(71.5f, 12.2f) })
            {
                var light = Shape("Luz da Janela", hive, p, new Vector2(2f, 2f), glowSprite, new Color(1f, 0.85f, 0.4f, 0.8f), -17);
                light.GetComponent<SpriteRenderer>().sharedMaterial = unlitSprite;
                light.AddComponent<PulseGlow>();
                var window = Go("Janela de Favo", hive, p);
                AddSprite(window, tiles.hiveWindow, new Color(0.72f, 0.6f, 0.5f), -16);
                window.transform.localScale = new Vector3(1.4f, 1.4f, 1f);
            }

            if (hiveWall != null)
            {
                var wallSr = hiveWall.GetComponent<SpriteRenderer>();
                var size = wallSr.size;
                wallSr.sprite = tiles.hiveCombDark;
                wallSr.size = size;
                wallSr.color = Color.white;
            }
            var arch = Go("Arco da Colmeia", hive, new Vector2(75.5f, 7f));
            AddSprite(arch, tiles.hiveArch, Color.white, 1);
            arch.transform.localScale = new Vector3(1.6f, 1.6f, 1f);

            foreach (var x in new[] { 60.2f, 70.3f })
            {
                Decor(hive, tiles.honeyDrip, new Vector2(x + 0.9f, 9.45f), 1f, 4);
                var nest = Go("Colmeia Pendurada", hive, new Vector2(x, 9.45f - (tiles.hive != null ? tiles.hive.bounds.extents.y : 0f)));
                if (tiles.hive != null) AddSprite(nest, tiles.hive, Color.white, 4);
            }

            Decor(hive, tiles.hiveDipper, new Vector2(56.4f, 7f), 1f, 3);
            PowerChestAt(hive, new Vector2(77.3f, 7f));

            if (tiles.mine != null)
            {
                var mine = Go("Entrada da Mina", hive, new Vector2(46.3f, 0f));
                AddSprite(mine, tiles.mine, Color.white, -1);
                mine.transform.localScale = new Vector3(0.9f, 0.9f, 1f);
            }
        }
    }
}

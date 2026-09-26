using UnityEngine;

namespace ButterflyStep.EditorTools
{
    public static partial class GameBuilder
    {
        private static Sprite ItemIcon(string id)
        {
            if (tiles == null) return diamondSprite;
            switch (id)
            {
                case "chave": return tiles.key;
                case "pinha": return tiles.pineSmall;
                case "semente": return tiles.sprout;
                case "engrenagem": return tiles.gear != null ? tiles.gear : diamondSprite;
                default: return diamondSprite;
            }
        }

        private static ItemPickup Pickup(string id, string displayName, Vector2 position, string message, params TemporalCondition[] existsWhen)
        {
            var go = Go($"Item_{id}", groupInteract, position);
            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.6f;

            var visual = Go("Visual", go.transform, position);
            var sparkle = MakeParticles("Brilho", visual.transform, new Color(1f, 0.92f, 0.5f), 0, 0.5f, 1f, 0.12f, false, 0.45f, -0.05f);
            var sparkleEmission = sparkle.emission;
            sparkleEmission.rateOverTime = 10f;
            var sparkleMain = sparkle.main;
            sparkleMain.loop = true;
            sparkleMain.playOnAwake = true;
            var icon = ItemIcon(id);
            var iconGo = Go("Icone", visual.transform, position);
            AddSprite(iconGo, icon, Color.white, 10);
            if (icon != null)
            {
                float size = Mathf.Max(icon.bounds.size.x, icon.bounds.size.y);
                float scale = size > 0.01f ? 0.7f / size : 1f;
                iconGo.transform.localScale = new Vector3(scale, scale, 1f);
                iconGo.transform.position = position - new Vector2(0f, icon.bounds.center.y * scale);
            }

            var pickup = go.AddComponent<ItemPickup>();
            pickup.Setup(id, displayName, icon, message, visual.transform, existsWhen);
            return pickup;
        }

        private static void CheckpointAt(float x, float y)
        {
            var go = Go("Flor do Tempo (checkpoint)", groupTriggers, new Vector2(x, y));
            var col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(1.6f, 3f);
            col.offset = new Vector2(0f, 1.4f);
            var visual = Go("Visual", go.transform, new Vector2(x, y));
            var sr = AddSprite(visual, tiles != null ? tiles.blueFlower : diamondSprite, Color.white, 4);
            visual.transform.localScale = new Vector3(1.8f, 1.8f, 1f);
            var glow = MakeParticles("Brilho", go.transform, new Color(0.55f, 0.85f, 1f), 0, 0.6f, 1.2f, 0.14f, false, 0.4f, -0.08f);
            glow.transform.localPosition = new Vector3(0f, 1.1f, 0f);
            var em = glow.emission;
            em.rateOverTime = 14f;
            var main = glow.main;
            main.loop = true;
            go.AddComponent<Checkpoint>().Setup(sr, glow);
        }

        private static void Pollen(int id, float x, float y, params TemporalCondition[] existsWhen)
        {
            var go = Go($"Pólen {id + 1}", groupInteract, new Vector2(x, y));
            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.5f;
            var visual = Go("Visual", go.transform, new Vector2(x, y));
            var ring = Go("Relógio", visual.transform, new Vector2(x, y));
            if (fxLibrary != null && fxLibrary.timeClock != null)
            {
                var ringSr = AddSprite(ring, fxLibrary.timeClock, new Color(0.85f, 0.95f, 1f, 0.8f), 11);
                ringSr.sharedMaterial = unlitSprite;
                ring.transform.localScale = new Vector3(0.75f, 0.75f, 1f);
            }
            var aura = Shape("Aura", visual.transform, new Vector2(x, y), new Vector2(1.6f, 1.6f), glowSprite, new Color(0.55f, 0.85f, 1f, 0.7f), 10);
            aura.GetComponent<SpriteRenderer>().sharedMaterial = unlitSprite;
            SpriteRenderer sr;
            if (fxLibrary != null && fxLibrary.butterflies.Length > 6)
            {
                var wing = Go("Borboleta", visual.transform, new Vector2(x, y));
                var body = Go("Asas", wing.transform, new Vector2(x, y));
                sr = AddSprite(body, fxLibrary.butterflies[6], Color.white, 13);
                sr.sharedMaterial = unlitSprite;
                wing.AddComponent<Butterfly>().Setup(sr, 1f, new Vector2(0.12f, 0.06f), 1.6f);
            }
            else sr = AddSprite(Go("Grão", visual.transform, new Vector2(x, y)), diamondSprite, new Color(1f, 0.85f, 0.35f), 12);
            var sparkle = MakeParticles("Poeira do Tempo", visual.transform, new Color(0.65f, 0.8f, 1f), 0, 0.4f, 1.1f, 0.1f, true, 0.3f, -0.05f);
            var em = sparkle.emission;
            em.rateOverTime = 8f;
            var main = sparkle.main;
            main.loop = true;
            main.playOnAwake = true;
            var pollen = go.AddComponent<PollenCollectible>();
            pollen.Setup(id, visual.transform, sr, existsWhen);
            Set(pollen, "ring", ring.transform);
        }

        private static TemporalObject Gate(string name, float xMin, float yMin, float w, float h, string openFlag, bool freezes)
        {
            var gate = TemporalRect(name, xMin, yMin, w, h, DoorColor, 2, true);
            if (tiles != null && tiles.gate != null) gate.GetComponent<SpriteRenderer>().sprite = tiles.gate;
            var closed = gate.AddState("Trancado");
            closed.overrideColor = true;
            closed.color = Color.white;
            if (freezes)
            {
                var frozen = gate.AddState("Congelado (inverno)", TemporalCondition.In(Season.Inverno));
                frozen.overrideColor = true;
                frozen.color = new Color(0.7f, 0.85f, 1f);
            }
            var open = Hidden(gate.AddState("Aberto", TemporalCondition.Flag(openFlag)));
            open.isConsequence = true;
            return gate;
        }

        private static Interactable KeyLock(Vector2 position, string openFlag, string itemId, string itemName, string blocked, params TemporalCondition[] conditions)
        {
            var interact = Interact(groupInteract, position, 1.1f, openFlag, "Abrir com a chave", "O portão se abriu.", conditions);
            interact.RequireItem(itemId, itemName, true, blocked);
            return interact;
        }

        private static void PlantedPine(string name, float x, float groundY, string flag, int growDays, string itemId, string itemName, string prompt, string blocked, Vector3[] branches, params TemporalCondition[] plantWhen)
        {
            var tree = TemporalHolder(name, new Vector2(x, groundY));
            var soil = Holder(tree.transform, "TerraFertil", new Vector2(x, groundY));
            Part(soil.transform, "Terra", x - 0.7f, groundY, 1.4f, 0.15f, new Color(0.3f, 0.2f, 0.12f), 3, false, squareSprite);
            var sprout = Holder(tree.transform, "Broto", new Vector2(x, groundY));
            if (tiles != null) Decor(sprout.transform, tiles.sprout, new Vector2(x, groundY), 1f, 5);
            else Part(sprout.transform, "Broto", x - 0.1f, groundY, 0.2f, 0.6f, new Color(0.45f, 0.8f, 0.35f), 5, false, squareSprite);

            var grown = Holder(tree.transform, "Pinheiro", new Vector2(x, groundY));
            float top = 0f;
            foreach (var b in branches) top = Mathf.Max(top, b.y - groundY);
            if (tiles != null) SeasonalTree(grown.transform, new Vector2(x, groundY), 2, Mathf.Max(0.6f, (top + 3f) / 11.5f), 2);
            else Part(grown.transform, "Tronco", x - 0.4f, groundY, 0.8f, 6f, WoodColor, 4, false);
            for (int i = 0; i < branches.Length; i++) Branch($"Galho {i + 1}", grown.transform, x, branches[i].x, branches[i].y, branches[i].z);

            With(tree.AddState("Terra fértil"), soil);
            With(tree.AddState("Semente plantada", TemporalCondition.Flag(flag)), soil, sprout);
            Consequence(With(tree.AddState($"Pinheiro crescido (+{growDays} dias)", TemporalCondition.Flag(flag, growDays)), grown));

            var interact = Interact(tree.transform, new Vector2(x, groundY + 0.7f), 1.2f, flag, prompt, "Plantado! Agora é esperar o tempo passar.", plantWhen);
            interact.RequireItem(itemId, itemName, true, blocked);
        }
    }
}

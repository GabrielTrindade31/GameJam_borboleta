using UnityEngine;

namespace ButterflyStep.EditorTools
{
    public static partial class GameBuilder
    {
        private static Sprite FirstFrame(ArtSet art, string clip)
        {
            if (art == null) return null;
            foreach (var c in art.clips)
            {
                if (c.name == clip && c.frames.Length > 0) return c.frames[0];
            }
            return art.First;
        }

        private static TemporalBoss Boss(string name, Vector2 spawn, ArtSet art, string startClip, Vector2 bodySize, int health, string deathFlag, float arenaLeft, float arenaRight, float flyHeight, Color placeholderColor)
        {
            var root = new GameObject(name) { layer = enemyLayer };
            root.transform.SetParent(groupEnemies, false);
            root.transform.position = spawn;
            var body = root.AddComponent<Rigidbody2D>();
            body.gravityScale = 3f;
            body.mass = 50f;
            body.freezeRotation = true;
            body.sharedMaterial = noFriction;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            var col = root.AddComponent<BoxCollider2D>();
            col.size = bodySize;
            col.offset = new Vector2(0f, bodySize.y * 0.5f);
            col.sharedMaterial = noFriction;

            var visual = new GameObject("Visual") { layer = enemyLayer };
            visual.transform.SetParent(root.transform, false);
            SpriteRenderer sr;
            SpriteAnimator animator = null;
            if (art != null)
            {
                sr = AddSprite(visual, FirstFrame(art, startClip), Color.white, 15);
                animator = AttachAnimator(visual, sr, art, startClip);
                animator.SetAffectedByStasis(true);
            }
            else
            {
                visual.transform.localPosition = new Vector3(0f, bodySize.y * 0.5f, 0f);
                visual.transform.localScale = new Vector3(bodySize.x, bodySize.y, 1f);
                sr = AddSprite(visual, squareSprite, placeholderColor, 15);
            }

            var hurt = new GameObject("Hurtbox") { layer = 0 };
            hurt.transform.SetParent(root.transform, false);
            var hurtCol = hurt.AddComponent<BoxCollider2D>();
            hurtCol.isTrigger = true;
            hurtCol.size = bodySize * 0.9f;
            hurtCol.offset = new Vector2(0f, bodySize.y * 0.5f);

            var remains = new GameObject("Restos");
            remains.transform.SetParent(root.transform, false);
            var remainsSprite = FirstFrame(art, startClip);
            var rs = AddSprite(remains, remainsSprite != null ? remainsSprite : squareSprite, new Color(0.45f, 0.42f, 0.45f), 11);
            rs.flipY = true;
            if (art != null) rs.flipX = art.facesLeft;
            remains.SetActive(false);

            var boss = root.AddComponent<TemporalBoss>();
            var minion = slimePrefab != null ? slimePrefab.GetComponent<TemporalEnemy>() : null;
            boss.Setup(name, health, deathFlag, arenaLeft, arenaRight, sr, animator, art != null && art.facesLeft, col, hurtCol, remains, projectilePrefab, shockwavePrefab, minion);
            Set(boss, "flyHeight", flyHeight);
            return boss;
        }

        private static TemporalObject SeasonalHazard(string name, float xMin, float yMin, float w, float h, Season season, Sprite decor, Color decorTint, float decorScale)
        {
            var hazard = TemporalHolder(name, new Vector2(xMin + w * 0.5f, yMin));
            var active = Holder(hazard.transform, "Ativo", new Vector2(xMin + w * 0.5f, yMin));
            var zone = Go("Zona Mortal", active.transform, new Vector2(xMin + w * 0.5f, yMin + h * 0.5f));
            var col = zone.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(w, h);
            zone.AddComponent<DeathZone>();
            if (decor != null)
            {
                int count = Mathf.Max(1, Mathf.RoundToInt(w / 1.2f));
                for (int i = 0; i < count; i++)
                {
                    float x = xMin + (i + 0.5f) * w / count;
                    float y = yMin + ((i * 37) % 5) * 0.12f * h;
                    Decor(active.transform, decor, new Vector2(x, y), decorScale * (0.8f + (i % 3) * 0.15f), 6, decorTint);
                }
            }
            hazard.AddState("Seguro");
            With(hazard.AddState($"Perigo ({SeasonCalendar.Name(season)})", TemporalCondition.In(season)), active);
            return hazard;
        }

        private static TemporalObject SeasonalWall(string name, float xMin, float yMin, float w, float h, Season season, Color color)
        {
            var wall = TemporalRect(name, xMin, yMin, w, h, color, 2, true);
            Hidden(wall.AddState("Sem bloqueio"));
            var blocked = wall.AddState($"Bloqueado ({SeasonCalendar.Name(season)})", TemporalCondition.In(season));
            blocked.overrideColor = true;
            blocked.color = color;
            return wall;
        }

        private static TemporalObject River(string name, float xMin, float yTop, float w, float depth, bool padsInSummer)
        {
            var river = TemporalHolder(name, new Vector2(xMin + w * 0.5f, yTop));
            Rect("Leito", groupEnv, xMin, yTop - depth - 1f, w, 1f, GroundColor, 0, true);
            var water = Holder(river.transform, "Agua", new Vector2(xMin + w * 0.5f, yTop));
            Water(water.transform, "Correnteza", xMin, yTop - 0.6f, w, depth - 0.6f, new Vector2(-4f, 0f));
            var ice = Holder(river.transform, "Gelo", new Vector2(xMin + w * 0.5f, yTop));
            FrozenWater(ice.transform, xMin, yTop - 0.2f, w, depth - 0.2f, 0.5f);
            With(river.AddState("Correnteza"), water);
            With(river.AddState("Congelado", TemporalCondition.In(Season.Inverno)), ice);
            if (padsInSummer)
            {
                var pads = Holder(river.transform, "VitoriasRegias", new Vector2(xMin + w * 0.5f, yTop));
                int count = Mathf.Max(2, Mathf.RoundToInt(w / 2.6f));
                for (int i = 0; i < count; i++)
                {
                    float x = xMin + (i + 0.5f) * w / count;
                    var pad = OneWay("Vitória-régia", pads.transform, x - 0.6f, yTop - 0.45f, 1.2f, LilyColor, 4);
                    if (tiles != null && tiles.lilyPad != null)
                    {
                        pad.GetComponent<SpriteRenderer>().enabled = false;
                        float s = 1.35f / tiles.lilyPad.bounds.size.x;
                        var leaf = Go("Folha", pad.transform, new Vector2(x, yTop - 0.35f - tiles.lilyPad.bounds.extents.y * s));
                        AddSprite(leaf, tiles.lilyPad, Color.white, 7);
                        leaf.transform.localScale = new Vector3(s, s, 1f);
                    }
                }
                With(river.AddState("Vitórias-régias (verão)", TemporalCondition.In(Season.Verao)), water, pads);
            }
            return river;
        }
    }
}

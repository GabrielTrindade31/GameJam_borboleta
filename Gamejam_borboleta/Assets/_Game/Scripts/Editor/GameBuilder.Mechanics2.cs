using UnityEngine;

namespace ButterflyStep.EditorTools
{
    public static partial class GameBuilder
    {
        private static TemporalObject BounceMushroom(string name, float x, float groundY, float velocity, params TemporalCondition[] growsWhen)
        {
            var t = TemporalHolder(name, new Vector2(x, groundY));
            var sprout = Holder(t.transform, "Brotinho", new Vector2(x, groundY));
            if (tiles != null) Decor(sprout.transform, tiles.mushroomSmall, new Vector2(x, groundY), 0.9f, 4);
            var grown = Holder(t.transform, "Cogumelo Gigante", new Vector2(x, groundY));
            var visual = Go("Chapéu", grown.transform, new Vector2(x, groundY));
            float height = 2.2f;
            if (tiles != null && tiles.mushroom != null)
            {
                float s = height / tiles.mushroom.bounds.size.y;
                AddSprite(visual, tiles.mushroom, Color.white, 4);
                visual.transform.localScale = new Vector3(s * 1.1f, s, 1f);
            }
            else Shape("Chapéu", visual.transform, new Vector2(x, groundY + 1.8f), new Vector2(2f, 0.8f), circleSprite, new Color(0.9f, 0.35f, 0.3f), 4);
            var pad = Go("Mola", grown.transform, new Vector2(x, groundY + height - 0.35f));
            var col = pad.AddComponent<BoxCollider2D>();
            col.size = new Vector2(1.7f, 0.5f);
            pad.AddComponent<BouncePad>().Setup(velocity, visual.transform);
            With(t.AddState("Brotinho"), sprout);
            Consequence(With(t.AddState("Cogumelo gigante: pule nele", growsWhen), grown));
            return t;
        }

        private static TemporalObject SeasonWind(string name, float xMin, float yMin, float w, float h, Vector2 force, Season season, Color leafColor)
        {
            var t = TemporalHolder(name, new Vector2(xMin + w * 0.5f, yMin + h * 0.5f));
            var gust = Holder(t.transform, "Rajada", new Vector2(xMin + w * 0.5f, yMin + h * 0.5f));
            var box = gust.AddComponent<BoxCollider2D>();
            box.size = new Vector2(w, h);
            gust.AddComponent<WindZone2D>().Setup(force);
            var ps = MakeParticles("Folhas ao Vento", gust.transform, leafColor, 0, 0.01f, 2.2f, 0.16f, true, 0.1f);
            ps.transform.position = new Vector3(force.x >= 0f ? xMin : xMin + w, yMin + h * 0.5f, 0f);
            var main = ps.main;
            main.loop = true;
            main.playOnAwake = true;
            main.duration = 1f;
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(0.2f, h, 0.1f);
            var em = ps.emission;
            em.rateOverTime = h * 6f;
            var vel = ps.velocityOverLifetime;
            vel.enabled = true;
            vel.space = ParticleSystemSimulationSpace.World;
            float speed = Mathf.Abs(force.x) * 0.45f + 2f;
            vel.x = new ParticleSystem.MinMaxCurve(Mathf.Sign(force.x == 0f ? 1f : force.x) * speed * 0.7f, Mathf.Sign(force.x == 0f ? 1f : force.x) * speed * 1.2f);
            vel.y = new ParticleSystem.MinMaxCurve(force.y * 0.3f - 0.6f, force.y * 0.3f + 0.6f);
            vel.z = new ParticleSystem.MinMaxCurve(0f, 0f);
            var leaf = season == Season.Inverno ? snowFlake : season == Season.Outono ? tiles != null ? tiles.leafDry : null : tiles != null ? tiles.leafGreen : null;
            if (leaf != null)
            {
                var sheet = ps.textureSheetAnimation;
                sheet.enabled = true;
                sheet.mode = ParticleSystemAnimationMode.Sprites;
                sheet.SetSprite(0, leaf);
                main.startSize = new ParticleSystem.MinMaxCurve(season == Season.Inverno ? 0.22f : 0.4f, season == Season.Inverno ? 0.3f : 0.55f);
                var sol = ps.sizeOverLifetime;
                sol.enabled = false;
                var rot = ps.rotationOverLifetime;
                rot.enabled = true;
                rot.z = new ParticleSystem.MinMaxCurve(-3f, 3f);
            }
            var r = ps.GetComponent<ParticleSystemRenderer>();
            r.sortingOrder = 20;
            if (leaf != null) r.sharedMaterial = unlitSprite;
            main.startLifetime = w / speed;
            t.AddState("Calmo");
            With(t.AddState($"Vento de {SeasonCalendar.Name(season).ToLower()}", TemporalCondition.In(season)), gust);
            return t;
        }

        private static TemporalObject ClimbVine(string name, float x, float groundY, float height, params TemporalCondition[] grownWhen)
        {
            var t = TemporalHolder(name, new Vector2(x, groundY));
            var seedling = Holder(t.transform, "Muda", new Vector2(x, groundY));
            if (tiles != null) Decor(seedling.transform, tiles.leafPlant, new Vector2(x, groundY), 0.8f, 4);
            var vine = Holder(t.transform, "Trepadeira", new Vector2(x, groundY));
            BuildVine(vine.transform, x, groundY, height, false);
            var zone = Go("Escalada", vine.transform, new Vector2(x, groundY + height * 0.5f));
            zone.AddComponent<BoxCollider2D>().size = new Vector2(0.9f, height);
            zone.AddComponent<ClimbZone>();
            var dry = Holder(t.transform, "Seca", new Vector2(x, groundY));
            BuildVine(dry.transform, x, groundY, height * 0.6f, true);
            With(t.AddState("Muda"), seedling);
            Consequence(With(t.AddState("Trepadeira crescida: dá para escalar (W/S)", grownWhen), vine));
            With(t.AddState("Trepadeira seca pelo outono", TemporalCondition.In(Season.Outono)), dry);
            With(t.AddState("Trepadeira seca pelo inverno", TemporalCondition.In(Season.Inverno)), dry);
            return t;
        }

        private static void RockPassage(string name, float xMin, float w, float gapHeight)
        {
            Wall(xMin, gapHeight, w, 60f - gapHeight, name);
            if (tiles == null) return;
            Part(groupEnv, name + " (túnel)", xMin, 0f, w, gapHeight, new Color(0.25f, 0.23f, 0.27f), -5, false, tiles.groundFill);
            Decor(groupEnv, tiles.boulders, new Vector2(xMin - 0.3f, 0f), 0.7f, -4);
            Decor(groupEnv, tiles.boulders, new Vector2(xMin + w + 0.3f, 0f), 0.7f, -4);
        }

        private static void ArenaPlatform(float xMin, float yTop, float w)
        {
            var platform = OneWay("Plataforma", groupEnv, xMin, yTop, w, WoodColor);
            foreach (float x in new[] { xMin + 0.35f, xMin + w - 0.6f })
                Part(platform.transform, "Poste", x, 0f, 0.25f, yTop - 0.3f, WoodColor, -1, false);
            Part(platform.transform, "Travessa", xMin + 0.35f, yTop * 0.45f, w - 0.7f, 0.18f, WoodColor, -1, false);
        }

        private static void ThornVines(Transform parent, float xMin, float w, float height)
        {
            if (tiles == null || brambleSprite == null) return;
            int stems = Mathf.Max(3, Mathf.RoundToInt(w / 0.3f));
            for (int i = 0; i < stems; i++)
            {
                float x = xMin + 0.15f + i * (w - 0.3f) / (stems - 1);
                var stem = Go("Espinhos", parent, new Vector2(x, height * 0.5f));
                var sr = AddSprite(stem, brambleSprite, Color.white, 3 + i % 2);
                sr.drawMode = SpriteDrawMode.Tiled;
                sr.size = new Vector2(brambleSprite.bounds.size.x, height - (i % 2) * 0.4f);
                stem.transform.localScale = new Vector3(i % 2 == 0 ? 1f : -1f, 1f, 1f);
                stem.transform.position = new Vector3(x, (height - (i % 2) * 0.4f) * 0.5f, 0f);
            }
            if (tiles.lavender == null) return;
            for (float x = xMin + 0.2f; x < xMin + w - 0.1f; x += 0.45f)
                Decor(parent, tiles.lavender, new Vector2(x, 0f), 1.2f, 5);
        }

        private static void FallenTree(Transform parent, float baseX, float groundY, float length)
        {
            float thick = 2.5f;
            if (tiles.stump != null)
            {
                float s = thick / tiles.stump.bounds.size.x;
                var log = Go("Tronco Deitado", parent, new Vector2(baseX, groundY + thick * 0.5f));
                var sr = AddSprite(log, tiles.stump, new Color(1f, 0.92f, 0.85f), 4);
                sr.drawMode = SpriteDrawMode.Tiled;
                sr.size = new Vector2(tiles.stump.bounds.size.x, length / s);
                log.transform.localScale = new Vector3(s, s, 1f);
                log.transform.rotation = Quaternion.Euler(0f, 0f, 90f);
            }
            if (tiles.bush == null) return;
            float[] xs = { 0.15f, 0.02f, 0.62f, 0.95f };
            for (int i = 0; i < xs.Length; i++)
            {
                float x = baseX - length * xs[i];
                bool top = i % 2 == 0;
                var bush = Go("Arbusto do Tronco", parent, new Vector2(x, top ? groundY + thick - 0.55f : groundY - 0.05f));
                var bsr = AddSprite(bush, tiles.bush, Color.white, top ? 5 : 6);
                float sc = (top ? 3.2f : 2.6f) / tiles.bush.bounds.size.x;
                bush.transform.localScale = new Vector3(i % 3 == 0 ? -sc : sc, sc, 1f);
                var seasonal = bush.AddComponent<SeasonalSprite>();
                seasonal.Setup(bsr, null, null, null, null);
                Set(seasonal, "spring", tiles.bush);
                Set(seasonal, "autumn", bushDry);
                Set(seasonal, "winter", bushSnow);
                Set(seasonal, "springTint", Color.white);
                Set(seasonal, "summerTint", new Color(1f, 1f, 1f, 0f));
                Set(seasonal, "autumnTint", Color.white);
                Set(seasonal, "winterTint", Color.white);
            }
            if (tiles.leafDry == null) return;
            var fallenLeaves = Go("Folhas Secas", parent, new Vector2(baseX - length * 0.5f, groundY));
            for (int i = 0; i < 9; i++)
            {
                var leaf = Go("Folha Seca", fallenLeaves.transform, new Vector2(baseX + 0.4f - i * (length + 0.8f) / 8f, groundY + 0.12f + (i % 3) * 0.05f));
                AddSprite(leaf, tiles.leafDry, Color.white, 7);
                leaf.transform.rotation = Quaternion.Euler(0f, 0f, i * 53f);
                leaf.transform.localScale = new Vector3(0.8f, 0.8f, 1f);
            }
            fallenLeaves.AddComponent<SeasonalActive>().Setup(Season.Outono);
        }

        private static void BuildVine(Transform parent, float x, float groundY, float height, bool dry)
        {
            if (tiles == null || vineSprite == null)
            {
                Part(parent, "Caule", x - 0.07f, groundY, 0.14f, height, dry ? new Color(0.5f, 0.38f, 0.25f) : new Color(0.35f, 0.62f, 0.3f), 4, false, squareSprite);
                return;
            }
            var stem = Go("Caule", parent, new Vector2(x, groundY + height * 0.5f));
            var sr = AddSprite(stem, vineSprite, dry ? new Color(0.75f, 0.55f, 0.35f) : Color.white, 4);
            sr.drawMode = SpriteDrawMode.Tiled;
            sr.size = new Vector2(vineSprite.bounds.size.x, height);
            int i = 0;
            for (float y = groundY + 0.35f; y < groundY + height - 0.15f; y += 0.55f, i++)
            {
                var sprite = dry ? tiles.leafDry : i % 2 == 0 ? tiles.leafGreen : tiles.leafGreen2;
                if (sprite == null) continue;
                bool right = i % 2 == 0;
                var leaf = Go("Folha", parent, new Vector2(x + (right ? 0.28f : -0.28f), y));
                AddSprite(leaf, sprite, Color.white, 5);
                leaf.transform.localScale = new Vector3(right ? 1f : -1f, 1f, 1f);
            }
        }
    }
}

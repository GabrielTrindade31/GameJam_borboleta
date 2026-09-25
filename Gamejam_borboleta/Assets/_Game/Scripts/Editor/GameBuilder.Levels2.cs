using UnityEngine;

namespace ButterflyStep.EditorTools
{
    public static partial class GameBuilder
    {
        private static readonly Color SnowColor = new Color(0.93f, 0.97f, 1f);
        private static readonly Color LilyColor = new Color(0.35f, 0.75f, 0.35f);

        private static TemporalObject SnowDrift(string name, float xMin, float yMin, float w, float h, bool climbable = true)
        {
            var drift = TemporalHolder(name, new Vector2(xMin + w * 0.5f, yMin));
            var snow = Holder(drift.transform, "Neve", new Vector2(xMin + w * 0.5f, yMin));
            SnowMound(snow.transform, xMin, yMin, w, h, climbable);
            drift.AddState("Sem neve");
            With(drift.AddState(climbable ? "Monte de neve (inverno): dá para escalar" : "Nevasca (inverno): fecha a passagem", TemporalCondition.In(Season.Inverno)), snow);
            return drift;
        }

        private static void BuildLevel06()
        {
            var scene = BeginLevel(new LevelSpec
            {
                title = "Capítulo 6 — O Futuro Devorado",
                intro = "Inverno. Algo devorou o futuro deste vale.\nPara seguir em frente, Eco vai precisar voltar.",
                complete = "Um roedor a menos na primavera, um caminho aberto no inverno.",
                next = "Level07",
                season = Season.Primavera, maxDay = 90, step = 30, startIndex = 3, stasis = true,
                spawn = new Vector2(-9f, 1f),
                camMin = new Vector2(-13f, -4f), camMax = new Vector2(55f, 16f)
            });

            Wall(-14f, -10f, 1f, 26f, "Limite");
            Wall(54f, -10f, 1f, 26f, "Limite");
            Ground(-13f, -5f, 25f, 5f);
            River("Rio", 12f, 0f, 10f, 3.4f, false);
            Ground(22f, -5f, 32f, 5f);
            Kill(12f, -9f, 10f);

            const string gnawerDead = "L6_RoedorAfastado";
            Rule("L6_ArvoreCaiu", "Roedor vivo no dia 31 → a árvore roída cai no outono", TemporalCondition.NotFlag(gnawerDead, 30), TemporalCondition.Since(60));

            var tree = TemporalHolder("Árvore roída", new Vector2(9f, 0f));
            var standing = Holder(tree.transform, "Em pé", new Vector2(9f, 0f));
            if (tiles != null) SeasonalTree(standing.transform, new Vector2(9.5f, 0f), 1, 1.1f, 2);
            else Part(standing.transform, "Tronco", 9f, 0f, 0.9f, 6f, WoodColor, 3, false);
            var fallen = Holder(tree.transform, "Caída", new Vector2(6f, 0f));
            if (tiles != null)
            {
                Rect("Tronco caído (bloqueio)", fallen.transform, 2.2f, 0f, 7.3f, 3.2f, Color.clear, 3, true, squareSprite);
                FallenTree(fallen.transform, 9.4f, 0f, 7.6f);
            }
            else Rect("Tronco caído (bloqueio)", fallen.transform, 3f, 0f, 6.5f, 4f, WoodColor, 3, true);
            With(tree.AddState("Em pé"), standing);
            Consequence(With(tree.AddState("Caída: bloqueia o caminho", TemporalCondition.Flag("L6_ArvoreCaiu")), fallen));
            Enemy(chronoferaPrefab, "Roedor da Árvore", 10.5f, 0f, 1f, 1f, gnawerDead);

            SnowDrift("Monte de neve", 30f, 0f, 5.2f, 4.5f);
            SeasonWind("Nevasca do Corredor", 26f, 0f, 24f, 7f, new Vector2(-9f, 0f), Season.Inverno, new Color(0.95f, 0.97f, 1f, 0.9f));
            Enemy(thornPrefab, "Espinheiro do Corredor", 37f, 0f, 0f, 0f);
            Enemy(thornPrefab, "Espinheiro do Corredor", 42f, 0f, 0f, 0f);
            Enemy(thornPrefab, "Espinheiro do Corredor", 46.5f, 0f, 0f, 0f);
            Enemy(slimePrefab, "Mãe Lodo", 44.5f, 0f, 1.5f, 1.5f, "L6_MaeLodo");
            Offspring(slimePrefab, "Filhote de Lodo (nasce no dia 31)", 39.5f, 0f, 1.5f, 1.5f, 30, "L6_MaeLodo");
            Offspring(slimePrefab, "Filhote de Lodo (nasce no dia 61)", 41.5f, 0f, 1.5f, 1.5f, 60, "L6_MaeLodo");
            Ground(47.5f, 0f, 6.5f, 5f);
            ClimbVine("Trepadeira do Paredão", 47.1f, 0f, 5.6f);
            Sign(45.6f, 0f, "A saída fica no alto do paredão.\nNa primavera e no verão uma trepadeira cobre a rocha: segure W para subir.");
            Sign(35f, 0f, "Uma mãe lodo vive neste corredor. Seus filhotes nascem nos dias 31 e 61...\nse ela ainda estiver viva quando chegar a hora.");

            Sign(-9f, 0f, "Esta fase começa no INVERNO. Você pode voltar no tempo (Q) até a primavera.\nSegure SHIFT + Q para ESPIAR outro dia sem sair do lugar.");
            Sign(-3f, 0f, "Uma árvore caída bloqueia a trilha. Quem será que a derrubou?\nDica: sua posição continua a mesma quando o tempo muda.");
            Sign(26f, 0f, "No inverno a nevasca sopra contra você e a neve cobre o corredor: suba pelos degraus.\nNas outras estações os espinheiros atiram. C PAUSA O TEMPO.");
            Scenery(0f, -11f, -6f, 25f);
            Scenery(5f, 50.4f);
            Exit(52f, 6.2f);
            Save(scene, "Level06");
        }

        private static void BuildLevel07()
        {
            var scene = BeginLevel(new LevelSpec
            {
                title = "Capítulo 7 — O Nível da Água",
                intro = "Outono. Uma semente na ilha do lago, um penhasco sem caminho.\nO lago muda com as estações.",
                complete = "Gelo, vitórias-régias e uma semente plantada na hora certa.",
                next = "Level08",
                season = Season.Primavera, maxDay = 90, step = 30, startIndex = 2, stasis = true,
                spawn = new Vector2(-9f, 1f),
                camMin = new Vector2(-13f, -4f), camMax = new Vector2(47f, 18f)
            });

            Wall(-14f, -10f, 1f, 30f, "Limite");
            Wall(46f, -10f, 1f, 30f, "Limite");
            Ground(-13f, -5f, 23f, 5f);
            River("Lago", 10f, 0f, 14f, 3.4f, true);
            Ground(15.8f, -3.4f, 2.4f, 3.9f);
            if (tiles != null)
            {
                Decor(groupEnv, tiles.cattail, new Vector2(16.1f, 0.5f), 1f, 3);
                Decor(groupEnv, tiles.cattail, new Vector2(17.9f, 0.5f), 0.8f, 3);
            }
            Ground(24f, -5f, 12f, 5f);
            Ground(36f, -5f, 10f, 13f);
            Kill(10f, -9f, 14f);

            Pickup("semente", "Semente do lago", new Vector2(17f, 1.3f), "Uma semente na ilha! Ela vai com você para qualquer dia.");
            PlantedPine("Pinheiro do Penhasco", 29f, 0f, "L7_SementePlantada", 60, "semente", "Semente do lago", "Plantar a semente", "Terra fértil. Só brota se plantada na primavera — e leva 60 dias.",
                new[] { new Vector3(27f, 2.7f, 3f), new Vector3(28.6f, 5.2f, 3f), new Vector3(30.2f, 7.7f, 3.4f), new Vector3(33.2f, 8.2f, 2.6f) },
                TemporalCondition.In(Season.Primavera));

            Enemy(slimePrefab, "Lodo da Margem", 4f, 0f, 3f, 3f);
            Enemy(mothPrefab, "Lagarta da Margem", 32f, 0f, 1.5f, 1.5f, "L7_LagartaMorta");
            Enemy(thornPrefab, "Espinheiro do Penhasco", 40f, 8f, 0f, 0f);

            Sign(-9f, 0f, "O lago é fundo demais para nadar. Mas no inverno ele congela...\nE no verão as vitórias-régias florescem.");
            Sign(26f, 0f, "Terra fértil. Uma semente aqui viraria uma árvore alta em 60 dias.\nMas só brota se plantada na primavera.");
            Scenery(0f, -11f, -4f, 6f);
            Scenery(8f, 44f);
            Exit(42.5f, 9.2f);
            Save(scene, "Level07");
        }

        private static void BuildLevel08()
        {
            var scene = BeginLevel(new LevelSpec
            {
                title = "Capítulo 8 — Estações em Sequência",
                intro = "Outono. Cada trecho do caminho só é seguro em uma estação.\nAvance, volte e avance de novo.",
                complete = "Deslizamentos, rio, neve e espinhos. Nenhuma estação sozinha abria o caminho.",
                next = "Level09",
                season = Season.Primavera, maxDay = 90, step = 30, startIndex = 2, stasis = true,
                spawn = new Vector2(-9f, 1f),
                camMin = new Vector2(-13f, -4f), camMax = new Vector2(62f, 16f)
            });

            Wall(-14f, -10f, 1f, 26f, "Limite");
            Wall(61f, -10f, 1f, 26f, "Limite");
            Ground(-13f, -5f, 29f, 5f);
            River("Rio", 16f, 0f, 10f, 3.4f, false);
            Ground(26f, -5f, 24f, 5f);
            Ground(57.5f, -5f, 3.5f, 5f);
            Kill(16f, -9f, 10f);
            Kill(50f, -9f, 7.5f);
            SeasonWind("Vento de Outono", 47.8f, 0f, 11.2f, 7f, new Vector2(12f, 0f), Season.Outono, new Color(1f, 0.75f, 0.45f));

            SeasonalHazard("Deslizamento de pedras", 3f, 0f, 9f, 3f, Season.Outono, tiles != null ? tiles.rockSmall : null, Color.white, 0.5f);
            SnowDrift("Nevasca", 32f, 0f, 5f, 5f, false);
            SeasonalHazard("Espinhos floridos", 42f, 0f, 8f, 1.6f, Season.Verao, tiles != null ? tiles.bush : null, new Color(0.9f, 0.55f, 0.85f), 0.3f);

            Enemy(slimePrefab, "Mãe Lodo", 29f, 0f, 2f, 2f, "L8_MaeLodo");
            Offspring(slimePrefab, "Filhote de Lodo (nasce no dia 31)", 39f, 0f, 1.5f, 1.5f, 30, "L8_MaeLodo");
            Offspring(slimePrefab, "Filhote de Lodo (nasce no dia 61)", 47f, 0f, 1.5f, 1.5f, 60, "L8_MaeLodo");
            Sign(27.5f, 0f, "Derrote a mãe lodo cedo e os filhotes dela nunca vão nascer.\nDerrote tarde, e eles já estarão esperando mais adiante.");
            Sign(45.4f, 0f, "Um abismo largo demais para um pulo comum.\nNo outono, o vento da encosta sopra forte para a direita...");
            Enemy(chronoferaPrefab, "Cronofera da Encosta", 35.5f, 0f, 1.5f, 1.5f);

            Sign(-9f, 0f, "Outono: pedras rolam da encosta. Em outra estação elas estariam quietas...");
            Sign(14f, 0f, "Um rio. Só dá para atravessar congelado.");
            Sign(30f, 0f, "No inverno, uma nevasca fecha a passagem.");
            Sign(40f, 0f, "No verão, estes arbustos soltam espinhos venenosos.");
            Scenery(0f, -11f, 1f, 28f, 38f, 44.5f);
            Exit(59.4f, 1.2f);
            Save(scene, "Level08");
        }

        private static void BuildLevel09()
        {
            var scene = BeginLevel(new LevelSpec
            {
                title = "Capítulo 9 — A Porta do Passado",
                intro = "Inverno. Uma porta que só se abre se algo foi feito na primavera...\nnum lugar onde, na primavera, ninguém consegue entrar.",
                complete = "Você entrou no passado por um caminho que só existia no futuro.",
                next = "Level10",
                season = Season.Primavera, maxDay = 90, step = 30, startIndex = 3, stasis = true,
                spawn = new Vector2(-9f, 1f),
                camMin = new Vector2(-13f, -4f), camMax = new Vector2(58f, 16f)
            });

            Wall(-14f, -10f, 1f, 26f, "Limite");
            Wall(57f, -10f, 1f, 26f, "Limite");
            Ground(-13f, -5f, 35f, 5f);
            Ground(22f, -5f, 1.3f, 4f, false);
            Ground(23.3f, -5f, 34f, 5f);
            RockPassage("Rocha Alta", 11.25f, 3f, 4.5f);

            var vines = TemporalHolder("Muralha de trepadeiras", new Vector2(12.75f, 0f));
            var alive = Holder(vines.transform, "Viva", new Vector2(12.75f, 0f));
            Rect("Trepadeiras (bloqueio)", alive.transform, 12f, 0f, 1.5f, 4.5f, tiles != null ? Color.clear : new Color(0.45f, 0.3f, 0.55f), 3, true, tiles != null ? squareSprite : null);
            if (tiles != null)
            {
                ThornVines(alive.transform, 11.3f, 2.9f, 4.5f);
            }
            var dead = Holder(vines.transform, "Seca", new Vector2(12.75f, 0f));
            if (tiles != null)
            {
                BuildVine(dead.transform, 12.3f, 0f, 1.6f, true);
                BuildVine(dead.transform, 13.2f, 0f, 1.1f, true);
            }
            With(vines.AddState("Viva (primavera e verão)"), alive);
            With(vines.AddState("Seca (outono)", TemporalCondition.In(Season.Outono)), dead);
            With(vines.AddState("Seca (inverno)", TemporalCondition.In(Season.Inverno)), dead);

            const string plate = "L9_CaixaNaPlaca";
            var box = Box(17f, 0.5f, "Caixa do Relógio");
            var sensor = new GameObject("Sensor_Placa");
            sensor.transform.SetParent(groupRules, false);
            sensor.transform.position = new Vector3(22.65f, -0.5f, 0f);
            sensor.AddComponent<PositionSensor>().Setup(plate, "Caixa encaixada na placa do relógio", box, new Vector2(1.3f, 1.2f));
            Part(groupEnv, "Placa do Relógio", 22.05f, -1.12f, 1.2f, 0.14f, WoodColor, 2, false);
            if (tiles != null && tiles.gear != null) Decor(groupEnv, tiles.gear, new Vector2(27.6f, 4.6f), 2f, 1, new Color(0.85f, 0.75f, 0.5f));

            Wall(27f, 3f, 1.2f, 13f, "Parede do Relógio");
            var door = Gate("Porta do Relógio", 27f, 0f, 1.2f, 3f, "L9_PortaDoRelogio", false);
            Rule("L9_PortaDoRelogio", "Caixa na placa no Dia 1 (primavera) → a porta do relógio fica aberta para sempre", TemporalCondition.FlagAt(plate, 0));

            Enemy(chronoferaPrefab, "Cronofera do Corredor", 38f, 0f, 5f, 5f);
            Set(Enemy(chronoferaPrefab, "Cronofera que nasce no verão", 47f, 0f, 4f, 4f), "birthDay", 30);
            Enemy(thornPrefab, "Espinheiro do Corredor", 43f, 0f, 0f, 0f);

            Sign(-9f, 0f, "No inverno a trepadeira está seca. Na primavera, ela fecha a passagem.");
            Sign(19.5f, 0f, "A porta do relógio só abre se esta placa estiver pressionada no DIA 1.\nEmpurrar a caixa agora (no inverno) não conta...");
            Sign(31f, 0f, "Cronoferas jovens são rápidas. Velhas, lentas. No fim do ano, já não existem.");
            Scenery(0f, -11f, -4f, 5f, 34f, 52f);
            Exit(53.5f, 1.2f);
            Save(scene, "Level09");
        }

        private static void BuildLevel10()
        {
            var scene = BeginLevel(new LevelSpec
            {
                title = "Capítulo 10 — O Devorador do Tempo",
                intro = "O Cronófago, quem estilhaçou o relógio de Eco, guarda o último fragmento. No inverno, sua armadura de gelo é invencível.\nMas todo inverno começa numa primavera...",
                complete = "O Cronófago caiu. O último fragmento volta ao seu lugar: o relógio de Eco bate outra vez.",
                next = "Ending",
                season = Season.Primavera, maxDay = 90, step = 30, startIndex = 3, stasis = true,
                spawn = new Vector2(-9f, 1f),
                camMin = new Vector2(-13f, -4f), camMax = new Vector2(43f, 16f)
            });

            Wall(-14f, -10f, 1f, 26f, "Limite");
            Wall(42f, -10f, 1f, 26f, "Limite");
            Ground(-13f, -5f, 55f, 5f);
            RockPassage("Portal da Arena", 0.2f, 2.6f, 3.5f);
            ArenaPlatform(7f, 2.8f, 4f);
            ArenaPlatform(25f, 2.8f, 4f);
            ArenaPlatform(15.5f, 5.4f, 5f);
            Wall(36f, 3f, 1f, 13f, "Muro Final");
            Gate("Portão do Fim do Tempo", 36f, 0f, 1f, 3f, "L10_CronofagoDerrotado", false);

            var boss = Boss("Cronófago", new Vector2(22f, 0f), boarArt, "Run", new Vector2(1.1f, 0.75f), 30, "L10_CronofagoDerrotado", 2f, 35f, 0f, new Color(0.5f, 0.2f, 0.4f));
            boss.AddPhase(new BossPhase { name = "Cria do Tempo (primavera)", attacks = { BossAttack.Investida, BossAttack.Investida, BossAttack.Rajada }, attackInterval = 1.6f, moveSpeed = 4.5f, dashSpeed = 15f, projectileCount = 3, projectileSpeed = 7f, spread = 30f, vulnerable = true, stunTime = 1.2f, size = 3.2f, animation = "Run", color = new Color(1f, 0.88f, 0.78f), message = "Primavera: o Cronófago é jovem, rápido e vulnerável." });
            boss.AddPhase(new BossPhase { name = "Devorador voraz (verão)", conditions = { TemporalCondition.In(Season.Verao) }, attacks = { BossAttack.Rajada, BossAttack.Invocar, BossAttack.Rajada, BossAttack.Investida }, attackInterval = 1.7f, moveSpeed = 2.5f, dashSpeed = 12f, projectileCount = 5, projectileSpeed = 6.5f, spread = 60f, vulnerable = true, stunTime = 1f, size = 3.8f, animation = "Walk", color = new Color(1f, 0.72f, 0.55f), message = "Verão: rajadas e lodos. Mude de dia para apagar os ataques." });
            boss.AddPhase(new BossPhase { name = "Titã do outono", conditions = { TemporalCondition.In(Season.Outono) }, attacks = { BossAttack.Pancada, BossAttack.Pancada, BossAttack.Rajada }, attackInterval = 2.2f, moveSpeed = 2f, projectileCount = 4, projectileSpeed = 6f, spread = 45f, vulnerable = false, vulnerableWhenStunned = true, stunTime = 2.4f, size = 4.4f, animation = "Walk", color = new Color(0.85f, 0.62f, 0.52f), message = "Outono: blindado — mas cada pancada o deixa atordoado." });
            boss.AddPhase(new BossPhase { name = "Colosso de gelo (inverno)", conditions = { TemporalCondition.In(Season.Inverno) }, attacks = { BossAttack.Invocar, BossAttack.Rajada, BossAttack.Pancada }, attackInterval = 2f, moveSpeed = 1.6f, projectileCount = 6, projectileSpeed = 6f, spread = 80f, vulnerable = false, vulnerableWhenStunned = false, stunTime = 1.5f, size = 4.8f, animation = "Walk", color = new Color(0.62f, 0.82f, 1f), message = "Inverno: armadura de gelo. Nada o fere agora..." });

            Sign(-9f, 0f, "Os ataques do Cronófago pertencem ao dia em que foram lançados:\nmude de dia (Q/E) e eles desaparecem.");
            Sign(-3.5f, 0f, "O dano que você causa num dia continua nos dias seguintes.\nFerir o jovem Cronófago enfraquece o colosso do inverno.");
            Scenery(0f, -11f, -6f);
            Exit(39f, 1.2f);
            Save(scene, "Level10");
        }
    }
}

using UnityEngine;

namespace ButterflyStep.EditorTools
{
    public static partial class GameBuilder
    {
        private static readonly Color IceColor = new Color(0.78f, 0.92f, 1f, 0.95f);
        private static readonly Color AutumnLeaves = new Color(0.9f, 0.5f, 0.2f, 0.9f);

        private static GameObject Holder(Transform parent, string name, Vector2 position)
        {
            return Go(name, parent, position);
        }

        private static TemporalObject TemporalRect(string name, float xMin, float yMin, float w, float h, Color color, int order, bool solid)
        {
            var go = Rect(name, groupTemporal, xMin, yMin, w, h, color, order, solid);
            var t = go.AddComponent<TemporalObject>();
            t.Setup(name, go.GetComponent<SpriteRenderer>(), solid ? new Collider2D[] { go.GetComponent<BoxCollider2D>() } : new Collider2D[0]);
            return t;
        }

        private static TemporalObject TemporalHolder(string name, Vector2 position)
        {
            var go = Holder(groupTemporal, name, position);
            var t = go.AddComponent<TemporalObject>();
            t.Setup(name, null);
            return t;
        }

        private static TemporalStateData Hidden(TemporalStateData s)
        {
            s.visible = false;
            s.solid = false;
            return s;
        }

        private static TemporalStateData With(TemporalStateData s, params GameObject[] objects)
        {
            s.activeObjects.AddRange(objects);
            return s;
        }

        private static TemporalStateData Consequence(TemporalStateData s)
        {
            s.isConsequence = true;
            return s;
        }

        private static void BuildLevel01()
        {
            var scene = BeginLevel(new LevelSpec
            {
                title = "Capítulo 1 — O Primeiro Passo",
                intro = "Primavera. Eco desperta preso num tempo que não é o seu. Um fragmento do seu relógio brilha lá no alto.\nQ e E fazem os dias passarem. Itens que você pega viajam no tempo com você.",
                complete = "Uma gota d'água virou uma árvore em poucas semanas... e abriu o caminho até o primeiro fragmento.",
                next = "Level02",
                season = Season.Primavera, maxDay = 60, step = 10,
                spawn = new Vector2(-8f, 1f),
                camMin = new Vector2(-13f, -2.5f), camMax = new Vector2(62f, 22f)
            });

            Wall(-14f, -5f, 1f, 22f, "Limite");
            Wall(61f, -5f, 1f, 30f, "Limite");
            Ground(-13f, -5f, 31f, 5f);
            Ground(18f, -5f, 20f, 12f);
            if (tiles != null)
            {
                Decor(groupEnv, tiles.rockSmall, new Vector2(-1f, 0f), 0.62f, 2);
                Rect("Pedra (colisão)", groupEnv, -1.6f, 0f, 1.2f, 1f, Color.clear, 0, true, squareSprite);
            }
            else Rect("Pedra", groupEnv, -2f, 0f, 2f, 1f, RockColor, 0, true);

            const string watered = "L1_PlantaRegada";
            var t = TemporalHolder("Planta", new Vector2(10f, 0f));
            var plant = t.gameObject;
            bool art = tiles != null;

            var sproutColor = new Color(0.45f, 0.8f, 0.35f);
            GameObject muda, seca, broto;
            if (art)
            {
                muda = Holder(plant.transform, "Muda", new Vector2(10f, 0f));
                Decor(muda.transform, tiles.sprout, new Vector2(10f, 0f), 1f, 5);
                seca = Holder(plant.transform, "MudaSeca", new Vector2(10f, 0f));
                Decor(seca.transform, tiles.sprout, new Vector2(10f, 0f), 1f, 5, new Color(0.6f, 0.45f, 0.25f));
                broto = Holder(plant.transform, "Broto", new Vector2(10f, 0f));
                Decor(broto.transform, tiles.leafPlant, new Vector2(10f, 0f), 1.1f, 5);
            }
            else
            {
                muda = Part(plant.transform, "Muda", 9.8f, 0f, 0.4f, 0.6f, sproutColor, 5, false, squareSprite);
                seca = Part(plant.transform, "MudaSeca", 9.8f, 0f, 0.4f, 0.35f, new Color(0.55f, 0.42f, 0.25f), 5, false, squareSprite);
                broto = Holder(plant.transform, "Broto", new Vector2(10f, 0f));
                Part(broto.transform, "Caule", 9.85f, 0f, 0.3f, 1.3f, sproutColor, 5, false, squareSprite);
                Shape("Folha", broto.transform, new Vector2(10.3f, 1.2f), new Vector2(0.6f, 0.35f), circleSprite, sproutColor, 6);
            }

            var jovem = Holder(plant.transform, "ArvoreJovem", new Vector2(10f, 0f));
            if (art) SeasonalTree(jovem.transform, new Vector2(10f, 0f), 1, 0.75f, 2);
            else
            {
                Part(jovem.transform, "Tronco", 9.6f, 0f, 0.8f, 2.6f, WoodColor, 4, false);
                Shape("Folhagem", jovem.transform, new Vector2(10f, 3.3f), new Vector2(4f, 1.8f), circleSprite, new Color(0.3f, 0.7f, 0.35f, 0.9f), 2);
            }
            Branch("Galho", jovem.transform, 10f, 8.3f, 2.8f, 3.4f);

            var adulta = Holder(plant.transform, "ArvoreAdulta", new Vector2(10f, 0f));
            if (art) SeasonalTree(adulta.transform, new Vector2(10f, 0f), 2, 0.82f, 2);
            else
            {
                Part(adulta.transform, "Tronco", 9.5f, 0f, 1f, 6.2f, WoodColor, 4, false);
                Shape("Folhagem", adulta.transform, new Vector2(12.5f, 6.8f), new Vector2(9f, 2.2f), circleSprite, new Color(0.25f, 0.65f, 0.3f, 0.9f), 2);
            }
            Branch("Galho Baixo", adulta.transform, 10f, 7.8f, 2.8f, 3f);
            Branch("Galho Medio", adulta.transform, 10f, 9.6f, 5.3f, 3f);
            Branch("Galho Alto", adulta.transform, 10f, 11.5f, 6.4f, 5f);

            With(t.AddState("Muda"), muda);
            With(t.AddState("Muda seca", TemporalCondition.Since(20), TemporalCondition.NotFlag(watered)), seca);
            With(t.AddState("Broto (regado)", TemporalCondition.Flag(watered)), broto);
            Consequence(With(t.AddState("Árvore jovem", TemporalCondition.Flag(watered, 10)), jovem));
            Consequence(With(t.AddState("Árvore adulta (muda com as estações)", TemporalCondition.Flag(watered, 20)), adulta));
            Scenery(0f, -11f, -5.5f, 4f, 15.5f);
            Scenery(7f, 21f, 27f);
            Scenery(12f, 48f, 56f);
            Ground(44f, -5f, 17f, 17f);
            Kill(38f, -9f, 6f);
            Enemy(slimePrefab, "Lodo do Platô", 25f, 7f, 3f, 3f);
            BounceMushroom("Cogumelo de Outono", 28.8f, 7f, 19f, TemporalCondition.In(Season.Outono));

            Pickup("pinha", "Pinha", new Vector2(14f, 7.2f), "Você pegou uma pinha! Itens viajam no tempo com você.", TemporalCondition.Flag(watered, 20), TemporalCondition.NotIn(Season.Primavera));
            PlantedPine("Pinheiro Plantado", 34f, 7f, "L1_PinhaPlantada", 20, "pinha", "Pinha", "Plantar a pinha", "Terra fértil: uma pinha só brota se plantada na primavera.",
                new[] { new Vector3(32f, 9.4f, 3f), new Vector3(34.5f, 11.6f, 3f), new Vector3(38f, 12.4f, 4.5f) },
                TemporalCondition.In(Season.Primavera));

            Interact(plant.transform, new Vector2(10f, 0.8f), 1.3f, watered, "Regar a muda", "Você regou a muda. Ela vai se lembrar disso.");

            Enemy(mothPrefab, "Lagarta", 1.5f, 0f, 1.5f, 1.5f, "L1_LagartaMorta");

            Sign(-9f, 0f, "Você é Eco, um ser nascido da linha do tempo.\nA/D para andar, ESPAÇO para pular.");
            Sign(3.5f, 0f, "Uma lagarta... em alguns dias ela vai mudar.\nPule em cima dela para afastá-la — para sempre.");
            Sign(6.5f, 0f, "Uma muda frágil. Sozinha, ela nunca vai crescer...\nChegue perto e aperte F para regar.");
            Sign(14f, 0f, "E avança 10 dias. Q volta 10 dias.\nVeja no topo o dia e a estação. Nunca dá para voltar antes do Dia 1.");
            Sign(21.5f, 7f, "No verão, o pinheiro adulto dá pinhas lá no alto.\nPinhas só brotam se forem plantadas na primavera...");
            Sign(30f, 7f, "O penhasco é alto demais. Uma árvore aqui ajudaria.\nSe ao menos alguém tivesse plantado algo antes...");
            Exit(52f, 13.2f);
            Save(scene, "Level01");
        }

        private static void BuildLevel02()
        {
            var scene = BeginLevel(new LevelSpec
            {
                title = "Capítulo 2 — A Porta que Ficou",
                intro = "Outono. O que você move hoje continua movido amanhã.\nE o inverno muda a paisagem.",
                complete = "Uma caixa esquecida e um rio congelado abriram o caminho.",
                next = "Level03",
                season = Season.Outono, maxDay = 60, step = 10,
                spawn = new Vector2(-8f, 1f),
                camMin = new Vector2(-13f, -6f), camMax = new Vector2(47f, 16f)
            });

            Wall(-14f, -9f, 1f, 26f, "Limite");
            Wall(46f, -9f, 1f, 26f, "Limite");
            Ground(-13f, -5f, 30f, 5f);
            Ground(17f, -5f, 1.3f, 4f, false);
            Ground(18.3f, -5f, 1.9f, 5f);
            Wall(19f, 3f, 1.2f, 14f, "Parede da Porta");
            Ground(30f, -5f, 16f, 5f);
            Rect("Leito do Rio", groupEnv, 20.2f, -5f, 9.8f, 1f, GroundColor, 0, true);

            var trilho = Part(groupEnv, "Trilho", 17f, -1.05f, 1.3f, 0.1f, new Color(0.3f, 0.3f, 0.35f), 1, false, squareSprite);
            trilho.name = "Trilho da Porta";
            if (tiles != null && tiles.gear != null) Decor(groupEnv, tiles.gear, new Vector2(19.6f, 3.6f), 1.6f, 1, new Color(0.75f, 0.75f, 0.8f));
            else Shape("Engrenagem", groupEnv, new Vector2(18.6f, 2.3f), new Vector2(0.7f, 0.7f), circleSprite, new Color(0.4f, 0.4f, 0.45f), 1);

            const string boxInRail = "L2_CaixaNoTrilho";
            var box = Box(9f, 0.5f, "Caixa");
            var sensor = new GameObject("Sensor_TrilhoDaPorta");
            sensor.transform.SetParent(groupRules, false);
            sensor.transform.position = new Vector3(17.65f, -0.5f, 0f);
            sensor.AddComponent<PositionSensor>().Setup(boxInRail, "Caixa encaixada no trilho da porta", box, new Vector2(1.3f, 1.2f));

            var door = TemporalRect("Porta", 19f, 0f, 1.2f, 3f, DoorColor, 2, true);
            var aberta = Part(door.transform, "PortaRecolhida", 19f, 2.65f, 1.2f, 0.35f, DoorColor, 2, false);
            var travada = Holder(door.transform, "PortaTravada", new Vector2(19.6f, 2.5f));
            Part(travada.transform, "Porta", 19f, 2.2f, 1.2f, 0.8f, DoorColor, 2, false);
            Part(travada.transform, "Trava", 18.95f, 2.1f, 1.3f, 0.15f, ConsequenceColor, 3, false, squareSprite);

            With(Hidden(door.AddState("Aberta (enferrujando)")), aberta);
            door.AddState("Fechada para sempre", TemporalCondition.Since(20), TemporalCondition.NotFlagAt(boxInRail, 10));
            Consequence(With(Hidden(door.AddState("Travada aberta pela caixa", TemporalCondition.Since(20), TemporalCondition.FlagAt(boxInRail, 10))), travada));

            var river = TemporalHolder("Rio", new Vector2(25f, -2f));
            var agua = Holder(river.transform, "Agua", new Vector2(25f, -2f));
            Water(agua.transform, "Correnteza", 20.2f, -0.6f, 9.8f, 3.4f, new Vector2(-4f, 0f));
            var gelo = Holder(river.transform, "Gelo", new Vector2(25f, -2f));
            FrozenWater(gelo.transform, 20.2f, -0.2f, 9.8f, 3.8f, 0.5f);
            With(river.AddState("Correnteza forte"), agua);
            With(river.AddState("Congelado", TemporalCondition.In(Season.Inverno)), gelo);

            Kill(20f, -8f, 10f);
            Enemy(slimePrefab, "Lodo", 2f, 0f, 3f, 3f);
            Enemy(slimePrefab, "Lodo do Outro Lado", 35.5f, 0f, 2f, 2f);
            Enemy(thornPrefab, "Espinheiro", 32f, 0f, 0f, 0f);
            Enemy(thornPrefab, "Espinheiro da Margem", 6f, 0f, 0f, 0f);
            Wall(40f, 3f, 1f, 13f, "Muro do Portão");
            Gate("Portão", 40f, 0f, 1f, 3f, "L2_PortaoAberto", false);
            KeyLock(new Vector2(39.2f, 1f), "L2_PortaoAberto", "chave", "Chave enferrujada", "Portão trancado. Onde estará a chave?");
            Pickup("chave", "Chave enferrujada", new Vector2(15.5f, 0.7f), "A correnteza da primavera trouxe uma chave até a margem! Ela vai com você para qualquer dia.", TemporalCondition.Since(60));

            Sign(-9f, 0f, "O que você move no passado continua movido no futuro.\nEmpurre caixas andando contra elas.");
            Ground(-13f, 0f, 3f, 4.5f);
            ClimbVine("Trepadeira do Barranco", -9.7f, 0f, 4.8f, TemporalCondition.In(Season.Primavera));
            Sign(-7.2f, 0f, "Uma muda de trepadeira. Na primavera ela cobre o barranco:\nsegure W para escalar.");
            Sign(5f, 0f, "Depois da porta corre um rio forte demais para atravessar.\nMas o inverno começa no Dia 31...");
            Sign(14f, 0f, "Esta porta enferruja e se fecha para sempre no Dia 21.\nSe algo travar o trilho antes disso...");
            Scenery(0f, -10f, -3f, 12f, 34f, 44f);
            Sign(-3f, 0f, "Na primavera, a correnteza do degelo arrasta coisas rio abaixo...");
            Sign(37.5f, 0f, "Um portão trancado guarda o fragmento do relógio.");
            Exit(44f, 1.2f);
            Save(scene, "Level02");
        }

        private static void BuildLevel03()
        {
            var scene = BeginLevel(new LevelSpec
            {
                title = "Capítulo 3 — Degelo",
                intro = "Inverno. Uma represa de pedra e gelo segura um lago congelado.\nQuando a primavera chegar, uma pequena fissura bastará.",
                complete = "Sessenta dias de água paciente abriram o que nenhuma força abriria.",
                next = "Level04",
                season = Season.Inverno, maxDay = 90, step = 15,
                spawn = new Vector2(-8f, 1f),
                camMin = new Vector2(-13f, -2.5f), camMax = new Vector2(45f, 18f)
            });

            Wall(-14f, -6f, 1f, 40f, "Limite");
            Wall(45f, -6f, 1f, 40f, "Limite");
            Ground(-13f, -5f, 33f, 5f);
            Ground(24f, -5f, 21f, 5f);
            Rect("Leito da Muralha", groupEnv, 20f, -5f, 4f, 5f, GroundColor, 0, true);
            Wall(20f, 2.5f, 4f, 32f, "Muralha");
            Ground(0f, 0f, 2f, 2.3f);
            Ground(2f, 0f, 2f, 4.6f);
            Ground(4f, 0f, 12f, 5.2f, false);
            Ground(4f, 5.2f, 1.4f, 1.8f);
            Ground(12f, 5.2f, 4f, 1.8f);

            const string cracked = "L3_PedrinhaQuebrada";

            var baseRock = TemporalRect("Base da Muralha", 20f, 0f, 4f, 2.5f, RockColor, 0, true);
            var rachaduras = Holder(baseRock.transform, "Rachaduras", new Vector2(22f, 1.2f));
            Part(rachaduras.transform, "Fenda", 20.6f, 0.3f, 0.15f, 1.8f, new Color(0.2f, 0.2f, 0.25f), 1, false, squareSprite);
            Part(rachaduras.transform, "Fenda", 21.8f, 0.6f, 0.15f, 1.5f, new Color(0.2f, 0.2f, 0.25f), 1, false, squareSprite);
            Part(rachaduras.transform, "Fenda", 23f, 0.2f, 0.15f, 2f, new Color(0.2f, 0.2f, 0.25f), 1, false, squareSprite);
            Part(rachaduras.transform, "Umidade", 20f, 0f, 4f, 0.9f, new Color(0.45f, 0.55f, 0.7f), 1, false, tiles != null ? tiles.groundFill : squareSprite);
            WaterMesh(rachaduras.transform, "Poça", 19.4f, 0.12f, 5.2f, 0.12f, 0f, 3, RiverTop, RiverBottom);
            var tunel = Holder(baseRock.transform, "Tunel", new Vector2(22f, 1.2f));
            Part(tunel.transform, "Fundo", 20f, 0f, 4f, 2.5f, new Color(0.3f, 0.28f, 0.32f), -5, false, tiles != null ? tiles.groundFill : blockSprite);
            Part(tunel.transform, "Arco", 19.8f, 2.2f, 4.4f, 0.45f, WoodColor, 3, false);
            Part(tunel.transform, "Pilar", 19.8f, 0f, 0.4f, 2.3f, WoodColor, 3, false);
            Part(tunel.transform, "Pilar", 23.8f, 0f, 0.4f, 2.3f, WoodColor, 3, false);
            WaterMesh(tunel.transform, "Agua", 19f, 0.3f, 6f, 0.3f, 2.5f, 3, RiverTop, RiverBottom);
            baseRock.AddState("Rocha intacta");
            With(baseRock.AddState("Rocha úmida e rachada", TemporalCondition.Flag(cracked, 30), TemporalCondition.NotIn(Season.Inverno)), rachaduras);
            Consequence(With(Hidden(baseRock.AddState("Túnel escavado pela água", TemporalCondition.Flag(cracked, 60))), tunel));

            var lakeT = TemporalHolder("Lago", new Vector2(7.9f, 6.5f));
            var cheio = Holder(lakeT.transform, "Cheio", new Vector2(7.9f, 6.9f));
            Water(cheio.transform, "Agua", 5.4f, 6.9f, 5.6f, 1.7f, Vector2.zero, true);
            var congelado = Holder(lakeT.transform, "Congelado", new Vector2(7.9f, 6.9f));
            FrozenWater(congelado.transform, 5.4f, 7f, 5.6f, 1.8f, 0.4f);
            var medio = Holder(lakeT.transform, "Medio", new Vector2(7.9f, 6.2f));
            Water(medio.transform, "Agua", 5.4f, 6.2f, 5.6f, 1f, Vector2.zero, true);
            var baixo = Holder(lakeT.transform, "Baixo", new Vector2(7.9f, 5.5f));
            WaterMesh(baixo.transform, "Poca", 5.4f, 5.5f, 5.6f, 0.3f, 0f, 6, LakeTop, LakeBottom);
            With(lakeT.AddState("Lago cheio"), cheio);
            With(lakeT.AddState("Lago congelado", TemporalCondition.In(Season.Inverno)), congelado);
            With(lakeT.AddState("Lago baixando", TemporalCondition.Flag(cracked, 30), TemporalCondition.NotIn(Season.Inverno)), medio);
            With(lakeT.AddState("Lago quase seco", TemporalCondition.Flag(cracked, 60)), baixo);

            var dam = TemporalRect("Represa", 11f, 5.2f, 1f, 2.6f, RockColor, 1, true);
            var pedrinha = Shape("Pedrinha", dam.transform, new Vector2(12.25f, 7.25f), new Vector2(0.5f, 0.5f), circleSprite, InteractColor, 5);
            var fissura = Part(dam.transform, "Fissura", 11.4f, 5.6f, 0.2f, 2.2f, new Color(0.2f, 0.2f, 0.25f), 3, false, squareSprite);
            var gotas = Holder(dam.transform, "Gotas Congeladas", new Vector2(12.2f, 7.4f));
            Shape("Gelo", gotas.transform, new Vector2(12.2f, 7.3f), new Vector2(0.4f, 0.6f), triangleSprite, IceColor, 4);
            var fio = Holder(dam.transform, "FioDagua", new Vector2(14f, 4f));
            WaterMesh(fio.transform, "Filete", 12f, 7.08f, 4f, 0.08f, 1.5f, 6, RiverTop, RiverBottom);
            Waterfall(fio.transform, "Queda", 16f, 0f, 0.12f, 7.05f);
            var riacho = Holder(dam.transform, "Riacho", new Vector2(14f, 4f));
            WaterMesh(riacho.transform, "Topo", 12f, 7.22f, 4f, 0.22f, 2.5f, 6, RiverTop, RiverBottom);
            Waterfall(riacho.transform, "Queda", 16f, 0f, 0.4f, 7.2f);
            WaterMesh(riacho.transform, "Leito", 16.4f, 0.22f, 3.6f, 0.22f, 2.5f, 6, RiverTop, RiverBottom);
            var cachoeira = Holder(dam.transform, "Cachoeira", new Vector2(14f, 4f));
            WaterMesh(cachoeira.transform, "Topo", 12f, 7.3f, 4f, 0.3f, 4f, 6, RiverTop, RiverBottom);
            Waterfall(cachoeira.transform, "Queda", 16f, 0f, 1.1f, 7.3f);
            WaterMesh(cachoeira.transform, "Leito", 17.1f, 0.35f, 2.9f, 0.35f, 4f, 6, RiverTop, RiverBottom);

            With(dam.AddState("Represa intacta"), pedrinha);
            With(dam.AddState("Fissura congelada", TemporalCondition.Flag(cracked), TemporalCondition.In(Season.Inverno)), fissura, gotas);
            With(dam.AddState("Fissura pingando", TemporalCondition.Flag(cracked), TemporalCondition.NotIn(Season.Inverno)), fissura, fio);
            Consequence(With(dam.AddState("O degelo formou um riacho", TemporalCondition.Flag(cracked, 30), TemporalCondition.NotIn(Season.Inverno)), fissura, riacho));
            Consequence(With(Hidden(dam.AddState("Represa rompida: cachoeira", TemporalCondition.Flag(cracked, 60))), cachoeira));

            Interact(dam.transform, new Vector2(12.2f, 7.6f), 1.2f, cracked, "Quebrar a pedrinha", "Uma pedrinha se soltou da represa. Quase nada mudou... por enquanto.");

            Enemy(thornPrefab, "Espinheiro do Penhasco", 14.8f, 7f, 0f, 0f);
            Enemy(slimePrefab, "Lodo", 27.5f, 0f, 2.5f, 2.5f);
            Enemy(thornPrefab, "Espinheiro do Penhasco Alto", 41.3f, 12f, 0f, 0f);
            Ground(34f, -5f, 11f, 12f);
            Ground(40f, 7f, 5f, 5f);
            BounceMushroom("Cogumelo do Penhasco", 37.4f, 7f, 19f, TemporalCondition.In(Season.Outono));
            Sign(35.3f, 7f, "A saída fica lá no alto, alta demais para um pulo.\nNo outono, um cogumelo gigante brota nesta terra úmida.");
            Pickup("semente", "Semente de pinheiro", new Vector2(7.9f, 7.5f), "Uma semente presa no gelo do lago! Ela vai com você para qualquer dia.", TemporalCondition.In(Season.Inverno));
            PlantedPine("Pinheiro do Vale", 30.5f, 0f, "L3_SementePlantada", 45, "semente", "Semente de pinheiro", "Plantar a semente", "Terra fértil. Uma semente levaria 45 dias para virar árvore aqui.",
                new[] { new Vector3(28.6f, 2.6f, 3f), new Vector3(30f, 5f, 3f), new Vector3(31.4f, 7.3f, 3.2f) });

            Sign(-9f, 0f, "Inverno, Dia 1. Aqui cada salto no tempo dura 15 dias.");
            Sign(-3.5f, 0f, "O degelo leva tempo: quanto antes a água começar, mais longe ela chega.");
            Sign(1f, 2.3f, "Lá em cima, uma represa de pedra e gelo segura um lago.\nUma pequena pedra parece solta...");
            Sign(18.6f, 0f, "A muralha parece indestrutível... por enquanto.");
            Scenery(0f, -11f, -4f, 25.5f);
            Scenery(12f, 43.8f);
            Sign(26f, 0f, "Uma árvore levaria 45 dias para crescer aqui...\nE o túnel só existe no verão. Talvez voltar alguns dias ajude.");
            Exit(43f, 13.2f);
            Save(scene, "Level03");
        }

        private static void BuildLevel04()
        {
            var scene = BeginLevel(new LevelSpec
            {
                title = "Capítulo 4 — A Criatura das Eras",
                intro = "Verão. As criaturas também atravessam o tempo:\nnascem, crescem, envelhecem... e morrem.",
                complete = "Até o peso da velhice pode abrir caminhos.",
                next = "Level05",
                season = Season.Verao, maxDay = 80, step = 20,
                spawn = new Vector2(-8f, 1f),
                camMin = new Vector2(-13f, -9f), camMax = new Vector2(36f, 14f)
            });

            Wall(-14f, -12f, 1f, 28f, "Limite");
            Ground(-13f, -5f, 15f, 5f);
            Ground(9f, -12f, 15f, 12f);
            Ground(24f, -12f, 11f, 5f);
            Wall(32f, -7f, 3f, 23f, "Paredão");

            Rect("Suporte", groupEnv, 2f, -5f, 0.25f, 4.6f, WoodColor, -1, false);
            Rect("Suporte", groupEnv, 8.75f, -5f, 0.25f, 4.6f, WoodColor, -1, false);

            var scaffold = TemporalRect("Andaime", 2f, -0.4f, 7f, 0.4f, WoodColor, 3, true);
            var destrocos = Holder(scaffold.transform, "Destrocos", new Vector2(5.5f, -1f));
            Part(destrocos.transform, "Tabua", 2f, -1.2f, 1.2f, 0.25f, WoodColor, 3, false);
            Part(destrocos.transform, "Tabua", 7.8f, -1.6f, 1.2f, 0.25f, WoodColor, 3, false);
            scaffold.AddState("Andaime novo");
            With(Hidden(scaffold.AddState("Andaime apodrecido", TemporalCondition.Since(20))), destrocos);
            Kill(2f, -9f, 7f);

            var valley = Enemy(chronoferaPrefab, "Cronofera do Vale", 18.5f, 0f, 5f, 4.5f);
            Tameable(valley, "L4_ComedouroCheio", 20, false);
            var trough = Interact(groupInteract, new Vector2(10.3f, 0.6f), 1.1f, "L4_ComedouroCheio", "Encher o comedouro", "Você encheu o comedouro. Uma cronofera bem alimentada quando jovem cresce mansa.", TemporalCondition.NotIn(Season.Inverno));
            trough.SetBlockedPrompt("No inverno não há frutos para o comedouro.");
            if (tiles != null)
            {
                Decor(groupInteract, tiles.crate, new Vector2(10.3f, 0f), 0.7f, 3);
                Decor(groupInteract, tiles.mushroom, new Vector2(10.3f, 0.6f), 0.6f, 4);
            }
            Enemy(thornPrefab, "Espinheiro do Vale", 12f, 0f, 0f, 0f);

            const string broken = "L4_PonteQuebrada";
            var bridge = TemporalRect("Ponte Frágil", 24f, -0.4f, 8f, 0.4f, WoodColor, 3, true);
            bridge.gameObject.AddComponent<FragilePlatform>().Setup(broken, bridge.transform);
            var quebrada = Holder(bridge.transform, "PonteQuebrada", new Vector2(28f, -1f));
            Part(quebrada.transform, "Pedaco", 24f, -1.8f, 0.35f, 1.8f, WoodColor, 3, false);
            Part(quebrada.transform, "Pedaco", 31.65f, -2.1f, 0.35f, 2.1f, WoodColor, 3, false);
            bridge.AddState("Ponte de madeira");
            Consequence(With(Hidden(bridge.AddState("Ponte quebrada pelo peso", TemporalCondition.Flag(broken))), quebrada));

            Enemy(chronoferaPrefab, "Cronofera da Ponte", 30.2f, 0f, 0.8f, 0.6f);

            Sign(-9f, 0f, "As criaturas também atravessam o tempo.\nJovens: rápidas. Velhas: lentas e pesadas. Um dia, morrem.");
            Sign(-1f, 0f, "Este andaime é novo no verão.\nMadeira não dura para sempre.");
            Sign(10f, 0f, "O vale é vigiado. Em qual dia ele é mais seguro?\nE qual dia você PRECISA ver?");
            Sign(22.5f, 0f, "O fragmento do relógio brilha lá embaixo, sob a ponte...");
            Scenery(0f, -11f, -4f, 10.5f, 21.5f);
            Scenery(-7f, 24.8f);
            Wall(27f, -3.6f, 0.8f, 3.2f, "Batente do Portão");
            Enemy(slimePrefab, "Lodo do Fosso", 30f, -7f, 1.2f, 1.2f);
            Gate("Portão do Fosso", 27f, -7f, 0.8f, 3.4f, "L4_PortaoAberto", true);
            KeyLock(new Vector2(26.2f, -6.2f), "L4_PortaoAberto", "chave", "Chave do ninho", "Trancado. E no inverno o gelo prende a fechadura.", TemporalCondition.NotIn(Season.Inverno));
            Pickup("chave", "Chave do ninho", new Vector2(20.5f, 0.7f), "Uma chave brilhando no ninho abandonado! Ela vai com você para qualquer dia.", TemporalCondition.Since(60));
            Sign(16.5f, 0f, "O ninho da cronofera. Enquanto ela viver, ninguém chega perto.");
            Sign(25f, -7f, "Um portão com fechadura. No inverno, o gelo não deixa a chave girar.");
            Exit(30.8f, -5.8f);
            Save(scene, "Level04");
        }

        private static void BuildLevel05()
        {
            var scene = BeginLevel(new LevelSpec
            {
                title = "Capítulo 5 — O Bater de Asas",
                intro = "Primavera. O próximo fragmento está do outro lado do abismo, guardado pela Rainha Vespa.\nNenhuma ponte existe... ainda.",
                complete = "A Rainha caiu, e com ela o fragmento que guardava.\nO próximo está num futuro que foi devorado.",
                next = "Level06",
                season = Season.Primavera, maxDay = 80, step = 10,
                spawn = new Vector2(-9f, 1f),
                camMin = new Vector2(-13f, -5f), camMax = new Vector2(82f, 18f),
                stasis = true
            });

            Wall(-14f, -10f, 1f, 26f, "Limite");
            Wall(81f, -10f, 1f, 32f, "Limite");
            Ground(55f, -5f, 26f, 12f);
            OneWay("Galho da Arena", groupEnv, 59f, 9.8f, 3f, ConsequenceColor);
            OneWay("Galho da Arena", groupEnv, 69f, 9.8f, 3f, ConsequenceColor);
            var hiveWall = Wall(75f, 10f, 1f, 12f, "Muro da Colmeia");
            Gate("Portão da Colmeia", 75f, 7f, 1f, 3f, "L5_RainhaDerrotada", false);
            var queen = Boss("Rainha Vespa", new Vector2(66f, 7f), mothArt, "Fly", new Vector2(0.8f, 0.7f), 12, "L5_RainhaDerrotada", 56f, 74f, 3.2f, new Color(1f, 0.8f, 0.3f));
            queen.AddPhase(new BossPhase { name = "Rainha jovem (veloz)", attacks = { BossAttack.Investida }, attackInterval = 2.2f, moveSpeed = 4f, dashSpeed = 13f, vulnerable = true, stunTime = 1.4f, size = 3.2f, flying = true, animation = "Fly", color = new Color(1f, 0.95f, 0.8f), message = "Primavera: a Rainha é jovem e veloz — mas frágil." });
            queen.AddPhase(new BossPhase { name = "Rainha furiosa", conditions = { TemporalCondition.In(Season.Verao) }, attacks = { BossAttack.Rajada, BossAttack.Rajada, BossAttack.Investida }, attackInterval = 1.8f, moveSpeed = 3f, dashSpeed = 11f, projectileCount = 3, projectileSpeed = 6.5f, spread = 35f, vulnerable = false, vulnerableWhenStunned = true, stunTime = 1.6f, size = 3.6f, flying = true, animation = "Fly", color = new Color(1f, 0.75f, 0.55f), message = "Verão: a Rainha cospe ferrões. Mude de dia para apagá-los!" });
            queen.AddPhase(new BossPhase { name = "Rainha cansada", conditions = { TemporalCondition.In(Season.Outono) }, attacks = { BossAttack.Pancada }, attackInterval = 2.6f, moveSpeed = 1.6f, projectileSpeed = 5f, vulnerable = false, vulnerableWhenStunned = true, stunTime = 2.6f, size = 4f, flying = true, animation = "Fly", color = new Color(0.8f, 0.7f, 0.85f), message = "Outono: a Rainha está velha. Depois de cada mergulho, ela fica atordoada." });
            Ground(-13f, -5f, 41f, 5f);
            Ground(38f, -5f, 17f, 5f);
            Rect("Plataforma do Ninho", groupEnv, 2f, 3.3f, 4f, 0.5f, WoodColor, 0, true);
            Part(groupEnv, "Poste do Ninho", 2.3f, 0f, 0.25f, 3.3f, WoodColor, -1, false);
            Part(groupEnv, "Poste do Ninho", 5.45f, 0f, 0.25f, 3.3f, WoodColor, -1, false);
            Part(groupEnv, "Travessa do Ninho", 2.3f, 1.5f, 3.4f, 0.18f, WoodColor, -1, false);
            Kill(28f, -9f, 10f);

            Box(-4f, 0.5f, "Caixa");

            const string birdFreed = "L5_PassaroLivre";
            const string flock = "L5_BandoCresce";
            const string vinesEaten = "L5_TrepadeiraComida";
            const string river = "L5_RioCorre";
            const string seed = "L5_SementePlantada";
            const string treeGrown = "L5_ArvoreCresce";
            const string treeFallen = "L5_ArvoreCaida";

            Rule(flock, "Pássaro livre há 10 dias → o bando cresceu", TemporalCondition.Flag(birdFreed, 10));
            Rule(vinesEaten, "Bando há 10 dias → a trepadeira invasora foi comida", TemporalCondition.Flag(flock, 10));
            Rule(river, "Sem trepadeira → a nascente volta a correr", TemporalCondition.Flag(vinesEaten));
            Rule(treeGrown, "Semente há 10 dias + rio correndo → árvore cresce inclinada", TemporalCondition.Flag(seed, 10), TemporalCondition.Flag(river));
            Rule(treeFallen, "Árvore inclinada há 10 dias → cai sobre o abismo", TemporalCondition.Flag(treeGrown, 10));

            var birdColor = new Color(0.3f, 0.9f, 0.85f);
            var cageColor = new Color(0.35f, 0.35f, 0.4f);
            var birdT = TemporalHolder("Pássaro", new Vector2(4.5f, 3.8f));
            var gaiola = Holder(birdT.transform, "Gaiola", new Vector2(4.5f, 4.4f));
            var aberta = Holder(birdT.transform, "GaiolaAberta", new Vector2(4.5f, 4.4f));
            if (tiles != null)
            {
                Bird(gaiola.transform, new Vector2(4.5f, 4.3f), 1f, 5, false);
                Bird(aberta.transform, new Vector2(6.5f, 6.5f), 1f, 5, true, 3f);
                Decor(gaiola.transform, tiles.grate, new Vector2(4.5f, 3.8f), 0.8f, 6);
                Decor(aberta.transform, tiles.grate, new Vector2(4.5f, 3.8f), 0.8f, 6, new Color(1f, 1f, 1f, 0.55f));
            }
            else
            {
                for (int i = 0; i < 5; i++) Part(gaiola.transform, "Grade", 3.9f + i * 0.28f, 3.8f, 0.07f, 1.2f, cageColor, 6, false, squareSprite);
                Part(gaiola.transform, "Teto", 3.85f, 4.95f, 1.3f, 0.1f, cageColor, 6, false, squareSprite);
                Shape("Passaro", gaiola.transform, new Vector2(4.5f, 4.15f), new Vector2(0.45f, 0.35f), circleSprite, birdColor, 5);
                Part(aberta.transform, "Grade", 3.9f, 3.8f, 0.07f, 1.2f, cageColor, 6, false, squareSprite);
                Part(aberta.transform, "Grade", 5.02f, 3.8f, 0.07f, 1.2f, cageColor, 6, false, squareSprite);
                Part(aberta.transform, "Teto", 3.85f, 4.95f, 1.3f, 0.1f, cageColor, 6, false, squareSprite);
            }
            With(birdT.AddState("Preso na gaiola"), gaiola);
            Consequence(With(birdT.AddState("Livre", TemporalCondition.Flag(birdFreed)), aberta));
            Interact(birdT.transform, new Vector2(4.5f, 4.3f), 1.3f, birdFreed, "Libertar o pássaro", "O pássaro voou livre. Talvez ele encontre outros...");

            var flockT = TemporalHolder("Bando de pássaros", new Vector2(12f, 5f));
            var birds = Holder(flockT.transform, "Passaros", new Vector2(12f, 5f));
            Vector2[] spots = { new Vector2(10.5f, 3.4f), new Vector2(11.4f, 4f), new Vector2(12.6f, 3.6f), new Vector2(13.5f, 4.4f), new Vector2(9.6f, 4.6f), new Vector2(14.3f, 3.2f) };
            foreach (var p in spots)
            {
                if (tiles != null) Bird(birds.transform, p + Vector2.up * 1.5f, 0.8f, 7, true, 1.5f + p.x % 1.2f);
                else Shape("Passaro", birds.transform, p, new Vector2(0.45f, 0.3f), circleSprite, birdColor, 7);
            }
            flockT.AddState("Nenhum pássaro");
            Consequence(With(flockT.AddState("O bando cresceu", TemporalCondition.Flag(flock)), birds));

            WaterMesh(groupEnv, "Nascente", 8f, 0.28f, 4f, 0.28f, 1.5f, 3, RiverTop, RiverBottom);

            var vinesT = TemporalHolder("Trepadeira invasora", new Vector2(12.75f, 1.3f));
            var vineColor = new Color(0.45f, 0.3f, 0.55f);
            var trepadeira = Holder(vinesT.transform, "Viva", new Vector2(12.75f, 1.3f));
            if (tiles != null)
            {
                Decor(trepadeira.transform, tiles.bush, new Vector2(12.75f, 0f), 0.55f, 3, new Color(0.75f, 0.55f, 0.9f));
                for (int i = 0; i < 4; i++) Decor(trepadeira.transform, tiles.lavender, new Vector2(12.05f + i * 0.45f, 0f), 1.4f, 4);
            }
            else
            {
                Part(trepadeira.transform, "Moita", 12f, 0f, 1.5f, 2.6f, vineColor, 3, false);
                Shape("Espinhos", trepadeira.transform, new Vector2(12.75f, 2.6f), new Vector2(2f, 1.2f), circleSprite, vineColor, 3);
            }
            var tocos = Holder(vinesT.transform, "Comida", new Vector2(12.75f, 0.2f));
            if (tiles != null)
            {
                Decor(tocos.transform, tiles.sprout, new Vector2(12.3f, 0f), 1f, 3, new Color(0.6f, 0.45f, 0.3f));
                Decor(tocos.transform, tiles.sprout, new Vector2(13.1f, 0f), 0.8f, 3, new Color(0.6f, 0.45f, 0.3f));
            }
            else
            {
                Part(tocos.transform, "Toco", 12.1f, 0f, 0.3f, 0.4f, new Color(0.4f, 0.35f, 0.3f), 3, false);
                Part(tocos.transform, "Toco", 12.9f, 0f, 0.3f, 0.3f, new Color(0.4f, 0.35f, 0.3f), 3, false);
            }
            With(vinesT.AddState("Sufocando a nascente"), trepadeira);
            Consequence(With(vinesT.AddState("Comida pelo bando", TemporalCondition.Flag(vinesEaten)), tocos));

            var riverT = TemporalHolder("Rio", new Vector2(20f, 0f));
            var agua = Holder(riverT.transform, "Agua", new Vector2(20f, 0f));
            WaterMesh(agua.transform, "Leito", 12f, 0.3f, 16f, 0.3f, 2.5f, 3, RiverTop, RiverBottom);
            Waterfall(agua.transform, "Queda", 27.6f, -9f, 0.6f, 9.3f);
            riverT.AddState("Nascente parada");
            Consequence(With(riverT.AddState("O rio voltou a correr", TemporalCondition.Flag(river)), agua));

            var treeT = TemporalHolder("Árvore da beira", new Vector2(25f, 0f));
            var terra = Part(treeT.transform, "TerraFertil", 24.3f, 0f, 1.4f, 0.15f, new Color(0.3f, 0.2f, 0.12f), 3, false, squareSprite);
            var semente = Holder(treeT.transform, "Semente", new Vector2(25f, 0.1f));
            Shape("Monte", semente.transform, new Vector2(25f, 0.1f), new Vector2(0.8f, 0.35f), circleSprite, new Color(0.35f, 0.25f, 0.15f), 3);
            Part(semente.transform, "Broto", 24.95f, 0.1f, 0.1f, 0.35f, new Color(0.45f, 0.8f, 0.35f), 4, false, squareSprite);
            var mudaSeca = Part(treeT.transform, "MudaSeca", 24.9f, 0f, 0.2f, 0.9f, new Color(0.55f, 0.45f, 0.25f), 4, false, squareSprite);
            GameObject Leaning(string name, Color leaves)
            {
                var h = Holder(treeT.transform, name, new Vector2(25f, 0f));
                if (tiles != null) SeasonalTree(h.transform, new Vector2(25f, 0f), 1, 1.2f, 3);
                else
                {
                    Part(h.transform, "Tronco", 24.6f, 0f, 0.8f, 7f, WoodColor, 4, false);
                    Shape("Copa", h.transform, new Vector2(25f, 7.4f), new Vector2(3.5f, 2.4f), circleSprite, leaves, 3);
                }
                h.transform.rotation = Quaternion.Euler(0f, 0f, -28f);
                return h;
            }
            var inclinada = Leaning("ArvoreInclinada", new Color(0.3f, 0.7f, 0.35f));
            var caida = Holder(treeT.transform, "TroncoPonte", new Vector2(32f, 0f));
            Part(caida.transform, "Toco", 24.6f, 0f, 0.8f, 0.6f, WoodColor, 4, false);
            Rect("Tronco (ponte)", caida.transform, 25.4f, -0.45f, 14f, 0.5f, ConsequenceColor, 4, true);
            Shape("Copa caída", caida.transform, new Vector2(39.8f, 0.4f), new Vector2(3f, 1.6f), circleSprite, new Color(0.3f, 0.6f, 0.3f), 3);

            With(treeT.AddState("Terra fértil"), terra);
            With(treeT.AddState("Semente plantada", TemporalCondition.Flag(seed)), semente);
            With(treeT.AddState("Muda sem água", TemporalCondition.Flag(seed, 10)), mudaSeca);
            Consequence(With(treeT.AddState("Árvore inclinada sobre o rio", TemporalCondition.Flag(treeGrown)), inclinada));
            Consequence(With(treeT.AddState("Tronco caído: uma ponte!", TemporalCondition.Flag(treeFallen)), caida));
            Interact(treeT.transform, new Vector2(25f, 0.7f), 1.3f, seed, "Plantar semente", "Você plantou uma semente à beira do abismo.");

            Enemy(slimePrefab, "Lodo", 17f, 0f, 3f, 3f);
            Enemy(mothPrefab, "Lagarta da Beira", 21f, 0f, 1.5f, 1.5f, "L5_Lagarta1_Morta");
            Enemy(mothPrefab, "Lagarta do Outro Lado", 40.5f, 0f, 1.5f, 1.5f, "L5_Lagarta2_Morta");
            Ground(47f, -5f, 8f, 12f);
            Enemy(thornPrefab, "Espinheiro do Casulo", 49.2f, 7f, 0f, 0f);
            Pickup("pinha", "Pinha", new Vector2(33f, 0.9f), "Uma pinha caiu do tronco! Ela vai com você para qualquer dia.", TemporalCondition.Flag(treeFallen));
            PlantedPine("Pinheiro da Outra Margem", 43.8f, 0f, "L5_PinhaPlantada", 20, "pinha", "Pinha", "Plantar a pinha", "Terra fértil. No verão ela seca: só a primavera faz uma pinha brotar.",
                new[] { new Vector3(42f, 2.6f, 3f), new Vector3(43.5f, 5f, 3f), new Vector3(44.8f, 7.3f, 3f) },
                TemporalCondition.In(Season.Primavera));

            if (tiles == null) Shape("Casulo", groupEnv, new Vector2(53.5f, 8.6f), new Vector2(1.6f, 2.6f), circleSprite, new Color(1f, 0.8f, 0.35f, 0.6f), 5);

            Sign(-10f, 0f, "Tudo está conectado.\nUma escolha pode atravessar semanas... e estações.");
            Sign(-5.5f, 0f, "Uma caixa velha. Pode ajudar a alcançar lugares altos.");
            Sign(0f, 0f, "Um pássaro preso lá no alto.\nSem ajuda, a espécie vai desaparecer deste vale.");
            Sign(10f, 0f, "A trepadeira invasora sufoca a nascente.");
            Sign(19f, 0f, "Lagartas na primavera viram mariposas famintas no verão.\nNo outono, o ciclo delas termina.");
            Sign(23f, 0f, "Terra fértil à beira do abismo. Uma árvore precisa de água para crescer.");
            Scenery(0f, -11f, -1.5f, 9f, 16f);
            Scenery(0f, 39.2f);
            Scenery(7f, 49.5f);
            Sign(40.5f, 0f, "A pinha precisa ser plantada na primavera...\nmas na primavera ainda não existe ponte. Ou existe outro jeito?");
            Sign(57f, 7f, "C congela o tempo por alguns segundos: inimigos e ferrões param.\nO dano que você causa num dia continua valendo nos dias seguintes.");
            HiveArena(hiveWall);
            Exit(78.5f, 8.2f);
            Save(scene, "Level05");
        }
    }
}

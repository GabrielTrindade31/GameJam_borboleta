using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

namespace ButterflyStep.EditorTools
{
    public static partial class GameBuilder
    {
        private const string StoryText =
            "Eco não é deste tempo. Ele vivia no fluxo dos dias, guiado pelo seu Relógio,\n" +
            "até que o Cronófago, uma criatura que devora estações, estilhaçou o relógio em dez fragmentos.\n\n" +
            "Sem ele, Eco ficou preso neste vale, num tempo que não é o seu.\n" +
            "Para voltar para casa, precisa reunir os fragmentos espalhados pelos dias e estações.\n\n" +
            "Mas cuidado: aqui, cada pequeno gesto ecoa no futuro.\n" +
            "Uma borboleta que bate as asas hoje pode fechar ou abrir o caminho de volta.";

        private const string ControlsText =
            "A / D  ou  ← →          andar\n" +
            "ESPAÇO                  pular\n" +
            "J  ou  clique           atacar  (pule em cima de inimigos pequenos)\n" +
            "F                       interagir\n" +
            "Q / E                   voltar / avançar no tempo\n" +
            "SHIFT + Q / E           espiar outro dia sem viajar\n" +
            "C                       pausar o tempo\n" +
            "ESC                     pausa\n" +
            "R                       reiniciar a fase\n\n" +
            "Controle: analógico, A pular, X atacar, Y interagir, LB/RB tempo, LT espiar, RT pausar o tempo, START pausa";

        private const string CreditsText =
            "BUTTERFLY STEP — protótipo de Game Jam (tema: Efeito Borboleta)\n\n" +
            "Arte: Legacy Fantasy – High Forest (Anokolisa) · Free Slime Mobs e Free Predator Plant (Craftpix)\n" +
            "Pássaros: Bird asset (OpenGameArt, CC0) · Fonte: Pixelify Sans (OFL)\n" +
            "Música e efeitos: gerados por código\n\n" +
            "“Toda escolha deixa uma marca no futuro.”";

        private static void BuildMenuScene()
        {
            var scene = NewScene();

            var camGo = new GameObject("Camera") { tag = "MainCamera" };
            var cam = camGo.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 7f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.62f, 0.82f, 0.9f);
            camGo.transform.position = new Vector3(0f, 1f, -10f);
            camGo.AddComponent<AudioListener>();
            camGo.AddComponent<UniversalAdditionalCameraData>();
            var lightGo = new GameObject("GlobalLight2D");
            var light = lightGo.AddComponent<Light2D>();
            light.lightType = Light2D.LightType.Global;

            var canvas = MakeCanvas("MenuCanvas", 0);
            var root = canvas.transform;
            var seasonLabel = UIText(UIRect("Estacao", root, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-40f, -30f), new Vector2(400f, 40f)), "PRIMAVERA", 26, TextAnchor.MiddleRight, new Color(1f, 1f, 1f, 0.85f), FontStyle.Bold);

            var cycle = camGo.AddComponent<MenuSeasonCycle>();
            cycle.Setup(cam, seasonLabel);

            groupBackground = new GameObject("--- Background").transform;
            groupEnv = new GameObject("--- Cenario").transform;
            if (skySprite != null && tiles != null)
            {
                ArtLayer("Sky", skySprite, 0f, -20f, 20f, -12f, 30f, Color.white, -100);
                ArtLayer("Mountains_Far", tiles.mountLight, 0f, -20f, 20f, -12f, 18f, new Color(0.9f, 0.96f, 1f, 0.6f), -98);
                Silhouettes("Trees_Far", tiles.treeLight, 0f, -20f, 20f, -5f, 10f, 5f, new Color(0.92f, 0.98f, 1f, 0.8f), -96);
                ArtLayer("Mountains_Near", tiles.mountDark, 0f, -20f, 20f, -14f, 15f, new Color(0.85f, 0.9f, 0.95f), -94);
                Silhouettes("Trees_Near", tiles.treeDark, 0f, -20f, 20f, -5f, 8f, 4f, Color.white, -92);
            }
            Ground(-20f, -9f, 40f, 5f);

            if (tiles != null)
            {
                float[] pines = { 3f, 9.5f, 13.5f, -12.5f };
                int[] sizes = { 1, 2, 0, 1 };
                for (int i = 0; i < pines.Length; i++)
                {
                    Sprite green = sizes[i] == 2 ? tiles.pineBig : sizes[i] == 1 ? tiles.pineMid : tiles.pineSmall;
                    Sprite gold = sizes[i] == 2 ? tiles.pineBigGold : sizes[i] == 1 ? tiles.pineMidGold : tiles.pineSmallGold;
                    Sprite red = sizes[i] == 2 ? tiles.pineBigRed : sizes[i] == 1 ? tiles.pineMidRed : tiles.pineSmallRed;
                    Sprite bare = sizes[i] == 2 ? tiles.pineBigBare : sizes[i] == 1 ? tiles.pineMidBare : tiles.pineSmallRed;
                    var go = Go("Pinheiro", groupEnv, new Vector2(pines[i], -4f));
                    var sr = AddSprite(go, green, Color.white, -20);
                    float scale = sizes[i] == 2 ? 0.75f : 1f;
                    go.transform.localScale = new Vector3(scale, scale, 1f);
                    cycle.AddSeasonal(sr, green, gold, red, bare);
                }
                Decor(groupEnv, tiles.mushroom, new Vector2(7.2f, -4f), 1f, -10);
                Decor(groupEnv, tiles.blueFlower, new Vector2(11.5f, -4f), 1f, -10);
                if (fxLibrary != null && fxLibrary.timeClock != null)
                {
                    var fragment = Go("Fragmento do Relógio", groupEnv, new Vector2(15.5f, -2f));
                    var fragSr = AddSprite(fragment, fxLibrary.timeClock, Color.white, -9);
                    fragSr.sharedMaterial = unlitSprite;
                    fragment.AddComponent<PulseGlow>();
                }
            }

            if (playerArt != null)
            {
                var hero = Go("Eco", groupEnv, new Vector2(6f, -4f));
                var sr = AddSprite(hero, playerArt.First, Color.white, 20);
                hero.transform.localScale = new Vector3(-1.4f, 1.4f, 1f);
                AttachAnimator(hero, sr, playerArt, "Idle");
            }

            foreach (var r in groupBackground.GetComponentsInChildren<SpriteRenderer>()) cycle.AddTinted(r);
            foreach (var r in groupEnv.GetComponentsInChildren<SpriteRenderer>())
            {
                if (r.name != "Eco") cycle.AddTinted(r);
            }

            var weather = new ParticleSystem[4];
            weather[0] = MakeWeather("Weather_Primavera", camGo.transform, new Color(1f, 0.7f, 0.85f, 0.9f), 10f, -1.2f, 0.12f, 0.6f, false);
            weather[1] = MakeWeather("Weather_Verao", camGo.transform, new Color(1f, 0.95f, 0.5f, 0.7f), 8f, 0.25f, 0.08f, 0.4f, true);
            weather[2] = MakeWeather("Weather_Outono", camGo.transform, new Color(0.95f, 0.5f, 0.2f, 0.95f), 14f, -1.6f, 0.18f, 1.2f, false);
            weather[3] = MakeWeather("Weather_Inverno", camGo.transform, new Color(1f, 1f, 1f, 0.95f), 45f, -2.2f, 0.12f, 0.5f, false);
            cycle.SetWeather(weather);
            var pollen = MakeParticles("Polen", groupEnv, new Color(1f, 0.85f, 0.35f), 0, 0.5f, 3f, 0.12f, true, 3f, -0.03f);
            pollen.transform.position = new Vector3(8f, -1f, 0f);
            var pollenEm = pollen.emission;
            pollenEm.rateOverTime = 6f;
            var pollenMain = pollen.main;
            pollenMain.loop = true;
            pollenMain.playOnAwake = true;

            var title = UIRect("Titulo", root, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(90f, -60f), new Vector2(900f, 120f));
            UIText(title, "BUTTERFLY STEP", 100, TextAnchor.MiddleLeft, new Color(1f, 0.87f, 0.45f), FontStyle.Bold);
            UIText(UIRect("Subtitulo", root, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(96f, -180f), new Vector2(900f, 44f)), "Um passo pequeno. Um futuro inteiro.", 32, TextAnchor.MiddleLeft, new Color(1f, 1f, 1f, 0.9f), FontStyle.Italic);

            var main = Stretch("Principal", root);
            var mainGroup = Group(main, 1f);
            var left = new Vector2(0f, 0.5f);
            var cont = MenuButton(main, "Continuar", "Continuar", left, new Vector2(300f, 120f), new Vector2(400f, 70f), 30);
            var newGame = MenuButton(main, "NovoJogo", "Novo jogo", left, new Vector2(300f, 35f), new Vector2(400f, 70f), 30);
            var chapters = MenuButton(main, "Capitulos", "Capítulos", left, new Vector2(300f, -50f), new Vector2(400f, 70f), 30);
            var controlsBtn = MenuButton(main, "Controles", "Controles", left, new Vector2(300f, -135f), new Vector2(400f, 70f), 30);
            var credits = MenuButton(main, "Creditos", "Créditos", left, new Vector2(300f, -220f), new Vector2(400f, 70f), 30);
            var quit = MenuButton(main, "Sair", "Sair", left, new Vector2(300f, -305f), new Vector2(400f, 70f), 30);
            var contLabel = cont.GetComponentInChildren<Text>();
            UIText(UIRect("Dica", main, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 30f), new Vector2(1200f, 36f)), "Setas / analógico para escolher  ·  ENTER / A para confirmar  ·  ESC / B para voltar", 20, TextAnchor.MiddleCenter, new Color(1f, 1f, 1f, 0.7f));

            RectTransform Panel(string name, Vector2 size, out CanvasGroup group)
            {
                var overlay = Stretch(name, root);
                UIImage(overlay, new Color(0f, 0f, 0f, 0.45f));
                group = Group(overlay, 0f);
                var box = UIRect("Caixa", overlay, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, size);
                UIImage(box, tiles != null ? Color.white : new Color(0.12f, 0.1f, 0.08f, 0.95f), tiles != null ? tiles.uiParchment : null);
                return box;
            }

            var dark = new Color(0.3f, 0.18f, 0.08f);
            var chaptersBox = Panel("PainelCapitulos", new Vector2(1400f, 800f), out var chaptersGroup);
            UIText(UIRect("Titulo", chaptersBox, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -30f), new Vector2(1000f, 70f)), "CAPÍTULOS", 54, TextAnchor.MiddleCenter, dark, FontStyle.Bold, false);
            var chapterButtons = new Button[10];
            var chapterLabels = new Text[10];
            for (int i = 0; i < 10; i++)
            {
                int col = i % 5;
                int row = i / 5;
                var b = MenuButton(chaptersBox, $"Capitulo{i + 1}", $"{i + 1}.", new Vector2(0.5f, 0.5f), new Vector2(-520f + col * 260f, 110f - row * 170f), new Vector2(240f, 140f), 22);
                chapterButtons[i] = b;
                chapterLabels[i] = b.GetComponentInChildren<Text>();
            }
            var reset = MenuButton(chaptersBox, "Apagar", "Apagar progresso", new Vector2(0.5f, 0f), new Vector2(-220f, 70f), new Vector2(360f, 64f), 24);
            var backChapters = MenuButton(chaptersBox, "Voltar", "Voltar", new Vector2(0.5f, 0f), new Vector2(220f, 70f), new Vector2(300f, 64f), 26);

            var controlsBox = Panel("PainelControles", new Vector2(1300f, 760f), out var controlsGroup);
            UIText(UIRect("Titulo", controlsBox, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -30f), new Vector2(1000f, 70f)), "CONTROLES", 54, TextAnchor.MiddleCenter, dark, FontStyle.Bold, false);
            UIText(UIRect("Texto", controlsBox, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 10f), new Vector2(1100f, 520f)), ControlsText, 26, TextAnchor.MiddleLeft, dark, FontStyle.Normal, false);
            var backControls = MenuButton(controlsBox, "Voltar", "Voltar", new Vector2(0.5f, 0f), new Vector2(0f, 70f), new Vector2(300f, 64f), 26);

            var creditsBox = Panel("PainelCreditos", new Vector2(1300f, 620f), out var creditsGroup);
            UIText(UIRect("Titulo", creditsBox, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -30f), new Vector2(1000f, 70f)), "CRÉDITOS", 54, TextAnchor.MiddleCenter, dark, FontStyle.Bold, false);
            UIText(UIRect("Texto", creditsBox, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 10f), new Vector2(1150f, 380f)), CreditsText, 26, TextAnchor.MiddleCenter, dark, FontStyle.Normal, false);
            var backCredits = MenuButton(creditsBox, "Voltar", "Voltar", new Vector2(0.5f, 0f), new Vector2(0f, 70f), new Vector2(300f, 64f), 26);

            var storyBox = Panel("PainelHistoria", new Vector2(1400f, 700f), out var storyGroup);
            UIText(UIRect("Titulo", storyBox, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -30f), new Vector2(1000f, 70f)), "O RELÓGIO PARTIDO", 50, TextAnchor.MiddleCenter, dark, FontStyle.Bold, false);
            UIText(UIRect("Texto", storyBox, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 20f), new Vector2(1250f, 420f)), StoryText, 28, TextAnchor.MiddleCenter, dark, FontStyle.Normal, false);
            var start = MenuButton(storyBox, "Comecar", "Começar", new Vector2(0.5f, 0f), new Vector2(-180f, 70f), new Vector2(320f, 64f), 28);
            var backStory = MenuButton(storyBox, "Voltar", "Voltar", new Vector2(0.5f, 0f), new Vector2(180f, 70f), new Vector2(300f, 64f), 26);

            var fade = Stretch("Fade", root);
            UIImage(fade, Color.black);
            var fadeGroup = Group(fade, 1f);
            EnsureEventSystem(root);

            var menu = canvas.gameObject.AddComponent<MainMenu>();
            menu.Setup(mainGroup, chaptersGroup, controlsGroup, creditsGroup, storyGroup, fadeGroup);
            menu.SetMainButtons(newGame, cont, contLabel, chapters, controlsBtn, credits, quit);
            menu.SetOtherButtons(chapterButtons, chapterLabels, reset, start, new[] { backChapters, backControls, backCredits, backStory });
            Save(scene, "MainMenu");
        }
    }
}

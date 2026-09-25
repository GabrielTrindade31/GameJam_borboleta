# BUTTERFLY STEP

> *Um passo pequeno. Um futuro inteiro.*

Protótipo de Game Jam (tema **EFEITO BORBOLETA**) — Plataforma 2D + Puzzle + Manipulação Temporal.
Unity **6000.3.12f1** · URP 2D · Input System.

Projeto Unity: pasta `Gamejam_borboleta/`. Tudo do jogo está em `Assets/_Game/`.

---

## 1. História

**Eco não é deste tempo.** Ele vivia no fluxo dos dias, guiado pelo seu Relógio, até que o **Cronófago**, uma criatura que devora estações, estilhaçou o relógio em **dez fragmentos**. Preso neste vale, num tempo que não é o seu, Eco precisa reunir os fragmentos (um no fim de cada fase) para voltar para casa. Mas cada pequeno gesto ecoa no futuro: é o **efeito borboleta**. No fim, o relógio volta a bater, e o vale que Eco deixa para trás foi mudado por cada escolha dele.


## 2. Conceito

Cada fase tem uma **linha do tempo em dias** (ex.: Dia 1 → Dia 61, de 10 em 10 dias) e um **calendário com estações**
(Primavera, Verão, Outono, Inverno — 30 dias cada). O jogador avança e volta entre os dias.

- Ações feitas em um dia (regar, empurrar, quebrar, libertar, derrotar) ficam registradas e **alteram todos os dias seguintes**.
- As **estações mudam o mundo**: rios congelam no inverno, a água degela na primavera, plantas murcham, folhas mudam de cor.
- As **criaturas vivem no tempo**: nascem, crescem, ganham habilidades, hibernam, congelam, **morrem de velhice**.
- Voltar e mudar uma decisão **recalcula o futuro**.

## 3. Controles

| Ação | Teclado | Gamepad |
|---|---|---|
| Andar | A / D ou ← → | Analógico / D-pad |
| Pular | Espaço | A (Sul) |
| Interagir | F | Y (Norte) |
| Atacar | J ou clique esquerdo | X (Oeste) |
| **Voltar dias** | **Q** | LB |
| **Avançar dias** | **E** | RB |
| **Espiar outro dia** (sem viajar) | segurar **Shift** + Q/E | LT + LB/RB |
| **Pausar o tempo** (congela inimigos e projéteis) | **C** | RT |
| Reiniciar fase | R | Select |
| Menu → começar | Enter / Espaço | Start |

Debug: **F1** painel · **F2/F3** muda o dia sem cooldown · **F5** reinicia · **F6** pula fase.
Trocar teclas: `Assets/_Game/Settings/ButterflyControls.inputactions`.

Empurre caixas andando contra elas. Inimigos pequenos (marcados como *Stompable*) morrem com um pulo em cima.

## 4. Como jogar

Abra `Assets/_Game/Scenes/MainMenu.unity` e aperte **Play**.
Ordem (já no Build Settings): `MainMenu → Level01 … Level10 → Ending`. Qualquer `LevelXX` pode ser testada direto.
Clique na janela Game: fora de foco o Unity pausa o Play Mode.

### Menu principal
- Fundo vivo: as quatro estações passam a cada 6 s (céu, pinheiros, clima e música mudam).
- **Continuar** (último capítulo jogado), **Novo jogo** (mostra a história e começa o Capítulo 1), **Capítulos** (os liberados, com o pólen coletado em cada um), **Controles**, **Créditos**, **Sair**.
- Navegação por setas/analógico, ENTER/A confirma, ESC/B volta. Mouse também funciona.
- O progresso (capítulos liberados, pólen, último capítulo) fica salvo em `PlayerPrefs` (`GameProgress`). "Apagar progresso" fica na tela de Capítulos.

### O que deixa o jogo mais gostoso
- **Flor do Tempo (checkpoint)**: ao morrer, o Eco renasce na última flor tocada **com o mundo e o dia exatamente como estavam** (nada do que você fez se perde). `R` ainda reinicia a fase inteira.
- **Borboletas do tempo** (colecionáveis): 5 por fase. Uma borboleta azul batendo as asas diante de um relógio que gira. Vários só aparecem numa estação ou em lugares que só existem depois de uma consequência (árvore crescida, gelo, tronco caído...). Aparece no HUD e no menu de capítulos.
- **Pausa (ESC)**: continuar, reiniciar a fase ou voltar ao menu.
- **Som**: efeitos para pulo, golpe, salto no tempo, bloqueio, coleta, checkpoint etc. e **música que muda com a estação** — tudo gerado por código (`SoundSynth`, `GameAudio`), sem arquivos de áudio.
- **Impacto**: pequena congelada (hit-stop) ao acertar, poeira ao pular/aterrissar.
- **Golpe maior**: o alcance da espada quase dobrou (caixa de acerto 2,3 × 1,7). O corte usa um sprite de golpe em pixel art e cada acerto solta um anel de impacto.
- **Água**: rios e lagos fundos **afogam** o Eco (ele afunda e renasce na última Flor do Tempo). A superfície é uma malha com ondas: quem cai faz ondas e respingos, e a correnteza aparece como partículas. No inverno a água congela: a mesma água fica parada sob uma placa de gelo translúcida com rachaduras e reflexos, e dá para andar em cima. O componente `WaterBody` tem a opção **Mortal**; desligada, a água volta a permitir nadar (ESPAÇO braçada, S mergulha, W sobe).
- **Morte das criaturas e o tempo**:
  - Ao morrer, a criatura pisca, o corpo fica **parado no chão** (não é mais empurrado nem lançado) e um bando de **borboletas** sai dele. Um **relógio** gira e some: ela foi apagada da linha do tempo.
  - **Avançando** para depois do dia da morte, o relógio e duas borboletas aparecem onde ela estava, e só sobram os restos.
  - Os restos **envelhecem**: desbotam com os dias e, depois de 15 dias, **flores** nascem no lugar com uma borboleta voando em volta.
  - **Voltando** para antes da morte, a criatura reaparece com um redemoinho e fica com um **relógio girando sobre a cabeça**, que marca que você a derrota mais tarde.
  - Quando uma criatura envelhece ou evolui, aparece um redemoinho temporal.
- Ao coletar uma borboleta do tempo, ela **some** e outras saem voando junto com uma explosão de brilho.
- **Ataques dos bichos**: o espinheiro cospe um **esporo** verde animado e a onda de choque do chefe virou um **∞ roxo do tempo** (efeitos CC0).
- **Animações que estavam sem uso**: dano (slime, planta, abelha, javali, caracol se escondendo) e ataque (slime ao pular, abelha ao atirar).
- **Cenário**: pedras com musgo, **pedras com runas azuis que pulsam**, pilhas de troncos, cercas, pedregulhos e cogumelos, tirados do pacote Legacy Fantasy.
- **Fase 5 (Rainha Vespa)**: a arena virou o interior de uma colmeia. Tem parede de favos, janelas hexagonais iluminadas, colmeias e mel pendurados nos galhos, arco de colmeia no portão, baú aberto com uma gema azul e uma colher de mel. Antes da arena, uma **entrada de mina** fica cavada no penhasco.
- **Fase 3 redesenhada**: o lago agora fica numa **colina ligada ao chão**, com dois degraus de terra para subir. Saíram as plataformas de madeira soltas no ar. As cachoeiras caem pela face da colina até o riacho, e a muralha sobe além da câmera, em vez de parecer um pilar solto. A margem do lago é mais larga, a água rasa tem espuma fina e o túnel tem pilares e arco de madeira.
- **Árvores escuras**: nova camada de pinheiros escuros (Dark-Tree) entre o fundo e as árvores de perto, para dar profundidade.
- **Troncos ocos**: o tronco oco e o toco oco (Tree-Assets) entram na rotação de peças de cenário de todas as fases.
- **Árvores amarelas**: metade das árvores de cenário usa a variante amarela (Yellow-Tree) no verão e fica dourada no outono, misturando as cores da floresta.
- **Novas mecânicas** (cada uma tem um puzzle obrigatório):
  - **Cogumelo saltador** (`BouncePad`): brota no outono e sempre dá o impulso cheio.
    - **Fase 3 (obrigatório):** a saída fica num patamar alto do penhasco. Plante o pinheiro a tempo para subir ao penhasco e, no outono (Dia 91), pule no cogumelo.
    - **Fase 1 (extra):** leva a uma borboleta em cima do paredão.
  - **Trepadeira** (`ClimbZone`): cresce na primavera e no verão e seca no outono e no inverno. Segure W/S para escalar e ESPAÇO para saltar dela.
    - **Fase 6 (obrigatório):** a saída fica no alto de um paredão no fim do corredor. Atravesse no inverno pela neve e volte para a primavera ao pé do paredão, ou passe pelos espinheiros com a pausa do tempo.
    - **Fase 2 (extra):** sobe o barranco até uma borboleta.
  - **Vento de estação** (`WindZone2D`): empurra o Eco e muda o alcance do pulo.
    - **Fase 8 (obrigatório):** um abismo largo antes da saída só pode ser cruzado no outono, com o vento a favor. Isso fecha a sequência de estações da fase.
    - **Fase 6:** a nevasca de inverno sopra contra você no corredor.
- **Combate com chefes**:
  - aviso antes de cada ataque: anel vermelho no chão onde a pancada vai cair e anel no chefe antes da investida e da rajada;
  - estrelas sobre a cabeça quando atordoado, sem dano por contato enquanto ele está tonto;
  - acerto com clarão branco, pausa de impacto, tremida e borboletas;
  - com metade da vida ele fica **furioso** e ataca mais rápido;
  - mudança de fase (ao trocar de dia) com redemoinho;
  - derrota em câmera lenta, com relógio do tempo e enxame de borboletas.
- **Revisão das fases 7, 9 e 10**:
  - Fase 9: a "Rocha Alta" virou um paredão com uma passagem de caverna embaixo; na primavera e no verão, trepadeiras espinhosas (feitas com as peças do pacote) fecham a passagem. A placa de pressão agora é visível no buraco do chão e a engrenagem fica presa na parede do relógio.
  - Fase 10: o portal da arena virou uma passagem na rocha, e as plataformas de madeira ganharam postes e travessa.
  - Fase 7: juncos na ilha do lago.
  - Fases 4, 6 e 8: o batente do portão do fosso (fase 4) voltou a ter a altura certa (a regra de muros altos o esticava até o céu); o monte de neve da fase 6 ficou mais largo e com degraus menores, parecendo um monte e não um bolo; a textura de neve perdeu a faixa azulada que marcava cada degrau.
  - Fases 1 e 2: os galhos das árvores (fase 1) usam a altura real do sprite de galho, em vez de uma tira achatada, sem mudar a colisão. A engrenagem da porta da fase 2 foi presa na parede, em vez de flutuar ao lado.
  - Fases 3 e 5 (segunda revisão): o fundo do lago da fase 3 não tem mais grama por baixo da água; a plataforma do ninho do pássaro na fase 5 ganhou postes e travessa.
  - Neve: o monte da fase 6 e a nevasca da fase 8 viraram montes arredondados em pixel art (a colisão da fase 6 segue o formato do monte em degraus invisíveis; a da fase 8 é um bloqueio). No inverno o chão de todas as fases fica coberto de neve; nas outras estações (e no editor) aparece a grama.
  - Árvore caída da fase 6: um tronco deitado grosso com arbustos presos a ele: verdes na primavera, secos (laranja-marrom) no outono com folhas secas no chão, brancos de neve no inverno, e somem no verão.
  - Trepadeiras e espinhos usam sprites em pixel art próprios com fundo transparente (as peças do pacote tinham fundo escuro). A parede de espinhos da fase 9 cobre toda a passagem.
  - Cachoeira da fase 3: borda curvando, sombra e brilho nas laterais e névoa na base; a água rasa ficou mais azul.
  - Fase 4: a cronofera anciã agora patrulha no meio da ponte e cai no fosso quando a ponte quebra (antes ela ficava apoiada no batente do portão). O toco oco saiu do cenário.
- **Estilo único**: a trepadeira usa o caule e as folhas do pacote. Neve, gelo e flocos usam texturas em pixel art geradas no mesmo tamanho de pixel (32 px por unidade, filtro ponto). As folhas ao vento são as folhas do pacote.
- **Limpeza visual**:
  - o céu acompanha a câmera, sem faixa lisa no alto;
  - paredes de limite e muros vão até fora da tela, em vez de terminar no ar;
  - a colmeia da fase 5 ganhou parede e teto de favo;
  - a árvore caída da fase 6 virou um pinheiro tombado de verdade (era um bloco de tábuas).
- **Recortes de sprites**: os retângulos corrigidos à mão no `Tiles.png` (tronco alto e vitória-régia com caule) agora estão no código, então o *Construir Projeto* não os desfaz. Corrigi também `rockSmall` e a pedra larga com musgo, e a folha `Hive.png` passou a ter recortes próprios.
- **Monte de neve** (fase 6): virou degraus arredondados de neve que dá para escalar. A **nevasca** da fase 8 continua fechando a passagem, porque ela faz parte do puzzle.
- **Fundo**: as árvores de trás são montadas em grupos contínuos (começo + meios + fim), como a arte foi desenhada, em vez de pedaços soltos.
---

## 5. Arquitetura

```
LevelSystems (1 por fase)
 ├─ LevelContext   → ponto central da fase (Tempo, Mundo, Player, HUD)
 ├─ TimeManager    → dia atual, dias possíveis, calendário/estações, avançar/voltar
 ├─ WorldState     → flags (ações e consequências) + posições registradas
 ├─ LevelFlow      → título, textos, próxima cena, morte/conclusão
 ├─ FeedbackFX     → partículas de feedback
 └─ TimeAtmosphere → cor do céu/luz e clima por estação (pétalas, pólen, folhas, neve)

Objetos do mundo (cada um decide sozinho como reage ao tempo)
 ├─ TemporalObject  → estados visuais/físicos por condição
 ├─ TemporalEnemy   → estágios de vida (tamanho, velocidade, comportamento, existência...)
 ├─ PushableBox     → posição registrada por dia
 ├─ Interactable    → ação do jogador que registra uma flag
 ├─ ConsequenceRule → consequência derivada de outras flags (encadeável)
 ├─ PositionSensor  → flag ativa quando uma caixa está numa área
 └─ FragilePlatform → quebra quando um inimigo pesado pisa
```

Pastas principais:

```
Assets/_Game/
  Art/Placeholders    sprites gerados (Square, Block, Circle, Triangle, Diamond)
  Prefabs/Player      Player
  Prefabs/Enemies     Enemy_Lodo, Enemy_Cronofera, Enemy_Mariposa, Enemy_Espinheiro, Projectile_Espinho
  Prefabs/Temporal    PushBox
  Prefabs/Environment LevelSystems, GameCamera (com clima), LevelExit_Polen, StorySign, DeathZone
  Prefabs/UI          HUD
  Scenes/             MainMenu, Level01..05, Ending
  Scripts/Time        TimeManager, SeasonCalendar, TimeState, TemporalBehaviour, TemporalObject, TimeAtmosphere
  Scripts/World       WorldState, TemporalCondition, ConsequenceRule, Interactable, PushableBox, ...
  Scripts/Enemies     TemporalEnemy, EnemyProjectile
  Scripts/Editor      GameBuilder (gera prefabs e cenas)
```

### 5.1 TimeManager (em `LevelSystems`)

- **Max Day** — último dia da fase (interno, começa em 0; o HUD mostra "Dia 1").
- **Day Step** — quantos dias cada salto avança/volta.
- **Custom Days** — opcional, lista exata de dias (ex.: 0, 5, 30, 31, 90).
- **Calendar** — `Days Per Season` (30), `Start Season` (estação do Dia 1), `Start Day Of Season`.
- Nunca é possível voltar antes do Dia 1. Se o jogador fosse aparecer dentro de algo sólido, a troca é cancelada.

### 5.2 WorldState

- **Flags diretas**: `nome → dia em que aconteceu`. Verdadeira nesse dia e em todos os seguintes.
- **Flags derivadas** (`ConsequenceRule`, `PositionSensor`): calculadas na hora → o futuro é sempre recalculado.
- **Posições** (caixas): registradas por dia; mover num dia apaga o que havia nos dias posteriores.

### 5.3 Condições (`TemporalCondition`)

| Tipo | Significado | Campo |
|---|---|---|
| `DaysSinceStartAtLeast` / `Below` | dias desde o início da fase | `value` = dias |
| `SeasonIs` / `SeasonIsNot` | estação do dia atual | `season` |
| `FlagActive` / `FlagInactive` | flag ativa **há pelo menos X dias** (0 = agora) | `flag`, `value` |
| `FlagActiveAtDay` / `FlagInactiveAtDay` | flag ativa num dia exato (0 = Dia 1) | `flag`, `value` |

### 5.4 TemporalObject

Lista de **estados**; o **último estado válido** vence. Cada estado: condições, *Visible*, *Solid*, *Sprite*, *Color*, *Scale*,
**Active Objects** (filhos ligados só nesse estado) e *Is Consequence* (brilho verde).

### 5.5 TemporalEnemy — criaturas com ciclo de vida

Cada inimigo tem uma lista de **estágios** (o último válido vence). Cada estágio define:

| Campo | Uso |
|---|---|
| Conditions | dias, estação, flags |
| Message | texto mostrado quando o jogador chega a um dia com esse estágio |
| **Present** | desmarque = a criatura não existe (morreu de velhice, migrou, hibernou) |
| **Show Remains** | mostra os restos (ossos, casca, galho seco) |
| Size / Color / Sprite | aparência |
| **Movement** | `Parado`, `Patrulha`, `Persegue`, `Pula`, `Voa` |
| Speed / Damage / Health | números |
| **Harmless** | não machuca (casulo, broto) |
| **Solid Platform** | vira plataforma (lodo congelado) |
| **Heavy** | quebra plataformas frágeis |
| **Stompable** | morre com pulo em cima |
| **Shoot Interval** | atira projéteis (0 = não atira) |

**Regras de tempo para TODOS os inimigos** (grupo *Vida no tempo* e *Domesticar* no Inspector):

| Regra | Como funciona |
|---|---|
| **Morte persistente** | Derrotado no dia X = morto do dia X em diante (fica o corpo), **vivo nos dias anteriores**. Ligado por padrão (*Persistent Death*). *Death Flag* é opcional: vazio gera um nome automático; preencha quando outra coisa depender da morte (ex.: `L6_MaeLodo`). |
| **Ferimentos persistem** | Cada golpe é registrado no dia em que aconteceu e vale para os dias seguintes. Dá para enfraquecer um inimigo no passado e terminar o serviço no futuro. |
| **Nascimento** (*Birth Day*) | Antes desse dia a criatura não existe; os estágios (`DaysSinceStart...`) contam a idade a partir do nascimento. |
| **Descendência** (*Parent Death Flag*) | A criatura só nasce se a mãe/pai estava vivo no dia do nascimento. Matar a mãe cedo apaga os filhotes do futuro. |
| **Domesticar** (*Tame Flag*, *Tame After Days*, *Tame Platform*) | Se a flag estava ativa X dias antes, a criatura fica mansa: não ataca, anda devagar e fica esverdeada. Com *Tame Platform*, para e vira plataforma. |

Ex.: matar a lagarta na primavera = nunca existirá a mariposa no verão. Lodos invocados por chefes não registram morte (somem ao mudar de dia).

Onde aparecem: **Fase 4** (comedouro: encha-o no verão e a cronofera do vale cresce mansa), **Fases 6 e 8** (mãe lodo com filhotes que nascem nos dias 31 e 61), **Fase 9** (uma cronofera que só nasce no verão).

### 5.6 Itens, portões e dicas

- **Player Inventory** (no Player): itens coletados **não pertencem a nenhum dia** — viajam no tempo com o Eco. Aparecem no canto inferior esquerdo do HUD.
- **Item Pickup**: um item no mundo. Em *Exists When* use condições (ex.: `SeasonIs Inverno`, `DaysSinceStartAtLeast 60`, `FlagActive ...`). Coletado, some de todos os dias.
- **Interactable** ganhou *Required Item* (ex.: `chave`, `pinha`, `semente`), *Consume Item* e *Blocked Prompt* (dica mostrada quando a ação não está disponível, ex.: "Congelado no inverno").
- Receita de puzzle de ida e volta: o item só existe no **futuro** e a ação só funciona no **passado** (estação ou prazo de crescimento).

### 5.7 Poderes do tempo

- **Espiar** (Shift + Q/E): mostra outro dia sem viajar. Eco vira um fantasma parado e intocável; solte Shift para voltar ao dia de origem. Ótimo para planejar.
- **Pausa do Tempo** (C): congela inimigos, chefes e projéteis por 3 s; recarrega em 7 s (barra no HUD). Configurável em `LevelSystems/TimeStasis`. Desbloqueada a partir da fase 5.
- **Fases que começam no futuro**: `TimeManager` → *Start Index* (0 = primeiro período). As fases 6 a 10 começam no fim do calendário e exigem voltar.
- **Ataques pertencem ao dia**: todo projétil some quando o dia muda — trocar de época também é uma esquiva.

### 5.8 Chefes (`TemporalBoss`)

- **Vida temporal**: o dano é registrado no dia em que acontece e vale para esse dia e todos os seguintes. Voltar para antes do golpe devolve a vida.
- **Phases**: uma por condição (estação/dias), com *Attacks* (`Investida`, `Rajada`, `Pancada`, `Invocar`), velocidade, projéteis, *Vulnerable*, *Vulnerable When Stunned* e *Stun Time*.
- **Death Flag**: registrada no dia da derrota; use num `Gate` para liberar a saída.
- A barra no topo mostra a vida **no dia atual** e se ele está vulnerável.

### Os 4 tipos de criatura

| Prefab | Ciclo |
|---|---|
| **Lodo** (desenvolvimento + estações) | Filhote → Adulto que **pula** (+20 dias) · **Ressecado** e lento no verão · **Congelado** no inverno (vira **plataforma**) |
| **Cronofera** (inversão + velhice) | Jovem **muito rápida** que persegue → Adulta (+20) → Anciã **lenta e pesada** (+40, quebra pontes) → **Morre de velhice** (+60, deixa ossos) |
| **Mariposa** (metamorfose) | Lagarta → **Casulo** inofensivo (+20) → Mariposa que **voa** e persegue (+30) → fim do ciclo (+60) |
| **Espinheiro** (planta) | Broto inofensivo → Madura que **atira espinhos** (+20) → **Murcha no inverno** |

---

## 6. Guias para a equipe (sem programar)

### 6.1 Novo objeto temporal
1. **GameObject → 2D Object → Sprites → Square**; **Add Component → Box Collider 2D** e **Temporal Object**.
2. Arraste o Sprite Renderer em *Visual* e o collider em *Colliders*.
3. Em **States**: `Normal` (sem condições) · `Congelado` (`SeasonIs Inverno`, cor azul) · `Quebrado` (`DaysSinceStartAtLeast 30`, desmarque *Visible*).

### 6.2 Ação do jogador + consequência
1. Filho `Interacao` no layer **Interactable** com **Circle Collider 2D** (*Is Trigger*).
2. **Add Component → Interactable**: *Prompt* `Regar a planta`, *Flag* `MinhaFase_PlantaRegada`.
3. No objeto temporal: estados com `FlagActive MinhaFase_PlantaRegada` e `value` 0, 10, 20...

Interagir de novo **no mesmo dia** desfaz a ação.

### 6.3 Consequência encadeada
Objeto vazio em `--- Consequence Rules` + **Consequence Rule**: *Result Flag*, *Description*, *Causes* (ex.: `FlagActive L5_TrepadeiraComida 0`).
Regras podem depender de outras (a fase 5 tem 5 em cadeia). Veja todas com **F1**.

### 6.4 Novo inimigo
Arraste um prefab de `Prefabs/Enemies` (ou duplique para criar um tipo novo) e edite os **Stages**.
Exemplos rápidos:
- *Morre de velhice*: último estágio com `DaysSinceStartAtLeast 60`, *Present* desmarcado, *Show Remains* marcado.
- *Hiberna*: estágio `SeasonIs Inverno` com *Present* desmarcado.
- *Se aprimora*: estágio posterior com *Movement* `Pula` ou *Shoot Interval* `2`.
- *Migra no outono*: `SeasonIs Outono` sem *Present*.

### 6.5 Nova fase
Duplique uma cena. Em `LevelSystems`: `TimeManager` (Max Day, Day Step, Calendar) e `LevelFlow` (textos, **Next Scene**).
No `GameCamera`: *Min/Max Bounds*. Coloque `LevelExit_Polen`, `StorySign`, `DeathZone`. Adicione a cena ao Build Settings.

### 6.6 Arte e animações

Player e inimigos já usam arte de terceiros (ver `Assets/_Game/Art/ThirdParty/CREDITS.md`):
herói do **Legacy Fantasy** (Eco), javali = **Cronofera**, caracol → casco → abelha = **Mariposa**, slime **Craftpix** = **Lodo**, planta **Craftpix** = **Espinheiro**, céu e florestas em paralaxe.

- As animações usam o componente **Sprite Animator** (no filho `Visual`): uma lista de clips com nome, quadros e FPS. Sem Animator Controller.
- Cada estágio de inimigo escolhe o clip no campo **Animation** (ex.: `Walk`, `Run`, `Hide`, `Fly`) e opcionalmente **Attack Animation**.
- O player troca sozinho entre `Idle`, `Run`, `Jump`, `Fall`, `Attack`, `Dead` (componente **Player Sprite Animation**).
- Se a pasta `ThirdParty` não existir, o construtor volta a usar os placeholders.
- **Licença:** os PNGs de terceiros não vão para o GitHub público (ver CREDITS.md).
- **Cenário:** chão, paredes, tábuas, galhos, água, porta, caixa, gaiola, placas e decoração usam os tiles do Legacy Fantasy. O construtor troca automaticamente as cores placeholder (chão, rocha, madeira, água, porta) pelos tiles.
- **Pinheiros das estações:** componente **Seasonal Sprite** — um sprite por estação (verde, dourado, vermelho, tronco pelado no inverno). Serve para qualquer objeto: é só arrastar 4 sprites.
- **UI:** fonte Pixelify Sans, painel de madeira no relógio, pergaminho nas placas, corações do HUD.

Para trocar por outra arte:
- **Player/Inimigos**: troque o sprite do filho `Visual`. Para animar, adicione um `Animator` nele (Player usa `Speed`, `VelocityY`, `Grounded`, `Jump`, `Hurt`, `Attack`). Cada estágio de inimigo também aceita um *Sprite* próprio (lagarta/casulo/mariposa).
- **Chão/paredes**: *Draw Mode = Tiled* → troque por um tile.
- **Clima**: `GameCamera/Weather_*` são Particle Systems — troque material/textura.
- **Cores das estações**: `LevelSystems/TimeAtmosphere/Looks`.

### 6.7 Regenerar tudo (cuidado)
**Butterfly Step → Construir Projeto Completo** recria prefabs e cenas a partir de `Scripts/Editor/GameBuilder*.cs` — **apaga edições manuais**.

---

## 7. As dez fases

Toda fase agora exige **ir ao futuro buscar algo e voltar ao passado para usar** (e depois avançar de novo).

### Fase 1 — O Primeiro Passo (Primavera, Dia 1–61, passo 10)
1. Regar a muda → árvore adulta no Dia 21, com galhos até o platô.
2. **No verão (Dia 31+)** o pinheiro adulto dá uma **pinha** lá no alto → pegue.
3. Pinhas só brotam **na primavera** → **volte** ao Dia 1–21 e plante na terra do platô.
4. **Avance** 20 dias: o novo pinheiro tem galhos até o penhasco alto, onde está a saída.
Extras: lagarta que vira mariposa (pise nela na primavera para ela nunca existir) e um lodo no platô.

### Fase 2 — A Porta que Ficou (Outono, Dia 1–61, passo 10)
1. Empurre a caixa para o trilho até o Dia 11 → a porta não fecha no Dia 21.
2. O rio só congela no **inverno (Dia 31–60)**. Do outro lado há um **portão trancado**.
3. A **chave** só aparece na **primavera (Dia 61)**, trazida pela correnteza até a margem → pegue.
   Cair no rio afoga: só o gelo do inverno deixa atravessar.
4. **Volte** ao inverno, atravesse o gelo e abra o portão.
Extras: espinheiros nas duas margens (atiram no outono, murcham no inverno).

### Fase 3 — Degelo (Inverno, Dia 1–91, passo 15)
1. **No inverno**, atravesse o lago congelado, pegue a **semente presa no gelo** e solte a pedrinha da represa. Fora do inverno o lago afoga.
2. O túnel na muralha só existe **60 dias depois** (verão).
3. Do outro lado, o penhasco da saída precisa de uma árvore que leva **45 dias** para crescer — plantar no Dia 61 é tarde demais.
. A saída fica num patamar alto do penhasco. No **outono (Dia 91)**, um cogumelo gigante brota ali: pule nele.
Extras: espinheiros no penhasco do lago e no penhasco da saída.

### Fase 4 — A Criatura das Eras (Verão, Dia 1–81, passo 20)
1. Atravesse o andaime no Dia 1 (ele apodrece depois).
2. Veja o **Dia 41**: a cronofera anciã, pesada, **quebra a ponte**.
3. No **Dia 61** as cronoferas morreram de velhice e o **ninho** ficou vazio: pegue a **chave**.
4. Desça ao fosso: o portão está **congelado no inverno** → **volte** ao Dia 41 (outono) e destranque.
Extras: a cronofera da ponte cai no fosso junto com você no Dia 41, e há um lodo perto da saída.

### Fase 5 — O Bater de Asas (Primavera, Dia 1–81, passo 10)
```
Libertar o pássaro (com a caixa) → +10: bando cresce → +10: comem a trepadeira → o rio volta a correr
Plantar a semente → semente +10 dias e rio correndo: árvore inclinada → +10: cai e vira PONTE (Dia 31)
```
1. No Dia 31+ pegue a **pinha** que caiu do tronco-ponte e atravesse.
2. A saída (a colmeia dourada) fica num penhasco. A pinha só brota **na primavera** — mas na primavera ainda não existe ponte.
3. **Parado na outra margem**, volte ao Dia 21, plante, e avance ao Dia 41.
Extras: mariposas no verão, espinheiro no penhasco final.

### Fase 5 — miniboss: Rainha Vespa
Depois do penhasco, uma arena. A Rainha muda com a estação:
- **Primavera**: jovem e veloz (investidas), **vulnerável**.
- **Verão**: cospe ferrões em leque, **invulnerável** (só fica vulnerável atordoada após a investida). Mude de dia para apagar os ferrões.
- **Outono**: velha e cansada: mergulha no chão, solta ondas de pedra e fica **atordoada**.
O dano causado em qualquer dia continua valendo nos dias seguintes. Derrotada, o portão da colmeia abre.

### Fase 6 — O Futuro Devorado (começa no INVERNO)
Uma árvore caída bloqueia a trilha. Ela caiu porque um **roedor** a roeu no verão. Volte, afaste o roedor (ou apenas atravesse a posição da árvore numa época em que ela está em pé) e avance ao inverno para cruzar o rio congelado. No inverno o monte de neve tem degraus, mas a nevasca sopra contra você. A saída fica no alto de um paredão, e a **trepadeira** só existe na primavera e no verão: chegue ao pé do paredão (pela neve no inverno ou pelos espinheiros com **C**) e volte à primavera para escalar.

### Fase 7 — O Nível da Água (começa no OUTONO)
A semente está numa ilha no lago: chegue pelo **gelo (inverno)** ou pelas **vitórias-régias (verão)**. A terra do penhasco só aceita sementes **na primavera** e a árvore leva **60 dias**.

### Fase 8 — Estações em Sequência (começa no OUTONO)
Cada trecho só é seguro numa estação: deslizamento de pedras (outono), rio (só congelado), nevasca (inverno), espinhos floridos (verão). Vá e volte várias vezes. No fim, um **abismo largo** só é cruzado no **outono**, com o vento soprando a favor.

### Fase 9 — A Porta do Passado (começa no INVERNO)
A porta abre se a caixa estiver na placa **no Dia 1**. Na primavera, trepadeiras fecham a área da caixa. Entre no inverno (trepadeira seca), **volte à primavera já lá dentro**, empurre a caixa para a placa e avance.

### Fase 10 — Chefe final: o Cronófago (começa no INVERNO)
- **Primavera**: cria veloz — investidas e rajadas curtas, **vulnerável**.
- **Verão**: rajadas em leque e invocação de lodos, **vulnerável**.
- **Outono**: blindado; cada **pancada** o deixa atordoado e vulnerável.
- **Inverno**: armadura de gelo, **invencível** (os lodos invocados nascem congelados e viram plataformas).
Estratégia: fira-o nas estações vulneráveis — o dano vale para os dias seguintes — e troque de dia para **apagar ataques**. **C** congela o chefe e os projéteis. Derrotado, o portão do fim do tempo abre.

---

## 8. Limitações

- Sem áudio; arte placeholder; fonte padrão do Unity.
- Morrer reinicia a fase inteira.
- Inimigos voltam ao ponto inicial a cada troca de dia.
- O botão de regenerar cenas sobrescreve edições manuais.
- Fora de foco, o Unity pausa o Play Mode.

## 9. Próximos passos

1. Importar a arte escolhida e trocar os sprites dos filhos `Visual` (seção 6.6).
2. Sons (salto temporal, estações, criaturas).
3. Checkpoints, tela de pausa, pós-processamento por estação.

# Créditos de arte (terceiros)

| Pasta | Pacote | Autor | Link | Licença |
|---|---|---|---|---|
| `LegacyFantasy/` | Legacy Fantasy – High Forest 2.3 (herói, javali, caracol, abelha, céu, florestas, tiles) | Anokolisa | https://anokolisa.itch.io/sidescroller-pixelart-sprites-asset-pack-forest-16x16 | Grátis, uso pessoal e comercial permitido pelo autor. Não revender/redistribuir os arquivos. |
| `CraftpixSlime/` | Free Slime Mobs Pixel Art (Slime1) | Craftpix (Free Game Assets) | https://free-game-assets.itch.io/free-slime-mobs-pixel-art-top-down-sprite-pack | Craftpix Free License: uso comercial ok, crédito opcional, **proibido redistribuir os arquivos brutos**. https://craftpix.net/file-licenses/ |
| `CraftpixPlant/` | Free Pixel Predator Plant Mob Sprites (Plant2) | Craftpix (Free Game Assets) | https://free-game-assets.itch.io/free-predator-plant-mobs-pixel-art-pack | Craftpix Free License (idem acima). |

## Por que os PNGs não vão para o GitHub

O repositório é público e as licenças não permitem redistribuir os arquivos brutos.
O `.gitignore` ignora os `.png` desta pasta, mas mantém os `.meta` — assim as referências dos prefabs e cenas continuam válidas.

**Opção recomendada:** deixar o repositório privado e remover a linha
`/Gamejam_borboleta/Assets/_Game/Art/ThirdParty/**/*.png` do `.gitignore`.

**Se o repositório continuar público:** cada integrante baixa os 3 pacotes pelos links acima e copia os PNGs
para os mesmos caminhos desta pasta (mesmos nomes de arquivo). Os `.meta` versionados cuidam do resto.

Mapeamento dos arquivos Craftpix: `PNG/Slime1/<Ação>/Slime1_<Ação>_body.png` → `CraftpixSlime/Slime_<Ação>.png`
e `PNG/Plant2/<Ação>/Plant2_<Ação>_body.png` → `CraftpixPlant/Plant_<Ação>.png`.
No Legacy Fantasy, espaços viraram `_` e a pasta `Jumlp-All` virou `Jump-All`.

## Arte livre (pode ir para o GitHub)

| Pasta | Recurso | Autor | Link | Licença |
|---|---|---|---|---|
| `Assets/_Game/Art/CC0/Bird/` | Bird asset (pássaros da fase 5) | OpenGameArt | https://opengameart.org/content/bird-asset | CC0 |
| `Assets/_Game/Art/CC0/FX/Slash.png` | Pixel art sword slash effect (golpe do Eco) | OpenGameArt | https://opengameart.org/content/pixel-art-sword-slash-effect | CC0 |
| `Assets/_Game/Art/CC0/FX/HitRing, Sparkle, Bubbles, Burst, Soul, Spore, TimeWave` | Free Pixel Effects Pack (impacto, brilho, coleta, morte, esporo do espinheiro e onda do chefe) | CodeManu / Davit Masia | https://opengameart.org/content/free-pixel-effects-pack | CC0 |
| `Assets/_Game/Art/CC0/FX/TimeClock, TimeSwirl` | Cosmic Time - Magic Effect (relógio que apaga a criatura da linha do tempo) | OpenGameArt | https://opengameart.org/content/cosmic-time-magic-effect | CC0 |
| `Assets/_Game/Art/CC0/FX/Butterflies.png` | Butterflies (9 borboletas em pixel art: pólen e almas das criaturas) | Ivan Voirol | https://opengameart.org/content/butterflies | CC0 (também CC-BY 3.0 / GPL) |
| `Assets/_Game/Art/ThirdParty/Derived/` | Arbustos seco e nevado, gerados pelo Construir Projeto a partir do arbusto do Legacy Fantasy (mesma licença do pacote, também fora do Git) | — | — | Legacy Fantasy |
| `Assets/_Game/Art/Fonts/` | Pixelify Sans (fonte da UI) | The Pixelify Sans Project Authors | https://fonts.google.com/specimen/Pixelify+Sans | SIL Open Font License 1.1 (`OFL.txt`) |

## O que vem de cada imagem do Legacy Fantasy

- `Assets/Tiles.png`: grama, terra, tijolos, tábuas, galhos, água, cogumelos, brotos, flores e juncos
- `Assets/Interior-01.png`: caixa, porta, grade (gaiola) e placa
- `Assets/Props-Rocks.png` e `Assets/Tree-Assets.png`: pedras e arbusto (trepadeira)
- `Trees/Green|Golden|Red-Tree.png`: pinheiros das estações (primavera, verão, outono; tronco pelado no inverno)
- `HUD/Base-01.png`: painéis de madeira e pergaminho, botão e corações

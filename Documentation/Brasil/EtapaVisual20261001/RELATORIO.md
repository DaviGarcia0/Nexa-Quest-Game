# Nova Quest e HUD — 01/10/2026

Integração concluída na Unity 2022.3.62f3, na pasta `C:\Users\Davi\Desktop\Nexa-Quest-Game`. Checkpoints: `66f49c0` e `ce3060d` (este último inclui a cena salva pelo usuário antes da integração).

## Resultado e arquivos

- **Brasil.unity:** montagem visual de `Canvas/HUD`. Objetos atuais de texto e barra foram reutilizados dentro de `CoinsHUD` e `KnowledgeHUD`, com duas novas imagens de fundo. Anchors no canto superior direito, referência 1920×1080 e proporções originais dos PNGs.
- **Prefabs/Brasil/Quest.prefab:** sprite inicial do SpriteRenderer substituído. Nenhuma mudança de movimento, escala do Transform, velocidade ou Collider2D. A Unity também explicitou a referência nula já existente de `areaBounds` ao salvar.
- **Animations/Quest:** os oito clips `IdleDown`, `IdleLeft`, `IdleRight`, `IdleUp`, `WalkDown`, `WalkLeft`, `WalkRight`, `WalkUp` tiveram somente as referências dos frames trocadas. Mesmos GUIDs, tempos, loops e Animator Controller. Parâmetros `Direction`, `Speed`, `WalkRate` e `IdleRate` preservados.
- **Scripts/Brasil/ProgressionHUD.cs:** único script de gameplay alterado; três linhas de formatação. Continua no GameObject `Canvas/HUD`.
- **Sprites/Quest/Design20261001:** novos `Quest_Walk.png`, `Quest_Idle.png`, dois JSONs de recorte e respectivos `.meta`.
- **Sprites/HUD/HUD_CEREBRO.png.meta e HUD_NEXACOIN.png.meta:** importação ajustada. Os PNGs já estavam no projeto e eram idênticos aos enviados; foram reutilizados.
- **Editor/QuestHUDVisualSetup.cs e QuestHUDVisualValidation.cs:** preparação e testes, executados somente no Editor; não são componentes de gameplay nem entram no build.

Os caminhos acima estão dentro de `Assets/Projeto`. Evidências e capturas ficam nesta pasta do relatório. Os PNGs enviados não foram redesenhados, deformados ou alterados; a comparação dos arquivos confirmou conteúdo idêntico.

## Quest

32 frames Walk e 16 Idle, com recortes individuais e pivôs referenciados ao centro dos pés. A segunda linha do Idle olha para a direita e a terceira para a esquerda; o mapeamento respeita isso. No Walk, esquerda e direita estão na ordem oposta.

PPU local: Walk **540**, Idle **640**, mantendo altura visual próxima de **0,47 unidade**, compatível com a Quest anterior. Point, sem compressão/mipmaps e sem redução da resolução. Nenhum ajuste global de PPU, câmera ou configurações do projeto.

O PNG Walk possui resíduos de transparência muito baixa no fundo; foram preservados, sem regenerar a arte. Nas capturas do jogo não aparece um retângulo opaco ou preto. O retrato da Quest nos diálogos foi mantido, conforme o escopo de trocar somente o player.

## HUD e progressão

- `AddCoins(valor)` continua atualizando `ProgressionManager.Coins`. O evento `Changed` atualiza `CoinsText`, exibindo apenas o número.
- `AddXP(valor)` continua atualizando Conhecimento. O preenchimento usa `CurrentXP / XPToNextLevel` e cresce da esquerda para a direita.
- A regra existente é **100 XP por nível**, com excedente preservado. `LevelText`, abaixo do cérebro, exibe apenas `ProgressionManager.Level` e atualiza no mesmo evento.
- Label fixo `CONHECIMENTO`; textos em TextMeshPro usando a fonte existente LiberationSans SDF. Não havia fonte pixel art própria configurada.
- Para reposicionar ou redimensionar a HUD, use os RectTransforms de `Canvas/HUD`, `CoinsHUD` e `KnowledgeHUD`, mantendo o aspect ratio das imagens.

Nenhuma nova variável de Coins, XP ou Level foi criada. Os ganhos usados nos testes foram temporários; o estado salvo continua em **0 moedas, 0 XP e nível 1**.

## Testes e preservação

Antes: **201 verificações aprovadas** (90 NPC/diálogo/HUD + 111 exploração/Portal).

Depois: as mesmas **201 verificações aprovadas**, mais **102 verificações visuais e de integração**. Total final: **303 aprovadas, 0 falhas**. Console final: **0 erros e 0 avisos**, em 01/10/2026 às 20:40:08.

Testado em Play Mode real: quatro direções de Idle e Walk, última direção ao parar, diagonal normalizada, frames e escala, collider dos pés, câmera, seis passagens, fade, diálogos e interação, Coins, barra em 0/25/50%, level up e excedente. A Game View e as capturas foram confirmadas em **1920×1080**. Movimento e interação foram acionados pelas mesmas rotinas chamadas pelas teclas, com física real; não houve injeção de teclado do Windows.

Comparação com o checkpoint: **157 colliders serializados na cena, seus GameObjects e Transforms intactos**. Também preservados mapas, NPCs, dados/retratos de diálogo, Menu, MapaMundi, MinhaQuest, Animator Controller, PlayerMovement, BrazilWorld, DialogueController, ProgressionManager, Packages e ProjectSettings. Detalhes em `preservacao.txt`.

Uma rodada intermediária foi interrompida por recarga de scripts da Unity; após sair de Play Mode e concluir a importação, a execução final passou integralmente. A cena ficou salva e fora de Play Mode, sem solicitações automáticas pendentes.

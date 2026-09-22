# NPCs da Feira, diálogo com retratos e HUD

## Conteúdo

Foram preparados seis NPCs somente para `Maps/Feira/NPCs`:

| Objeto | Função / arte | Posição de referência no PNG (pés; origem no canto superior esquerdo) |
|---|---|---|
| NPC_Introducao_Lia | Interativa; NPC 1.png; marca vermelha 1 | 1450, 700 |
| NPC_Desafios_Bento | Interativo; NPC 2.png; marca vermelha 2 | 900, 735 |
| NPC_Decorativo_Feirante | Homem de chapéu de palha; roupa clara e calça terracota; frente | 630, 461 |
| NPC_Decorativo_Moradora | Mulher de cabelo crespo e lenço, blusa turquesa e saia amarela; esquerda | 1036, 385 |
| NPC_Decorativo_Torcedor | Rapaz de camisa azul e bermuda; direita, no campo | 1625, 752 |
| NPC_Decorativo_Senhora | Senhora de cabelo grisalho e vestido floral; frente | 1212, 174 |

Os quatro decorativos originais correspondem a quatro das marcações azuis. Na retomada, a cena já continha também NPC_Decorativo_Torcedor (1), que foi preservado: o total atual é sete NPCs, dois interativos e cinco decorativos. Repetições decorativas são permitidas pelo pedido. Os nomes Lia e Bento são editáveis nos dados de diálogo.

## Arte, escala e animação

Os dois PNGs fornecidos foram copiados sem alteração de pixels. Foram inspecionados e recortados pelo importador, pois dividir 1254 × 1254 em quatro colunas iguais cortaria personagens. Os limites das colunas seguem os espaços reais entre as figuras. Cada direção usa quatro frames e dimensões coerentes dentro de sua sequência, com referência nos pés. Os metadados de recorte ficam nos JSONs junto dos PNGs.

`DecorativosBrasil.png` é uma nova arte gerada com image_gen, usando os dois NPCs como referências de estilo. O resultado real tem 1254 × 1254, quatro personagens e quatro poses Idle de cada um. As roupas e aparências são variações brasileiras cotidianas. Não são placeholders geométricos; continuam editáveis/substituíveis como qualquer asset de arte gerada.

Prompt usado: sprite sheet transparente de quatro personagens brasileiros cotidianos em quatro frames Idle por personagem, no estilo chibi pixel art das referências; feirante de chapéu de palha, mulher com lenço e roupa turquesa/amarela, rapaz de camisa azul e bermuda e senhora grisalha de vestido floral; sem textos, logos ou cenário, corpos completos, pés alinhados, poses discretas e piscar.

Todos os novos sprites usam Point, sem compressão/mipmaps, Max Size 4096 e PPU 550. Os prefabs foram criados com escala 1; os ajustes de escala feitos na cena foram preservados (incluindo o Torcedor menor no campo). O PPU acompanha o atlas existente da Quest: ela tem cerca de 0,46 unidade de altura visível; os NPCs ficam próximos de 0,5 unidade. Nenhum mapa foi alterado.

Cada NPC tem um Animator com um único Idle em loop, quatro frames com pausas nas poses neutras. Edite o clip correspondente em `Assets/Projeto/Animations/NPCs` para mudar os tempos. Alterar somente o SpriteRenderer não substitui uma animação: para trocar arte definitiva, atualize o clip/controller; ou retire o Animator para usar um sprite estático.

Os retratos são recortes dos pixels dos personagens fornecidos e da Quest existente. `QuestPortraitSource.png` é uma cópia integral do atlas Idle da Quest, importada apenas com o recorte do retrato, para não modificar os subassets usados nas animações da personagem.

## Diálogos

Um único `DialogueController` no GameManager atende todos os NPCs. `NPCInteraction` só registra proximidade por CircleCollider2D trigger, sem colisão sólida. Os decorativos não têm Collider2D nem script de interação.

`Canvas/DialoguePanel` fica na parte inferior da tela e contém `Portrait`, `SpeakerName`, `DialogueText` e `ContinueIndicator`, além das molduras. Usa TextMeshPro já existente no projeto. Começa fechado.

E perto de um NPC abre a conversa. Cada E avança uma linha. Na última, encerra. O mesmo pressionamento não abre e avança duas vezes no mesmo frame. O painel muda automaticamente retrato, nome e texto. Durante a conversa, `PlayerMovement.SetControlsLocked(true)` bloqueia a Quest e BrazilWorld recusa novas transições. No encerramento o controle é devolvido. Desativar o NPC/área ou o controlador também fecha a conversa.

O `InteractionPrompt` existente foi reutilizado; o Portal mantém seu comportamento. Não foi criado um prompt por NPC.

Edite as falas nos assets:
- `Assets/Projeto/Dialogue/Feira_BoasVindas.asset`: dez falas solicitadas, mais uma resposta curta da Quest (11 linhas).
- `Assets/Projeto/Dialogue/Feira_Desafios.asset`: sete falas solicitadas, mais uma resposta curta da Quest (8 linhas).

Cada elemento de `Lines` possui `Speaker` (NPC/Quest/Other), `Speaker Name`, `Portrait` e `Text`. Nome e retrato são explícitos por linha: ao escolher Quest, preencha também o nome e arraste seu retrato. Isso permite futuramente outros personagens sem criar scripts.

Para outro NPC, use um prefab em `Assets/Projeto/Prefabs/NPCs`, coloque-o dentro da área desejada, atribua o `DialogueController` da cena e crie um asset em **Create > Nexa Quest > Dialogue**. Ajuste a posição, o trigger e o campo Dialogue. Os NPCs desta tarefa estão apenas na Feira.

`On Dialogue Finished`, no NPCInteraction, é um UnityEvent vazio nos dois NPCs. No futuro poderá abrir uma tela de escolha entre Quiz e Minigame pelo Inspector. Nenhuma ação, recompensa, quiz, chave ou ativação do Portal foi conectada agora. O evento roda somente ao chegar ao final da conversa, não quando ela é cancelada por desativação.

## HUD e progressão

`Canvas/HUD`, no canto superior direito, contém Coins, BrainLevel, Knowledge e KnowledgeBar/Fill. Os textos usam os dados do `ProgressionManager`, nunca contadores isolados no texto.

Estado inicial: Coins = 0, CurrentXP = 0, Level = 1; XP por nível = 100. O limiar é configurável no Inspector. O sistema usa um custo constante por nível nesta etapa. Ao receber XP suficiente, sobe quantos níveis forem necessários e guarda o excedente na barra. Valores negativos são ignorados.

O ProgressionManager é um objeto raiz único com DontDestroyOnLoad. Permanece entre áreas e cenas durante a sessão, sem gravar save em disco. MinhaQuest poderá ler `ProgressionManager.Instance` quando esse sistema for integrado naquela cena; a cena MinhaQuest não foi modificada.

Chamadas futuras:

```csharp
ProgressionManager.Instance.AddCoins(50);
ProgressionManager.Instance.AddXP(25);
```

Nenhum gameplay chama esses métodos nesta etapa. A validação os exercita temporariamente em Play Mode; isso não altera os valores iniciais salvos na cena.

## Scripts e arquivos

Criados em `Assets/Projeto/Scripts/Brasil`:
- DialogueData.cs: asset de conversa e definição de uma linha.
- NPCInteraction.cs: proximidade e evento final, nos dois NPCs interativos.
- DialogueController.cs: painel e avanço, no GameManager.
- ProgressionManager.cs: dados centrais, no objeto raiz com o mesmo nome.
- ProgressionHUD.cs: atualização visual por evento, em Canvas/HUD.

Ferramentas do Editor: FeiraNPCSetup.cs e FeiraNPCValidation.cs. Não entram no build. BrasilEditorJob.cs recebe os comandos locais de configuração e teste. BrazilWorld.cs ganha a recusa de viagens enquanto os controles estiverem bloqueados.

Assets novos: três sheets de NPCs, cópia do atlas para retrato da Quest, JSONs de recorte, seis controllers/clips Idle, seis prefabs, dois assets de diálogo e respectivos metadados. A cena Brasil recebe os NPCs, o painel, o HUD e as referências.

## Validação

Resultados da execução ficam em `playmode.txt`; capturas mostram falas de ambos NPCs e da Quest. A regressão das passagens e do Portal usa o teste existente e grava em `Documentation/Brasil/EtapaPortal/playmode.txt` e `console.txt`.

Os testes de interação chamam o mesmo método utilizado pela tecla E, após entrada real da Quest nos triggers. Isso valida a lógica, mas não representa injeção de teclas físicas. A câmera e a física são executadas em Play Mode real.

## Ajuste de chegada na Feira

A regressão identificou que `Maps/Feira/SpawnPoints/EntradaPortal`, em (6,30; 3,20), coincidia com `colisão (23)`. Apenas esse Transform de chegada foi movido para (6,45; 2,55), um trecho livre da rua, verificado por sobreposição física. Nenhum collider foi movido ou editado.


## Resultado final — 22/09/2026

- NPCs/diálogos/HUD: **90 verificações aprovadas, 0 falhas**.
- Movimento, passagens e Portal: **111 verificações aprovadas, 0 falhas**.
- Total: **201 verificações aprovadas** em Play Mode.
- Console final: **0 erros, 0 avisos, 0 mensagens**.
- Os **84 colliders preexistentes**, seus GameObjects e Transforms permaneceram idênticos ao registro anterior à implementação.
- Menu, MapaMundi, MinhaQuest, PNGs dos mapas e ProjectSettings permaneceram intactos.
- A cena ficou fora de Play Mode, com valores iniciais salvos em 0 moedas, 0 XP e nível 1.

Para repetir a verificação completa no Editor: **Nexa Quest > Testar NPCs e regressao Brasil**. Os ajustes de escala e o Torcedor duplicado encontrados na cena durante o trabalho foram preservados.

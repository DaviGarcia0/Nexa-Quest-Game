# Desafios da Feira — diálogo e seleção de atividade

Checkpoint anterior às alterações: `113b2da`. A validação anterior passou sem falhas e sem erros/avisos. Inclui a caminhada lateral e os ajustes manuais recentes da HUD.

## Integração

NPC: `Maps/Feira/NPCs/NPC_Desafios_Bento`.
Diálogo alterado: `Assets/Projeto/Dialogue/Feira_Desafios.asset`.
O evento persistente `NPCInteraction → On Dialogue Finished` da instância na cena chama `PainelDesafiosFeira.Abrir`. O prefab e o DialogueController permanecem inalterados.

Texto final:

1. **Bento:** Ei, Quest! Preparada para os desafios da Feira Brasileira?
2. **Quest:** Claro! O que eu preciso fazer?
3. **Bento:** Aqui temos dois desafios: um Quiz e um Minijogo.
4. **Bento:** Complete os dois e eu entrego a Chave da Feira.
5. **Quest:** Uma das chaves do Portal?
6. **Bento:** Isso! Você vai precisar das três para ativá-lo.
7. **Bento:** Escolha por qual desafio quer começar!

Os retratos existentes foram mantidos, inclusive a expressão da Quest. Lia não foi alterada.

## Objetos e dimensões

Criados:

- `Canvas/PainelDesafiosFeira`: inicialmente desativado, ocupa a tela; fundo preto com alfa 0,55 intercepta cliques fora dos botões. Componente `PainelDesafiosFeira`.
- `Canvas/PainelDesafiosFeira/ArtePainel`: imagem original sem edição, centralizada, 1280 × 960 na referência 1920 × 1080; posição ancorada (0, 0). Proporção 4:3 preservada.
- `ArtePainel/BotaoQuiz`: botão transparente, sem texto adicional.
- `ArtePainel/BotaoMinijogo`: botão transparente, sem texto adicional.
- `SistemaEventos`: EventSystem e StandaloneInputModule, compatíveis com o Input tradicional existente.

O Canvas existente recebeu GraphicRaycaster. O painel fica acima das HUDs e abaixo do FadePanel. Nenhum Canvas foi recriado.

Áreas de clique medidas em pixels do PNG 1448 × 1086, origem no canto superior esquerdo:

| Botão | Retângulo no PNG (x, y, largura, altura) | Posição ancorada na arte | Tamanho na UI |
|---|---|---|---|
| BotaoQuiz | (250, 570, 430, 172) | (-228,9503; -99,8895) | 380,1105 × 152,0442 |
| BotaoMinijogo | (768, 570, 432, 172) | (229,8343; -99,8895) | 381,8785 × 152,0442 |

Os retângulos ficam dentro das superfícies coloridas das opções, evitando molduras e cantos arredondados. Sem sobreposição nem texto duplicado. Hover não altera a arte.

## Comportamento

`SelecionarQuiz` e `SelecionarMinijogo` registram a escolha no Console. O painel continua aberto; nenhuma atividade completa, chave, XP, moeda ou consumo de energia foi implementado.

A abertura usa `PlayerMovement.SetControlsLocked(true)`. O bloqueio já existente impede movimento, novos diálogos e `BrazilWorld.TravelTo`. Esc chama `Fechar`, desativa o painel e libera seu bloqueio. Desativar o objeto também limpa o bloqueio que pertence ao painel. O painel não abre se outro sistema já bloqueou a Quest.

Para conectar as atividades futuramente, os métodos `SelecionarQuiz` e `SelecionarMinijogo` são os pontos de entrada. A regra da chave será implementada posteriormente, somente após ambas serem concluídas.

## Arquivos

Criados (com os respectivos .meta):

- `Assets/Projeto/Scripts/Brasil/PainelDesafiosFeira.cs`: único script novo de gameplay.
- `Assets/Projeto/Editor/ConfiguracaoDesafiosFeira.cs`: montagem e validação de Editor; não participa do build.
- `Assets/Projeto/Sprites/Brasil/Interface/PainelDesafiosFeira.png`: cópia byte a byte do PNG enviado; Point, sem compressão/mipmap, Max Size 2048, Sprite Single. Pasta Interface também possui .meta.
- `Documentation/Brasil/DesafiosFeira20261003/`: evidências e este relatório.

Alterados:

- `Assets/Projeto/Scenes/Brasil.unity`: objetos novos, GraphicRaycaster no Canvas e evento da instância Bento.
- `Assets/Projeto/Dialogue/Feira_Desafios.asset`: sete falas.
- `Assets/Projeto/Editor/EnergyHUDValidation.cs` e `FeiraNPCValidation.cs`: passaram a verificar a abertura do modal após Bento e fechá-lo antes de verificar a devolução do controle. Validações anteriores mantidas.

## Preservação

A auditoria da cena registrou 157 componentes de colisão/física idênticos ao checkpoint. Nenhum bloco existente foi removido. Entre os blocos existentes, somente Canvas, sua lista de filhos, o evento da instância Bento e a lista de raízes foram alterados. HUDs, mapas, câmera, movimentação, transições, progresso e colisões manuais permanecem iguais. A imagem importada tem os mesmos bytes do original.

## Validação

Resultados detalhados: `antes/`, `painel-playmode.txt` e `depois/`. Imagem da interface: `Painel-1920x1080.png`.

Os testes exercitam os mesmos métodos usados por E e Esc. Os cliques passam por EventSystem.RaycastAll e pelos eventos de ponteiro dos Buttons, incluindo centro, cantos internos e pontos fora das bordas. Não se trata de teste com teclado/mouse físicos.

Resultado final em Play Mode: nenhuma falha.

painel-playmode.txt: 63 verificações aprovadas

energia.txt: 67 verificações aprovadas

visual.txt: 100 verificações aprovadas

npcs.txt: 88 verificações aprovadas

portal.txt: 111 verificações aprovadas

Console final: 0 erros, 0 avisos. Os dois registros de seleção são mensagens esperadas. Prévia visual conferida em 1920 × 1080, sem corte nem distorção.

Durante a preparação, o carregamento do componente temporário de teste precisou ser corrigido para seguir o padrão dos validadores existentes. A execução final completa passou; nenhum sistema de gameplay precisou ser refeito. O executor permite execução em segundo plano durante Play Mode, sem modificar ProjectSettings.

Os validadores existentes também atualizaram relatórios e capturas em EtapaEnergia20261002, EtapaVisual20261001, EtapaNPCs e EtapaPortal. Essas evidências foram mantidas: a revisão automática rejeitou a restauração ampla dessas pastas por risco de descartar alterações não commitadas.


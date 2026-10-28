# Minha Quest — entrega de 27/10/2026

Implementação na cena existente `Assets/Projeto/Scenes/MinhaQuest.unity`, Unity 2022.3.62f3, projeto real `C:/Users/Davi/Desktop/Nexa-Quest-Game`.

Checkpoint desta etapa: **d7249df**, posterior ao commit do usuário `3065448` com colisões no Ponto Turístico. A cena aberta estava salva; compilação sem erros. O aviso encontrado antes da implementação vinha do vídeo do Menu. O Console ao final desta validação ficou com **0 erros e 0 avisos**.

## 1. Arquivos criados

Os caminhos abaixo começam em `Assets/Projeto/`:

- `Scripts/MinhaQuest/MinhaQuestController.cs`: tela, Mercado, Vestuário, pop-ups, HUD e atualização do estoque visível.
- `Scripts/MinhaQuest/FoodType.cs`: os três alimentos, valores oficiais e resultados da tentativa de alimentação.
- `Scripts/MinhaQuest/FoodDragHandler.cs`: eventos de ponteiro e arraste.
- `Scripts/MinhaQuest/QuestFeedingController.cs`: detecção da boca, aproximação e reprodução das animações.
- `Scripts/MinhaQuest/MinhaQuestNavigation.cs`: origem e destino de retorno.
- `Editor/MinhaQuestBridge.cs`, `MinhaQuestSetup.cs` e `MinhaQuestValidation.cs`: inspeção/montagem e testes direcionados na Unity aberta.
- `Animations/MinhaQuest/`: quatro clips (`TorsoIdle`, `MouthOpen`, `Eating`, `CorpoIdle`) e dois Animator Controllers (`Torso`, `Corpo`).
- `Sprites/MinhaQuest20261027/`: os 19 PNGs fornecidos, com seus importadores e slicing. Não houve edição de pixels, recoloração ou geração de arte. A comparação SHA-256 está em `auditoria.json`.
- `Documentation/MinhaQuest20261027/`: relatório, inventário de arquivos, resultados e dez capturas em 1920×1080.

A lista completa, incluindo `.meta`, está em `arquivos.txt`.

## 2. Arquivos existentes modificados

- `Scenes/MinhaQuest.unity`: montagem da interface na cena que estava vazia.
- `Scripts/Brasil/ProgressionManager.cs`: três contadores de alimento adicionados ao estado e ao mesmo save v1; compra e alimentação como operações únicas, com uma gravação e uma notificação cada. Removido o ajuste temporário que substituía a Energia salva por 100 ao iniciar, conforme a própria orientação anterior do campo para desativá-lo quando a recuperação estivesse pronta.
- `Scripts/SceneLoader.cs`: registra a origem ao carregar MinhaQuest pelo Menu.
- `Scripts/Global/PauseSettingsManager.cs`: registra origem na entrada pelas Configurações e respeita a prioridade de Esc dos painéis/arraste da Minha Quest.

As cenas Brasil, Menu e MapaMundi, as colisões do usuário, PlayerMovement, BrazilWorld, perguntas e regras dos quizzes/minijogos permaneceram intactos. Evidência: `auditoria.json`.

## 3. Objetos e Canvas

Na cena MinhaQuest foi adicionado `MinhaQuestCanvas`, com Canvas Scaler de referência 1920×1080, Screen Space Overlay, GraphicRaycaster e escala proporcional. O EventSystem global existente é reutilizado; não há segundo EventSystem na cena.

A hierarquia contém:

- `TelaPrincipal`: cenário aprovado, Quest torso, zona invisível da boca, três slots com um alimento por tipo, quatro botões fornecidos e HUD de Energia.
- `DragLayer`: coloca o alimento arrastado acima da tela principal.
- `MarketLayer`: overlay semitransparente, arte do Mercado, `ButtonApple`, `ButtonSandwich`, `ButtonNexaKing`, X transparente e saldo dinâmico de NexaCoins em área livre, sem sobrepor nomes/preços dos cards.
- `WardrobeLayer`: overlay, arte de Vestuário, Quest de corpo inteiro e X transparente. A grade permanece inteiramente vazia.
- `PopupLayer`: overlay e imagem do pop-up atual, com apenas duas hitboxes transparentes, OK e X, reposicionadas de acordo com os botões desenhados em cada PNG.

Mercado/Vestuário bloqueiam a tela principal; pop-up bloqueia também o Mercado. As Configurações globais mantêm sua ordenação superior e sua pausa existente. Sprites importados sem mipmaps, com filtro Point, sem compressão e com proporção preservada.

## 4. Inventário e preços

O inventário está dentro do **ProgressionManager existente**, nos campos `apples`, `sandwiches`, `nexaKings`. Não foi criado FoodInventory/SaveManager/GameManager paralelo. Os campos são gravados no mesmo JSON `NexaQuest.Progresso.v1` que já contém moedas, Energia, EXP, nível e flags.

| Alimento | Preço | Recuperação |
|---|---:|---:|
| Maçã / Apple | 5 NexaCoins | +10 Energia |
| Sanduíche / Sandwich | 15 NexaCoins | +25 Energia |
| Nexa King | 40 NexaCoins | +50 Energia |

`TryBuyFood` valida tipo, saldo e limite do contador, desconta moedas e acrescenta uma unidade antes de salvar/notificar. Se falhar, nada muda. Saves v1 anteriores, sem os campos de alimento, carregam os três estoques como zero, preservando os demais valores. Valores negativos de estoque carregados são limitados a zero.

São somente três objetos de comida na bancada. A quantidade é interna. Cada sprite fica ativo enquanto o contador correspondente for maior que zero; não são criadas cópias por unidade.

## 5. Arraste e alimentação

`FoodDragHandler` implementa `IPointerDownHandler`, `IBeginDragHandler`, `IDragHandler` e `IEndDragHandler` do EventSystem. A imagem original sai temporariamente do slot e acompanha o ponteiro na camada de arraste. Não há clone, mão, seta, carrossel ou seletor adicional.

A aproximação é medida contra uma zona invisível filha da Quest. Entrar perto reproduz MouthOpen; afastar retorna ao Idle. Apenas soltar dentro da zona menor faz a tentativa de alimentação. Soltar fora devolve o mesmo objeto ao mesmo slot sem alterar save, estoque ou Energia.

Na entrega válida, a transação consome uma unidade e aplica `min(energia + recuperação, 100)`. A atualização é imediata no início da animação de comer, junto com o save, para não perder a recuperação se houver uma troca de cena ou encerramento durante a animação. A comida arrastada deixa a boca, reaparece no slot se ainda houver estoque e a Quest fica ocupada até terminar de comer. Outra alimentação fica bloqueada nesse intervalo.

Com Energia máxima, o item não é consumido e aparece o PNG Energia Cheia. Abrir Configurações durante o arraste cancela o arraste com segurança; durante alimentação, a animação e seu tempo param até retomar.

## 6. Animações

Foram usados exclusivamente os sprite sheets aprovados:

- TorsoIdle: 6 frames, 7 fps, loop.
- MouthOpen: 6 frames, 10 fps, sem loop, mantendo a pose final enquanto a comida está próxima.
- Eating: 16 frames em duas linhas, 10 fps, sem loop, duração de 1,6 segundo; depois retorna ao Idle.
- CorpoIdle: 4 frames, 5 fps, loop no Vestuário.

O slicing foi feito no importador da Unity, com retângulos de frame e margens transparentes ajustadas, sem regravar as imagens. Os Animation Clips animam a propriedade Sprite do componente Image. O controlador do torso possui estados Idle, MouthOpen e Eating; o do corpo possui Idle. A troca é acionada pelo controle de alimentação, sem estados de roupa ou personagens alternativos.

## 7. Energia, NexaCoins e save

O máximo já utilizado pelo jogo era 100; foi explicitado como `ProgressionManager.EnergiaMaxima`. Energia e moedas continuam sendo os mesmos valores globais, com o mesmo evento Changed. Os HUDs novos assinam esse evento; os HUDs existentes do Brasil recebem o estado atualizado quando a cena é carregada.

O HUD de Energia usa o PNG fornecido com texto dinâmico `atual / 100`. O preenchimento cresce da esquerda para a direita através do tamanho do RectTransform, dentro da moldura aprovada. Não há barra de fome/felicidade ou nova moeda.

O singleton existente de progresso é criado ao abrir MinhaQuest pelo Menu caso ainda não exista; ao voltar para o Brasil, a cópia da cena é descartada pelo mecanismo existente. Saves e estoque também foram recarregados em uma segunda entrada completa em Play Mode, comprovando persistência além da troca de cenas.

## 8. Retorno à origem

`MinhaQuestNavigation` registra o nome real da cena antes da entrada pelo SceneLoader e pelas Configurações. Também observa `activeSceneChanged` para cobrir entradas futuras feitas diretamente com SceneManager. Entradas originadas da própria MinhaQuest não substituem a origem válida.

Voltar carrega a origem registrada, com bloqueio de navegação duplicada. Foram verificados Menu → MinhaQuest → Menu, MapaMundi → Configurações → MinhaQuest → MapaMundi e Brasil → Configurações → MinhaQuest → Brasil. Uma abertura direta, sem contexto, usa Menu como fallback. Essa volta recarrega a cena de origem; não restaura uma partida de quiz/minijogo que tenha sido abandonada para sair da cena. Os valores e flags persistentes são mantidos.

## 9. Testes novos executados

Foram feitas **150 verificações direcionadas**, em Play Mode real 1920×1080:

- Migração de save v1 sem inventário; preservação de EXP, nível, chaves e flags.
- Preços dos três alimentos, compra de três maçãs, uma imagem por tipo, atualização do saldo e bloqueio de compra enquanto um pop-up está aberto.
- Compra sem saldo sem mutação do save.
- PointerDown, início de drag, acompanhamento do ponteiro, aproximação, afastamento e entrega, através dos handlers reais da UI.
- Soltar fora sem consumo; 3→2→1→0 com desaparecimento apenas na última unidade.
- Recuperações de +10/+25/+50 e caso 95+10=100.
- Tentativa com Energia cheia mantendo estoque e Energia.
- Frames reais de MouthOpen/Eating, Idle com loop e pausa da alimentação.
- Raycasts reais dos cards, quatro botões principais, X do Mercado/Vestuário e OK/X dos quatro pop-ups; overlays bloqueando cliques atrás.
- Corpo inteiro em Idle e grade vazia no Vestuário.
- Prioridade de Esc e acesso por Configurações.
- Retorno a Menu, MapaMundi e Brasil, singleton e saldo atualizado no HUD existente.
- Persistência de moedas, Energia e três estoques após sair e entrar em novo Play Mode.

Foi utilizado o caminho de testes já existente (`NexaQuest.Testes.Quiz`), sem escrever na chave de progresso real. A comparação antes/depois está em `save-real.txt`. Não foram reexecutadas as grandes suítes antigas de física, mapas, NPCs, perguntas e recompensas.

## 10. Resultado e capturas

**150 aprovações, 0 falhas.** O progresso real permaneceu idêntico. Capturas verificadas: PrincipalInicial, BancadaComAlimentos, BocaAberta, Comendo, Mercado, CompraRealizada, SaldoInsuficiente, EnergiaCheia, Vestuario e LojaIndisponivel, todas em PNG 1920×1080.

Na revisão visual, foram ajustados o preenchimento da barra e a posição da zona de boca. A verificação de pausa passou a medir o tempo após o primeiro frame pausado completo, evitando comparar com a avaliação de animação já iniciada no frame de abertura. Não houve falha nova de gameplay ou necessidade de restaurar o checkpoint.

## 11. Console e estado de entrega

Console final: **0 erros, 0 avisos, 0 mensagens**, registrado em `console.txt`. Compilação concluída sem referências faltantes ou exceções nos testes. A Unity foi devolvida à cena Menu, fora do Play Mode e sem cena pendente de salvar (`editor-final.txt`).

Não foi criado um build Standalone nesta etapa. O escopo de Vestuário continua limitado à apresentação da Quest e à grade vazia; Loja continua indisponível como solicitado.

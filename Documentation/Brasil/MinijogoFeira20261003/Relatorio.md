# Minijogo da Feira — entrega

Implementado na cena Brasil, usando as imagens originais fornecidas e nomes novos em português brasileiro.

## Segurança e resultado

- Checkpoint anterior à implementação: `441d409` — Preserva desafios da Feira e ajustes atuais antes do minijogo de frutas.
- Baseline: 429 verificações aprovadas antes das alterações.
- Validação final: **500 verificações aprovadas, zero falhas** em Play Mode real a 1920 × 1080.
- Console final: **0 erros, 0 avisos**, uma mensagem informativa do botão Quiz ainda provisório.
- Cena Brasil salva, sem alterações pendentes no Editor e fora de Play Mode.
- Auditoria por ID dos objetos serializados: nenhum bloco anterior removido; **157 componentes físicos/colliders idênticos ao checkpoint**.
- PlayerMovement, CameraFollow, DialogueController, NPCs, retratos, mapas, Portal, Project Settings e Build Settings preservados. As posições, dimensões e artes das HUDs anteriores também foram preservadas.
- Os 15 PNGs importados têm SHA-256 idêntico aos arquivos enviados. Detalhes em `auditoria.json`.

## Como jogar

Converse com Bento até abrir Desafios da Feira e selecione MINIJOGO. No primeiro acesso, clique JOGAR nas instruções. Use A/D ou as setas esquerda/direita para levar o cesto até as frutas. Pegue cinco de cada tipo e continue coletando para ganhar moedas adicionais.

SAIR durante a partida abre o resultado correspondente. SAIR no resultado retorna ao mesmo ponto do mapa. Esc executa a mesma ação de saída; nas instruções, permite voltar sem gastar Energia.

## Arquivos importados

Pasta: `Assets/Projeto/Sprites/Brasil/MinijogoFeira/`.

| Nome no projeto | Arquivo fornecido |
|---|---|
| CenarioFeira.png | cenário do minijogo.png |
| PainelInstrucoes.png | painel de instrução.png |
| PainelVitoriaComChave.png | painel de vitória.png |
| PainelVitoriaSemChave.png | Painel Vitoria sem chave.png |
| PainelDerrota.png | painel de game over.png |
| IndicadorFrutas.png | HUD de progresso das frutas.png |
| IndicadorErros.png | indicador de erros.png |
| BotaoSair.png | botão sair.png |
| QuestComCesto.png | Sprite sheet - Quest com cesto.png |
| Banana.png | Banana.png |
| Abacaxi.png | Abacaxi.png |
| Melancia.png | Melancia.png |
| Chinelo.png | Chinelo.png |
| Disco.png | CD.png |
| ConsoleXbox360.png | XBOX360.png |

A imagem duplicada de cenário foi representada por um único asset. Importação: Sprite, Point, sem compressão, sem mipmaps, transparência preservada, sem redimensionamento NPOT. As diferenças originais de proporção entre os painéis foram respeitadas. Nenhuma imagem foi redesenhada ou teve seus pixels modificados.

`RecortesQuestCesto.json` documenta cada retângulo e pivô do sprite sheet. Os `.meta` guardam os recortes efetivamente importados.

## Scripts

Criados em `Assets/Projeto/Scripts/Brasil/`:

- `MinijogoFeira.cs`: estados, regras, resultados, energia de início, recompensas e bloqueio do mapa.
- `QuestCestoFeira.cs`: movimento horizontal, limites, animação e alinhamento de cada quadro.
- `QuedaItensFeira.cs`: tipos dos itens, surgimento, queda, captura e limpeza.
- `InterfaceMinijogoFeira.cs`: painéis, contadores, aviso de energia e impacto visual.
- `IndicadorEnergia.cs`: atualiza o texto da HUD existente a partir do progresso real.

Modificados:

- `ProgressionManager.cs`: preserva Coins, EXP e Level; acrescenta Energia, Chave da Vila, instruções vistas e persistência local dessas informações.
- `PainelDesafiosFeira.cs`: conecta a seleção de MINIJOGO ao controlador real.
- `Assets/Projeto/Editor/ConfiguracaoDesafiosFeira.cs`: adapta a regressão do botão à atividade implementada e à regra atual da chave.

Ferramentas novas de Editor:

- `MontagemMinijogoFeira.cs`: importação, recortes, animações, montagem e referências persistentes da cena; recusa montagem duplicada.
- `ValidacaoMinijogoFeira.cs`: cenários de Play Mode e encadeamento dos testes anteriores, com progresso isolado.

Também foram modificados `Brasil.unity` e uma fala de `Feira_Desafios.asset`: Bento agora explica que cinco frutas de cada tipo concedem a Chave da Vila. Seus retratos, objeto, eventos e restante da conversa foram mantidos.

## Estrutura e objetos

Foram adicionados 38 GameObjects, incluindo textos e áreas transparentes de clique. A lista completa está em `auditoria.json`.

```text
Canvas
  PainelDesafiosFeira
    ArtePainel
      BotaoMinijogo (botão existente reutilizado)
    AvisoEnergia
  MinijogoFeira
    VisualMinijogo
      CenarioFeira
      AreaJogavel
        ItensEmQueda
        QuestComCesto
          ArteQuest
          AberturaCesto
    ProgressoFrutas
      QuantidadeBananas / QuantidadeAbacaxis / QuantidadeMelancias
    IndicadorErros
      FrutasPerdidas / ItensErrados
    BotaoSair / AreaClique
    Controles
    HUDsExistentes
    Instrucoes / ArtePainel / BotaoJogar
    VitoriaComChave / ArtePainel / BotaoRejogar / BotaoSair / MoedasDaPartida
    VitoriaSemChave / ArtePainel / BotaoRejogar / BotaoSair / MoedasDaPartida
    Derrota / ArtePainel / BotaoRejogar / BotaoSair
    AvisoEnergia
```

`Canvas/PainelDesafiosFeira/ArtePainel/BotaoMinijogo` já existia: foi preservado sobre a área impressa do PNG e conectado ao minijogo. Os demais botões são transparentes sobre as regiões impressas de Jogar, Rejogar e Sair, sem textos duplicados.

Objetos existentes modificados: Canvas recebe o novo root; PainelDesafiosFeira recebe a referência e o aviso; EnergyText recebe IndicadorEnergia; o ProgressionManager existente recebe os novos campos serializados. Durante a atividade, as HUDs existentes são temporariamente colocadas em HUDsExistentes, conservando seus retângulos. Ao fechar, voltam aos mesmos pais e posições na hierarquia.

## Quest, animações e captura

O sprite sheet foi inspecionado quadro a quadro e recortado por limites individuais, pois a primeira fileira possui quatro poses e as outras seis. Total: 16 sprites.

- Parada: quatro quadros, **2 FPS**.
- Esquerda: seis quadros, **8 FPS**.
- Direita: seis quadros, **8 FPS**.
- Animator: `Assets/Projeto/Animations/MinijogoFeira/QuestComCesto.controller`.
- Clipes: `QuestCesto_Parada.anim`, `QuestCesto_Esquerda.anim`, `QuestCesto_Direita.anim`.
- Parâmetro inteiro Direcao: -1, 0, 1; troca imediata, sem reiniciar o mesmo estado a cada atualização.
- Pivô horizontal baseado no centro do cesto; base dos pés alinhada; escala única de 0,9 unidade de UI por pixel da folha.
- Velocidade inicial: **680 unidades de UI/s**.
- Limites horizontais: **-550 a +550**; Y fixo **-385**, relativo ao centro da área 1920 × 1080.

AberturaCesto mede **124 × 12**, posição relativa **(0, 250)** sobre os pés. A coleta usa a travessia da abertura por cima, verificando o percurso vertical entre atualizações. A área do corpo, dos pés e da cauda não captura. Esta solução opera na UI e preserva a física 2D do mapa.

## Surgimento, erros e efeito de impacto

- Intervalo: **1,25 s**; primeiro item após 0,7 s.
- Queda: **210 unidades de UI/s**.
- Surgimento horizontal: **-520 a +520**, altura 400.
- Chão: -390.
- Chance de item errado: **18%**, com escolha entre chinelo, disco e console.
- Frutas distribuídas em grupos embaralhados contendo uma de cada tipo, para evitar longas sequências sem a fruta necessária.
- Limite de 14 itens ativos; remoção ao capturar, tocar o chão, encerrar ou reiniciar.

Os valores ficam expostos no Inspector em QuestCestoFeira e QuedaItensFeira. Fruta perdida aumenta FrutasPerdidas; objeto errado capturado aumenta ItensErrados. Objeto errado que chega ao chão não penaliza. Os contadores são independentes: 2 + 2 ainda permite jogar; 3 em qualquer um encerra.

Cada erro causa deslocamento suave somente de VisualMinijogo: duração **0,14 s**, intensidade máxima **4 unidades**. O deslocamento decai e volta à origem exata, inclusive ao sair. A câmera do Brasil permanece intacta.

## Recompensas e progresso

Os contadores de banana, abacaxi e melancia chegam visualmente até 5/5. Frutas extras continuam aumentando a contagem total da rodada. **A cada três frutas corretas, ProgressionManager.AddCoins(1)** credita uma NexaCoin real imediatamente. O resultado mostra somente as moedas ganhas naquela rodada.

Ao completar 5/5/5 pela primeira vez na rodada, ObjetivoConcluido fica verdadeiro e `AddXP(50)` executa uma única vez. O jogo e o surgimento de itens continuam. A fórmula de Level existente foi mantida: **100 EXP por nível**, preservando o restante da divisão como EXP do nível atual. Duas conclusões a partir do nível 1 e zero EXP resultam em nível 2, zero EXP restante.

Energia máxima inicial: **100**. Cada início ou Rejogar consome **20** pelo mesmo gerenciador. Nas instruções o custo só acontece ao clicar Jogar. Energia abaixo de 20 bloqueia a ação e exibe aviso; custos negativos e saldo insuficiente são recusados. EnergyText acompanha o valor real por evento, sem alteração visual da HUD. Recuperação de Energia continua fora desta etapa, conforme solicitado.

A primeira conclusão de 5/5/5 no progresso concede a Chave da Vila. `ConquistarChaveVila()` retorna falso quando ela já existe; o indicador persistente impede duplicação, inclusive após carregar. ChaveConquistadaNestaPartida seleciona o painel com chave somente na rodada que a concedeu. Vitórias posteriores usam o painel sem chave e continuam recebendo +50 EXP por rodada. Não há popup separado da chave.

O Portal mantém o comportamento existente. A informação pública `ProgressionManager.PossuiChaveVila` fica disponível para sua futura condição de abertura; esta etapa não implementa as outras duas chaves nem modifica o Portal.

## Resultados, saída e repetição

ObjetivoConcluido define o resultado tanto ao atingir três erros quanto ao apertar Sair: antes da meta, Derrota; depois dela, Vitória. O estado da rodada impede iniciar duas vezes, capturar o mesmo item novamente ou repetir a recompensa de EXP.

Rejogar verifica Energia e reinicia somente quantidades de frutas, erros, contagem interna para moedas, moedas da rodada, objetivo, sinal de chave daquela rodada, itens e posição da Quest. Coins acumuladas, EXP, Level, Energia restante e chave persistem.

SAIR no resultado fecha o overlay, limpa itens e restaura movimento, interação e transições do mapa na posição original do jogador.

## Persistência

O projeto tinha ProgressionManager, mas não havia persistência em disco desse progresso. A gravação foi incorporada ao mesmo gerenciador usando JSON versionado em PlayerPrefs, chave **NexaQuest.Progresso.v1**. São salvos Coins, EXP do nível, Level, Energia, chave e instruções vistas. Cada alteração real grava os dados.

Os testes usam exclusivamente **NexaQuest.Testes.Minijogo**, habilitada por sinal de sessão apenas no Editor. Ao final, a chave de progresso real ainda não existia: os testes não gastaram a Energia do jogador nem criaram recompensas em seu progresso.

## Validação executada

Play Mode real, com cliques verificados por raycast e eventos do EventSystem. Movimento e queda foram exercitados pelos métodos usados durante o jogo; animações avançadas pelo Animator. Os cenários de progressão resolveram itens de forma controlada, além dos testes específicos de travessia da abertura e do chão. Capturas da Game View inspecionadas em 1920 × 1080.

| Grupo | Verificações aprovadas | Falhas |
|---|---:|---:|
| Minijogo, cenários A–G e persistência | 70 | 0 |
| Painel de Desafios e Bento | 64 | 0 |
| Energia e expressão da HUD | 67 | 0 |
| Quest, visual e progressão | 100 | 0 |
| NPCs e diálogos | 88 | 0 |
| Portal e transições | 111 | 0 |
| **Total** | **500** | **0** |

Cobertura inclui primeiro acesso, custo único, energia 20 → 0, recusa em zero, 16 quadros animados, limites, queda dos seis tipos, coleta apenas pelo cesto, duplicação de item recusada, moedas reais, erros independentes, shake sem desvio, derrota, primeira vitória, vitória posterior, EXP/Level, continuidade após 5/5/5, limpeza, retorno ao mapa e carregamento persistente.

As primeiras tentativas da automação revelaram cliques executados antes do registro dos gráficos recém-ativados no Canvas. A validação passou a aguardar um frame antes de cada clique e então completou toda a sequência. O resultado final acima corresponde à execução completa posterior à correção.

Evidências: `minijogo-playmode.txt`, `depois/*.txt`, `auditoria.json` e as cinco capturas de instruções, partida, derrota e vitórias. Os validadores anteriores também atualizaram suas capturas de regressão nas pastas históricas.

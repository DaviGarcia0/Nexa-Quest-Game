# Quiz da Feira — entrega de 09/10/2026

Implementado como interface sobre a cena Brasil, no Canvas existente. O botão Quiz do Painel de Desafios abre a introdução. O Minijogo da Feira continua funcional e preservado.

## Segurança e resultado

Projeto real: `C:/Users/Davi/Desktop/Nexa-Quest-Game`. Unity 2022.3.62f3.

Novo checkpoint anterior à implementação: **415c625**, criado sobre o estado atual funcional (5888862). O checkpoint histórico 441d409 não foi utilizado.

Baseline executado antes da alteração, concluído às 11:42:26: **500 verificações, zero falhas, zero erros e zero avisos**. Evidências em `antes/`.

Validação final em Play Mode real, 1920 × 1080, concluída às 12:12:32: **635 verificações, zero falhas**. Console final: **zero erros, zero avisos**. Brasil salva, sem alterações pendentes na cena e fora de Play Mode. O teste usa chaves de persistência separadas; o progresso real foi comparado antes/depois e permaneceu idêntico.

| Suíte | Antes | Depois |
|---|---:|---:|
| Quiz | — | 134 |
| Minijogo | 70 | 70 |
| Painel de Desafios | 64 | 65 |
| Energia | 67 | 67 |
| HUD/Quest visual | 100 | 100 |
| NPCs/diálogo | 88 | 88 |
| Portal/transições | 111 | 111 |
| Total | 500 | 635 |

Os testes antigos foram mantidos. O teste do botão anteriormente provisório agora verifica a introdução real do Quiz e seu retorno ao painel.

A comparação dos componentes serializados com 415c625 encontrou 1.178 componentes anteriores preservados e 123 novos componentes. Desconsiderando a renumeração automática dos índices de irmãos pela Unity, somente três blocos anteriores receberam conteúdo: filhos do Canvas, referência `quiz` no PainelDesafiosFeira e campo `quizFeiraConcluido` no ProgressionManager. Os componentes anteriores do Minijogo, mapas, câmera, Player, NPCs e colliders não foram alterados. Scripts e sprites do Minijogo não foram modificados.

## Imagens originais

Pasta: `Assets/Projeto/Sprites/Brasil/QuizFeira/`. Todos os oito PNGs foram copiados sem alteração de bytes, verificada por SHA-256 contra Downloads. Nomes em português, sem espaços/acentos nos identificadores.

| Arquivo fornecido | Nome no projeto |
|---|---|
| Painel inicial do quiz.png | PainelInicialQuiz.png |
| Painel principal do quiz.png | PainelPrincipalQuiz.png |
| Painel final.png | PainelFinalQuiz.png |
| Feedback de acerto.png | FeedbackAcerto.png |
| Feedback de erro.png | FeedbackErro.png |
| Quest pensando.png | QuestPensando.png |
| Quest triste.png | QuestTriste.png |
| Quest feliz.png | QuestFeliz.png |

Importação: Sprite Single, Point, Compression None, Mip Maps Off, alpha preservado, proporções preservadas, NPOT None e limite 4096 para não reduzir o PNG de feedback com largura 2172. Nenhum redesenho, mudança de cores ou de textos incorporados nas imagens.

## Arquivos criados e modificados

Criados em `Assets/Projeto/Scripts/Brasil/`:

- `DadosQuizFeira.cs`: perguntas editáveis e validação de três perguntas/quatro alternativas.
- `QuizFeira.cs`: estados, tempo, respostas, energia, recompensas e saída.
- `InterfaceQuizFeira.cs`: painéis, TextMeshPro, retratos, feedback e empréstimo/restauração dos HUDs existentes.

Criados em `Assets/Projeto/Editor/`:

- `MontagemQuizFeira.cs`: montagem única, importação e configuração da cena.
- `ValidacaoQuizFeira.cs`: testes de Play Mode e encadeamento da regressão anterior.

Criado `Assets/Projeto/Dados/PerguntasQuizFeira.asset`, editável pelo Inspector. Também foram criados os respectivos arquivos .meta e as pastas de dados/sprites.

Modificados:

- `Assets/Projeto/Scenes/Brasil.unity`: novo root e referências.
- `Assets/Projeto/Scripts/Brasil/PainelDesafiosFeira.cs`: ligação do Quiz e proteção para Esc não fechar no mesmo quadro o painel que acaba de reabrir.
- `Assets/Projeto/Scripts/Brasil/ProgressionManager.cs`: flag persistente de conclusão e isolamento dos testes do Quiz.
- `Assets/Projeto/Editor/ConfiguracaoDesafiosFeira.cs`: teste do botão real do Quiz substitui o teste do aviso provisório.
- Relatórios/capturas das suítes anteriores: atualizados pela execução da regressão.

## Objetos e localização dos campos

A raiz criada é `Canvas/QuizFeira`, inicialmente inativa, com QuizFeira, InterfaceQuizFeira e fundo modal. O Canvas, EventSystem, HUDs e ProgressionManager existentes são reutilizados.

```text
Canvas/QuizFeira
  HUDsExistentes
  PainelInicial/ArtePainel
    ExplicacaoQuiz
    BotaoComecar
    BotaoVoltar
  PainelPrincipal/ArtePainel
    ProgressoPerguntas
    TempoRestante
    Enunciado
    RetratoQuest
    RespostaA/TextoAlternativa
    RespostaB/TextoAlternativa
    RespostaC/TextoAlternativa
    RespostaD/TextoAlternativa
    MensagemStatus
  FeedbackErro/ArteFeedback
  FeedbackAcerto/ArteFeedback
  PainelFinal
    ArtePainel/BotaoContinuar
    RecompensasReais
```

`Canvas/PainelDesafiosFeira/ArtePainel/BotaoQuiz` continua sendo o Button existente, conectado a `PainelDesafiosFeira.SelecionarQuiz`; a referência `quiz` aponta para o novo controlador. Não foi criado outro painel de desafios.

Correspondência dos nomes conceituais solicitados: QuestionProgressText → ProgressoPerguntas; TimerText → TempoRestante; QuestionText → Enunciado; QuestPortrait → RetratoQuest; StatusText → MensagemStatus.

Os painéis Inicial/Principal têm largura 1040, altura calculada pela proporção original e centro deslocado 95 unidades para baixo, deixando os HUDs legíveis. Final: largura 1340. Feedback: largura 850, proporção original, posição inferior. Retrato: um único Image com Preserve Aspect, no mesmo retângulo para as três expressões.

As alternativas A/B/C/D têm Buttons transparentes sobre as quatro caixas da arte; seus TextMeshPro ficam em filhos separados. Começar, Voltar e Continuar usam Buttons transparentes sem duplicar os textos desenhados. As áreas dos botões foram verificadas por raycast no centro e nos quatro cantos internos após a renderização.

HUDs de Energia e progressão são temporariamente emprestados para HUDsExistentes, conservando seus componentes e posições. O aviso existente de Energia é reutilizado. Pais, índices e posição do aviso são restaurados antes de desativar o Quiz, evitando mudanças na hierarquia durante a desativação.

## Regras e estados

Abrir Quiz mostra a introdução em toda entrada, sem custo. Voltar retorna ao Painel de Desafios sem custo. Começar usa `ProgressionManager.TentarGastarEnergia(20)`: 100→80, 20→0; valores abaixo de 20 bloqueiam com o aviso existente. EnergyText atualiza pelo evento Changed. Duplo clique em Começar não desconta novamente.

O controle do Player fica bloqueado; o sistema existente recusa interações com NPCs e viagens enquanto bloqueado. O painel de desafios fica oculto e o Minijogo não abre simultaneamente. Ao sair, o painel de desafios retoma seu bloqueio normal; ao fechá-lo, o mapa volta a responder. A posição do Player é preservada.

Timer real: `Time.unscaledDeltaTime`, independente da escala de tempo, 30 segundos por pergunta. O número exibido arredonda para cima. Apenas o estado Pergunta consome tempo.

Erro: mesma pergunta, zero recompensa, retrato triste, respostas desabilitadas, timer pausado. Feedback permanece opaco por **1,2 s** e faz fade linear por **0,3 s**, total **1,5 s**. Após o fade, volta o retrato pensando, os botões são habilitados e o tempo continua do valor anterior. Cada nova exibição restaura alpha 1.

Timeout: tempo chega a zero, mensagem Tempo esgotado, mesmo feedback de erro e nenhuma recompensa. Após o feedback, a mesma pergunta reinicia em 30 s.

Acerto: timer para, retrato feliz e mesmo feedback de 1,5 s. `perguntasPremiadas` impede crédito repetido por pergunta; o estado também bloqueia respostas enquanto há feedback. Cada resposta correta concede **10 NexaCoins reais**, total **30 por rodada**. Depois do fade, a pergunta seguinte inicia com 30 s. O terceiro feedback termina antes do painel final.

Conclusão: `RodadaConcluida` protege **+50 EXP uma vez por rodada**. O sistema existente calcula Level a cada 100 EXP. `MarcarQuizFeiraConcluido()` grava a flag true no mesmo JSON v1 e não a reinicia em novas rodadas. Saves antigos sem esse campo continuam válidos e carregam false para ele.

A chave usa exclusivamente `ProgressionManager.ConquistarChaveVila()`, compartilhado com o Minijogo. Só concede quando a flag existente ainda é false. Quiz repetido e Minijogo posterior reconhecem a mesma chave, sem duplicação.

Esc retorna ao Painel de Desafios em qualquer tela do Quiz, cancela feedback/fade e restaura a interface. Não reembolsa Energia nem moedas já creditadas. Saída antes de completar não concede EXP nem conclusão; uma conclusão anterior permanece true. Nova entrada mostra a introdução e uma nova rodada paga 20, preservando progresso global.

## Perguntas editáveis

| Número | Pergunta | Alternativas A / B / C / D | Correta |
|---|---|---|---|
| 1 | Qual é a capital do Brasil? | Rio de Janeiro / Brasília / Salvador / São Paulo | B: Brasília |
| 2 | Qual destes alimentos vendidos na feira é uma fruta? | Alface / Batata / Banana / Cenoura | C: Banana |
| 3 | Qual é a moeda oficial usada nas compras em uma feira brasileira? | Real / Euro / Dólar / Peso | A: Real |

Fontes primárias registradas no asset: [Governo federal — Brasília](https://www.gov.br/g20/pt-br/sobre-o-g20/cidades-sede/brasilia-df), [Embrapa — Banana](https://www.embrapa.br/web/mandioca-e-fruticultura/cultivos/banana), [Banco Central — Plano Real](https://www.bcb.gov.br/controleinflacao/planoreal).

## Limitação visual da arte final

O PNG original contém **+1 NexaCoin e +1 Chave da Vila desenhados**, incluindo quando a chave já existe. Esses pixels foram preservados conforme solicitado. A lógica credita 30 Coins na rodada, 50 EXP ao concluir e nunca duplica a chave. Um resumo dinâmico abaixo do painel informa as recompensas reais e distingue Chave conquistada de Chave já obtida. A divergência dos textos fixos da imagem permanece visível.

## Evidências e verificação

`quiz-playmode.txt` registra as 134 verificações do Quiz: entrada/retorno, gasto exato, Energia 0/19/20, timer pelo Update real, erros repetidos, espera/fade reais, timeout, duplo clique, sequências de moedas, EXP/Level, replay, JSON anterior/novo, chave compartilhada com Minijogo, Esc, bloqueios e restauração. `depois/` contém as seis suítes anteriores, Console, estado da cena, data de conclusão e confirmação de progresso real preservado.

Capturas revisadas visualmente: Inicial-1080p.png, Pergunta-1080p.png, Erro-1080p.png, Acerto-1080p.png, FinalPrimeiraChave-1080p.png, FinalChaveExistente-1080p.png e EnergiaInsuficiente-1080p.png. Textos das três perguntas/alternativas passaram na verificação de overflow. Retratos não alteram o retângulo entre estados.

Durante a implementação, o teste identificou a restauração do aviso dentro de OnDisable; ela foi antecipada para antes da desativação. As verificações de raycast também passaram a aguardar um quadro renderizado ao ativar painéis. Após essas correções, a suíte completa passou sem mensagens novas no Console.
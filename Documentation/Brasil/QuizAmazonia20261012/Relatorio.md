# Quiz da Amazônia — entrega de 12/10/2026

Implementado dentro da cena Brasil existente. A Guardiã abre o Painel de Desafios da Amazônia; clicar Quiz verifica Energia, cobra 20 e começa diretamente a primeira pergunta. O Quiz tem dados, controlador e interface próprios. Os sistemas da Feira permanecem preservados.

## Checkpoint e validação

Projeto real: `C:/Users/Davi/Desktop/Nexa-Quest-Game`. Unity 2022.3.62f3.

Novo checkpoint criado imediatamente antes desta implementação: **00866c9**. Inclui o estado atual salvo pelo usuário e o baseline executado nesta etapa. Nenhum checkpoint histórico foi utilizado como rollback.

Baseline: **722 verificações aprovadas, zero falhas, zero erros e zero avisos**, concluído às 15:36:25. Evidências em `antes/`.

Resultado final: **891 verificações aprovadas, zero falhas, zero erros e zero avisos no Console**, em Play Mode real a 1920 × 1080. Regressão concluída às 15:57:14. A cena Brasil está salva e fora do Play Mode.

| Suíte | Baseline | Final |
|---|---:|---:|
| Quiz da Amazônia | — | 168 |
| NPCs/diálogo Amazônia e Lia | 81 | 82 |
| Quiz da Feira | 134 | 134 |
| Minijogo da Feira | 70 | 70 |
| Painel da Feira | 65 | 65 |
| Energia | 67 | 67 |
| Quest/HUD visual | 100 | 100 |
| NPCs/diálogos da Feira | 94 | 94 |
| Portal/transições | 111 | 111 |
| Total | 722 | 891 |

O teste anterior dos NPCs da Amazônia foi mantido e atualizado para verificar o painel real conectado, em vez de esperar um evento ainda vazio. Foi acrescentada uma verificação da abertura do painel ao concluir o diálogo.

O progresso real foi comparado antes/depois e permaneceu idêntico. As validações reutilizam as chaves de teste isoladas já existentes; não existe um segundo save do produto. Confirmações em `progresso-real.txt` e `depois/progresso-real.txt`.

## Assets

Importados em `Assets/Projeto/Sprites/Brasil/QuizAmazonia/`:

| PNG fornecido | Nome no projeto |
|---|---|
| Painel_Desafio_Amazonia.png | PainelDesafiosAmazonia.png |
| Painel Quiz - amazonia.png | PainelPrincipalQuizAmazonia.png |

Os arquivos são idênticos em bytes aos originais de Downloads, conferidos por SHA-256 em `auditoria.txt`. Configuração: Sprite Single, Point, Compression None, Mip Maps Off, NPOT None, alpha preservado, Max Size 4096, proporções preservadas. Nenhum pixel, cor ou texto desenhado foi alterado.

Reutilizados diretamente de `Assets/Projeto/Sprites/Brasil/QuizFeira/`: QuestPensando, QuestTriste, QuestFeliz, FeedbackErro e FeedbackAcerto. Esses PNGs não foram duplicados nem editados. Nenhuma arte inicial ou final nova foi inventada, e os painéis temáticos da Feira não são exibidos na Amazônia.

## Arquivos e arquitetura

Criados em `Assets/Projeto/Scripts/Brasil/`:

- `PainelDesafiosAmazonia.cs`: abertura/fechamento do painel, bloqueio modal, botões e aviso existente de Energia.
- `QuizAmazonia.cs`: estados, timer, respostas, recompensas, replay e saída.
- `InterfaceQuizAmazonia.cs`: campos dinâmicos, retrato único, feedback/fade, conclusão e HUDs compartilhados.
- `DadosQuizAmazonia.cs`: perguntas editáveis e validação de exatamente três perguntas/quatro alternativas.

Criados em `Assets/Projeto/Editor/`:

- `MontagemQuizAmazonia.cs`: importação, montagem única na cena e ajuste das margens das alternativas.
- `ValidacaoQuizAmazonia.cs`: testes reais de interface, regras, persistência e encadeamento das suítes anteriores.

Criado `Assets/Projeto/Dados/PerguntasQuizAmazonia.asset`, com as três perguntas solicitadas. Também foram criados os respectivos arquivos .meta.

Modificados:

- `Assets/Projeto/Scripts/Brasil/ProgressionManager.cs`: somente integração das flags e métodos da Amazônia ao progresso atual.
- `Assets/Projeto/Scenes/Brasil.unity`: dois roots de interface, referências e evento da Guardiã.
- `Assets/Projeto/Dialogue/Amazonia_Desafios.asset`: última fala passa a ser exatamente “Posso te desafiar de duas formas.”
- `Assets/Projeto/Editor/ValidacaoNPCsAmazonia.cs`: verifica a conexão e a abertura do novo painel; fecha o modal antes de verificar a restauração do controle.
- Documentação/capturas das suítes anteriores: atualizadas pela regressão.

Os scripts QuizFeira, InterfaceQuizFeira, DadosQuizFeira, MinijogoFeira, PlayerMovement, CameraFollow, DialogueController e NPCInteraction permanecem idênticos aos do checkpoint. Não houve refatoração da Feira.

A auditoria da cena registra 1.334 componentes anteriores preservados e 116 novos. Somente três blocos anteriores receberam conteúdo novo: evento On Dialogue Finished da Guardiã, lista de filhos do Canvas e os dois campos de progressão. Nenhum collider, mapa, câmera, Player ou NPC decorativo anterior foi deslocado/removido.

## Conexão e GameObjects

`Amazonia/NPCsAmazonia/NPC_Amazonia_01 → NPCInteraction → On Dialogue Finished` tem um listener persistente para `PainelDesafiosAmazonia.Abrir`. A interação E e o DialogueController são os existentes. A Guardiã continua sendo a única NPC interativa dessa região. NPCs 02, 03 e 04 permanecem decorativos, nos mesmos lugares, com os mesmos Idle.

```text
Canvas
  PainelDesafiosAmazonia
    ArtePainel
      BotaoQuiz
      BotaoMinijogo
    MensagemProvisoria
  QuizAmazonia
    HUDsExistentes
    PainelPrincipal
      ArtePainel
        ProgressoPerguntas
        TempoRestante
        Enunciado
        RetratoQuest
        RespostaA/TextoAlternativa
        RespostaB/TextoAlternativa
        RespostaC/TextoAlternativa
        RespostaD/TextoAlternativa
        MensagemStatus
        BotaoVoltar
    FeedbackErro/ArteFeedback
    FeedbackAcerto/ArteFeedback
```

Os dois roots começam inativos e têm fundo modal no próprio root. O Canvas e EventSystem atuais são reutilizados. Painéis: largura 1040, altura calculada pela proporção original, centro (0, -95), deixando os HUDs superiores legíveis.

BotaoQuiz e BotaoMinijogo são Buttons transparentes sobre as regiões impressas, sem outro texto “Quiz”/“Minijogo” sobre a arte. BotaoMinijogo mostra **“Minijogo em desenvolvimento.”**, permanece no painel e não cobra Energia nem inicia gameplay.

Durante o Quiz, os HUDs existentes de Energia e progressão são temporariamente reparentados para HUDsExistentes, conservando seus componentes e posições. Ao retornar, os pais e índices originais são restaurados. O mesmo aviso de Energia da Feira é emprestado e devolvido antes da desativação dos roots.

## Campos visuais

Todos os campos abaixo ficam em `Canvas/QuizAmazonia/PainelPrincipal/ArtePainel`:

- **ProgressoPerguntas**: 1/3, 2/3, 3/3, ao lado de PERGUNTA.
- **TempoRestante**: número real arredondado para cima com “s”, na caixa vermelha do relógio.
- **Enunciado**: texto completo centralizado, word wrapping e quebra após cada sete palavras.
- **RetratoQuest**: um único Image, Preserve Aspect ativo; troca somente sprite entre pensando/triste/feliz. Posição, tamanho e anchors não mudam.
- **Respostas A/B/C/D**: Buttons transparentes com TextMeshPro em filhos separados. Margem interna horizontal 18 e vertical 12; tamanho automático de 14 a 20 para manter as alternativas longas completas dentro das caixas.
- **MensagemStatus**: faixa verde inferior. Exibe orientação normal, retorno aos desafios na conclusão e “Tempo esgotado!” no timeout.
- **BotaoVoltar**: usa a faixa verde inferior apenas na conclusão, com o texto dinâmico “Voltar aos desafios”.

Os feedbacks usam CanvasGroup e interceptam cliques em toda a tela enquanto ativos. Suas imagens preservam a proporção original e são exibidas na parte inferior.

## Regras

Estados: Fechado, Pergunta, Erro, Acerto e Concluido. Não existe um novo painel inicial.

Abrir o painel de escolha após o diálogo é gratuito. Clicar Quiz chama `QuizAmazonia.TentarIniciar` e `ProgressionManager.TentarGastarEnergia(20)`. Energia 100 passa a 80; Energia 20 passa a zero; abaixo de 20, não desconta nem abre o Quiz e mostra o aviso existente. O estado e o painel ativo impedem gasto duplo por clique repetido.

O Player fica bloqueado durante painel/Quiz. O sistema atual bloqueia interação de NPC e transição enquanto os controles estão travados. Ao sair do Quiz, o painel de escolha reassume o bloqueio modal; ao fechar esse painel com Esc, o mapa volta a responder.

Timer: **30 segundos por pergunta**, avançado por `Time.unscaledDeltaTime`. Foi testado pelo Update real inclusive com timeScale zero. Somente Pergunta consome tempo.

Erro: não avança nem premia. Desabilita respostas, pausa o timer, troca para QuestTriste e mostra FeedbackErro. Opaco por **1,2 s**, fade linear por **0,3 s**, total **1,5 s**. Depois volta QuestPensando e continua a mesma pergunta do tempo anterior. Cada nova exibição restaura alpha 1.

Timeout: chega a zero, usa QuestTriste/FeedbackErro e MensagemStatus **“Tempo esgotado!”**, sem prêmio. Depois do feedback, a mesma pergunta reinicia em 30 segundos.

Acerto: bloqueia respostas e para o timer; mostra QuestFeliz e FeedbackAcerto com a mesma duração. Chama `ProgressionManager.AddCoins(10)` exatamente uma vez para aquela pergunta. Um array de perguntas premiadas e os estados bloqueiam duplicação durante cliques/fade. Depois, a próxima pergunta começa com 30 segundos.

Terceiro acerto: aguarda o feedback e fade; então concede `ProgressionManager.AddXP(50)` uma única vez por rodada, marca QuizAmazoniaConcluido e tenta conceder a chave. O Level mantém a regra existente de 100 EXP.

Conclusão permanece na interface amazônica atual: enunciado mostra “Quiz concluído!”, +30 NexaCoins e +50 de Conhecimento. Só mostra “Chave da Amazônia conquistada!” quando a chave foi concedida naquela rodada. Em replay com chave existente, não anuncia outra chave.

Esc em qualquer estado do Quiz retorna ao painel da Amazônia, limpa feedback/fade e restaura HUDs. Não reembolsa Energia nem apaga Coins já creditados. Sair antes de concluir não concede EXP final, conclusão ou chave. Nova tentativa custa outros 20, reseta perguntas/timer/recompensas locais e permite receber novamente 30 Coins e 50 EXP; flags de conclusão/chave já conquistadas continuam true.

## ProgressionManager e save

Novos campos/propriedades independentes:

- `quizAmazoniaConcluido` / `QuizAmazoniaConcluido`.
- `possuiChaveAmazonia` / `PossuiChaveAmazonia`.

Novos métodos:

- `MarcarQuizAmazoniaConcluido()`: só muda false para true e salva/notifica.
- `ConquistarChaveAmazonia()`: retorna true ao conceder pela primeira vez; retorna false quando já existe, sem duplicar.

Os campos são salvos/carregados no **mesmo JSON v1 e mesma chave NexaQuest.Progresso.v1**. Saves anteriores sem eles carregam ambos como false, conservando os campos antigos. Foram testados save antigo, gravação, recarga e preservação simultânea das flags/chaves da Feira e da Amazônia.

PossuiChaveVila e ConquistarChaveVila não foram modificados. O Portal também não foi reimplementado; PossuiChaveAmazonia está disponível para sua futura integração.

## Perguntas configuradas

Os textos/alternativas foram mantidos exatamente como solicitados, com somente wrapping visual. São editáveis em `Assets/Projeto/Dados/PerguntasQuizAmazonia.asset`.

1. **O que é a Amazônia Legal?**
   A: Somente a parte da floresta que fica no estado do Amazonas.
   B: Uma região definida por lei para planejamento e políticas públicas de desenvolvimento da Amazônia brasileira.
   C: Uma unidade de conservação onde nenhuma pessoa pode viver.
   D: Outro nome dado apenas ao Bioma Amazônia.
   **Correta: B.**

2. **Qual processo das árvores amazônicas ajuda a devolver água para a atmosfera e alimentar os chamados 'rios voadores'?**
   A: Sedimentação. B: Evapotranspiração. C: Erosão. D: Fossilização.
   **Correta: B — Evapotranspiração.** Os textos no asset não têm pontuação adicional nas alternativas.

3. **Por que o desmatamento da Amazônia pode alterar o regime de chuvas?**
   A: Porque aumenta permanentemente a quantidade de água liberada pelas árvores.
   B: Porque transforma os rios da floresta em água salgada.
   C: Porque reduz a vegetação que recicla água para a atmosfera, afetando o transporte de umidade e as chuvas.
   D: Porque impede completamente a entrada de ventos do Oceano Atlântico.
   **Correta: C.**

Fontes primárias consultadas e registradas no asset: [IBGE — Amazônia Legal](https://www.ibge.gov.br/geociencias/organizacao-do-territorio/estrutura-territorial/15819-amazonia-legal.html) e [DNIT — Rios voadores](https://www.gov.br/dnit/pt-br/assuntos/portais-tematicos/br-319-am-ro/noticias/rios-voadores-mantem-o-clima-e-abastecem-o-brasil). O conteúdo fornecido pelo usuário foi preservado, sem substituir perguntas por outras.

## Evidências finais

`quiz-amazonia-playmode.txt` registra 168 verificações: texto exato das perguntas/alternativas/corretas, clique real, cantos das caixas, transparência, margens das letras, energia 0/19/20, timer real, erro repetido, bloqueio de cliques, espera/fade reais, timeout, moedas acumuladas, duplo clique, conclusão, replay, EXP/Level, duas chaves/flags, JSON, Esc e restauração.

`depois/` contém todas as suítes anteriores e Console final. Capturas finais revisadas: Desafios, Perguntas 1/2/3, Erro, Acerto, ConclusaoPrimeiraChave, ConclusaoReplay e EnergiaInsuficiente, todas em 1080p.

A revisão visual identificou alternativas longas próximas demais da moldura na primeira montagem. As margens foram aumentadas e a fonte foi limitada ao intervalo 14–20. A correção foi salva na cena, conferida em capturas e validada novamente com toda a regressão. Não houve alteração nos PNGs.
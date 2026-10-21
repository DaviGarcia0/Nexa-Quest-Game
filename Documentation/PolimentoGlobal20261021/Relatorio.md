# Polimento global — Nexa Quest Game

Entrega de 21/10/2026, Unity 2022.3.62f3. Projeto real: `C:/Users/Davi/Desktop/Nexa-Quest-Game`.

## Segurança e escopo

1. **Checkpoint:** `5cbb2ad` — preserva o estado encontrado, incluindo as alterações anteriores do usuário no Mapa-Múndi. A inspeção anterior à implementação confirmou compilação sem erros e Console com 0 erros e 0 avisos. Evidências: `baseline.txt` e `antes-*.txt`.
2. **Arquivos criados:** scripts `Global/NexaAudioManager.cs`, `PauseSettingsManager.cs`, `UIButtonAudio.cs`, `GlobalUIActions.cs`, `MenuVideoBackground.cs`, `MapaMundiUI.cs`; editores `ImportacaoMediaNexa.cs`, `MontagemPolimentoGlobal.cs`, `ValidacaoPolimentoGlobal.cs`; prefab `Resources/NexaGlobalSystems.prefab`; mixer `Audio/Polimento20261021/NexaMaster.mixer`; RenderTexture `Video/MenuVideo.renderTexture`; 17 arquivos de mídia fornecidos, seus `.meta`, `.gitattributes` e esta documentação. Os caminhos sob Assets têm prefixo `Assets/Projeto/`. O inventário completo está em `arquivos.txt`.
3. **Arquivos modificados:** cenas `Menu.unity`, `MapaMundi.unity`; `Scripts/SceneLoader.cs`; em `Scripts/Brasil/`: `BrazilWorld`, `DialogueController`, `PortalInteraction`, `QuizFeira`, `QuizAmazonia`, `InterfaceQuizFeira`, `InterfaceQuizAmazonia`, `MinijogoFeira`, `MinijogoAmazoniaController`, `AmazoniaItemSpawner`, `QuedaItensFeira`, `QuestCestoFeira`, `PainelDesafiosFeira`, `PainelDesafiosAmazonia`. Alterações nos sistemas existentes limitadas a chamadas de áudio e proteção da entrada/timers durante a pausa. `SceneLoader` ganhou proteção contra chamadas repetidas.
4. **GameObjects:** prefab persistente `NexaGlobalSystems`, filhos `MusicSource`, `UISource`, `SFXSource`, Canvas `ConfiguracoesGlobais`, `Overlay`, `PainelVolume`, `VolumeGeralSlider` e partes visuais, botões `Voltar`, `MinhaQuest`, `Sair`. No Menu: `VideoBackground` e `botaoSair`. No Mapa: `NavegacaoMapaMundi`, `MapaAmericaDoSul`, quatro áreas de botão, popup de indisponibilidade e `VoltarMapaMundi`. `EventSystem` é criado somente quando a cena não possui um.

## Vídeo e configurações

5. **Vídeo:** `VideoPlayer → RenderTexture 3840×2160 → RawImage`, dentro de `JanelaMenu`, depois do fundo estático e antes dos botões/logo. `VideoAudioOutputMode.None`, `FitOutside`, sem deformação. Os nove RectTransforms originais do Menu foram comparados com o checkpoint e estão idênticos (`auditoria-escopo.json`). O MP4 original não foi editado nem transcodificado; está configurado para Git LFS por ter 654.473.052 bytes.
6. **Início e loop:** Play On Awake e Loop habilitados na cena. A rotina de início para a preparação automática, chama `Prepare()`, aguarda a preparação e então chama `Play()` fora do callback nativo. O fundo original fica visível até o primeiro frame em reprodução; somente então a RawImage aparece. O relógio do vídeo usa tempo sem escala. A chegada ao fim e o loop foram confirmados em Play Mode.
7. **Painel:** prefab único carregado antes da primeira cena e mantido com `DontDestroyOnLoad`. Overlay preto com alpha 0,55 e Canvas acima do jogo. Usa os quatro PNGs fornecidos e um Slider real, azul e dourado, sem duplicar os textos das artes. A interface continua operando com `Time.timeScale=0`.
8. **Disponibilidade:** Menu pelo botão Configurações; Menu, MapaMundi, Brasil e MinhaQuest por teclado. O bootstrap também atende outras cenas. O painel restaura o estado e atualiza as referências ao trocar de cena.
9. **P:** tenta abrir/fechar configurações. Durante uma transição de área, a abertura é adiada por bloqueio para não interromper o fade.
10. **Esc:** fecha configurações se abertas e consome a entrada daquele frame. Com configurações fechadas, quizzes, minijogos, desafios, diálogo e demais bloqueios do Brasil mantêm prioridade. Sem atividade consumindo Esc, abre configurações. As entradas dos sistemas afetados respeitam o bloqueio do frame para evitar fechar a partida junto com o painel.
11. **Voltar:** fecha o painel e restaura exatamente o TimeScale, o estado anterior do AudioListener, o bloqueio anterior do player e a seleção de UI; não recarrega a cena. Verificados TimeScale anterior de 0,65 e player previamente bloqueado.
12. **Minha Quest:** restaura a pausa antes de carregar a cena real `MinhaQuest`, com proteção contra navegação duplicada. A cena existente foi preservada.
13. **Sair:** chama `Application.Quit()` fora do Editor. No Editor, apenas registra a solicitação e restaura a pausa. O encerramento real do executável não foi exercitado nesta validação em Editor.
14. **AudioMixer:** criado `NexaMaster.mixer`, com grupo Master; as três AudioSources passam por ele.
15. **Master Volume:** parâmetro exposto `MasterVolume`. Zero aplica −80 dB; valores positivos aplicam `20*log10(volume)`, com piso seguro em −80 dB. Testados 0, 0,5 e 1, clique na barra, arraste e recarga do valor.
16. **Preferência:** `NexaQuest.Audio.MasterVolume`, padrão 1. PlayerPrefs somente para essa configuração em produção. A validação usa a chave de testes já existente para o progresso e restaura a preferência anterior ao terminar. A persistência foi confirmada em novo Play Mode.
17. **AudioManager:** singleton `NexaAudioManager`, com referências centralizadas dos clips e API reutilizável `PlayQuizCorrect`, `PlayQuizWrong`, `PlayFeiraError`, `PlayMerge` e `PlayArea`. Cópia duplicada do prefab é eliminada.
18. **Categorias:** Music, UI e SFX são três AudioSources, todas no grupo Master. Não foram criados mixers independentes por categoria. Ganhos iniciais: música 0,28; UI 0,70; SFX 0,65. Música e UI recebem `ignoreListenerPause=true` em runtime, pois essa propriedade não persistiu no prefab. SFX respeita a pausa e os sons ativos de gameplay são interrompidos ao abrir o painel. Não há música em AudioSources espalhadas pelos botões.

## Clips e músicas

Os arquivos abaixo foram copiados de Downloads sem mudar seu conteúdo. `assets.json` registra os nomes originais e destinos; `integridade-assets.json` confirma SHA-256 idêntico nos 17 arquivos.

| Item | Original fornecido | Asset usado e ligação |
|---|---|---|
| 19. Normal | som de selecionar normal.mp3 | `SFX/Normal.mp3`: botões comuns do Menu, Mapa e Configurações. Listener único executado antes da navegação; proteção contra repetição no mesmo frame. Slider não emite cliques contínuos. |
| 20. Jogar/Brasil | som de game-start e Pais-6104.mp3 | `SFX/Iniciar.mp3`: exatamente o mesmo clip em Jogar e Brasil. |
| 21. Quiz certo | Som de acertar no quiz.mp3 | `SFX/QuizCorreto.mp3`: feedback correto dos dois quizzes. |
| 22. Quiz errado | Som de errar no quiz.mp3 | `SFX/QuizErrado.mp3`: erro e timeout, uma chamada por feedback. |
| 23. Erro Feira | som de errar no minigame da feira.mp3 | `SFX/ErroFeira.mp3`: fruta perdida e item errado capturado, na penalidade real. Fruta correta não emite erro. |
| 24. Fusão | som da fusao de frutas no minigame da amazonia.mp3 | `SFX/Fusao.mp3`: no ponto em que um merge válido concede a moeda; uma chamada por fusão, incluindo cadeias. |
| 25. Menu/Mapa | Musica do menu e do mapamundi.mp3 | `Music/MenuMapa.mp3`: a mesma fonte persiste entre Menu e Mapa sem reiniciar a música. MinhaQuest utiliza essa trilha também. |
| 26. Feira | NexaQuest_Feira_Brasil_8bit.ogg | `Music/Feira.ogg`: entrada no Brasil e área Feira. |
| 27. Amazônia | NexaQuest_Amazonia_8bit.ogg | `Music/Amazonia.ogg`. |
| 28. Ponto Turístico | NexaQuest_PontoTuristico_8bit.ogg | `Music/PontoTuristico.ogg`. |
| 29. Portal | NexaQuest_Portal_8bit.ogg | `Music/Portal.ogg`. |

Os caminhos Music/SFX pertencem a `Assets/Projeto/Audio/Polimento20261021/`. Músicas: Streaming, Vorbis qualidade 0,8. Efeitos: Decompress On Load, PCM. Sample Rate preservado. Músicas em loop. PNGs importados como sprites sem compressão, sem mipmaps, filtro Point e proporção preservada.

30. **Troca de área:** uma chamada em `BrazilWorld.SwitchArea`, quando a área ativa muda durante o preto do fade já existente. Não há busca de região em cada Update. Foram exercitados os trajetos de ida e volta Feira/Amazônia, Amazônia/Ponto Turístico e Feira/Portal. Quiz e minijogo mantêm a música da região.
31. **Crossfade:** não utilizado. Há somente uma MusicSource, que encerra o clip anterior antes de iniciar o próximo; solicitar novamente o clip que já toca não reinicia sua reprodução.

## América do Sul e navegação

32. **Estrutura:** preservado o Canvas existente `JanelaAmericaSul`, que estava vazio e com escala zero. Escala normalizada, imagem `MapaAmericaDoSul` dentro dele e Buttons transparentes sobre as placas da arte. O botão existente América do Sul alterna os dois Canvas. Os cadeados dos demais continentes foram preservados.
33. **Áreas clicáveis:** coordenadas medidas no PNG original de 1672×941, origem no canto superior esquerdo, formato `(x,y,largura,altura)`: Brasil `(910,278,265,85)`; Chile `(610,389,174,139)`; Argentina `(810,548,275,95)`. Convertidas proporcionalmente para o RectTransform da arte, com largura de referência 1920. Sem texto sobreposto. Brasil usa `SceneLoader.CarregarCena("Brasil")` e bloqueio de carregamento repetido.
34. **Indisponível:** Chile e Argentina mostram `Mapa indisponível por enquanto.` e não carregam cena. O popup fecha automaticamente após 2,5 segundos de tempo sem escala.
35. **Voltar América do Sul:** área `(45,820,260,82)` sobre o botão já desenhado na imagem; desativa América do Sul e reativa o Canvas Mapa-Múndi, sem carregar cena.
36. **Voltar Mapa-Múndi:** usa o PNG Voltar, ancorado no canto inferior esquerdo, posição `(150,75)` no Canvas 1920×1080 e largura 260. Retorna ao Menu, origem confirmada na inspeção do fluxo atual.

## Verificação e resultado

37. **Testes novos:** 128 verificações aprovadas em Play Mode real, 1920×1080, com segunda entrada em Play para a persistência. Incluem preparação/fallback/loop do vídeo, ordem de UI, raycasts, cliques e áudio único, navegação de países e voltas, singleton, EventSystems, Slider via ponteiro/arraste, Master dB, volume persistente, pausa, restauração de tempo/controles, música durante pausa e prioridade de Esc. Resultados completos em `testes.txt`; capturas `MenuVideo`, `Configuracoes`, `MapaMundi`, `AmericaDoSul` e `PausaBrasil` em PNG 1080p.
38. **Smoke tests:** entrada Brasil/Feira, transições das quatro áreas, feedback certo/errado e timeout dos quizzes, duas penalidades reais da Feira, captura correta sem erro, congelamento da queda, merge real, colisão inválida sem som, cadeia curta e congelamento da pilha durante pausa. Não foi executada a regressão antiga completa de física, recompensas, NPCs ou colisões.
39. **Falhas encontradas e corrigidas:** áudio de música inicialmente pausava porque `ignoreListenerPause` não foi serializado; inicialização agora aplica a opção às fontes. Reprodução do vídeo inicialmente ficava no primeiro frame; preparação e Play foram separados usando coroutine. A entrada Esc do controlador principal do Merge foi protegida. A altura do handle foi ajustada a 42 pixels. A execução final tem zero falhas. Um aviso nativo de cor do MP4 permanece: `Color primaries 0 is unknown or unsupported by WindowsMediaFoundation`. O decodificador usa a interpretação padrão e avisa sobre possível diferença de cor. Não alterei os metadados do vídeo, conforme a restrição do pedido.
40. **Console final:** 0 erros, 1 aviso nativo de vídeo, 0 mensagens na captura final (`console.txt`). Editor devolvido ao MapaMundi, fora do Play Mode e sem cena pendente de salvar (`estado-editor.txt`). Compilação concluída. Não foi produzido nem executado um build Standalone.

`ProgressionManager`, `PlayerMovement`, cenas Brasil e MinhaQuest e os dados de perguntas/recompensas não foram alterados. O progresso real armazenado antes e depois dos testes é idêntico (`progresso-real.txt`). Não houve novo sistema de save. A comparação de escopo está em `auditoria-escopo.json`.

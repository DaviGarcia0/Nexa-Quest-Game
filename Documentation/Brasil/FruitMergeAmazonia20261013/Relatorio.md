# Fruit Merge da Amazônia — entrega

Checkpoint inicial: `6fbd216`, criado após Play Mode e toda a suíte existente. Baseline real: 914 PASS, 0 FAIL, Console 0 erros / 0 avisos.

## Resultado e como jogar

Na cena Brasil, fale com a Guardiã (NPC 1 da Amazônia), abra Desafios e escolha Minijogo. As instruções abrem gratuitamente. Começar Jogo cobra 20 Energia. Mova o mouse horizontalmente e clique dentro da caixa para soltar; A/D ou setas também movem, Espaço solta. ESC e Sair retornam aos desafios em qualquer estado. Fechar os desafios devolve os controles do mapa.

Dois iguais evoluem pela cadeia Açaí → Guaraná → Castanha → Cupuaçu → Sapo → Arara → Boto → Onça. Cada fusão dá exatamente +1 NexaCoin imediatamente. Duas Onças continuam colidindo e não dão outra evolução ou Coin.

Criar Cupuaçu, Arara e Onça na mesma rodada conclui a missão: +50 Conhecimento, registro persistente e a mesma Chave da Amazônia, caso ainda não exista. Continuar Jogando preserva as peças, posições, velocidades, fila, Coins e objetivos. Novas fusões continuam dando Coins; EXP não se repete naquela rodada.

## Arquitetura e objetos

`Assets/Projeto/Scenes/Brasil.unity` ganhou somente o root `MinijogoAmazoniaRoot`, inicialmente desativado. Ele tem controlador, Canvas Screen Space Overlay, GraphicRaycaster e Canvas Scaler Scale With Screen Size, referência 1920×1080, match 0,5.

Hierarquia persistente:

- Instrucoes/Arte: botões transparentes Comecar e Voltar; nenhum texto duplicando o PNG.
- HUD: HUDQuest/QuestPortrait, HUDProximo/NextItemImage, HUDCoins/CoinsRodadaText/NexaCoinOficial, HUDObjetivos (Cupuaçu, Arara, Onça e checks), LinhaPerigo, BotaoSair, PopupCoin.
- ObjetivoComChave/Arte: Continuar e Sair.
- ObjetivoSemChave/Arte: Continuar e Sair.
- GameOver/Arte: JogarNovamente, Sair, CoinsDaRodada e RodadaEncerrada.

Ao iniciar, cria uma cena temporária com LocalPhysicsMode.Physics2D e `GameplayWorldAmazonia`: CameraMinijogo, BackgroundAmazonia, LeftWall, RightWall, Floor, DangerZone e ItemsRuntime. Peças e efeitos são GameObjects reais com SpriteRenderer. A câmera dedicada usa layer 30 e centro X=10000, orthographic size 5,4; a câmera principal e a física dos mapas continuam intactas. A simulação local avança pelo FixedUpdate, apenas enquanto Jogando. Pausar não altera Time.timeScale e os botões continuam funcionando.

## Scripts

Criados em `Assets/Projeto/Scripts/Brasil/`:

- MinijogoAmazoniaController.cs: estados, cena física, entrada/saída, fusão/rewards, objetivos e pausa; também contém o efeito breve AmazoniaMergeEffect.
- AmazoniaMergeItem.cs: corpo, collider, nível e proteção IsMerging; configuração serializável EvolucaoAmazonia.
- AmazoniaItemSpawner.cs: Atual/Próximo, movimento, drop e cooldown.
- AmazoniaDangerZone.cs: ocupação por peça solta e temporização.
- AmazoniaMinigameUI.cs: HUDs, painéis, retrato, popup e aviso compartilhado.

Criados em `Assets/Projeto/Editor/`: MontagemFruitMergeAmazonia.cs e ValidacaoFruitMergeAmazonia.cs.

Modificados: PainelDesafiosAmazonia.cs (referência e abertura do minijogo), ProgressionManager.cs (campo/getter/marcação/persistência de minijogoAmazoniaConcluido) e ValidacaoQuizAmazonia.cs (substitui a expectativa antiga de “em desenvolvimento” por instruções gratuitas e retorno). Runtime do Quiz da Amazônia e da Feira, minijogo da Feira, dados, animações globais, diálogos, NPCs, Player, Portal e ProjectSettings não foram alterados.

## Artes e importação

Pasta: `Assets/Projeto/Sprites/Brasil/FruitMergeAmazonia/`. Os 21 PNGs fornecidos foram copiados sem alterar bytes; hashes e resoluções em auditoria.json. Nomes organizados: Instrucoes, Cenario, Nivel1..Nivel8, Objetivos, Proximo, QuestMoldura, LinhaPerigo, Coins, CoinPopup, ObjetivoComChave, ObjetivoSemChave, GameOver, Sair e Fusao.

Quest Idle reutiliza os seis sprites de QuestExpressaoHUD já existentes. QuestComemoracao.png é uma cópia idêntica da arte QuestExpressaoComemoracaoHUD, dividida em oito frames. CoinOficial.png é uma cópia idêntica de HUD_NEXACOIN.png; a região da moeda oficial é usada com máscara no indicador da rodada. Nenhuma importação global foi modificada.

Sprite, Point, None/Uncompressed, sem mipmaps, NPOT None, alpha FromInput, PPU 100, FullRect e sem fallback de física. Max Size 8192 para preservar integralmente todos os arquivos. Margens de alpha residual dos corpos e do HUD de objetivos foram excluídas apenas dos SpriteRects, com margem de 4 pixels; os PNGs continuam intactos. Nenhum chroma-key de preto foi aplicado.

**Resolução recebida do cenário: 1672×941.** Embora descrito como 4K no pedido, o arquivo disponível não é 4K. Foi preservado sem redução, geração de substituto ou upscale do PNG, e compõe a tela 16:9. As demais dimensões estão na auditoria.

## Física e medidas

Unidades relativas ao centro da caixa (X global 10000): parede esquerda interna -5,9; direita +5,9; chão -3,42; spawn Y=3,25. BoxCollider2D estáticos: esquerda em (-6,0), direita em (+6,0), tamanho (0,2; 8); chão em (0;-3,52), tamanho (12,2;0,2). Topo aberto.

Cada peça tem CircleCollider2D no corpo principal, sem usar folhas, bicos ou transparências como extensão física. Transform da peça tem escala 1; a escala abaixo é do filho Sprite, considerando PPU 100 e seu SpriteRect recortado. Centro visual Y é um ajuste relativo ao diâmetro, aplicado somente ao desenho.

| Evolução | Diâmetro | Raio collider | Escala Sprite | Centro visual Y |
|---|---:|---:|---:|---:|
| 1 Açaí | 0.800 | 0.400 | 0.19608 | 0.00 |
| 2 Guaraná | 0.920 | 0.460 | 0.11736 | -0.08 |
| 3 Castanha | 1.056 | 0.528 | 0.11094 | -0.02 |
| 4 Cupuaçu | 1.216 | 0.608 | 0.12905 | -0.04 |
| 5 Sapo | 1.392 | 0.696 | 0.11581 | 0.00 |
| 6 Arara | 1.600 | 0.800 | 0.17443 | -0.02 |
| 7 Boto | 1.840 | 0.920 | 0.19484 | 0.00 |
| 8 Onça | 2.080 | 1.040 | 0.18890 | -0.02 |

PhysicsMaterial2D `AmazoniaFruitMerge.physicsMaterial2D`: friction 0,45, bounciness 0,04. Rigidbody2D Dynamic ao soltar; Gravity Scale 1, drag 0,12, angular drag 0,35, Collision Detection Continuous, interpolation Interpolate. Massa cresce com o quadrado do diâmetro relativo. Preview Kinematic, collider desabilitado. Não utiliza Rigidbody2D em elementos de UI.

## Sorteio, merge e efeitos

Atual e Próximo são sorteados independentemente em 1..3. Próximo vira Atual após drop, seguido de novo sorteio. O preview acompanha mouse ou teclado, limitado por raio e paredes. Cooldown de 0,45 s e verificação de ocupação da zona de spawn evitam duplicação e sobreposição no lançamento.

Contato físico Enter/Stay chama TentarFundir. Valida dono, estado Jogando, duas peças distintas soltas, nível igual abaixo de 8 e IsMerging false. Bloqueia ambas antes de remover; calcula ponto e velocidade média; desativa colliders/corpos; remove da lista e destrói; cria exatamente uma peça seguinte no ponto médio. A velocidade média é limitada a 8 e o giro a ±120 para manter a pilha estável. Pequeno impulso para cima de 0,06 produz o pop sem explosão.

Cada merge escreve +1 Coin pelo AddCoins existente e incrementa CoinsDaRodada somente para UI. Efeito de seis frames da primeira linha do sheet no ponto da fusão, 14 FPS, duração curta, largura aproximada 1,1 unidade. Popup +1 sobe e faz fade em 0,85 s. Quest comemora por aproximadamente 1 s; cadeias renovam o período sem reiniciar a fase da animação a cada colisão.

## Objetivos, EXP, chave e save

Flags da rodada registram a criação dos níveis 4, 6 e 8; não exigem manter a peça viva. Checks dinâmicos usam gráficos de UI, sem depender de caractere ausente na fonte. Na primeira conclusão da rodada, MissaoConcluidaNaRodada é marcada antes das rewards: ConquistarChaveAmazonia(), AddXP(50), MarcarMinijogoAmazoniaConcluido(). Não há save paralelo de produção.

O mesmo JSON v1 de ProgressionManager recebeu minijogoAmazoniaConcluido. Em saves antigos, o campo ausente assume false. Coins, XP, Level e as duas chaves continuam no mesmo save. A chave compartilhada continua única: se veio do Quiz ou de uma rodada anterior, ConquistarChaveAmazonia retorna false e aparece o painel SEM chave. Caso contrário, painel COM chave, com pedra verde. Nova rodada pode dar mais 50 EXP; continuar a mesma rodada não repete EXP.

## DangerZone, Game Over e saídas

Trigger BoxCollider2D cobre toda a largura interna entre Y=2,4 e 4,5. A faixa vermelha visível foi alinhada ao início real do trigger, conferido com diferença inferior a 1 pixel em 1080p. Apenas peças soltas contam. Temporização por peça: sair, ser removida/fundida ou desaparecer reseta sua entrada. Preview não participa. Uma ocupação persistente atinge 1,5 s de simulação antes de Game Over; passagem breve não perde. Pausas congelam a física e esse timer.

Game Over mantém Coins e mostra CoinsDaRodada no espaço vazio. Se a missão já foi concluída, cobre somente a faixa original com RODADA ENCERRADA. Jogar Novamente exige outros 20 Energia e limpa peças, fila, objetivos da rodada, Coin counter, DangerTimer, efeitos e estado da Quest. Não altera Coins/EXP/Level/chaves/flags persistentes. Sem Energia, permanece em Game Over com o aviso existente.

Sair antes da conclusão mantém Coins, sem EXP final ou chave e sem reembolso. Depois da conclusão mantém todas as rewards. Sair e ESC retornam ao PainelDesafiosAmazonia; fechar o painel devolve o Player. Corpos e cena temporária são removidos, aviso de Energia devolvido ao pai original e HUDs globais restaurados.

## Validação final

**1037 PASS, 0 FAIL.** Fruit Merge: 123; regressão anterior: 914. Play Mode real 1920×1080. Console final 0 erros, 0 avisos. Seu progresso real foi preservado; os testes reutilizam o save de testes já existente do projeto. Brasil salva, Dirty False, fora do Play Mode.

Contagem por suíte: {'fruit-merge': 123, 'quiz-amazonia': 191, 'npcs-amazonia': 82, 'quiz-feira': 134, 'minijogo': 70, 'painel': 65, 'energia': 67, 'visual': 100, 'npcs': 94, 'portal': 111}.

Cobertura: entrada/voltar/ESC, Energia 0/19/20 e duplo clique, cliques via EventSystem/raycast nos locais desenhados, preview/limites/cooldown, quedas rápidas, paredes/chão, colisão entre peças, empurrão/rolamento, crescimento das oito peças, sete fusões por contato real, Onça sem evolução, merge triplo sem duplicação, reação em cadeia, popup/fade/Quest, Coins imediatas e saída antecipada, objetivos persistentes, +50 e Level, pausa/retomada, Game Over antes/depois da missão, replay, chave já obtida pelo Quiz e save retrocompatível. Toda a suíte anterior foi executada em seguida.

Capturas: Instrucoes, EnergiaInsuficiente, FisicaPilha, ObjetivoComChave, ObjetivoSemChave, RodadaBonus, GameOver e GameOverPosObjetivo. Conferência visual dos HUDs, botões, número de Coins e faixa de fim de rodada.

A auditoria de componentes da cena identificou **zero objetos originais removidos**. Apenas três blocos originais mudaram: referência minijogo no painel, nova flag no ProgressionManager e lista de roots para incluir MinijogoAmazoniaRoot. Câmera, mapas, colliders, NPCs e outros componentes originais são idênticos ao checkpoint, desconsiderando somente a numeração automática de ordem dos roots. Relatórios completos em antes/ e depois/; parâmetros editáveis no componente MinijogoAmazoniaController e nas classes do minijogo.

# Brasil — mapas, transições e Portal

## Qualidade e fidelidade

Os mapas existentes e os fornecidos em Downloads são arquivos idênticos (comparação SHA-256). Feira, Amazônia, Ponto Turístico, Portal desligado e Portal ligado têm **1672 × 941 pixels**. Permanecem em world space, escala 1 e **100 PPU**. O tamanho do mundo de cada imagem é 16,72 × 9,41 unidades.

Importação conferida: Sprite/Single, Point, sem compressão, sem mipmaps, sem redimensionamento NPOT, Max Size padrão **4096**, sem overrides de plataforma que reduzam a imagem. O limite 4096 não inventa pixels: a textura continua 1672 × 941. As configurações dos mapas já estavam em grande parte corretas.

A câmera mantém Pixel Perfect, Assets PPU 100, referência 960 × 540 (ampliação inteira 2× em 1920 × 1080), Pixel Snapping e Windowbox. O filtro final foi explicitamente configurado como **Point**, em vez de RetroAA. Não se alterou enquadramento, escala ou posição dos mapas para esconder a baixa resolução. A suavização de movimento da câmera foi preservada.

**Não foi obtido um mapa de resolução superior com fidelidade suficiente.** Uma tentativa de restauração da Feira com a ferramenta integrada de edição de imagens manteve o arranjo geral, mas reinterpretou detalhes de vegetação, produtos e objetos. Além disso, retornou 1672 × 941, sem aumentar a resolução. Foi rejeitada e não entrou nos assets. Nenhum mapa original foi substituído. Os arquivos originais limitam a definição sob ampliação; este trabalho não elimina esse limite.

Prompt da tentativa: restaurar/ampliar a imagem fornecida da Feira para 3344 × 1882, preservando posições, dimensões, silhuetas, ruas, barracas, construções, vegetação, caminhos, água, cores e composição; sem cortes, novos objetos ou mudanças geométricas, pois as colisões dependem do alinhamento original. Ferramenta: image_gen integrada, sem CLI.

## Passagens

Os nomes reais encontrados foram preservados, inclusive `TrocaPontoTurisco`.

| Origem / gatilho | Destino / SpawnPoints |
|---|---|
| Feira / Colisoes_Manuais / TrocaAmazonia | Amazonia / EntradaFeira |
| Feira / Colisoes_Manuais / TrocaPortal | Portal / EntradaSul |
| Amazonia / Colisoes_Manuais / TrocaFeira | Feira / EntradaAmazonia |
| Amazonia / Colisoes_Manuais / TrocaPontoTurisco | PontoTuristico / EntradaAmazonia |
| PontoTuristico / Colisoes_Manuais / TrocaAmazonia | Amazonia / EntradaTurismo |
| Portal / Transitions / RetornoFeira | Feira / EntradaPortal |

As cinco saídas existentes receberam MapTransition e Is Trigger. Posição, tamanho, offset, nome, hierarquia e demais configurações dos colliders existentes foram preservados. O novo retorno é somente um trigger de transição. Nenhuma colisão de cenário foi gerada.

Selecione o gatilho e altere **Destination Area** e **Destination Spawn Point** no MapTransition. O spawn deve ser filho da área de destino. Ajuste os Transforms em `Maps/<Area>/SpawnPoints` para mudar a chegada. Os spawns antigos não foram movidos. Os novos pontos ficam afastados dos gatilhos de retorno.

BrazilWorld mantém fade de 0,25 s por etapa, bloqueio de movimento e rejeição de transições simultâneas. O cooldown existente de 0,2 s foi preservado. PlayerMovement considera também as saídas ativas no limite de movimento, permitindo alcançar os triggers que você colocou além da borda da imagem. A câmera continua limitada ao mapa.

## Portal e painel

`Maps/Portal` contém Mapa, SpawnPoints, Colisoes_Manuais, Transitions e InteracaoPortal. Começa inativo enquanto Feira é a área atual. O SpriteRenderer usa `Portal.png`, desligado.

`Maps/Portal/InteracaoPortal` possui BoxCollider2D trigger de proximidade e PortalInteraction. Aproximar mostra `E - Interagir`; apertar E abre `Canvas/PortalKeysPanel`, sempre com o sprite do painel vazio. E novamente ou Esc fecha. Enquanto aberto, a Quest fica bloqueada. A indicação desaparece ao sair da proximidade. O FadePanel foi preservado e fica acima dos demais elementos.

O painel vazio tem 1448 × 1086; o cheio, 1402 × 1122. Ambos são importados sem compressão, Point e Max Size 4096. Não se tentou intercambiá-los como três peças individuais: os layouts das duas imagens têm diferenças.

PortalInteraction guarda apenas referências `Activated Portal` e `Filled Keys Panel` para uso futuro. Não há aquisição de chaves, estados falsos de progresso nem código de desbloqueio. Para indicar uma chave por vez futuramente, será necessário criar três indicadores independentes e conectá-los ao sistema real de recompensas. A troca futura do mapa poderá atribuir o sprite ligado ao SpriteRenderer; antes disso, conferir as diferenças visuais entre os dois originais fornecidos.

## Arquivos

Criados:
- Assets/Projeto/Scripts/Brasil/PortalInteraction.cs — em Maps/Portal/InteracaoPortal.
- Assets/Projeto/Editor/BrasilPortalSetup.cs — ferramenta de configuração no Editor.
- Assets/Projeto/Editor/BrasilPortalValidation.cs — verificação em Play Mode; não entra no build.
- Assets/Projeto/Sprites/Brasil/Portal/ — quatro PNGs originais fornecidos e metadados.
- Documentation/Brasil/EtapaPortal/ — relatório, comparação das colisões e evidências.

Alterados:
- Brasil.unity — Portal, painel, referências de passagens e filtro final da câmera.
- BrazilWorld.cs — usa limite de movimento que inclui saídas.
- PlayerMovement.cs — cálculo do limite por área e suas saídas ativas.
- BrasilEditorJob.cs — comandos locais para configurar/testar esta etapa.
- BrasilValidation.cs — o menu de teste existente encaminha para a validação atual quando a cena contém Portal.
- Feira.png.meta — normalização dos overrides redundantes de plataforma.

MapTransition e o sistema de fade existentes foram reutilizados. Quest, Animator, velocidade, Menu, MapaMundi e MinhaQuest não foram refeitos.

## Validação

Consultar `playmode.txt` e as capturas desta pasta. O teste executa física real dos triggers e o mesmo motor de movimento da Quest. E/Esc são exercitados pelo mesmo método chamado pela leitura do teclado; não é uma simulação de teclas físicas. A preservação dos colliders também foi comparada com a cena salva antes das alterações, independentemente da montagem.


Resultado final: 111 verificações aprovadas, 0 falhas. Foram testadas as seis passagens caminhando até os triggers, fade, bloqueio, cooldown, spawns fora das saídas, limites da câmera, movimento normalizado, direção do Idle e painel do Portal. Console final: 0 erros, 0 avisos e 0 mensagens. As exceções intermediárias da ferramenta de montagem e do teste foram corrigidas antes desta execução final.

A cena foi deixada fora de Play Mode. Menu, MapaMundi e MinhaQuest permanecem sem diferenças. Nenhum arquivo de mapa original teve seu conteúdo alterado.


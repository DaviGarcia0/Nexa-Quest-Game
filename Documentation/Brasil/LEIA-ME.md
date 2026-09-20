# Brasil - exploração 2D

Abra `Assets/Projeto/Scenes/Brasil.unity` e pressione Play. Movimento: WASD ou setas. Diagonais têm a mesma velocidade total; nas diagonais a pose horizontal tem prioridade. Ao soltar as teclas, a Quest mantém a direção do último movimento.

## Cena e scripts

- `Main Camera`: CameraFollow e Pixel Perfect Camera do URP que já estava instalado. Acompanha a Quest, suavização 0,06 s, limites do mapa ativo.
- `GameManager`: BrazilWorld. Referências das três áreas, área/spawn inicial e duração do fade (0,25 s por etapa).
- `Player/Quest`: PlayerMovement, Animator, SpriteRenderer, Rigidbody2D e CapsuleCollider2D pequeno nos pés. Gravidade zero; rotação Z travada; material físico sem atrito.
- `Maps/Feira`, `Maps/Amazonia`, `Maps/PontoTuristico`: BrazilArea. Cada área possui Mapa (SpriteRenderer), SpawnPoints/EntradaSul, Colisoes_Manuais (vazio) e Transitions.
- `Canvas/FadePanel`: imagem preta com CanvasGroup. Somente o fade usa UI; nenhum mapa está em Canvas.
- `Maps/<area>/Transitions/Saida_MODELO_DESATIVADA`: MapTransition e BoxCollider2D marcado como trigger. Todos os modelos começam desativados. Não são colisores do cenário.

Os scripts de gameplay estão em `Assets/Projeto/Scripts/Brasil/`. As ferramentas em `Assets/Projeto/Editor/` servem apenas para montagem e validação; não entram no build. Não é necessário executar a montagem para jogar uma cena já preparada. O comando de montagem se recusa a sobrescrever a cena se já encontrar BrazilWorld.

## Ajustes pelo Inspector

Selecione `Player/Quest`:

- **Move Speed**: velocidade em unidades por segundo; inicial 2 (200 pixels do mapa por segundo).
- **Walk Animation Speed** e **Idle Animation Speed**: multiplicadores independentes. 1 = original, 0,5 = metade, 2 = dobro.
- **Area Bounds**: preenchido pelo GameManager na troca de área. É somente um limite retangular de posição para impedir sair da imagem; não impede atravessar árvores, água ou construções.

Para mudar o nascimento, mova `Maps/Feira/SpawnPoints/EntradaSul` com a ferramenta Move. Também é possível trocar **Initial Area** e **Initial Spawn** no GameManager. O spawn deve ser um Transform descendente da área escolhida. Os spawns da Floresta e do Ponto Turístico também são ajustáveis dessa forma.

## Animator

Arquivo: `Assets/Projeto/Animations/Quest/Quest.controller`.

Oito estados: IdleDown, IdleLeft, IdleRight, IdleUp, WalkDown, WalkLeft, WalkRight, WalkUp. Padrão: IdleDown. Transições imediatas de Any State, sem espera de Exit Time, sem transição para o próprio estado.

Parâmetros:

| Nome | Tipo | Uso |
| --- | --- | --- |
| Direction | Int | 0 baixo, 1 esquerda, 2 direita, 3 cima; conserva o valor ao parar |
| Speed | Float | Magnitude ao quadrado do movimento; zero parado |
| WalkRate | Float | Multiplicador da caminhada |
| IdleRate | Float | Multiplicador do Idle |

Walk: oito frames por direção, 10 fps, loop. Idle: quatro frames por direção, 4 fps, loop. As curvas possuem uma chave final repetindo o primeiro frame para completar a duração do último. Para alterar o tempo base, abra o clip `.anim` na janela Animation e ajuste Samples/posição das chaves; normalmente basta mudar os multiplicadores no PlayerMovement. Os scripts sobrescrevem WalkRate e IdleRate a cada atualização com esses campos do Inspector.

## Escala e nitidez

Os mapas originais têm 1672 × 941 pixels, PPU 100, Transform Scale 1: ocupam 16,72 × 9,41 unidades. Não foram reduzidos para caber na tela. A câmera usa base lógica 960 × 540, ampliada em fator inteiro para a saída de referência 1920 × 1080. Mostra aproximadamente 960 × 540 pixels do mapa nessa resolução. Em outras proporções pode haver barras para preservar a grade.

Point, sem compressão, sem mipmaps, sem redução de textura. Câmera sem pós-processamento, HDR ou antialiasing. Material unlit compatível com o URP existente.

A arte de caminhada tem aproximadamente 181-198 pixels de altura útil e o Idle 253-273. Por isso as cópias usam PPU 400 e 550, respectivamente, deixando a Quest em torno de 46-50 pixels de altura na escala do mapa, coerente com as portas e barracas. Essa diferença evita que ela aumente ao parar. Os PNGs de origem são ilustrações de aparência pixel art com transparências e variações de desenho; Point preserva a amostragem, mas não transforma a arte em uma grade de pixels uniforme nem elimina diferenças já desenhadas.

## Sprite sheets e recortes

Originais preservados em `Assets/Projeto/Sprites/Quest/Sprite Sheets/`. Cópias usadas nas animações em `Assets/Projeto/Sprites/Quest/Organizados/QuestWalk.png` e `QuestIdle.png`, ambas Sprite Multiple.

A caminhada não permite uma grade 8 × 4 diretamente sobre a imagem original: há margens superiores/inferiores e silhuetas que invadem a faixa horizontal dos vizinhos. Foram separadas as silhuetas usando seus pixels conectados; pixels periféricos foram atribuídos à silhueta mais próxima. Todos os pixels não transparentes foram copiados uma única vez, preservando RGBA. Nenhum frame foi redesenhado ou espelhado.

As células têm dimensões constantes dentro de cada sheet, espaço suficiente para as margens transparentes e pivot customizado igual dentro do sheet. A referência vertical é a base dos pés; o alinhamento horizontal usa o centro superior da personagem para não oscilar com a alternância das pernas. Isso equivale a uma referência inferior consistente, compensando as margens. Os retângulos e pivots estão registrados nos JSONs ao lado das texturas. As variações de pose, proporção e transparência já existentes na arte permanecem.

O script `Documentation/Brasil/organizar_sprites.py` registra a separação. Não é necessário executá-lo para abrir ou jogar o projeto. Depende de Python, Pillow e NumPy somente para reproduzir a preparação da arte.

## Colisores manuais

Nenhum colisor foi gerado nos mapas, árvores, água, construções ou outros elementos. Para fazê-los, crie objetos vazios em `Maps/<area>/Colisoes_Manuais`, adicione BoxCollider2D, CapsuleCollider2D ou outro Collider2D apropriado e ajuste cada um manualmente. Deixe **Is Trigger** desmarcado nos obstáculos sólidos. Não precisa de Rigidbody2D nos obstáculos estáticos: a Quest já tem um Rigidbody2D dinâmico. A pasta acompanha a ativação/desativação da área.

Como cada mapa é uma imagem única, a Quest aparece na frente da imagem inteira. Oclusão atrás de copas/telhados exigirá separar esses elementos posteriormente; não foi alterada a arte nesta etapa.

## Criar passagens

1. Duplique `Saida_MODELO_DESATIVADA` na pasta Transitions da área de origem e renomeie.
2. Posicione o objeto numa saída caminhável, dentro dos limites da imagem. Ajuste manualmente o tamanho do BoxCollider2D; mantenha Is Trigger marcado.
3. No MapTransition, mantenha **World = GameManager**, escolha **Destination Area** arrastando o componente da área de destino e **Destination Spawn Point** arrastando um Transform dentro de SpawnPoints dessa área.
4. Ative o objeto da saída depois de posicioná-lo.
5. Para voltar, crie outro objeto na área oposta, com destino e spawn invertidos. Para conectar Floresta aos dois mapas, ela precisa de duas saídas.

Novas entradas: crie um Transform filho de SpawnPoints e dê um nome claro, como EntradaNorte. Não precisa mudar código. Não coloque o spawn dentro do trigger de retorno. O sistema evita transições simultâneas e aplica um pequeno intervalo após o fade, mas posicionar as entradas fora dos triggers continua sendo a configuração correta.

Fluxo: bloqueia controles → fade preto → ativa destino/desativa outras áreas → reposiciona Quest e câmera → fade transparente → libera controles. Toda a operação ocorre dentro de Brasil, sem alterar Menu/MapaMundi ou instalar pacotes.

## Validação

O menu `Nexa Quest > Testar Brasil` executa verificações em Play Mode e sai do Play ao terminar. O relatório fica em `Documentation/Brasil/Validacao/playmode.txt`, acompanhado das capturas da câmera. Os testes exercitam o motor de movimento usando o mesmo FixedUpdate e Animator do jogo, além de um trigger temporário para testar a transição; esse objeto não é salvo na cena.

O resultado final e a comparação dos arquivos preservados são registrados separadamente após os testes. A imagem fornecida inicialmente já mostrava um MissingReferenceException de TextureImporter; não é um script de exploração. Erros/avisos surgidos durante a execução de validação são capturados no relatório.
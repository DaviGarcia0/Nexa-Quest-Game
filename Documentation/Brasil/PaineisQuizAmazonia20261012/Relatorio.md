# Painéis inicial e final do Quiz da Amazônia

Checkpoint antes das alterações: `bf4a023`. Baseline: 891 verificações aprovadas, nenhuma falha e Console sem erros/avisos.

## Integração

- Assets: `Assets/Projeto/Sprites/Brasil/QuizAmazonia/PainelInicialQuizAmazonia.png` e `PainelFinalQuizAmazonia.png`. Cópias byte a byte dos PNGs enviados, confirmadas por SHA-256; sem edição da arte. Importação Sprite Single, Point, sem compressão/mipmaps, Full Rect.
- Cena: `Assets/Projeto/Scenes/Brasil.unity`, em `QuizAmazonia/PainelInicial/ArtePainel` e `QuizAmazonia/PainelFinal/ArtePainel`. Painel principal e feedbacks existentes preservados. Botão antigo de retorno do rodapé foi substituído pelos controles dos novos painéis.
- Referência efetivamente consultada: InterfaceQuizFeira da mesma cena. Largura inicial 1040 e final 1340; posição (0, -95), âncoras/pivot centralizados e escala 1. Altura acompanha a proporção de cada PNG. Caption real abaixo da arte segue a solução da Feira.
- TextoExplicativo central, TMP com ajuste automático entre 22 e 24. Botões transparentes alinhados sobre Começar Quiz, Voltar e Continuar. Sem TMP duplicando recompensas dentro da arte final.

## Fluxo e preservação

Guardiã → Desafios → Quiz → Inicial (gratuito) → Começar (20 Energia) → três perguntas → feedback final → conclusão → Continuar → Desafios.

Abrir, Voltar e ESC na tela inicial não gastam Energia. Com 0 ou 19 Energia, Começar mantém a tela inicial e mostra o aviso existente. Clique repetido durante a pergunta não cobra novamente. ESC e bloqueios/restauração do HUD seguem o fluxo existente.

Perguntas, alternativas, timer de 30 segundos, feedback de 1,2 s mais 0,3 s de fade, 10 NexaCoins por acerto, 50 Conhecimento e chave única preservados. O trecho do controlador a partir de CarregarPergunta permanece idêntico ao checkpoint. ProgressionManager, dados, Feira, minijogo, NPCs, mapas, câmera e colliders não foram alterados. A comparação dos objetos já existentes da cena identificou somente mudanças de hierarquia/referências da interface do Quiz.

O PNG final tem +30, +50 e +1 incorporados na imagem. No replay, esses números continuam na arte; o resumo abaixo mostra as recompensas reais da rodada e “Chave da Amazônia já obtida”. Isso segue a solução da Feira e evita conceder outra chave. Não houve edição do PNG.

## Validação final

Play Mode real 1920×1080. 914 verificações PASS, 0 FAIL. Console: 0 erros, 0 avisos. Progresso real preservado. Brasil salva, Dirty False e fora do Play Mode. Conclusão da regressão: 2026-10-12 17:08:13.

Contagens: {'quiz-amazonia': 191, 'npcs-amazonia': 82, 'quiz-feira': 134, 'minijogo': 70, 'painel': 65, 'energia': 67, 'visual': 100, 'npcs': 94, 'portal': 111}.

Capturas inicial, primeira conclusão, replay e energia insuficiente conferidas visualmente; texto sem overflow. Relatórios completos em antes/ e depois/.

## Arquivos de código alterados

QuizAmazonia.cs, InterfaceQuizAmazonia.cs e PainelDesafiosAmazonia.cs (entrada/estados/controles e resumo); MontagemQuizAmazonia.cs (integração e ajuste do texto); ValidacaoQuizAmazonia.cs (novas verificações e regressão existente). Cena Brasil, dois PNGs/metadados e evidências de teste completam a entrega.

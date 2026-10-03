Caminhada lateral da Quest — 03/10/2026

Corrigidos os clips WalkLeft e WalkRight. A sequência antiga tinha pouca variação de pernas/pés, especialmente olhando para a direita. Foram gerados novos desenhos laterais usando a Quest atual como referência, mantendo sua identidade e os arquivos originais.

- Novos sprites: `Assets/Projeto/Sprites/Quest/Design20261001/SideWalk20261003/WalkLeft.png` e `WalkRight.png`, com seus JSONs de recorte e metadados Unity.
- Alterados: somente `Assets/Projeto/Animations/Quest/WalkLeft.anim` e `WalkRight.anim` entre os assets de gameplay existentes. GUIDs e estados do Animator mantidos.
- Cada clip contém 8 poses, 10 FPS e loop de 0,8 s. Há poses de contato e de passagem, joelhos dobrados e pés levantados. Retirada a repetição extra do primeiro frame no final, que acrescentava uma pausa ao loop.
- Recortes 310 × 452, Point, sem compressão/mipmaps. PPU 948 mantém altura próxima de 0,477 unidades; pivôs compensam o alinhamento dos desenhos e deixam os pés na referência do chão. A escala do GameObject não foi alterada.
- Ferramenta de Editor criada: `Assets/Projeto/Editor/QuestSideWalkEditor.cs`, para importar e verificar os clips. Nenhum script de gameplay foi alterado.
- Para ajustar a cadência no Inspector: Quest → PlayerMovement → Walk Animation Speed (atualmente 1). Esse controle existente afeta todas as direções; os novos clips laterais ficam em `Assets/Projeto/Animations/Quest`.
- Preservados: Idle, caminhada para cima/baixo, Animator Controller, controles, velocidade de deslocamento, prefab, cena Brasil, mapas, colisões, diálogos e HUD.

Teste real em Play Mode, 1920 × 1080: **317 verificações aprovadas** (16 específicas da caminhada, 100 visuais, 90 de NPCs/progresso, 111 de exploração/portal). Confirmados os oito sprites de cada lado durante mais de dois ciclos, deslocamento real, escala, alinhamento dos pés e retorno ao Idle da última direção. Console final: **0 erros e 0 avisos**. Fora de Play Mode ao finalizar.

Uma margem de recorte ultrapassou o topo da textura na primeira importação. Os clips anteriores foram recuperados antes de corrigir o recorte; os oito sprites de cada lado foram reimportados e testados com sucesso. Nenhum PNG original foi sobrescrito.

Checkpoint anterior: `e12e576`.

A comparação `comparacao-unity.png` foi renderizada pela Unity: linha 1 = esquerda antiga; linha 2 = esquerda nova; linha 3 = direita antiga; linha 4 = direita nova. As capturas em jogo são `WalkLeft-play.png` e `WalkRight-play.png`. Método e prompts da geração: `Prompts.md`.

# Resultado da validação - 19/09/2026

Cena Brasil montada e salva no projeto Unity 2022.3.62f3. Execução real em Play Mode: **62 verificações aprovadas, zero falhas, zero erros/avisos capturados**. Console consultado pela API do Editor ao final: **0 erros e 0 avisos**.

Movimento cardinal e diagonal: todos percorreram 0,800 unidade nos mesmos 20 passos de física, a 2 unidades/s. Estados Walk e Idle nas quatro direções validados. Câmera, spawn, limites externos, referências, importação, ordem dos frames, loop, trigger, bloqueio de controle, fade e retorno à Feira validados. O teste alimenta o motor de movimento programaticamente; as teclas WASD/setas usam os eixos Horizontal/Vertical preexistentes do Input Manager, que foram inspecionados e preservados.

Capturas em Play Mode confirmaram posição visível da Quest em terreno livre na Feira, entrada da Amazônia e praça do Ponto Turístico. Mapas maiores que o enquadramento. Nenhum colisor de cenário criado. Os três triggers de exemplo estão desativados e devem ser posicionados antes de ativar.

42 arquivos existentes verificados por SHA-256 permaneceram idênticos: Menu, MapaMundi, MinhaQuest, SceneLoader, manifest de pacotes, configurações globais e imagens originais. O Git já tinha alterações do usuário em Menu/MapaMundi e mudança da pasta Images para Sprites antes desta tarefa; elas não foram revertidas nem editadas.

## Arquivos alterados nesta tarefa

- Assets/Projeto/Scenes/Brasil.unity
- Assets/Projeto/Sprites/Brasil/Mapa/Feira.png.meta
- Assets/Projeto/Sprites/Brasil/Mapa/Amazonas.png.meta
- Assets/Projeto/Sprites/Brasil/Mapa/Ponto Turisitico.png.meta

As três imagens de mapa e os sheets originais não foram modificados. Nos mapas, somente a configuração de importação mudou.

## Arquivos criados

- Assets/Projeto/Scripts/Brasil/PlayerMovement.cs
- Assets/Projeto/Scripts/Brasil/CameraFollow.cs
- Assets/Projeto/Scripts/Brasil/BrazilArea.cs
- Assets/Projeto/Scripts/Brasil/BrazilWorld.cs
- Assets/Projeto/Scripts/Brasil/MapTransition.cs
- Assets/Projeto/Animations/Quest/Quest.controller
- Assets/Projeto/Animations/Quest/IdleDown.anim, IdleLeft.anim, IdleRight.anim, IdleUp.anim
- Assets/Projeto/Animations/Quest/WalkDown.anim, WalkLeft.anim, WalkRight.anim, WalkUp.anim
- Assets/Projeto/Prefabs/Brasil/Quest.prefab
- Assets/Projeto/Materials/Brasil/PixelArtUnlit.mat
- Assets/Projeto/Materials/Brasil/QuestFeet.physicsMaterial2D
- Assets/Projeto/Sprites/Quest/Organizados/QuestWalk.png e QuestWalk.json
- Assets/Projeto/Sprites/Quest/Organizados/QuestIdle.png e QuestIdle.json
- Assets/Projeto/Editor/BrasilSetup.cs, BrasilValidation.cs e BrasilEditorJob.cs (somente Editor)
- Documentation/Brasil/LEIA-ME.md, RESULTADO.md, organizar_sprites.py e relatórios/capturas em Validacao
- Arquivos .meta correspondentes gerados pela Unity.

## Observações sobre a arte

Foram feitos recortes e alinhamentos personalizados nas cópias autorizadas, em vez de Slice em grade sobre os originais. Todos os pixels não transparentes foram preservados com o mesmo RGBA: 716.928 na caminhada e 784.098 no Idle. Nenhum desenho foi gerado, redesenhado ou espelhado. Pivots consistentes por sheet e referência na base dos pés. Continuam presentes as variações de pose/proporção e os pixels parcialmente transparentes da arte original.

## Uso

Detalhes dos componentes, parâmetros, velocidades, spawns, colisores manuais e passagens: LEIA-ME.md. Para repetir a validação: Nexa Quest > Testar Brasil. Para jogar: abrir Brasil e apertar Play normalmente. Não é necessário executar Preparar Brasil outra vez.
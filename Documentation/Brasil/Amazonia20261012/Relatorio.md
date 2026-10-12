# Lia e NPCs da Amazônia — 12/10/2026

O diálogo inicial de Lia foi atualizado, e quatro NPCs foram adicionados à Amazônia nas posições do print. Apenas a NPC 1 é interativa. Os demais são decorativos com Idle.

## Segurança e validação

Projeto: `C:/Users/Davi/Desktop/Nexa-Quest-Game`, Unity 2022.3.62f3.

Checkpoint imediatamente anterior à implementação: **081e737**. Inclui o estado atual do Quiz, a quebra dos enunciados a cada sete palavras e o ajuste temporário de Energia solicitado anteriormente. Nenhum checkpoint histórico foi usado como rollback.

Baseline antes de alterar esta etapa: **635 verificações aprovadas**, zero falhas, erros ou avisos. Conclusão às 13:20:20. Relatórios em `antes/`.

Depois: **722 verificações aprovadas**, zero falhas, zero erros e zero avisos no Console. Regressão concluída às 13:32:05. Brasil salva e fora de Play Mode. O progresso real permanece idêntico, conforme `depois/progresso-real.txt`; os testes usam a persistência isolada já existente.

| Validação | Antes | Depois |
|---|---:|---:|
| Nova etapa: Lia/Amazônia | — | 81 |
| Quiz da Feira | 134 | 134 |
| Minijogo da Feira | 70 | 70 |
| Painel da Feira | 65 | 65 |
| Energia | 67 | 67 |
| Quest/HUD visual | 100 | 100 |
| NPCs/diálogos anteriores | 88 | 94 |
| Portal/transições | 111 | 111 |
| Total | 635 | 722 |

As seis verificações adicionais na suíte antiga decorrem das duas falas adicionais de Lia. Nenhum teste anterior foi removido.

A auditoria da cena contra o checkpoint encontrou **1.301 componentes anteriores preservados e 29 novos**. O único bloco anterior com conteúdo acrescentado é a lista de filhos da área Amazonia, para incluir NPCsAmazonia. Nenhum componente anterior de câmera, collider, Player, mapa, HUD, Portal ou desafio foi alterado. O viewport recalculado automaticamente pelo PixelPerfectCamera durante a serialização foi reposto ao valor exato do checkpoint. Conferência em `auditoria.txt`.

## Diálogo inicial

GameObject existente: `Feira/NPCs/NPC_Introducao_Lia`, ligado ao `NPCInteraction` e ao asset `Assets/Projeto/Dialogue/Feira_BoasVindas.asset`.

Lia foi identificada pelo objeto e pelo asset real. Bento usa outro asset, `Feira_Desafios.asset`, e permanece sem alterações. O retrato original de Lia e o retrato original da Quest são reutilizados, com o retrato de cada fala correspondente ao seu speaker. DialogueController, caixa, fonte, input E, animações e bloqueio de movimento continuam os mesmos.

Texto final, na ordem:

1. **Lia:** Oi! Você parece meio perdida. Chegou agora?
2. **Quest:** Cheguei! Eu sou a Quest. Escolhi o Brasil no mapa e vim parar aqui... ainda estou tentando entender como tudo funciona.
3. **Lia:** Prazer, Quest! Então seja bem-vinda ao Brasil. Você caiu num ótimo lugar para começar.
4. **Quest:** Eu queria conhecer melhor o país... mas também estou procurando uma forma de continuar minha viagem. Ouvi falar de um portal por aqui.
5. **Lia:** Tem sim. Só que ele não abre tão fácil. Você vai precisar conseguir três chaves.
6. **Quest:** Três chaves?
7. **Lia:** Isso. Existe uma em cada parte da sua jornada: uma na Feira, outra na Amazônia e outra no Ponto Turístico.
8. **Lia:** Começa por aqui mesmo. Procure o Bento. Ele tem um desafio para você e pode te ajudar a conseguir a primeira chave.
9. **Quest:** E depois?
10. **Lia:** Depois siga para a Amazônia e para o Ponto Turístico. Em cada lugar, procure o NPC responsável e complete o desafio.
11. **Lia:** Quando estiver com as três chaves, é só voltar ao Portal.
12. **Quest:** Entendi! Então, além de encontrar as chaves, vou conhecer um pouco mais do Brasil pelo caminho.
13. **Lia:** Exatamente. Boa sorte, Quest! O Bento está te esperando.

Todas as falas passaram na verificação de overflow em 1920 × 1080. Foram testados ordem, nome, retrato, entrada por proximidade/E, bloqueio durante diálogo e liberação ao concluir.

## NPCs e posições

Criados como filhos de `Amazonia/NPCsAmazonia`, dentro da cena Brasil existente. Cada objeto tem um filho `Visual`, com SpriteRenderer e Animator. O deslocamento do Player continua controlado pelo sistema atual.

Os pontos abaixo indicam os pés do NPC em pixels do mapa `Amazonas.png` (1672 × 941), com origem no canto superior esquerdo. A montagem transforma esses pontos para a posição correspondente no mundo, respeitando o mapa e seus pixels por unidade.

| GameObject | Sheet original | Posição no mapa | Local |
|---|---|---|---|
| NPC_Amazonia_01 | NPC 1 Amazonia.png | (1004, 183) | Entrada do templo, marca vermelha |
| NPC_Amazonia_02 | NPC 2 Amazonia.png | (407, 247) | Caminho à esquerda, pedras/vegetação |
| NPC_Amazonia_03 | NPC 3 Amazonia.png | (239, 379) | Cais à esquerda, junto à construção |
| NPC_Amazonia_04 | NPC 4 Amazonia.png | (1202, 156) | Caminho superior direito, próximo à ponte/cachoeira |

As posições foram verificadas em Play Mode e comparadas visualmente com o print. A captura `Amazonia-posicionamento-1080p.png` mostra os quatro NPCs. `Amazonia-templo-1080p.png` mostra a NPC 1 de perto, inteira e na entrada indicada.

## Sheets, recortes e Idle

Pasta: `Assets/Projeto/Sprites/Brasil/NPCsAmazonia/`. Nomes no projeto: `NPCAmazonia01.png`, `NPCAmazonia02.png`, `NPCAmazonia03.png`, `NPCAmazonia04.png`. Os quatro arquivos são idênticos em bytes aos PNGs enviados. Não houve redesenho, recoloração ou edição de pixels.

Importação: Sprite Multiple, Point, Compression None, Mip Maps Off, NPOT None, transparência preservada, limite 4096. Cada sheet tem seu JSON de recortes. `recortes.txt` lista todos os retângulos, pivôs e pixels por unidade; suas coordenadas Y usam a origem inferior da Unity.

| Sheet | Dimensões | Sprites recortados | Idle utilizado | PPU |
|---|---|---:|---|---:|
| NPC 1 | 2172 × 724 | 4 | Quatro poses frontais/piscar | 1022 |
| NPC 2 | 1916 × 821 | 4 | Quatro poses frontais/piscar | 1230 |
| NPC 3 | 1774 × 887 | 4 | Pose frontal com respiração leve | 1012 |
| NPC 4 | 1774 × 887 | 8 | Frente aberta/fechada, quadros 1 e 5 | 705 |

Todas as animações têm frameRate **3 FPS**, loop de **2,4 segundos** e pausas maiores na pose neutra. O NPC 3 não possui um segundo frame frontal de piscar: por isso usa o sprite frontal e uma oscilação vertical mínima no filho Visual, de 1 a 1,008, para respirar; não muda de direção. Os outros recortes direcionais dos NPCs 3/4 ficam disponíveis, sem fazer os personagens girarem durante o Idle.

Os recortes foram definidos individualmente a partir dos personagens visíveis, com margens para penas, cabelo, braços, pés e acessórios. Não se presumiu uma grade igual nos quatro sheets. A escala de cada sheet foi ajustada para manter os personagens próximos de 0,6 unidade de altura visível. Os pivôs ficam alinhados aos pés, evitando saltos ao piscar.

## Interação e diálogo da NPC 1

Nome exibido: **Guardiã da Amazônia**. É a personagem feminina do arquivo NPC 1, preservada exatamente como enviada.

Componentes na raiz:

- `BoxCollider2D` físico: tamanho (0,22; 0,10), offset (0; 0,03), concentrado nos pés.
- `CircleCollider2D` de interação: isTrigger true, raio 0,58, offset (0; 0,12).
- `NPCInteraction` existente: referência ao DialogueController atual e ao novo asset `Amazonia_Desafios.asset`.

DialogueData: `Assets/Projeto/Dialogue/Amazonia_Desafios.asset`. Retrato da Guardiã: primeiro sprite frontal original; retrato da Quest: o já utilizado nas conversas da Feira.

Falas:

1. **Guardiã da Amazônia:** Olá, viajante. Veio explorar a Amazônia?
2. **Quest:** Vim! Também estou procurando uma das chaves que preciso para abrir o Portal.
3. **Guardiã da Amazônia:** Então chegou ao lugar certo. Aqui você vai ter que provar o que aprendeu.
4. **Guardiã da Amazônia:** Posso te desafiar de duas formas. Em breve, vou te mostrar como.

Aproximar-se mostra o indicador E existente. E abre e avança a conversa. Ao concluir, o controle volta ao mapa, na mesma posição.

NPCs 2, 3 e 4 não têm NPCInteraction, DialogueTrigger, Collider de interação ou diálogo. Ao aproximar-se e pressionar E, não aparece indicação nem abre conversa/painel. Seguem o padrão decorativo dos NPCs da Feira, sem corpo físico novo.

## Painel da Amazônia — pendência confirmada

O usuário confirmou que **a arte ainda não foi feita**. Por isso, não foi criado um PainelDesafiosAmazonia com imagem inventada ou com a arte da Feira.

O evento existente `NPC_Amazonia_01 → NPCInteraction → On Dialogue Finished` está disponível para ligar futuramente a abertura desse painel. Atualmente não tem listener funcional: terminar a conversa libera o Player. Ainda não existem BotaoQuiz ou BotaoMinijogo da Amazônia, nem lógica de gameplay ou recompensas dessa região.

Nenhuma Chave da Amazônia foi implementada. A Chave da Vila, moedas, EXP, Energia, Quiz, Minijogo e lógica do Portal permanecem como estavam. O diálogo das três chaves é a preparação narrativa solicitada para as etapas posteriores.

## Arquivos desta etapa

Criados:

- `Assets/Projeto/Editor/MontagemNPCsAmazonia.cs` e .meta: importação, recortes, Idle, posicionamento e edição do conteúdo da conversa de Lia. A montagem recusa cena suja ou NPCs já existentes.
- `Assets/Projeto/Editor/ValidacaoNPCsAmazonia.cs` e .meta: testes de Play Mode e encadeamento da regressão anterior.
- `Assets/Projeto/Dialogue/Amazonia_Desafios.asset` e .meta.
- `Assets/Projeto/Sprites/Brasil/NPCsAmazonia/`: quatro PNGs originais, quatro JSONs e arquivos .meta.
- `Assets/Projeto/Animations/NPCsAmazonia/`: `NPC_Amazonia_01` até `04`, cada um com .anim e .controller, mais .meta.
- `Documentation/Brasil/Amazonia20261012/`: baseline, testes finais, auditoria, recortes, capturas e este relatório.

Modificados:

- `Assets/Projeto/Dialogue/Feira_BoasVindas.asset`: somente as falas, seus speakers e as referências corretas aos retratos existentes.
- `Assets/Projeto/Scenes/Brasil.unity`: raiz NPCsAmazonia e quatro personagens; nenhum objeto anterior removido.
- Relatórios/capturas de regressão: atualizados pela execução dos validadores anteriores.

Nenhum script de gameplay existente foi modificado nesta etapa.

## Evidências

`amazonia-playmode.txt`: 81 verificações da nova etapa. `depois/`: Quiz e seis suítes anteriores, Console e confirmação do progresso real preservado. As capturas de Lia, Quest e Guardiã foram revisadas visualmente.

A captura panorâmica desativa temporariamente o PixelPerfectCamera durante o teste para mostrar o mapa inteiro; essa mudança não é salva na cena. Ao retornar ao fluxo, a câmera e seu viewport são restaurados.

No teste inicial, a comparação de posição absolutamente exata após a conversa de Lia detectou uma acomodação física de 0,000161 unidade. A comparação usa a tolerância física de 0,03 já compatível com o padrão da suíte anterior; não houve alteração no movimento ou nas colisões do jogo. Os testes seguintes e a regressão completa passaram.
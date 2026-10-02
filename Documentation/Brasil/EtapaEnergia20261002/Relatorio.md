HUD de Energia e retratos da Quest — 02/10/2026

Concluído na cena Brasil, Unity 2022.3.62f3. Checkpoint anterior às alterações: `56da873`. O projeto em uso está em `C:/Users/Davi/Desktop/Nexa-Quest-Game`.

- **Importados:** `QuestExpressaoHUD.png`, `QuestExpressaoComemoracaoHUD.png` e `ExpressaoQuest.png` em `Assets/Projeto/Sprites/HUD/QuestExpressions/`. A arte `Assets/Projeto/Sprites/HUD/HUD_ENERGIA.png` já existia e foi reaproveitada. Os quatro PNGs são idênticos aos arquivos enviados, conferidos por SHA-256.
- **Criados:** `QuestHUD.controller` e `QuestHUD_Idle.anim` em `Assets/Projeto/Animations/HUD/`; metadados e `QuestExpressaoHUD.json` com os recortes; ferramentas de Editor `EnergyHUDSetup.cs` e `EnergyHUDValidation.cs`. Nenhum script novo executa lógica de Energia no jogo.
- **Alterados:** `Brasil.unity`; o campo Portrait da fala da Quest em `Feira_BoasVindas.asset` e `Feira_Desafios.asset`; configurações de importação em `HUD_ENERGIA.png.meta`; a verificação do collider em `QuestHUDVisualValidation.cs`.
- **GameObjects criados:** `Canvas/EnergyHUD`, com `Background`, `PortraitMask/QuestPortrait` e `EnergyText`. O Canvas existente recebeu esse novo filho; seus objetos anteriores permaneceram intactos. A HUD fica no alto à esquerda, 540 × 180,46, preservando a proporção do PNG.
- **Diálogo:** as duas falas com speaker Quest usam o sprite estático `ExpressaoQuest`. Textos, ordem, nomes, retratos de Lia/Bento e DialogueController foram preservados. O componente Portrait existente continua atualizando normalmente.
- **Idle:** seis frames de 370 × 505, pivô central igual, Point, sem compressão ou mipmaps, PPU 100 para a UI e Max Size 4096. Os contornos individuais da Unity separam bordas que se sobrepõem horizontalmente; o PNG permanece intacto. A comparação das malhas importadas não encontrou pixels opacos cortados nem pixels opacos dos vizinhos incluídos (alpha > 128).
- **Animator:** em `Canvas/EnergyHUD/PortraitMask/QuestPortrait`, separado do Player. Controller `QuestHUD`, um estado `QuestHUD_Idle`, sem parâmetros. Clip com Samples 12, loop de aproximadamente 4,08 s e pausas entre poses; o piscar dura aproximadamente 0,17 s. Para acelerar/desacelerar o conjunto, ajuste Speed desse estado; para alterar as pausas, edite os tempos das chaves no clip. Image usa o contorno do sprite e a máscara circular limita a exibição ao círculo preto.
- **Comemoração:** apenas importada, conforme opção 1. Slicing, clip e acionamento por vitória ficam para a próxima etapa.
- **Energia:** `Canvas/EnergyHUD/EnergyText` é um TextMeshPro separado, com `100` temporário. O valor pode ser editado no Inspector ou posteriormente por script. Nenhuma regra de consumo/recuperação foi implementada.

A validação em Play Mode, com Game View em 1920 × 1080, aprovou **369 verificações**: 66 da nova interface/retratos, 102 visuais, 90 de NPCs/progresso e 111 de exploração/portal. Foram exercitados o loop real de seis poses, os retratos NPC → Quest → NPC nos dois diálogos, bloqueio/liberação de movimento, Coins, XP, níveis, movimento normalizado, câmera e seis passagens entre áreas. As conversas foram acionadas pelo mesmo método chamado pela tecla E; não houve simulação física de teclado. As capturas de cada frame e dos diálogos foram inspecionadas visualmente.

Console ao término: **0 erros, 0 avisos**, às 11:01:03. Brasil permaneceu salva, fora de Play Mode. A auditoria do checkpoint confirmou mapas, colliders manuais, escala/collider atuais da Quest, scripts de gameplay, prefabs, animações do Player e HUD anterior preservados.

A única divergência inicial estava em um teste antigo que exigia medidas fixas do collider. A verificação agora compara com a configuração salva antes do Play, respeitando seus ajustes. Não houve falha funcional causada por esta integração.

Evidências nesta pasta: `energia-playmode.txt`, `depois/`, `auditoria-final.txt`, `mesh-audit.txt`, capturas `HUD-frame*-1080p.png` e `NPC_*-fala*-1080p.png`. Os relatórios históricos das etapas anteriores foram preservados; as novas capturas da regressão estão em `regressao/`.

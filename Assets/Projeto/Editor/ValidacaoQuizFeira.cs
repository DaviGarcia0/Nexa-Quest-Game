using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using NexaQuest.Brasil;

[InitializeOnLoad]
public static class ValidacaoQuizFeira
{
    const string Pedido = "Library/QuizFeira.solicitacao";
    public const string Ativo = "NexaQuest.ValidandoQuiz";
    const string Suite = "NexaQuest.SuiteQuiz", Inicio = "NexaQuest.IniciarQuiz";
    static readonly BindingFlags Flags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
    static ValidacaoQuizFeira()
    {
        EditorApplication.update += Consultar;
        EditorApplication.playModeStateChanged += estado => {
            if (estado == PlayModeStateChange.EnteredPlayMode && SessionState.GetBool(Inicio, false))
            {
                SessionState.SetBool(Inicio, false); Application.runInBackground = true;
                new GameObject("ValidacaoQuiz_Temporaria").AddComponent<VerificacoesQuizFeira>();
            }
            if (estado == PlayModeStateChange.EnteredEditMode && SessionState.GetBool(Suite, false))
            {
                SessionState.SetBool(Suite, false); SessionState.SetBool(Ativo, false);
                var pasta = MontagemQuizFeira.Docs + "depois/"; Directory.CreateDirectory(pasta);
                var origens = new[]{
                    new[]{MontagemMinijogoFeira.Documentacao + "minijogo-playmode.txt", "minijogo.txt"},
                    new[]{EnergyHUDSetup.Docs + "energia-playmode.txt", "energia.txt"},
                    new[]{QuestHUDVisualSetup.Docs + "visual-playmode.txt", "visual.txt"},
                    new[]{ConfiguracaoDesafiosFeira.Pasta + "painel-playmode.txt", "painel.txt"},
                    new[]{"Documentation/Brasil/EtapaNPCs/playmode.txt", "npcs.txt"},
                    new[]{"Documentation/Brasil/EtapaPortal/playmode.txt", "portal.txt"},
                    new[]{"Documentation/Brasil/EtapaPortal/console.txt", "console.txt"}
                };
                foreach (var par in origens) if (File.Exists(par[0])) File.Copy(par[0], pasta + par[1], true);
                bool tinha = SessionState.GetBool("NexaQuest.Quiz.ProgressoExistia", false);
                bool preservado = tinha == PlayerPrefs.HasKey(ProgressionManager.ChavePersistencia) &&
                    (!tinha || SessionState.GetString("NexaQuest.Quiz.ProgressoAntes", "") == PlayerPrefs.GetString(ProgressionManager.ChavePersistencia));
                File.WriteAllText(pasta + "progresso-real.txt", "Progresso real preservado: " + preservado);
                typeof(QuestHUDVisualValidation).GetMethod("RestoreGameView", Flags).Invoke(null, null);
                File.WriteAllText(pasta + "estado-editor.txt", SceneManager.GetActiveScene().path + "\nDirty: " + SceneManager.GetActiveScene().isDirty + "\nPlay Mode: " + EditorApplication.isPlaying);
                File.WriteAllText(pasta + "concluido.txt", DateTime.Now.ToString("s"));
            }
        };
    }
    static void Consultar()
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode || !File.Exists(Pedido)) return;
        var acao = File.ReadAllText(Pedido).Trim(); File.Delete(Pedido);
        try
        {
            var cena = SceneManager.GetActiveScene();
            if (cena.path != "Assets/Projeto/Scenes/Brasil.unity" || cena.isDirty) throw new InvalidOperationException("Brasil precisa estar salva e fora de Play Mode.");
            if (acao == "montar") MontagemQuizFeira.Montar();
            if (acao == "testar")
            {
                SessionState.SetBool("NexaQuest.Quiz.ProgressoExistia", PlayerPrefs.HasKey(ProgressionManager.ChavePersistencia));
                SessionState.SetString("NexaQuest.Quiz.ProgressoAntes", PlayerPrefs.GetString(ProgressionManager.ChavePersistencia, ""));
                QuestHUDVisualValidation.CapturePlayerCollider(); typeof(QuestHUDVisualValidation).GetMethod("SetGameView", Flags).Invoke(null, null);
                typeof(Editor).Assembly.GetType("UnityEditor.LogEntries").GetMethod("Clear", Flags).Invoke(null, null);
                SessionState.SetBool(Ativo, true); SessionState.SetBool(Suite, true); SessionState.SetBool(Inicio, true);
                SessionState.SetBool("NexaQuest.ValidandoMinijogo", false);
                // JSON v1 anterior ao Quiz: testa a compatibilidade com progresso já existente.
                PlayerPrefs.SetString("NexaQuest.Testes.Quiz", "{\"versao\":1,\"coins\":0,\"currentXP\":0,\"level\":1,\"energia\":100,\"possuiChaveVila\":false,\"instrucoesFeiraVistas\":false}"); PlayerPrefs.Save();
                EditorApplication.isPaused = false; EditorApplication.isPlaying = true;
            }
        }
        catch (Exception erro) { Directory.CreateDirectory(MontagemQuizFeira.Docs); File.WriteAllText(MontagemQuizFeira.Docs + "falha.txt", erro.ToString()); Debug.LogException(erro); }
    }
}

public class VerificacoesQuizFeira : MonoBehaviour
{
    readonly List<string> resultados = new List<string>(), problemas = new List<string>();
    int falhas;
    QuizFeira quiz; InterfaceQuizFeira ui; PainelDesafiosFeira desafios; ProgressionManager progresso;
    string Pasta => MontagemQuizFeira.Docs;
    void Conferir(bool ok, string mensagem)
    {
        resultados.Add((ok ? "PASS: " : "FAIL: ") + mensagem); if (!ok) falhas++;
        File.WriteAllLines(Pasta + "andamento.txt", resultados);
    }
    void Registrar(string texto, string pilha, LogType tipo) { if (tipo != LogType.Log) problemas.Add(tipo + ": " + texto); }
    GameObject Alvo(Vector2 posicao)
    {
        Canvas.ForceUpdateCanvases(); var dados = new PointerEventData(EventSystem.current) { position = posicao };
        var acertos = new List<RaycastResult>(); EventSystem.current.RaycastAll(dados, acertos); return acertos.Count > 0 ? acertos[0].gameObject : null;
    }
    IEnumerator Clicar(Button botao)
    {
        yield return null; var posicao = (Vector2)botao.transform.position; var alvo = Alvo(posicao);
        Conferir(alvo == botao.gameObject, "Raycast acerta " + botao.name + " (alvo: " + (alvo != null ? alvo.name : "nenhum") + ")");
        if (alvo == null) yield break;
        var dados = new PointerEventData(EventSystem.current) { position = posicao, button = PointerEventData.InputButton.Left };
        ExecuteEvents.Execute(alvo, dados, ExecuteEvents.pointerDownHandler); ExecuteEvents.Execute(alvo, dados, ExecuteEvents.pointerUpHandler); ExecuteEvents.Execute(alvo, dados, ExecuteEvents.pointerClickHandler);
    }
    Button Botao(GameObject painel, string nome) => painel.transform.Find("ArtePainel/" + nome).GetComponent<Button>();
    Button QuizBotao => desafios.transform.Find("ArtePainel/BotaoQuiz").GetComponent<Button>();
    IEnumerator Capturar(string nome) { yield return new WaitForEndOfFrame(); ScreenCapture.CaptureScreenshot(Pasta + nome + ".png"); yield return null; }
    void TerminarFeedback() { quiz.Simular(quiz.duracaoExibicaoFeedback + quiz.duracaoFadeFeedback + .01f); }
    void AcertarTodas() { for (int i = 0; i < 3; i++) { quiz.Responder(quiz.dados.perguntas[quiz.PerguntaAtual].respostaCorreta); TerminarFeedback(); } }
    IEnumerator Abrir() { if (!desafios.EstaAberto) desafios.Abrir(); yield return Clicar(QuizBotao); }
    void DefinirEnergia(int energia)
    {
        var json = PlayerPrefs.GetString("NexaQuest.Testes.Quiz");
        json = System.Text.RegularExpressions.Regex.Replace(json, "\"energia\":\\s*\\d+", "\"energia\":" + energia);
        PlayerPrefs.SetString("NexaQuest.Testes.Quiz", json); progresso.CarregarProgresso();
    }
    void Layout()
    {
        foreach (var t in new[]{ui.progressoPerguntas, ui.tempo, ui.enunciado, ui.status}.Concat(ui.alternativas))
        { t.ForceMeshUpdate(); Conferir(!t.isTextOverflowing, "Texto cabe: " + t.name); }
    }
    IEnumerator Start()
    {
        Application.logMessageReceived += Registrar; yield return null; yield return null;
        quiz = FindObjectOfType<QuizFeira>(true); ui = quiz.interfaceQuiz; desafios = FindObjectOfType<PainelDesafiosFeira>(true); progresso = ProgressionManager.Instance;
        quiz.SimulacaoManual = true;
        var mundo = FindObjectOfType<BrazilWorld>(); var jogador = mundo.player; var posicaoOriginal = jogador.transform.position;
        var textoEnergia = GameObject.Find("Canvas").transform.Find("EnergyHUD/EnergyText").GetComponent<TMPro.TMP_Text>();
        Conferir(Screen.width == 1920 && Screen.height == 1080, "Play Mode 1920x1080");
        Conferir(!quiz.gameObject.activeSelf && quiz.Estado == EstadoQuizFeira.Fechado, "Quiz inicialmente fechado");
        Conferir(progresso.Energia == 100 && !progresso.QuizFeiraConcluido && !progresso.PossuiChaveVila, "JSON v1 anterior ao Quiz carrega sem apagar progresso");
        Conferir(quiz.dados.EstaValido() && quiz.dados.perguntas.Length == 3, "Tres perguntas editaveis, quatro alternativas e um indice correto");
        var retratoOriginal = ui.retrato.rectTransform.anchoredPosition; var tamanhoRetrato = ui.retrato.rectTransform.sizeDelta;
        yield return Abrir();
        Conferir(quiz.Estado == EstadoQuizFeira.Inicial && ui.painelInicial.activeSelf && !desafios.EstaAberto, "Botao Quiz abre introducao e oculta desafios");
        Conferir(progresso.Energia == 100 && textoEnergia.text == "100", "Abrir introducao nao custa Energia");
        Conferir(jogador.ControlsLocked && !mundo.TravelTo(mundo.areas[1], mundo.areas[1].defaultSpawn), "Mapa e transicoes bloqueados");
        FindObjectOfType<DialogueController>().HandleInteract(); Conferir(!FindObjectOfType<DialogueController>().IsOpen, "NPCs nao recebem interacao durante Quiz");
        Conferir(!desafios.minijogo.TentarAbrir(desafios), "Minijogo nao pode abrir simultaneamente");
        var explicacao = ui.painelInicial.transform.Find("ArtePainel/ExplicacaoQuiz").GetComponent<TMPro.TMP_Text>(); explicacao.ForceMeshUpdate();
        Conferir(!explicacao.isTextOverflowing, "Explicacao cabe na area vazia da introducao"); yield return Capturar("Inicial-1080p");
        yield return Clicar(Botao(ui.painelInicial, "BotaoVoltar"));
        Conferir(desafios.EstaAberto && quiz.Estado == EstadoQuizFeira.Fechado && progresso.Energia == 100, "Voltar retorna aos desafios sem gastar Energia");
        yield return Abrir(); yield return Clicar(Botao(ui.painelInicial, "BotaoComecar")); quiz.Comecar();
        Conferir(quiz.Estado == EstadoQuizFeira.Pergunta && progresso.Energia == 80 && textoEnergia.text == "80", "Comecar gasta exatamente 20, mesmo com clique repetido");
        Conferir(quiz.PerguntaAtual == 0 && quiz.TempoRestante == 30 && ui.tempo.text == "30s" && ui.progressoPerguntas.text == "1/3", "Primeira pergunta inicia em 30 segundos e 1/3");
        Conferir(ui.retrato.sprite == ui.questPensando && ui.botoesRespostas.All(b => b.interactable), "Retrato pensando e quatro respostas habilitadas"); Layout();
        yield return null; Canvas.ForceUpdateCanvases();
        foreach (var b in ui.botoesRespostas)
        {
            var cantos = new Vector3[4]; ((RectTransform)b.transform).GetWorldCorners(cantos);
            foreach (var p in new[]{cantos[0] + new Vector3(2,2), cantos[1] + new Vector3(2,-2), cantos[2] + new Vector3(-2,-2), cantos[3] + new Vector3(-2,2)})
                { var alvo = Alvo(p); Conferir(alvo == b.gameObject, "Area interna clicavel: " + b.name + " em " + p + " alvo " + (alvo != null ? alvo.name : "nenhum")); }
            Conferir(b.GetComponent<Image>().color.a == 0, "Botao de resposta transparente: " + b.name);
        }
        yield return Capturar("Pergunta-1080p");
        quiz.SimulacaoManual = false; yield return new WaitForSecondsRealtime(.25f); quiz.SimulacaoManual = true;
        Conferir(quiz.TempoRestante < 30 && quiz.TempoRestante > 29.3f, "Timer decresce em tempo real pelo Update");
        int correta = quiz.dados.perguntas[0].respostaCorreta; yield return Clicar(ui.botoesRespostas[correta]); quiz.Responder(correta);
        Conferir(progresso.Coins == 10 && quiz.MoedasDaRodada == 10 && quiz.Estado == EstadoQuizFeira.Acerto, "Primeiro acerto credita 10 Coins uma unica vez");
        quiz.ProcessarFechamento(true); desafios.ProcessarFechamento(true);
        Conferir(desafios.EstaAberto && progresso.Energia == 80 && progresso.Coins == 10 && progresso.CurrentXP == 0 && !progresso.QuizFeiraConcluido, "Esc no feedback conserva Coins e custo, sem EXP ou conclusao");
        Conferir(!ui.feedbackAcerto.gameObject.activeSelf && !ui.feedbackErro.gameObject.activeSelf, "Esc cancela feedback e fade pendentes");
        yield return Abrir(); yield return Clicar(Botao(ui.painelInicial, "BotaoComecar"));
        Conferir(quiz.PerguntaAtual == 0 && quiz.MoedasDaRodada == 0 && quiz.TempoRestante == 30 && progresso.Energia == 60, "Nova rodada reseta somente estado do Quiz e custa 20");
        correta = quiz.dados.perguntas[0].respostaCorreta;
        int errada = (correta + 1) % 4; yield return Clicar(ui.botoesRespostas[errada]);
        yield return null; Canvas.ForceUpdateCanvases();
        float tempoPausado = quiz.TempoRestante;
        Conferir(quiz.Estado == EstadoQuizFeira.Erro && ui.feedbackErro.gameObject.activeSelf && ui.feedbackErro.alpha == 1 && ui.retrato.sprite == ui.questTriste, "Erro mostra feedback e retrato triste");
        Conferir(quiz.PerguntaAtual == 0 && progresso.Coins == 10 && progresso.CurrentXP == 0, "Erro nao avanca nem concede recompensas");
        Conferir(ui.botoesRespostas.All(b => !b.interactable) && Alvo(ui.botoesRespostas[0].transform.position) == ui.feedbackErro.gameObject, "Feedback bloqueia cliques atraves da interface (alvo: " + (Alvo(ui.botoesRespostas[0].transform.position)?.name ?? "nenhum") + ")");
        quiz.Responder(correta); Conferir(progresso.Coins == 10, "Resposta durante feedback e ignorada");
        yield return Capturar("Erro-1080p");
        quiz.SimulacaoManual = false; yield return new WaitForSecondsRealtime(.6f);
        Conferir(quiz.Estado == EstadoQuizFeira.Erro && ui.feedbackErro.alpha == 1 && quiz.TempoRestante == tempoPausado, "Feedback mantem alpha 1 e timer pausado durante espera real");
        yield return new WaitForSecondsRealtime(.73f);
        Conferir(ui.feedbackErro.alpha > 0 && ui.feedbackErro.alpha < 1, "Fade suave real durante os ultimos 0,3 segundo");
        yield return new WaitForSecondsRealtime(.3f); quiz.SimulacaoManual = true;
        Conferir(quiz.Estado == EstadoQuizFeira.Pergunta && !ui.feedbackErro.gameObject.activeSelf && ui.retrato.sprite == ui.questPensando && ui.botoesRespostas.All(b => b.interactable), "Feedback termina perto de 1,5s e restaura pergunta");
        Conferir(quiz.TempoRestante <= tempoPausado && quiz.TempoRestante > tempoPausado - .5f, "Timer retoma a partir do tempo anterior");
        for (int i = 0; i < 3; i++)
        {
            quiz.Responder(errada); Conferir(ui.feedbackErro.alpha == 1 && ui.feedbackErro.gameObject.activeSelf, "Reexibir erro restaura alpha: tentativa " + (i+1));
            quiz.Simular(1.2f); Conferir(quiz.Estado == EstadoQuizFeira.Erro && Mathf.Approximately(ui.feedbackErro.alpha, 1), "Espera padrao de 1,2s");
            quiz.Simular(.15f); Conferir(ui.feedbackErro.alpha > .4f && ui.feedbackErro.alpha < .6f, "Metade do fade de 0,3s");
            quiz.Simular(.16f); Conferir(quiz.PerguntaAtual == 0 && progresso.Coins == 10 && !ui.feedbackErro.gameObject.activeSelf, "Erros repetidos nao recompensam nem mudam pergunta");
        }
        quiz.Simular(31);
        Conferir(quiz.TempoRestante == 0 && quiz.Estado == EstadoQuizFeira.Erro && ui.status.text.Contains("Tempo esgotado"), "Timeout chega a zero e mostra estado de erro");
        Conferir(progresso.Coins == 10 && ui.retrato.sprite == ui.questTriste, "Timeout nao premia e usa retrato triste"); TerminarFeedback();
        Conferir(quiz.PerguntaAtual == 0 && quiz.TempoRestante == 30 && ui.tempo.text == "30s" && ui.retrato.sprite == ui.questPensando, "Timeout reinicia mesma pergunta em 30s");
        yield return Clicar(ui.botoesRespostas[correta]); quiz.Responder(correta);
        Conferir(progresso.Coins == 20 && ui.retrato.sprite == ui.questFeliz && ui.feedbackAcerto.gameObject.activeSelf && !ui.botoesRespostas.Any(b => b.interactable), "Acerto premia uma vez e mostra Quest feliz");
        float tempoAcerto = quiz.TempoRestante; quiz.Simular(.5f);
        Conferir(quiz.TempoRestante == tempoAcerto, "Timer para durante feedback de acerto"); yield return Capturar("Acerto-1080p"); TerminarFeedback();
        Conferir(quiz.PerguntaAtual == 1 && ui.progressoPerguntas.text == "2/3" && quiz.TempoRestante == 30, "Pergunta 2 carrega com novo tempo"); Layout();
        quiz.Responder((quiz.dados.perguntas[1].respostaCorreta + 1) % 4); Conferir(progresso.Coins == 20 && quiz.PerguntaAtual == 1, "Erro na segunda preserva saldo X+10 da rodada"); TerminarFeedback();
        yield return Clicar(ui.botoesRespostas[quiz.dados.perguntas[1].respostaCorreta]); quiz.Responder(quiz.dados.perguntas[1].respostaCorreta);
        Conferir(progresso.Coins == 30 && quiz.MoedasDaRodada == 20, "Segundo acerto: X+20, sem duplicacao"); TerminarFeedback();
        Conferir(quiz.PerguntaAtual == 2 && ui.progressoPerguntas.text == "3/3" && quiz.TempoRestante == 30, "Pergunta 3 carrega com novo tempo"); Layout();
        yield return Clicar(ui.botoesRespostas[quiz.dados.perguntas[2].respostaCorreta]); quiz.Responder(quiz.dados.perguntas[2].respostaCorreta);
        Conferir(progresso.Coins == 40 && quiz.MoedasDaRodada == 30 && progresso.CurrentXP == 0 && !ui.painelFinal.activeSelf, "Terceiro acerto chega a 30 por rodada e aguarda feedback"); TerminarFeedback(); quiz.Simular(10); quiz.Comecar();
        Conferir(quiz.RodadaConcluida && progresso.CurrentXP == 50 && progresso.QuizFeiraConcluido && progresso.PossuiChaveVila && quiz.ChaveConquistadaNestaRodada, "Conclusao concede 50 EXP, flag persistente e chave pela primeira vez");
        Conferir(quiz.Estado == EstadoQuizFeira.Concluido && ui.painelFinal.activeSelf && ui.recompensas.text.Contains("+30") && ui.recompensas.text.Contains("conquistada"), "Painel final mostra resumo real da rodada");
        Conferir(ui.retrato.rectTransform.anchoredPosition == retratoOriginal && ui.retrato.rectTransform.sizeDelta == tamanhoRetrato && ui.retrato.preserveAspect, "Pensando, triste e feliz usam o mesmo retangulo e proporcao preservada");
        yield return Capturar("FinalPrimeiraChave-1080p");
        yield return Clicar(Botao(ui.painelFinal, "BotaoContinuar"));
        Conferir(desafios.EstaAberto && !quiz.gameObject.activeSelf && jogador.transform.position == posicaoOriginal, "Continuar retorna ao painel de desafios na mesma posicao do mapa");
        yield return Abrir(); Conferir(quiz.Estado == EstadoQuizFeira.Inicial && progresso.Energia == 60, "Introducao reaparece em toda nova execucao sem custo");
        yield return Clicar(Botao(ui.painelInicial, "BotaoComecar")); AcertarTodas();
        Conferir(progresso.Coins == 70 && progresso.Level == 2 && progresso.CurrentXP == 0 && progresso.Energia == 40, "Segunda rodada: mais 30 Coins e 50 EXP, usando nivel de 100 EXP");
        Conferir(progresso.QuizFeiraConcluido && progresso.PossuiChaveVila && !quiz.ChaveConquistadaNestaRodada && ui.recompensas.text.Contains("já obtida"), "Quiz repetido preserva flag e nao duplica chave"); yield return Capturar("FinalChaveExistente-1080p");
        var salvo = PlayerPrefs.GetString("NexaQuest.Testes.Quiz");
        JsonUtility.FromJsonOverwrite("{\"coins\":0,\"currentXP\":0,\"level\":1,\"energia\":100,\"quizFeiraConcluido\":false,\"possuiChaveVila\":false}", progresso); progresso.CarregarProgresso();
        Conferir(progresso.Coins == 70 && progresso.Level == 2 && progresso.Energia == 40 && progresso.QuizFeiraConcluido && progresso.PossuiChaveVila, "JSON existente persiste Quiz, chave, Coins, EXP, Level e Energia");
        Conferir(PlayerPrefs.GetString("NexaQuest.Testes.Quiz") == salvo, "Carregar nao reescreve ou apaga o save");
        yield return Clicar(Botao(ui.painelFinal, "BotaoContinuar"));
        var mini = desafios.minijogo; mini.TentarAbrir(desafios); mini.IniciarPartida(); mini.queda.enabled = false; mini.quest.enabled = false;
        for (int i = 0; i < 5; i++) foreach (var tipo in new[]{TipoItemFeira.Banana, TipoItemFeira.Abacaxi, TipoItemFeira.Melancia})
        { int id = mini.queda.CriarItem(tipo, new Vector2(0,300), 0); mini.queda.Resolver(id, true); }
        Conferir(mini.ObjetivoConcluido && mini.EmPartida && !mini.ChaveConquistadaNestaPartida && progresso.QuizFeiraConcluido && progresso.PossuiChaveVila, "Minijogo reconhece a chave do Quiz e preserva flag do Quiz");
        Conferir(progresso.Coins == 75 && progresso.CurrentXP == 50 && progresso.Energia == 20, "Minijogo mantem suas recompensas e custo originais");
        mini.Sair(); mini.Sair(); mini.queda.enabled = true; mini.quest.enabled = true;
        DefinirEnergia(20); yield return Abrir(); yield return Clicar(Botao(ui.painelInicial, "BotaoComecar"));
        Conferir(progresso.Energia == 0 && textoEnergia.text == "0" && quiz.Estado == EstadoQuizFeira.Pergunta, "Energia 20 permite comecar e chega exatamente a zero");
        quiz.ProcessarFechamento(false); Conferir(quiz.Estado == EstadoQuizFeira.Pergunta, "Sem Esc a atividade continua"); quiz.ProcessarFechamento(true);
        Conferir(progresso.Energia == 0 && progresso.QuizFeiraConcluido && progresso.CurrentXP == 50, "Esc nao reembolsa Energia nem apaga conclusao anterior");
        yield return Abrir(); Conferir(quiz.Estado == EstadoQuizFeira.Inicial && progresso.Energia == 0, "Energia zero ainda permite abrir introducao");
        yield return Clicar(Botao(ui.painelInicial, "BotaoComecar"));
        Conferir(quiz.Estado == EstadoQuizFeira.Inicial && progresso.Energia == 0 && ui.avisoEnergia.gameObject.activeSelf, "Comecar em zero e recusado com o aviso existente"); yield return Capturar("EnergiaInsuficiente-1080p");
        DefinirEnergia(19); quiz.Comecar(); Conferir(quiz.Estado == EstadoQuizFeira.Inicial && progresso.Energia == 19, "Energia 19 tambem bloqueia sem desconto");
        yield return Clicar(Botao(ui.painelInicial, "BotaoVoltar")); desafios.Fechar(); yield return null;
        Conferir(!jogador.ControlsLocked && !mundo.IsTransitioning && jogador.transform.position == posicaoOriginal, "Fechar desafios restaura controle do mapa");
        Conferir(ui.hudsCompartilhadas.All(h => h.parent.name == "Canvas") && ui.avisoEnergia.transform.parent == desafios.transform, "HUDs e aviso compartilhados voltam a seus pais originais");
        Conferir(!ui.feedbackAcerto.gameObject.activeSelf && !ui.feedbackErro.gameObject.activeSelf && !quiz.gameObject.activeSelf && mini.queda.QuantidadeAtiva == 0, "Saida limpa todos os estados visuais e itens");
        Conferir(problemas.Count == 0, "Console sem erros ou avisos durante o Quiz"); resultados.AddRange(problemas); resultados.Add("Falhas: " + falhas);
        File.WriteAllLines(Pasta + "quiz-playmode.txt", resultados); Application.logMessageReceived -= Registrar;
        if (falhas > 0) { EditorApplication.isPlaying = false; yield break; }
        // A suite antiga continua intacta, com sua propria chave isolada e valores iniciais conhecidos.
        SessionState.SetBool(ValidacaoQuizFeira.Ativo, false); SessionState.SetBool("NexaQuest.ValidandoMinijogo", true);
        PlayerPrefs.SetString("NexaQuest.Testes.Minijogo", "{\"versao\":1,\"coins\":0,\"currentXP\":0,\"level\":1,\"energia\":100,\"possuiChaveVila\":false,\"instrucoesFeiraVistas\":false}"); PlayerPrefs.Save(); progresso.CarregarProgresso();
        quiz.SimulacaoManual = false; new GameObject("RegressaoMinijogo_Temporaria").AddComponent<VerificacoesMinijogoFeira>(); Destroy(gameObject);
    }
}

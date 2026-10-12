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
public static class ValidacaoQuizAmazonia
{
    const string Pedido="Library/QuizAmazonia.solicitacao",Inicio="NexaQuest.QuizAmazonia.Inicio",Fim="NexaQuest.QuizAmazonia.Fim";
    static readonly BindingFlags Flags=BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Static;
    static ValidacaoQuizAmazonia()
    {
        EditorApplication.update+=Consultar;
        EditorApplication.playModeStateChanged+=estado=>{
            if(estado==PlayModeStateChange.EnteredPlayMode&&SessionState.GetBool(Inicio,false)) {
                SessionState.SetBool(Inicio,false);Application.runInBackground=true;new GameObject("ValidacaoQuizAmazonia_Temporaria").AddComponent<VerificacoesQuizAmazonia>();
            }
            if(estado==PlayModeStateChange.EnteredEditMode&&SessionState.GetBool(Fim,false)) {
                SessionState.SetBool(Fim,false);SessionState.SetBool("NexaQuest.ValidandoQuiz",false);
                bool existia=SessionState.GetBool("NexaQuest.QuizAmazonia.Existia",false);
                bool igual=existia==PlayerPrefs.HasKey(ProgressionManager.ChavePersistencia)&&(!existia||SessionState.GetString("NexaQuest.QuizAmazonia.Save","")==PlayerPrefs.GetString(ProgressionManager.ChavePersistencia));
                File.WriteAllText(MontagemQuizAmazonia.Docs+"progresso-real.txt","Progresso real preservado: "+igual);
                typeof(QuestHUDVisualValidation).GetMethod("RestoreGameView",Flags).Invoke(null,null);
                File.WriteAllText(MontagemQuizAmazonia.Docs+"estado-editor.txt",SceneManager.GetActiveScene().path+"\nDirty: "+SceneManager.GetActiveScene().isDirty+"\nPlay Mode: "+EditorApplication.isPlaying);
                if(SessionState.GetBool("NexaQuest.QuizAmazonia.Aprovado",false))File.WriteAllText("Library/NPCsAmazonia.solicitacao","testar");
            }
        };
    }
    static void Consultar()
    {
        if(EditorApplication.isCompiling||EditorApplication.isUpdating||EditorApplication.isPlayingOrWillChangePlaymode||!File.Exists(Pedido))return;
        string acao=File.ReadAllText(Pedido).Trim();File.Delete(Pedido);
        try {
            var cena=SceneManager.GetActiveScene();if(cena.path!="Assets/Projeto/Scenes/Brasil.unity"||cena.isDirty)throw new InvalidOperationException("Salve Brasil e deixe fora de Play Mode.");
            if(acao=="montar")MontagemQuizAmazonia.Montar();
            else if(acao=="ajustar")MontagemQuizAmazonia.AjustarTextos();
            else if(acao=="testar") {
                Directory.CreateDirectory(MontagemQuizAmazonia.Docs);
                SessionState.SetBool("NexaQuest.QuizAmazonia.Existia",PlayerPrefs.HasKey(ProgressionManager.ChavePersistencia));SessionState.SetString("NexaQuest.QuizAmazonia.Save",PlayerPrefs.GetString(ProgressionManager.ChavePersistencia,""));
                typeof(QuestHUDVisualValidation).GetMethod("SetGameView",Flags).Invoke(null,null);
                typeof(Editor).Assembly.GetType("UnityEditor.LogEntries").GetMethod("Clear",Flags).Invoke(null,null);
                SessionState.SetBool("NexaQuest.ValidandoMinijogo",false);SessionState.SetBool("NexaQuest.ValidandoQuiz",true);
                PlayerPrefs.SetString("NexaQuest.Testes.Quiz","{\"versao\":1,\"coins\":0,\"currentXP\":0,\"level\":1,\"energia\":100,\"possuiChaveVila\":true,\"quizFeiraConcluido\":true,\"instrucoesFeiraVistas\":true}");PlayerPrefs.Save();
                SessionState.SetBool(Inicio,true);SessionState.SetBool(Fim,true);EditorApplication.isPlaying=true;
            }
        }catch(Exception e){Directory.CreateDirectory(MontagemQuizAmazonia.Docs);File.WriteAllText(MontagemQuizAmazonia.Docs+"falha.txt",e.ToString());Debug.LogException(e);}
    }
}
public class VerificacoesQuizAmazonia:MonoBehaviour
{
    readonly List<string> resultados=new List<string>(),problemas=new List<string>();int falhas;
    QuizAmazonia quiz;InterfaceQuizAmazonia ui;PainelDesafiosAmazonia painel;ProgressionManager progresso;
    string Pasta=>MontagemQuizAmazonia.Docs;
    void Conferir(bool ok,string mensagem){resultados.Add((ok?"PASS: ":"FAIL: ")+mensagem);if(!ok)falhas++;File.WriteAllLines(Pasta+"andamento.txt",resultados);}
    void Log(string mensagem,string pilha,LogType tipo){if(tipo!=LogType.Log)problemas.Add(tipo+": "+mensagem);}
    GameObject Alvo(Vector2 ponto){Canvas.ForceUpdateCanvases();var acertos=new List<RaycastResult>();EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current){position=ponto},acertos);return acertos.Count>0?acertos[0].gameObject:null;}
    IEnumerator Clicar(Button botao){yield return null;var ponto=(Vector2)botao.transform.position;var alvo=Alvo(ponto);Conferir(alvo==botao.gameObject,"Raycast/clique real: "+botao.name);if(alvo==null)yield break;var dados=new PointerEventData(EventSystem.current){position=ponto,button=PointerEventData.InputButton.Left};ExecuteEvents.Execute(alvo,dados,ExecuteEvents.pointerDownHandler);ExecuteEvents.Execute(alvo,dados,ExecuteEvents.pointerUpHandler);ExecuteEvents.Execute(alvo,dados,ExecuteEvents.pointerClickHandler);}
    Button BotaoQuiz=>painel.transform.Find("ArtePainel/BotaoQuiz").GetComponent<Button>();
    IEnumerator Capturar(string nome){yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Pasta+nome+".png");yield return null;}
    void Terminar(){quiz.Simular(1.51f);}
    void AcertarTudo(){for(int i=0;i<3;i++){quiz.Responder(quiz.dados.perguntas[quiz.PerguntaAtual].respostaCorreta);Terminar();}}
    void Energia(int energia){var json=PlayerPrefs.GetString("NexaQuest.Testes.Quiz");json=System.Text.RegularExpressions.Regex.Replace(json,"\"energia\":\\s*\\d+","\"energia\":"+energia);PlayerPrefs.SetString("NexaQuest.Testes.Quiz",json);progresso.CarregarProgresso();}
    void Layout(){foreach(var texto in new[]{ui.progressoPerguntas,ui.tempo,ui.enunciado,ui.status}.Concat(ui.alternativas)){texto.ForceMeshUpdate();Conferir(!texto.isTextOverflowing,"Sem overflow: "+texto.name+" pergunta "+(quiz.PerguntaAtual+1)); if(ui.alternativas.Contains(texto)) Conferir(texto.textBounds.min.y>=texto.rectTransform.rect.yMin-.5f && texto.textBounds.max.y<=texto.rectTransform.rect.yMax+.5f,"Letras dentro da margem interna: "+texto.text);}}
    IEnumerator Start()
    {
        Application.logMessageReceived+=Log;yield return null;yield return null;
        quiz=FindObjectOfType<QuizAmazonia>(true);ui=quiz.interfaceQuiz;painel=FindObjectOfType<PainelDesafiosAmazonia>(true);progresso=ProgressionManager.Instance;quiz.SimulacaoManual=true;
        var mundo=FindObjectOfType<BrazilWorld>();var jogador=mundo.player;var area=mundo.areas.Single(a=>a.name=="Amazonia");
        Conferir(Screen.width==1920&&Screen.height==1080,"Play Mode real 1920x1080");
        Conferir(!painel.gameObject.activeSelf&&!quiz.gameObject.activeSelf,"Roots iniciam fechados e sem painel inicial extra");
        Conferir(progresso.PossuiChaveVila&&progresso.QuizFeiraConcluido&&!progresso.PossuiChaveAmazonia&&!progresso.QuizAmazoniaConcluido,"Save antigo mantém Feira e inicia flags da Amazônia false");
        string[] perguntas={"O que é a Amazônia Legal?","Qual processo das árvores amazônicas ajuda a devolver água para a atmosfera e alimentar os chamados 'rios voadores'?","Por que o desmatamento da Amazônia pode alterar o regime de chuvas?"};
        string[][] alternativas={new[]{"Somente a parte da floresta que fica no estado do Amazonas.","Uma região definida por lei para planejamento e políticas públicas de desenvolvimento da Amazônia brasileira.","Uma unidade de conservação onde nenhuma pessoa pode viver.","Outro nome dado apenas ao Bioma Amazônia."},new[]{"Sedimentação","Evapotranspiração","Erosão","Fossilização"},new[]{"Porque aumenta permanentemente a quantidade de água liberada pelas árvores.","Porque transforma os rios da floresta em água salgada.","Porque reduz a vegetação que recicla água para a atmosfera, afetando o transporte de umidade e as chuvas.","Porque impede completamente a entrada de ventos do Oceano Atlântico."}};
        Conferir(quiz.dados.EstaValido()&&quiz.dados.perguntas.Length==3,"Três perguntas editáveis com quatro alternativas");
        for(int i=0;i<3;i++){Conferir(quiz.dados.perguntas[i].enunciado==perguntas[i],"Enunciado exato solicitado: "+perguntas[i]);Conferir(quiz.dados.perguntas[i].alternativas.SequenceEqual(alternativas[i]),"Quatro alternativas completas e acentuadas: "+(i+1));Conferir(quiz.dados.perguntas[i].respostaCorreta==new[]{1,1,2}[i],"Correta: "+new[]{"B","B — Evapotranspiração","C"}[i]);}
        jogador.enabled=false;Conferir(mundo.TravelTo(area,area.defaultSpawn),"Entrar na Amazônia funciona");yield return new WaitForSecondsRealtime(1);
        var guardia=area.transform.Find("NPCsAmazonia/NPC_Amazonia_01").GetComponent<NPCInteraction>();var dc=FindObjectOfType<DialogueController>();
        jogador.Teleport(guardia.transform.position+Vector3.down*.35f);Physics2D.SyncTransforms();yield return new WaitForFixedUpdate();yield return new WaitForFixedUpdate();yield return null;
        Conferir(dc.interactionPrompt.activeSelf,"Guardiã mostra E");dc.HandleInteract();yield return null;
        foreach(var fala in guardia.dialogue.lines){Conferir(dc.dialogueText.text==fala.text&&dc.portrait.sprite==fala.portrait,"Fala/retrato correto da Guardiã "+dc.LineIndex);yield return null;dc.HandleInteract();yield return null;}
        Conferir(painel.EstaAberto&&jogador.ControlsLocked&&!dc.IsOpen,"Fim da Guardiã abre painel e mantém bloqueio modal");
        Conferir(progresso.Energia==100,"Abrir escolha não custa energia");var posicao=jogador.transform.position;
        Conferir(AssetDatabase.GetAssetPath(painel.transform.Find("ArtePainel").GetComponent<Image>().sprite)==MontagemQuizAmazonia.Arte+"PainelDesafiosAmazonia.png","Painel usa exatamente a arte da Amazônia");yield return Capturar("Desafios-1080p");
        yield return Clicar(painel.transform.Find("ArtePainel/BotaoMinijogo").GetComponent<Button>());
        Conferir(painel.EstaAberto&&progresso.Energia==100&&painel.mensagem.text=="Minijogo em desenvolvimento."&&!quiz.gameObject.activeSelf,"Minijogo preparado não inicia gameplay ou cobra energia");
        yield return Clicar(BotaoQuiz);painel.SelecionarQuiz();
        Conferir(quiz.Estado==EstadoQuizAmazonia.Pergunta&&!painel.EstaAberto&&progresso.Energia==80,"Clique Quiz inicia direto e cobra 20 apenas uma vez");
        Conferir(quiz.PerguntaAtual==0&&quiz.TempoRestante==30&&ui.progressoPerguntas.text=="1/3"&&ui.retrato.sprite==ui.questPensando,"Pergunta 1, timer real 30s e Quest pensando");
        var posRetrato=ui.retrato.rectTransform.anchoredPosition;var tamanhoRetrato=ui.retrato.rectTransform.sizeDelta;yield return null;Layout();
        foreach(var b in ui.botoesRespostas){var cantos=new Vector3[4];((RectTransform)b.transform).GetWorldCorners(cantos);foreach(var ponto in new[]{cantos[0]+new Vector3(2,2),cantos[1]+new Vector3(2,-2),cantos[2]+new Vector3(-2,-2),cantos[3]+new Vector3(-2,2)})Conferir(Alvo(ponto)==b.gameObject,"Canto interno clicável: "+b.name);Conferir(b.GetComponent<Image>().color.a==0,"Resposta transparente: "+b.name);}
        yield return Capturar("Pergunta1-1080p");quiz.SimulacaoManual=false;Time.timeScale=0;yield return new WaitForSecondsRealtime(.25f);Time.timeScale=1;quiz.SimulacaoManual=true;
        Conferir(quiz.TempoRestante<30&&quiz.TempoRestante>29.3f,"Update usa tempo real mesmo com timeScale zero");
        yield return Clicar(ui.botoesRespostas[0]);yield return null;float restante=quiz.TempoRestante;
        Conferir(quiz.Estado==EstadoQuizAmazonia.Erro&&ui.retrato.sprite==ui.questTriste&&progresso.Coins==0&&quiz.PerguntaAtual==0,"Erro não premia nem avança, retrato triste");
        Conferir(ui.botoesRespostas.All(b=>!b.interactable)&&Alvo(ui.botoesRespostas[0].transform.position)==ui.feedbackErro.gameObject,"Feedback bloqueia respostas e cliques por trás");
        quiz.Responder(1);Conferir(progresso.Coins==0,"Cliques durante feedback ignorados");yield return Capturar("Erro-1080p");
        quiz.SimulacaoManual=false;yield return new WaitForSecondsRealtime(.6f);Conferir(ui.feedbackErro.alpha==1&&quiz.TempoRestante==restante,"Feedback opaco e timer pausado em tempo real");yield return new WaitForSecondsRealtime(.73f);Conferir(ui.feedbackErro.alpha>0&&ui.feedbackErro.alpha<1,"Fade real ocorre no último 0,3s");yield return new WaitForSecondsRealtime(.3f);quiz.SimulacaoManual=true;
        Conferir(quiz.Estado==EstadoQuizAmazonia.Pergunta&&ui.retrato.sprite==ui.questPensando&&quiz.TempoRestante<restante&&quiz.TempoRestante>restante-.4f,"Após cerca de 1,5s retoma mesma pergunta e timer anterior");
        for(int i=0;i<3;i++){quiz.Responder(0);Conferir(ui.feedbackErro.alpha==1,"Novo erro restaura alpha 1");quiz.Simular(1.2f);Conferir(ui.feedbackErro.alpha==1,"Espera de 1,2 segundo");quiz.Simular(.15f);Conferir(Mathf.Abs(ui.feedbackErro.alpha-.5f)<.02f,"Metade do fade de 0,3s");quiz.Simular(.16f);Conferir(quiz.Estado==EstadoQuizAmazonia.Pergunta&&progresso.Coins==0,"Erros repetidos mantêm pergunta e zero Coins");}
        quiz.Simular(31);Conferir(quiz.TempoRestante==0&&quiz.Estado==EstadoQuizAmazonia.Erro&&ui.status.text=="Tempo esgotado!"&&progresso.Coins==0,"Timeout mostra erro sem premiar");Terminar();Conferir(quiz.PerguntaAtual==0&&quiz.TempoRestante==30,"Timeout reinicia mesma pergunta em 30s");
        yield return Clicar(ui.botoesRespostas[1]);quiz.Responder(1);Conferir(progresso.Coins==10&&ui.retrato.sprite==ui.questFeliz,"Acerto dá exatamente 10, duplo clique não duplica");yield return Capturar("Acerto-1080p");float parado=quiz.TempoRestante;quiz.Simular(.4f);Conferir(quiz.TempoRestante==parado,"Timer para no acerto");
        quiz.ProcessarFechamento(true);painel.ProcessarFechamento(true);
        Conferir(painel.EstaAberto&&progresso.Energia==80&&progresso.Coins==10&&progresso.CurrentXP==0&&!progresso.QuizAmazoniaConcluido&&!progresso.PossuiChaveAmazonia,"Esc conserva custo e Coins parciais sem EXP/chave/conclusão");
        Conferir(!ui.feedbackAcerto.gameObject.activeSelf&&!ui.feedbackErro.gameObject.activeSelf,"Esc limpa feedback/fade");
        yield return Clicar(BotaoQuiz);Conferir(progresso.Energia==60&&quiz.PerguntaAtual==0&&quiz.MoedasDaRodada==0,"Replay custa 20 e reseta rodada");
        for(int i=0;i<3;i++) {
            Layout();yield return Capturar("Pergunta"+(i+1)+"-1080p");
            int correta=quiz.dados.perguntas[i].respostaCorreta;quiz.Responder((correta+1)%4);Conferir(progresso.Coins==10+i*10&&quiz.PerguntaAtual==i,"Erro preserva saldo acumulado: pergunta "+(i+1));Terminar();
            yield return Clicar(ui.botoesRespostas[correta]);quiz.Responder(correta);Conferir(progresso.Coins==20+i*10&&progresso.CurrentXP==0,"Acerto +10 uma vez e sem EXP antecipado: "+(i+1));Terminar();
            if(i<2)Conferir(quiz.PerguntaAtual==i+1&&quiz.TempoRestante==30&&ui.progressoPerguntas.text==(i+2)+"/3","Próxima pergunta tem novo timer e contador");
        }
        Conferir(quiz.Estado==EstadoQuizAmazonia.Concluido&&quiz.MoedasDaRodada==30&&progresso.CurrentXP==50&&progresso.QuizAmazoniaConcluido&&progresso.PossuiChaveAmazonia&&quiz.ChaveConquistadaNestaRodada,"Três acertos: 30 Coins, 50 EXP, flag e primeira chave da Amazônia");
        Conferir(progresso.PossuiChaveVila&&progresso.QuizFeiraConcluido,"Chave da Vila e conclusão da Feira permanecem intactas");quiz.Simular(10);quiz.Responder(2);Conferir(progresso.CurrentXP==50&&progresso.Coins==40,"Conclusão não duplica rewards");
        Conferir(ui.enunciado.text.Contains("Chave da Amazônia conquistada!")&&ui.botaoVoltar.gameObject.activeSelf&&ui.botoesRespostas.All(b=>!b.gameObject.activeSelf),"Conclusão usa interface atual e retorno funcional");Layout();yield return Capturar("ConclusaoPrimeiraChave-1080p");
        yield return Clicar(ui.botaoVoltar);yield return Clicar(BotaoQuiz);AcertarTudo();
        Conferir(progresso.Coins==70&&progresso.Level==2&&progresso.CurrentXP==0&&progresso.Energia==40,"Segunda rodada dá outros 30 Coins e 50 EXP, Level segue 100 EXP");
        Conferir(!quiz.ChaveConquistadaNestaRodada&&progresso.PossuiChaveAmazonia&&progresso.QuizAmazoniaConcluido&&!ui.enunciado.text.Contains("Chave da Amazônia conquistada!"),"Replay preserva flag/chave e não anuncia nova chave");
        Conferir(ui.retrato.preserveAspect&&ui.retrato.rectTransform.anchoredPosition==posRetrato&&ui.retrato.rectTransform.sizeDelta==tamanhoRetrato,"Três retratos preservam exatamente o retângulo");yield return Capturar("ConclusaoReplay-1080p");
        string salvo=PlayerPrefs.GetString("NexaQuest.Testes.Quiz");JsonUtility.FromJsonOverwrite("{\"coins\":0,\"currentXP\":0,\"level\":1,\"energia\":0,\"possuiChaveVila\":false,\"quizFeiraConcluido\":false,\"quizAmazoniaConcluido\":false,\"possuiChaveAmazonia\":false}",progresso);progresso.CarregarProgresso();
        Conferir(progresso.Coins==70&&progresso.Energia==40&&progresso.Level==2&&progresso.PossuiChaveVila&&progresso.PossuiChaveAmazonia&&progresso.QuizFeiraConcluido&&progresso.QuizAmazoniaConcluido&&PlayerPrefs.GetString("NexaQuest.Testes.Quiz")==salvo,"Mesmo JSON persiste as duas chaves/flags e todos os saldos");
        yield return Clicar(ui.botaoVoltar);Energia(20);yield return Clicar(BotaoQuiz);Conferir(progresso.Energia==0&&quiz.Estado==EstadoQuizAmazonia.Pergunta,"Energia 20 inicia e vai exatamente a zero");quiz.Voltar();
        yield return Clicar(BotaoQuiz);Conferir(painel.EstaAberto&&!quiz.gameObject.activeSelf&&progresso.Energia==0&&painel.avisoEnergia.gameObject.activeInHierarchy,"Energia zero bloqueia com aviso existente");yield return Capturar("EnergiaInsuficiente-1080p");
        Energia(19);yield return Clicar(BotaoQuiz);Conferir(painel.EstaAberto&&progresso.Energia==19&&!quiz.gameObject.activeSelf,"Energia 19 também bloqueia sem desconto");
        painel.Fechar();yield return null;Conferir(!jogador.ControlsLocked&&Vector3.Distance(jogador.transform.position,posicao)<.03f,"Fechar painel libera Player na posição original");
        Conferir(ui.hudsCompartilhadas.All(h=>h.parent.name=="Canvas")&&painel.avisoEnergia.transform.parent.name=="PainelDesafiosFeira","HUDs e aviso retornam aos pais originais");
        Conferir(mundo.TravelTo(mundo.initialArea,mundo.initialSpawn),"Transição de retorno continua disponível");yield return new WaitForSecondsRealtime(1);jogador.enabled=true;
        Conferir(problemas.Count==0,"Console sem erros ou avisos");resultados.AddRange(problemas);resultados.Add("Falhas: "+falhas);File.WriteAllLines(Pasta+"quiz-amazonia-playmode.txt",resultados);Application.logMessageReceived-=Log;
        SessionState.SetBool("NexaQuest.QuizAmazonia.Aprovado",falhas==0);EditorApplication.isPlaying=false;
    }
}
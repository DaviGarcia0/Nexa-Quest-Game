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
public static class ValidacaoMinijogoFeira
{
    const string Pedido="Library/MinijogoFeira.solicitacao", Ativo="NexaQuest.ValidandoMinijogo",Inicio="NexaQuest.IniciarTesteMinijogo";
    static readonly BindingFlags Flags=BindingFlags.Static|BindingFlags.Public|BindingFlags.NonPublic;
    static ValidacaoMinijogoFeira(){
        EditorApplication.update+=Consultar;
        EditorApplication.playModeStateChanged+=estado=>{
            if(estado==PlayModeStateChange.EnteredPlayMode&&SessionState.GetBool(Inicio,false)){
                SessionState.SetBool(Inicio,false);Application.runInBackground=true;new GameObject("TesteMinijogo_Temporario").AddComponent<VerificacoesMinijogoFeira>();
            }
            if(estado==PlayModeStateChange.EnteredEditMode&&SessionState.GetBool(Ativo,false)){
                SessionState.SetBool(Ativo,false);var pasta=MontagemMinijogoFeira.Documentacao+"depois/";Directory.CreateDirectory(pasta);
                foreach(var par in new[]{new[]{EnergyHUDSetup.Docs+"energia-playmode.txt","energia.txt"},new[]{QuestHUDVisualSetup.Docs+"visual-playmode.txt","visual.txt"},new[]{ConfiguracaoDesafiosFeira.Pasta+"painel-playmode.txt","painel.txt"},new[]{"Documentation/Brasil/EtapaNPCs/playmode.txt","npcs.txt"},new[]{"Documentation/Brasil/EtapaPortal/playmode.txt","portal.txt"},new[]{"Documentation/Brasil/EtapaPortal/console.txt","console.txt"}})if(File.Exists(par[0]))File.Copy(par[0],pasta+par[1],true);
                typeof(QuestHUDVisualValidation).GetMethod("RestoreGameView",Flags).Invoke(null,null);
                File.WriteAllText(pasta+"concluido.txt",DateTime.Now.ToString("s"));
            }
        };
    }
    static void Consultar(){
        if(EditorApplication.isCompiling||EditorApplication.isUpdating||EditorApplication.isPlayingOrWillChangePlaymode||!File.Exists(Pedido))return;
        var acao=File.ReadAllText(Pedido).Trim();File.Delete(Pedido);
        try{
            var cena=SceneManager.GetActiveScene();if(cena.path!="Assets/Projeto/Scenes/Brasil.unity"||cena.isDirty)throw new InvalidOperationException("Brasil precisa estar salva e fora de Play Mode.");
            if(acao=="montar")MontagemMinijogoFeira.Montar();
            if(acao=="testar"){
                QuestHUDVisualValidation.CapturePlayerCollider();typeof(QuestHUDVisualValidation).GetMethod("SetGameView",Flags).Invoke(null,null);
                typeof(Editor).Assembly.GetType("UnityEditor.LogEntries").GetMethod("Clear",Flags).Invoke(null,null);
                SessionState.SetBool(Ativo,true);SessionState.SetBool(Inicio,true);
                PlayerPrefs.SetString("NexaQuest.Testes.Minijogo","{\"versao\":1,\"coins\":0,\"currentXP\":0,\"level\":1,\"energia\":100,\"possuiChaveVila\":false,\"instrucoesFeiraVistas\":false}");PlayerPrefs.Save();
                EditorApplication.isPaused=false;EditorApplication.isPlaying=true;
            }
        }catch(Exception erro){File.WriteAllText(MontagemMinijogoFeira.Documentacao+"falha.txt",erro.ToString());Debug.LogException(erro);}
    }
}
public class VerificacoesMinijogoFeira:MonoBehaviour
{
    readonly List<string> resultados=new List<string>(),problemas=new List<string>();int falhas;
    string Pasta=>MontagemMinijogoFeira.Documentacao;
    void Conferir(bool ok,string mensagem){resultados.Add((ok?"PASS: ":"FAIL: ")+mensagem);if(!ok)falhas++;File.WriteAllLines(Pasta+"andamento.txt",resultados);}
    void Log(string texto,string pilha,LogType tipo){if(tipo!=LogType.Log)problemas.Add(tipo+": "+texto);}
    IEnumerator Clicar(Transform pai,string caminho){
        yield return null;
        Canvas.ForceUpdateCanvases();var botao=pai.Find(caminho).GetComponent<Button>();var dados=new PointerEventData(EventSystem.current){position=botao.transform.position,button=PointerEventData.InputButton.Left};
        var acertos=new List<RaycastResult>();EventSystem.current.RaycastAll(dados,acertos);
        Conferir(acertos.Count>0&&acertos[0].gameObject==botao.gameObject,"Raycast do botao: "+caminho+" (alvo: "+(acertos.Count>0?acertos[0].gameObject.name:"nenhum")+")");
        if(acertos.Count==0)yield break;
        ExecuteEvents.Execute(acertos[0].gameObject,dados,ExecuteEvents.pointerDownHandler);ExecuteEvents.Execute(acertos[0].gameObject,dados,ExecuteEvents.pointerUpHandler);ExecuteEvents.Execute(acertos[0].gameObject,dados,ExecuteEvents.pointerClickHandler);
    }
    IEnumerator Capturar(string nome){yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Pasta+nome+".png");yield return null;}
    int Item(MinijogoFeira m,TipoItemFeira tipo,bool capturado){int id=m.queda.CriarItem(tipo,new Vector2(0,300),0);m.queda.Resolver(id,capturado);return id;}
    void Completar(MinijogoFeira m){for(int i=0;i<5;i++)foreach(var tipo in new[]{TipoItemFeira.Banana,TipoItemFeira.Abacaxi,TipoItemFeira.Melancia})Item(m,tipo,true);}
    IEnumerator Start(){
        Application.logMessageReceived+=Log;yield return null;yield return null;
        var mundo=FindObjectOfType<BrazilWorld>();var jogador=mundo.player;var progresso=ProgressionManager.Instance;
        var desafios=FindObjectOfType<PainelDesafiosFeira>(true);var mini=FindObjectOfType<MinijogoFeira>(true);var ui=mini.interfaceJogo;
        var energia=GameObject.Find("Canvas").transform.Find("EnergyHUD/EnergyText").GetComponent<TMPro.TMP_Text>();
        var posicaoOriginal=jogador.transform.position;
        Conferir(Screen.width==1920&&Screen.height==1080,"Resolucao 1920x1080");Conferir(!mini.gameObject.activeSelf&&mini.Estado==EstadoMinijogoFeira.Fechado,"Minijogo fechado inicialmente");
        Conferir(progresso.Energia==100&&energia.text=="100","Energia real inicial conectada a HUD");
        desafios.Abrir();yield return null;yield return Clicar(desafios.transform,"ArtePainel/BotaoMinijogo");yield return null;
        Conferir(mini.Estado==EstadoMinijogoFeira.Instrucoes&&ui.instrucoes.activeSelf,"Primeira entrada apresenta instrucoes");
        Conferir(progresso.Energia==100&&!progresso.InstrucoesFeiraVistas,"Instrucoes ainda nao gastam Energia");
        mini.quest.Mover(1,.1f);mini.queda.Simular(.1f);Conferir(mini.quest.Entrada==0&&mini.queda.QuantidadeAtiva==0,"Sem movimento ou spawn nas instrucoes");
        Conferir(jogador.ControlsLocked&&!mundo.TravelTo(mundo.areas[1],mundo.areas[1].defaultSpawn),"Mapa e transicoes bloqueados");
        FindObjectOfType<DialogueController>().HandleInteract();Conferir(!FindObjectOfType<DialogueController>().IsOpen,"Interacao com NPC bloqueada");
        yield return Capturar("Instrucoes-1080p");
        yield return Clicar(ui.instrucoes.transform,"ArtePainel/BotaoJogar");mini.IniciarPartida();yield return null;
        mini.queda.enabled=false;mini.quest.enabled=false;
        Conferir(mini.EmPartida&&progresso.Energia==80&&energia.text=="80","Jogar custa exatamente 20; clique repetido nao duplica custo");
        Conferir(progresso.InstrucoesFeiraVistas,"Instrucoes vistas salvas ao iniciar");
        foreach(var direcao in new[]{0f,-1f,1f}){
            var vistos=new HashSet<Sprite>();mini.quest.Mover(direcao,.1f);
            for(int i=0;i<25;i++){mini.quest.animador.Update(.11f);mini.quest.AlinharArte();vistos.Add(mini.quest.imagem.sprite);}
            Conferir(vistos.Count>=(direcao==0?4:6),"Animator mostra todos os frames na direcao "+direcao);
        }
        mini.quest.Mover(-1,10);Conferir(Mathf.Approximately(((RectTransform)mini.quest.transform).anchoredPosition.x,-550),"Limite esquerdo");
        mini.quest.Mover(1,10);Conferir(Mathf.Approximately(((RectTransform)mini.quest.transform).anchoredPosition.x,550),"Limite direito");
        Conferir(((RectTransform)mini.quest.transform).anchoredPosition.y==-385,"Sem movimento vertical");mini.quest.Reiniciar();mini.quest.animador.Update(0);mini.quest.AlinharArte();
        foreach(var tipo in Enum.GetValues(typeof(TipoItemFeira)).Cast<TipoItemFeira>())Conferir(mini.queda.CriarItem(tipo,new Vector2(-440+(int)tipo*170,330-(int)tipo*60),210)>0,"Item original importado: "+tipo);
        var alturas=mini.queda.camada.Cast<Transform>().ToDictionary(t=>t,t=>((RectTransform)t).anchoredPosition.y);mini.queda.Simular(.1f);
        Conferir(alturas.All(p=>Mathf.Approximately(((RectTransform)p.Key).anchoredPosition.y,p.Value-21)),"Os seis tipos caem verticalmente na velocidade configurada");
        yield return Capturar("Partida-1080p");mini.queda.Limpar();
        // Captura pela travessia da abertura, sem chamar diretamente a recompensa.
        int primeiro=mini.queda.CriarItem(TipoItemFeira.Banana,new Vector2(0,-85),210);mini.queda.Simular(.1f);
        Conferir(mini.Quantidade(TipoItemFeira.Banana)==1,"Fruta atravessa abertura e e capturada");
        Conferir(!mini.queda.Resolver(primeiro,true)&&mini.FrutasColetadas==1,"Mesmo item nao pode ser coletado novamente");
        Item(mini,TipoItemFeira.Abacaxi,true);Item(mini,TipoItemFeira.Melancia,true);
        Conferir(progresso.Coins==1&&mini.MoedasDaPartida==1,"Tres frutas diferentes concedem uma moeda real");
        mini.queda.CriarItem(TipoItemFeira.Banana,new Vector2(0,-340),210);mini.queda.Simular(.1f);
        Conferir(mini.FrutasColetadas==3,"Corpo e pes nao capturam fruta");mini.queda.Simular(.1f);
        Conferir(mini.FrutasPerdidas==1,"Fruta chega ao chao e soma um erro");
        Item(mini,TipoItemFeira.Disco,false);Conferir(mini.ItensErrados==0,"Objeto errado no chao nao penaliza");
        Item(mini,TipoItemFeira.Chinelo,true);Conferir(mini.ItensErrados==1,"Objeto errado capturado penaliza");
        yield return null;Conferir(ui.visual.anchoredPosition.magnitude<=6,"Impacto suave aplicado apenas ao visual do minijogo");
        yield return new WaitForSecondsRealtime(.2f);Conferir(ui.visual.anchoredPosition==Vector2.zero,"Impacto retorna exatamente a origem");
        Item(mini,TipoItemFeira.Banana,false);Item(mini,TipoItemFeira.Abacaxi,false);
        Conferir(mini.Estado==EstadoMinijogoFeira.Derrota&&ui.derrota.activeSelf,"Tres frutas perdidas antes de 5/5/5: derrota");
        Conferir(progresso.CurrentXP==0&&!progresso.PossuiChaveVila,"Derrota nao concede XP nem chave");yield return Capturar("Derrota-1080p");
        yield return Clicar(ui.derrota.transform,"ArtePainel/BotaoRejogar");yield return null;
        Conferir(progresso.Energia==60&&mini.FrutasColetadas==0&&mini.FrutasPerdidas==0&&mini.ItensErrados==0&&mini.MoedasDaPartida==0&&mini.queda.QuantidadeAtiva==0,"Rejogar custa 20 e limpa somente rodada");
        Conferir(progresso.Coins==1,"Rejogar preserva moedas anteriores");Completar(mini);
        Conferir(mini.ObjetivoConcluido&&mini.EmPartida&&!ui.vitoriaComChave.activeSelf,"5/5/5 nao interrompe gameplay");
        Conferir(progresso.CurrentXP==50&&progresso.PossuiChaveVila&&mini.ChaveConquistadaNestaPartida,"Primeira conclusao: 50 XP e chave unica");
        for(int i=0;i<3;i++)Item(mini,TipoItemFeira.Banana,true);
        Conferir(progresso.Coins==7&&mini.MoedasDaPartida==6&&progresso.CurrentXP==50,"Frutas extras dao moedas sem repetir XP");
        Conferir(ui.frutas.All(t=>t.text=="5/5"),"HUD limita contadores em 5/5");
        mini.queda.Reiniciar();for(int i=0;i<9;i++)mini.queda.Simular(.1f);
        Conferir(mini.queda.QuantidadeAtiva==1&&mini.EmPartida,"Surgimento automatico continua apos concluir objetivo");mini.queda.Limpar();
        yield return Clicar(mini.transform,"BotaoSair/AreaClique");yield return null;
        Conferir(mini.Estado==EstadoMinijogoFeira.Vitoria&&ui.vitoriaComChave.activeSelf&&ui.moedasPrimeiraVitoria.text=="+6","Sair apos concluir: primeira vitoria com moedas da rodada");yield return Capturar("PrimeiraVitoria-1080p");
        yield return Clicar(ui.vitoriaComChave.transform,"ArtePainel/BotaoRejogar");yield return null;Completar(mini);
        Conferir(progresso.Energia==40&&progresso.Level==2&&progresso.CurrentXP==0&&!mini.ChaveConquistadaNestaPartida,"Segunda conclusao: mais 50 XP, nivel 2 e nenhuma chave duplicada");
        Item(mini,TipoItemFeira.Chinelo,true);Item(mini,TipoItemFeira.Disco,true);Item(mini,TipoItemFeira.ConsoleXbox,true);
        Conferir(mini.Estado==EstadoMinijogoFeira.Vitoria&&ui.vitoriaSemChave.activeSelf&&!ui.vitoriaComChave.activeSelf,"Tres itens errados depois da conclusao: vitoria sem chave");yield return Capturar("OutraVitoria-1080p");
        yield return Clicar(ui.vitoriaSemChave.transform,"ArtePainel/BotaoSair");yield return null;
        Conferir(!mini.gameObject.activeSelf&&!jogador.ControlsLocked&&jogador.transform.position==posicaoOriginal,"Sair retorna a Brasil no mesmo ponto");
        desafios.Abrir();yield return null;yield return Clicar(desafios.transform,"ArtePainel/BotaoMinijogo");yield return null;
        Conferir(mini.EmPartida&&!ui.instrucoes.activeSelf&&progresso.Energia==20,"Nova entrada pula instrucoes e custa 20");
        Item(mini,TipoItemFeira.Banana,false);Item(mini,TipoItemFeira.Abacaxi,false);Item(mini,TipoItemFeira.Chinelo,true);Item(mini,TipoItemFeira.Disco,true);
        Conferir(mini.EmPartida&&mini.FrutasPerdidas==2&&mini.ItensErrados==2,"Dois contadores independentes: 2+2 nao encerra");
        Item(mini,TipoItemFeira.ConsoleXbox,true);Conferir(mini.Estado==EstadoMinijogoFeira.Derrota,"Tres itens errados sem objetivo: derrota");
        yield return Clicar(ui.derrota.transform,"ArtePainel/BotaoRejogar");yield return null;
        Conferir(mini.EmPartida&&progresso.Energia==0&&energia.text=="0","Energia 20 permite iniciar e chega exatamente a zero");
        yield return Clicar(mini.transform,"BotaoSair/AreaClique");yield return null;Conferir(mini.Estado==EstadoMinijogoFeira.Derrota,"Sair antes de concluir mostra derrota");
        yield return Clicar(ui.derrota.transform,"ArtePainel/BotaoRejogar");yield return null;
        Conferir(mini.Estado==EstadoMinijogoFeira.Derrota&&progresso.Energia==0&&ui.avisoPartida.gameObject.activeSelf,"Rejogar sem energia bloqueado com aviso");
        yield return Clicar(ui.derrota.transform,"ArtePainel/BotaoSair");yield return null;
        desafios.Abrir();yield return null;yield return Clicar(desafios.transform,"ArtePainel/BotaoMinijogo");yield return null;
        Conferir(desafios.EstaAberto&&mini.Estado==EstadoMinijogoFeira.Fechado&&ui.avisoEntrada.gameObject.activeSelf,"Entrada sem energia recusada sem fechar painel");desafios.Fechar();
        Conferir(!progresso.TentarGastarEnergia(20)&&!progresso.TentarGastarEnergia(-20)&&progresso.Energia==0,"Energia nao fica negativa nem permite custo negativo");
        int moedas=progresso.Coins;JsonUtility.FromJsonOverwrite("{\"coins\":0,\"currentXP\":0,\"level\":1,\"energia\":100,\"possuiChaveVila\":false,\"instrucoesFeiraVistas\":false}",progresso);progresso.CarregarProgresso();
        Conferir(progresso.Coins==moedas&&progresso.Level==2&&progresso.Energia==0&&progresso.PossuiChaveVila&&progresso.InstrucoesFeiraVistas,"Carregamento restaura moedas, nivel, energia, chave e instrucoes salvos");
        Conferir(!progresso.ConquistarChaveVila(),"Chave persistida recusa segunda concessao");
        Conferir(!jogador.ControlsLocked&&mini.queda.QuantidadeAtiva==0&&ui.visual.anchoredPosition==Vector2.zero,"Saida limpa itens, bloqueio e deslocamento visual");
        Conferir(ui.hudsCompartilhadas.All(h=>h.parent.name=="Canvas"),"HUDs existentes retornam ao Canvas");
        Conferir(problemas.Count==0,"Sem erros ou avisos durante os cenarios A-G");resultados.AddRange(problemas);resultados.Add("Falhas: "+falhas);File.WriteAllLines(Pasta+"minijogo-playmode.txt",resultados);
        // Progresso de teste separado do jogador; regressao com seus valores iniciais conhecidos.
        PlayerPrefs.SetString("NexaQuest.Testes.Minijogo","{\"versao\":1,\"coins\":0,\"currentXP\":0,\"level\":1,\"energia\":100,\"possuiChaveVila\":false,\"instrucoesFeiraVistas\":false}");PlayerPrefs.Save();progresso.CarregarProgresso();
        mini.queda.enabled=true;mini.quest.enabled=true;Application.logMessageReceived-=Log;
        if(falhas>0){EditorApplication.isPlaying=false;yield break;}
        new GameObject("RegressaoDesafios_Temporaria").AddComponent<TestesDesafiosFeira>();Destroy(gameObject);
    }
}

using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using NexaQuest.Brasil;

[InitializeOnLoad]
public static class ConfiguracaoDesafiosFeira
{
    public const string Pasta = "Documentation/Brasil/DesafiosFeira20261003/";
    public const string Arte = "Assets/Projeto/Sprites/Brasil/Interface/PainelDesafiosFeira.png";
    const string Solicitacao = "Library/DesafiosFeira.solicitacao";
    const string Ativo = "NexaQuest.TesteDesafiosFeira", Iniciar = "NexaQuest.IniciarDesafiosFeira";
    static readonly BindingFlags Flags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

    static ConfiguracaoDesafiosFeira()
    {
        EditorApplication.update += Consultar;
        EditorApplication.playModeStateChanged += estado => {
            if (estado == PlayModeStateChange.EnteredPlayMode && SessionState.GetBool(Iniciar, false)) {
                SessionState.SetBool(Iniciar, false);
                Application.runInBackground = true; var teste = new GameObject("ValidacaoDesafios_Temporaria").AddComponent<TestesDesafiosFeira>(); File.WriteAllText(Pasta+"executor.txt", "Criado: "+(teste!=null)+"; pausado: "+EditorApplication.isPaused+"; tempo: "+Time.timeScale);
            }
            if (estado == PlayModeStateChange.EnteredEditMode && SessionState.GetBool(Ativo, false)) {
                SessionState.SetBool(Ativo, false);
                Directory.CreateDirectory(Pasta + "depois");
                foreach (var item in new[] {
                    new[]{EnergyHUDSetup.Docs+"energia-playmode.txt", "energia.txt"},
                    new[]{QuestHUDVisualSetup.Docs+"visual-playmode.txt", "visual.txt"},
                    new[]{"Documentation/Brasil/EtapaNPCs/playmode.txt", "npcs.txt"},
                    new[]{"Documentation/Brasil/EtapaPortal/playmode.txt", "portal.txt"},
                    new[]{"Documentation/Brasil/EtapaPortal/console.txt", "console.txt"}})
                    if(File.Exists(item[0])) File.Copy(item[0], Pasta+"depois/"+item[1], true);
                Tela("RestoreGameView");
                File.WriteAllText(Pasta+"depois/concluido.txt", DateTime.Now.ToString("s"));
            }
        };
    }

    static void Tela(string metodo) => typeof(QuestHUDVisualValidation).GetMethod(metodo, Flags).Invoke(null, null);
    static void Consultar()
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode || !File.Exists(Solicitacao)) return;
        var comando = File.ReadAllText(Solicitacao).Trim(); File.Delete(Solicitacao);
        try {
            var cena = SceneManager.GetActiveScene();
            if (cena.path != "Assets/Projeto/Scenes/Brasil.unity" || cena.isDirty)
                throw new InvalidOperationException("Abra e salve Brasil fora de Play Mode.");
            if (comando == "montar") Montar();
            if (comando == "testar") {
                Directory.CreateDirectory(Pasta);
                QuestHUDVisualValidation.CapturePlayerCollider(); Tela("SetGameView");
                typeof(Editor).Assembly.GetType("UnityEditor.LogEntries").GetMethod("Clear", Flags).Invoke(null, null);
                SessionState.SetBool(Ativo, true); SessionState.SetBool(Iniciar, true);
                EditorApplication.isPaused = false; EditorWindow.GetWindow(typeof(Editor).Assembly.GetType("UnityEditor.GameView")).Focus(); EditorApplication.isPlaying = true;
            }
        } catch (Exception erro) {
            Directory.CreateDirectory(Pasta); File.WriteAllText(Pasta+"falha.txt", erro.ToString()); Debug.LogException(erro);
        }
    }

    static RectTransform Objeto(string nome, Transform pai, Vector2 tamanho)
    {
        var retangulo = new GameObject(nome, typeof(RectTransform)).GetComponent<RectTransform>();
        retangulo.gameObject.layer = 5; retangulo.SetParent(pai, false);
        retangulo.sizeDelta = tamanho; return retangulo;
    }
    static Button Botao(string nome, Transform pai, Rect origem, UnityEngine.Events.UnityAction acao)
    {
        var retangulo = Objeto(nome, pai, origem.size * (1280f/1448f));
        retangulo.anchoredPosition = new Vector2(origem.center.x-724f, 543f-origem.center.y) * (1280f/1448f);
        var imagem = retangulo.gameObject.AddComponent<Image>(); imagem.color = Color.clear;
        var botao = retangulo.gameObject.AddComponent<Button>(); botao.targetGraphic = imagem;
        botao.transition = Selectable.Transition.None;
        botao.navigation = new Navigation { mode = Navigation.Mode.None };
        UnityEventTools.AddPersistentListener(botao.onClick, acao); return botao;
    }

    public static void Montar()
    {
        var mundo = UnityEngine.Object.FindObjectOfType<BrazilWorld>();
        var canvas = UnityEngine.Object.FindObjectOfType<DialogueController>().GetComponentInParent<Canvas>();
        if (canvas == null) canvas = GameObject.Find("Canvas").GetComponent<Canvas>();
        if (canvas.transform.Find("PainelDesafiosFeira") != null) throw new InvalidOperationException("Painel já existe; montagem não deve duplicá-lo.");
        var npc = UnityEngine.Object.FindObjectsOfType<NPCInteraction>(true).Single(n => n.name == "NPC_Desafios_Bento");
        if (npc.onDialogueFinished.GetPersistentEventCount() != 0) throw new InvalidOperationException("Bento já possui uma integração; revisar antes de mudar.");
        var importador = (TextureImporter)AssetImporter.GetAtPath(Arte);
        importador.textureType = TextureImporterType.Sprite; importador.spriteImportMode = SpriteImportMode.Single;
        importador.filterMode = FilterMode.Point; importador.textureCompression = TextureImporterCompression.Uncompressed;
        importador.mipmapEnabled = false; importador.maxTextureSize = 2048; importador.alphaIsTransparency = true;
        importador.spritePixelsPerUnit = 100; importador.npotScale = TextureImporterNPOTScale.None;
        importador.SaveAndReimport();
        var retratoBento = npc.dialogue.lines.First(l => l.speaker == DialogueSpeaker.NPC).portrait;
        var retratoQuest = npc.dialogue.lines.First(l => l.speaker == DialogueSpeaker.Quest).portrait;
        string[] falas = {
            "Ei, Quest! Preparada para os desafios da Feira Brasileira?",
            "Claro! O que eu preciso fazer?",
            "Aqui temos dois desafios: um Quiz e um Minijogo.",
            "Complete os dois e eu entrego a Chave da Feira.",
            "Uma das chaves do Portal?",
            "Isso! Você vai precisar das três para ativá-lo.",
            "Escolha por qual desafio quer começar!"
        };
        npc.dialogue.lines = falas.Select((fala, i) => new DialogueLine {
            speaker = i==1 || i==4 ? DialogueSpeaker.Quest : DialogueSpeaker.NPC,
            speakerName = i==1 || i==4 ? "Quest" : "Bento",
            portrait = i==1 || i==4 ? retratoQuest : retratoBento, text = fala
        }).ToArray();
        EditorUtility.SetDirty(npc.dialogue);
        if (canvas.GetComponent<GraphicRaycaster>() == null) canvas.gameObject.AddComponent<GraphicRaycaster>();
        if (UnityEngine.Object.FindObjectOfType<EventSystem>(true) == null)
            new GameObject("SistemaEventos", typeof(EventSystem), typeof(StandaloneInputModule));
        var raiz = Objeto("PainelDesafiosFeira", canvas.transform, Vector2.zero);
        raiz.anchorMin = Vector2.zero; raiz.anchorMax = Vector2.one; raiz.offsetMin = raiz.offsetMax = Vector2.zero;
        raiz.gameObject.AddComponent<Image>().color = new Color(0,0,0,.55f);
        var painel = raiz.gameObject.AddComponent<PainelDesafiosFeira>();
        var dados = new SerializedObject(painel); dados.FindProperty("mundo").objectReferenceValue = mundo; dados.ApplyModifiedPropertiesWithoutUndo();
        var arte = Objeto("ArtePainel", raiz, new Vector2(1280,960));
        var imagemArte = arte.gameObject.AddComponent<Image>();
        imagemArte.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(Arte); imagemArte.preserveAspect = true; imagemArte.raycastTarget = false;
        Botao("BotaoQuiz", arte, new Rect(250,570,430,172), painel.SelecionarQuiz);
        Botao("BotaoMinijogo", arte, new Rect(768,570,432,172), painel.SelecionarMinijogo);
        raiz.SetAsLastSibling(); mundo.fadePanel.transform.SetAsLastSibling(); raiz.gameObject.SetActive(false);
        UnityEventTools.AddPersistentListener(npc.onDialogueFinished, painel.Abrir);
        PrefabUtility.RecordPrefabInstancePropertyModifications(npc);
        EditorUtility.SetDirty(npc);
        AssetDatabase.SaveAssets(); EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        Directory.CreateDirectory(Pasta); File.WriteAllText(Pasta+"montagem.txt", "Montagem concluída: "+DateTime.Now.ToString("s"));
    }
}

public class TestesDesafiosFeira : MonoBehaviour
{
    readonly List<string> resultados = new List<string>(), problemas = new List<string>(), mensagens = new List<string>();
    int falhas;
    void Conferir(bool correto, string descricao) { resultados.Add((correto ? "PASS: " : "FAIL: ")+descricao); if(!correto) falhas++; File.WriteAllLines(ConfiguracaoDesafiosFeira.Pasta+"andamento.txt", resultados); }
    void Registrar(string mensagem, string pilha, LogType tipo) {
        if(tipo==LogType.Log) mensagens.Add(mensagem); else problemas.Add(tipo+": "+mensagem);
    }
    GameObject Alvo(Vector2 ponto) {
        var dados = new PointerEventData(EventSystem.current) { position = ponto };
        var acertos = new List<RaycastResult>(); EventSystem.current.RaycastAll(dados, acertos);
        return acertos.Count == 0 ? null : acertos[0].gameObject;
    }
    void Clicar(Vector2 ponto) {
        var alvo = Alvo(ponto); if(alvo==null) return;
        var dados = new PointerEventData(EventSystem.current) { position=ponto, button=PointerEventData.InputButton.Left };
        ExecuteEvents.Execute(alvo, dados, ExecuteEvents.pointerDownHandler);
        ExecuteEvents.Execute(alvo, dados, ExecuteEvents.pointerUpHandler);
        ExecuteEvents.Execute(alvo, dados, ExecuteEvents.pointerClickHandler);
    }
    IEnumerator Mover(PlayerMovement jogador) {
        jogador.SetMovementInput(Vector2.right);
        for(int i=0;i<4;i++) { jogador.SendMessage("FixedUpdate"); yield return new WaitForFixedUpdate(); }
        jogador.SetMovementInput(Vector2.zero);
    }
    IEnumerator Start() {
        File.WriteAllText(ConfiguracaoDesafiosFeira.Pasta+"andamento.txt", "Inicio do teste"); Application.logMessageReceived += Registrar;
        yield return null; yield return null;
        var mundo=FindObjectOfType<BrazilWorld>(); var jogador=mundo.player;
        var dialogo=FindObjectOfType<DialogueController>(); var painel=FindObjectOfType<PainelDesafiosFeira>(true);
        var bento=FindObjectsOfType<NPCInteraction>().Single(n=>n.name=="NPC_Desafios_Bento");
        Conferir(Screen.width==1920&&Screen.height==1080,"Game View 1920x1080");
        Conferir(!painel.gameObject.activeSelf&&!jogador.ControlsLocked,"Painel inicia desativado; controle liberado");
        Conferir(bento.onDialogueFinished.GetPersistentEventCount()==1&&bento.onDialogueFinished.GetPersistentTarget(0)==painel&&bento.onDialogueFinished.GetPersistentMethodName(0)=="Abrir","Evento persistente do Bento abre o painel");
        Conferir(FindObjectsOfType<EventSystem>().Length==1,"Um unico EventSystem ativo");
        jogador.enabled=false; var inicio=jogador.transform.position;
        yield return Mover(jogador); Conferir(Vector3.Distance(inicio,jogador.transform.position)>.02f,"Movimento antes da conversa");
        jogador.Teleport(bento.transform.position+Vector3.down*.35f); Physics2D.SyncTransforms();
        yield return new WaitForFixedUpdate(); yield return new WaitForFixedUpdate(); yield return null;
        Conferir(dialogo.interactionPrompt.activeSelf,"Proximidade mostra E");
        dialogo.HandleInteract(); yield return null;
        Conferir(dialogo.IsOpen&&dialogo.CurrentNPC==bento,"Acao da tecla E inicia conversa com Bento");
        Conferir(bento.dialogue.lines.Length==7,"Conversa curta com sete falas");
        var texto=string.Join(" ",bento.dialogue.lines.Select(l=>l.text));
        Conferir(texto.Contains("Quiz e um Minijogo")&&texto.Contains("cinco frutas")&&texto.Contains("Chave da Vila")&&texto.Contains("Portal")&&texto.Contains("três"),"Regra do minijogo, chave e tres chaves do Portal");
        for(int i=0;i<bento.dialogue.lines.Length;i++) {
            var fala=bento.dialogue.lines[i];
            Conferir(dialogo.LineIndex==i&&dialogo.portrait.sprite==fala.portrait&&fala.portrait!=null&&dialogo.speakerName.text==fala.speakerName&&dialogo.dialogueText.text==fala.text,"Retrato/nome/texto: fala "+(i+1));
            Conferir(jogador.ControlsLocked&&!painel.EstaAberto,"Bloqueio permanece durante fala "+(i+1));
            dialogo.dialogueText.ForceMeshUpdate(); Conferir(!dialogo.dialogueText.isTextOverflowing,"Fala cabe no painel: "+(i+1));
            dialogo.HandleInteract(); yield return null;
        }
        Conferir(!dialogo.IsOpen&&!dialogo.panel.activeSelf&&painel.EstaAberto&&jogador.ControlsLocked,"Fim abre modal e mantem jogador bloqueado");
        Conferir(!dialogo.interactionPrompt.activeSelf,"Indicacao E oculta durante modal");
        dialogo.HandleInteract(); yield return null; Conferir(!dialogo.IsOpen,"E nao inicia outra conversa durante modal");
        inicio=jogador.transform.position; yield return Mover(jogador);
        Conferir(Vector3.Distance(inicio,jogador.transform.position)<.001f&&jogador.MoveInput==Vector2.zero,"Movimento recusado durante modal");
        Conferir(!mundo.TravelTo(mundo.areas[1],mundo.areas[1].defaultSpawn)&&!mundo.IsTransitioning,"Transicao recusada durante modal");
        var arte=painel.transform.Find("ArtePainel").GetComponent<Image>();
        Conferir(arte.preserveAspect&&arte.rectTransform.rect.size==new Vector2(1280,960)&&arte.sprite.texture.width==1448&&arte.sprite.texture.height==1086,"Arte original integral em proporcao 4:3");
        var cantos=new Vector3[4]; arte.rectTransform.GetWorldCorners(cantos);
        Conferir(cantos[0].x>=0&&cantos[0].y>=0&&cantos[2].x<=1920&&cantos[2].y<=1080,"Painel inteiro dentro da tela");
        foreach(var nome in new[]{"BotaoQuiz","BotaoMinijogo"}) {
            var botao=arte.transform.Find(nome).GetComponent<Button>(); var retangulo=botao.GetComponent<RectTransform>();
            Conferir(botao.GetComponent<Image>().color.a==0&&botao.GetComponentsInChildren<TMPro.TMP_Text>().Length==0,"Botao transparente sem texto duplicado: "+nome);
            var centro=(Vector2)retangulo.position;
            Conferir(Alvo(centro)==botao.gameObject,"Raycast acerta centro: "+nome);
            retangulo.GetWorldCorners(cantos);
            foreach(var ponto in new[]{cantos[0]+new Vector3(2,2),cantos[1]+new Vector3(2,-2),cantos[2]+new Vector3(-2,-2),cantos[3]+new Vector3(-2,2)})
                Conferir(Alvo(ponto)==botao.gameObject,"Raycast acerta canto interno: "+nome);
            Conferir(Alvo(new Vector2(cantos[0].x-3,centro.y))!=botao.gameObject&&Alvo(new Vector2(cantos[2].x+3,centro.y))!=botao.gameObject,"Clique fora da borda nao aciona: "+nome);
            Clicar(centro); yield return null;
            if(nome=="BotaoMinijogo" && painel.minijogo!=null){
                Conferir(painel.minijogo.Estado==EstadoMinijogoFeira.Instrucoes,"Minijogo abre instrucoes pela primeira vez");
                painel.minijogo.Sair();painel.Abrir();yield return null;
            }
        }
        Conferir(mensagens.Count(m=>m.StartsWith("Quiz da Feira selecionado"))==1,"Clique real UI chama Quiz uma vez");
        Conferir(painel.minijogo!=null,"Botao Minijogo integrado ao controlador real");
        Conferir(Alvo(new Vector2(15,15))==painel.gameObject,"Fundo modal intercepta cliques fora das opcoes");
        int antes=mensagens.Count; Clicar(new Vector2(15,15));
        Conferir(mensagens.Count==antes&&painel.EstaAberto,"Clique externo nao seleciona nem fecha");
        Conferir(ProgressionManager.Instance.Coins==0&&ProgressionManager.Instance.CurrentXP==0&&ProgressionManager.Instance.Level==1,"Abrir e clicar nao concede moedas ou XP");
        yield return new WaitForEndOfFrame(); ScreenCapture.CaptureScreenshot(ConfiguracaoDesafiosFeira.Pasta+"Painel-1920x1080.png"); yield return null;
        painel.ProcessarFechamento(false); Conferir(painel.EstaAberto,"Sem Esc modal permanece aberto");
        painel.ProcessarFechamento(true); yield return null;
        Conferir(!painel.EstaAberto&&!painel.gameObject.activeSelf&&!jogador.ControlsLocked,"Acao de Esc fecha e devolve controle");
        jogador.Teleport(mundo.initialSpawn.position); Physics2D.SyncTransforms(); yield return new WaitForFixedUpdate();
        inicio=jogador.transform.position; yield return Mover(jogador);
        Conferir(Vector3.Distance(inicio,jogador.transform.position)>.02f,"Movimento restaurado depois de fechar");
        painel.Abrir(); painel.Abrir(); Conferir(painel.EstaAberto&&jogador.ControlsLocked,"Reabrir nao duplica bloqueio");
        painel.gameObject.SetActive(false); Conferir(!jogador.ControlsLocked,"Desativar painel tambem libera bloqueio proprio");
        jogador.SetControlsLocked(true); painel.Abrir(); Conferir(!painel.EstaAberto&&jogador.ControlsLocked,"Modal nao rouba bloqueio de outro sistema"); jogador.SetControlsLocked(false);
        jogador.Teleport(mundo.initialSpawn.position); jogador.enabled=true;
        Conferir(problemas.Count==0,"Sem erros ou avisos no fluxo do painel");
        resultados.AddRange(problemas); resultados.Add("Falhas: "+falhas);
        File.WriteAllLines(ConfiguracaoDesafiosFeira.Pasta+"painel-playmode.txt",resultados);
        Application.logMessageReceived-=Registrar;
        new GameObject("RegressaoEnergia_Temporaria").AddComponent<EnergyHUDPlayChecks>(); Destroy(gameObject);
    }
}

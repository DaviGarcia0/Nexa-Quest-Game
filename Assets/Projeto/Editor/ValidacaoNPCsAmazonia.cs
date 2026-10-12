using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using NexaQuest.Brasil;

[InitializeOnLoad]
public static class ValidacaoNPCsAmazonia
{
    const string Pedido="Library/NPCsAmazonia.solicitacao",Inicio="NexaQuest.Amazonia.Inicio",Fim="NexaQuest.Amazonia.Fim";
    static ValidacaoNPCsAmazonia()
    {
        EditorApplication.update+=Consultar;
        EditorApplication.playModeStateChanged+=estado=>{
            if(estado==PlayModeStateChange.EnteredPlayMode && SessionState.GetBool(Inicio,false)) {
                SessionState.SetBool(Inicio,false);Application.runInBackground=true;
                new GameObject("ValidacaoAmazonia_Temporaria").AddComponent<VerificacoesNPCsAmazonia>();
            }
            if(estado==PlayModeStateChange.EnteredEditMode && SessionState.GetBool(Fim,false)) {
                SessionState.SetBool(Fim,false);SessionState.SetBool("NexaQuest.ValidandoMinijogo",false);
                typeof(QuestHUDVisualValidation).GetMethod("RestoreGameView",BindingFlags.Static|BindingFlags.NonPublic|BindingFlags.Public).Invoke(null,null);
                File.WriteAllText(MontagemNPCsAmazonia.Docs+"estado-editor.txt",SceneManager.GetActiveScene().path+"\nDirty: "+SceneManager.GetActiveScene().isDirty+"\nPlay Mode: "+EditorApplication.isPlaying);
                if(SessionState.GetBool("NexaQuest.Amazonia.Aprovado",false)) File.WriteAllText("Library/QuizFeira.solicitacao","testar");
            }
        };
    }
    static void Consultar()
    {
        if(EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode || !File.Exists(Pedido)) return;
        string acao=File.ReadAllText(Pedido).Trim();File.Delete(Pedido);
        try {
            if(acao=="recarregar") {
                if(SceneManager.GetActiveScene().isDirty)throw new InvalidOperationException("Há alterações não salvas na cena.");
                UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Projeto/Scenes/Brasil.unity");
            }
            else if(acao=="montar")MontagemNPCsAmazonia.Montar();
            else if(acao=="testar") {
                var cena=SceneManager.GetActiveScene();if(cena.isDirty || cena.path!="Assets/Projeto/Scenes/Brasil.unity")throw new InvalidOperationException("Salve Brasil antes de testar.");
                Directory.CreateDirectory(MontagemNPCsAmazonia.Docs);
                typeof(QuestHUDVisualValidation).GetMethod("SetGameView",BindingFlags.Static|BindingFlags.Public|BindingFlags.NonPublic).Invoke(null,null);
                typeof(Editor).Assembly.GetType("UnityEditor.LogEntries").GetMethod("Clear",BindingFlags.Static|BindingFlags.Public).Invoke(null,null);
                SessionState.SetBool("NexaQuest.ValidandoQuiz",false);SessionState.SetBool("NexaQuest.ValidandoMinijogo",true);
                PlayerPrefs.SetString("NexaQuest.Testes.Minijogo","{\"versao\":1,\"coins\":0,\"currentXP\":0,\"level\":1,\"energia\":100,\"possuiChaveVila\":false,\"instrucoesFeiraVistas\":false}");
                SessionState.SetBool(Inicio,true);SessionState.SetBool(Fim,true);EditorApplication.isPlaying=true;
            }
        }catch(Exception e){Directory.CreateDirectory(MontagemNPCsAmazonia.Docs);File.WriteAllText(MontagemNPCsAmazonia.Docs+"falha.txt",e.ToString());Debug.LogException(e);}
    }
}
public class VerificacoesNPCsAmazonia:MonoBehaviour
{
    readonly List<string> resultados=new List<string>(),problemas=new List<string>();int falhas;
    void Conferir(bool condicao,string texto){resultados.Add((condicao?"PASS: ":"FAIL: ")+texto);if(!condicao)falhas++;File.WriteAllLines(MontagemNPCsAmazonia.Docs+"andamento.txt",resultados);}
    void Log(string mensagem,string pilha,LogType tipo){if(tipo!=LogType.Log)problemas.Add(tipo+": "+mensagem);}
    IEnumerator Capturar(string nome){yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(MontagemNPCsAmazonia.Docs+nome+".png");yield return null;}
    IEnumerator Conversar(NPCInteraction npc,DialogueController dc,BrazilWorld mundo)
    {
        mundo.player.Teleport(npc.transform.position+Vector3.down*.35f);Physics2D.SyncTransforms();
        yield return new WaitForFixedUpdate();yield return new WaitForFixedUpdate();yield return null;
        Conferir(dc.interactionPrompt.activeSelf,"E aparece ao se aproximar: "+npc.name);
        dc.HandleInteract();yield return null;
        Conferir(dc.IsOpen&&dc.CurrentNPC==npc&&mundo.player.ControlsLocked,"E abre diálogo e bloqueia Quest: "+npc.name);
        var posicao=mundo.player.transform.position;
        foreach(var fala in npc.dialogue.lines) {
            Conferir(dc.speakerName.text==fala.speakerName && dc.portrait.sprite==fala.portrait && dc.dialogueText.text==fala.text,"Fala/retrato/ordem: "+npc.name+" "+dc.LineIndex);
            dc.dialogueText.ForceMeshUpdate();Conferir(!dc.dialogueText.isTextOverflowing,"Texto cabe: "+npc.name+" "+dc.LineIndex);
            if(dc.LineIndex<2)yield return Capturar(npc.name+"-fala"+dc.LineIndex);
            yield return null;dc.HandleInteract();yield return null;
        }
        var painelAmazonia=FindObjectOfType<PainelDesafiosAmazonia>(true);
        if(npc.name=="NPC_Amazonia_01" && painelAmazonia!=null) {
            Conferir(painelAmazonia.EstaAberto&&mundo.player.ControlsLocked,"Fim da Guardiã abre o painel da Amazônia");painelAmazonia.Fechar();yield return null;
        }
        Conferir(!dc.IsOpen&&!mundo.player.ControlsLocked&&Vector3.Distance(mundo.player.transform.position,posicao)<.03f,"Fim restaura controle/posição: "+npc.name+" (deslocamento: "+Vector3.Distance(mundo.player.transform.position,posicao)+", diálogo: "+dc.IsOpen+", bloqueio: "+mundo.player.ControlsLocked+")");
    }
    IEnumerator Start()
    {
        Application.logMessageReceived+=Log;yield return null;yield return null;
        var mundo=FindObjectOfType<BrazilWorld>();var dc=FindObjectOfType<DialogueController>();var progresso=ProgressionManager.Instance;
        mundo.player.enabled=false;
        Conferir(Screen.width==1920&&Screen.height==1080,"Play Mode 1920x1080");
        var lia=mundo.areas.Single(a=>a.name=="Feira").transform.Find("NPCs/NPC_Introducao_Lia").GetComponent<NPCInteraction>();
        Conferir(lia.dialogue.lines.Length==13 && lia.dialogue.lines.Count(l=>l.speaker==DialogueSpeaker.Quest)==5,"Lia mantém identidade e conversa natural de 13 falas");
        Conferir(lia.dialogue.lines.Any(l=>l.text.Contains("Bento"))&&lia.dialogue.lines.Any(l=>l.text.Contains("Ponto Turístico"))&&lia.dialogue.lines.Any(l=>l.text.Contains("três chaves")),"Introdução explica percurso e três chaves");
        yield return Conversar(lia,dc,mundo);
        var area=mundo.areas.Single(a=>a.name=="Amazonia");
        Conferir(mundo.TravelTo(area,area.defaultSpawn),"Transição para Amazônia aceita");
        yield return new WaitForSecondsRealtime(1);Conferir(mundo.CurrentArea==area&&!mundo.IsTransitioning&&!mundo.player.ControlsLocked,"Entrada na Amazônia termina sem bloqueio");
        var raiz=area.transform.Find("NPCsAmazonia");Conferir(raiz.childCount==4,"Quatro NPCs na Amazônia");
        Vector2[] pontos={new Vector2(1004,183),new Vector2(407,247),new Vector2(239,379),new Vector2(1202,156)};
        for(int n=1;n<=4;n++) {
            var npc=raiz.Find("NPC_Amazonia_0"+n);var render=npc.GetComponentInChildren<SpriteRenderer>();var animador=npc.GetComponentInChildren<Animator>();
            var local=area.map.transform.InverseTransformPoint(npc.position);var pixel=new Vector2(local.x,-local.y)*area.map.sprite.pixelsPerUnit+area.map.sprite.rect.size/2;
            Conferir(Vector2.Distance(pixel,pontos[n-1])<.1f,"Posição corresponde à marcação: NPC "+n);
            Conferir(render.sprite!=null&&render.sprite.name.StartsWith("NPCAmazonia0"+n),"Sheet correto: NPC "+n);
            var clip=animador.runtimeAnimatorController.animationClips.Single();
            Conferir(AnimationUtility.GetAnimationClipSettings(clip).loopTime&&clip.frameRate==3,"Idle em loop a 3 FPS: NPC "+n);
            float antes=animador.GetCurrentAnimatorStateInfo(0).normalizedTime;yield return new WaitForSecondsRealtime(.1f);
            Conferir(animador.GetCurrentAnimatorStateInfo(0).normalizedTime>antes,"Animator avança em Play: NPC "+n);
            Conferir(n==1?npc.GetComponent<NPCInteraction>()!=null:npc.GetComponent<NPCInteraction>()==null,"Interação somente no NPC 1: NPC "+n);
            if(n>1) {
                Conferir(npc.GetComponents<Collider2D>().Length==0,"Decorativo sem área de interação: NPC "+n);
                mundo.player.Teleport(npc.position+Vector3.down*.25f);Physics2D.SyncTransforms();yield return new WaitForFixedUpdate();yield return new WaitForFixedUpdate();yield return null;
                Conferir(!dc.interactionPrompt.activeSelf,"Decorativo não mostra E: NPC "+n);
                dc.HandleInteract();yield return null;Conferir(!dc.IsOpen,"Decorativo não abre diálogo: NPC "+n);
            }
        }
        var especial=raiz.Find("NPC_Amazonia_01").GetComponent<NPCInteraction>();
        Conferir(especial.GetComponent<CircleCollider2D>().isTrigger&&!especial.GetComponent<BoxCollider2D>().isTrigger,"NPC 1 tem gatilho e corpo separados");
        Conferir(especial.onDialogueFinished.GetPersistentEventCount()==1 && especial.onDialogueFinished.GetPersistentTarget(0) is PainelDesafiosAmazonia,"Evento conectado ao painel real da Amazônia");
        yield return Conversar(especial,dc,mundo);
        var camera=mundo.followCamera.GetComponent<Camera>();if(camera==null)camera=Camera.main;
        var pixelPerfect=camera.GetComponents<Behaviour>().FirstOrDefault(c=>c.GetType().Name=="PixelPerfectCamera");
        if(pixelPerfect!=null)pixelPerfect.enabled=false;
        var viewport=camera.rect;camera.rect=new Rect(0,0,1,1);
        mundo.followCamera.enabled=false;var posCamera=camera.transform.position;float tamanho=camera.orthographicSize;
        camera.transform.position=new Vector3(area.map.bounds.center.x,area.map.bounds.center.y,-10);camera.orthographicSize=area.map.bounds.size.y*.53f;
        yield return Capturar("Amazonia-posicionamento-1080p");
        camera.transform.position=raiz.Find("NPC_Amazonia_01").position+new Vector3(0,.2f,-10);camera.orthographicSize=1.5f;
        yield return Capturar("Amazonia-templo-1080p");
        camera.transform.position=posCamera;camera.orthographicSize=tamanho;camera.rect=viewport;if(pixelPerfect!=null)pixelPerfect.enabled=true;mundo.followCamera.enabled=true;
        Conferir(progresso.Coins==0&&progresso.CurrentXP==0&&progresso.Energia==100&&!progresso.PossuiChaveVila,"NPCs não alteram Energia, Coins, EXP ou Chave da Vila");
        Conferir(mundo.TravelTo(mundo.initialArea,mundo.initialSpawn),"Retorno à Feira aceito");yield return new WaitForSecondsRealtime(1);
        Conferir(mundo.CurrentArea==mundo.initialArea&&!mundo.player.ControlsLocked,"Feira volta com controles livres");mundo.player.enabled=true;
        Conferir(problemas.Count==0,"Console sem erros ou avisos");resultados.AddRange(problemas);resultados.Add("Falhas: "+falhas);
        File.WriteAllLines(MontagemNPCsAmazonia.Docs+"amazonia-playmode.txt",resultados);
        Application.logMessageReceived-=Log;SessionState.SetBool("NexaQuest.Amazonia.Aprovado",falhas==0);EditorApplication.isPlaying=false;
    }
}
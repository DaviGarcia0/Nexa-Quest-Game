using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using NexaQuest.Brasil;

[InitializeOnLoad]
public static class FeiraNPCValidation
{
    const string Key="NexaQuest.NPCValidation";
    static FeiraNPCValidation(){EditorApplication.playModeStateChanged+=s=>{
        if(s==PlayModeStateChange.EnteredPlayMode&&SessionState.GetBool(Key,false)){
            SessionState.SetBool(Key,false);new GameObject("ValidacaoNPC_Temporaria").AddComponent<FeiraNPCPlayChecks>();
        }
    };}
    [MenuItem("Nexa Quest/Testar NPCs e regressao Brasil")]
    public static void Run(){
        if(EditorApplication.isPlayingOrWillChangePlaymode)return;
        var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        if(scene.path!="Assets/Projeto/Scenes/Brasil.unity"||scene.isDirty){Debug.LogError("Abra e salve Brasil antes de testar.");return;}
        SessionState.SetBool(Key,true);EditorApplication.isPlaying=true;
    }
}
public class FeiraNPCPlayChecks:MonoBehaviour
{
    const string Dir="Documentation/Brasil/EtapaNPCs/";
    readonly List<string> results=new List<string>();readonly List<string> issues=new List<string>();int fails;
    void Check(bool ok,string label){results.Add((ok?"PASS: ":"FAIL: ")+label);if(!ok)fails++;}
    void Log(string m,string trace,LogType type){if(type==LogType.Error||type==LogType.Exception||type==LogType.Warning||type==LogType.Assert)issues.Add(type+": "+m);}
    IEnumerator Start(){
        Application.logMessageReceived+=Log;yield return null;yield return null;
        var w=FindObjectOfType<BrazilWorld>();var p=w.player;var dc=FindObjectOfType<DialogueController>();
        var feira=w.areas.Single(a=>a.name=="Feira");var root=feira.transform.Find("NPCs");
        Check(root.childCount>=6,"Pelo menos seis NPCs na Feira; repeticoes decorativas permitidas");
        var npcs=root.GetComponentsInChildren<NPCInteraction>();Check(npcs.Length==2,"Dois NPCs interativos");
        var decorative=root.Cast<Transform>().Where(t=>t.GetComponent<NPCInteraction>()==null).ToArray();
        Check(decorative.Length>=4&&decorative.All(t=>t.GetComponentsInChildren<Collider2D>().Length==0),"Todos os decorativos sem colisores (minimo quatro)");
        Check(npcs.All(n=>n.GetComponent<CircleCollider2D>().isTrigger),"Interativos somente com trigger");
        Check(!dc.IsOpen&&!dc.panel.activeSelf,"Dialogo comeca fechado");
        Check(FindObjectsOfType<Transform>(true).All(t=>t.GetComponents<Component>().All(c=>c!=null)),"Sem scripts ausentes");
        foreach(var a in root.GetComponentsInChildren<Animator>()){
            var clip=a.runtimeAnimatorController.animationClips.Single();Check(AnimationUtility.GetAnimationClipSettings(clip).loopTime,"Idle em loop: "+a.name);
        }
        var prog=ProgressionManager.Instance;var hud=FindObjectOfType<ProgressionHUD>();
        Check(prog.Coins==0&&prog.CurrentXP==0&&prog.Level==1&&prog.XPToNextLevel==100,"Valores iniciais 0 moedas, 0/100 XP, nivel 1");
        Check(hud.knowledgeFill.fillAmount==0&&hud.coins.text.EndsWith("0")&&hud.brainLevel.text.EndsWith("1"),"HUD reflete dados iniciais");
        p.enabled=false;
        foreach(var npc in npcs){
            p.Teleport(npc.transform.position+Vector3.down*.35f);Physics2D.SyncTransforms();yield return new WaitForFixedUpdate();yield return new WaitForFixedUpdate();yield return null;
            Check(dc.interactionPrompt.activeSelf&&!dc.IsOpen,"Proximidade mostra E sem abrir: "+npc.name);
            int completed=0;npc.onDialogueFinished.AddListener(()=>completed++);
            dc.HandleInteract();yield return null;
            Check(dc.IsOpen&&dc.CurrentNPC==npc&&dc.LineIndex==0,"E abre primeira fala: "+npc.name);
            var start=p.transform.position;p.SetMovementInput(Vector2.right);p.SendMessage("FixedUpdate");yield return new WaitForFixedUpdate();
            Check(p.ControlsLocked&&p.MoveInput==Vector2.zero&&Vector3.Distance(start,p.transform.position)<.03f,"Quest bloqueada durante conversa");
            Check(!w.TravelTo(w.areas[1],w.areas[1].defaultSpawn),"Transicao recusada durante conversa");
            for(int line=0;line<npc.dialogue.lines.Length;line++){
                var data=npc.dialogue.lines[line];
                Check(dc.speakerName.text==data.speakerName&&dc.dialogueText.text==data.text&&dc.portrait.sprite==data.portrait,"Nome/retrato/texto: "+npc.name+" fala "+line);
                dc.dialogueText.ForceMeshUpdate();Check(!dc.dialogueText.isTextOverflowing,"Texto cabe no painel: "+line);
                if(line<2){yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Dir+npc.name+"-fala"+line+".png");}
                yield return null;dc.HandleInteract();
                int index=dc.LineIndex;dc.HandleInteract();Check(dc.LineIndex==index,"Uma tecla avanca uma fala por frame");
                yield return null;
            }
            Check(!dc.IsOpen&&!dc.panel.activeSelf&&!p.ControlsLocked&&completed==1,"Fim fecha, libera Quest e emite evento uma vez");
            Check(prog.Coins==0&&prog.CurrentXP==0&&prog.Level==1,"Conversa nao concede recompensas");
            p.Teleport(w.initialSpawn.position);Physics2D.SyncTransforms();yield return new WaitForFixedUpdate();yield return new WaitForFixedUpdate();yield return null;
            Check(!dc.interactionPrompt.activeSelf,"Sair oculta indicacao");
        }
        // Teste de dados isolado do gameplay: nenhum NPC chama estes metodos.
        prog.AddCoins(25);prog.AddXP(250);yield return null;
        Check(prog.Coins==25&&prog.Level==3&&prog.CurrentXP==50,"AddXP atravessa varios niveis e preserva excedente");
        Check(Mathf.Approximately(hud.knowledgeFill.fillAmount,.5f)&&hud.coins.text.EndsWith("25")&&hud.brainLevel.text.EndsWith("3"),"HUD atualiza por evento");
        prog.AddXP(-100);prog.AddCoins(-5);Check(prog.CurrentXP==50&&prog.Coins==25,"Valores negativos nao corrompem progresso");
        p.enabled=true;
        Check(issues.Count==0,"Sem erros ou avisos nos NPCs");
        results.AddRange(issues);results.Add("Falhas: "+fails);Directory.CreateDirectory(Dir);File.WriteAllLines(Dir+"playmode.txt",results);
        Application.logMessageReceived-=Log;
        // Reexecuta os testes reais de movimento, seis passagens e Portal existentes.
        new GameObject("RegressaoPortal_Temporaria").AddComponent<PortalPlayChecks>();
        Destroy(gameObject);
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using NexaQuest.Brasil;

[InitializeOnLoad]
public static class QuestHUDVisualValidation
{
    const string Request="Library/QuestHUDVisual.request";
    const string Phase="NexaQuest.VisualPhase";
    const string StartKey="NexaQuest.VisualStart";
    static QuestHUDVisualValidation()
    {
        EditorApplication.update+=Poll;
        EditorApplication.playModeStateChanged+=s=>{
            if(s==PlayModeStateChange.EnteredPlayMode&&SessionState.GetBool(StartKey,false)){
                SessionState.SetBool(StartKey,false);new GameObject("ValidacaoVisual_Temporaria").AddComponent<QuestHUDPlayChecks>();
            }
            if(s==PlayModeStateChange.EnteredEditMode&&SessionState.GetString(Phase,"")!=""){
                string phase=SessionState.GetString(Phase,"");SessionState.EraseString(Phase);
                string folder=QuestHUDVisualSetup.Docs+phase+"/";Directory.CreateDirectory(folder);
                foreach(string src in new[]{"Documentation/Brasil/EtapaNPCs/playmode.txt","Documentation/Brasil/EtapaPortal/playmode.txt","Documentation/Brasil/EtapaPortal/console.txt"})
                    if(File.Exists(src))File.Copy(src,folder+(src.Contains("EtapaNPCs")?"npcs-":"portal-")+Path.GetFileName(src),true);
                File.WriteAllText(folder+"concluido.txt",DateTime.Now.ToString("s"));
                RestoreGameView();
            }
        };
    }
    static void Poll()
    {
        if(EditorApplication.isCompiling||EditorApplication.isUpdating||!File.Exists(Request))return;
        string pending=File.ReadAllText(Request).Trim();
        if(pending=="stop"){File.Delete(Request);EditorApplication.isPlaying=false;return;}
        if(pending=="refresh"){File.Delete(Request);AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);return;}
        if(EditorApplication.isPlayingOrWillChangePlaymode)return;
        string job=File.ReadAllText(Request).Trim();File.Delete(Request);
        try {
            var scene=SceneManager.GetActiveScene();
            if(job=="inspect"){Directory.CreateDirectory(QuestHUDVisualSetup.Docs);File.WriteAllText(QuestHUDVisualSetup.Docs+"editor-state.txt",scene.path+"\nDirty: "+scene.isDirty+"\n"+DateTime.Now.ToString("s"));return;}
            if(scene.path!="Assets/Projeto/Scenes/Brasil.unity"||scene.isDirty)throw new InvalidOperationException("Abra e salve Brasil antes da operacao visual.");
            if(job=="apply")QuestHUDVisualSetup.Run();
            else if(job=="baseline"||job=="test"){
                Directory.CreateDirectory(QuestHUDVisualSetup.Docs);
                SetGameView();
                // Limpa somente o historico do Console; erros da execucao sao registrados nos relatórios.
                typeof(Editor).Assembly.GetType("UnityEditor.LogEntries").GetMethod("Clear",BindingFlags.Static|BindingFlags.Public).Invoke(null,null);
                SessionState.SetString(Phase,job=="baseline"?"antes":"depois");
                if(job=="baseline")FeiraNPCValidation.Run();
                else {SessionState.SetBool(StartKey,true);EditorApplication.isPlaying=true;}
            }
        } catch(Exception e){Directory.CreateDirectory(QuestHUDVisualSetup.Docs);File.WriteAllText(QuestHUDVisualSetup.Docs+"falha-operacao.txt",e.ToString());Debug.LogException(e);}
    }
    static readonly BindingFlags Flags=BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.Static|BindingFlags.FlattenHierarchy;
    static void SetGameView()
    {
        var assembly=typeof(Editor).Assembly;var sizesType=assembly.GetType("UnityEditor.GameViewSizes");
        var instance=sizesType.GetProperty("instance",Flags).GetValue(null);
        var group=sizesType.GetMethod("GetGroup").Invoke(instance,new[]{sizesType.GetProperty("currentGroupType",Flags).GetValue(instance)});
        int count=(int)group.GetType().GetMethod("GetTotalCount").Invoke(group,null);int chosen=-1;
        for(int i=0;i<count;i++){
            var size=group.GetType().GetMethod("GetGameViewSize").Invoke(group,new object[]{i});
            if((int)size.GetType().GetProperty("width").GetValue(size)==1920&&(int)size.GetType().GetProperty("height").GetValue(size)==1080){chosen=i;break;}
        }
        if(chosen<0)throw new InvalidOperationException("Selecione uma resolucao fixa Full HD 1920x1080 no Game View para a validacao.");
        var view=EditorWindow.GetWindow(assembly.GetType("UnityEditor.GameView"));var selected=view.GetType().GetProperty("selectedSizeIndex",Flags);
        SessionState.SetInt("NexaQuest.PreviousGameSize",(int)selected.GetValue(view));selected.SetValue(view,chosen);view.Repaint();
    }
    static void RestoreGameView()
    {
        int index=SessionState.GetInt("NexaQuest.PreviousGameSize",-1);if(index<0)return;
        var view=EditorWindow.GetWindow(typeof(Editor).Assembly.GetType("UnityEditor.GameView"));
        view.GetType().GetProperty("selectedSizeIndex",Flags).SetValue(view,index);SessionState.EraseInt("NexaQuest.PreviousGameSize");
    }
}

public class QuestHUDPlayChecks:MonoBehaviour
{
    readonly List<string> results=new List<string>();readonly List<string> issues=new List<string>();int fails;
    void Check(bool ok,string message){results.Add((ok?"PASS: ":"FAIL: ")+message);if(!ok)fails++;}
    void Log(string m,string trace,LogType type){if(type==LogType.Error||type==LogType.Exception||type==LogType.Warning||type==LogType.Assert)issues.Add(type+": "+m);}
    IEnumerator Screenshot(string name){yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(QuestHUDVisualSetup.Docs+name+".png");yield return null;}
    IEnumerator Start()
    {
        Application.logMessageReceived+=Log;yield return null;yield return null;
        var w=FindObjectOfType<BrazilWorld>();var p=w.player;var animator=p.GetComponent<Animator>();var renderer=p.GetComponent<SpriteRenderer>();
        var hud=FindObjectOfType<ProgressionHUD>();var progress=ProgressionManager.Instance;
        Check(Screen.width==1920&&Screen.height==1080,"Game View real 1920x1080: "+Screen.width+"x"+Screen.height);
        Check(hud.coins.text==progress.Coins.ToString(),"Coins exibe somente numero real");
        Check(hud.brainLevel.text==progress.Level.ToString(),"Level exibe somente numero real");
        Check(hud.knowledge.text=="CONHECIMENTO","Label CONHECIMENTO");
        Check(hud.knowledgeFill.type==Image.Type.Filled&&hud.knowledgeFill.fillMethod==Image.FillMethod.Horizontal&&hud.knowledgeFill.fillOrigin==0,"Barra horizontal da esquerda para direita");
        foreach(var name in new[]{"CoinsHUD","KnowledgeHUD"}){
            var background=hud.transform.Find(name+"/Background").GetComponent<Image>();
            var r=background.rectTransform.rect;var sprite=background.sprite;
            Check(background.preserveAspect&&Mathf.Abs(r.width/r.height-sprite.rect.width/sprite.rect.height)<.001f,"Proporcao intacta: "+name);
            Check(sprite.texture.width==(name=="CoinsHUD"?2172:1983),"PNG em resolucao integral: "+name);
        }
        Check(hud.brainLevel.transform.parent.name=="KnowledgeHUD"&&hud.brainLevel.rectTransform.anchoredPosition.y<-125,"Level na caixa abaixo do cerebro");
        foreach(var text in new[]{hud.coins,hud.knowledge,hud.brainLevel}){text.ForceMeshUpdate();Check(!text.isTextOverflowing,"Texto cabe: "+text.name);}
        yield return Screenshot("HUD-inicial-1080p");
        string original=EditorJsonUtility.ToJson(progress);
        progress.AddCoins(25);progress.AddXP(25);yield return null;
        Check(hud.coins.text=="25"&&progress.Coins==25,"AddCoins atualiza numero imediatamente");
        Check(Mathf.Approximately(hud.knowledgeFill.fillAmount,.25f),"AddXP(25) preenche 25 por cento");
        yield return Screenshot("HUD-25XP-25coins-1080p");
        progress.AddXP(25);Check(Mathf.Approximately(hud.knowledgeFill.fillAmount,.5f),"50 XP preenche metade");
        progress.AddXP(50);Check(progress.Level==2&&progress.CurrentXP==0&&hud.brainLevel.text=="2"&&hud.knowledgeFill.fillAmount==0,"100 XP sobe nivel e zera barra pela regra existente");
        progress.AddXP(25);yield return Screenshot("HUD-nivel2-25XP-1080p");
        // Restaura somente o estado temporario de Play para executar a regressao existente a partir de 0/0/1.
        EditorJsonUtility.FromJsonOverwrite(original,progress);hud.SendMessage("Refresh");
        p.enabled=false;
        Vector2[] directions={Vector2.down,Vector2.left,Vector2.right,Vector2.up};string[] names={"Down","Left","Right","Up"};
        for(int d=0;d<4;d++){
            p.SetMovementInput(directions[d]);yield return null;yield return null;
            Check(animator.GetCurrentAnimatorStateInfo(0).IsName("Walk"+names[d]),"Animator Walk"+names[d]);
            Check(renderer.sprite.name.StartsWith("Walk"+names[d]+"_"),"Sprite correto andando: "+names[d]);
            p.SetMovementInput(Vector2.zero);yield return null;yield return null;
            Check(animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"+names[d]),"Animator Idle"+names[d]);
            Check(renderer.sprite.name.StartsWith("Idle"+names[d]+"_"),"Idle mantem ultima direcao: "+names[d]);
            yield return Screenshot("Quest-Idle"+names[d]+"-1080p");
        }
        p.SetMovementInput(new Vector2(1,1));yield return null;yield return null;
        Check(Mathf.Approximately(p.MoveInput.magnitude,1)&&p.FacingDirection==2,"Diagonal normalizada e direcao preservada");
        p.SetMovementInput(Vector2.zero);yield return null;yield return null;
        Check(p.FacingDirection==2&&animator.GetCurrentAnimatorStateInfo(0).IsName("IdleRight"),"Parada diagonal mantem IdleRight");
        foreach(var clip in animator.runtimeAnimatorController.animationClips.Distinct()){
            var binding=AnimationUtility.GetObjectReferenceCurveBindings(clip).Single(b=>b.type==typeof(SpriteRenderer));
            var keys=AnimationUtility.GetObjectReferenceCurve(clip,binding);
            Check(keys.Select(k=>k.value).Distinct().Count()==(clip.name.StartsWith("Walk")?8:4),"Todos os frames: "+clip.name);
            Check(AnimationUtility.GetAnimationClipSettings(clip).loopTime,"Loop mantido: "+clip.name);
            foreach(var key in keys.Take(keys.Length-1)){
                var sprite=key.value as Sprite;
                Check(sprite!=null&&AssetDatabase.GetAssetPath(sprite).StartsWith(QuestHUDVisualSetup.Art)&&sprite.bounds.size.y>.45f&&sprite.bounds.size.y<.49f&&Mathf.Abs(sprite.bounds.min.y)<.004f,"Novo frame alinhado/escala correta: "+sprite.name);
            }
        }
        var collider=p.GetComponent<CapsuleCollider2D>();
        Check(collider.offset==new Vector2(0,.05f)&&collider.size==new Vector2(.22f,.1f),"Collider original dos pes preservado");
        Check(w.followCamera.target==p.transform,"Camera continua seguindo o player");
        p.SetMovementInput(Vector2.down);p.SetMovementInput(Vector2.zero);p.enabled=true;
        Check(issues.Count==0,"Sem erros ou avisos nos testes visuais");results.AddRange(issues);results.Add("Falhas: "+fails);
        File.WriteAllLines(QuestHUDVisualSetup.Docs+"visual-playmode.txt",results);
        Application.logMessageReceived-=Log;
        new GameObject("RegressaoNPC_Temporaria").AddComponent<FeiraNPCPlayChecks>();Destroy(gameObject);
    }
}



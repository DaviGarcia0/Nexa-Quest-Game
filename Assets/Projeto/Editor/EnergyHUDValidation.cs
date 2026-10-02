using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using NexaQuest.Brasil;

// Ferramenta de Editor: nao participa do jogo nem cria logica de Energia.
[InitializeOnLoad]
public static class EnergyHUDValidation
{
    const string Request="Library/EnergyHUD.request", Active="NexaQuest.EnergyTest", StartKey="NexaQuest.EnergyStart";
    static readonly BindingFlags Flags=BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Static;
    static EnergyHUDValidation(){
        EditorApplication.update+=Poll;
        EditorApplication.playModeStateChanged+=state=>{
            if(state==PlayModeStateChange.EnteredPlayMode&&SessionState.GetBool(StartKey,false)){
                SessionState.SetBool(StartKey,false);new GameObject("ValidacaoEnergia_Temporaria").AddComponent<EnergyHUDPlayChecks>();
            }
            if(state==PlayModeStateChange.EnteredEditMode&&SessionState.GetBool(Active,false)){
                SessionState.SetBool(Active,false);
                string target=EnergyHUDSetup.Docs+"depois/";Directory.CreateDirectory(target);
                foreach(var item in new[]{
                    new[]{QuestHUDVisualSetup.Docs+"visual-playmode.txt","visual-playmode.txt"},
                    new[]{"Documentation/Brasil/EtapaNPCs/playmode.txt","npcs-playmode.txt"},
                    new[]{"Documentation/Brasil/EtapaPortal/playmode.txt","portal-playmode.txt"},
                    new[]{"Documentation/Brasil/EtapaPortal/console.txt","console.txt"}})
                    if(File.Exists(item[0]))File.Copy(item[0],target+item[1],true);
                GameView("RestoreGameView");File.WriteAllText(target+"concluido.txt",DateTime.Now.ToString("s"));
            }
        };
    }
    static void GameView(string method){typeof(QuestHUDVisualValidation).GetMethod(method,Flags).Invoke(null,null);}
    static void Poll(){
        if(EditorApplication.isCompiling||EditorApplication.isUpdating||EditorApplication.isPlayingOrWillChangePlaymode||!File.Exists(Request))return;
        string command=File.ReadAllText(Request).Trim();File.Delete(Request);
        try {
            var scene=SceneManager.GetActiveScene();
            if(scene.path!="Assets/Projeto/Scenes/Brasil.unity"||scene.isDirty)throw new InvalidOperationException("Abra e salve Brasil fora de Play Mode.");
            if(command=="apply")EnergyHUDSetup.Run();
            if(command=="test"){
                Directory.CreateDirectory(EnergyHUDSetup.Docs);QuestHUDVisualValidation.CapturePlayerCollider();GameView("SetGameView");
                typeof(Editor).Assembly.GetType("UnityEditor.LogEntries").GetMethod("Clear",Flags).Invoke(null,null);
                SessionState.SetBool(Active,true);SessionState.SetBool(StartKey,true);EditorApplication.isPlaying=true;
            }
        }catch(Exception e){Directory.CreateDirectory(EnergyHUDSetup.Docs);File.WriteAllText(EnergyHUDSetup.Docs+"falha-operacao.txt",e.ToString());Debug.LogException(e);}
    }
}
public class EnergyHUDPlayChecks:MonoBehaviour
{
    readonly List<string> results=new List<string>(),issues=new List<string>();int fails;
    void Check(bool ok,string message){results.Add((ok?"PASS: ":"FAIL: ")+message);if(!ok)fails++;}
    void Log(string m,string trace,LogType type){if(type==LogType.Error||type==LogType.Exception||type==LogType.Warning||type==LogType.Assert)issues.Add(type+": "+m);}
    IEnumerator Screenshot(string name){yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(EnergyHUDSetup.Docs+name+".png");yield return null;}
    [Serializable] class MeshData {public string name;public Rect rect;public Vector2[] vertices;public int[] triangles;}
    [Serializable] class Meshes {public MeshData[] frames;}
    IEnumerator Start(){
        Application.logMessageReceived+=Log;yield return null;yield return null;
        var world=FindObjectOfType<BrazilWorld>();var player=world.player;var progressHUD=FindObjectOfType<ProgressionHUD>();
        var root=progressHUD.GetComponentInParent<Canvas>().transform.Find("EnergyHUD").GetComponent<RectTransform>();
        var background=root.Find("Background").GetComponent<Image>();var mask=root.Find("PortraitMask").GetComponent<Mask>();
        var portrait=mask.transform.Find("QuestPortrait").GetComponent<Image>();var animator=portrait.GetComponent<Animator>();
        var energy=root.Find("EnergyText").GetComponent<TMP_Text>();
        var frames=AssetDatabase.LoadAllAssetsAtPath(EnergyHUDSetup.Art+"QuestExpressaoHUD.png").OfType<Sprite>().OrderBy(s=>s.name).ToArray();
        Check(Screen.width==1920&&Screen.height==1080,"Game View 1920x1080");
        Check(root.anchorMin==new Vector2(0,1)&&root.anchorMax==root.anchorMin,"HUD ancorada no alto a esquerda");
        Check(root.anchoredPosition.x>=0&&root.anchoredPosition.y<=0,"HUD dentro da tela");
        Check(background.preserveAspect&&Mathf.Abs(root.rect.width/root.rect.height-775f/259)<.001f,"Proporcao original da HUD preservada");
        Check(background.sprite.texture.width==775&&background.sprite.texture.height==259,"Resolucao integral do fundo");
        Check(energy.text=="100"&&energy.transform!=background.transform,"Energia e texto separado com valor temporario 100");
        energy.ForceMeshUpdate();Check(!energy.isTextOverflowing,"EnergyText cabe na area horizontal");
        Check(energy.rectTransform.anchoredPosition.x>220&&energy.rectTransform.anchoredPosition.y< -60,"EnergyText na faixa vinho");
        energy.text="80";yield return null;Check(energy.text=="80","EnergyText pode ser atualizado independentemente");energy.text="100";
        Check(mask.enabled&&!mask.showMaskGraphic&&portrait.maskable&&portrait.useSpriteMesh,"Mascara circular e contorno individual ativos");
        Check(animator.runtimeAnimatorController!=player.GetComponent<Animator>().runtimeAnimatorController,"Animator UI separado do Animator do Player");
        var clip=animator.runtimeAnimatorController.animationClips.Single();
        Check(clip.name=="QuestHUD_Idle"&&clip.frameRate==12&&AnimationUtility.GetAnimationClipSettings(clip).loopTime,"QuestHUD_Idle 12 FPS em loop");
        Check(frames.Length==6,"Seis sprites Idle importados");
        foreach(var s in frames){
            Check(s.rect.size==new Vector2(370,505)&&s.pivot==new Vector2(185,252.5f),"Recorte e pivot uniformes: "+s.name);
            Check(Vector2.Distance(s.bounds.size,new Vector2(3.7f,5.05f))<.001f,"Mesh mantem tamanho uniforme: "+s.name);
            Check(s.vertices.Length>4,"Contorno individual sem vizinhos: "+s.name);
        }
        File.WriteAllText(EnergyHUDSetup.Docs+"mesh-importado.json",JsonUtility.ToJson(new Meshes{frames=frames.Select(s=>new MeshData{name=s.name,rect=s.rect,vertices=s.vertices,triangles=s.triangles.Select(t=>(int)t).ToArray()}).ToArray()},true));
        foreach(var path in new[]{EnergyHUDSetup.Art+"QuestExpressaoHUD.png",EnergyHUDSetup.Art+"ExpressaoQuest.png",EnergyHUDSetup.Art+"QuestExpressaoComemoracaoHUD.png","Assets/Projeto/Sprites/HUD/HUD_ENERGIA.png"}){
            var i=(TextureImporter)AssetImporter.GetAtPath(path);
            Check(i.filterMode==FilterMode.Point&&!i.mipmapEnabled&&i.textureCompression==TextureImporterCompression.Uncompressed,"Importacao sem filtro/compressao/mipmap: "+Path.GetFileName(path));
        }
        var source=new Texture2D(2,2);source.LoadImage(File.ReadAllBytes("Assets/Projeto/Sprites/HUD/HUD_ENERGIA.png"));
        Check(source.GetPixel(0,0).a==0,"Exterior do PNG transparente");var center=source.GetPixel(142,259-127);Check(center.a>.99f&&center.r+center.g+center.b<.01f,"Circulo interno preto opaco preservado");Destroy(source);
        var progressionRect=progressHUD.GetComponent<RectTransform>();
        var corners=new Vector3[4];var otherCorners=new Vector3[4];root.GetWorldCorners(corners);progressionRect.GetWorldCorners(otherCorners);
        Check(corners[2].x<otherCorners[0].x,"Energia nao sobrepoe Coins/Conhecimento");
        var startSize=portrait.rectTransform.sizeDelta;var startPosition=portrait.rectTransform.anchoredPosition;
        var visited=new HashSet<Sprite>();float begin=Time.realtimeSinceStartup;bool wrapped=false;int last=-1;
        while(Time.realtimeSinceStartup-begin<5.2f){
            visited.Add(portrait.sprite);int current=Array.IndexOf(frames,portrait.sprite);if(last==5&&current==0)wrapped=true;last=current;
            yield return null;
        }
        Check(visited.Count==6&&frames.All(s=>visited.Contains(s)),"Animator exibiu seis poses, incluindo piscar");
        Check(wrapped,"Idle retornou naturalmente do ultimo para primeiro frame");
        Check(portrait.rectTransform.sizeDelta==startSize&&portrait.rectTransform.anchoredPosition==startPosition,"Animacao preserva tamanho e centro do RectTransform");
        Check(animator.updateMode==AnimatorUpdateMode.UnscaledTime,"Idle da interface independente da escala de tempo");
        animator.enabled=false;
        for(int j=0;j<frames.Length;j++){portrait.sprite=frames[j];yield return Screenshot("HUD-frame"+(j+1)+"-1080p");}
        animator.enabled=true;yield return null;
        var dc=FindObjectOfType<DialogueController>();var expected=AssetDatabase.LoadAssetAtPath<Sprite>(EnergyHUDSetup.Art+"ExpressaoQuest.png");
        player.enabled=false;
        foreach(var npc in FindObjectsOfType<NPCInteraction>()){
            player.Teleport(npc.transform.position+Vector3.down*.35f);Physics2D.SyncTransforms();yield return new WaitForFixedUpdate();yield return new WaitForFixedUpdate();yield return null;
            Check(dc.interactionPrompt.activeSelf,"Proximidade apresenta E: "+npc.name);
            dc.HandleInteract();yield return null;
            for(int j=0;j<3;j++){
                var line=npc.dialogue.lines[j];
                Check(dc.IsOpen&&dc.LineIndex==j&&dc.portrait.sprite==line.portrait&&dc.speakerName.text==line.speakerName,"Fala/retrato correto: "+npc.name+" linha "+j);
                Check(j==1?dc.portrait.sprite==expected:dc.portrait.sprite!=expected,"Sequencia NPC-Quest-NPC: "+npc.name+" linha "+j);
                Check(player.ControlsLocked,"Movimento bloqueado enquanto dialogo aberto");
                yield return Screenshot(npc.name+"-fala"+j+"-1080p");dc.HandleInteract();yield return null;
            }
            while(dc.IsOpen){dc.HandleInteract();yield return null;}
            Check(!player.ControlsLocked&&!dc.panel.activeSelf,"Fim do dialogo devolve controle: "+npc.name);
            player.Teleport(world.initialSpawn.position);Physics2D.SyncTransforms();yield return new WaitForFixedUpdate();yield return new WaitForFixedUpdate();yield return null;
        }
        player.enabled=true;
        Check(ProgressionManager.Instance.Coins==0&&ProgressionManager.Instance.Level==1&&ProgressionManager.Instance.CurrentXP==0,"Energia e conversas preservam progresso inicial");
        Check(issues.Count==0,"Sem erros/avisos durante teste da Energia");
        results.AddRange(issues);results.Add("Falhas: "+fails);File.WriteAllLines(EnergyHUDSetup.Docs+"energia-playmode.txt",results);
        Application.logMessageReceived-=Log;new GameObject("RegressaoVisual_Temporaria").AddComponent<QuestHUDPlayChecks>();Destroy(gameObject);
    }
}

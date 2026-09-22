using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using NexaQuest.Brasil;

[InitializeOnLoad]
public static class BrasilPortalValidation
{
    const string Key="NexaQuest.PortalValidation";
    static BrasilPortalValidation(){EditorApplication.playModeStateChanged+=s=>{
        if(s==PlayModeStateChange.EnteredPlayMode && SessionState.GetBool(Key,false)){
            SessionState.SetBool(Key,false);new GameObject("TestePortal_Temporario").AddComponent<PortalPlayChecks>();
        }
    };}
    public static void Run(){
        if(EditorApplication.isPlayingOrWillChangePlaymode)return;
        if(UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty){Debug.LogError("Salve Brasil antes do teste.");return;}
        SessionState.SetBool(Key,true);EditorApplication.isPlaying=true;
    }
}
public class PortalPlayChecks:MonoBehaviour
{
    const string Dir="Documentation/Brasil/EtapaPortal/";
    List<string> lines=new List<string>(); List<string> issues=new List<string>();int fails;
    void Check(bool ok,string s){lines.Add((ok?"PASS: ":"FAIL: ")+s);if(!ok)fails++;}
    void Log(string m,string stack,LogType t){if(t==LogType.Error||t==LogType.Exception||t==LogType.Warning||t==LogType.Assert)issues.Add(t+": "+m);}
    IEnumerator Start(){
        Application.logMessageReceived+=Log;
        yield return null;
        var w=FindObjectOfType<BrazilWorld>();var p=w.player;var foot=p.GetComponent<Collider2D>();
        Check(w.areas.Length==4,"Quatro areas cadastradas");
        Check(w.CurrentArea==w.initialArea,"Area inicial preservada");
        Check(FindObjectsOfType<Transform>(true).All(t=>t.GetComponents<Component>().All(c=>c!=null)),"Sem scripts ausentes");
        foreach(var a in w.areas){
            var i=(TextureImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(a.map.sprite));
            Check(i.filterMode==FilterMode.Point&&!i.mipmapEnabled&&i.textureCompression==TextureImporterCompression.Uncompressed,"Importacao nitida: "+a.name);
            Check(a.map.sprite.texture.width==1672&&a.map.sprite.texture.height==941,"Resolucao integral: "+a.name);
            Check(a.map.transform.localScale==Vector3.one&&a.map.sprite.pixelsPerUnit==100,"Escala/PPU: "+a.name);
        }
        yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Dir+"Feira-play.png");
        var exits=w.areas.SelectMany(a=>a.GetComponentsInChildren<MapTransition>(true)).Where(t=>t.gameObject.activeSelf).ToArray();
        Check(exits.Length==6,"Cinco saidas existentes e retorno do Portal");
        p.enabled=false;
        foreach(var exit in exits){
            var source=exit.GetComponentInParent<BrazilArea>(true);
            yield return new WaitForSecondsRealtime(0.3f);
            Check(w.TravelTo(source,source.defaultSpawn),"Preparar origem: "+source.name);
            yield return new WaitForSecondsRealtime(w.fadeDuration*2+0.35f);
            var trigger=exit.GetComponent<BoxCollider2D>();
            var center=trigger.bounds.center;
            Vector2 inward=((Vector2)(source.map.bounds.center-center)).normalized;
            p.Teleport(center+(Vector3)(inward*1.2f));Physics2D.SyncTransforms();yield return new WaitForFixedUpdate();
            p.SetMovementInput(-inward);
            for(int step=0;step<100&&!w.IsTransitioning;step++) {p.SendMessage("FixedUpdate");yield return new WaitForFixedUpdate();}
            p.SetMovementInput(Vector2.zero);
            yield return new WaitForFixedUpdate();yield return new WaitForFixedUpdate();
            Check(w.IsTransitioning&&p.ControlsLocked,"Trigger real bloqueia controles: "+source.name+"/"+exit.name);
            Check(!w.TravelTo(source,source.defaultSpawn),"Transicao simultanea recusada");
            yield return new WaitForSecondsRealtime(w.fadeDuration*0.65f);
            Check(w.fadePanel.alpha>0,"Fade visivel");
            yield return new WaitForSecondsRealtime(w.fadeDuration*2+0.4f);
            Check(w.CurrentArea==exit.destinationArea,"Destino: "+exit.name);
            Check(Vector3.Distance(p.transform.position,exit.destinationSpawnPoint.position)<0.03f,"Spawn correto: "+exit.name);
            Check(!w.IsTransitioning&&!p.ControlsLocked&&w.fadePanel.alpha==0,"Fade terminou e controles liberados");
            Check(w.areas.Count(a=>a.gameObject.activeSelf)==1,"Somente destino ativo");
            Check(w.followCamera.mapBounds==w.CurrentArea.map,"Camera usa limites do destino");
            foreach(var other in w.CurrentArea.GetComponentsInChildren<MapTransition>())
                Check(!foot.IsTouching(other.GetComponent<Collider2D>()),"Spawn fora da saida: "+other.name);
            var stable=w.CurrentArea;yield return new WaitForSecondsRealtime(0.7f);
            Check(w.CurrentArea==stable&&!w.IsTransitioning,"Sem loop apos cooldown");
        }
        var portal=w.areas.Single(a=>a.name=="Portal");
        w.TravelTo(portal,portal.defaultSpawn);yield return new WaitForSecondsRealtime(w.fadeDuration*2+0.35f);
        var interact=portal.GetComponentInChildren<PortalInteraction>();
        Check(portal.map.sprite.name=="Portal","Portal desligado");
        Check(!interact.keysPanel.activeSelf&&!interact.interactionPrompt.activeSelf,"UI comeca fechada e longe");
        // Mesmo motor de movimento e mesma logica de animacao usados pelo teclado.
        Vector2[] dirs={Vector2.down,Vector2.left,Vector2.right,Vector2.up,new Vector2(1,1)};
        int[] poses={0,1,2,3,2};
        for(int d=0;d<dirs.Length;d++){
            p.Teleport(new Vector3(0,-2.2f,0));yield return new WaitForFixedUpdate();
            var start=p.transform.position;p.SetMovementInput(dirs[d]);
            for(int j=0;j<10;j++){p.SendMessage("FixedUpdate");yield return new WaitForFixedUpdate();}
            Check(Mathf.Abs(Vector3.Distance(p.transform.position,start)-p.moveSpeed*Time.fixedDeltaTime*10)<0.06f,"Velocidade normalizada direcao "+d);
            Check(p.FacingDirection==poses[d],"Pose ao andar "+d);p.SetMovementInput(Vector2.zero);
            yield return null;Check(p.FacingDirection==poses[d],"Idle lembra direcao "+d);
        }
        p.Teleport(interact.transform.position);Physics2D.SyncTransforms();yield return new WaitForFixedUpdate();yield return new WaitForFixedUpdate();
        Check(interact.interactionPrompt.activeSelf&&!interact.IsOpen,"Proximidade mostra E sem abrir painel");
        interact.HandleInput(true,false);yield return null;
        Check(interact.IsOpen&&interact.keysPanel.activeSelf&&p.ControlsLocked,"E abre painel e bloqueia movimento");
        var artwork=interact.keysPanel.transform.Find("PainelVazio").GetComponent<UnityEngine.UI.Image>();
        Check(artwork.sprite.name.Contains("vazia"),"Painel sempre vazio");
        yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Dir+"Portal-painel.png");
        interact.HandleInput(false,true);Check(!interact.IsOpen&&!p.ControlsLocked,"Esc fecha e libera controle");
        interact.HandleInput(true,false);interact.HandleInput(true,false);Check(!interact.IsOpen&&!p.ControlsLocked,"E tambem fecha");
        p.Teleport(portal.defaultSpawn.position);Physics2D.SyncTransforms();yield return new WaitForFixedUpdate();yield return new WaitForFixedUpdate();
        Check(!interact.interactionPrompt.activeSelf,"Sair da proximidade oculta E");
        Check(portal.map.sprite.name=="Portal","Portal continua desligado apos interacao");
        p.enabled=true;
        yield return new WaitForSecondsRealtime(0.3f);
        w.TravelTo(w.initialArea,w.initialSpawn);yield return new WaitForSecondsRealtime(w.fadeDuration*2+0.3f);
        Check(issues.Count==0,"Sem erros ou avisos durante Play Mode");
        lines.AddRange(issues);lines.Add("Falhas: "+fails);File.WriteAllLines(Dir+"playmode.txt",lines);
        var logs=typeof(Editor).Assembly.GetType("UnityEditor.LogEntries");
        var count=logs.GetMethod("GetCountsByType",System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Static);
        object[] counts={0,0,0};count.Invoke(null,counts);
        File.WriteAllText(Dir+"console.txt","Erros: "+counts[0]+"\nAvisos: "+counts[1]+"\nMensagens: "+counts[2]+"\nDurante teste: "+issues.Count+"\nData: "+DateTime.Now.ToString("s"));
        Application.logMessageReceived-=Log;EditorApplication.isPlaying=false;
    }
}


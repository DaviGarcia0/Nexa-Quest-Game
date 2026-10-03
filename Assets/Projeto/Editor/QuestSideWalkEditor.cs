using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;
using UnityEngine.SceneManagement;
using NexaQuest.Brasil;

// Ajuste pontual dos dois clips laterais; ferramenta exclusiva do Editor.
[InitializeOnLoad]
public static class QuestSideWalkEditor
{
    public const string Art="Assets/Projeto/Sprites/Quest/Design20261001/SideWalk20261003/";
    public const string Docs="Documentation/Brasil/CaminhadaLateral20261003/";
    const string Request="Library/QuestSideWalk.request", Active="NexaQuest.SideWalkTest", StartKey="NexaQuest.SideWalkStart";
    [Serializable] class Sheet{public float pixelsPerUnit;public Frame[] frames;}
    [Serializable] class Frame{public string name;public int x,y,width,height;public float pivotX,pivotY;}
    static readonly BindingFlags Flags=BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Static;
    static QuestSideWalkEditor(){
        EditorApplication.update+=Poll;
        EditorApplication.playModeStateChanged+=s=>{
            if(s==PlayModeStateChange.EnteredPlayMode&&SessionState.GetBool(StartKey,false)){
                SessionState.SetBool(StartKey,false);new GameObject("ValidacaoCaminhadaLateral_Temporaria").AddComponent<QuestSideWalkChecks>();
            }
            if(s==PlayModeStateChange.EnteredEditMode&&SessionState.GetBool(Active,false)){
                SessionState.SetBool(Active,false);Directory.CreateDirectory(Docs);
                foreach(var pair in new[]{new[]{QuestHUDVisualSetup.Docs+"visual-playmode.txt","visual-playmode.txt"},new[]{"Documentation/Brasil/EtapaNPCs/playmode.txt","npcs-playmode.txt"},new[]{"Documentation/Brasil/EtapaPortal/playmode.txt","portal-playmode.txt"},new[]{"Documentation/Brasil/EtapaPortal/console.txt","console.txt"}})
                    if(File.Exists(pair[0]))File.Copy(pair[0],Docs+pair[1],true);
                typeof(QuestHUDVisualValidation).GetMethod("RestoreGameView",Flags).Invoke(null,null);
                File.WriteAllText(Docs+"concluido.txt",DateTime.Now.ToString("s"));
            }
        };
    }
    static void Poll(){
        if(EditorApplication.isCompiling||EditorApplication.isUpdating||EditorApplication.isPlayingOrWillChangePlaymode||!File.Exists(Request))return;
        var job=File.ReadAllText(Request).Trim();File.Delete(Request);Directory.CreateDirectory(Docs);
        try{
            var scene=SceneManager.GetActiveScene();
            if(scene.path!="Assets/Projeto/Scenes/Brasil.unity"||scene.isDirty)throw new InvalidOperationException("Brasil precisa estar salva e fora de Play Mode.");
            if(job=="apply")Apply();
            if(job=="test"){
                QuestHUDVisualValidation.CapturePlayerCollider();
                typeof(QuestHUDVisualValidation).GetMethod("SetGameView",Flags).Invoke(null,null);
                typeof(Editor).Assembly.GetType("UnityEditor.LogEntries").GetMethod("Clear",Flags).Invoke(null,null);
                SessionState.SetBool(Active,true);SessionState.SetBool(StartKey,true);EditorApplication.isPlaying=true;
            }
        }catch(Exception e){File.WriteAllText(Docs+"falha.txt",e.ToString());Debug.LogException(e);}
    }
    static void Apply(){
        foreach(var direction in new[]{"Left","Right"}){
            var path=Art+"Walk"+direction+".png";var data=JsonUtility.FromJson<Sheet>(File.ReadAllText(Art+"Walk"+direction+".json"));
            if(data.frames.Length!=8)throw new InvalidOperationException("Oito frames esperados.");
            var importer=(TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Multiple;importer.spritePixelsPerUnit=data.pixelsPerUnit;
            importer.filterMode=FilterMode.Point;importer.mipmapEnabled=false;importer.textureCompression=TextureImporterCompression.Uncompressed;
            importer.maxTextureSize=4096;importer.npotScale=TextureImporterNPOTScale.None;importer.alphaSource=TextureImporterAlphaSource.FromInput;importer.alphaIsTransparency=true;
            var settings=new TextureImporterSettings();importer.ReadTextureSettings(settings);settings.spriteMeshType=SpriteMeshType.FullRect;settings.spriteGenerateFallbackPhysicsShape=false;importer.SetTextureSettings(settings);
            var platform=importer.GetDefaultPlatformTextureSettings();platform.maxTextureSize=4096;platform.textureCompression=TextureImporterCompression.Uncompressed;importer.SetPlatformTextureSettings(platform);importer.SaveAndReimport();
            var factory=new SpriteDataProviderFactories();factory.Init();var provider=factory.GetSpriteEditorDataProviderFromObject(importer);provider.InitSpriteEditorDataProvider();
            var previous=provider.GetSpriteRects().ToDictionary(r=>r.name,r=>r.spriteID);
            var rects=data.frames.Select(f=>new SpriteRect{name=f.name,rect=new Rect(f.x,f.y,f.width,f.height),pivot=new Vector2(f.pivotX,f.pivotY),alignment=SpriteAlignment.Custom,spriteID=previous.ContainsKey(f.name)?previous[f.name]:GUID.Generate()}).ToArray();
            provider.SetSpriteRects(rects);provider.GetDataProvider<ISpriteNameFileIdDataProvider>().SetNameFileIdPairs(rects.Select(r=>new SpriteNameFileIdPair(r.name,r.spriteID)));provider.Apply();AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceUpdate);
            var frames=AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().OrderBy(s=>s.name).ToArray();
            if(frames.Length!=8)throw new InvalidOperationException("Importacao nao produziu oito sprites; clip preservado.");
            var clip=AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/Projeto/Animations/Quest/Walk"+direction+".anim");
            var binding=AnimationUtility.GetObjectReferenceCurveBindings(clip).Single(b=>b.type==typeof(SpriteRenderer)&&b.propertyName=="m_Sprite");
            AnimationUtility.SetObjectReferenceCurve(clip,binding,frames.Select((s,i)=>new ObjectReferenceKeyframe{time=i/10f,value=s}).ToArray());
            clip.frameRate=10;var clipSettings=AnimationUtility.GetAnimationClipSettings(clip);clipSettings.loopTime=true;clipSettings.stopTime=.8f;AnimationUtility.SetAnimationClipSettings(clip,clipSettings);EditorUtility.SetDirty(clip);
        }
        AssetDatabase.SaveAssets();File.WriteAllText(Docs+"integracao.txt","WalkLeft e WalkRight: oito frames, 10 FPS, loop de 0.8s. Cena/prefab/scripts/Animator preservados.\n"+DateTime.Now.ToString("s"));
    }
}
public class QuestSideWalkChecks:MonoBehaviour
{
    readonly List<string> results=new List<string>(),issues=new List<string>();int fails;
    void Check(bool ok,string s){results.Add((ok?"PASS: ":"FAIL: ")+s);if(!ok)fails++;}
    void Log(string m,string trace,LogType t){if(t==LogType.Error||t==LogType.Warning||t==LogType.Exception||t==LogType.Assert)issues.Add(t+": "+m);}
    IEnumerator Start(){
        Application.logMessageReceived+=Log;yield return null;yield return null;
        var w=FindObjectOfType<BrazilWorld>();var p=w.player;var animator=p.GetComponent<Animator>();var sr=p.GetComponent<SpriteRenderer>();
        var originalPosition=p.transform.position;var originalScale=p.transform.localScale;p.enabled=false;
        Check(Screen.width==1920&&Screen.height==1080,"Teste real em Full HD");
        foreach(var dir in new[]{"Left","Right"}){
            Vector2 input=dir=="Left"?Vector2.left:Vector2.right;
            p.Teleport(new Vector3(dir=="Left"?1:-1,-3,0));p.SetMovementInput(input);
            var seen=new HashSet<Sprite>();float start=Time.realtimeSinceStartup;float maxTime=0;var walkedFrom=p.transform.position;
            while(Time.realtimeSinceStartup-start<1.85f){
                p.SendMessage("FixedUpdate");yield return new WaitForFixedUpdate();
                var state=animator.GetCurrentAnimatorStateInfo(0);maxTime=Mathf.Max(maxTime,state.normalizedTime);
                if(sr.sprite.name.StartsWith("Walk"+dir+"_"))seen.Add(sr.sprite);
            }
            Check(seen.Count==8,"Oito frames diferentes exibidos continuamente: "+dir);
            Check(maxTime>2,"Animator completou mais de dois ciclos sem reiniciar a cada entrada: "+dir);
            Check(seen.All(s=>AssetDatabase.GetAssetPath(s)==QuestSideWalkEditor.Art+"Walk"+dir+".png"),"Usa os novos desenhos: "+dir);
            Check(seen.All(s=>s.bounds.size.y>.47f&&s.bounds.size.y<.48f&&Mathf.Abs(s.bounds.min.y)<.002f),"Escala uniforme e referencia dos pes: "+dir);
            Check(Vector3.Distance(walkedFrom,p.transform.position)>.1f,"Personagem realmente se deslocou: "+dir);
            yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(QuestSideWalkEditor.Docs+"Walk"+dir+"-play.png");
            p.SetMovementInput(Vector2.zero);yield return null;yield return null;
            Check(animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"+dir)&&sr.sprite.name.StartsWith("Idle"+dir+"_"),"Parada conserva Idle original: "+dir);
            Check(p.transform.localScale==originalScale,"Escala do GameObject preservada: "+dir);
        }
        // Contato visual: linhas 1/3 sao os frames antigos, 2/4 sao os novos, todos renderizados pela Unity.
        RenderComparison();
        p.Teleport(originalPosition);p.SetMovementInput(Vector2.down);p.SetMovementInput(Vector2.zero);p.enabled=true;
        Check(issues.Count==0,"Sem erros/avisos durante caminhada");results.AddRange(issues);results.Add("Falhas: "+fails);File.WriteAllLines(QuestSideWalkEditor.Docs+"lateral-playmode.txt",results);
        Application.logMessageReceived-=Log;new GameObject("RegressaoVisual_Temporaria").AddComponent<QuestHUDPlayChecks>();Destroy(gameObject);
    }
    static void RenderComparison(){
        var root=new GameObject("ComparacaoTemporaria");var camera=root.AddComponent<Camera>();camera.transform.position=new Vector3(100,100,-10);camera.orthographic=true;camera.orthographicSize=1.2f;
        camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.18f,.22f,.25f,1);camera.cullingMask=1<<31;
        var rt=new RenderTexture(1600,960,24);camera.targetTexture=rt;
        for(int row=0;row<4;row++){
            string direction=row<2?"Left":"Right";bool newer=row%2==1;
            string path=newer?QuestSideWalkEditor.Art+"Walk"+direction+".png":QuestHUDVisualSetup.Art+"Quest_Walk.png";
            var frames=AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().Where(s=>s.name.StartsWith("Walk"+direction+"_")).OrderBy(s=>s.name).ToArray();
            for(int i=0;i<8;i++){var go=new GameObject("Frame");go.transform.SetParent(root.transform,false);go.layer=31;go.transform.position=new Vector3(98.25f+i*.5f,100.7f-row*.6f,0);go.AddComponent<SpriteRenderer>().sprite=frames[i];}
        }
        camera.Render();var previous=RenderTexture.active;RenderTexture.active=rt;var image=new Texture2D(1600,960,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,1600,960),0,0);image.Apply();
        File.WriteAllBytes(QuestSideWalkEditor.Docs+"comparacao-unity.png",image.EncodeToPNG());RenderTexture.active=previous;camera.targetTexture=null;rt.Release();Destroy(rt);Destroy(image);Destroy(root);
    }
}


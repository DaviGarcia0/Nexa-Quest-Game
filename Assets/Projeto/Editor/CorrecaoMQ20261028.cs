using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using NexaQuest;
public static class CorrecaoMQ20261028
{
 public const string Docs="Documentation/CorrecaoMQ20261028/";
 const BindingFlags F=BindingFlags.Static|BindingFlags.Public|BindingFlags.NonPublic;
 [InitializeOnLoadMethod] static void Init(){EditorApplication.update+=()=>{
  const string req="Library/CorrecaoMQ.request";
  if(EditorApplication.isCompiling||EditorApplication.isUpdating||EditorApplication.isPlayingOrWillChangePlaymode||!File.Exists(req))return;
  var action=File.ReadAllText(req).Trim();File.Delete(req);Directory.CreateDirectory(Docs);
  try{if(action=="inspecao")Inspect();else if(action=="aplicar")Apply();else if(action=="testar")Type.GetType("CorrecaoMQValidation, Assembly-CSharp-Editor").GetMethod("Run").Invoke(null,null);}
  catch(Exception e){File.WriteAllText(Docs+"falha.txt",e.ToString());Debug.LogException(e);}
 };}
 public static void Inspect(){
  var setup=EditorSceneManager.GetSceneManagerSetup();var text=new StringBuilder();
  foreach(var s in setup)if(SceneManager.GetSceneByPath(s.path).isDirty)throw new InvalidOperationException("A cena aberta tem alterações não salvas.");
  try{
   foreach(string name in new[]{"MinhaQuest","Brasil"}){
    var scene=EditorSceneManager.OpenScene("Assets/Projeto/Scenes/"+name+".unity");
    foreach(var b in scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<Button>(true))){
     text.Append(name+"/"+Path(b.transform)+": ");
     for(int i=0;i<b.onClick.GetPersistentEventCount();i++)text.Append(b.onClick.GetPersistentMethodName(i)+"; ");
     text.AppendLine(" cue="+b.GetComponent<UIButtonAudio>()?.cue);
    }
   }
  }finally{EditorSceneManager.RestoreSceneManagerSetup(setup);}
  object[] counts={0,0,0};typeof(Editor).Assembly.GetType("UnityEditor.LogEntries").GetMethod("GetCountsByType",F).Invoke(null,counts);text.AppendLine("Console: erros="+counts[0]+" avisos="+counts[1]);
  var clip=AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Projeto/Audio/Polimento20261021/SFX/Normal.mp3");clip.LoadAudioData();var data=new float[clip.samples*clip.channels];clip.GetData(data,0);
  text.AppendLine("Normal: "+clip.samples+" samples; "+clip.frequency+" Hz; channels="+clip.channels+"; loadState="+clip.loadState);
  foreach(float db in new[]{-70f,-60f,-50f,-40f}){float threshold=Mathf.Pow(10,db/20);int first=Array.FindIndex(data,s=>Mathf.Abs(s)>=threshold);text.AppendLine("onset "+db+" dB: "+(first<0?-1:first/(float)clip.channels/clip.frequency*1000)+" ms");}
  File.WriteAllText(Docs+"inspecao.txt",text.ToString());
 }
 static string Path(Transform t)=>t.parent==null?t.name:Path(t.parent)+"/"+t.name;
 public static void Apply(){
  var setup=EditorSceneManager.GetSceneManagerSetup();
  if(setup.Any(s=>SceneManager.GetSceneByPath(s.path).isDirty))throw new InvalidOperationException("Salve a cena antes da correção.");
  try{
   var scene=EditorSceneManager.OpenScene("Assets/Projeto/Scenes/MinhaQuest.unity");
   foreach(string path in new[]{"MinhaQuestCanvas/MarketLayer/Mercado","MinhaQuestCanvas/WardrobeLayer/Vestuario"}){
    var r=scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<RectTransform>(true)).Single(t=>Path(t)==path);float scale=Mathf.Max(1920/r.sizeDelta.x,1080/r.sizeDelta.y);r.localScale=Vector3.one*scale;r.anchoredPosition=Vector2.zero;
    r.parent.GetComponent<Image>().color=Color.clear;
   }
   EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
   scene=EditorSceneManager.OpenScene("Assets/Projeto/Scenes/Brasil.unity");
   var buttons=scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<Button>(true));
   var report=new StringBuilder();
   foreach(var b in buttons){
    bool activity=Enumerable.Range(0,b.onClick.GetPersistentEventCount()).Any(i=>new[]{"SelecionarQuiz","SelecionarMinijogo"}.Contains(b.onClick.GetPersistentMethodName(i)));
    if(!activity)continue;
    var audio=b.GetComponent<UIButtonAudio>()??b.gameObject.AddComponent<UIButtonAudio>();
    if(audio.cue!=AudioCue.Normal){report.AppendLine("Preservado especial: "+Path(b.transform));continue;}
    if(!Enumerable.Range(0,b.onClick.GetPersistentEventCount()).Any(i=>b.onClick.GetPersistentTarget(i)==audio&&b.onClick.GetPersistentMethodName(i)=="Play")){
     UnityEditor.Events.UnityEventTools.AddPersistentListener(b.onClick,audio.Play);
    }
    var so=new SerializedObject(b);var calls=so.FindProperty("m_OnClick.m_PersistentCalls.m_Calls");
    for(int i=0;i<calls.arraySize;i++)if(calls.GetArrayElementAtIndex(i).FindPropertyRelative("m_Target").objectReferenceValue==audio){calls.MoveArrayElement(i,0);break;}
    so.ApplyModifiedPropertiesWithoutUndo();report.AppendLine(Path(b.transform));
   }
   EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);File.WriteAllText(Docs+"botoes-npc.txt",report.ToString());
   var importer=(AudioImporter)AssetImporter.GetAtPath("Assets/Projeto/Audio/Polimento20261021/SFX/Normal.mp3");var settings=importer.defaultSampleSettings;settings.preloadAudioData=true;importer.defaultSampleSettings=settings;importer.SaveAndReimport();
  }finally{EditorSceneManager.RestoreSceneManagerSetup(setup);}
 }
}

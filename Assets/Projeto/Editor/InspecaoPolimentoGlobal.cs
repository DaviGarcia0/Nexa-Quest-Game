using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
[InitializeOnLoad] public static class InspecaoPolimentoGlobal {
 public const string Docs="Documentation/PolimentoGlobal20261021/";
 const BindingFlags Flags=BindingFlags.Static|BindingFlags.Public|BindingFlags.NonPublic;
 static InspecaoPolimentoGlobal(){EditorApplication.update+=()=>{string p="Library/InspecaoGlobal.request";if(EditorApplication.isCompiling||EditorApplication.isUpdating||EditorApplication.isPlayingOrWillChangePlaymode||!File.Exists(p))return;File.Delete(p);try{Executar();}catch(Exception e){Directory.CreateDirectory(Docs);File.WriteAllText(Docs+"falha-inspecao.txt",e.ToString());}};}
 public static void Executar(){
  var setup=EditorSceneManager.GetSceneManagerSetup();if(setup.Any(s=>UnityEngine.SceneManagement.SceneManager.GetSceneByPath(s.path).isDirty))throw new InvalidOperationException("Salve as cenas abertas antes da inspeção.");
  Directory.CreateDirectory(Docs);var all=new StringBuilder();
  try{foreach(string name in new[]{"Menu","MapaMundi","MinhaQuest","Brasil"}){
   var scene=EditorSceneManager.OpenScene("Assets/Projeto/Scenes/"+name+".unity",OpenSceneMode.Single);var s=new StringBuilder();
   foreach(var root in scene.GetRootGameObjects())foreach(var t in root.GetComponentsInChildren<Transform>(true)){
    string path=t.name;for(var parent=t.parent;parent!=null;parent=parent.parent)path=parent.name+"/"+path;
    s.AppendLine(path+" | ativo="+t.gameObject.activeSelf+" | "+string.Join(",",t.GetComponents<Component>().Select(c=>c==null?"MISSING":c.GetType().Name)));
    var r=t as RectTransform;if(r!=null)s.AppendLine("  rect="+r.sizeDelta+" pos="+r.anchoredPosition+" anchors="+r.anchorMin+"/"+r.anchorMax+" scale="+r.localScale);
    var image=t.GetComponent<Image>();if(image!=null)s.AppendLine("  imagem="+AssetDatabase.GetAssetPath(image.sprite));
    var button=t.GetComponent<Button>();if(button!=null)for(int i=0;i<button.onClick.GetPersistentEventCount();i++)s.AppendLine("  clique="+button.onClick.GetPersistentTarget(i)+"."+button.onClick.GetPersistentMethodName(i));
   }
   File.WriteAllText(Docs+"antes-"+name+".txt",s.ToString());all.AppendLine(name+": objetos="+scene.GetRootGameObjects().Sum(g=>g.GetComponentsInChildren<Transform>(true).Length)+", audios="+scene.GetRootGameObjects().Sum(g=>g.GetComponentsInChildren<AudioSource>(true).Length));
  }}finally{EditorSceneManager.RestoreSceneManagerSetup(setup);}
  object[] counts={0,0,0};typeof(Editor).Assembly.GetType("UnityEditor.LogEntries").GetMethod("GetCountsByType",Flags).Invoke(null,counts);
  all.AppendLine("Console erros="+counts[0]+" avisos="+counts[1]+" mensagens="+counts[2]);all.AppendLine("Compilando="+EditorApplication.isCompiling+"\n"+DateTime.Now.ToString("s"));File.WriteAllText(Docs+"baseline.txt",all.ToString());
 }
}

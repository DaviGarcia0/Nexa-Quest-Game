using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
[InitializeOnLoad] public static class MinhaQuestBridge {
 public const string Docs="Documentation/MinhaQuest20261027/";
 const BindingFlags F=BindingFlags.Static|BindingFlags.Public|BindingFlags.NonPublic;
 static MinhaQuestBridge(){EditorApplication.update+=()=>{var p="Library/MinhaQuest.request";if(EditorApplication.isCompiling||EditorApplication.isUpdating||EditorApplication.isPlayingOrWillChangePlaymode||!File.Exists(p))return;var action=File.ReadAllText(p).Trim();File.Delete(p);Directory.CreateDirectory(Docs);try{if(action=="preflight"){var s=SceneManager.GetActiveScene();object[] counts={0,0,0};typeof(Editor).Assembly.GetType("UnityEditor.LogEntries").GetMethod("GetCountsByType",F).Invoke(null,counts);File.WriteAllText(Docs+"preflight.txt",s.path+"\nDirty: "+s.isDirty+"\nCompilando: "+EditorApplication.isCompiling+"\nErros: "+counts[0]+"\nAvisos: "+counts[1]+"\n"+DateTime.Now.ToString("s"));}else if(action=="montar")Type.GetType("MinhaQuestSetup, Assembly-CSharp-Editor").GetMethod("Run").Invoke(null,null);else if(action=="testar")Type.GetType("MinhaQuestValidation, Assembly-CSharp-Editor").GetMethod("Run").Invoke(null,null);}catch(Exception e){File.WriteAllText(Docs+"falha.txt",e.ToString());Debug.LogException(e);}};}
}

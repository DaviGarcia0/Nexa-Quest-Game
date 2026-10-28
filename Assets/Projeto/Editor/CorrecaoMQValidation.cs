using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using NexaQuest;
using NexaQuest.Brasil;
using NexaQuest.MinhaQuest;
[InitializeOnLoad] public static class CorrecaoMQValidation {
 const BindingFlags F=BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Static;
 [Serializable] class Setup {public SceneSetup[] scenes;}
 static CorrecaoMQValidation(){EditorApplication.playModeStateChanged+=s=>{
  if(s==PlayModeStateChange.EnteredPlayMode&&SessionState.GetBool("CMQ.Start",false)){SessionState.SetBool("CMQ.Start",false);new GameObject("TestesCorrecaoMQ").AddComponent<CorrecaoMQChecks>();}
  if(s!=PlayModeStateChange.EnteredEditMode||!SessionState.GetBool("CMQ.End",false))return;
  SessionState.SetBool("CMQ.End",false);SessionState.SetBool("NexaQuest.ValidandoQuiz",false);
  if(SessionState.GetBool("CMQ.VolumeExists",false))PlayerPrefs.SetFloat(NexaAudioManager.VolumeKey,SessionState.GetFloat("CMQ.Volume",1));else PlayerPrefs.DeleteKey(NexaAudioManager.VolumeKey);PlayerPrefs.Save();
  bool same=PlayerPrefs.GetString(ProgressionManager.ChavePersistencia,"")==SessionState.GetString("CMQ.Real","");File.WriteAllText(CorrecaoMQ20261028.Docs+"save-real.txt","Save real preservado: "+same);
  EditorSceneManager.RestoreSceneManagerSetup(JsonUtility.FromJson<Setup>(SessionState.GetString("CMQ.Setup","")).scenes);typeof(QuestHUDVisualValidation).GetMethod("RestoreGameView",F).Invoke(null,null);
  object[] counts={0,0,0};typeof(Editor).Assembly.GetType("UnityEditor.LogEntries").GetMethod("GetCountsByType",F).Invoke(null,counts);
  File.WriteAllText(CorrecaoMQ20261028.Docs+"console.txt","Erros: "+counts[0]+"\nAvisos: "+counts[1]+"\nMensagens: "+counts[2]+"\n"+SceneManager.GetActiveScene().path+"\nDirty: "+SceneManager.GetActiveScene().isDirty);
 };}
 public static void Run(){
  var setup=EditorSceneManager.GetSceneManagerSetup();if(setup.Any(s=>SceneManager.GetSceneByPath(s.path).isDirty))throw new InvalidOperationException("Salve antes de testar.");
  SessionState.SetString("CMQ.Setup",JsonUtility.ToJson(new Setup{scenes=setup}));SessionState.SetString("CMQ.Real",PlayerPrefs.GetString(ProgressionManager.ChavePersistencia,""));
  SessionState.SetBool("CMQ.VolumeExists",PlayerPrefs.HasKey(NexaAudioManager.VolumeKey));SessionState.SetFloat("CMQ.Volume",PlayerPrefs.GetFloat(NexaAudioManager.VolumeKey,1));
  PlayerPrefs.SetString("NexaQuest.Testes.Quiz","{\"versao\":1,\"coins\":200,\"energia\":100,\"apples\":1,\"sandwiches\":1,\"nexaKings\":1}");
  SessionState.SetBool("NexaQuest.ValidandoQuiz",true);SessionState.SetBool("CMQ.Start",true);SessionState.SetBool("CMQ.End",true);
  EditorSceneManager.OpenScene("Assets/Projeto/Scenes/MinhaQuest.unity");typeof(QuestHUDVisualValidation).GetMethod("SetGameView",F).Invoke(null,null);typeof(Editor).Assembly.GetType("UnityEditor.LogEntries").GetMethod("Clear",F).Invoke(null,null);EditorApplication.isPlaying=true;
 }
}
public class CorrecaoMQProbe:MonoBehaviour {
 public volatile bool armed;public double first=-1;int sampleRate;
 void Awake(){sampleRate=AudioSettings.outputSampleRate;}
 void OnAudioFilterRead(float[] samples,int channels){if(!armed)return;for(int i=0;i<samples.Length;i++)if(Mathf.Abs(samples[i])>.00001f){first=AudioSettings.dspTime+(i/channels)/(double)sampleRate;armed=false;break;}}
}
public class CorrecaoMQChecks:MonoBehaviour {
 readonly List<string> lines=new List<string>();int failures,normal,special;NexaAudioManager sound;MinhaQuestController mq;ProgressionManager progress;
 string Docs=>CorrecaoMQ20261028.Docs;
 void C(bool ok,string text){lines.Add((ok?"PASS: ":"FAIL: ")+text);if(!ok)failures++;File.WriteAllLines(Docs+"andamento.txt",lines);}
 void Heard(AudioCue cue){if(cue==AudioCue.Normal)normal++;if(cue==AudioCue.Iniciar)special++;}
 void Log(string m,string stack,LogType type){if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert){C(false,type+": "+m);File.WriteAllLines(Docs+"testes.txt",lines);EditorApplication.delayCall+=()=>EditorApplication.isPlaying=false;}}
 Button B(Transform root,string name)=>root.GetComponentsInChildren<Button>(true).Single(b=>b.name==name);
 PointerEventData Data(Vector2 pos)=>new PointerEventData(EventSystem.current){position=pos,button=PointerEventData.InputButton.Left};
 IEnumerator Click(Button b,bool isSpecial=false){
  yield return null;Canvas.ForceUpdateCanvases();var data=Data(b.transform.position);var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(data,hits);
  C(hits.Count>0&&hits[0].gameObject==b.gameObject,"Raycast alinhado: "+b.name);
  int n=isSpecial?special:normal;int frame=Time.frameCount;ExecuteEvents.Execute(b.gameObject,data,ExecuteEvents.pointerDownHandler);
  C((isSpecial?special:normal)==n+1&&Time.frameCount==frame,"Som no mesmo frame do pressionar: "+b.name);
  yield return new WaitForSecondsRealtime(.05f);
  ExecuteEvents.Execute(b.gameObject,data,ExecuteEvents.pointerUpHandler);ExecuteEvents.Execute(b.gameObject,data,ExecuteEvents.pointerClickHandler);
  C((isSpecial?special:normal)==n+1,"Um som por pressionar/soltar: "+b.name);yield return null;
 }
 IEnumerator Photo(string name){yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Docs+name+".png");yield return null;}
 void Fullscreen(Image art){
  var corners=new Vector3[4];art.rectTransform.GetWorldCorners(corners);
  C(corners[0].x<=.01f&&corners[0].y<=.01f&&corners[2].x>=Screen.width-.01f&&corners[2].y>=Screen.height-.01f,"Arte cobre viewport: "+art.name);
  C(Mathf.Abs(art.rectTransform.localScale.x-art.rectTransform.localScale.y)<.0001f&&art.preserveAspect,"Escala uniforme preserva aspecto: "+art.name);
 }
 IEnumerator Ready(string name){yield return null;float until=Time.unscaledTime+15;while(SceneManager.GetActiveScene().name!=name&&Time.unscaledTime<until)yield return null;yield return null;C(SceneManager.GetActiveScene().name==name,"Navegação: "+name);}
 IEnumerator Start(){
  DontDestroyOnLoad(gameObject);Application.runInBackground=true;Application.logMessageReceived+=Log;yield return null;yield return null;
  mq=FindObjectOfType<MinhaQuestController>();progress=ProgressionManager.Instance;sound=NexaAudioManager.Instance;sound.Played+=Heard;
  C(Screen.width==1920&&Screen.height==1080,"Play Mode real 1920x1080");
  C(sound.normalClick.loadState==AudioDataLoadState.Loaded&&sound.NormalStartSample>10000,"Clip original pré-carregado e silêncio inicial identificado");
  var importer=(AudioImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(sound.normalClick));C(importer.defaultSampleSettings.preloadAudioData,"Importador pré-carrega som normal");
  C(sound.uiSource.outputAudioMixerGroup.audioMixer==sound.mixer&&sound.uiSource.ignoreListenerPause,"UI usa Master e continua na pausa");
  foreach(var b in mq.GetComponentsInChildren<Button>(true)){var a=b.GetComponent<UIButtonAudio>();C(a!=null&&a.cue==AudioCue.Normal&&Enumerable.Range(0,b.onClick.GetPersistentEventCount()).Count(i=>b.onClick.GetPersistentTarget(i)==a&&b.onClick.GetPersistentMethodName(i)=="Play")==1,"Listener único normal: "+b.name);}
  var food=mq.foods[0];int previous=normal;var data=Data(food.transform.position);ExecuteEvents.Execute(food.gameObject,data,ExecuteEvents.pointerDownHandler);ExecuteEvents.Execute(food.gameObject,data,ExecuteEvents.beginDragHandler);data.position=new Vector2(1850,1000);ExecuteEvents.Execute(food.gameObject,data,ExecuteEvents.dragHandler);ExecuteEvents.Execute(food.gameObject,data,ExecuteEvents.endDragHandler);C(normal==previous,"Alimento sem som no PointerDown ou drag");
  sound.uiSource.Stop();yield return new WaitForSecondsRealtime(.12f);var probe=sound.uiSource.gameObject.AddComponent<CorrecaoMQProbe>();probe.armed=true;double press=AudioSettings.dspTime;
  var marketButton=B(mq.transform,"ButtonMercado");data=Data(marketButton.transform.position);previous=normal;ExecuteEvents.Execute(marketButton.gameObject,data,ExecuteEvents.pointerDownHandler);
  C(normal==previous+1&&!mq.market.activeSelf,"Som começa antes da abertura do Mercado");
  yield return new WaitForSecondsRealtime(.2f);AudioSettings.GetDSPBufferSize(out int buffer,out int blocks);
  double latency=(probe.first-press)*1000;File.WriteAllText(Docs+"latencia.txt","Offset fonte original: "+(sound.NormalStartSample/(float)sound.normalClick.frequency*1000)+" ms\nPrimeiro sinal no filtro DSP após pressionar: "+latency+" ms\nBuffer: "+buffer+" x "+blocks+"; saída "+AudioSettings.outputSampleRate+" Hz\nMede pipeline interno da Unity; não inclui dispositivo físico.");
  C(probe.first>=press&&latency<150,"Sinal DSP sem os 250ms de silêncio inicial");Destroy(probe);
  ExecuteEvents.Execute(marketButton.gameObject,data,ExecuteEvents.pointerUpHandler);ExecuteEvents.Execute(marketButton.gameObject,data,ExecuteEvents.pointerClickHandler);yield return null;
  C(normal==previous+1&&mq.market.activeSelf,"Soltar abre Mercado sem segundo som");var art=mq.market.transform.Find("Mercado").GetComponent<Image>();Fullscreen(art);C(!mq.mainControls.blocksRaycasts,"Tela principal bloqueada");yield return Photo("MercadoTelaCheia");
  int coins=progress.Coins;foreach(string name in new[]{"ButtonApple","ButtonSandwich","ButtonNexaKing"}){yield return Click(B(mq.market.transform,name));C(mq.popupLayer.activeSelf&&mq.popupArt.sprite==mq.purchaseArt,"Compra continua funcionando: "+name);yield return Click(B(mq.popupLayer.transform,"ButtonOK"));}
  C(progress.Coins==coins-60,"Preços 5, 15 e 40 preservados");C(progress.Energia==100,"Compras não alteram Energia");
  foreach(var sprite in new[]{mq.purchaseArt,mq.insufficientArt,mq.fullArt,mq.unavailableArt})foreach(string name in new[]{"ButtonOK","ButtonX"}){mq.ShowPopup(sprite);yield return null;yield return Click(B(mq.popupLayer.transform,name));C(!mq.popupLayer.activeSelf&&mq.market.activeSelf,"Popup fecha sobre Mercado: "+sprite.name+"/"+name);}
  mq.ShowPopup(mq.purchaseArt);yield return Photo("PopupSobreMercado");yield return Click(B(mq.popupLayer.transform,"ButtonX"));
  yield return Click(B(mq.market.transform,"ButtonCloseMarket"));C(!mq.market.activeSelf&&sound.uiSource.isPlaying,"Fechar Mercado mantém som global tocando");
  yield return Click(B(mq.transform,"ButtonVestuario"));var wardrobe=mq.wardrobe.transform.Find("Vestuario");Fullscreen(wardrobe.GetComponent<Image>());var animator=wardrobe.Find("QuestCorpo").GetComponent<Animator>();C(animator.GetCurrentAnimatorStateInfo(0).IsName("Idle")&&animator.GetCurrentAnimatorStateInfo(0).loop&&wardrobe.childCount==2,"Quest Idle e grade vazia preservadas");yield return Photo("VestuarioTelaCheia");yield return Click(B(mq.wardrobe.transform,"ButtonCloseWardrobe"));C(!mq.wardrobe.activeSelf,"X retorna sem trocar cena");
  yield return Click(B(mq.transform,"ButtonLoja"));C(mq.popupArt.sprite==mq.unavailableArt,"Loja indisponível preservada");yield return Click(B(mq.popupLayer.transform,"ButtonOK"));
  previous=normal;B(mq.transform,"ButtonLoja").onClick.Invoke();C(normal==previous+1,"Clique de teclado/programático continua com áudio");yield return null;yield return Click(B(mq.popupLayer.transform,"ButtonX"));
  var pause=PauseSettingsManager.Instance;pause.Open();sound.SetMasterVolume(0);float db;C(sound.mixer.GetFloat("MasterVolume",out db)&&db==-80,"Volume geral zero silencia o Master");yield return Click(B(pause.panel.transform,"Voltar"));sound.SetMasterVolume(1);
  yield return Click(B(mq.transform,"ButtonVoltar"));yield return Ready("Menu");C(sound==NexaAudioManager.Instance&&sound.uiSource.isPlaying,"Fonte de UI sobrevive à troca de cena");
  yield return Click(B(GameObject.Find("JanelaMenu").transform,"botaoJogar"),true);yield return Ready("MapaMundi");
  var worldMap=GameObject.Find("Janela_MapaMundi");yield return Click(B(worldMap.transform,"botao_AmericaSul"));var south=FindObjectsOfType<Canvas>(true).Single(c=>c.name=="JanelaAmericaSul");yield return Click(B(south.transform,"Brasil"),true);yield return Ready("Brasil");C(special==2,"Jogar e Brasil mantêm clip especial");
  var world=FindObjectOfType<BrazilWorld>();var pf=FindObjectOfType<PainelDesafiosFeira>(true);pf.Abrir();yield return Click(B(pf.transform,"BotaoQuiz"));C(FindObjectOfType<QuizFeira>(true).Estado!=EstadoQuizFeira.Fechado,"Quiz Feira abre");FindObjectOfType<QuizFeira>(true).Voltar();pf.Fechar();yield return null;
  pf.Abrir();yield return Click(B(pf.transform,"BotaoMinijogo"));var mf=FindObjectOfType<MinijogoFeira>(true);C(mf.Estado!=EstadoMinijogoFeira.Fechado,"Minijogo Feira abre");mf.Sair();mf.Sair();pf.Fechar();yield return null;
  var area=world.areas.Single(a=>a.name=="Amazonia");world.TravelTo(area,area.defaultSpawn);yield return new WaitForSecondsRealtime(.85f);var pa=FindObjectOfType<PainelDesafiosAmazonia>(true);pa.Abrir();yield return Click(B(pa.transform,"BotaoQuiz"));C(FindObjectOfType<QuizAmazonia>(true).Estado!=EstadoQuizAmazonia.Fechado,"Quiz Amazônia abre");FindObjectOfType<QuizAmazonia>(true).Voltar();pa.Fechar();yield return null;
  pa.Abrir();yield return Click(B(pa.transform,"BotaoMinijogo"));var ma=FindObjectOfType<MinijogoAmazoniaController>(true);C(ma.Estado!=EstadoMergeAmazonia.Fechado,"Minijogo Amazônia abre");ma.Sair();pa.Fechar();
  C(FindObjectsOfType<NexaAudioManager>().Length==1,"Sem AudioManager duplicado");
  sound.Played-=Heard;Application.logMessageReceived-=Log;lines.Add("Falhas: "+failures);File.WriteAllLines(Docs+"testes.txt",lines);EditorApplication.isPlaying=false;
 }
}

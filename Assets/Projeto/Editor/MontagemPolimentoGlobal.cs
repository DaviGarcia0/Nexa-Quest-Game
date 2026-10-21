using System;
using System.IO;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;
using NexaQuest;
[InitializeOnLoad] public static class MontagemPolimentoGlobal {
 public const string Docs="Documentation/PolimentoGlobal20261021/",Art="Assets/Projeto/Sprites/Global20261021/",AudioPath="Assets/Projeto/Audio/Polimento20261021/";
 const BindingFlags Flags=BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.Static;
 static TMP_FontAsset font;
 static MontagemPolimentoGlobal(){EditorApplication.update+=()=>{string p="Library/PolimentoGlobal.request";if(EditorApplication.isCompiling||EditorApplication.isUpdating||EditorApplication.isPlayingOrWillChangePlaymode||!File.Exists(p))return;string action=File.ReadAllText(p).Trim();File.Delete(p);try{if(action=="montar")Montar();else if(action=="testar")Type.GetType("ValidacaoPolimentoGlobal, Assembly-CSharp-Editor").GetMethod("Executar").Invoke(null,null);}catch(Exception e){Directory.CreateDirectory(Docs);File.WriteAllText(Docs+"falha.txt",e.ToString());Debug.LogException(e);}};}
 static RectTransform R(string n,Transform p,Vector2 size,Vector2 pos){var r=new GameObject(n,typeof(RectTransform)).GetComponent<RectTransform>();r.gameObject.layer=5;r.SetParent(p,false);r.sizeDelta=size;r.anchoredPosition=pos;return r;}
 static RectTransform Tela(string n,Transform p){var r=R(n,p,Vector2.zero,Vector2.zero);r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=r.offsetMax=Vector2.zero;return r;}
 static Sprite S(string n)=>AssetDatabase.LoadAssetAtPath<Sprite>(Art+n+".png");
 static Image Im(string n,Transform p,Sprite sprite,float w,Vector2 pos){var r=R(n,p,new Vector2(w,w*sprite.rect.height/sprite.rect.width),pos);var i=r.gameObject.AddComponent<Image>();i.sprite=sprite;i.preserveAspect=true;i.raycastTarget=false;return i;}
 static Image Cor(string n,Transform p,Vector2 size,Vector2 pos,Color c){var i=R(n,p,size,pos).gameObject.AddComponent<Image>();i.color=c;i.raycastTarget=false;return i;}
 static TMP_Text T(string n,Transform p,Vector2 size,Vector2 pos,string text,int fs){var t=R(n,p,size,pos).gameObject.AddComponent<TextMeshProUGUI>();t.font=font;t.fontSize=fs;t.alignment=TextAlignmentOptions.Center;t.color=Color.white;t.text=text;t.raycastTarget=false;return t;}
 static void Sound(Button b,bool special=false){var s=b.GetComponent<UIButtonAudio>()??b.gameObject.AddComponent<UIButtonAudio>();s.cue=special?AudioCue.Iniciar:AudioCue.Normal;
  if(Enumerable.Range(0,b.onClick.GetPersistentEventCount()).Any(i=>b.onClick.GetPersistentTarget(i)==s))return;
  UnityEventTools.AddPersistentListener(b.onClick,s.Play);var so=new SerializedObject(b);var calls=so.FindProperty("m_OnClick.m_PersistentCalls.m_Calls");calls.MoveArrayElement(calls.arraySize-1,0);so.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(b);
 }
 static Button Botao(string n,Transform p,Sprite sprite,float w,Vector2 pos,UnityEngine.Events.UnityAction action){var image=Im(n,p,sprite,w,pos);image.raycastTarget=true;var b=image.gameObject.AddComponent<Button>();b.targetGraphic=image;b.navigation=new Navigation{mode=Navigation.Mode.None};if(action!=null)UnityEventTools.AddPersistentListener(b.onClick,action);Sound(b);return b;}
 static Vector2 Pixel(Image i,Vector2 p)=>new Vector2((p.x-i.sprite.rect.width/2)*i.rectTransform.sizeDelta.x/i.sprite.rect.width,(i.sprite.rect.height/2-p.y)*i.rectTransform.sizeDelta.y/i.sprite.rect.height);
 static Button Area(string n,Image art,Rect px){var r=R(n,art.transform,new Vector2(px.width*art.rectTransform.sizeDelta.x/art.sprite.rect.width,px.height*art.rectTransform.sizeDelta.y/art.sprite.rect.height),Pixel(art,px.center));var img=r.gameObject.AddComponent<Image>();img.color=Color.clear;var b=r.gameObject.AddComponent<Button>();b.targetGraphic=img;b.transition=Selectable.Transition.None;b.navigation=new Navigation{mode=Navigation.Mode.None};return b;}
 static void ConfigureCanvas(Canvas c){c.renderMode=RenderMode.ScreenSpaceOverlay;var s=c.GetComponent<CanvasScaler>()??c.gameObject.AddComponent<CanvasScaler>();s.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;s.referenceResolution=new Vector2(1920,1080);s.matchWidthOrHeight=.5f;var r=(RectTransform)c.transform;r.localScale=Vector3.one;}
 static AudioClip Clip(string file)=>AssetDatabase.LoadAssetAtPath<AudioClip>(AudioPath+file);
 static AudioMixer CriarMixer(){
  string path=AudioPath+"NexaMaster.mixer";var type=AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("UnityEditor.Audio.AudioMixerController")).First(t=>t!=null);
  var mixer=(AudioMixer)type.GetMethod("CreateMixerControllerAtPath",Flags).Invoke(null,new object[]{path});var master=type.GetProperty("masterGroup",Flags).GetValue(mixer);var guid=master.GetType().GetMethod("GetGUIDForVolume",Flags).Invoke(master,null);
  var prop=type.GetProperty("exposedParameters",Flags);var element=prop.PropertyType.GetElementType();var parameter=Activator.CreateInstance(element);element.GetField("guid",Flags).SetValue(parameter,guid);element.GetField("name",Flags).SetValue(parameter,"MasterVolume");var array=Array.CreateInstance(element,1);array.SetValue(parameter,0);prop.SetValue(mixer,array);EditorUtility.SetDirty(mixer);return mixer;
 }
 static void GlobalPrefab(){
  Directory.CreateDirectory("Assets/Projeto/Resources");var mixer=CriarMixer();var master=mixer.FindMatchingGroups("Master").First();var root=new GameObject("NexaGlobalSystems");var a=root.AddComponent<NexaAudioManager>();a.mixer=mixer;
  AudioSource Source(string n,float gain,bool ignore,bool loop){var g=new GameObject(n);g.transform.SetParent(root.transform);var s=g.AddComponent<AudioSource>();s.playOnAwake=false;s.loop=loop;s.volume=gain;s.ignoreListenerPause=ignore;s.outputAudioMixerGroup=master;return s;}
  a.musicSource=Source("MusicSource",.28f,true,true);a.uiSource=Source("UISource",.7f,true,false);a.sfxSource=Source("SFXSource",.65f,false,false);
  a.menuMusic=Clip("Music/MenuMapa.mp3");a.feiraMusic=Clip("Music/Feira.ogg");a.amazoniaMusic=Clip("Music/Amazonia.ogg");a.pontoMusic=Clip("Music/PontoTuristico.ogg");a.portalMusic=Clip("Music/Portal.ogg");a.normalClick=Clip("SFX/Normal.mp3");a.iniciar=Clip("SFX/Iniciar.mp3");a.quizCorreto=Clip("SFX/QuizCorreto.mp3");a.quizErrado=Clip("SFX/QuizErrado.mp3");a.erroFeira=Clip("SFX/ErroFeira.mp3");a.fusao=Clip("SFX/Fusao.mp3");
  var settings=root.AddComponent<PauseSettingsManager>();var cv=R("ConfiguracoesGlobais",root.transform,Vector2.zero,Vector2.zero);var canvas=cv.gameObject.AddComponent<Canvas>();ConfigureCanvas(canvas);canvas.sortingOrder=32700;cv.gameObject.AddComponent<GraphicRaycaster>();
  var shade=Tela("Overlay",cv).gameObject.AddComponent<Image>();shade.color=new Color(0,0,0,.55f);
  var panel=Im("PainelVolume",cv,S("Volume"),700,new Vector2(0,65));var sliderRect=R("VolumeGeralSlider",panel.transform,new Vector2(450,42),new Vector2(0,-35));var slider=sliderRect.gameObject.AddComponent<Slider>();
  Cor("Borda",sliderRect,new Vector2(450,30),Vector2.zero,new Color(1,.7f,.06f));var bg=Cor("Background",sliderRect,new Vector2(442,22),Vector2.zero,new Color(.015f,.04f,.23f));bg.raycastTarget=true;
  var fillArea=R("FillArea",sliderRect,new Vector2(430,16),Vector2.zero);var fill=Cor("Fill",fillArea,Vector2.zero,Vector2.zero,new Color(.06f,.35f,1));fill.rectTransform.anchorMin=Vector2.zero;fill.rectTransform.anchorMax=Vector2.one;fill.rectTransform.offsetMin=fill.rectTransform.offsetMax=Vector2.zero;
  var handleArea=R("HandleArea",sliderRect,new Vector2(430,42),Vector2.zero);var handle=Cor("Handle",handleArea,new Vector2(24,0),Vector2.zero,new Color(1,.8f,.16f));handle.raycastTarget=true;
  slider.fillRect=fill.rectTransform;slider.handleRect=handle.rectTransform;slider.targetGraphic=handle;slider.minValue=0;slider.maxValue=1;slider.value=1;slider.navigation=new Navigation{mode=Navigation.Mode.None};UnityEventTools.AddPersistentListener(slider.onValueChanged,a.SetMasterVolume);
  settings.panel=cv.gameObject;settings.volume=slider;
  Botao("Voltar",cv,S("Voltar"),330,new Vector2(-375,-340),settings.Close);Botao("MinhaQuest",cv,S("MinhaQuest"),370,new Vector2(0,-340),settings.MinhaQuest);Botao("Sair",cv,S("Sair"),330,new Vector2(375,-340),settings.Sair);
  cv.gameObject.SetActive(false);PrefabUtility.SaveAsPrefabAsset(root,"Assets/Projeto/Resources/NexaGlobalSystems.prefab");UnityEngine.Object.DestroyImmediate(root);
 }
 static void Menu(){
  var scene=EditorSceneManager.OpenScene("Assets/Projeto/Scenes/Menu.unity",OpenSceneMode.Single);var canvas=GameObject.Find("JanelaMenu").GetComponent<Canvas>();var actions=GameObject.Find("SceneLoader").AddComponent<GlobalUIActions>();
  foreach(var b in canvas.GetComponentsInChildren<Button>(true))Sound(b,b.name=="botaoJogar");var config=canvas.transform.Find("botaoConfig").GetComponent<Button>();UnityEventTools.AddPersistentListener(config.onClick,actions.Configuracoes);
  Botao("botaoSair",canvas.transform,S("Sair"),280,new Vector2(508.93f,-470),actions.Sair);
  var fallback=canvas.transform.Find("Background").GetComponent<Image>();var r=Tela("VideoBackground",canvas.transform);r.SetSiblingIndex(fallback.transform.GetSiblingIndex()+1);var image=r.gameObject.AddComponent<RawImage>();image.raycastTarget=false;
  var rt=new RenderTexture(3840,2160,0,RenderTextureFormat.ARGB32){name="MenuVideo4K"};AssetDatabase.CreateAsset(rt,"Assets/Projeto/Video/MenuVideo.renderTexture");image.texture=rt;
  var player=r.gameObject.AddComponent<VideoPlayer>();player.clip=AssetDatabase.LoadAssetAtPath<VideoClip>("Assets/Projeto/Video/MenuFinal.mp4");player.renderMode=VideoRenderMode.RenderTexture;player.targetTexture=rt;player.audioOutputMode=VideoAudioOutputMode.None;player.isLooping=true;player.playOnAwake=true;player.waitForFirstFrame=true;player.aspectRatio=VideoAspectRatio.FitOutside;
  var video=r.gameObject.AddComponent<MenuVideoBackground>();video.display=image;video.fallback=fallback;image.enabled=false;
  EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
 }
 static void Mapa(){
  var scene=EditorSceneManager.OpenScene("Assets/Projeto/Scenes/MapaMundi.unity",OpenSceneMode.Single);var world=GameObject.Find("Janela_MapaMundi").GetComponent<Canvas>();var south=Resources.FindObjectsOfTypeAll<Canvas>().First(c=>c.gameObject.scene==scene&&c.name=="JanelaAmericaSul");ConfigureCanvas(south);south.sortingOrder=1;
  var root=new GameObject("NavegacaoMapaMundi");var nav=root.AddComponent<SceneLoader>();var ui=root.AddComponent<MapaMundiUI>();var art=Im("MapaAmericaDoSul",south.transform,S("AmericaDoSul"),1920,Vector2.zero);
  var brasil=Area("Brasil",art,new Rect(910,278,265,85));UnityEventTools.AddStringPersistentListener(brasil.onClick,nav.CarregarCena,"Brasil");Sound(brasil,true);
  foreach(var def in new[]{new {name="Chile",rect=new Rect(610,389,174,139)},new{name="Argentina",rect=new Rect(810,548,275,95)}}){var b=Area(def.name,art,def.rect);UnityEventTools.AddPersistentListener(b.onClick,ui.Indisponivel);Sound(b);}
  var back=Area("Voltar",art,new Rect(45,820,260,82));UnityEventTools.AddBoolPersistentListener(back.onClick,south.gameObject.SetActive,false);UnityEventTools.AddBoolPersistentListener(back.onClick,world.gameObject.SetActive,true);Sound(back);
  foreach(string n in new[]{"botao_AmericaSul","botao_AmericaSul_brilhante"}){var b=world.transform.Find(n).GetComponent<Button>();b.onClick=new Button.ButtonClickedEvent();UnityEventTools.AddBoolPersistentListener(b.onClick,south.gameObject.SetActive,true);UnityEventTools.AddBoolPersistentListener(b.onClick,world.gameObject.SetActive,false);Sound(b);}
  foreach(var b in world.GetComponentsInChildren<Button>(true))Sound(b);
  var voltar=Botao("VoltarMapaMundi",world.transform,S("Voltar"),260,Vector2.zero,null);var vr=(RectTransform)voltar.transform;vr.anchorMin=vr.anchorMax=new Vector2(0,0);vr.anchoredPosition=new Vector2(150,75);UnityEventTools.AddStringPersistentListener(voltar.onClick,nav.CarregarCena,"Menu");
  var popup=Cor("MapaIndisponivel",south.transform,new Vector2(610,70),new Vector2(0,-375),new Color(.015f,.04f,.23f,.96f));ui.popup=popup.gameObject;ui.message=T("Mensagem",popup.transform,new Vector2(590,60),Vector2.zero,"Mapa indisponível por enquanto.",28);popup.gameObject.SetActive(false);south.gameObject.SetActive(false);
  EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
 }
 public static void Montar(){
  if(EditorSceneManager.GetSceneManagerSetup().Any(s=>SceneManager.GetSceneByPath(s.path).isDirty))throw new InvalidOperationException("Salve as cenas antes da montagem.");
  if(File.Exists("Assets/Projeto/Resources/NexaGlobalSystems.prefab"))throw new InvalidOperationException("Integração já existe; não sobrescrever ajustes.");
  foreach(string path in AssetDatabase.FindAssets("t:Texture2D",new[]{Art.TrimEnd('/')} ).Select(AssetDatabase.GUIDToAssetPath)){var i=(TextureImporter)AssetImporter.GetAtPath(path);i.textureType=TextureImporterType.Sprite;i.spriteImportMode=SpriteImportMode.Single;i.filterMode=FilterMode.Point;i.textureCompression=TextureImporterCompression.Uncompressed;i.maxTextureSize=8192;i.mipmapEnabled=false;i.npotScale=TextureImporterNPOTScale.None;i.alphaSource=TextureImporterAlphaSource.FromInput;i.alphaIsTransparency=true;var st=new TextureImporterSettings();i.ReadTextureSettings(st);st.spriteMeshType=SpriteMeshType.FullRect;st.spriteGenerateFallbackPhysicsShape=false;i.SetTextureSettings(st);i.SaveAndReimport();}
  font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");if(font==null)font=Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
  var setup=EditorSceneManager.GetSceneManagerSetup();try{GlobalPrefab();Menu();Mapa();AssetDatabase.SaveAssets();}finally{EditorSceneManager.RestoreSceneManagerSetup(setup);}
  File.WriteAllText(Docs+"montagem.txt","Montagem concluída: "+DateTime.Now.ToString("s"));
 }
}

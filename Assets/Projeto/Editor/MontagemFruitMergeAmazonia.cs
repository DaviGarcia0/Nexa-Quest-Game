using System;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEditor.U2D.Sprites;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using NexaQuest.Brasil;
[InitializeOnLoad] public static class MontagemFruitMergeAmazonia {
 public const string Arte="Assets/Projeto/Sprites/Brasil/FruitMergeAmazonia/",Docs="Documentation/Brasil/FruitMergeAmazonia20261013/";
 [Serializable] class AssetsInfo {public AssetInfo[] assets;}
 [Serializable] class AssetInfo {public string name;public int width,height,x,y,w,h;}
 static TMP_FontAsset fonte;
 static MontagemFruitMergeAmazonia(){EditorApplication.update+=()=>{
  string path="Library/FruitMergeAmazonia.solicitacao";if(EditorApplication.isCompiling||EditorApplication.isUpdating||EditorApplication.isPlayingOrWillChangePlaymode||!File.Exists(path))return;
  string action=File.ReadAllText(path).Trim();File.Delete(path);try {if(action=="montar")Montar();else if(action=="ajustar")Ajustar();else if(action=="testar")ValidacaoFruitMergeAmazonia.Executar();}catch(Exception e){Directory.CreateDirectory(Docs);File.WriteAllText(Docs+"falha.txt",e.ToString());Debug.LogException(e);}
 };}
 static Sprite S(string nome)=>AssetDatabase.LoadAllAssetsAtPath(Arte+nome+".png").OfType<Sprite>().First();
 static void Importar(){
  foreach(var d in JsonUtility.FromJson<AssetsInfo>(File.ReadAllText(Arte+"Importacao.json")).assets){
   bool crop=d.name.StartsWith("Nivel")||new[]{"Coins","CoinPopup","LinhaPerigo","Proximo","Objetivos","QuestMoldura","Sair"}.Contains(d.name);
   Configurar(Arte+d.name+".png",crop?new[]{new SpriteRect{name=d.name,rect=new Rect(d.x,d.y,d.w,d.h),pivot=new Vector2(.5f,.5f),alignment=SpriteAlignment.Center,spriteID=GUID.Generate()}}:null);
  }
  Configurar(Arte+"CoinOficial.png",new[]{new SpriteRect{name="NexaCoinOficial",rect=new Rect(350,140,400,400),pivot=new Vector2(.5f,.5f),alignment=SpriteAlignment.Center,spriteID=GUID.Generate()}});
  var rects=new SpriteRect[8];for(int n=0;n<8;n++)rects[n]=new SpriteRect{name="QuestFesta"+n,rect=new Rect(n*271.5f,160,271.5f,410),pivot=new Vector2(.5f,.5f),alignment=SpriteAlignment.Center,spriteID=GUID.Generate()};Configurar(Arte+"QuestComemoracao.png",rects);
  rects=new SpriteRect[6];for(int n=0;n<6;n++)rects[n]=new SpriteRect{name="Fusao"+n,rect=new Rect(n*256,768,256,256),pivot=new Vector2(.5f,.5f),alignment=SpriteAlignment.Center,spriteID=GUID.Generate()};Configurar(Arte+"Fusao.png",rects);
 }
 static void Configurar(string path,SpriteRect[] rects){
  var i=(TextureImporter)AssetImporter.GetAtPath(path);i.textureType=TextureImporterType.Sprite;i.spriteImportMode=rects==null?SpriteImportMode.Single:SpriteImportMode.Multiple;i.filterMode=FilterMode.Point;i.textureCompression=TextureImporterCompression.Uncompressed;i.mipmapEnabled=false;i.npotScale=TextureImporterNPOTScale.None;i.maxTextureSize=8192;i.alphaSource=TextureImporterAlphaSource.FromInput;i.alphaIsTransparency=true;i.spritePixelsPerUnit=100;
  var settings=new TextureImporterSettings();i.ReadTextureSettings(settings);settings.spriteMeshType=SpriteMeshType.FullRect;settings.spriteGenerateFallbackPhysicsShape=false;i.SetTextureSettings(settings);i.SaveAndReimport();
  if(rects!=null){var factory=new SpriteDataProviderFactories();factory.Init();var p=factory.GetSpriteEditorDataProviderFromObject(i);p.InitSpriteEditorDataProvider();p.SetSpriteRects(rects);p.GetDataProvider<ISpriteNameFileIdDataProvider>().SetNameFileIdPairs(rects.Select(r=>new SpriteNameFileIdPair(r.name,r.spriteID)));p.Apply();AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceUpdate);}
 }
 static RectTransform R(string name,Transform parent,Vector2 size,Vector2 pos){var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();r.gameObject.layer=5;r.SetParent(parent,false);r.sizeDelta=size;r.anchoredPosition=pos;return r;}
 static RectTransform Tela(string name,Transform parent){var r=R(name,parent,Vector2.zero,Vector2.zero);r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=r.offsetMax=Vector2.zero;return r;}
 static Image I(string name,Transform parent,Sprite sprite,float w,Vector2 pos){var r=R(name,parent,new Vector2(w,w*sprite.rect.height/sprite.rect.width),pos);var image=r.gameObject.AddComponent<Image>();image.sprite=sprite;image.preserveAspect=true;image.raycastTarget=false;return image;}
 static TMP_Text T(string name,Transform parent,Vector2 size,Vector2 pos,int fs){var t=R(name,parent,size,pos).gameObject.AddComponent<TextMeshProUGUI>();t.font=fonte;t.fontSize=fs;t.alignment=TextAlignmentOptions.Center;t.raycastTarget=false;t.color=new Color(1,.96f,.75f);return t;}
 static Vector2 P(Image image,Vector2 pixels){var rect=image.sprite.rect;return new Vector2((pixels.x-rect.x-rect.width/2)*image.rectTransform.sizeDelta.x/rect.width,(image.sprite.texture.height-pixels.y-rect.y-rect.height/2)*image.rectTransform.sizeDelta.y/rect.height);}
 static Button B(string name,Image image,Rect pixels,UnityEngine.Events.UnityAction action){var rect=image.sprite.rect;var r=R(name,image.transform,new Vector2(pixels.width*image.rectTransform.sizeDelta.x/rect.width,pixels.height*image.rectTransform.sizeDelta.y/rect.height),P(image,pixels.center));var target=r.gameObject.AddComponent<Image>();target.color=Color.clear;var b=r.gameObject.AddComponent<Button>();b.targetGraphic=target;b.transition=Selectable.Transition.None;b.navigation=new Navigation{mode=Navigation.Mode.None};UnityEventTools.AddPersistentListener(b.onClick,action);return b;}
 static Image Painel(string name,Transform parent,string sprite,float w){var root=Tela(name,parent);root.gameObject.AddComponent<Image>().color=new Color(0,0,0,.24f);var i=I("Arte",root,S(sprite),w,Vector2.zero);root.gameObject.SetActive(false);return i;}
 static void CriarCoinOficial(Image frame){var r=R("NexaCoinOficial",frame.transform,new Vector2(60,60),P(frame,new Vector2(650,340)));var mask=r.gameObject.AddComponent<Image>();mask.sprite=Resources.GetBuiltinResource<Sprite>("UI/Skin/Knob.psd");mask.raycastTarget=false;r.gameObject.AddComponent<Mask>().showMaskGraphic=false;I("SpriteOficial",r,S("CoinOficial"),60,Vector2.zero);}
 static RectTransform CriarCheck(string name,Transform parent,Vector2 pos){
  var r=R(name,parent,new Vector2(25,25),pos);var bg=r.gameObject.AddComponent<Image>();bg.sprite=Resources.GetBuiltinResource<Sprite>("UI/Skin/Knob.psd");bg.color=new Color(.1f,.65f,.12f);bg.raycastTarget=false;
  var a=R("TracoCurto",r,new Vector2(9,3),new Vector2(-5,-1));a.localEulerAngles=new Vector3(0,0,-45);a.gameObject.AddComponent<Image>().raycastTarget=false;
  var b=R("TracoLongo",r,new Vector2(16,3),new Vector2(3,1));b.localEulerAngles=new Vector3(0,0,45);b.gameObject.AddComponent<Image>().raycastTarget=false;return r;
 }
 public static void Ajustar(){
  var cena=SceneManager.GetActiveScene();if(cena.isDirty)throw new InvalidOperationException("Salve Brasil.");Importar();var jogo=UnityEngine.Object.FindObjectOfType<MinijogoAmazoniaController>(true);var ui=jogo.interfaceJogo;
  for(int i=0;i<8;i++)jogo.evolucoes[i].sprite=S("Nivel"+(i+1));
  float[] centros={0,-.08f,-.02f,-.04f,0,-.02f,0,-.02f};for(int i=0;i<8;i++)jogo.evolucoes[i].centroVisual=new Vector2(0,centros[i]);
  ui.quest.rectTransform.sizeDelta=new Vector2(90,123);ui.proximo.rectTransform.sizeDelta=new Vector2(60,67);
  var hud=ui.hud.transform;var obj=hud.Find("HUDObjetivos").GetComponent<Image>();obj.rectTransform.anchoredPosition=new Vector2(0,-420);
  for(int i=0;i<3;i++){UnityEngine.Object.DestroyImmediate(ui.checks[i]);ui.checks[i]=CriarCheck("Check"+i,obj.transform,P(obj,new Vector2(765+i*575,330))).gameObject;ui.checks[i].SetActive(false);}
  var coin=hud.Find("HUDCoins/NexaCoinOficial");if(coin!=null)UnityEngine.Object.DestroyImmediate(coin.gameObject);CriarCoinOficial(hud.Find("HUDCoins").GetComponent<Image>());
  var fields=new[]{"HUDCoins","HUDProximo","HUDQuest","HUDObjetivos","BotaoSair","LinhaPerigo"};foreach(string name in fields){var image=hud.Find(name).GetComponent<Image>();image.sprite=S(name=="HUDCoins"?"Coins":name=="HUDProximo"?"Proximo":name=="HUDQuest"?"QuestMoldura":name=="HUDObjetivos"?"Objetivos":name=="BotaoSair"?"Sair":"LinhaPerigo");if(name=="HUDObjetivos")image.rectTransform.sizeDelta=new Vector2(800,800*image.sprite.rect.height/image.sprite.rect.width);}
  ui.comemoracao=AssetDatabase.LoadAllAssetsAtPath(Arte+"QuestComemoracao.png").OfType<Sprite>().OrderBy(s=>s.name).ToArray();ui.popupCoin.sprite=S("CoinPopup");jogo.efeitos=AssetDatabase.LoadAllAssetsAtPath(Arte+"Fusao.png").OfType<Sprite>().OrderBy(s=>s.name).ToArray();
  int[] slots={4,6,8};for(int i=0;i<3;i++){var image=obj.transform.Find(new[]{"ObjetivoCupuaçu","ObjetivoArara","ObjetivoOnça"}[i]).GetComponent<Image>();if(image!=null)image.sprite=jogo.evolucoes[slots[i]-1].sprite;}
  foreach(var def in new[]{new[]{"Instrucoes","Comecar","470","1023"},new[]{"Instrucoes","Voltar","900","1023"},new[]{"ObjetivoComChave","Continuar","515","822.5"},new[]{"ObjetivoComChave","Sair","1030","822.5"},new[]{"ObjetivoSemChave","Continuar","650","712.5"},new[]{"ObjetivoSemChave","Sair","1122.5","712.5"},new[]{"GameOver","JogarNovamente","520","822.5"},new[]{"GameOver","Sair","1040","822.5"}}){var art=jogo.transform.Find(def[0]+"/Arte").GetComponent<Image>();var r=art.transform.Find(def[1]).GetComponent<RectTransform>();r.anchoredPosition=P(art,new Vector2(float.Parse(def[2],System.Globalization.CultureInfo.InvariantCulture),float.Parse(def[3],System.Globalization.CultureInfo.InvariantCulture)));}
  var dangerArt=hud.Find("LinhaPerigo").GetComponent<Image>();dangerArt.rectTransform.anchoredPosition=new Vector2(0,240)-P(dangerArt,new Vector2(1086,360));
  var nextArt=hud.Find("HUDProximo").GetComponent<Image>();ui.proximo.rectTransform.anchoredPosition=P(nextArt,new Vector2(880,530));
  var coinArt=hud.Find("HUDCoins").GetComponent<Image>();ui.coinsRodada.rectTransform.anchoredPosition=P(coinArt,new Vector2(1300,360));
  var overArt=ui.gameOver.transform.Find("Arte").GetComponent<Image>();ui.coinsFinal.rectTransform.anchoredPosition=P(overArt,new Vector2(645,565));ui.faixaRodadaEncerrada.GetComponent<RectTransform>().anchoredPosition=P(overArt,new Vector2(768,385));
  for(int i=0;i<3;i++){var image=obj.transform.Find(new[]{"ObjetivoCupuaçu","ObjetivoArara","ObjetivoOnça"}[i]).GetComponent<Image>();image.rectTransform.anchoredPosition=P(obj,new Vector2(490+i*570,350));ui.checks[i].GetComponent<RectTransform>().anchoredPosition=P(obj,new Vector2(765+i*575,330));}
  EditorUtility.SetDirty(jogo);EditorUtility.SetDirty(ui);EditorSceneManager.MarkSceneDirty(cena);AssetDatabase.SaveAssets();EditorSceneManager.SaveScene(cena);File.WriteAllText(Docs+"ajuste.txt",DateTime.Now.ToString("s"));
 }
 public static void Montar(){
  var scene=SceneManager.GetActiveScene();if(scene.path!="Assets/Projeto/Scenes/Brasil.unity"||scene.isDirty)throw new InvalidOperationException("Abra e salve Brasil fora do Play Mode.");
  if(UnityEngine.Object.FindObjectOfType<MinijogoAmazoniaController>(true)!=null)throw new InvalidOperationException("Minijogo já existe.");
  Importar();var desafios=UnityEngine.Object.FindObjectOfType<PainelDesafiosAmazonia>(true);fonte=desafios.mensagem.font;
  var root=R("MinijogoAmazoniaRoot",null,Vector2.zero,Vector2.zero);var jogo=root.gameObject.AddComponent<MinijogoAmazoniaController>();jogo.mundo=desafios.mundo;
  var canvas=root.gameObject.AddComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=100;var scaler=root.gameObject.AddComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1920,1080);scaler.matchWidthOrHeight=.5f;root.gameObject.AddComponent<GraphicRaycaster>();
  var ui=root.gameObject.AddComponent<AmazoniaMinigameUI>();jogo.interfaceJogo=ui;ui.jogo=jogo;ui.avisoEnergia=desafios.avisoEnergia;ui.hudsGlobais=desafios.quiz.interfaceQuiz.hudsCompartilhadas.Select(t=>t.gameObject).ToArray();
  jogo.evolucoes=new EvolucaoAmazonia[8];string[] nomes={"Açaí","Guaraná","Castanha","Cupuaçu","Sapo","Arara","Boto","Onça"};float[] diam={.8f,.92f,1.056f,1.216f,1.392f,1.6f,1.84f,2.08f};float[] corpo={1,.82f,.95f,.9f,1,.91f,.85f,.93f};
  for(int i=0;i<8;i++)jogo.evolucoes[i]=new EvolucaoAmazonia{nome=nomes[i],sprite=S("Nivel"+(i+1)),diametro=diam[i],corpoVisual=corpo[i]};
  jogo.cenario=S("Cenario");jogo.efeitos=AssetDatabase.LoadAllAssetsAtPath(Arte+"Fusao.png").OfType<Sprite>().OrderBy(s=>s.name).ToArray();
  var material=new PhysicsMaterial2D("AmazoniaFruitMerge"){friction=.45f,bounciness=.04f};AssetDatabase.CreateAsset(material,Arte+"AmazoniaFruitMerge.physicsMaterial2D");jogo.materialFisico=material;
  var instructions=Painel("Instrucoes",root,"Instrucoes",870);ui.instrucoes=instructions.transform.parent.gameObject;B("Comecar",instructions,new Rect(260,953,420,140),jogo.Comecar);B("Voltar",instructions,new Rect(750,953,300,140),jogo.Sair);
  var hud=Tela("HUD",root);ui.hud=hud.gameObject;
  var frame=I("HUDQuest",hud,S("QuestMoldura"),185,new Vector2(-820,380));ui.quest=I("QuestPortrait",frame.transform,AssetDatabase.LoadAllAssetsAtPath(EnergyHUDSetup.Art+"QuestExpressaoHUD.png").OfType<Sprite>().First(),90,new Vector2(0,-2));ui.quest.useSpriteMesh=true;
  ui.idle=AssetDatabase.LoadAllAssetsAtPath(EnergyHUDSetup.Art+"QuestExpressaoHUD.png").OfType<Sprite>().OrderBy(s=>s.name).ToArray();ui.comemoracao=AssetDatabase.LoadAllAssetsAtPath(Arte+"QuestComemoracao.png").OfType<Sprite>().OrderBy(s=>s.name).ToArray();
  var next=I("HUDProximo",hud,S("Proximo"),245,new Vector2(820,360));ui.proximo=I("NextItemImage",next.transform,jogo.evolucoes[0].sprite,60,P(next,new Vector2(880,530)));
  var coins=I("HUDCoins",hud,S("Coins"),255,new Vector2(-820,220));ui.coinsRodada=T("CoinsRodadaText",coins.transform,new Vector2(125,45),P(coins,new Vector2(1300,360)),30);
  CriarCoinOficial(coins);
  // Usa a mesma arte oficial da moeda do HUD global.
  var exit=I("BotaoSair",hud,S("Sair"),215,new Vector2(820,140));B("Click",exit,new Rect(0,0,1987,791),jogo.Sair);
  var line=I("LinhaPerigo",hud,S("LinhaPerigo"),1180,new Vector2(0,240));line.rectTransform.anchoredPosition-=P(line,new Vector2(1086,360));
  var objectives=I("HUDObjetivos",hud,S("Objetivos"),800,new Vector2(0,-420));ui.checks=new GameObject[3];int[] slots={4,6,8};
  for(int n=0;n<3;n++){I("Objetivo"+nomes[slots[n]-1],objectives.transform,jogo.evolucoes[slots[n]-1].sprite,90,P(objectives,new Vector2(490+n*570,350)));var check=CriarCheck("Check"+n,objectives.transform,P(objectives,new Vector2(765+n*575,330)));ui.checks[n]=check.gameObject;}
  ui.popupCoin=I("PopupCoin",hud,S("CoinPopup"),140,Vector2.zero);
  var com=Painel("ObjetivoComChave",root,"ObjetivoComChave",1120);ui.painelComChave=com.transform.parent.gameObject;B("Continuar",com,new Rect(305,750,420,145),jogo.Continuar);B("Sair",com,new Rect(820,750,420,145),jogo.Sair);
  var sem=Painel("ObjetivoSemChave",root,"ObjetivoSemChave",1340);ui.painelSemChave=sem.transform.parent.gameObject;B("Continuar",sem,new Rect(440,650,420,125),jogo.Continuar);B("Sair",sem,new Rect(930,650,385,125),jogo.Sair);
  var over=Painel("GameOver",root,"GameOver",1180);ui.gameOver=over.transform.parent.gameObject;B("JogarNovamente",over,new Rect(290,750,460,145),jogo.Comecar);B("Sair",over,new Rect(830,750,420,145),jogo.Sair);ui.coinsFinal=T("CoinsDaRodada",over.transform,new Vector2(190,65),P(over,new Vector2(645,565)),38);
  var faixa=R("RodadaEncerrada",over.transform,new Vector2(770,80),P(over,new Vector2(768,385)));faixa.gameObject.AddComponent<Image>().color=new Color(.35f,.08f,.015f);T("Texto",faixa,faixa.sizeDelta,Vector2.zero,38).text="RODADA ENCERRADA";ui.faixaRodadaEncerrada=faixa.gameObject;
  desafios.minijogo=jogo;EditorUtility.SetDirty(desafios);root.gameObject.SetActive(false);AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);Directory.CreateDirectory(Docs);File.WriteAllText(Docs+"montagem.txt","Minijogo montado e Brasil salva. "+DateTime.Now.ToString("s"));
 }
}

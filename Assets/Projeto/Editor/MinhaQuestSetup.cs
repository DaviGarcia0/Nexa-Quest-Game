using System;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEditor.U2D.Sprites;
using UnityEngine;
using UnityEngine.UI;
using NexaQuest;
using NexaQuest.MinhaQuest;
public static class MinhaQuestSetup
{
    public const string Art = "Assets/Projeto/Sprites/MinhaQuest20261027/";
    public const string Anim = "Assets/Projeto/Animations/MinhaQuest/";
    static TMP_FontAsset font;
    static Sprite Sprite(string name) => AssetDatabase.LoadAssetAtPath<Sprite>(Art + name + ".png");
    static RectTransform Rect(string name, Transform parent, Vector2 size, Vector2 pos)
    {
        var r = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
        r.gameObject.layer = 5; r.SetParent(parent, false); r.anchorMin = r.anchorMax = r.pivot = new Vector2(.5f,.5f);
        r.sizeDelta = size; r.anchoredPosition = pos; return r;
    }
    static Image Image(string name, Transform parent, Sprite sprite, float width, Vector2 pos)
    {
        var r = Rect(name, parent, new Vector2(width, width * sprite.rect.height / sprite.rect.width), pos);
        var image = r.gameObject.AddComponent<Image>(); image.sprite = sprite; image.preserveAspect = true; image.raycastTarget = false; return image;
    }
    static RectTransform Pixels(Transform parent, string name, Vector2 size, Rect px)
    {
        var r = (RectTransform)parent;
        return Rect(name, parent, new Vector2(px.width / size.x * r.sizeDelta.x, px.height / size.y * r.sizeDelta.y),
            new Vector2((px.center.x / size.x - .5f) * r.sizeDelta.x, (.5f - px.center.y / size.y) * r.sizeDelta.y));
    }
    static Button Button(RectTransform r, UnityEngine.Events.UnityAction action, bool transparent)
    {
        var image = r.GetComponent<Image>() ?? r.gameObject.AddComponent<Image>();
        if (transparent) image.color = Color.clear;
        image.raycastTarget = true;
        var b = r.gameObject.AddComponent<Button>(); b.targetGraphic = image; b.transition = Selectable.Transition.None; b.navigation = new Navigation { mode = Navigation.Mode.None };
        var sound = r.gameObject.AddComponent<UIButtonAudio>(); UnityEventTools.AddPersistentListener(b.onClick, sound.Play);
        UnityEventTools.AddPersistentListener(b.onClick, action); return b;
    }
    static Button Hotspot(Image art, string name, Rect px, UnityEngine.Events.UnityAction action)
        => Button(Pixels(art.transform, name, art.sprite.rect.size, px), action, true);
    static TMP_Text Label(string name, Transform parent, Vector2 size, Vector2 pos, int fs)
    {
        var t = Rect(name, parent, size, pos).gameObject.AddComponent<TextMeshProUGUI>();
        t.font = font; t.fontSize = fs; t.fontStyle = FontStyles.Bold; t.alignment = TextAlignmentOptions.Center; t.color = new Color(1,.94f,.8f); t.raycastTarget = false; return t;
    }
    static GameObject Layer(string name, Transform parent)
    {
        var r = Rect(name, parent, new Vector2(1920,1080), Vector2.zero);
        var shade = r.gameObject.AddComponent<Image>(); shade.color = new Color(0,0,0,.55f); shade.raycastTarget = true; return r.gameObject;
    }
    static void Import(string name, bool multiple)
    {
        var importer = (TextureImporter)AssetImporter.GetAtPath(Art + name + ".png");
        importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = multiple ? SpriteImportMode.Multiple : SpriteImportMode.Single;
        importer.filterMode = FilterMode.Point; importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.mipmapEnabled = false; importer.npotScale = TextureImporterNPOTScale.None; importer.maxTextureSize = 4096;
        importer.alphaSource = TextureImporterAlphaSource.FromInput; importer.alphaIsTransparency = true;
        var settings = new TextureImporterSettings(); importer.ReadTextureSettings(settings);
        settings.spriteMeshType = SpriteMeshType.FullRect; settings.spriteGenerateFallbackPhysicsShape = false; importer.SetTextureSettings(settings);
        importer.SaveAndReimport();
    }
    static Sprite[] Sheet(string name, int cols, int rows, int top, int bottom)
    {
        Import(name, true); var importer = (TextureImporter)AssetImporter.GetAtPath(Art + name + ".png");
        importer.GetSourceTextureWidthAndHeight(out int w, out int h);
        var factory = new SpriteDataProviderFactories(); factory.Init();
        var provider = factory.GetSpriteEditorDataProviderFromObject(importer); provider.InitSpriteEditorDataProvider();
        var old = provider.GetSpriteRects().ToDictionary(s=>s.name,s=>s.spriteID);
        var sprites = new SpriteRect[cols * rows];
        for (int row=0;row<rows;row++) for(int col=0;col<cols;col++)
        {
            int index = row*cols+col; string id = name + "_" + index.ToString("00");
            int left = Mathf.RoundToInt(col*w/(float)cols), right = Mathf.RoundToInt((col+1)*w/(float)cols);
            int y1 = rows==1 ? top : Mathf.RoundToInt(row*h/(float)rows);
            int y2 = rows==1 ? bottom : Mathf.RoundToInt((row+1)*h/(float)rows);
            sprites[index] = new SpriteRect { name=id, rect=new Rect(left,h-y2,right-left,y2-y1), pivot=new Vector2(.5f,.5f), alignment=SpriteAlignment.Center, spriteID=old.ContainsKey(id)?old[id]:GUID.Generate() };
        }
        provider.SetSpriteRects(sprites); provider.GetDataProvider<ISpriteNameFileIdDataProvider>().SetNameFileIdPairs(sprites.Select(s=>new SpriteNameFileIdPair(s.name,s.spriteID)));
        provider.Apply(); AssetDatabase.ImportAsset(Art+name+".png", ImportAssetOptions.ForceUpdate);
        return AssetDatabase.LoadAllAssetsAtPath(Art+name+".png").OfType<Sprite>().OrderBy(s=>s.name).ToArray();
    }
    static AnimationClip Clip(string name, Sprite[] frames, float fps, bool loop)
    {
        var clip = new AnimationClip { name=name, frameRate=fps };
        var keys = new ObjectReferenceKeyframe[frames.Length+1];
        for(int i=0;i<frames.Length;i++) keys[i] = new ObjectReferenceKeyframe { time=i/fps, value=frames[i] };
        keys[frames.Length] = new ObjectReferenceKeyframe { time=frames.Length/fps, value=loop?frames[0]:frames[frames.Length-1] };
        AnimationUtility.SetObjectReferenceCurve(clip, EditorCurveBinding.PPtrCurve("",typeof(Image),"m_Sprite"), keys);
        var settings = AnimationUtility.GetAnimationClipSettings(clip); settings.loopTime = loop; AnimationUtility.SetAnimationClipSettings(clip,settings);
        AssetDatabase.CreateAsset(clip,Anim+name+".anim"); return clip;
    }
    static AnimatorController Controller(string name, AnimationClip[] clips, string[] states)
    {
        var controller = AnimatorController.CreateAnimatorControllerAtPath(Anim+name+".controller");
        var machine = controller.layers[0].stateMachine;
        for(int i=0;i<clips.Length;i++) { var state=machine.AddState(states[i]); state.motion=clips[i]; if(i==0)machine.defaultState=state; }
        return controller;
    }
    public static void Adjust()
    {
        var previous=EditorSceneManager.GetSceneManagerSetup();
        try {
            var scene=EditorSceneManager.OpenScene("Assets/Projeto/Scenes/MinhaQuest.unity");var c=UnityEngine.Object.FindObjectOfType<MinhaQuestController>();
            var fill=c.energyFill;var track=Rect("EnergyTrack",fill.transform.parent,fill.rectTransform.sizeDelta,fill.rectTransform.anchoredPosition);
            fill.rectTransform.SetParent(track,false);fill.rectTransform.anchorMin=Vector2.zero;fill.rectTransform.anchorMax=Vector2.one;fill.rectTransform.offsetMin=fill.rectTransform.offsetMax=Vector2.zero;
            fill.sprite=null;fill.type=UnityEngine.UI.Image.Type.Simple;c.feeding.mouth.anchoredPosition=new Vector2(53,-85);
            EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
        } finally {EditorSceneManager.RestoreSceneManagerSetup(previous);}
    }
    public static void Run()
    {
        var setup = EditorSceneManager.GetSceneManagerSetup();
        if (setup.Any(s=>UnityEngine.SceneManagement.SceneManager.GetSceneByPath(s.path).isDirty)) throw new InvalidOperationException("Salve a cena antes da montagem.");
        if (AssetDatabase.LoadAssetAtPath<AnimatorController>(Anim+"Torso.controller")!=null) throw new InvalidOperationException("Minha Quest já montada.");
        Directory.CreateDirectory(Anim);
        font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
        foreach(string name in new[]{"Cenario","Mercado","Apple","Sandwich","NexaKing","Voltar","MercadoBotao","VestuarioBotao","Loja","Energia","Vestuario","EnergiaCheia","SaldoInsuficiente","CompraRealizada","LojaIndisponivel"}) Import(name,false);
        var idle = Sheet("TorsoIdle",6,1,115,605); var open = Sheet("Boca",6,1,115,605); var eat = Sheet("Comendo",8,2,0,0); var body = Sheet("CorpoIdle",4,1,50,855);
        var eatingClip = Clip("Eating",eat,10,false);
        var torso = Controller("Torso",new[]{Clip("TorsoIdle",idle,7,true),Clip("MouthOpen",open,10,false),eatingClip},new[]{"Idle","MouthOpen","Eating"});
        var fullBody = Controller("Corpo",new[]{Clip("CorpoIdle",body,5,true)},new[]{"Idle"});
        try
        {
            var scene = EditorSceneManager.OpenScene("Assets/Projeto/Scenes/MinhaQuest.unity");
            var root = Rect("MinhaQuestCanvas",null,new Vector2(1920,1080),Vector2.zero);
            var canvas=root.gameObject.AddComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;
            var scaler=root.gameObject.AddComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1920,1080);scaler.matchWidthOrHeight=.5f;root.gameObject.AddComponent<GraphicRaycaster>();
            var ctrl=root.gameObject.AddComponent<MinhaQuestController>();
            var main=Rect("TelaPrincipal",root,new Vector2(1920,1080),Vector2.zero);ctrl.mainControls=main.gameObject.AddComponent<CanvasGroup>();
            var art=Image("Cenario",main,Sprite("Cenario"),1920,Vector2.zero);
            var questRect=Pixels(art.transform,"QuestTorso",art.sprite.rect.size,new Rect(705,178,260,345));
            var quest=questRect.gameObject.AddComponent<Image>();quest.sprite=idle[0];quest.preserveAspect=true;quest.raycastTarget=false;
            var animator=questRect.gameObject.AddComponent<Animator>();animator.runtimeAnimatorController=torso;animator.cullingMode=AnimatorCullingMode.AlwaysAnimate;
            ctrl.feeding=questRect.gameObject.AddComponent<QuestFeedingController>();ctrl.feeding.animator=animator;ctrl.feeding.eatingDuration=eatingClip.length;
            ctrl.feeding.mouth=Rect("MouthZone",questRect,new Vector2(98,70),new Vector2(53,-85));
            var foodTypes=new[]{FoodType.Apple,FoodType.Sandwich,FoodType.NexaKing};var foodNames=new[]{"Apple","Sandwich","NexaKing"};var xs=new[]{515,830,1150};
            ctrl.foods=new FoodDragHandler[3];
            for(int i=0;i<3;i++)
            {
                var slot=Pixels(art.transform,"Slot"+foodNames[i],art.sprite.rect.size,new Rect(xs[i]-60,540,120,85));
                var icon=Image(foodNames[i],slot,Sprite(foodNames[i]),slot.sizeDelta.x,Vector2.zero);icon.raycastTarget=true;
                var handler=icon.gameObject.AddComponent<FoodDragHandler>();handler.controller=ctrl;handler.type=foodTypes[i];ctrl.foods[i]=handler;
            }
            ctrl.dragLayer=Rect("DragLayer",root,new Vector2(1920,1080),Vector2.zero);
            Button(Image("ButtonMercado",main,Sprite("MercadoBotao"),340,new Vector2(-770,220)).rectTransform,ctrl.OpenMarket,false);
            Button(Image("ButtonVestuario",main,Sprite("VestuarioBotao"),340,new Vector2(-770,95)).rectTransform,ctrl.OpenWardrobe,false);
            Button(Image("ButtonLoja",main,Sprite("Loja"),340,new Vector2(-770,-30)).rectTransform,ctrl.OpenShop,false);
            Button(Image("ButtonVoltar",main,Sprite("Voltar"),300,new Vector2(-780,-470)).rectTransform,ctrl.Back,false);
            var energy=Image("EnergyHUD",main,Sprite("Energia"),820,new Vector2(130,-440));
            var track=Pixels(energy.transform,"EnergyTrack",energy.sprite.rect.size,new Rect(155,330,1450,112));
            var fillRect=Rect("EnergyFill",track,Vector2.zero,Vector2.zero);fillRect.anchorMin=Vector2.zero;fillRect.anchorMax=Vector2.one;fillRect.offsetMin=fillRect.offsetMax=Vector2.zero;
            var fill=fillRect.gameObject.AddComponent<Image>();fill.color=new Color(1,.65f,.05f);fill.raycastTarget=false;ctrl.energyFill=fill;
            var value=Pixels(energy.transform,"EnergyValue",energy.sprite.rect.size,new Rect(1675,325,365,130));ctrl.energyText=Label("Value",value,value.sizeDelta,Vector2.zero,24);
            ctrl.market=Layer("MarketLayer",root);ctrl.marketControls=ctrl.market.AddComponent<CanvasGroup>();
            var market=Image("Mercado",ctrl.market.transform,Sprite("Mercado"),1600,Vector2.zero);
            Hotspot(market,"ButtonApple",new Rect(430,315,255,275),ctrl.BuyApple);Hotspot(market,"ButtonSandwich",new Rect(704,315,255,275),ctrl.BuySandwich);Hotspot(market,"ButtonNexaKing",new Rect(980,315,255,275),ctrl.BuyNexaKing);
            Hotspot(market,"ButtonCloseMarket",new Rect(1418,38,90,87),ctrl.CloseMarket);
            ctrl.balanceText=Label("NexaCoinsBalance",market.transform,new Vector2(500,65),new Vector2(0,-340),32);
            ctrl.wardrobe=Layer("WardrobeLayer",root);
            var wardrobe=Image("Vestuario",ctrl.wardrobe.transform,Sprite("Vestuario"),1600,Vector2.zero);
            var bodyRect=Pixels(wardrobe.transform,"QuestCorpo",wardrobe.sprite.rect.size,new Rect(330,328,265,480));
            var bodyImage=bodyRect.gameObject.AddComponent<Image>();bodyImage.sprite=body[0];bodyImage.preserveAspect=true;bodyImage.raycastTarget=false;
            var bodyAnimator=bodyRect.gameObject.AddComponent<Animator>();bodyAnimator.runtimeAnimatorController=fullBody;bodyAnimator.cullingMode=AnimatorCullingMode.AlwaysAnimate;
            Hotspot(wardrobe,"ButtonCloseWardrobe",new Rect(1480,59,115,105),ctrl.CloseWardrobe);
            ctrl.popupLayer=Layer("PopupLayer",root);ctrl.popupArt=Image("Popup",ctrl.popupLayer.transform,Sprite("CompraRealizada"),1120,Vector2.zero);
            ctrl.popupOk=Hotspot(ctrl.popupArt,"ButtonOK",new Rect(515,710,500,165),ctrl.ClosePopup).GetComponent<RectTransform>();
            ctrl.popupX=Hotspot(ctrl.popupArt,"ButtonX",new Rect(1255,145,190,170),ctrl.ClosePopup).GetComponent<RectTransform>();
            ctrl.purchaseArt=Sprite("CompraRealizada");ctrl.insufficientArt=Sprite("SaldoInsuficiente");ctrl.fullArt=Sprite("EnergiaCheia");ctrl.unavailableArt=Sprite("LojaIndisponivel");
            ctrl.market.SetActive(false);ctrl.wardrobe.SetActive(false);ctrl.popupLayer.SetActive(false);
            EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
            File.WriteAllText(MinhaQuestBridge.Docs+"montagem.txt","MinhaQuest montada. Animações: 6 idle, 6 boca, 16 comendo, 4 corpo.\n"+DateTime.Now.ToString("s"));
        }
        finally { EditorSceneManager.RestoreSceneManagerSetup(setup); }
    }
}

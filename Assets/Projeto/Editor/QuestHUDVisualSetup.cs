using System;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.U2D.Sprites;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using NexaQuest.Brasil;

// Integra somente os assets visuais de outubro; nao remonta a cena Brasil.
public static class QuestHUDVisualSetup
{
    public const string Docs = "Documentation/Brasil/EtapaVisual20261001/";
    public const string Art = "Assets/Projeto/Sprites/Quest/Design20261001/";
    const string HUDArt = "Assets/Projeto/Sprites/HUD/";
    [Serializable] class Sheet { public float pixelsPerUnit; public Frame[] frames; }
    [Serializable] class Frame { public string name; public int x,y,width,height; public float pivotX,pivotY; }

    static TextureImporter Configure(string path, bool multiple, float ppu)
    {
        var i = (TextureImporter)AssetImporter.GetAtPath(path);
        if (i == null) throw new InvalidOperationException("Asset ausente: " + path);
        i.textureType = TextureImporterType.Sprite;
        i.spriteImportMode = multiple ? SpriteImportMode.Multiple : SpriteImportMode.Single;
        i.spritePixelsPerUnit = ppu;
        i.filterMode = FilterMode.Point; i.mipmapEnabled = false;
        i.textureCompression = TextureImporterCompression.Uncompressed;
        i.npotScale = TextureImporterNPOTScale.None; i.maxTextureSize = 4096;
        i.alphaSource = TextureImporterAlphaSource.FromInput; i.alphaIsTransparency = true;
        var settings = new TextureImporterSettings(); i.ReadTextureSettings(settings);
        settings.spriteMeshType = SpriteMeshType.FullRect; settings.spriteGenerateFallbackPhysicsShape = false;
        i.SetTextureSettings(settings);
        var platform = i.GetDefaultPlatformTextureSettings(); platform.maxTextureSize = 4096;
        platform.textureCompression = TextureImporterCompression.Uncompressed; i.SetPlatformTextureSettings(platform);
        i.SaveAndReimport(); return i;
    }
    static Sprite[] ImportSheet(string name)
    {
        string path = Art + name + ".png";
        var data = JsonUtility.FromJson<Sheet>(File.ReadAllText(Art + name + ".json"));
        var importer = Configure(path, true, data.pixelsPerUnit);
        var factory = new SpriteDataProviderFactories(); factory.Init();
        var provider = factory.GetSpriteEditorDataProviderFromObject(importer); provider.InitSpriteEditorDataProvider();
        var previous = provider.GetSpriteRects().ToDictionary(r => r.name, r => r.spriteID);
        var rects = data.frames.Select(f => new SpriteRect {
            name=f.name, rect=new Rect(f.x,f.y,f.width,f.height), pivot=new Vector2(f.pivotX,f.pivotY),
            alignment=SpriteAlignment.Custom, spriteID=previous.ContainsKey(f.name)?previous[f.name]:GUID.Generate()
        }).ToArray();
        provider.SetSpriteRects(rects);
        provider.GetDataProvider<ISpriteNameFileIdDataProvider>().SetNameFileIdPairs(rects.Select(r=>new SpriteNameFileIdPair(r.name,r.spriteID)));
        provider.Apply(); AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceUpdate);
        return AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().ToArray();
    }
    static RectTransform Rect(Transform parent, string name, Vector2 position, Vector2 size)
    {
        var r = new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();
        Place(r,parent,position,size); return r;
    }
    static void Place(RectTransform r, Transform parent, Vector2 position, Vector2 size)
    {
        r.SetParent(parent,false); r.anchorMin=r.anchorMax=new Vector2(0,1);r.pivot=new Vector2(0,1);
        r.anchoredPosition=position;r.sizeDelta=size;r.localScale=Vector3.one;
    }
    static void Label(TMP_Text t, Transform parent, Vector2 position, Vector2 size, float fontSize)
    {
        Place(t.rectTransform,parent,position,size); t.fontSize=fontSize;t.fontStyle=FontStyles.Bold;
        t.enableAutoSizing=true;t.fontSizeMin=14;t.fontSizeMax=fontSize;t.alignment=TextAlignmentOptions.Center;
        t.enableWordWrapping=false;t.raycastTarget=false;t.color=new Color32(255,235,180,255);
    }
    static void Background(RectTransform parent, Sprite sprite)
    {
        var r=Rect(parent,"Background",Vector2.zero,parent.sizeDelta);
        var i=r.gameObject.AddComponent<Image>();i.sprite=sprite;i.preserveAspect=true;i.raycastTarget=false;
    }
    [MenuItem("Nexa Quest/Integrar nova Quest e HUD")]
    public static void Run()
    {
        var scene=SceneManager.GetActiveScene();
        if(EditorApplication.isPlayingOrWillChangePlaymode||scene.path!="Assets/Projeto/Scenes/Brasil.unity"||scene.isDirty)
            throw new InvalidOperationException("Abra Brasil salva e fora do Play Mode.");
        var hud=UnityEngine.Object.FindObjectOfType<ProgressionHUD>(true);
        if(hud.transform.Find("CoinsHUD")!=null) throw new InvalidOperationException("Nova HUD ja integrada; nao sobrescrever ajustes manuais.");
        Directory.CreateDirectory(Docs);
        string collisions=BrasilPortalSetup.CollisionSnapshot();File.WriteAllText(Docs+"colisoes-antes.txt",collisions);
        var world=UnityEngine.Object.FindObjectOfType<BrazilWorld>();
        string movement=EditorJsonUtility.ToJson(world.player);
        string body=EditorJsonUtility.ToJson(world.player.GetComponent<Rigidbody2D>());
        string collider=EditorJsonUtility.ToJson(world.player.GetComponent<Collider2D>());
        var controller=world.player.GetComponent<Animator>().runtimeAnimatorController;
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        var sprites=ImportSheet("Quest_Walk").Concat(ImportSheet("Quest_Idle")).ToArray();
        // Preserva os mesmos clips, GUIDs, bindings e tempos; substitui apenas referencias aos frames.
        foreach(var clip in controller.animationClips.Distinct())
        {
            var frames=sprites.Where(s=>s.name.StartsWith(clip.name+"_")).OrderBy(s=>s.name).ToArray();
            if(frames.Length!=(clip.name.StartsWith("Walk")?8:4)) throw new InvalidOperationException("Frames inesperados: "+clip.name);
            var binding=AnimationUtility.GetObjectReferenceCurveBindings(clip).Single(b=>b.type==typeof(SpriteRenderer)&&b.propertyName=="m_Sprite");
            var keys=AnimationUtility.GetObjectReferenceCurve(clip,binding);
            if(keys.Length!=frames.Length+1) throw new InvalidOperationException("Curva inesperada: "+clip.name);
            for(int k=0;k<keys.Length;k++)keys[k].value=frames[k%frames.Length];
            AnimationUtility.SetObjectReferenceCurve(clip,binding,keys);EditorUtility.SetDirty(clip);
        }
        var idle=sprites.Single(s=>s.name=="IdleDown_01");
        string prefabPath=PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(world.player.gameObject);
        if(string.IsNullOrEmpty(prefabPath))throw new InvalidOperationException("Prefab da Quest nao encontrado.");
        var prefab=PrefabUtility.LoadPrefabContents(prefabPath);
        try {prefab.GetComponent<SpriteRenderer>().sprite=idle;PrefabUtility.SaveAsPrefabAsset(prefab,prefabPath);}
        finally {PrefabUtility.UnloadPrefabContents(prefab);}
        var renderer=world.player.GetComponent<SpriteRenderer>();renderer.sprite=idle;
        PrefabUtility.RecordPrefabInstancePropertyModifications(renderer);
        Configure(HUDArt+"HUD_NEXACOIN.png",false,100);Configure(HUDArt+"HUD_CEREBRO.png",false,100);
        var root=(RectTransform)hud.transform;root.anchorMin=root.anchorMax=root.pivot=Vector2.one;
        root.anchoredPosition=new Vector2(-24,-14);root.sizeDelta=new Vector2(500,350);
        hud.GetComponent<Image>().enabled=false;
        var coins=Rect(root,"CoinsHUD",Vector2.zero,new Vector2(500,500f/3f));
        Background(coins,AssetDatabase.LoadAssetAtPath<Sprite>(HUDArt+"HUD_NEXACOIN.png"));
        Label(hud.coins,coins,new Vector2(190,-64),new Vector2(250,45),32);hud.coins.name="CoinsText";hud.coins.text="0";
        var knowledge=Rect(root,"KnowledgeHUD",new Vector2(0,-150),new Vector2(500,500f*793/1983));
        Background(knowledge,AssetDatabase.LoadAssetAtPath<Sprite>(HUDArt+"HUD_CEREBRO.png"));
        Label(hud.knowledge,knowledge,new Vector2(177,-66),new Vector2(244,31),24);hud.knowledge.name="KnowledgeLabel";hud.knowledge.text="CONHECIMENTO";
        Label(hud.brainLevel,knowledge,new Vector2(87,-133),new Vector2(50,24),23);hud.brainLevel.name="LevelText";hud.brainLevel.text="1";
        var bar=(RectTransform)hud.knowledgeFill.transform.parent;
        Place(bar,knowledge,new Vector2(178,-100),new Vector2(241,12));bar.name="XPBar";
        bar.GetComponent<Image>().color=new Color32(45,34,68,255);
        Place(hud.knowledgeFill.rectTransform,bar,Vector2.zero,bar.sizeDelta);
        hud.knowledgeFill.color=new Color32(239,183,255,255);
        hud.knowledgeFill.type=Image.Type.Filled;hud.knowledgeFill.fillMethod=Image.FillMethod.Horizontal;
        hud.knowledgeFill.fillOrigin=(int)Image.OriginHorizontal.Left;hud.knowledgeFill.fillAmount=0;
        if(movement!=EditorJsonUtility.ToJson(world.player)||body!=EditorJsonUtility.ToJson(world.player.GetComponent<Rigidbody2D>())||collider!=EditorJsonUtility.ToJson(world.player.GetComponent<Collider2D>()))
            throw new InvalidOperationException("Player mudou alem do visual; cancelar salvamento.");
        string after=BrasilPortalSetup.CollisionSnapshot();File.WriteAllText(Docs+"colisoes-depois.txt",after);
        if(collisions!=after)throw new InvalidOperationException("Colisoes divergentes; cancelar salvamento.");
        EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
        File.WriteAllText(Docs+"integracao.txt","OK: Quest, 8 clips e HUD atualizados. Animator, movimento, Rigidbody e colliders preservados.\n"+DateTime.Now.ToString("s"));
    }
}


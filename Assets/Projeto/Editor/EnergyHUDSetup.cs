using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEditor.U2D.Sprites;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using NexaQuest.Brasil;

public static class EnergyHUDSetup
{
    public const string Docs="Documentation/Brasil/EtapaEnergia20261002/";
    public const string Art="Assets/Projeto/Sprites/HUD/QuestExpressions/";
    public const string Anim="Assets/Projeto/Animations/HUD/";
    [Serializable] class Sheet { public float pixelsPerUnit; public Frame[] frames; }
    [Serializable] class Frame { public string name; public int x,y,width,height; public float pivotX,pivotY; public Vector2[] outline; }
    static TextureImporter Configure(string path,bool multiple=false)
    {
        var i=(TextureImporter)AssetImporter.GetAtPath(path);
        if(i==null)throw new InvalidOperationException("Asset ausente: "+path);
        i.textureType=TextureImporterType.Sprite;i.spriteImportMode=multiple?SpriteImportMode.Multiple:SpriteImportMode.Single;
        i.spritePixelsPerUnit=100;i.filterMode=FilterMode.Point;i.mipmapEnabled=false;
        i.textureCompression=TextureImporterCompression.Uncompressed;i.maxTextureSize=4096;i.npotScale=TextureImporterNPOTScale.None;
        i.alphaSource=TextureImporterAlphaSource.FromInput;i.alphaIsTransparency=true;
        var settings=new TextureImporterSettings();i.ReadTextureSettings(settings);
        settings.spriteMeshType=multiple?SpriteMeshType.Tight:SpriteMeshType.FullRect;settings.spriteGenerateFallbackPhysicsShape=false;i.SetTextureSettings(settings);
        var platform=i.GetDefaultPlatformTextureSettings();platform.maxTextureSize=4096;platform.textureCompression=TextureImporterCompression.Uncompressed;i.SetPlatformTextureSettings(platform);
        i.SaveAndReimport();return i;
    }
    static Sprite[] ImportIdle()
    {
        string path=Art+"QuestExpressaoHUD.png";
        var data=JsonUtility.FromJson<Sheet>(File.ReadAllText(Art+"QuestExpressaoHUD.json"));
        if(data.frames.Length!=6)throw new InvalidOperationException("Idle deve ter os seis frames inspecionados.");
        var importer=Configure(path,true);var factory=new SpriteDataProviderFactories();factory.Init();
        var provider=factory.GetSpriteEditorDataProviderFromObject(importer);provider.InitSpriteEditorDataProvider();
        var rects=data.frames.Select(f=>new SpriteRect{name=f.name,rect=new Rect(f.x,f.y,f.width,f.height),pivot=new Vector2(f.pivotX,f.pivotY),alignment=SpriteAlignment.Custom,spriteID=GUID.Generate()}).ToArray();
        provider.SetSpriteRects(rects);
        provider.GetDataProvider<ISpriteNameFileIdDataProvider>().SetNameFileIdPairs(rects.Select(r=>new SpriteNameFileIdPair(r.name,r.spriteID)));
        var outlineProvider=provider.GetDataProvider<ISpriteOutlineDataProvider>();
        // Contornos atravessam somente os espacos vazios entre os frames que se sobrepoem horizontalmente.
        // O PNG original permanece intacto; Image.useSpriteMesh utiliza estes contornos na UI.
        for(int f=0;f<rects.Length;f++)outlineProvider.SetOutlines(rects[f].spriteID,new List<Vector2[]>{data.frames[f].outline});
        provider.Apply();AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceUpdate);
        return AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().OrderBy(s=>s.name).ToArray();
    }
    static RectTransform Rect(Transform parent,string name,Vector2 size,Vector2 pos,Vector2 pivot)
    {
        var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(parent,false);
        r.anchorMin=r.anchorMax=new Vector2(0,1);r.pivot=pivot;r.sizeDelta=size;r.anchoredPosition=pos;return r;
    }
    [MenuItem("Nexa Quest/Preparar HUD de Energia")]
    public static void Run()
    {
        var scene=SceneManager.GetActiveScene();
        if(EditorApplication.isPlayingOrWillChangePlaymode||scene.path!="Assets/Projeto/Scenes/Brasil.unity"||scene.isDirty)throw new InvalidOperationException("Abra e salve Brasil fora de Play Mode.");
        var hud=UnityEngine.Object.FindObjectOfType<ProgressionHUD>(true);var canvas=hud.GetComponentInParent<Canvas>();
        if(canvas.transform.Find("EnergyHUD")!=null)throw new InvalidOperationException("EnergyHUD ja existe; ajustes manuais nao serao sobrescritos.");
        Directory.CreateDirectory(Docs);Directory.CreateDirectory(Anim);
        string collisions=BrasilPortalSetup.CollisionSnapshot();File.WriteAllText(Docs+"colisoes-antes.txt",collisions);
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        var frames=ImportIdle();
        Configure(Art+"ExpressaoQuest.png");Configure(Art+"QuestExpressaoComemoracaoHUD.png");Configure("Assets/Projeto/Sprites/HUD/HUD_ENERGIA.png");
        var portrait=AssetDatabase.LoadAssetAtPath<Sprite>(Art+"ExpressaoQuest.png");int portraitCount=0;
        foreach(string guid in AssetDatabase.FindAssets("t:DialogueData",new[]{"Assets/Projeto/Dialogue"})){
            var data=AssetDatabase.LoadAssetAtPath<DialogueData>(AssetDatabase.GUIDToAssetPath(guid));bool changed=false;
            foreach(var line in data.lines)if(line.speaker==DialogueSpeaker.Quest){line.portrait=portrait;portraitCount++;changed=true;}
            if(changed)EditorUtility.SetDirty(data);
        }
        const float width=540;float scale=width/775f;
        var root=Rect(canvas.transform,"EnergyHUD",new Vector2(width,259*scale),new Vector2(24,-18),new Vector2(0,1));
        root.SetSiblingIndex(hud.transform.GetSiblingIndex()+1);
        var background=Rect(root,"Background",root.sizeDelta,Vector2.zero,new Vector2(0,1)).gameObject.AddComponent<Image>();
        background.sprite=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Projeto/Sprites/HUD/HUD_ENERGIA.png");background.preserveAspect=true;background.raycastTarget=false;
        var maskRect=Rect(root,"PortraitMask",Vector2.one*(144*scale),new Vector2(142*scale,-127*scale),new Vector2(.5f,.5f));
        var maskImage=maskRect.gameObject.AddComponent<Image>();maskImage.sprite=AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");maskImage.raycastTarget=false;
        if(maskImage.sprite==null)throw new InvalidOperationException("Sprite circular nativo ausente.");
        var mask=maskRect.gameObject.AddComponent<Mask>();mask.showMaskGraphic=false;
        var portraitRect=Rect(maskRect,"QuestPortrait",new Vector2(78,78*505f/370f),new Vector2(0,-1.5f),new Vector2(.5f,.5f));
        portraitRect.anchorMin=portraitRect.anchorMax=new Vector2(.5f,.5f);
        var image=portraitRect.gameObject.AddComponent<Image>();image.sprite=frames[0];image.preserveAspect=true;image.useSpriteMesh=true;image.raycastTarget=false;
        var controller=AnimatorController.CreateAnimatorControllerAtPath(Anim+"QuestHUD.controller");
        var clip=new AnimationClip{name="QuestHUD_Idle",frameRate=12};
        float[] times={0,1,1.5f,21f/12,23f/12,29f/12,4};
        var keys=times.Select((t,j)=>new ObjectReferenceKeyframe{time=t,value=frames[j%6]}).ToArray();
        AnimationUtility.SetObjectReferenceCurve(clip,EditorCurveBinding.PPtrCurve("",typeof(Image),"m_Sprite"),keys);
        var settings=AnimationUtility.GetAnimationClipSettings(clip);settings.loopTime=true;AnimationUtility.SetAnimationClipSettings(clip,settings);
        AssetDatabase.CreateAsset(clip,Anim+"QuestHUD_Idle.anim");
        var state=controller.layers[0].stateMachine.AddState("QuestHUD_Idle");state.motion=clip;controller.layers[0].stateMachine.defaultState=state;
        var animator=portraitRect.gameObject.AddComponent<Animator>();animator.runtimeAnimatorController=controller;animator.updateMode=AnimatorUpdateMode.UnscaledTime;animator.cullingMode=AnimatorCullingMode.AlwaysAnimate;
        var text=Rect(root,"EnergyText",new Vector2(250,42),new Vector2(232,-67),new Vector2(0,1)).gameObject.AddComponent<TextMeshProUGUI>();
        text.font=hud.coins.font;text.fontSize=30;text.fontStyle=FontStyles.Bold;text.alignment=TextAlignmentOptions.Center;
        text.enableWordWrapping=false;text.color=hud.coins.color;text.raycastTarget=false;text.text="100";
        string after=BrasilPortalSetup.CollisionSnapshot();File.WriteAllText(Docs+"colisoes-depois.txt",after);
        if(collisions!=after)throw new InvalidOperationException("Colisoes divergiram; cancelar salvamento.");
        EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
        File.WriteAllText(Docs+"integracao.txt","OK\nRetratos Quest atualizados: "+portraitCount+"\nIdle: seis frames, 12 FPS, pausas e blink de 2 frames; loop.\nComemoracao: somente importada.\n"+DateTime.Now.ToString("s"));
    }
}

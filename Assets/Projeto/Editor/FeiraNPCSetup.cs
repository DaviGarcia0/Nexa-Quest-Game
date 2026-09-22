using System;
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

public static class FeiraNPCSetup
{
    const string Art="Assets/Projeto/Sprites/Brasil/NPCs/";
    const string Anim="Assets/Projeto/Animations/NPCs/";
    const string Docs="Documentation/Brasil/EtapaNPCs/";
    [Serializable] class Sheet { public float pixelsPerUnit; public Frame[] frames; }
    [Serializable] class Frame { public string name; public int x,y,width,height; public float pivotX,pivotY; }
    static Transform Child(Transform parent,string name){var t=new GameObject(name).transform;t.SetParent(parent,false);return t;}
    static Sprite[] Import(string name)
    {
        string path=Art+name+".png";var data=JsonUtility.FromJson<Sheet>(File.ReadAllText(Art+name+".json"));
        var i=(TextureImporter)AssetImporter.GetAtPath(path);
        i.textureType=TextureImporterType.Sprite;i.spriteImportMode=SpriteImportMode.Multiple;i.spritePixelsPerUnit=data.pixelsPerUnit;
        i.filterMode=FilterMode.Point;i.mipmapEnabled=false;i.textureCompression=TextureImporterCompression.Uncompressed;i.npotScale=TextureImporterNPOTScale.None;i.maxTextureSize=4096;i.alphaIsTransparency=true;
        var settings=new TextureImporterSettings();i.ReadTextureSettings(settings);settings.spriteMeshType=SpriteMeshType.FullRect;settings.spriteGenerateFallbackPhysicsShape=false;i.SetTextureSettings(settings);
        var defaults=i.GetDefaultPlatformTextureSettings();defaults.maxTextureSize=4096;defaults.textureCompression=TextureImporterCompression.Uncompressed;i.SetPlatformTextureSettings(defaults);
        i.SaveAndReimport();
        var factory=new SpriteDataProviderFactories();factory.Init();var provider=factory.GetSpriteEditorDataProviderFromObject(i);provider.InitSpriteEditorDataProvider();
        var old=provider.GetSpriteRects().ToDictionary(s=>s.name,s=>s.spriteID);
        var rects=data.frames.Select(f=>new SpriteRect{name=f.name,rect=new Rect(f.x,f.y,f.width,f.height),pivot=new Vector2(f.pivotX,f.pivotY),alignment=SpriteAlignment.Custom,spriteID=old.ContainsKey(f.name)?old[f.name]:GUID.Generate()}).ToArray();
        provider.SetSpriteRects(rects);provider.GetDataProvider<ISpriteNameFileIdDataProvider>().SetNameFileIdPairs(rects.Select(r=>new SpriteNameFileIdPair(r.name,r.spriteID)));provider.Apply();AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceUpdate);
        return AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().ToArray();
    }
    static AnimatorController Animate(string name,Sprite[] frames)
    {
        var controller=AnimatorController.CreateAnimatorControllerAtPath(Anim+name+".controller");
        var clip=new AnimationClip{name="Idle",frameRate=4};
        // Pausas na pose neutra e piscar breve evitam um movimento mecanico rapido.
        float[] times={0,.8f,1.1f,1.3f,2.4f};
        var keys=new ObjectReferenceKeyframe[5];for(int j=0;j<5;j++)keys[j]=new ObjectReferenceKeyframe{time=times[j],value=frames[j%4]};
        AnimationUtility.SetObjectReferenceCurve(clip,EditorCurveBinding.PPtrCurve("",typeof(SpriteRenderer),"m_Sprite"),keys);
        var options=AnimationUtility.GetAnimationClipSettings(clip);options.loopTime=true;AnimationUtility.SetAnimationClipSettings(clip,options);
        AssetDatabase.CreateAsset(clip,Anim+name+".anim");var state=controller.layers[0].stateMachine.AddState("Idle");state.motion=clip;controller.layers[0].stateMachine.defaultState=state;return controller;
    }
    static RectTransform Rect(Transform parent,string name,Vector2 position,Vector2 size,Vector2 anchor)
    {
        var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(parent,false);r.anchorMin=r.anchorMax=anchor;r.pivot=new Vector2(.5f,.5f);r.anchoredPosition=position;r.sizeDelta=size;return r;
    }
    static Image Fill(RectTransform r,Color color){var i=r.gameObject.AddComponent<Image>();i.color=color;i.raycastTarget=false;return i;}
    static TextMeshProUGUI Text(RectTransform r,string text,float size,Color color)
    {
        var t=r.gameObject.AddComponent<TextMeshProUGUI>();t.font=Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");t.text=text;t.fontSize=size;t.color=color;t.raycastTarget=false;t.enableWordWrapping=true;return t;
    }
    static DialogueData Dialogue(string file,Sprite npc,Sprite quest,string name,string[] phrases,string response)
    {
        var d=ScriptableObject.CreateInstance<DialogueData>();
        var lines=phrases.Select(s=>new DialogueLine{speaker=DialogueSpeaker.NPC,speakerName=name,portrait=npc,text=s}).ToList();
        lines.Insert(1,new DialogueLine{speaker=DialogueSpeaker.Quest,speakerName="Quest",portrait=quest,text=response});
        d.lines=lines.ToArray();AssetDatabase.CreateAsset(d,"Assets/Projeto/Dialogue/"+file+".asset");return d;
    }
    [MenuItem("Nexa Quest/Preparar NPCs da Feira")]
    public static void Run()
    {
        var scene=SceneManager.GetActiveScene();if(scene.path!="Assets/Projeto/Scenes/Brasil.unity"||EditorApplication.isPlayingOrWillChangePlaymode)return;
        var w=UnityEngine.Object.FindObjectOfType<BrazilWorld>();var feira=w.areas.Single(a=>a.name=="Feira");
        if(feira.transform.Find("NPCs")!=null){Debug.LogWarning("NPCs ja configurados; montagem nao sobrescreve.");return;}
        Directory.CreateDirectory(Docs);string before=BrasilPortalSetup.CollisionSnapshot();File.WriteAllText(Docs+"colisoes-antes.txt",before);
        var female=Import("NPC 1");var male=Import("NPC 2");var decorations=Import("DecorativosBrasil");var quest=Import("QuestPortraitSource").Single();
        var canvas=w.fadePanel.GetComponentInParent<Canvas>();var prompt=canvas.transform.Find("InteractionPrompt").gameObject;
        var controller=w.gameObject.AddComponent<DialogueController>();controller.world=w;controller.interactionPrompt=prompt;
        var panel=Rect(canvas.transform,"DialoguePanel",new Vector2(0,196),new Vector2(1792,336),new Vector2(.5f,0));
        Fill(panel,new Color32(218,170,79,255));
        Fill(Rect(panel,"Interior",Vector2.zero,new Vector2(1780,324),new Vector2(.5f,.5f)),new Color32(34,24,50,250));
        Fill(Rect(panel,"PortraitFrame",new Vector2(-729,0),new Vector2(290,290),new Vector2(.5f,.5f)),new Color32(87,58,93,255));
        var portrait=Fill(Rect(panel,"Portrait",new Vector2(-729,0),new Vector2(272,272),new Vector2(.5f,.5f)),Color.white);portrait.preserveAspect=true;
        var title=Text(Rect(panel,"SpeakerName",new Vector2(140,113),new Vector2(1390,48),new Vector2(.5f,.5f)),"",34,new Color32(249,214,141,255));title.fontStyle=FontStyles.Bold;
        var body=Text(Rect(panel,"DialogueText",new Vector2(140,-2),new Vector2(1390,164),new Vector2(.5f,.5f)),"",32,new Color32(255,248,230,255));body.verticalAlignment=VerticalAlignmentOptions.Middle;
        var indicator=Text(Rect(panel,"ContinueIndicator",new Vector2(595,-124),new Vector2(430,35),new Vector2(.5f,.5f)),"",23,new Color32(229,206,247,255));indicator.alignment=TextAlignmentOptions.Right;
        controller.panel=panel.gameObject;controller.portrait=portrait;controller.speakerName=title;controller.dialogueText=body;controller.continueIndicator=indicator;panel.gameObject.SetActive(false);
        string[] welcome={"Olá, Quest! Bem-vinda ao Brasil!","Durante sua viagem, você encontrará pessoas espalhadas por diferentes lugares.","Algumas delas terão desafios, jogos e perguntas para você completar.","Seu objetivo é conseguir as três chaves escondidas nesta jornada.","Para conquistar uma chave, você precisará completar os desafios daquele local.","Explore a Feira, a Amazônia e o Ponto Turístico e converse com os NPCs que encontrar.","Depois de conseguir todas as chaves, vá até o Portal.","Coloque as três chaves no Portal para ativá-lo.","Quando o Portal estiver aberto, você poderá continuar sua aventura para o próximo país da América do Sul!","Boa viagem, Quest. E não deixe nenhum desafio para trás!"};
        string[] challenges={"Oi, Quest! Veio conhecer um pouco mais da nossa feira?","Aqui você terá dois desafios para completar.","Um deles será um Quiz sobre o que você descobrir durante sua viagem.","O outro será um jogo especial desta região.","Quando você completar os dois desafios, poderá conquistar a chave da Feira.","Essa chave será importante para ativar o Portal depois.","Por enquanto, explore o lugar. Quando os desafios estiverem disponíveis, volte e fale comigo!"};
        var intro=Dialogue("Feira_BoasVindas",female.Single(s=>s.name=="Portrait"),quest,"Lia",welcome,"Obrigada! Estou pronta para explorar. Por onde começo?");
        var challenge=Dialogue("Feira_Desafios",male.Single(s=>s.name=="Portrait"),quest,"Bento",challenges,"Vim, sim! Que desafios vou encontrar por aqui?");
        string[] names={"NPC_Introducao_Lia","NPC_Desafios_Bento","NPC_Decorativo_Feirante","NPC_Decorativo_Moradora","NPC_Decorativo_Torcedor","NPC_Decorativo_Senhora"};
        Vector2[] positions={new Vector2(1450,700),new Vector2(900,735),new Vector2(630,461),new Vector2(1036,385),new Vector2(1625,752),new Vector2(1212,174)};
        string[] prefixes={"Down","Down","Feirante","Moradora","Torcedor","Senhora"};
        var parent=Child(feira.transform,"NPCs");
        for(int j=0;j<6;j++){
            var sheet=j==0?female:j==1?male:decorations;var frames=sheet.Where(s=>s.name.StartsWith(prefixes[j]+"_")).OrderBy(s=>s.name).ToArray();
            var npc=Child(parent,names[j]);var render=npc.gameObject.AddComponent<SpriteRenderer>();render.sprite=frames[0];render.sharedMaterial=feira.map.sharedMaterial;render.sortingOrder=9;
            var animator=npc.gameObject.AddComponent<Animator>();animator.runtimeAnimatorController=Animate(names[j],frames);
            if(j<2){var trigger=npc.gameObject.AddComponent<CircleCollider2D>();trigger.isTrigger=true;trigger.radius=.58f;trigger.offset=new Vector2(0,.12f);var interaction=npc.gameObject.AddComponent<NPCInteraction>();interaction.dialogue=j==0?intro:challenge;}
            PrefabUtility.SaveAsPrefabAssetAndConnect(npc.gameObject,"Assets/Projeto/Prefabs/NPCs/"+names[j]+".prefab",InteractionMode.AutomatedAction);
            var rect=feira.map.sprite.rect;Vector2 pp=positions[j];Vector2 local=(pp-new Vector2(rect.width/2,rect.height/2))/100f;local.y=-local.y;npc.position=feira.map.transform.TransformPoint(local);
            if(j<2){npc.GetComponent<NPCInteraction>().controller=controller;PrefabUtility.RecordPrefabInstancePropertyModifications(npc.GetComponent<NPCInteraction>());}
            PrefabUtility.RecordPrefabInstancePropertyModifications(npc);
        }
        new GameObject("ProgressionManager").AddComponent<ProgressionManager>();
        var hud=Rect(canvas.transform,"HUD",new Vector2(-245,-105),new Vector2(430,170),Vector2.one);Fill(hud,new Color32(34,24,50,232));
        var hudScript=hud.gameObject.AddComponent<ProgressionHUD>();
        hudScript.coins=Text(Rect(hud,"Coins",new Vector2(0,52),new Vector2(386,38),new Vector2(.5f,.5f)),"",25,new Color32(255,215,115,255));
        hudScript.brainLevel=Text(Rect(hud,"BrainLevel",new Vector2(0,12),new Vector2(386,36),new Vector2(.5f,.5f)),"",24,new Color32(236,195,247,255));
        hudScript.knowledge=Text(Rect(hud,"Knowledge",new Vector2(0,-25),new Vector2(386,32),new Vector2(.5f,.5f)),"",21,Color.white);
        var bar=Rect(hud,"KnowledgeBar",new Vector2(0,-61),new Vector2(384,12),new Vector2(.5f,.5f));Fill(bar,new Color32(76,63,89,255));
        hudScript.knowledgeFill=Fill(Rect(bar,"Fill",Vector2.zero,new Vector2(384,12),new Vector2(.5f,.5f)),new Color32(122,225,196,255));hudScript.knowledgeFill.type=Image.Type.Filled;hudScript.knowledgeFill.fillMethod=Image.FillMethod.Horizontal;hudScript.knowledgeFill.fillAmount=0;
        hudScript.knowledgeFill.sprite=AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        w.fadePanel.transform.SetAsLastSibling();
        string after=BrasilPortalSetup.CollisionSnapshot();File.WriteAllText(Docs+"colisoes-depois.txt",after);if(before!=after)throw new Exception("Colisoes divergiram; cena nao salva.");
        EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();File.WriteAllText(Docs+"setup.txt","NPC_SETUP_OK\n6 NPCs, 2 interativos, 4 decorativos. Colisoes preservadas.\n");
    }
    public static void FixReferences()
    {
        var controller=UnityEngine.Object.FindObjectOfType<DialogueController>();
        foreach(var npc in UnityEngine.Object.FindObjectsOfType<NPCInteraction>(true))
        {
            npc.controller=controller;
            PrefabUtility.RecordPrefabInstancePropertyModifications(npc);
        }
        var scene=SceneManager.GetActiveScene();EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
    }
    public static void CheckReturnSpawn()
    {
        var world=UnityEngine.Object.FindObjectOfType<BrazilWorld>();
        var feira=world.areas.Single(a=>a.name=="Feira");
        var spawn=feira.transform.Find("SpawnPoints/EntradaPortal");
        Physics2D.SyncTransforms();
        var foot=world.player.GetComponent<CapsuleCollider2D>();
        Func<Vector2,Collider2D[]> obstacles=p=>Physics2D.OverlapBoxAll(p+foot.offset,foot.size+Vector2.one*.04f,0).Where(c=>!c.isTrigger&&c.gameObject!=world.player.gameObject).ToArray();
        var existing=obstacles(spawn.position);var candidate=new Vector3(6.45f,2.55f,0);var next=obstacles(candidate);
        var info="Spawn anterior "+spawn.position+": "+string.Join(", ",existing.Select(c=>c.name))+"\nCandidato "+candidate+": "+string.Join(", ",next.Select(c=>c.name));
        if(existing.Length>0&&next.Length==0){spawn.position=candidate;var scene=SceneManager.GetActiveScene();EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);info+="\nSpawn reposicionado em rua livre. Colliders intactos.";}
        File.WriteAllText(Docs+"spawn-retorno.txt",info);
    }
}


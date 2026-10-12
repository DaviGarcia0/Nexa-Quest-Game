using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEditor.U2D.Sprites;
using UnityEngine;
using UnityEngine.SceneManagement;
using NexaQuest.Brasil;

public static class MontagemNPCsAmazonia
{
    public const string Docs = "Documentation/Brasil/Amazonia20261012/";
    const string Arte = "Assets/Projeto/Sprites/Brasil/NPCsAmazonia/";
    const string Animacoes = "Assets/Projeto/Animations/NPCsAmazonia/";
    [Serializable] class Sheet { public float pixelsPerUnit; public Frame[] frames; }
    [Serializable] class Frame { public string name; public int x,y,width,height; public float pivotX,pivotY; }
    static Sprite[] Importar(int numero)
    {
        string nome = "NPCAmazonia0" + numero, caminho = Arte + nome + ".png";
        var dados = JsonUtility.FromJson<Sheet>(File.ReadAllText(Arte + nome + ".json"));
        var i = (TextureImporter)AssetImporter.GetAtPath(caminho);
        i.textureType = TextureImporterType.Sprite; i.spriteImportMode = SpriteImportMode.Multiple;
        i.spritePixelsPerUnit = dados.pixelsPerUnit; i.filterMode = FilterMode.Point;
        i.mipmapEnabled = false; i.textureCompression = TextureImporterCompression.Uncompressed;
        i.npotScale = TextureImporterNPOTScale.None; i.maxTextureSize = 4096; i.alphaIsTransparency = true;
        var configuracao = new TextureImporterSettings(); i.ReadTextureSettings(configuracao);
        configuracao.spriteMeshType = SpriteMeshType.FullRect; configuracao.spriteGenerateFallbackPhysicsShape = false; i.SetTextureSettings(configuracao);
        i.SaveAndReimport();
        var fabrica = new SpriteDataProviderFactories(); fabrica.Init();
        var provedor = fabrica.GetSpriteEditorDataProviderFromObject(i); provedor.InitSpriteEditorDataProvider();
        var recortes = dados.frames.Select(f => new SpriteRect {name=f.name,rect=new Rect(f.x,f.y,f.width,f.height),pivot=new Vector2(f.pivotX,f.pivotY),alignment=SpriteAlignment.Custom,spriteID=GUID.Generate()}).ToArray();
        provedor.SetSpriteRects(recortes); provedor.GetDataProvider<ISpriteNameFileIdDataProvider>().SetNameFileIdPairs(recortes.Select(r => new SpriteNameFileIdPair(r.name,r.spriteID)));
        provedor.Apply(); AssetDatabase.ImportAsset(caminho,ImportAssetOptions.ForceUpdate);
        return AssetDatabase.LoadAllAssetsAtPath(caminho).OfType<Sprite>().OrderBy(s=>s.name).ToArray();
    }
    static AnimatorController Animar(int numero, Sprite[] sprites)
    {
        string nome="NPC_Amazonia_0"+numero;
        var clip=new AnimationClip {name="Parado",frameRate=3};
        int[] quadros=numero==4?new[]{0,4,0}:numero==3?new[]{0,0,0}:new[]{0,1,2,3,0};
        float[] tempos=numero==4?new[]{0f,2.1f,2.4f}:numero==3?new[]{0f,1.2f,2.4f}:new[]{0f,1.7f,2f,2.2f,2.4f};
        AnimationUtility.SetObjectReferenceCurve(clip,EditorCurveBinding.PPtrCurve("",typeof(SpriteRenderer),"m_Sprite"),quadros.Select((q,k)=>new ObjectReferenceKeyframe{time=tempos[k],value=sprites[q]}).ToArray());
        if(numero==3) AnimationUtility.SetEditorCurve(clip,EditorCurveBinding.FloatCurve("",typeof(Transform),"m_LocalScale.y"),new AnimationCurve(new Keyframe(0,1),new Keyframe(1.2f,1.008f),new Keyframe(2.4f,1)));
        var ajustes=AnimationUtility.GetAnimationClipSettings(clip);ajustes.loopTime=true;AnimationUtility.SetAnimationClipSettings(clip,ajustes);
        AssetDatabase.CreateAsset(clip,Animacoes+nome+".anim");
        var controlador=AnimatorController.CreateAnimatorControllerAtPath(Animacoes+nome+".controller");
        var estado=controlador.layers[0].stateMachine.AddState("Parado");estado.motion=clip;controlador.layers[0].stateMachine.defaultState=estado;return controlador;
    }
    public static void Montar()
    {
        var cena=SceneManager.GetActiveScene();
        if(cena.path!="Assets/Projeto/Scenes/Brasil.unity"||cena.isDirty) throw new InvalidOperationException("Brasil deve estar salva e fora de Play Mode.");
        var mundo=UnityEngine.Object.FindObjectOfType<BrazilWorld>();
        var area=mundo.areas.Single(a=>a.name=="Amazonia" || a.name=="Amazonas" || a.name=="Amazônia");
        if(area.transform.Find("NPCsAmazonia")!=null) throw new InvalidOperationException("Os NPCs da Amazônia já foram montados.");
        var controlador=UnityEngine.Object.FindObjectOfType<DialogueController>();
        var lia=mundo.areas.Single(a=>a.name=="Feira").transform.Find("NPCs/NPC_Introducao_Lia").GetComponent<NPCInteraction>();
        var retratoLia=lia.dialogue.lines.First(l=>l.speaker==DialogueSpeaker.NPC).portrait;
        var retratoQuest=lia.dialogue.lines.First(l=>l.speaker==DialogueSpeaker.Quest).portrait;
        string[] falas={
            "Oi! Você parece meio perdida. Chegou agora?",
            "Cheguei! Eu sou a Quest. Escolhi o Brasil no mapa e vim parar aqui... ainda estou tentando entender como tudo funciona.",
            "Prazer, Quest! Então seja bem-vinda ao Brasil. Você caiu num ótimo lugar para começar.",
            "Eu queria conhecer melhor o país... mas também estou procurando uma forma de continuar minha viagem. Ouvi falar de um portal por aqui.",
            "Tem sim. Só que ele não abre tão fácil. Você vai precisar conseguir três chaves.",
            "Três chaves?",
            "Isso. Existe uma em cada parte da sua jornada: uma na Feira, outra na Amazônia e outra no Ponto Turístico.",
            "Começa por aqui mesmo. Procure o Bento. Ele tem um desafio para você e pode te ajudar a conseguir a primeira chave.",
            "E depois?",
            "Depois siga para a Amazônia e para o Ponto Turístico. Em cada lugar, procure o NPC responsável e complete o desafio.",
            "Quando estiver com as três chaves, é só voltar ao Portal.",
            "Entendi! Então, além de encontrar as chaves, vou conhecer um pouco mais do Brasil pelo caminho.",
            "Exatamente. Boa sorte, Quest! O Bento está te esperando."
        };
        int[] quest={1,3,5,8,11};
        lia.dialogue.lines=falas.Select((t,k)=>new DialogueLine {speaker=quest.Contains(k)?DialogueSpeaker.Quest:DialogueSpeaker.NPC,speakerName=quest.Contains(k)?"Quest":"Lia",portrait=quest.Contains(k)?retratoQuest:retratoLia,text=t}).ToArray();
        EditorUtility.SetDirty(lia.dialogue);
        Directory.CreateDirectory(Animacoes); AssetDatabase.Refresh();
        var raiz=new GameObject("NPCsAmazonia").transform;raiz.SetParent(area.transform,false);
        Vector2[] pontos={new Vector2(1004,183),new Vector2(407,247),new Vector2(239,379),new Vector2(1202,156)};
        for(int n=1;n<=4;n++)
        {
            var sprites=Importar(n);
            var npc=new GameObject("NPC_Amazonia_0"+n).transform;npc.SetParent(raiz,false);
            var visual=new GameObject("Visual").transform;visual.SetParent(npc,false);
            var render=visual.gameObject.AddComponent<SpriteRenderer>();render.sprite=sprites[0];render.sharedMaterial=area.map.sharedMaterial;render.sortingOrder=9;
            visual.gameObject.AddComponent<Animator>().runtimeAnimatorController=Animar(n,sprites);
            Vector2 local=(pontos[n-1]-area.map.sprite.rect.size/2)/area.map.sprite.pixelsPerUnit;local.y=-local.y;
            npc.position=area.map.transform.TransformPoint(local);
            if(n==1)
            {
                var corpo=npc.gameObject.AddComponent<BoxCollider2D>();corpo.size=new Vector2(.22f,.1f);corpo.offset=new Vector2(0,.03f);
                var gatilho=npc.gameObject.AddComponent<CircleCollider2D>();gatilho.isTrigger=true;gatilho.radius=.58f;gatilho.offset=new Vector2(0,.12f);
                var conversa=ScriptableObject.CreateInstance<DialogueData>();
                string[] frases={"Olá, viajante. Veio explorar a Amazônia?","Vim! Também estou procurando uma das chaves que preciso para abrir o Portal.","Então chegou ao lugar certo. Aqui você vai ter que provar o que aprendeu.","Posso te desafiar de duas formas. Em breve, vou te mostrar como."};
                conversa.lines=frases.Select((t,k)=>new DialogueLine {speaker=k==1?DialogueSpeaker.Quest:DialogueSpeaker.NPC,speakerName=k==1?"Quest":"Guardiã da Amazônia",portrait=k==1?retratoQuest:sprites[0],text=t}).ToArray();
                AssetDatabase.CreateAsset(conversa,"Assets/Projeto/Dialogue/Amazonia_Desafios.asset");
                var interacao=npc.gameObject.AddComponent<NPCInteraction>();interacao.controller=controlador;interacao.dialogue=conversa;
                // onDialogueFinished fica disponível para conectar o painel quando sua arte estiver pronta.
            }
        }
        AssetDatabase.SaveAssets(); EditorSceneManager.MarkSceneDirty(cena);EditorSceneManager.SaveScene(cena);
        Directory.CreateDirectory(Docs);File.WriteAllText(Docs+"montagem.txt","Montado em "+DateTime.Now.ToString("s")+"\nPainel ausente: arte ainda não criada, conforme informado pelo usuário.");
    }
}
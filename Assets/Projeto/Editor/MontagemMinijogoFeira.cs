using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEditor.U2D.Sprites;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using NexaQuest.Brasil;

public static class MontagemMinijogoFeira
{
    public const string Arte="Assets/Projeto/Sprites/Brasil/MinijogoFeira/";
    public const string Animacoes="Assets/Projeto/Animations/MinijogoFeira/";
    public const string Documentacao="Documentation/Brasil/MinijogoFeira20261003/";
    [Serializable]class Folha{public Quadro[] quadros;}
    [Serializable]class Quadro{public string nome;public int x,y,largura,altura;public float pivoX,pivoY;}
    static TMP_FontAsset fonte;
    static void Importar(){
        foreach(var arquivo in Directory.GetFiles(Arte,"*.png")){
            var caminho=arquivo.Replace('\\','/');var i=(TextureImporter)AssetImporter.GetAtPath(caminho);
            i.textureType=TextureImporterType.Sprite;i.spriteImportMode=caminho.EndsWith("QuestComCesto.png")?SpriteImportMode.Multiple:SpriteImportMode.Single;
            i.filterMode=FilterMode.Point;i.textureCompression=TextureImporterCompression.Uncompressed;i.mipmapEnabled=false;i.maxTextureSize=2048;i.npotScale=TextureImporterNPOTScale.None;
            i.spritePixelsPerUnit=100;i.alphaIsTransparency=true;
            var configuracao=new TextureImporterSettings();i.ReadTextureSettings(configuracao);configuracao.spriteGenerateFallbackPhysicsShape=false;configuracao.spriteMeshType=SpriteMeshType.FullRect;i.SetTextureSettings(configuracao);i.SaveAndReimport();
        }
        var dados=JsonUtility.FromJson<Folha>(File.ReadAllText(Arte+"RecortesQuestCesto.json"));
        var importador=(TextureImporter)AssetImporter.GetAtPath(Arte+"QuestComCesto.png");
        var fabrica=new SpriteDataProviderFactories();fabrica.Init();var provedor=fabrica.GetSpriteEditorDataProviderFromObject(importador);provedor.InitSpriteEditorDataProvider();
        var recortes=dados.quadros.Select(q=>new SpriteRect{name=q.nome,rect=new Rect(q.x,q.y,q.largura,q.altura),pivot=new Vector2(q.pivoX,q.pivoY),alignment=SpriteAlignment.Custom,spriteID=GUID.Generate()}).ToArray();
        provedor.SetSpriteRects(recortes);provedor.GetDataProvider<ISpriteNameFileIdDataProvider>().SetNameFileIdPairs(recortes.Select(r=>new SpriteNameFileIdPair(r.name,r.spriteID)));provedor.Apply();importador.SaveAndReimport();
    }
    static AnimatorController Animador(){
        Directory.CreateDirectory(Animacoes);AssetDatabase.Refresh();var controle=AnimatorController.CreateAnimatorControllerAtPath(Animacoes+"QuestComCesto.controller");
        controle.AddParameter("Direcao",AnimatorControllerParameterType.Int);var maquina=controle.layers[0].stateMachine;
        var todos=AssetDatabase.LoadAllAssetsAtPath(Arte+"QuestComCesto.png").OfType<Sprite>().ToArray();
        foreach(var direcao in new[]{"Parada","Esquerda","Direita"}){
            var quadros=todos.Where(s=>s.name.StartsWith("Cesto"+direcao+"_")).OrderBy(s=>s.name).ToArray();
            var clipe=new AnimationClip{name="QuestCesto_"+direcao,frameRate=direcao=="Parada"?2:8};
            var chaves=quadros.Select((s,n)=>new ObjectReferenceKeyframe{time=n/clipe.frameRate,value=s}).ToArray();
            AnimationUtility.SetObjectReferenceCurve(clipe,new EditorCurveBinding{path="ArteQuest",type=typeof(Image),propertyName="m_Sprite"},chaves);
            var ajustes=AnimationUtility.GetAnimationClipSettings(clipe);ajustes.loopTime=true;ajustes.stopTime=quadros.Length/clipe.frameRate;AnimationUtility.SetAnimationClipSettings(clipe,ajustes);
            AssetDatabase.CreateAsset(clipe,Animacoes+clipe.name+".anim");
            var estado=maquina.AddState(direcao);estado.motion=clipe;if(direcao=="Parada")maquina.defaultState=estado;
            var transicao=maquina.AddAnyStateTransition(estado);transicao.hasExitTime=false;transicao.duration=0;transicao.canTransitionToSelf=false;
            transicao.AddCondition(AnimatorConditionMode.Equals,direcao=="Parada"?0:direcao=="Esquerda"?-1:1,"Direcao");
        }
        return controle;
    }
    static RectTransform Objeto(string nome,Transform pai,Vector2 tamanho,Vector2 posicao){
        var r=new GameObject(nome,typeof(RectTransform)).GetComponent<RectTransform>();r.gameObject.layer=5;r.SetParent(pai,false);r.sizeDelta=tamanho;r.anchoredPosition=posicao;return r;
    }
    static RectTransform Tela(string nome,Transform pai){var r=Objeto(nome,pai,Vector2.zero,Vector2.zero);r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=r.offsetMax=Vector2.zero;return r;}
    static Image Imagem(string nome,Transform pai,string arquivo,Vector2 tamanho,Vector2 posicao){var r=Objeto(nome,pai,tamanho,posicao);var i=r.gameObject.AddComponent<Image>();i.sprite=AssetDatabase.LoadAssetAtPath<Sprite>(Arte+arquivo+".png");i.preserveAspect=true;i.raycastTarget=false;return i;}
    static TMP_Text Texto(string nome,Transform pai,Vector2 tamanho,Vector2 posicao,float altura=28){
        var r=Objeto(nome,pai,tamanho,posicao);var t=r.gameObject.AddComponent<TextMeshProUGUI>();t.font=fonte;t.fontSize=altura;t.alignment=TextAlignmentOptions.Center;t.color=Color.white;t.raycastTarget=false;t.enableWordWrapping=true;return t;
    }
    static Button Botao(string nome,Image imagem,Rect pixels,UnityEngine.Events.UnityAction acao){
        var tamanho=imagem.sprite.rect.size;var escala=imagem.rectTransform.sizeDelta/tamanho;
        var r=Objeto(nome,imagem.transform,Vector2.Scale(pixels.size,escala),Vector2.Scale(new Vector2(pixels.center.x-tamanho.x*.5f,tamanho.y*.5f-pixels.center.y),escala));
        var i=r.gameObject.AddComponent<Image>();i.color=Color.clear;var b=r.gameObject.AddComponent<Button>();b.targetGraphic=i;b.transition=Selectable.Transition.None;b.navigation=new Navigation{mode=Navigation.Mode.None};UnityEventTools.AddPersistentListener(b.onClick,acao);return b;
    }
    static TMP_Text TextoNaArte(string nome,Image imagem,Vector2 centro,Vector2 tamanho,float altura){
        var original=imagem.sprite.rect.size;var escala=imagem.rectTransform.sizeDelta/original;
        return Texto(nome,imagem.transform,Vector2.Scale(tamanho,escala),Vector2.Scale(new Vector2(centro.x-original.x*.5f,original.y*.5f-centro.y),escala),altura);
    }
    static Image Painel(string nome,Transform pai,string arquivo){
        var r=Tela(nome,pai);r.gameObject.AddComponent<Image>().color=new Color(0,0,0,.55f);
        var sprite=AssetDatabase.LoadAssetAtPath<Sprite>(Arte+arquivo+".png");
        var arte=Imagem("ArtePainel",r,arquivo,new Vector2(1180,1180*sprite.rect.height/sprite.rect.width),Vector2.zero);r.gameObject.SetActive(false);return arte;
    }
    public static void Montar(){
        var cena=SceneManager.GetActiveScene();if(cena.isDirty||cena.path!="Assets/Projeto/Scenes/Brasil.unity")throw new InvalidOperationException("Abra Brasil salva e fora de Play Mode.");
        var canvas=GameObject.Find("Canvas").transform;if(canvas.Find("MinijogoFeira")!=null)throw new InvalidOperationException("Minijogo já montado; não duplicar.");
        Importar();var controladorAnimacao=Animador();fonte=UnityEngine.Object.FindObjectOfType<DialogueController>().speakerName.font;
        var mundo=UnityEngine.Object.FindObjectOfType<BrazilWorld>();var desafios=UnityEngine.Object.FindObjectOfType<PainelDesafiosFeira>(true);
        var raiz=Tela("MinijogoFeira",canvas);raiz.gameObject.SetActive(false);var mini=raiz.gameObject.AddComponent<MinijogoFeira>();mini.mundo=mundo;
        var ui=raiz.gameObject.AddComponent<InterfaceMinijogoFeira>();mini.interfaceJogo=ui;
        var visual=Objeto("VisualMinijogo",raiz,new Vector2(1920,1080),Vector2.zero);ui.visual=visual;
        var fundo=Imagem("CenarioFeira",visual,"CenarioFeira",new Vector2(1920,1080),Vector2.zero);fundo.raycastTarget=true;
        var area=Objeto("AreaJogavel",visual,new Vector2(1920,1080),Vector2.zero);
        var camadaItens=Objeto("ItensEmQueda",area,new Vector2(1920,1080),Vector2.zero);
        var jogador=Objeto("QuestComCesto",area,new Vector2(280,300),new Vector2(0,-385));var quest=jogador.gameObject.AddComponent<QuestCestoFeira>();quest.minijogo=mini;mini.quest=quest;
        quest.animador=jogador.gameObject.AddComponent<Animator>();quest.animador.runtimeAnimatorController=controladorAnimacao;quest.animador.updateMode=AnimatorUpdateMode.UnscaledTime;
        var arteQuest=Objeto("ArteQuest",jogador,new Vector2(185,293)*.9f,Vector2.zero);quest.imagem=arteQuest.gameObject.AddComponent<Image>();quest.imagem.raycastTarget=false;
        quest.imagem.sprite=AssetDatabase.LoadAllAssetsAtPath(Arte+"QuestComCesto.png").OfType<Sprite>().Single(s=>s.name=="CestoParada_01");
        quest.aberturaCesto=Objeto("AberturaCesto",jogador,new Vector2(124,12),new Vector2(0,250));quest.AlinharArte();
        var queda=camadaItens.gameObject.AddComponent<QuedaItensFeira>();queda.minijogo=mini;queda.quest=quest;queda.camada=camadaItens;mini.queda=queda;
        queda.sprites=new[]{"Banana","Abacaxi","Melancia","Chinelo","Disco","ConsoleXbox360"}.Select(n=>AssetDatabase.LoadAssetAtPath<Sprite>(Arte+n+".png")).ToArray();
        var hudFrutas=Imagem("ProgressoFrutas",raiz,"IndicadorFrutas",new Vector2(275,275*1448f/1086),new Vector2(-805,60));
        ui.frutas=new[]{390f,755f,1120f}.Select((y,i)=>TextoNaArte("Quantidade"+new[]{"Bananas","Abacaxis","Melancias"}[i],hudFrutas,new Vector2(685,y),new Vector2(380,140),29)).ToArray();
        var hudErros=Imagem("IndicadorErros",raiz,"IndicadorErros",new Vector2(275,275*1448f/1086),new Vector2(805,-30));
        ui.frutasPerdidas=TextoNaArte("FrutasPerdidas",hudErros,new Vector2(705,560),new Vector2(380,140),29);
        ui.itensErrados=TextoNaArte("ItensErrados",hudErros,new Vector2(705,950),new Vector2(380,140),29);
        var sair=Imagem("BotaoSair",raiz,"BotaoSair",new Vector2(280,280*1024f/1536),new Vector2(775,-405));Botao("AreaClique",sair,new Rect(230,355,1070,300),mini.Sair);
        Texto("Controles",raiz,new Vector2(950,42),new Vector2(0,-490),23).text="A / D ou setas: mover o cesto   |   Esc: sair";
        var hudCompartilhada=Tela("HUDsExistentes",raiz);ui.camadaHUD=hudCompartilhada;
        ui.hudsCompartilhadas=new[]{canvas.Find("EnergyHUD"),UnityEngine.Object.FindObjectOfType<ProgressionHUD>().transform}.OrderBy(t=>t.GetSiblingIndex()).ToArray();
        var energia=canvas.Find("EnergyHUD/EnergyText").GetComponent<TMP_Text>();
        var indicador=energia.gameObject.AddComponent<IndicadorEnergia>();var so=new SerializedObject(indicador);so.FindProperty("texto").objectReferenceValue=energia;so.ApplyModifiedPropertiesWithoutUndo();
        var instrucoes=Painel("Instrucoes",raiz,"PainelInstrucoes");ui.instrucoes=instrucoes.transform.parent.gameObject;
        Botao("BotaoJogar",instrucoes,new Rect(515,887,422,103),mini.IniciarPartida);
        var primeira=Painel("VitoriaComChave",raiz,"PainelVitoriaComChave");ui.vitoriaComChave=primeira.transform.parent.gameObject;
        var seguinte=Painel("VitoriaSemChave",raiz,"PainelVitoriaSemChave");ui.vitoriaSemChave=seguinte.transform.parent.gameObject;
        foreach(var painel in new[]{primeira,seguinte}){Botao("BotaoRejogar",painel,new Rect(410,907,282,85),mini.IniciarPartida);Botao("BotaoSair",painel,new Rect(775,907,286,85),mini.Sair);}
        ui.moedasPrimeiraVitoria=TextoNaArte("MoedasDaPartida",primeira,new Vector2(957,500),new Vector2(190,50),28);
        ui.moedasOutrasVitorias=TextoNaArte("MoedasDaPartida",seguinte,new Vector2(957,500),new Vector2(190,50),28);
        var derrota=Painel("Derrota",raiz,"PainelDerrota");ui.derrota=derrota.transform.parent.gameObject;
        Botao("BotaoRejogar",derrota,new Rect(400,878,283,86),mini.IniciarPartida);Botao("BotaoSair",derrota,new Rect(776,878,275,86),mini.Sair);
        ui.avisoEntrada=Texto("AvisoEnergia",desafios.transform,new Vector2(1000,70),new Vector2(0,-465),23);ui.avisoEntrada.gameObject.SetActive(false);
        ui.avisoPartida=Texto("AvisoEnergia",raiz,new Vector2(1400,55),new Vector2(0,-510),23);ui.avisoPartida.gameObject.SetActive(false);
        ui.Atualizar(new int[3],0,0);desafios.minijogo=mini;EditorUtility.SetDirty(desafios);
        // A regra nova substitui a explicacao antiga de Quiz + Minijogo, mantendo retratos e os NPCs.
        var bento=UnityEngine.Object.FindObjectsOfType<NPCInteraction>(true).Single(n=>n.name=="NPC_Desafios_Bento");
        bento.dialogue.lines[3].text="No Minijogo, pegue cinco frutas de cada tipo para ganhar a Chave da Vila.";
        bento.dialogue.lines[4].text="Uma das chaves do Portal?";EditorUtility.SetDirty(bento.dialogue);
        raiz.SetAsLastSibling();mundo.fadePanel.transform.SetAsLastSibling();
        AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(cena);EditorSceneManager.SaveScene(cena);
        Directory.CreateDirectory(Documentacao);File.WriteAllText(Documentacao+"montagem.txt",DateTime.Now.ToString("s"));
    }
}

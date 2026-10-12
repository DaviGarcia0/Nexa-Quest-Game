using System;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using NexaQuest.Brasil;

public static class MontagemQuizAmazonia
{
    public const string Arte = "Assets/Projeto/Sprites/Brasil/QuizAmazonia/";
    public const string Docs = "Documentation/Brasil/QuizAmazonia20261012/";
    static TMP_FontAsset fonte;
    static RectTransform Objeto(string nome, Transform pai, Vector2 tamanho, Vector2 posicao)
    {
        var r = new GameObject(nome, typeof(RectTransform)).GetComponent<RectTransform>();
        r.gameObject.layer = 5; r.SetParent(pai, false); r.sizeDelta = tamanho; r.anchoredPosition = posicao; return r;
    }
    static RectTransform Tela(string nome, Transform pai)
    {
        var r = Objeto(nome, pai, Vector2.zero, Vector2.zero);
        r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one; r.offsetMin = r.offsetMax = Vector2.zero; return r;
    }
    static Image Imagem(string nome, Transform pai, string arquivo, float largura, Vector2 posicao)
    {
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>((arquivo.StartsWith("Feedback") ? "Assets/Projeto/Sprites/Brasil/QuizFeira/" : Arte) + arquivo + ".png");
        var r = Objeto(nome, pai, new Vector2(largura, largura * sprite.rect.height / sprite.rect.width), posicao);
        var imagem = r.gameObject.AddComponent<Image>(); imagem.sprite = sprite; imagem.preserveAspect = true; imagem.raycastTarget = false; return imagem;
    }
    static TMP_Text Texto(string nome, Transform pai, Vector2 tamanho, Vector2 posicao, float altura, Color cor)
    {
        var r = Objeto(nome, pai, tamanho, posicao); var t = r.gameObject.AddComponent<TextMeshProUGUI>();
        t.font = fonte; t.fontSize = altura; t.alignment = TextAlignmentOptions.Center; t.color = cor; t.raycastTarget = false; return t;
    }
    static TMP_Text TextoNaArte(string nome, Image arte, Vector2 centro, Vector2 tamanho, float altura, Color cor)
    {
        var original = arte.sprite.rect.size; var escala = arte.rectTransform.sizeDelta / original;
        return Texto(nome, arte.transform, Vector2.Scale(tamanho, escala), Vector2.Scale(new Vector2(centro.x - original.x / 2, original.y / 2 - centro.y), escala), altura, cor);
    }
    static Button Botao(string nome, Image arte, Rect pixels, UnityEngine.Events.UnityAction acao)
    {
        var original = arte.sprite.rect.size; var escala = arte.rectTransform.sizeDelta / original;
        var r = Objeto(nome, arte.transform, Vector2.Scale(pixels.size, escala), Vector2.Scale(new Vector2(pixels.center.x - original.x / 2, original.y / 2 - pixels.center.y), escala));
        var imagem = r.gameObject.AddComponent<Image>(); imagem.color = Color.clear;
        var b = r.gameObject.AddComponent<Button>(); b.targetGraphic = imagem; b.transition = Selectable.Transition.None;
        b.navigation = new Navigation { mode = Navigation.Mode.None }; if (acao != null) UnityEventTools.AddPersistentListener(b.onClick, acao); return b;
    }
    static Image Painel(string nome, Transform pai, string arquivo, float largura = 1040)
    {
        var raiz = Tela(nome, pai); var arte = Imagem("ArtePainel", raiz, arquivo, largura, new Vector2(0, -95));
        raiz.gameObject.SetActive(false); return arte;
    }
    static CanvasGroup Feedback(string nome, Transform pai, string arquivo)
    {
        var raiz = Tela(nome, pai); var grupo = raiz.gameObject.AddComponent<CanvasGroup>();
        raiz.gameObject.AddComponent<Image>().color = new Color(0, 0, 0, .18f);
        Imagem("ArteFeedback", raiz, arquivo, 850, new Vector2(0, -340));
        grupo.blocksRaycasts = false; raiz.gameObject.SetActive(false); return grupo;
    }
    public static void AjustarTextos()
    {
        var cena=SceneManager.GetActiveScene();if(cena.isDirty)throw new InvalidOperationException("Salve Brasil antes de ajustar.");
        var ui=UnityEngine.Object.FindObjectOfType<InterfaceQuizAmazonia>(true);
        foreach(var texto in ui.alternativas) {
            texto.enableAutoSizing=true;texto.enableWordWrapping=true;texto.fontSizeMin=14;texto.fontSizeMax=20;
            texto.rectTransform.offsetMin=new Vector2(18,12);texto.rectTransform.offsetMax=new Vector2(-18,-12);
            EditorUtility.SetDirty(texto);EditorUtility.SetDirty(texto.rectTransform);
        }
        EditorSceneManager.MarkSceneDirty(cena);EditorSceneManager.SaveScene(cena);
    }
    public static void AjustarExplicacao()
    {
        var cena=SceneManager.GetActiveScene();if(cena.isDirty)throw new InvalidOperationException("Salve Brasil antes de ajustar.");
        var ui=UnityEngine.Object.FindObjectOfType<InterfaceQuizAmazonia>(true);
        var texto=ui.painelInicial.transform.Find("ArtePainel/TextoExplicativo").GetComponent<TMP_Text>();
        texto.fontSizeMax=24;texto.text=texto.text.Replace("\n\nComeçar", "\nComeçar");
        EditorUtility.SetDirty(texto);EditorSceneManager.MarkSceneDirty(cena);EditorSceneManager.SaveScene(cena);
    }
    public static void IntegrarPaineis()
    {
        var cena=SceneManager.GetActiveScene();
        if(cena.isDirty || cena.path!="Assets/Projeto/Scenes/Brasil.unity")throw new InvalidOperationException("Brasil precisa estar salva.");
        var quiz=UnityEngine.Object.FindObjectOfType<QuizAmazonia>(true);var ui=quiz.interfaceQuiz;
        if(ui.painelInicial!=null || ui.painelFinal!=null)throw new InvalidOperationException("Painéis já integrados; não duplicar.");
        var feira=UnityEngine.Object.FindObjectOfType<InterfaceQuizFeira>(true);
        foreach(var nome in new[]{"PainelInicialQuizAmazonia","PainelFinalQuizAmazonia"}) {
            var i=(TextureImporter)AssetImporter.GetAtPath(Arte+nome+".png");
            i.textureType=TextureImporterType.Sprite;i.spriteImportMode=SpriteImportMode.Single;i.filterMode=FilterMode.Point;
            i.textureCompression=TextureImporterCompression.Uncompressed;i.mipmapEnabled=false;i.npotScale=TextureImporterNPOTScale.None;i.maxTextureSize=4096;i.alphaIsTransparency=true;
            var ajustes=new TextureImporterSettings();i.ReadTextureSettings(ajustes);ajustes.spriteMeshType=SpriteMeshType.FullRect;ajustes.spriteGenerateFallbackPhysicsShape=false;i.SetTextureSettings(ajustes);i.SaveAndReimport();
        }
        fonte=UnityEngine.Object.FindObjectOfType<DialogueController>().speakerName.font;
        var referenciaInicial=feira.painelInicial.transform.Find("ArtePainel").GetComponent<Image>();
        var inicial=Painel("PainelInicial",quiz.transform,"PainelInicialQuizAmazonia",referenciaInicial.rectTransform.sizeDelta.x);
        inicial.rectTransform.anchoredPosition=referenciaInicial.rectTransform.anchoredPosition;
        inicial.rectTransform.anchorMin=referenciaInicial.rectTransform.anchorMin;inicial.rectTransform.anchorMax=referenciaInicial.rectTransform.anchorMax;inicial.rectTransform.pivot=referenciaInicial.rectTransform.pivot;
        inicial.transform.localScale=referenciaInicial.transform.localScale;ui.painelInicial=inicial.transform.parent.gameObject;
        var explicacao=TextoNaArte("TextoExplicativo",inicial,new Vector2(725,538),new Vector2(900,335),24,new Color(.19f,.075f,.035f));
        explicacao.enableWordWrapping=true;explicacao.enableAutoSizing=true;explicacao.fontSizeMin=22;explicacao.fontSizeMax=24;
        explicacao.text="Responda 3 perguntas sobre a Amazônia!\n\nVocê terá 30 segundos para acertar cada pergunta.\nCada resposta correta vale 10 NexaCoins.\n\nComplete o Quiz para receber 50 de Conhecimento\ne conquistar a Chave da Amazônia.\nComeçar o Quiz custa 20 de Energia.";
        Botao("BotaoComecar",inicial,new Rect(285,805,445,128),quiz.Comecar);
        if(ui.botaoVoltar!=null)UnityEngine.Object.DestroyImmediate(ui.botaoVoltar.gameObject);
        ui.botaoVoltar=Botao("BotaoVoltar",inicial,new Rect(787,805,373,128),quiz.Voltar);
        var referenciaFinal=feira.painelFinal.transform.Find("ArtePainel").GetComponent<Image>();
        var final=Painel("PainelFinal",quiz.transform,"PainelFinalQuizAmazonia",referenciaFinal.rectTransform.sizeDelta.x);
        final.rectTransform.anchoredPosition=referenciaFinal.rectTransform.anchoredPosition;final.rectTransform.anchorMin=referenciaFinal.rectTransform.anchorMin;final.rectTransform.anchorMax=referenciaFinal.rectTransform.anchorMax;final.rectTransform.pivot=referenciaFinal.rectTransform.pivot;
        final.transform.localScale=referenciaFinal.transform.localScale;ui.painelFinal=final.transform.parent.gameObject;
        ui.botaoContinuar=Botao("BotaoContinuar",final,new Rect(770,773,600,83),quiz.Voltar);
        var referenciaResumo=feira.recompensas;
        ui.recompensas=Texto("RecompensasReais",ui.painelFinal.transform,referenciaResumo.rectTransform.sizeDelta,referenciaResumo.rectTransform.anchoredPosition,referenciaResumo.fontSize,referenciaResumo.color);
        ui.recompensas.alignment=referenciaResumo.alignment;
        ui.recompensas.rectTransform.anchorMin=referenciaResumo.rectTransform.anchorMin;ui.recompensas.rectTransform.anchorMax=referenciaResumo.rectTransform.anchorMax;
        EditorUtility.SetDirty(ui);AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(cena);EditorSceneManager.SaveScene(cena);
        const string documentos="Documentation/Brasil/PaineisQuizAmazonia20261012/";Directory.CreateDirectory(documentos);
        File.WriteAllText(documentos+"montagem.txt","Painéis integrados: "+DateTime.Now.ToString("s")+"\nReferências da Feira: Inicial "+referenciaInicial.rectTransform.sizeDelta+" em "+referenciaInicial.rectTransform.anchoredPosition+"; Final "+referenciaFinal.rectTransform.sizeDelta+" em "+referenciaFinal.rectTransform.anchoredPosition);
    }
    public static void Montar()
    {
        var cena=SceneManager.GetActiveScene();
        if(cena.path!="Assets/Projeto/Scenes/Brasil.unity" || cena.isDirty) throw new InvalidOperationException("Brasil precisa estar salva.");
        var mundo=UnityEngine.Object.FindObjectOfType<BrazilWorld>();var canvas=mundo.fadePanel.GetComponentInParent<Canvas>().transform;
        if(canvas.Find("QuizAmazonia")!=null)throw new InvalidOperationException("Quiz da Amazônia já montado.");
        foreach(var arquivo in Directory.GetFiles(Arte,"*.png")) {
            var i=(TextureImporter)AssetImporter.GetAtPath(arquivo.Replace('\\','/'));
            i.textureType=TextureImporterType.Sprite;i.spriteImportMode=SpriteImportMode.Single;i.filterMode=FilterMode.Point;
            i.textureCompression=TextureImporterCompression.Uncompressed;i.mipmapEnabled=false;i.npotScale=TextureImporterNPOTScale.None;i.maxTextureSize=4096;i.alphaIsTransparency=true;
            var opcoes=new TextureImporterSettings();i.ReadTextureSettings(opcoes);opcoes.spriteMeshType=SpriteMeshType.FullRect;opcoes.spriteGenerateFallbackPhysicsShape=false;i.SetTextureSettings(opcoes);i.SaveAndReimport();
        }
        fonte=UnityEngine.Object.FindObjectOfType<DialogueController>().speakerName.font;
        var dados=ScriptableObject.CreateInstance<DadosQuizAmazonia>();
        dados.perguntas=new[]{
            new PerguntaQuizAmazonia {enunciado="O que é a Amazônia Legal?",alternativas=new[]{"Somente a parte da floresta que fica no estado do Amazonas.","Uma região definida por lei para planejamento e políticas públicas de desenvolvimento da Amazônia brasileira.","Uma unidade de conservação onde nenhuma pessoa pode viver.","Outro nome dado apenas ao Bioma Amazônia."},respostaCorreta=1,fonte="https://www.ibge.gov.br/geociencias/organizacao-do-territorio/estrutura-territorial/15819-amazonia-legal.html"},
            new PerguntaQuizAmazonia {enunciado="Qual processo das árvores amazônicas ajuda a devolver água para a atmosfera e alimentar os chamados 'rios voadores'?",alternativas=new[]{"Sedimentação","Evapotranspiração","Erosão","Fossilização"},respostaCorreta=1,fonte="https://www.gov.br/dnit/pt-br/assuntos/portais-tematicos/br-319-am-ro/noticias/rios-voadores-mantem-o-clima-e-abastecem-o-brasil"},
            new PerguntaQuizAmazonia {enunciado="Por que o desmatamento da Amazônia pode alterar o regime de chuvas?",alternativas=new[]{"Porque aumenta permanentemente a quantidade de água liberada pelas árvores.","Porque transforma os rios da floresta em água salgada.","Porque reduz a vegetação que recicla água para a atmosfera, afetando o transporte de umidade e as chuvas.","Porque impede completamente a entrada de ventos do Oceano Atlântico."},respostaCorreta=2,fonte="https://www.gov.br/dnit/pt-br/assuntos/portais-tematicos/br-319-am-ro/noticias/rios-voadores-mantem-o-clima-e-abastecem-o-brasil"}
        };
        AssetDatabase.CreateAsset(dados,"Assets/Projeto/Dados/PerguntasQuizAmazonia.asset");
        var desafioRaiz=Tela("PainelDesafiosAmazonia",canvas);desafioRaiz.gameObject.SetActive(false);
        desafioRaiz.gameObject.AddComponent<Image>().color=new Color(0,0,0,.55f);
        var desafios=desafioRaiz.gameObject.AddComponent<PainelDesafiosAmazonia>();desafios.mundo=mundo;desafios.area=mundo.areas.Single(a=>a.name=="Amazonia");
        desafios.avisoEnergia=UnityEngine.Object.FindObjectOfType<MinijogoFeira>(true).interfaceJogo.avisoEntrada;
        var arteDesafios=Imagem("ArtePainel",desafioRaiz,"PainelDesafiosAmazonia",1040,new Vector2(0,-95));
        Botao("BotaoQuiz",arteDesafios,new Rect(262,592,440,204),desafios.SelecionarQuiz);
        Botao("BotaoMinijogo",arteDesafios,new Rect(751,592,440,204),desafios.SelecionarMinijogo);
        desafios.mensagem=Texto("MensagemProvisoria",desafioRaiz,new Vector2(1400,50),new Vector2(0,-480),22,Color.white);
        var raiz=Tela("QuizAmazonia",canvas);raiz.gameObject.SetActive(false);raiz.gameObject.AddComponent<Image>().color=new Color(0,0,0,.55f);
        var quiz=raiz.gameObject.AddComponent<QuizAmazonia>();quiz.mundo=mundo;quiz.dados=dados;desafios.quiz=quiz;
        var ui=raiz.gameObject.AddComponent<InterfaceQuizAmazonia>();quiz.interfaceQuiz=ui;
        ui.camadaHUD=Tela("HUDsExistentes",raiz);ui.hudsCompartilhadas=new[]{canvas.Find("EnergyHUD"),UnityEngine.Object.FindObjectOfType<ProgressionHUD>().transform}.OrderBy(t=>t.GetSiblingIndex()).ToArray();ui.avisoEnergia=desafios.avisoEnergia;
        var principal=Painel("PainelPrincipal",raiz,"PainelPrincipalQuizAmazonia");ui.painelPrincipal=principal.transform.parent.gameObject;
        var marrom=new Color(.19f,.075f,.035f);
        ui.progressoPerguntas=TextoNaArte("ProgressoPerguntas",principal,new Vector2(621,332),new Vector2(100,55),27,Color.white);
        ui.tempo=TextoNaArte("TempoRestante",principal,new Vector2(963,332),new Vector2(140,60),28,Color.white);
        ui.enunciado=TextoNaArte("Enunciado",principal,new Vector2(570,501),new Vector2(790,215),26,marrom);ui.enunciado.enableWordWrapping=true;
        var retrato=Objeto("RetratoQuest",principal.transform,new Vector2(220,235)*(1040f/1448),new Vector2((1192-724)*(1040f/1448),(543-495)*(1040f/1448)));
        ui.retrato=retrato.gameObject.AddComponent<Image>();ui.retrato.preserveAspect=true;ui.retrato.raycastTarget=false;
        const string compartilhada="Assets/Projeto/Sprites/Brasil/QuizFeira/";
        ui.questPensando=AssetDatabase.LoadAssetAtPath<Sprite>(compartilhada+"QuestPensando.png");ui.questTriste=AssetDatabase.LoadAssetAtPath<Sprite>(compartilhada+"QuestTriste.png");ui.questFeliz=AssetDatabase.LoadAssetAtPath<Sprite>(compartilhada+"QuestFeliz.png");ui.retrato.sprite=ui.questPensando;
        ui.botoesRespostas=new Button[4];ui.alternativas=new TMP_Text[4];
        var regioes=new[]{new Rect(140,651,573,106),new Rect(739,651,573,106),new Rect(140,765,573,106),new Rect(739,765,573,106)};
        for(int i=0;i<4;i++) {
            var b=Botao("Resposta"+(char)('A'+i),principal,regioes[i],null);ui.botoesRespostas[i]=b;UnityEventTools.AddIntPersistentListener(b.onClick,quiz.Responder,i);
            var texto=Texto("TextoAlternativa",b.transform,Vector2.zero,Vector2.zero,20,marrom);texto.enableWordWrapping=true;texto.enableAutoSizing=true;texto.fontSizeMin=14;texto.fontSizeMax=20;
            texto.rectTransform.anchorMin=Vector2.zero;texto.rectTransform.anchorMax=Vector2.one;texto.rectTransform.offsetMin=new Vector2(18,12);texto.rectTransform.offsetMax=new Vector2(-18,-12);ui.alternativas[i]=texto;
        }
        ui.status=TextoNaArte("MensagemStatus",principal,new Vector2(726,946),new Vector2(735,90),19,Color.white);
        ui.botaoVoltar=Botao("BotaoVoltar",principal,new Rect(345,894,759,112),quiz.Voltar);ui.botaoVoltar.gameObject.SetActive(false);
        ui.feedbackErro=Feedback("FeedbackErro",raiz,"FeedbackErro");ui.feedbackAcerto=Feedback("FeedbackAcerto",raiz,"FeedbackAcerto");
        var npc=desafios.area.transform.Find("NPCsAmazonia/NPC_Amazonia_01").GetComponent<NPCInteraction>();
        npc.dialogue.lines[npc.dialogue.lines.Length-1].text="Posso te desafiar de duas formas.";EditorUtility.SetDirty(npc.dialogue);
        UnityEventTools.AddPersistentListener(npc.onDialogueFinished,desafios.Abrir);EditorUtility.SetDirty(npc);
        mundo.fadePanel.transform.SetAsLastSibling();AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(cena);EditorSceneManager.SaveScene(cena);
        Directory.CreateDirectory(Docs);File.WriteAllText(Docs+"montagem.txt","Quiz da Amazônia montado: "+DateTime.Now.ToString("s"));
    }
}

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

public static class MontagemQuizFeira
{
    public const string Arte = "Assets/Projeto/Sprites/Brasil/QuizFeira/";
    public const string Docs = "Documentation/Brasil/QuizFeira20261009/";
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
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(Arte + arquivo + ".png");
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
    public static void Montar()
    {
        var cena = SceneManager.GetActiveScene();
        if (cena.path != "Assets/Projeto/Scenes/Brasil.unity" || cena.isDirty) throw new InvalidOperationException("Brasil precisa estar salva.");
        var canvas = GameObject.Find("Canvas").transform;
        if (canvas.Find("QuizFeira") != null) throw new InvalidOperationException("Quiz já montado; não duplicar.");
        foreach (var arquivo in Directory.GetFiles(Arte, "*.png"))
        {
            var i = (TextureImporter)AssetImporter.GetAtPath(arquivo.Replace('\\', '/'));
            i.textureType = TextureImporterType.Sprite; i.spriteImportMode = SpriteImportMode.Single;
            i.filterMode = FilterMode.Point; i.textureCompression = TextureImporterCompression.Uncompressed;
            i.mipmapEnabled = false; i.alphaIsTransparency = true; i.maxTextureSize = 4096; i.npotScale = TextureImporterNPOTScale.None;
            var opcoes = new TextureImporterSettings(); i.ReadTextureSettings(opcoes); opcoes.spriteGenerateFallbackPhysicsShape = false;
            opcoes.spriteMeshType = SpriteMeshType.FullRect; i.SetTextureSettings(opcoes); i.SaveAndReimport();
        }
        var mundo = UnityEngine.Object.FindObjectOfType<BrazilWorld>();
        var desafios = UnityEngine.Object.FindObjectOfType<PainelDesafiosFeira>(true);
        fonte = UnityEngine.Object.FindObjectOfType<DialogueController>().speakerName.font;
        const string pastaDados = "Assets/Projeto/Dados/";
        Directory.CreateDirectory(pastaDados); AssetDatabase.Refresh();
        var dados = ScriptableObject.CreateInstance<DadosQuizFeira>();
        dados.perguntas = new[] {
            new PerguntaQuizFeira { enunciado = "Qual é a capital do Brasil?", alternativas = new[]{"Rio de Janeiro", "Brasília", "Salvador", "São Paulo"}, respostaCorreta = 1, fonte = "https://www.gov.br/g20/pt-br/sobre-o-g20/cidades-sede/brasilia-df" },
            new PerguntaQuizFeira { enunciado = "Qual destes alimentos vendidos na feira é uma fruta?", alternativas = new[]{"Alface", "Batata", "Banana", "Cenoura"}, respostaCorreta = 2, fonte = "https://www.embrapa.br/web/mandioca-e-fruticultura/cultivos/banana" },
            new PerguntaQuizFeira { enunciado = "Qual é a moeda oficial usada nas compras em uma feira brasileira?", alternativas = new[]{"Real", "Euro", "Dólar", "Peso"}, respostaCorreta = 0, fonte = "https://www.bcb.gov.br/controleinflacao/planoreal" }
        };
        AssetDatabase.CreateAsset(dados, pastaDados + "PerguntasQuizFeira.asset");
        var raiz = Tela("QuizFeira", canvas); raiz.gameObject.SetActive(false);
        raiz.gameObject.AddComponent<Image>().color = new Color(0, 0, 0, .55f);
        var quiz = raiz.gameObject.AddComponent<QuizFeira>(); quiz.mundo = mundo; quiz.dados = dados;
        var ui = raiz.gameObject.AddComponent<InterfaceQuizFeira>(); quiz.interfaceQuiz = ui;
        ui.camadaHUD = Tela("HUDsExistentes", raiz);
        ui.hudsCompartilhadas = new[]{canvas.Find("EnergyHUD"), UnityEngine.Object.FindObjectOfType<ProgressionHUD>().transform}.OrderBy(t => t.GetSiblingIndex()).ToArray();
        // O mesmo aviso de Energia usado pelo painel/minijogo é emprestado durante o Quiz.
        ui.avisoEnergia = desafios.minijogo.interfaceJogo.avisoEntrada;
        var inicial = Painel("PainelInicial", raiz, "PainelInicialQuiz"); ui.painelInicial = inicial.transform.parent.gameObject;
        var marrom = new Color(.19f, .075f, .035f);
        var explicacao = TextoNaArte("ExplicacaoQuiz", inicial, new Vector2(726, 553), new Vector2(910, 330), 26, marrom);
        explicacao.text = "Responda 3 perguntas sobre o Brasil e a Feira!\n\nVocê terá 30 segundos para acertar cada pergunta.\nCada resposta correta vale 10 NexaCoins.\nComplete as 3 para ganhar 50 de Conhecimento.\n\nComeçar o Quiz custa 20 de Energia.";
        Botao("BotaoComecar", inicial, new Rect(280, 828, 445, 126), quiz.Comecar);
        Botao("BotaoVoltar", inicial, new Rect(782, 828, 374, 126), quiz.Voltar);
        var principal = Painel("PainelPrincipal", raiz, "PainelPrincipalQuiz"); ui.painelPrincipal = principal.transform.parent.gameObject;
        ui.progressoPerguntas = TextoNaArte("ProgressoPerguntas", principal, new Vector2(619, 304), new Vector2(100, 55), 27, Color.white);
        ui.tempo = TextoNaArte("TempoRestante", principal, new Vector2(952, 306), new Vector2(140, 60), 28, Color.white);
        ui.enunciado = TextoNaArte("Enunciado", principal, new Vector2(566, 475), new Vector2(790, 210), 28, marrom);
        var retrato = Objeto("RetratoQuest", principal.transform, new Vector2(220, 235) * (1040f / 1448), new Vector2((1194 - 724) * (1040f / 1448), (543 - 482) * (1040f / 1448)));
        ui.retrato = retrato.gameObject.AddComponent<Image>(); ui.retrato.preserveAspect = true; ui.retrato.raycastTarget = false;
        ui.questPensando = AssetDatabase.LoadAssetAtPath<Sprite>(Arte + "QuestPensando.png");
        ui.questTriste = AssetDatabase.LoadAssetAtPath<Sprite>(Arte + "QuestTriste.png");
        ui.questFeliz = AssetDatabase.LoadAssetAtPath<Sprite>(Arte + "QuestFeliz.png"); ui.retrato.sprite = ui.questPensando;
        ui.botoesRespostas = new Button[4]; ui.alternativas = new TMP_Text[4];
        var regioes = new[]{ new Rect(139, 642, 576, 114), new Rect(739, 642, 576, 114), new Rect(139, 768, 576, 114), new Rect(739, 768, 576, 114) };
        for (int i = 0; i < 4; i++)
        {
            var botao = Botao("Resposta" + (char)('A' + i), principal, regioes[i], null); ui.botoesRespostas[i] = botao;
            UnityEventTools.AddIntPersistentListener(botao.onClick, quiz.Responder, i);
            var texto = Texto("TextoAlternativa", botao.transform, Vector2.zero, Vector2.zero, 25, marrom);
            texto.rectTransform.anchorMin = Vector2.zero; texto.rectTransform.anchorMax = Vector2.one;
            texto.rectTransform.offsetMin = new Vector2(22, 8); texto.rectTransform.offsetMax = new Vector2(-22, -8); ui.alternativas[i] = texto;
        }
        ui.status = TextoNaArte("MensagemStatus", principal, new Vector2(726, 968), new Vector2(735, 90), 19, Color.white);
        ui.feedbackErro = Feedback("FeedbackErro", raiz, "FeedbackErro"); ui.feedbackAcerto = Feedback("FeedbackAcerto", raiz, "FeedbackAcerto");
        var final = Painel("PainelFinal", raiz, "PainelFinalQuiz", 1340); ui.painelFinal = final.transform.parent.gameObject;
        Botao("BotaoContinuar", final, new Rect(782, 780, 581, 88), quiz.Voltar);
        ui.recompensas = Texto("RecompensasReais", ui.painelFinal.transform, new Vector2(1500, 50), new Vector2(0, -490), 24, Color.white);
        desafios.quiz = quiz; EditorUtility.SetDirty(desafios);
        raiz.SetAsLastSibling(); mundo.fadePanel.transform.SetAsLastSibling();
        AssetDatabase.SaveAssets(); EditorSceneManager.MarkSceneDirty(cena); EditorSceneManager.SaveScene(cena);
        Directory.CreateDirectory(Docs); File.WriteAllText(Docs + "montagem.txt", "Quiz montado: " + DateTime.Now.ToString("s"));
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using NexaQuest;
using NexaQuest.Brasil;
using NexaQuest.MinhaQuest;
[InitializeOnLoad] public static class MinhaQuestValidation
{
    const BindingFlags F=BindingFlags.Static|BindingFlags.Public|BindingFlags.NonPublic;
    [Serializable] class Setup { public SceneSetup[] scenes; }
    static MinhaQuestValidation()
    {
        EditorApplication.playModeStateChanged+=s=>{
            if(s==PlayModeStateChange.EnteredPlayMode&&SessionState.GetBool("MQ.Start",false)){SessionState.SetBool("MQ.Start",false);new GameObject("TestesMinhaQuest").AddComponent<MinhaQuestChecks>();}
            if(s!=PlayModeStateChange.EnteredEditMode||!SessionState.GetBool("MQ.End",false))return;
            SessionState.SetBool("MQ.End",false);SessionState.SetBool("NexaQuest.ValidandoQuiz",false);
            if(SessionState.GetInt("MQ.Phase",1)==1&&SessionState.GetBool("MQ.Pass",false)){SessionState.SetInt("MQ.Phase",2);EditorApplication.delayCall+=Start;return;}
            bool same=SessionState.GetString("MQ.Real","")==PlayerPrefs.GetString(ProgressionManager.ChavePersistencia,"")&&SessionState.GetBool("MQ.Exists",false)==PlayerPrefs.HasKey(ProgressionManager.ChavePersistencia);
            File.WriteAllText(MinhaQuestBridge.Docs+"save-real.txt","Progresso real preservado: "+same);
            var setup=JsonUtility.FromJson<Setup>(SessionState.GetString("MQ.Setup",""));EditorSceneManager.RestoreSceneManagerSetup(setup.scenes);
            typeof(QuestHUDVisualValidation).GetMethod("RestoreGameView",F).Invoke(null,null);
            object[] c={0,0,0};typeof(Editor).Assembly.GetType("UnityEditor.LogEntries").GetMethod("GetCountsByType",F).Invoke(null,c);
            File.WriteAllText(MinhaQuestBridge.Docs+"console.txt","Erros: "+c[0]+"\nAvisos: "+c[1]+"\nMensagens: "+c[2]);
            File.WriteAllText(MinhaQuestBridge.Docs+"editor-final.txt",SceneManager.GetActiveScene().path+"\nDirty: "+SceneManager.GetActiveScene().isDirty+"\nPlayMode: "+EditorApplication.isPlaying);
        };
    }
    public static void Run()
    {
        if(EditorSceneManager.GetSceneManagerSetup().Any(s=>SceneManager.GetSceneByPath(s.path).isDirty))throw new InvalidOperationException("Salve as cenas antes de testar.");
        SessionState.SetString("MQ.Setup",JsonUtility.ToJson(new Setup{scenes=EditorSceneManager.GetSceneManagerSetup()}));
        SessionState.SetBool("MQ.Exists",PlayerPrefs.HasKey(ProgressionManager.ChavePersistencia));SessionState.SetString("MQ.Real",PlayerPrefs.GetString(ProgressionManager.ChavePersistencia,""));
        SessionState.SetInt("MQ.Phase",1);PlayerPrefs.SetString("NexaQuest.Testes.Quiz","{\"versao\":1,\"coins\":200,\"energia\":20,\"level\":3,\"currentXP\":37,\"possuiChaveVila\":true,\"quizFeiraConcluido\":true,\"quizAmazoniaConcluido\":true,\"possuiChaveAmazonia\":true,\"minijogoAmazoniaConcluido\":true,\"instrucoesFeiraVistas\":true}");PlayerPrefs.Save();
        typeof(QuestHUDVisualValidation).GetMethod("SetGameView",F).Invoke(null,null);typeof(Editor).Assembly.GetType("UnityEditor.LogEntries").GetMethod("Clear",F).Invoke(null,null);Start();
    }
    static void Start()
    {
        EditorSceneManager.OpenScene("Assets/Projeto/Scenes/MinhaQuest.unity");
        SessionState.SetBool("NexaQuest.ValidandoQuiz",true);SessionState.SetBool("MQ.Start",true);SessionState.SetBool("MQ.End",true);EditorApplication.isPlaying=true;
    }
}
public class MinhaQuestChecks:MonoBehaviour
{
    readonly List<string> lines=new List<string>();int failures;MinhaQuestController c;ProgressionManager p;
    string Docs=>MinhaQuestBridge.Docs;
    void C(bool ok,string name){lines.Add((ok?"PASS: ":"FAIL: ")+name);if(!ok)failures++;File.WriteAllLines(Docs+"andamento.txt",lines);}
    void Log(string message,string trace,LogType type){if(type==LogType.Exception||type==LogType.Error||type==LogType.Assert){C(false,type+": "+message);File.WriteAllLines(Docs+"testes.txt",lines);SessionState.SetBool("MQ.Pass",false);EditorApplication.delayCall+=()=>EditorApplication.isPlaying=false;}}
    Button B(Transform root,string name)=>root.GetComponentsInChildren<Button>(true).Single(b=>b.name==name);
    IEnumerator Click(Button b)
    {
        yield return null;Canvas.ForceUpdateCanvases();
        var data=new PointerEventData(EventSystem.current){position=b.transform.position,button=PointerEventData.InputButton.Left};
        var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(data,hits);C(hits.Count>0&&hits[0].gameObject==b.gameObject,"Raycast real: "+b.name);
        if(hits.Count>0){ExecuteEvents.Execute(hits[0].gameObject,data,ExecuteEvents.pointerDownHandler);ExecuteEvents.Execute(hits[0].gameObject,data,ExecuteEvents.pointerUpHandler);ExecuteEvents.Execute(hits[0].gameObject,data,ExecuteEvents.pointerClickHandler);}yield return null;
    }
    IEnumerator Photo(string name){yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Docs+name+".png");yield return null;}
    IEnumerator Ready(string name){float deadline=Time.unscaledTime+20;while(SceneManager.GetActiveScene().name!=name&&Time.unscaledTime<deadline)yield return null;yield return null;yield return null;C(SceneManager.GetActiveScene().name==name,"Cena: "+name);c=FindObjectOfType<MinhaQuestController>();}
    PointerEventData Pointer(Vector2 pos)=>new PointerEventData(EventSystem.current){position=pos,button=PointerEventData.InputButton.Left};
    IEnumerator Drag(FoodType type,bool deliver,bool checkNear=false)
    {
        yield return null;var food=c.foods[(int)type];var originalParent=food.transform.parent;var position=((RectTransform)food.transform).anchoredPosition;
        var data=Pointer(food.transform.position);ExecuteEvents.Execute(food.gameObject,data,ExecuteEvents.pointerDownHandler);ExecuteEvents.Execute(food.gameObject,data,ExecuteEvents.beginDragHandler);
        C(food.Dragging&&food.transform.parent==c.dragLayer,"Arraste real começa: "+type);
        data.position=c.feeding.mouth.position;ExecuteEvents.Execute(food.gameObject,data,ExecuteEvents.dragHandler);yield return null;
        C(c.feeding.MouthOpen,"Aproximar comida abre boca: "+type);
        if(checkNear)
        {
            int qty=p.GetFoodCount(type),energy=p.Energia;
            data.position=(Vector2)c.feeding.mouth.position+new Vector2(-50,50);ExecuteEvents.Execute(food.gameObject,data,ExecuteEvents.dragHandler);yield return new WaitForSecondsRealtime(.65f);C(c.feeding.animator.GetCurrentAnimatorStateInfo(0).IsName("MouthOpen"),"Animator executa MouthOpen");yield return Photo("BocaAberta");C(p.GetFoodCount(type)==qty&&p.Energia==energy,"Aproximar não consome");
            data.position=new Vector2(1800,1000);ExecuteEvents.Execute(food.gameObject,data,ExecuteEvents.dragHandler);C(!c.feeding.MouthOpen,"Afastar retorna Idle");
            data.position=c.feeding.mouth.position;ExecuteEvents.Execute(food.gameObject,data,ExecuteEvents.dragHandler);
        }
        if(!deliver){data.position=new Vector2(1800,1000);ExecuteEvents.Execute(food.gameObject,data,ExecuteEvents.dragHandler);}
        ExecuteEvents.Execute(food.gameObject,data,ExecuteEvents.endDragHandler);
        C(!food.Dragging&&food.transform.parent==originalParent&&((RectTransform)food.transform).anchoredPosition==position,"Alimento retorna ao slot sem clone: "+type);yield return null;
    }
    IEnumerator EatingDone(){yield return new WaitForSecondsRealtime(1.85f);C(!c.feeding.Eating,"Comer termina e volta ao Idle");}
    void Invariants(){C(p.Level==3&&p.CurrentXP==37&&p.PossuiChaveVila&&p.QuizFeiraConcluido&&p.QuizAmazoniaConcluido&&p.PossuiChaveAmazonia&&p.MinijogoAmazoniaConcluido&&p.InstrucoesFeiraVistas,"XP chaves e flags antigas preservadas");}
    IEnumerator Start()
    {
        DontDestroyOnLoad(gameObject);Application.runInBackground=true;Application.logMessageReceived+=Log;yield return null;yield return null;c=FindObjectOfType<MinhaQuestController>();p=ProgressionManager.Instance;
        if(SessionState.GetInt("MQ.Phase",1)==2)
        {
            lines.AddRange(File.ReadAllLines(Docs+"testes.txt"));C(PlayerPrefs.GetString("NexaQuest.Testes.Quiz")==SessionState.GetString("MQ.Expected",""),"Save persiste em novo Play Mode");
            C(p.Energia==SessionState.GetInt("MQ.Energy",-1)&&p.Coins==SessionState.GetInt("MQ.Coins",-1),"Moedas e Energia recarregadas sem override temporário");
            C(p.GetFoodCount(FoodType.Apple)==SessionState.GetInt("MQ.Apple",-1)&&p.GetFoodCount(FoodType.Sandwich)==SessionState.GetInt("MQ.Sandwich",-1)&&p.GetFoodCount(FoodType.NexaKing)==SessionState.GetInt("MQ.King",-1),"Três estoques recarregados em novo Play");Invariants();Finish();yield break;
        }
        C(Screen.width==1920&&Screen.height==1080,"Play Mode 1920x1080");C(c!=null&&p!=null,"MinhaQuest usa ProgressionManager existente");C(p.Coins==200&&p.Energia==20,"Save v1 antigo carrega moedas e Energia");Invariants();
        C(p.GetFoodCount(FoodType.Apple)==0&&p.GetFoodCount(FoodType.Sandwich)==0&&p.GetFoodCount(FoodType.NexaKing)==0,"Migração de save antigo inicia comida em zero");
        C(c.foods.All(f=>!f.gameObject.activeSelf),"Bancada vazia sem estoque");C(c.energyText.text=="20 / 100"&&Mathf.Abs(c.energyFill.fillAmount-.2f)<.001f&&Mathf.Abs(c.energyFill.rectTransform.rect.width/c.energyFill.rectTransform.parent.GetComponent<RectTransform>().rect.width-.2f)<.001f,"HUD numérico e preenchimento refletem Energia");
        C(FindObjectsOfType<EventSystem>().Length==1,"Um EventSystem");C(c.feeding.animator.GetCurrentAnimatorStateInfo(0).IsName("Idle")&&c.feeding.animator.GetCurrentAnimatorStateInfo(0).loop,"Torso inicia em Idle com loop");
        C(MinhaQuestNavigation.ReturnScene=="Menu","Abertura direta usa fallback seguro");yield return Photo("PrincipalInicial");
        yield return Click(B(c.transform,"ButtonMercado"));C(c.market.activeSelf&&!c.mainControls.blocksRaycasts,"Mercado bloqueia fundo");
        var behind=Pointer(B(c.transform,"ButtonLoja").transform.position);var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(behind,hits);C(hits.Count>0&&hits[0].gameObject.name!="ButtonLoja","Overlay impede clique atravessado");
        for(int i=0;i<3;i++){int coins=p.Coins;yield return Click(B(c.market.transform,"ButtonApple"));C(p.Coins==coins-5&&p.GetFoodCount(FoodType.Apple)==i+1&&c.popupArt.sprite==c.purchaseArt,"Compra maçã "+(i+1));c.BuyApple();C(p.Coins==coins-5,"Popup impede compra duplicada");yield return Click(B(c.popupLayer.transform,i%2==0?"ButtonOK":"ButtonX"));C(c.market.activeSelf&&!c.popupLayer.activeSelf,"Compra fecha e mantém Mercado");}
        yield return Click(B(c.market.transform,"ButtonSandwich"));C(p.Coins==170&&p.GetFoodCount(FoodType.Sandwich)==1,"Sanduíche custa 15");yield return Click(B(c.popupLayer.transform,"ButtonX"));
        yield return Click(B(c.market.transform,"ButtonNexaKing"));C(p.Coins==130&&p.GetFoodCount(FoodType.NexaKing)==1,"Nexa King custa 40");yield return Photo("CompraRealizada");yield return Click(B(c.popupLayer.transform,"ButtonOK"));yield return Photo("Mercado");
        yield return Click(B(c.market.transform,"ButtonCloseMarket"));C(c.foods.All(f=>f.gameObject.activeSelf)&&FindObjectsOfType<FoodDragHandler>().Length==3,"Um sprite por tipo apesar de três maçãs");yield return Photo("BancadaComAlimentos");
        int stock=p.GetFoodCount(FoodType.Apple);yield return Drag(FoodType.Apple,false,true);C(p.GetFoodCount(FoodType.Apple)==stock&&p.Energia==20,"Soltar fora não altera inventário ou Energia");
        yield return Drag(FoodType.Apple,true);C(p.GetFoodCount(FoodType.Apple)==2&&p.Energia==30&&c.foods[0].gameObject.activeSelf,"Maçã 3 para 2 e Energia mais 10");C(c.feeding.Eating,"Animação comer começa");var firstEating=c.feeding.animator.GetComponent<Image>().sprite;yield return new WaitForSecondsRealtime(.35f);C(c.feeding.animator.GetCurrentAnimatorStateInfo(0).IsName("Eating")&&c.feeding.animator.GetComponent<Image>().sprite!=firstEating,"Sprites da animação comer avançam");yield return Photo("Comendo");
        var pause=PauseSettingsManager.Instance;pause.Open();yield return null;float animation=c.feeding.animator.GetCurrentAnimatorStateInfo(0).normalizedTime;yield return new WaitForSecondsRealtime(.35f);C(c.feeding.Eating&&Mathf.Abs(c.feeding.animator.GetCurrentAnimatorStateInfo(0).normalizedTime-animation)<.01f,"Pausa congela animação e tempo de alimentação");pause.Close();yield return null;yield return EatingDone();
        yield return Drag(FoodType.Apple,true);C(p.GetFoodCount(FoodType.Apple)==1&&c.foods[0].gameObject.activeSelf,"Estoque restante mantém uma maçã");yield return EatingDone();
        yield return Drag(FoodType.Apple,true);C(p.GetFoodCount(FoodType.Apple)==0&&!c.foods[0].gameObject.activeSelf,"Última maçã some da bancada");yield return EatingDone();
        yield return Drag(FoodType.Sandwich,true);C(p.Energia==75&&p.GetFoodCount(FoodType.Sandwich)==0,"Sanduíche recupera 25");yield return EatingDone();
        p.TentarGastarEnergia(55);yield return Drag(FoodType.NexaKing,true);C(p.Energia==70&&p.GetFoodCount(FoodType.NexaKing)==0,"Nexa King recupera 50");yield return EatingDone();
        p.TryBuyFood(FoodType.Sandwich);p.TryBuyFood(FoodType.Apple);yield return Drag(FoodType.Sandwich,true);C(p.Energia==95,"Energia prepara próximo ao máximo");yield return EatingDone();
        yield return Drag(FoodType.Apple,true);C(p.Energia==100&&c.energyText.text=="100 / 100"&&c.energyFill.fillAmount==1,"95 mais 10 limita em 100 com HUD imediato");yield return EatingDone();
        p.TryBuyFood(FoodType.Apple);stock=p.GetFoodCount(FoodType.Apple);yield return Drag(FoodType.Apple,true);C(p.Energia==100&&p.GetFoodCount(FoodType.Apple)==stock&&c.popupArt.sprite==c.fullArt&&c.popupLayer.activeSelf,"Energia cheia não consome e abre popup");yield return Photo("EnergiaCheia");yield return Click(B(c.popupLayer.transform,"ButtonX"));
        foreach(var sprite in new[]{c.purchaseArt,c.insufficientArt,c.fullArt,c.unavailableArt})foreach(string close in new[]{"ButtonOK","ButtonX"}){c.ShowPopup(sprite);yield return null;yield return Click(B(c.popupLayer.transform,close));C(!c.popupLayer.activeSelf,"OK e X fecham "+sprite.name);}
        while(p.Coins>=5)p.TryBuyFood(FoodType.Apple);
        yield return Click(B(c.transform,"ButtonMercado"));string saved=PlayerPrefs.GetString("NexaQuest.Testes.Quiz");yield return Click(B(c.market.transform,"ButtonApple"));C(c.popupArt.sprite==c.insufficientArt&&saved==PlayerPrefs.GetString("NexaQuest.Testes.Quiz"),"Sem saldo não muda moedas estoque ou Energia");yield return Photo("SaldoInsuficiente");
        pause.ProcessInput(false,true);C(!PauseSettingsManager.IsOpen,"Esc respeita popup da MinhaQuest");c.Escape();C(!c.popupLayer.activeSelf&&c.market.activeSelf,"Esc fecha só popup sobre Mercado");yield return null;pause.ProcessInput(false,true);c.Escape();C(!c.market.activeSelf&&!PauseSettingsManager.IsOpen,"Esc fecha Mercado sem abrir Configurações");
        yield return Click(B(c.transform,"ButtonVestuario"));yield return null;var full=c.wardrobe.transform.Find("Vestuario/QuestCorpo").GetComponent<Animator>();C(full.GetCurrentAnimatorStateInfo(0).IsName("Idle")&&full.GetCurrentAnimatorStateInfo(0).loop,"Corpo inteiro Idle com loop");C(c.wardrobe.transform.Find("Vestuario").childCount==2,"Vestuário sem roupas acessórios ou placeholders");yield return Photo("Vestuario");yield return Click(B(c.wardrobe.transform,"ButtonCloseWardrobe"));C(!c.wardrobe.activeSelf,"X do Vestuário retorna");
        yield return Click(B(c.transform,"ButtonLoja"));C(c.popupArt.sprite==c.unavailableArt,"Loja abre indisponível");yield return Photo("LojaIndisponivel");yield return Click(B(c.popupLayer.transform,"ButtonOK"));
        C(!p.TryBuyFood((FoodType)99)&&p.TryFeed((FoodType)99)==FeedingResult.Invalid,"Tipos inválidos rejeitados");Invariants();
        yield return Click(B(c.transform,"ButtonVoltar"));yield return Ready("Menu");
        yield return Click(B(GameObject.Find("JanelaMenu").transform,"botaoMinhaQuest"));yield return Ready("MinhaQuest");C(MinhaQuestNavigation.Origin=="Menu","Entrada Menu registra origem");yield return Click(B(c.transform,"ButtonVoltar"));yield return Ready("Menu");
        yield return Click(B(GameObject.Find("JanelaMenu").transform,"botaoJogar"));yield return Ready("MapaMundi");pause.Open();yield return Click(B(pause.panel.transform,"MinhaQuest"));yield return Ready("MinhaQuest");C(MinhaQuestNavigation.Origin=="MapaMundi"&&Time.timeScale==1,"Configurações Mapa registra origem e restaura tempo");yield return Click(B(c.transform,"ButtonVoltar"));yield return Ready("MapaMundi");
        SceneManager.LoadScene("Brasil");yield return Ready("Brasil");C(ProgressionManager.Instance==p&&FindObjectsOfType<ProgressionManager>().Length==1,"Brasil reutiliza mesmo progresso sem duplicação");var hud=FindObjectOfType<ProgressionHUD>();C(hud.coins.text==p.Coins.ToString(),"HUD Brasil recebe saldo atualizado");var indicators=FindObjectsOfType<IndicadorEnergia>();C(indicators.Length>0,"HUD de Energia existente presente");Invariants();
        pause.Open();yield return Click(B(pause.panel.transform,"MinhaQuest"));yield return Ready("MinhaQuest");C(MinhaQuestNavigation.Origin=="Brasil","Configurações Brasil registra origem");yield return Click(B(c.transform,"ButtonVoltar"));yield return Ready("Brasil");C(p.Energia==100,"Retorno ao Brasil mantém Energia recuperada");Invariants();
        SceneManager.LoadScene("MinhaQuest");yield return Ready("MinhaQuest");C(MinhaQuestNavigation.Origin=="Brasil","Carregamento direto de outra cena também captura origem");
        SessionState.SetString("MQ.Expected",PlayerPrefs.GetString("NexaQuest.Testes.Quiz"));SessionState.SetInt("MQ.Coins",p.Coins);SessionState.SetInt("MQ.Energy",p.Energia);SessionState.SetInt("MQ.Apple",p.GetFoodCount(FoodType.Apple));SessionState.SetInt("MQ.Sandwich",p.GetFoodCount(FoodType.Sandwich));SessionState.SetInt("MQ.King",p.GetFoodCount(FoodType.NexaKing));Finish();
    }
    void Finish(){lines.Add("Falhas: "+failures);File.WriteAllLines(Docs+"testes.txt",lines);Application.logMessageReceived-=Log;SessionState.SetBool("MQ.Pass",failures==0);EditorApplication.isPlaying=false;}
}

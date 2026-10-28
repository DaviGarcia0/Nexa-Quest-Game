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
using NexaQuest.Brasil;
using NexaQuest.MinhaQuest;

[InitializeOnLoad]
public static class HeadFeedingValidation
{
    public const string Docs = "Documentation/AlimentacaoCabeca20261028/";
    const BindingFlags Flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
    [Serializable] class Setup { public SceneSetup[] scenes; }
    static HeadFeedingValidation()
    {
        EditorApplication.update += Poll;
        EditorApplication.playModeStateChanged += state => {
            if (state == PlayModeStateChange.EnteredPlayMode && SessionState.GetBool("Head.Start", false)) {
                SessionState.SetBool("Head.Start", false);
                new GameObject("TesteAlimentacaoCabeca").AddComponent<HeadFeedingChecks>();
            }
            if (state != PlayModeStateChange.EnteredEditMode || !SessionState.GetBool("Head.End", false)) return;
            SessionState.SetBool("Head.End", false);
            SessionState.SetBool("NexaQuest.ValidandoQuiz", SessionState.GetBool("Head.PreviousFlag", false));
            if (SessionState.GetBool("Head.TestExists", false)) PlayerPrefs.SetString("NexaQuest.Testes.Quiz", SessionState.GetString("Head.Test", ""));
            else PlayerPrefs.DeleteKey("NexaQuest.Testes.Quiz");
            PlayerPrefs.Save();
            bool same = PlayerPrefs.GetString(ProgressionManager.ChavePersistencia, "") == SessionState.GetString("Head.Real", "") && PlayerPrefs.HasKey(ProgressionManager.ChavePersistencia) == SessionState.GetBool("Head.RealExists", false);
            File.WriteAllText(Docs + "save-real.txt", "Save real preservado: " + same);
            EditorSceneManager.RestoreSceneManagerSetup(JsonUtility.FromJson<Setup>(SessionState.GetString("Head.Setup", "")).scenes);
            typeof(QuestHUDVisualValidation).GetMethod("RestoreGameView", Flags).Invoke(null, null);
            object[] counts = { 0, 0, 0 };
            typeof(Editor).Assembly.GetType("UnityEditor.LogEntries").GetMethod("GetCountsByType", Flags).Invoke(null, counts);
            File.WriteAllText(Docs + "console.txt", "Erros: " + counts[0] + "\nAvisos: " + counts[1] + "\nMensagens: " + counts[2] + "\nCena restaurada: " + SceneManager.GetActiveScene().path + "\nDirty: " + SceneManager.GetActiveScene().isDirty);
        };
    }
    static void Poll()
    {
        const string request = "Library/HeadFeeding.request";
        if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode || !File.Exists(request)) return;
        File.Delete(request); Directory.CreateDirectory(Docs);
        try {
            var setup = EditorSceneManager.GetSceneManagerSetup();
            if (setup.Any(s => SceneManager.GetSceneByPath(s.path).isDirty)) throw new InvalidOperationException("A cena tem ajustes não salvos; preservados sem iniciar teste.");
            SessionState.SetString("Head.Setup", JsonUtility.ToJson(new Setup { scenes = setup }));
            SessionState.SetString("Head.Real", PlayerPrefs.GetString(ProgressionManager.ChavePersistencia, ""));
            SessionState.SetBool("Head.RealExists", PlayerPrefs.HasKey(ProgressionManager.ChavePersistencia));
            SessionState.SetBool("Head.TestExists", PlayerPrefs.HasKey("NexaQuest.Testes.Quiz"));
            SessionState.SetString("Head.Test", PlayerPrefs.GetString("NexaQuest.Testes.Quiz", ""));
            SessionState.SetBool("Head.PreviousFlag", SessionState.GetBool("NexaQuest.ValidandoQuiz", false));
            SessionState.SetBool("NexaQuest.ValidandoQuiz", true);
            PlayerPrefs.SetString("NexaQuest.Testes.Quiz", "{\"versao\":1,\"energia\":20,\"coins\":321,\"apples\":3,\"sandwiches\":3,\"nexaKings\":3}");
            EditorSceneManager.OpenScene("Assets/Projeto/Scenes/MinhaQuest.unity");
            typeof(QuestHUDVisualValidation).GetMethod("SetGameView", Flags).Invoke(null, null);
            typeof(Editor).Assembly.GetType("UnityEditor.LogEntries").GetMethod("Clear", Flags).Invoke(null, null);
            SessionState.SetBool("Head.Start", true); SessionState.SetBool("Head.End", true); EditorApplication.isPlaying = true;
        } catch (Exception e) { File.WriteAllText(Docs + "falha.txt", e.ToString()); Debug.LogException(e); }
    }
}

public class HeadFeedingChecks : MonoBehaviour
{
    readonly List<string> lines = new List<string>();
    int failures; MinhaQuestController c; ProgressionManager p;
    QuestFeedingController Feeding => c.feeding;
    void Check(bool ok, string name) { lines.Add((ok ? "PASS: " : "FAIL: ") + name); if (!ok) failures++; File.WriteAllLines(HeadFeedingValidation.Docs + "andamento.txt", lines); }
    void Log(string message, string trace, LogType type) {
        if (type != LogType.Error && type != LogType.Exception && type != LogType.Assert) return;
        Check(false, type + ": " + message); File.WriteAllLines(HeadFeedingValidation.Docs + "testes.txt", lines);
        EditorApplication.delayCall += () => EditorApplication.isPlaying = false;
    }
    void Seed(int energy = 20, int apples = 3) {
        PlayerPrefs.SetString("NexaQuest.Testes.Quiz", "{\"versao\":1,\"energia\":" + energy + ",\"coins\":321,\"apples\":" + apples + ",\"sandwiches\":3,\"nexaKings\":3}");
        p.CarregarProgresso();
    }
    Vector2 Point(float x, float y) => RectTransformUtility.WorldToScreenPoint(null, Feeding.mouth.TransformPoint(new Vector2(x, y)));
    PointerEventData Begin(FoodDragHandler food) {
        var data = new PointerEventData(EventSystem.current) { position = food.transform.position, button = PointerEventData.InputButton.Left };
        ExecuteEvents.Execute(food.gameObject, data, ExecuteEvents.beginDragHandler); return data;
    }
    void Move(FoodDragHandler food, PointerEventData data, Vector2 point) { data.position = point; ExecuteEvents.Execute(food.gameObject, data, ExecuteEvents.dragHandler); }
    void Drop(FoodDragHandler food, PointerEventData data) => ExecuteEvents.Execute(food.gameObject, data, ExecuteEvents.endDragHandler);
    IEnumerator Photo(string name) { yield return new WaitForEndOfFrame(); ScreenCapture.CaptureScreenshot(HeadFeedingValidation.Docs + name + ".png"); yield return null; }
    IEnumerator Finished() {
        yield return new WaitForSeconds(Feeding.eatingDuration + .15f);
        Check(!Feeding.Eating && !Feeding.MouthOpen && Feeding.animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"), "Após Eating volta ao Idle sem ficar presa");
    }
    IEnumerator Consume(FoodType type, Vector2 point, string name, bool approach = true) {
        var food = c.foods[(int)type]; int stock = p.GetFoodCount(type), energy = p.Energia, frame = Time.frameCount;
        var data = Begin(food);
        if (approach) { Move(food, data, point); Check(Feeding.MouthOpen && !Feeding.Eating, "Aproximação abre boca: " + name); }
        data.position = point; Drop(food, data);
        Check(Time.frameCount == frame && p.GetFoodCount(type) == stock - 1 && p.Energia == Mathf.Min(100, energy + FoodCatalog.Energy(type)), "Consumo e Energia na mesma soltura: " + name);
        var state = Feeding.animator.GetCurrentAnimatorStateInfo(0);
        Check(Feeding.Eating && !Feeding.MouthOpen && state.IsName("Eating") && state.normalizedTime == 0 && !Feeding.animator.IsInTransition(0), "Eating interrompe MouthOpen sem espera no mesmo frame: " + name);
        Check(!food.Dragging && food.gameObject.activeSelf == (stock > 1), "Sprite no slot corresponde ao estoque: " + name);
        var saved = JsonUtility.FromJson<Saved>(PlayerPrefs.GetString("NexaQuest.Testes.Quiz"));
        int stored = type == FoodType.Apple ? saved.apples : type == FoodType.Sandwich ? saved.sandwiches : saved.nexaKings;
        Check(stored == stock - 1 && saved.energia == p.Energia && p.Coins == 321, "Save imediato e moedas preservadas: " + name);
        yield return null;
    }
    [Serializable] class Saved { public int energia, apples, sandwiches, nexaKings; }
    IEnumerator Start()
    {
        Application.logMessageReceived += Log; Application.runInBackground = true;
        yield return null; yield return null;
        c = FindObjectOfType<MinhaQuestController>(); p = c.Progress;
        Check(Screen.width == 1920 && Screen.height == 1080, "Play Mode real 1920x1080");
        Check(Feeding.mouth.name == "HeadFeedingZone" && Feeding.mouth.GetComponent<Graphic>() == null && Feeding.mouth.sizeDelta == new Vector2(330, 290), "Uma zona invisível cobre a cabeça (330x290)");
        File.WriteAllText(HeadFeedingValidation.Docs + "range.txt", "Tamanho local: " + Feeding.mouth.sizeDelta + "\nPosição local: " + Feeding.mouth.anchoredPosition + "\nMínimo em tela (origem inferior esquerda): " + Point(-165, -145) + "\nMáximo: " + Point(165, 145));
        // Sobreposição temporária somente para a captura de diagnóstico. Não existe na cena salva.
        var diagnostic = new GameObject("DiagnosticoTemporario", typeof(RectTransform), typeof(Image));
        var rect = (RectTransform)diagnostic.transform; rect.SetParent(Feeding.mouth, false); rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.sizeDelta = Vector2.zero;
        var image = diagnostic.GetComponent<Image>(); image.color = new Color(0, 1, 0, .22f); image.raycastTarget = false;
        yield return Photo("RangeCabecaDiagnostico"); Destroy(diagnostic); yield return null;
        yield return Photo("PrincipalSemZonaVisivel");
        var apple = c.foods[0]; var parent = apple.transform.parent; var slot = ((RectTransform)apple.transform).anchoredPosition;
        var data = Begin(apple); Move(apple, data, Point(0, 0)); yield return null;
        Check(Feeding.MouthOpen && Feeding.animator.GetCurrentAnimatorStateInfo(0).IsName("MouthOpen"), "Entrar no range inicia MouthOpen");
        yield return Photo("MouthOpenAproximacao");
        Move(apple, data, Point(-180, 0)); yield return null;
        Check(!Feeding.MouthOpen && Feeding.animator.GetCurrentAnimatorStateInfo(0).IsName("Idle") && p.Energia == 20 && p.GetFoodCount(FoodType.Apple) == 3, "Sair sem soltar retorna Idle sem consumir");
        Drop(apple, data);
        Check(apple.transform.parent == parent && ((RectTransform)apple.transform).anchoredPosition == slot && p.Energia == 20 && p.GetFoodCount(FoodType.Apple) == 3, "Soltar fora retorna ao mesmo slot sem consumo");
        yield return Consume(FoodType.Apple, Point(0, 0), "centro / maçã 3 para 2");
        var sandwich = c.foods[1]; var second = Begin(sandwich); c.Feed(FoodType.Sandwich); Feeding.Approach(true);
        Check(!sandwich.Dragging && p.GetFoodCount(FoodType.Sandwich) == 3 && p.Energia == 30 && Feeding.Eating && !Feeding.MouthOpen, "Eating bloqueia novo arraste, alimentação e MouthOpen");
        yield return Photo("EatingImediato"); yield return Finished();
        yield return Consume(FoodType.Apple, Point(0, 110), "testa / maçã 2 para 1"); yield return Finished();
        yield return Consume(FoodType.Apple, Point(-125, -50), "bochecha esquerda / maçã 1 para 0"); yield return Finished();
        Seed(); yield return Consume(FoodType.Sandwich, Point(125, -55), "lateral direita / sanduíche +25"); yield return Finished();
        Seed(); yield return Consume(FoodType.NexaKing, Point(50, -105), "focinho / Nexa King +50"); yield return Finished();
        Seed(); yield return Consume(FoodType.Apple, Point(0, 110), "soltura sem atualizar MouthOpen antes", false); yield return Finished();
        Seed();
        foreach (var point in new[] { Point(-175, 0), Point(175, 0), Point(0, 155), Point(0, -155), new Vector2(1800, 900) }) {
            data = Begin(apple); Move(apple, data, point); Drop(apple, data);
            Check(!Feeding.Eating && !Feeding.MouthOpen && p.Energia == 20 && p.GetFoodCount(FoodType.Apple) == 3 && apple.transform.parent == parent && ((RectTransform)apple.transform).anchoredPosition == slot, "Soltura fora não alimenta: " + point);
        }
        Seed(100); data = Begin(apple); Move(apple, data, Point(0, 0)); Drop(apple, data);
        Check(p.Energia == 100 && p.GetFoodCount(FoodType.Apple) == 3 && !Feeding.Eating && !Feeding.MouthOpen && c.popupLayer.activeSelf && c.popupArt.sprite == c.fullArt && apple.transform.parent == parent && ((RectTransform)apple.transform).anchoredPosition == slot, "Energia cheia: popup, sem consumo/Eating, retorna ao slot");
        yield return null; c.ClosePopup(); Seed(20, 0); data = Begin(apple); c.Feed(FoodType.Apple);
        Check(!apple.Dragging && !apple.gameObject.activeSelf && p.Energia == 20 && !Feeding.Eating, "Sem estoque não inicia arraste nem alimentação");
        Check(!c.market.activeSelf && !c.wardrobe.activeSelf && !c.popupLayer.activeSelf, "Fluxo permanece na tela principal da mesma cena");
        Application.logMessageReceived -= Log; lines.Add("Falhas: " + failures); File.WriteAllLines(HeadFeedingValidation.Docs + "testes.txt", lines); EditorApplication.isPlaying = false;
    }
}

using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using NexaQuest.Brasil;
namespace NexaQuest.MinhaQuest
{
    [DefaultExecutionOrder(-10000)]
    public class MinhaQuestController : MonoBehaviour
    {
        public static MinhaQuestController Instance { get; private set; }
        public static bool ConsumesEscape => Instance != null && (Instance.ModalOpen || Instance.dragged != null || Instance.feeding.Eating);
        public CanvasGroup mainControls, marketControls;
        public GameObject market, wardrobe, popupLayer;
        public Image popupArt, energyFill;
        public TMP_Text energyText, balanceText;
        public Sprite purchaseArt, insufficientArt, fullArt, unavailableArt;
        public RectTransform popupOk, popupX, dragLayer;
        public FoodDragHandler[] foods;
        public QuestFeedingController feeding;
        public ProgressionManager Progress { get; private set; }
        FoodDragHandler dragged; bool navigating; int popupFrame = -1;
        public bool ModalOpen => market.activeSelf || wardrobe.activeSelf || popupLayer.activeSelf;
        public bool CanInteract => !navigating && !ModalOpen && !feeding.Eating && !PauseSettingsManager.GameplayBlocked;
        public bool CanDrag => !navigating && !ModalOpen && !feeding.Eating && !PauseSettingsManager.GameplayBlocked;
        void Awake()
        {
            Instance = this;
            Progress = ProgressionManager.Instance;
            if (Progress == null) Progress = new GameObject("ProgressionManager").AddComponent<ProgressionManager>();
        }
        void Start() { Progress.Changed += Refresh; Refresh(); SetControls(); }
        void OnDestroy() { CancelDrag(); if (Progress != null) Progress.Changed -= Refresh; if (Instance == this) Instance = null; }
        void Update()
        {
            if (PauseSettingsManager.IsOpen) { CancelDrag(); return; }
            if (PauseSettingsManager.GameplayBlocked) return;
            if (Input.GetKeyDown(KeyCode.Escape)) Escape();
        }
        public void Escape()
        {
            if (PauseSettingsManager.GameplayBlocked) return;
            if (popupLayer.activeSelf) ClosePopup();
            else if (market.activeSelf) CloseMarket();
            else if (wardrobe.activeSelf) CloseWardrobe();
            else CancelDrag();
        }
        public void Refresh()
        {
            energyText.text = Progress.Energia + " / " + ProgressionManager.EnergiaMaxima;
            energyFill.fillAmount = (float)Progress.Energia / ProgressionManager.EnergiaMaxima;
            energyFill.rectTransform.anchorMax = new Vector2(energyFill.fillAmount, 1);
            balanceText.text = Progress.Coins + " NexaCoins";
            foreach (var food in foods) food.gameObject.SetActive(Progress.GetFoodCount(food.type) > 0);
        }
        void SetControls()
        {
            mainControls.interactable = mainControls.blocksRaycasts = !ModalOpen;
            marketControls.interactable = marketControls.blocksRaycasts = !popupLayer.activeSelf;
        }
        public bool BeginDrag(FoodDragHandler food)
        {
            if (!CanInteract || dragged != null || Progress.GetFoodCount(food.type) == 0) return false;
            dragged = food; return true;
        }
        public void EndDrag(FoodDragHandler food) { if (dragged == food) dragged = null; }
        void CancelDrag() { if (dragged != null) dragged.Cancel(); }
        public void Feed(FoodType type)
        {
            if (!CanInteract) return;
            var result = Progress.TryFeed(type);
            if (result == FeedingResult.FullEnergy) ShowPopup(fullArt);
            else if (result == FeedingResult.Consumed) feeding.Eat();
        }
        public void OpenMarket() { if (!CanInteract) return; CancelDrag(); market.SetActive(true); SetControls(); }
        public void CloseMarket() { if (popupLayer.activeSelf || PauseSettingsManager.GameplayBlocked) return; market.SetActive(false); SetControls(); }
        public void OpenWardrobe() { if (!CanInteract) return; CancelDrag(); wardrobe.SetActive(true); SetControls(); }
        public void CloseWardrobe() { if (popupLayer.activeSelf || PauseSettingsManager.GameplayBlocked) return; wardrobe.SetActive(false); SetControls(); }
        public void OpenShop() { if (CanInteract) ShowPopup(unavailableArt); }
        public void BuyApple() => Buy(FoodType.Apple);
        public void BuySandwich() => Buy(FoodType.Sandwich);
        public void BuyNexaKing() => Buy(FoodType.NexaKing);
        void Buy(FoodType type)
        {
            if (!market.activeSelf || popupLayer.activeSelf || PauseSettingsManager.GameplayBlocked) return;
            ShowPopup(Progress.TryBuyFood(type) ? purchaseArt : insufficientArt);
        }
        public void ShowPopup(Sprite art)
        {
            CancelDrag(); popupArt.sprite = art; popupLayer.SetActive(true); popupFrame = Time.frameCount;
            // Cada PNG tem botões desenhados em posições ligeiramente diferentes.
            Rect ok = art == fullArt ? new Rect(505,740,525,165) : art == purchaseArt ? new Rect(515,710,500,165) : new Rect(510,705,515,165);
            Rect close = art == fullArt ? new Rect(1295,95,195,180) : art == purchaseArt ? new Rect(1255,145,190,170) : art == insufficientArt ? new Rect(1275,115,185,180) : new Rect(1255,145,190,180);
            PlaceHotspot(popupOk, ok); PlaceHotspot(popupX, close); SetControls();
        }
        void PlaceHotspot(RectTransform t, Rect px)
        {
            var size = popupArt.rectTransform.sizeDelta;
            t.sizeDelta = new Vector2(px.width * size.x / 1536f, px.height * size.y / 1024f);
            t.anchoredPosition = new Vector2((px.center.x / 1536f - .5f) * size.x, (.5f - px.center.y / 1024f) * size.y);
        }
        public void ClosePopup()
        {
            if (PauseSettingsManager.GameplayBlocked || Time.frameCount == popupFrame) return;
            popupLayer.SetActive(false); SetControls();
        }
        public void Back()
        {
            if (!CanInteract) return;
            CancelDrag(); navigating = true; SceneManager.LoadSceneAsync(MinhaQuestNavigation.ReturnScene);
        }
    }
}

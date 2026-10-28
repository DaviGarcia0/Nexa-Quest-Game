using UnityEngine;
using UnityEngine.EventSystems;
namespace NexaQuest.MinhaQuest
{
    public class FoodDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public MinhaQuestController controller;
        public FoodType type;
        Transform slotParent; Vector2 slotPosition; int slotIndex; bool dragging;
        RectTransform Rect => (RectTransform)transform;
        public bool Dragging => dragging;
        void Awake() { slotParent = transform.parent; slotPosition = Rect.anchoredPosition; slotIndex = transform.GetSiblingIndex(); }
        public void OnBeginDrag(PointerEventData data)
        {
            if (data.button != PointerEventData.InputButton.Left || !controller.BeginDrag(this)) return;
            dragging = true; transform.SetParent(controller.dragLayer, false); transform.SetAsLastSibling(); Move(data);
        }
        public void OnDrag(PointerEventData data) { if (dragging) Move(data); }
        void Move(PointerEventData data)
        {
            if (!controller.CanDrag) { Cancel(); return; }
            Vector2 local;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(controller.dragLayer, data.position, data.pressEventCamera, out local))
                Rect.anchoredPosition = local;
            controller.feeding.Approach(controller.feeding.Contains(data.position, data.pressEventCamera, true));
        }
        public void OnEndDrag(PointerEventData data)
        {
            if (!dragging) return;
            bool deliver = controller.CanDrag && controller.feeding.Contains(data.position, data.pressEventCamera, false);
            Cancel();
            if (deliver) controller.Feed(type);
        }
        public void Cancel()
        {
            if (!dragging) return;
            dragging = false; transform.SetParent(slotParent, false); transform.SetSiblingIndex(slotIndex); Rect.anchoredPosition = slotPosition;
            controller.EndDrag(this); controller.feeding.Approach(false);
        }
        void OnDisable() { Cancel(); }
    }
}

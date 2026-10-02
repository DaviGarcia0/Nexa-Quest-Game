using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NexaQuest.Brasil
{
    public class ProgressionHUD : MonoBehaviour
    {
        public TMP_Text coins;
        public TMP_Text knowledge;
        public TMP_Text brainLevel;
        public Image knowledgeFill;
        ProgressionManager progression;
        void Start()
        {
            progression = ProgressionManager.Instance;
            if (progression == null) return;
            progression.Changed += Refresh; Refresh();
        }
        void Refresh()
        {
            coins.text = progression.Coins.ToString();
            knowledge.text = "CONHECIMENTO";
            brainLevel.text = progression.Level.ToString();
            knowledgeFill.fillAmount = (float)progression.CurrentXP / progression.XPToNextLevel;
        }
        void OnDestroy() { if (progression != null) progression.Changed -= Refresh; }
    }
}

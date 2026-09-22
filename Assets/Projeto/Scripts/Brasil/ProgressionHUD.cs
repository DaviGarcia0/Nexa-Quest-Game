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
            coins.text = "Nexa Coins  " + progression.Coins;
            knowledge.text = "Conhecimento  " + progression.CurrentXP + " / " + progression.XPToNextLevel;
            brainLevel.text = "Cérebro  •  Nível " + progression.Level;
            knowledgeFill.fillAmount = (float)progression.CurrentXP / progression.XPToNextLevel;
        }
        void OnDestroy() { if (progression != null) progression.Changed -= Refresh; }
    }
}

using System;
using UnityEngine;

namespace NexaQuest.Brasil
{
    public enum DialogueSpeaker { NPC, Quest, Other }

    [Serializable]
    public class DialogueLine
    {
        public DialogueSpeaker speaker;
        public string speakerName;
        public Sprite portrait;
        [TextArea(2, 5)] public string text;
    }

    [CreateAssetMenu(menuName = "Nexa Quest/Dialogue", fileName = "Dialogue")]
    public class DialogueData : ScriptableObject
    {
        public DialogueLine[] lines;
    }
}

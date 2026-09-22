using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NexaQuest.Brasil
{
    public class DialogueController : MonoBehaviour
    {
        public BrazilWorld world;
        public GameObject panel;
        public Image portrait;
        public TMP_Text speakerName;
        public TMP_Text dialogueText;
        public TMP_Text continueIndicator;
        public GameObject interactionPrompt;
        public bool IsOpen { get; private set; }
        public int LineIndex { get; private set; }
        public NPCInteraction CurrentNPC { get; private set; }
        readonly HashSet<NPCInteraction> nearby = new HashSet<NPCInteraction>();
        bool ownsPrompt;
        int lastInputFrame = -1;

        void Update()
        {
            RefreshPrompt();
            if (Input.GetKeyDown(KeyCode.E)) HandleInteract();
        }
        public void Register(NPCInteraction npc) { nearby.Add(npc); RefreshPrompt(); }
        public void Unregister(NPCInteraction npc)
        {
            nearby.Remove(npc);
            if (IsOpen && CurrentNPC == npc) Close(false);
            RefreshPrompt();
        }
        NPCInteraction Closest()
        {
            NPCInteraction best = null; float distance = float.PositiveInfinity;
            foreach (var npc in nearby)
            {
                if (npc == null || !npc.isActiveAndEnabled || npc.dialogue == null || npc.dialogue.lines == null || npc.dialogue.lines.Length == 0) continue;
                float d = (npc.transform.position - world.player.transform.position).sqrMagnitude;
                if (d < distance) { best = npc; distance = d; }
            }
            return best;
        }
        void RefreshPrompt()
        {
            if (world == null || interactionPrompt == null) return;
            bool show = !IsOpen && !world.IsTransitioning && !world.player.ControlsLocked && Closest() != null;
            if (show) { interactionPrompt.SetActive(true); ownsPrompt = true; }
            else if (ownsPrompt) { interactionPrompt.SetActive(false); ownsPrompt = false; }
        }
        // Um unico leitor de E para todos os NPCs evita abrir e avancar no mesmo frame.
        public void HandleInteract()
        {
            if (lastInputFrame == Time.frameCount || world == null || world.IsTransitioning) return;
            lastInputFrame = Time.frameCount;
            if (IsOpen)
            {
                LineIndex++;
                if (LineIndex >= CurrentNPC.dialogue.lines.Length) Close(true);
                else ShowLine();
                return;
            }
            if (world.player.ControlsLocked) return;
            var npc = Closest(); if (npc == null) return;
            CurrentNPC = npc; LineIndex = 0; IsOpen = true;
            world.player.SetControlsLocked(true);
            panel.SetActive(true); interactionPrompt.SetActive(false); ownsPrompt = false;
            ShowLine();
        }
        void ShowLine()
        {
            var line = CurrentNPC.dialogue.lines[LineIndex];
            speakerName.text = line.speakerName;
            portrait.sprite = line.portrait; portrait.enabled = line.portrait != null;
            dialogueText.text = line.text;
            continueIndicator.text = LineIndex + 1 == CurrentNPC.dialogue.lines.Length ? "E  •  Encerrar" : "E  •  Continuar";
        }
        void Close(bool completed)
        {
            var npc = CurrentNPC; bool wasOpen = IsOpen;
            IsOpen = false; CurrentNPC = null;
            if (panel != null) panel.SetActive(false);
            if (wasOpen && world != null && !world.IsTransitioning) world.player.SetControlsLocked(false);
            RefreshPrompt();
            if (completed && npc != null) npc.onDialogueFinished.Invoke();
        }
        void OnDisable()
        {
            nearby.Clear(); Close(false);
            if (ownsPrompt && interactionPrompt != null) interactionPrompt.SetActive(false);
            ownsPrompt = false;
        }
    }
}

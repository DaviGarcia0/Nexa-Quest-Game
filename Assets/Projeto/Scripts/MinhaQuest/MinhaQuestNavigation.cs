using UnityEngine;
using UnityEngine.SceneManagement;
namespace NexaQuest.MinhaQuest
{
    public static class MinhaQuestNavigation
    {
        public static string Origin { get; private set; }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Reset() { Origin = null; SceneManager.activeSceneChanged -= Changed; SceneManager.activeSceneChanged += Changed; }
        static void Changed(Scene before, Scene after) { if (after.name == "MinhaQuest") Register(before.name); }
        public static void RegisterCurrent() => Register(SceneManager.GetActiveScene().name);
        static void Register(string name) { if (!string.IsNullOrEmpty(name) && name != "MinhaQuest" && Application.CanStreamedLevelBeLoaded(name)) Origin = name; }
        public static string ReturnScene => !string.IsNullOrEmpty(Origin) && Origin != "MinhaQuest" && Application.CanStreamedLevelBeLoaded(Origin) ? Origin : "Menu";
    }
}

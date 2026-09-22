using System;
using UnityEngine;

namespace NexaQuest.Brasil
{
    public class ProgressionManager : MonoBehaviour
    {
        public static ProgressionManager Instance { get; private set; }
        [SerializeField, Min(0)] int coins;
        [SerializeField, Min(0)] int currentXP;
        [SerializeField, Min(1)] int level = 1;
        [SerializeField, Min(1)] int xpPerLevel = 100;
        public int Coins => coins;
        public int CurrentXP => currentXP;
        public int Level => level;
        public int XPToNextLevel => Mathf.Max(1, xpPerLevel);
        public event Action Changed;
        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this; DontDestroyOnLoad(gameObject);
        }
        void OnDestroy() { if (Instance == this) Instance = null; }
        public void AddCoins(int amount)
        {
            if (amount <= 0) return;
            coins = (int)Math.Min(int.MaxValue, (long)coins + amount); Changed?.Invoke();
        }
        public void AddXP(int amount)
        {
            if (amount <= 0) return;
            long total = (long)currentXP + amount;
            level = (int)Math.Min(int.MaxValue, level + total / XPToNextLevel);
            currentXP = (int)(total % XPToNextLevel); Changed?.Invoke();
        }
    }
}

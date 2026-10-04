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
        [SerializeField, Range(0, 100)] int energia = 100;
        [SerializeField] bool possuiChaveVila;
        [SerializeField] bool instrucoesFeiraVistas;
        public int Coins => coins;
        public int CurrentXP => currentXP;
        public int Level => level;
        public int XPToNextLevel => Mathf.Max(1, xpPerLevel);
        public int Energia => energia;
        public bool PossuiChaveVila => possuiChaveVila;
        public bool InstrucoesFeiraVistas => instrucoesFeiraVistas;
        public event Action Changed;
        public const string ChavePersistencia = "NexaQuest.Progresso.v1";
        static string ChaveUsada {
            get {
#if UNITY_EDITOR
                // Os testes usam outra chave; nunca gastam a energia do jogador real.
                if (UnityEditor.SessionState.GetBool("NexaQuest.ValidandoMinijogo", false))
                    return "NexaQuest.Testes.Minijogo";
#endif
                return ChavePersistencia;
            }
        }
        [Serializable] class Dados {
            public int versao = 1, coins, currentXP, level = 1, energia = 100;
            public bool possuiChaveVila, instrucoesFeiraVistas;
        }
        void Awake() {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this; DontDestroyOnLoad(gameObject); CarregarProgresso();
        }
        void OnDestroy() { if (Instance == this) Instance = null; }
        public void CarregarProgresso() {
            if (PlayerPrefs.HasKey(ChaveUsada)) {
                try {
                    var dados = JsonUtility.FromJson<Dados>(PlayerPrefs.GetString(ChaveUsada));
                    if (dados != null && dados.versao == 1) {
                        coins = Mathf.Max(0, dados.coins); level = Mathf.Max(1, dados.level);
                        currentXP = Mathf.Clamp(dados.currentXP, 0, XPToNextLevel - 1);
                        energia = Mathf.Clamp(dados.energia, 0, 100);
                        possuiChaveVila = dados.possuiChaveVila; instrucoesFeiraVistas = dados.instrucoesFeiraVistas;
                    }
                } catch (ArgumentException) { Debug.LogWarning("Progresso salvo inválido; valores atuais preservados."); }
            }
            Changed?.Invoke();
        }
        void SalvarENotificar() {
            var dados = new Dados { coins=coins, currentXP=currentXP, level=level, energia=energia,
                possuiChaveVila=possuiChaveVila, instrucoesFeiraVistas=instrucoesFeiraVistas };
            PlayerPrefs.SetString(ChaveUsada, JsonUtility.ToJson(dados)); PlayerPrefs.Save(); Changed?.Invoke();
        }
        public void AddCoins(int amount) {
            if (amount <= 0) return;
            coins = (int)Math.Min(int.MaxValue, (long)coins + amount); SalvarENotificar();
        }
        public void AddXP(int amount) {
            if (amount <= 0) return;
            long total = (long)currentXP + amount;
            level = (int)Math.Min(int.MaxValue, level + total / XPToNextLevel);
            currentXP = (int)(total % XPToNextLevel); SalvarENotificar();
        }
        public bool TentarGastarEnergia(int quantidade) {
            if (quantidade <= 0 || energia < quantidade) return false;
            energia -= quantidade; SalvarENotificar(); return true;
        }
        public bool ConquistarChaveVila() {
            if (possuiChaveVila) return false;
            possuiChaveVila = true; SalvarENotificar(); return true;
        }
        public void MarcarInstrucoesFeiraVistas() {
            if (instrucoesFeiraVistas) return;
            instrucoesFeiraVistas = true; SalvarENotificar();
        }
    }
}

using System.Collections.Generic;
using UnityEngine;

namespace ButterflyStep
{
    public class ConsequenceRule : MonoBehaviour, IFlagProvider
    {
        [Tooltip("Flag criada por esta consequência. Outros objetos podem reagir a ela.")]
        [SerializeField] private string resultFlag = "NovaConsequencia";
        [Tooltip("Texto exibido no debug para explicar a regra.")]
        [SerializeField] private string description = "Causa -> Consequência";
        [Tooltip("Todas as causas precisam ser verdadeiras para a consequência existir naquele dia. Use 'value' nas flags para exigir que a causa tenha acontecido há X dias.")]
        [SerializeField] private List<TemporalCondition> causes = new List<TemporalCondition>();

        public string FlagKey => resultFlag;
        public string Description => description;

        public void Setup(string flag, string text, params TemporalCondition[] conditions)
        {
            resultFlag = flag;
            description = text;
            causes = new List<TemporalCondition>(conditions);
        }

        private void Awake()
        {
            var ctx = LevelContext.Current;
            if (ctx != null) ctx.World.RegisterProvider(this);
        }

        private void OnDestroy()
        {
            var ctx = LevelContext.Current;
            if (ctx != null) ctx.World.UnregisterProvider(this);
        }

        public bool IsActiveAt(WorldState world, int day)
        {
            var ctx = LevelContext.Current;
            if (ctx == null) return false;
            return TemporalCondition.All(causes, day, ctx.Time.StartDay, ctx.Time.Calendar, world);
        }
    }
}

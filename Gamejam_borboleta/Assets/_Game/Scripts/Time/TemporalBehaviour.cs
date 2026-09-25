using System.Collections.Generic;
using UnityEngine;

namespace ButterflyStep
{
    public abstract class TemporalBehaviour : MonoBehaviour
    {
        protected LevelContext Context { get; private set; }
        protected TimeState Now => Context.Time.State;

        protected virtual void Start()
        {
            Context = LevelContext.Current;
            if (Context == null)
            {
                Debug.LogError($"{name}: nenhum LevelContext na cena.", this);
                enabled = false;
                return;
            }
            Context.Time.BeforeTimeChange += OnBeforeTimeChange;
            Context.Time.TimeChanged += HandleTimeChanged;
            Context.World.Changed += HandleWorldChanged;
            Refresh(true);
        }

        protected virtual void OnDestroy()
        {
            if (Context == null) return;
            Context.Time.BeforeTimeChange -= OnBeforeTimeChange;
            Context.Time.TimeChanged -= HandleTimeChanged;
            Context.World.Changed -= HandleWorldChanged;
        }

        private void HandleTimeChanged(TimeState state) => Refresh(false);

        private void HandleWorldChanged() => Refresh(false);

        protected virtual void OnBeforeTimeChange(TimeState state) { }

        protected abstract void Refresh(bool instant);

        protected bool Check(List<TemporalCondition> conditions)
        {
            return TemporalCondition.All(conditions, Now.Day, Context.Time.StartDay, Context.Time.Calendar, Context.World);
        }

        protected bool CheckFrom(List<TemporalCondition> conditions, int originDay)
        {
            return TemporalCondition.All(conditions, Now.Day, originDay, Context.Time.Calendar, Context.World);
        }
    }
}

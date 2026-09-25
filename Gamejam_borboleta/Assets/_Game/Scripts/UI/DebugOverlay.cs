using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace ButterflyStep
{
    public class DebugOverlay : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private Text output;
        [SerializeField] private bool visibleOnStart;
        [SerializeField] private float refreshInterval = 0.2f;

        private readonly StringBuilder builder = new StringBuilder();
        private TemporalObject[] objects;
        private TemporalEnemy[] enemies;
        private float nextRefresh;

        private void Start()
        {
            objects = FindObjectsByType<TemporalObject>(FindObjectsSortMode.None).OrderBy(o => o.transform.position.x).ToArray();
            enemies = FindObjectsByType<TemporalEnemy>(FindObjectsSortMode.None);
            if (panel != null) panel.SetActive(visibleOnStart);
        }

        private void Update()
        {
            var kb = Keyboard.current;
            var ctx = LevelContext.Current;
            if (kb == null || ctx == null) return;

            if (kb.f1Key.wasPressedThisFrame && panel != null) panel.SetActive(!panel.activeSelf);
            if (kb.f2Key.wasPressedThisFrame) ctx.Time.JumpToIndex(ctx.Now.Index - 1);
            if (kb.f3Key.wasPressedThisFrame) ctx.Time.JumpToIndex(ctx.Now.Index + 1);
            if (kb.f5Key.wasPressedThisFrame) ctx.Flow.Restart();
            if (kb.f6Key.wasPressedThisFrame) ctx.Flow.SkipLevel();

            if (ctx.Player != null && ctx.Player.Input.RestartPressed) ctx.Flow.Restart();

            if (panel == null || !panel.activeSelf || output == null || UnityEngine.Time.unscaledTime < nextRefresh) return;
            nextRefresh = UnityEngine.Time.unscaledTime + refreshInterval;
            output.text = Build(ctx);
        }

        private string Build(LevelContext ctx)
        {
            var state = ctx.Now;
            var world = ctx.World;
            builder.Clear();
            builder.AppendLine("<b>DEBUG  (F1 fecha)</b>");
            builder.AppendLine($"Dia atual: <b>{state.DisplayDay}</b>  [1 .. {state.MaxDay + 1}]");
            builder.AppendLine($"Estação: <b>{SeasonCalendar.Name(state.Season)}</b> (dia {state.DayOfSeason})  Ano {state.YearNumber}");
            builder.AppendLine("F2/F3 tempo  F5 reinicia  F6 pula fase");
            builder.AppendLine();
            builder.AppendLine("<b>Ações do jogador</b>");
            foreach (var key in world.DirectFlags)
            {
                world.TryGetFlagDay(key, out int d);
                builder.AppendLine($"{key} = {Bool(world.IsActive(key, state.Day))}  (desde o dia {d + 1})");
            }
            builder.AppendLine();
            builder.AppendLine("<b>Consequências</b>");
            foreach (var p in world.Providers)
            {
                builder.AppendLine($"{p.FlagKey} = {Bool(world.IsActive(p.FlagKey, state.Day))}");
            }
            builder.AppendLine();
            builder.AppendLine("<b>Objetos temporais e criaturas</b>");
            foreach (var o in objects)
            {
                if (o != null) builder.AppendLine($"{o.DisplayName}: {o.CurrentStateName}");
            }
            foreach (var e in enemies)
            {
                if (e != null) builder.AppendLine($"{e.DisplayName}: {e.StageName}");
            }
            return builder.ToString();
        }

        private static string Bool(bool value) => value ? "<color=#7CFF7C>true</color>" : "<color=#FF8080>false</color>";
    }
}

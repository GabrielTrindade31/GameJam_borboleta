using System.Collections;
using UnityEngine;

namespace ButterflyStep
{
    public class FragilePlatform : MonoBehaviour
    {
        [Tooltip("Flag registrada no dia em que a plataforma quebra. Um TemporalObject pode usar essa flag para esconder a plataforma.")]
        [SerializeField] private string brokenFlag = "PlataformaQuebrada";
        [Tooltip("Tempo tremendo antes de quebrar.")]
        [SerializeField] private float shakeTime = 0.7f;
        [SerializeField] private Transform shakeTarget;
        [SerializeField] private string brokenMessage = "O peso da criatura quebrou a ponte!";

        private bool breaking;

        public void Setup(string flag, Transform target)
        {
            brokenFlag = flag;
            shakeTarget = target;
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            if (breaking) return;
            var enemy = collision.collider.GetComponentInParent<TemporalEnemy>();
            if (enemy == null || !enemy.IsHeavy) return;
            StartCoroutine(Break());
        }

        private IEnumerator Break()
        {
            breaking = true;
            var ctx = LevelContext.Current;
            int day = ctx.Now.Day;
            Transform t = shakeTarget != null ? shakeTarget : transform;
            Vector3 basePos = t.localPosition;

            for (float time = 0f; time < shakeTime; time += UnityEngine.Time.deltaTime)
            {
                if (ctx.Now.Day != day) break;
                t.localPosition = basePos + (Vector3)Random.insideUnitCircle * 0.08f;
                yield return null;
            }
            t.localPosition = basePos;

            if (ctx.Now.Day == day)
            {
                ctx.World.SetFlag(brokenFlag, day);
                if (ctx.Hud != null)
                {
                    ctx.Hud.ShowMessage(brokenMessage);
                    ctx.Hud.Shake(0.35f, 0.4f);
                }
            }
            breaking = false;
        }
    }
}

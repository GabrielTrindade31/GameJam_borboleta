using UnityEngine;

namespace ButterflyStep
{
    public class PowerChest : MonoBehaviour
    {
        [SerializeField] private GameObject closedVisual;
        [SerializeField] private GameObject openVisual;

        private void Start() => Refresh();

        public void Unlock()
        {
            bool first = !GameProgress.HasBolt;
            GameProgress.UnlockBolt();
            Refresh();
            var fx = FeedbackFX.Instance;
            if (fx != null)
            {
                fx.TimeRestore(transform.position + Vector3.up * 0.5f, 1.4f);
                fx.ButterflyBurst(transform.position + Vector3.up * 0.5f, 6, 0.4f);
            }
            GameAudio.Play(Sfx.Pollen);
            var hud = LevelContext.Current != null ? LevelContext.Current.Hud : null;
            if (hud != null && first) hud.ShowMessage("Novo poder: DISPARO DO TEMPO!  K (ou B no controle) lança um raio que fere de longe. Recarrega em alguns segundos.", 5f);
        }

        private void Refresh()
        {
            bool open = GameProgress.HasBolt;
            if (closedVisual != null) closedVisual.SetActive(!open);
            if (openVisual != null) openVisual.SetActive(open);
        }
    }
}

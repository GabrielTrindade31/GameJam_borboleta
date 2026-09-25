using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ButterflyStep
{
    public class LevelFlow : MonoBehaviour
    {
        [Header("Apresentação")]
        [SerializeField] private string levelTitle = "Fase";
        [TextArea(2, 5)] [SerializeField] private string introText = "";
        [TextArea(2, 5)] [SerializeField] private string completeText = "";

        [Header("Fluxo")]
        [Tooltip("Nome da cena carregada ao concluir esta fase.")]
        [SerializeField] private string nextScene = "";
        [Tooltip("Número do capítulo (usado no progresso salvo e no menu).")]
        [SerializeField] private int levelNumber = 1;
        [SerializeField] private float completeDelay = 2.8f;
        [SerializeField] private float deathDelay = 1.1f;

        private bool finishing;
        private bool respawning;
        private Vector2 respawnPoint;
        private Checkpoint activeCheckpoint;

        public string LevelTitle => levelTitle;
        public string IntroText => introText;
        public bool IsFinishing => finishing;
        public int LevelNumber => levelNumber;

        public void Configure(string title, string intro, string complete, string next)
        {
            levelTitle = title;
            introText = intro;
            completeText = complete;
            nextScene = next;
        }

        private void Start()
        {
            var ctx = LevelContext.Current;
            if (ctx.Player != null) respawnPoint = ctx.Player.transform.position;
            GameProgress.SetLast(levelNumber);
            var hud = ctx.Hud;
            if (hud != null) hud.ShowIntro(levelTitle, introText);
        }

        public void SetCheckpoint(Checkpoint checkpoint)
        {
            if (checkpoint == activeCheckpoint) return;
            if (activeCheckpoint != null) activeCheckpoint.SetActive(false);
            activeCheckpoint = checkpoint;
            activeCheckpoint.SetActive(true);
            respawnPoint = checkpoint.RespawnPoint;
            GameAudio.Play(Sfx.Checkpoint);
            var hud = LevelContext.Current.Hud;
            if (hud != null) hud.ShowMessage("Flor do Tempo desperta: se cair, você volta aqui — o mundo e o dia continuam como estão.", 2.8f);
        }

        public void CompleteLevel()
        {
            if (finishing) return;
            finishing = true;
            GameProgress.Unlock(levelNumber + 1);
            StartCoroutine(CompleteRoutine());
        }

        public void PlayerDied()
        {
            if (finishing || respawning) return;
            respawning = true;
            StartCoroutine(DeathRoutine());
        }

        public void Restart()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void SkipLevel()
        {
            if (finishing) return;
            finishing = true;
            GameProgress.Unlock(levelNumber + 1);
            LoadNext();
        }

        private IEnumerator CompleteRoutine()
        {
            var ctx = LevelContext.Current;
            if (ctx.Player != null) ctx.Player.SetControlEnabled(false);
            int pollen = GameProgress.PollenCount(SceneManager.GetActiveScene().name);
            if (ctx.Hud != null) ctx.Hud.ShowLevelComplete($"{completeText}\n\nFragmento do relógio recuperado: {Mathf.Clamp(levelNumber, 1, 10)}/10\nBorboletas do tempo: {pollen}/{GameProgress.PollenPerLevel}");
            GameAudio.Play(Sfx.Complete);
            yield return new WaitForSeconds(completeDelay);
            if (ctx.Hud != null) yield return ctx.Hud.FadeOut(0.6f);
            LoadNext();
        }

        private IEnumerator DeathRoutine()
        {
            var ctx = LevelContext.Current;
            if (ctx.Hud != null) ctx.Hud.ShowDeath();
            yield return new WaitForSeconds(deathDelay);
            if (ctx.Hud != null) yield return ctx.Hud.FadeOut(0.35f);

            if (ctx.Time.IsPeeking) ctx.Time.EndPeek();
            var player = ctx.Player;
            if (player != null)
            {
                player.Respawn(respawnPoint);
                var health = player.GetComponent<PlayerHealth>();
                if (health != null) health.Revive();
            }
            if (ctx.Hud != null)
            {
                ctx.Hud.HideDeath();
                yield return ctx.Hud.FadeIn(0.35f);
            }
            respawning = false;
        }

        private void LoadNext()
        {
            Time.timeScale = 1f;
            if (!string.IsNullOrEmpty(nextScene) && Application.CanStreamedLevelBeLoaded(nextScene)) SceneManager.LoadScene(nextScene);
            else SceneManager.LoadScene(0);
        }
    }
}

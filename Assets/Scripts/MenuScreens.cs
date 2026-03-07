using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace DinosBattle.UI
{
    public class MainMenuScreen : MonoBehaviour
    {
        [SerializeField] private Button          playButton;
        [SerializeField] private Button          quitButton;
        [SerializeField] private CanvasGroup     rootGroup;
        [SerializeField] private TextMeshProUGUI versionLabel;

        private void Awake()
        {
            playButton?.onClick.AddListener(() => { playButton.interactable = false; GameStateManager.Instance?.StartGame(); });
            quitButton?.onClick.AddListener(() => GameStateManager.Instance?.QuitGame());
            if (versionLabel != null) versionLabel.text = $"v{Application.version}";
        }

        private void Start()
        {
            if (rootGroup != null) StartCoroutine(FadeIn());
        }

        private IEnumerator FadeIn()
        {
            rootGroup.alpha = 0f;
            for (float t = 0; t < 0.6f; t += Time.unscaledDeltaTime)
            {
                rootGroup.alpha = t / 0.6f;
                yield return null;
            }
            rootGroup.alpha = 1f;
        }
    }

    public class PauseMenuScreen : MonoBehaviour
    {
        [SerializeField] private GameObject  pausePanel;
        [SerializeField] private CanvasGroup panelGroup;
        [SerializeField] private Button      resumeButton;
        [SerializeField] private Button      mainMenuButton;
        [SerializeField] private Button      quitButton;

        private void Awake()
        {
            resumeButton  ?.onClick.AddListener(() => GameStateManager.Instance?.ResumeGame());
            mainMenuButton?.onClick.AddListener(() => GameStateManager.Instance?.GoToMainMenu());
            quitButton    ?.onClick.AddListener(() => GameStateManager.Instance?.QuitGame());
            SetVisible(false, instant: true);
            if (GameStateManager.Instance != null)
                GameStateManager.Instance.OnStateChanged += OnStateChanged;
        }

        private void OnDestroy()
        {
            if (GameStateManager.Instance != null)
                GameStateManager.Instance.OnStateChanged -= OnStateChanged;
        }

        private void Update()
        {
            if ( GameStateManager.Instance == null) return;
            if (!GameStateManager.Instance.IsInBattle) return;

            if (GameStateManager.Instance.IsPaused) GameStateManager.Instance.ResumeGame();
            else                                    GameStateManager.Instance.PauseGame();
        }

        private void OnStateChanged(GameState _, GameState next)
        {
            if (next == GameState.Paused)                            SetVisible(true);
            if (next == GameState.Battle || next == GameState.Victory
                                         || next == GameState.Defeat) SetVisible(false);
        }

        private void SetVisible(bool on, bool instant = false)
        {
            pausePanel?.SetActive(on);
            if (panelGroup == null) return;

            if (instant) { panelGroup.alpha = on ? 1f : 0f; return; }
            StopAllCoroutines();
            StartCoroutine(FadeTo(on ? 1f : 0f));
        }

        private IEnumerator FadeTo(float target)
        {
            float start = panelGroup.alpha;
            for (float t = 0; t < 0.15f; t += Time.unscaledDeltaTime)
            {
                panelGroup.alpha = Mathf.Lerp(start, target, t / 0.15f);
                yield return null;
            }
            panelGroup.alpha = target;
        }
    }
}

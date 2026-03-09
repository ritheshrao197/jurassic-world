using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace DinosBattle.UI.Screens
{
    /// <summary>
    /// Main Menu screen controller.
    /// Attach to the root Canvas in the MainMenu scene.
    /// Buttons are wired in Awake via code — no Inspector event wiring needed.
    /// </summary>
    public class MainMenuScreen : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private Button playButton;
        [SerializeField] private Button quitButton;

        [Header("Animated Elements")]
        [SerializeField] private CanvasGroup rootGroup;       // for fade-in
        [SerializeField] private RectTransform titleRect;     // for slide-in

        [Header("Version")]
        [SerializeField] private TextMeshProUGUI versionLabel;

        private void Awake()
        {
            if (playButton != null) playButton.onClick.AddListener(OnPlayClicked);
            if (quitButton != null) quitButton.onClick.AddListener(OnQuitClicked);

            if (versionLabel != null)
                versionLabel.text = $"v{Application.version}";
        }


        private void OnPlayClicked()
        {
            if (playButton != null) playButton.interactable = false;
            GameStateManager.Instance?.StartGame();
        }

        private void OnQuitClicked()
        {
            GameStateManager.Instance?.QuitGame();
        }

    
    }
}
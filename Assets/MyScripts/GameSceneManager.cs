using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class GameSceneManager : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button musicOnButton;
    [SerializeField] private Button musicOfButton;
    [SerializeField] private Button sfxOnButton;
    [SerializeField] private Button sfxOfButton;

    [Header("UI Panels & Fields")]
    public GameObject homeScreenPanel;
    public GameObject playerProfilePanel;
    public GameObject flagSelectionPanel;
    public GameObject rodSelectionPanel;
    public GameObject hookSelectionPanel;
    public GameObject pausePanelPanel;
    public GameObject settingPanel;
    public TMP_InputField writeYourName;

    [Header("Control Buttons")]
    public Button pauseButton;
    public Button settingButton;

    [Header("Flag Highlighting System")]
    public Button[] flagButtons;
    public GameObject[] highlightBorders;

    private int selectedIndex = -1;

    void Start()
    {
        // Handle Banner Ads
        if (AdsManager.Instance != null)
        {
            AdsManager.Instance.BannerLeftHandler?.Show();
            AdsManager.Instance.BannerRightHandler?.Show();
        }

        // Assign Flag click listeners
        for (int i = 0; i < flagButtons.Length; i++)
        {
            int index = i;
            flagButtons[i].onClick.AddListener(() => OnFlagClicked(index));
        }

        // Ensure all borders are hidden at start
        for (int i = 0; i < highlightBorders.Length; i++)
        {
            highlightBorders[i].SetActive(false);
        }

        // Audio button listeners
        if (musicOnButton != null && musicOfButton != null && sfxOnButton != null && sfxOfButton != null)
        {
            musicOnButton.onClick.AddListener(MusicOnButtonPressed);
            musicOfButton.onClick.AddListener(MusicOffButtonPressed);
            sfxOnButton.onClick.AddListener(SFXOnButtonPressed);
            sfxOfButton.onClick.AddListener(SFXOffButtonPressed);
            InitAudioButtonStates();
        }
    }

    #region Audio Settings Code

    private void InitAudioButtonStates()
    {
        bool musicMuted = PlayerPrefs.GetInt("MusicMuted", 0) == 1;
        bool sfxMuted = PlayerPrefs.GetInt("SFXMuted", 0) == 1;

        MusicManager.Instance.SetMuted(musicMuted);
        SFXManager.Instance.SetMuted(sfxMuted);

        UpdateAudioButtonVisuals(musicMuted, sfxMuted);
    }

    public void MusicOnButtonPressed()
    {
        MusicManager.Instance.SetMuted(false);
        PlayerPrefs.SetInt("MusicMuted", 0);
        UpdateAudioButtonVisuals(false, PlayerPrefs.GetInt("SFXMuted", 0) == 1);
    }

    public void MusicOffButtonPressed()
    {
        MusicManager.Instance.SetMuted(true);
        PlayerPrefs.SetInt("MusicMuted", 1);
        UpdateAudioButtonVisuals(true, PlayerPrefs.GetInt("SFXMuted", 0) == 1);
    }

    public void SFXOnButtonPressed()
    {
        SFXManager.Instance.SetMuted(false);
        PlayerPrefs.SetInt("SFXMuted", 0);
        UpdateAudioButtonVisuals(PlayerPrefs.GetInt("MusicMuted", 0) == 1, false);
    }

    public void SFXOffButtonPressed()
    {
        SFXManager.Instance.SetMuted(true);
        PlayerPrefs.SetInt("SFXMuted", 1);
        UpdateAudioButtonVisuals(PlayerPrefs.GetInt("MusicMuted", 0) == 1, true);
    }

    private void UpdateAudioButtonVisuals(bool musicMuted, bool sfxMuted)
    {
        // Music: On button interactable when muted; Off button interactable when playing
        musicOnButton.interactable = musicMuted;
        musicOfButton.interactable = !musicMuted;

        // SFX: same logic
        sfxOnButton.interactable = sfxMuted;
        sfxOfButton.interactable = !sfxMuted;
    }

    #endregion

    #region Home Screen Buttons

    public void PlayButtonPressed()
    {
        playerProfilePanel.gameObject.SetActive(true);
        homeScreenPanel.gameObject.SetActive(false);
    }

    public void PrivacyPolicyButtonPressed()
    {
        Application.OpenURL("https://samtechnologiesgamingstudio.blogspot.com/p/privacy-policy.html?m=1");
    }

    public void ExitButtonPressed()
    {
        Application.Quit();
        Debug.Log("Game is Exited");
    }

    #endregion

    #region Player Profile Screen Buttons

    public void PlayerProfileSaveButtonPresed()
    {
        playerProfilePanel.gameObject.SetActive(false);
        flagSelectionPanel.gameObject.SetActive(true);
        PlayerPrefs.SetString("UserName", writeYourName.text);
        PlayerPrefs.Save();
    }

    public void PlayerProfileBackButtonPressed()
    {
        playerProfilePanel.gameObject.SetActive(false);
        homeScreenPanel.gameObject.SetActive(true);
    }

    #endregion

    #region Flag Selection Screen & System

    public void FlagScreenSaveButtonPresed()
    {
        flagSelectionPanel.gameObject.SetActive(false);
        rodSelectionPanel.gameObject.SetActive(true);
    }

    public void FlagScreenBackButtonPressed()
    {
        playerProfilePanel.gameObject.SetActive(true);
        flagSelectionPanel.gameObject.SetActive(false);
    }

    void OnFlagClicked(int index)
    {
        for (int i = 0; i < highlightBorders.Length; i++)
        {
            highlightBorders[i].SetActive(false);
        }

        highlightBorders[index].SetActive(true);
        selectedIndex = index;
        Debug.Log("Selected flag index: " + selectedIndex);
    }

    #endregion

    #region Rod & Hook Selection Screens

    public void RodSelection()
    {
        rodSelectionPanel.gameObject.SetActive(false);
        hookSelectionPanel.gameObject.SetActive(true);
    }

    public void HookSelection()
    {
        SceneManager.LoadScene("GamePlayScene");
    }

    #endregion

    #region Pause Panel Buttons

    public void PauseButtonPressed()
    {
        pausePanelPanel.gameObject.SetActive(true);
    }

    public void PauseResumeButtonPressed()
    {
        pausePanelPanel.gameObject.SetActive(false);
    }

    public void PauseHomeButtonPressed()
    {
        SceneManager.LoadScene("HomeScene");
    }

    #endregion

    #region Setting Panel Buttons

    public void SettingButtonPressed()
    {
        settingPanel.gameObject.SetActive(true);
    }

    public void SettingSaveButtonPressed()
    {
        settingPanel.gameObject.SetActive(false);
        homeScreenPanel.gameObject.SetActive(true);
    }

    #endregion
}

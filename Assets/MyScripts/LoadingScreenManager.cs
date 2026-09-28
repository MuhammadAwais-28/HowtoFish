using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoadingScreenManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Slider loadingSlider;

    [Header("Loading Settings")]
    [Tooltip("How long in seconds should the fake loading take to fill?")]
    [SerializeField] private float loadingDuration = 3.0f;

    [Header("Target Scene")]
    [SerializeField] private string targetSceneName = "HomeScene";

    void Start()
    {
        // Safety check to ensure a slider is attached
        if (loadingSlider != null)
        {
            loadingSlider.value = 0f;
            StartCoroutine(StartLoadingProcess());
        }
        else
        {
            Debug.LogError("Please assign the Loading Slider in the Inspector!");
            // Fallback: If no slider is found, load the scene immediately so the game doesn't get stuck
            SceneManager.LoadScene(targetSceneName);
        }
    }

    private IEnumerator StartLoadingProcess()
    {
        float elapsedTime = 0f;

        // Smoothly update the loading bar fill based on time elapsed
        while (elapsedTime < loadingDuration)
        {
            elapsedTime += Time.deltaTime;

            // Calculates a percentage value between 0 and 1
            loadingSlider.value = Mathf.Clamp01(elapsedTime / loadingDuration);

            yield return null; // Wait for the next frame
        }

        // Ensure it visually hits exactly 100% full before moving on
        loadingSlider.value = 1f;

        // Wait a tiny split second at 100% for smooth visual impact
        yield return new WaitForSeconds(0.2f);

        // Load your main home screen panel scene
        SceneManager.LoadScene(targetSceneName);
    }
}

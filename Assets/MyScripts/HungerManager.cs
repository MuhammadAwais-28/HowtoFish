using UnityEngine;
using UnityEngine.UI;

public class HungerManager : MonoBehaviour
{
    public float currentHunger = 100f;
    public float maxHunger = 100f;
    public float consumeRate = 1f; 

    [Header("Slider Reference")]
    public Slider hungerSlider;

    void Start()
    {
        // Set the slider's max limits dynamically
        hungerSlider.maxValue = maxHunger;
        hungerSlider.value = currentHunger;
    }

    void Update()
    {
        ConsumeHunger();
    }

    void ConsumeHunger()
    {
        // Time.deltaTime ensures the rate is per second, not per frame
        currentHunger -= consumeRate * Time.deltaTime;

        // Prevent hunger from dropping below zero
        currentHunger = Mathf.Max(currentHunger, 0f);

        // Update the visual UI slider
        hungerSlider.value = currentHunger;
    }


    public void AddHunger(float amount)
    {
        currentHunger += amount;
        currentHunger = Mathf.Clamp(currentHunger, 0f, maxHunger);
        hungerSlider.value = currentHunger;
    }
}

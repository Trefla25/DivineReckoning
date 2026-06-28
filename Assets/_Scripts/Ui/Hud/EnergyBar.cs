using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.ParticleSystem;

[RequireComponent(typeof(Slider))]
public class EnergyBar : MonoBehaviour
{
    [SerializeField] Slider slider;
    [SerializeField] TMP_Text label;

    [Header("LazyBar")]
    [SerializeField] Slider lazySlider;
    [SerializeField] float lazyDelay = 1f;

    float lazyCatchupTime = 0f;

    private void OnValidate()
    {
        if (slider == null)
        {
            slider = GetComponent<Slider>();
        }
    }

    public void SetValues(float current, float max)
    {
        slider.maxValue = max;

        if (label != null)
        {
            label.text = $"{Mathf.CeilToInt(current)}/{Mathf.RoundToInt(max)}";
        }

        if (lazySlider is null)
        {
            slider.value = current;
            return;
        }

        lazySlider.maxValue = max;

        if (current < slider.value)
        {
            lazyCatchupTime = Time.time + lazyDelay;
        }
        else
        {
            lazySlider.value = current;
        }

        slider.value = current;
    }


    private void Update()
    {
        if (lazySlider is null) return;

        if (Time.time >= lazyCatchupTime)
        {
            lazySlider.value = Mathf.Lerp(lazySlider.value, slider.value, Time.deltaTime * 10f);
        }
    }
}

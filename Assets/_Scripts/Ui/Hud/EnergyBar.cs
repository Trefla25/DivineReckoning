using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class EnergyBar : MonoBehaviour
{
    [SerializeField] Slider slider;

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

    private void Update()
    {
        if (lazySlider is null) return;

        if (Time.time >= lazyCatchupTime)
        {
            lazySlider.value = Mathf.Lerp(lazySlider.value, slider.value, Time.deltaTime * 10f);
        }
    }

    public void SetMaxPoints(float maxPoints)
    {
        slider.maxValue = maxPoints;

        if (lazySlider is null) return;

        lazySlider.maxValue = maxPoints;
    }

    public void SetPoints(float points)
    {
        slider.value = points;

        if (lazySlider is null) return;
        
        lazySlider.value = points;
        
    }

    public void DamagePoints(float damage)
    {
        slider.value -= damage;

        if (lazySlider is null) return;

        lazyCatchupTime = Time.time + lazyDelay;
    }

    public void RestorePoints(float points)
    {
        slider.value += points;

        if (lazySlider is null) return;
        
        lazySlider.value = slider.value;
        
    }
}

using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;
    void Awake() => instance = this;

    [Header("Hitmarker")]
    [SerializeField] CanvasGroup hitmarker;
    [SerializeField] float hitmarkerAlphaDecay = 1f;
    
    void Update()
    {
        HandleHitmarkerDecay();
    }
    
    void HandleHitmarkerDecay() 
    {
        if (hitmarker.alpha <= 0f) return;
        
        hitmarker.alpha = Mathf.Lerp(hitmarker.alpha, 0f, hitmarkerAlphaDecay * Time.deltaTime);
    }
    
    public void DisplayHitmarker() 
    {
        hitmarker.alpha = 1f;
    }
}

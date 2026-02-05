using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ToggleSwitch : MonoBehaviour, IPointerClickHandler
{
    [SerializeField, Range(0, 1)] private float m_sliderValue;
    [SerializeField, Range(0, 1)] public float m_animationDuration = 0.5f;
    [SerializeField] AnimationCurve m_animation = AnimationCurve.EaseInOut(timeStart: 0, valueStart: 0, timeEnd: 1, valueEnd: 1);
    [SerializeField] private UnityEvent m_ontoggleOn;
    [SerializeField] private UnityEvent m_ontoggleOff;
    public bool CurrentValue { get; private set; }
    private Slider m_sliders;
    private Coroutine m_Coroutine;
    
    //private ToggleSwitchGroupManager 
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void OnPointerClick(PointerEventData eventData)
    {

    }
}

using System;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUIController : MonoBehaviour
{
    [Header("References:")]
    [SerializeField]
    private Text timerFront;

    [SerializeField]
    private Text timerBack;

    [SerializeField]
    private Transform clockArmHinge;

    [SerializeField]
    private Image fill_CleanAmount;

    private void OnEnable()
    {
        HouseManager.OnTimeChanged += UpdateTimer;
    }

    private void Update()
    {
        fill_CleanAmount.fillAmount = HouseManager.CleanPercentage;    
    }

    private void OnDestroy() 
    {
        HouseManager.OnTimeChanged -= UpdateTimer;
    }

    public void UpdateTimer(TimeSpan timeRemaining) 
    {
        const int END_ROTATION = -360;
        const float SECOND_BASED_ROTATION = 1.2f;

        string timeLeftText = timeRemaining.Minutes.ToString() + ':' + (timeRemaining.Seconds < 10 ? 0.ToString() : "") + timeRemaining.Seconds.ToString();

        Vector3 rotation = Vector3.zero;
        rotation.z = -END_ROTATION + (SECOND_BASED_ROTATION * (float)timeRemaining.TotalSeconds);

        clockArmHinge.transform.localEulerAngles = rotation;

        timerFront.text = timeLeftText;
        timerBack.text = timeLeftText;
    }
}

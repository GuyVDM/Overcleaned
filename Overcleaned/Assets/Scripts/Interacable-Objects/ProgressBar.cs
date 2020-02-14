using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Animator))]
public class ProgressBar : MonoBehaviour
{
    private static float tooltipUpdateTime = 0.4f;

    private const string TOOLTIP_COMPLETIONTEXT = "Success!";
    private const string ANIM_POPUPSTATENAME = "Popup";
    private const float MAX_FILLAMOUNT = 1;
    
    [SerializeField]
    private Text progressTooltip;

    [SerializeField]
    private Image fillImage;

    [SerializeField]
    private Color starting_Color, accomplished_Color;

    #region ### Properties ###
    private Animator m_Animator;
    private Animator ThisAnimator => m_Animator ?? (m_Animator = GetComponent<Animator>());
    #endregion

    #region ### Hidden Variables ###
    private string startingContentTooltip = "";
    #endregion

    private void Awake() => startingContentTooltip = progressTooltip.text;

    private void OnEnable()
    {
        fillImage.fillAmount = 0;
        StartCoroutine(nameof(UpdateTooltip));
        fillImage.color = starting_Color;
        progressTooltip.text = startingContentTooltip;
        ThisAnimator.SetBool(ANIM_POPUPSTATENAME, true);
    }

    private void OnDisable() 
    {
        StopCoroutine(nameof(UpdateTooltip));
        ThisAnimator.SetBool(ANIM_POPUPSTATENAME, false);
    }

    private void Update()
    {
        //TODO: Update fillamount based on connected interactable;
        if(Mathf.Approximately(fillImage.fillAmount, MAX_FILLAMOUNT)) 
        {
            fillImage.color = accomplished_Color;
            progressTooltip.text = TOOLTIP_COMPLETIONTEXT;
            enabled = false;
        }
    }

    private IEnumerator UpdateTooltip() 
    {
        string[] states = new string[] { ".", "..", "..." };
        const int ADDON = 1;
        int index = 0;

        while (enabled)
        {
            yield return new WaitForSeconds(tooltipUpdateTime);

            index = (index == states.Length - 1) ? 0 : index + ADDON;
            progressTooltip.text = startingContentTooltip + states[index];
        }
    }
}

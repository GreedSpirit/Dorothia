using UnityEngine;
using UnityEngine.UI;

public class MysteryGauge : MonoBehaviour
{
    private Animator anim;
    private Toggle toggle;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        toggle = GetComponent<Toggle>();
    }

    private void OnEnable()
    {
        SkillManager.Instance.OnMysteryGaugeChanged += UpdateAnimation;
        toggle.onValueChanged.AddListener(OnToggleValueChanged);

        UpdateAnimation(SkillManager.Instance.MysteryGauge);
    }

    private void OnDisable()
    {
        if (SkillManager.Instance != null)
            SkillManager.Instance.OnMysteryGaugeChanged -= UpdateAnimation;

        toggle.onValueChanged.RemoveListener(OnToggleValueChanged);
    }

    private void UpdateAnimation(float gauge)
    {
        bool isFull = (gauge >= SkillManager.MYSTERY_LIMIT);

        anim.SetBool("IsFull", isFull);

        if (!isFull)
        {
            toggle.isOn = false;
            toggle.interactable = false;
            anim.SetBool("IsOn", false); 
        }
        else
        {
            toggle.interactable = true;
        }
    }

    private void OnToggleValueChanged(bool isOn)
    {
        if (toggle.interactable)
        {
            anim.SetBool("IsOn", isOn);
        }
        else
        {
            // 이벤트 알림 없이 값만 바꾸기
            if (isOn) toggle.SetIsOnWithoutNotify(false);
        }
    }
}
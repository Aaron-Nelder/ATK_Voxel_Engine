using UnityEngine;
using UnityEngine.UI;

public class PlayerHudManager : MonoBehaviour
{
    [SerializeField] Image staminaBar;

    void OnEnable()
    {
        PlayerManager.Instance.MotionHandler.StamController.OnStaminaChanged += OnStaminaChanged;
    }

    void OnDisable()
    {
        PlayerManager.Instance.MotionHandler.StamController.OnStaminaChanged -= OnStaminaChanged;
    }

    void OnStaminaChanged(float current, float max)
    {
        staminaBar.fillAmount = current / max;
    }
}

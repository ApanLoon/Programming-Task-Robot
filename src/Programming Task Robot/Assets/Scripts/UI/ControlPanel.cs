using TMPro;
using UnityEngine;

public class ControlPanel : MonoBehaviour
{
    private TMP_InputField _input;

    public void Activate()
    {
        GameController.Instance.ActivateRobot(_input.text);
    }

    private void Start()
    {
        _input = GetComponentInChildren<TMP_InputField>();
    }
}

using UnityEngine;

public class GameController : MonoBehaviour
{
    public static GameController Instance { get; private set; }

    [SerializeField] protected RobotController RobotController;

    private void OnEnable()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError($"GameController: More than one instance detected. Disabling {gameObject.name}.");
            gameObject.SetActive (false);
            return;
        }

        Instance = this;
    }

    public void ActivateRobot(string s)
    {
        RobotController.Activate(s);
    }
}

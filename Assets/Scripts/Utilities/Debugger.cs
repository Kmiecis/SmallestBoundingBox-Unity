using UnityEngine;

public class Debugger : MonoBehaviour
{
    #region Singleton

    public static Debugger Get;

    private void Awake()
    {
        if (Get == null)
        {
            Get = this;
        }
        else if (Get != this)
        {
            Destroy(this);
        }
    }

    private void OnValidate()
    {
        Awake();
    }

    #endregion

    public DebugOption[] debugOptions;


    public void Log(object message, DebugOption debugOption)
    {
        if (CheckDebugOption(debugOption))
        {
            Debug.Log(debugOption.ToString() + ": " + message);
        }
    }


    public void Warning(object message, DebugOption debugOption)
    {
        if (CheckDebugOption(debugOption))
        {
            Debug.LogWarning(debugOption.ToString() + ": " + message);
        }
    }


    public void Error(object message, DebugOption debugOption)
    {
        if (CheckDebugOption(debugOption))
        {
            Debug.LogError(debugOption.ToString() + ": " + message);
        }
    }


    bool CheckDebugOption(DebugOption debugOption)
    {
        foreach (DebugOption option in debugOptions)
        {
            if (option == debugOption)
            {
                return true;
            }
        }

        return false;
    }


    private void Start()
    {
        Tester.TestExtMath();
    }
}

public enum DebugOption
{
    DataCloud,
    ExtMath,
    CH2,
    CH3,
    SBB2,
    SBB3
}
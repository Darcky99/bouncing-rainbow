using KobGamesSDKSlim;
using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TestingButtons : MonoBehaviour
{
    private int m_UpdatesBeforeException;
    private bool m_IsSimulateNonFatalEnabled = false;

    public void SimulateLevelFailed()
    {
        Debug.Log($"{Utils.GetFuncName()}()");
        Managers.Instance.SimulateLevelFailed();
    }

    public void SimulateLevelCompleted()
    {
        Debug.Log($"{Utils.GetFuncName()}()");
        Managers.Instance.SimulateLevelCompleted();
    }    

    [DllImport("kernel32.dll")]
    static extern void RaiseException(uint dwExceptionCode, uint dwExceptionFlags, uint nNumberOfArguments, IntPtr lpArguments);
    public void SimulateFatalCrash()
    {
        Debug.Log($"{Utils.GetFuncName()}()");

        KillMe();
        //RaiseException(13, 0, 0, new IntPtr(1));
        //UnityEngine.Diagnostics.Utils.ForceCrash(UnityEngine.Diagnostics.ForcedCrashCategory.FatalError);

        //AndroidJavaClass myClass = new AndroidJavaClass("com.companyName.productName.CrashMe");

        //myClass.Call("testMethod", new object[] { "testString" });
    }

    public void KillMe()
    {
        while (true)
        {
            for (int i = 1; i < 100; i++)
            {
                Invoke("KillMe", i / 10000);
            }
        }
    }

    public void SimulateNonFatal()
    {
        Debug.Log($"{Utils.GetFuncName()}()");

        //Firebase.FirebaseApp.LogLevel = Firebase.LogLevel.Debug;

        m_IsSimulateNonFatalEnabled = true;
        m_UpdatesBeforeException = 0;
    }

    public void StopSimulateNonFatal()
    {
        m_IsSimulateNonFatalEnabled = false;
    }

    public void ReloadScene()
    {
        Debug.Log($"{Utils.GetFuncName()}()");
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void Update()
    {
        if (m_IsSimulateNonFatalEnabled)
        {
            throwExceptionEvery60Updates();
        }
    }

    void throwExceptionEvery60Updates()
    {
        if (m_UpdatesBeforeException > 0)
        {
            m_UpdatesBeforeException--;
        }
        else
        {
            // Set the counter to 60 updates
            m_UpdatesBeforeException = 60;

            // Throw an exception to test your Crashlytics implementation
            throw new System.Exception("test exception please ignore");
        }
    }
}

using IngameDebugConsole;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InitialScene : MonoBehaviour
{
    static bool useSampleScene = false;

    void Start()
    {
        var launchScene = PlayerPrefs.GetString("launch-scene", string.Empty);
        if (launchScene == "example")
        {
            SceneManager.LoadScene("SampleScene");
            useSampleScene = true;
        }
        else
        {
            SceneManager.LoadScene("DynamicTest");
            useSampleScene = false;
        }
    }

    [ConsoleMethod("toggleScene", "Creates a cube at specified position")]
    public static void ToggleScene()
    {
        useSampleScene = !useSampleScene;
        PlayerPrefs.SetString("launch-scene", useSampleScene ? "example" : string.Empty);
        PlayerPrefs.Save();

        if (useSampleScene)
        {
            SceneManager.LoadScene("SampleScene");
            useSampleScene = true;
        }
        else
        {
            SceneManager.LoadScene("DynamicTest");
            useSampleScene = false;
        }
	}
}

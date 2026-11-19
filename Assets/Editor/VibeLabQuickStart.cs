using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class VibeLabQuickStart
{
    private const string SessionKey = "VibeLab.QuickStart.RanThisSession";

    static VibeLabQuickStart()
    {
        EditorApplication.delayCall += RunOnceAfterProjectLoad;
    }

    [MenuItem("VibeLab/Open Demo Scene")]
    public static void OpenDemoScene()
    {
        VibeLabAndroidExport.OpenDemoScene();
        Debug.Log("[VibeLab] Demo 场景已打开，直接点击 Play 即可运行。");
    }

    private static void RunOnceAfterProjectLoad()
    {
        if (SessionState.GetBool(SessionKey, false))
        {
            return;
        }

        SessionState.SetBool(SessionKey, true);
        VibeLabAndroidExport.EnsureDemoProjectReady(openSceneAfterPrepare: false);
        TryOpenDemoSceneForFirstUse();
        Debug.Log("[VibeLab] 已自动准备好 Demo 场景与 Android 设置。直接点击 Play 即可运行；如果你切走了场景，可用菜单 VibeLab/Open Demo Scene 再次打开。");
    }

    private static void TryOpenDemoSceneForFirstUse()
    {
        var activeScene = EditorSceneManager.GetActiveScene();
        var shouldOpenDemoScene = string.IsNullOrWhiteSpace(activeScene.path) || activeScene.path != VibeLabAndroidExport.ScenePath;

        if (!shouldOpenDemoScene)
        {
            return;
        }

        if (activeScene.isDirty && !string.IsNullOrWhiteSpace(activeScene.path))
        {
            return;
        }

        EditorSceneManager.OpenScene(VibeLabAndroidExport.ScenePath, OpenSceneMode.Single);
    }
}

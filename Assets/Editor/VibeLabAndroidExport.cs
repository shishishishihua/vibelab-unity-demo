using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class VibeLabAndroidExport
{
    public const string SceneDirectory = "Assets/Scenes";
    public const string ScenePath = SceneDirectory + "/VibeLabBootstrap.unity";
    public const string DefaultPackageName = "com.vibelab.unitydemo";

    [MenuItem("VibeLab/Prepare Demo Project")]
    public static void PrepareDemoProject()
    {
        PrepareDemoProjectInternal(openSceneAfterPrepare: false, verbose: true);
    }

    public static bool EnsureDemoProjectReady(bool openSceneAfterPrepare)
    {
        return PrepareDemoProjectInternal(openSceneAfterPrepare, verbose: false);
    }

    public static void OpenDemoScene()
    {
        PrepareDemoProjectInternal(openSceneAfterPrepare: true, verbose: false);
    }

    [MenuItem("VibeLab/Validate Submission Settings")]
    public static void ValidateSubmissionSettings()
    {
        PrepareDemoProject();

        var unityVersion = Application.unityVersion;
        var packageName = PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.Android);
        var scriptingBackend = PlayerSettings.GetScriptingBackend(BuildTargetGroup.Android);
        var architectures = PlayerSettings.Android.targetArchitectures;
        var targetSdk = PlayerSettings.Android.targetSdkVersion;
        var customKeystore = PlayerSettings.Android.useCustomKeystore;
        var sceneConfigured = EditorBuildSettings.scenes.Any(scene => scene.enabled && scene.path == ScenePath);

        var report =
            "[VibeLab] 规则检查结果\n" +
            $"- Unity 版本：{unityVersion}\n" +
            $"- Android 包名：{packageName}\n" +
            $"- Scripting Backend：{scriptingBackend}\n" +
            $"- 目标架构：{architectures}\n" +
            $"- targetSdk：{targetSdk}\n" +
            $"- Custom Keystore：{customKeystore}\n" +
            $"- 导出场景：{sceneConfigured}\n" +
            $"- 检查结论：{BuildConclusion(unityVersion, packageName, scriptingBackend, architectures, targetSdk, customKeystore, sceneConfigured)}";

        Debug.Log(report);
        EditorUtility.DisplayDialog("VibeLab 校验完成", report, "知道了");
    }

    [MenuItem("VibeLab/Export/Export Android Gradle Project...")]
    public static void ExportAndroidGradleProject()
    {
        PrepareDemoProject();

        var exportFolder = EditorUtility.OpenFolderPanel("选择导出的 Android Gradle 目录（必须为空且位于 Unity 工程外）", string.Empty, string.Empty);
        if (string.IsNullOrWhiteSpace(exportFolder))
        {
            return;
        }

        var fullExportFolder = Path.GetFullPath(exportFolder);
        var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));

        if (fullExportFolder.StartsWith(projectRoot, StringComparison.OrdinalIgnoreCase))
        {
            EditorUtility.DisplayDialog("导出被拒绝", "导出目录必须位于 Unity 工程外部。", "知道了");
            return;
        }

        if (Directory.EnumerateFileSystemEntries(fullExportFolder).Any())
        {
            EditorUtility.DisplayDialog("导出被拒绝", "导出目录必须是空目录。", "知道了");
            return;
        }

        var options = new BuildPlayerOptions
        {
            scenes = new[] { ScenePath },
            target = BuildTarget.Android,
            locationPathName = fullExportFolder,
            options = BuildOptions.AcceptExternalModificationsToPlayer
        };

        BuildReport buildReport = BuildPipeline.BuildPlayer(options);
        BuildSummary summary = buildReport.summary;

        if (summary.result != BuildResult.Succeeded)
        {
            EditorUtility.DisplayDialog("导出失败", $"Android Gradle 导出失败：{summary.result}", "知道了");
            return;
        }

        WriteNextStepNote(fullExportFolder);
        EditorUtility.DisplayDialog(
            "导出完成",
            "Android Gradle 工程已导出。\n\n下一步请在导出的目录上执行 IL2CPP 预编译、删除 local.properties、清空 unityProjectPath / unity.projectPath，并完成 :launcher:assembleRelease 终验。",
            "好的");
    }

    private static bool PrepareDemoProjectInternal(bool openSceneAfterPrepare, bool verbose)
    {
        var changed = false;
        changed |= EnsureSceneExists();
        changed |= ConfigureAndroidPlayerSettings();

        if (openSceneAfterPrepare)
        {
            var activeScene = EditorSceneManager.GetActiveScene();
            if (activeScene.path != ScenePath)
            {
                EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            }
        }

        if (changed)
        {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        if (verbose)
        {
            Debug.Log("[VibeLab] 项目准备完成：场景、Android Player Settings、Build Settings 已更新。");
        }

        return changed;
    }

    private static bool EnsureSceneExists()
    {
        var changed = false;

        if (!Directory.Exists(SceneDirectory))
        {
            Directory.CreateDirectory(SceneDirectory);
            changed = true;
        }

        if (!File.Exists(ScenePath))
        {
            var previousScenePath = EditorSceneManager.GetActiveScene().path;
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var marker = new GameObject("VibeLabRuntimeScene");
            marker.transform.position = Vector3.zero;
            EditorSceneManager.SaveScene(scene, ScenePath);
            changed = true;

            if (!string.IsNullOrWhiteSpace(previousScenePath) && File.Exists(previousScenePath))
            {
                EditorSceneManager.OpenScene(previousScenePath, OpenSceneMode.Single);
            }
        }

        if (EditorBuildSettings.scenes.Length != 1 || EditorBuildSettings.scenes[0].path != ScenePath || !EditorBuildSettings.scenes[0].enabled)
        {
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(ScenePath, true)
            };
            changed = true;
        }

        return changed;
    }

    private static bool ConfigureAndroidPlayerSettings()
    {
        var changed = false;

        changed |= AssignIfDifferent(() => PlayerSettings.productName, value => PlayerSettings.productName = value, "VibeLab Unity Demo");
        changed |= AssignIfDifferent(() => PlayerSettings.companyName, value => PlayerSettings.companyName = value, "VibeLab");
        changed |= AssignIfDifferent(() => PlayerSettings.bundleVersion, value => PlayerSettings.bundleVersion = value, "1.0.0");
        changed |= AssignIfDifferent(() => PlayerSettings.Android.bundleVersionCode, value => PlayerSettings.Android.bundleVersionCode = value, 1);
        changed |= AssignIfDifferent(() => PlayerSettings.Android.useCustomKeystore, value => PlayerSettings.Android.useCustomKeystore = value, false);
        changed |= AssignIfDifferent(
            () => PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.Android),
            value => PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, value),
            DefaultPackageName);
        changed |= AssignIfDifferent(
            () => PlayerSettings.GetScriptingBackend(BuildTargetGroup.Android),
            value => PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, value),
            ScriptingImplementation.IL2CPP);
        changed |= AssignIfDifferent(() => PlayerSettings.Android.targetArchitectures, value => PlayerSettings.Android.targetArchitectures = value, AndroidArchitecture.ARM64);

        if (TryGetAllowedTargetSdk(out var targetSdk))
        {
            changed |= AssignIfDifferent(() => PlayerSettings.Android.targetSdkVersion, value => PlayerSettings.Android.targetSdkVersion = value, targetSdk);
        }

        return changed;
    }

    private static bool AssignIfDifferent<T>(Func<T> getter, Action<T> setter, T expected)
    {
        if (Equals(getter(), expected))
        {
            return false;
        }

        setter(expected);
        return true;
    }

    private static bool TryGetAllowedTargetSdk(out AndroidSdkVersions version)
    {
        if (TryGetSdkVersion("AndroidApiLevel36", out version))
        {
            return true;
        }

        return TryGetSdkVersion("AndroidApiLevel35", out version);
    }

    private static bool TryGetSdkVersion(string enumName, out AndroidSdkVersions version)
    {
        version = default;
        if (!Enum.IsDefined(typeof(AndroidSdkVersions), enumName))
        {
            return false;
        }

        version = (AndroidSdkVersions)Enum.Parse(typeof(AndroidSdkVersions), enumName);
        return true;
    }

    private static string BuildConclusion(
        string unityVersion,
        string packageName,
        ScriptingImplementation scriptingBackend,
        AndroidArchitecture architectures,
        AndroidSdkVersions targetSdk,
        bool customKeystore,
        bool sceneConfigured)
    {
        var unityOk = unityVersion.StartsWith("6000.", StringComparison.OrdinalIgnoreCase);
        var packageOk = !string.IsNullOrWhiteSpace(packageName) && packageName != "com.DefaultCompany.New Unity Project";
        var backendOk = scriptingBackend == ScriptingImplementation.IL2CPP;
        var architectureOk = architectures == AndroidArchitecture.ARM64;
        var sdkOk = targetSdk.ToString().Contains("35") || targetSdk.ToString().Contains("36");
        var keystoreOk = !customKeystore;

        return unityOk && packageOk && backendOk && architectureOk && sdkOk && keystoreOk && sceneConfigured
            ? "通过（仍需在导出后的 Gradle 工程上完成 IL2CPP 预编译与最终 assembleRelease 验证）"
            : "未完全通过，请按 Console 输出项逐条修正";
    }

    private static void WriteNextStepNote(string exportFolder)
    {
        var content =
            "VibeLab 导出后下一步\n" +
            "1. 不要把 Unity 源工程作为最终提交物；提交的是这里导出的 Gradle 工程。\n" +
            "2. 删除 local.properties。\n" +
            "3. 清空 gradle.properties 中 unityProjectPath / unity.projectPath 的值。\n" +
            "4. 用 :unityLibrary:buildIl2Cpp --rerun 做 IL2CPP 预编译。\n" +
            "5. 终验时显式设置 ANDROID_HOME / ANDROID_SDK_ROOT，再执行 :launcher:assembleRelease。\n" +
            "6. 确认最终只产出一个 release APK。\n";

        File.WriteAllText(Path.Combine(exportFolder, "VIBELAB_EXPORT_NOTE.txt"), content);
    }
}

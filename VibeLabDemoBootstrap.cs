using System.Collections.Generic;
using UnityEngine;

public sealed class VibeLabDemoBootstrap : MonoBehaviour
{
    private static readonly string[] ChecklistItems =
    {
        "Unity 6（6000.x）打开工程",
        "Android 使用 IL2CPP",
        "只勾选 arm64-v8a",
        "包名不可沿用模板默认值",
        "导出到 Unity 工程外的空目录",
        "最终提交物应为导出的 Gradle 仓库"
    };

    private static readonly string[] CardTitles =
    {
        "规则准备",
        "导出要求",
        "提交检查"
    };

    private static readonly string[] CardDescriptions =
    {
        "确认 Unity 版本、包名、IL2CPP 与 arm64 设置。",
        "导出到空目录，并保留 Unity 导出工程原结构。",
        "提交的是导出的 Gradle 工程，不是 Unity 源工程。"
    };

    private readonly List<Transform> animatedTargets = new List<Transform>();
    private readonly List<Vector3> animatedBasePositions = new List<Vector3>();

    private Transform focusRoot;
    private Transform worldRoot;
    private int selectedIndex;
    private bool acknowledged;
    private bool autoRotate = true;

    private GUIStyle panelStyle;
    private GUIStyle titleStyle;
    private GUIStyle subTitleStyle;
    private GUIStyle bodyStyle;
    private GUIStyle badgeStyle;
    private GUIStyle buttonStyle;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        if (FindAnyObjectByType<VibeLabDemoBootstrap>() != null)
        {
            return;
        }

        var root = new GameObject("VibeLabDemoBootstrap");
        DontDestroyOnLoad(root);
        root.AddComponent<VibeLabDemoBootstrap>();
    }

    private void Awake()
    {
        Application.targetFrameRate = 72;
        BuildWorld();
        CreateCameraIfNeeded();
        CreateLightingIfNeeded();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            selectedIndex = (selectedIndex + CardTitles.Length - 1) % CardTitles.Length;
        }

        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            selectedIndex = (selectedIndex + 1) % CardTitles.Length;
        }

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
        {
            acknowledged = !acknowledged;
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            autoRotate = !autoRotate;
        }

        AnimateTargets();
    }

    private void OnGUI()
    {
        EnsureGuiStyles();

        var panelRect = new Rect(24f, 24f, 470f, 480f);
        GUI.Box(panelRect, GUIContent.none, panelStyle);

        var contentRect = new Rect(panelRect.x + 18f, panelRect.y + 18f, panelRect.width - 36f, panelRect.height - 36f);
        GUILayout.BeginArea(contentRect);
        GUILayout.Label("VibeLab Unity Demo", titleStyle);
        GUILayout.Label("按你提供的 VibeLab Unity 规则做的最小 3D 应用模板", subTitleStyle);
        GUILayout.Space(10f);

        var badgeColor = GUI.color;
        GUI.color = acknowledged ? new Color(0.30f, 0.86f, 0.55f, 1f) : new Color(0.96f, 0.74f, 0.20f, 1f);
        GUILayout.Label(acknowledged ? "已确认：理解导出与提交规则" : "未确认：按 Space / Enter 切换状态", badgeStyle);
        GUI.color = badgeColor;

        GUILayout.Space(12f);
        GUILayout.Label("当前选中", subTitleStyle);
        GUILayout.Label(CardTitles[selectedIndex] + " · " + CardDescriptions[selectedIndex], bodyStyle);

        GUILayout.Space(12f);
        GUILayout.Label("规则清单", subTitleStyle);
        for (var i = 0; i < ChecklistItems.Length; i++)
        {
            var prefix = i == selectedIndex ? "▶ " : "• ";
            GUILayout.Label(prefix + ChecklistItems[i], bodyStyle);
        }

        GUILayout.Space(12f);
        GUILayout.Label("交互说明", subTitleStyle);
        GUILayout.Label("A / D 或左右方向键：切换演示项", bodyStyle);
        GUILayout.Label("右键拖动 / 滚轮：旋转与缩放视角", bodyStyle);
        GUILayout.Label("R：切换展台自动旋转", bodyStyle);
        GUILayout.Label("Space / Enter：切换“已阅读规则”状态", bodyStyle);

        GUILayout.FlexibleSpace();
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("上一项", buttonStyle, GUILayout.Height(34f)))
        {
            selectedIndex = (selectedIndex + CardTitles.Length - 1) % CardTitles.Length;
        }

        if (GUILayout.Button(autoRotate ? "暂停旋转" : "继续旋转", buttonStyle, GUILayout.Height(34f)))
        {
            autoRotate = !autoRotate;
        }

        if (GUILayout.Button(acknowledged ? "取消确认" : "确认已阅读", buttonStyle, GUILayout.Height(34f)))
        {
            acknowledged = !acknowledged;
        }
        GUILayout.EndHorizontal();
        GUILayout.EndArea();
    }

    private void BuildWorld()
    {
        if (worldRoot != null)
        {
            return;
        }

        worldRoot = new GameObject("RuntimeWorld").transform;
        worldRoot.SetParent(transform, false);

        focusRoot = new GameObject("CameraFocus").transform;
        focusRoot.SetParent(worldRoot, false);
        focusRoot.position = new Vector3(0f, 1.2f, 2.2f);

        var floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
        floor.name = "Floor";
        floor.transform.SetParent(worldRoot, false);
        floor.transform.position = new Vector3(0f, -0.05f, 2.2f);
        floor.transform.localScale = new Vector3(1.2f, 1f, 1.2f);
        ApplyMaterial(floor, new Color(0.07f, 0.10f, 0.16f, 1f));

        var backWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        backWall.name = "BackWall";
        backWall.transform.SetParent(worldRoot, false);
        backWall.transform.position = new Vector3(0f, 2.2f, 6.4f);
        backWall.transform.localScale = new Vector3(9f, 3.6f, 0.2f);
        ApplyMaterial(backWall, new Color(0.10f, 0.15f, 0.26f, 1f));

        CreateStage(new Vector3(-2.5f, 0f, 1.8f), PrimitiveType.Cube, new Color(0.20f, 0.58f, 0.98f, 1f), 0);
        CreateStage(new Vector3(0f, 0f, 2.6f), PrimitiveType.Capsule, new Color(0.47f, 0.85f, 0.50f, 1f), 1);
        CreateStage(new Vector3(2.5f, 0f, 1.8f), PrimitiveType.Sphere, new Color(0.96f, 0.53f, 0.19f, 1f), 2);
    }

    private void CreateStage(Vector3 origin, PrimitiveType topPrimitive, Color accentColor, int index)
    {
        var pedestal = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        pedestal.name = CardTitles[index] + "Pedestal";
        pedestal.transform.SetParent(worldRoot, false);
        pedestal.transform.position = origin + new Vector3(0f, 0.4f, 0f);
        pedestal.transform.localScale = new Vector3(1.1f, 0.4f, 1.1f);
        ApplyMaterial(pedestal, new Color(0.14f, 0.18f, 0.28f, 1f));

        var top = GameObject.CreatePrimitive(topPrimitive);
        top.name = CardTitles[index] + "Display";
        top.transform.SetParent(worldRoot, false);
        top.transform.position = origin + new Vector3(0f, 1.4f, 0f);
        top.transform.localScale = new Vector3(0.95f, 0.95f, 0.95f);
        ApplyMaterial(top, accentColor);

        animatedTargets.Add(top.transform);
        animatedBasePositions.Add(top.transform.position);
    }

    private void CreateCameraIfNeeded()
    {
        if (Camera.main != null)
        {
            var existingOrbit = Camera.main.GetComponent<VibeLabOrbitCamera>();
            if (existingOrbit != null)
            {
                existingOrbit.Configure(focusRoot);
            }
            return;
        }

        var cameraObject = new GameObject("Main Camera");
        cameraObject.tag = "MainCamera";
        cameraObject.transform.SetParent(worldRoot, false);

        var cameraComponent = cameraObject.AddComponent<Camera>();
        cameraComponent.clearFlags = CameraClearFlags.SolidColor;
        cameraComponent.backgroundColor = new Color(0.03f, 0.04f, 0.08f, 1f);
        cameraComponent.nearClipPlane = 0.03f;
        cameraComponent.farClipPlane = 100f;
        cameraComponent.fieldOfView = 52f;

        cameraObject.AddComponent<AudioListener>();

        var orbit = cameraObject.AddComponent<VibeLabOrbitCamera>();
        orbit.Configure(focusRoot);
    }

    private void CreateLightingIfNeeded()
    {
        if (FindAnyObjectByType<Light>() != null)
        {
            return;
        }

        var lightObject = new GameObject("Directional Light");
        lightObject.transform.SetParent(worldRoot, false);
        lightObject.transform.rotation = Quaternion.Euler(38f, -26f, 0f);

        var directional = lightObject.AddComponent<Light>();
        directional.type = LightType.Directional;
        directional.intensity = 1.2f;
        directional.color = new Color(0.95f, 0.97f, 1f, 1f);

        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.22f, 0.24f, 0.32f, 1f);
    }

    private void AnimateTargets()
    {
        for (var i = 0; i < animatedTargets.Count; i++)
        {
            var target = animatedTargets[i];
            if (target == null)
            {
                continue;
            }

            if (autoRotate)
            {
                target.Rotate(0f, (28f + i * 12f) * Time.deltaTime, 0f, Space.World);
            }

            var basePosition = animatedBasePositions[i];
            var lift = Mathf.Sin(Time.time * 1.8f + i) * 0.08f;
            var selectedLift = i == selectedIndex ? 0.22f : 0f;
            target.position = basePosition + Vector3.up * (lift + selectedLift);

            var targetScale = i == selectedIndex ? Vector3.one * 1.18f : Vector3.one * 0.95f;
            target.localScale = Vector3.Lerp(target.localScale, targetScale, Time.deltaTime * 6f);
        }
    }

    private void ApplyMaterial(GameObject target, Color color)
    {
        var renderer = target.GetComponent<Renderer>();
        if (renderer == null)
        {
            return;
        }

        var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard") ?? Shader.Find("Sprites/Default");
        var material = new Material(shader);

        if (material.HasProperty("_BaseColor"))
        {
            material.SetColor("_BaseColor", color);
        }

        if (material.HasProperty("_Color"))
        {
            material.color = color;
        }

        renderer.material = material;
    }

    private void EnsureGuiStyles()
    {
        if (panelStyle != null)
        {
            return;
        }

        panelStyle = new GUIStyle(GUI.skin.box)
        {
            padding = new RectOffset(18, 18, 18, 18)
        };
        panelStyle.normal.background = MakeTexture(new Color(0.07f, 0.09f, 0.15f, 0.92f));

        titleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 24,
            fontStyle = FontStyle.Bold,
            wordWrap = true
        };
        titleStyle.normal.textColor = Color.white;

        subTitleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 15,
            fontStyle = FontStyle.Bold,
            wordWrap = true
        };
        subTitleStyle.normal.textColor = new Color(0.56f, 0.81f, 1f, 1f);

        bodyStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 14,
            wordWrap = true
        };
        bodyStyle.normal.textColor = new Color(0.90f, 0.93f, 0.98f, 1f);

        badgeStyle = new GUIStyle(GUI.skin.box)
        {
            fontSize = 14,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            padding = new RectOffset(10, 10, 8, 8)
        };
        badgeStyle.normal.textColor = Color.black;
        badgeStyle.normal.background = MakeTexture(Color.white);

        buttonStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize = 13,
            fontStyle = FontStyle.Bold
        };
    }

    private Texture2D MakeTexture(Color color)
    {
        var texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, color);
        texture.Apply();
        return texture;
    }
}

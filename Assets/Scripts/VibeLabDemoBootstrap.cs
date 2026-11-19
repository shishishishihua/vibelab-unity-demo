using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public sealed class VibeLabDemoBootstrap : MonoBehaviour
{
    private sealed class FeatureSpotlight
    {
        public FeatureSpotlight(string title, string tag, string description, string[] bullets, PrimitiveType visualShape, Color accent)
        {
            Title = title;
            Tag = tag;
            Description = description;
            Bullets = bullets;
            VisualShape = visualShape;
            Accent = accent;
        }

        public string Title { get; }
        public string Tag { get; }
        public string Description { get; }
        public string[] Bullets { get; }
        public PrimitiveType VisualShape { get; }
        public Color Accent { get; }
    }

    private sealed class SceneModule
    {
        public Transform Root;
        public Transform Display;
        public Transform Halo;
        public Transform Backplate;
        public Renderer DisplayRenderer;
        public Renderer HaloRenderer;
        public Renderer BackplateRenderer;
        public Vector3 BasePosition;
        public Vector3 FocusPoint;
    }

    private static readonly FeatureSpotlight[] Features =
    {
        new FeatureSpotlight(
            "沉浸入口",
            "ENTRY EXPERIENCE",
            "用更像正式产品发布页的方式展示欢迎信息、品牌氛围和主叙事，让 Demo 一打开就有“产品首屏”的感觉。",
            new[]
            {
                "自动生成主舞台与欢迎引导",
                "突出品牌主视觉与入口 CTA",
                "适合展厅首屏或方案汇报开场"
            },
            PrimitiveType.Capsule,
            new Color(0.27f, 0.70f, 1.00f, 1f)),
        new FeatureSpotlight(
            "能力舞台",
            "CAPABILITY STAGE",
            "把“规则校验、空间展示、演示内容”组合成更完整的产品能力板，让用户理解这个 Demo 不只是样例代码。",
            new[]
            {
                "更突出核心能力与状态信息",
                "聚合规则、亮点与演示说明",
                "适合继续扩成真实业务模块"
            },
            PrimitiveType.Cube,
            new Color(0.36f, 0.92f, 0.58f, 1f)),
        new FeatureSpotlight(
            "发布控制台",
            "DELIVERY CONSOLE",
            "把导出、检查、交付链路包装成“产品级交付面板”，让这个工程看起来更接近可上线的展示样机。",
            new[]
            {
                "更自然地承接 Android 导出流程",
                "强调可提交、可继续工程化",
                "适合作为产品 Demo 的收束页"
            },
            PrimitiveType.Sphere,
            new Color(1.00f, 0.62f, 0.25f, 1f))
    };

    private static readonly string[] Metrics =
    {
        "72 FPS 演示目标",
        "3 个产品化展项",
        "1 条最短演示路径"
    };

    private readonly List<SceneModule> modules = new List<SceneModule>();
    private readonly List<Transform> floatingDecorations = new List<Transform>();
    private readonly List<Vector3> floatingDecorationBases = new List<Vector3>();
    private readonly List<Renderer> accentRenderers = new List<Renderer>();

    private Transform worldRoot;
    private Transform focusRoot;
    private Transform heroOrb;
    private Transform heroRingLarge;
    private Transform heroRingSmall;
    private Transform heroBeaconLeft;
    private Transform heroBeaconRight;

    private int selectedIndex = 1;
    private bool autoRotate = true;
    private bool presentationMode = true;
    private bool actionTriggered;
    private float actionPulse;

    private GUIStyle glassPanelStyle;
    private GUIStyle heroTitleStyle;
    private GUIStyle sectionLabelStyle;
    private GUIStyle bodyStyle;
    private GUIStyle captionStyle;
    private GUIStyle chipStyle;
    private GUIStyle metricStyle;
    private GUIStyle buttonStyle;
    private GUIStyle secondaryButtonStyle;
    private GUIStyle highlightStyle;

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
        HandleInput();
        AnimateWorld();
    }

    private void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            selectedIndex = (selectedIndex + Features.Length - 1) % Features.Length;
        }

        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            selectedIndex = (selectedIndex + 1) % Features.Length;
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            selectedIndex = 0;
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            selectedIndex = 1;
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            selectedIndex = 2;
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            autoRotate = !autoRotate;
        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            presentationMode = !presentationMode;
        }

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
        {
            actionTriggered = !actionTriggered;
            actionPulse = 1f;
        }
    }

    private void OnGUI()
    {
        EnsureGuiStyles();

        DrawPrimaryPanel();
        DrawMetricsPanel();
        DrawControlsPanel();
    }

    private void DrawPrimaryPanel()
    {
        var panelRect = new Rect(24f, 22f, 530f, 560f);
        GUI.Box(panelRect, GUIContent.none, glassPanelStyle);

        var innerRect = new Rect(panelRect.x + 20f, panelRect.y + 18f, panelRect.width - 40f, panelRect.height - 36f);
        GUILayout.BeginArea(innerRect);

        GUILayout.Label("VibeLab Spatial Product Demo", heroTitleStyle);
        GUILayout.Label("把 Unity 提交样例提升为更像正式产品演示的空间化舞台，既能开箱即用，也更适合继续扩成真实业务。", bodyStyle);
        GUILayout.Space(10f);

        GUILayout.BeginHorizontal();
        GUILayout.Label(presentationMode ? "演示模式：ON" : "演示模式：OFF", chipStyle, GUILayout.Width(160f));
        GUILayout.Space(8f);
        GUILayout.Label(actionTriggered ? "交互状态：已激活 CTA" : "交互状态：等待触发", highlightStyle, GUILayout.Width(180f));
        GUILayout.EndHorizontal();

        GUILayout.Space(16f);
        GUILayout.Label("当前展项", sectionLabelStyle);
        GUILayout.Label(Features[selectedIndex].Title + " · " + Features[selectedIndex].Tag, metricStyle);
        GUILayout.Label(Features[selectedIndex].Description, bodyStyle);

        GUILayout.Space(10f);
        for (var i = 0; i < Features[selectedIndex].Bullets.Length; i++)
        {
            GUILayout.Label("• " + Features[selectedIndex].Bullets[i], bodyStyle);
        }

        GUILayout.Space(16f);
        GUILayout.Label("产品化升级点", sectionLabelStyle);
        GUILayout.Label("• 更强的首屏氛围：中心主舞台 + 双侧品牌灯塔 + 背景数据墙", bodyStyle);
        GUILayout.Label("• 更完整的演示结构：入口、能力、交付三段式内容组织", bodyStyle);
        GUILayout.Label("• 更自然的继续开发路径：保留导出链路，同时把视觉表达做成产品 Demo 风格", bodyStyle);

        GUILayout.Space(16f);
        GUILayout.Label("推荐操作", sectionLabelStyle);
        GUILayout.Label("先按 A / D 浏览三块内容，再按 Space 触发主操作状态，整体效果会更像一页完整产品演示。", bodyStyle);

        GUILayout.FlexibleSpace();
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("上一块", secondaryButtonStyle, GUILayout.Height(36f)))
        {
            selectedIndex = (selectedIndex + Features.Length - 1) % Features.Length;
        }

        if (GUILayout.Button("切换演示模式", secondaryButtonStyle, GUILayout.Height(36f)))
        {
            presentationMode = !presentationMode;
        }

        if (GUILayout.Button(actionTriggered ? "重置 CTA" : "触发 CTA", buttonStyle, GUILayout.Height(36f)))
        {
            actionTriggered = !actionTriggered;
            actionPulse = 1f;
        }
        GUILayout.EndHorizontal();

        GUILayout.EndArea();
    }

    private void DrawMetricsPanel()
    {
        var panelRect = new Rect(Screen.width - 320f, 24f, 296f, 236f);
        GUI.Box(panelRect, GUIContent.none, glassPanelStyle);

        var innerRect = new Rect(panelRect.x + 16f, panelRect.y + 16f, panelRect.width - 32f, panelRect.height - 32f);
        GUILayout.BeginArea(innerRect);
        GUILayout.Label("演示看板", sectionLabelStyle);
        GUILayout.Label("这个版本更接近“正式产品演示”而不是单纯样例页。", captionStyle);
        GUILayout.Space(10f);

        for (var i = 0; i < Metrics.Length; i++)
        {
            GUILayout.Label(Metrics[i], metricStyle);
            GUILayout.Space(6f);
        }

        GUILayout.Space(10f);
        GUILayout.Label("当前焦点：" + Features[selectedIndex].Title, bodyStyle);
        GUILayout.Label(autoRotate ? "舞台动画：自动播放中" : "舞台动画：手动浏览中", bodyStyle);
        GUILayout.Label(presentationMode ? "镜头状态：产品演示视角" : "镜头状态：自由查看视角", bodyStyle);
        GUILayout.EndArea();
    }

    private void DrawControlsPanel()
    {
        var panelRect = new Rect(Screen.width - 320f, Screen.height - 236f, 296f, 212f);
        GUI.Box(panelRect, GUIContent.none, glassPanelStyle);

        var innerRect = new Rect(panelRect.x + 16f, panelRect.y + 16f, panelRect.width - 32f, panelRect.height - 32f);
        GUILayout.BeginArea(innerRect);
        GUILayout.Label("控制说明", sectionLabelStyle);
        GUILayout.Label("A / D 或 ← / →：切换展项", bodyStyle);
        GUILayout.Label("1 / 2 / 3：直达指定展项", bodyStyle);
        GUILayout.Label("右键拖动 + 滚轮：旋转/缩放镜头", bodyStyle);
        GUILayout.Label("R：切换舞台自动旋转", bodyStyle);
        GUILayout.Label("Tab：切换产品演示模式", bodyStyle);
        GUILayout.Label("Space / Enter：触发主 CTA 状态", bodyStyle);
        GUILayout.Space(12f);
        GUILayout.Label("适合下一步扩展成展厅、能力发布页、控制台演示或方案汇报样机。", captionStyle);
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
        focusRoot.position = new Vector3(0f, 1.35f, 2.1f);

        BuildBackdrop();
        BuildHeroStage();
        BuildFeatureModules();
        BuildAmbientDetails();
    }

    private void BuildBackdrop()
    {
        var floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
        floor.name = "ShowFloor";
        floor.transform.SetParent(worldRoot, false);
        floor.transform.position = new Vector3(0f, -0.05f, 2.4f);
        floor.transform.localScale = new Vector3(1.65f, 1f, 1.65f);
        ApplyMaterial(floor, new Color(0.05f, 0.07f, 0.11f, 1f), false, Color.black);

        var lane = GameObject.CreatePrimitive(PrimitiveType.Cube);
        lane.name = "LightLane";
        lane.transform.SetParent(worldRoot, false);
        lane.transform.position = new Vector3(0f, 0.02f, 2.1f);
        lane.transform.localScale = new Vector3(0.7f, 0.02f, 7.2f);
        ApplyMaterial(lane, new Color(0.13f, 0.18f, 0.28f, 1f), true, new Color(0.18f, 0.40f, 0.90f, 1f));

        var backWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        backWall.name = "BackWall";
        backWall.transform.SetParent(worldRoot, false);
        backWall.transform.position = new Vector3(0f, 2.5f, 7.4f);
        backWall.transform.localScale = new Vector3(11.0f, 4.3f, 0.24f);
        ApplyMaterial(backWall, new Color(0.08f, 0.11f, 0.19f, 1f), false, Color.black);

        var upperBand = GameObject.CreatePrimitive(PrimitiveType.Cube);
        upperBand.name = "UpperBand";
        upperBand.transform.SetParent(worldRoot, false);
        upperBand.transform.position = new Vector3(0f, 3.75f, 7.1f);
        upperBand.transform.localScale = new Vector3(8.6f, 0.12f, 0.4f);
        ApplyMaterial(upperBand, new Color(0.12f, 0.18f, 0.32f, 1f), true, new Color(0.20f, 0.42f, 0.96f, 1f));

        heroBeaconLeft = CreateBeacon("BeaconLeft", new Vector3(-4.7f, 1.9f, 5.8f), new Color(0.16f, 0.61f, 1.0f, 1f));
        heroBeaconRight = CreateBeacon("BeaconRight", new Vector3(4.7f, 1.9f, 5.8f), new Color(0.16f, 0.61f, 1.0f, 1f));

        var dataStripLeft = CreateDisplayStrip(new Vector3(-3.7f, 1.65f, 6.65f), new Color(0.14f, 0.55f, 0.98f, 1f));
        var dataStripRight = CreateDisplayStrip(new Vector3(3.7f, 1.65f, 6.65f), new Color(0.14f, 0.55f, 0.98f, 1f));
        accentRenderers.Add(dataStripLeft.GetComponent<Renderer>());
        accentRenderers.Add(dataStripRight.GetComponent<Renderer>());
    }

    private void BuildHeroStage()
    {
        var mainPlatform = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        mainPlatform.name = "MainPlatform";
        mainPlatform.transform.SetParent(worldRoot, false);
        mainPlatform.transform.position = new Vector3(0f, 0.32f, 2.4f);
        mainPlatform.transform.localScale = new Vector3(1.9f, 0.32f, 1.9f);
        ApplyMaterial(mainPlatform, new Color(0.12f, 0.16f, 0.25f, 1f), false, Color.black);

        var mainRing = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        mainRing.name = "MainRing";
        mainRing.transform.SetParent(worldRoot, false);
        mainRing.transform.position = new Vector3(0f, 0.67f, 2.4f);
        mainRing.transform.localScale = new Vector3(2.15f, 0.025f, 2.15f);
        ApplyMaterial(mainRing, new Color(0.16f, 0.27f, 0.48f, 1f), true, new Color(0.14f, 0.60f, 1f, 1f));
        accentRenderers.Add(mainRing.GetComponent<Renderer>());

        heroOrb = GameObject.CreatePrimitive(PrimitiveType.Sphere).transform;
        heroOrb.name = "HeroOrb";
        heroOrb.SetParent(worldRoot, false);
        heroOrb.position = new Vector3(0f, 1.72f, 2.4f);
        heroOrb.localScale = Vector3.one * 1.15f;
        ApplyMaterial(heroOrb.gameObject, new Color(0.30f, 0.72f, 1f, 1f), true, new Color(0.24f, 0.77f, 1f, 1f));
        accentRenderers.Add(heroOrb.GetComponent<Renderer>());

        heroRingLarge = GameObject.CreatePrimitive(PrimitiveType.Cylinder).transform;
        heroRingLarge.name = "HeroRingLarge";
        heroRingLarge.SetParent(worldRoot, false);
        heroRingLarge.position = new Vector3(0f, 1.72f, 2.4f);
        heroRingLarge.localScale = new Vector3(1.8f, 0.018f, 1.8f);
        ApplyMaterial(heroRingLarge.gameObject, new Color(0.18f, 0.36f, 0.72f, 1f), true, new Color(0.10f, 0.62f, 1f, 1f));
        accentRenderers.Add(heroRingLarge.GetComponent<Renderer>());

        heroRingSmall = GameObject.CreatePrimitive(PrimitiveType.Cylinder).transform;
        heroRingSmall.name = "HeroRingSmall";
        heroRingSmall.SetParent(worldRoot, false);
        heroRingSmall.position = new Vector3(0f, 2.15f, 2.4f);
        heroRingSmall.localScale = new Vector3(0.95f, 0.018f, 0.95f);
        ApplyMaterial(heroRingSmall.gameObject, new Color(0.20f, 0.48f, 0.94f, 1f), true, new Color(0.26f, 0.86f, 1f, 1f));
        accentRenderers.Add(heroRingSmall.GetComponent<Renderer>());

        CreateWorldLabel("VibeLab Demo", new Vector3(0f, 3.15f, 4.85f), 0.34f, Color.white, TextAlignment.Center);
        CreateWorldLabel("Open • Explore • Deliver", new Vector3(0f, 2.72f, 4.85f), 0.16f, new Color(0.65f, 0.82f, 1f, 1f), TextAlignment.Center);
    }

    private void BuildFeatureModules()
    {
        CreateFeatureModule(0, new Vector3(-3.25f, 0f, 2.15f));
        CreateFeatureModule(1, new Vector3(0f, 0f, 4.15f));
        CreateFeatureModule(2, new Vector3(3.25f, 0f, 2.15f));
    }

    private void BuildAmbientDetails()
    {
        CreateTimelinePillar(new Vector3(-2.1f, 0f, 5.65f), Features[0].Accent, "Auto Ready");
        CreateTimelinePillar(new Vector3(0f, 0f, 5.95f), Features[1].Accent, "Spatial Stage");
        CreateTimelinePillar(new Vector3(2.1f, 0f, 5.65f), Features[2].Accent, "Export Path");

        CreateFloatingDecoration(new Vector3(-1.5f, 1.9f, 1.0f), 0.28f, new Color(0.24f, 0.58f, 1f, 1f));
        CreateFloatingDecoration(new Vector3(1.3f, 2.35f, 1.25f), 0.20f, new Color(0.40f, 0.88f, 0.62f, 1f));
        CreateFloatingDecoration(new Vector3(2.5f, 2.1f, 3.4f), 0.16f, new Color(1f, 0.66f, 0.34f, 1f));
        CreateFloatingDecoration(new Vector3(-2.65f, 2.45f, 3.35f), 0.18f, new Color(0.38f, 0.77f, 1f, 1f));
    }

    private void CreateFeatureModule(int index, Vector3 origin)
    {
        var feature = Features[index];
        var module = new SceneModule();

        module.Root = new GameObject(feature.Title + "Module").transform;
        module.Root.SetParent(worldRoot, false);
        module.Root.position = origin;
        module.BasePosition = origin;
        module.FocusPoint = origin + new Vector3(0f, 1.25f, 0.15f);

        var basePodium = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        basePodium.name = feature.Title + "Podium";
        basePodium.transform.SetParent(module.Root, false);
        basePodium.transform.localPosition = new Vector3(0f, 0.36f, 0f);
        basePodium.transform.localScale = new Vector3(1.02f, 0.36f, 1.02f);
        ApplyMaterial(basePodium, new Color(0.11f, 0.15f, 0.24f, 1f), false, Color.black);

        var halo = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        halo.name = feature.Title + "Halo";
        halo.transform.SetParent(module.Root, false);
        halo.transform.localPosition = new Vector3(0f, 0.74f, 0f);
        halo.transform.localScale = new Vector3(1.22f, 0.02f, 1.22f);
        module.Halo = halo.transform;
        module.HaloRenderer = ApplyMaterial(halo, feature.Accent * 0.35f, true, feature.Accent);
        accentRenderers.Add(module.HaloRenderer);

        var backplate = GameObject.CreatePrimitive(PrimitiveType.Cube);
        backplate.name = feature.Title + "Backplate";
        backplate.transform.SetParent(module.Root, false);
        backplate.transform.localPosition = new Vector3(0f, 1.75f, 1.22f);
        backplate.transform.localScale = new Vector3(1.55f, 1.18f, 0.08f);
        module.Backplate = backplate.transform;
        module.BackplateRenderer = ApplyMaterial(backplate, new Color(0.10f, 0.14f, 0.24f, 1f), true, feature.Accent * 0.6f);
        accentRenderers.Add(module.BackplateRenderer);

        var display = GameObject.CreatePrimitive(feature.VisualShape);
        display.name = feature.Title + "Display";
        display.transform.SetParent(module.Root, false);
        display.transform.localPosition = new Vector3(0f, 1.35f, 0f);
        display.transform.localScale = index == 1 ? new Vector3(1.15f, 1.15f, 1.15f) : Vector3.one;
        module.Display = display.transform;
        module.DisplayRenderer = ApplyMaterial(display, feature.Accent, true, feature.Accent);
        accentRenderers.Add(module.DisplayRenderer);

        CreateWorldLabel(feature.Title, origin + new Vector3(0f, 2.62f, 1.27f), 0.18f, Color.white, TextAlignment.Center);
        CreateWorldLabel(feature.Tag, origin + new Vector3(0f, 2.35f, 1.27f), 0.09f, feature.Accent, TextAlignment.Center);

        modules.Add(module);
    }

    private void CreateTimelinePillar(Vector3 position, Color accent, string title)
    {
        var pillar = GameObject.CreatePrimitive(PrimitiveType.Cube);
        pillar.name = title + "Pillar";
        pillar.transform.SetParent(worldRoot, false);
        pillar.transform.position = position + new Vector3(0f, 0.75f, 0f);
        pillar.transform.localScale = new Vector3(0.42f, 1.5f, 0.42f);
        ApplyMaterial(pillar, new Color(0.10f, 0.14f, 0.22f, 1f), false, Color.black);

        var cap = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cap.name = title + "Cap";
        cap.transform.SetParent(worldRoot, false);
        cap.transform.position = position + new Vector3(0f, 1.58f, 0f);
        cap.transform.localScale = new Vector3(0.56f, 0.08f, 0.56f);
        ApplyMaterial(cap, accent * 0.65f, true, accent);
        accentRenderers.Add(cap.GetComponent<Renderer>());

        CreateWorldLabel(title, position + new Vector3(0f, 2.06f, 0.42f), 0.10f, new Color(0.84f, 0.92f, 1f, 1f), TextAlignment.Center);
    }

    private void CreateFloatingDecoration(Vector3 position, float scale, Color accent)
    {
        var decoration = GameObject.CreatePrimitive(PrimitiveType.Sphere).transform;
        decoration.name = "FloatingDecoration";
        decoration.SetParent(worldRoot, false);
        decoration.position = position;
        decoration.localScale = Vector3.one * scale;
        ApplyMaterial(decoration.gameObject, accent, true, accent);
        floatingDecorations.Add(decoration);
        floatingDecorationBases.Add(position);
        accentRenderers.Add(decoration.GetComponent<Renderer>());
    }

    private Transform CreateBeacon(string name, Vector3 position, Color accent)
    {
        var beacon = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
        beacon.name = name;
        beacon.SetParent(worldRoot, false);
        beacon.position = position;
        beacon.localScale = new Vector3(0.42f, 2.35f, 0.42f);
        ApplyMaterial(beacon.gameObject, accent * 0.4f, true, accent);
        accentRenderers.Add(beacon.GetComponent<Renderer>());
        return beacon;
    }

    private GameObject CreateDisplayStrip(Vector3 position, Color accent)
    {
        var strip = GameObject.CreatePrimitive(PrimitiveType.Cube);
        strip.name = "DisplayStrip";
        strip.transform.SetParent(worldRoot, false);
        strip.transform.position = position;
        strip.transform.localScale = new Vector3(2.1f, 0.06f, 0.20f);
        ApplyMaterial(strip, accent * 0.45f, true, accent);
        return strip;
    }

    private void CreateWorldLabel(string text, Vector3 position, float characterSize, Color color, TextAlignment alignment)
    {
        var label = new GameObject(text + "Label");
        label.transform.SetParent(worldRoot, false);
        label.transform.position = position;
        label.transform.rotation = Quaternion.Euler(0f, 180f, 0f);

        var textMesh = label.AddComponent<TextMesh>();
        textMesh.text = text;
        textMesh.characterSize = characterSize;
        textMesh.fontSize = 64;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = alignment;
        textMesh.color = color;
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
        cameraComponent.backgroundColor = new Color(0.02f, 0.03f, 0.06f, 1f);
        cameraComponent.nearClipPlane = 0.03f;
        cameraComponent.farClipPlane = 120f;
        cameraComponent.fieldOfView = 48f;
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

        var directionalRoot = new GameObject("Directional Light");
        directionalRoot.transform.SetParent(worldRoot, false);
        directionalRoot.transform.rotation = Quaternion.Euler(38f, -24f, 0f);
        var directional = directionalRoot.AddComponent<Light>();
        directional.type = LightType.Directional;
        directional.intensity = 1.22f;
        directional.color = new Color(0.96f, 0.98f, 1f, 1f);

        var rimRoot = new GameObject("Rim Light");
        rimRoot.transform.SetParent(worldRoot, false);
        rimRoot.transform.position = new Vector3(0f, 3.4f, 0.6f);
        rimRoot.transform.rotation = Quaternion.Euler(46f, 180f, 0f);
        var rimLight = rimRoot.AddComponent<Light>();
        rimLight.type = LightType.Spot;
        rimLight.intensity = 5.8f;
        rimLight.range = 18f;
        rimLight.spotAngle = 56f;
        rimLight.color = new Color(0.26f, 0.62f, 1f, 1f);

        RenderSettings.ambientMode = AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = new Color(0.24f, 0.28f, 0.36f, 1f);
        RenderSettings.ambientEquatorColor = new Color(0.11f, 0.14f, 0.22f, 1f);
        RenderSettings.ambientGroundColor = new Color(0.04f, 0.05f, 0.08f, 1f);
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogColor = new Color(0.03f, 0.04f, 0.07f, 1f);
        RenderSettings.fogDensity = 0.015f;
    }

    private void AnimateWorld()
    {
        if (modules.Count == 0)
        {
            return;
        }

        actionPulse = Mathf.Max(0f, actionPulse - Time.deltaTime * 1.35f);
        var focusTarget = presentationMode ? modules[selectedIndex].FocusPoint : new Vector3(0f, 1.35f, 2.25f);
        focusRoot.position = Vector3.Lerp(focusRoot.position, focusTarget, Time.deltaTime * 3.25f);

        var heroBob = Mathf.Sin(Time.time * 1.25f) * 0.08f;
        heroOrb.position = new Vector3(0f, 1.72f + heroBob, 2.4f);
        heroOrb.Rotate(0f, 26f * Time.deltaTime, 0f, Space.World);
        heroRingLarge.Rotate(0f, (autoRotate ? 32f : 12f) * Time.deltaTime, autoRotate ? 24f * Time.deltaTime : 0f, Space.World);
        heroRingSmall.Rotate(autoRotate ? 48f * Time.deltaTime : 12f * Time.deltaTime, 0f, 20f * Time.deltaTime, Space.World);

        for (var i = 0; i < modules.Count; i++)
        {
            var module = modules[i];
            var selected = i == selectedIndex;
            var lift = Mathf.Sin(Time.time * 1.6f + i * 0.8f) * 0.09f;
            var emphasisLift = selected ? 0.18f : 0f;
            module.Root.position = module.BasePosition + Vector3.up * (lift + emphasisLift);

            if (autoRotate || selected)
            {
                module.Display.Rotate(0f, (22f + i * 16f) * Time.deltaTime, selected ? 14f * Time.deltaTime : 0f, Space.World);
            }

            var targetScale = selected ? Vector3.one * 1.18f : Vector3.one * 0.95f;
            if (i == 1)
            {
                targetScale *= 1.08f;
            }
            module.Display.localScale = Vector3.Lerp(module.Display.localScale, targetScale, Time.deltaTime * 4.8f);

            if (module.Halo != null)
            {
                var haloScale = selected ? 1.36f : 1.08f;
                module.Halo.localScale = Vector3.Lerp(module.Halo.localScale, new Vector3(haloScale, 0.02f, haloScale), Time.deltaTime * 5.5f);
                module.Halo.Rotate(0f, (selected ? 48f : 18f) * Time.deltaTime, 0f, Space.World);
            }

            UpdateEmissive(module.DisplayRenderer, Features[i].Accent, selected ? 1.55f : 0.85f);
            UpdateEmissive(module.HaloRenderer, Features[i].Accent, selected ? 1.9f : 0.9f);
            UpdateEmissive(module.BackplateRenderer, Features[i].Accent * 0.65f, selected ? 1.35f : 0.7f);
        }

        for (var i = 0; i < floatingDecorations.Count; i++)
        {
            var decoration = floatingDecorations[i];
            var basePosition = floatingDecorationBases[i];
            decoration.position = basePosition + Vector3.up * (Mathf.Sin(Time.time * (1.45f + i * 0.2f) + i) * 0.12f);
            decoration.Rotate(18f * Time.deltaTime, 32f * Time.deltaTime, 0f, Space.World);
        }

        var beaconPulse = 1.0f + Mathf.Sin(Time.time * 2.5f) * 0.08f + actionPulse * 0.18f;
        heroBeaconLeft.localScale = new Vector3(0.42f, 2.35f * beaconPulse, 0.42f);
        heroBeaconRight.localScale = new Vector3(0.42f, 2.35f * beaconPulse, 0.42f);

        for (var i = 0; i < accentRenderers.Count; i++)
        {
            if (accentRenderers[i] == null)
            {
                continue;
            }

            var baseIntensity = 0.8f + Mathf.Sin(Time.time * 1.4f + i * 0.5f) * 0.12f + actionPulse * 0.35f;
            UpdateEmissionOnly(accentRenderers[i], baseIntensity);
        }
    }

    private Renderer ApplyMaterial(GameObject target, Color color, bool emissive, Color emissionColor)
    {
        var renderer = target.GetComponent<Renderer>();
        if (renderer == null)
        {
            return null;
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

        if (emissive)
        {
            if (material.HasProperty("_EmissionColor"))
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", emissionColor * 1.15f);
                material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            }
        }

        renderer.material = material;
        return renderer;
    }

    private void UpdateEmissive(Renderer renderer, Color emissionColor, float multiplier)
    {
        if (renderer == null)
        {
            return;
        }

        var material = renderer.material;
        if (material == null || !material.HasProperty("_EmissionColor"))
        {
            return;
        }

        material.EnableKeyword("_EMISSION");
        material.SetColor("_EmissionColor", emissionColor * multiplier);
    }

    private void UpdateEmissionOnly(Renderer renderer, float multiplier)
    {
        if (renderer == null)
        {
            return;
        }

        var material = renderer.material;
        if (material == null || !material.HasProperty("_EmissionColor"))
        {
            return;
        }

        var current = material.GetColor("_EmissionColor");
        var maxComponent = Mathf.Max(0.0001f, Mathf.Max(current.r, Mathf.Max(current.g, current.b)));
        var baseColor = new Color(current.r / maxComponent, current.g / maxComponent, current.b / maxComponent, 1f);
        material.SetColor("_EmissionColor", baseColor * Mathf.Max(0.35f, multiplier));
    }

    private void EnsureGuiStyles()
    {
        if (glassPanelStyle != null)
        {
            return;
        }

        glassPanelStyle = new GUIStyle(GUI.skin.box)
        {
            padding = new RectOffset(18, 18, 18, 18),
            border = new RectOffset(12, 12, 12, 12)
        };
        glassPanelStyle.normal.background = MakeTexture(new Color(0.05f, 0.07f, 0.11f, 0.92f));

        heroTitleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 28,
            fontStyle = FontStyle.Bold,
            wordWrap = true
        };
        heroTitleStyle.normal.textColor = Color.white;

        sectionLabelStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 14,
            fontStyle = FontStyle.Bold,
            wordWrap = true
        };
        sectionLabelStyle.normal.textColor = new Color(0.50f, 0.82f, 1f, 1f);

        bodyStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 14,
            wordWrap = true,
            richText = true
        };
        bodyStyle.normal.textColor = new Color(0.90f, 0.94f, 0.98f, 1f);

        captionStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 12,
            wordWrap = true
        };
        captionStyle.normal.textColor = new Color(0.70f, 0.78f, 0.88f, 1f);

        chipStyle = new GUIStyle(GUI.skin.box)
        {
            fontSize = 12,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            padding = new RectOffset(10, 10, 7, 7)
        };
        chipStyle.normal.textColor = new Color(0.82f, 0.94f, 1f, 1f);
        chipStyle.normal.background = MakeTexture(new Color(0.10f, 0.22f, 0.42f, 0.95f));

        highlightStyle = new GUIStyle(chipStyle);
        highlightStyle.normal.textColor = new Color(0.08f, 0.12f, 0.16f, 1f);
        highlightStyle.normal.background = MakeTexture(new Color(0.98f, 0.77f, 0.30f, 0.96f));

        metricStyle = new GUIStyle(GUI.skin.box)
        {
            fontSize = 14,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleLeft,
            padding = new RectOffset(12, 12, 10, 10),
            wordWrap = true
        };
        metricStyle.normal.textColor = Color.white;
        metricStyle.normal.background = MakeTexture(new Color(0.09f, 0.14f, 0.24f, 0.95f));

        buttonStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize = 13,
            fontStyle = FontStyle.Bold,
            padding = new RectOffset(10, 10, 9, 9)
        };
        buttonStyle.normal.textColor = new Color(0.05f, 0.08f, 0.12f, 1f);
        buttonStyle.normal.background = MakeTexture(new Color(0.38f, 0.84f, 1f, 1f));
        buttonStyle.hover.background = MakeTexture(new Color(0.54f, 0.90f, 1f, 1f));

        secondaryButtonStyle = new GUIStyle(buttonStyle);
        secondaryButtonStyle.normal.textColor = Color.white;
        secondaryButtonStyle.normal.background = MakeTexture(new Color(0.14f, 0.20f, 0.32f, 1f));
        secondaryButtonStyle.hover.background = MakeTexture(new Color(0.18f, 0.28f, 0.44f, 1f));
    }

    private Texture2D MakeTexture(Color color)
    {
        var texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, color);
        texture.Apply();
        return texture;
    }
}

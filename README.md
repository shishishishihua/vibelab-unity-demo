# VibeLab Unity Demo（按 VibeLab Unity 提交规则设计）

这是一个**Unity 6 源码工程模板**，我已经把它继续升级成更像正式产品演示的 3D Demo：启动后会生成带主舞台、品牌灯塔、能力展项和交付面板的空间化展示场景，并内置 **VibeLab 规则校验 / 导出辅助菜单**。

先说明一个关键点：

- 你给的规则里，**最终提交物不是 Unity 源工程本身**
- 平台一期只接受：**从 Unity 本地导出的 Android Gradle 工程仓库**
- 所以这个目录的定位是：**Unity 源码侧项目模板 + 导出辅助工具**

## 我已经帮你对齐的方向

### 运行层

- 一个下载后可直接在 Unity 里打开并运行、且更像正式产品演示的 Demo
- 首次打开项目时会自动准备场景、Build Settings 和 Android 关键设置
- 默认支持“打开项目后直接点 Play”这一条最短路径
- 场景升级为主舞台 + 品牌灯塔 + 三段式能力展项 + 交付看板
- 支持键盘/鼠标控制视角与切换演示项
- 画面上会展示规则摘要、状态信息与 CTA 交互反馈

### 导出层

- 内置 `VibeLab/Prepare Demo Project` 菜单：自动准备场景与 Android Player Settings
- 内置 `VibeLab/Validate Submission Settings` 菜单：校验是否满足关键规则
- 内置 `VibeLab/Export/Export Android Gradle Project...` 菜单：导出到**工程外的空目录**

### 规则对齐层

已按你给的文档把这些规则落实到项目脚本/说明里：

- Unity 6（6000.x）
- Android 导出使用 **IL2CPP**
- 仅启用 **arm64**
- 包名使用自定义值，不沿用模板默认包名
- 导出目标要求是**工程外部的空目录**
- 明确区分：**Unity 源工程** 与 **最终提交的 Gradle 仓库**
- 附带一个导出后清理脚本：`tools/prepare_exported_project.py`

## 目录结构

- [工程目录](vibelab-unity-demo)
- `Assets/Scripts/VibeLabDemoBootstrap.cs`：运行时生成更像正式产品演示的空间舞台、展项与 HUD
- `Assets/Scripts/VibeLabOrbitCamera.cs`：视角控制
- `Assets/Editor/VibeLabQuickStart.cs`：首次打开项目时自动准备 Demo 并打开场景
- `Assets/Editor/VibeLabAndroidExport.cs`：规则校验与导出菜单
- `tools/prepare_exported_project.py`：导出后做基础合规清理

## 推荐使用方式

### 1. 在本机 Unity 6 打开这个工程

建议环境：

- Unity 6（6000.x，推荐 LTS）
- Android Build Support
- 使用 Unity 自带 Android SDK / NDK / OpenJDK

**开箱即用路径：**

1. 下载或 clone 这个仓库
2. 用 Unity 6 打开项目
3. 等待首次导入完成
4. 项目会自动准备 Demo 场景与 Android 关键设置
5. 直接点击 Unity 顶部的 `Play` 就能跑起来，更像正式产品演示页的空间舞台会自动生成

如果 Unity 没有自动切到 Demo 场景，也可以手动使用：

- `VibeLab/Open Demo Scene`

### 2. 执行菜单准备项目

如果你只是想**先把 Demo 跑起来**，其实到上一步就够了。  
这一节更偏向“我要开始导出 Android / 对齐提交流程”。

Unity 顶部菜单：

- `VibeLab/Prepare Demo Project`

它会自动做这些事：

- 创建 `Assets/Scenes/VibeLabBootstrap.unity`
- 把该场景加入 Build Settings
- 设置 Android 包名为 `com.vibelab.unitydemo`
- 设置 Scripting Backend 为 IL2CPP
- 设置目标架构仅 arm64
- 尝试把 targetSdk 设为 36，若不可用则回退 35
- 关闭 Custom Keystore

### 3. 检查规则

Unity 顶部菜单：

- `VibeLab/Validate Submission Settings`

它会在 Console 输出一个规则检查结果。

### 4. 导出 Android Gradle 工程

Unity 顶部菜单：

- `VibeLab/Export/Export Android Gradle Project...`

注意：

- 目标目录必须是**空目录**
- 目标目录必须在 **Unity 工程外部**
- 导出后的目录，才是你后续要作为 GitHub 仓库提交的基础

### 5. 导出后做仓库清理

在**这个 Unity 工程根目录**执行：

```bash
python3 tools/prepare_exported_project.py /path/to/exported-gradle-project
```

这个脚本会：

- 清空 `gradle.properties` 里的 `unityProjectPath` / `unity.projectPath`
- 删除 `local.properties`
- 生成 `.gitignore`
- 生成一份 `VIBELAB_SUBMISSION_CHECKLIST.md`

## 还需要你在本机继续完成的事

因为 VibeLab 的最终校验依赖 **Unity 本机导出结果** 和 **本机 Android 工具链**，所以这些步骤我没有在沙箱里伪造：

- 本机导出 Android Gradle 工程
- 按规则对导出工程执行 IL2CPP 预编译
- 在导出工程上执行最终 `:launcher:assembleRelease`
- 确认只有唯一一个 release APK
- 把导出工程单独作为 GitHub 仓库提交

## 这个 Demo 应用具体是什么

我默认给你做成了一个**最小空间展示 Demo**：

- 启动后自动生成一个简洁 3D 舞台
- 中间有三个旋转展示体
- 屏幕左侧显示规则摘要、交互说明与当前选中内容
- 适合作为你后续替换成真实业务逻辑的起点

## 如果你下一步要我继续做

我可以继续帮你往两个方向走：

1. **把这个 Demo 改成你真正的业务应用**
   - 比如展厅、播放器、看板、3D 交互、表单、账号登录壳子等
2. **继续帮你做“导出后的 Gradle 工程合规改造脚本”**
   - 更细地自动检查 `launcher/unityLibrary`、Gradle 配置、提交前清单

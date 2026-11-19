#!/usr/bin/env python3
import argparse
import json
from pathlib import Path

EXPORT_GITIGNORE = """.gradle/
build/
**/build/
local.properties
*.iml
*.log
*_BurstDebugInformation_DoNotShip/
"""

CHECKLIST_TEMPLATE = """# VIBELAB_SUBMISSION_CHECKLIST

## 已自动处理
- 清空 `gradle.properties` 中的 `unityProjectPath` / `unity.projectPath`
- 删除 `local.properties`
- 补充导出仓库根目录 `.gitignore`

## 仍需你在本机继续做的事
1. 按 Unity 导出规则确认：不要补 `gradlew` / `gradlew.bat`，这属于 Unity 6 导出时的正常现象。
2. 在导出工程上执行 `:unityLibrary:buildIl2Cpp --rerun`，确保 IL2CPP 真正重编译，而不是 `SKIPPED` / `UP-TO-DATE` / `FROM-CACHE`。
3. 在**完成验证之后**，如果需要禁用 `buildIl2Cpp`，应在相关 Gradle 文件末尾**追加**：
   `tasks.named('buildIl2Cpp').configure { enabled = false }`
4. 删除 `Il2CppOutputProject/` 之前先确认所有验证都已通过。
5. 终验前显式设置 `ANDROID_HOME` / `ANDROID_SDK_ROOT`（指向 Unity 自带 Android SDK）。
6. 最终执行 `:launcher:assembleRelease`，确认只产出唯一一个 release APK。
7. 保留 `gradle/wrapper/gradle-wrapper.properties`，不要删除也不要改成私有源。
8. 不要改动指向本机 Unity 安装目录的其他路径类配置；规则里唯一要求清空的是 `unityProjectPath` / `unity.projectPath`。
"""


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="为 VibeLab Unity 导出的 Android Gradle 工程做基础合规清理")
    parser.add_argument("export_root", help="Unity 导出的 Android Gradle 工程根目录")
    return parser.parse_args()


def require(condition: bool, message: str) -> None:
    if not condition:
        raise SystemExit(message)


def clear_gradle_path_keys(text: str) -> tuple[str, list[str]]:
    lines = text.splitlines()
    updated: list[str] = []
    touched: list[str] = []
    wanted_keys = ["unityProjectPath", "unity.projectPath"]

    for line in lines:
        stripped = line.strip()
        if stripped.startswith("unityProjectPath="):
            updated.append("unityProjectPath=")
            touched.append("unityProjectPath")
            continue
        if stripped.startswith("unity.projectPath="):
            updated.append("unity.projectPath=")
            touched.append("unity.projectPath")
            continue
        updated.append(line)

    for key in wanted_keys:
        if key not in touched:
            updated.append(f"{key}=")
            touched.append(key)

    return "\n".join(updated).rstrip() + "\n", touched


def detect_application_modules(export_root: Path) -> list[str]:
    application_modules: list[str] = []
    for module_name in ("launcher", "unityLibrary", "app"):
        build_file = export_root / module_name / "build.gradle"
        if build_file.exists() and "com.android.application" in build_file.read_text(encoding="utf-8", errors="ignore"):
            application_modules.append(module_name)
    return application_modules


def main() -> None:
    args = parse_args()
    export_root = Path(args.export_root).expanduser().resolve()

    require(export_root.exists(), f"目录不存在：{export_root}")
    require(export_root.is_dir(), f"不是目录：{export_root}")
    require((export_root / "launcher").exists(), "缺少 launcher 模块，像是不完整的 Unity Android 导出工程")
    require((export_root / "unityLibrary").exists(), "缺少 unityLibrary 模块，像是不完整的 Unity Android 导出工程")

    gradle_properties = export_root / "gradle.properties"
    require(gradle_properties.exists(), "缺少 gradle.properties")

    original_gradle_properties = gradle_properties.read_text(encoding="utf-8", errors="ignore")
    rewritten_gradle_properties, touched_keys = clear_gradle_path_keys(original_gradle_properties)
    gradle_properties.write_text(rewritten_gradle_properties, encoding="utf-8")

    deleted_files: list[str] = []
    local_properties = export_root / "local.properties"
    if local_properties.exists():
        local_properties.unlink()
        deleted_files.append("local.properties")

    gitignore_path = export_root / ".gitignore"
    gitignore_path.write_text(EXPORT_GITIGNORE, encoding="utf-8")

    checklist_path = export_root / "VIBELAB_SUBMISSION_CHECKLIST.md"
    checklist_path.write_text(CHECKLIST_TEMPLATE, encoding="utf-8")

    application_modules = detect_application_modules(export_root)
    settings_gradle = export_root / "settings.gradle"
    included_modules = []
    if settings_gradle.exists():
        settings_text = settings_gradle.read_text(encoding="utf-8", errors="ignore")
        for token in [":launcher", ":unityLibrary", ":app"]:
            if token in settings_text:
                included_modules.append(token)

    summary = {
        "export_root": str(export_root),
        "updated_files": ["gradle.properties", ".gitignore", "VIBELAB_SUBMISSION_CHECKLIST.md"],
        "deleted_files": deleted_files,
        "cleared_gradle_keys": touched_keys,
        "included_modules": included_modules,
        "application_modules": application_modules,
        "warnings": [],
    }

    if len(application_modules) != 1:
        summary["warnings"].append("检测到的 Application module 数量不是 1，请人工确认只保留一个 application 模块。")
    elif application_modules[0] != "launcher":
        summary["warnings"].append("检测到 application 模块不是 launcher，请人工确认是否符合 Unity 导出预期。")

    print(json.dumps(summary, ensure_ascii=False, indent=2))
    print("\n已完成基础清理。请继续执行 IL2CPP 预编译与 :launcher:assembleRelease 终验。")


if __name__ == "__main__":
    main()

# VIBELAB_SUBMISSION_CHECKLIST

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

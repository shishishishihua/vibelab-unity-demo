# VIBELAB_SUBMISSION_CHECKLIST

## 已完成
- 已清空 `gradle.properties` 中的 `unityProjectPath` / `unity.projectPath`
- 已删除 `local.properties`
- 已保留 Unity 6 导出的 `gradle/wrapper/gradle-wrapper.properties`，未补 `gradlew` / `gradlew.bat`
- 已在本机完成 IL2CPP 预编译验证
- 已在验证后删除 `unityLibrary/src/main/Il2CppOutputProject/`，避免云端误判为未预编译工程
- 已在当前提交内容上完成 `:launcher:assembleRelease` 验证

## 当前仓库结构
- 根目录为 Android Gradle 工程根
- Application module 为 `launcher`
- 预编译产物保留在 `unityLibrary/src/main/jniLibs/arm64-v8a/`
- 仓库中不包含 `local.properties`、APK、`.gradle/`、根级 `build/`

## 提交说明
- 提交 VibeLab 时请使用当前 GitHub 仓库地址和对应的 40 位 commit id

using System.Diagnostics.CodeAnalysis;
using UnityEngine;

// 在其他脚本顶部使用：using RDG;
namespace RDG
{
    /// <summary>
    /// Android 设备震动控制。在场景加载前通过 RuntimeInitializeOnLoadMethod 自动初始化。
    /// </summary>
    public static class Vibration
    {
        /// <summary>日志输出级别（调试用）。</summary>
        public static logLevel LogLevel = logLevel.Disabled;

        private static AndroidJavaObject vibrator = null;
        private static AndroidJavaClass vibrationEffectClass = null;
        private static int defaultAmplitude = 255;

        private static int ApiLevel = 1;
        /// <summary>是否支持 VibrationEffect（API &gt;= 26）。</summary>
        private static bool doesSupportVibrationEffect () => ApiLevel >= 26;
        /// <summary>是否支持预定义震动效果（API &gt;= 29）。</summary>
        private static bool doesSupportPredefinedEffect () => ApiLevel >= 29;

        #region Initialization
        private static bool isInitialized = false;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        [SuppressMessage("Code quality", "IDE0051", Justification = "Called on scene load")]
        private static void Initialize ()
        {
            // 需在 AndroidManifest 中声明 VIBRATE 权限
#if UNITY_ANDROID
            if (Application.isConsolePlatform) { Handheld.Vibrate(); }
#endif

            // 安全加载 Android 震动相关引用
            if (isInitialized == false && Application.platform == RuntimePlatform.Android) {
                // 获取系统 API 等级
                using (AndroidJavaClass androidVersionClass = new AndroidJavaClass("android.os.Build$VERSION")) {
                    ApiLevel = androidVersionClass.GetStatic<int>("SDK_INT");
                }

                // UnityPlayer 与当前 Activity
                using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using (AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity")) {
                    if (currentActivity != null) {
                        vibrator = currentActivity.Call<AndroidJavaObject>("getSystemService", "vibrator");

                        // 支持 VibrationEffect 时获取对应 Java 类
                        if (doesSupportVibrationEffect()) {
                            vibrationEffectClass = new AndroidJavaClass("android.os.VibrationEffect");
                            defaultAmplitude = Mathf.Clamp(vibrationEffectClass.GetStatic<int>("DEFAULT_AMPLITUDE"), 1, 255);
                        }

                        // 支持预定义效果时缓存效果 ID
                        if (doesSupportPredefinedEffect()) {
                            PredefinedEffect.EFFECT_CLICK = vibrationEffectClass.GetStatic<int>("EFFECT_CLICK");
                            PredefinedEffect.EFFECT_DOUBLE_CLICK = vibrationEffectClass.GetStatic<int>("EFFECT_DOUBLE_CLICK");
                            PredefinedEffect.EFFECT_HEAVY_CLICK = vibrationEffectClass.GetStatic<int>("EFFECT_HEAVY_CLICK");
                            PredefinedEffect.EFFECT_TICK = vibrationEffectClass.GetStatic<int>("EFFECT_TICK");
                        }
                    }
                }

                logAuto("Vibration component initialized", logLevel.Info);
                isInitialized = true;
            }
        }
        #endregion

        #region Vibrate Public
        /// <summary>
        /// 按毫秒数震动；可选振幅（设备支持时）。
        /// 振幅为 -1 时使用设备默认；有效范围 1–255。
        /// <paramref name="cancel"/> 为 true 时先取消当前震动再开始。
        /// </summary>
        /// <param name="milliseconds">震动时长（毫秒）。</param>
        /// <param name="amplitude">振幅，-1 表示使用默认/最大策略。</param>
        /// <param name="cancel">是否在开始前取消当前震动。</param>
        public static void Vibrate (long milliseconds, int amplitude = -1, bool cancel = false)
        {
            string funcToStr () => string.Format("Vibrate ({0}, {1}, {2})", milliseconds, amplitude, cancel);

            Initialize(); // make sure script is initialized
            if (isInitialized == false) {
                logAuto(funcToStr() + ": Not initialized", logLevel.Warning);
            }
            else if (HasVibrator() == false) {
                logAuto(funcToStr() + ": Device doesn't have Vibrator", logLevel.Warning);
            }
            else {
                if (cancel) Cancel();
                if (doesSupportVibrationEffect()) {
                    // validate amplitude
                    amplitude = Mathf.Clamp(amplitude, -1, 255);
                    if (amplitude == -1) amplitude = 255; // if -1, disable amplitude (use maximum amplitude)
                    if (amplitude != 255 && HasAmplitudeControl() == false) { // if amplitude was set, but not supported, notify developer
                        logAuto(funcToStr() + ": Device doesn't have Amplitude Control, but Amplitude was set", logLevel.Warning);
                    }
                    if (amplitude == 0) amplitude = defaultAmplitude; // if 0, use device DefaultAmplitude

                    // if amplitude is not supported, use 255; if amplitude is -1, use systems DefaultAmplitude. Otherwise use user-defined value.
                    amplitude = HasAmplitudeControl() == false ? 255 : amplitude;
                    vibrateEffect(milliseconds, amplitude);
                    logAuto(funcToStr() + ": Effect called", logLevel.Info);
                }
                else {
                    vibrateLegacy(milliseconds);
                    logAuto(funcToStr() + ": Legacy called", logLevel.Info);
                }
            }
        }
        /// <summary>
        /// 按时间序列震动（关-开-关-开…交替，单位为毫秒）。
        /// <paramref name="amplitudes"/> 可为 null；若提供须与 <paramref name="pattern"/> 等长，元素为 1–255。
        /// <paramref name="repeat"/> 为重复起始下标，-1 表示不重复。
        /// </summary>
        /// <param name="pattern">各段时长数组。</param>
        /// <param name="amplitudes">各段振幅，可为 null。</param>
        /// <param name="repeat">重复起始索引，-1 不重复。</param>
        /// <param name="cancel">是否在开始前取消当前震动。</param>
        public static void Vibrate (long[] pattern, int[] amplitudes = null, int repeat = -1, bool cancel = false)
        {
            string funcToStr () => string.Format("Vibrate (({0}), ({1}), {2}, {3})", arrToStr(pattern), arrToStr(amplitudes), repeat, cancel);

            Initialize(); // make sure script is initialized
            if (isInitialized == false) {
                logAuto(funcToStr() + ": Not initialized", logLevel.Warning);
            }
            else if (HasVibrator() == false) {
                logAuto(funcToStr() + ": Device doesn't have Vibrator", logLevel.Warning);
            }
            else {
                // check Amplitudes array length
                if (amplitudes != null && amplitudes.Length != pattern.Length) {
                    logAuto(funcToStr() + ": Length of Amplitudes array is not equal to Pattern array. Amplitudes will be ignored.", logLevel.Warning);
                    amplitudes = null;
                }
                // limit amplitudes between 1 and 255
                if (amplitudes != null) {
                    clampAmplitudesArray(amplitudes);
                }

                // vibrate
                if (cancel) Cancel();
                if (doesSupportVibrationEffect()) {
                    if (amplitudes != null && HasAmplitudeControl() == false) {
                        logAuto(funcToStr() + ": Device doesn't have Amplitude Control, but Amplitudes was set", logLevel.Warning);
                        amplitudes = null;
                    }
                    if (amplitudes != null) {
                        vibrateEffect(pattern, amplitudes, repeat);
                        logAuto(funcToStr() + ": Effect with amplitudes called", logLevel.Info);
                    }
                    else {
                        vibrateEffect(pattern, repeat);
                        logAuto(funcToStr() + ": Effect called", logLevel.Info);
                    }
                }
                else {
                    vibrateLegacy(pattern, repeat);
                    logAuto(funcToStr() + ": Legacy called", logLevel.Info);
                }
            }
        }

        /// <summary>
        /// 使用系统预定义震动效果（见 <see cref="PredefinedEffect"/>）。需 API 等级 &gt;= 29。
        /// </summary>
        /// <param name="effectId">预定义效果 ID。</param>
        /// <param name="cancel">是否在开始前取消当前震动。</param>
        public static void VibratePredefined (int effectId, bool cancel = false)
        {
            string funcToStr () => string.Format("VibratePredefined ({0})", effectId);

            Initialize(); // make sure script is initialized
            if (isInitialized == false) {
                logAuto(funcToStr() + ": Not initialized", logLevel.Warning);
            }
            else if (HasVibrator() == false) {
                logAuto(funcToStr() + ": Device doesn't have Vibrator", logLevel.Warning);
            }
            else if (doesSupportPredefinedEffect() == false) {
                logAuto(funcToStr() + ": Device doesn't support Predefined Effects (Api Level >= 29)", logLevel.Warning);
            }
            else {
                if (cancel) Cancel();
                vibrateEffectPredefined(effectId);
                logAuto(funcToStr() + ": Predefined effect called", logLevel.Info);
            }
        }
        #endregion

        #region Public Properties & Controls
        /// <summary>
        /// 将逗号分隔的字符串解析为时长数组（用于震动模式）。
        /// </summary>
        /// <param name="pattern">如 "100,200,100"。</param>
        public static long[] ParsePattern (string pattern)
        {
            if (pattern == null) return new long[0];
            pattern = pattern.Trim();
            string[] split = pattern.Split(',');

            long[] timings = new long[split.Length];
            for (int i = 0; i < split.Length; i++) {
                if (int.TryParse(split[i].Trim(), out int duration)) {
                    timings[i] = duration < 0 ? 0 : duration;
                }
                else {
                    timings[i] = 0;
                }
            }

            return timings;
        }

        /// <summary>返回当前 Android API 等级。</summary>
        public static int GetApiLevel () => ApiLevel;
        /// <summary>返回设备默认振幅（若不可用可能为 0）。</summary>
        public static int GetDefaultAmplitude () => defaultAmplitude;

        /// <summary>设备是否具备震动器。</summary>
        public static bool HasVibrator ()
        {
            return vibrator != null && vibrator.Call<bool>("hasVibrator");
        }
        /// <summary>设备是否支持振幅控制（API 26+）。</summary>
        public static bool HasAmplitudeControl ()
        {
            if (HasVibrator() && doesSupportVibrationEffect()) {
                return vibrator.Call<bool>("hasAmplitudeControl"); // API 26+ specific
            }
            else {
                return false; // no amplitude control below API level 26
            }
        }

        /// <summary>尝试取消当前正在进行的震动。</summary>
        public static void Cancel ()
        {
            if (HasVibrator()) {
                vibrator.Call("cancel");
                logAuto("Cancel (): Called", logLevel.Info);
            }
        }
        #endregion

        #region Vibrate Internal
        #region Vibration Callers
        private static void vibrateEffect (long milliseconds, int amplitude)
        {
            using (AndroidJavaObject effect = createEffect_OneShot(milliseconds, amplitude)) {
                vibrator.Call("vibrate", effect);
            }
        }
        private static void vibrateLegacy (long milliseconds)
        {
            vibrator.Call("vibrate", milliseconds);
        }

        private static void vibrateEffect (long[] pattern, int repeat)
        {
            using (AndroidJavaObject effect = createEffect_Waveform(pattern, repeat)) {
                vibrator.Call("vibrate", effect);
            }
        }
        private static void vibrateLegacy (long[] pattern, int repeat)
        {
            vibrator.Call("vibrate", pattern, repeat);
        }

        private static void vibrateEffect (long[] pattern, int[] amplitudes, int repeat)
        {
            using (AndroidJavaObject effect = createEffect_Waveform(pattern, amplitudes, repeat)) {
                vibrator.Call("vibrate", effect);
            }
        }
        private static void vibrateEffectPredefined (int effectId)
        {
            using (AndroidJavaObject effect = createEffect_Predefined(effectId)) {
                vibrator.Call("vibrate", effect);
            }
        }
        #endregion

        #region Vibration Effect
        /// <summary>封装 Android VibrationEffect.createOneShot（API &gt;= 26）。</summary>
        private static AndroidJavaObject createEffect_OneShot (long milliseconds, int amplitude)
        {
            return vibrationEffectClass.CallStatic<AndroidJavaObject>("createOneShot", milliseconds, amplitude);
        }
        /// <summary>封装 Android VibrationEffect.createPredefined（API &gt;= 29）。</summary>
        private static AndroidJavaObject createEffect_Predefined (int effectId)
        {
            return vibrationEffectClass.CallStatic<AndroidJavaObject>("createPredefined", effectId);
        }
        /// <summary>封装带振幅数组的 createWaveform（API &gt;= 26）。</summary>
        private static AndroidJavaObject createEffect_Waveform (long[] timings, int[] amplitudes, int repeat)
        {
            return vibrationEffectClass.CallStatic<AndroidJavaObject>("createWaveform", timings, amplitudes, repeat);
        }
        /// <summary>封装无振幅的 createWaveform（API &gt;= 26）。</summary>
        private static AndroidJavaObject createEffect_Waveform (long[] timings, int repeat)
        {
            return vibrationEffectClass.CallStatic<AndroidJavaObject>("createWaveform", timings, repeat);
        }
        #endregion
        #endregion

        #region Internal
        private static void logAuto (string text, logLevel level)
        {
            if (level == logLevel.Disabled) level = logLevel.Info;

            if (text != null) {
                if (level == logLevel.Warning && LogLevel == logLevel.Warning) {
                    Debug.LogWarning(text);
                }
                else if (level == logLevel.Info && LogLevel >= logLevel.Info) {
                    Debug.Log(text);
                }
            }
        }
        private static string arrToStr (long[] array) => array == null ? "null" : string.Join(", ", array);
        private static string arrToStr (int[] array) => array == null ? "null" : string.Join(", ", array);

        private static void clampAmplitudesArray (int[] amplitudes)
        {
            for (int i = 0; i < amplitudes.Length; i++) {
                amplitudes[i] = Mathf.Clamp(amplitudes[i], 1, 255);
            }
        }
        #endregion

        /// <summary>Android 预定义震动效果 ID（API &gt;= 29，运行时从系统读取）。</summary>
        public static class PredefinedEffect
        {
            /// <summary>轻触反馈。</summary>
            public static int EFFECT_CLICK;
            /// <summary>双击反馈。</summary>
            public static int EFFECT_DOUBLE_CLICK;
            /// <summary>重触反馈。</summary>
            public static int EFFECT_HEAVY_CLICK;
            /// <summary>滴答反馈。</summary>
            public static int EFFECT_TICK;
        }
        /// <summary>内部调试日志级别。</summary>
        public enum logLevel
        {
            /// <summary>不输出。</summary>
            Disabled,
            /// <summary>信息。</summary>
            Info,
            /// <summary>警告。</summary>
            Warning,
        }
    }
}
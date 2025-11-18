using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Skinetic
{
    public static class PropertiesUtils
    {
        public const float MIN_VOLUME = 0.0f;
        public const float MAX_VOLUME = 250.0f;
        public const float MIN_SPEED = 0.01f;
        public const float MAX_SPEED = 100.0f;
        public const int MIN_REPEATCOUNT = 0;
        public const float MAX_REPEATCOUNT = 255;
        public const float MIN_REPEATDELAY = 0.0f;
        public const float MIN_PLAYAT = 0.0f;
        public const float MIN_MAXDURATION = 0.0f;
        public const int MAX_BOOST = 100;
        public const int MIN_BOOST = -100;
        public const int MIN_PRIORITY = 1;
        public const int MAX_PRIORITY = 10;

        public static string GetPropertyTooltip(string str)
        {
            try
            {
                /*if (EditorPrefs.GetInt("m_toolTipLevel") < (int)Settings.TooltipLvl.PropertiesOnly)
                    return "";*/
                return m_propertyTooltips[str];
            }
            catch
            {
                return "";
            }
        }

        private static readonly Dictionary<string, string> m_propertyTooltips =
            new Dictionary<string, string>()
            {
                ["StrategyOnPlay"] = "Defines the haptic effect behavior when PlayEffect() is called.",
                ["E_DEFAULT"] = "By default, an effect can only play once. While it is playing, any call to PlayEffect() is ignored.",
                ["E_FORCE"] = "Immediately stop the current playing instance and start a new one.",
                ["E_PULLED"] = "Each call to PlayEffect() starts a new independent instance of the effect. StopEffect() " +
                                     "simultaenously stops all pulled effects that are still playing.",
                ["Volume"] = "The volume property is a percentage of the base volume between [0; 250]%: [0;100[% the pattern attenuated, " +
                         "100% the pattern's base volume is preserved, ]100; 250]% the pattern is amplified. " +
                         "Too much amplification may lead to the clipping of the haptic effects, distorting them " +
                         "and producing audible noise.",
                ["Speed"] = "The speed is a time scale between [0.01; 100]: [0.01; 1[the pattern is slowed down, 1 the pattern " +
                        "timing is preserved,]1; 100] the pattern is accelerated. The resulting speed between " +
                        "the haptic effect's and the samples' speed within the pattern cannot exceed these " +
                        "bounds. Slowing down or accelerating a sample too much may result in an haptically poor effect.",
                ["RepeatCount"] = "Number of repetition of the pattern as the effect is playing. If 0, the pattern is repeated indefinitely " +
                              "until it is either stopped with stopEffect() or reach the maxDuration value.",
                ["RepeatDelay"] = "Pause in second between two repetition of the pattern, this value is not affected by the speed parameter.",
                ["PlayAtTime"] = "Time in the pattern at which the effect start to play. This value need to be lower than the maxDuration. " +
                                "It also takes into account the repeatCount and the repeatDelay of the pattern.",
                ["MaxDuration"] = "Maximum duration of the effect, it is automatically stopped if the duration " +
                              "is reached without any regards for the actual state of the repeatCount. A maxDuration " +
                              "of 0 remove the duration limit, making the effect ables to play indefinitely. " +
                               "This value is not affected by the speed and the playAtTime parameters.",
                ["OverridePatternBoost"] = "By setting this boolean to true, the effect will use the effectBoost value instead of the default pattern value.",
                ["EffectBoost"] = "Boost intensity level percent [-100; 100] of the effect to use instead of the default pattern value " +
                                  "if overridePatternBoost is set to true. By using a negative value, can decrease or even nullify " +
                                  "the global intensity boost set by the user.",
                ["Priority"] = "Level of priority [1; 10] of the effect. In case too many effects are playing " +
                           "simultaneously, the effect with lowest priority(10) will be muted.",
                ["TargetDevice"] = "Device to which the effect will be played on. A default device can be set in the inspector. " +
                                   "However, it can also be set at runtime through scripting.",
                ["TargetPattern"] = "PatternAsset from which the effect will be instantiated. A default scriptable object can be set in the inspector. " +
                                    "However, it can also be set at runtime through scripting. Notice that the target device should have already loaded " +
                                    "the PatternAsset before the HapticEffect can be played.",
                ["HeightTranslation"] = "Height in meter to translate the pattern.",
                ["HeadingRotation"] = "Heading angle in degree to rotate the pattern in the horizontal plane (y axis).",
                ["TiltingRotation"] = "Tilting angle in degree to rotate the pattern in the sagittal plane (x axis).",
                ["AudioPreset"] = "Preset of audio devices. E_CUSTOMDEVICE is to be used for a custom configuration.",
                ["E_CUSTOMDEVICE"] = "Audio stream with a custom configuration",
                ["E_SKINETIC"] = "Autoconfiguration of the audioStream for the Skinetic device",
                ["E_HSDMKI"] = "Autoconfiguration of the audioStream for the HSD mk.I device",
                ["E_HSDMKII"] = "Autoconfiguration of the audioStream for the HSD mk.II device",
                ["E_HSD0"] = "Autoconfiguration of the audioStream for the HSD 0 device",
                ["DeviceAudioPreset"] = "Selected preset of audio device",
                ["DeviceName"] = "Name of the targeted audio device. If using a specific audioPreset other than eAudioDevice, the parameter will be ignored.",
                ["AudioAPI"] = "Name of the targeted API. Default value 'any_API' uses any available API which match the configuration, if any.",
                ["SampleRate"] = "Sample rate of the audio stream. If using a specific audioPreset, the parameter will be ignored.",
                ["BufferSize"] = "Size of a chunk of data sent over the audio stream.",
                ["NbStreamChannel"] = "Number of channels to use while streaming to the haptic output. If using a specific audio " + 
                                      "preset, the parameter will be ignored. Setting - 1 will use the number of actuator of the layout, " + 
                                      "or a portion of it.",
                ["SuggestedLatency"] = "Desired latency in seconds. The value is rounded to the closest available latency value from the audio API.",
            };

    }
}

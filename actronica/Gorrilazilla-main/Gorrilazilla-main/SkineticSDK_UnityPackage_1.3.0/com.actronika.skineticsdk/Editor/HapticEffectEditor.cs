using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Skinetic;

[CustomEditor(typeof(HapticEffect))]
[CanEditMultipleObjects]
public class HapticEffectEditor : Editor
{
    private HapticEffect m_targetEffect;


    private void DisplayEffect(HapticEffect effect)
    {
        
        EditorGUILayout.Space(5f, false);
        EditorGUI.BeginChangeCheck();

        Undo.RecordObject(effect, "Haptic Effect" + effect.gameObject);

        effect.StrategyOnPlay = (HapticEffect.PlayStrategy) EditorGUILayout.EnumPopup(new GUIContent("StrategyOnPlay", PropertiesUtils.GetPropertyTooltip("StrategyOnPlay")), effect.StrategyOnPlay);
        EditorGUILayout.HelpBox(PropertiesUtils.GetPropertyTooltip(effect.StrategyOnPlay.ToString()), MessageType.Info);


        EditorGUILayout.LabelField(new GUIContent("Priority level", PropertiesUtils.GetPropertyTooltip("Priority")), new GUIContent("(Low)   " + PropertiesUtils.MAX_PRIORITY + " --> " + PropertiesUtils.MIN_PRIORITY + "   (High)"));
        effect.PriorityLevel = EditorGUILayout.IntSlider(effect.PriorityLevel, PropertiesUtils.MAX_PRIORITY, PropertiesUtils.MIN_PRIORITY);//, new GUIContent("PriorityLevel", Properties.GetPropertyTooltip("PriorityLevel")));


        EditorGUILayout.BeginHorizontal();
        effect.Volume = EditorGUILayout.FloatField(new GUIContent("Volume", PropertiesUtils.GetPropertyTooltip("Volume")), effect.Volume);
        EditorGUILayout.LabelField("[" + PropertiesUtils.MIN_VOLUME + "; " + PropertiesUtils.MAX_VOLUME + "] %");
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        effect.Speed = EditorGUILayout.FloatField(new GUIContent("Speed", PropertiesUtils.GetPropertyTooltip("Speed")), effect.Speed);
        EditorGUILayout.LabelField("[" + PropertiesUtils.MIN_SPEED + "; " + PropertiesUtils.MAX_SPEED + "]");
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        effect.RepeatCount = EditorGUILayout.IntField(new GUIContent("RepeatCount", PropertiesUtils.GetPropertyTooltip("RepeatCount")), effect.RepeatCount);
        EditorGUILayout.LabelField("[" + (PropertiesUtils.MIN_REPEATCOUNT+1) + "; " + PropertiesUtils.MAX_REPEATCOUNT + "] or 0 = \u221E");
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        effect.RepeatDelay = EditorGUILayout.FloatField(new GUIContent("RepeatDelay", PropertiesUtils.GetPropertyTooltip("RepeatDelay")), effect.RepeatDelay);
        EditorGUILayout.LabelField("s");
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        effect.PlayAtTime = EditorGUILayout.FloatField(new GUIContent("PlayAtTime", PropertiesUtils.GetPropertyTooltip("PlayAtTime")), effect.PlayAtTime);
        EditorGUILayout.LabelField("s");
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        effect.MaxDuration = EditorGUILayout.FloatField(new GUIContent("MaxDuration", PropertiesUtils.GetPropertyTooltip("MaxDuration")), effect.MaxDuration);
        EditorGUILayout.LabelField("s ; 0 = no limits");
        EditorGUILayout.EndHorizontal();


        effect.OverridePatternBoost = EditorGUILayout.BeginToggleGroup(new GUIContent("OverridePatternBoost", PropertiesUtils.GetPropertyTooltip("OverridePatternBoost")), effect.OverridePatternBoost);
        EditorGUILayout.BeginHorizontal();
        effect.EffectBoost = EditorGUILayout.IntField(new GUIContent("EffectBoost", PropertiesUtils.GetPropertyTooltip("EffectBoost")), effect.EffectBoost);
        EditorGUILayout.LabelField("[" + PropertiesUtils.MIN_BOOST + "; " + PropertiesUtils.MAX_BOOST + "]");
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndToggleGroup();


        EditorGUILayout.BeginHorizontal();
        effect.MaxDuration = EditorGUILayout.FloatField(new GUIContent("HeightTranslation", PropertiesUtils.GetPropertyTooltip("HeightTranslation")), effect.HeightTranslation);
        EditorGUILayout.LabelField("m");
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        effect.MaxDuration = EditorGUILayout.FloatField(new GUIContent("HeadingRotation", PropertiesUtils.GetPropertyTooltip("HeadingRotation")), effect.HeadingRotation);
        EditorGUILayout.LabelField("°");
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        effect.MaxDuration = EditorGUILayout.FloatField(new GUIContent("TiltingRotation", PropertiesUtils.GetPropertyTooltip("TiltingRotation")), effect.TiltingRotation);
        EditorGUILayout.LabelField("°");
        EditorGUILayout.EndHorizontal();


        EditorGUILayout.BeginHorizontal();
        effect.FrontBackInversion = EditorGUILayout.ToggleLeft(new GUIContent("FrontBackInversion", PropertiesUtils.GetPropertyTooltip("FrontBackInversion")), effect.FrontBackInversion);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        effect.UpDownInversion = EditorGUILayout.ToggleLeft(new GUIContent("UpDownInversion", PropertiesUtils.GetPropertyTooltip("UpDownInversion")), effect.UpDownInversion);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        effect.RightLeftInversion = EditorGUILayout.ToggleLeft(new GUIContent("RightLeftInversion", PropertiesUtils.GetPropertyTooltip("RightLeftInversion")), effect.RightLeftInversion);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        effect.FrontBackAddition = EditorGUILayout.ToggleLeft(new GUIContent("FrontBackAddition", PropertiesUtils.GetPropertyTooltip("FrontBackAddition")), effect.FrontBackAddition);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        effect.UpDownAddition = EditorGUILayout.ToggleLeft(new GUIContent("UpDownAddition", PropertiesUtils.GetPropertyTooltip("UpDownAddition")), effect.UpDownAddition);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        effect.RightLeftAddition = EditorGUILayout.ToggleLeft(new GUIContent("RightLeftAddition", PropertiesUtils.GetPropertyTooltip("RightLeftAddition")), effect.RightLeftAddition);
        EditorGUILayout.EndHorizontal();




        effect.TargetDevice = EditorGUILayout.ObjectField(new GUIContent("TargetDevice", PropertiesUtils.GetPropertyTooltip("TargetDevice")), 
            effect.TargetDevice, typeof(SkineticDevice), !EditorUtility.IsPersistent(effect)) as SkineticDevice;

        effect.TargetPattern = EditorGUILayout.ObjectField(new GUIContent("TargetPattern", PropertiesUtils.GetPropertyTooltip("TargetDevice")), 
            effect.TargetPattern, typeof(PatternAsset), true) as PatternAsset;


        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(effect);
        }
    }

    private void OnEnable()
    {   
        m_targetEffect = (HapticEffect)target;
        serializedObject.ApplyModifiedProperties();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        DisplayEffect(m_targetEffect);
        serializedObject.ApplyModifiedProperties();
    }
}

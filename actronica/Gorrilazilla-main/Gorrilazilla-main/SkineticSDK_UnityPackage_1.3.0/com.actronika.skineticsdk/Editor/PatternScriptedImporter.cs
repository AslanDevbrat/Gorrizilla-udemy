using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor.AssetImporters;
using UnityEditor.Experimental.AssetImporters;
using UnityEngine;

namespace Skinetic
{
    [ScriptedImporter(1, "spn")]
    public class PatternScriptedImporter : ScriptedImporter
    {
        public override void OnImportAsset(AssetImportContext ctx)
        {
            PatternAsset effect = CreatePatternAsset(ctx.assetPath);
            ctx.AddObjectToAsset("Main", effect);
            ctx.SetMainObject(effect);
        }

        private static PatternAsset CreatePatternAsset(string filePath)
        {
            PatternAsset effect = ScriptableObject.CreateInstance<PatternAsset>();

            effect.Json = File.ReadAllText(filePath);
            effect.Name = Path.GetFileNameWithoutExtension(filePath);
            return effect;
        }
    }
}

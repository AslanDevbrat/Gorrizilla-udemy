using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Skinetic
{
    public class PatternAsset : ScriptableObject
    {
        /// <summary>
        /// Name of the pattern which is also the name of the .spn file.
        /// </summary>
        public string Name;

        /// <summary>
        /// Json describing the pattern.
        /// </summary>
        public string Json;
    }
}

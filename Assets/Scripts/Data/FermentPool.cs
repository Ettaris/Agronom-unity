using System.Collections.Generic;
using UnityEngine;

namespace Data
{
    [CreateAssetMenu(fileName = "FermentPool", menuName = "Game/Ferment Pool")]
    public class FermentPool : ScriptableObject
    {
        public List<FermentData> ferments = new List<FermentData>();
    }
}
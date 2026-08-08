using Gameplay;
using UnityEngine;

namespace Infrastructure
{
    public interface IRunAware
    {
        void OnRunDataSetup(RunData runData);
    }
}

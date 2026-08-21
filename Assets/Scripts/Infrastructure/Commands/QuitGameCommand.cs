using Infrastructure;
using UnityEngine;

namespace Commands
{
    public struct QuitGameCommand : ICommand
    {
        public void Execute()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
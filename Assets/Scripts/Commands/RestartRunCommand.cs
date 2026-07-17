using Infrastructure;

namespace Commands
{
    public struct RestartRunCommand : ICommand
    {
        public void Execute()
        {
            var runManager = ServiceLocator.Get<Managers.RunManager>();
            if (runManager != null)
            {
                int newSeed = UnityEngine.Random.Range(0, int.MaxValue);
                runManager.StartNewRun(newSeed);
            }
        }
    }
}
using Infrastructure;

namespace Commands
{
    public struct RestartRunCommand : ICommand
    {
        public void Execute()
        {
            var gameManager = ServiceLocator.Get<GameManager>();
            if (gameManager != null)
            {
                gameManager.RestartGame();
            }
        }
    }
}
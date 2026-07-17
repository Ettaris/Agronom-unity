using Infrastructure;

namespace Commands
{
    public struct GoToMainMenuCommand : ICommand
    {
        public void Execute()
        {
            // Загружаем сцену главного меню
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        }
    }
}
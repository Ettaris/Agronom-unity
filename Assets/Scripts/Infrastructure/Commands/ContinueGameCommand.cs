using Infrastructure;
using Managers;
using UnityEngine.SceneManagement;

namespace Commands
{
    public struct ContinueGameCommand : ICommand
    {
        public void Execute()
        {
            // Убеждаемся, что флаг новой игры сброшен
            GameManager.IsNewGame = false;
            GameManager.NewGameSeed = -1;
            // Загружаем игровую сцену
            SceneManager.LoadScene("GameScene");
        }
    }
}
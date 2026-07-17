using Infrastructure;
using Managers;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Commands
{
    public struct StartNewGameCommand : ICommand
    {
        public void Execute()
        {
            // Устанавливаем флаг новой игры
            GameManager.IsNewGame = true;
            // Можно передать seed (опционально)
            GameManager.NewGameSeed = Random.Range(0, int.MaxValue);
            // Загружаем игровую сцену
            SceneManager.LoadScene("GameScene");
        }
    }
}
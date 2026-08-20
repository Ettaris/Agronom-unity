using Infrastructure;
using Managers;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Commands
{
    [Serializable]
    public struct StartNewGameCommand : ICommand
    {
        public void Execute()
        {
            GameManager.IsNewGame = true;
            GameManager.NewGameSeed = UnityEngine.Random.Range(0, int.MaxValue);
            SceneManager.LoadScene("GameScene");
        }
    }
}
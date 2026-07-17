using Gameplay;
using Data;
using Infrastructure;

namespace Commands
{
    public struct AnalyzeCommand : ICommand
    {
        public PlantInstance Plant;
        public FermentData Ferment;

        public void Execute()
        {
            var analyzer = ServiceLocator.Get<Systems.AnalyzerSystem>();
            analyzer.AnalyzePlant(Plant, Ferment);
        }
    }
}
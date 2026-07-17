using Gameplay;
using Data;
using Infrastructure;

namespace Commands
{
    public struct TransferGenomeCommand : ICommand
    {
        public PlantInstance Donor;
        public PlantInstance Target;
        public BatteryData Battery;

        public void Execute()
        {
            var centrifuge = ServiceLocator.Get<Systems.CentrifugeSystem>();
            centrifuge.TransferGenome(Donor, Target, Battery);
        }
    }
}
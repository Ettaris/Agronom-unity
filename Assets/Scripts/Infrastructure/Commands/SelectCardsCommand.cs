using System.Collections.Generic;
using Infrastructure;
using Gameplay;
using Systems;

public struct SelectCardsCommand : ICommand
{
    public List<ItemInstance> SelectedItems;

    public void Execute()
    {
        var system = ServiceLocator.Get<CardDrawSystem>();
        system.SelectCards(SelectedItems);
    }
}
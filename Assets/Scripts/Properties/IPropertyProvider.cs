using Gameplay;
using System.Collections.Generic;

public interface IPropertyProvider
{
    List<GenomePropertyInstance> GetProperties(PlantInstance plant);
}
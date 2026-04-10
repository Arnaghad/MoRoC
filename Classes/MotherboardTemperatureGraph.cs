namespace MoRoC.Classes;

public class MotherboardTemperatureGraph : BaseGraph
{
    public MotherboardTemperatureGraph()
    {
        InitializePlot("Time (s)", "Temperature (°C)");
    }
}
namespace MoRoC.Classes;

public class CpuTemperatureGraph : BaseGraph
{
    public CpuTemperatureGraph()
    {
        InitializePlot("Time (s)", "Temperature (°C)");
    }
}
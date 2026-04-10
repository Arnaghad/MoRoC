namespace MoRoC.Classes;

public class GpuTemperatureGraph : BaseGraph
{
    public GpuTemperatureGraph()
    {
        InitializePlot("Time (s)", "Temperature (°C)");
    }
}
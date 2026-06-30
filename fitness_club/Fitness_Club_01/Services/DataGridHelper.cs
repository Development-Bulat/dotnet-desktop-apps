using Avalonia.Controls;

namespace Fitness_Club_01.Services;

public static class DataGridHelper
{
    public static void RemoveColumn(DataGrid grid, int index)
    {
        if (index >= 0 && index < grid.Columns.Count)
            grid.Columns.RemoveAt(index);
    }
}

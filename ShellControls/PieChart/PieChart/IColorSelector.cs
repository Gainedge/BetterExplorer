using System.Windows.Media;

namespace ShellControls.PieChart.PieChart
{
    public interface IColorSelector
    {
        /// <summary>
        /// Selects a suitable brush based on the item and/or its position withn a collection.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        Brush SelectBrush(object item, int index);
    }
}

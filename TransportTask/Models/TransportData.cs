using System.Text.Json.Serialization;

namespace TransportTask.Models
{
    /// <summary>
    /// Модель входных данных транспортной задачи
    /// </summary>
    public class TransportData
    {
        [JsonPropertyName("costs")]
        public int[][] Costs { get; set; }   // Матрица стоимостей

        [JsonPropertyName("supply")]
        public int[] Supply { get; set; }    // Запасы поставщиков

        [JsonPropertyName("demand")]
        public int[] Demand { get; set; }    // Потребности потребителей

        public TransportData() { }

        public TransportData(int rows, int cols)
        {
            Costs = new int[rows][];
            for (int i = 0; i < rows; i++)
                Costs[i] = new int[cols];

            Supply = new int[rows];
            Demand = new int[cols];
        }
    }
}
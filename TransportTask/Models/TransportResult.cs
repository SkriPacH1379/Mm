using System.Collections.Generic;
using System.Text;

namespace TransportTask.Models
{
    /// <summary>
    /// Результат решения транспортной задачи
    /// Содержит план, стоимость и пошаговое описание (для понятности)
    /// </summary>
    public class TransportResult
    {
        /// <summary>
        /// План перевозок (сколько от кого кому везём)
        /// </summary>
        public int[][] Plan { get; set; }

        /// <summary>
        /// Общая стоимость всех перевозок
        /// </summary>
        public int TotalCost { get; set; }

        /// <summary>
        /// Название использованного метода
        /// </summary>
        public string MethodName { get; set; }

        /// <summary>
        /// Пошаговое описание, как алгоритм пришёл к решению
        /// </summary>
        public List<string> Steps { get; set; } = new List<string>();

        /// <summary>
        /// Формирует красивый текст результата для отображения
        /// </summary>
        public string ToFormattedString()
        {
            var sb = new StringBuilder();

            sb.AppendLine($"=== {MethodName} ===");
            sb.AppendLine();

            if (Steps.Count > 0)
            {
                sb.AppendLine("Пошаговое решение:");
                foreach (var step in Steps)
                    sb.AppendLine("  " + step);
                sb.AppendLine();
            }

            sb.AppendLine("Итоговый план перевозок:");
            for (int i = 0; i < Plan.Length; i++)
            {
                for (int j = 0; j < Plan[i].Length; j++)
                {
                    sb.Append(Plan[i][j].ToString().PadLeft(6));
                }
                sb.AppendLine();
            }

            sb.AppendLine();
            sb.AppendLine($"Общая стоимость: {TotalCost} у.е.");

            return sb.ToString();
        }
    }
}
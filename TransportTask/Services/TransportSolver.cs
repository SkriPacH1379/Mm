using System;
using System.Collections.Generic;
using TransportTask.Models;

namespace TransportTask.Services
{
    /// <summary>
    /// Класс, реализующий алгоритмы решения транспортной задачи
    /// Здесь вся бизнес-логика расчёта плана перевозок
    /// </summary>
    public static class TransportSolver
    {
        /// <summary>
        /// Метод северо-западного угла
        /// Простой алгоритм: заполняем таблицу начиная с верхнего левого угла
        /// </summary>
        public static TransportResult SolveNorthWest(TransportData data)
        {
            int rows = data.Supply.Length;
            int cols = data.Demand.Length;

            // Копируем массивы, чтобы не менять оригинальные данные
            int[] supply = (int[])data.Supply.Clone();
            int[] demand = (int[])data.Demand.Clone();

            int[][] plan = new int[rows][];
            for (int r = 0; r < rows; r++)
                plan[r] = new int[cols];

            var steps = new List<string>();
            steps.Add("🔹 Метод северо-западного угла");
            steps.Add("Алгоритм начинает заполнение с верхнего левого угла (A1-B1) и движется вправо или вниз.");

            int step = 1;
            int i = 0, j = 0;

            while (i < rows && j < cols)
            {
                int quantity = Math.Min(supply[i], demand[j]);

                plan[i][j] = quantity;

                steps.Add($"Шаг {step++}. Ячейка A{i + 1} → B{j + 1}: перевозим {quantity} единиц " +
                          $"(ограничение: запас A{i + 1} = {supply[i]}, потребность B{j + 1} = {demand[j]})");

                supply[i] -= quantity;
                demand[j] -= quantity;

                if (supply[i] == 0)
                    steps.Add($"   → Поставщик A{i + 1} полностью использован. Переходим к следующему поставщику.");
                else
                    steps.Add($"   → Потребитель B{j + 1} полностью удовлетворён. Переходим к следующему потребителю.");

                if (supply[i] == 0) i++;
                if (demand[j] == 0) j++;
            }

            steps.Add("✅ Решение по методу северо-западного угла завершено.");

            return new TransportResult
            {
                Plan = plan,
                TotalCost = CalculateTotalCost(plan, data.Costs),
                MethodName = "Северо-западный угол",
                Steps = steps
            };
        }

        /// <summary>
        /// Метод минимального элемента (более оптимальный)
        /// На каждом шаге выбирается самая дешёвая доступная ячейка
        /// </summary>
        public static TransportResult SolveMinimumCost(TransportData data)
        {
            int rows = data.Supply.Length;
            int cols = data.Demand.Length;

            int[] supply = (int[])data.Supply.Clone();
            int[] demand = (int[])data.Demand.Clone();
            int[][] plan = new int[rows][];

            for (int r = 0; r < rows; r++)
                plan[r] = new int[cols];

            var steps = new List<string>();
            steps.Add("🔹 Метод минимального элемента");
            steps.Add("Алгоритм на каждом шаге ищет самую дешёвую доступную ячейку.");

            int step = 1;

            while (true)
            {
                int minCost = int.MaxValue;
                int bestRow = -1;
                int bestCol = -1;

                // Поиск самой дешёвой ячейки среди доступных
                for (int r = 0; r < rows; r++)
                {
                    if (supply[r] <= 0) continue;
                    for (int c = 0; c < cols; c++)
                    {
                        if (demand[c] <= 0) continue;

                        if (data.Costs[r][c] < minCost)
                        {
                            minCost = data.Costs[r][c];
                            bestRow = r;
                            bestCol = c;
                        }
                    }
                }

                if (bestRow == -1) break; // Все ячейки заполнены

                int quantity = Math.Min(supply[bestRow], demand[bestCol]);
                plan[bestRow][bestCol] = quantity;

                steps.Add($"Шаг {step++}. Самая дешёвая ячейка A{bestRow + 1} → B{bestCol + 1} " +
                          $"(стоимость = {minCost} руб.). Перевозим {quantity} единиц.");

                supply[bestRow] -= quantity;
                demand[bestCol] -= quantity;
            }

            steps.Add("✅ Решение по методу минимального элемента завершено.");

            return new TransportResult
            {
                Plan = plan,
                TotalCost = CalculateTotalCost(plan, data.Costs),
                MethodName = "Минимальный элемент",
                Steps = steps
            };
        }

        /// <summary>
        /// Расчёт общей стоимости по плану
        /// </summary>
        private static int CalculateTotalCost(int[][] plan, int[][] costs)
        {
            int total = 0;
            for (int i = 0; i < plan.Length; i++)
                for (int j = 0; j < plan[i].Length; j++)
                    total += plan[i][j] * costs[i][j];
            return total;
        }
    }
}
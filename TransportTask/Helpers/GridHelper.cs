using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using TransportTask.Models;

namespace TransportTask.Helpers
{
    /// <summary>
    /// Вспомогательный класс для работы с DataGrid
    /// Содержит все операции по созданию, чтению и заполнению таблиц
    /// Это позволяет держать MainWindow чистым
    /// </summary>
    public static class GridHelper
    {
        /// <summary>
        /// Создаёт таблицу стоимостей с подписями A1, A2... и B1, B2...
        /// </summary>
        public static void CreateCostGrid(DataGrid grid, int rows, int cols)
        {
            grid.Columns.Clear();
            var items = new List<object[]>();

            // Создаём пустые строки
            for (int i = 0; i < rows; i++)
            {
                var row = new object[cols];
                for (int j = 0; j < cols; j++) row[j] = 0;
                items.Add(row);
            }

            // Создаём столбцы с заголовками B1, B2...
            for (int j = 0; j < cols; j++)
            {
                grid.Columns.Add(new DataGridTextColumn
                {
                    Header = $"B{j + 1}",
                    Binding = new Binding($"[{j}]"),
                    Width = new DataGridLength(90)
                });
            }

            // Добавляем подписи строк A1, A2... слева
            grid.RowHeaderWidth = 60;
            grid.LoadingRow += (sender, e) =>
            {
                e.Row.Header = $"A{e.Row.GetIndex() + 1}";
            };

            grid.ItemsSource = items;
            grid.CanUserSortColumns = false; // Отключаем сортировку, чтобы не ломать данные
        }

        /// <summary>
        /// Создаёт таблицу запасов (поставщики)
        /// </summary>
        public static void CreateSupplyGrid(DataGrid grid, int rows)
        {
            grid.Columns.Clear();
            grid.Columns.Add(new DataGridTextColumn
            {
                Header = "Запасы",
                Binding = new Binding("[0]"),
                Width = 130
            });

            grid.RowHeaderWidth = 60;
            grid.LoadingRow += (sender, e) =>
            {
                e.Row.Header = $"A{e.Row.GetIndex() + 1}";
            };

            var items = new List<object[]>();
            for (int i = 0; i < rows; i++)
                items.Add(new object[] { 0 });

            grid.ItemsSource = items;
            grid.CanUserSortColumns = false;
        }

        /// <summary>
        /// Создаёт таблицу потребностей (потребители)
        /// </summary>
        public static void CreateDemandGrid(DataGrid grid, int cols)
        {
            grid.Columns.Clear();
            grid.Columns.Add(new DataGridTextColumn
            {
                Header = "Потребности",
                Binding = new Binding("[0]"),
                Width = 130
            });

            var items = new List<object[]>();
            for (int i = 0; i < cols; i++)
                items.Add(new object[] { 0 });

            grid.ItemsSource = items;
            grid.CanUserSortColumns = false;
        }

        /// <summary>
        /// Читает матрицу стоимостей из DataGrid
        /// </summary>
        public static int[][] ReadCostMatrix(DataGrid grid)
        {
            var matrix = new List<int[]>();
            if (grid.ItemsSource is List<object[]> items)
            {
                foreach (var row in items)
                {
                    var newRow = new int[row.Length];
                    for (int j = 0; j < row.Length; j++)
                    {
                        newRow[j] = int.TryParse(row[j]?.ToString() ?? "0", out int v) ? v : 0;
                    }
                    matrix.Add(newRow);
                }
            }
            return matrix.ToArray();
        }

        /// <summary>
        /// Читает вектор запасов
        /// </summary>
        public static int[] ReadSupply(DataGrid grid)
        {
            return ReadSingleColumn(grid);
        }

        /// <summary>
        /// Читает вектор потребностей
        /// </summary>
        public static int[] ReadDemand(DataGrid grid)
        {
            return ReadSingleColumn(grid);
        }

        private static int[] ReadSingleColumn(DataGrid grid)
        {
            var list = new List<int>();
            if (grid.ItemsSource is List<object[]> items)
            {
                foreach (var row in items)
                {
                    if (row != null && row.Length > 0)
                    {
                        int val = int.TryParse(row[0]?.ToString() ?? "0", out int v) ? v : 0;
                        list.Add(val);
                    }
                }
            }
            return list.ToArray();
        }

        /// <summary>
        /// Заполняет таблицу стоимостей данными из JSON
        /// </summary>
        public static void FillCostGrid(DataGrid grid, int[][] costs)
        {
            if (costs == null) return;
            CreateCostGrid(grid, costs.Length, costs[0].Length);

            if (grid.ItemsSource is List<object[]> items)
            {
                for (int i = 0; i < costs.Length && i < items.Count; i++)
                {
                    for (int j = 0; j < costs[i].Length && j < items[i].Length; j++)
                    {
                        items[i][j] = costs[i][j];
                    }
                }
            }
        }

        public static void FillSupplyGrid(DataGrid grid, int[] supply)
        {
            if (supply == null) return;
            CreateSupplyGrid(grid, supply.Length);

            if (grid.ItemsSource is List<object[]> items)
            {
                for (int i = 0; i < supply.Length && i < items.Count; i++)
                    items[i][0] = supply[i];
            }
        }

        public static void FillDemandGrid(DataGrid grid, int[] demand)
        {
            if (demand == null) return;
            CreateDemandGrid(grid, demand.Length);

            if (grid.ItemsSource is List<object[]> items)
            {
                for (int i = 0; i < demand.Length && i < items.Count; i++)
                    items[i][0] = demand[i];
            }
        }

        /// <summary>
        /// Очищает все таблицы
        /// </summary>
        public static void ClearGrids(DataGrid costs, DataGrid supply, DataGrid demand)
        {
            costs.ItemsSource = null;
            supply.ItemsSource = null;
            demand.ItemsSource = null;
        }

        /// <summary>
        /// Глобальный обработчик валидации (только цифры)
        /// </summary>
        public static void NumberValidation(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !char.IsDigit(e.Text, 0);
        }
               
    }
}
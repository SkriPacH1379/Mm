using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TransportTask.Helpers;
using TransportTask.Models;
using TransportTask.Services;

namespace TransportTask.Views
{
    /// <summary>
    /// Главное окно приложения "Транспортная задача"
    /// Здесь собрана вся логика взаимодействия с пользователем
    /// </summary>
    public partial class MainWindow : Window
    {
        private TransportData currentData; // Хранит текущие данные для сохранения

        public MainWindow()
        {
            InitializeComponent();            
            CreateInitialGrids();      // Создаём таблицы по умолчанию (3x4)

            dgCosts.CellEditEnding += Grid_CellEditEnding;
            dgSupply.CellEditEnding += Grid_CellEditEnding;
            dgDemand.CellEditEnding += Grid_CellEditEnding;

            statusText.Text = "Приложение готово к работе";
        }

        /// <summary>
        /// Создаёт таблицы по умолчанию при запуске приложения
        /// </summary>
        private void CreateInitialGrids()
        {
            GridHelper.CreateCostGrid(dgCosts, 3, 4);
            GridHelper.CreateSupplyGrid(dgSupply, 3);
            GridHelper.CreateDemandGrid(dgDemand, 4);
        }

        /// <summary>
        /// Создаёт таблицы по размерам, введённым пользователем
        /// </summary>
        private void BtnCreateGrids_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(tbRows.Text, out int rows) && rows > 0 &&
                int.TryParse(tbCols.Text, out int cols) && cols > 0)
            {
                GridHelper.CreateCostGrid(dgCosts, rows, cols);
                GridHelper.CreateSupplyGrid(dgSupply, rows);
                GridHelper.CreateDemandGrid(dgDemand, cols);

                statusText.Text = $"Созданы таблицы: {rows} поставщиков × {cols} потребителей";
            }
            else
            {
                MessageBox.Show("Введите корректные размеры таблиц (числа больше 0).", "Ошибка");
            }
        }

        /// <summary>
        /// Читает все данные из таблиц (матрица + запасы + потребности)
        /// </summary>
        private TransportData ReadData()
        {
            currentData = new TransportData
            {
                Costs = GridHelper.ReadCostMatrix(dgCosts),
                Supply = GridHelper.ReadSupply(dgSupply),
                Demand = GridHelper.ReadDemand(dgDemand)
            };
            return currentData;
        }

        /// <summary>
        /// Основной метод решения задачи (вызывается из кнопок и меню)
        /// </summary>
        private void Solve(SolveMethod method)
        {
            try
            {
                var data = ReadData();

                // Проверки перед решением
                if (!ValidationService.CheckBalance(data) || !ValidationService.IsValidData(data))
                    return;

                // Вызываем нужный алгоритм
                TransportResult result = method == SolveMethod.NorthWest
                    ? TransportSolver.SolveNorthWest(data)
                    : TransportSolver.SolveMinimumCost(data);

                // Отображаем результат
                tbResult.Text = result.ToFormattedString();
                tbTotalCost.Text = $"Общая стоимость: {result.TotalCost} у.е.";

                statusText.Text = $"Решено: {result.MethodName}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при решении:\n{ex.Message}", "Ошибка",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ================== Кнопки и меню ==================

        private void BtnNorthWest_Click(object sender, RoutedEventArgs e) => Solve(SolveMethod.NorthWest);
        private void BtnMinimumCost_Click(object sender, RoutedEventArgs e) => Solve(SolveMethod.MinimumCost);

        private void MenuNorthWest_Click(object sender, RoutedEventArgs e) => Solve(SolveMethod.NorthWest);
        private void MenuMinimumCost_Click(object sender, RoutedEventArgs e) => Solve(SolveMethod.MinimumCost);

        private void BtnLoadJson_Click(object sender, RoutedEventArgs e) => LoadJson();
        private void MenuLoadJson_Click(object sender, RoutedEventArgs e) => LoadJson();

        private void LoadJson()
        {
            var dialog = new OpenFileDialog
            {
                Filter = "JSON файлы (*.json)|*.json|Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*"
            };

            if (dialog.ShowDialog() != true) return;

            try
            {
                string extension = System.IO.Path.GetExtension(dialog.FileName).ToLower();

                if (extension == ".txt")
                {
                    // Загрузка из TXT (просто показываем содержимое)
                    string content = File.ReadAllText(dialog.FileName);
                    tbResult.Text = content;
                    statusText.Text = "Загружен TXT файл (только просмотр)";
                    MessageBox.Show("TXT файл загружен в поле результата (только для просмотра).", "Успех");
                    return;
                }

                // Загрузка JSON (как раньше)
                string json = File.ReadAllText(dialog.FileName);
                var root = JsonSerializer.Deserialize<JsonElement>(json);

                TransportData data;

                if (root.TryGetProperty("Data", out var dataElement))
                {
                    data = JsonSerializer.Deserialize<TransportData>(dataElement.GetRawText());
                }
                else
                {
                    data = JsonSerializer.Deserialize<TransportData>(json);
                }

                currentData = data;

                int rows = data.Costs.Length;
                int cols = data.Costs[0].Length;

                GridHelper.CreateCostGrid(dgCosts, rows, cols);
                GridHelper.CreateSupplyGrid(dgSupply, rows);
                GridHelper.CreateDemandGrid(dgDemand, cols);

                GridHelper.FillCostGrid(dgCosts, data.Costs);
                GridHelper.FillSupplyGrid(dgSupply, data.Supply);
                GridHelper.FillDemandGrid(dgDemand, data.Demand);

                statusText.Text = $"Загружено: {dialog.FileName}";
                MessageBox.Show("Данные успешно загружены!", "Успех");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки:\n{ex.Message}", "Ошибка");
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e) => SaveResult();
        private void MenuSave_Click(object sender, RoutedEventArgs e) => SaveResult();

        private void SaveResult()
        {
            if (string.IsNullOrWhiteSpace(tbResult.Text))
            {
                MessageBox.Show("Сначала решите задачу.");
                return;
            }

            var dialog = new SaveFileDialog
            {
                Filter = "JSON файлы (*.json)|*.json|Текстовые файлы (*.txt)|*.txt",
                DefaultExt = "json"
            };

            if (dialog.ShowDialog() != true) return;

            try
            {
                string content;

                if (dialog.FileName.EndsWith(".txt"))
                {
                    // Сохранение в читаемый TXT
                    content = tbResult.Text + "\n\n" + tbTotalCost.Text;
                }
                else
                {
                    // Сохранение в JSON
                    var saveObject = new
                    {
                        Data = currentData,
                        Result = tbResult.Text,
                        TotalCost = tbTotalCost.Text,
                        Date = DateTime.Now
                    };

                    content = JsonSerializer.Serialize(saveObject, new JsonSerializerOptions { WriteIndented = true });
                }

                File.WriteAllText(dialog.FileName, content);

                statusText.Text = $"Результат сохранён в {dialog.FileName}";
                MessageBox.Show("Результат успешно сохранён!", "Успех");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}");
            }
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            GridHelper.ClearGrids(dgCosts, dgSupply, dgDemand);
            tbResult.Clear();
            tbTotalCost.Text = "";
            statusText.Text = "Все поля очищены";
        }

        private void BtnLoadExample_Click(object sender, RoutedEventArgs e)
        {
            // Загружаем готовый пример
            GridHelper.CreateCostGrid(dgCosts, 3, 3);
            GridHelper.CreateSupplyGrid(dgSupply, 3);
            GridHelper.CreateDemandGrid(dgDemand, 3);

            var costs = new int[][]
            {
                new int[] {5, 3, 7},
                new int[] {4, 6, 2},
                new int[] {8, 1, 4}
            };

            GridHelper.FillCostGrid(dgCosts, costs);
            GridHelper.FillSupplyGrid(dgSupply, new int[] { 30, 25, 35 });
            GridHelper.FillDemandGrid(dgDemand, new int[] { 20, 40, 30 });

            statusText.Text = "Загружен пример данных (3x3)";
        }

        private void MenuExit_Click(object sender, RoutedEventArgs e) => Close();

        private void MenuAbout_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Транспортная задача\n", "О программе");
        }

        // ================== ВАЛИДАЦИЯ ВВОДА ==================

        private void NumberValidation(object sender, TextCompositionEventArgs e)
        {
            // Разрешаем только цифры
            e.Handled = !char.IsDigit(e.Text, 0);
        }
              
        private void Grid_PreparingCellForEdit(object sender, DataGridPreparingCellForEditEventArgs e)
        {
            if (e.EditingElement is TextBox textBox)
            {
                textBox.PreviewTextInput += (s, args) =>
                {
                    args.Handled = !char.IsDigit(args.Text, 0);
                };

                textBox.PreviewKeyDown += (s, args) =>
                {
                    if (args.Key == Key.V && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
                        args.Handled = true;
                };
            }
        }

        // Добавь этот новый метод
        private void Grid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditingElement is TextBox textBox)
            {
                string clean = new string(textBox.Text.Where(char.IsDigit).ToArray());
                if (clean != textBox.Text)
                {
                    textBox.Text = clean;
                }
            }
        }
    }
}
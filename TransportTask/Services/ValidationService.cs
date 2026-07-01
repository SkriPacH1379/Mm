using System.Linq;
using System.Windows;
using TransportTask.Models;

namespace TransportTask.Services
{
    /// <summary>
    /// Сервис проверки данных перед решением задачи
    /// </summary>
    public static class ValidationService
    {
        /// <summary>
        /// Проверяет баланс: сумма запасов должна равняться сумме потребностей
        /// </summary>
        public static bool CheckBalance(TransportData data)
        {
            if (data == null) return false;

            int totalSupply = data.Supply?.Sum() ?? 0;
            int totalDemand = data.Demand?.Sum() ?? 0;

            if (totalSupply != totalDemand)
            {
                MessageBox.Show(
                    $"Нарушение баланса!\n\n" +
                    $"Запасы поставщиков: {totalSupply}\n" +
                    $"Потребности потребителей: {totalDemand}\n\n" +
                    "Сумма запасов должна быть равна сумме потребностей.",
                    "Ошибка баланса",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return false;
            }

            return true;
        }

        /// <summary>
        /// Проверяет, что все значения неотрицательные
        /// </summary>
        public static bool IsValidData(TransportData data)
        {
            if (data?.Costs == null || data.Supply == null || data.Demand == null)
                return false;

            // Проверка матрицы стоимостей
            foreach (var row in data.Costs)
            {
                foreach (var cost in row)
                {
                    if (cost < 0) return false;
                }
            }

            // Проверка запасов и потребностей
            return data.Supply.All(x => x >= 0) && data.Demand.All(x => x >= 0);
        }
    }
}
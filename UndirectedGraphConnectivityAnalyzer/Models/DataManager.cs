using Avalonia.Controls;
using Avalonia.Platform.Storage;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace UndirectedGraphConnectivityAnalyzer.Models
{
    /// <summary>
    /// Абстрактный класс манипулирования данными некоторых элементов.
    /// </summary>
    public abstract class DataManager<T>
    {
        /// <summary>
        /// Определяет лист элементов, которыми будет управлять.
        /// </summary>
        public ObservableCollection<T> Elements { get; set; } = new ObservableCollection<T>();

        /// <summary>
        /// Определяет Excel формат файлов.
        /// </summary>
        public static FilePickerFileType ExcelType { get; } = new("Excel")
        {
            Patterns = new[] { "*.xlsx" }
        };

        /// <summary>
        /// Загружает данные, вызывая диалоговое окно с выбором файла.
        /// </summary>
        public abstract Task ReLoadAsync(UserControl view);

        /// <summary>
        /// Добавляет данные к уже существующим, вызывая диалоговое окно с выбором файла.
        /// </summary>
        public abstract Task AddAsync(UserControl view);

        /// <summary>
        /// Добавляет элемент к существующим данным, вызывая диалоговое окно с установкой параметров.
        /// </summary>
        public abstract Task CreateAsync(UserControl view);

        /// <summary>
        /// Очищает данные.
        /// </summary>
        public void Clear()
        {
            Elements.Clear();
        }
    }
}

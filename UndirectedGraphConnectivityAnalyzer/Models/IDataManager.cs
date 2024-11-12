using System.Threading.Tasks;

namespace UndirectedGraphConnectivityAnalyzer.Models
{
    /// <summary>
    /// Интерфейс манипулирования данными.
    /// </summary>
    internal interface IDataManager
    {
        /// <summary>
        /// Загружает данные, вызывая диалоговое окно с выбором файла.
        /// </summary>
        Task LoadAsync();

        /// <summary>
        /// Добавляет данные к уже существующим, вызывая диалоговое окно с выбором файла.
        /// </summary>
        Task AddAsync();

        /// <summary>
        /// Добавляет элемент к существующим данным, вызывая диалоговое окно с установкой параметров.
        /// </summary>
        void Create();

        /// <summary>
        /// Очищает данные.
        /// </summary>
        void Clear();
    }
}

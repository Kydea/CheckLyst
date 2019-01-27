namespace Matematici.DataEngine
{
    public class PagingRequest<T>
    {
        private const int DefaultItemsPerPage = 10;

        public int CurrentPage { get; set; } = 1;

        private int _itemsPerPage = DefaultItemsPerPage;

        public int ItemsPerPage
        {
            get => _itemsPerPage;
            set => _itemsPerPage = value == 0 ? DefaultItemsPerPage : value;
        }

        public int Skip => (CurrentPage - 1) * ItemsPerPage;

        public T Data { get; set; }
    }
}

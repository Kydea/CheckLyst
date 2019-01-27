using System;

namespace Matematici.DataEngine
{
    public class PagingResponse<T> 
    {
        public int Count { get; set; }
        public int PagesCount => (int)Math.Ceiling(Count / (decimal)ItemsPerPage);
        public int CurrentPage { get; set; }
        public int ItemsPerPage { get; set; }
        public T Data { get; set; }
    }
}

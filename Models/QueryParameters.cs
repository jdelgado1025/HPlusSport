namespace HPlusSport.API.Models
{
    public class QueryParameters
    {
        //Private fields
        const int _maxSize = 100;
        private int _size = 50;
        private string _sortOrder = "asc";
        public int Page { get; set; } = 1;
        public int Size
        { 
            get {
                return _size;
            }
            set {
                _size = Math.Min(_maxSize, value);
            }
        }

        public string SortBy { get; set; } = "Id";

        public string SortOrder
        {
            get
            {
                return _sortOrder;
            }
            set
            {
                if(value == "asc" || value == "desc")
                {
                    _sortOrder = value;
                }
            }
        }
    }
}

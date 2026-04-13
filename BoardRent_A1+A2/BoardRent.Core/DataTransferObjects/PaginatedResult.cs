namespace BoardRent.DataTransferObjects
{
    using System;
    using System.Collections.Generic;

    public class PaginatedResult<TItem>
    {
        public List<TItem> Items { get; set; } = new List<TItem>();

        public int TotalItemCount { get; set; }

        public int PageNumber { get; set; }

        public int PageSize { get; set; }

        public int TotalPageCount =>
            this.PageSize > 0
                ? (int)Math.Ceiling((double)this.TotalItemCount / this.PageSize)
                : 0;
    }
}
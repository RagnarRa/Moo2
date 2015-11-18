using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoursesAPI.Services.Pagination
{
    /// <summary>
    /// A pagination envelope. Used for returning items in a page as well as some facts about the number of items to serve etc. 
    /// </summary>
    /// <typeparam name="T">The type of objects to return.</typeparam>
    public class Envelope<T>
    {
        public class PagingInfo
        {
            /// <summary>
            /// The number of items on a page.
            /// Example: 3
            /// </summary>
            public int PageCount { get; set; }
            /// <summary>
            /// The size of a page.
            /// Example: 3
            /// </summary>
            public int PageSize { get; set; }
            /// <summary>
            /// The number of pages.
            /// Example: 3
            /// </summary>
            public int PageNumber { get; set; }
            /// <summary>
            /// The total number of items available.
            /// Example: 3
            /// </summary>
            public int TotalNumberOfItems { get; set; }
        }
        //The actual data returned.. 
        public List<T> Items { get; set; }
        /// <summary>
        /// Object that contains general paging info.
        /// </summary>
        public PagingInfo Paging { get; set; }

        public Envelope(IEnumerable<T> items, int pageSize, int pageNumber, int totalNumberOfItems) 
        {
            Items = items.ToList();
            Paging = new PagingInfo
            {
                PageCount = totalNumberOfItems > 0 ? (int) Math.Ceiling(totalNumberOfItems / (double) pageSize) : 0,
                PageSize = pageSize,
                PageNumber = pageNumber,
                TotalNumberOfItems = totalNumberOfItems
            };
        }
    }
}

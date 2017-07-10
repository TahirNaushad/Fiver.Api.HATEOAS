using System.Collections.Generic;

namespace Fiver.Api.HATEOAS.Lib
{
    public class LinksWrapperList<T>
    {
        public List<LinksWrapper<T>> Values { get; set; }
        public List<LinkInfo> Links { get; set; }
    }
}

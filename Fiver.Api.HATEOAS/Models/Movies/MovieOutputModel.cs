using Fiver.Api.HATEOAS.Lib;
using System;
using System.Collections.Generic;

namespace Fiver.Api.HATEOAS.Models.Movies
{
    public class MovieListOutputModel
    {
        public PagingHeader Paging { get; set; }
        public List<LinkInfo> Links { get; set; }
        public List<MovieInfo> Items { get; set; }
    }

    public class MovieInfo
    {
        public List<LinkInfo> Links { get; set; }
        public int Id { get; set; }
        public string Title { get; set; }
        public int ReleaseYear { get; set; }
        public string Summary { get; set; }
        public DateTime LastReadAt { get; set; }
    }

    public class MovieOutputModel
    {
        public List<LinkInfo> Links { get; set; }
        public int Id { get; set; }
        public string Title { get; set; }
        public int ReleaseYear { get; set; }
        public string Summary { get; set; }
        public DateTime LastReadAt { get; set; }
    }
}

using Fiver.Api.HATEOAS.Lib;
using Fiver.Api.HATEOAS.Models.Movies;
using Fiver.Api.HATEOAS.OtherLayers;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fiver.Api.HATEOAS.Controllers
{
    [Route("movies")]
    public class MoviesController : BaseController
    {
        private readonly IMovieService service;
        private readonly IUrlHelper urlHelper;

        public MoviesController(
            IMovieService service,
            IUrlHelper urlHelper)
        {
            this.service = service;
            this.urlHelper = urlHelper;
        }

        [HttpGet(Name = "GetMovies")]
        public IActionResult Get(PagingParams pagingParams)
        {
            var model = service.GetMovies(pagingParams);

            Response.Headers.Add("X-Pagination", model.GetHeader().ToJson());

            var outputModel = new MovieListOutputModel
            {
                Paging = model.GetHeader(),
                Links = GetLinks(model),
                Items = model.List.Select(m => ToMovieInfo(m)).ToList(),
            };
            return Ok(outputModel);
        }

        [HttpGet("{id}", Name = "GetMovie")]
        public IActionResult Get(int id)
        {
            var model = service.GetMovie(id);
            if (model == null)
                return NotFound();

            var outputModel = ToOutputModel(model);
            return Ok(outputModel);
        }

        [HttpPost]
        public IActionResult Create([FromBody]MovieInputModel inputModel)
        {
            if (inputModel == null)
                return BadRequest();

            if (!ModelState.IsValid)
                return Unprocessable(ModelState);

            var model = ToDomainModel(inputModel);
            service.AddMovie(model);

            var outputModel = ToOutputModel(model);
            return CreatedAtRoute("GetMovie", new { id = outputModel.Id }, outputModel);
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody]MovieInputModel inputModel)
        {
            if (inputModel == null || id != inputModel.Id)
                return BadRequest();

            if (!service.MovieExists(id))
                return NotFound();

            if (!ModelState.IsValid)
                return new UnprocessableObjectResult(ModelState);

            var model = ToDomainModel(inputModel);
            service.UpdateMovie(model);

            return NoContent();
        }

        [HttpPatch("{id}")]
        public IActionResult UpdatePatch(
            int id, [FromBody]JsonPatchDocument<MovieInputModel> patch)
        {
            if (patch == null)
                return BadRequest();

            var model = service.GetMovie(id);
            if (model == null)
                return NotFound();

            var inputModel = ToInputModel(model);
            patch.ApplyTo(inputModel);

            TryValidateModel(inputModel);
            if (!ModelState.IsValid)
                return new UnprocessableObjectResult(ModelState);

            model = ToDomainModel(inputModel);
            service.UpdateMovie(model);

            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            if (!service.MovieExists(id))
                return NotFound();

            service.DeleteMovie(id);

            return NoContent();
        }

        #region " Links "

        private List<LinkInfo> GetLinks(PagedList<Movie> list)
        {
            var links = new List<LinkInfo>();

            if (list.HasPreviousPage)
                links.Add(CreateLink("GetMovies", list.PreviousPageNumber, list.PageSize, "previousPage", "GET"));

            links.Add(CreateLink("GetMovies", list.PageNumber, list.PageSize, "self", "GET"));

            if (list.HasNextPage)
                links.Add(CreateLink("GetMovies", list.NextPageNumber, list.PageSize, "nextPage", "GET"));

            return links;
        }

        private LinkInfo CreateLink(
            string routeName, int pageNumber, int pageSize,
            string rel, string method)
        {
            return new LinkInfo
            {
                Href = urlHelper.Link(routeName,
                            new { PageNumber = pageNumber, PageSize = pageSize }),
                Rel = rel,
                Method = method
            };
        }

        #endregion

        #region " Mappings "

        private MovieInfo ToMovieInfo(Movie model)
        {
            return new MovieInfo
            {
                Id = model.Id,
                Title = model.Title,
                ReleaseYear = model.ReleaseYear,
                Summary = model.Summary,
                LastReadAt = DateTime.Now
            };
        }

        private MovieOutputModel ToOutputModel(Movie model)
        {
            return new MovieOutputModel
            {
                Id = model.Id,
                Title = model.Title,
                ReleaseYear = model.ReleaseYear,
                Summary = model.Summary,
                LastReadAt = DateTime.Now
            };
        }
        
        private Movie ToDomainModel(MovieInputModel inputModel)
        {
            return new Movie
            {
                Id = inputModel.Id,
                Title = inputModel.Title,
                ReleaseYear = inputModel.ReleaseYear,
                Summary = inputModel.Summary
            };
        }

        private MovieInputModel ToInputModel(Movie model)
        {
            return new MovieInputModel
            {
                Id = model.Id,
                Title = model.Title,
                ReleaseYear = model.ReleaseYear,
                Summary = model.Summary
            };
        }
        
        #endregion
    }
}

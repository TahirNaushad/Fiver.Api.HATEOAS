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
        public IActionResult Get(
            PagingParams pagingParams,
            [FromHeader(Name = "Accept")]string acceptHeader)
        {
            var model = service.GetMovies(pagingParams);

            Response.Headers.Add("X-Pagination", model.GetHeader().ToJson());

            if (string.Equals(acceptHeader, "application/vnd.fiver.hateoas+json"))
            {
                var outputModel = ToOutputModel_Links(model);
                return Ok(outputModel);
            }
            else
            {
                var outputModel = ToOutputModel_Default(model);
                return Ok(outputModel);
            }
        }

        [HttpGet("{id}", Name = "GetMovie")]
        public IActionResult Get(int id,
            [FromHeader(Name = "Accept")]string acceptHeader)
        {
            var model = service.GetMovie(id);
            if (model == null)
                return NotFound();

            if (string.Equals(acceptHeader, "application/vnd.fiver.hateoas+json"))
            {
                var outputModel = ToOutputModel_Links(model);
                return Ok(outputModel);
            }
            else
            {
                var outputModel = ToOutputModel_Default(model);
                return Ok(outputModel);
            }
        }

        [ContentTypeOf("application/vnd.fiver.movie.input+json")]
        [HttpPost(Name = "CreateMovie")]
        public IActionResult Create(
            [FromBody]MovieInputModel inputModel,
            [FromHeader(Name = "Accept")]string acceptHeader)
        {
            if (inputModel == null)
                return BadRequest();

            if (!ModelState.IsValid)
                return Unprocessable(ModelState);

            var model = ToDomainModel(inputModel);
            service.AddMovie(model);

            if (string.Equals(acceptHeader, "application/vnd.fiver.hateoas+json"))
            {
                var outputModel = ToOutputModel_Links(model);
                return CreatedAtRoute("GetMovie", new { id = outputModel.Value.Id }, outputModel);
            }
            else
            {
                var outputModel = ToOutputModel_Default(model);
                return CreatedAtRoute("GetMovie", new { id = outputModel.Id }, outputModel);
            }
        }

        [HttpPut("{id}", Name = "UpdateMovie")]
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

        [HttpPatch("{id}", Name = "UpdatePatchMovie")]
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

        [HttpDelete("{id}", Name = "DeleteMovie")]
        public IActionResult Delete(int id)
        {
            if (!service.MovieExists(id))
                return NotFound();

            service.DeleteMovie(id);

            return NoContent();
        }

        #region " Links "

        private List<LinkInfo> GetLinks_List(PagedList<Movie> model)
        {
            var links = new List<LinkInfo>();

            links.Add(new LinkInfo
            {
                Href = urlHelper.Link("GetMovies",
                            new { PageNumber = model.PageNumber, PageSize = model.PageSize }),
                Rel = "self",
                Method = "GET"
            });

            if (model.HasPreviousPage)
                links.Add(new LinkInfo
                {
                    Href = urlHelper.Link("GetMovies",
                            new { PageNumber = model.PreviousPageNumber, PageSize = model.PageSize }),
                    Rel = "previous-page",
                    Method = "GET"
                });

            if (model.HasNextPage)
                links.Add(new LinkInfo
                {
                    Href = urlHelper.Link("GetMovies",
                            new { PageNumber = model.NextPageNumber, PageSize = model.PageSize }),
                    Rel = "next-page",
                    Method = "GET"
                });

            links.Add(new LinkInfo
            {
                Href = urlHelper.Link("CreateMovie", new { }),
                Rel = "create-movie",
                Method = "POST"
            });

            return links;
        }

        private List<LinkInfo> GetLinks_Model(Movie model)
        {
            var links = new List<LinkInfo>();

            links.Add(new LinkInfo
            {
                Href = urlHelper.Link("GetMovie", new { id = model.Id }),
                Rel = "self",
                Method = "GET"
            });

            links.Add(new LinkInfo
            {
                Href = urlHelper.Link("UpdateMovie", new { id = model.Id }),
                Rel = "update-movie",
                Method = "PUT"
            });

            links.Add(new LinkInfo
            {
                Href = urlHelper.Link("UpdatePatchMovie", new { id = model.Id }),
                Rel = "update-partial-movie",
                Method = "PATCH"
            });

            links.Add(new LinkInfo
            {
                Href = urlHelper.Link("DeleteMovie", new { id = model.Id }),
                Rel = "delete-movie",
                Method = "DELETE"
            });

            return links;
        }

        #endregion

        #region " Mappings "

        private List<MovieOutputModel> ToOutputModel_Default(PagedList<Movie> model)
        {
            return model.List.Select(m => ToOutputModel_Default(m)).ToList();
        }

        private LinksWrapperList<MovieOutputModel>
            ToOutputModel_Links(PagedList<Movie> model)
        {
            return new LinksWrapperList<MovieOutputModel>
            {
                Values = model.List.Select(m => ToOutputModel_Links(m)).ToList(),
                Links = GetLinks_List(model)
            };
        }

        private MovieOutputModel ToOutputModel_Default(Movie model)
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

        private LinksWrapper<MovieOutputModel> ToOutputModel_Links(Movie model)
        {
            return new LinksWrapper<MovieOutputModel>
            {
                Value = new MovieOutputModel
                {
                    Id = model.Id,
                    Title = model.Title,
                    ReleaseYear = model.ReleaseYear,
                    Summary = model.Summary,
                    LastReadAt = DateTime.Now
                },
                Links = GetLinks_Model(model)
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

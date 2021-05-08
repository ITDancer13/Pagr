using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pagr.Models;
using Pagr.Sample.Entities;
using Pagr.Services;

namespace Pagr.Sample.Controllers
{
    [Route("api/[controller]/[action]")]
    public class PostsController : Controller
    {
        private readonly IPagrProcessor _pagrProcessor;
        private readonly ApplicationDbContext _dbContext;

        public PostsController(IPagrProcessor pagrProcessor,
            ApplicationDbContext dbContext)
        {
            _pagrProcessor = pagrProcessor;
            _dbContext = dbContext;
        }

        [HttpGet]
        public JsonResult GetAllWithPagr(PagrModel pagrModel)
        {
            var result = _dbContext.Posts.AsNoTracking();

            result = _pagrProcessor.Apply(pagrModel, result);

            return Json(result.ToList());
        }

        [HttpGet]
        public JsonResult Create(int number = 10)
        {
            for (var i = 0; i < number; i++)
            {
                _dbContext.Posts.Add(new Post());
            }

            _dbContext.SaveChanges();

            return Json(_dbContext.Posts.ToList());
        }

        [HttpGet]
        public JsonResult GetAll()
        {
            return Json(_dbContext.Posts.ToList());
        }
    }
}

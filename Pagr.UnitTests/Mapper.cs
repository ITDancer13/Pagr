using System.Collections.Generic;
using System.Linq;
using Pagr.Exceptions;
using Pagr.Models;
using Pagr.UnitTests.Entities;
using Pagr.UnitTests.Services;
using Xunit;

namespace Pagr.UnitTests
{
    public class Mapper
    {
        private readonly ApplicationPagrProcessor _processor;
        private readonly IQueryable<Post> _posts;

        public Mapper()
        {
            _processor = new ApplicationPagrProcessor(new PagrOptionsAccessor(),
                new PagrCustomSortMethods(),
                new PagrCustomFilterMethods());

            _posts = new List<Post>
            {
                new Post
                {
                    Id = 1,
                    ThisHasNoAttributeButIsAccessible = "A",
                    ThisHasNoAttribute = "A",
                    OnlySortableViaFluentApi = 100
                },
                new Post
                {
                    Id = 2,
                    ThisHasNoAttributeButIsAccessible = "B",
                    ThisHasNoAttribute = "B",
                    OnlySortableViaFluentApi = 50
                },
                new Post
                {
                    Id = 3,
                    ThisHasNoAttributeButIsAccessible = "C",
                    ThisHasNoAttribute = "C",
                    OnlySortableViaFluentApi = 0
                },
            }.AsQueryable();
        }

        [Fact]
        public void MapperWorks()
        {
            var model = new PagrModel
            {
                Filters = "shortname@=A",
            };

            var result = _processor.Apply(model, _posts);

            Assert.Equal("A", result.First().ThisHasNoAttributeButIsAccessible);

            Assert.True(result.Count() == 1);
        }

        [Fact]
        public void MapperSortOnlyWorks()
        {
            var model = new PagrModel
            {
                Filters = "OnlySortableViaFluentApi@=50",
                Sorts = "OnlySortableViaFluentApi"
            };

            var result = _processor.Apply(model, _posts, applyFiltering: false, applyPagination: false);

            Assert.Throws<PagrMethodNotFoundException>(() => _processor.Apply(model, _posts));

            Assert.Equal(3, result.First().Id);

            Assert.True(result.Count() == 3);
        }
    }
}

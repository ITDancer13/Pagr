using System;
using System.Collections.Generic;
using System.Linq;
using Pagr.Models;
using Pagr.Services;
using Pagr.UnitTests.Entities;
using Pagr.UnitTests.Services;
using Xunit;

namespace Pagr.UnitTests
{
    public class StringFilterNullTests
    {
        private readonly IQueryable<Comment> _comments;
        private readonly PagrProcessor _processor;

        public StringFilterNullTests()
        {
            _processor = new PagrProcessor(new PagrOptionsAccessor());

            _comments = new List<Comment>
            {
                new Comment
                {
                    Id = 0,
                    DateCreated = DateTimeOffset.UtcNow,
                    Text = "This text contains null somewhere in the middle of a string",
                },
                new Comment {Id = 1, DateCreated = DateTimeOffset.UtcNow, Text = "null is here in the text",},
                new Comment {Id = 2, DateCreated = DateTimeOffset.UtcNow, Text = "Regular comment without n*ll.",},
                new Comment {Id = 100, DateCreated = DateTimeOffset.UtcNow, Text = null,},
            }.AsQueryable();
        }

        [Fact]
        public void Filter_Equals_Null()
        {
            var model = new PagrModel {Filters = "Text==null"};

            var result = _processor.Apply(model, _comments);

            Assert.Equal(100, result.Single().Id);
        }

        [Fact]
        public void Filter_NotEquals_Null()
        {
            var model = new PagrModel {Filters = "Text!=null"};

            var result = _processor.Apply(model, _comments);

            Assert.Equal(new[] {0, 1, 2}, result.Select(p => p.Id));
        }

        [Theory]
        [InlineData("Text@=null")]
        [InlineData("Text@=*null")]
        [InlineData("Text@=*NULL")]
        [InlineData("Text@=*NulL")]
        [InlineData("Text@=*null|text")]
        public void Filter_Contains_NullString(string filter)
        {
            var model = new PagrModel {Filters = filter};

            var result = _processor.Apply(model, _comments);

            Assert.Equal(new[] {0, 1}, result.Select(p => p.Id));
        }

        [Theory]
        [InlineData("Text_=null")]
        [InlineData("Text_=*null")]
        [InlineData("Text_=*NULL")]
        [InlineData("Text_=*NulL")]
        [InlineData("Text_=*null|text")]
        public void Filter_StartsWith_NullString(string filter)
        {
            var model = new PagrModel {Filters = filter};

            var result = _processor.Apply(model, _comments);

            Assert.Equal(new[] {1}, result.Select(p => p.Id));
        }

        [Theory]
        [InlineData("Text!@=null")]
        [InlineData("Text!@=*null")]
        [InlineData("Text!@=*NULL")]
        [InlineData("Text!@=*NulL")]
        [InlineData("Text!@=*null|text")]
        public void Filter_DoesNotContain_NullString(string filter)
        {
            var model = new PagrModel {Filters = filter};

            var result = _processor.Apply(model, _comments);

            Assert.Equal(new[] {2}, result.Select(p => p.Id));
        }

        [Theory]
        [InlineData("Text!_=null")]
        [InlineData("Text!_=*null")]
        [InlineData("Text!_=*NULL")]
        [InlineData("Text!_=*NulL")]
        public void Filter_DoesNotStartsWith_NullString(string filter)
        {
            var model = new PagrModel {Filters = filter};

            var result = _processor.Apply(model, _comments);

            Assert.Equal(new[] {0, 2}, result.Select(p => p.Id));
        }
    }
}

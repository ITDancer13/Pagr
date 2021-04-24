using System;
using System.ComponentModel.DataAnnotations.Schema;
using Pagr.Attributes;

namespace Pagr.Sample.Entities
{
    public class Post
    {
        public int Id { get; set; }

        [Pagr(CanFilter = true, CanSort = true)]
        public string Title { get; set; } = Guid.NewGuid().ToString().Replace("-", string.Empty)[..8];

        [Pagr(CanFilter = true, CanSort = true)]
        public int LikeCount { get; set; } = new Random().Next(0, 1000);


        [Pagr(CanFilter = true, CanSort = true)]
        public int CommentCount { get; set; } = new Random().Next(0, 1000);

        [Pagr(CanFilter = true, CanSort = true)]
        public DateTimeOffset DateCreated { get; set; } = DateTimeOffset.UtcNow;

        [Pagr(CanFilter = true, CanSort = true)]
        [Column(TypeName = "datetime")]
        public DateTime DateLastViewed { get; set; } = DateTime.UtcNow;

        [Pagr(CanFilter = true, CanSort = true)]
        public int? CategoryId { get; set; } = new Random().Next(0, 4);
    }
}

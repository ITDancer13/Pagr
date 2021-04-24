using Pagr.Attributes;
using Pagr.UnitTests.Entities;

namespace Pagr.UnitTests.Abstractions.Entity
{
    public interface IPost: IBaseEntity
    {
        [Pagr(CanFilter = true, CanSort = true)]
        string Title { get; set; }
        [Pagr(CanFilter = true, CanSort = true)]
        int LikeCount { get; set; }
        [Pagr(CanFilter = true, CanSort = true)]
        int CommentCount { get; set; }
        [Pagr(CanFilter = true, CanSort = true)]
        int? CategoryId { get; set; }
        [Pagr(CanFilter = true, CanSort = true)]
        bool IsDraft { get; set; }
        string ThisHasNoAttribute { get; set; }
        string ThisHasNoAttributeButIsAccessible { get; set; }
        int OnlySortableViaFluentApi { get; set; }
        Comment TopComment { get; set; }
        Comment FeaturedComment { get; set; }
    }
}

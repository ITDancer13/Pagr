using System.Linq;
using Pagr.Services;
using Pagr.Tests.Entities;

namespace Pagr.Tests.Services
{
    public class PagrCustomSortMethods : IPagrCustomSortMethods
    {
        public IQueryable<Post> Popularity(IQueryable<Post> source, bool useThenBy) => useThenBy
            ? ((IOrderedQueryable<Post>)source).ThenBy(p => p.LikeCount)
            : source.OrderBy(p => p.LikeCount)
                .ThenBy(p => p.CommentCount)
                .ThenBy(p => p.DateCreated);
    }
}

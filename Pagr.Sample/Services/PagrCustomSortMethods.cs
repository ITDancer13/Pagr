using System.Linq;
using Pagr.Sample.Entities;
using Pagr.Services;

namespace Pagr.Sample.Services
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

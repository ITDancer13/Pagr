using System.Linq;
using Pagr.Services;
using Pagr.UnitTests.Abstractions.Entity;
using Pagr.UnitTests.Entities;

namespace Pagr.UnitTests.Services
{
    public class PagrCustomSortMethods : IPagrCustomSortMethods
    {
        public IQueryable<Post> Popularity(IQueryable<Post> source, bool useThenBy, bool desc)
        {
            var result = useThenBy ?
                ((IOrderedQueryable<Post>)source).ThenBy(p => p.LikeCount) :
                source.OrderBy(p => p.LikeCount)
                .ThenBy(p => p.CommentCount)
                .ThenBy(p => p.DateCreated);

            return result;
        }

        public IQueryable<IPost> Popularity(IQueryable<IPost> source, bool useThenBy, bool desc)
        {
            var result = useThenBy ?
                ((IOrderedQueryable<IPost>)source).ThenBy(p => p.LikeCount) :
                source.OrderBy(p => p.LikeCount)
                    .ThenBy(p => p.CommentCount)
                    .ThenBy(p => p.DateCreated);

            return result;
        }

        public IQueryable<T> Oldest<T>(IQueryable<T> source, bool useThenBy, bool desc) where T : IBaseEntity
        {
            var result = useThenBy ?
                ((IOrderedQueryable<T>)source).ThenByDescending(p => p.DateCreated) :
                source.OrderByDescending(p => p.DateCreated);

            return result;
        }
    }
}

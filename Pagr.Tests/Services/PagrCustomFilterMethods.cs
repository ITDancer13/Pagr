using System.Linq;
using Pagr.Services;
using Pagr.Tests.Entities;

namespace Pagr.Tests.Services
{
    public class PagrCustomFilterMethods : IPagrCustomFilterMethods
    {
        public IQueryable<Post> IsNew(IQueryable<Post> source, string op, string[] values)
            => source.Where(p => p.LikeCount < 100 && p.CommentCount < 5);
    }
}

using System.Linq;
using Pagr.Sample.Entities;
using Pagr.Services;

namespace Pagr.Sample.Services
{
    public class PagrCustomFilterMethods : IPagrCustomFilterMethods
    {
        public IQueryable<Post> IsNew(IQueryable<Post> source, string op, string[] values)
            => source.Where(p => p.LikeCount < 100 && p.CommentCount < 5);
    }
}

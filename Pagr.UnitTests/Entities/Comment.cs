using Pagr.Attributes;
using Pagr.UnitTests.Abstractions.Entity;

namespace Pagr.UnitTests.Entities
{
    public class Comment : BaseEntity, IComment
    {
        [Pagr(CanFilter = true)]
        public string Text { get; set; }
        
        [Pagr(CanFilter = true)]
        public string Author { get; set; }
    }
}

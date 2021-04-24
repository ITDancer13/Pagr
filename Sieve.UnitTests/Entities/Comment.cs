using Sieve.Attributes;
using Sieve.UnitTests.Abstractions.Entity;

namespace Sieve.UnitTests.Entities
{
    public class Comment : BaseEntity, IComment
    {
        [Sieve(CanFilter = true)]
        public string Text { get; set; }
    }
}

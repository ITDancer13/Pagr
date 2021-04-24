using System;
using Sieve.Attributes;
using Sieve.UnitTests.Abstractions.Entity;

namespace Sieve.UnitTests.Entities
{
    public class BaseEntity : IBaseEntity
    {
        public int Id { get; set; }

        [Sieve(CanFilter = true, CanSort = true)]
        public DateTimeOffset DateCreated { get; set; } = DateTimeOffset.UtcNow;
    }
}

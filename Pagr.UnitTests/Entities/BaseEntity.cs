using System;
using Pagr.Attributes;
using Pagr.UnitTests.Abstractions.Entity;

namespace Pagr.UnitTests.Entities
{
    public class BaseEntity : IBaseEntity
    {
        public int Id { get; set; }

        [Pagr(CanFilter = true, CanSort = true)]
        public DateTimeOffset DateCreated { get; set; } = DateTimeOffset.UtcNow;
    }
}

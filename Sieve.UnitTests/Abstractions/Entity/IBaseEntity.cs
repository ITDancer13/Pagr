using System;

namespace Sieve.UnitTests.Abstractions.Entity
{
    public interface IBaseEntity
    {
        int Id { get; set; }
        DateTimeOffset DateCreated { get; set; }
    }
}

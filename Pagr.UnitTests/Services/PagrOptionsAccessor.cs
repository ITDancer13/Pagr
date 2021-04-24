using Microsoft.Extensions.Options;
using Pagr.Models;

namespace Pagr.UnitTests.Services
{
    public class PagrOptionsAccessor : IOptions<PagrOptions>
    {
        public PagrOptions Value { get; }

        public PagrOptionsAccessor()
        {
            Value = new PagrOptions
            {
                ThrowExceptions = true
            };
        }
    }
}

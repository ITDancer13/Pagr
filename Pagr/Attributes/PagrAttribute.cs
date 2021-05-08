using System;
using Pagr.Models;

namespace Pagr.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class PagrAttribute : Attribute, IPagrPropertyMetadata
    {
        public string Name { get; set; }

        public string FullName => Name;

        public bool CanSort { get; set; }

        public bool CanFilter { get; set; }
    }
}

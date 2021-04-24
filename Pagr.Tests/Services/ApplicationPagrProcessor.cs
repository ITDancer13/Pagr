﻿using Microsoft.Extensions.Options;
using Pagr.Models;
using Pagr.Services;
using Pagr.Tests.Entities;

namespace Pagr.Tests.Services
{
    public class ApplicationPagrProcessor : PagrProcessor
    {
        public ApplicationPagrProcessor(IOptions<PagrOptions> options, IPagrCustomSortMethods customSortMethods, IPagrCustomFilterMethods customFilterMethods) : base(options, customSortMethods, customFilterMethods)
        {
        }

        protected override PagrPropertyMapper MapProperties(PagrPropertyMapper mapper)
        {
            mapper.Property<Post>(p => p.Title)
                .CanSort()
                .CanFilter()
                .HasName("CustomTitleName");

            return mapper;
        }
    }
}

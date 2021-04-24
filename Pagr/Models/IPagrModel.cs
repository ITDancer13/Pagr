using System.Collections.Generic;

namespace Pagr.Models
{
    public interface IPagrModel : IPagrModel<IFilterTerm, ISortTerm>
    {

    }

    public interface IPagrModel<TFilterTerm, TSortTerm>
        where TFilterTerm : IFilterTerm
        where TSortTerm : ISortTerm
    {
        string Filters { get; set; }

        string Sorts { get; set; }

        int? Page { get; set; }

        int? PageSize { get; set; }

        List<TFilterTerm> GetFiltersParsed();

        List<TSortTerm> GetSortsParsed();
    }
}

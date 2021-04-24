using System.Linq;
using Pagr.Models;

namespace Pagr.Services
{
    public interface IPagrProcessor : IPagrProcessor<PagrModel, FilterTerm, SortTerm> { }

    public interface IPagrProcessor<TFilterTerm, TSortTerm> : IPagrProcessor<PagrModel<TFilterTerm, TSortTerm>, TFilterTerm, TSortTerm>
        where TFilterTerm : IFilterTerm, new()
        where TSortTerm : ISortTerm, new()
    { }

    public interface IPagrProcessor<in TPagrModel, TFilterTerm, TSortTerm>
        where TPagrModel : class, IPagrModel<TFilterTerm, TSortTerm>
        where TFilterTerm : IFilterTerm, new()
        where TSortTerm : ISortTerm, new()
    {
        IQueryable<TEntity> Apply<TEntity>(TPagrModel model, IQueryable<TEntity> source, object[] dataForCustomMethods = null,
            bool applyFiltering = true, bool applySorting = true, bool applyPagination = true);
    }
}

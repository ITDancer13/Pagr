namespace Pagr.UnitTests.Abstractions.Entity
{
    public interface IComment: IBaseEntity
    {
        string Text { get; set; }
    }
}

namespace Pagr.Models
{
    public interface IPagrPropertyMetadata
    {
        string Name { get; set; }

        string FullName { get; }

        bool CanFilter { get; set; }

        bool CanSort { get; set; }
    }
}

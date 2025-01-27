namespace GwentCardDownloader.Models
{
    public interface IValidatable
    {
        bool IsValid();
        IEnumerable<string> GetValidationErrors();
    }
}

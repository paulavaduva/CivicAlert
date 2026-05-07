namespace CivicAlert.Services.Interfaces
{
    public interface IFileService
    {
        Task<string> UploadImageAsync(IFormFile file);
    }
}

using CivicAlert.Services.Interfaces;

namespace CivicAlert.Services
{
    public class FileService : IFileService
    {
        private readonly Supabase.Client _supabaseClient;

        public FileService(Supabase.Client supabaseClient)
        {
            _supabaseClient = supabaseClient;
        }

        public async Task<string> UploadImageAsync(IFormFile file)
        {
            var fileName = $"{Guid.NewGuid()}_{file.FileName}";

            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);
            var fileBytes = stream.ToArray();

            await _supabaseClient.Storage.From("uploads").Upload(fileBytes, fileName);

            return _supabaseClient.Storage.From("uploads").GetPublicUrl(fileName);
        }
    }
}

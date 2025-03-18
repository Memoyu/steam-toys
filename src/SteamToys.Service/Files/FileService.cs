namespace SteamToys.Service.Files;

public class FileService : IFileService
{
    private readonly ILogger _logger;

    public FileService(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<FileService>();
    }

    public async Task<string> FileReadAsync(string path)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(path)) throw new FriendlyException("文件路径不能为空");
            if (!File.Exists(path)) throw new FriendlyException("文件不存在");

            using (var sr = new StreamReader(path))
            {
                return await sr.ReadToEndAsync();
            }
        }
        catch (FileNotFoundException ex)
        {
            _logger.LogError(ex, $"读取文件失败 path:{path}");
            throw new FriendlyException ($"读取文件失败 path:{path}", ex);
        }
    }
}

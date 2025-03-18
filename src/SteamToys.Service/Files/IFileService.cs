namespace SteamToys.Service.Files;

public interface IFileService
{
    /// <summary>
    /// 读取文件内容
    /// </summary>
    /// <param name="path">文件路径</param>
    /// <returns></returns>
    Task<string> FileReadAsync(string path);
}

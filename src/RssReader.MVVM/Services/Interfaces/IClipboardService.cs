using System.Threading.Tasks;

namespace RssReader.MVVM.Services.Interfaces;

public interface IClipboardService
{
    Task<string?> GetTextAsync();
    Task SetTextAsync(string text);
}

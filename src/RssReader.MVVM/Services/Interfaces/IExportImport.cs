namespace RssReader.MVVM.Services.Interfaces;

public interface IExportImport
{
    void Export(string filePath);
    void Import(string filePath);
}

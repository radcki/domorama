using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Domorama.PictureFrame.Core.Library.DataStorage;

public class JsonDataStorageConfiguration<T>()
{
    public string Path { get; init; }
}

public class JsonDataStorage<T>(ILogger<JsonDataStorage<T>> logger, JsonDataStorageConfiguration<T> configuration) : IDataStorage<T> where T : new()
{
    private readonly string _path = configuration.Path;
    private ILogger<JsonDataStorage<T>> _logger = logger;


    /// <inheritdoc />
    public async Task Save(T data)
    {
        var tempFileName = Path.GetTempFileName();
        try
        {
            if (File.Exists(_path))
            {
            }

            await using (FileStream createStream = File.Create(tempFileName))
            {
                await JsonSerializer.SerializeAsync(createStream, data);
            }

            File.Move(tempFileName, _path, true);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to save data");
        }
        finally
        {
            if (File.Exists(tempFileName))
            {
                File.Delete(tempFileName);
            }
        }
    }

    /// <inheritdoc />
    public async Task<T> Load()
    {
        T? data = default;
        try
        {
            await using FileStream readStream = File.OpenRead(_path);
            data = await JsonSerializer.DeserializeAsync<T>(readStream);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to load data");
        }

        return data ?? new T();
    }
}
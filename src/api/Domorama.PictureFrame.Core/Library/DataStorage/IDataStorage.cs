namespace Domorama.PictureFrame.Core.Library.DataStorage;

public interface IDataStorage<T>
{
    Task Save(T data);
    Task<T> Load();
}
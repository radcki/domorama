using Domorama.PictureFrame.Core.Library.DataStorage;
using Domorama.PictureFrame.Core.Library.Files;
using Microsoft.Extensions.Logging;

namespace Domorama.PictureFrame.Core.Library.PictureFrameEntries;

public class PictureFrameEntryLibrary(FileLibrary fileLibrary, IDataStorage<PictureFrameEntryLibraryData> storage, ILogger<PictureFrameEntryLibrary> logger){}
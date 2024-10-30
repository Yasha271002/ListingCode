using Newtonsoft.Json;
using Xceed.Words.NET;

namespace ListingCode;

public class Listing
{
    private DocX? _document;
    private string? _code;

    private Files _businessLogic = new();
    private Files _helpers = new();
    private Files _models = new();
    private Files _userInterface = new();

    private Settings _settings = new();
    private readonly JsonHelper _jsonHelper = new();

    private List<string?> _directories = [];
    private List<string?> _allText = [];

    static void Main(string[] args)
    {
        var code = new Listing();
        code.Initialize();
    }

    private void Initialize()
    {
        GetSettings();
        GetFilesName();

        _document = !File.Exists(_settings.ListingFileName) ? DocX.Create(_settings.ListingFileName) : DocX.Load(_settings.ListingFileName);

        StartListing();
    }

    private void GetSettings()
    {
        var path = "Settings.json";
        if (!File.Exists(path))
        {
            var settings = new Settings
            {
                ListingFileName = "",
                PathToProject = ""
            };
            _jsonHelper.WriteJsonToFile(path, settings, false);
        }

        _settings = _jsonHelper.ReadJsonFromFile(path, _settings);

    }

    private void StartListing()
    {
        SetHyperlinkData(_businessLogic, "Business", "1.АЛГОРИТМ, БИЗНЕС-ЛОГИКА, ОСНОВНЫЕ КЛАССЫ (ФОРМЫ ОКНА)", 0, 1);
        SetHyperlinkData(_userInterface, "Business", "", 21, 1);                                                       //Только если есть control с code behind
        SetHyperlinkData(_helpers, "Business", "2.ВСПОМОГАТЕЛЬНЫЕ ФУНКЦИОНАЛЬНЫЕ КЛАССЫ", 0, 2);
        SetHyperlinkData(_models, "Business", "3.МОДЕЛИ ДАННЫХ (КЛАССЫ МОДЕЛИ ДАННЫХ)", 0, 3);
        SetHyperlinkData(_userInterface, "UI", "4.ПОЛЬЗОВАТЕЛЬСКИИ ИНТЕРФЕИС", 0, 4);

        SetListingFileData(_businessLogic, "Business", "1.АЛГОРИТМ, БИЗНЕС-ЛОГИКА, ОСНОВНЫЕ КЛАССЫ (ФОРМЫ ОКНА)", 0, 1);
        SetListingFileData(_userInterface, "Business", "", 21, 1);                                                       //Только если есть control с code behind
        SetListingFileData(_helpers, "Business", "2.ВСПОМОГАТЕЛЬНЫЕ ФУНКЦИОНАЛЬНЫЕ КЛАССЫ", 0, 2);
        SetListingFileData(_models, "Business", "3.МОДЕЛИ ДАННЫХ (КЛАССЫ МОДЕЛИ ДАННЫХ)", 0, 3);
        SetListingFileData(_userInterface, "UI", "4.ПОЛЬЗОВАТЕЛЬСКИИ ИНТЕРФЕИС", 0, 4);

        WriteFile(_allText);
    }

    private void SetHyperlinkData(Files fileModel, string type, string text, int number, int count)
    {
        switch (type)
        {
            case "UI":
                _allText.Add(text);
                _allText.Add(" ");
                foreach (var uiText in fileModel.UIFilesName!
                             .Select(ui => Path.GetFileName(ui)))
                {
                    number += 1;
                    _allText.Add(count + "." + number + " " + uiText);
                    _allText.Add(" ");
                }

                break;
            case "Business":
                _allText.Add(text);
                _allText.Add(" ");
                foreach (var businessText in fileModel.BusinessFilesName!
                             .Select(business => Path.GetFileName(business)))
                {
                    number += 1;
                    _allText.Add(count + "." + number + " " + businessText);
                    _allText.Add(" ");
                }

                break;
        }
    }

    private void SetListingFileData(Files fileModel, string type, string text, int number, int count)
    {
        switch (type)
        {
            case "UI":
                _allText.Add(text);
                _allText.Add(" ");
                foreach (var ui in fileModel.UIFilesName!)
                {
                    var fileName = Path.GetFileName(ui);
                    var path = Path.GetFullPath(ui);
                    var uiText = ReadFile(path);

                    _allText.Add(count + "." + number + " " + fileName);
                    number += 1;
                    _allText.Add(" ");
                    _allText.Add(uiText);
                    _allText.Add(" ");
                }

                break;
            case "Business":
                _allText.Add(text);
                _allText.Add(" ");
                foreach (var ui in fileModel.BusinessFilesName!)
                {
                    var fileName = Path.GetFileName(ui);
                    var path = Path.GetFullPath(ui);
                    var uiText = ReadFile(path);

                    _allText.Add(count + "." +  number + " " + fileName);
                    number += 1;
                    _allText.Add(" ");
                    _allText.Add(uiText);
                    _allText.Add(" ");
                }

                break;
        }
    }

    private string ReadFile(string path)
    {
        return File.ReadAllText(path);
    }

    private void WriteFile(List<string> texts)
    {
        foreach (var text in texts)
        {
            _document!.InsertParagraph(text);
        }

        _document.Save();
    }

    private void GetFilesName()
    {
        var directories = Directory.GetDirectories(_settings.PathToProject!, "*")
            .Where(directory => !directory.ToLower().EndsWith("bin", StringComparison.OrdinalIgnoreCase) &&
                                !directory.ToLower().EndsWith("obj", StringComparison.OrdinalIgnoreCase) &&
                                !directory.ToLower().EndsWith("Resources", StringComparison.OrdinalIgnoreCase) &&
                                !directory.ToLower().EndsWith("Libs", StringComparison.OrdinalIgnoreCase) &&
                                !directory.ToLower().EndsWith("Properties", StringComparison.OrdinalIgnoreCase))
            .ToList();

        foreach (var path in directories.Select(d => Path.GetFullPath(d)))
        {
            if (path.ToLower().EndsWith("Behaviors", StringComparison.OrdinalIgnoreCase))
            {
                _helpers = DirectorySearch(path, _helpers = new Files());
            }
            else if (path.ToLower().EndsWith("Converters", StringComparison.OrdinalIgnoreCase))
            {
                DirectorySearch(path, _helpers);
            }
            else if (path.ToLower().EndsWith("Helpers", StringComparison.OrdinalIgnoreCase))
            {
                DirectorySearch(path, _helpers);
            }
            else if (path.ToLower().EndsWith("Utilities", StringComparison.OrdinalIgnoreCase))
            {
                DirectorySearch(path, _helpers);
            }
            else if (path.ToLower().EndsWith("Views", StringComparison.OrdinalIgnoreCase))
            {
                _userInterface = DirectorySearch(path, _userInterface = new Files());
            }
            else if (path.ToLower().EndsWith("ViewModels", StringComparison.OrdinalIgnoreCase))
            {
                _businessLogic = DirectorySearch(path, _businessLogic = new Files());
            }
            else if (path.ToLower().EndsWith("Models", StringComparison.OrdinalIgnoreCase))
            {
                _models = DirectorySearch(path, _models = new Files());
            }
        }
    }

    private Files DirectorySearch(string path, Files FilesModel)
    {
        var subDirectories = Directory.GetDirectories(path).ToList();
        FilesModel = FilesSearch(path, FilesModel);

        if (subDirectories.Count == 0)
            return FilesModel;

        foreach (var subDirectory in subDirectories)
        {
            var newPath = Path.GetFullPath(subDirectory);
            FilesModel = FilesSearch(newPath, FilesModel);
            _directories.Add(subDirectory);
            DirectorySearch(newPath, FilesModel);
        }


        return FilesModel!;
    }

    private Files FilesSearch(string path, Files filesModel)
    {
        var UIFiles = Directory
            .GetFiles(path, "*.*")
            .Where(file => file.ToLower().EndsWith("xaml"))
            .ToList();
        var BusinessLogic = Directory
            .GetFiles(path, "*.*")
            .Where(file => file.ToLower().EndsWith("xaml.cs") || file.ToLower().EndsWith(".cs"))
            .ToList();

        foreach (var businessFileName in BusinessLogic.Select(bf => Path.GetFullPath(bf)))
        {
            if (filesModel.BusinessFilesName!.Contains(Path.GetFullPath(businessFileName)))
                return filesModel;

            filesModel.BusinessFilesName!.Add(Path.GetFullPath(businessFileName));
        }

        foreach (var uiFileName in UIFiles.Select(bf => Path.GetFullPath(bf)))
        {
            if (filesModel.UIFilesName!.Contains(Path.GetFullPath(uiFileName)))
                return filesModel;

            filesModel.UIFilesName!.Add(Path.GetFullPath(uiFileName));
        }

        return filesModel;
    }
}

public class Files
{
    public List<string>? UIFilesName { get; set; } = [];
    public List<string>? BusinessFilesName { get; set; } = [];
}

public class Settings
{
    [JsonProperty("ListingFileName")] public string? ListingFileName { get; set; }
    [JsonProperty("PathToProject")] public string? PathToProject { get; set; } 
}
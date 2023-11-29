using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace OnlineMuhasebeCodeGenerator;

public partial class MyToolWindowControl : UserControl
{
    private string Path = string.Empty;
    private string SelectedEntities = string.Empty;
    private string SelectedDb = string.Empty;
    private string SelectedRepository = string.Empty;
    private string SelectedService = string.Empty;
    private string RepositoryDISpot = nameof(RepositoryDISpot);
    private string ServiceDISpot = nameof(ServiceDISpot);
    private string UsingSpot = nameof(UsingSpot);

    public MyToolWindowControl()
    {
        InitializeComponent();

        dataBaseCB.Items.Add(DatabaseNames.AppDbContext);
        dataBaseCB.Items.Add(DatabaseNames.CompanyDbContext);
        dataBaseCB.Text = DatabaseNames.AppDbContext;

        ClearClassCheck();

        GetEntities();
    }

    public void ClearClassCheck()
    {
        checkFileExist(entitiesCB.Text);
        createRepositoriesCheckBox.Visibility = Visibility.Hidden;
        createServiceCheckBox.Visibility = Visibility.Hidden;
        createControllerCheckBox.Visibility = Visibility.Hidden;
        createFiles.Visibility = Visibility.Hidden;
    }

    private void dataBaseCB_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        SelectedDb = e.AddedItems[0].ToString();
        if (SelectedDb == DatabaseNames.AppDbContext)
        {
            Path = Paths.AppEntities;
            SelectedEntities = Paths.AppEntitiesUsingPath;
            SelectedRepository = "App";
            SelectedService = "AppServices";
            RepositoryDISpot = "AppRepositoryDISpot";
            ServiceDISpot = "AppServiceDISpot";
        }
        else if (SelectedDb == DatabaseNames.CompanyDbContext)
        {
            Path = Paths.CompanyEntities;
            SelectedEntities = Paths.CompanyEntitiesUsingPath;
            SelectedRepository = "CompanyDb";
            SelectedService = "CompanyServices";
            RepositoryDISpot = "CompanyRepositoryDISpot";
            ServiceDISpot = "CompanyServiceDISpot";
        }

        GetEntities();
    }

    public void GetEntities()
    {
        if (!string.IsNullOrEmpty(Path))
        {
            entitiesCB.Items.Clear();
            var findFiles = System.IO.Directory.GetFiles(Path);
            foreach (var file in findFiles)
            {
                entitiesCB.Items.Add(file.Replace(Path, "").Replace(@"\", "").Replace(".cs", ""));
            }
        }
    }

    private void entitiesCB_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        try
        {
            string fileName = e.AddedItems[0].ToString();
            checkFileExist(fileName);
        }
        catch (Exception ex)
        {
            
        }
        
    }

    public void checkFileExist(string fileName)
    {
        if (!string.IsNullOrEmpty(fileName))
        {
            #region Repository Check Control
            string commandPath = $"{Paths.AppDbDomainRepositoryPath}/{fileName}{FolderNames.Repositories}/I{fileName}{Paths.CommandRepository}.cs";
            bool isCommandFileExists = System.IO.File.Exists(commandPath);

            string queryPath = $"{Paths.AppDbDomainRepositoryPath}/{fileName}{FolderNames.Repositories}/I{fileName}{Paths.QueryRepository}.cs";
            bool isQueryFileExists = System.IO.File.Exists(queryPath);
            if (isCommandFileExists && isQueryFileExists)
            {
                createRepositoriesCheckBox.IsChecked = false;
                createRepositoriesCheckBox.Visibility = Visibility.Hidden;
            }
            else
                createRepositoriesCheckBox.Visibility = Visibility.Visible;
            #endregion

            #region Service Check Control
            string servicePath = $"{Paths.AppServiceInterfacePath}/I{fileName}{FileNames.Service}.cs";
            bool isServiceFileExists = System.IO.File.Exists(servicePath);
            if (isServiceFileExists)
            {
                createServiceCheckBox.IsChecked = false;
                createServiceCheckBox.Visibility = Visibility.Hidden;
            }
            else
                createServiceCheckBox.Visibility = Visibility.Visible;
            #endregion

            #region Service Check Control
            fileName = Files.ChangeFileNameToMultipleName(fileName);
            string controllerPath = $"{Paths.ControllerPath}/{fileName}Controller.cs";
            bool isControllerExists = System.IO.File.Exists(controllerPath);
            if (isControllerExists)
            {
                createControllerCheckBox.IsChecked = false;
                createControllerCheckBox.Visibility = Visibility.Hidden;
            }
            else
                createControllerCheckBox.Visibility = Visibility.Visible;
            #endregion

            createFiles.Visibility = Visibility.Visible;

        }
    }

    private void createFiles_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(entitiesCB.Text))
            return;

        resultLB.Items.Clear();
        string fileName = entitiesCB.Text;
        if (!string.IsNullOrEmpty(fileName))
        {
            if ((bool)createRepositoriesCheckBox.IsChecked)
            {
                Result result1 = Files.CreateRepositoryFile(fileName, SelectedEntities, SelectedDb, SelectedRepository, RepositoryTypes.Command,"interface");
                Result result2 = Files.CreateRepositoryFile(fileName, SelectedEntities, SelectedDb, SelectedRepository, RepositoryTypes.Query, "interface");

                Result result3 = Files.CreateRepositoryFile(fileName, SelectedEntities, SelectedDb, SelectedRepository, RepositoryTypes.Command,"class");
                Result result4 = Files.CreateRepositoryFile(fileName, SelectedEntities, SelectedDb, SelectedRepository, RepositoryTypes.Query, "class");

                resultLB.Items.Add(result1.Message);
                resultLB.Items.Add(result2.Message);
                resultLB.Items.Add(result3.Message);
                resultLB.Items.Add(result4.Message);

                string[] dependencyInjectionFile = System.IO.File.ReadAllLines(Paths.PersistanceDIFilePath);
                List<string> newDependencyInjectionFile = new List<string>();
                string rowEmpty = "                ";
                string using1 = $"using {Paths.Domain}.{FolderNames.Repositories}.{SelectedDb}.{fileName}{FolderNames.Repositories};";
                string using2 = $"using {Paths.Persistance}.{FolderNames.Repositories}.{SelectedDb}.{fileName}{FolderNames.Repositories};";
                string scopedRow1 = $"services.AddScoped<I{fileName}CommandRepository, {fileName}CommandRepository>();";
                string scopedRow2 = $"services.AddScoped<I{fileName}QueryRepository, {fileName}QueryRepository>();";
                for (int i = 0; i < dependencyInjectionFile.Length; i++)
                {
                    string row = dependencyInjectionFile[i];
                    if (row.Contains(UsingSpot))
                    {
                        if (result1.IsSuccess)
                        {
                            newDependencyInjectionFile.Add(using1);
                            newDependencyInjectionFile.Add(using2);
                        }                            
                    }
                    else if(row.Contains(RepositoryDISpot))
                    {
                        newDependencyInjectionFile.Add($"{rowEmpty}{scopedRow1}");
                        newDependencyInjectionFile.Add($"{rowEmpty}{scopedRow2}");
                    }

                    newDependencyInjectionFile.Add(row);
                }
                System.IO.File.Delete(Paths.PersistanceDIFilePath);
                System.IO.File.AppendAllLines(Paths.PersistanceDIFilePath, newDependencyInjectionFile);
            }

            if ((bool)createServiceCheckBox.IsChecked)
            {
                Result result1 = Files.CreateServiceFile(fileName, "interface", SelectedDb, SelectedService);
                Result result2 = Files.CreateServiceFile(fileName, "class", SelectedDb, SelectedService);

                resultLB.Items.Add(result1.Message);
                resultLB.Items.Add(result2.Message);

                string[] dependencyInjectionFile = System.IO.File.ReadAllLines(Paths.PersistanceDIFilePath);
                List<string> newDependencyInjectionFile = new List<string>();

                string rowEmpty = "                ";                
                string scopedRow = $"services.AddScoped<I{fileName}Service, {fileName}Service>();";
                for (int i = 0; i < dependencyInjectionFile.Length; i++)
                {
                    string row = dependencyInjectionFile[i];
                    if (row.Contains(ServiceDISpot))
                    {
                        newDependencyInjectionFile.Add($"{rowEmpty}{scopedRow}");                        
                    }

                    newDependencyInjectionFile.Add(row);
                }
                System.IO.File.Delete(Paths.PersistanceDIFilePath);
                System.IO.File.AppendAllLines(Paths.PersistanceDIFilePath, newDependencyInjectionFile);
            }

            if ((bool)createControllerCheckBox.IsChecked)
            {
                Result result = Files.CreateController(fileName);
                resultLB.Items.Add(result.Message);
            }

            if ((bool)createFeatureCheckBox.IsChecked)
            {
                Result result = Files.CreateFeatureFolders(fileName, SelectedDb);
                resultLB.Items.Add(result.Message);
            }

            ClearClassCheck();
        }
    }    
}

public static class DatabaseNames
{
    public static readonly string AppDbContext = nameof(AppDbContext);
    public static readonly string CompanyDbContext = nameof(CompanyDbContext);
}

public static class Paths
{
    public static readonly string ProjectName =
        "OnlineMuhasebeServer";
    public static readonly string ProjectPath =
        "C:/Users/samet/source/repos/samettas/online-muhasebe-server/OnlineMuhasebeServer";

    #region RepositoryNames
    public static readonly string CommandRepository =
        nameof(CommandRepository);
    public static readonly string QueryRepository =
        nameof(QueryRepository);

    public static readonly string GenericRepositories =
        nameof(GenericRepositories);
    #endregion

    #region Layer Names
    public static readonly string Domain =
        $"{Paths.ProjectName}.{nameof(Domain)}";
    public static readonly string Application =
        $"{Paths.ProjectName}.{nameof(Application)}";
    public static readonly string Infrastructure =
        $"{Paths.ProjectName}.{nameof(Infrastructure)}";
    public static readonly string Persistance =
        $"{Paths.ProjectName}.{nameof(Persistance)}";
    public static readonly string Presentation =
        $"{Paths.ProjectName}.{nameof(Presentation)}";
    public static readonly string WebApi =
        $"{Paths.ProjectName}.{nameof(WebApi)}";
    #endregion

    #region Entities
    public static readonly string AppEntities =
        $"{Paths.ProjectPath}/{Paths.Domain}/{nameof(AppEntities)}";
    public static readonly string CompanyEntities =
        $"{Paths.ProjectPath}/{Paths.Domain}/{nameof(CompanyEntities)}";

    public static readonly string AppEntitiesUsingPath =
        $"{Paths.Domain}.{nameof(AppEntities)}";
    public static readonly string CompanyEntitiesUsingPath =
        $"{Paths.Domain}.{nameof(CompanyEntities)}";
    #endregion

    #region Repository Paths
    public static readonly string AppDbDomainRepositoryPath =
        $"{Paths.ProjectPath}/{Paths.Domain}/{FolderNames.Repositories}/{DatabaseTypes.AppDbContext}";
    public static readonly string AppDbPersistanceRepositoryPath =
        $"{Paths.ProjectPath}/{Paths.Persistance}/{FolderNames.Repositories}/{DatabaseTypes.AppDbContext}";

    public static readonly string CompanyDbDomainRepositoryPath =
        $"{Paths.ProjectPath}/{Paths.Domain}/{FolderNames.Repositories}/{DatabaseTypes.CompanyDbContext}";
    public static readonly string CompanyDbPersistanceRepositoryPath =
       $"{Paths.ProjectPath}/{Paths.Persistance}/{FolderNames.Repositories}/{DatabaseTypes.CompanyDbContext}";

    public static readonly string GenericInterfaceRepositoriesPath =
        $"{Paths.Domain}.{FolderNames.Repositories}.{Paths.GenericRepositories}";
    public static readonly string GenericClassRepositoriesPath =
        $"{Paths.Persistance}.{FolderNames.Repositories}.{Paths.GenericRepositories}";
    #endregion

    #region Service Folder Path
    public static readonly string AppServiceInterfacePath =
        $"{Paths.ProjectPath}/{Paths.Application}/{FolderNames.Services}/{FolderNames.AppServices}";
    public static readonly string AppServiceClassPath =
        $"{Paths.ProjectPath}/{Paths.Persistance}/{FolderNames.Services}/{FolderNames.AppServices}";

    public static readonly string CompanyServiceInterfacePath =
        $"{Paths.ProjectPath}/{Paths.Application}/{FolderNames.Services}/{FolderNames.CompanyServices}";
    public static readonly string CompanyServiceClassPath =
        $"{Paths.ProjectPath}/{Paths.Persistance}/{FolderNames.Services}/{FolderNames.CompanyServices}";
    #endregion

    #region ControllerPath
    public static readonly string ControllerPath =
        $"{Paths.ProjectPath}/{Paths.Presentation}/{FolderNames.Controller}";
    #endregion

    #region Dependency Injection Files Path
    public static readonly string PersistanceDIFilePath =
        $"{Paths.ProjectPath}/{Paths.WebApi}/{FolderNames.Configurations}/{FileNames.PersistanceDIServiceInstaller}.cs";
    #endregion

    #region Feature Folder Path
    public static readonly string AppFeatureFoldersPath =
        $"{Paths.ProjectPath}/{Paths.Application}/{FolderNames.Features}/{FolderNames.AppFeatures}";
    public static readonly string CompanyFeatureFoldersPath =
        $"{Paths.ProjectPath}/{Paths.Application}/{FolderNames.Features}/{FolderNames.CompanyFeatures}";
    #endregion
}

public static class Files
{   

    public static Result CreateRepositoryFile(string fileName, string selectedEntities, string selectedDb, string selectedRepository, RepositoryTypes repositoryType, string classType = "class")
    {
        string folder;
        string repositoryPath = classType == "class" 
            ? (selectedDb == DatabaseTypes.AppDbContext.ToString() ? Paths.AppDbPersistanceRepositoryPath : Paths.CompanyDbPersistanceRepositoryPath) 
            : (selectedDb == DatabaseTypes.AppDbContext.ToString() ? Paths.AppDbDomainRepositoryPath : Paths.CompanyDbDomainRepositoryPath);
        folder = $"{repositoryPath}/{fileName}{FolderNames.Repositories}";

        string createFileName;            
        if(repositoryType == RepositoryTypes.Command)
            createFileName = $"{(classType == "class" ? "" : "I")}{fileName}{Paths.CommandRepository}.cs";
        else
            createFileName = $"{(classType == "class" ? "" : "I")}{fileName}{Paths.QueryRepository}.cs";

        string path = $"{folder}/{createFileName}";
        
        try
        {
            if (!System.IO.Directory.Exists(folder))
                System.IO.Directory.CreateDirectory(folder);

            string row = "";
            string repositoryName = "";
            if (!System.IO.File.Exists(path))
            {
               string nameSpace = $"{(classType == "class" ? Paths.Persistance : Paths.Domain)}.{FolderNames.Repositories}.{selectedDb}.{fileName}{FolderNames.Repositories}";                    

                string usingContent = classType == "class"
                    ? $"using {Paths.GenericClassRepositoriesPath}.{selectedDb};"
                    : $"using {Paths.GenericInterfaceRepositoriesPath}.{selectedDb};";

                string publicContent = classType == "class"
                    ? $"public {classType} {fileName}{repositoryType}Repository : {selectedRepository}{repositoryType}Repository<{fileName}>, I{fileName}{repositoryType}Repository"
                    : $"public {classType} I{fileName}{repositoryType}Repository : I{selectedRepository}{repositoryType}Repository<{fileName}>";

                repositoryName =
                    classType == "class"
                    ? $"{fileName}{repositoryType}Repository"
                    : $"I{fileName}{repositoryType}Repository";


                string[] contents =
                {
                        $"using {selectedEntities};",
                        $"{usingContent}",
                        (classType == "class"
                            ? $"using {Paths.Domain}.{FolderNames.Repositories}.{selectedDb}.{fileName}Repositories;"
                            : ""),                                                        
                        "",
                        $"namespace {nameSpace};",
                        "",
                        $"{publicContent}",
                        "{",
                        ((classType == "class" && selectedDb != DatabaseTypes.CompanyDbContext.ToString())
                            ? $"    public {fileName}{repositoryType}Repository(Persistance.Context.{selectedDb} context) : base(context)" + @"{ }"
                            : ""),                            
                        "}",
                };

                System.IO.File.AppendAllLines(path, contents);

                row = $"services.AddScoped<I{fileName}{repositoryType}Repository, {fileName}{repositoryType}Repository>();";
            }
            return new Result(
                true, 
                $"{repositoryName} başarıyla oluşturuldu!");
        }
        catch (Exception ex)
        {
            return new Result(false, $"{ex.Message}");
        }
        
    }

    public static Result CreateServiceFile(string fileName, string classType, string selectedDb, string selectedService)
    {
        string folder =
            selectedDb == DatabaseTypes.AppDbContext.ToString()
            ? (classType == "class" ? $"{Paths.AppServiceClassPath}" : $"{Paths.AppServiceInterfacePath}")
            : (classType == "class" ? $"{Paths.CompanyServiceClassPath}" : $"{Paths.CompanyServiceInterfacePath}");
        string folderName =
            classType == "class"
            ? $"{Paths.Persistance}"
            : $"{Paths.Application}";
        string fileTypeName = 
            classType == "class" 
            ? $"{fileName}{FileNames.Service}" 
            : $"I{fileName}{FileNames.Service}";
        string createFileName = $"{fileTypeName}.cs";            
        

        string path = $"{folder}/{createFileName}";

        string nameSpace = $"{folderName}.{FolderNames.Services}.{selectedService}";
        string className =
            classType == "class"
            ? $"{classType} {fileTypeName} : I{fileTypeName}"
            : $"{classType} {fileTypeName}";
        try
        {
            if (!System.IO.Directory.Exists(folder))
                System.IO.Directory.CreateDirectory(folder);

            if (!System.IO.File.Exists(path))
            {
                string[] contents =
                {
                        classType == "class" 
                            ? $"using {
                                (classType != "class"
                                    ? $"{Paths.Persistance}"
                                    : $"{Paths.Application}")
                            }.{FolderNames.Services}.{selectedService};" 
                            : "",
                        $"namespace {nameSpace};",
                        "",
                        $"public {className}",
                        "{",
                        "}",
                    };

                System.IO.File.AppendAllLines(path, contents);
            }
            return new Result(true, $"{fileTypeName} başarıyla oluşturuldu!");
        }
        catch (Exception ex)
        {
            return new Result(false, $"{ex.Message}");
        }
    }

    public static string ChangeFileNameToMultipleName(string fileName)
    {
        char[] fileNameLetters = fileName.ToCharArray();
        string lastLetter = fileNameLetters[fileNameLetters.Length - 1].ToString();

        string newLastLetter = "";
        if (lastLetter == "s")
            newLastLetter = "es";
        else if (lastLetter == "y")
            newLastLetter = "ies";
        else
            newLastLetter = lastLetter + "s";

        string newFileName = $"{fileName.Substring(0, fileNameLetters.Length - 1)}{newLastLetter}";
        return newFileName;
    }

    public static Result CreateController(string fileName)
    {

        fileName = ChangeFileNameToMultipleName(fileName);
        string folder = $"{Paths.ControllerPath}";
        string path = $"{folder}/{fileName}Controller.cs";
        try
        {
            if (!System.IO.Directory.Exists(folder))
                System.IO.Directory.CreateDirectory(folder);

            if (!System.IO.File.Exists(path))
            {
                string[] contents =
                {
                        "using MediatR;",                        
                        "using OnlineMuhasebeServer.Presentation.Abstraction; ",
                        "",
                        $"namespace {Paths.Presentation}.Controller;",
                        "",
                        $"public class {fileName}Controller : ApiController",
                        "{",
                        $"    public {fileName}Controller(IMediator mediator) : base(mediator) " + "{}",
                        "}",
                    };

                System.IO.File.AppendAllLines(path, contents);
            }
            return new Result(true, $"{fileName}Controller başarıyla oluşturuldu!");
        }
        catch (Exception ex)
        {
            return new Result(false, $"{ex.Message}");
        }
    }

    public static Result CreateFeatureFolders(string fileName, string selectedDb)
    {
        string commandFolder = $"{(selectedDb == DatabaseTypes.AppDbContext.ToString() ? Paths.AppFeatureFoldersPath : Paths.CompanyFeatureFoldersPath)}/{fileName}{FolderNames.Features}/{FolderNames.Commands}";
        string queryFolder = $"{(selectedDb == DatabaseTypes.AppDbContext.ToString() ? Paths.AppFeatureFoldersPath : Paths.CompanyFeatureFoldersPath)}/{fileName}{FolderNames.Features}/{FolderNames.Queries}";

        try
        {
            if (!System.IO.Directory.Exists(commandFolder))
                System.IO.Directory.CreateDirectory(commandFolder);

            if (!System.IO.Directory.Exists(queryFolder))
                System.IO.Directory.CreateDirectory(queryFolder);
            return new Result(true, $"{fileName}{FolderNames.Features} başarıyla oluşturuldu!");
        }
        catch (Exception ex)
        {
            return new Result(false, $"{ex.Message}");
        }  
    }
}

public sealed record Result(
    bool IsSuccess,
    string Message);

public enum RepositoryTypes
{
    Command,
    Query           
}

public enum DatabaseTypes
{
    AppDbContext,
    CompanyDbContext
}

public enum FolderNames
{
    Repositories,
    Features,
    AppServices,
    CompanyServices,
    Services,
    Configurations,
    Controller,
    AppFeatures,
    Commands,
    Queries,
    CompanyFeatures
}

public enum FileNames
{
    Service,
    PersistanceDIServiceInstaller
}    
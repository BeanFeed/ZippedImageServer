using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using ZippedImageApi;
using ZippedImageApi.Models;
using ZippedImageApi.Services;

namespace ZIClient;

class Program
{
    private static string appConfigDir = Path.Join(Environment.GetEnvironmentVariable("HOME"), ".config", "ZIClient");
    private static string _tokenFilePath = Path.Join(appConfigDir, ".authToken");
    private static string _configFilePath = Path.Join(appConfigDir, "config.json");
    
    static async Task<int> Main(string[] args)
    {
        if(!Directory.Exists(appConfigDir)) Directory.CreateDirectory(appConfigDir);
        
        if(!File.Exists(_tokenFilePath)) File.Create(_tokenFilePath).Close();
        var token = await File.ReadAllTextAsync(_tokenFilePath);
        
        Connection.JwtBearerToken = token;
        
        await SetConnection();
        
        var rootCommand = new RootCommand("ZIClient Command Line Tool");

        #region SetConfig

        var setConfigSchemeOption = new Option<string?>(
            "--scheme",
            description: "Set the connection scheme (http or https)")
        {
            IsRequired = false,
        };
        
        var setConfigHostnameOption = new Option<string?>(
            "--hostname",
            description: "Set the connection hostname")
        {
            IsRequired = false,
        };
        
        var setConfigPortOption = new Option<int?>(
            "--port",
            description: "Set the connection port")
        {
            IsRequired = false,
        };
        
        var setConfigCommand = new Command("setconfig", "Set the connection configuration for the server");
        
        setConfigCommand.AddOption(setConfigSchemeOption);
        setConfigCommand.AddOption(setConfigHostnameOption);
        setConfigCommand.AddOption(setConfigPortOption);
        
        setConfigCommand.SetHandler(async (scheme, hostname, port) =>
        {
            if (!string.IsNullOrEmpty(scheme))
            {
                switch (scheme)
                {
                    case "http":
                        Connection.Protocol = Connection.Protocols.Http;
                        break;
                    case "https":
                        Connection.Protocol = Connection.Protocols.Https;
                        break;
                }
            }
            if (!string.IsNullOrEmpty(hostname))
            {
                Connection.Hostname = hostname;
            }
            if (port.HasValue)
            {
                Connection.Port = port.Value;
            }
            var config = new ConnectionConfig
            {
                Protocol = Connection.Protocol.ToString().ToLower(),
                Hostname = Connection.Hostname!.ToLower(),
                Port = Connection.Port
            };
            await File.WriteAllTextAsync(_configFilePath, System.Text.Json.JsonSerializer.Serialize(config));
            
            Console.WriteLine($"Connection configuration set to {Connection.Origin}");
        }, setConfigSchemeOption, setConfigHostnameOption, setConfigPortOption);
        
        rootCommand.AddCommand(setConfigCommand);

        #endregion
        
        #region Login Command

        // Add options and arguments
        var usernameOption = new Option<string>(
            "--username",
            description: "Provide Username") 
        { 
            IsRequired = true,
        };
        
        usernameOption.AddAlias("-u");

        var passwordOption = new Option<string>(
            "--password",
            description: "Provide Username")
        {
            IsRequired = true,
        };
        
        passwordOption.AddAlias("-p");
        // Add commands with options
        var loginCommand = new Command("login", "Login to server")
        {
            
        };
        loginCommand.AddOption(usernameOption);
        loginCommand.AddOption(passwordOption);
        
        loginCommand.SetHandler(async (username, password) => 
        {
            try
            {
                password = password.Replace("\\@", "@");
                var authService = new AuthService();
                var token = await authService.Login(username, password);
                
                var writeStream = File.OpenWrite(_tokenFilePath);
                writeStream.Seek(0, SeekOrigin.Begin);
                await using (var writer = new StreamWriter(writeStream))
                {
                    await writer.WriteAsync(token);
                }
                writeStream.Close();
                Console.WriteLine($"Logged in successfully");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }, usernameOption, passwordOption);
        
        rootCommand.AddCommand(loginCommand);

        #endregion
        
        #region Create Command

        var createCommand = new Command("create", "Create a new entry on the server");

        #region Api Key

        var apiDescriptionOption = new Option<string>("--description", "Description of the API key")
            { IsRequired = true };

        var apiCategoryOption = new Option<string>("--category", "Category for the API key")
            { IsRequired = true };

        var createApiKeyCommand = new Command("apikey", "Create a new API key");
        
        createApiKeyCommand.AddOption(apiDescriptionOption);
        createApiKeyCommand.AddOption(apiCategoryOption);
        
        createApiKeyCommand.SetHandler(async (category, description) =>
        {
            try
            {
                var apiKeyService = new ApiKeyService();
                var key = await apiKeyService.Create(new CreateKeyModel()
                {
                    Category = category,
                    Description = description
                });
                
                Console.WriteLine($"API Key created successfully: {key}\nKey will not be accessible again, please save it securely.");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }, apiCategoryOption, apiDescriptionOption);
        
        createCommand.AddCommand(createApiKeyCommand);
        
        #endregion

        #region Category

        var createCategoryNameOption = new Option<string>("--name", "Name of the category") { IsRequired = true };
        var createCategoryFolderOption = new Option<string>("--folder", "Folder of the category") { IsRequired = true };

        var createCategoryCommand = new Command("category", "Create a new category");
        
        createCategoryCommand.AddOption(createCategoryNameOption);
        createCategoryCommand.AddOption(createCategoryFolderOption);
        
        createCategoryCommand.SetHandler(async (name, folder) =>
        {
            try
            {
                var categoryService = new CategoryService();
                
                await categoryService.Create(new CreateCategoryModel()
                {
                    Name = name,
                    Folder = folder
                });
                
                Console.WriteLine($"Category '{name}' created successfully.");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }, createCategoryNameOption, createCategoryFolderOption);
        
        createCommand.AddCommand(createCategoryCommand);

        #endregion

        #region Image

        var createImageCategoryOption = new Option<string>("--category", "Category of the image") { IsRequired = true };
        var createImageFileOption = new Option<string>("--file", "File of the image") { IsRequired = true };
        
        var createImageCommand = new Command("image", "Create a new image");
        
        createImageCommand.AddOption(createImageCategoryOption);
        createImageCommand.AddOption(createImageFileOption);
        
        createImageCommand.SetHandler(async (category, file) =>
        {
            try
            {
                if (!File.Exists(Path.Join(Environment.CurrentDirectory, file))) throw new Exception($"Couldn't read file: {file}");
                FileStream fileStream = File.OpenRead(file);
                var imageService = new ImageService();
                Console.WriteLine($"Uploading image '{file}' to category '{category}'...");
                await imageService.UploadImage(category, fileStream);
                
                Console.WriteLine($"Image '{file}' created successfully in category '{category}'.");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }, createImageCategoryOption, createImageFileOption);
        
        createCommand.AddCommand(createImageCommand);

        #endregion
        
        rootCommand.AddCommand(createCommand);

        #endregion

        #region Delete Command

        var deleteCommand = new Command("delete", "Delete an entry on the server");
        
        #region Api Key

        var deleteApiKeyIdOption = new Option<int>("--id", "ID of the API key to delete")
            { IsRequired = true };
        
        var deleteApiKeyCommand = new Command("apikey", "Delete an API key");
        
        deleteApiKeyCommand.AddOption(deleteApiKeyIdOption);
        
        deleteApiKeyCommand.SetHandler(async (id) =>
        {
            try
            {
                var apiKeyService = new ApiKeyService();
                await apiKeyService.Delete(id);
                
                Console.WriteLine($"API Key with ID {id} deleted successfully.");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }, deleteApiKeyIdOption);
        
        deleteCommand.AddCommand(deleteApiKeyCommand);

        #endregion
        
        #region Category
        
        var deleteCategoryNameOption = new Option<string>("--name", "Name of the category to delete")
            { IsRequired = true };
        
        var deleteCategoryCommand = new Command("category", "Delete a category");
        
        deleteCategoryCommand.AddOption(deleteCategoryNameOption);
        
        deleteCategoryCommand.SetHandler(async (name) =>
        {
            try
            {
                var categoryService = new CategoryService();
                await categoryService.Delete(name);
                
                Console.WriteLine($"Category '{name}' deleted successfully.");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }, deleteCategoryNameOption);
        
        deleteCommand.AddCommand(deleteCategoryCommand);
        
        #endregion
        
        #region Image
        
        var deleteImageNameOption = new Option<string>("--name", "Name of the image to delete")
            { IsRequired = true };
        
        var deleteImageCategoryOption = new Option<string>("--category", "Category of the image to delete from")
            { IsRequired = true };
        
        var deleteImageCommand = new Command("image", "Delete an image");
        
        deleteImageCommand.AddOption(deleteImageNameOption);
        deleteImageCommand.AddOption(deleteImageCategoryOption);
        
        deleteImageCommand.SetHandler(async (name, category) =>
        {
            try
            {
                var imageService = new ImageService();
                await imageService.DeleteImage(name, category);
                
                Console.WriteLine($"Image '{name}' in category '{category}' deleted successfully.");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }, deleteImageNameOption, deleteImageCategoryOption);
        
        deleteCommand.AddCommand(deleteImageCommand);
        
        #endregion
        
        rootCommand.AddCommand(deleteCommand);

        #endregion
        
        #region Get Command
        
        var getCommand = new Command("get", "Get an entry from the server");
        
        #region Api Key
        
        var getApiKeyCommand = new Command("apikeys", "Get API keys");
        
        getApiKeyCommand.SetHandler(async () =>
        {
            try
            {
                var apiKeyService = new ApiKeyService();

                ApiKey[] keys = await apiKeyService.Get();
                
                //Print header
                Console.WriteLine($"{"Id".ToFixedString(10)} {"Category".ToFixedString(20)} {"Description".ToFixedString(40)}");
                
                bool isOdd = true;
                var defaultBackgroundColor = Console.BackgroundColor;
                foreach (var key in keys)
                {
                    if(isOdd) Console.BackgroundColor = ConsoleColor.DarkGray;
                    else Console.BackgroundColor = defaultBackgroundColor;
                    
                    Console.WriteLine($"{key.Id.ToString().ToFixedString(10)} {key.Category.Name.ToFixedString(20)} {(key.Description ?? "").ToFixedString(40)}");
                    
                    isOdd = !isOdd;
                    Console.BackgroundColor = defaultBackgroundColor;
                }
                Console.BackgroundColor = defaultBackgroundColor;
                Console.WriteLine("");
            }
            catch (Exception e) 
            {
                Console.WriteLine(e.Message);
            }
            
        });
        
        getCommand.AddCommand(getApiKeyCommand);
        
        #endregion
        
        #region Category
        
        var getCategoryCommand = new Command("categories", "Get categories");

        getCategoryCommand.SetHandler(async () =>
        {
            try
            {
                var categoryService = new CategoryService();
                Category[] categories = await categoryService.Get();

                //Print header
                Console.WriteLine($"{"Name".ToFixedString(20)} {"Folder".ToFixedString(40)}");

                bool isOdd = true;
                var defaultBackgroundColor = Console.BackgroundColor;

                foreach (var category in categories)
                {
                    if (isOdd) Console.BackgroundColor = ConsoleColor.DarkGray;
                    else Console.BackgroundColor = defaultBackgroundColor;

                    Console.WriteLine($"{category.Name.ToFixedString(20)} {category.Folder.ToFixedString(40)}");

                    isOdd = !isOdd;
                    Console.BackgroundColor = defaultBackgroundColor;
                }

                Console.BackgroundColor = defaultBackgroundColor;
                Console.WriteLine("");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        });
        
        getCommand.AddCommand(getCategoryCommand);
        
        #endregion
        
        #region Image
        
        var getImageCategoryOption = new Option<string>(
            "--category",
            description: "Category of the images to get")
        {
            IsRequired = false
        };
        
        var getImageNameOption = new Option<string>(
            "--name",
            description: "Name of the image to get")
        {
            IsRequired = false
        };

        var getImageCommand = new Command("image", "Get images");
        
        getImageCommand.AddOption(getImageCategoryOption);
        getImageCommand.AddOption(getImageNameOption);
        
        getImageCommand.SetHandler(async (name, category) =>
        {
            try
            {
                var imageService = new ImageService();
                Image image = await imageService.GetImage(name, category);

                //Print header
                Console.WriteLine($"{"Name".ToFixedString(20)} {"Category".ToFixedString(20)}");

                var defaultBackgroundColor = Console.BackgroundColor;

                Console.BackgroundColor = ConsoleColor.DarkGray;
                
                Console.WriteLine($"{image.Name.ToFixedString(20)} {image.Category.Name.ToFixedString(20)}");

                Console.BackgroundColor = defaultBackgroundColor;
                Console.WriteLine("");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }, getImageNameOption, getImageCategoryOption);
        
        getCommand.AddCommand(getImageCommand);
        
        #endregion
        
        #region Images
        
        var getImagesCategoryOption = new Option<string?>(
            "--category",
            description: "Category of the images to get")
        {
            IsRequired = false
        };
        
        var getImagesCommand = new Command("images", "Get all images in a category");
        
        getImagesCommand.AddOption(getImagesCategoryOption);
        
        getImagesCommand.SetHandler(async (category) =>
        {
            try
            {
                var imageService = new ImageService();
                Image[] images = await imageService.GetImages(category);

                //Print header
                Console.WriteLine($"{"Name".ToFixedString(20)} {"Category".ToFixedString(20)}");

                bool isOdd = true;
                var defaultBackgroundColor = Console.BackgroundColor;

                foreach (var image in images)
                {
                    if (isOdd) Console.BackgroundColor = ConsoleColor.DarkGray;
                    else Console.BackgroundColor = defaultBackgroundColor;

                    Console.WriteLine($"{image.Name.ToFixedString(20)} {image.Category.Name.ToFixedString(20)}");

                    isOdd = !isOdd;
                    Console.BackgroundColor = defaultBackgroundColor;
                }

                Console.BackgroundColor = defaultBackgroundColor;
                Console.WriteLine("");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }, getImagesCategoryOption);
        
        getCommand.AddCommand(getImagesCommand);
        
        #endregion
        
        rootCommand.AddCommand(getCommand);
        
        #endregion
        
        #region Download Command
        
        var downloadCategoryOption = new Option<string>(
            "--category",
            description: "Category of the image to download")
        {
            IsRequired = true
        };
        
        var downloadNameOption = new Option<string>(
            "--name",
            description: "Name of the image to download")
        {
            IsRequired = true
        };
        
        var downloadCommand = new Command("download", "Download an image from the server");
        
        downloadCommand.AddOption(downloadCategoryOption);
        downloadCommand.AddOption(downloadNameOption);
        
        downloadCommand.SetHandler(async (category, name) =>
        {
            try
            {
                var imageService = new ImageService();
                Console.WriteLine($"Downloading image '{name}' from category '{category}'...");
                await imageService.DownloadImage(category, name, Path.Join(Environment.CurrentDirectory, name));
                
                Console.WriteLine($"Image '{name}' downloaded successfully to {Path.Join(Environment.CurrentDirectory, name)}");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }, downloadCategoryOption, downloadNameOption);
        
        #endregion
        // Build the parser with completion
        var parser = new CommandLineBuilder(rootCommand)
            .UseDefaults()
            .Build();
        
        // Parse the arguments and invoke the handler
        return await parser.InvokeAsync(args);
    }

    static async Task SetConnection()
    {
        if (!File.Exists(_configFilePath))
        {
            Console.WriteLine("Config file not found, creating a new one...");
            var newConfig = new ConnectionConfig();
            await File.WriteAllTextAsync(_configFilePath, System.Text.Json.JsonSerializer.Serialize(newConfig));
        }

        var configContent = await File.ReadAllTextAsync(_configFilePath);
        var config = System.Text.Json.JsonSerializer.Deserialize<ConnectionConfig>(configContent);

        if (config == null)
        {
            Console.WriteLine("Failed to read config file.");
            return;
        }

        
        Connection.Hostname = config.Hostname;
        Connection.Port = config.Port;

        switch (config.Protocol)
        {
            case "http":
                Connection.Protocol = Connection.Protocols.Http;
                break;
            case "https":
                Connection.Protocol = Connection.Protocols.Https;
                break;
        }

        Console.WriteLine($"Connecting to {Connection.Origin}\n");
    }
}

class ConnectionConfig
{
    public string Protocol { get; set; } = "http";
    public string Hostname { get; set; } = "localhost";
    public int Port { get; set; } = 5000;
}

public static class StringExtensions
{
    /// <summary>
    /// Extends the <code>String</code> class with this <code>ToFixedString</code> method.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="length">The prefered fixed string size</param>
    /// <param name="appendChar">The <code>char</code> to append</param>
    /// <returns></returns>
    public static String ToFixedString(this String value, int length, char appendChar = ' ')
    {
        int currlen = value.Length;
        int needed = length == currlen ? 0 : (length - currlen);

        return needed == 0 ? value :
            (needed > 0 ? value + new string(' ', needed) :
                new string(new string(value.ToCharArray().Reverse().ToArray()).
                    Substring(needed * -1, value.Length - (needed * -1)).ToCharArray().Reverse().ToArray()));
    }
}
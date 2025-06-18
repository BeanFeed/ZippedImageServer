// See https://aka.ms/new-console-template for more information

using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;

namespace ZIClient;

class Program
{
    static async Task<int> Main(string[] args)
    {
        var rootCommand = new RootCommand("ZIClient Command Line Tool");
        
        // Add options and arguments
        var categoryOption = new Option<string>(
            "--category",
            description: "Specify a category") 
        { 
            IsRequired = false 
        };
        
        // Add custom completions for the category option
        categoryOption.AddCompletions(ctx => new[] { "images", "documents", "videos" });
        
        // Add commands with options
        var listCommand = new Command("list", "List items");
        listCommand.AddOption(categoryOption);
        
        listCommand.SetHandler((category) => 
        {
            Console.WriteLine($"Listing items in category: {category}");
        }, categoryOption);
        
        rootCommand.AddCommand(listCommand);
        
        // Build the parser with completion
        var parser = new CommandLineBuilder(rootCommand)
            .UseDefaults()
            .Build();
        
        // Parse the arguments and invoke the handler
        return await parser.InvokeAsync(args);
    }
    
    static void 
}


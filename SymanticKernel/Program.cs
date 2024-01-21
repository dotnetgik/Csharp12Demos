// See https://aka.ms/new-console-template for more information
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.ComponentModel;

Console.WriteLine("Hello, World!");
// Create kernel
var builder = Kernel.CreateBuilder();
// Add a text or chat completion service using either:
// builder.Services.AddAzureOpenAIChatCompletion()
// builder.Services.AddAzureOpenAITextGeneration()
//builder.Services.AddOpenAIChatCompletion()
// builder.Services.AddOpenAITextGeneration()
builder.Plugins.AddFromType<LightPlugin>();
var kernel = builder.Build();
 
// Create chat history
ChatHistory history = [];

// Get chat completion service
var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

// Start the conversation
while (true)
{
    // Get user input
    Console.Write("User > ");
    history.AddUserMessage(Console.ReadLine()!);

    // Enable auto function calling
    OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
    {
        ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
    };

    // Get the response from the AI
    var result = await chatCompletionService.GetChatMessageContentAsync(
        history,
        executionSettings: openAIPromptExecutionSettings,
        kernel: kernel);

    // Print the results
    Console.WriteLine("Assistant > " + result);

    // Add the message from the agent to the chat history
    history.AddMessage(result.Role, result.Content);
}


public class LightPlugin
{
    public bool IsOn { get; set; } = false;

    [KernelFunction]
    [Description("Gets the state of the light.")]
    public string GetState() => this.IsOn ? "on" : "off";

    [KernelFunction]
    [Description("Changes the state of the light.'")]
    public string ChangeState(bool newState)
    {
        this.IsOn = newState;
        var state = this.GetState();

        // Print the state to the console
        Console.ForegroundColor = ConsoleColor.DarkBlue;
        Console.WriteLine($"[Light is now {state}]");
        Console.ResetColor();

        return state;
    }
}
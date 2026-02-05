using Azure.AI.OpenAI;
using Azure;
using HrDesk.Core.Interfaces;
using HrDesk.Core.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using OpenAI.Chat;

namespace HrDesk.Ai;

public class LlmOrchestrator : IAiOrchestrator
{
    private readonly ChatClient _chatClient;
    private readonly ILogger<LlmOrchestrator> _logger;
    private readonly string _modelName;

    public LlmOrchestrator(IConfiguration configuration, ILogger<LlmOrchestrator> logger)
    {
        _logger = logger;
        var apiKey = configuration["OpenAI:ApiKey"];
        _modelName = configuration["OpenAI:Model"] ?? "gpt-3.5-turbo";

        if (string.IsNullOrEmpty(apiKey))
        {
            _logger.LogError("OpenAI API Key is missing.");
            throw new ArgumentNullException(nameof(apiKey), "OpenAI API Key is required for Live AI.");
        }

        // Initialize OpenAI Client
        // Note: In production code, consider injecting the client via DI
        var client = new AzureOpenAIClient(new Uri("https://api.openai.com/v1"), new System.ClientModel.ApiKeyCredential(apiKey)); 
        // Or if using non-Azure OpenAI directly:
        // var client = new OpenAIClient(apiKey);
        // Using the new Azure.AI.OpenAI v2 / OpenAI library patterns
        // Constructing ChatClient directly for simplicity if using OpenAI official vs Azure
        
        // Let's assume standard OpenAI usage for now, relying on Azure.AI.OpenAI package which supports both.
        // If using standard OpenAI (not Azure), the instantiation might differ slightly depending on version.
        // For v2.0.0+ of Azure.AI.OpenAI (which wraps the official OpenAI logic):
        
        var openAiClient = new OpenAI.OpenAIClient(apiKey);
        _chatClient = openAiClient.GetChatClient(_modelName);
    }

    public async Task<AiResponse> HandleAsync(ChatRequest request, UserContext userContext, CancellationToken cancellationToken = default)
    {
        var userId = userContext.EmployeeId ?? "System";
        var message = request.Message;
        _logger.LogInformation("Processing message for User {UserId} with Live AI", userId);

        var messages = new List<ChatMessage>
        {
            new SystemChatMessage(@"You are a helpful HR Assistant for a company called 'Regulus'.
Your goal is to classify the user's intent and provide a helpful response.
You must return your response in a structured JSON format.

Supported Intents:
- 'GeneralInquiry': For general questions.
- 'SoftwareRequest': When user asks for software or licenses.
- 'Payroll': For salary, payslip, or tax questions.
- 'LeaveRequest': For holiday or time off.
- 'Complaint': For grievances.

JSON Format:
{
  ""intent"": ""IntentName"",
  ""response"": ""Your natural language response here.""
}"),
            new UserChatMessage(message)
        };

        try
        {
            ChatCompletion completion = await _chatClient.CompleteChatAsync(messages, cancellationToken: cancellationToken);
            
            var rawContent = completion.Content[0].Text;
            _logger.LogDebug("Raw LLM Response: {RawContent}", rawContent);

            var parsed = JsonSerializer.Deserialize<LlmResponseSchema>(rawContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (parsed == null)
            {
                return new AiResponse 
                { 
                    Intent = "Unknown", 
                    Response = "I'm sorry, I couldn't process that properly (Invalid JSON from AI)." 
                };
            }

            return new AiResponse
            {
                Intent = parsed.Intent,
                Response = parsed.Response
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling OpenAI API");
            return new AiResponse { Intent = "Error", Response = "I'm having trouble connecting to my brain right now. Please try again later." };
        }
    }

    private class LlmResponseSchema
    {
        public string Intent { get; set; } = string.Empty;
        public string Response { get; set; } = string.Empty;
    }
}

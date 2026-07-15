using System;
using System.Threading.Tasks;
using System.ClientModel;
using OpenAI.Chat;
using OpenAI;

class Program {
    static async Task Main() {
        var apiKey = "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";
        var endpoint = "https://openrouter.ai/api/v1";
        
        // Let's test with tencent/hy3:free
        await TestModel(apiKey, endpoint, "tencent/hy3:free");
        
        // Let's test with google/gemini-2.5-flash:free (or pro if flash free is not available, or meta-llama/llama-3.3-70b-instruct:free)
        await TestModel(apiKey, endpoint, "google/gemini-2.5-flash:free");
        await TestModel(apiKey, endpoint, "meta-llama/llama-3.3-70b-instruct:free");
    }

    static async Task TestModel(string apiKey, string endpoint, string model) {
        Console.WriteLine($"\n--- Testing Model: {model} ---");
        try {
            var client = new ChatClient(model, new ApiKeyCredential(apiKey), new OpenAIClientOptions { Endpoint = new Uri(endpoint) });
            var messages = new[] { new UserChatMessage("What's my current folio balance?") };
            var options = new ChatCompletionOptions { ToolChoice = ChatToolChoice.CreateAutoChoice() };
            options.Tools.Add(ChatTool.CreateFunctionTool(
                "GetFolioBalance",
                "Get the guest's current folio balance and billing details."
            ));

            var completion = await client.CompleteChatAsync(messages, options);
            var response = completion.Value;
            
            Console.WriteLine($"FinishReason: {response.FinishReason}");
            Console.WriteLine($"ToolCalls Count: {response.ToolCalls.Count}");
            foreach (var tc in response.ToolCalls) {
                Console.WriteLine($"  ToolCall: {tc.FunctionName} (Id: {tc.Id})");
            }
            Console.WriteLine($"Content text: {response.Content[0]?.Text}");
        } catch (Exception ex) {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}

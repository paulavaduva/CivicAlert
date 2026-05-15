using System.Text.Json;
using System.Text;

public class GeminiService
{
    private readonly string _apiKey;
    private readonly HttpClient _httpClient;

    public GeminiService(IConfiguration config, HttpClient httpClient)
    {
        _apiKey = config["Gemini:ApiKey"];
        _httpClient = httpClient;
    }

    public async Task<AiResponse> ProcessIssueAsync(byte[] imageBytes, string description, string categoriesJson, string departmentsJson, List<string> nearbyImageUrls = null)
    {
        var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={_apiKey}";

        var parts = new List<object>();

        parts.Add(new { inline_data = new { mime_type = "image/jpeg", data = Convert.ToBase64String(imageBytes) } });

        string duplicateContext = "Nu sunt sesizări în apropiere pentru comparație.";

        if (nearbyImageUrls != null && nearbyImageUrls.Any())
        {
            try
            {
                var existingImageUrl = nearbyImageUrls.First();

                var existingImageBytes = await _httpClient.GetByteArrayAsync(existingImageUrl);

                parts.Add(new { inline_data = new { mime_type = "image/jpeg", data = Convert.ToBase64String(existingImageBytes) } });

                duplicateContext = "Ți-am trimis DOUĂ imagini. Prima este sesizarea curentă, a doua este o sesizare deja existentă în baza de date la aceeași locație. " +
                                   "Analizează-le vizual. Dacă reprezintă același defect (aceeași groapă, același obiect stricat), chiar dacă unghiul diferă, setează 'isDuplicate': true.";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Nu s-a putut descărca imaginea veche: {ex.Message}");
                duplicateContext = "Există o sesizare în apropiere, dar nu am putut încărca imaginea ei pentru comparație vizuală. Bazează-te pe text și locație.";
            }
        }

        var prompt = @$"Ești un dispecer inteligent pentru un sistem Smart City. 
            Sarcina ta este să analizezi sesizarea primită și să ajuți la validarea ei.

            DATE DE INTRARE:
            - Descriere user: '{description}'
            - Categorii: {categoriesJson}
            - Departamente: {departmentsJson}
            - Context duplicate: {duplicateContext}

            INSTRUCȚIUNI DE VALIDARE:
            1. 'isValid': Verifică dacă imaginea și descrierea reprezintă o problemă reală de infrastructură urbană (gropi, gunoi, iluminat, etc.). Setează 'false' dacă este un selfie, o poză cu mâncare, un ecran negru sau conținut irelevant.
            2. 'confidenceScore': Acordă un scor de la 0 la 100 bazat pe cât de clară este imaginea și cât de bine se potrivește cu descrierea.
            3. 'validationReason': Explică pe scurt decizia (ex: 'Imagine clară cu un stâlp căzut' sau 'Respins: poza nu are legătură cu descrierea').

            Răspunde STRICT în format JSON:
            {{
                ""isValid"": bool,
                ""confidenceScore"": int,
                ""validationReason"": ""string"",
                ""isDuplicate"": bool,
                ""isNewCategory"": bool,
                ""categoryId"": int_sau_null,
                ""newCategoryName"": string_sau_null,
                ""departmentId"": int,
                ""severity"": ""Low/Medium/High/Urgent"",
                ""summary"": ""max 10 cuvinte""
            }}";

        parts.Insert(0, new { text = prompt });

        var payload = new
        {
            contents = new[] { new { parts = parts.ToArray() } },
            generationConfig = new { response_mime_type = "application/json" }
        };

        var response = await _httpClient.PostAsJsonAsync(url, payload);
        var jsonResponse = await response.Content.ReadAsStringAsync();

        using var doc = JsonDocument.Parse(jsonResponse);

        if (!doc.RootElement.TryGetProperty("candidates", out var candidates))
        {
            Console.WriteLine("Eroare Gemini: " + jsonResponse);
            throw new Exception("AI-ul nu a returnat niciun rezultat.");
        }

        var resultText = candidates[0].GetProperty("content").GetProperty("parts")[0].GetProperty("text").GetString();

        resultText = resultText.Replace("```json", "").Replace("```", "").Trim();

        return JsonSerializer.Deserialize<AiResponse>(resultText, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }
}

public class AiResponse
{
    public bool IsValid { get; set; }
    public int ConfidenceScore { get; set; }
    public string ValidationReason { get; set; }


    public bool IsDuplicate { get; set; }
    public bool IsNewCategory { get; set; }
    public int? CategoryId { get; set; }
    public string NewCategoryName { get; set; }
    public int DepartmentId { get; set; }
    public string Severity { get; set; }
    public string Summary { get; set; }
}
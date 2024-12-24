using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace CpfValidatorHttpFunction;

public class CpfValidatorFunction
{
    private readonly ILogger<CpfValidatorFunction> _logger;

    public CpfValidatorFunction(ILogger<CpfValidatorFunction> logger)
    {
        _logger = logger;
    }

    [Function("CpfValidatorFunction")]
    public async Task<IActionResult> RunAsync([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request.");

        var cpf = req.Query["cpf"];
        
        if (string.IsNullOrEmpty(cpf))
        {
            if (req.Method == "POST")
            {
                using var reader = new StreamReader(req.Body);
                cpf = await reader.ReadToEndAsync();
            }
            else
            {
                return new BadRequestObjectResult("Por favor, informe o CPF.");
            }
        }

        if (!string.IsNullOrEmpty(cpf) && IsValid(cpf))
            return new OkObjectResult("CPF válido!");
        else
            return new BadRequestObjectResult("CPF inválido!");
    }

    public static bool IsValid(string cpf)
    {
        if (cpf.Length != 11 || !cpf.All(char.IsDigit))
            return false;

        var cpfArray = cpf.Select(c => int.Parse(c.ToString())).ToArray();

        for (int j = 0; j < 10; j++)
            if (cpfArray.All(d => d == j))
                return false;

        int[] multiplicador1 = new int[9] { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
        int[] multiplicador2 = new int[10] { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };

        int soma = 0;
        for (int i = 0; i < 9; i++)
            soma += cpfArray[i] * multiplicador1[i];

        int resto = soma % 11;
        if (resto < 2)
            resto = 0;
        else
            resto = 11 - resto;

        if (cpfArray[9] != resto)
            return false;

        soma = 0;
        for (int i = 0; i < 10; i++)
            soma += cpfArray[i] * multiplicador2[i];

        resto = soma % 11;
        if (resto < 2)
            resto = 0;
        else
            resto = 11 - resto;

        if (cpfArray[10] != resto)
            return false;

        return true;
    }
}

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace httpValidaCpf
{
    public static class fnvalidacpf
    {
        [FunctionName("fnvalidacpf")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Iniciando validação de CPF.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            if(data == null)
            {
                return new BadRequestObjectResult("Por favor, informe o CPF.");
            }
            string cpf = data?.cpf;

            if(!ValidarCpf(cpf))
            {
                log.LogInformation("CPF inválido.");
                return new BadRequestObjectResult("CPF inválido.");
            }

            var responseMessage = "CPF válido.";

            return new OkObjectResult(responseMessage);
        }

        public static bool ValidarCpf(string cpf)
        {
            if (string.IsNullOrEmpty(cpf))
                return false;
        
            cpf = cpf.Replace(".", "").Replace("-", "");
        
            if (cpf.Length != 11)
                return false;
        
            bool todosDigitosIguais = true;
            for (int i = 1; i < 11 && todosDigitosIguais; i++)
            {
                if (cpf[i] != cpf[0])
                    todosDigitosIguais = false;
            }
        
            if (todosDigitosIguais)
                return cpf == "00000000000";
        
            int[] multiplicador1 = new int[9] { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
            int[] multiplicador2 = new int[10] { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };
        
            string tempCpf = cpf.Substring(0, 9);
            int soma = 0;
        
            for (int i = 0; i < 9; i++)
                soma += int.Parse(tempCpf[i].ToString()) * multiplicador1[i];
        
            int resto = soma % 11;
            if (resto < 2)
                resto = 0;
            else
                resto = 11 - resto;
        
            string digito = resto.ToString();
            tempCpf = tempCpf + digito;
            soma = 0;
        
            for (int i = 0; i < 10; i++)
                soma += int.Parse(tempCpf[i].ToString()) * multiplicador2[i];
        
            resto = soma % 11;
            if (resto < 2)
                resto = 0;
            else
                resto = 11 - resto;
        
            digito = digito + resto.ToString();
        
            return cpf.EndsWith(digito);
        }
        
    }
}

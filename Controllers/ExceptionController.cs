using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace localizeBackendAPI.Controllers
{
    public class ExceptionController : Controller
    {
        public string GetFullExceptionMessage(Exception ex)
        {

            if (ex is DbUpdateException || ex is DbUpdateConcurrencyException)
                return "Ocorreu um erro ao salvar os dados. Verifique os campos informados e tente novamente.";

            return "Ocorreu um erro inesperado ao processar a solicitação.";
        }

    }
}

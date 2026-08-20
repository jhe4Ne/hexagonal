using Galaxy.Lol.Domain.Ports.Repositories;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Galaxy.Lol.API.Filters
{

    public class UnitOfWorkFilter(IUnitOfWork unitOfWork) : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var ejecutada = await next();

            if (ejecutada.Exception is null || ejecutada.ExceptionHandled)
                await unitOfWork.SaveChangesAsync(context.HttpContext.RequestAborted);
        }
    }
}

using System.Threading.Tasks;
using counter.Models;
using Microsoft.AspNetCore.Authorization;

namespace counter.Authorization
{
    public class BusinessObjectAuthorizationHandler : AuthorizationHandler<SameOwnerRequirement,IBusinessObject>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, 
                                                        SameOwnerRequirement requirement, 
                                                        IBusinessObject resource)
        {
                if (context.User.Identity?.Name == resource.Owner.UserName)
                {
                    context.Succeed(requirement);
                }

                return Task.CompletedTask;
        }
    }
    public class SameOwnerRequirement : IAuthorizationRequirement { }
}
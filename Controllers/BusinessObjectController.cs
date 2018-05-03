using counter.Data;
using counter.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace counter.Controllers
{
    public class BusinessObjectController:Controller
    {
        protected ApplicationDbContext _ctx;
        protected UserManager<ApplicationUser> _userManager;
        protected IAuthorizationService _authorizationService;
        public BusinessObjectController(UserManager<ApplicationUser> userManager,ApplicationDbContext ctx,  IAuthorizationService authorizationService)
        {
            _ctx = ctx;
            _userManager = userManager;
            _authorizationService = authorizationService;
        }
        public async Task<bool> IsOwner(IBusinessObject bo)
        {
            if(bo==null)
                return false;
            var authorizationResult = await _authorizationService.AuthorizeAsync(User,bo,"EditPolicy");
            return authorizationResult.Succeeded;
        }
    }
}
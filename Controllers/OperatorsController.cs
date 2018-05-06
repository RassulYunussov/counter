using System.Collections.Generic;
using System.Threading.Tasks;
using counter.Data;
using counter.Models;
using counter.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using AspNet.Security.OpenIdConnect.Primitives;
using Microsoft.AspNetCore.Authorization;
using counter.Authorization;

namespace counter.Controllers
{
    [Route("/api/[controller]")]
    [Authorize(Roles="Owner")]
    public class OperatorsController :BusinessObjectController
    {

         public OperatorsController(UserManager<ApplicationUser> userManager,
                                    ApplicationDbContext ctx,
                                    IAuthorizationService authorizationService):base(userManager,ctx,authorizationService)   
         {
         }
         [HttpGet]
         public async Task<IEnumerable<Operator>> Get()
         {
             var user = await _userManager.GetUserAsync(User);
             var operators = await (from o in _ctx.Users
                             where o.Owner.Id == user.Id
                             select new Operator { Id = o.Id, OperatorName = o.UserName }).AsNoTracking().ToListAsync();
             return operators;
         }
         [HttpGet("{id}")]
         public async Task<Operator> Get(string id)
         {
             var user = await _userManager.GetUserAsync(User);
             var oper = await (from o in _ctx.Users
                             where o.Id == id && o.Owner.Id == user.Id
                             select new Operator { Id = o.Id, OperatorName = o.UserName }).AsNoTracking().SingleOrDefaultAsync();
             return oper;
         }

         [HttpPost]
         public async Task<IActionResult> Post([FromBody]Operator oper) 
         {
             if(ModelState.IsValid)
             {
                ApplicationUser user  = new ApplicationUser { UserName = oper.OperatorName,Email = oper.OperatorName+"counter.com" };
                user.Owner = await _userManager.GetUserAsync(User);
                var result = await _userManager.CreateAsync(user,oper.OperatorPassword);
                if(result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user,"Operator");
                    await _userManager.AddClaimAsync(user,new Claim(OpenIdConnectConstants.Claims.Subject,user.Id));
                    return Json(new Operator {Id = user.Id, OperatorName = oper.OperatorName });
                }
             }
             return BadRequest(new { error="Can't create operator", name=oper.OperatorName});
         }
         [HttpDelete("{id}")]
         public async Task<IActionResult> Delete(string id)
         {
            var user = await _ctx.Users.Include(u=>u.Owner)
                                        .Where(u=>u.Id==id)
                                        .SingleOrDefaultAsync();
            if(await IsOwner(user))
            {
                var result =await _userManager.DeleteAsync(user);
                if(result.Succeeded)
                    return Ok();
            }
            return BadRequest();
         }
         [HttpPut]
         public async Task<IActionResult> Put([FromBody] Operator oper)
         {
            if(ModelState.IsValid)
            {
                var user = await _ctx.Users.Include(u=>u.Owner)
                                        .Where(u=>u.Id==oper.Id)
                                        .SingleOrDefaultAsync();
                if(await IsOwner(user))
                {
                    user.UserName = oper.OperatorName;
                    user.PasswordHash = _userManager.PasswordHasher.HashPassword(user,oper.OperatorPassword);
                    var result =  await _userManager.UpdateAsync(user);
                    if(result.Succeeded)
                        return Ok();
                }
            }
            return BadRequest();
         }
    }
}
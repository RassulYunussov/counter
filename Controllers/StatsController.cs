using System;
using System.Linq;
using System.Threading.Tasks;
using counter.Data;
using counter.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace counter.Controllers
{
    [Route("/api/[controller]/[action]")]
    [Authorize(Roles="Owner")]
    public class StatsController: BusinessObjectController
    {
        public StatsController(UserManager<ApplicationUser> userManager,ApplicationDbContext ctx,  IAuthorizationService authorizationService):base( userManager, ctx, authorizationService)
        {
            
        }
        
        private bool InPeriod(string period, DateTime date)
        {
            bool result = false;
            switch(period)
            {
                case "y": 
                        result = date.Year==DateTime.Now.Year;
                    break;
                case "m": 
                        result = date.ToString("yyyy-MM")== DateTime.Now.ToString("yyyy-MM");
                    break;
                default:
                        result = date.ToShortDateString()==DateTime.Now.ToShortDateString();
                    break;
            }
            return result;
        }
        [HttpGet("{period?}")]
        public async Task<IActionResult> BusinessPoints(string period)
        {
           var user = await _userManager.GetUserAsync(User);
           string ownerId = user.Id;
           var res = (from bp in _ctx.BusinessPoints
                        join t in _ctx.Tickets on bp.Id equals t.BusinessPoint.Id
                        where bp.Owner.Id==ownerId && InPeriod(period,t.OperationDate)
                        select new {bp.Id,t.Amount}).GroupBy(b=>b.Id)
                                                    .Select(b => new { businessPointId = b.Key, amount = b.Sum(t=>t.Amount)});
           return Json(res);
        }
        [HttpGet("{businessPointId}/{startDate}/{endDate}/{period?}")]
        public async Task<IActionResult> BusinessPointDynamics(int businessPointId, string period, DateTime startDate, DateTime endDate)
        {
           var user = await _userManager.GetUserAsync(User);
           string ownerId = user.Id;
           var res = (from bp in _ctx.BusinessPoints
                        join t in _ctx.Tickets on bp.Id equals t.BusinessPoint.Id
                        where bp.Id==businessPointId 
                                && bp.Owner.Id==ownerId 
                                && t.OperationDate >= startDate
                                && t.OperationDate < endDate
                        select t);
          
           switch(period) 
           {
               case "y":
                        return Json(res.GroupBy(t=>t.OperationDate.Year).Select(g=>new { period = g.Key, total = g.Sum(gg=>gg.Amount)}));
               case "m":
                        return Json(res.GroupBy(t=>t.OperationDate.ToString("yyyy-MM")).Select(g=>new { period = g.Key, total = g.Sum(gg=>gg.Amount)}));
               default:
                        return Json(res.GroupBy(t=>t.OperationDate.ToShortDateString()).Select(g=>new { period = g.Key, total = g.Sum(gg=>gg.Amount)}));
           }
        }
    }
}
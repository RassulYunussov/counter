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
        private IQueryable<Ticket> GetTickets(string ownerId, int businessPointId, string period,DateTime startDate, DateTime endDate) 
        {
          return from bp in _ctx.BusinessPoints
                        join t in _ctx.Tickets on bp.Id equals t.BusinessPoint.Id
                        where bp.Id==businessPointId 
                                && bp.Owner.Id==ownerId 
                                && t.OperationDate >= startDate
                                && t.OperationDate < endDate
                        select t;
        }
        private IQueryable<Ticket> GetTickets(string ownerId, int businessPointId, string period)
        {
           switch(period) 
           {
               case "y":
                //return months in current year
                return (from bp in _ctx.BusinessPoints
                                        join t in _ctx.Tickets on bp.Id equals t.BusinessPoint.Id
                                        where bp.Id==businessPointId 
                                                && bp.Owner.Id==ownerId 
                                                && t.OperationDate.Year == DateTime.Now.Year
                                        select t);
               case "m":
                //return days in current month
                return (from bp in _ctx.BusinessPoints
                                        join t in _ctx.Tickets on bp.Id equals t.BusinessPoint.Id
                                        where bp.Id==businessPointId 
                                                && bp.Owner.Id==ownerId 
                                                && t.OperationDate.ToString("yyyy-MM") == DateTime.Now.ToString("yyyy-MM")
                                        select t);
               default:
               //return all in current day
                return (from bp in _ctx.BusinessPoints
                                        join t in _ctx.Tickets on bp.Id equals t.BusinessPoint.Id
                                        where bp.Id==businessPointId 
                                                && bp.Owner.Id==ownerId 
                                                && t.OperationDate.ToShortDateString() == DateTime.Now.ToShortDateString()
                                        select t);
           }
        }
        private async Task<IActionResult> GetBPStats(string ownerId, int businessPointId, string period)
        {
                var tickets = GetTickets(ownerId,businessPointId,period);
                switch(period) 
                {
                    case "y":
                            return Json(tickets.GroupBy(t=>t.OperationDate.Month).Select(g=>new object []{g.Key,g.Sum(gg=>gg.Amount)}));
                    case "m":
                            return Json(tickets.GroupBy(t=>t.OperationDate.Day).Select(g=>new object []{g.Key,g.Sum(gg=>gg.Amount)}));
                    default:
                            return Json(tickets.GroupBy(t=>t.OperationDate.ToString("yyyy-MM-hh")).Select(g=>new object []{g.Key,g.Sum(gg=>gg.Amount)}));
                }
        }
        private async Task<IActionResult> GetBPStats(string ownerId, int businessPointId, string period,DateTime startDate, DateTime endDate)
        {
                var tickets  = GetTickets(ownerId,businessPointId,period,startDate,endDate);
                switch(period) 
                {
                        case "y":
                                return Json(tickets.GroupBy(t=>t.OperationDate.Year).Select(g=>new object []{g.Key,g.Sum(gg=>gg.Amount)}));
                        case "m":
                                return Json(tickets.GroupBy(t=>t.OperationDate.ToString("yyyy-MM")).Select(g=>new object []{g.Key,g.Sum(gg=>gg.Amount)}));
                        default:
                                return Json(tickets.GroupBy(t=>t.OperationDate.ToShortDateString()).Select(g=>new object []{g.Key,g.Sum(gg=>gg.Amount)}));
                }
        }
        [HttpGet("{businessPointId}/{startDate}/{endDate}/{period?}")]
        public async Task<IActionResult> BusinessPointDynamics(int businessPointId, string period, DateTime startDate, DateTime endDate)
        {
           var user = await _userManager.GetUserAsync(User);
           string ownerId = user.Id;
           if(startDate==endDate)
                return await GetBPStats(ownerId,businessPointId,period);
           return await GetBPStats(ownerId,businessPointId,period,startDate,endDate);
        }
    }
}
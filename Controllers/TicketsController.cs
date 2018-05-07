using System;
using System.Threading.Tasks;
using counter.Data;
using counter.Models;
using counter.Observers;
using counter.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
namespace counter.Controllers
{
    [Route("/api/[controller]")]
    [Authorize(Roles="Operator")]
    public class TicketsController: Controller
    {
        ApplicationDbContext _ctx;
        UserManager<ApplicationUser> _userManager;
        StatsObservables _so;
        public TicketsController(ApplicationDbContext ctx,
                                UserManager<ApplicationUser> userManager,
                                StatsObservables so)
        {
            _ctx = ctx;
            _userManager = userManager;
            _so = so;
        }
        [HttpPost]
        public async Task<IActionResult> AddTicket([FromBody] TicketView tv)
        {
            var oper = await _userManager.GetUserAsync(User);
            BusinessPoint bp = await _ctx.BusinessPoints
                                        .Include(p=>p.Owner)
                                        .Where(p=>p.Id==tv.BusinessPointId)
                                        .SingleOrDefaultAsync();
            Ticket t = new Ticket{OperationDate = DateTime.Now,Amount = tv.Amount,Operator = oper, BusinessPoint = bp};
            _ctx.Tickets.Add(t);
            var result = await _ctx.SaveChangesAsync();
            if(result>0) 
            {

                decimal Amount = await _ctx.Tickets.Where(tt => tt.OperationDate.ToShortDateString() == DateTime.Now.ToShortDateString())
                                .SumAsync(tt=>tt.Amount);
                _so.GetObservable(bp.Owner.UserName).BroadcastStats(oper.UserName,bp.Id,Amount);
                return Ok(tv);
            }
            return BadRequest(tv);
        }
    }
}
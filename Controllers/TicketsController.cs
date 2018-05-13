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
using counter.Stats;

namespace counter.Controllers
{
    [Route("/api/[controller]")]
    [Authorize(Roles="Operator")]
    public class TicketsController: Controller
    {
        ApplicationDbContext _ctx;
        UserManager<ApplicationUser> _userManager;
        BusinessPointStatsChannels _sc;
        public TicketsController(ApplicationDbContext ctx,
                                UserManager<ApplicationUser> userManager,
                                BusinessPointStatsChannels sc)
        {
            _ctx = ctx;
            _userManager = userManager;
            _sc = sc;
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

                decimal Amount = await _ctx.Tickets.Where(tt => tt.BusinessPoint.Id==bp.Id && tt.OperationDate.ToShortDateString() == DateTime.Now.ToShortDateString())
                                .SumAsync(tt=>tt.Amount);
                await _sc.GetChannel(bp.Owner.UserName).Writer.WriteAsync(new BusinessPointStats{BusinessPointId = bp.Id, TotalAmount = Amount}); 
                return Ok(tv);
            }
            return BadRequest(tv);
        }
    }
}
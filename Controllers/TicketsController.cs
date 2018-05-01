using System;
using System.Threading.Tasks;
using counter.Data;
using counter.Models;
using counter.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace counter.Controllers
{
    [Route("/api/[controller]")]
    [Authorize(Roles="Operator")]
    public class TicketsController: Controller
    {
        ApplicationDbContext _ctx;
        UserManager<ApplicationUser> _userManager;
        public TicketsController(ApplicationDbContext ctx,UserManager<ApplicationUser> userManager)
        {
            _ctx = ctx;
            _userManager = userManager;
        }
        [HttpPost]
        public async Task<IActionResult> AddTicket([FromBody] TicketView tv)
        {
            var oper = await _userManager.GetUserAsync(User);
            BusinessPoint bp = new BusinessPoint{Id= tv.BusinessPointId};
            _ctx.Attach(bp);
            Ticket t = new Ticket{OperationDate = DateTime.Now,Amount = tv.Amount,Operator = oper, BusinessPoint = bp};
            _ctx.Tickets.Add(t);
            var result = await _ctx.SaveChangesAsync();
            if(result>0) 
            {
                return Ok(tv);
            }
            return BadRequest(tv);
        }
    }
}
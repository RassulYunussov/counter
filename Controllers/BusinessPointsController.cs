using System.Collections.Generic;
using System.Threading.Tasks;
using counter.Data;
using counter.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using counter.ViewModels;
using System;

namespace counter.Controllers
{
    [Route("/api/[controller]")]
    [Authorize(Roles="Owner")]
    public class BusinessPointsController: Controller
    {
        ApplicationDbContext _ctx;
        UserManager<ApplicationUser> _userManager;
        public BusinessPointsController(UserManager<ApplicationUser> userManager,ApplicationDbContext ctx)
        {
            _ctx = ctx;
            _userManager = userManager;
        }
        [HttpGet]
        public async Task<IEnumerable<BusinessPoint>> Get()
        {
           var user = await _userManager.GetUserAsync(User);
           return await _ctx.BusinessPoints.Where(bp=>bp.Owner.Id==user.Id).ToListAsync();
        }
        [HttpGet("{id}")]
        public async Task<BusinessPointView> Get(int id)
        {
            var bp = await _ctx.BusinessPoints.FindAsync(id);
            return new BusinessPointView {Id = bp.Id, Name = bp.Name,Location = bp.Location,Price = (int)bp.Price,Duration = bp.Duration.Minutes};
        }
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] BusinessPointView bpv)
        {
            var user = await _userManager.GetUserAsync(User);
            BusinessPoint bp = new BusinessPoint{Name = bpv.Name,
                                                Location = bpv.Location, 
                                                Price = bpv.Price, 
                                                Duration = TimeSpan.FromMinutes(bpv.Duration),
                                                Owner = user};
            _ctx.Add(bp);
            var result = await _ctx.SaveChangesAsync();
            if(result>0){
                bpv.Id = bp.Id;
                return Ok(bpv);
            }
            return BadRequest(bpv);
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
             var bp = await _ctx.BusinessPoints.FindAsync(id);
             if(bp!=null)
             {
                 _ctx.BusinessPoints.Remove(bp);
                 await _ctx.SaveChangesAsync();
                 return Ok();
             }
             return BadRequest(id);
        }
    }
}
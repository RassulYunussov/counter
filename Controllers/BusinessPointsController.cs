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
        [Authorize(Roles="Owner,Operator")]
        public async Task<IEnumerable<BusinessPointView>> Get()
        {
           var user = await _userManager.GetUserAsync(User);
           var isOperator = await _userManager.IsInRoleAsync(user,"Operator");
           string ownerId = user.Id;
           if(isOperator)
           {
              var oper = await _ctx.Users.Include(u=>u.Owner).FirstOrDefaultAsync(u=>u.Id == user.Id);
              ownerId = oper.Owner.Id;
           }
           return await _ctx.BusinessPoints.Where(bp=>bp.Owner.Id==ownerId).Select(bp=>new BusinessPointView {Id = bp.Id, Name = bp.Name,Location = bp.Location,Price = bp.Price,Duration = bp.Duration}).ToListAsync();
        }
        [HttpGet("{id}")]
        [Authorize(Roles="Owner,Operator")]
        public async Task<BusinessPointView> Get(int id)
        {
            var bp = await _ctx.BusinessPoints.FindAsync(id);
            return new BusinessPointView {Id = bp.Id, Name = bp.Name,Location = bp.Location,Price = bp.Price,Duration = bp.Duration};
        }
        [HttpPut]
        [Authorize(Roles="Owner")]
        public async Task<IActionResult> Put([FromBody] BusinessPointView bpv)
        {
            var bp = await _ctx.BusinessPoints.FindAsync(bpv.Id);
            if(bp!=null)
            {
                bp.Name = bpv.Name;
                bp.Location = bpv.Location;
                bp.Price = bpv.Price;
                bp.Duration = bpv.Duration;
                await _ctx.SaveChangesAsync();
                return Ok(bpv);
            }
            return BadRequest(bpv);
        }
        [HttpPost]
        [Authorize(Roles="Owner")]
        public async Task<IActionResult> Post([FromBody] BusinessPointView bpv)
        {
            var user = await _userManager.GetUserAsync(User);
            BusinessPoint bp = new BusinessPoint{Name = bpv.Name,
                                                Location = bpv.Location, 
                                                Price = bpv.Price, 
                                                Duration = bpv.Duration,
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
        [Authorize(Roles="Owner")]
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
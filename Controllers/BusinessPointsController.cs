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
           string ownerId = user.Id;
           if(User.IsInRole("Operator"))
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
            var user = await _userManager.GetUserAsync(User);
            string ownerId = user.Id;
            if(User.IsInRole("Operator"))
            {
                var oper = await _ctx.Users.Include(u=>u.Owner).FirstOrDefaultAsync(u=>u.Id == user.Id);
                ownerId = oper.Owner.Id;
            }
            var result = await _ctx.BusinessPoints.Where(bp=> bp.Id == id && bp.Owner.Id == ownerId).SingleOrDefaultAsync();
            if(result!=null)
                return new BusinessPointView {Id = result.Id, Name = result.Name,Location = result.Location,Price =result.Price,Duration = result.Duration};
            return null;
        }
        [HttpPut]
        [Authorize(Roles="Owner")]
        public async Task<IActionResult> Put([FromBody] BusinessPointView bpv)
        {
            var user = await _userManager.GetUserAsync(User);
            var result = await _ctx.BusinessPoints.Where(bp=> bp.Id == bpv.Id && bp.Owner.Id == user.Id).SingleOrDefaultAsync();
            if(result!=null)
            {
                result.Name = bpv.Name;
                result.Location = bpv.Location;
                result.Price = bpv.Price;
                result.Duration = bpv.Duration;
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
            var user = await _userManager.GetUserAsync(User);
            var result = await _ctx.BusinessPoints.Where(bp=> bp.Id == id && bp.Owner.Id == user.Id).SingleOrDefaultAsync();
            if(result!=null)
            {
                _ctx.BusinessPoints.Remove(result);
                await _ctx.SaveChangesAsync();
                return Ok();
            }
            return BadRequest(id);
        }
    }
}
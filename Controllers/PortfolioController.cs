using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Extension;
using api.Interfaces;
using api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [Route("api/portfolio")]
    [ApiController]
    public class PortfolioController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManger;
        private readonly IStockRepository _stockrepo;
        private readonly IPortfolioRepository _portfolioRepo;
        public PortfolioController(UserManager<AppUser> userManger, IStockRepository stockrepo, IPortfolioRepository portfolioRepo)
        {
            _userManger = userManger;
            _stockrepo = stockrepo;
            _portfolioRepo = portfolioRepo;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetUserPortfolio()
        {
            var username = User.GetUserName();
            var AppUser = await _userManger.FindByNameAsync(username);
            var userPortfolio = await _portfolioRepo.GetUserPortfolio(AppUser);

            return Ok(userPortfolio);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddPortfolio (string symbol)
        {
            var username = User.GetUserName();
            var appUser = await _userManger.FindByNameAsync(username);
            var stock = await _stockrepo.GetBySymbolAsync(symbol);

            if (stock == null)
            {
                return BadRequest("Stock not found");
            }

            var userportfolio = await _portfolioRepo.GetUserPortfolio(appUser);
            if (userportfolio.Any(p => p.Symbol.ToLower() == symbol.ToLower()))
            {
                return BadRequest("Cannot add same stock to portfolio");
            }

            var portfolioModel = new Portfolio
            {
                AppUserId = appUser.Id,
                StockId = stock.Id
            };

            await _portfolioRepo.CreateAsync(portfolioModel);

            if (portfolioModel == null)
            {
                return StatusCode(500, "Could not created");
            }
            else 
            {
                return Created();
            }
        }

        [HttpDelete]
        [Authorize]
        public async Task<IActionResult>DeletePortfolio (string symbol)
        {
            var username = User.GetUserName();
            var appUser = await _userManger.FindByNameAsync(username);
            var userportfolio = await _portfolioRepo.GetUserPortfolio(appUser);

            var filterstpock = userportfolio.Where(s => s.Symbol.ToLower() == symbol.ToLower()).ToList();

            if (filterstpock.Count() == 1)
            {
                await _portfolioRepo.DeletePortfolio(appUser, symbol);
            }
            else
            {
                return BadRequest("Stock not in your portfolio");
            }

            return Ok();

        }
    }
}
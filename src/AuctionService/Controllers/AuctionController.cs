using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Controllers;

[ApiController]
[Route("api/auctions")]
public class AuctionController : ControllerBase
{
    private readonly AuctionDbContext _dbContext;
    private readonly IMapper _mapper;

    public AuctionController(AuctionDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<List<AuctionDto>>> GetAllAuctions()
    {
        var auctions = await _dbContext.Actions
            .Include(x => x.Item)
            .OrderBy(x => x.Item.Make)
            .ToListAsync();
        return _mapper.Map<List<AuctionDto>>(auctions);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AuctionDto>> GetSingleAction(Guid id)
    {
        var action = await _dbContext.Actions.Include(x => x.Item).FirstOrDefaultAsync(i => i.Id == id);
        if (action == null)
        {
            return NotFound();
        }

        return _mapper.Map<AuctionDto>(action);
    }

    [HttpPost]
    public async Task<ActionResult<AuctionDto>> SaveAuction(CreateAuctionDto auctionDto)
    {
        var auction = _mapper.Map<Auction>(auctionDto);
        auction.Seller = "Test";
        _dbContext.Actions.Add(auction);

        var res = await _dbContext.SaveChangesAsync() > 0;
        if (!res)
        {
            return BadRequest("Auction not saved in DB");
        }
        else
        {
            return CreatedAtAction(
                nameof(GetSingleAction),
                new { auction.Id }, 
                _mapper.Map<AuctionDto>(auction)
            );
        }
    }

    [HttpPut]
    public async Task<ActionResult> UpdateAuction(UpdateAuctionDto auctionDto)
    {
        var auction = await _dbContext.Actions.Include(x => x.Item).FirstOrDefaultAsync();
        if (auction == null) return NotFound();

        auction.Item.Make = auctionDto.Make ?? auction.Item.Make;
        auction.Item.Model = auctionDto.Model ?? auction.Item.Model;
        auction.Item.Year = auctionDto.Year ?? auction.Item.Year;
        auction.Item.Color = auctionDto.Color ?? auction.Item.Color;
        auction.Item.Mileage = auctionDto.Mileage ?? auction.Item.Mileage;

        var res = await _dbContext.SaveChangesAsync();
        if (res > 0)
        {
            return Ok();
        }

        return BadRequest("Update went wrong...");
    }

    [HttpDelete]
    public async Task<ActionResult> DeleteAuction(Guid id)
    {
        var auction = await _dbContext.Actions.Include(x => x.Item).FirstOrDefaultAsync();
        if (auction == null) return NotFound();

        _dbContext.Actions.Remove(auction);
        var res = await _dbContext.SaveChangesAsync();
        if (res > 0)
        {
            return Ok();
        }
        return BadRequest("Problem with Delete...");

    }
}
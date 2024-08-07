using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;
using AutoMapper.QueryableExtensions;
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
    public async Task<ActionResult<List<AuctionDto>>> GetAllAuctions(string date)
    {
        var quary = _dbContext.Actions.OrderBy(x => x.Item.Make).AsQueryable();

        if (!string.IsNullOrEmpty(date))
        {
            quary = quary.Where(x => x.UpdatedAt.CompareTo(DateTime.Parse(date).ToUniversalTime()) > 0);
        }
        // var auctions = await _dbContext.Actions
        //     .Include(x => x.Item)
        //     .OrderBy(x => x.Item.Make)
        //     .ToListAsync();
        // return _mapper.Map<List<AuctionDto>>(auctions);

        return await quary.ProjectTo<AuctionDto>(_mapper.ConfigurationProvider).ToListAsync();
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

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateAuction(Guid id, UpdateAuctionDto auctionDto)
    {
        var auction = await _dbContext.Actions.Include(x => x.Item).FirstOrDefaultAsync(x => x.Id == id);
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

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteAuction(Guid id)
    {
        var auction = await _dbContext.Actions.Include(x => x.Item).FirstOrDefaultAsync(x => x.Id == id);
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
using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoMapper.QueryableExtensions;
using System;
using System.Collections.Generic;
using System.Linq;


namespace AuctionService.Controllers;
[ApiController]
[Route("api/auctions")]
public class AuctionController: ControllerBase
{
    private readonly AuctionDbContext _context;
    private readonly IMapper _mapper;
    public AuctionController(AuctionDbContext context, IMapper mapper)
    {
            _mapper = mapper;
            _context = context;        
    }
    [HttpGet]
    public async Task<ActionResult<List<AuctionDto>>> GetAuctions(string date)
    {
        var query = _context.Auctions.OrderBy(x => x.Item.Make).AsQueryable();
        
          if (!string.IsNullOrEmpty(date))
    {
        if (DateTime.TryParse(date, out DateTime parsedDate))
        {
            var utcDate = parsedDate.AddSeconds(-1).ToUniversalTime();
            query = query.Where(x => x.UpdatedAt.HasValue && x.UpdatedAt.Value.CompareTo(utcDate) > 0);
        }
        else
        {
            // 如果日期字符串无效，则返回一个错误消息
            return BadRequest("Invalid date format.");
        }
    }
        // var auctions = await query
        //     .Include(a => a.Item)
        //     .OrderBy(a => a.Item.Make)
        //     .ToListAsync();
        // return _mapper.Map<List<AuctionDto>>(auctions);
        return await  query.ProjectTo<AuctionDto>(_mapper.ConfigurationProvider).ToListAsync();
    }
    [HttpGet("{id}")]
    public async Task<ActionResult<AuctionDto>> GetAuctionById(Guid id)
    {
        var auction = await _context.Auctions
            .Include(a => a.Item)
            .FirstOrDefaultAsync(a => a.Id == id);
        if (auction == null)
        {
            return NotFound();
        }
        return _mapper.Map<AuctionDto>(auction);
    }
    [HttpPost]
    public async Task<ActionResult<AuctionDto>> CreateAuction(CreateAuctionDto auctionDto)
    {
        var auction = _mapper.Map<Auction>(auctionDto);
        //TODO : add current user as a seller
        auction.Seller="test";
        _context.Auctions.Add(auction);
        var result=await _context.SaveChangesAsync() > 0;
        if(!result) return BadRequest("Could not save changes to the DB");
        return CreatedAtAction(nameof(GetAuctionById),new {auction.Id},
        _mapper.Map<AuctionDto>(auction));

    }
    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateAuction(Guid id, UpdateAuctionDto updateAuctionDto)
    {
        var auction = await _context.Auctions.Include(x=>x.Item).FirstOrDefaultAsync(x => x.Id == id);
        if (auction == null)
        {
            return NotFound();
        }
        //TODO: check seller ==usernamer

        auction.Item.Make = updateAuctionDto.Make?? auction.Item.Make;
        auction.Item.Model = updateAuctionDto.Model?? auction.Item.Model;
        auction.Item.Color = updateAuctionDto.Color?? auction.Item.Color;
        auction.Item.Mileage = updateAuctionDto.Mileage?? auction.Item.Mileage;
        auction.Item.Year = updateAuctionDto.Year?? auction.Item.Year;
        var result=await _context.SaveChangesAsync() > 0;
        if(result) return Ok();
        return BadRequest("Could not save changes to the DB");
    }
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteAuction(Guid id)
    {
        var auction = await _context.Auctions.FindAsync(id);
        if (auction == null)
        {
            return NotFound();
        }
        _context.Remove(auction);
        var result=await _context.SaveChangesAsync() > 0;
        if(result) return Ok();
        return BadRequest("Could not save changes to the DB");
    }

}

﻿using AutoMapper;
using Contracts;
using MassTransit;
using MongoDB.Entities;

namespace SearchService;

public class AuctionCreatedConsumer : IConsumer<AuctionCreated>
{
    private IMapper _mapper;

    public AuctionCreatedConsumer(IMapper mapper)
    {
        _mapper = mapper;
    }
    
    public async Task Consume(ConsumeContext<AuctionCreated> context)
    {
        System.Console.WriteLine("--> Consuming auction created:"+context.Message.Id);
        var item=_mapper.Map<Item>(context.Message);
        await item.SaveAsync();        
    }
}

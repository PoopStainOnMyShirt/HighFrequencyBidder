using HighFreqBidder;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<BidChannel>(); //One rail instance to be shared
builder.Services.AddHostedService<BidWorker>(); //Background service to process bids

var app = builder.Build();

app.MapPost("/bid", (BidRequest bidRequest, BidChannel bidChannel) =>
{
    bool added = bidChannel.AddBid(bidRequest);
    if (added)
    {
        return Results.Accepted($"/bid/{bidRequest.RequestId}", bidRequest);
    }
    else
    {
        return Results.StatusCode(503); //Service Unavailable - our channel is full
    }
});

app.Run();
using HighFreqBidder;
using Microsoft.AspNetCore.Mvc;
using Aerospike.Client;

var builder = WebApplication.CreateBuilder(args);

string aerospikeHost = "127.0.0.1"; //We are running a local instance of Aerospike for testing via Docker
int aerospikePort = 3000;

var policy = new ClientPolicy(); //Default policy is sufficient for our testing, we can tune for performance later
policy.maxConnsPerNode = 500;

//connecting Aerospike client to db on startup
AerospikeClient aerospikeClient = new AerospikeClient(policy, aerospikeHost, aerospikePort);

//register as Singleton

builder.Services.AddSingleton<AerospikeClient>(aerospikeClient);

builder.Services.AddSingleton<BidChannel>(); //One rail instance to be shared
builder.Services.AddHostedService<BidWorker>(); //Background service to process bids

var app = builder.Build();

//close connection to db cleanly on shutdown
app.Lifetime.ApplicationStopping.Register(() =>
{
    aerospikeClient.Close();
});

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
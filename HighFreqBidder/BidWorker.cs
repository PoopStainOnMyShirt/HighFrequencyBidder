namespace HighFreqBidder;
using Aerospike.Client;

public class BidWorker: BackgroundService
{
    private readonly BidChannel _bidChannel;
    private readonly ILogger<BidWorker> _logger;
    private readonly AerospikeClient _aerospikeClient;

    //CTOR
    //System responsible for giving channel and logger. We do not create these.

    public BidWorker(BidChannel bidChannel, ILogger<BidWorker> logger, AerospikeClient aerospikeClient)
    {
        _bidChannel = bidChannel;
        _logger = logger;
        _aerospikeClient = aerospikeClient;
    }

    //This method starts automatically when the app turns on.
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Bid Worker started at: {time}", DateTimeOffset.Now);
        _logger.LogInformation("Waiting for bid requests...");

        await foreach(var bidRequest in _bidChannel.ReadAllAsync(stoppingToken))
        {
            try
            {
                _logger.LogInformation($"Processing Order: {bidRequest.RequestId} from {bidRequest.Country} for amount ${bidRequest.BidAmount}");
                var key = new Key("test", "bids", bidRequest.RequestId);
                
                //Columns(Bins) defintion
                var bins = new Bin[]
                {
                    new Bin("device", bidRequest.DeviceId),
                    new Bin("country", bidRequest.Country),
                    new Bin("amount", bidRequest.BidAmount),
                    new Bin("timestamp", DateTime.Now.Ticks)
                };

                _aerospikeClient.Put(null, key, bins);
                _logger.LogInformation($"Bid Request {bidRequest.RequestId} stored successfully.");
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, $"Error processing bid request! {bidRequest.RequestId}");
            }
        }
    }
}
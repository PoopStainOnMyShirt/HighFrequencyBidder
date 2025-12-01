namespace HighFreqBidder;

public class BidWorker: BackgroundService
{
    private readonly BidChannel _bidChannel;
    private readonly ILogger<BidWorker> _logger;

    //CTOR
    //System responsible for giving channel and logger. We do not create these.

    public BidWorker(BidChannel bidChannel, ILogger<BidWorker> logger)
    {
        _bidChannel = bidChannel;
        _logger = logger;
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
                await Task.Delay(10); //Simulate small processing time
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, $"Error processing bid request! {bidRequest.RequestId}");
            }
        }
    }
}
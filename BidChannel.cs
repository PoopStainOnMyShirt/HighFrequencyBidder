using System.Threading.Channels;

namespace HighFreqBidder;

public class BidChannel
{
    //This channel carries the bid request record from API endpoint to database
    private readonly Channel<BidRequest> _channel;

    public BidChannel(int capacity = 1000)
    {
        //By default, we only allow 1000 pending items, preventing our server from running out of RAM if the db dies.
        var options = new BoundedChannelOptions(capacity)
        {
            FullMode = BoundedChannelFullMode.DropWrite //If queue full, drop write attempt to channel immediately
        };
        _channel = Channel.CreateBounded<BidRequest>(options);
    }

    //Producer method, returns true if record added to channel, false if dropped
    public bool AddBid(BidRequest bidRequest)
    {
        return _channel.Writer.TryWrite(bidRequest);
    }

    //Consumer method, is called by background service. Allows worker to sit and wait for new items.
    public IAsyncEnumerable<BidRequest> ReadAllAsync(CancellationToken cancellationToken)
    {
        return _channel.Reader.ReadAllAsync(cancellationToken);
    }
}
namespace HighFreqBidder;

public record BidRequest //Holds data sent by advertiser
(
    string RequestId,
    string DeviceId,
    string Country,
    int BidAmount
);
//We're using a record here because of its immutability, making it thread-safe and suitable for parallel processing
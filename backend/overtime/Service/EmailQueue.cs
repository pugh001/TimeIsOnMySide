using System.Threading.Channels;
using Overtime.Service.Models.Bookings;

namespace Overtime.Service;

public sealed class EmailQueue
{
    private readonly Channel<BookingConfirmation> _channel =
        Channel.CreateBounded<BookingConfirmation>(new BoundedChannelOptions(256)
        {
            FullMode = BoundedChannelFullMode.DropOldest,
            SingleReader = true
        });

    public bool TryEnqueue(BookingConfirmation confirmation) =>
        _channel.Writer.TryWrite(confirmation);

    public ChannelReader<BookingConfirmation> Reader => _channel.Reader;
}

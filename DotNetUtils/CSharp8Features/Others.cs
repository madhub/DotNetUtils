using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CSharp8Features;
internal class Others
{
    public void ThrowHelpers(string notNull, int value)
    {
        // annotated with [[CallerArgumentExpression] and [DoesNotReturn] as necessary
        ArgumentNullException.ThrowIfNull(notNull);
        ArgumentOutOfRangeException.ThrowIfLessThan(value, 3);
        ArgumentException.ThrowIfNullOrWhiteSpace(notNull);
    }
    public async Task TimeProviderAbstraction()
    {
        // tests can use https://www.nuget.org/packages/Microsoft.Extensions.TimeProvider.Testing
        var timeProvider = TimeProvider.System;
        var start = timeProvider.GetTimestamp();
        await Task.Delay(TimeSpan.FromSeconds(30), timeProvider);
        var now = timeProvider.GetLocalNow();
        now = timeProvider.GetUtcNow();
        var elapsed = timeProvider.GetElapsedTime(start);
    }
    public void DeconstructDateTime()
    {
        var now = DateTime.Now;
        var (date, time) = now;
        var (year, month, day) = date;
        var (hour, minute, second) = time;
    }
    public void RandomMethods(Card[] cards)
    {
        // draws with replacement -- don't use this for most card games!
        var drawThree = Random.Shared.GetItems(cards, 3);

        // new method to shuffle in-place
        Random.Shared.Shuffle(cards);
        drawThree = cards.Take(3).ToArray();
    }
    public void FrozenTypes()
    {
        // "Frozen" collections are immutable and thread-safe and optimized for reading
        var frozenDictionary = new Dictionary<string, int> { { "one", 1 }, { "two", 2 } }.ToFrozenDictionary();
        var frozenSet = frozenDictionary.Keys.ToFrozenSet();
    }
    public void FasterReflection()
    {
        // invoke private DateTime(ulong) constructor
        var constructorInfo = typeof(DateTime).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, [typeof(ulong)])!;
        var constructorInvoker = ConstructorInvoker.Create(constructorInfo); // cache this
        var dateTime = (DateTime)constructorInvoker.Invoke(((ulong)DateTime.UtcNow.Ticks) | 0x4000_0000_0000_0000UL);

        // invoke private DateTime.ValidateLeapSecond() method
        var methodInfo = dateTime.GetType().GetMethod("ValidateLeapSecond", BindingFlags.NonPublic | BindingFlags.Instance)!;
        var methodInvoker = MethodInvoker.Create(methodInfo);
        methodInvoker.Invoke(dateTime);
    }
    public sealed record class Card(int Suit, int Value);

    public void Examples()
    {
        var dictionary = new Dictionary<string, int>();
        var readOnlyDictionary = dictionary.AsReadOnly();

        IList<string> list = new List<string>();
        var readOnlyList = list.AsReadOnly();

        var orderedList = list.Order();
        var orderedList2 = list.Order(StringComparer.OrdinalIgnoreCase);

        var micros = DateTime.Now.Microsecond;

        using var stream = new MemoryStream();
        Span<byte> buffer = stackalloc byte[100];
        stream.ReadExactly(buffer);

        var bytesRead = stream.ReadAtLeast(buffer, minimumBytes: 10, throwOnEndOfStream: true);

        bytesRead = stream.ReadAtLeastAsync(new byte[100], minimumBytes: 10, throwOnEndOfStream: false).Result; // example only; never use .Result
    }

}

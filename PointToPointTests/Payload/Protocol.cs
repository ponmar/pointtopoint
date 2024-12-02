namespace PointToPointTests.Payload;

public record PayloadForTest(int Value, string Text);

// Note: some payload serializers requires parameterless constructors
public class PayloadWithParameterlessConstructorForTest
{
    public int Value { get; set; }
    public string? Text { get; set; }
}

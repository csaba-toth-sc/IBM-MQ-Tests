using FluentAssertions;
using IBM.XMS;
using mqTests2.Contexts;
using mqTests2.Helpers;
using TechTalk.SpecFlow;

namespace mqTests2.Bindings;

[Binding]
public class BasicFeatureSteps
{
    private readonly MqContext _mqContext;

    public BasicFeatureSteps(MqContext mqContext)
    {
        _mqContext = mqContext;
    }

    [Given(@"I send message with the following details")]
    [When(@"I send message with the following details")]
    public void WhenISendMessageWithTheFollowingDetails(Table table)
    {
        // Extract test data from Feature file
        var row = table.Rows[0].ToDictionary(r => r.Key, r => r.Value);

        // Store them in context file
        _mqContext.QueueName = row["queueName"];
        _mqContext.MsgType = row["msgType"];
        _mqContext.MsgPriority = row["msgPriority"] != "" ? Convert.ToInt32(row["msgPriority"]) : null;
        _mqContext.MsgCorrelationId =
            Guid.TryParse(row["msgCorrelationId"], out var msgCorrelationId) ? msgCorrelationId : null;
        _mqContext.MsgStringPropertyType = row["msgStringPropertyType"] != "" ? row["msgStringPropertyType"] : null;
        _mqContext.MsgStringPropertyValue = row["msgStringPropertyValue"] != "" ? row["msgStringPropertyValue"] : null;

        // Store message content in context based on its type
        switch (row["msgType"])
        {
            case "text":
                _mqContext.MsgContentText = row["msgContent"];
                break;

            case "binary":
                var fileName = row["msgContent"];
                var binaryContent = File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, fileName));
                _mqContext.MsgContentBinary = binaryContent;
                break;
        }

        MqHelper.SendMessage(
            _mqContext.QueueName,
            _mqContext.MsgType,
            content: _mqContext.MsgType == "text" ? _mqContext.MsgContentText : _mqContext.MsgContentBinary,
            priority: _mqContext.MsgPriority,
            correlationId: _mqContext.MsgCorrelationId,
            stringPropertyType: _mqContext.MsgStringPropertyType,
            stringPropertyValue: _mqContext.MsgStringPropertyValue
        );
    }


    [Then(@"I should receive a message with the same details")]
    public void ThenIShouldReceiveAMessageWithTheSameDetails()
    {
        var receivedMessage = MqHelper.ReceiveMessage(_mqContext.QueueName, null, null);

        // Validate the received message content based on the type of it
        switch (_mqContext.MsgType)
        {
            case "text" when receivedMessage is ITextMessage textMessage:
                _mqContext.MsgContentText.Should().Be(textMessage.Text, "Message content does not match.");
                break;

            case "binary" when receivedMessage is IBytesMessage bytesMessage:
            {
                var receivedBytes = new byte[bytesMessage.BodyLength];
                bytesMessage.ReadBytes(receivedBytes);
                _mqContext.MsgContentBinary.Should().Equal(receivedBytes);
                break;
            }
        }

        // Validate correlation ID
        if (_mqContext.MsgCorrelationId.HasValue)
        {
            switch (_mqContext.MsgType)
            {
                case "text" when receivedMessage is ITextMessage textMessage:
                    _mqContext.MsgCorrelationId.Should().Be(
                        textMessage.JMSCorrelationID, "Message correlation ID does not match."
                    );
                    break;

                case "binary" when receivedMessage is IBytesMessage bytesMessage:
                    _mqContext.MsgCorrelationId.Should().Be(
                        bytesMessage.JMSCorrelationID, "Message correlation ID does not match."
                    );
                    break;
            }
        }

        // Validate priority
        if (_mqContext.MsgPriority.HasValue)
        {
            switch (_mqContext.MsgType)
            {
                case "text" when receivedMessage is ITextMessage textMessage:
                    _mqContext.MsgPriority.Should().Be(
                        textMessage.JMSPriority, "Message priority does not match."
                    );
                    break;

                case "binary" when receivedMessage is IBytesMessage bytesMessage:
                    _mqContext.MsgPriority.Should().Be(
                        bytesMessage.JMSPriority, "Message priority does not match."
                    );
                    break;
            }
        }
        
        // Validate string properties
        if (_mqContext is { MsgStringPropertyType: not null, MsgStringPropertyValue: not null })
        {
            switch (_mqContext.MsgType)
            {
                case "text" when receivedMessage is ITextMessage textMessage:
                    var actualTextStringPropertyValue =
                        textMessage.GetStringProperty(_mqContext.MsgStringPropertyType);
                    _mqContext.MsgStringPropertyValue.Should().Be(
                        actualTextStringPropertyValue, "Message priority does not match."
                    );
                    break;

                case "binary" when receivedMessage is IBytesMessage bytesMessage:
                    var actualBytesStringPropertyValue =
                        bytesMessage.GetStringProperty(_mqContext.MsgStringPropertyType);
                    _mqContext.MsgStringPropertyValue.Should().Be(
                        actualBytesStringPropertyValue, "Message priority does not match."
                    );
                    break;
            }
        }
    }
}
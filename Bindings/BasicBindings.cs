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
                // var fileName = row["msgContent"];
                var msgContent = row["msgContent"];
                var binaryContent = Convert.FromBase64String(msgContent);
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

    [Then(@"I should receive the following message")]
    public void ThenIShouldReceiveTheFollowingMessage(Table table)
    {
        // Extract test data from Feature file
        var row = table.Rows[0].ToDictionary(r => r.Key, r => r.Value);
        
        // Store them in context file
        var queueName = row["queueName"];
        var msgType = row["msgType"];
        int? msgPriority = row["msgPriority"] != "" ? Convert.ToInt32(row["msgPriority"]) : null;
        Guid? msgCorrelationId = Guid.TryParse(row["msgCorrelationId"], out var parsedMsgCorrelationId) ? parsedMsgCorrelationId : null;
        var msgStringPropertyType = row["msgStringPropertyType"] != "" ? row["msgStringPropertyType"] : null;
        var msgStringPropertyValue = row["msgStringPropertyValue"] != "" ? row["msgStringPropertyValue"] : null;
        var msgContent = row["msgContent"];
        
        var receivedMessage = MqHelper.ReceiveMessage(queueName, null, null);

        // Validate the received message content based on the type of it
        switch (msgType)
        {
            case "text" when receivedMessage is ITextMessage textMessage:
                msgContent.Should().Be(textMessage.Text, "Message content does not match.");
                break;

            case "binary" when receivedMessage is IBytesMessage bytesMessage:
            {
                var receivedBytes = new byte[bytesMessage.BodyLength];
                bytesMessage.ReadBytes(receivedBytes);
                var receivedBytesInBase64 = Convert.ToBase64String(receivedBytes);
                receivedBytesInBase64.Should().Be(msgContent, "Message content does not match.");
                break;
            }
        }

        // Validate correlation ID
        if (msgCorrelationId.HasValue)
        {
            switch (msgType)
            {
                case "text" when receivedMessage is ITextMessage textMessage:
                    msgCorrelationId.Should().Be(
                        textMessage.JMSCorrelationID, "Message correlation ID does not match."
                    );
                    break;

                case "binary" when receivedMessage is IBytesMessage bytesMessage:
                    msgCorrelationId.Should().Be(
                        bytesMessage.JMSCorrelationID, "Message correlation ID does not match."
                    );
                    break;
            }
        }

        // Validate priority
        if (msgPriority.HasValue)
        {
            switch (msgType)
            {
                case "text" when receivedMessage is ITextMessage textMessage:
                    msgPriority.Should().Be(
                        textMessage.JMSPriority, "Message priority does not match."
                    );
                    break;

                case "binary" when receivedMessage is IBytesMessage bytesMessage:
                    msgPriority.Should().Be(
                        bytesMessage.JMSPriority, "Message priority does not match."
                    );
                    break;
            }
        }
        
        // Validate string properties
        if (msgStringPropertyType != null && msgStringPropertyValue != null)
        {
            switch (msgType)
            {
                case "text" when receivedMessage is ITextMessage textMessage:
                    var actualTextStringPropertyValue =
                        textMessage.GetStringProperty(msgStringPropertyType);
                    msgStringPropertyValue.Should().Be(
                        actualTextStringPropertyValue, "Message priority does not match."
                    );
                    break;

                case "binary" when receivedMessage is IBytesMessage bytesMessage:
                    var actualBytesStringPropertyValue =
                        bytesMessage.GetStringProperty(msgStringPropertyType);
                    msgStringPropertyValue.Should().Be(
                        actualBytesStringPropertyValue, "Message priority does not match."
                    );
                    break;
            }
        }
    }
}
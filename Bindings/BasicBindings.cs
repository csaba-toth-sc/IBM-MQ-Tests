using FluentAssertions;
using IBM.XMS;
using mqTests2.Contexts;
using mqTests2.Helpers;
using TechTalk.SpecFlow;

namespace mqTests2.Bindings;

[Binding]
public class BasicFeatureSteps(MqContext mqContext)
{
    [Given(@"I send message with the following details")]
    [When(@"I send message with the following details")]
    public void WhenISendMessageWithTheFollowingDetails(Table table)
    {
        // Extract test data from Feature file
        var row = table.Rows[0].ToDictionary(r => r.Key, r => r.Value);

        // Store them in context file
        mqContext.QueueName = row["queueName"];
        mqContext.MsgType = row["msgType"];
        mqContext.MsgPriority = row["msgPriority"] != "" ? Convert.ToInt32(row["msgPriority"]) : null;
        mqContext.MsgCorrelationId =
            Guid.TryParse(row["msgCorrelationId"], out var msgCorrelationId) ? msgCorrelationId : null;
        mqContext.MsgStringPropertyType = row["msgStringPropertyType"] != "" ? row["msgStringPropertyType"] : null;
        mqContext.MsgStringPropertyValue = row["msgStringPropertyValue"] != "" ? row["msgStringPropertyValue"] : null;

        // Store message content in context based on its type
        switch (row["msgType"])
        {
            case "text":
                mqContext.MsgContentText = row["msgContent"];
                break;

            case "binary":
                var msgContent = TestConstants.TestDataDictionary[row["msgContent"]];
                var binaryContent = Convert.FromBase64String(msgContent);
                mqContext.MsgContentBinary = binaryContent;
                break;
            
            default:
                throw new ArgumentException("Unknown message type");
        }

        MqHelper.SendMessage(
            mqContext.QueueName,
            mqContext.MsgType,
            content: mqContext.MsgType == "text" ? mqContext.MsgContentText : mqContext.MsgContentBinary,
            priority: mqContext.MsgPriority,
            correlationId: mqContext.MsgCorrelationId,
            stringPropertyType: mqContext.MsgStringPropertyType,
            stringPropertyValue: mqContext.MsgStringPropertyValue
        );
    }

    [Then(@"I should receive the following message")]
    public static void ThenIShouldReceiveTheFollowingMessage(Table table)
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
                msgContent = TestConstants.TestDataDictionary[msgContent];
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
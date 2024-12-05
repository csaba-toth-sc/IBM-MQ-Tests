using IBM.XMS;

namespace mqTests2.Helpers;

public static class MqHelper
{
    public static void SendMessage(
        string queueName,
        string contentType,
        object? content,
        int? priority= null,
        Guid? correlationId = null,
        string? stringPropertyType = null,
        string? stringPropertyValue = null
        )
    {
        // MQ Connection setup
        var factoryFactory = XMSFactoryFactory.GetInstance(XMSC.CT_WMQ);
        var connectionFactory = factoryFactory.CreateConnectionFactory();
    
        // Connection attributes
        connectionFactory.SetStringProperty(XMSC.WMQ_HOST_NAME, "localhost");
        connectionFactory.SetIntProperty(XMSC.WMQ_PORT, 1414);
        connectionFactory.SetStringProperty(XMSC.WMQ_CHANNEL, "DEV.APP.SVRCONN");
        connectionFactory.SetIntProperty(XMSC.WMQ_CONNECTION_MODE, XMSC.WMQ_CM_CLIENT);
        connectionFactory.SetStringProperty(XMSC.WMQ_QUEUE_MANAGER, "QM1");
        connectionFactory.SetStringProperty(XMSC.USERID, "app");
        connectionFactory.SetStringProperty(XMSC.PASSWORD, "passw0rd");
    
        using var connection = connectionFactory.CreateConnection();
        using var session = connection.CreateSession(false, AcknowledgeMode.AutoAcknowledge);
        
        // Setting up queue
        var destination = session.CreateQueue(queueName);
        using var producer = session.CreateProducer(destination);
        
        // Set priority, if given
        if (priority is > 0)
        {
            producer.Priority = priority.Value;
        }
        
        // Send the message, use switch for dynamic binary/text types handling
        switch (contentType.ToLower())
        {
            case "text" when content is string textContent:
                var textMessage = session.CreateTextMessage(textContent);
                
                // Set correlation ID, if given
                if (correlationId != null)
                {
                    textMessage.JMSCorrelationID = correlationId?.ToString();
                }
                
                // Set StringPropertyType and StringPropertyValue, if given
                if (stringPropertyType != null && stringPropertyValue != null)
                {
                    textMessage.SetStringProperty(stringPropertyType, stringPropertyValue);
                }
                
                producer.Send(textMessage);
                break;
    
            case "binary" when content is byte[] binaryContent:
                // case "binary" when content is string filePath:
                //     // Read the file as a byte array
                //     var fileBytes = File.ReadAllBytes(filePath);
                //     
                //     // Create and send a bytes message
                //     var bytesMessage = session.CreateBytesMessage();
                //     bytesMessage.WriteBytes(fileBytes);
                
                // Create and send a bytes message
                var bytesMessage = session.CreateBytesMessage();
                bytesMessage.WriteBytes(binaryContent);
            
                // Set correlation ID, if given
                if (correlationId != Guid.Empty)
                {
                    bytesMessage.JMSCorrelationID = correlationId?.ToString();
                }
                
                // Set StringPropertyType and StringPropertyValue, if given
                if (stringPropertyType != null && stringPropertyValue != null)
                {
                    bytesMessage.SetStringProperty(stringPropertyType, stringPropertyValue);
                }
                
                producer.Send(bytesMessage);
                break;
    
            default:
                throw new ArgumentException("Unsupported content type or content format. Use 'text' or 'binary'.");
        }
    }
    
    public static object ReceiveMessage(string? queueName, string? selectorType, string? selectorValue)
    {
        // MQ Connection setup
        var factoryFactory = XMSFactoryFactory.GetInstance(XMSC.CT_WMQ);
        var connectionFactory = factoryFactory.CreateConnectionFactory();
        
        // Connection attributes
        connectionFactory.SetStringProperty(XMSC.WMQ_HOST_NAME, "localhost");
        connectionFactory.SetIntProperty(XMSC.WMQ_PORT, 1414);
        connectionFactory.SetStringProperty(XMSC.WMQ_CHANNEL, "DEV.APP.SVRCONN");
        connectionFactory.SetIntProperty(XMSC.WMQ_CONNECTION_MODE, XMSC.WMQ_CM_CLIENT);
        connectionFactory.SetStringProperty(XMSC.WMQ_QUEUE_MANAGER, "QM1");
        connectionFactory.SetStringProperty(XMSC.USERID, "app");
        connectionFactory.SetStringProperty(XMSC.PASSWORD, "passw0rd");

        using var connection = connectionFactory.CreateConnection();
        using var session = connection.CreateSession(false, AcknowledgeMode.AutoAcknowledge);
        
        // Setting up queue
        var destination = session.CreateQueue(queueName);
        IMessage receivedMessage;
        
        // Check if selector type and value is present and valid, if yes use it to create a filtering consumer
        if (!string.IsNullOrEmpty(selectorType) && !string.IsNullOrEmpty(selectorValue))
        {
            List<string> selectors = [
                "JMSCorrelationID", "JMSMessageID", "JMSType", "JMSPriority", "JMSExpiration", "JMSDeliveryMode"
            ];
            
            if (selectors.Contains(selectorType))
            {
                var selector = $"{selectorType} = '{selectorValue}'";
                using var consumer = session.CreateConsumer(destination, selector);
                receivedMessage = consumer.Receive();
            }
            else
            {
                throw new ArgumentException(
                "Unsupported selector type. Use one of the following values:" +
                "\"JMSCorrelationID\", \"JMSMessageID\", \"JMSType\"," +
                "\"JMSPriority\", \"JMSExpiration\", \"JMSDeliveryMode\"");
            }
        }
        else
        {
            using var consumer = session.CreateConsumer(destination);
            receivedMessage = consumer.Receive();
        }

        connection.Start();

        // Receive the message, use switch for dynamic binary/text message types handling
        

        return receivedMessage switch
        {
            IBytesMessage bytesMessage => bytesMessage,
            ITextMessage textMessage => textMessage,
            _ => throw new Exception("Unsupported message type received.")
        };
    }
}
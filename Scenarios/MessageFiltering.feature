Feature: Message filtering in IBM MQ

    Scenario Outline: Verify text message sending and receiving

        When I send message with the following details
            | queueName   | msgType   | msgContent   | msgPriority   | msgCorrelationId   | msgStringPropertyType   | msgStringPropertyValue   |
            | <queueName> | <msgType> | <msgContent> | <msgPriority> | <msgCorrelationId> | <msgStringPropertyType> | <msgStringPropertyValue> |
        Then I should receive a message with the same details

        Examples: 
            | description           | queueName      | msgType | msgContent                            | msgPriority | msgCorrelationId                     | msgStringPropertyType | msgStringPropertyValue |
            | Text                  | DEV.QUEUE.TEXT | text    | Hello IBM MQ!                         |             |                                      |                       |                        |
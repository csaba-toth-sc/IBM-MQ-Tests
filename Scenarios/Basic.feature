Feature: Send and receive text and binary messages with different attributes in IBM MQ

    Scenario Outline: Verify text message sending and receiving with different attributes
        When I send message with the following details
          | queueName   | msgType   | msgContent   | msgPriority   | msgCorrelationId   | msgStringPropertyType   | msgStringPropertyValue   |
          | <queueName> | <msgType> | <msgContent> | <msgPriority> | <msgCorrelationId> | <msgStringPropertyType> | <msgStringPropertyValue> |
        Then I should receive a message with the same details

      Examples: 
        | description          | queueName      | msgType | msgContent    | msgPriority | msgCorrelationId                     | msgStringPropertyType | msgStringPropertyValue |
        | Text                 | DEV.QUEUE.TEXT | text    | Hello IBM MQ! |             |                                      |                       |                        |
        | Text Priority 3      | DEV.QUEUE.TEXT | text    | Hello IBM MQ! | 3           |                                      |                       |                        |
        | Text Correlation ID  | DEV.QUEUE.TEXT | text    | Hello IBM MQ! |             | 3d0c00f4-b574-48c7-9e90-590caf7c7de0 |                       |                        |
        | Text String Property | DEV.QUEUE.TEXT | text    | Hello IBM MQ! |             |                                      | Category              | Type_A                 |

    Scenario Outline: Verify binary message sending and receiving with different attributes
        When I send message with the following details
          | queueName   | msgType   | msgContent   | msgPriority   | msgCorrelationId   | msgStringPropertyType   | msgStringPropertyValue   |
          | <queueName> | <msgType> | <msgContent> | <msgPriority> | <msgCorrelationId> | <msgStringPropertyType> | <msgStringPropertyValue> |
        Then I should receive a message with the same details

        Examples: 
          | description            | queueName     | msgType | msgContent                            | msgPriority | msgCorrelationId                     | msgStringPropertyType | msgStringPropertyValue |
          | Binary                 | DEV.QUEUE.IMG | binary  | pexels-karolina-grabowska-4389667.jpg |             |                                      |                       |                        |
          | Binary Priority 5      | DEV.QUEUE.IMG | binary  | pexels-karolina-grabowska-4389667.jpg | 5           |                                      |                       |                        |
          | Binary Correlation ID  | DEV.QUEUE.IMG | binary  | pexels-karolina-grabowska-4389667.jpg |             | 0986707f-4173-4967-8ee2-aaf210e92f68 |                       |                        |
          | Binary String Property | DEV.QUEUE.IMG | binary  | pexels-karolina-grabowska-4389667.jpg |             |                                      | Category              | Type_B                 |
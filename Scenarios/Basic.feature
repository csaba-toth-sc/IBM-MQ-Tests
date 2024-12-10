Feature: Send and receive text and binary messages with different attributes in IBM MQ

    Scenario Outline: Verify text message sending and receiving with different attributes
        When I send message with the following details
          | queueName   | msgType   | msgPriority   | msgCorrelationId   | msgStringPropertyType   | msgStringPropertyValue   | msgContent   |
          | <queueName> | <msgType> | <msgPriority> | <msgCorrelationId> | <msgStringPropertyType> | <msgStringPropertyValue> | <msgContent> |
        Then I should receive the following message
          | queueName   | msgType   | msgPriority   | msgCorrelationId   | msgStringPropertyType   | msgStringPropertyValue   | msgContent   |
          | <queueName> | <msgType> | <msgPriority> | <msgCorrelationId> | <msgStringPropertyType> | <msgStringPropertyValue> | <msgContent> |

      Examples: 
        | description          | queueName      | msgType | msgPriority | msgCorrelationId                     | msgStringPropertyType | msgStringPropertyValue | msgContent    |
        | Text                 | DEV.QUEUE.TEXT | text    |             |                                      |                       |                        | Hello IBM MQ! |
        | Text Priority 3      | DEV.QUEUE.TEXT | text    | 3           |                                      |                       |                        | Hello IBM MQ! |
        | Text Correlation ID  | DEV.QUEUE.TEXT | text    |             | 3d0c00f4-b574-48c7-9e90-590caf7c7de0 |                       |                        | Hello IBM MQ! |
        | Text String Property | DEV.QUEUE.TEXT | text    |             |                                      | Category              | Type_A                 | Hello IBM MQ! |

    Scenario Outline: Verify binary message sending and receiving with different attributes
        When I send message with the following details
          | queueName   | msgType   | msgPriority   | msgCorrelationId   | msgStringPropertyType   | msgStringPropertyValue   | msgContent   |
          | <queueName> | <msgType> | <msgPriority> | <msgCorrelationId> | <msgStringPropertyType> | <msgStringPropertyValue> | <msgContent> |
        Then I should receive the following message
          | queueName   | msgType   | msgPriority   | msgCorrelationId   | msgStringPropertyType   | msgStringPropertyValue   | msgContent   |
          | <queueName> | <msgType> | <msgPriority> | <msgCorrelationId> | <msgStringPropertyType> | <msgStringPropertyValue> | <msgContent> |

        Examples: 
          | description            | queueName     | msgType | msgPriority | msgCorrelationId                     | msgStringPropertyType | msgStringPropertyValue | msgContent     |
          | Binary                 | DEV.QUEUE.IMG | binary  |             |                                      |                       |                        | BinaryBase64_1 |
          | Binary Priority 5      | DEV.QUEUE.IMG | binary  | 5           |                                      |                       |                        | BinaryBase64_1 |
          | Binary Correlation ID  | DEV.QUEUE.IMG | binary  |             | 0986707f-4173-4967-8ee2-aaf210e92f68 |                       |                        | BinaryBase64_1 |
          | Binary String Property | DEV.QUEUE.IMG | binary  |             |                                      | Category              | Type_B                 | BinaryBase64_1 |
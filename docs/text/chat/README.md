# Extended chat

The chat message and client.

In `Trivial.Text` [namespace](../) of `Trivial.Messages.dll` [library](../../).

## Message models

The class `ExtendedChatMessage` is used to transfer with participators, message content, state
and other information. It reserves capabilities to extend further attachments and functions.

The message content is based on plain text or formatted text.
Please check `MessageFormat` property before reading content string by `Message` property.
Other common-used properties are `Priority`, `ModificationKind` and `Sender`.
Some of these are read-only after initializing.

To extend the message, use property `MessageType` as the identifier of the message type,
and set struct data into property `Info` for presenting and background states.

All `ExtendedChatMessage` instances should belong to an `ExtendedChatConversation` instance.
The conversation is about all messages with the same participators and a topic thread.
The methods to send and receive message are powered by the client bound with the conversation.
Each user can have one or more clients to manage the conversations.

The conversation can be private or public. It requires a source to bind.
The source can be a recipient, a chat group, a post or other kind of resource to chat.
So the source is the object or place with topics to keep the conversation.
Its related information is the profile of conversation.

## Chat client

The client is used to implement how to send and receive messages of the specific user.
Developer should derive the abstract class `BaseExtendedChatClient` to implement the logic.

The instance is the client of a real chat service to the user.
The chat service can be online or local. It can also be the bottom layer to provide data exchange.
The client access the chat service to provide the functionalities in server-side or client-side.
So the client can also be the chat service to service it client via network.

The client manage all conversations with the same chat service
so the count of the conversation may be one or more. A user may has more than one clients
if the app or service is connecting with multiple chat services so there are multiple clients.

A conversation is created with the source and the client instance.
to chat with other user, bot or other kind of participator,
All accessing including sending and receiving messages in the conversation will invoke the client.

You need implement the sending handler to return an instance of `ExtendedChatMessageSendResult`.
The `ExtendedChatMessageContext` instance is the context of a message sending.

## Implementations

- `ResponsiveChatClient` is a client implementation for chat bot service. See [AI chat task](../../tasks/chat/) for details.

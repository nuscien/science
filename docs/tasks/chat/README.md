# Chat Command Guidance

The numerals, mathematics symbols and number utilities.

In `Trivial.Tasks` [namespace](../) of `Trivial.Messages.dll` library.

## AI client provider

`ResponsiveChatClient` is the class derived from `BaseExtendedChatClient` to provide a way to chat with AI.
It works in turn-based mode which means the user can send sub-sequent message after receiving response.
The client requires a provider to create a connection to an AI service, send questions and recieving answers.

> [Click here](../../text/chat/) to get details about `BaseExtendedChatClient`.

`BaseResponsiveChatProvider` is the abstract class of the client provider.
Derive and implement this base class to fill the logic.
`ResponsiveChatContext` is the context for a message sending.

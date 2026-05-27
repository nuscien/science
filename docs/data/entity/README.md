# Resource Entity

The info instance about resource entity used.

In `Trivial.Data` [namespace](../) of `Trivial.Messages.dll` [library](../../).

## Common Concept of Entity

`BaseAccountEntityInfo` is the base class of resource entity and defines following common properties.

- `Id` (`$id`) _string_: The unique identifier of the resource.
- `State` (`state`) _enum_(`ResourceEntityStates`): The state of the resource entity. Including one of following states.
  - `Deleted = 0`: The entity does not exist or is removed.
  - `Recycle = 1`: The entity is in trash bin. It means it is being considered for deletion but has not yet.
  - `Placehoder = 2`: This is a placeholder. It means the entity is not ready but is planed to create at the right time.
  - `Progress = 3`: The entity is in a progress to create or is initializing.
  - `Request = 4`: The entity is applying for approval and its fields are already.
  - `Draft = 5`: The entity is a draft. Its fields are not fully completed.
  - `Publishing = 6`: The entity is in progress of publishing and may take some time. There are no further actions that require approval.
  - `Normal = 7`: The entity is in service (is published).
- `CreationTime` (`created`) _date time_: The date time when the entity creates.
- `LastModificationTime` (`updated`) _date time_: The date time when the entity updates recently.
- `InitializationTime` _date time_: The date time of this instance initialization.
- `LastSavingStatus`: The latest saving status of this entity.
- `Supertype` (`supertype`) _string_: The supertype of the resource, e.g. message, account, article, task, etc.
- `ResourceType` (`$type`) _string_: The type of the resource.
- `RevisionId` (`rev`) _string_ (_protected_): The hash value of the entity revision. It will change on entity updating.
- `DisplayName` _string_ (_optional_): The display name of this entity. Only the entity with a display name contains this property.
- `ConfigInfo` (`config`) _JSON_ (_protected optional_): The configuration data or metadata. Only the entity with a configuration or metadata contains this property.

Note:

- About __entity state__:

  Entity instance is about the information of the resource no matter if it is alive. The state (property `State`) is a flag about if the entity is alive or not.

  The full lifecycle of a resource entity in order may contains (not applicable to all):

  1. Preparing: `Placeholder = 2`, `Progress = 3`, `Draft = 5`, `Request = 4`, `Publishing = 6`;
  2. In-service: `Normal = 7`;
  3. End: `Recycle = 1`, `Deleted = 0`.

- About __resource type__:

  The resource entity is about any kind of asset, account, content or data. The property `Supertype` is such top-level classification of the resource.

  And under `Supertype`, it subdivides into specific classifications, such as blog, note and wiki in content. It is the property `ResourceType`.

  That means `Supertype` and `ResourceType` are two-level classification for the resource.
  So in above example, `ResourceType` may returns `blog`, `note` and `wiki`; but `Supertype` of all of these may returns `content`.

## Hierarchy

Following are built-in classes derived from `BaseAccountEntityInfo`.

- `Trivial.Users.BaseAccountEntityInfo` (_abstract_): The base entity of users, groups and other kind of authorized entities.
  - `Trivial.Users.BaseUserItemInfo` (_abstract_)
    - `Trivial.Users.UserItemInfo`: User account.
    - `Trivial.Devices.AuthDeviceItemInfo`: The device which can be verified through identity authentication.
    - `Trivial.Tasks.ServiceAccountItemInfo`: The service account used by a back-end service, a local app or an automation, which requires to run as an identity and related roles to access resources under permission scope.
    - `Trivial.Users.BotAccountItemInfo`: Bot account, including AI agent.
    - `Trivial.Users.OrgAccountItemInfo`: The account of companies, schools, governments, institutions or other kind of organizations. It may also be a department of a large organization. It can be controled by its administrators configured.
    - `Trivial.Security.AgentAccountItemInfo`: The agent or proxy of another account entity.
  - `Trivial.Users.BaseUserGroupItemInfo`: The base entity of communication group, role, team and interest group.
  - `Trivial.Users.UnknownAccountEntityInfo`: Unknown account.
- `Trivial.Data.RelatedResourceEntityInfo` (_abstract_) and `Trivial.Data.RelatedResourceEntityInfo<TOwner>` (_abstract_): The relationship information entity. 
- `Trivial.Data.RelatedResourceEntityInfo<TOwner, TTarget>` (_abstract_)
    - `Trivial.Security.ResourcePermissionItemInfo<TOwner>`: The permission set of a specific account associated to a resource.
- `Trivial.Users.UserItemRelatedInfo` and `Trivial.Users.UserItemRelatedInfo<T>` (_abstract_): The base related entity of assets or data associated to an account.
    - `Trivial.Security.UserItemCredentialNameInfo`: The credential name of a user account item.
    - `Trivial.Security.UserItemCredentialKeyInfo`: The credential key of a user account item.
  - `Trivial.Text.ExtendedChatMessage`: The chat message record.
  - `Trivial.Text.MessageSenderMethodInfo`
  - `Trivial.Data.BasePropertyResourceEntityInfo<TOwner>`: The base entity of property associated to a resource entity.
    - `Trivial.Data.PropertyResourceEntityInfo<TOwner>`: The property data entity of a resource entity.
    - `Trivial.Data.MetadataResourceEntityInfo<TOwner>`: The metadata entity of a resource entity.

Developers can derive the following base class to implement the resource entity.

- `Trivial.Users.BaseUserGroupItemInfo`: A specific of communication group, role, team or interest group.
- `Trivial.Users.UserItemRelatedInfo`: The assets or data bound to an account.
- `Trivial.Data.RelatedResourceEntityInfo<TOwner>`: The assets or data bound to a resource (but not to an account).
- `Trivial.Data.RelatedResourceEntityInfo<TOwner, TTarget>`:The assets or data bound to 2 resources. One is prime and another is related.

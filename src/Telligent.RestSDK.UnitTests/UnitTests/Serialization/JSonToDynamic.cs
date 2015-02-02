using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Telligent.Evolution.RestSDK.Implementations;
using Telligent.Evolution.RestSDK.Json;

namespace Telligent.RestSDK.IntegrationTests.UnitTests.Serialization
{
    [TestFixture]
   public class JSonToDynamic
   {
       #region JSON
       private static readonly string json_paged = @"{
  ""PageSize"": 2,
  ""PageIndex"": 0,
  ""TotalCount"": 2,
  ""Users"": [
    {
      ""ContentId"": ""49fec544-6df7-4a82-872b-f8be586d5e9e"",
      ""ContentTypeId"": ""9f5a6721-639d-4e1d-ab6a-ce63b7750f46"",
      ""Content"": {
        ""CreatedByUser"": {
          ""AvatarUrl"": ""avatarurl"",
          ""DisplayName"": ""displayname"",
          ""ProfileUrl"": ""profileurl"",
          ""Username"": ""username"",
          ""CurrentStatus"": {
            ""Author"": {
              ""AvatarUrl"": ""avatarurl"",
              ""DisplayName"": ""displayname"",
              ""ProfileUrl"": ""profileurl"",
              ""Username"": ""username"",
              ""CurrentStatus"": null,
              ""Id"": null
            },
            ""Body"": ""body"",
            ""CreatedDate"": ""2012-01-04T00:00:00"",
            ""ReplyCount"": 6,
            ""Group"": {
              ""AvatarUrl"": ""avatarurl"",
              ""DateCreated"": null,
              ""Description"": ""description"",
              ""Key"": ""key"",
              ""Name"": ""name"",
              ""Url"": ""url"",
              ""TotalMembers"": null,
              ""HasGroups"": true,
              ""GroupCount"": null,
              ""GroupType"": ""grouptype"",
              ""EnableGroupMessages"": true,
              ""EnableContact"": true,
              ""SearchUniqueId"": ""searchuniqueid"",
              ""IsEnabled"": true,
              ""ExtendedAttributes"": [],
              ""ContainerId"": ""fe65240b-044c-4292-9946-f10e0361ecff"",
              ""ContainerTypeId"": ""23b05a61-c3e5-4451-90d9-bfa00453bce4"",
              ""Container"": null,
              ""Id"": null
            },
            ""AttachedUrl"": ""attachedurl"",
            ""HasReplies"": true,
            ""ContentId"": ""6cdcf200-3bad-476d-af45-97b52545c337"",
            ""ContentType"": ""56f1a3ec-14bb-45c6-949f-ee7776d68c78"",
            ""Url"": ""url"",
            ""Content"": {
              ""CreatedByUser"": null,
              ""ContentId"": ""9ba1ec43-dc34-4e27-a579-4a0855144e2f"",
              ""ContentTypeId"": ""e3715662-2528-4ba1-84a7-bfcd9d548f80"",
              ""CreatedDate"": ""2012-05-02T00:00:00"",
              ""HtmlName"": ""htmlname"",
              ""HtmlDescription"": ""htmldescription"",
              ""Url"": ""url"",
              ""AvatarUrl"": ""avatarurl"",
              ""Application"": null
            },
            ""Id"": ""6cdcf200-3bad-476d-af45-97b52545c337""
          },
          ""Id"": 25
        },
        ""ContentId"": ""b04540ec-eb38-4fa1-b7bb-e3fde05401b4"",
        ""ContentTypeId"": ""44df201a-07e6-42eb-96c7-8ce7ce35aab6"",
        ""CreatedDate"": ""2012-06-04T00:00:00"",
        ""HtmlName"": ""htmlname"",
        ""HtmlDescription"": ""htmldescription"",
        ""Url"": ""url"",
        ""AvatarUrl"": ""avatarurl"",
        ""Application"": {
          ""ApplicationId"": ""49fec544-6df7-4a82-872b-f8be586d5e9e"",
          ""ApplicationTypeId"": ""9f5a6721-639d-4e1d-ab6a-ce63b7750f46"",
          ""HtmlName"": ""htmlname"",
          ""HtmlDescription"": ""htmldescription"",
          ""Url"": ""url"",
          ""AvatarUrl"": ""avatarurl"",
          ""Container"": {
            ""ContainerId"": ""fe65240b-044c-4292-9946-f10e0361ecff"",
            ""ContainerTypeId"": ""2d525bad-c4df-470d-a193-a1c6d66e5c3e"",
            ""HtmlName"": ""htmlname"",
            ""Url"": ""url"",
            ""AvatarUrl"": ""avatarurl""
          }
        }
      },
      ""AllowSitePartnersToContact"": true,
      ""AllowSiteToContact"": true,
      ""AvatarUrl"": ""avatarurl"",
      ""Bio"": ""bio"",
      ""Birthday"": ""2011-05-28T00:00:00"",
      ""CurrentStatus"": {
        ""Author"": {
          ""AvatarUrl"": ""avatarurl"",
          ""DisplayName"": ""displayname"",
          ""ProfileUrl"": ""profileurl"",
          ""Username"": ""username"",
          ""CurrentStatus"": {
            ""Author"": {
              ""AvatarUrl"": ""avatarurl"",
              ""DisplayName"": ""displayname"",
              ""ProfileUrl"": ""profileurl"",
              ""Username"": ""username"",
              ""CurrentStatus"": null,
              ""Id"": null
            },
            ""Body"": ""body"",
            ""CreatedDate"": ""2011-12-09T00:00:00"",
            ""ReplyCount"": 8,
            ""Group"": {
              ""AvatarUrl"": ""avatarurl"",
              ""DateCreated"": null,
              ""Description"": ""description"",
              ""Key"": ""key"",
              ""Name"": ""name"",
              ""Url"": ""url"",
              ""TotalMembers"": null,
              ""HasGroups"": true,
              ""GroupCount"": null,
              ""GroupType"": ""grouptype"",
              ""EnableGroupMessages"": true,
              ""EnableContact"": true,
              ""SearchUniqueId"": ""searchuniqueid"",
              ""IsEnabled"": true,
              ""ExtendedAttributes"": [],
              ""ContainerId"": ""bd519379-b61f-4e2f-b0f9-c0f28c5cceb1"",
              ""ContainerTypeId"": ""23b05a61-c3e5-4451-90d9-bfa00453bce4"",
              ""Container"": null,
              ""Id"": null
            },
            ""AttachedUrl"": ""attachedurl"",
            ""HasReplies"": true,
            ""ContentId"": ""44df201a-07e6-42eb-96c7-8ce7ce35aab6"",
            ""ContentType"": ""56f1a3ec-14bb-45c6-949f-ee7776d68c78"",
            ""Url"": ""url"",
            ""Content"": {
              ""CreatedByUser"": null,
              ""ContentId"": ""6cdcf200-3bad-476d-af45-97b52545c337"",
              ""ContentTypeId"": ""b04540ec-eb38-4fa1-b7bb-e3fde05401b4"",
              ""CreatedDate"": ""2012-03-29T00:00:00"",
              ""HtmlName"": ""htmlname"",
              ""HtmlDescription"": ""htmldescription"",
              ""Url"": ""url"",
              ""AvatarUrl"": ""avatarurl"",
              ""Application"": null
            },
            ""Id"": ""44df201a-07e6-42eb-96c7-8ce7ce35aab6""
          },
          ""Id"": 4
        },
        ""Body"": ""body"",
        ""CreatedDate"": ""2012-04-19T00:00:00"",
        ""ReplyCount"": 7,
        ""Group"": {
          ""AvatarUrl"": ""avatarurl"",
          ""DateCreated"": ""2012-01-16T00:00:00"",
          ""Description"": ""description"",
          ""Key"": ""key"",
          ""Name"": ""name"",
          ""ParentGroupId"": 27,
          ""Url"": ""url"",
          ""TotalMembers"": 32,
          ""HasGroups"": true,
          ""GroupCount"": 52,
          ""GroupType"": ""grouptype"",
          ""EnableGroupMessages"": true,
          ""EnableContact"": true,
          ""SearchUniqueId"": ""searchuniqueid"",
          ""IsEnabled"": true,
          ""ExtendedAttributes"": [
            {
              ""Key"": ""key"",
              ""Value"": ""value""
            },
            {
              ""Key"": ""key"",
              ""Value"": ""value""
            }
          ],
          ""ContainerId"": ""49fec544-6df7-4a82-872b-f8be586d5e9e"",
          ""ContainerTypeId"": ""23b05a61-c3e5-4451-90d9-bfa00453bce4"",
          ""Container"": {
            ""ContainerId"": ""9f5a6721-639d-4e1d-ab6a-ce63b7750f46"",
            ""ContainerTypeId"": ""fe65240b-044c-4292-9946-f10e0361ecff"",
            ""HtmlName"": ""htmlname"",
            ""Url"": ""url"",
            ""AvatarUrl"": ""avatarurl""
          },
          ""Id"": 19
        },
        ""AttachedUrl"": ""attachedurl"",
        ""HasReplies"": true,
        ""ContentId"": ""2d525bad-c4df-470d-a193-a1c6d66e5c3e"",
        ""ContentType"": ""56f1a3ec-14bb-45c6-949f-ee7776d68c78"",
        ""Url"": ""url"",
        ""Content"": {
          ""CreatedByUser"": {
            ""AvatarUrl"": ""avatarurl"",
            ""DisplayName"": ""displayname"",
            ""ProfileUrl"": ""profileurl"",
            ""Username"": ""username"",
            ""CurrentStatus"": {
              ""Author"": null,
              ""Body"": ""body"",
              ""CreatedDate"": null,
              ""ReplyCount"": 112,
              ""Group"": null,
              ""AttachedUrl"": ""attachedurl"",
              ""HasReplies"": null,
              ""ContentId"": ""9ba1ec43-dc34-4e27-a579-4a0855144e2f"",
              ""ContentType"": ""56f1a3ec-14bb-45c6-949f-ee7776d68c78"",
              ""Url"": ""url"",
              ""Content"": null,
              ""Id"": ""9ba1ec43-dc34-4e27-a579-4a0855144e2f""
            },
            ""Id"": 61
          },
          ""ContentId"": ""6cdcf200-3bad-476d-af45-97b52545c337"",
          ""ContentTypeId"": ""b04540ec-eb38-4fa1-b7bb-e3fde05401b4"",
          ""CreatedDate"": ""2012-01-04T00:00:00"",
          ""HtmlName"": ""htmlname"",
          ""HtmlDescription"": ""htmldescription"",
          ""Url"": ""url"",
          ""AvatarUrl"": ""avatarurl"",
          ""Application"": {
            ""ApplicationId"": ""44df201a-07e6-42eb-96c7-8ce7ce35aab6"",
            ""ApplicationTypeId"": ""49fec544-6df7-4a82-872b-f8be586d5e9e"",
            ""HtmlName"": ""htmlname"",
            ""HtmlDescription"": ""htmldescription"",
            ""Url"": ""url"",
            ""AvatarUrl"": ""avatarurl"",
            ""Container"": {
              ""ContainerId"": ""9f5a6721-639d-4e1d-ab6a-ce63b7750f46"",
              ""ContainerTypeId"": ""fe65240b-044c-4292-9946-f10e0361ecff"",
              ""HtmlName"": ""htmlname"",
              ""Url"": ""url"",
              ""AvatarUrl"": ""avatarurl""
            }
          }
        },
        ""Id"": ""2d525bad-c4df-470d-a193-a1c6d66e5c3e""
      },
      ""DateFormat"": ""dateformat"",
      ""DisplayName"": ""displayname"",
      ""ConversationContactType"": ""conversationcontacttype"",
      ""EditorType"": ""editortype"",
      ""EnableCommentNotifications"": true,
      ""EnableConversationNotifications"": true,
      ""EnableDisplayInMemberList"": true,
      ""EnableDisplayName"": true,
      ""EnableEmoticons"": true,
      ""EnableFavoriteSharing"": true,
      ""ReceiveEmails"": true,
      ""EnableHtmlEmail"": true,
      ""EnableTracking"": true,
      ""EnableUserSignatures"": true,
      ""Gender"": 0,
      ""JoinDate"": ""2012-05-02T00:00:00"",
      ""LastLoginDate"": ""2012-06-04T00:00:00"",
      ""Language"": ""language"",
      ""Location"": ""location"",
      ""Points"": 18,
      ""QualityPercentile"": 76,
      ""PostSortOrder"": 0,
      ""PrivateEmail"": ""privateemail"",
      ""ProfileUrl"": ""profileurl"",
      ""PublicEmail"": ""publicemail"",
      ""Signature"": ""signature"",
      ""TimeZone"": 0.33,
      ""TimeZoneInfo"": ""timezoneinfo"",
      ""TimeZoneId"": ""timezoneid"",
      ""Username"": ""username"",
      ""WebUrl"": ""weburl"",
      ""ProfileFields"": [
        {
          ""LocalName"": ""label"",
          ""Label"": ""label"",
          ""Value"": ""value""
        },
        {
          ""LocalName"": ""label"",
          ""Label"": ""label"",
          ""Value"": ""value""
        }
      ],
      ""AccountStatus"": ""accountstatus"",
      ""TotalPosts"": 6,
      ""RssFeeds"": [
        ""string"",
        ""string""
      ],
      ""SearchUniqueId"": ""searchuniqueid"",
      ""ExtendedAttributes"": [
        {
          ""Key"": ""key"",
          ""Value"": ""value""
        },
        {
          ""Key"": ""key"",
          ""Value"": ""value""
        }
      ],
      ""ModerationLevel"": ""moderationlevel"",
      ""Id"": 25
    },
    {
      ""ContentId"": ""bd519379-b61f-4e2f-b0f9-c0f28c5cceb1"",
      ""ContentTypeId"": ""9ba1ec43-dc34-4e27-a579-4a0855144e2f"",
      ""Content"": {
        ""CreatedByUser"": {
          ""AvatarUrl"": ""avatarurl"",
          ""DisplayName"": ""displayname"",
          ""ProfileUrl"": ""profileurl"",
          ""Username"": ""username"",
          ""CurrentStatus"": {
            ""Author"": {
              ""AvatarUrl"": ""avatarurl"",
              ""DisplayName"": ""displayname"",
              ""ProfileUrl"": ""profileurl"",
              ""Username"": ""username"",
              ""CurrentStatus"": null,
              ""Id"": null
            },
            ""Body"": ""body"",
            ""CreatedDate"": ""2011-05-28T00:00:00"",
            ""ReplyCount"": 8,
            ""Group"": {
              ""AvatarUrl"": ""avatarurl"",
              ""DateCreated"": null,
              ""Description"": ""description"",
              ""Key"": ""key"",
              ""Name"": ""name"",
              ""Url"": ""url"",
              ""TotalMembers"": null,
              ""HasGroups"": true,
              ""GroupCount"": null,
              ""GroupType"": ""grouptype"",
              ""EnableGroupMessages"": true,
              ""EnableContact"": true,
              ""SearchUniqueId"": ""searchuniqueid"",
              ""IsEnabled"": true,
              ""ExtendedAttributes"": [],
              ""ContainerId"": ""e3715662-2528-4ba1-84a7-bfcd9d548f80"",
              ""ContainerTypeId"": ""23b05a61-c3e5-4451-90d9-bfa00453bce4"",
              ""Container"": null,
              ""Id"": null
            },
            ""AttachedUrl"": ""attachedurl"",
            ""HasReplies"": true,
            ""ContentId"": ""9f5a6721-639d-4e1d-ab6a-ce63b7750f46"",
            ""ContentType"": ""56f1a3ec-14bb-45c6-949f-ee7776d68c78"",
            ""Url"": ""url"",
            ""Content"": {
              ""CreatedByUser"": null,
              ""ContentId"": ""44df201a-07e6-42eb-96c7-8ce7ce35aab6"",
              ""ContentTypeId"": ""49fec544-6df7-4a82-872b-f8be586d5e9e"",
              ""CreatedDate"": ""2011-12-09T00:00:00"",
              ""HtmlName"": ""htmlname"",
              ""HtmlDescription"": ""htmldescription"",
              ""Url"": ""url"",
              ""AvatarUrl"": ""avatarurl"",
              ""Application"": null
            },
            ""Id"": ""9f5a6721-639d-4e1d-ab6a-ce63b7750f46""
          },
          ""Id"": 4
        },
        ""ContentId"": ""fe65240b-044c-4292-9946-f10e0361ecff"",
        ""ContentTypeId"": ""2d525bad-c4df-470d-a193-a1c6d66e5c3e"",
        ""CreatedDate"": ""2012-03-29T00:00:00"",
        ""HtmlName"": ""htmlname"",
        ""HtmlDescription"": ""htmldescription"",
        ""Url"": ""url"",
        ""AvatarUrl"": ""avatarurl"",
        ""Application"": {
          ""ApplicationId"": ""bd519379-b61f-4e2f-b0f9-c0f28c5cceb1"",
          ""ApplicationTypeId"": ""9ba1ec43-dc34-4e27-a579-4a0855144e2f"",
          ""HtmlName"": ""htmlname"",
          ""HtmlDescription"": ""htmldescription"",
          ""Url"": ""url"",
          ""AvatarUrl"": ""avatarurl"",
          ""Container"": {
            ""ContainerId"": ""e3715662-2528-4ba1-84a7-bfcd9d548f80"",
            ""ContainerTypeId"": ""6cdcf200-3bad-476d-af45-97b52545c337"",
            ""HtmlName"": ""htmlname"",
            ""Url"": ""url"",
            ""AvatarUrl"": ""avatarurl""
          }
        }
      },
      ""AllowSitePartnersToContact"": true,
      ""AllowSiteToContact"": true,
      ""AvatarUrl"": ""avatarurl"",
      ""Bio"": ""bio"",
      ""Birthday"": ""2012-04-19T00:00:00"",
      ""CurrentStatus"": {
        ""Author"": {
          ""AvatarUrl"": ""avatarurl"",
          ""DisplayName"": ""displayname"",
          ""ProfileUrl"": ""profileurl"",
          ""Username"": ""username"",
          ""CurrentStatus"": {
            ""Author"": {
              ""AvatarUrl"": ""avatarurl"",
              ""DisplayName"": ""displayname"",
              ""ProfileUrl"": ""profileurl"",
              ""Username"": ""username"",
              ""CurrentStatus"": null,
              ""Id"": null
            },
            ""Body"": ""body"",
            ""CreatedDate"": ""2012-01-16T00:00:00"",
            ""ReplyCount"": 7,
            ""Group"": {
              ""AvatarUrl"": ""avatarurl"",
              ""DateCreated"": null,
              ""Description"": ""description"",
              ""Key"": ""key"",
              ""Name"": ""name"",
              ""Url"": ""url"",
              ""TotalMembers"": null,
              ""HasGroups"": true,
              ""GroupCount"": null,
              ""GroupType"": ""grouptype"",
              ""EnableGroupMessages"": true,
              ""EnableContact"": true,
              ""SearchUniqueId"": ""searchuniqueid"",
              ""IsEnabled"": true,
              ""ExtendedAttributes"": [],
              ""ContainerId"": ""b04540ec-eb38-4fa1-b7bb-e3fde05401b4"",
              ""ContainerTypeId"": ""23b05a61-c3e5-4451-90d9-bfa00453bce4"",
              ""Container"": null,
              ""Id"": null
            },
            ""AttachedUrl"": ""attachedurl"",
            ""HasReplies"": true,
            ""ContentId"": ""2d525bad-c4df-470d-a193-a1c6d66e5c3e"",
            ""ContentType"": ""56f1a3ec-14bb-45c6-949f-ee7776d68c78"",
            ""Url"": ""url"",
            ""Content"": {
              ""CreatedByUser"": null,
              ""ContentId"": ""9f5a6721-639d-4e1d-ab6a-ce63b7750f46"",
              ""ContentTypeId"": ""fe65240b-044c-4292-9946-f10e0361ecff"",
              ""CreatedDate"": ""2012-01-04T00:00:00"",
              ""HtmlName"": ""htmlname"",
              ""HtmlDescription"": ""htmldescription"",
              ""Url"": ""url"",
              ""AvatarUrl"": ""avatarurl"",
              ""Application"": null
            },
            ""Id"": ""2d525bad-c4df-470d-a193-a1c6d66e5c3e""
          },
          ""Id"": 27
        },
        ""Body"": ""body"",
        ""CreatedDate"": ""2012-05-02T00:00:00"",
        ""ReplyCount"": 32,
        ""Group"": {
          ""AvatarUrl"": ""avatarurl"",
          ""DateCreated"": ""2012-06-04T00:00:00"",
          ""Description"": ""description"",
          ""Key"": ""key"",
          ""Name"": ""name"",
          ""ParentGroupId"": 52,
          ""Url"": ""url"",
          ""TotalMembers"": 19,
          ""HasGroups"": true,
          ""GroupCount"": 112,
          ""GroupType"": ""grouptype"",
          ""EnableGroupMessages"": true,
          ""EnableContact"": true,
          ""SearchUniqueId"": ""searchuniqueid"",
          ""IsEnabled"": true,
          ""ExtendedAttributes"": [
            {
              ""Key"": ""key"",
              ""Value"": ""value""
            },
            {
              ""Key"": ""key"",
              ""Value"": ""value""
            }
          ],
          ""ContainerId"": ""bd519379-b61f-4e2f-b0f9-c0f28c5cceb1"",
          ""ContainerTypeId"": ""23b05a61-c3e5-4451-90d9-bfa00453bce4"",
          ""Container"": {
            ""ContainerId"": ""9ba1ec43-dc34-4e27-a579-4a0855144e2f"",
            ""ContainerTypeId"": ""e3715662-2528-4ba1-84a7-bfcd9d548f80"",
            ""HtmlName"": ""htmlname"",
            ""Url"": ""url"",
            ""AvatarUrl"": ""avatarurl""
          },
          ""Id"": 61
        },
        ""AttachedUrl"": ""attachedurl"",
        ""HasReplies"": true,
        ""ContentId"": ""6cdcf200-3bad-476d-af45-97b52545c337"",
        ""ContentType"": ""56f1a3ec-14bb-45c6-949f-ee7776d68c78"",
        ""Url"": ""url"",
        ""Content"": {
          ""CreatedByUser"": {
            ""AvatarUrl"": ""avatarurl"",
            ""DisplayName"": ""displayname"",
            ""ProfileUrl"": ""profileurl"",
            ""Username"": ""username"",
            ""CurrentStatus"": {
              ""Author"": null,
              ""Body"": ""body"",
              ""CreatedDate"": null,
              ""ReplyCount"": 18,
              ""Group"": null,
              ""AttachedUrl"": ""attachedurl"",
              ""HasReplies"": null,
              ""ContentId"": ""44df201a-07e6-42eb-96c7-8ce7ce35aab6"",
              ""ContentType"": ""56f1a3ec-14bb-45c6-949f-ee7776d68c78"",
              ""Url"": ""url"",
              ""Content"": null,
              ""Id"": ""44df201a-07e6-42eb-96c7-8ce7ce35aab6""
            },
            ""Id"": 76
          },
          ""ContentId"": ""9f5a6721-639d-4e1d-ab6a-ce63b7750f46"",
          ""ContentTypeId"": ""fe65240b-044c-4292-9946-f10e0361ecff"",
          ""CreatedDate"": ""2011-05-28T00:00:00"",
          ""HtmlName"": ""htmlname"",
          ""HtmlDescription"": ""htmldescription"",
          ""Url"": ""url"",
          ""AvatarUrl"": ""avatarurl"",
          ""Application"": {
            ""ApplicationId"": ""2d525bad-c4df-470d-a193-a1c6d66e5c3e"",
            ""ApplicationTypeId"": ""bd519379-b61f-4e2f-b0f9-c0f28c5cceb1"",
            ""HtmlName"": ""htmlname"",
            ""HtmlDescription"": ""htmldescription"",
            ""Url"": ""url"",
            ""AvatarUrl"": ""avatarurl"",
            ""Container"": {
              ""ContainerId"": ""9ba1ec43-dc34-4e27-a579-4a0855144e2f"",
              ""ContainerTypeId"": ""e3715662-2528-4ba1-84a7-bfcd9d548f80"",
              ""HtmlName"": ""htmlname"",
              ""Url"": ""url"",
              ""AvatarUrl"": ""avatarurl""
            }
          }
        },
        ""Id"": ""6cdcf200-3bad-476d-af45-97b52545c337""
      },
      ""DateFormat"": ""dateformat"",
      ""DisplayName"": ""displayname"",
      ""ConversationContactType"": ""conversationcontacttype"",
      ""EditorType"": ""editortype"",
      ""EnableCommentNotifications"": true,
      ""EnableConversationNotifications"": true,
      ""EnableDisplayInMemberList"": true,
      ""EnableDisplayName"": true,
      ""EnableEmoticons"": true,
      ""EnableFavoriteSharing"": true,
      ""ReceiveEmails"": true,
      ""EnableHtmlEmail"": true,
      ""EnableTracking"": true,
      ""EnableUserSignatures"": true,
      ""Gender"": 0,
      ""JoinDate"": ""2011-12-09T00:00:00"",
      ""LastLoginDate"": ""2012-03-29T00:00:00"",
      ""Language"": ""language"",
      ""Location"": ""location"",
      ""Points"": 6,
      ""QualityPercentile"": 25,
      ""PostSortOrder"": 0,
      ""PrivateEmail"": ""privateemail"",
      ""ProfileUrl"": ""profileurl"",
      ""PublicEmail"": ""publicemail"",
      ""Signature"": ""signature"",
      ""TimeZone"": 0.25,
      ""TimeZoneInfo"": ""timezoneinfo"",
      ""TimeZoneId"": ""timezoneid"",
      ""Username"": ""username"",
      ""WebUrl"": ""weburl"",
      ""ProfileFields"": [
        {
          ""LocalName"": ""label"",
          ""Label"": ""label"",
          ""Value"": ""value""
        },
        {
          ""LocalName"": ""label"",
          ""Label"": ""label"",
          ""Value"": ""value""
        }
      ],
      ""AccountStatus"": ""accountstatus"",
      ""TotalPosts"": 8,
      ""RssFeeds"": [
        ""string"",
        ""string""
      ],
      ""SearchUniqueId"": ""searchuniqueid"",
      ""ExtendedAttributes"": [
        {
          ""Key"": ""key"",
          ""Value"": ""value""
        },
        {
          ""Key"": ""key"",
          ""Value"": ""value""
        }
      ],
      ""ModerationLevel"": ""moderationlevel"",
      ""Id"": 4
    }
  ],
  ""Info"": [
    ""string"",
    ""string""
  ],
  ""Warnings"": [
    ""string"",
    ""string""
  ],
  ""Errors"": [
    ""string"",
    ""string""
  ]
}";
       #endregion

        [Test]
        public void can_deserialze_to_paged_dynamic()
        {
            var deserializer = new Deserializer();
            dynamic obj = new ExpandoObject();
            deserializer.Deserialize(obj,new JsonReader(json_paged));

            Assert.IsNotNull(obj.Users);
        }
   }
}

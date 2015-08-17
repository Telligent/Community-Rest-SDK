# Telligent Community REST SDK
###System Requirements
- Telligent Community 8.0 or higher
- .NET Framework version 4.5

##Release Notes  (v1.0.40)
While we make an effort not to break anything in new releases sometimes it is necessary in order to add features or improve existing ones. v1.0.40 introduces the following breaking changes:

1.  The SSO node of the config file should be moved under the Host node rather than its current location
2.  IUserResolver has been removed in favor of a delegate pattern.  It is easy to update your code.  Most of it can be copied and pasted into the new pattern.  Below is a snippet of the new pattern and should be run when your application initializes, such as your application start in global.asax.  You should also remove the userResolver from the config file.

```c#

            Host.Get("default").ResolveLocalUser = (host, resolveArgs) =>
            {
                
                return new LocalUser(userNameOfLocalUser,emailOfLocalUser);
            };
```

###Executing the Tests
To execute all the tests you must modify some items. 
- In your Community site, create an Oauth Client that is confidential using Client Credentials
- In the test project, locate Setup.cs in the root
- At the top of th file modify the Url, user and Oauth variables to be appropriate to your site.

>Note that these tests are meant to test the communication infrastrature and SDK logic, not the REST Apis themselves.  If you are having issues with a specific API not related to the SDK please contact support.

####Where is the documentation?
Please refer to the [wiki section](https://github.com/Telligent/Social-Rest-SDK/wiki/) of this repository.

####How do I report a bug?
You can use the [issues section](https://github.com/Telligent/Social-Rest-SDK/issues/) of this repository to report any issues.

####Where can I ask questions?
Please visit our [developer community](http://community.telligent.com/community/f/554) to ask questions, get answers, collaborate and connect with other developers. Plus, give us feedback there so we can continue to improve these tools for you.

####Can I contribute?
Yes, we will have more details soon on how you can contribute.
